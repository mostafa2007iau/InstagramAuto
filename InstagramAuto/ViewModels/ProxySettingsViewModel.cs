using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using InstagramAuto.Client.Models;
using InstagramAuto.Client.Services;

namespace InstagramAuto.Client.ViewModels
{
    /// <summary>
    /// Persian:
    ///     ?????? ??????? ??????.
    /// English:
    ///     View model for proxy settings.
    /// </summary>
    public class ProxySettingsViewModel : BaseViewModel
    {
        private readonly IProxyService _proxyService;
        private Client.Models.ProxyConfig _config;
        private bool _isBusy;
        private string _errorMessage;

        public ICommand SaveCommand { get; }
        public ICommand TestCommand { get; }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string Address
        {
            get => _config?.Address;
            set
            {
                if (_config != null)
                {
                    _config.Address = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FullAddress));
                }
            }
        }

        public string Username
        {
            get => _config?.Username;
            set
            {
                if (_config != null)
                {
                    _config.Username = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Password
        {
            get => _config?.Password;
            set
            {
                if (_config != null)
                {
                    _config.Password = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Enabled
        {
            get => _config?.Enabled ?? false;
            set
            {
                if (_config != null)
                {
                    _config.Enabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public string FullAddress => _config?.FullAddress;

        public ProxySettingsViewModel(IProxyService proxyService)
        {
            _proxyService = proxyService;

            SaveCommand = new Command(async () => await SaveAsync(), () => !IsBusy);
            TestCommand = new Command(async () => await TestAsync(), () => !IsBusy);

            // fire-and-forget load
            _ = LoadConfigAsync();
        }

        public async Task LoadAsync()
        {
            await LoadConfigAsync();
        }

        private async Task LoadConfigAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                _config = await _proxyService.GetActiveProxyAsync();
                if (_config == null)
                    _config = new ProxyConfig();

                OnPropertyChanged(nameof(Address));
                OnPropertyChanged(nameof(Username));
                OnPropertyChanged(nameof(Password));
                OnPropertyChanged(nameof(Enabled));
                OnPropertyChanged(nameof(FullAddress));
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SaveAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                if (_config == null)
                    _config = new ProxyConfig();

                if (_config.Enabled)
                {
                    await _proxyService.SetActiveProxyAsync(_config);
                }
                else
                {
                    await _proxyService.DisableProxyAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task TestAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                var addressToTest = _config?.FullAddress ?? Address;
                var isWorking = await _proxyService.TestProxyAsync(addressToTest);
                ErrorMessage = isWorking ? "Proxy is working" : "Proxy test failed";
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}