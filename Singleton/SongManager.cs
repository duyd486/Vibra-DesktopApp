using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Vibra_DesktopApp.Models;
using Vibra_DesktopApp.Views.Components;

namespace Vibra_DesktopApp.Singleton
{
    public partial class SongManager : ObservableObject
    {
        public static SongManager? Instance { get; private set; }

        private readonly MediaPlayer mediaPlayer = new();

        [ObservableProperty] public Song? currentTrack;

        [ObservableProperty] public bool isPlaying;

        public static SongManager GetInstace()
        {
            if (Instance == null)
            {
                Instance = new SongManager();
            }
            return Instance;
        }


        public void PlayOrPauseThisSong(Song song)
        {
            if (CurrentTrack != null && CurrentTrack.id == song.id)
            {
                if (IsPlaying)
                {
                    Pause();
                }
                else
                {
                    Play();
                }
                return;
            }
            else
            {
                CurrentTrack = song;
                Play(song.song_path);
            }
        }

        public void PlayOrPauseSong()
        {
            if (CurrentTrack == null)
            {
                return;
            }
            else
            {
                if (IsPlaying)
                {
                    Pause();
                }
                else
                {
                    Play();
                }

            }
        }

        public void Play()
        {
            mediaPlayer.Play();
            IsPlaying = true;
        }

        public void Play(string url)
        {
            mediaPlayer.Open(new Uri(url));
            mediaPlayer.Play();
            IsPlaying = true;
        }

        public void Pause()
        {
            mediaPlayer.Pause();
            IsPlaying = false;
        }

    }
}
