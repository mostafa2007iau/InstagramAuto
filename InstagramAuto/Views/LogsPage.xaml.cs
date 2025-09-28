using Microsoft.Maui.Controls;
using InstagramAuto.Client.ViewModels;

namespace InstagramAuto.Client.Views
{
    public partial class LogsPage : ContentPage
    {
        private readonly LogsViewModel ViewModel;

        public LogsPage(LogsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = ViewModel = viewModel;

            this.Appearing += async (_, __) =>
            {
                if (ViewModel.Items.Count == 0)
                    await ViewModel.LoadMoreAsync();
            };
        }
    }
}
