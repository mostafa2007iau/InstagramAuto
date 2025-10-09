using System;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ??? ????? ??? ????
    /// English: Model for creating a new session
    /// </summary>
    public class SessionCreateIn
    {
        [JsonProperty("account_id")]
        public string Account_id { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("proxy")]
        public string Proxy { get; set; }

        [JsonProperty("proxy_enabled")]
        public bool Proxy_enabled { get; set; }
    }

    /// <summary>
    /// Persian: ??? ????? ????? ???
    /// English: Session output model
    /// </summary>
    public class SessionOut
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("account_id")]
        public string Account_id { get; set; }

        [JsonProperty("session_blob")]
        public string Session_blob { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset Created_at { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset Updated_at { get; set; }
    }

    /// <summary>
    /// Persian: ????? ???? ???? ???
    /// English: Session import result
    /// </summary>
    public class SessionImportResult
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}