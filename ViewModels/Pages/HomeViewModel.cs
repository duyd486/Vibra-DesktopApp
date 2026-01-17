using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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

        public HomeViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
            RefreshListSong();
            RefreshListAlbum();
            RefreshListArtist();
        }

        public async void RefreshListSong()
        {
            ListSong = await ApiManager.GetInstance().HttpGetAsync<List<Song>>("home/list-song");
        }
        public async void RefreshListAlbum()
        {
            ListAlbum = await ApiManager.GetInstance().HttpGetAsync<List<Album>>("home/list-album");
        }
        public async void RefreshListArtist()
        {
            ListArtist = await ApiManager.GetInstance().HttpGetAsync<List<User>>("home/list-artist");
        }



        [RelayCommand]
        public void PlayOrPauseThisSong(Song song)
        {
            SongManager.GetInstace().PlayOrPauseThisSong(song);
        }

        [RelayCommand]
        public void OpenAlbumDetail(Album album)
        {
            _mainVM.NavigateTo(new AlbumViewModel(_mainVM, album));
        }
    }
}
