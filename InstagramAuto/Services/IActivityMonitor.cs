using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramAuto.Client.Models;

namespace InstagramAuto.Client.Services
{
    /// <summary>
    /// Persian:
    ///     ?????????? ?????????? ??????.
    /// English:
    ///     Monitors application activities.
    /// </summary>
    public interface IActivityMonitor
    {
        /// <summary>
        /// Persian:
        ///     ?????? ?????????? ????.
        /// English:
        ///     Get live activities.
        /// </summary>
        Task<IEnumerable<ActivityItem>> GetLiveActivitiesAsync(string accountId);

        /// <summary>
        /// Persian:
        ///     ?????? ?? ????????? ??????.
        /// English:
        ///     Subscribe to activity events.
        /// </summary>
        IDisposable SubscribeToActivityEvents(Action<ActivityItem> handler);

        /// <summary>
        /// Persian:
        ///     ??? ?????? ????.
        /// English:
        ///     Record new activity.
        /// </summary>
        Task RecordActivityAsync(ActivityItem activity);

        /// <summary>
        /// Persian:
        ///     ??? ???? ?????????? ?????.
        /// English:
        ///     Clear old activities.
        /// </summary>
        Task ClearOldActivitiesAsync(TimeSpan age);
    }
}