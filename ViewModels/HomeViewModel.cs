using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using Vibra_DesktopApp.Models;
using Vibra_DesktopApp.Singleton;

namespace Vibra_DesktopApp.ViewModels
{
    partial class HomeViewModel : ObservableObject
    {
        [ObservableProperty] private List<Song>? listSong;

        public HomeViewModel()
        {
            RefreshListSong();
        }

        public async void RefreshListSong()
        {
            ListSong = await ApiManager.GetInstance().GetListSong();
        }


        [RelayCommand]
        public void PlayOrPauseThisSong(Song song)
        {
            SongManager.GetInstace().PlayOrPauseThisSong(song);
        }
    }
}
