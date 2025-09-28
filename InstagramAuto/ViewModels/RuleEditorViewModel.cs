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
    ///   ?????? ?????? ? ????? ????? ?????? ? ??????.
    /// English:
    ///   ViewModel for editing and defining reply/DM rules.
    /// </summary>
    public class RuleEditorViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private string _name;
        private string _condition;
        private string _repliesText;
        private string _dmsText;
        private bool _sendDM;
        private bool _enabled = true;
        private string _errorMessage;

        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
        public string Condition { get => _condition; set { _condition = value; OnPropertyChanged(); } }
        public string RepliesText { get => _repliesText; set { _repliesText = value; OnPropertyChanged(); } }
        public string DMsText { get => _dmsText; set { _dmsText = value; OnPropertyChanged(); } }
        public bool SendDM { get => _sendDM; set { _sendDM = value; OnPropertyChanged(); } }
        public bool Enabled { get => _enabled; set { _enabled = value; OnPropertyChanged(); } }
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); } }
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public ICommand SaveCommand { get; }

        public RuleEditorViewModel(IAuthService authService)
        {
            _authService = authService;
            SaveCommand = new Command(async () => await SaveAsync());
        }

        public async Task InitializeAsync()
        {
            // TODO: ???????? ????? ???? ?????? ?? ????? ????? ????
        }

        public async Task SaveAsync()
        {
            ErrorMessage = string.Empty;
            try
            {
                var session = await _authService.LoadSessionAsync();
                var rule = new RuleItem
                {
                    AccountId = session.AccountId,
                    Name = Name,
                    Condition = Condition,
                    Replies = RepliesText?.Split('\n').Where(x => !string.IsNullOrWhiteSpace(x)).ToList(),
                    SendDM = SendDM,
                    DMs = DMsText?.Split('\n').Where(x => !string.IsNullOrWhiteSpace(x)).ToList(),
                    Enabled = Enabled
                };
                await _authService.SaveRuleAsync(rule);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}
