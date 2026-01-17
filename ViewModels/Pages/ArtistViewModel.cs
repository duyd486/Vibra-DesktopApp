using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using Vibra_DesktopApp.Models;

namespace Vibra_DesktopApp.ViewModels.Pages
{
    public partial class ArtistViewModel : ObservableObject
    {
        private readonly MainViewModel _mainVM;
        private User artist;

        public ArtistViewModel(MainViewModel mainVM, User artist)
        {
            _mainVM = mainVM;
            this.artist = artist;
        }
    }
}
