// File: ViewModels/LogsViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using InstagramAuto.Client;
using InstagramAuto.Client.Models;
using InstagramAuto.Client.Services;
using Microsoft.Maui.Controls;

namespace InstagramAuto.Client.ViewModels
{
    /// <summary>
    /// Persian:
    ///   ویومدل صفحه Logs برای نمایش و صفحه‌بندی لاگ‌ها.
    /// English:
    ///   ViewModel for LogsPage to display and paginate log entries.
    /// </summary>
    public class LogsViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly InstagramAutoClient _apiClient;
        private bool _isBusy;
        private string _errorMessage;
        private string _cursor;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Persian: مجموعه‌ای از لاگ‌ها  
        /// English: Collection of log entries  
        /// </summary>
        public ObservableCollection<LogEntryDto> Items { get; } = new ObservableCollection<LogEntryDto>();

        /// <summary>
        /// Persian: آیا در حال بارگذاری است؟  
        /// English: Indicates if loading is in progress  
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
                ((Command)LoadMoreCommand).ChangeCanExecute();
            }
        }

        /// <summary>
        /// Persian: پیام خطا در صورت عدم موفقیت  
        /// English: Error message on failure  
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage == value) return;
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(HasError));
            }
        }

        /// <summary>
        /// Persian: آیا خطا نمایش داده شود؟  
        /// English: Whether to show error message  
        /// </summary>
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        /// <summary>
        /// Persian: دستور بارگذاری صفحه بعد  
        /// English: Command to load next page  
        /// </summary>
        public ICommand LoadMoreCommand { get; }

        public LogsViewModel(
            IAuthService authService,
            InstagramAutoClient apiClient)
        {
            _authService = authService;
            _apiClient = apiClient;

            LoadMoreCommand = new Command(async () => await LoadMoreAsync(), () => !IsBusy);
        }

        /// <summary>
        /// Persian:
        ///   متد بارگذاری صفحه بعدی لاگ‌ها.
        /// English:
        ///   Loads the next page of log entries.
        /// </summary>
        public async Task LoadMoreAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var session = await _authService.LoadSessionAsync();
                var page = await _apiClient.ListLogsAsync(
                    level: null,
                    limit: 50,
                    cursor: _cursor);

                foreach (var log in page.Items)
                    if (!Items.Any(x => x.Id == log.Id))
                        Items.Add(log);

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

        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
