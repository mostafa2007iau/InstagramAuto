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
            Routing.RegisterRoute(nameof(LogsPage), typeof(LogsPage));
            // جدید:
            Routing.RegisterRoute(nameof(CommentsPage), typeof(CommentsPage));
            Routing.RegisterRoute(nameof(RuleEditorPage), typeof(RuleEditorPage));
            Routing.RegisterRoute(nameof(ImportExportPage), typeof(ImportExportPage));
            Routing.RegisterRoute(nameof(LiveActivityPage), typeof(LiveActivityPage));
            Routing.RegisterRoute(nameof(StatsPage), typeof(StatsPage));
        }
    }
}
