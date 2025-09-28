using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using InstagramAuto.Client.Models;
using InstagramAuto.Client.Services;

namespace InstagramAuto.Client.ViewModels
{
    /// <summary>
    /// Persian:
    ///   ویومدل صفحه Tasks برای نمایش و مدیریت وظایف صف.
    /// English:
    ///   ViewModel for TasksPage to display and manage queue jobs.
    /// </summary>
    public class TasksViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly InstagramAutoClient _apiClient;
        private bool _isBusy;
        private string _errorMessage;
        private string _cursor;

        /// <summary>
        /// Persian: لیست وظایف صف  
        /// English: List of queue jobs.
        /// </summary>
        public ObservableCollection<JobSummaryDto> Items { get; } = new ObservableCollection<JobSummaryDto>();

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                OnPropertyChanged();
                ((Command)LoadMoreCommand).ChangeCanExecute();
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

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        /// <summary>
        /// Persian:
        ///   دستور بارگذاری وظایف (صفحه‌بندی شده).
        /// English:
        ///   Command to load paginated jobs.
        /// </summary>
        public ICommand LoadMoreCommand { get; }

        /// <summary>
        /// Persian:
        ///   دستور تکرار کار  
        /// English:
        ///   Command to retry a job.
        /// </summary>
        public ICommand RetryCommand { get; }

        /// <summary>
        /// Persian:
        ///   دستور لغو کار  
        /// English:
        ///   Command to cancel a job.
        /// </summary>
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Persian:
        ///   دستور انتقال به DLQ  
        /// English:
        ///   Command to move a job to DLQ.
        /// </summary>
        public ICommand DlqCommand { get; }

        public TasksViewModel(
            IAuthService authService,
            InstagramAutoClient apiClient)
        {
            _authService = authService;
            _apiClient = apiClient;

            LoadMoreCommand = new Command(async () => await LoadMoreAsync(), () => !IsBusy);
            RetryCommand = new Command<JobSummaryDto>(async item => await ExecuteActionAsync(item, ActionType.Retry));
            CancelCommand = new Command<JobSummaryDto>(async item => await ExecuteActionAsync(item, ActionType.Cancel));
            DlqCommand = new Command<JobSummaryDto>(async item => await ExecuteActionAsync(item, ActionType.Dlq));
        }

        /// <summary>
        /// Persian:
        ///   بارگذاری صفحه بعد از وظایف.
        /// English:
        ///   Loads next page of jobs.
        /// </summary>
        public async Task LoadMoreAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var session = await _authService.LoadSessionAsync();
                var page = await _apiClient.ListJobsAsync(cursor: _cursor, limit: 20);

                foreach (var j in page.Items)
                    if (!Items.Any(x => x.Id == j.Id))
                        Items.Add(j);

                _cursor = page.Meta.NextCursor;
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

        enum ActionType { Retry, Cancel, Dlq }

        async Task ExecuteActionAsync(JobSummaryDto item, ActionType type)
        {
            if (item == null) return;
            try
            {
                switch (type)
                {
                    case ActionType.Retry:
                        await _apiClient.RetryJobAsync(item.Id);
                        break;
                    case ActionType.Cancel:
                        await _apiClient.CancelJobAsync(item.Id);
                        break;
                    case ActionType.Dlq:
                        await _apiClient.MoveJobToDlqAsync(item.Id);
                        break;
                }
                // Refresh: پاک‌سازی و بارگذاری مجدد
                Items.Clear();
                _cursor = null;
                await LoadMoreAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}
