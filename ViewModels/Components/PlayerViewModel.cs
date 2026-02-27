using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
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
        public async Task PlayOrPause()
        {
            await SongManager.PlayOrPauseAsync().ConfigureAwait(false);
        }

        // Seek to an absolute time (seconds)
        [RelayCommand]
        public async Task Seek(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds))
                return;

            // clamp to valid range
            var target = Math.Max(0.0, seconds);
            if (SongManager.Duration > 0)
                target = Math.Min(target, SongManager.Duration);

            await SongManager.SeekAsync(target).ConfigureAwait(false);
        }

        // Seek by percentage (0.0 - 1.0) of the track duration
        [RelayCommand]
        public async Task SeekToPercentage(double percentage)
        {
            // normalize and clamp
            percentage = Math.Clamp(percentage, 0.0, 1.0);

            await SongManager.SeekToPercentageAsync(percentage).ConfigureAwait(false);
        }
    }
}
