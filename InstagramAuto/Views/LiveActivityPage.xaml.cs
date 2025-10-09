using Microsoft.Maui.Controls;
using InstagramAuto.Client.ViewModels;
using System;
using System.Threading.Tasks;

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
        private readonly LiveActivityViewModel _viewModel;

        public LiveActivityPage(LiveActivityViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InitializeAsync();
        }

        private void OnFilterChanged(object sender, EventArgs e)
        {
            if (sender is Picker picker)
            {
                string status = picker.SelectedIndex switch
                {
                    1 => "success",
                    2 => "error",
                    3 => "warning",
                    4 => "in_progress",
                    _ => null // All
                };

                _viewModel.FilterCommand.Execute(status);
            }
        }

        private async void OnActivityMenuClicked(object sender, EventArgs e)
        {
            if (sender is Button button && 
                button.CommandParameter is ValueTuple<ActivityItemViewModel, LiveActivityViewModel> param)
            {
                var (activity, vm) = param;
                
                var action = await DisplayActionSheet(
                    "??????",
                    "??????",
                    null,
                    "??? ??????",
                    "????????????");

                switch (action)
                {
                    case "??? ??????":
                        vm.CopyDetailsCommand.Execute(activity);
                        break;

                    case "????????????":
                        vm.ShareCommand.Execute(activity);
                        break;
                }
            }
        }
    }
}
