using InstagramAuto.Client.ViewModels;
using Microsoft.Maui.Controls;

namespace InstagramAuto.Client.Views
{
    public partial class DashboardPage : ContentPage
    {
        private readonly DashboardViewModel dashboardViewModel;
        public DashboardPage(DashboardViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = dashboardViewModel = viewModel;
        }
    }
}
