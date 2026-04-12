using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Vibra_DesktopApp.Models;
using Vibra_DesktopApp.Singleton;

namespace Vibra_DesktopApp.ViewModels.Components
{
    public partial class TrackRowViewModel : ObservableObject
    {
        private readonly SongManager _songManager;

        public Song Track { get; }
        public int Index { get; }

        public Song? CurrentTrack => _songManager.CurrentTrack;
        public bool IsPlaying => _songManager.IsPlaying;

        public TrackRowViewModel(Song track, int index, SongManager songManager)
        {
            Track = track ?? throw new ArgumentNullException(nameof(track));
            Index = index;
            _songManager = songManager ?? throw new ArgumentNullException(nameof(songManager));

            _songManager.PropertyChanged += OnSongManagerPropertyChanged;
        }

        private void OnSongManagerPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SongManager.CurrentTrack) || e.PropertyName == nameof(SongManager.IsPlaying))
            {
                OnPropertyChanged(nameof(CurrentTrack));
                OnPropertyChanged(nameof(IsPlaying));
                OnPropertyChanged(nameof(IsCurrentTrack));
            }
        }

        public bool IsCurrentTrack => CurrentTrack?.id != null && Track?.id != null && CurrentTrack.id == Track.id;

        [RelayCommand]
        private async Task PlayOrPauseAsync()
        {
            await _songManager.PlayOrPauseThisSongAsync(Track).ConfigureAwait(false);
        }

        [RelayCommand]
        private void AddToWaitlist()
        {
            _songManager.Enqueue(Track);
        }
    }
}
