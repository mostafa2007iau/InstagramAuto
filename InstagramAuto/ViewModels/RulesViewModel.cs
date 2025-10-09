using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using InstagramAuto.Client.Models;
using InstagramAuto.Client.Services;
using Microsoft.Maui.Controls;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace InstagramAuto.Client.ViewModels
{
    public class RulesViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private string _mediaId;
        private string _postCaption;
        private ObservableCollection<RuleItem> _rules = new();
        private bool _isBusy;
        private string _errorMessage;
        private string _errorDetails;

        public string MediaId 
        { 
            get => _mediaId;
            set { _mediaId = value; OnPropertyChanged(); }
        }

        public string PostCaption
        {
            get => _postCaption;
            set { _postCaption = value; OnPropertyChanged(); }
        }

        public ObservableCollection<RuleItem> Rules
        {
            get => _rules;
            set { _rules = value; OnPropertyChanged(); OnPropertyChanged(nameof(RulesCount)); }
        }

        public int RulesCount => Rules?.Count ?? 0;

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public string ErrorDetails
        {
            get => _errorDetails;
            set { _errorDetails = value; OnPropertyChanged(); }
        }

        public ICommand AddRuleCommand { get; }
        public ICommand EditRuleCommand { get; }
        public ICommand DeleteRuleCommand { get; }
        public ICommand ToggleRuleCommand { get; }

        public RulesViewModel(IAuthService authService)
        {
            _authService = authService;
            
            AddRuleCommand = new Command(async () => await AddRuleAsync());
            EditRuleCommand = new Command<RuleItem>(async (rule) => await EditRuleAsync(rule));
            DeleteRuleCommand = new Command<RuleItem>(async (rule) => await DeleteRuleAsync(rule));
            ToggleRuleCommand = new Command<RuleItem>(async (rule) => await ToggleRuleAsync(rule));
        }

        public async Task InitializeAsync(string mediaId)
        {
            MediaId = mediaId;
            await LoadRulesAsync();
        }

        public async Task LoadRulesAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                var session = await _authService.LoadSessionAsync();
                var rulesPage = await _authService.GetRulesAsync(session.AccountId);
                
                Rules.Clear();
                foreach (var rule in rulesPage.Items.Where(r => r.MediaId == MediaId))
                {
                    // Create a RuleItem instance for UI using available data
                    Rules.Add(new RuleItem
                    {
                        Id = rule.Id,
                        Name = rule.Name,
                        Account_id = rule.Account_id,
                        Attachments = rule.AdditionalProperties != null && rule.AdditionalProperties.ContainsKey("attachments")
                            ? JsonConvert.DeserializeObject<List<MediaAttachment>>(rule.AdditionalProperties["attachments"].ToString())
                            : new List<MediaAttachment>(),
                        Expression = rule.Expression,
                        Enabled = rule.Enabled
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                ErrorDetails = ex.ToString();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task AddRuleAsync()
        {
            var parameters = new Dictionary<string, object>
            {
                { "mediaId", MediaId }
            };
            await Shell.Current.GoToAsync("rule_editor", parameters);
        }

        private async Task EditRuleAsync(RuleItem rule)
        {
            if (rule == null) return;
            
            var parameters = new Dictionary<string, object>
            {
                { "ruleId", rule.Id },
                { "mediaId", MediaId }
            };
            await Shell.Current.GoToAsync("rule_editor", parameters);
        }

        private async Task DeleteRuleAsync(RuleItem rule)
        {
            if (rule == null) return;

            var confirm = await Shell.Current.DisplayAlert(
                "تأیید حذف",
                "آیا از حذف این قانون مطمئن هستید؟",
                "بله",
                "خیر");

            if (!confirm) return;

            try
            {
                IsBusy = true;
                // TODO: Add delete API call
                Rules.Remove(rule);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                ErrorDetails = ex.ToString();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ToggleRuleAsync(RuleItem rule)
        {
            if (rule == null) return;

            try
            {
                IsBusy = true;
                rule.Enabled = !rule.Enabled;
                await _authService.SaveRuleAsync(rule);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                ErrorDetails = ex.ToString();
                // Revert toggle if failed
                rule.Enabled = !rule.Enabled;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
