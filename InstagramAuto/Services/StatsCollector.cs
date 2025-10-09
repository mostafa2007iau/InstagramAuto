using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using InstagramAuto.Client.Models;

namespace InstagramAuto.Client.Services
{
    /// <summary>
    /// Persian:
    ///     ?????????? ???????? ????.
    ///     ??? ???? ????? ???????? ? ????? ???? ?????????? ????? ???.
    /// English:
    ///     Statistics collection implementation.
    ///     This class is responsible for collecting and analyzing system activity statistics.
    /// </summary>
    public class StatsCollector : IStatsCollector
    {
        private readonly ILogger<StatsCollector> _logger;
        private readonly IAuthService _authService;
        private readonly ConcurrentDictionary<string, List<ActionResult>> _actionResults;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public StatsCollector(ILogger<StatsCollector> logger, IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
            _actionResults = new ConcurrentDictionary<string, List<ActionResult>>();
        }

        /// <summary>
        /// Persian:
        ///     ?????? ???? ?????????.
        /// English:
        ///     Get reply statistics.
        /// </summary>
        public async Task<IEnumerable<ReplyStatItem>> GetReplyStatsAsync(string accountId)
        {
            try
            {
                // Get stats from server
                var serverStats = await _authService.GetReplyStatsAsync(accountId);

                // Merge with local stats
                await _lock.WaitAsync();
                try
                {
                    var localStats = GetLocalReplyStats(accountId);
                    return MergeReplyStats(serverStats, localStats);
                }
                finally
                {
                    _lock.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get reply stats for account {AccountId}", accountId);
                throw;
            }
        }

        /// <summary>
        /// Persian:
        ///     ?????? ???? ???????.
        /// English:
        ///     Get action statistics.
        /// </summary>
        public async Task<ActionStats> GetActionStatsAsync(string accountId, DateTime? from = null, DateTime? to = null)
        {
            await _lock.WaitAsync();
            try
            {
                if (!_actionResults.TryGetValue(accountId, out var results))
                    return new ActionStats();

                var filteredResults = results.Where(r =>
                    (!from.HasValue || r.Timestamp >= from.Value) &&
                    (!to.HasValue || r.Timestamp <= to.Value)
                ).ToList();

                return new ActionStats
                {
                    TotalActions = filteredResults.Count,
                    SuccessfulActions = filteredResults.Count(r => r.Success),
                    FailedActions = filteredResults.Count(r => !r.Success),
                    ActionsByType = filteredResults
                        .GroupBy(r => r.Type)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    ActionsByMedia = filteredResults
                        .GroupBy(r => r.MediaId)
                        .ToDictionary(g => g.Key, g => g.Count())
                };
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
        ///     Record action result.
        /// </summary>
        public async Task RecordActionResultAsync(ActionResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            await _lock.WaitAsync();
            try
            {
                if (!_actionResults.TryGetValue(result.AccountId, out var results))
                {
                    results = new List<ActionResult>();
                    _actionResults[result.AccountId] = results;
                }

                results.Add(result);
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Persian:
        ///     ?????? ????? ?????.
        /// English:
        ///     Get statistical report.
        /// </summary>
        public async Task<StatisticalReport> GetStatisticalReportAsync(string accountId, ReportTimeframe timeframe)
        {
            var (from, to) = GetTimeframeRange(timeframe);
            var actionStats = await GetActionStatsAsync(accountId, from, to);
            
            return new StatisticalReport
            {
                Timeframe = timeframe,
                From = from ?? DateTime.UtcNow,
                To = to ?? DateTime.UtcNow,
                ActionStats = actionStats,
                TopMedias = GetTopMedias(accountId, from, to),
                TopRules = GetTopRules(accountId, from, to),
                SuccessRateByHour = GetSuccessRateByHour(accountId, from, to)
            };
        }

        /// <summary>
        /// Persian:
        ///     ?????? ???? ?????????? ????.
        /// English:
        ///     Get local reply statistics.
        /// </summary>
        private List<ReplyStatItem> GetLocalReplyStats(string accountId)
        {
            if (!_actionResults.TryGetValue(accountId, out var results))
                return new List<ReplyStatItem>();

            return results
                .Where(r => r.Type == ActionType.CommentReply)
                .GroupBy(r => r.MediaId)
                .Select(g => new ReplyStatItem
                {
                    Media_id = g.Key,
                    Successful_replies = g.Count(r => r.Success),
                    Failed_replies = g.Count(r => !r.Success)
                })
                .ToList();
        }

        /// <summary>
        /// Persian:
        ///     ????? ???? ?????????? ???? ? ????.
        /// English:
        ///     Merge server and local reply statistics.
        /// </summary>
        private List<ReplyStatItem> MergeReplyStats(IEnumerable<ReplyStatItem> serverStats, IEnumerable<ReplyStatItem> localStats)
        {
            var merged = new Dictionary<string, ReplyStatItem>();

            foreach (var stat in serverStats)
            {
                merged[stat.Media_id] = stat;
            }

            foreach (var stat in localStats)
            {
                if (merged.TryGetValue(stat.Media_id, out var existing))
                {
                    existing.Successful_replies += stat.Successful_replies;
                    existing.Failed_replies += stat.Failed_replies;
                }
                else
                {
                    merged[stat.Media_id] = stat;
                }
            }

            return merged.Values.ToList();
        }

        /// <summary>
        /// Persian:
        ///     ?????? ???? ????? ?????.
        /// English:
        ///     Get report timeframe range.
        /// </summary>
        private (DateTime? From, DateTime? To) GetTimeframeRange(ReportTimeframe timeframe)
        {
            var now = DateTime.UtcNow;
            
            return timeframe switch
            {
                ReportTimeframe.Today => (now.Date, now),
                ReportTimeframe.ThisWeek => (now.AddDays(-(int)now.DayOfWeek), now),
                ReportTimeframe.ThisMonth => (new DateTime(now.Year, now.Month, 1), now),
                _ => (null, null)
            };
        }

        /// <summary>
        /// Persian:
        ///     ?????? ??????? ????.
        /// English:
        ///     Get top media posts.
        /// </summary>
        private Dictionary<string, int> GetTopMedias(string accountId, DateTime? from, DateTime? to)
        {
            if (!_actionResults.TryGetValue(accountId, out var results))
                return new Dictionary<string, int>();

            return results
                .Where(r => r.Success &&
                           (!from.HasValue || r.Timestamp >= from.Value) &&
                           (!to.HasValue || r.Timestamp <= to.Value))
                .GroupBy(r => r.MediaId)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Persian:
        ///     ?????? ?????? ????.
        /// English:
        ///     Get top rules.
        /// </summary>
        private Dictionary<string, int> GetTopRules(string accountId, DateTime? from, DateTime? to)
        {
            if (!_actionResults.TryGetValue(accountId, out var results))
                return new Dictionary<string, int>();

            return results
                .Where(r => r.Success &&
                           (!from.HasValue || r.Timestamp >= from.Value) &&
                           (!to.HasValue || r.Timestamp <= to.Value))
                .GroupBy(r => r.RuleId)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Persian:
        ///     ?????? ??? ?????? ?? ????? ????.
        /// English:
        ///     Get success rate by hour.
        /// </summary>
        private Dictionary<string, double> GetSuccessRateByHour(string accountId, DateTime? from, DateTime? to)
        {
            if (!_actionResults.TryGetValue(accountId, out var results))
                return new Dictionary<string, double>();

            return results
                .Where(r => (!from.HasValue || r.Timestamp >= from.Value) &&
                           (!to.HasValue || r.Timestamp <= to.Value))
                .GroupBy(r => r.Timestamp.Hour)
                .OrderBy(g => g.Key)
                .ToDictionary(
                    g => g.Key.ToString("D2"),
                    g => g.Count(r => r.Success) * 100.0 / g.Count()
                );
        }
    }
}