using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Vibra_DesktopApp.Models;

namespace Vibra_DesktopApp.Singleton
{
    class SongManager
    {
        public static SongManager? Instance { get; private set; }
        public static SongManager GetInstace()
        {
            if (Instance == null)
            {
                Instance = new SongManager();
            }
            return Instance;
        }

        private Song? currentTrack;
        
        public void PlayOrPauseThisSong(Song song)
        {
            MessageBox.Show(song.song_path);
        }

    }
}
