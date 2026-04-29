using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Vibra_DesktopApp.Singleton;
using Vibra_DesktopApp.Models;
using Vibra_DesktopApp.ViewModels;

namespace Vibra_DesktopApp.ViewModels.Components
{
    public partial class PlayerViewModel : ObservableObject
    {
        private readonly MainViewModel _mainVM;

        private readonly FavoriteSongManager _favoriteSongManager = FavoriteSongManager.GetInstance();

        public SongManager SongManager => SongManager.GetInstace();

        // Tracks whether playback was active when the user started seeking
        private bool _wasPlaying;

        // Bound to the Slider.Value. While dragging we modify this and only apply to the player on seek complete.
        [ObservableProperty] private double _sliderValue;

        // True while user is dragging the slider thumb
        [ObservableProperty] private bool _isSeeking;

        // Volume (0.0 - 1.0) bound to the UI
        [ObservableProperty] private double _volume;

        [ObservableProperty] private bool _isAddToPlaylistMenuOpen;

        [ObservableProperty]
        private ObservableCollection<Album> _availablePlaylistsForCurrentTrack = new();

        public PlayerViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;

            // initialize slider and volume from player
            SliderValue = SongManager.CurrentTime;
            Volume = Math.Clamp(SongManager.Volume, 0.0, 1.0);

            // keep SliderValue in sync with SongManager.CurrentTime when not seeking
            SongManager.PropertyChanged += OnSongManagerPropertyChanged;

            _favoriteSongManager.Songs.CollectionChanged += (_, __) =>
                OnPropertyChanged(nameof(IsCurrentTrackLoved));

            _ = RefreshAvailablePlaylistsForCurrentTrackAsync();
        }

        public bool IsCurrentTrackLoved => _favoriteSongManager.IsFavorite(SongManager.CurrentTrack);

        partial void OnVolumeChanged(double value)
        {
            var v = Math.Clamp(value, 0.0, 1.0);
            // Apply volume to the SongManager (which will marshal to the UI dispatcher)
            SongManager.Volume = v;
        }

