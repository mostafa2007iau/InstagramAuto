using Newtonsoft.Json;

public class ChallengeState
{
    [JsonProperty("session_id")]
    public string SessionId { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("payload")]
    public Dictionary<string, object> Payload { get; set; }

    [JsonProperty("created_at")]
    public double CreatedAt { get; set; }
}

public class ChallengeEnvelope
{
    [JsonProperty("ok")]
    public bool Ok { get; set; }

    [JsonProperty("state")]
    public ChallengeState State { get; set; }
}

public class ChallengeEvent
{
    [JsonProperty("token")]
    public string Token { get; set; }

    [JsonProperty("event")]
    public string Event { get; set; }

    [JsonProperty("state")]
    public ChallengeState State { get; set; }

    [JsonProperty("response")]
    public Dictionary<string, object> Response { get; set; }
}
