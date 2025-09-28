// InstagramAuto.Client.Views/LoginPage.xaml.cs
using InstagramAuto.Client.ViewModels;
using Microsoft.Maui.Controls;

namespace InstagramAuto.Client.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage(LoginViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
