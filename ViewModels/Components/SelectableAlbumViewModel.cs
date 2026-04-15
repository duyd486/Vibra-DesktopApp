using CommunityToolkit.Mvvm.ComponentModel;
using Vibra_DesktopApp.Models;

namespace Vibra_DesktopApp.ViewModels.Components
{
    public sealed partial class SelectableAlbumViewModel : ObservableObject
    {
        public Album Album { get; }

        [ObservableProperty]
        private bool isSelected;

        public SelectableAlbumViewModel(Album album)
        {
            Album = album;
        }
    }
}
