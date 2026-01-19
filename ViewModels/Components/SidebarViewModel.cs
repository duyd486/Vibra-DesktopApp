using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Vibra_DesktopApp.Models;
using Vibra_DesktopApp.Singleton;
using Vibra_DesktopApp.ViewModels.Pages;
using Vibra_DesktopApp.Views.Pages;

namespace Vibra_DesktopApp.ViewModels.Components
{
    public partial class SidebarViewModel : ObservableObject
    {
        private readonly MainViewModel _mainVM;

        [ObservableProperty] List<Album>? myAlbums;
        [ObservableProperty] List<User>? myArtists;
        [ObservableProperty] List<Album>? myPlaylists;

        public SidebarViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;

            GetMyPlaylist();
            GetMyAlbum();
            GetMyArtist();
        }




        public async void GetMyPlaylist()
        {
            MyPlaylists = await ApiManager.GetInstance().HttpGetAsync<List<Album>>("library/list-playlist?type=2");
        }
        public async void GetMyAlbum()
        {
            MyAlbums = await ApiManager.GetInstance().HttpGetAsync<List<Album>>("library/list-playlist?type=1");
        }
        public async void GetMyArtist()
        {
            MyArtists = await ApiManager.GetInstance().HttpGetAsync<List<User>>("library/list-artist");
        }



        [RelayCommand]
        public void AlbumClick(Album album)
        {
            _mainVM.NavigateTo(new AlbumViewModel(_mainVM, album));
        }

        [RelayCommand]
        public void ArtistClick(User artist)
        {
            _mainVM.NavigateTo(new ArtistViewModel(_mainVM, artist));
        }

        [RelayCommand]
        public void PlaylistClick(Album playlist)
        {
            _mainVM.NavigateTo(new AlbumViewModel(_mainVM, playlist));
        }
    }
}
