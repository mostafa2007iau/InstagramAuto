using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using InstagramAuto.Client.Models;
using InstagramAuto.Client.Services;

namespace InstagramAuto.Client.ViewModels
{
    /// <summary>
    /// Persian:
    ///     ?????? ??????? ????.
    /// English:
    ///     View model for risk settings.
    /// </summary>
    public class RiskSettingsViewModel : BaseViewModel
    {
        private readonly IRiskManager _riskManager;
        private RiskSettings _settings;
        private bool _isBusy;
        private string _errorMessage;
        private int _cooldownMinutes;
        private bool _autoPause;

        public ICommand SaveCommand { get; }
        public ICommand ResetCommand { get; }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public int CooldownMinutes
        {
            get => _cooldownMinutes;
            set => SetProperty(ref _cooldownMinutes, value);
        }

        public bool AutoPause
        {
            get => _autoPause;
            set => SetProperty(ref _autoPause, value);
        }

        public Dictionary<ActionType, ActionLimits> Limits
        {
            get => _settings?.Limits;
            set
            {
                if (_settings != null)
                {
                    _settings.Limits = value;
                    OnPropertyChanged();
                }
            }
        }

        public RiskSettingsViewModel(IRiskManager riskManager)
        {
            _riskManager = riskManager;

            SaveCommand = new Command(async () => await SaveAsync(), () => !IsBusy);
            ResetCommand = new Command(async () => await LoadSettingsAsync(), () => !IsBusy);

            LoadSettingsAsync().ConfigureAwait(false);
        }

        public async Task LoadSettingsAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                _settings = await _riskManager.GetSettingsAsync();
                
                if (_settings != null)
                {
                    CooldownMinutes = _settings.CooldownMinutes;
                    AutoPause = _settings.AutoPause;
                    OnPropertyChanged(nameof(Limits));
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SaveAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                if (_settings != null)
                {
                    _settings.CooldownMinutes = CooldownMinutes;
                    _settings.AutoPause = AutoPause;
                    await _riskManager.SaveSettingsAsync(_settings);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}