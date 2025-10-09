using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using System.Threading;
using InstagramAuto.Client.Services;
using InstagramAuto.Client.Models;

namespace InstagramAuto.Client.ViewModels
{
    public class HealthViewModel : BaseViewModel
    {
        private readonly IHealthMonitor _healthMonitor;
        private bool _isBusy;
        private string _errorMessage;
        private string _errorDetails;
        private bool _isAutoRefreshEnabled;
        private IDisposable _refreshTimer;

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                OnPropertyChanged();
                ((Command)RefreshCommand).ChangeCanExecute();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage == value) return;
                _errorMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasError));
            }
        }

        public string ErrorDetails
        {
            get => _errorDetails;
            set
            {
                if (_errorDetails == value) return;
                _errorDetails = value;
                OnPropertyChanged();
            }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public bool IsAutoRefreshEnabled
        {
            get => _isAutoRefreshEnabled;
            set
            {
                if (_isAutoRefreshEnabled == value) return;
                _isAutoRefreshEnabled = value;
                OnPropertyChanged();
                UpdateAutoRefresh();
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand ToggleAutoRefreshCommand { get; }
        
        public ServiceStatusViewModel InstagramService { get; } = new();
        public ServiceStatusViewModel QueueService { get; } = new();
        public ServiceStatusViewModel DatabaseService { get; } = new();
        public ServiceStatusViewModel CacheService { get; } = new();

        public HealthViewModel(IHealthMonitor healthMonitor)
        {
            _healthMonitor = healthMonitor;

            RefreshCommand = new Command(async () => await RefreshAsync(), () => !IsBusy);
            ToggleAutoRefreshCommand = new Command(() => IsAutoRefreshEnabled = !IsAutoRefreshEnabled);
        }

        public async Task LoadAsync()
        {
            await RefreshAsync();
        }

        private void MapComponentToViewModel(ServiceStatusViewModel vm, HealthComponentStatus component)
        {
            if (component == null)
            {
                vm.Name = "-";
                vm.Status = "unknown";
                vm.Details = "No data";
                vm.LastChecked = DateTimeOffset.MinValue;
                vm.IsHealthy = false;
                vm.UsagePercent = 0;
                return;
            }
            vm.Name = component.Name;
            vm.Status = component.Status;
            vm.Details = component.Message;
            vm.LastChecked = component.Last_check;
            vm.IsHealthy = component.Status == "healthy" || component.Status == "operational";
            vm.UsagePercent = 0; // If you have usage info, set it here
        }

        private async Task RefreshAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;
            ErrorDetails = string.Empty;

            try
            {
                var health = await _healthMonitor.GetHealthStatusAsync();
                // If health is null, skip
                if (health == null)
                {
                    MapComponentToViewModel(InstagramService, null);
                    MapComponentToViewModel(QueueService, null);
                    MapComponentToViewModel(DatabaseService, null);
                    MapComponentToViewModel(CacheService, null);
                }
                else
                {
                    // Try to get HealthComponentStatus from health if available
                    var instagram = health.GetType().GetProperty("Instagram")?.GetValue(health) as HealthComponentStatus;
                    var queue = health.GetType().GetProperty("Queue")?.GetValue(health) as HealthComponentStatus;
                    var database = health.GetType().GetProperty("Database")?.GetValue(health) as HealthComponentStatus;
                    var cache = health.GetType().GetProperty("Cache")?.GetValue(health) as HealthComponentStatus;
                    MapComponentToViewModel(InstagramService, instagram);
                    MapComponentToViewModel(QueueService, queue);
                    MapComponentToViewModel(DatabaseService, database);
                    MapComponentToViewModel(CacheService, cache);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                ErrorDetails = ex.ToString();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void UpdateAutoRefresh()
        {
            _refreshTimer?.Dispose();

            if (IsAutoRefreshEnabled)
            {
                _refreshTimer = new Timer(async _ => await RefreshAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
            }
        }
    }
}