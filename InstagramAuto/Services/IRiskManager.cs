using InstagramAuto.Client.Models;
using System;
using System.Threading.Tasks;

namespace InstagramAuto.Client.Services
{
    /// <summary>
    /// Persian: ????? ?????? ????
    /// English: Risk management service
    /// </summary>
    public interface IRiskManager
    {
        /// <summary>
        /// Persian: ?????? ??????? ????
        /// English: Get risk management settings
        /// </summary>
        Task<RiskSettings> GetSettingsAsync();

        /// <summary>
        /// Persian: ????? ??????? ????
        /// English: Save risk management settings
        /// </summary>
        Task SaveSettingsAsync(RiskSettings settings);

        /// <summary>
        /// Persian: ????? ????? ????? ????
        /// English: Check if an action can be performed
        /// </summary>
        Task<bool> CanPerformActionAsync(ActionType actionType);

        /// <summary>
        /// Persian: ??? ????? ????
        /// English: Record that an action was performed
        /// </summary>
        Task RecordActionAsync(ActionType actionType);

        /// <summary>
        /// Persian: ?????? ??????????? ????
        /// English: Get limits for action type
        /// </summary>
        Task<ActionLimits> GetLimitsForActionAsync(ActionType actionType);

        /// <summary>
        /// Persian: ???? ???? ??????????? ??????
        /// English: Reset daily action counters
        /// </summary>
        Task ResetDailyCountersAsync();

        /// <summary>
        /// Persian: ????? ???? ???? ???????
        /// English: Temporarily pause actions
        /// </summary>
        Task PauseActionsAsync(TimeSpan duration);

        /// <summary>
        /// Persian: ?? ?????? ???????
        /// English: Resume actions
        /// </summary>
        Task ResumeActionsAsync();

        /// <summary>
        /// Persian: ????? ???? ???????
        /// English: Whether actions are paused
        /// </summary>
        bool IsActionsPaused { get; }
    }
}