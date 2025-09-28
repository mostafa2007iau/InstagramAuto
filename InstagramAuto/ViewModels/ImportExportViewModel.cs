using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using InstagramAuto.Client.Models;
using InstagramAuto.Client.Services;

namespace InstagramAuto.Client.ViewModels
{
    /// <summary>
    /// Persian:
    ///   ?????? ???? ???? ? ????? ????? ?????? (CSV/JSON).
    /// English:
    ///   ViewModel for importing and exporting rules (CSV/JSON).
    /// </summary>
    public class ImportExportViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private string _dataText;
        private string _errorMessage;

        public string DataText { get => _dataText; set { _dataText = value; OnPropertyChanged(); } }
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); } }
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public ICommand ExportJsonCommand { get; }
        public ICommand ExportCsvCommand { get; }
        public ICommand ImportJsonCommand { get; }
        public ICommand ImportCsvCommand { get; }

        public ImportExportViewModel(IAuthService authService)
        {
            _authService = authService;
            ExportJsonCommand = new Command(async () => await ExportJsonAsync());
            ExportCsvCommand = new Command(async () => await ExportCsvAsync());
            ImportJsonCommand = new Command(async () => await ImportJsonAsync());
            ImportCsvCommand = new Command(async () => await ImportCsvAsync());
        }

        public async Task ExportJsonAsync()
        {
            ErrorMessage = string.Empty;
            try
            {
                var session = await _authService.LoadSessionAsync();
                DataText = await _authService.ExportRulesAsync(session.AccountId, "json");
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        public async Task ExportCsvAsync()
        {
            ErrorMessage = string.Empty;
            try
            {
                var session = await _authService.LoadSessionAsync();
                DataText = await _authService.ExportRulesAsync(session.AccountId, "csv");
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        public async Task ImportJsonAsync()
        {
            ErrorMessage = string.Empty;
            try
            {
                var session = await _authService.LoadSessionAsync();
                await _authService.ImportRulesAsync(session.AccountId, DataText, "json");
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        public async Task ImportCsvAsync()
        {
            ErrorMessage = string.Empty;
            try
            {
                var session = await _authService.LoadSessionAsync();
                await _authService.ImportRulesAsync(session.AccountId, DataText, "csv");
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}
