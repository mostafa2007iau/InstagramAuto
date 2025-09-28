using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramAuto.Client.Models;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
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
            var login = await _client.LoginAsync(sessionMeta.Id, username, password);

            // 3) اگر Ok است
            if (login?.Result?.Ok == true)
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

            // 4) اگر نیاز به چالش است → ناوبری به صفحه‌ی ChallengePage
            if (login?.Result?.Challenge != null)
            {
                // توجه کنید پارامترها باید با QueryPropertyهای ChallengePage یکی باشند
                var route = $"challenge?ChallengeToken={sessionMeta.Id}"
                          + $"&Username={Uri.EscapeDataString(username)}"
                          + $"&Password={Uri.EscapeDataString(password)}";
                await Shell.Current.GoToAsync(route);
                return null;
            }

            throw new Exception("Login failed: neither success nor challenge.");
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

            throw new InvalidOperationException("No session cached and no credentials available.");
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
