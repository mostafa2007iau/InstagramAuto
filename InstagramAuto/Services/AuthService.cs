using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using InstagramAuto.Client.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Services
{
    /// <summary>
    /// Persian:
    ///     سرویس احراز هویت، یک wrapper روی IInstagramAutoClient.
    ///     این کلاس مسئول مدیریت جلسات و احراز هویت است.
    /// English:
    ///     Authentication service, a wrapper over IInstagramAutoClient.
    ///     This class handles session management and authentication.
    /// </summary>
    public partial class AuthService : IAuthService, IDisposable
    {
        private readonly InstagramAutoClient _client;
        private readonly IProxyService _proxyService;
        private readonly ILogger<AuthService> _logger;
        private readonly SemaphoreSlim _loginThrottle = new SemaphoreSlim(1, 1);
        private readonly ConcurrentDictionary<string, DateTime> _lastLoginAttempts = new();
        private IAsyncPolicy<AccountSession> _retryPolicy;

        private const int MinLoginIntervalSeconds = 60;
        private AccountSession _cachedSession;
        private string _lastUsername;
        private string _lastPassword;
        private bool _disposed;

        public AuthService(
            InstagramAutoClient client,
            IProxyService proxyService,
            ILogger<AuthService> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _proxyService = proxyService ?? throw new ArgumentNullException(nameof(proxyService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Configure retry policy
            _retryPolicy = Policy<AccountSession>
                .Handle<HttpRequestException>()
                .Or<ApiException>()
                .WaitAndRetryAsync(3, attempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (ex, timeSpan, attempt, ctx) =>
                    {
                        _logger.LogWarning(
                            ex.Exception,
                            "Retry attempt {Attempt} after {Delay}s delay",
                            attempt, timeSpan.TotalSeconds);
                    });
        }

        /// <summary>
        /// Persian:
        ///     ورود کاربر با نام کاربری و رمز عبور.
        /// English:
        ///     Login user with username and password.
        /// </summary>
        public async Task<AccountSession> LoginAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentException("نام کاربری نمی‌تواند خالی باشد", nameof(username));
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("رمز عبور نمی‌تواند خالی باشد", nameof(password));

            await _loginThrottle.WaitAsync();
            try
            {
                _logger.LogInformation("Attempting login for user {Username}", username);

                // Check rate limit
                if (_lastLoginAttempts.TryGetValue(username, out var lastAttempt))
                {
                    var timeSinceLastLogin = DateTime.UtcNow - lastAttempt;
                    if (timeSinceLastLogin.TotalSeconds < MinLoginIntervalSeconds)
                    {
                        var waitTime = MinLoginIntervalSeconds - timeSinceLastLogin.TotalSeconds;
                        _logger.LogWarning(
                            "Rate limit hit for {Username}. Must wait {WaitTime:0} seconds",
                            username, waitTime);
                        throw new RateLimitException(
                            $"تعداد تلاش‌های ورود بیش از حد مجاز. لطفاً {waitTime:0} ثانیه صبر کنید.");
                    }
                }

                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    _lastUsername = username;
                    _lastPassword = password;

                    // Get active proxy
                    var proxy = await _proxyService.GetActiveProxyAsync();
                    _logger.LogDebug("Using proxy: {ProxyAddress}", proxy?.Address ?? "none");

                    // Create session
                    var sessionMeta = await _client.SessionsAsync(new SessionCreateIn
                    {
                        Account_id = username,
                        Locale = SessionCreateInLocale.Fa,
                        Proxy = proxy?.Address,
                        Proxy_enabled = proxy != null
                    });

                    var loginResponse = await _client.LoginAsync(
                        sessionMeta.Id, username, password);

                    // Update last attempt timestamp
                    _lastLoginAttempts[username] = DateTime.UtcNow;

                    // Process response
                    var (authenticated, challengeToken, challengeRequired, errorMessage) =
                        ProcessLoginResponse(loginResponse);

                    if (challengeRequired || !string.IsNullOrEmpty(challengeToken))
                    {
                        _logger.LogInformation(
                            "Challenge required for user {Username}", username);
                        await NavigateToChallenge(
                            challengeToken ?? sessionMeta.Id, username, password);
                        return new AccountSession
                        {
                            ChallengeToken = challengeToken ?? sessionMeta.Id,
                            Id = sessionMeta.Id,
                            AccountId = sessionMeta.Account_id
                        };
                    }

                    if (authenticated)
                    {
                        var session = new AccountSession
                        {
                            Id = sessionMeta.Id,
                            AccountId = sessionMeta.Account_id,
                            SessionBlob = sessionMeta.Session_blob,
                            CreatedAt = sessionMeta.Created_at,
                            UpdatedAt = sessionMeta.Updated_at,
                            ProxyId = proxy?.Id
                        };

                        _cachedSession = session;
                        _logger.LogInformation(
                            "Login successful for user {Username}", username);
                        return session;
                    }

                    _logger.LogError(
                        "Login failed for user {Username}: {Error}",
                        username, errorMessage);
                    throw new AuthenticationException(errorMessage);
                });
            }
            finally
            {
                _loginThrottle.Release();
            }
        }

        /// <summary>
        /// Persian:
        ///     بارگذاری جلسه کاربر از حافظه پنهان یا ورود مجدد.
        /// English:
        ///     Load user session from cache or re-login.
        /// </summary>
        public async Task<AccountSession> LoadSessionAsync()
        {
            if (_cachedSession != null)
            {
                try
                {
                    await _client.ExportAsync(_cachedSession.Id);
                    _logger.LogDebug("Using cached session for {AccountId}",
                        _cachedSession.AccountId);
                    return _cachedSession;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to use cached session, attempting re-login");
                    _cachedSession = null;
                }
            }

            if (!string.IsNullOrEmpty(_lastUsername) && !string.IsNullOrEmpty(_lastPassword))
            {
                _logger.LogInformation(
                    "Re-logging in user {Username}", _lastUsername);
                return await LoginAsync(_lastUsername, _lastPassword);
            }

            const string error = "هیچ جلسه‌ای در حافظه پنهان نیست و اطلاعات ورود در دسترس نیست.";
            _logger.LogError(error);
            throw new InvalidOperationException(error);
        }

        /// <summary>
        /// Persian:
        ///     ذخیره جلسه کاربر.
        /// English:
        ///     Save user session.
        /// </summary>
        public async Task SaveSessionAsync(AccountSession session)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));

            _logger.LogDebug("Saving session for {AccountId}", session.AccountId);
            _cachedSession = session;

            var payload = new Dictionary<string, object>
            {
                ["id"] = session.Id,
                ["account_id"] = session.AccountId,
                ["locale"] = SessionCreateInLocale.Fa
            };

            if (!string.IsNullOrEmpty(session.SessionBlob))
                payload["session_blob"] = session.SessionBlob;

            await (_client as InstagramAutoClient)?.ImportAsync(payload);
        }

        /// <summary>
        /// Persian:
        ///     پردازش پاسخ لاگین و استخراج وضعیت احراز هویت.
        /// English:
        ///     Process login response and extract authentication status.
        /// </summary>
        private (bool authenticated, string challengeToken, bool challengeRequired, string errorMessage)
            ProcessLoginResponse(object response)
        {
            var respObj = response as JObject;
            var resultObj = respObj?["result"] as JObject;

            if (resultObj == null)
            {
                try
                {
                    resultObj = respObj;
                    if (respObj["message"] != null)
                        resultObj["message"] = respObj["message"];
                }
                catch
                {
                    resultObj = null;
                }
            }

            string challengeToken = null;
            bool challengeRequired = false;
            bool authenticated = false;

            if (resultObj != null)
            {
                challengeToken = resultObj.Value<string>("challenge_token");
                challengeRequired =
                    resultObj.Value<bool?>("challenge_required") == true ||
                    resultObj.Value<bool?>("two_factor_required") == true;
                authenticated =
                    resultObj.Value<bool?>("authenticated") == true ||
                    resultObj.Value<bool?>("ok") == true ||
                    resultObj.Value<string>("status") == "ok";
            }

            if (string.IsNullOrEmpty(challengeToken) &&
                respObj?["challenge_token"] != null)
            {
                challengeToken = respObj.Value<string>("challenge_token");
            }

            string errorMessage = null;
            if (resultObj != null && respObj != null)
            {
                errorMessage = resultObj.Value<string>("message") ??
                             respObj.Value<string>("error_detail");
            }

            return (authenticated, challengeToken, challengeRequired, errorMessage);
        }

        /// <summary>
        /// Persian:
        ///     هدایت به صفحه چالش برای تکمیل احراز هویت.
        /// English:
        ///     Navigate to challenge page for authentication completion.
        /// </summary>
        private async Task NavigateToChallenge(string token, string username, string password)
        {
            var route = $"challenge?ChallengeToken={Uri.EscapeDataString(token)}"
                     + $"&Username={Uri.EscapeDataString(username)}"
                     + $"&Password={Uri.EscapeDataString(password)}";
            await Shell.Current.GoToAsync(route);
        }

        // Implement missing IAuthService methods as stubs for build
        // Implemented wrappers for client endpoints
        public async Task<PaginatedComments> GetCommentsAsync(string mediaId, string cursor = null)
        {
            // call generated client extension
            return await _client.GetCommentsAsync(mediaId, cursor: cursor);
        }

        public async Task<PaginatedMedias> GetMediasAsync(string accountId, int? limit = null, string cursor = null)
        {
            var session = await LoadSessionAsync();
            return await _client.MediasAsync(session.Id, limit: limit, cursor: cursor);
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetReplyHistoryAsync(string accountId, string mediaId)
        {
            // Backend endpoint not present in generated client; throw for now so caller knows to implement
            throw new NotImplementedException("GetReplyHistoryAsync requires backend client extension; implement mapping to backend endpoint.");
        }

        public Task ClearReplyHistoryAsync(string accountId, string mediaId)
        {
            throw new NotImplementedException("ClearReplyHistoryAsync not yet implemented; needs backend endpoint mapping.");
        }

        public async Task SaveRuleAsync(RuleItem rule)
        {
            var inRule = new RuleIn
            {
                Account_id = rule.Account_id,
                Name = rule.Name,
                Expression = rule.Expression,
                Enabled = rule.Enabled
            };

            // Attach media_id via AdditionalProperties if needed
            if (!string.IsNullOrEmpty(rule.MediaId))
            {
                inRule.AdditionalProperties = inRule.AdditionalProperties ?? new Dictionary<string, object>();
                inRule.AdditionalProperties["media_id"] = rule.MediaId;
            }

            await _client.RulesPOSTAsync(inRule);
        }

        public async Task<string> ExportRulesAsync(string accountId, string format)
        {
            var page = await GetRulesAsync(accountId);
            return JsonConvert.SerializeObject(page.Items);
        }

        public Task ImportRulesAsync(string accountId, string data, string format)
        {
            throw new NotImplementedException("ImportRulesAsync not implemented; needs backend support.");
        }

        public async Task<PaginatedRules> GetRulesAsync(string accountId, int limit = 50, string cursor = null)
        {
            var page = await _client.RulesGETAsync(account_id: accountId, limit: limit, cursor: cursor);
            return page;
        }

        public async Task CreateRuleAsync(RuleIn rule)
        {
            await _client.RulesPOSTAsync(rule);
        }

        public Task<List<ActivityItem>> GetLiveActivitiesAsync(string accountId)
        {
            throw new NotImplementedException();
        }

        public Task<List<ReplyStatItem>> GetReplyStatsAsync(string accountId)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _loginThrottle.Dispose();
                _disposed = true;
            }
        }
    }

    // Add custom exception definitions if missing
    public class RateLimitException : Exception
    {
        public RateLimitException(string message) : base(message) { }
    }
    public class AuthenticationException : Exception
    {
        public AuthenticationException(string message) : base(message) { }
    }
}