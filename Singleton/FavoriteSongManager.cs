using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Vibra_DesktopApp.Models;

namespace Vibra_DesktopApp.Singleton
{
    public partial class FavoriteSongManager : ObservableObject
    {
        public static FavoriteSongManager? Instance { get; private set; }

        public static FavoriteSongManager GetInstance()
        {
            Instance ??= new FavoriteSongManager();
            return Instance;
        }

        public ObservableCollection<Song> Songs { get; } = new();

        private readonly HashSet<int> _songIdSet = new();

        [ObservableProperty] private bool _isLoaded;

        private FavoriteSongManager() { }

        public bool IsFavorite(Song? song)
        {
            if (song?.id is null) return false;
            lock (_songIdSet)
            {
                return _songIdSet.Contains(song.id.Value);
            }
        }

        public async Task<bool> LoveAsync(Song song, CancellationToken cancellationToken = default)
        {
            if (song?.id is null) return false;
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await ApiManager.GetInstance()
                    .HttpGetNoDataAsync($"song/store/{song.id}")
                    .ConfigureAwait(false);

                AddLocal(song);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UnloveAsync(Song song, CancellationToken cancellationToken = default)
        {
            if (song?.id is null) return false;
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await ApiManager.GetInstance()
                    .HttpGetNoDataAsync($"library/destroy-favorite-song/{song.id}")
                    .ConfigureAwait(false);

                RemoveLocal(song);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void AddLocal(Song song)
        {
            if (song.id is null) return;

            var added = false;
            lock (_songIdSet)
            {
                added = _songIdSet.Add(song.id.Value);
            }

            if (!added) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!Songs.Any(s => s.id == song.id))
                    Songs.Insert(0, song);
            });
        }

        private void RemoveLocal(Song song)
        {
            if (song.id is null) return;

            lock (_songIdSet)
            {
                _songIdSet.Remove(song.id.Value);
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                var existing = Songs.FirstOrDefault(s => s.id == song.id);
                if (existing is not null)
                    Songs.Remove(existing);
            });
        }

        public void SetFavorites(IEnumerable<Song>? songs)
        {
            var list = (songs ?? Enumerable.Empty<Song>())
                .Where(s => s.id is not null)
                .GroupBy(s => s.id!.Value)
                .Select(g => g.First())
                .ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                Songs.Clear();
                foreach (var s in list) Songs.Add(s);
            });

            lock (_songIdSet)
            {
                _songIdSet.Clear();
                foreach (var s in list)
                {
                    if (s.id is not null) _songIdSet.Add(s.id.Value);
                }
            }

            IsLoaded = true;
        }

        public async Task LoadAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // API returns: { code, data: [ { song: {..} }, ... ] }
                var raw = await ApiManager.GetInstance()
                    .HttpGetAsync<JsonElement>("library/list-song")
                    .ConfigureAwait(false);

                var songs = new List<Song>();

                if (raw.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in raw.EnumerateArray())
                    {
                        if (item.ValueKind != JsonValueKind.Object) continue;
                        if (!item.TryGetProperty("song", out var songEl)) continue;

                        try
                        {
                            var s = songEl.Deserialize<Song>();
                            if (s is not null) songs.Add(s);
                        }
                        catch
                        {
                        }
                    }
                }

                SetFavorites(songs);
            }
            catch
            {
                // keep UI responsive; ApiManager already shows errors
            }
        }
    }
}
