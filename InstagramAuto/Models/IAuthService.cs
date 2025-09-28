using System.Threading.Tasks;
using InstagramAuto.Client.Models;

namespace InstagramAuto.Client.Services
{
    /// <summary>
    /// Persian:
    ///   سرویس احراز هویت، wrapper روی IInstagramAutoClient.
    /// English:
    ///   Authentication service, a wrapper over IInstagramAutoClient.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Persian:
        ///   لاگین به اینستاگرام و دریافت سشن.
        /// English:
        ///   Logs into Instagram and obtains a session.
        /// </summary>
        Task<AccountSession> LoginAsync(string username, string password);

        /// <summary>
        /// Persian:
        ///   بارگذاری سشن ذخیره‌شده از API.
        /// English:
        ///   Fetches saved session from the API.
        /// </summary>
        Task<AccountSession> LoadSessionAsync();

        /// <summary>
        /// Persian:
        ///   ذخیره‌سازی blob سشن در API.
        /// English:
        ///   Persists the session blob to the API.
        /// </summary>
        Task SaveSessionAsync(AccountSession session);
    }
}
