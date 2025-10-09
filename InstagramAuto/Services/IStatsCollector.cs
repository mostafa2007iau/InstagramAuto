using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramAuto.Client.Models;

namespace InstagramAuto.Client.Services
{
    /// <summary>
    /// Persian:
    ///     ???????? ???? ??????.
    /// English:
    ///     Collects application statistics.
    /// </summary>
    public interface IStatsCollector
    {
        /// <summary>
        /// Persian:
        ///     ?????? ???? ?????????.
        /// English:
        ///     Get reply statistics.
        /// </summary>
        Task<IEnumerable<ReplyStatItem>> GetReplyStatsAsync(string accountId);

        /// <summary>
        /// Persian:
        ///     ?????? ???? ???????.
        /// English:
        ///     Get action statistics.
        /// </summary>
        Task<ActionStats> GetActionStatsAsync(string accountId, DateTime? from = null, DateTime? to = null);

        /// <summary>
        /// Persian:
        ///     ??? ????? ??????.
        /// English:
        ///     Record action result.
        /// </summary>
        Task RecordActionResultAsync(ActionResult result);

        /// <summary>
        /// Persian:
        ///     ?????? ????? ?????.
        /// English:
        ///     Get statistical report.
        /// </summary>
        Task<StatisticalReport> GetStatisticalReportAsync(string accountId, ReportTimeframe timeframe);
    }

    /// <summary>
    /// Persian:
    ///     ???????? ????? ?????.
    /// English:
    ///     Report timeframes.
    /// </summary>
    public enum ReportTimeframe
    {
        /// <summary>
        /// Persian: ?????
        /// English: Today
        /// </summary>
        Today,

        /// <summary>
        /// Persian: ??? ????
        /// English: ThisWeek
        /// </summary>
        ThisWeek,

        /// <summary>
        /// Persian: ??? ???
        /// English: ThisMonth
        /// </summary>
        ThisMonth,

        /// <summary>
        /// Persian: ??????
        /// English: Custom
        /// </summary>
        Custom
    }

    /// <summary>
    /// Persian:
    ///     ???? ?????????.
    /// English:
    ///     Action statistics.
    /// </summary>
    public class ActionStats
    {
        public int TotalActions { get; set; }
        public int SuccessfulActions { get; set; }
        public int FailedActions { get; set; }
        public Dictionary<ActionType, int> ActionsByType { get; set; }
        public Dictionary<string, int> ActionsByMedia { get; set; }
    }

    /// <summary>
    /// Persian:
    ///     ????? ??????.
    /// English:
    ///     Action result.
    /// </summary>
    public class ActionResult
    {
        public string AccountId { get; set; }
        public string MediaId { get; set; }
        public ActionType Type { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; }
        public string RuleId { get; internal set; }
    }

    /// <summary>
    /// Persian:
    ///     ????? ?????.
    /// English:
    ///     Statistical report.
    /// </summary>
    public class StatisticalReport
    {
        public ReportTimeframe Timeframe { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public ActionStats ActionStats { get; set; }
        public Dictionary<string, int> TopMedias { get; set; }
        public Dictionary<string, int> TopRules { get; set; }
        public Dictionary<string, double> SuccessRateByHour { get; set; }
    }
}