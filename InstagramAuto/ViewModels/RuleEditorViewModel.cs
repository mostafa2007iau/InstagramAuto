using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using InstagramAuto.Client.Models;
using InstagramAuto.Client.Services;
using System.Collections.Generic;

namespace InstagramAuto.Client.ViewModels
{
    /// <summary>
    /// Persian:
    ///   ?????? ?????? ? ????? ?????? ??????/??????.
    /// English:
    ///   ViewModel for editing and defining reply/DM rules.
    /// </summary>
    public class RuleEditorViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private string _name;
        private string _conditionValue;
        private string _selectedConditionType;
        private ObservableCollection<ReplyViewModel> _replies = new();
        private string _newReplyText;
        private bool _sendDM;
        private string _dmText;
        private ObservableCollection<MediaAttachmentViewModel> _dmAttachments = new();
        private bool _enabled = true;
        private string _errorMessage;
        public ObservableCollection<string> ConditionTypes { get; } = new(new[] { "???? ???", "????? ?????", "???? ??", "????? ??" });

        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
        public string ConditionValue { get => _conditionValue; set { _conditionValue = value; OnPropertyChanged(); } }
        public string SelectedConditionType { get => _selectedConditionType; set { _selectedConditionType = value; OnPropertyChanged(); } }
        public ObservableCollection<ReplyViewModel> Replies { get => _replies; set { _replies = value; OnPropertyChanged(); } }
        public string NewReplyText { get => _newReplyText; set { _newReplyText = value; OnPropertyChanged(); } }
        public bool SendDM { get => _sendDM; set { _sendDM = value; OnPropertyChanged(); } }
        public string DMText { get => _dmText; set { _dmText = value; OnPropertyChanged(); } }
        public ObservableCollection<MediaAttachmentViewModel> DMAttachments { get => _dmAttachments; set { _dmAttachments = value; OnPropertyChanged(); } }
        public bool Enabled { get => _enabled; set { _enabled = value; OnPropertyChanged(); } }
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); } }
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public ICommand SaveCommand { get; }
        public ICommand AddReplyCommand { get; }
        public ICommand AddImageReplyCommand { get; }
        public ICommand AddLinkReplyCommand { get; }
        public ICommand RemoveReplyCommand { get; }
        public ICommand AddDMImageCommand { get; }
        public ICommand RemoveDMAttachmentCommand { get; }

        public RuleEditorViewModel(IAuthService authService)
        {
            _authService = authService;
            SaveCommand = new Command(async () => await SaveAsync());
            AddReplyCommand = new Command(AddReply);
            AddImageReplyCommand = new Command(async () => await AddImageReplyAsync());
            AddLinkReplyCommand = new Command(async () => await AddLinkReplyAsync());
            RemoveReplyCommand = new Command<ReplyViewModel>(RemoveReply);
            AddDMImageCommand = new Command(async () => await AddDMImageAsync());
            RemoveDMAttachmentCommand = new Command<MediaAttachmentViewModel>(RemoveDMAttachment);
            SelectedConditionType = ConditionTypes.FirstOrDefault();
        }

        private void AddReply()
        {
            if (!string.IsNullOrWhiteSpace(NewReplyText))
            {
                Replies.Add(new ReplyViewModel { Text = NewReplyText });
                NewReplyText = string.Empty;
            }
        }

        private async Task AddImageReplyAsync()
        {
            var file = await PickImageAsync();
            if (file != null)
            {
                Replies.Add(new ReplyViewModel
                {
                    Text = "[?????]",
                    Attachment = new MediaAttachment { Type = "image", Url = file, Caption = null }
                });
            }
        }

        private async Task AddLinkReplyAsync()
        {
            string url = await Application.Current.MainPage.DisplayPromptAsync("?????? ????", "???? ???? ?? ???? ????:");
            if (!string.IsNullOrWhiteSpace(url))
            {
                Replies.Add(new ReplyViewModel
                {
                    Text = url,
                    Attachment = new MediaAttachment { Type = "link", Url = url }
                });
            }
        }

        private void RemoveReply(ReplyViewModel reply)
        {
            if (reply != null)
                Replies.Remove(reply);
        }

        private async Task AddDMImageAsync()
        {
            var file = await PickImageAsync();
            if (file != null)
            {
                DMAttachments.Add(new MediaAttachmentViewModel
                {
                    Attachment = new MediaAttachment { Type = "image", Url = file }
                });
            }
        }

        private void RemoveDMAttachment(MediaAttachmentViewModel att)
        {
            if (att != null)
                DMAttachments.Remove(att);
        }

        private async Task<string> PickImageAsync()
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "?????? ?????",
                    FileTypes = FilePickerFileType.Images
                });
                return result?.FullPath;
            }
            catch { return null; }
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task SaveAsync()
        {
            ErrorMessage = string.Empty;
            try
            {
                var session = await _authService.LoadSessionAsync();
                var rule = new RuleItem
                {
                    Account_id = session.AccountId,
                    Name = Name,
                    Expression = BuildConditionJson(),
                    Replies = Replies.Select(r => r.Text).ToList(),
                    Attachments = Replies.Where(r => r.Attachment != null).Select(r => r.Attachment).ToList(),
                    SendDM = SendDM,
                    DM = SendDM ? new DMTemplate
                    {
                        Text = DMText,
                        Attachments = DMAttachments.Select(a => a.Attachment).ToList()
                    } : null,
                    Enabled = Enabled
                };
                await _authService.SaveRuleAsync(rule);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private string BuildConditionJson()
        {
            // ??? ?? ?? ???? json-logic ???? ????? ??????
            if (string.IsNullOrWhiteSpace(SelectedConditionType) || string.IsNullOrWhiteSpace(ConditionValue))
                return null;
            var op = SelectedConditionType switch
            {
                "???? ???" => "contains",
                "????? ?????" => "==",
                "???? ??" => "startsWith",
                "????? ??" => "endsWith",
                _ => "contains"
            };
            return $"{{\"{op}\":[{{\"var\":\"comment\"}},\"{ConditionValue}\"]}}";
        }
    }

    public class ReplyViewModel
    {
        public string Text { get; set; }
        public MediaAttachment Attachment { get; set; }
        public bool HasAttachment => Attachment != null;
        public string AttachmentPreview => Attachment?.Type == "image" ? Attachment.Url : null;
    }

    public class MediaAttachmentViewModel
    {
        public MediaAttachment Attachment { get; set; }
        public string AttachmentPreview => Attachment?.Type == "image" ? Attachment.Url : null;
    }
}
