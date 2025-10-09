using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InstagramAuto.Client.Models;
using Newtonsoft.Json;

namespace InstagramAuto.Client
{
    /// <summary>
    /// Persian:
    ///   ??????? ????? ?? ?????? ?????? ???? ????? ?? GeneratedClient.g.cs
    /// English:
    ///   Extensions for proxy management without modifying GeneratedClient.g.cs
    /// </summary>
    public partial class InstagramAutoClient
    {
        /// <summary>
        /// Persian:
        ///   ??? ????? ??????
        /// English:
        ///   Test proxy connection
        /// </summary>
        public async Task<bool> TestProxyAsync(
            string proxyAddress,
            CancellationToken cancellationToken = default)
        {
            var urlBuilder = new StringBuilder();
            urlBuilder.Append(BaseUrl).Append("api/proxy/test");

            var payload = new { proxy = proxyAddress };

            using var request = new HttpRequestMessage(HttpMethod.Post, urlBuilder.ToString());
            request.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

            var json = JsonConvert.SerializeObject(payload);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            PrepareRequest(_httpClient, request, urlBuilder.ToString());

            using var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            ProcessResponse(_httpClient, response);

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Persian:
        ///   ????/??????? ???? ?????? ???? ?? ???
        /// English:
        ///   Enable/disable proxy for a session
        /// </summary>
        public async Task<ApiActionResult> ToggleProxyAsync(
            string sessionId,
            bool enabled,
            string proxyAddress = null,
            CancellationToken cancellationToken = default)
        {
            var urlBuilder = new StringBuilder();
            urlBuilder.Append(BaseUrl)
                     .Append("api/proxy/toggle/")
                     .Append(Uri.EscapeDataString(sessionId));

            var payload = new 
            { 
                enabled = enabled,
                proxy = proxyAddress
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, urlBuilder.ToString());
            request.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

            var json = JsonConvert.SerializeObject(payload);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            PrepareRequest(_httpClient, request, urlBuilder.ToString());

            using var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            ProcessResponse(_httpClient, response);

            if ((int)response.StatusCode == 200)
            {
                var result = await ReadObjectResponseAsync<ApiActionResult>(
                    response,
                    headers: null,
                    cancellationToken).ConfigureAwait(false);

                return result.Object;
            }

            var content = response.Content == null
                ? null 
                : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            throw new ApiException(
                $"HTTP status code {(int)response.StatusCode} toggling proxy",
                (int)response.StatusCode,
                content,
                headers: null,
                innerException: null);
        }
    }
}