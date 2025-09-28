using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramAuto.Client.Models;

namespace InstagramAuto.Client.Services
{
    /// <summary>
    /// Persian:
    ///   ????? ????? ????? wrapper ??? IInstagramAutoClient.
    /// English:
    ///   Authentication service, a wrapper over IInstagramAutoClient.
    /// </summary>
    public interface IAuthService
    {
        Task<AccountSession> LoginAsync(string username, string password);
        Task<AccountSession> LoadSessionAsync();
        Task SaveSessionAsync(AccountSession session);

        // Comments
        Task<PaginatedComments> GetCommentsAsync(string mediaId, string cursor = null);

        // Rules
        Task SaveRuleAsync(RuleItem rule);
        Task<string> ExportRulesAsync(string accountId, string format);
        Task ImportRulesAsync(string accountId, string data, string format);
        Task<PaginatedRules> GetRulesAsync(string accountId, int limit = 50, string cursor = null);
        Task CreateRuleAsync(RuleIn rule);

        // Live Activity & Stats
        Task<List<ActivityItem>> GetLiveActivitiesAsync(string accountId);
        Task<List<ReplyStatItem>> GetReplyStatsAsync(string accountId);
    }
}
