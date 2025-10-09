using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using InstagramAuto.Client.Models;
using InstagramAuto.Client.Services;

namespace InstagramAuto.Client.ViewModels
{
    /// <summary>
    /// Persian:
    ///   ?????? ???? ???????? ???? ????? ? ?????? ????????? ?? ???.
    /// English:
    ///   ViewModel for CommentsPage to display and manage comments of a post.
    /// </summary>
    public class CommentsViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private bool _isBusy;
        private string _errorMessage;
        private string _cursor;
        private string _mediaId;

        public ObservableCollection<CommentItem> Items { get; } = new ObservableCollection<CommentItem>();

        public bool IsBusy
        {
            get => _isBusy;
            set { if (_isBusy == value) return; _isBusy = value; OnPropertyChanged(); ((Command)LoadMoreCommand).ChangeCanExecute(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { if (_errorMessage == value) return; _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public ICommand LoadMoreCommand { get; }
        public ICommand SetRuleCommand { get; }

        public CommentsViewModel(IAuthService authService)
        {
            _authService = authService;
            LoadMoreCommand = new Command(async () => await LoadMoreAsync(), () => !IsBusy);
            SetRuleCommand = new Command<CommentItem>(OnSetRule);
        }

        public async Task InitializeAsync(string mediaId)
        {
            _mediaId = mediaId;
            Items.Clear();
            _cursor = null;
            await LoadMoreAsync();
        }

        public async Task LoadMoreAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ErrorMessage = string.Empty;
            try
            {
                var session = await _authService.LoadSessionAsync();
                var comments = await _authService.GetCommentsAsync(_mediaId, _cursor);
                foreach (var c in comments.Items)
                    if (!Items.Contains(c))
                        Items.Add(c);
                _cursor = comments.Meta.Next_cursor;
            }
            catch (System.Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void OnSetRule(CommentItem comment)
        {
            // TODO: Navigate to RuleEditorPage with comment info
        }
    }
}
