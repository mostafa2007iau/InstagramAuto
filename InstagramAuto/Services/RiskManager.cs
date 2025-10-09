using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;
using System.Linq;
using InstagramAuto.Client.Models;

namespace InstagramAuto.Client.Services
{
    /// <summary>
    /// Persian:
    ///     ?????????? ?????? ????.
    ///     ??? ???? ????? ????? ??????????? ?????? ? ??????? ???.
    /// English:
    ///     Risk management implementation.
    ///     This class is responsible for controlling action limits and delays.
    /// </summary>
    public class RiskManager : IRiskManager
    {
        private readonly ILogger<RiskManager> _logger;
        private readonly string _settingsPath;
        private RiskSettings _settings;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<ActionType, List<DateTime>>> _actionHistory;
        private readonly Random _random;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private DateTime? _pauseUntil;

        public bool IsActionsPaused => _pauseUntil.HasValue && DateTime.UtcNow < _pauseUntil.Value;

        public RiskManager(ILogger<RiskManager> logger)
        {
            _logger = logger;
            _settingsPath = Path.Combine(FileSystem.AppDataDirectory, "risk_settings.json");
            _actionHistory = new ConcurrentDictionary<string, ConcurrentDictionary<ActionType, List<DateTime>>>();
            _random = new Random();
            LoadSettings();
        }

