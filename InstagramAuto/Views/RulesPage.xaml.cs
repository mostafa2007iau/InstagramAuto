using Microsoft.Maui.Controls;
using InstagramAuto.Client.ViewModels;

namespace InstagramAuto.Client.Views
{
    public partial class RulesPage : ContentPage
    {
        private RulesViewModel ViewModel;

        public RulesPage(RulesViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = ViewModel = viewModel;
            this.Appearing += async (_, __) =>
            {
                if (ViewModel.Rules.Count == 0)
                    await ViewModel.LoadRulesAsync();
            };
        }
    }
}
