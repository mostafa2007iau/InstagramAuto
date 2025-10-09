// File: MauiProgram.cs
using System;
using System.Net.Http;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Extensions.DependencyInjection;
using InstagramAuto.Client;
using InstagramAuto.Client.Services;
using InstagramAuto.Client.ViewModels;
using InstagramAuto.Client.Views;
using Microsoft.Extensions.Logging;

namespace InstagramAuto.Client
{
    /// <summary>
    /// Persian:
    ///     کلاس اصلی برنامه MAUI.
    ///     مسئول راه‌اندازی و پیکربندی سرویس‌ها.
    /// English:
    ///     MAUI application main class.
    ///     Responsible for bootstrapping and configuring services.
    /// </summary>
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Bold.ttf", "OpenSansBold");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                });

            // --- HTTP client & concrete API client ---
            builder.Services.AddSingleton(sp =>
            {
                var http = new HttpClient
                {
                    BaseAddress = new Uri("http://217.197.97.69:8000/")
                };
                return new InstagramAutoClient(http);
            });
            
            builder.Services.AddSingleton<IInstagramAutoClient>(sp =>
                sp.GetRequiredService<InstagramAutoClient>());

            // --- Logging ---
            builder.Services.AddLogging(logging =>
            {
                logging.AddDebug();
                logging.SetMinimumLevel(LogLevel.Debug);
            });

            // --- Core services ---
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<IProxyService, ProxyService>();
            builder.Services.AddSingleton<ISessionManager, SessionManager>();
            builder.Services.AddSingleton<IRiskManager, RiskManager>();
            builder.Services.AddSingleton<IActivityMonitor, ActivityMonitor>();
            builder.Services.AddSingleton<IStatsCollector, StatsCollector>();
            builder.Services.AddSingleton<IHealthMonitor, HealthMonitor>();
            //builder.Services.AddSingleton<IAnalyticsService, AnalyticsService>();
            //builder.Services.AddSingleton<IRefreshTimer, RefreshTimer>();

            // --- ViewModels ---
            RegisterViewModels(builder.Services);

            // --- Views ---
            RegisterViews(builder.Services);

            return builder.Build();
        }

        private static void RegisterViewModels(IServiceCollection services)
        {
            // Single instance ViewModels (shared state)
            services.AddSingleton<LoginViewModel>();
            services.AddSingleton<DashboardViewModel>();
            services.AddSingleton<SessionManagerViewModel>();

            // Transient ViewModels (new instance per navigation)
            services.AddTransient<PostsViewModel>();
            services.AddTransient<StoriesViewModel>();
            services.AddTransient<CommentsViewModel>();
            services.AddTransient<TasksViewModel>();
            services.AddTransient<RulesViewModel>();
            services.AddTransient<RuleEditorViewModel>();
            services.AddTransient<ImportExportViewModel>();
            services.AddTransient<LiveActivityViewModel>();
            services.AddTransient<ReplyHistoryViewModel>();
            services.AddTransient<StatsViewModel>();
            services.AddTransient<LogsViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<ProxySettingsViewModel>();
            services.AddTransient<RiskSettingsViewModel>();
            services.AddTransient<ChallengeViewModel>();
        }

        private static void RegisterViews(IServiceCollection services)
        {
            // Authentication & Session
            services.AddTransient<LoginPage>();
            services.AddTransient<ChallengePage>();
            services.AddTransient<SessionManagerPage>();

            // Main Dashboard
            services.AddTransient<DashboardPage>();

            // Content Management
            services.AddTransient<PostsPage>();
            services.AddTransient<StoriesPage>();
            services.AddTransient<CommentsPage>();

            // Automation
            services.AddTransient<RulesPage>();
            services.AddTransient<RuleEditorPage>();
            services.AddTransient<TasksPage>();
            services.AddTransient<ImportExportPage>();

            // Monitoring
            services.AddTransient<LiveActivityPage>();
            services.AddTransient<ReplyHistoryPage>();
            services.AddTransient<StatsPage>();
            services.AddTransient<LogsPage>();

            // Settings
            services.AddTransient<SettingsPage>();
            services.AddTransient<ProxySettingsPage>();
            services.AddTransient<RiskSettingsPage>();
        }
    }
}
