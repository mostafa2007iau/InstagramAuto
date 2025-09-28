using Microsoft.Maui.Controls;
using InstagramAuto.Client.ViewModels;
using System;

namespace InstagramAuto.Client.Views
{
    public partial class TasksPage : ContentPage
    {
        private TasksViewModel ViewModel;

        public TasksPage(TasksViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = ViewModel = viewModel;
            this.Appearing += async (_, __) =>
            {
                if (ViewModel.Items.Count == 0)
                    await ViewModel.LoadMoreAsync();
            };
        }

        private async void OnCopyErrorClicked(object sender, EventArgs e)
        {
            if (BindingContext is TasksViewModel vm)
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
