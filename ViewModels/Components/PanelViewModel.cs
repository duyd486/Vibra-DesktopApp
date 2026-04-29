using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Vibra_DesktopApp.Models;
using Vibra_DesktopApp.Singleton;
using Vibra_DesktopApp.ViewModels;

namespace Vibra_DesktopApp.ViewModels.Components
{
    public partial class PanelViewModel : ObservableObject
    {
        private readonly MainViewModel _mainVM;

        public SongManager SongManager => SongManager.GetInstace();

        public ObservableCollection<Song> Waitlist => SongManager.Waitlist;

        public PanelContent PanelContent => _mainVM.PanelContent;

        public bool IsQueueContent => _mainVM.PanelContent == PanelContent.Queue;

        public bool IsDetailsContent => _mainVM.PanelContent == PanelContent.Details;

        [ObservableProperty] private bool _isMe;
        [ObservableProperty] private bool _isFollowed;

        public PanelViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM ?? throw new ArgumentNullException(nameof(mainVM));
            SongManager.PropertyChanged += OnSongManagerPropertyChanged;
            _mainVM.PropertyChanged += OnMainVmPropertyChanged;

            _ = RefreshFollowStateAsync();
        }

        private void OnMainVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.PanelContent))
            {
                OnPropertyChanged(nameof(PanelContent));
                OnPropertyChanged(nameof(IsQueueContent));
                OnPropertyChanged(nameof(IsDetailsContent));
            }
        }

        private void OnSongManagerPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SongManager.CurrentTrack) || e.PropertyName == nameof(SongManager.IsPlaying))
            {
                OnPropertyChanged(nameof(SongManager));

                if (e.PropertyName == nameof(SongManager.CurrentTrack))
                {
                    _ = RefreshFollowStateAsync();
                }
            }
        }

        private async Task RefreshFollowStateAsync()
        {
            try
            {
                var me = ApiManager.GetInstance().GetCurrentUser();
                var artist = SongManager.CurrentTrack?.author;

                IsMe = me?.id != null && artist?.id != null && me.id == artist.id;

                if (IsMe || artist?.id == null)
                {
                    IsFollowed = false;
                    return;
                }

                var followed = await ApiManager.GetInstance()
                    .HttpGetAsync<System.Collections.Generic.List<User>>("library/list-artist")
                    .ConfigureAwait(false);

                var isFollowed = followed?.Any(a => a?.id == artist.id || a?.artist?.id == artist.id) == true;

                Application.Current?.Dispatcher.Invoke(() => IsFollowed = isFollowed);
            }
            catch
            {
                Application.Current?.Dispatcher.Invoke(() => IsFollowed = false);
            }
        }

        [RelayCommand]
        private async Task FollowOrUnfollowCurrentArtistAsync()
        {
            if (IsMe) return;

            var artist = SongManager.CurrentTrack?.author;
            if (artist?.id == null) return;

            try
            {
                if (!IsFollowed)
                {
                    await ApiManager.GetInstance()
                        .HttpGetNoDataAsync($"artist/follow/{artist.id}")
                        .ConfigureAwait(false);

                    Application.Current?.Dispatcher.Invoke(() => IsFollowed = true);
                }
                else
                {
                    await ApiManager.GetInstance()
                        .HttpGetNoDataAsync($"library/destroy-favorite-artist/{artist.id}")
                        .ConfigureAwait(false);

                    Application.Current?.Dispatcher.Invoke(() => IsFollowed = false);
                }
            }
            catch (Exception ex)
            {
                Application.Current?.Dispatcher.Invoke(() => MessageBox.Show(ex.Message));
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

        [RelayCommand]
        private void ClosePanel()
        {
            _mainVM.ClosePanel();
        }
    }
}
