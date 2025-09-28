// InstagramAuto.Client.ViewModels/LoginViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using InstagramAuto.Client.Models;
using InstagramAuto.Client.Services;
using InstagramAuto.Client.Views;
using Newtonsoft.Json.Linq;

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

        public event PropertyChangedEventHandler PropertyChanged;

        public string Username
        {
            get => _username;
            set { if (_username == value) return; _username = value; OnPropertyChanged(nameof(Username)); }
        }

        public string Password
        {
            get => _password;
            set { if (_password == value) return; _password = value; OnPropertyChanged(nameof(Password)); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
                ((Command)LoginCommand).ChangeCanExecute();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage == value) return;
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(HasError));
            }
        }

        // Detailed error (traceback / full JSON) for copy action
        public string ErrorDetails
        {
            get => _errorDetails;
            set
            {
                if (_errorDetails == value) return;
                _errorDetails = value;
                OnPropertyChanged(nameof(ErrorDetails));
            }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public ICommand LoginCommand { get; }

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
            LoginCommand = new Command(async () => await ExecuteLoginAsync(), () => !IsBusy);
        }

        private async Task ExecuteLoginAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            ErrorMessage = string.Empty;
            ErrorDetails = string.Empty;

            try
            {
                // 1) احراز هویت
                var session = await _authService.LoginAsync(Username, Password);

                // 2) چک کردن challenge
                if (!string.IsNullOrWhiteSpace(session?.ChallengeToken))
                {
                    // به صفحه‌ی Challenge منتقل شو
                    var parameters = new Dictionary<string, object>
                    {
                        { "ChallengeToken", session.ChallengeToken },
                        { "Username",       Username               },
                        { "Password",       Password               }
                    };

                    await Shell.Current.GoToAsync(
                        $"{nameof(Views.ChallengePage)}",
                        true,
                        parameters
                    );
                }
                else
                {
                    // لاگین موفق → صفحه‌ی Home
                    await Shell.Current.GoToAsync($"//{nameof(DashboardPage)}");
                }
            }
            catch (Exception ex)
            {
                // try to extract structured details if backend included them
                try
                {
                    var msg = ex.Message ?? string.Empty;
                    var detailsIndex = msg.IndexOf("Details:");
                    if (detailsIndex >= 0)
                    {
                        ErrorMessage = msg.Substring(0, detailsIndex).Trim();
                        ErrorDetails = msg.Substring(detailsIndex + "Details:".Length).Trim();
                        // attempt to pretty-print JSON if possible
                        try
                        {
                            var parsed = JToken.Parse(ErrorDetails);
                            ErrorDetails = parsed.ToString(Newtonsoft.Json.Formatting.Indented);
                        }
                        catch { }
                    }
                    else
                    {
                        ErrorMessage = msg;
                    }
                }
                catch
                {
                    ErrorMessage = ex.Message;
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
