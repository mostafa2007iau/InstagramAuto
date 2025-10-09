using System;
using Microsoft.Maui.Controls;
using InstagramAuto.Client.ViewModels;

namespace InstagramAuto.Client.Views
{
    /// <summary>
    /// Persian:
    ///     ???? ??????? ??????.
    /// English:
    ///     Proxy settings page.
    /// </summary>
    public partial class ProxySettingsPage : ContentPage
    {
        public ProxySettingsViewModel ViewModel => BindingContext as ProxySettingsViewModel;

        public ProxySettingsPage(ProxySettingsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (ViewModel != null)
                await ViewModel.LoadAsync();
        }
    }
}