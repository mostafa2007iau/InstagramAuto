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
    ///   الحاقات مربوط به کامنت‌ها بدون تغییر در GeneratedClient.g.cs
    /// English:
    ///   Extensions for comment-related endpoints without modifying GeneratedClient.g.cs
    /// </summary>
    public partial class InstagramAutoClient
    {
        /// <summary>
        /// Persian:
        ///   دریافت کامنت‌های یک پست به صورت صفحه‌بندی شده
        /// English:
        ///   Get paginated comments for a media post
        /// </summary>
        /// <param name="mediaId">شناسه پست / Media ID</param>
        /// <param name="limit">حداکثر تعداد در هر صفحه / Max items per page</param>
        /// <param name="cursor">شناسه صفحه بعد / Next page cursor</param>
        /// <param name="cancellationToken">توکن لغو عملیات / Cancellation token</param>
        public async Task<PaginatedComments> GetCommentsAsync(
            string mediaId,
            int? limit = null,
            string cursor = null,
            CancellationToken cancellationToken = default)
        {
            var urlBuilder = new StringBuilder();
            urlBuilder.Append(BaseUrl).Append("api/comments?media_id=").Append(Uri.EscapeDataString(mediaId));

            if (limit.HasValue)
                urlBuilder.Append("&limit=").Append(Uri.EscapeDataString(limit.Value.ToString()));
            
            if (!string.IsNullOrEmpty(cursor))
                urlBuilder.Append("&cursor=").Append(Uri.EscapeDataString(cursor));

            using var request = new HttpRequestMessage(HttpMethod.Get, urlBuilder.ToString());
            request.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));

            PrepareRequest(_httpClient, request, urlBuilder.ToString());

            using var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            ProcessResponse(_httpClient, response);

            if ((int)response.StatusCode == 200)
            {
                var result = await ReadObjectResponseAsync<PaginatedComments>(
                    response, 
                    headers: null,
                    cancellationToken).ConfigureAwait(false);

                return result.Object;
            }

            var content = response.Content == null
                ? null
                : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            throw new ApiException(
                $"HTTP status code {(int)response.StatusCode} getting comments",
                (int)response.StatusCode,
                content,
                headers: null,
                innerException: null);
        }

        /// <summary>
        /// Persian:
        ///   ارسال کامنت جدید برای یک پست
        /// English:
        ///   Post a new comment on a media
        /// </summary>
        public async Task<ApiActionResult> PostCommentAsync(
            string mediaId,
            string text,
            CancellationToken cancellationToken = default)
        {
            var urlBuilder = new StringBuilder();
            urlBuilder.Append(BaseUrl).Append("api/comments");

            var payload = new
            {
                media_id = mediaId,
                text = text
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
                $"HTTP status code {(int)response.StatusCode} posting comment",
                (int)response.StatusCode,
                content,
                headers: null,
                innerException: null);
        }
    }
}