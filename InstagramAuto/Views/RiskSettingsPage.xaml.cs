using System;
using Microsoft.Maui.Controls;
using InstagramAuto.Client.ViewModels;

namespace InstagramAuto.Client.Views
{
    /// <summary>
    /// Persian:
    ///     ???? ??????? ????.
    /// English:
    ///     Risk settings page.
    /// </summary>
    public partial class RiskSettingsPage : ContentPage
    {
        public RiskSettingsPage(RiskSettingsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await ((RiskSettingsViewModel)BindingContext).LoadSettingsAsync();
        }
    }
}