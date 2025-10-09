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
            // Register DashboardPage so Shell navigation can resolve //DashboardPage
            Routing.RegisterRoute(nameof(DashboardPage), typeof(DashboardPage));

            // Register routes
            Routing.RegisterRoute("challenge", typeof(Views.ChallengePage));
            Routing.RegisterRoute("posts", typeof(Views.PostsPage));
            Routing.RegisterRoute("rules", typeof(Views.RulesPage));
            Routing.RegisterRoute("rule_editor", typeof(Views.RuleEditorPage));
            Routing.RegisterRoute("live_activity", typeof(Views.LiveActivityPage));
            Routing.RegisterRoute("health", typeof(Views.HealthPage));
            Routing.RegisterRoute("stats", typeof(Views.StatsPage));
            Routing.RegisterRoute("logs", typeof(Views.LogsPage));
            Routing.RegisterRoute("settings", typeof(Views.SettingsPage));
            Routing.RegisterRoute("proxy_settings", typeof(Views.ProxySettingsPage));
            Routing.RegisterRoute("risk_settings", typeof(Views.RiskSettingsPage));
            Routing.RegisterRoute("sessions", typeof(Views.SessionManagerPage));
        }
    }
}
