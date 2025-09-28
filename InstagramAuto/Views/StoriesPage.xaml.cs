using Microsoft.Maui.Controls;
using InstagramAuto.Client.ViewModels;

namespace InstagramAuto.Client.Views
{
    public partial class StoriesPage : ContentPage
    {
        private StoriesViewModel ViewModel;

        public StoriesPage(StoriesViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = ViewModel = viewModel;
            Appearing += async (_, __) =>
            {
                if (ViewModel.Items.Count == 0)
                    await ViewModel.LoadAsync();
            };
        }
    }
}
