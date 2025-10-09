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
    ///     ?????? ???? ?????? ? ???????.
    /// English:
    ///     ViewModel for reply and action statistics.
    /// </summary>
    public class StatsViewModel : BaseViewModel
    {
        private readonly IStatsCollector _statsCollector;
        private readonly IAuthService _authService;
        private ObservableCollection<ReplyStatItem> _replyStats = new();
        private string _accountId;
        private bool _isBusy;
        private string _errorMessage;

        public ObservableCollection<ReplyStatItem> ReplyStats { get => _replyStats; set { _replyStats = value; OnPropertyChanged(); } }
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }

        public StatsViewModel(IStatsCollector statsCollector, IAuthService authService)
        {
            _statsCollector = statsCollector;
            _authService = authService;
        }

        public async Task LoadAsync()
        {
            try
            {
                IsBusy = true;
                var session = await _authService.LoadSessionAsync();
                _accountId = session.AccountId;
                var stats = await _statsCollector.GetReplyStatsAsync(_accountId);
                ReplyStats = new ObservableCollection<ReplyStatItem>(stats);
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
