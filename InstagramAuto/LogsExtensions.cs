using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using InstagramAuto.Client.Models;

namespace InstagramAuto.Client
{
    /// <summary>
    /// Persian:
    ///   الحاقات مربوط به لاگ‌ها بدون تغییر در GeneratedClient.g.cs
    /// English:
    ///   Extensions for logs API without modifying GeneratedClient.g.cs
    /// </summary>
    public partial class InstagramAutoClient
    {
        /// <summary>
        /// Persian:
        ///   یک لیست صفحه‌بندی شده از لاگ‌ها را برمی‌گرداند.
        /// English:
        ///   Retrieves a paginated list of log entries.
        /// </summary>
        /// <param name="level">
        /// Persian: فیلتر براساس سطح لاگ (Error, Warning, Info)  
        /// English: Filter by log level.
        /// </param>
        /// <param name="limit">
        /// Persian: حداکثر تعداد آیتم در هر صفحه  
        /// English: Maximum items per page.
        /// </param>
        /// <param name="cursor">
        /// Persian: شناسه صفحه بعد  
        /// English: The pagination cursor for next page.
        /// </param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>PaginatedLogsDto</returns>
        public async Task<PaginatedLogsDto> ListLogsAsync(
            string level = null,
            int? limit = null,
            string cursor = null,
            CancellationToken cancellationToken = default)
        {
            var urlBuilder = new StringBuilder();
            urlBuilder.Append(BaseUrl).Append("api/logs?");
            if (!string.IsNullOrEmpty(level))
                urlBuilder.Append("level=").Append(Uri.EscapeDataString(level)).Append("&");
            if (limit.HasValue)
                urlBuilder.Append("limit=").Append(Uri.EscapeDataString(limit.Value.ToString())).Append("&");
            if (!string.IsNullOrEmpty(cursor))
                urlBuilder.Append("cursor=").Append(Uri.EscapeDataString(cursor)).Append("&");
            if (urlBuilder[urlBuilder.Length - 1] == '?' || urlBuilder[urlBuilder.Length - 1] == '&')
                urlBuilder.Length--;

            using var request = new HttpRequestMessage(HttpMethod.Get, urlBuilder.ToString());
            request.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));
            PrepareRequest(_httpClient, request, urlBuilder);

            using var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            ProcessResponse(_httpClient, response);
            var statusCode = (int)response.StatusCode;

            if (statusCode == 200)
            {
                var result = await ReadObjectResponseAsync<PaginatedLogsDto>(
                    response,
                    response.Headers.ToDictionary(h => h.Key, h => h.Value),
                    cancellationToken).ConfigureAwait(false);
                return result.Object!;
            }

            var error = response.Content == null
                ? null
                : await ReadAsStringAsync(response.Content, cancellationToken).ConfigureAwait(false);
            throw new ApiException(
                $"Unexpected HTTP status code: {statusCode}",
                statusCode,
                error,
                response.Headers.ToDictionary(h => h.Key, h => h.Value),
                null);
        }
    }
}
