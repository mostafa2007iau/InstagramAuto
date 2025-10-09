using Microsoft.Maui.Controls;
using InstagramAuto.Client.ViewModels;
using System;

namespace InstagramAuto.Client.Views
{
    public partial class RulesPage : ContentPage
    {
        private RulesViewModel ViewModel;

        public RulesPage(RulesViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = ViewModel = viewModel;
            this.Appearing += async (_, __) =>
            {
                if (ViewModel.Rules.Count == 0)
                    await ViewModel.LoadRulesAsync();
            };
        }

        private async void OnCopyErrorClicked(object sender, EventArgs e)
        {
            if (BindingContext is RulesViewModel vm)
            {
                var textToCopy = string.IsNullOrEmpty(vm.ErrorMessage) ? vm.ErrorMessage : vm.ErrorDetails;
                if (!string.IsNullOrEmpty(textToCopy))
                {
                    await Clipboard.SetTextAsync(textToCopy);
                    await DisplayAlert("??? ??", "??? ??? ?? ????????? ???? ????.", "????");
                }
            }
        }
    }
}
