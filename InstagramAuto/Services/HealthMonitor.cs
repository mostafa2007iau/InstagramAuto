using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace InstagramAuto.Client.Services
{
    /// <summary>
    /// Persian:
    ///     ?????????? ?????????? ?????.
    ///     ??? ???? ????? ????? ?? ????? ????? ? ???????? ??????????.
    /// English:
    ///     Health monitoring implementation.
    ///     This class is responsible for monitoring system health and collecting metrics.
    /// </summary>
    public class HealthMonitor : IHealthMonitor
    {
        private readonly ILogger<HealthMonitor> _logger;
        private readonly IInstagramAutoClient _client;
        private readonly ConcurrentDictionary<string, double> _metrics;
        private readonly ConcurrentDictionary<string, (DateTime Time, ServiceState State, string Message)> _serviceStates;
        private DateTime _lastCheckTime;

        public HealthMonitor(ILogger<HealthMonitor> logger, IInstagramAutoClient client)
        {
            _logger = logger;
            _client = client;
            _metrics = new ConcurrentDictionary<string, double>();
            _serviceStates = new ConcurrentDictionary<string, (DateTime, ServiceState, string)>();
            _lastCheckTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Persian:
        ///     ?????? ????? ?????.
        /// English:
        ///     Get health status.
        /// </summary>
        public async Task<HealthStatus> GetHealthStatusAsync()
        {
            try
            {
                // Check API health
                await CheckApiHealthAsync();

                var status = new HealthStatus
                {
                    IsHealthy = true,
                    Message = "System is healthy",
                    LastCheckTime = _lastCheckTime,
                    Services = new Dictionary<string, ServiceStatus>()
                };

                foreach (var (service, (time, state, message)) in _serviceStates)
                {
                    status.Services[service] = new ServiceStatus
                    {
                        Name = service,
                        Status = state,
                        Message = message,
                        LastResponseTime = GetMetric($"{service}_response_time") ?? 0
                    };

                    if (state != ServiceState.Operational)
                        status.IsHealthy = false;
                }

                return status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return new HealthStatus
                {
                    IsHealthy = false,
                    Message = $"Health check failed: {ex.Message}",
                    LastCheckTime = _lastCheckTime,
                    Services = new Dictionary<string, ServiceStatus>()
                };
            }
        }

        /// <summary>
        /// Persian:
        ///     ?????? ????????.
        /// English:
        ///     Get metrics.
        /// </summary>
        public Task<Dictionary<string, double>> GetMetricsAsync()
        {
            return Task.FromResult(new Dictionary<string, double>(_metrics));
        }

        /// <summary>
        /// Persian:
        ///     ??? ?????.
        /// English:
        ///     Record metric.
        /// </summary>
        public Task RecordMetricAsync(string name, double value)
        {
            _metrics.AddOrUpdate(name, value, (_, __) => value);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Persian:
        ///     ??? ???.
        /// English:
        ///     Record error.
        /// </summary>
        public Task RecordErrorAsync(Exception ex, string context)
        {
            _logger.LogError(ex, "Error in context {Context}", context);
            
            var serviceName = context.Split('/')[0];
            _serviceStates.AddOrUpdate(
                serviceName,
                (DateTime.UtcNow, ServiceState.Down, ex.Message),
                (_, __) => (DateTime.UtcNow, ServiceState.Down, ex.Message)
            );

            IncrementMetric($"{serviceName}_error_count");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Persian:
        ///     ????? ????? API.
        /// English:
        ///     Check API health.
        /// </summary>
        private async Task CheckApiHealthAsync()
        {
            try
            {
                var start = DateTime.UtcNow;
                await _client.MetricsAsync();
                var responseTime = (DateTime.UtcNow - start).TotalMilliseconds;

                await RecordMetricAsync("api_response_time", responseTime);
                _serviceStates["API"] = (DateTime.UtcNow, ServiceState.Operational, "API is responding");

                if (responseTime > 1000)
                {
                    _serviceStates["API"] = (DateTime.UtcNow, ServiceState.Degraded, $"High latency: {responseTime:F0}ms");
                }
            }
            catch (Exception ex)
            {
                _serviceStates["API"] = (DateTime.UtcNow, ServiceState.Down, ex.Message);
                throw;
            }
            finally
            {
                _lastCheckTime = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Persian:
        ///     ?????? ????? ?????.
        /// English:
        ///     Get metric value.
        /// </summary>
        private double? GetMetric(string name)
        {
            return _metrics.TryGetValue(name, out var value) ? value : null;
        }

        /// <summary>
        /// Persian:
        ///     ?????? ????? ?????.
        /// English:
        ///     Increment metric value.
        /// </summary>
        private void IncrementMetric(string name)
        {
            _metrics.AddOrUpdate(name, 1, (_, v) => v + 1);
        }
    }
}