using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Vibra_DesktopApp.Singleton;

namespace Vibra_DesktopApp.ViewModels.Components
{
    public partial class PlayerViewModel : ObservableObject
    {
        private readonly MainViewModel _mainVM;

        public SongManager SongManager => SongManager.GetInstace();

        // Tracks whether playback was active when the user started seeking
        private bool _wasPlaying;

        // Bound to the Slider.Value. While dragging we modify this and only apply to the player on seek complete.
        [ObservableProperty] private double _sliderValue;

        // True while user is dragging the slider thumb
        [ObservableProperty] private bool _isSeeking;

        // Volume (0.0 - 1.0) bound to the UI
        [ObservableProperty] private double _volume;

        public PlayerViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;

            // initialize slider and volume from player
            SliderValue = SongManager.CurrentTime;
            Volume = Math.Clamp(SongManager.Volume, 0.0, 1.0);

            // keep SliderValue in sync with SongManager.CurrentTime when not seeking
            SongManager.PropertyChanged += OnSongManagerPropertyChanged;
        }

        partial void OnVolumeChanged(double value)
        {
            var v = Math.Clamp(value, 0.0, 1.0);
            // Apply volume to the SongManager (which will marshal to the UI dispatcher)
            SongManager.Volume = v;
        }

        private void OnSongManagerPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SongManager.CurrentTime) && !IsSeeking)
            {
                // update the slider to reflect current playback position
                SliderValue = SongManager.CurrentTime;
            }

            if (e.PropertyName == nameof(SongManager.Duration))
            {
                // clamp slider if duration changed and current value is out of range
                if (SongManager.Duration > 0 && SliderValue > SongManager.Duration)
                {
                    SliderValue = SongManager.Duration;
                }
            }

            if (e.PropertyName == nameof(SongManager.Volume))
            {
                // keep viewmodel volume in sync if volume changed elsewhere
                var v = Math.Clamp(SongManager.Volume, 0.0, 1.0);
                if (Math.Abs(Volume - v) > 0.0001)
                {
                    Volume = v;
                }
            }
        }

        [RelayCommand]
        public void TogglePanel()
        {
            _mainVM.TogglePanel();
        }

        [RelayCommand]
        public async Task PlayOrPause()
        {
            await SongManager.PlayOrPauseAsync().ConfigureAwait(false);
        }

        // Called when the user begins dragging the slider thumb
        [RelayCommand]
        public async Task SeekStarted()
        {
            // mark seeking and remember play state
            IsSeeking = true;
            _wasPlaying = SongManager.IsPlaying;

            if (_wasPlaying)
            {
                await SongManager.PauseAsync().ConfigureAwait(false);
            }
        }

        // Called when the user finishes dragging the slider thumb.
        // Uses the bound SliderValue property (two-way) rather than a parameter.
        [RelayCommand]
        public async Task SeekCompleted()
        {
            // ensure valid value
            if (double.IsNaN(SliderValue) || double.IsInfinity(SliderValue))
            {
                IsSeeking = false;
                return;
            }

            var target = Math.Max(0.0, SliderValue);
            if (SongManager.Duration > 0)
                target = Math.Min(target, SongManager.Duration);

            // perform seek while paused
            await SongManager.SeekAsync(target).ConfigureAwait(false);

            // resume if it was playing before seek started
            if (_wasPlaying)
            {
                await SongManager.PlayAsync().ConfigureAwait(false);
                _wasPlaying = false;
            }

            IsSeeking = false;
        }

        // Fallback seek command (immediate seek) — kept for existing bindings if needed.
        [RelayCommand]
        public async Task Seek(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds))
                return;

            var target = Math.Max(0.0, seconds);
            if (SongManager.Duration > 0)
                target = Math.Min(target, SongManager.Duration);

            await SongManager.SeekAsync(target).ConfigureAwait(false);
        }

        // Seek by percentage (0.0 - 1.0) of the track duration
        [RelayCommand]
        public async Task SeekToPercentage(double percentage)
        {
            percentage = Math.Clamp(percentage, 0.0, 1.0);
            await SongManager.SeekToPercentageAsync(percentage).ConfigureAwait(false);
        }
    }
}
