using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Vibra_DesktopApp.Singleton;

namespace Vibra_DesktopApp.ViewModels.Components
{
    public partial class PlayerViewModel : ObservableObject
    {
        private readonly MainViewModel _mainVM;

        public SongManager SongManager => SongManager.GetInstace();

        public PlayerViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
        }

        [RelayCommand]
        public void TogglePanel()
        {
            _mainVM.TogglePanel();
        }

        [RelayCommand]
        public void PlayOrPause()
        {
            SongManager.PlayOrPauseSong();
        }

    }
}
