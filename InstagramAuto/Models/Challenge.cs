using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: پاسخ چالش
    /// English: Challenge response model
    /// </summary>
    public class ChallengeResponse
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("choice")]
        public string Choice { get; set; }
    }

    /// <summary>
    /// Persian: درخواست چالش
    /// English: Challenge request model
    /// </summary>
    public class ChallengeRequest
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }
    }

    /// <summary>
    /// Persian: وضعیت چالش
    /// English: Challenge state model
    /// </summary>
    public class ChallengeStateExtended : ChallengeState
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }

        [JsonProperty("attempts_remaining")]
        public int Attempts_remaining { get; set; }

        [JsonProperty("expires_at")]
        public string Expires_at { get; set; }
    }
}