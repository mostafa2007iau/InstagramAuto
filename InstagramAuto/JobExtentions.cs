using InstagramAuto.Client;
using InstagramAuto.Client.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InstagramAuto.Client
{
    /// <summary>
    /// Persian:
    ///   الحاقات مربوط به API صف (Jobs/Queue) بدون تغییر در GeneratedClient.g.cs
    /// English:
    ///   Extensions for the Jobs/Queue API without modifying the GeneratedClient.g.cs file.
    /// </summary>
    public partial class InstagramAutoClient
    {
        /// <summary>
        /// Persian:
        ///   یک لیست صفحه‌بندی شده از jobهای صف را برمی‌گرداند.
        /// English:
        ///   Retrieves a paginated list of queue jobs.
        /// </summary>
        /// <param name="cursor">
        /// Persian: شناسه صفحه بعد (cursor)  
        /// English: The pagination cursor for the next page.
        /// </param>
        /// <param name="limit">
        /// Persian: حداکثر تعداد آیتم‌ها در هر صفحه  
        /// English: Maximum number of items per page.
        /// </param>
        /// <param name="status">
        /// Persian: فیلتر براساس وضعیت job (queued, sent, failed)  
        /// English: Filter by job status.
        /// </param>
        /// <param name="type">
        /// Persian: فیلتر براساس نوع job (comment, dm)  
        /// English: Filter by job type.
        /// </param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>PaginatedJobsDto</returns>
        public async Task<PaginatedJobsDto> ListJobsAsync(
            string cursor = null,
            int? limit = null,
            string status = null,
            string type = null,
            CancellationToken cancellationToken = default)
        {
            // 1) ساخت URL با query string
            var urlBuilder = new StringBuilder();
            urlBuilder.Append(BaseUrl).Append("api/jobs?");
            if (!string.IsNullOrEmpty(cursor)) urlBuilder.Append("cursor=").Append(Uri.EscapeDataString(cursor)).Append("&");
            if (limit.HasValue) urlBuilder.Append("limit=").Append(Uri.EscapeDataString(limit.Value.ToString())).Append("&");
            if (!string.IsNullOrEmpty(status)) urlBuilder.Append("status=").Append(Uri.EscapeDataString(status)).Append("&");
            if (!string.IsNullOrEmpty(type)) urlBuilder.Append("type=").Append(Uri.EscapeDataString(type)).Append("&");
            if (urlBuilder[urlBuilder.Length - 1] == '&' || urlBuilder[urlBuilder.Length - 1] == '?')
                urlBuilder.Length--;

            // 2) آماده‌سازی درخواست HTTP GET
            using var request = new HttpRequestMessage(HttpMethod.Get, urlBuilder.ToString());
            request.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));
            PrepareRequest(_httpClient, request, urlBuilder);

            // 3) ارسال و خواندن پاسخ
            using var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            ProcessResponse(_httpClient, response);
            var statusCode = (int)response.StatusCode;

            if (statusCode == 200)
            {
                var result = await ReadObjectResponseAsync<PaginatedJobsDto>(response, response.Headers.ToDictionary(h => h.Key, h => h.Value), cancellationToken)
                                .ConfigureAwait(false);
                return result.Object!;
            }

            var errorText = response.Content == null
                ? null
                : await ReadAsStringAsync(response.Content, cancellationToken).ConfigureAwait(false);
            throw new ApiException($"Unexpected HTTP status code: {statusCode}", statusCode, errorText, response.Headers.ToDictionary(h => h.Key, h => h.Value), null);
        }

        /// <summary>
        /// Persian:
        ///   جزئیات یک job براساس شناسه آن را بازیابی می‌کند.
        /// English:
        ///   Retrieves details of a job by its ID.
        /// </summary>
        /// <param name="jobId">The job identifier / شناسه‌ی وظیفه</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>JobDetailDto</returns>
        public async Task<JobDetailDto> GetJobAsync(string jobId, CancellationToken cancellationToken = default)
        {
            if (jobId == null) throw new ArgumentNullException(nameof(jobId));

            var url = $"{BaseUrl}api/jobs/{Uri.EscapeDataString(jobId)}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));
            PrepareRequest(_httpClient, request, url);

            using var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            ProcessResponse(_httpClient, response);
            var statusCode = (int)response.StatusCode;

            if (statusCode == 200)
            {
                var result = await ReadObjectResponseAsync<JobDetailDto>(response, response.Headers.ToDictionary(h => h.Key, h => h.Value), cancellationToken)
                                .ConfigureAwait(false);
                return result.Object!;
            }

            var errorText = response.Content == null
                ? null
                : await ReadAsStringAsync(response.Content, cancellationToken).ConfigureAwait(false);
            throw new ApiException($"Unexpected HTTP status code: {statusCode}", statusCode, errorText, response.Headers.ToDictionary(h => h.Key, h => h.Value), null);
        }

        /// <summary>
        /// Persian:
        ///   درخواست تکرار (retry) یک job را به سرور ارسال می‌کند.
        /// English:
        ///   Sends a retry request for the specified job to the server.
        /// </summary>
        /// <param name="jobId">The job identifier / شناسه‌ی وظیفه</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>ApiActionResult</returns>
        public Task<ApiActionResult> RetryJobAsync(string jobId, CancellationToken cancellationToken = default) =>
            ExecuteJobActionAsync($"api/jobs/{Uri.EscapeDataString(jobId)}/retry", cancellationToken);

        /// <summary>
        /// Persian:
        ///   درخواست لغو (cancel) یک job را به سرور ارسال می‌کند.
        /// English:
        ///   Sends a cancel request for the specified job to the server.
        /// </summary>
        public Task<ApiActionResult> CancelJobAsync(string jobId, CancellationToken cancellationToken = default) =>
            ExecuteJobActionAsync($"api/jobs/{Uri.EscapeDataString(jobId)}/cancel", cancellationToken);

        /// <summary>
        /// Persian:
        ///   یک job را به DLQ انتقال می‌دهد.
        /// English:
        ///   Moves the specified job to the Dead Letter Queue (DLQ).
        /// </summary>
        public Task<ApiActionResult> MoveJobToDlqAsync(string jobId, CancellationToken cancellationToken = default) =>
            ExecuteJobActionAsync($"api/jobs/{Uri.EscapeDataString(jobId)}/dlq", cancellationToken);

        /// <summary>
        /// Persian:
        ///   متدی کمکی برای ارسال درخواست POST خالی به مسیرهای retry/cancel/dlq.
        /// English:
        ///   Helper method to send an empty POST to retry/cancel/dlq endpoints.
        /// </summary>
        private async Task<ApiActionResult> ExecuteJobActionAsync(string relativePath, CancellationToken cancellationToken)
        {
            // 1) آماده‌سازی درخواست POST خالی
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}{relativePath}");
            var json = JsonConvert.SerializeObject(new { }, JsonSerializerSettings);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            request.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));
            PrepareRequest(_httpClient, request, relativePath);

            // 2) ارسال و پروسس پاسخ
            using var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            ProcessResponse(_httpClient, response);
            var statusCode = (int)response.StatusCode;
            var respText = response.Content == null
                ? null
                : await ReadAsStringAsync(response.Content, cancellationToken).ConfigureAwait(false);

            // 3) بررسی موفقیت و تبدیل JSON به ApiActionResult
            if (statusCode == 200)
                return JsonConvert.DeserializeObject<ApiActionResult>(respText!, JsonSerializerSettings)!;

            throw new ApiException($"Unexpected HTTP status code: {statusCode}", statusCode, respText, response.Headers.ToDictionary(h => h.Key, h => h.Value), null);
        }
    }
}
