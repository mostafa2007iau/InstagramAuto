using Microsoft.Maui.Controls;
using InstagramAuto.Client.ViewModels;
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

            // وقتی صفحه ظاهر می‌شود، اولین صفحه را بارگذاری کن
            this.Appearing += async (_, __) =>
            {
                if (ViewModel.Items.Count == 0)
                    await ViewModel.LoadMoreAsync();
            };
        }

        // Persian:
        //   هندل انتخاب پست و ناوبری به صفحه کامنت‌ها
        // English:
        //   Handle post selection and navigate to CommentsPage
        private async void OnPostSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count > 0 && e.CurrentSelection[0] is MediaItem post)
            {
                await ViewModel.GoToCommentsAsync(post);
            }
        }

        // Persian:
        //   کپی خطا و جزئیات JSON
        // English:
        //   Copy error message and JSON details
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
    }
}
