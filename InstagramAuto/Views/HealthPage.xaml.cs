using InstagramAuto.Client.ViewModels;
using Microsoft.Maui.Controls;
using InstagramAuto.Client.ViewModels;

namespace InstagramAuto.Client.Views
{
    public partial class HealthPage : ContentPage
    {
        private readonly HealthViewModel _viewModel;

        public HealthPage(HealthViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadAsync();
        }

        public HealthViewModel ViewModel => BindingContext as HealthViewModel;
    }
}