using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace InstagramAuto.Client.ViewModels
{
    /// <summary>
    /// Persian:
    ///   ویومدل صفحه داشبورد؛ ناوبری به تمام ماژول‌ها بدون خطا.
    /// English:
    ///   ViewModel for DashboardPage; navigates safely to all modules.
    /// </summary>
    public class DashboardViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand NavigateToPostsCommand { get; }
        public ICommand NavigateToTasksCommand { get; }
        public ICommand NavigateToLogsCommand { get; }
        public ICommand NavigateToRulesCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }
        public ICommand NavigateToStoriesCommand { get; }
        public ICommand NavigateToLoginCommand { get; }

        public DashboardViewModel()
        {
            // Use absolute routes (//) to avoid navigation errors
            NavigateToPostsCommand = new Command(async () =>
                await Shell.Current.GoToAsync($"//{nameof(Views.PostsPage)}"));

            NavigateToTasksCommand = new Command(async () =>
                await Shell.Current.GoToAsync($"//{nameof(Views.TasksPage)}"));

            NavigateToLogsCommand = new Command(async () =>
                await Shell.Current.GoToAsync($"//{nameof(Views.LogsPage)}"));

            NavigateToRulesCommand = new Command(async () =>
                await Shell.Current.GoToAsync($"//{nameof(Views.RulesPage)}"));

            NavigateToSettingsCommand = new Command(async () =>
                await Shell.Current.GoToAsync($"//{nameof(Views.SettingsPage)}"));

            NavigateToLoginCommand = new Command(async () =>
                await Shell.Current.GoToAsync($"//{nameof(Views.LoginPage)}"));
        }
    }
}
