using Microsoft.Maui.Controls;
using InstagramAuto.Client.ViewModels;
using InstagramAuto.Client.Models;
using System;

namespace InstagramAuto.Client.Views
{
    public partial class PostsPage : ContentPage
    {
        private readonly PostsViewModel ViewModel;

        public PostsPage(PostsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = ViewModel = viewModel;

            Appearing += async (_, __) =>
            {
                if (ViewModel.Items.Count == 0)
                    await ViewModel.LoadMoreAsync();
            };
        }

        private async void OnPostSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count > 0 && e.CurrentSelection[0] is MediaItem post)
            {
                await ViewModel.GoToCommentsAsync(post);
                ((CollectionView)sender).SelectedItem = null;
            }
        }

        private async void OnCopyErrorClicked(object sender, EventArgs e)
        {
            if (BindingContext is PostsViewModel vm)
            {
                var textToCopy = string.IsNullOrEmpty(vm.ErrorDetails) ? vm.ErrorMessage : vm.ErrorDetails;
                if (!string.IsNullOrEmpty(textToCopy))
                {
                    await Clipboard.SetTextAsync(textToCopy);
                    await DisplayAlert("کپی شد", "متن خطا در کلیپ‌بورد قرار گرفت.", "باشه");
                }
            }
        }

        private async void OnOpenMediaClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is MediaItem item)
            {
                await ViewModel.OpenMediaAsync(item);
            }
        }

        private async void OnOpenCommentsClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is MediaItem item)
            {
                await ViewModel.GoToCommentsAsync(item);
            }
        }
    }
}
