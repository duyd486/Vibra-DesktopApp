using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vibra_DesktopApp.Models;
using Vibra_DesktopApp.Singleton;

namespace Vibra_DesktopApp.ViewModels.Pages
{
    public partial class CategorySongsViewModel : ObservableObject
    {
        private readonly MainViewModel _mainVM;

        [ObservableProperty] private Category _category;
        [ObservableProperty] private bool _isLoading;

        [ObservableProperty] private List<Song> _songs = [];
        [ObservableProperty] private List<Song> _topSongs = [];

        public CategorySongsViewModel(MainViewModel mainVM, Category category)
        {
            _mainVM = mainVM ?? throw new ArgumentNullException(nameof(mainVM));
            Category = category ?? throw new ArgumentNullException(nameof(category));

            _ = LoadAsync();
        }

        partial void OnSongsChanged(List<Song> value)
        {
            TopSongs = (value ?? []).Take(4).ToList();
        }

        [RelayCommand]
        private void Back()
        {
            _mainVM.NavigateTo(new CategoriesViewModel(_mainVM), NavigationItem.Categories);
        }

        [RelayCommand]
        private async Task LoadAsync()
        {
            if (Category?.id is null)
                return;

            IsLoading = true;
            try
            {
                var list = await ApiManager.GetInstance()
                    .HttpGetAsync<List<Song>>($"category/show/{Category.id}")
                    .ConfigureAwait(false);

                Songs = list ?? [];
            }
            finally
            {
                IsLoading = false;
            }
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
    }
}
