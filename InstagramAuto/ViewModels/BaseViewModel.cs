using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace InstagramAuto.Client.ViewModels
{
    /// <summary>
    /// Base ViewModel implementing INotifyPropertyChanged for all ViewModels.
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
