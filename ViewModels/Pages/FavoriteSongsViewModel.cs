using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Vibra_DesktopApp.Models;
using Vibra_DesktopApp.Singleton;
using Vibra_DesktopApp.ViewModels.Components;

namespace Vibra_DesktopApp.ViewModels.Pages
{
    public partial class FavoriteSongsViewModel : ObservableObject
    {
        private readonly MainViewModel _mainVM;

        [ObservableProperty] private Album album;

        [ObservableProperty] private ObservableCollection<TrackRowViewModel> tracks = new();

        private readonly FavoriteSongManager _favoriteSongManager;

        public FavoriteSongsViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM ?? throw new ArgumentNullException(nameof(mainVM));
            _favoriteSongManager = FavoriteSongManager.GetInstance();

            Album = BuildFavoriteAlbumShell();

            _favoriteSongManager.Songs.CollectionChanged += (_, __) => _ = ReloadTracksFromSingletonAsync();

            _ = EnsureLoadedAndReloadAsync();
        }

        private static Album BuildFavoriteAlbumShell()
        {
            var user = ApiManager.GetInstance().GetCurrentUser();
            return new Album
            {
                id = null,
                name = "Bài hát yêu thích",
                total_song = 0,
                thumbnail_path = string.Empty,
                author = user is null ? null : new User { name = user.name },
                created_at = DateTime.Now,
                price = 0,
                type = 0,
            };
        }

        private async Task EnsureLoadedAndReloadAsync()
        {
            if (!_favoriteSongManager.IsLoaded)
            {
                await _favoriteSongManager.LoadAsync().ConfigureAwait(false);
            }

            await ReloadTracksFromSingletonAsync().ConfigureAwait(false);
        }

        private Task ReloadTracksFromSingletonAsync()
        {
            try
            {
                var songManager = SongManager.GetInstace();
                var list = _favoriteSongManager.Songs.ToList();
                var vms = new ObservableCollection<TrackRowViewModel>();
                var i = 1;
                foreach (var s in list)
                {
                    vms.Add(new TrackRowViewModel(s, i++, songManager));
                }

                Tracks = vms;
                Album.total_song = list.Count;

                return Task.CompletedTask;
            }
            catch
            {
                Tracks = new ObservableCollection<TrackRowViewModel>();
                return Task.CompletedTask;
            }
        }

        [RelayCommand]
        private void AddPlaylistToQueue()
        {
            foreach (var t in Tracks)
            {
                SongManager.GetInstace().Enqueue(t.Track);
            }
        }
    }
}
