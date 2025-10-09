using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net;
using System.IO;
using InstagramAuto.Client.Models;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Services
{
    /// <summary>
    /// Persian:
    ///     ?????????? ????? ?????? ?????????.
    /// English:
    ///     Implementation of proxy management service.
    /// </summary>
    public class ProxyService : IProxyService
    {
        private readonly ILogger<ProxyService> _logger;
        private readonly string _configPath;
        private ProxyConfig _activeProxy;
        private readonly object _lock = new object();

        public ProxyService(ILogger<ProxyService> logger)
        {
            _logger = logger;
            _configPath = Path.Combine(FileSystem.AppDataDirectory, "proxy_config.json");
            LoadConfiguration();
        }

        /// <summary>
        /// Persian:
        ///     ?????? ?????? ????.
        /// English:
        ///     Get the active proxy configuration.
        /// </summary>
        public Task<ProxyConfig> GetActiveProxyAsync()
        {
            lock (_lock)
            {
                return Task.FromResult(_activeProxy);
            }
        }

        /// <summary>
        /// Persian:
        ///     ????? ?????? ????.
        /// English:
        ///     Set the active proxy configuration.
        /// </summary>
        public async Task SetActiveProxyAsync(ProxyConfig proxy)
        {
            if (proxy != null && !await TestProxyAsync(proxy))
            {
                throw new Exception("Proxy test failed");
            }

            lock (_lock)
            {
                _activeProxy = proxy;
                SaveConfiguration();
            }
        }

        /// <summary>
        /// Persian:
        ///     ??????? ???? ??????.
        /// English:
        ///     Disable proxy configuration.
        /// </summary>
        public Task DisableProxyAsync()
        {
            lock (_lock)
            {
                _activeProxy = null;
                SaveConfiguration();
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Persian:
        ///     ??? ?????? ?? ????.
        /// English:
        ///     Test proxy with address.
        /// </summary>
        public Task<bool> TestProxyAsync(string proxyAddress)
        {
            if (string.IsNullOrEmpty(proxyAddress))
                return Task.FromResult(false);

            var parts = proxyAddress.Split(':');
            if (parts.Length != 2 || !int.TryParse(parts[1], out int port))
                return Task.FromResult(false);

            // Create temporary proxy config for testing
            var tempProxy = new ProxyConfig 
            { 
                Address = parts[0],
                Port = port,
                Enabled = true
            };

            return TestProxyAsync(tempProxy);
        }

        /// <summary>
        /// Persian:
        ///     ??? ??????.
        /// English:
        ///     Test proxy configuration.
        /// </summary>
        private async Task<bool> TestProxyAsync(ProxyConfig proxy)
        {
            if (proxy == null) return false;

            try
            {
                var handler = new HttpClientHandler
                {
                    Proxy = new WebProxy(proxy.FullAddress)
                    {
                        Credentials = !string.IsNullOrEmpty(proxy.Username)
                            ? new NetworkCredential(proxy.Username, proxy.Password)
                            : null
                    }
                };

                using var client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromSeconds(10);
                
                var response = await client.GetAsync("https://api.instagram.com");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Proxy test failed");
                return false;
            }
        }

        /// <summary>
        /// Persian:
        ///     ???????? ??????? ?? ????.
        /// English:
        ///     Load configuration from file.
        /// </summary>
        private void LoadConfiguration()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    _activeProxy = JsonConvert.DeserializeObject<ProxyConfig>(json);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load proxy configuration");
            }
        }

        /// <summary>
        /// Persian:
        ///     ????? ??????? ?? ????.
        /// English:
        ///     Save configuration to file.
        /// </summary>
        private void SaveConfiguration()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_activeProxy, Formatting.Indented);
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save proxy configuration");
            }
        }
    }
}