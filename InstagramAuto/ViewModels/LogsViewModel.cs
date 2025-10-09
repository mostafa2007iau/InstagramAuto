// File: ViewModels/LogsViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using InstagramAuto.Client;
using InstagramAuto.Client.Models;
using InstagramAuto.Client.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using InstagramAuto.Client.Helpers;

namespace InstagramAuto.Client.ViewModels
{
    /// <summary>
    /// Persian:
    ///   ویومدل صفحه Logs برای نمایش و صفحه‌بندی لاگ‌ها.
    ///   این کلاس مسئول نمایش و مدیریت لاگ‌های سیستم است.
    /// English:
    ///   ViewModel for LogsPage to display and paginate log entries.
    ///   This class handles displaying and managing system logs.
    /// </summary>
    public class LogsViewModel : BaseViewModel, IDisposable
    {
        private readonly IAuthService _authService;
        private readonly InstagramAutoClient _apiClient;
        private readonly ILogger<LogsViewModel> _logger;
        private readonly SemaphoreSlim _loadLock = new(1, 1);
        private CancellationTokenSource _cts;
        
        private bool _isBusy;
        private string _errorMessage;
        private string _errorDetails;
        private string _cursor;
        private string _selectedLevel;
        private bool _isRefreshing;
        private bool _disposed;

        /// <summary>
        /// Persian: مجموعه‌ای از لاگ‌ها  
        /// English: Collection of log entries  
        /// </summary>
        public ObservableCollection<LogEntryDto> Items { get; } = new();

        /// <summary>
        /// Persian: سطح لاگ انتخاب شده
        /// English: Selected log level
        /// </summary>
        public string SelectedLevel
        {
            get => _selectedLevel;
            set
            {
                if (_selectedLevel == value) return;
                _selectedLevel = value;
                OnPropertyChanged();
                _ = RefreshAsync();
            }
        }

        /// <summary>
        /// Persian: آیا در حال تازه‌سازی است؟
        /// English: Whether a refresh is in progress
        /// </summary>
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                if (_isRefreshing == value) return;
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }

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
        /// Persian: دستور بارگذاری صفحه بعد  
        /// English: Command to load next page  
        /// </summary>
        public ICommand LoadMoreCommand { get; }

        /// <summary>
        /// Persian: دستور تازه‌سازی
        /// English: Command to refresh
        /// </summary>
        public ICommand RefreshCommand { get; }

        /// <summary>
        /// Persian: دستور پاک کردن خطا
        /// English: Command to clear error
        /// </summary>
        public ICommand ClearErrorCommand { get; }

        /// <summary>
        /// Persian: دستور لغو عملیات
        /// English: Command to cancel operation
        /// </summary>
        public ICommand CancelCommand { get; }

        public LogsViewModel(
            IAuthService authService,
            InstagramAutoClient apiClient,
            ILogger<LogsViewModel> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            LoadMoreCommand = new Command(async () => await LoadMoreAsync());
            RefreshCommand = new Command(async () => await RefreshAsync());
            ClearErrorCommand = new Command(ClearError);
            CancelCommand = new Command(CancelOperation, () => IsBusy);

            _logger.LogInformation("LogsViewModel initialized");
        }

        private void UpdateCommandStates()
        {
            ((Command)LoadMoreCommand).ChangeCanExecute();
            ((Command)CancelCommand).ChangeCanExecute();
        }

        /// <summary>
        /// Persian: تازه‌سازی لیست لاگ‌ها
        /// English: Refresh log list
        /// </summary>
        public async Task RefreshAsync()
        {
            if (IsBusy) return;

            IsRefreshing = true;
            ClearError();
            Items.Clear();
            _cursor = null;

            try
            {
                await LoadMoreAsync();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        /// <summary>
        /// Persian: بارگذاری صفحه بعد از لاگ‌ها
        /// English: Load next page of logs
        /// </summary>
        public async Task LoadMoreAsync()
        {
            if (IsBusy) return;

            await _loadLock.WaitAsync();
            try
            {
                IsBusy = true;
                ClearError();

                _cts?.Dispose();
                _cts = new CancellationTokenSource();

                _logger.LogDebug(
                    "Loading logs page. Level: {Level}, Cursor: {Cursor}",
                    _selectedLevel ?? "all", _cursor ?? "start");

                var session = await _authService.LoadSessionAsync();
                var page = await _apiClient.ListLogsAsync(
                    level: _selectedLevel,
                    limit: 50,
                    cursor: _cursor,
                    cancellationToken: _cts.Token);

                foreach (var log in page.Items)
                {
                    if (!Items.Any(x => x.Id == log.Id))
                    {
                        Items.Add(log);
                        _logger.LogTrace("Added log {LogId} to list", log.Id);
                    }
                }

                _cursor = page.Meta?.NextCursor;
                _logger.LogDebug(
     "Loaded {Count} logs, next cursor: {NextCursor}",
      page.Items.Count(), _cursor ?? "end");

            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Log loading operation was cancelled");
            }
            catch (Exception ex)
            {
                var parsed = ErrorHelper.Parse(ex);
                SetError(parsed.Message, parsed.Details);
                _logger.LogError(ex, "Failed to load logs");
            }
            finally
            {
                IsBusy = false;
                _loadLock.Release();
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
                _loadLock.Dispose();
                _disposed = true;
            }
        }
    }
}
