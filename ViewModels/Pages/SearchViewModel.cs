using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vibra_DesktopApp.Models;
using Vibra_DesktopApp.Singleton;

namespace Vibra_DesktopApp.ViewModels.Pages
{
    public partial class SearchViewModel : ObservableObject
    {
        private readonly MainViewModel _mainVM;

        private CancellationTokenSource? _debounceCts;

        [ObservableProperty] private string _searchText = string.Empty;
        [ObservableProperty] private bool _isLoading;

        [ObservableProperty] private List<Song> _songs = [];
        [ObservableProperty] private List<Album> _albums = [];
        [ObservableProperty] private List<User> _artists = [];

        [ObservableProperty] private Song? _topSong;

        public SearchViewModel(MainViewModel mainVM, string searchText)
        {
            _mainVM = mainVM ?? throw new ArgumentNullException(nameof(mainVM));

            var initialText = searchText ?? string.Empty;

            // keep results up-to-date while user types in the header search box
            if (_mainVM.HeaderVM is INotifyPropertyChanged)
            {
                PropertyChangedEventManager.AddHandler(
                    _mainVM.HeaderVM,
                    OnHeaderPropertyChanged,
                    nameof(_mainVM.HeaderVM.SearchText));

                // if Header has a newer value than the passed argument (e.g. focus), prefer header
                var headerText = _mainVM.HeaderVM.SearchText ?? string.Empty;
                if (!string.Equals(headerText, initialText, StringComparison.Ordinal))
                    initialText = headerText;
            }

            _searchText = initialText;
            OnPropertyChanged(nameof(SearchText));

            _ = FetchSearchDataAsync();
        }

        private void OnHeaderPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(_mainVM.HeaderVM.SearchText))
                return;

            var txt = _mainVM.HeaderVM.SearchText ?? string.Empty;
            if (!string.Equals(txt, SearchText, StringComparison.Ordinal))
                SearchText = txt;
        }

        partial void OnSearchTextChanged(string value)
        {
            _ = DebounceFetchAsync(value);
        }

        partial void OnSongsChanged(List<Song> value)
        {
            TopSong = value?.FirstOrDefault();
        }

        private async Task DebounceFetchAsync(string value)
        {
            _debounceCts?.Cancel();
            _debounceCts?.Dispose();
            _debounceCts = new CancellationTokenSource();
            var ct = _debounceCts.Token;

            try
            {
                await Task.Delay(300, ct).ConfigureAwait(false);
                await FetchSearchDataAsync(ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
        }

        private async Task FetchSearchDataAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var key = SearchText?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(key))
            {
                Songs = [];
                Albums = [];
                Artists = [];
                IsLoading = false;
                return;
            }

            IsLoading = true;
            try
            {
                var url = $"home/search?search-key={Uri.EscapeDataString(key)}";
                var result = await ApiManager.GetInstance()
                    .HttpGetAsync<SearchResult>(url)
                    .ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                Songs = result?.songs ?? [];
                Albums = result?.albums ?? [];
                Artists = result?.artists ?? [];
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task PlayTopSongAsync()
        {
            if (TopSong is null)
                return;

            await SongManager.GetInstace().PlayOrPauseThisSongAsync(TopSong).ConfigureAwait(false);
        }

        [RelayCommand]
        private async Task PlaySongAsync(Song song)
        {
            if (song is null)
                return;

            await SongManager.GetInstace().PlayOrPauseThisSongAsync(song).ConfigureAwait(false);
        }

        [RelayCommand]
        private void AddToWaitlist(Song song)
        {
            if (song is null)
                return;

            SongManager.GetInstace().Enqueue(song);
        }

        [RelayCommand]
        private void OpenAlbumDetail(Album album)
        {
            if (album is null)
                return;

            _mainVM.NavigateTo(new AlbumViewModel(_mainVM, album));
        }

        [RelayCommand]
        private void OpenArtistDetail(User artist)
        {
            if (artist is null)
                return;

            _mainVM.NavigateTo(new ArtistViewModel(_mainVM, artist));
        }
    }
}
