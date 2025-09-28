// File: AppShell.xaml.cs
using Microsoft.Maui.Controls;
using InstagramAuto.Client.Views;

namespace InstagramAuto.Client
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register route for Login (modal or separate navigation)
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        }
    }
}
