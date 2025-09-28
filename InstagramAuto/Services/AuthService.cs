using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramAuto.Client.Models;

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
    }
}
