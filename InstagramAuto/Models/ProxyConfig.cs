using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace InstagramAuto.Client.Models
{
    /// <summary>
    /// Persian: ??????? ??????
    /// English: Proxy configuration
    /// </summary>
    public class ProxyConfig : INotifyPropertyChanged
    {
        private string _id;
        private bool _enabled;
        private string _address;
        private int _port;
        private string _username;
        private string _password;
        private string _title;
        private int _priority;
        private bool _isAvailable;
        private string _lastCheckResult;
        private string _lastCheckError;
        private string _status;

        [JsonProperty("id")]
        public string Id 
        { 
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonProperty("title")]
        public string Title 
        { 
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonProperty("address")]
        public string Address 
        { 
            get => _address;
            set
            {
                if (_address != value)
                {
                    _address = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FullAddress));
                }
            }
        }

        [JsonProperty("port")]
        public int Port 
        { 
            get => _port;
            set
            {
                if (_port != value)
                {
                    _port = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FullAddress));
                }
            }
        }

        [JsonProperty("username")]
        public string Username 
        { 
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonProperty("password")]
        public string Password 
        { 
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonProperty("enabled")]
        public bool Enabled 
        { 
            get => _enabled;
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonProperty("priority")]
        public int Priority 
        { 
            get => _priority;
            set
            {
                if (_priority != value)
                {
                    _priority = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonProperty("is_available")]
        public bool IsAvailable 
        { 
            get => _isAvailable;
            set
            {
                if (_isAvailable != value)
                {
                    _isAvailable = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonProperty("last_check_result")]
        public string LastCheckResult 
        { 
            get => _lastCheckResult;
            set
            {
                if (_lastCheckResult != value)
                {
                    _lastCheckResult = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonProperty("last_check_error")]
        public string LastCheckError 
        { 
            get => _lastCheckError;
            set
            {
                if (_lastCheckError != value)
                {
                    _lastCheckError = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonProperty("status")]
        public string Status 
        { 
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Persian: ???? ???? ?????? ?? ????
        /// English: Full proxy address with port
        /// </summary>
        [JsonIgnore]
        public string FullAddress => $"{Address}:{Port}";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"{Title} ({FullAddress})";
        }
    }
}