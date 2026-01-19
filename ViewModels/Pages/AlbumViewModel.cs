using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using Vibra_DesktopApp.Models;

namespace Vibra_DesktopApp.ViewModels.Pages
{
    public partial class AlbumViewModel : ObservableObject
    {
        private readonly MainViewModel _mainVM;
        [ObservableProperty] private Album album;

        public AlbumViewModel(MainViewModel mainVM, Album album)
        {
            _mainVM = mainVM;
            Album = album;
        }
    }
}
