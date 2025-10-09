using InstagramAuto.Client.ViewModels;
using Microsoft.Maui.Controls;

namespace InstagramAuto.Client.Views
{
    public partial class SessionManagerPage : ContentPage
    {
        private readonly SessionManagerViewModel _viewModel;
        public SessionManagerPage(SessionManagerViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadAsync();
        }
    }
}
