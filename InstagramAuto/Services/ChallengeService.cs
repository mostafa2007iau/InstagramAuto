using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;

public class ChallengeService
{
    private readonly HttpClient _http;

    public ChallengeService(HttpClient http)
    {
        _http = http;
    }

    // گرفتن وضعیت چالش
    public async Task<ChallengeState> GetStateAsync(string token)
    {
        var resp = await _http.GetAsync($"/api/challenge/{token}");
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        var env = JsonConvert.DeserializeObject<ChallengeEnvelope>(json);
        return env?.State;
    }

    // ارسال پاسخ (مثلاً کد)
    public async Task<bool> ResolveAsync(string token, Dictionary<string, object> payload)
    {
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var resp = await _http.PostAsync($"/api/challenge/{token}/resolve", content);
        return resp.IsSuccessStatusCode;
    }

    // گوش دادن به رویدادهای WebSocket
    public async Task ListenEventsAsync(string token, Action<ChallengeEvent> onEvent)
    {
        var wsUrl = new Uri(_http.BaseAddress, $"/ws/challenge/{token}")
            .ToString()
            .Replace("http", "ws");

        using var ws = new ClientWebSocket();
        await ws.ConnectAsync(new Uri(wsUrl), CancellationToken.None);

        var buffer = new byte[4096];
        while (ws.State == WebSocketState.Open)
        {
            var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
                break;

            var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var ev = JsonConvert.DeserializeObject<ChallengeEvent>(msg);
            onEvent?.Invoke(ev);
        }
    }
}
