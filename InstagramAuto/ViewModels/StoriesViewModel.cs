// File: ViewModels/StoriesViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using InstagramAuto.Client;
using InstagramAuto.Client.Helpers;
using InstagramAuto.Client.Models;
using InstagramAuto.Client.Services;
using Microsoft.Maui.Controls;

namespace InstagramAuto.Client.ViewModels
{
    /// <summary>
    /// Persian:
    ///   ویومدل صفحه استوری‌ها برای بارگذاری و پاسخ خودکار.
    /// English:
    ///   ViewModel for StoriesPage to load and auto-reply to stories.
    /// </summary>
    public class StoriesViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly InstagramAutoClient _apiClient;
        private bool _isBusy;
        private string _errorMessage;
        private string _cursor;
        private StoryDto _selectedStory;
        private string _replyMessage;
        private string _linkUrl;
        private string _imageUrl;
        private string _errorDetails;

        /// <summary>
        /// Persian: لیست استوری‌ها  
        /// English: Collection of stories  
        /// </summary>
        public ObservableCollection<StoryDto> Items { get; } = new ObservableCollection<StoryDto>();

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
                OnPropertyChanged();
                ((Command)LoadCommand).ChangeCanExecute();
                ((Command)ReplyCommand).ChangeCanExecute();
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
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasError));
            }
        }

        /// <summary>
        /// Persian: جزئیات خطا
        /// English: Error details (full JSON / server response)
        /// </summary>
        public string ErrorDetails
        {
            get => _errorDetails;
            set
            {
                if (_errorDetails == value) return;
                _errorDetails = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Persian: آیا خطا نمایش داده شود؟  
        /// English: Whether to show the error message  
        /// </summary>
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        /// <summary>
        /// Persian: استوری انتخاب‌شده  
        /// English: Currently selected story  
        /// </summary>
        public StoryDto SelectedStory
        {
            get => _selectedStory;
            set
            {
                if (_selectedStory == value) return;
                _selectedStory = value;
                OnPropertyChanged();
                ((Command)ReplyCommand).ChangeCanExecute();
            }
        }

        /// <summary>
        /// Persian: پیام پاسخ  
        /// English: Reply message text  
        /// </summary>
        public string ReplyMessage
        {
            get => _replyMessage;
            set
            {
                if (_replyMessage == value) return;
                _replyMessage = value;
                OnPropertyChanged();
                ((Command)ReplyCommand).ChangeCanExecute();
            }
        }

        /// <summary>
        /// Persian: لینک اختیاری  
        /// English: Optional link URL  
        /// </summary>
        public string LinkUrl
        {
            get => _linkUrl;
            set { if (_linkUrl == value) return; _linkUrl = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Persian: آدرس تصویر اختیاری  
        /// English: Optional image URL  
        /// </summary>
        public string ImageUrl
        {
            get => _imageUrl;
            set { if (_imageUrl == value) return; _imageUrl = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Persian: دستور بارگذاری استوری‌ها  
        /// English: Command to load stories  
        /// </summary>
        public ICommand LoadCommand { get; }

        /// <summary>
        /// Persian: دستور ارسال پاسخ  
        /// English: Command to send reply  
        /// </summary>
        public ICommand ReplyCommand { get; }

        public StoriesViewModel(
            IAuthService authService,
            InstagramAutoClient apiClient)
        {
            _authService = authService;
            _apiClient = apiClient;

            LoadCommand = new Command(async () => await LoadAsync(), () => !IsBusy);
            ReplyCommand = new Command(async () => await ReplyAsync(),
                                       () => !IsBusy && SelectedStory != null && !string.IsNullOrWhiteSpace(ReplyMessage));
        }

        /// <summary>
        /// Persian:
        ///   بارگذاری صفحه‌بندی شده استوری‌ها.
        /// English:
        ///   Loads a paginated list of stories.
        /// </summary>
        public async Task LoadAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = string.Empty;
            this.ErrorDetails = string.Empty;

            try
            {
                var session = await _authService.LoadSessionAsync();
                var page = await _apiClient.ListStoriesAsync(
                    session.Id,
                    limit: 20,
                    cursor: _cursor);

                foreach (var s in page.Items)
                    if (!Items.Any(x => x.Id == s.Id))
                        Items.Add(s);

                _cursor = page.Meta.NextCursor;
            }
            catch (Exception ex)
            {
                var parsed = ErrorHelper.Parse(ex);
                ErrorMessage = parsed.Message;
                this.ErrorDetails = parsed.Details;
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Persian:
        ///   ارسال پاسخ دایرکت به استوری انتخاب‌شده.
        /// English:
        ///   Sends a direct message reply to the selected story.
        /// </summary>
        public async Task ReplyAsync()
        {
            if (IsBusy || SelectedStory == null) return;
            IsBusy = true;
            ErrorMessage = string.Empty;
            this.ErrorDetails = string.Empty;

            try
            {
                var session = await _authService.LoadSessionAsync();
                var dto = new DirectMessageIn
                {
                    Text = ReplyMessage,
                    Link = LinkUrl,
                    ImageUrl = ImageUrl
                };
                await _apiClient.ReplyToStoryAsync(
                    session.Id,
                    SelectedStory.Id,
                    dto);

                // Reset form
                ReplyMessage = string.Empty;
                LinkUrl = string.Empty;
                ImageUrl = string.Empty;
            }
            catch (Exception ex)
            {
                var parsed = ErrorHelper.Parse(ex);
                ErrorMessage = parsed.Message;
                this.ErrorDetails = parsed.Details;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
