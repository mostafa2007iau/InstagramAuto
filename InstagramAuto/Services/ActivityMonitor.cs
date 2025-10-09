using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstagramAuto.Client.Models;
using Microsoft.Extensions.Logging;

namespace InstagramAuto.Client.Services
{
    /// <summary>
    /// Persian:
    ///     ?????????? ?????????? ?????????.
    ///     ??? ???? ????? ??? ? ?????? ?????????? ????? ???.
    /// English:
    ///     Activity monitoring implementation.
    ///     This class is responsible for recording and tracking system activities.
    /// </summary>
    public class ActivityMonitor : IActivityMonitor
    {
        private readonly ILogger<ActivityMonitor> _logger;
        private readonly IAuthService _authService;
        private readonly ConcurrentDictionary<string, List<ActivityItem>> _activities;
        private readonly ConcurrentDictionary<string, Action<ActivityItem>> _subscribers;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public ActivityMonitor(ILogger<ActivityMonitor> logger, IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
            _activities = new ConcurrentDictionary<string, List<ActivityItem>>();
            _subscribers = new ConcurrentDictionary<string, Action<ActivityItem>>();
            
            _logger.LogInformation("ActivityMonitor initialized");
        }

        /// <summary>
        /// Persian:
        ///     ?????? ?????????? ????.
        /// English:
        ///     Get live activities.
        /// </summary>
        public async Task<IEnumerable<ActivityItem>> GetLiveActivitiesAsync(string accountId)
        {
            try
            {
                _logger.LogDebug("Fetching live activities for account {AccountId}", accountId);
                
                // Get activities from server
                var serverActivities = await _authService.GetLiveActivitiesAsync(accountId);

                // Merge with local activities
                await _lock.WaitAsync();
                try
                {
                    if (!_activities.TryGetValue(accountId, out var localActivities))
                    {
                        localActivities = new List<ActivityItem>();
                        _activities[accountId] = localActivities;
                    }

                    // Add new server activities
                    foreach (var activity in serverActivities)
                    {
                        if (!localActivities.Any(a => 
                            a.Id == activity.Id)) // Use proper ID comparison
                        {
                            localActivities.Add(activity);
                            NotifySubscribers(activity);
                        }
                    }

                    // Sort by timestamp descending
                    localActivities.Sort((a, b) => b.Created_at.CompareTo(a.Created_at));

                    _logger.LogDebug("Retrieved {Count} activities for account {AccountId}", localActivities.Count, accountId);
                    return localActivities.ToList();
                }
                finally
                {
                    _lock.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get live activities for account {AccountId}", accountId);
                throw;
            }
        }

        /// <summary>
        /// Persian:
        ///     ?????? ?? ????????? ??????.
        /// English:
        ///     Subscribe to activity events.
        /// </summary>
        public IDisposable SubscribeToActivityEvents(Action<ActivityItem> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            var id = Guid.NewGuid().ToString();
            _subscribers[id] = handler;
            _logger.LogDebug("Added new activity subscriber {Id}", id);
            
            return new Subscription(() => 
            {
                _subscribers.TryRemove(id, out _);
                _logger.LogDebug("Removed activity subscriber {Id}", id);
            });
        }

        /// <summary>
        /// Persian:
        ///     ??? ?????? ????.
        /// English:
        ///     Record new activity.
        /// </summary>
        public async Task RecordActivityAsync(ActivityItem activity)
        {
            if (activity == null) throw new ArgumentNullException(nameof(activity));

            await _lock.WaitAsync();
            try
            {
                var accountId = activity.Account_id; // Use proper Account_id property
                if (string.IsNullOrEmpty(accountId))
                {
                    _logger.LogWarning("Activity recorded without account ID, using default");
                    accountId = "_default";
                }

                if (!_activities.TryGetValue(accountId, out var activities))
                {
                    activities = new List<ActivityItem>();
                    _activities[accountId] = activities;
                }

                activities.Add(activity);
                _logger.LogDebug("Recorded new activity {ActivityId} for account {AccountId}", activity.Id, accountId);
                
                NotifySubscribers(activity);
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Persian:
        ///     ??? ???? ?????????? ?????.
        /// English:
        ///     Clear old activities.
        /// </summary>
        public async Task ClearOldActivitiesAsync(TimeSpan age)
        {
            var cutoff = DateTime.UtcNow - age;
            _logger.LogInformation("Clearing activities older than {Cutoff}", cutoff);

            await _lock.WaitAsync();
            try
            {
                var totalRemoved = 0;
                foreach (var accountId in _activities.Keys)
                {
                    if (_activities.TryGetValue(accountId, out var activities))
                    {
                        var countBefore = activities.Count;
                        activities.RemoveAll(a => a.Updated_at < cutoff);
                        var removed = countBefore - activities.Count;
                        totalRemoved += removed;
                        
                        if (removed > 0)
                        {
                            _logger.LogDebug("Removed {Count} old activities for account {AccountId}", 
                                removed, accountId);
                        }
                    }
                }
                
                if (totalRemoved > 0)
                {
                    _logger.LogInformation("Cleared {Count} old activities in total", totalRemoved);
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Persian:
        ///     ??????????? ?? ???????.
        /// English:
        ///     Notify subscribers.
        /// </summary>
        private void NotifySubscribers(ActivityItem activity)
        {
            foreach (var subscriber in _subscribers.Values)
            {
                try
                {
                    subscriber(activity);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to notify activity subscriber");
                }
            }
        }

        /// <summary>
        /// Persian:
        ///     ???? ?????? ???? ??? ??????.
        /// English:
        ///     Subscription class for unsubscribing.
        /// </summary>
        private class Subscription : IDisposable
        {
            private readonly Action _unsubscribe;
            private bool _disposed;

            public Subscription(Action unsubscribe)
            {
                _unsubscribe = unsubscribe ?? throw new ArgumentNullException(nameof(unsubscribe));
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _unsubscribe();
                    _disposed = true;
                }
            }
        }
    }
}