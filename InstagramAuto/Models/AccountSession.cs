using System;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian:
    ///   اطلاعات سشن لاگین‌شده کاربر.
    /// English:
    ///   Represents an authenticated session for the user.
    /// </summary>
    public class AccountSession
    {
        /// <summary>
        /// Persian: شناسه‌ی سشن  
        /// English: The session identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Persian: شناسه‌ی حساب  
        /// English: The account identifier.
        /// </summary>
        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        /// <summary>
        /// Persian: blob سریالایز‌شدهٔ سشن  
        /// English: Serialized session blob.
        /// </summary>
        [JsonProperty("session_blob")]
        public string SessionBlob { get; set; }

        /// <summary>
        /// Persian: زمان ایجاد سشن  
        /// English: Creation timestamp.
        /// </summary>
        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Persian: زمان آخرین به‌روزرسانی سشن  
        /// English: Last updated timestamp.
        /// </summary>
        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("challenge_token")]
        public string ChallengeToken { get; set; }
    }
}
