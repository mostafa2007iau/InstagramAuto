using InstagramAuto.Client;
using InstagramAuto.Client.Services;
using InstagramAuto.Client.Views;
using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using InstagramAuto.Client.Models;

namespace InstagramAuto.Client.ViewModels
{
    [QueryProperty(nameof(ChallengeToken), "ChallengeToken")]
    [QueryProperty(nameof(Username), "Username")]
    [QueryProperty(nameof(Password), "Password")]
    public class ChallengeViewModel : BaseViewModel
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
            try
            {
                var state = await _challengeService.GetStateAsync(ChallengeToken);
                if (state != null)
                {
                    var parts = new List<string>();
                    if (state.Payload != null)
                    {
                        foreach (var kv in state.Payload)
                            parts.Add($"{kv.Key}={kv.Value}");
                    }
                    ChallengeInfo = $"Type: {state.Type}, Info: {string.Join(", ", parts)}";
                }
                else
                {
                    ChallengeInfo = "No challenge state available.";
                }
            }
            catch (Exception ex)
            {
                ChallengeInfo = "Failed to load challenge state.";
                StatusMessage = ex.Message;
            }
        }

        private async Task SubmitCode()
        {
            try
            {
                StatusMessage = string.Empty;
                var payload = new Dictionary<string, object>
                {
                    { "code", Code }
                };

                var ok = await _challengeService.ResolveAsync(ChallengeToken, payload);

                if (ok)
                {
                    try
                    {
                        var session = await _authService.LoginAsync(Username, Password);

                        if (session != null && string.IsNullOrEmpty(session.ChallengeToken))
                        {
                            await Shell.Current.GoToAsync($"//{nameof(DashboardPage)}");
                        }
                        else
                        {
                            StatusMessage = "Challenge still required.";
                        }
                    }
                    catch (Exception ex)
                    {
                        // Auth failed after resolving challenge
                        StatusMessage = ex.Message;
                    }
                }
                else
                {
                    StatusMessage = "Invalid code or failed to resolve challenge.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }
    }
}
