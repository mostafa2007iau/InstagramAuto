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
    ///   ?????? ????? ???? ????????? ???? ?? ???.
    /// English:
    ///   ViewModel for displaying reply stats per post.
    /// </summary>
    public class StatsViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private bool _isBusy;
        private string _errorMessage;

        public ObservableCollection<ReplyStatItem> Items { get; } = new ObservableCollection<ReplyStatItem>();
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); ((Command)RefreshCommand).ChangeCanExecute(); } }
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); } }
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public ICommand RefreshCommand { get; }

        public StatsViewModel(IAuthService authService)
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
                var stats = await _authService.GetReplyStatsAsync(session.AccountId);
                Items.Clear();
                foreach (var s in stats)
                    Items.Add(s);
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
