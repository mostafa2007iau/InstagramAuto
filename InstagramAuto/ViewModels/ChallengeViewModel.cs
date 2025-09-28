using InstagramAuto.Client;
using InstagramAuto.Client.Services;
using InstagramAuto.Client.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

[QueryProperty(nameof(ChallengeToken), "ChallengeToken")]
[QueryProperty(nameof(Username), "Username")]
[QueryProperty(nameof(Password), "Password")]
public class ChallengeViewModel : INotifyPropertyChanged
{
    private readonly ChallengeService _challengeService;
    private readonly AuthService _authService;

    public string ChallengeToken { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

    private string _code;
    private string _statusMessage;
    private string _challengeInfo;

    public string Code { get => _code; set { _code = value; OnPropertyChanged(); } }
    public string StatusMessage { get => _statusMessage; set { _statusMessage = value; OnPropertyChanged(); } }
    public string ChallengeInfo { get => _challengeInfo; set { _challengeInfo = value; OnPropertyChanged(); } }

    public ICommand SubmitCommand { get; }

    public ChallengeViewModel()
    {
        InstagramAutoClient apiclient = new InstagramAutoClient(new HttpClient());
        var http = new HttpClient { BaseAddress = new Uri(apiclient.BaseUrl) };
        _challengeService = new ChallengeService(http);
        var client = new InstagramAutoClient(http);
        _authService = new AuthService(client);

        SubmitCommand = new Command(async () => await SubmitCode());
    }

    public async Task LoadStateAsync()
    {
        var state = await _challengeService.GetStateAsync(ChallengeToken);
        ChallengeInfo = $"Type: {state.Type}, Info: {string.Join(", ", state.Payload.Select(kv => kv.Key + "=" + kv.Value))}";
    }

    private async Task SubmitCode()
    {
        // ۱) payload رو به صورت Dictionary آماده می‌کنیم
        var payload = new Dictionary<string, object>
    {
        { "code", Code }
    };

        // ۲) فراخوانی ResolveAsync با Dictionary
        var ok = await _challengeService.ResolveAsync(ChallengeToken, payload);

        if (ok)
        {
            // ۳) بعد از حل چالش، دوباره لاگین می‌کنیم
            var session = await _authService.LoginAsync(Username, Password);

            // ۴) اگر ChallengeToken خالی بود یعنی لاگین کامل موفق بود
            if (string.IsNullOrEmpty(session.ChallengeToken))
            {
                await Shell.Current.GoToAsync($"//{nameof(DashboardPage)}");
            }
            else
            {
                StatusMessage = "Challenge still required.";
            }
        }
        else
        {
            StatusMessage = "Invalid code.";
        }
    }


    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
