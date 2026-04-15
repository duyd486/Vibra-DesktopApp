using CommunityToolkit.Mvvm.ComponentModel;
using Vibra_DesktopApp.Models;

namespace Vibra_DesktopApp.ViewModels.Components
{
    public sealed partial class SelectableArtistViewModel : ObservableObject
    {
        public User Wrapper { get; }
        public User Artist => Wrapper.artist ?? Wrapper;

        [ObservableProperty]
        private bool isSelected;

        public SelectableArtistViewModel(User wrapper)
        {
            Wrapper = wrapper;
        }
    }
}
