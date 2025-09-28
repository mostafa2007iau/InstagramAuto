using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using InstagramAuto.Client.Models;
using InstagramAuto.Client.Services;

namespace InstagramAuto.Client.ViewModels
{
    /// <summary>
    /// Persian:
    ///   ویومدل صفحه تنظیمات برای نمایش و ویرایش محدودیت‌ها و رفتار کاربر.
    /// English:
    ///   ViewModel for SettingsPage to view and edit rate-limits and human-like behavior.
    /// </summary>
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly InstagramAutoClient _apiClient;
        private bool _enableDelay;
        private int _commentDelay;
        private int _likeDelay;
        private int _dmDelay;
        private int _hourlyLimit;
        private int _dailyLimit;
        private bool _randomJitter;
        private int _jitterMin;
        private int _jitterMax;
        private bool _isBusy;
        private string _errorMessage;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>  
        /// Persian: فعال‌سازی تأخیر بین اکشن‌ها  
        /// English: Enable delay between actions  
        /// </summary>
        public bool EnableDelay
        {
            get => _enableDelay;
            set { if (_enableDelay == value) return; _enableDelay = value; OnPropertyChanged(nameof(EnableDelay)); }
        }

        /// <summary>  
        /// Persian: تأخیر کامنت (ثانیه)  
        /// English: Comment delay in seconds  
        /// </summary>
        public int CommentDelay
        {
            get => _commentDelay;
            set { if (_commentDelay == value) return; _commentDelay = value; OnPropertyChanged(nameof(CommentDelay)); }
        }

        /// <summary>  
        /// Persian: تأخیر لایک (ثانیه)  
        /// English: Like delay in seconds  
        /// </summary>
        public int LikeDelay
        {
            get => _likeDelay;
            set { if (_likeDelay == value) return; _likeDelay = value; OnPropertyChanged(nameof(LikeDelay)); }
        }

        /// <summary>  
        /// Persian: تأخیر دایرکت (ثانیه)  
        /// English: DM delay in seconds  
        /// </summary>
        public int DmDelay
        {
            get => _dmDelay;
            set { if (_dmDelay == value) return; _dmDelay = value; OnPropertyChanged(nameof(DmDelay)); }
        }

        /// <summary>  
        /// Persian: حداکثر تعداد اکشن در هر ساعت  
        /// English: Maximum actions per hour  
        /// </summary>
        public int HourlyLimit
        {
            get => _hourlyLimit;
            set { if (_hourlyLimit == value) return; _hourlyLimit = value; OnPropertyChanged(nameof(HourlyLimit)); }
        }

        /// <summary>  
        /// Persian: حداکثر تعداد اکشن در هر روز  
        /// English: Maximum actions per day  
        /// </summary>
        public int DailyLimit
        {
            get => _dailyLimit;
            set { if (_dailyLimit == value) return; _dailyLimit = value; OnPropertyChanged(nameof(DailyLimit)); }
        }

        /// <summary>  
        /// Persian: فعال‌سازی تأخیر تصادفی (Jitter)  
        /// English: Enable random jitter delay  
        /// </summary>
        public bool RandomJitter
        {
            get => _randomJitter;
            set { if (_randomJitter == value) return; _randomJitter = value; OnPropertyChanged(nameof(RandomJitter)); }
        }

        /// <summary>  
        /// Persian: حداقل تأخیر تصادفی (ثانیه)  
        /// English: Minimum random jitter in seconds  
        /// </summary>
        public int JitterMin
        {
            get => _jitterMin;
            set { if (_jitterMin == value) return; _jitterMin = value; OnPropertyChanged(nameof(JitterMin)); }
        }

        /// <summary>  
        /// Persian: حداکثر تأخیر تصادفی (ثانیه)  
        /// English: Maximum random jitter in seconds  
        /// </summary>
        public int JitterMax
        {
            get => _jitterMax;
            set { if (_jitterMax == value) return; _jitterMax = value; OnPropertyChanged(nameof(JitterMax)); }
        }

        /// <summary>  
        /// Persian: آیا عملیات در حال انجام است؟  
        /// English: Indicates if a load/save operation is in progress.  
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
                ((Command)LoadCommand).ChangeCanExecute();
                ((Command)SaveCommand).ChangeCanExecute();
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
        /// English: Whether to show the error message  
        /// </summary>
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        /// <summary>  
        /// Persian: دستور بارگذاری تنظیمات  
        /// English: Command to load settings from server  
        /// </summary>
        public ICommand LoadCommand { get; }

        /// <summary>  
        /// Persian: دستور ذخیره‌سازی تنظیمات  
        /// English: Command to save settings to server  
        /// </summary>
        public ICommand SaveCommand { get; }

        public SettingsViewModel(
            IAuthService authService,
            IInstagramAutoClient apiClient)
        {
            _authService = authService;
            _apiClient = (InstagramAutoClient)apiClient;

            LoadCommand = new Command(async () => await LoadAsync(), () => !IsBusy);
            SaveCommand = new Command(async () => await SaveAsync(), () => !IsBusy);
        }

        /// <summary>
        /// Persian:
        ///   بارگذاری تنظیمات از API و مقداردهی ویومدل.
        /// English:
        ///   Loads settings from API and populates ViewModel.
        /// </summary>
        public async Task LoadAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var session = await _authService.LoadSessionAsync();
                var dto = await _apiClient.GetSettingsAsync(session.AccountId);

                EnableDelay = dto.EnableDelay;
                CommentDelay = dto.CommentDelaySec;
                LikeDelay = dto.LikeDelaySec;
                DmDelay = dto.DmDelaySec;
                HourlyLimit = dto.HourlyLimit;
                DailyLimit = dto.DailyLimit;
                RandomJitter = dto.RandomJitterEnabled;
                JitterMin = dto.JitterMinSec;
                JitterMax = dto.JitterMaxSec;
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

        /// <summary>
        /// Persian:
        ///   ارسال تنظیمات جدید به API و ذخیره آن.
        /// English:
        ///   Sends updated settings to API and saves them.
        /// </summary>
        public async Task SaveAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var session = await _authService.LoadSessionAsync();
                var input = new SettingsIn
                {
                    AccountId = session.AccountId,
                    EnableDelay = EnableDelay,
                    CommentDelaySec = CommentDelay,
                    LikeDelaySec = LikeDelay,
                    DmDelaySec = DmDelay,
                    HourlyLimit = HourlyLimit,
                    DailyLimit = DailyLimit,
                    RandomJitterEnabled = RandomJitter,
                    JitterMinSec = JitterMin,
                    JitterMaxSec = JitterMax
                };

                await _apiClient.UpdateSettingsAsync(input);
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
