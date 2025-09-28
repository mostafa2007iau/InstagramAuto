using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using InstagramAuto.Client.Models;
using InstagramAuto.Client.Services;
using InstagramAuto.Client.Helpers;

namespace InstagramAuto.Client.ViewModels
{
    /// <summary>
    /// Persian:
    ///   ویومدل صفحه Rules برای مدیریت قاعده‌های خودکار کامنت/دایرکت.
    /// English:
    ///   ViewModel for RulesPage to manage automatic comment/DM rules.
    /// </summary>
    public class RulesViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private bool _isBusy;
        private string _errorMessage;
        private string _cursor;

        private string _newName;
        private string _newExpression;
        private bool _newEnabled = true;

        private string _errorDetails;

        public ObservableCollection<RuleOut> Rules { get; } = new ObservableCollection<RuleOut>();

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                OnPropertyChanged();
                ((Command)LoadCommand).ChangeCanExecute();
                ((Command)CreateCommand).ChangeCanExecute();
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
        /// Persian: نام قاعده جدید  
        /// English: New rule name  
        /// </summary>
        public string NewName
        {
            get => _newName;
            set { if (_newName == value) return; _newName = value; OnPropertyChanged(); }
        }

        /// <summary>  
        /// Persian: عبارت JSON-Logic  
        /// English: JSON-Logic expression  
        /// </summary>
        public string NewExpression
        {
            get => _newExpression;
            set { if (_newExpression == value) return; _newExpression = value; OnPropertyChanged(); }
        }

        /// <summary>  
        /// Persian: وضعیت فعال‌بودن قاعده  
        /// English: Whether the rule is enabled  
        /// </summary>
        public bool NewEnabled
        {
            get => _newEnabled;
            set { if (_newEnabled == value) return; _newEnabled = value; OnPropertyChanged(); }
        }

        public ICommand LoadCommand { get; }
        public ICommand CreateCommand { get; }

        public RulesViewModel(IAuthService authService)
        {
            _authService = authService;

            LoadCommand = new Command(async () => await LoadRulesAsync(), () => !IsBusy);
            CreateCommand = new Command(async () => await CreateRuleAsync(), () => !IsBusy);
        }

        /// <summary>
        /// Persian:
        ///   بارگذاری فهرست قاعده‌ها از API.
        /// English:
        ///   Loads the list of rules from the API.
        /// </summary>
        public async Task LoadRulesAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = string.Empty;
            ErrorDetails = string.Empty;

            try
            {
                var session = await _authService.LoadSessionAsync();
                var page = await _authService.GetRulesAsync(session.AccountId, 50, _cursor);

                foreach (var rule in page.Items)
                    if (!Rules.Any(r => r.Id == rule.Id))
                        Rules.Add(rule);

                _cursor = page.Meta.Next_cursor;
            }
            catch (Exception ex)
            {
                var parsed = ErrorHelper.Parse(ex);
                ErrorMessage = parsed.Message;
                ErrorDetails = parsed.Details;
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Persian:
        ///   ایجاد یک قاعدهٔ جدید با داده‌های فرم و رفرش فهرست.
        /// English:
        ///   Creates a new rule using form data and refreshes the list.
        /// </summary>
        public async Task CreateRuleAsync()
        {
            if (IsBusy) return;
            if (string.IsNullOrWhiteSpace(NewName) || string.IsNullOrWhiteSpace(NewExpression))
            {
                ErrorMessage = "Name and expression cannot be empty.";
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;
            ErrorDetails = string.Empty;

            try
            {
                var session = await _authService.LoadSessionAsync();
                var ruleIn = new RuleIn
                {
                    Account_id = session.AccountId,
                    Name = NewName,
                    Expression = NewExpression,
                    Enabled = NewEnabled
                };

                await _authService.CreateRuleAsync(ruleIn);

                // ریست فرم
                NewName = string.Empty;
                NewExpression = string.Empty;
                NewEnabled = true;

                // ریست صفحه و بارگذاری دوباره
                Rules.Clear();
                _cursor = null;
                await LoadRulesAsync();
            }
            catch (Exception ex)
            {
                var parsed = ErrorHelper.Parse(ex);
                ErrorMessage = parsed.Message;
                ErrorDetails = parsed.Details;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
