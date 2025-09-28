using System;
using Microsoft.Maui.Controls;
using InstagramAuto.Client.ViewModels;

namespace InstagramAuto.Client.Views
{
    /// <summary>
    /// Persian:
    ///   ???? ???? ???? ? ????? ????? ?????? (CSV/JSON).
    /// English:
    ///   Page for importing and exporting rules (CSV/JSON).
    /// </summary>
    public partial class ImportExportPage : ContentPage
    {
        private readonly ImportExportViewModel ViewModel;

        public ImportExportPage(ImportExportViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = ViewModel = viewModel;
        }
    }
}
