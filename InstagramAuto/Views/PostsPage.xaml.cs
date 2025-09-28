using Microsoft.Maui.Controls;
using InstagramAuto.Client.ViewModels;

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
    }
}
