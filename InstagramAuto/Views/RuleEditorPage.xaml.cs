using System;
using Microsoft.Maui.Controls;
using InstagramAuto.Client.ViewModels;

namespace InstagramAuto.Client.Views
{
    /// <summary>
    /// Persian:
    ///   ???? ?????? ? ????? ????? ?????? ? ??????.
    /// English:
    ///   Page for editing and defining reply/DM rules.
    /// </summary>
    public partial class RuleEditorPage : ContentPage
    {
        private readonly RuleEditorViewModel ViewModel;

        public RuleEditorPage(RuleEditorViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = ViewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await ViewModel.InitializeAsync();
        }
    }
}
