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
    public partial class InstagramAutoClient
    {
        public async Task<SettingsDto> GetSettingsAsync(
            string accountId,
            CancellationToken cancellationToken = default)
        {
            var url = $"{BaseUrl}api/settings/{Uri.EscapeDataString(accountId)}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));
            PrepareRequest(_httpClient, request, url);

            using var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            ProcessResponse(_httpClient, response);
            if ((int)response.StatusCode == 200)
            {
                var result = await ReadObjectResponseAsync<SettingsDto>(
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

        public async Task<SettingsDto> UpdateSettingsAsync(
            SettingsIn body,
            CancellationToken cancellationToken = default)
        {
            var url = $"{BaseUrl}api/settings/{Uri.EscapeDataString(body.AccountId)}";
            var json = JsonConvert.SerializeObject(body, JsonSerializerSettings);
            using var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));
            PrepareRequest(_httpClient, request, url);

            using var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            ProcessResponse(_httpClient, response);
            if ((int)response.StatusCode == 200)
            {
                var result = await ReadObjectResponseAsync<SettingsDto>(
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
    }
}
