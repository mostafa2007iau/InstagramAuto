using System;
using Microsoft.Maui.Controls;
using InstagramAuto.Client.ViewModels;

namespace InstagramAuto.Client.Views
{
    /// <summary>
    /// Persian:
    ///   ???? ????? ????????? ?? ??? ? ?????? ??????/??????.
    /// English:
    ///   Page for displaying comments of a post and managing reply/DM.
    /// </summary>
    public partial class CommentsPage : ContentPage
    {
        private readonly CommentsViewModel ViewModel;

        public CommentsPage(CommentsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = ViewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (ViewModel.Items.Count == 0)
                await ViewModel.LoadMoreAsync();
        }
    }
}
