using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Vibra_DesktopApp.Models;
using Vibra_DesktopApp.Singleton;
using Vibra_DesktopApp.ViewModels.Pages;

namespace Vibra_DesktopApp.ViewModels.Components
{
    public partial class SidebarViewModel : ObservableObject
    {
        private readonly MainViewModel _mainVM;

        [ObservableProperty] private string _selectedFilter = "All";
        [ObservableProperty] private string _searchText = string.Empty;

        [ObservableProperty] private ObservableCollection<Album> _myAlbums = new();
        [ObservableProperty] private ObservableCollection<Album> _myPlaylists = new();
        [ObservableProperty] private ObservableCollection<User> _myArtists = new();

        public ICollectionView AlbumsView { get; }
        public ICollectionView PlaylistsView { get; }
        public ICollectionView ArtistsView { get; }

        public bool ShowPlaylists => SelectedFilter is "All" or "Playlist";
        public bool ShowAlbums => SelectedFilter is "All" or "Playlist";
        public bool ShowArtists => SelectedFilter is "All" or "Artist";

        public SidebarViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM ?? throw new ArgumentNullException(nameof(mainVM));

            AlbumsView = CollectionViewSource.GetDefaultView(MyAlbums);
            PlaylistsView = CollectionViewSource.GetDefaultView(MyPlaylists);
            ArtistsView = CollectionViewSource.GetDefaultView(MyArtists);

            AlbumsView.Filter = FilterAlbum;
            PlaylistsView.Filter = FilterPlaylist;
            ArtistsView.Filter = FilterArtist;

            _ = LoadAsync();
        }

        partial void OnSelectedFilterChanged(string value)
        {
            OnPropertyChanged(nameof(ShowPlaylists));
            OnPropertyChanged(nameof(ShowAlbums));
            OnPropertyChanged(nameof(ShowArtists));
        }

        partial void OnSearchTextChanged(string value)
        {
            AlbumsView.Refresh();
            PlaylistsView.Refresh();
            ArtistsView.Refresh();
        }

        private bool FilterAlbum(object obj)
        {
            if (obj is not Album album) return false;
            if (string.IsNullOrWhiteSpace(SearchText)) return true;
            return (album.name ?? string.Empty).Contains(SearchText, StringComparison.CurrentCultureIgnoreCase);
        }

        private bool FilterPlaylist(object obj)
        {
            if (obj is not Album playlist) return false;
            if (string.IsNullOrWhiteSpace(SearchText)) return true;
            return (playlist.name ?? string.Empty).Contains(SearchText, StringComparison.CurrentCultureIgnoreCase);
        }

        private bool FilterArtist(object obj)
        {
            if (obj is not User wrapper) return false;

            // Your API returns either a plain User or a wrapper with .artist populated.
            var name = wrapper.artist?.name ?? wrapper.name ?? string.Empty;

            if (string.IsNullOrWhiteSpace(SearchText)) return true;
            return name.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase);
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task LoadAsync()
        {
            try
            {
                var playlists = await ApiManager.GetInstance()
                    .HttpGetAsync<List<Album>>("library/list-playlist?type=2")
                    .ConfigureAwait(false);

                var albums = await ApiManager.GetInstance()
                    .HttpGetAsync<List<Album>>("library/list-playlist?type=1")
                    .ConfigureAwait(false);

                var artists = await ApiManager.GetInstance()
                    .HttpGetAsync<List<User>>("library/list-artist")
                    .ConfigureAwait(false);

                App.Current.Dispatcher.Invoke(() =>
                {
                    MyPlaylists = new ObservableCollection<Album>(playlists ?? []);
                    MyAlbums = new ObservableCollection<Album>(albums ?? []);
                    MyArtists = new ObservableCollection<User>(artists ?? []);

                    // Rebind views to the new collections
                    ((ListCollectionView)PlaylistsView).SourceCollection.Cast<object>();
                    PlaylistsView.Refresh();
                    AlbumsView.Refresh();
                    ArtistsView.Refresh();
                });
            }
            catch
            {
                // keep UI responsive; ApiManager already shows errors
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task CreatePlaylistAsync()
        {
            await ApiManager.GetInstance()
                .HttpPostNoDataAsync("library/store-playlist")
                .ConfigureAwait(false);

            await LoadAsync().ConfigureAwait(false);
        }

        [RelayCommand]
        private void SetFilter(string filter)
        {
            SelectedFilter = filter;
        }

        [RelayCommand]
        public void AlbumClick(Album album)
        {
            _mainVM.NavigateTo(new AlbumViewModel(_mainVM, album), NavigationItem.Album);
        }

        [RelayCommand]
        public void PlaylistClick(Album playlist)
        {
            _mainVM.NavigateTo(new AlbumViewModel(_mainVM, playlist), NavigationItem.Album);
        }

        [RelayCommand]
        public void ArtistClick(User artist)
        {
            _mainVM.NavigateTo(new ArtistViewModel(_mainVM, artist), NavigationItem.Artist);
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task DeletePlaylistAsync(Album playlist)
        {
            if (playlist.id is null) return;

            await ApiManager.GetInstance()
                .HttpGetNoDataAsync($"library/destroy-playlist/{playlist.id}")
                .ConfigureAwait(false);

            await LoadAsync().ConfigureAwait(false);

            _mainVM.NavigateTo(new HomeViewModel(_mainVM), NavigationItem.Home);
        }
    }
}
