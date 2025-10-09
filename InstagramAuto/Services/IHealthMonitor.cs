using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InstagramAuto.Client.Services
{
    /// <summary>
    /// Persian:
    ///     ?????????? ????? ??????.
    /// English:
    ///     Monitors application health.
    /// </summary>
    public interface IHealthMonitor
    {
        /// <summary>
        /// Persian:
        ///     ?????? ????? ?????.
        /// English:
        ///     Get health status.
        /// </summary>
        Task<HealthStatus> GetHealthStatusAsync();

        /// <summary>
        /// Persian:
        ///     ?????? ????????.
        /// English:
        ///     Get metrics.
        /// </summary>
        Task<Dictionary<string, double>> GetMetricsAsync();

        /// <summary>
        /// Persian:
        ///     ??? ?????.
        /// English:
        ///     Record metric.
        /// </summary>
        Task RecordMetricAsync(string name, double value);

        /// <summary>
        /// Persian:
        ///     ??? ???.
        /// English:
        ///     Record error.
        /// </summary>
        Task RecordErrorAsync(Exception ex, string context);
    }

    /// <summary>
    /// Persian:
    ///     ????? ????? ??????.
    /// English:
    ///     Application health status.
    /// </summary>
    public class HealthStatus
    {
        /// <summary>
        /// Persian:
        ///     ??? ?????? ???? ????
        /// English:
        ///     Is application healthy?
        /// </summary>
        public bool IsHealthy { get; set; }

        /// <summary>
        /// Persian:
        ///     ???? ?????.
        /// English:
        ///     Status message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Persian:
        ///     ?????? ????? ????????.
        /// English:
        ///     Service status details.
        /// </summary>
        public Dictionary<string, ServiceStatus> Services { get; set; }

        /// <summary>
        /// Persian:
        ///     ???? ????? ?????.
        /// English:
        ///     Last check time.
        /// </summary>
        public DateTime LastCheckTime { get; set; }
    }

    /// <summary>
    /// Persian:
    ///     ????? ?????.
    /// English:
    ///     Service status.
    /// </summary>
    public class ServiceStatus
    {
        /// <summary>
        /// Persian:
        ///     ??? ?????.
        /// English:
        ///     Service name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Persian:
        ///     ?????.
        /// English:
        ///     Status.
        /// </summary>
        public ServiceState Status { get; set; }

        /// <summary>
        /// Persian:
        ///     ????.
        /// English:
        ///     Message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Persian:
        ///     ????? ???? ???? (??????????).
        /// English:
        ///     Last response time (ms).
        /// </summary>
        public double LastResponseTime { get; set; }
    }

    /// <summary>
    /// Persian:
    ///     ????????? ???? ?????.
    /// English:
    ///     Possible service states.
    /// </summary>
    public enum ServiceState
    {
        /// <summary>
        /// Persian: ????
        /// English: Operational
        /// </summary>
        Operational,

        /// <summary>
        /// Persian: ??? ????
        /// English: Degraded
        /// </summary>
        Degraded,

        /// <summary>
        /// Persian: ????
        /// English: Down
        /// </summary>
        Down
    }
}