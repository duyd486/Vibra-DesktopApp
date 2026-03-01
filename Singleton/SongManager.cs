using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Vibra_DesktopApp.Models;

namespace Vibra_DesktopApp.Singleton
{
    public partial class SongManager : ObservableObject, IAsyncDisposable
    {
        // Backing fields will generate public properties: CurrentTrack, IsPlaying, Duration, CurrentTime
        [ObservableProperty] private Song? _currentTrack;
        [ObservableProperty] private bool _isPlaying;
        [ObservableProperty] private double _duration;
        [ObservableProperty] private double _currentTime;

        // Volume (0.0 - 1.0). Changes are applied to the MediaPlayer on the UI dispatcher.
        [ObservableProperty] private double _volume = 1.0;

        // NOTE: kept existing factory name to avoid breaking callers. Prefer DI registration instead.
        public static SongManager? Instance { get; private set; }

        private readonly MediaPlayer _mediaPlayer = new();
        private readonly Dispatcher _dispatcher;
        private readonly DispatcherTimer _timer;

        // Pending seek values used if a seek request happens before media has opened
        private TimeSpan? _pendingSeek;
        private double? _pendingSeekPercentage;

        public SongManager()
        {
            _dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;

            // apply initial volume to media player
            try
            {
                _mediaPlayer.Volume = Math.Clamp(_volume, 0.0, 1.0);
            }
            catch
            {
                // ignore; will be applied once dispatcher is available
            }

            // Timer updates CurrentTime on the UI dispatcher
            _timer = new DispatcherTimer(DispatcherPriority.Normal, _dispatcher)
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (s, e) =>
            {
                if (IsPlaying)
                {
                    try
                    {
                        CurrentTime = _mediaPlayer.Position.TotalSeconds;
                    }
                    catch
                    {
                        // swallow - Position may throw if media not opened
                    }
                }
            };

            // Attach media events once to avoid multiple subscriptions
            _mediaPlayer.MediaOpened += OnMediaOpened;
            _mediaPlayer.MediaEnded += OnMediaEnded;
        }

        partial void OnVolumeChanged(double value)
        {
            var v = Math.Clamp(value, 0.0, 1.0);

            // Marshal to UI dispatcher to respect MediaPlayer affinity
            _dispatcher?.InvokeAsync(() =>
            {
                try
                {
                    _mediaPlayer.Volume = v;
                }
                catch
                {
                    // swallow, do not throw from property change
                }
            }, DispatcherPriority.Normal);
        }

        public static SongManager GetInstace()
        {
            if (Instance == null)
            {
                Instance = new SongManager();
            }
            return Instance;
        }

        private void OnMediaOpened(object? sender, EventArgs e)
        {
            if (_mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                Duration = _mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            }
            else
            {
                Duration = 0;
            }

            // If there was a pending seek by absolute time, apply it.
            if (_pendingSeek.HasValue)
            {
                try
                {
                    var target = _pendingSeek.Value;
                    if (Duration > 0 && target.TotalSeconds > Duration)
                    {
                        target = TimeSpan.FromSeconds(Duration);
                    }

                    _mediaPlayer.Position = target;
                    CurrentTime = target.TotalSeconds;
                }
                catch
                {
                    // ignore failing seek
                }
                finally
                {
                    _pendingSeek = null;
                }
            }
            // If there was a pending seek by percentage, compute and apply it.
            else if (_pendingSeekPercentage.HasValue && Duration > 0)
            {
                try
                {
                    var seconds = Duration * Math.Clamp(_pendingSeekPercentage.Value, 0.0, 1.0);
                    var target = TimeSpan.FromSeconds(seconds);
                    _mediaPlayer.Position = target;
                    CurrentTime = seconds;
                }
                catch
                {
                    // ignore failing seek
                }
                finally
                {
                    _pendingSeekPercentage = null;
                }
            }

            _timer.Start();
        }

