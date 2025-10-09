using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace InstagramAuto.Client.ViewModels 
{
    public class ServiceStatusViewModel : BaseViewModel
    {
        private string _name;
        private string _status;
        private string _details;
        private DateTimeOffset _lastChecked;
        private bool _isHealthy;
        private int _usagePercent;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Status 
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public string Details
        {
            get => _details;
            set => SetProperty(ref _details, value);
        }

        public DateTimeOffset LastChecked
        {
            get => _lastChecked;
            set => SetProperty(ref _lastChecked, value);
        }

        public bool IsHealthy
        {
            get => _isHealthy; 
            set => SetProperty(ref _isHealthy, value);
        }

        public int UsagePercent
        {
            get => _usagePercent;
            set => SetProperty(ref _usagePercent, value);
        }

        public void UpdateFrom(ServiceStatus status)
        {
            Name = status.Name;
            Status = status.Status;
            Details = status.Details;
            LastChecked = status.LastChecked;
            IsHealthy = status.IsHealthy;
            UsagePercent = status.UsagePercent;
        }
    }
    
    public class ServiceStatus
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string Details { get; set; }
        public DateTimeOffset LastChecked { get; set; }
        public bool IsHealthy { get; set; }
        public int UsagePercent { get; set; }
    }
}