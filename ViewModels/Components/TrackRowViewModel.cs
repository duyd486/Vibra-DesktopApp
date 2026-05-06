using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Vibra_DesktopApp.Models;
using Vibra_DesktopApp.Singleton;

namespace Vibra_DesktopApp.ViewModels.Components
{
    public partial class TrackRowViewModel : ObservableObject
    {
        private readonly SongManager _songManager;

        public Song Track { get; }
        public int Index { get; }

        [ObservableProperty] private string? durationText;

        public Song? CurrentTrack => _songManager.CurrentTrack;
        public bool IsPlaying => _songManager.IsPlaying;

        public TrackRowViewModel(Song track, int index, SongManager songManager)
        {
            Track = track ?? throw new ArgumentNullException(nameof(track));
            Index = index;
            _songManager = songManager ?? throw new ArgumentNullException(nameof(songManager));

            _songManager.PropertyChanged += OnSongManagerPropertyChanged;

            DurationText = "0:00";
            _ = LoadDurationAsync();
        }

        private void OnSongManagerPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SongManager.CurrentTrack) || e.PropertyName == nameof(SongManager.IsPlaying))
            {
                OnPropertyChanged(nameof(CurrentTrack));
                OnPropertyChanged(nameof(IsPlaying));
                OnPropertyChanged(nameof(IsCurrentTrack));
            }
        }

        public bool IsCurrentTrack => CurrentTrack?.id != null && Track?.id != null && CurrentTrack.id == Track.id;

        public string TotalPlayedText => $"{(Track.total_played ?? 0):N0} lượt nghe";

        [RelayCommand]
        private async Task PlayOrPauseAsync()
        {
            await _songManager.PlayOrPauseThisSongAsync(Track).ConfigureAwait(false);
        }

        [RelayCommand]
        private void AddToWaitlist()
        {
            _songManager.Enqueue(Track);
        }

        private async Task LoadDurationAsync()
        {
            if (string.IsNullOrWhiteSpace(Track.song_path))
                return;

            try
            {
                var tcs = new TaskCompletionSource<TimeSpan>(TaskCreationOptions.RunContinuationsAsynchronously);
                MediaPlayer? player = null;
                Action? cleanup = null;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    player = new MediaPlayer();

                    void CleanupInternal()
                    {
                        try
                        {
                            player.MediaOpened -= OnOpened;
                            player.MediaFailed -= OnFailed;
                            player.Close();
                        }
                        catch
                        {
                            // ignore
                        }
                    }

                    cleanup = CleanupInternal;

                    void OnOpened(object? s, EventArgs e)
                    {
                        try
                        {
                            tcs.TrySetResult(player.NaturalDuration.HasTimeSpan
                                ? player.NaturalDuration.TimeSpan
                                : TimeSpan.Zero);
                        }
                        finally
                        {
                            CleanupInternal();
                        }
                    }

                    void OnFailed(object? s, ExceptionEventArgs e)
                    {
                        try
                        {
                            tcs.TrySetResult(TimeSpan.Zero);
                        }
                        finally
                        {
                            CleanupInternal();
                        }
                    }

                    player.MediaOpened += OnOpened;
                    player.MediaFailed += OnFailed;
                    player.Open(new Uri(Track.song_path, UriKind.RelativeOrAbsolute));
                });

                var completed = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(5))).ConfigureAwait(false);

                TimeSpan duration;
                if (completed == tcs.Task)
                {
                    duration = await tcs.Task.ConfigureAwait(false);
                }
                else
                {
                    duration = TimeSpan.Zero;
                }

                if (completed != tcs.Task)
                {
                    try
                    {
                        Application.Current.Dispatcher.Invoke(() => cleanup?.Invoke());
                    }
                    catch
                    {
                        // ignore
                    }
                }

                var text = $"{(int)duration.TotalMinutes}:{duration.Seconds:00}";

                Application.Current.Dispatcher.Invoke(() => DurationText = text);
            }
            catch
            {
                // ignore
            }
        }
    }
}
