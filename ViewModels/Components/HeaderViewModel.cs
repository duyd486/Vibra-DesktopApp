using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Vibra_DesktopApp.ViewModels.Pages;

namespace Vibra_DesktopApp.ViewModels.Components
{
    public partial class HeaderViewModel : ObservableObject
    {
        private readonly MainViewModel _mainVM;

        [ObservableProperty] public string searchText;

        public HeaderViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;

            SearchText = "Search...";
        }


        [RelayCommand]
        public void UserClick()
        {
            _mainVM.NavigateTo(new UserViewModel(_mainVM));
        }

        [RelayCommand]
        private void SearchBoxFocused()
        {
            // Khi user click vào search box
            SearchText = "";
            _mainVM.NavigateTo(new SearchViewModel(_mainVM, ""));
        }

        [RelayCommand]
        private void Search()
        {
            // Thực hiện tìm kiếm
            _mainVM.NavigateTo(new SearchViewModel(_mainVM, SearchText));
        }
        [RelayCommand]
        private void Home()
        {
            _mainVM.NavigateTo(new HomeViewModel(_mainVM));
        }
    }
}
