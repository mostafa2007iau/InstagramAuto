using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramAuto.Client.Models;

namespace InstagramAuto.Client.Services
{
    /// <summary>
    /// Persian:
    ///     ?????? ??????? ???????.
    /// English:
    ///     Manages user sessions.
    /// </summary>
    public interface ISessionManager
    {
        /// <summary>
        /// Persian:
        ///     ?????? ???? ??????? ????.
        /// English:
        ///     Get all active sessions.
        /// </summary>
        Task<IEnumerable<AccountSession>> GetAllSessionsAsync();

        /// <summary>
        /// Persian:
        ///     ?????? ??? ?? ?????.
        /// English:
        ///     Get session by id.
        /// </summary>
        Task<AccountSession> GetSessionAsync(string id);

        /// <summary>
        /// Persian:
        ///     ????? ???.
        /// English:
        ///     Save session.
        /// </summary>
        Task SaveSessionAsync(AccountSession session);

        /// <summary>
        /// Persian:
        ///     ??? ???.
        /// English:
        ///     Remove session.
        /// </summary>
        Task RemoveSessionAsync(string id);

        /// <summary>
        /// Persian:
        ///     ????????? ???.
        /// English:
        ///     Refresh session.
        /// </summary>
        Task<AccountSession> RefreshSessionAsync(string id);
    }
}