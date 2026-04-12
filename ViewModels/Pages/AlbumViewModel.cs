using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
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

        public AlbumViewModel(MainViewModel mainVM, Album album)
        {
            _mainVM = mainVM;
            Album = album;

            _ = LoadTracksAsync();
        }

        private async Task LoadTracksAsync()
        {
            if (Album?.id == null)
                return;

            try
            {
                // Endpoint name inferred from other API usage; adjust if your backend differs.
                var list = await ApiManager.GetInstance()
                    .HttpGetAsync<List<Song>>($"playlist/show/{Album.id}")
                    .ConfigureAwait(false);

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
