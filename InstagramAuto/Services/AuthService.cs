using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramAuto.Client.Models;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace InstagramAuto.Client.Services
{
    public class AuthService : IAuthService
    {
        private readonly IInstagramAutoClient _client;
        private AccountSession _cachedSession;
        private string _lastUsername;
        private string _lastPassword;

        public AuthService(IInstagramAutoClient client)
        {
            _client = client;
        }

        public async Task<AccountSession> LoginAsync(string username, string password)
        {
            _lastUsername = username;
            _lastPassword = password;

            // 1) ایجاد سشن
            var sessionMeta = await _client.SessionsAsync(new SessionCreateIn
            {
                Account_id = username,
                Locale = SessionCreateInLocale.Fa,
                Proxy = null,
                Proxy_enabled = false,
            });

            // 2) لاگین
            var loginResponse = await _client.LoginAsync(sessionMeta.Id, username, password);

            // Normalize response: some backends place actual result under a nested 'result' field.
            JObject respObj = null;
            try
            {
                var serialized = JsonConvert.SerializeObject(loginResponse);
                respObj = JObject.Parse(serialized);
            }
            catch
            {
                // fallback: try to inspect Result property if available
                respObj = new JObject();
                try
                {
                    var res = loginResponse?.Result;
                    if (res != null)
                        respObj["result"] = JObject.FromObject(res);
                }
                catch { }
            }

            JObject resultObj = null;
            if (respObj != null && respObj["result"] != null && respObj["result"].Type == JTokenType.Object)
                resultObj = (JObject)respObj["result"];

            // Fallback to top-level shape if no nested result
            if (resultObj == null)
            {
                // try to map common properties from older client wrapper
                try
                {
                    resultObj = new JObject();
                    // attempt to read some known properties
                    if (respObj["ok"] != null) resultObj["ok"] = respObj["ok"];
                    if (respObj["challenge_token"] != null) resultObj["challenge_token"] = respObj["challenge_token"];
                    if (respObj["challenge_required"] != null) resultObj["challenge_required"] = respObj["challenge_required"];
                    if (respObj["message"] != null) resultObj["message"] = respObj["message"];
                }
                catch
                {
                    resultObj = null;
                }
            }

            // 3) check for challenge in different possible places
            string challengeToken = null;
            bool challengeRequired = false;
            bool authenticated = false;

            if (resultObj != null)
            {
                challengeToken = resultObj.Value<string>("challenge_token");
                challengeRequired = resultObj.Value<bool?>("challenge_required") == true || resultObj.Value<bool?>("two_factor_required") == true;
                authenticated = resultObj.Value<bool?>("authenticated") == true || resultObj.Value<bool?>("ok") == true || resultObj.Value<string>("status") == "ok";
            }

            // Also check top-level for challenge_token (some server responses return it at top)
            if (string.IsNullOrEmpty(challengeToken) && respObj != null && respObj["challenge_token"] != null)
                challengeToken = respObj.Value<string>("challenge_token");

            if (challengeRequired || !string.IsNullOrEmpty(challengeToken))
            {
                // navigate to ChallengePage with token and credentials
                var route = $"challenge?ChallengeToken={Uri.EscapeDataString(challengeToken ?? sessionMeta.Id)}"
                          + $"&Username={Uri.EscapeDataString(username)}"
                          + $"&Password={Uri.EscapeDataString(password)}";
                await Shell.Current.GoToAsync(route);
                return new AccountSession { ChallengeToken = challengeToken ?? sessionMeta.Id, Id = sessionMeta.Id, AccountId = sessionMeta.Account_id };
            }

            // 4) if authenticated success
            if (authenticated)
            {
                _cachedSession = new AccountSession
                {
                    Id = sessionMeta.Id,
                    AccountId = sessionMeta.Account_id,
                    SessionBlob = sessionMeta.Session_blob,
                    CreatedAt = sessionMeta.Created_at,
                    UpdatedAt = sessionMeta.Updated_at
                };
                return _cachedSession;
            }

            // 5) otherwise prepare detailed error message (include server-provided details when available)
            string faMsg = null;
            string enMsg = null;
            string details = JsonConvert.SerializeObject(loginResponse);

            if (resultObj != null)
            {
                faMsg = resultObj.Value<string>("message");
                enMsg = resultObj.Value<string>("message_en");
            }

            // If no specific messages provided, try top-level fields
            if (string.IsNullOrEmpty(faMsg) && respObj != null)
                faMsg = respObj.Value<string>("message") ?? respObj.Value<string>("error_detail");

            if (string.IsNullOrEmpty(enMsg) && respObj != null)
                enMsg = respObj.Value<string>("message_en");

            var msg = $"ورود ناموفق: نه موفقیت و نه چالش.\nFA: {faMsg ?? "خطای داخلی هنگام ورود"}\nEN: {enMsg ?? string.Empty}\nDetails: {details}";
            throw new Exception(msg);
        }

        public async Task<AccountSession> LoadSessionAsync()
        {
            if (_cachedSession != null)
            {
                await _client.ExportAsync(_cachedSession.Id);
                return _cachedSession;
            }

            if (!string.IsNullOrEmpty(_lastUsername) && !string.IsNullOrEmpty(_lastPassword))
                return await LoginAsync(_lastUsername, _lastPassword);

            throw new InvalidOperationException("هیچ سشنی کش نشده و هیچ مدرکی در دسترس نیست.");
        }

        public async Task SaveSessionAsync(AccountSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            _cachedSession = session;

            var payload = new Dictionary<string, object>
            {
                ["id"] = session.Id,
                ["account_id"] = session.AccountId,
                ["locale"] = SessionCreateInLocale.Fa
            };
            await (_client as InstagramAutoClient)?.ImportAsync(payload);
        }

        // Persian:
        //   گرفتن کامنت‌های یک پست
        // English:
        //   Fetch comments for a media
        public async Task<PaginatedComments> GetCommentsAsync(string mediaId, string cursor = null)
        {
            // Direct HTTP call since not present in IInstagramAutoClient
            using var http = new HttpClient { BaseAddress = new Uri("http://156.236.31.41:8000/") };
            var url = $"/api/comments?media_id={mediaId}";
            if (!string.IsNullOrEmpty(cursor))
                url += $"&cursor={Uri.EscapeDataString(cursor)}";
            var resp = await http.GetAsync(url);
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PaginatedComments>(json);
        }

        // Persian:
        //   ذخیره قانون جدید یا ویرایش قانون موجود
        // English:
        //   Save new rule or edit existing rule
        public async Task SaveRuleAsync(RuleItem rule)
        {
            // Direct HTTP call for saving rule
            using var http = new HttpClient { BaseAddress = new Uri("http://156.236.31.41:8000/") };
            var url = "/api/rules";
            var json = JsonConvert.SerializeObject(rule);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await http.PostAsync(url, content);
            resp.EnsureSuccessStatusCode();
        }

        // Persian:
        //   خروجی گرفتن قوانین به فرمت مورد نظر
        // English:
        //   Export rules in the specified format
        public async Task<string> ExportRulesAsync(string accountId, string format)
        {
            using var http = new HttpClient { BaseAddress = new Uri("http://156.236.31.41:8000/") };
            var url = $"/api/rules/export?account_id={accountId}&format={format}";
            var resp = await http.GetAsync(url);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadAsStringAsync();
        }

        // Persian:
        //   وارد کردن قوانین از داده متنی
        // English:
        //   Import rules from text data
        public async Task ImportRulesAsync(string accountId, string data, string format)
        {
            using var http = new HttpClient { BaseAddress = new Uri("http://156.236.31.41:8000/") };
            var url = $"/api/rules/import?account_id={accountId}&format={format}";
            var content = new StringContent(data, Encoding.UTF8, "text/plain");
            var resp = await http.PostAsync(url, content);
            resp.EnsureSuccessStatusCode();
        }

        // Persian:
        //   دریافت فعالیت‌های زنده
        // English:
        //   Get live activities
        public async Task<List<ActivityItem>> GetLiveActivitiesAsync(string accountId)
        {
            using var http = new HttpClient { BaseAddress = new Uri("http://156.236.31.41:8000/") };
            var url = $"/api/activity/live?account_id={accountId}";
            var resp = await http.GetAsync(url);
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ActivityItem>>(json);
        }

        // Persian:
        //   دریافت آمار ریپلای‌ها برای هر پست
        // English:
        //   Get reply stats per post
        public async Task<List<ReplyStatItem>> GetReplyStatsAsync(string accountId)
        {
            using var http = new HttpClient { BaseAddress = new Uri("http://156.236.31.41:8000/") };
            var url = $"/api/stats/replies?account_id={accountId}";
            var resp = await http.GetAsync(url);
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ReplyStatItem>>(json);
        }

        // Persian:
        //   گرفتن لیست قوانین
        // English:
        //   Get rules list
        public async Task<PaginatedRules> GetRulesAsync(string accountId, int limit = 50, string cursor = null)
        {
            return await _client.RulesGETAsync(account_id: accountId, limit: limit, cursor: cursor);
        }

        // Persian:
        //   ایجاد قانون جدید
        // English:
        //   Create new rule
        public async Task CreateRuleAsync(RuleIn rule)
        {
            await _client.RulesPOSTAsync(rule);
        }
    }
}