        private void OnSongManagerPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SongManager.CurrentTime) && !IsSeeking)
            {
                // update the slider to reflect current playback position
                SliderValue = SongManager.CurrentTime;
            }

            if (e.PropertyName == nameof(SongManager.Duration))
            {
                // clamp slider if duration changed and current value is out of range
                if (SongManager.Duration > 0 && SliderValue > SongManager.Duration)
                {
                    SliderValue = SongManager.Duration;
                }

            }

            if (e.PropertyName == nameof(SongManager.Volume))
            {
                // keep viewmodel volume in sync if volume changed elsewhere
                var v = Math.Clamp(SongManager.Volume, 0.0, 1.0);
                if (Math.Abs(Volume - v) > 0.0001)
                {
                    Volume = v;
                }
            }

            if (e.PropertyName == nameof(SongManager.CurrentTrack))
            {
                OnPropertyChanged(nameof(IsCurrentTrackLoved));
                _ = RefreshAvailablePlaylistsForCurrentTrackAsync();
                IsAddToPlaylistMenuOpen = false;
            }
        }

        private async Task RefreshAvailablePlaylistsForCurrentTrackAsync()
        {
            try
            {
                var track = SongManager.CurrentTrack;
                if (track?.id is null)
                {
                    App.Current?.Dispatcher.Invoke(() => AvailablePlaylistsForCurrentTrack.Clear());
                    return;
                }

                var playlists = await ApiManager.GetInstance()
                    .HttpGetAsync<List<Album>>("library/list-playlist?type=2")
                    .ConfigureAwait(false);

                var candidatePlaylists = playlists ?? [];
                var eligible = new List<Album>();

                foreach (var p in candidatePlaylists)
                {
                    if (p.id is null) continue;

                    List<Song>? songsInPlaylist = null;
                    try
                    {
                        songsInPlaylist = await ApiManager.GetInstance()
                            .HttpGetAsync<List<Song>>($"library/list-playlist-song/{p.id}")
                            .ConfigureAwait(false);
                    }
                    catch
                    {
                        // ignore per-playlist failures; will just not show it
                    }

                    var alreadyHas = songsInPlaylist?.Any(s => s?.song_id == track.id) == true;
                    if (!alreadyHas)
                        eligible.Add(p);
                }

                App.Current.Dispatcher.Invoke(() =>
                {
                    AvailablePlaylistsForCurrentTrack.Clear();
                    foreach (var p in eligible) AvailablePlaylistsForCurrentTrack.Add(p);
                });
            }
            catch
            {
                // ApiManager already shows errors; keep UI responsive
                App.Current?.Dispatcher.Invoke(() => AvailablePlaylistsForCurrentTrack.Clear());
            }
        }

        [RelayCommand]
        private async Task ToggleAddToPlaylistMenuAsync()
        {
            IsAddToPlaylistMenuOpen = !IsAddToPlaylistMenuOpen;
            if (IsAddToPlaylistMenuOpen)
            {
                await RefreshAvailablePlaylistsForCurrentTrackAsync().ConfigureAwait(false);
            }
        }

        [RelayCommand]
        private async Task AddCurrentTrackToPlaylistAsync(Album playlist)
        {
            var track = SongManager.CurrentTrack;
            if (track?.id is null || playlist?.id is null) return;

            try
            {
                await ApiManager.GetInstance()
                    .HttpGetNoDataAsync($"song/add-song-to-playlist?song_id={track.id}&playlist_id={playlist.id}")
                    .ConfigureAwait(false);

                IsAddToPlaylistMenuOpen = false;
                await RefreshAvailablePlaylistsForCurrentTrackAsync().ConfigureAwait(false);

                await _mainVM.SidebarVM.RefreshAsync().ConfigureAwait(false);
            }
            catch
            {
                // ApiManager already shows errors
                IsAddToPlaylistMenuOpen = false;
            }
        }

        [RelayCommand]
        private async Task ToggleLoveAsync()
        {
            var track = SongManager.CurrentTrack;
            if (track is null || track.id is null) return;

            if (IsCurrentTrackLoved)
                await _favoriteSongManager.UnloveAsync(track).ConfigureAwait(false);
            else
                await _favoriteSongManager.LoveAsync(track).ConfigureAwait(false);

            OnPropertyChanged(nameof(IsCurrentTrackLoved));
        }

        [RelayCommand]
        public void TogglePanel()
        {
            _mainVM.TogglePanel();
        }

        [RelayCommand]
        public void ToggleQueuePanel()
        {
            if (_mainVM.IsPanelOpen && _mainVM.PanelContent == PanelContent.Queue)
            {
                _mainVM.ClosePanel();
                return;
            }

            _mainVM.OpenPanel(PanelContent.Queue);
        }

        [RelayCommand]
        public void ToggleSongDetailsPanel()
        {
            if (_mainVM.IsPanelOpen && _mainVM.PanelContent == PanelContent.Details)
            {
                _mainVM.ClosePanel();
                return;
            }

            _mainVM.OpenPanel(PanelContent.Details);
        }

        public bool IsShuffle
        {
            get => SongManager.IsShuffle;
            set
            {
                if (SongManager.IsShuffle == value)
                    return;

                SongManager.IsShuffle = value;
                OnPropertyChanged();
            }
        }

        [RelayCommand]
        private void ToggleShuffle()
        {
            IsShuffle = !IsShuffle;
        }

        [RelayCommand]
        private async Task Next()
        {
            await SongManager.NextAsync().ConfigureAwait(false);
        }

        [RelayCommand]
        private async Task Prev()
        {
            await SongManager.PrevAsync().ConfigureAwait(false);
        }

        [RelayCommand]
        public async Task PlayOrPause()
        {
            await SongManager.PlayOrPauseAsync().ConfigureAwait(false);
        }

        // Called when the user begins dragging the slider thumb
        [RelayCommand]
        public async Task SeekStarted()
        {
            // mark seeking and remember play state
            IsSeeking = true;
            _wasPlaying = SongManager.IsPlaying;

            if (_wasPlaying)
            {
                await SongManager.PauseAsync().ConfigureAwait(false);
            }
        }

        // Called when the user finishes dragging the slider thumb.
        // Uses the bound SliderValue property (two-way) rather than a parameter.
        [RelayCommand]
        public async Task SeekCompleted()
        {
            // ensure valid value
            if (double.IsNaN(SliderValue) || double.IsInfinity(SliderValue))
            {
                IsSeeking = false;
                return;
            }

            var target = Math.Max(0.0, SliderValue);
            if (SongManager.Duration > 0)
                target = Math.Min(target, SongManager.Duration);

            // perform seek while paused
            await SongManager.SeekAsync(target).ConfigureAwait(false);

            // resume if it was playing before seek started
            if (_wasPlaying)
            {
                await SongManager.PlayAsync().ConfigureAwait(false);
                _wasPlaying = false;
            }

            IsSeeking = false;
        }

        // Fallback seek command (immediate seek) — kept for existing bindings if needed.
        [RelayCommand]
        public async Task Seek(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds))
                return;

            var target = Math.Max(0.0, seconds);
            if (SongManager.Duration > 0)
                target = Math.Min(target, SongManager.Duration);

            await SongManager.SeekAsync(target).ConfigureAwait(false);
        }

        // Seek by percentage (0.0 - 1.0) of the track duration
        [RelayCommand]
        public async Task SeekToPercentage(double percentage)
        {
            percentage = Math.Clamp(percentage, 0.0, 1.0);
            await SongManager.SeekToPercentageAsync(percentage).ConfigureAwait(false);
        }
    }
}
