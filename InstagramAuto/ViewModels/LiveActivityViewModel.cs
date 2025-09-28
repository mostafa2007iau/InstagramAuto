using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using InstagramAuto.Client.Models;
using InstagramAuto.Client.Services;

namespace InstagramAuto.Client.ViewModels
{
    /// <summary>
    /// Persian:
    ///   ?????? ????? ?????????? ????.
    /// English:
    ///   ViewModel for displaying live activities.
    /// </summary>
    public class LiveActivityViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private bool _isBusy;
        private string _errorMessage;

        public ObservableCollection<ActivityItem> Items { get; } = new ObservableCollection<ActivityItem>();
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); ((Command)RefreshCommand).ChangeCanExecute(); } }
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); } }
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public ICommand RefreshCommand { get; }

        public LiveActivityViewModel(IAuthService authService)
        {
            _authService = authService;
            RefreshCommand = new Command(async () => await LoadAsync(), () => !IsBusy);
        }

        public async Task LoadAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = string.Empty;
            try
            {
                var session = await _authService.LoadSessionAsync();
                var activities = await _authService.GetLiveActivitiesAsync(session.AccountId);
                Items.Clear();
                foreach (var a in activities)
                    Items.Add(a);
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
