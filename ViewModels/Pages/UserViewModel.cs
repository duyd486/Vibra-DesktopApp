using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Vibra_DesktopApp.Models;
using Vibra_DesktopApp.Singleton;

namespace Vibra_DesktopApp.ViewModels.Pages
{
    public partial class UserViewModel : ObservableObject
    {
        private readonly MainViewModel _mainVM;

        public User? CurrentUser => ApiManager.GetInstance().GetCurrentUser();

        [ObservableProperty] private List<Album> myAlbums = [];
        [ObservableProperty] private List<Album> myPlaylists = [];
        [ObservableProperty] private List<User> followArtists = [];
        [ObservableProperty] private List<Song> mySongs = [];
        [ObservableProperty] private List<Payment> bills = [];

        [ObservableProperty] private bool isMenuOpen;

        public UserViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
            _ = RefreshAsync();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            try
            {
                // these endpoints already exist in your WPF codebase
                var albumsTask = ApiManager.GetInstance().HttpGetAsync<List<Album>>("library/list-playlist?type=1");
                var playlistsTask = ApiManager.GetInstance().HttpGetAsync<List<Album>>("library/list-playlist?type=2");
                var followArtistsTask = ApiManager.GetInstance().HttpGetAsync<List<User>>("library/list-artist");

                // these are from your Vue. If your API path differs, change here.
                var billsTask = ApiManager.GetInstance().HttpGetAsync<List<Payment>>("profile/payment-history");

                // TODO: confirm your backend route for "my songs"
                var mySongsTask = ApiManager.GetInstance().HttpGetAsync<List<Song>>("profile/list-song");

                await Task.WhenAll(albumsTask, playlistsTask, followArtistsTask, billsTask, mySongsTask);

                MyAlbums = albumsTask.Result ?? [];
                MyPlaylists = playlistsTask.Result ?? [];
                FollowArtists = followArtistsTask.Result ?? [];
                Bills = billsTask.Result ?? [];
                MySongs = mySongsTask.Result ?? [];
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show(ex.Message));
            }
        }

        [RelayCommand]
        private async Task PlaySongAsync(Song song)
        {
            if (song == null) return;
            await SongManager.GetInstace().PlayOrPauseThisSongAsync(song);
        }

        [RelayCommand]
        private void OpenAlbum(Album album)
        {
            if (album == null) return;
            _mainVM.NavigateTo(new AlbumViewModel(_mainVM, album), NavigationItem.Album);
        }

        [RelayCommand]
        private void OpenArtist(User artist)
        {
            if (artist == null) return;
            _mainVM.NavigateTo(new ArtistViewModel(_mainVM, artist), NavigationItem.Artist);
        }

        [RelayCommand]
        private async Task CreateAlbumAsync()
        {
            try
            {
                await ApiManager.GetInstance().HttpGetNoDataAsync("profile/create-album");
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show(ex.Message));
            }
        }

        [RelayCommand]
        private void ToggleMenu()
        {
            IsMenuOpen = !IsMenuOpen;
        }
    }
}
