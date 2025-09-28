// InstagramAuto.Client.Views/LoginPage.xaml.cs
using InstagramAuto.Client.ViewModels;
using Microsoft.Maui.Controls;
using System;

namespace InstagramAuto.Client.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage(LoginViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        // Handler for Copy Error button
        private async void OnCopyErrorClicked(object sender, EventArgs e)
        {
            if (BindingContext is LoginViewModel vm && !string.IsNullOrEmpty(vm.ErrorMessage))
            {
                await Clipboard.SetTextAsync(vm.ErrorMessage);
                await DisplayAlert("Copied", "Error message copied to clipboard.", "OK");
            }
        }
    }
}
