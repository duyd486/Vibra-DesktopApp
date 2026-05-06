using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Vibra_DesktopApp.Singleton;
using Vibra_DesktopApp.ViewModels.Components;
using Vibra_DesktopApp.Models;

namespace Vibra_DesktopApp.ViewModels.Pages
{
    public partial class AlbumViewModel : ObservableObject
    {
        private readonly MainViewModel _mainVM;
        [ObservableProperty] private Album album;

        [ObservableProperty] private ObservableCollection<TrackRowViewModel> tracks = new();

        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private bool isFollowed;

        public string HeaderTypeText => Album?.type == 1 ? "Album" : "Danh sách phát";
        public bool ShowFollowButton => Album?.type == 1 && Album?.id is not null;
        public bool ShowDownloadButton => Album?.type == 1 && Album?.id is not null;
        public bool ShowPrice => Album?.type == 1;

        public AlbumViewModel(MainViewModel mainVM, Album album)
        {
            _mainVM = mainVM;
            Album = album;

            SidebarSelectionBus.GetInstance().Publish(NavigationItem.Album, album?.id);

            RefreshFollowState();
            _ = LoadTracksAsync();
        }

        partial void OnAlbumChanged(Album value)
        {
            OnPropertyChanged(nameof(HeaderTypeText));
            OnPropertyChanged(nameof(ShowFollowButton));
            OnPropertyChanged(nameof(ShowDownloadButton));
            OnPropertyChanged(nameof(ShowPrice));
            RefreshFollowState();
            _ = LoadTracksAsync();
        }

        private void RefreshFollowState()
        {
            try
            {
                if (Album?.id is null || Album.type != 1)
                {
                    IsFollowed = false;
                    return;
                }

                IsFollowed = _mainVM.SidebarVM.MyAlbums.Any(x => x?.Album?.id == Album.id);
            }
            catch
            {
                IsFollowed = false;
            }
        }

        private async Task LoadTracksAsync()
        {
            if (Album?.id == null)
                return;

            IsLoading = true;
            try
            {
                List<Song> list;

                if (Album.type == 1)
                {
                    list = await ApiManager.GetInstance()
                        .HttpGetAsync<List<Song>>($"playlist/show/{Album.id}")
                        .ConfigureAwait(false);
                }
                else
                {
                    // API returns: { code, data: [ { song: {..} }, ... ] }
                    var raw = await ApiManager.GetInstance()
                        .HttpGetAsync<JsonElement>($"library/list-playlist-song/{Album.id}")
                        .ConfigureAwait(false);

                    list = new List<Song>();
                    if (raw.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in raw.EnumerateArray())
                        {
                            if (item.ValueKind != JsonValueKind.Object) continue;
                            if (!item.TryGetProperty("song", out var songEl)) continue;

                            try
                            {
                                var s = songEl.Deserialize<Song>();
                                if (s is not null) list.Add(s);
                            }
                            catch
                            {
                            }
                        }
                    }

                    Album.total_song = list.Count;
                    OnPropertyChanged(nameof(Album));
                }

                var songManager = SongManager.GetInstace();
                var vms = new ObservableCollection<TrackRowViewModel>();
                var i = 1;
                foreach (var s in list ?? [])
                {
                    vms.Add(new TrackRowViewModel(s, i++, songManager));
                }

                Tracks = vms;
            }
            catch
            {
                Tracks = new ObservableCollection<TrackRowViewModel>();
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ToggleFollowAsync()
        {
            if (!ShowFollowButton || Album?.id is null)
                return;

            try
            {
                if (IsFollowed)
                {
                    await ApiManager.GetInstance()
                        .HttpGetNoDataAsync($"library/destroy-playlist/{Album.id}")
                        .ConfigureAwait(false);
                    IsFollowed = false;
                }
                else
                {
                    await ApiManager.GetInstance()
                        .HttpGetNoDataAsync($"home/store/{Album.id}")
                        .ConfigureAwait(false);
                    IsFollowed = true;
                }

                await _mainVM.SidebarVM.RefreshAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show(ex.Message));
            }
        }

        [RelayCommand]
        private async Task DownloadPlaylistAsync()
        {
            if (!ShowDownloadButton || Album?.id is null)
                return;

            try
            {
                var raw = await ApiManager.GetInstance()
                    .HttpGetAsync<JsonElement>($"payment/create-bill?playlist_id={Album.id}")
                    .ConfigureAwait(false);

                if (raw.ValueKind == JsonValueKind.Object && raw.TryGetProperty("checkout_url", out var urlEl))
                {
                    var url = urlEl.GetString();
                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                        return;
                    }
                }

                Application.Current.Dispatcher.Invoke(() => MessageBox.Show("Không lấy được link thanh toán!"));
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show(ex.Message));
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
