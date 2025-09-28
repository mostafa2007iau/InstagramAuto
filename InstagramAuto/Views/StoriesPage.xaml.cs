using Microsoft.Maui.Controls;
using InstagramAuto.Client.ViewModels;
using System;

namespace InstagramAuto.Client.Views
{
    public partial class StoriesPage : ContentPage
    {
        public StoriesPage(StoriesViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private async void OnCopyErrorClicked(object sender, EventArgs e)
        {
            if (BindingContext is StoriesViewModel vm)
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
