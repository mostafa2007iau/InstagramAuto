using Microsoft.Maui.Controls;
using InstagramAuto.Client.ViewModels;

namespace InstagramAuto.Client.Views
{
    public partial class SettingsPage : ContentPage
    {
        private SettingsViewModel ViewModel;

        public SettingsPage(SettingsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = ViewModel = viewModel;
            Appearing += async (_, __) =>
            {
                if (!ViewModel.IsBusy)
                    await ViewModel.LoadAsync();
            };
        }
    }
}
