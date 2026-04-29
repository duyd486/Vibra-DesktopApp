using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using Vibra_DesktopApp.Models;
using Vibra_DesktopApp.Singleton;
using Vibra_DesktopApp.ViewModels.Components;
using Vibra_DesktopApp.ViewModels.Pages;

namespace Vibra_DesktopApp.ViewModels
{
    public enum PanelContent
    {
        Queue = 0,
        Details = 1,
    }

    public partial class MainViewModel : ObservableObject
    {
        public SidebarViewModel SidebarVM { get; }
        public HeaderViewModel HeaderVM { get; }
        public PlayerViewModel PlayerVM { get; }
        public PanelViewModel PanelVM { get; }

        [ObservableProperty] private ObservableObject _currentPageViewModel;
        [ObservableProperty] private bool _isPanelOpen = false;
        [ObservableProperty] private PanelContent _panelContent = PanelContent.Queue;
        
        // Current active navigation item
        [ObservableProperty] private NavigationItem _currentNavigationItem = NavigationItem.Home;

        public GridLength PanelWidth => IsPanelOpen ? new GridLength(1, GridUnitType.Star) : new GridLength(0);

        public MainViewModel()
        {
            SidebarVM = new SidebarViewModel(this);
            HeaderVM = new HeaderViewModel(this);
            PlayerVM = new PlayerViewModel(this);
            PanelVM = new PanelViewModel(this);

            CurrentPageViewModel = new HomeViewModel(this);
            CurrentNavigationItem = NavigationItem.Home;
        }

        partial void OnIsPanelOpenChanged(bool value)
        {
            OnPropertyChanged(nameof(PanelWidth));
        }

        public void OpenPanel(PanelContent content)
        {
            PanelContent = content;
            IsPanelOpen = true;
        }

        public void ClosePanel()
        {
            IsPanelOpen = false;
        }

        public void NavigateTo(ObservableObject vm, NavigationItem navItem = NavigationItem.None)
        {
            CurrentPageViewModel = vm;
            CurrentNavigationItem = navItem;

            SidebarVM.SelectedNavigationItem = navItem;

            switch (vm)
            {
                case AlbumViewModel albumVm:
                    SidebarVM.SelectAlbumIfExists(albumVm.Album);
                    break;
                case ArtistViewModel artistVm:
                    SidebarVM.SelectArtistIfExists(artistVm.Artist);
                    break;
                case FavoriteSongsViewModel:
                    SidebarVM.IsFavoriteSongsSelected = true;
                    SidebarVM.SelectedNavigationItem = NavigationItem.Album;
                    SidebarVM.SelectedAlbum = null;
                    SidebarVM.SelectedPlaylist = null;
                    SidebarVM.SelectedArtist = null;
                    SidebarVM.SelectedArtistWrapper = null;
                    SidebarVM.SelectedAlbumId = null;
                    SidebarVM.SelectedPlaylistId = null;
                    SidebarVM.SelectedArtistId = null;
                    break;
                default:
                    SidebarSelectionBus.GetInstance().Publish(navItem, null);
                    SidebarVM.SelectedAlbum = null;
                    SidebarVM.SelectedPlaylist = null;
                    SidebarVM.SelectedArtist = null;
                    SidebarVM.SelectedArtistWrapper = null;
                    SidebarVM.IsFavoriteSongsSelected = false;
                    SidebarVM.SelectedAlbumId = null;
                    SidebarVM.SelectedPlaylistId = null;
                    SidebarVM.SelectedArtistId = null;
                    break;
            }
        }

        [RelayCommand]
        public void TogglePanel()
        {
            if (!IsPanelOpen)
            {
                PanelContent = PanelContent.Queue;
                IsPanelOpen = true;
                return;
            }

            IsPanelOpen = false;
        }
    }
}
