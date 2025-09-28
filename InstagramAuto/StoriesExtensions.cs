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
    ///   الحاقات مربوط به استوری‌ها بدون تغییر فایل auto-generated.
    /// English:
    ///   Extensions for story-related endpoints without modifying GeneratedClient.g.cs.
    /// </summary>
    public partial class InstagramAutoClient
    {
        /// <summary>
        /// Persian:
        ///   دریافت یک لیست صفحه‌بندی شده از استوری‌های کاربر متصل.
        /// English:
        ///   Retrieves a paginated list of stories for the connected account.
        /// </summary>
        public async Task<PaginatedStoriesDto> ListStoriesAsync(
            string sessionId,
            int? limit = null,
            string cursor = null,
            CancellationToken cancellationToken = default)
        {
            var url = new StringBuilder();
            url.Append(BaseUrl).Append("api/stories?session_id=").Append(Uri.EscapeDataString(sessionId)).Append("&");
            if (limit.HasValue) url.Append("limit=").Append(Uri.EscapeDataString(limit.Value.ToString())).Append("&");
            if (!string.IsNullOrEmpty(cursor)) url.Append("cursor=").Append(Uri.EscapeDataString(cursor)).Append("&");
            if (url[url.Length - 1] == '&' || url[url.Length - 1] == '?')
                url.Length--;

            using var request = new HttpRequestMessage(HttpMethod.Get, url.ToString());
            request.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));
            PrepareRequest(_httpClient, request, url);

            using var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            ProcessResponse(_httpClient, response);
            if ((int)response.StatusCode == 200)
            {
                var result = await ReadObjectResponseAsync<PaginatedStoriesDto>(
                    response, response.Headers.ToDictionary(h => h.Key, h => h.Value), cancellationToken)
                    .ConfigureAwait(false);
                return result.Object!;
            }

            var err = response.Content == null
                ? null
                : await ReadAsStringAsync(response.Content, cancellationToken).ConfigureAwait(false);
            throw new ApiException(
                $"Unexpected HTTP status code: {(int)response.StatusCode}",
                (int)response.StatusCode,
                err,
                response.Headers.ToDictionary(h => h.Key, h => h.Value),
                null);
        }

        /// <summary>
        /// Persian:
        ///   ارسال پاسخ دایرکت به یک استوری.
        /// English:
        ///   Sends a direct message reply to a given story.
        /// </summary>
        public async Task<ApiActionResult> ReplyToStoryAsync(
            string sessionId,
            string storyId,
            DirectMessageIn body,
            CancellationToken cancellationToken = default)
        {
            var url = $"{BaseUrl}api/stories/{Uri.EscapeDataString(storyId)}/reply?session_id={Uri.EscapeDataString(sessionId)}";
            var json = JsonConvert.SerializeObject(body, JsonSerializerSettings);

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));
            PrepareRequest(_httpClient, request, url);

            using var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            ProcessResponse(_httpClient, response);
            var text = response.Content == null
                ? null
                : await ReadAsStringAsync(response.Content, cancellationToken).ConfigureAwait(false);

            if ((int)response.StatusCode == 200)
                return JsonConvert.DeserializeObject<ApiActionResult>(text!, JsonSerializerSettings)!;

            throw new ApiException(
                $"Unexpected HTTP status code: {(int)response.StatusCode}",
                (int)response.StatusCode,
                text,
                response.Headers.ToDictionary(h => h.Key, h => h.Value),
                null);
        }
    }
}
