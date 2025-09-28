using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    ///   ویومدل صفحه Posts برای بارگذاری و نمایش پست‌ها/ریلز/تریلز.
    /// English:
    ///   ViewModel for PostsPage that loads and exposes media items.
    /// </summary>
    public class PostsViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly IInstagramAutoClient _apiClient;
        private bool _isBusy;
        private string _errorMessage;
        private string _cursor;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Persian: مجموعه‌ای قابل مشاهده از آیتم‌های مدیا  
        /// English: Observable collection of media items.
        /// </summary>
        public ObservableCollection<MediaItem> Items { get; } = new ObservableCollection<MediaItem>();

        /// <summary>
        /// Persian: آیا داده در حال بارگذاری است؟  
        /// English: Indicates loading in progress.
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
        /// English: Error message on failure.
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
        /// Persian: آیا نمایش خطا فعال باشد؟  
        /// English: Whether to show the error message.
        /// </summary>
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        /// <summary>
        /// Persian:
        ///   دستور بارگذاری داده اولیه یا صفحه بعد.
        /// English:
        ///   Command to load initial or next page of items.
        /// </summary>
        public ICommand LoadMoreCommand { get; }

        public PostsViewModel(
            IAuthService authService,
            IInstagramAutoClient apiClient)
        {
            _authService = authService;
            _apiClient = apiClient;
            LoadMoreCommand = new Command(async () => await LoadMoreAsync(), () => !IsBusy);
        }

        /// <summary>
        /// Persian:
        ///   متد بارگذاری صفحه اول یا صفحات بعدی مدیا.
        /// English:
        ///   Loads initial or subsequent pages of media items.
        /// </summary>
        public async Task LoadMoreAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                // 1) بازیابی sessionId از احراز هویت
                var session = await _authService.LoadSessionAsync();

                // 2) فراخوانی API برای صفحه‌بندی
                var page = await _apiClient.MediasAsync(
                    session.Id,
                    limit: 20,
                    cursor: _cursor);

                // 3) اضافه کردن آیتم‌ها به ObservableCollection
                foreach (var item in page.Items)
                    if (!Items.Any(x => x.Id == item.Id))
                        Items.Add(item);

                // 4) ذخیره cursor صفحه بعد
                _cursor = page.Meta.Next_cursor;
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
