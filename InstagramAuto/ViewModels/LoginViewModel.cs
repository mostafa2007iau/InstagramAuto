using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using InstagramAuto.Client.Models;
using InstagramAuto.Client.Services;

namespace InstagramAuto.Client.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private string _username;
        private string _password;
        private bool _isBusy;
        private string _errorMessage;
        private string _errorDetails;
        private string _importSessionText;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Username { get => _username; set { _username = value; OnPropertyChanged(nameof(Username)); } }
        public string Password { get => _password; set { _password = value; OnPropertyChanged(nameof(Password)); } }
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(nameof(IsBusy)); } }
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); OnPropertyChanged(nameof(HasError)); } }
        public string ErrorDetails { get => _errorDetails; set { _errorDetails = value; OnPropertyChanged(nameof(ErrorDetails)); } }
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public string ImportSessionText { get => _importSessionText; set { _importSessionText = value; OnPropertyChanged(nameof(ImportSessionText)); } }

        public ICommand LoginCommand { get; }
        public ICommand ImportSessionCommand { get; }

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
            LoginCommand = new Command(async () => await ExecuteLoginAsync(), () => !IsBusy);
            ImportSessionCommand = new Command(async () => await ExecuteImportSessionAsync(), () => !IsBusy);
        }

        private async Task ExecuteLoginAsync()
        {
            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;
                var session = await _authService.LoginAsync(Username, Password);
                if (!string.IsNullOrWhiteSpace(session?.ChallengeToken))
                {
                    await Shell.Current.GoToAsync($"challenge?ChallengeToken={session.ChallengeToken}&Username={Username}&Password={Password}");
                }
                else
                {
                    await Shell.Current.GoToAsync("///Home");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally { IsBusy = false; }
        }

        private async Task ExecuteImportSessionAsync()
        {
            if (string.IsNullOrWhiteSpace(ImportSessionText)) return;
            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                Dictionary<string, object> payload;
                try
                {
                    payload = JsonSerializer.Deserialize<Dictionary<string, object>>(ImportSessionText);
                }
                catch
                {
                    payload = new Dictionary<string, object>
                    {
                        ["account_id"] = Username ?? "imported",
                        ["session_blob"] = ImportSessionText.Trim()
                    };
                }

                var session = new AccountSession
                {
                    Id = payload.ContainsKey("id") ? payload["id"].ToString() : Guid.NewGuid().ToString(),
                    AccountId = payload["account_id"].ToString(),
                    SessionBlob = payload.ContainsKey("session_blob") ? payload["session_blob"].ToString() : null
                };

                await _authService.SaveSessionAsync(session);
                await Shell.Current.GoToAsync("///Home");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Import failed: {ex.Message}";
            }
            finally { IsBusy = false; }
        }

        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
