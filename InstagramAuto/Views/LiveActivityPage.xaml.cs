using System;
using Microsoft.Maui.Controls;
using InstagramAuto.Client.ViewModels;

namespace InstagramAuto.Client.Views
{
    /// <summary>
    /// Persian:
    ///   ???? ????? ?????????? ???? (?? ??? ????? ?? ??????????? ???).
    /// English:
    ///   Page for displaying live or scheduled activities.
    /// </summary>
    public partial class LiveActivityPage : ContentPage
    {
        private readonly LiveActivityViewModel ViewModel;

        public LiveActivityPage(LiveActivityViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = ViewModel = viewModel;
        }
    }
}
