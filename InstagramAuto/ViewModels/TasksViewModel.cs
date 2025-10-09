using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using InstagramAuto.Client.Models;
using InstagramAuto.Client.Services;
using InstagramAuto.Client.Helpers;

namespace InstagramAuto.Client.ViewModels
{
    /// <summary>
    /// Persian:
    ///   ویومدل صفحه Tasks برای نمایش و مدیریت وظایف صف.
    ///   این کلاس مسئول نمایش و مدیریت وظایف در صف است.
    /// English:
    ///   ViewModel for TasksPage to display and manage queue jobs.
    ///   This class handles displaying and managing queued tasks.
    /// </summary>
    public class TasksViewModel : BaseViewModel, IDisposable
    {
        private readonly IAuthService _authService;
        private readonly InstagramAutoClient _apiClient;
        private readonly ILogger<TasksViewModel> _logger;
        private bool _isBusy;
        private string _errorMessage;
        private string _errorDetails;
        private string _cursor;
        private CancellationTokenSource _cts;
        private bool _disposed;

        /// <summary>
        /// Persian: لیست وظایف صف  
        /// English: List of queue jobs.
        /// </summary>
        public ObservableCollection<JobSummaryDto> Items { get; } = new();

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                OnPropertyChanged();
                UpdateCommandStates();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            private set
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
            private set
            {
                if (_errorDetails == value) return;
                _errorDetails = value;
                OnPropertyChanged();
            }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        /// <summary>
        /// Persian: دستور بارگذاری وظایف (صفحه‌بندی شده)
        /// English: Command to load paginated jobs
        /// </summary>
        public ICommand LoadMoreCommand { get; }

        /// <summary>
        /// Persian: دستور تکرار کار  
        /// English: Command to retry a job
        /// </summary>
        public ICommand RetryCommand { get; }

        /// <summary>
        /// Persian: دستور لغو کار  
        /// English: Command to cancel a job
        /// </summary>
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Persian: دستور انتقال به DLQ  
        /// English: Command to move a job to DLQ
        /// </summary>
        public ICommand DlqCommand { get; }

        /// <summary>
        /// Persian: دستور لغو عملیات در حال اجرا
        /// English: Command to cancel ongoing operation
        /// </summary>
        public ICommand CancelOperationCommand { get; }

        public TasksViewModel(
            IAuthService authService,
            InstagramAutoClient apiClient,
            ILogger<TasksViewModel> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            LoadMoreCommand = new Command(async () => await LoadMoreAsync());
            RetryCommand = new Command<JobSummaryDto>(async item => await ExecuteActionAsync(item, ActionType.Retry));
            CancelCommand = new Command<JobSummaryDto>(async item => await ExecuteActionAsync(item, ActionType.Cancel));
            DlqCommand = new Command<JobSummaryDto>(async item => await ExecuteActionAsync(item, ActionType.Dlq));
            CancelOperationCommand = new Command(CancelOperation, () => IsBusy);

            _logger.LogInformation("TasksViewModel initialized");
        }

        private void UpdateCommandStates()
        {
            ((Command)LoadMoreCommand).ChangeCanExecute();
            ((Command)CancelOperationCommand).ChangeCanExecute();
        }

        /// <summary>
        /// Persian: بارگذاری صفحه بعد از وظایف
        /// English: Loads next page of jobs
        /// </summary>
        public async Task LoadMoreAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            ClearError();

            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            try
            {
                var session = await _authService.LoadSessionAsync();
                _logger.LogDebug("Loading jobs page with cursor {Cursor}", _cursor ?? "null");

                var page = await _apiClient.ListJobsAsync(
                    cursor: _cursor,
                    limit: 20,
                    cancellationToken: _cts.Token);

                foreach (var job in page.Items)
                {
                    if (!Items.Any(x => x.Id == job.Id))
                    {
                        Items.Add(job);
                        _logger.LogTrace("Added job {JobId} to list", job.Id);
                    }
                }

                _cursor = page.Meta.NextCursor;
                _logger.LogDebug("Loaded {Count} jobs, next cursor: {NextCursor}",
                    page.Items.Count(), _cursor ?? "null");
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Load operation was cancelled");
            }
            catch (Exception ex)
            {
                var parsed = ErrorHelper.Parse(ex);
                SetError(parsed.Message, parsed.Details);
                _logger.LogError(ex, "Failed to load jobs");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private enum ActionType { Retry, Cancel, Dlq }

        private async Task ExecuteActionAsync(JobSummaryDto item, ActionType type)
        {
            if (item == null || IsBusy) return;

            IsBusy = true;
            ClearError();

            try
            {
                _logger.LogInformation("Executing {Action} action on job {JobId}", type, item.Id);

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

                // Refresh after action
                Items.Clear();
                _cursor = null;
                await LoadMoreAsync();

                _logger.LogInformation("{Action} action completed successfully on job {JobId}", 
                    type, item.Id);
            }
            catch (Exception ex)
            {
                var parsed = ErrorHelper.Parse(ex);
                SetError(parsed.Message, parsed.Details);
                _logger.LogError(ex, "Failed to execute {Action} action on job {JobId}", 
                    type, item.Id);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void CancelOperation()
        {
            _cts?.Cancel();
            _logger.LogInformation("Operation cancelled by user");
        }

        private void ClearError()
        {
            ErrorMessage = string.Empty;
            ErrorDetails = string.Empty;
        }

        private void SetError(string message, string details)
        {
            ErrorMessage = message;
            ErrorDetails = details;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _cts?.Dispose();
                _disposed = true;
            }
        }
    }
}
