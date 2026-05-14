using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Vibra_DesktopApp.Models;
using Vibra_DesktopApp.Singleton;
using Vibra_DesktopApp.ViewModels.Components;
using Vibra_DesktopApp.ViewModels.Pages;

namespace Vibra_DesktopApp.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly MainViewModel _mainVM;
        [ObservableProperty] private List<Song>? listSong;
        [ObservableProperty] private List<Song>? listSongForYou;
        [ObservableProperty] private List<Album>? listAlbum;
        [ObservableProperty] private List<User>? listArtist;
        [ObservableProperty] private List<Song>? listRecentRotation;

        [ObservableProperty] private ObservableCollection<TrackRowViewModel> tracks = new();

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
                IsLoading = true;
                await Task.WhenAll(
                    LoadTracksAsync(),
                    RefreshListSongAsync(),
                    RefreshListAlbumAsync(),
                    RefreshListArtistAsync(),
                    RefreshListRecentRotationAsync());
            }
            finally
            {
                IsLoading = false;
            }
        }


        private async Task RefreshListSongAsync()
        {
            ListSong = await ApiManager.GetInstance().HttpGetAsync<List<Song>>("home/list-song");
            ListSongForYou = ListSong?.Take(8).ToList();
        }

        private async Task RefreshListAlbumAsync()
        {
            ListAlbum = (await ApiManager.GetInstance().HttpGetAsync<List<Album>>("home/list-album"))?.Take(10).ToList();
        }

        private async Task RefreshListArtistAsync()
        {
            ListArtist = (await ApiManager.GetInstance().HttpGetAsync<List<User>>("home/list-artist"))?.Take(10).ToList();
        }

        private async Task RefreshListRecentRotationAsync()
        {
            var recent = await ApiManager.GetInstance()
                .HttpGetAsync<List<Song>>("home/recent-rotation?limit=5")
                .ConfigureAwait(false);

            ListRecentRotation = recent?.Take(5).ToList();
        }

        private async Task LoadTracksAsync()
        {
            IsLoading = true;
            try
            {
                List<Song>? list;

                var raw = await ApiManager.GetInstance()
                    .HttpGetAsync<List<Song>>($"home/list-song")
                    .ConfigureAwait(false);

                list = raw;

                list = list?.Take(8).ToList();

                var songManager = SongManager.GetInstace();
                var vms = new ObservableCollection<TrackRowViewModel>();
                var i = 1;
                foreach (var s in list ?? [])
                {
                    vms.Add(new TrackRowViewModel(s, i++, songManager));
                }

                Tracks = vms;

                //MessageBox.Show("Tracks loaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show("Failed to load tracks. Please try again later.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Tracks = new ObservableCollection<TrackRowViewModel>();
            }
            finally
            {
                IsLoading = false;
            }
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
