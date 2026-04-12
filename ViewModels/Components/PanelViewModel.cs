using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Vibra_DesktopApp.Models;
using Vibra_DesktopApp.Singleton;

namespace Vibra_DesktopApp.ViewModels.Components
{
    public partial class PanelViewModel : ObservableObject
    {
        private readonly MainViewModel _mainVM;

        public SongManager SongManager => SongManager.GetInstace();

        public ObservableCollection<Song> Waitlist => SongManager.Waitlist;

        public PanelViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM ?? throw new ArgumentNullException(nameof(mainVM));
            SongManager.PropertyChanged += OnSongManagerPropertyChanged;
        }

        private void OnSongManagerPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SongManager.CurrentTrack) || e.PropertyName == nameof(SongManager.IsPlaying))
            {
                OnPropertyChanged(nameof(SongManager));
            }
        }

        [RelayCommand]
        private async Task PlayOrPauseCurrentAsync()
        {
            await SongManager.PlayOrPauseAsync().ConfigureAwait(false);
        }

        [RelayCommand]
        private async Task PlayWaitlistItemAsync(Song song)
        {
            if (song is null)
                return;

            SongManager.RemoveFromWaitlist(song);
            await SongManager.PlayOrPauseThisSongAsync(song).ConfigureAwait(false);
        }

        [RelayCommand]
        private void RemoveWaitlistItem(Song song)
        {
            if (song is null)
                return;

            SongManager.RemoveFromWaitlist(song);
        }

        [RelayCommand]
        private void ClearWaitlist()
        {
            SongManager.ClearWaitlist();
        }

        [RelayCommand]
        private void TogglePanel()
        {
            _mainVM.TogglePanel();
        }
    }
}
