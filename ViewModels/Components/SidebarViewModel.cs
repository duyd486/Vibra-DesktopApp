using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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

        [ObservableProperty] private ObservableCollection<SelectableAlbumViewModel> _myAlbums = new();
        [ObservableProperty] private ObservableCollection<SelectableAlbumViewModel> _myPlaylists = new();
        [ObservableProperty] private ObservableCollection<SelectableArtistViewModel> _myArtists = new();

        private Album? _selectedAlbum;
        public Album? SelectedAlbum
        {
            get => _selectedAlbum;
            set => SetProperty(ref _selectedAlbum, value);
        }

        private Album? _selectedPlaylist;
        public Album? SelectedPlaylist
        {
            get => _selectedPlaylist;
            set => SetProperty(ref _selectedPlaylist, value);
        }

        private User? _selectedArtist;
        public User? SelectedArtist
        {
            get => _selectedArtist;
            set => SetProperty(ref _selectedArtist, value);
        }

        private User? _selectedArtistWrapper;
        public User? SelectedArtistWrapper
        {
            get => _selectedArtistWrapper;
            set => SetProperty(ref _selectedArtistWrapper, value);
        }

        private bool _isFavoriteSongsSelected;
        public bool IsFavoriteSongsSelected
        {
            get => _isFavoriteSongsSelected;
            set => SetProperty(ref _isFavoriteSongsSelected, value);
        }

        private NavigationItem _selectedNavigationItem = NavigationItem.None;
        public NavigationItem SelectedNavigationItem
        {
            get => _selectedNavigationItem;
            set => SetProperty(ref _selectedNavigationItem, value);
        }

        private int? _selectedAlbumId;
        public int? SelectedAlbumId
        {
            get => _selectedAlbumId;
            set => SetProperty(ref _selectedAlbumId, value);
        }

        private int? _selectedPlaylistId;
        public int? SelectedPlaylistId
        {
            get => _selectedPlaylistId;
            set => SetProperty(ref _selectedPlaylistId, value);
        }

        private int? _selectedArtistId;
        public int? SelectedArtistId
        {
            get => _selectedArtistId;
            set => SetProperty(ref _selectedArtistId, value);
        }

        public ICollectionView AlbumsView { get; }
        public ICollectionView PlaylistsView { get; }
        public ICollectionView ArtistsView { get; }

        public bool ShowPlaylists => SelectedFilter is "All" or "Playlist";
        public bool ShowAlbums => SelectedFilter is "All" or "Playlist";
        public bool ShowArtists => SelectedFilter is "All" or "Artist";

        public int FavoriteSongCount => FavoriteSongManager.GetInstance().Songs.Count;

        public SidebarViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM ?? throw new ArgumentNullException(nameof(mainVM));

            SidebarSelectionBus.GetInstance().SelectionChanged += (_, e) =>
                App.Current.Dispatcher.Invoke(() => ApplySelection(e.NavigationItem, e.Id));

            FavoriteSongManager.GetInstance().Songs.CollectionChanged += (_, __) =>
                OnPropertyChanged(nameof(FavoriteSongCount));

            AlbumsView = CollectionViewSource.GetDefaultView(MyAlbums);
            PlaylistsView = CollectionViewSource.GetDefaultView(MyPlaylists);
            ArtistsView = CollectionViewSource.GetDefaultView(MyArtists);

            AlbumsView.Filter = FilterAlbum;
            PlaylistsView.Filter = FilterPlaylist;
            ArtistsView.Filter = FilterArtist;

            _ = LoadAsync();    
            _ = FavoriteSongManager.GetInstance().LoadAsync();
        }

        public async Task RefreshAsync()
        {
            await LoadAsync().ConfigureAwait(false);
        }

        partial void OnSelectedFilterChanged(string value)
        {
            OnPropertyChanged(nameof(ShowPlaylists));
            OnPropertyChanged(nameof(ShowAlbums));
            OnPropertyChanged(nameof(ShowArtists));
        }

        [RelayCommand]
        private void OpenFavoriteSongs()
        {
            foreach (var a in MyAlbums) a.IsSelected = false;
            foreach (var p in MyPlaylists) p.IsSelected = false;
            foreach (var ar in MyArtists) ar.IsSelected = false;

            IsFavoriteSongsSelected = true;
            SelectedNavigationItem = NavigationItem.Album;
            SelectedAlbumId = null;
            SelectedPlaylistId = null;
            SelectedArtistId = null;
            SelectedAlbum = null;
            SelectedPlaylist = null;
            SelectedArtist = null;
            SelectedArtistWrapper = null;
            _mainVM.NavigateTo(new FavoriteSongsViewModel(_mainVM), NavigationItem.Album);
        }

        partial void OnSearchTextChanged(string value)
        {
            AlbumsView.Refresh();
            PlaylistsView.Refresh();
            ArtistsView.Refresh();
        }

        private bool FilterAlbum(object obj)
        {
            if (obj is not SelectableAlbumViewModel vm) return false;
            var album = vm.Album;
            if (string.IsNullOrWhiteSpace(SearchText)) return true;
            return (album.name ?? string.Empty).Contains(SearchText, StringComparison.CurrentCultureIgnoreCase);
        }

        private bool FilterPlaylist(object obj)
        {
            if (obj is not SelectableAlbumViewModel vm) return false;
            var playlist = vm.Album;
            if (string.IsNullOrWhiteSpace(SearchText)) return true;
            return (playlist.name ?? string.Empty).Contains(SearchText, StringComparison.CurrentCultureIgnoreCase);
        }

        private bool FilterArtist(object obj)
        {
            if (obj is not SelectableArtistViewModel vm) return false;
            var name = vm.Artist.name ?? string.Empty;

            if (string.IsNullOrWhiteSpace(SearchText)) return true;
            return name.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase);
        }

        [RelayCommand]
        private async Task LoadAsync()
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
                    MyPlaylists.Clear();
                    foreach (var p in playlists ?? []) MyPlaylists.Add(new SelectableAlbumViewModel(p));

                    MyAlbums.Clear();
                    foreach (var a in albums ?? []) MyAlbums.Add(new SelectableAlbumViewModel(a));

                    MyArtists.Clear();
                    foreach (var ar in artists ?? []) MyArtists.Add(new SelectableArtistViewModel(ar));

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
        private async Task CreatePlaylistAsync()
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
            SelectedAlbumId = album?.id;
            SelectedAlbumId = album?.id;
            SelectedNavigationItem = NavigationItem.Album;
            SelectedPlaylist = null;
            SelectedPlaylistId = null;
            SelectedArtist = null;
            SelectedArtistId = null;
            SelectedArtistWrapper = null;
            IsFavoriteSongsSelected = false;
            _mainVM.NavigateTo(new AlbumViewModel(_mainVM, album), NavigationItem.Album);
        }

        [RelayCommand]
        public void PlaylistClick(Album playlist)
        {
            SelectedPlaylistId = playlist?.id;
            SelectedNavigationItem = NavigationItem.Album;
            SelectedAlbum = null;
            SelectedAlbumId = null;
            SelectedArtist = null;
            SelectedArtistId = null;
            SelectedArtistWrapper = null;
            IsFavoriteSongsSelected = false;
            _mainVM.NavigateTo(new AlbumViewModel(_mainVM, playlist), NavigationItem.Album);
        }

        [RelayCommand]
        public void ArtistClick(User artist)
        {
            SelectedArtist = artist;
            SelectedArtistWrapper = artist;
            SelectedArtistId = artist?.id;
            SelectedNavigationItem = NavigationItem.Artist;
            SelectedAlbum = null;
            SelectedAlbumId = null;
            SelectedPlaylist = null;
            SelectedPlaylistId = null;
            IsFavoriteSongsSelected = false;
            _mainVM.NavigateTo(new ArtistViewModel(_mainVM, artist), NavigationItem.Artist);
        }

        public void SelectAlbumIfExists(Album? album)
        {
            if (album?.id is null) return;

            foreach (var a in MyAlbums)
            {
                if (a.Album?.id == album.id)
                {
                    SelectedAlbumId = a.Album.id;
                    SelectedNavigationItem = NavigationItem.Album;
                    SelectedPlaylist = null;
                    SelectedPlaylistId = null;
                    SelectedArtist = null;
                    SelectedArtistId = null;
                    SelectedArtistWrapper = null;
                    IsFavoriteSongsSelected = false;
                    return;
                }
            }

            foreach (var p in MyPlaylists)
            {
                if (p.Album?.id == album.id)
                {
                    SelectedPlaylistId = p.Album.id;
                    SelectedNavigationItem = NavigationItem.Album;
                    SelectedAlbum = null;
                    SelectedAlbumId = null;
                    SelectedArtist = null;
                    SelectedArtistId = null;
                    SelectedArtistWrapper = null;
                    IsFavoriteSongsSelected = false;
                    return;
                }
            }
        }

        public void SelectArtistIfExists(User? artist)
        {
            var targetId = artist?.id ?? artist?.artist?.id;
            if (targetId is null) return;

            foreach (var a in MyArtists)
            {
                var id = a.Artist.id;
                if (id == targetId)
                {
                    SelectedArtist = a.Artist;
                    SelectedArtistWrapper = a.Wrapper;
                    SelectedArtistId = targetId;
                    SelectedNavigationItem = NavigationItem.Artist;
                    SelectedAlbum = null;
                    SelectedAlbumId = null;
                    SelectedPlaylist = null;
                    SelectedPlaylistId = null;
                    IsFavoriteSongsSelected = false;
                    return;
                }
            }
        }

        private void ApplySelection(NavigationItem navigationItem, int? id)
        {
            SelectedNavigationItem = navigationItem;

            foreach (var a in MyAlbums) a.IsSelected = false;
            foreach (var p in MyPlaylists) p.IsSelected = false;
            foreach (var ar in MyArtists) ar.IsSelected = false;

            if (navigationItem == NavigationItem.Album && id is not null)
            {
                var album = MyAlbums.FirstOrDefault(x => x.Album.id == id);
                if (album is not null) album.IsSelected = true;

                var playlist = MyPlaylists.FirstOrDefault(x => x.Album.id == id);
                if (playlist is not null) playlist.IsSelected = true;
            }
            else if (navigationItem == NavigationItem.Artist && id is not null)
            {
                var artist = MyArtists.FirstOrDefault(x => x.Artist.id == id);
                if (artist is not null) artist.IsSelected = true;
            }
            else
            {
                IsFavoriteSongsSelected = false;
            }
        }

        [RelayCommand]
        private async Task DeletePlaylistAsync(Album playlist)
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