        /// <summary>
        /// Persian:
        ///     ????? ????? ????? ??????.
        /// English:
        ///     Check if action is allowed.
        /// </summary>
        public async Task<bool> CanPerformActionAsync(string accountId, ActionType actionType)
        {
            if (!_settings.LimitationsEnabled)
                return true;

            await _lock.WaitAsync();
            try
            {
                CleanupOldActions(accountId);

                if (!_actionHistory.TryGetValue(accountId, out var accountHistory))
                    return true;

                if (!accountHistory.TryGetValue(actionType, out var actionHistory))
                    return true;

                var now = DateTime.UtcNow;

                // Check daily limit
                if (_settings.MaxActionsPerDay.TryGetValue(actionType, out var dailyLimit))
                {
                    var last24Hours = actionHistory.Count(t => (now - t).TotalHours < 24);
                    if (last24Hours >= dailyLimit)
                        return false;
                }

                // Check hourly limit
                if (_settings.MaxActionsPerHour.TryGetValue(actionType, out var hourlyLimit))
                {
                    var lastHour = actionHistory.Count(t => (now - t).TotalHours < 1);
                    if (lastHour >= hourlyLimit)
                        return false;
                }

                // Check minimum delay
                if (_settings.MinDelayBetweenActions.TryGetValue(actionType, out var minDelay))
                {
                    var lastAction = actionHistory.Max();
                    var timeSinceLastAction = (now - lastAction).TotalSeconds;
                    if (timeSinceLastAction < minDelay)
                        return false;
                }

                return true;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Persian:
        ///     ??? ????? ??????.
        /// English:
        ///     Record action performed.
        /// </summary>
        public async Task RecordActionAsync(string accountId, ActionType actionType)
        {
            await _lock.WaitAsync();
            try
            {
                var accountHistory = _actionHistory.GetOrAdd(accountId, 
                    _ => new ConcurrentDictionary<ActionType, List<DateTime>>());
                
                var actionHistory = accountHistory.GetOrAdd(actionType, 
                    _ => new List<DateTime>());

                actionHistory.Add(DateTime.UtcNow);
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Persian:
        ///     ?????? ????? ???????? ???? ?????? ????.
        /// English:
        ///     Get suggested delay for next action.
        /// </summary>
        public async Task<int> GetSuggestedDelayAsync(string accountId, ActionType actionType)
        {
            if (!_settings.LimitationsEnabled)
                return 0;

            await _lock.WaitAsync();
            try
            {
                if (!_settings.MinDelayBetweenActions.TryGetValue(actionType, out var baseDelay))
                    return 0;

                // Apply random variation
                var variation = baseDelay * _settings.RandomDelayPercentage / 100.0;
                var randomDelay = _random.NextDouble() * variation;
                return (int)(baseDelay + randomDelay);
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Persian:
        ///     ??????????? ??????? ????.
        /// English:
        ///     Update risk settings.
        /// </summary>
        public async Task UpdateRiskSettingsAsync(RiskSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            await _lock.WaitAsync();
            try
            {
                _settings = settings;
                await SaveSettings();
            }
            finally
            {
                _lock.Release();
            }
        }

        public Task<RiskSettings> GetSettingsAsync()
        {
            return Task.FromResult(_settings);
        }

        public Task SaveSettingsAsync(RiskSettings settings)
        {
            return UpdateRiskSettingsAsync(settings);
        }

        public Task<bool> CanPerformActionAsync(ActionType actionType)
        {
            return CanPerformActionAsync("default", actionType);
        }

        public Task RecordActionAsync(ActionType actionType)
        {
            return RecordActionAsync("default", actionType);
        }

        public async Task<ActionLimits> GetLimitsForActionAsync(ActionType actionType)
        {
            return new ActionLimits
            {
                HourlyLimit = _settings.MaxActionsPerHour.GetValueOrDefault(actionType),
                DailyLimit = _settings.MaxActionsPerDay.GetValueOrDefault(actionType),
                MinIntervalSeconds = _settings.MinDelayBetweenActions.GetValueOrDefault(actionType)
            };
        }

        public async Task ResetDailyCountersAsync()
        {
            await _lock.WaitAsync();
            try
            {
                _actionHistory.Clear();
            }
            finally
            {
                _lock.Release();
            }
        }

        public Task PauseActionsAsync(TimeSpan duration)
        {
            _pauseUntil = DateTime.UtcNow.Add(duration);
            return Task.CompletedTask;
        }

        public Task ResumeActionsAsync()
        {
            _pauseUntil = null;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Persian:
        ///     ??????? ?????????? ?????.
        /// English:
        ///     Clean up old actions.
        /// </summary>
        private void CleanupOldActions(string accountId)
        {
            if (_actionHistory.TryGetValue(accountId, out var accountHistory))
            {
                var now = DateTime.UtcNow;
                foreach (var actionType in accountHistory.Keys)
                {
                    if (accountHistory.TryGetValue(actionType, out var history))
                    {
                        history.RemoveAll(t => (now - t).TotalHours >= 24);
                    }
                }
            }
        }

        /// <summary>
        /// Persian:
        ///     ???????? ???????.
        /// English:
        ///     Load settings.
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    _settings = JsonSerializer.Deserialize<RiskSettings>(json);
                }
                else
                {
                    _settings = CreateDefaultSettings();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load risk settings");
                _settings = CreateDefaultSettings();
            }
        }

        /// <summary>
        /// Persian:
        ///     ????? ???????.
        /// English:
        ///     Save settings.
        /// </summary>
        private async Task SaveSettings()
        {
            try
            {
                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_settingsPath, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save risk settings");
                throw;
            }
        }

        /// <summary>
        /// Persian:
        ///     ????? ??????? ???????.
        /// English:
        ///     Create default settings.
        /// </summary>
        private RiskSettings CreateDefaultSettings()
        {
            return new RiskSettings
            {
                LimitationsEnabled = true,
                RandomDelayPercentage = 20,
                MaxActionsPerDay = new Dictionary<ActionType, int>
                {
                    { ActionType.CommentReply, 100 },
                    { ActionType.DirectMessage, 50 },
                    { ActionType.Like, 200 },
                    { ActionType.Follow, 50 }
                },
                MinDelayBetweenActions = new Dictionary<ActionType, int>
                {
                    { ActionType.CommentReply, 30 },
                    { ActionType.DirectMessage, 60 },
                    { ActionType.Like, 15 },
                    { ActionType.Follow, 120 }
                },
                MaxActionsPerHour = new Dictionary<ActionType, int>
                {
                    { ActionType.CommentReply, 20 },
                    { ActionType.DirectMessage, 10 },
                    { ActionType.Like, 30 },
                    { ActionType.Follow, 10 }
                }
            };
        }
    }
}