using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ???? ???? ????
    /// English: Successful login response
    /// </summary>
    public class LoginSuccessResponse
    {
        [JsonProperty("authenticated")]
        public bool Authenticated { get; set; }

        [JsonProperty("challenge_token")]
        public string ChallengeToken { get; set; }

        [JsonProperty("challenge_required")]
        public bool ChallengeRequired { get; set; }

        [JsonProperty("two_factor_required")]
        public bool TwoFactorRequired { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    /// <summary>
    /// Persian: ????? ???? ?? ?????
    /// English: Two-factor challenge state
    /// </summary>
    public class ChallengeState
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }

        [JsonPropertyName("payload")]
        public Dictionary<string, object>? Payload { get; set; }
    }
}