        private void OnMediaEnded(object? sender, EventArgs e)
        {
            _timer.Stop();
            IsPlaying = false;
            CurrentTime = 0;
            // Optionally reset CurrentTrack or leave it as-is for replay
        }

        // Play or toggle for a Song
        public async Task PlayOrPauseThisSongAsync(Song song, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (CurrentTrack != null && CurrentTrack.id == song.id)
            {
                await PlayOrPauseAsync(cancellationToken).ConfigureAwait(false);
                return;
            }

            CurrentTrack = song;
            await PlayAsync(song.song_path ?? throw new InvalidOperationException("Song path is null"), cancellationToken).ConfigureAwait(false);
        }

        // Play or pause current track
        public async Task PlayOrPauseAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (CurrentTrack == null)
            {
                return;
            }

            if (IsPlaying)
            {
                await PauseAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await PlayAsync().ConfigureAwait(false);
            }
        }

        // Open and play by URL
        public async Task PlayAsync(string url, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var uri = new Uri(url, UriKind.RelativeOrAbsolute);

            var op = _dispatcher.InvokeAsync(() =>
            {
                _mediaPlayer.Open(uri);
                _mediaPlayer.Play();
            }, DispatcherPriority.Normal);

            await op.Task.ConfigureAwait(false);

            IsPlaying = true;
        }

        // Play current media (must be opened)
        public async Task PlayAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var op = _dispatcher.InvokeAsync(() => _mediaPlayer.Play(), DispatcherPriority.Normal);
            await op.Task.ConfigureAwait(false);

            IsPlaying = true;
        }

        // Pause playback
        public async Task PauseAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var op = _dispatcher.InvokeAsync(() => _mediaPlayer.Pause(), DispatcherPriority.Normal);
            await op.Task.ConfigureAwait(false);

            IsPlaying = false;
        }

        /// <summary>
        /// Seek to an absolute time (in seconds) in the currently loaded track.
        /// If the media has not finished opening, the seek will be queued and applied once opened.
        /// </summary>
        public async Task SeekAsync(double seconds, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (double.IsNaN(seconds) || double.IsInfinity(seconds))
            {
                throw new ArgumentOutOfRangeException(nameof(seconds));
            }

            var targetSeconds = Math.Max(0.0, seconds);

            // If media duration is known, clamp to duration
            if (Duration > 0)
            {
                targetSeconds = Math.Min(targetSeconds, Duration);
            }

            var target = TimeSpan.FromSeconds(targetSeconds);

            // If media not opened (duration unknown), store pending seek and return.
            if (!_mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                _pendingSeek = target;
                _pendingSeekPercentage = null;
                return;
            }

            var op = _dispatcher.InvokeAsync(() =>
            {
                _mediaPlayer.Position = target;
                CurrentTime = target.TotalSeconds;
            }, DispatcherPriority.Normal);

            await op.Task.ConfigureAwait(false);
        }

        /// <summary>
        /// Seek by percentage (0.0 - 1.0) of the track's duration.
        /// If duration is unknown yet, percentage will be queued and applied when media opens.
        /// </summary>
        public async Task SeekToPercentageAsync(double percentage, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            percentage = Math.Clamp(percentage, 0.0, 1.0);

            // If duration is known, compute and seek immediately.
            if (Duration > 0)
            {
                var seconds = Duration * percentage;
                await SeekAsync(seconds, cancellationToken).ConfigureAwait(false);
                return;
            }

            // Otherwise store pending percentage to apply when media opens.
            _pendingSeekPercentage = percentage;
            _pendingSeek = null;
        }

        public Song? GetCurrentTrack() => CurrentTrack;

        public async ValueTask DisposeAsync()
        {
            _timer.Stop();

            var op = _dispatcher.InvokeAsync(() =>
            {
                _mediaPlayer.Close();
            }, DispatcherPriority.Normal);

            await op.Task.ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }
    }
}
