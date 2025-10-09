using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using InstagramAuto.Client.Models;
using InstagramAuto.Client.Services;
using Microsoft.Maui.Controls;
using System.Linq;

namespace InstagramAuto.Client.ViewModels
{
    public class ReplyHistoryViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private bool _isRefreshing;
        private bool _isBusy;
        private string _errorMessage;
        private MediaItem _selectedPost;
        private ObservableCollection<MediaItem> _posts;
        private ObservableCollection<ReplyHistoryItemViewModel> _history;

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set { _isRefreshing = value; OnPropertyChanged(); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set 
            { 
                _errorMessage = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public MediaItem SelectedPost
        {
            get => _selectedPost;
            set 
            { 
                _selectedPost = value; 
                OnPropertyChanged();
                LoadHistoryAsync().ConfigureAwait(false);
            }
        }

        public ObservableCollection<MediaItem> Posts
        {
            get => _posts;
            set { _posts = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ReplyHistoryItemViewModel> History
        {
            get => _history;
            set { _history = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand ClearHistoryCommand { get; }

        public ReplyHistoryViewModel(IAuthService authService)
        {
            _authService = authService;
            Posts = new ObservableCollection<MediaItem>();
            History = new ObservableCollection<ReplyHistoryItemViewModel>();

            RefreshCommand = new Command(async () => await RefreshAsync());
            ClearHistoryCommand = new Command(async () => await ClearHistoryAsync());
        }

        public async Task InitializeAsync()
        {
            await LoadPostsAsync();
        }

        private async Task LoadPostsAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = null;

                var session = await _authService.LoadSessionAsync();
                var medias = await _authService.GetMediasAsync(session.AccountId);

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Posts.Clear();
                    foreach (var media in medias.Items)
                    {
                        Posts.Add(media);
                    }
                });
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

        private async Task LoadHistoryAsync()
        {
            if (IsBusy || SelectedPost == null) return;

            try
            {
                IsBusy = true;
                ErrorMessage = null;

                var session = await _authService.LoadSessionAsync();
                var history = await _authService.GetReplyHistoryAsync(
                    session.AccountId,
                    SelectedPost.Id);

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    History.Clear();
                    foreach (var item in history)
                    {
                        History.Add(new ReplyHistoryItemViewModel(item));
                    }
                });
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

        private async Task RefreshAsync()
        {
            IsRefreshing = true;
            await LoadHistoryAsync();
            IsRefreshing = false;
        }

        private async Task ClearHistoryAsync()
        {
            if (SelectedPost == null) return;

            var confirm = await Shell.Current.DisplayAlert(
                "?????",
                "??? ?? ??? ???? ??????? ??????? ??????",
                "???",
                "???");

            if (!confirm) return;

            try
            {
                IsBusy = true;
                ErrorMessage = null;

                var session = await _authService.LoadSessionAsync();
                await _authService.ClearReplyHistoryAsync(
                    session.AccountId,
                    SelectedPost.Id);

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    History.Clear();
                });
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

    public class ReplyHistoryItemViewModel
    {
        private readonly Dictionary<string, object> _data;

        public ReplyHistoryItemViewModel(Dictionary<string, object> data)
        {
            _data = data;
            TimestampText = FormatTimestamp(data["timestamp"].ToString());
        }

        public string CommentText => _data["comment_text"]?.ToString();
        public string ReplyText => _data["reply_text"]?.ToString();
        public string RuleName => _data["rule_name"]?.ToString();
        public string TimestampText { get; }

        private string FormatTimestamp(string isoDate)
        {
            if (DateTime.TryParse(isoDate, out var date))
            {
                var persianCalendar = new System.Globalization.PersianCalendar();
                return $"{persianCalendar.GetYear(date)}/{persianCalendar.GetMonth(date):00}/{persianCalendar.GetDayOfMonth(date):00} {date:HH:mm}";
            }
            return isoDate;
        }
    }
}