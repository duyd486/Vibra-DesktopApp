using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vibra_DesktopApp.Models;
using Vibra_DesktopApp.Singleton;

namespace Vibra_DesktopApp.ViewModels.Pages
{
    public partial class CategoriesViewModel : ObservableObject
    {
        private readonly MainViewModel _mainVM;

        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private List<Category> _categories = [];

        public CategoriesViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM ?? throw new ArgumentNullException(nameof(mainVM));
            _ = LoadAsync();
        }

        [RelayCommand]
        private async Task LoadAsync()
        {
            IsLoading = true;
            try
            {
                // Endpoint inferred from Vue; adjust if your backend differs.
                var list = await ApiManager.GetInstance()
                    .HttpGetAsync<List<Category>>("category/index")
                    .ConfigureAwait(false);

                Categories = list ?? [];
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void OpenCategory(Category category)
        {
            if (category is null)
                return;

            _mainVM.NavigateTo(new CategorySongsViewModel(_mainVM, category), NavigationItem.Categories);
        }
    }
}
