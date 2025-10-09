using System.Threading.Tasks;
using InstagramAuto.Client.Models;

namespace InstagramAuto.Client.Services
{
    /// <summary>
    /// Persian: ????? ?????? ??????
    /// English: Proxy management service
    /// </summary>
    public interface IProxyService
    {
        /// <summary>
        /// Persian: ?????? ?????? ????
        /// English: Get active proxy configuration
        /// </summary>
        Task<ProxyConfig> GetActiveProxyAsync();

        /// <summary>
        /// Persian: ????? ?????? ????
        /// English: Set active proxy configuration
        /// </summary>
        Task SetActiveProxyAsync(ProxyConfig proxy);

        /// <summary>
        /// Persian: ??????? ???? ??????
        /// English: Disable proxy
        /// </summary>
        Task DisableProxyAsync();

        /// <summary>
        /// Persian: ??? ??????
        /// English: Test proxy configuration
        /// </summary>
        Task<bool> TestProxyAsync(string proxyAddress);
    }
}