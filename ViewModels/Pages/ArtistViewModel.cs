using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Vibra_DesktopApp.Models;
using Vibra_DesktopApp.Singleton;

namespace Vibra_DesktopApp.ViewModels.Pages
{
    public partial class ArtistViewModel : ObservableObject
    {
        private readonly MainViewModel _mainVM;

        [ObservableProperty] private User artist;

        [ObservableProperty] private bool isMe;
        [ObservableProperty] private bool isFollowed;

        [ObservableProperty] private List<Song> artistSongs = [];
        [ObservableProperty] private List<Album> artistAlbums = [];

        public ArtistViewModel(MainViewModel mainVM, User artist)
        {
            _mainVM = mainVM ?? throw new ArgumentNullException(nameof(mainVM));
            Artist = artist ?? throw new ArgumentNullException(nameof(artist));

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            await CheckMeAsync().ConfigureAwait(false);
            await RefreshFollowStateAsync().ConfigureAwait(false);

            // Load songs/albums in parallel
            var songsTask = LoadArtistSongsAsync();
            var albumsTask = LoadArtistAlbumsAsync();
            await Task.WhenAll(songsTask, albumsTask).ConfigureAwait(false);
        }

        private Task CheckMeAsync()
        {
            var me = ApiManager.GetInstance().GetCurrentUser();
            IsMe = me?.id != null && Artist?.id != null && me.id == Artist.id;
            return Task.CompletedTask;
        }

        private async Task RefreshFollowStateAsync()
        {
            // Uses same library endpoint your sidebar uses
            var followed = await ApiManager.GetInstance()
                .HttpGetAsync<List<User>>("library/list-artist")
                .ConfigureAwait(false);

            if (followed == null || Artist?.id == null)
            {
                IsFollowed = false;
                return;
            }

            // Some APIs return artist in "artist" property; some return direct user.
            IsFollowed = followed.Any(a =>
                a?.id == Artist.id ||
                a?.artist?.id == Artist.id);
        }

        private async Task LoadArtistSongsAsync()
        {
            if (Artist?.id == null) return;

            var list = await ApiManager.GetInstance()
                .HttpGetAsync<List<Song>>($"artist/get-artist-songs/{Artist.id}")
                .ConfigureAwait(false);

            ArtistSongs = list ?? [];
        }

        private async Task LoadArtistAlbumsAsync()
        {
            if (Artist?.id == null) return;

            var list = await ApiManager.GetInstance()
                .HttpGetAsync<List<Album>>($"artist/get-artist-albums/{Artist.id}")
                .ConfigureAwait(false);

            ArtistAlbums = list ?? [];
        }

        [RelayCommand]
        private async Task FollowOrUnfollowAsync()
        {
            if (IsMe || Artist?.id == null) return;

            try
            {
                if (!IsFollowed)
                {
                    await ApiManager.GetInstance()
                        .HttpGetNoDataAsync($"artist/follow/{Artist.id}")
                        .ConfigureAwait(false);

                    IsFollowed = true;
                }
                else
                {
                    await ApiManager.GetInstance()
                        .HttpGetNoDataAsync($"library/destroy-favorite-artist/{Artist.id}")
                        .ConfigureAwait(false);

                    IsFollowed = false;
                }

                // refresh sidebar lists if you want (your sidebar doesn't auto-refresh now)
                // For now, just keep local state.
            }
            catch (Exception ex)
            {
                // keep similar behavior to your codebase
                Application.Current.Dispatcher.Invoke(() =>
                    MessageBox.Show(ex.Message));
            }
        }

        [RelayCommand]
        private async Task BlockArtistAsync()
        {
            if (IsMe || Artist?.id == null) return;

            try
            {
                await ApiManager.GetInstance()
                    .HttpGetNoDataAsync($"artist/block/{Artist.id}")
                    .ConfigureAwait(false);

                // try to unfollow too (matches your Vue logic)
                if (IsFollowed)
                {
                    await ApiManager.GetInstance()
                        .HttpGetNoDataAsync($"library/destroy-favorite-artist/{Artist.id}")
                        .ConfigureAwait(false);

                    IsFollowed = false;
                }

                // Navigate back home and set nav item
                Application.Current.Dispatcher.Invoke(() =>
                    _mainVM.NavigateTo(new HomeViewModel(_mainVM), NavigationItem.Home));
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                    MessageBox.Show(ex.Message));
            }
        }

        [RelayCommand]
        private async Task PlaySongAsync(Song song)
        {
            if (song == null) return;
            await SongManager.GetInstace().PlayOrPauseThisSongAsync(song).ConfigureAwait(false);
        }
    }
}
