using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using InstagramAuto.Client.Models;
using InstagramAuto.Client.Services;
using Microsoft.Maui.Controls;

namespace InstagramAuto.Client.ViewModels
{
    public class SessionManagerViewModel : BaseViewModel
    {
        private readonly ISessionManager _sessionManager;
        public ObservableCollection<AccountSession> Sessions { get; set; } = new();
        public ICommand RemoveSessionCommand { get; }

        public SessionManagerViewModel(ISessionManager sessionManager)
        {
            _sessionManager = sessionManager;
            RemoveSessionCommand = new Command<AccountSession>(async (s) => await RemoveSessionAsync(s));
        }

        public async Task LoadAsync()
        {
            var sessions = await _sessionManager.GetAllSessionsAsync();
            Sessions.Clear();
            foreach (var s in sessions)
                Sessions.Add(s);
        }

        private async Task RemoveSessionAsync(AccountSession session)
        {
            if (session == null) return;
            await _sessionManager.RemoveSessionAsync(session.Id);
            Sessions.Remove(session);
        }
    }
}
