using System;
using Microsoft.Maui.Controls;
using InstagramAuto.Client.ViewModels;

namespace InstagramAuto.Client.Views
{
    /// <summary>
    /// Persian:
    ///   ???? ????? ???? ?????????? ????/?????? ???? ?? ???.
    /// English:
    ///   Page for displaying stats of successful/failed replies per post.
    /// </summary>
    public partial class StatsPage : ContentPage
    {
        private readonly StatsViewModel ViewModel;

        public StatsPage(StatsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = ViewModel = viewModel;
        }
    }
}
