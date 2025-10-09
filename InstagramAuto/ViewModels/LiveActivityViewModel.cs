using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using InstagramAuto.Client.Models;
using InstagramAuto.Client.Services;
using Microsoft.Maui.Controls;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace InstagramAuto.Client.ViewModels
{
    /// <summary>
    /// Persian:
    ///     ?????? ????? ?????????? ????.
    /// English:
    ///     ViewModel for displaying live activities.
    /// </summary>
    public class LiveActivityViewModel : BaseViewModel, IDisposable
    {
        private readonly IAuthService _authService;
        private readonly Timer _refreshTimer;
        private bool _isRefreshing;
        private string _errorMessage;
        private ObservableCollection<ActivityGroupViewModel> _activityGroups;
        private ActivityItemViewModel _selectedActivity;
        private bool _isPaused;
        private readonly SemaphoreSlim _refreshSemaphore = new(1, 1);

        public ObservableCollection<ActivityGroupViewModel> ActivityGroups
        {
            get => _activityGroups;
            set { _activityGroups = value; OnPropertyChanged(); }
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set 
            { 
                _isRefreshing = value; 
                OnPropertyChanged();
                ((Command)RefreshCommand).ChangeCanExecute();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public bool IsPaused
        {
            get => _isPaused;
            set 
            { 
                _isPaused = value;
                OnPropertyChanged();
                if (_isPaused)
                    _refreshTimer.Change(Timeout.Infinite, Timeout.Infinite);
                else
                    _refreshTimer.Change(0, 10000); // 10 seconds
            }
        }

        public ActivityItemViewModel SelectedActivity
        {
            get => _selectedActivity;
            set { _selectedActivity = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand TogglePauseCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand CopyDetailsCommand { get; }
        public ICommand ShareCommand { get; }
        public ICommand FilterCommand { get; }

        public LiveActivityViewModel(IAuthService authService)
        {
            _authService = authService;
            ActivityGroups = new ObservableCollection<ActivityGroupViewModel>();
            
            RefreshCommand = new Command(async () => await RefreshAsync(), () => !IsRefreshing);
            TogglePauseCommand = new Command(() => IsPaused = !IsPaused);
            ClearCommand = new Command(ClearActivities);
            CopyDetailsCommand = new Command<ActivityItemViewModel>(CopyActivityDetails);
            ShareCommand = new Command<ActivityItemViewModel>(ShareActivity);
            FilterCommand = new Command<string>(FilterActivities);

            // Auto-refresh timer
            _refreshTimer = new Timer(async _ => await RefreshAsync(), null, 0, 10000);
        }

        public async Task InitializeAsync()
        {
            await RefreshAsync();
        }

        private async Task RefreshAsync()
        {
            if (!await _refreshSemaphore.WaitAsync(0)) return;

            try
            {
                IsRefreshing = true;
                ErrorMessage = null;

                var session = await _authService.LoadSessionAsync();
                var activities = await _authService.GetLiveActivitiesAsync(session.AccountId);

                // Group by date (use Created_at)
                var groups = activities
                    .GroupBy(a => a.Created_at.Date)
                    .OrderByDescending(g => g.Key)
                    .Select(g => new ActivityGroupViewModel(g.Key, g.ToList()))
                    .ToList();

                // Update UI
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    ActivityGroups.Clear();
                    foreach (var group in groups)
                    {
                        ActivityGroups.Add(group);
                    }
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsRefreshing = false;
                _refreshSemaphore.Release();
            }
        }

        private void ClearActivities()
        {
            ActivityGroups.Clear();
        }

        private async void CopyActivityDetails(ActivityItemViewModel activity)
        {
            if (activity == null) return;

            var details = $"????: {activity.TimeText}\n" +
                         $"???: {activity.Description}\n" +
                         $"?????: {activity.Status}";

            await Clipboard.SetTextAsync(details);
            await Shell.Current.DisplayAlert("??? ??", "??????? ?????? ?? ????????? ??? ??.", "????");
        }

        private async void ShareActivity(ActivityItemViewModel activity)
        {
            if (activity == null) return;

            await Share.RequestAsync(new ShareTextRequest
            {
                Text = $"???: {activity.Description}\n" +
                       $"????: {activity.TimeText}\n" +
                       $"?????: {activity.Status}",
                Title = "???????????? ??????"
            });
        }

        private void FilterActivities(string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                // Show all
                foreach (var group in ActivityGroups)
                {
                    foreach (var activity in group)
                    {
                        activity.IsVisible = true;
                    }
                }
                return;
            }

            // Filter by status
            foreach (var group in ActivityGroups)
            {
                foreach (var activity in group)
                {
                    activity.IsVisible = string.Equals(activity.Status, status, StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        public void Dispose()
        {
            _refreshTimer?.Dispose();
            _refreshSemaphore?.Dispose();
        }
    }

    public class ActivityGroupViewModel : ObservableCollection<ActivityItemViewModel>
    {
        public DateTime Date { get; }
        public string DateText { get; }
        public int Count { get; }

        public ActivityGroupViewModel(DateTime date, List<ActivityItem> activities)
        {
            Date = date;
            DateText = FormatDate(date);
            Count = activities.Count;

            foreach (var activity in activities.OrderByDescending(a => a.Created_at))
            {
                Add(new ActivityItemViewModel(activity));
            }
        }

        private string FormatDate(DateTime date)
        {
            var persianCalendar = new System.Globalization.PersianCalendar();
            return $"{persianCalendar.GetYear(date)}/{persianCalendar.GetMonth(date):00}/{persianCalendar.GetDayOfMonth(date):00}";
        }
    }

    public class ActivityItemViewModel : BaseViewModel
    {
        private readonly ActivityItem _activity;
        private bool _isVisible = true;

        public ActivityItemViewModel(ActivityItem activity)
        {
            _activity = activity;
        }

        public string Description => _activity.Data?.ToString() ?? _activity.Type ?? _activity.Id;
        public string Status => _activity.Status;
        public DateTimeOffset Timestamp => _activity.Created_at;
        public string TimeText => Timestamp.ToLocalTime().ToString("HH:mm:ss");

        public bool IsVisible
        {
            get => _isVisible;
            set { _isVisible = value; OnPropertyChanged(); }
        }

        public Color StatusColor => Status?.ToLowerInvariant() switch
        {
            "success" => Colors.Green,
            "warning" => Colors.Orange,
            "error" => Colors.Red,
            "in_progress" => Colors.Blue,
            _ => Colors.Gray
        };

        public string StatusIcon => Status?.ToLowerInvariant() switch
        {
            "success" => "?",
            "warning" => "!",
            "error" => "?",
            "in_progress" => "…",
            _ => "•"
        };
    }
}
