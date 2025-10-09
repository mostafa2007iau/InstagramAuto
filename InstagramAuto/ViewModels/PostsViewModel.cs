using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using InstagramAuto.Client.Models;
using InstagramAuto.Client.Services;
using InstagramAuto.Client.Helpers;
using System.Globalization;
using System.Collections.Generic;

namespace InstagramAuto.Client.ViewModels
{
    public class PostsViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly IInstagramAutoClient _apiClient;
        private bool _isBusy;
        private string _errorMessage;
        private string _errorDetails;
        private string _cursor;
        private ObservableCollection<PostItemViewModel> _items = new();
        private ObservableCollection<PostItemViewModel> _selectedPosts = new();

        public ObservableCollection<PostItemViewModel> Items 
        { 
            get => _items;
            set { _items = value; OnPropertyChanged(); }
        }

        public ObservableCollection<PostItemViewModel> SelectedPosts
        {
            get => _selectedPosts;
            set 
            { 
                _selectedPosts = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedPosts));
                OnPropertyChanged(nameof(SelectionInfoText));
            }
        }

        public bool HasSelectedPosts => SelectedPosts.Count > 0;
        public string SelectionInfoText => $"{SelectedPosts.Count} پست انتخاب شده";

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                OnPropertyChanged();
                ((Command)LoadMoreCommand).ChangeCanExecute();
                ((Command)RefreshCommand).ChangeCanExecute();
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

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public ICommand LoadMoreCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CreateRuleCommand { get; }
        public ICommand ManageRulesCommand { get; }
        public ICommand ViewPostCommand { get; }
        public ICommand ClearSelectionCommand { get; }
        public ICommand CopyErrorCommand { get; }

        public PostsViewModel(IAuthService authService, IInstagramAutoClient apiClient)
        {
            _authService = authService;
            _apiClient = apiClient;

            LoadMoreCommand = new Command(async () => await LoadMoreAsync(), () => !IsBusy);
            RefreshCommand = new Command(async () => await RefreshAsync(), () => !IsBusy);
            CreateRuleCommand = new Command(async () => await CreateRuleAsync(), () => HasSelectedPosts);
            ManageRulesCommand = new Command<PostItemViewModel>(async (post) => await ManageRulesAsync(post));
            ViewPostCommand = new Command<PostItemViewModel>(async (post) => await ViewPostAsync(post));
            ClearSelectionCommand = new Command(ClearSelection);
            CopyErrorCommand = new Command(async () => await CopyErrorAsync());
        }

        public async Task RefreshAsync()
        {
            if (IsBusy) return;
            Items.Clear();
            _cursor = null;
            await LoadMoreAsync();
        }

        public async Task LoadMoreAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;
            ErrorDetails = string.Empty;

            try
            {
                var session = await _authService.LoadSessionAsync();

                var page = await _apiClient.MediasAsync(
                    session.Id,
                    limit: 20,
                    cursor: _cursor);

                // Load rules for the posts
                var rules = await _authService.GetRulesAsync(session.AccountId);
                var rulesByMedia = rules.Items.GroupBy(r => r.MediaId)
                                            .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var item in page.Items)
                {
                    if (!Items.Any(x => x.Id == item.Id))
                    {
                        var hasRules = rulesByMedia.TryGetValue(item.Id, out var postRules);
                        var viewModel = new PostItemViewModel(item)
                        {
                            HasRules = hasRules,
                            RulesCount = hasRules ? postRules.Count : 0
                        };
                        Items.Add(viewModel);
                    }
                }

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

        private async Task CreateRuleAsync()
        {
            var parameters = new Dictionary<string, object>
            {
                { "mediaIds", string.Join(",", SelectedPosts.Select(p => p.Id)) }
            };
            await Shell.Current.GoToAsync("rule_editor", parameters);
        }

        private async Task ManageRulesAsync(PostItemViewModel post)
        {
            if (post == null) return;
            var parameters = new Dictionary<string, object>
            {
                { "mediaId", post.Id }
            };
            await Shell.Current.GoToAsync("rules", parameters);
        }

        private async Task ViewPostAsync(PostItemViewModel post)
        {
            if (post == null) return;
            // Open the post in browser or in-app viewer
            await Browser.OpenAsync($"https://instagram.com/p/{post.Id}");
        }

        private void ClearSelection()
        {
            SelectedPosts.Clear();
        }

        private async Task CopyErrorAsync()
        {
            if (string.IsNullOrEmpty(ErrorDetails)) return;
            await Clipboard.SetTextAsync(ErrorDetails);
            await Shell.Current.DisplayAlert("انجام شد", "خطا در کلیپ‌بورد کپی شد", "باشه");
        }

        public async Task GoToCommentsAsync(MediaItem post)
        {
            if (post == null) return;
            var parameters = new Dictionary<string, object>
            {
                { "mediaId", post.Id }
            };
            await Shell.Current.GoToAsync("comments", parameters);
        }

        public async Task OpenMediaAsync(MediaItem post)
        {
            if (post == null) return;
            await Browser.OpenAsync($"https://instagram.com/p/{post.Id}");
        }
    }

    public class PostItemViewModel : MediaItem
    {
        public PostItemViewModel(MediaItem item)
        {
            Id = item.Id;
            Caption = item.Caption;
            Thumbnail_url = item.Thumbnail_url;
        }

        public bool HasRules { get; set; }
        public int RulesCount { get; set; }
        public bool IsSelected { get; set; }
    }
}
