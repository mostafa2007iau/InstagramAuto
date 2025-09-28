using InstagramAuto.Client.ViewModels;
using Microsoft.Maui.Controls;

namespace InstagramAuto.Client.Views
{
    [QueryProperty(nameof(ChallengeToken), "ChallengeToken")]
    [QueryProperty(nameof(Username), "Username")]
    [QueryProperty(nameof(Password), "Password")]
    public partial class ChallengePage : ContentPage
    {
        public ChallengeViewModel ViewModel { get; }

        public ChallengePage()
        {
            InitializeComponent();

            ViewModel = new ChallengeViewModel();
            BindingContext = ViewModel;
        }

        // این سه پراپرتی توسط Shell پر می‌شوند
        public string ChallengeToken
        {
            get => ViewModel.ChallengeToken;
            set => ViewModel.ChallengeToken = value;
        }

        public string Username
        {
            get => ViewModel.Username;
            set => ViewModel.Username = value;
        }

        public string Password
        {
            get => ViewModel.Password;
            set => ViewModel.Password = value;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // به محض ظاهر شدن صفحه، وضعیت چالش را بگیر
            await ViewModel.LoadStateAsync();
        }
    }
}
