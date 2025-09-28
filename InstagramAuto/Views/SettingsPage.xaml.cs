using Microsoft.Maui.Controls;
using InstagramAuto.Client.ViewModels;
using System;

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

        private async void OnCopyErrorClicked(object sender, EventArgs e)
        {
            if (BindingContext is SettingsViewModel vm)
            {
                var textToCopy = string.IsNullOrEmpty(vm.ErrorDetails) ? vm.ErrorMessage : vm.ErrorDetails;
                if (!string.IsNullOrEmpty(textToCopy))
                {
                    await Clipboard.SetTextAsync(textToCopy);
                    await DisplayAlert("??? ??", "??? ??? ?? ????????? ???? ????.", "????");
                }
            }
        }
    }
}
