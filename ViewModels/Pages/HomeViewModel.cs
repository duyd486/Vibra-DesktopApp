using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Vibra_DesktopApp.Models;
using Vibra_DesktopApp.Singleton;
using Vibra_DesktopApp.ViewModels.Pages;

namespace Vibra_DesktopApp.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly MainViewModel _mainVM;
        [ObservableProperty] private List<Song>? listSong;
        [ObservableProperty] private List<Album>? listAlbum;
        [ObservableProperty] private List<User>? listArtist;
        [ObservableProperty] private List<Song>? listRecentRotation;

        [ObservableProperty] private bool isLoading = true;

        public HomeViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
            _ = RefreshAllAsync();
        }

        private async Task RefreshAllAsync()
        {
            try
            {
                isLoading = true;
                OnPropertyChanged(nameof(IsLoading));
                await Task.WhenAll(
                    RefreshListSongAsync(),
                    RefreshListAlbumAsync(),
                    RefreshListArtistAsync(),
                    RefreshListRecentRotationAsync());
            }
            finally
            {
                isLoading = false;
                OnPropertyChanged(nameof(IsLoading));
            }
        }


        private async Task RefreshListSongAsync()
        {
            ListSong = await ApiManager.GetInstance().HttpGetAsync<List<Song>>("home/list-song");
        }

        private async Task RefreshListAlbumAsync()
        {
            ListAlbum = await ApiManager.GetInstance().HttpGetAsync<List<Album>>("home/list-album");
        }

        private async Task RefreshListArtistAsync()
        {
            ListArtist = await ApiManager.GetInstance().HttpGetAsync<List<User>>("home/list-artist");
        }

        private async Task RefreshListRecentRotationAsync()
        {
            ListRecentRotation = await ApiManager.GetInstance().HttpGetAsync<List<Song>>("home/recent-rotation?limit=5");
        }
        


        [RelayCommand]
        public async Task PlayOrPauseThisSong(Song song)
        {
            await SongManager.GetInstace().PlayOrPauseThisSongAsync(song);
        }

        [RelayCommand]
        private void AddToWaitlist(Song song)
        {
            if (song is null)
                return;

            SongManager.GetInstace().Enqueue(song);
        }

        [RelayCommand]
        public void OpenAlbumDetail(Album album)
        {
            _mainVM.NavigateTo(new AlbumViewModel(_mainVM, album));
        }

        [RelayCommand]
        public void OpenArtistDetail(User artist)
        {
            _mainVM.NavigateTo(new ArtistViewModel(_mainVM, artist));
        }
    }
}
