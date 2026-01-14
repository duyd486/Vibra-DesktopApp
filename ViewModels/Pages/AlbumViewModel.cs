using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vibra_DesktopApp.ViewModels.Pages
{
    public partial class AlbumViewModel : ObservableObject
    {
        private readonly MainViewModel _mainVM;

        public AlbumViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
        }
    }
}
