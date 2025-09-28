using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace InstagramAuto.Client.Models
{
    // -------------------------
    // Session
    // -------------------------
    public class SessionCreateIn
    {
        [JsonPropertyName("account_id")]
        public string Account_id { get; set; }

        [JsonPropertyName("proxy")]
        public string Proxy { get; set; }

        [JsonPropertyName("proxy_enabled")]
        public bool Proxy_enabled { get; set; }

        [JsonPropertyName("locale")]
        public string Locale { get; set; }
    }

    public class SessionOut
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("account_id")]
        public string Account_id { get; set; }

        [JsonPropertyName("locale")]
        public string Locale { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime Created_at { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime Updated_at { get; set; }
    }

    // -------------------------
    // Login
    // -------------------------
    public class LoginSuccessResponse
    {
        [JsonPropertyName("result")]
        public LoginResult Result { get; set; }

        [JsonPropertyName("log")]
        public object Log { get; set; }
    }

    public class LoginResult
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    // -------------------------
    // Proxy
    // -------------------------
    public class ProxyTestResponse
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    public class ProxyToggleResponse
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("proxy_enabled")]
        public bool ProxyEnabled { get; set; }
    }

    // -------------------------
    // Medias
    // -------------------------
    public class MediaItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("caption")]
        public string Caption { get; set; }

        [JsonPropertyName("thumbnail_url")]
        public string ThumbnailUrl { get; set; }
    }

    public class PaginationMeta
    {
        [JsonPropertyName("next_cursor")]
        public string NextCursor { get; set; }

        [JsonPropertyName("count_returned")]
        public int CountReturned { get; set; }
    }

    public class PaginatedMedias
    {
        [JsonPropertyName("items")]
        public List<MediaItem> Items { get; set; }

        [JsonPropertyName("meta")]
        public PaginationMeta Meta { get; set; }
    }

    // -------------------------
    // Rules
    // -------------------------
    public class RuleIn
    {
        [JsonPropertyName("account_id")]
        public string AccountId { get; set; }

        [JsonPropertyName("condition")]
        public string Condition { get; set; }

        [JsonPropertyName("action")]
        public string Action { get; set; }
    }

    public class RuleOut
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("account_id")]
        public string AccountId { get; set; }

        [JsonPropertyName("condition")]
        public string Condition { get; set; }

        [JsonPropertyName("action")]
        public string Action { get; set; }
    }

    public class PaginatedRules
    {
        [JsonPropertyName("items")]
        public List<RuleOut> Items { get; set; }

        [JsonPropertyName("meta")]
        public PaginationMeta Meta { get; set; }
    }

    // -------------------------
    // Challenge
    // -------------------------
    public class ChallengeState
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("data")]
        public Dictionary<string, object> Data { get; set; }
    }

    // -------------------------
    // Inbound Event
    // -------------------------
    public class Response
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("matched")]
        public object Matched { get; set; }
    }

    // -------------------------
    // Logs
    // -------------------------
    public class LogOut
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("account_id")]
        public string AccountId { get; set; }

        [JsonPropertyName("level")]
        public string Level { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    public class PaginatedLogs
    {
        [JsonPropertyName("items")]
        public List<LogOut> Items { get; set; }

        [JsonPropertyName("meta")]
        public PaginationMeta Meta { get; set; }
    }

    // -------------------------
    // Health
    // -------------------------
    public class HealthResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("metrics")]
        public Dictionary<string, object> Metrics { get; set; }
    }

    // -------------------------
    // Session Import
    // -------------------------
    public class SessionImportResult
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("session_id")]
        public string SessionId { get; set; }
    }
}
