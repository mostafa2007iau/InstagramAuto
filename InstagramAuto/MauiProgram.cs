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

namespace InstagramAuto.Client
{
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
                });

            // --- HTTP client & concrete API client ---
            builder.Services.AddSingleton(sp =>
            {
                var http = new HttpClient
                {
                    BaseAddress = new Uri("http://156.236.31.41:8000/")
                };
                return new InstagramAutoClient(http);
            });
            builder.Services.AddSingleton<IInstagramAutoClient>(sp =>
                sp.GetRequiredService<InstagramAutoClient>());

            // --- Core services ---
            builder.Services.AddSingleton<IAuthService, AuthService>();
            

            // --- ViewModels ---
            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddSingleton<DashboardViewModel>();
            builder.Services.AddTransient<PostsViewModel>();
            builder.Services.AddTransient<TasksViewModel>();
            builder.Services.AddTransient<LogsViewModel>();
            builder.Services.AddTransient<RulesViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddTransient<StoriesViewModel>();
            builder.Services.AddTransient<ChallengeViewModel>();

            // --- Views (Pages) ---
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<PostsPage>();
            builder.Services.AddTransient<TasksPage>();
            builder.Services.AddTransient<LogsPage>();
            builder.Services.AddTransient<RulesPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<StoriesPage>();
            builder.Services.AddTransient<ChallengePage>();

            return builder.Build();
        }
    }
}
