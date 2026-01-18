using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Vibra_DesktopApp.Models;
using Vibra_DesktopApp.Views.Components;

namespace Vibra_DesktopApp.Singleton
{
    public partial class SongManager : ObservableObject
    {
        public static SongManager? Instance { get; private set; }

        private readonly MediaPlayer mediaPlayer = new();

        private readonly DispatcherTimer _timer;

        [ObservableProperty] public Song? currentTrack;

        [ObservableProperty] public bool isPlaying;

        [ObservableProperty] public double duration;

        [ObservableProperty] public double currentTime;

        public SongManager()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) =>
            {
                if (IsPlaying)
                {
                    // You can add code here to update UI components with the current playback position
                    CurrentTime = mediaPlayer.Position.TotalSeconds;
                }
            };
        }


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
        public void Play(string url)
        {
            mediaPlayer.Open(new Uri(url));
            mediaPlayer.MediaOpened += (s, e) =>
            {
                Duration = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                _timer.Start();
            };
            mediaPlayer.Play();
            IsPlaying = true;
        }

        public void Play()
        {
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
