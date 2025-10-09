using System;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ??? ??? ????? ??????????
    /// English: Instagram account session model
    /// </summary>
    public class AccountSession
    {
        /// <summary>
        /// Persian: ????? ???
        /// English: Session ID
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Persian: ????? ?????
        /// English: Account ID
        /// </summary>
        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        /// <summary>
        /// Persian: ???????? ??? ?? ???? ????
        /// English: Session blob data
        /// </summary>
        [JsonProperty("session_blob")]
        public string SessionBlob { get; set; }

        /// <summary>
        /// Persian: ????? ?????
        /// English: Creation date
        /// </summary>
        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Persian: ????? ???????????
        /// English: Update date
        /// </summary>
        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        /// <summary>
        /// Persian: ????? ??????
        /// English: Proxy ID
        /// </summary>
        [JsonProperty("proxy_id")]
        public string ProxyId { get; set; }

        /// <summary>
        /// Persian: ???? ???? ???? ????? ???? ?? ????????
        /// English: Challenge token for 2FA
        /// </summary>
        [JsonProperty("challenge_token")]
        public string ChallengeToken { get; set; }
    }
}