using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using Vibra_DesktopApp.Models;
using Vibra_DesktopApp.ViewModels.Components;

namespace Vibra_DesktopApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public SidebarViewModel SidebarVM { get; }
        public HeaderViewModel HeaderVM { get; }
        public PlayerViewModel PlayerVM { get; }

        [ObservableProperty] private ObservableObject _currentPageViewModel;
        [ObservableProperty] private bool _isPanelOpen = false;
        
        // Current active navigation item
        [ObservableProperty] private NavigationItem _currentNavigationItem = NavigationItem.Home;

        public GridLength PanelWidth => IsPanelOpen ? new GridLength(1, GridUnitType.Star) : new GridLength(0);

        public MainViewModel()
        {
            SidebarVM = new SidebarViewModel(this);
            HeaderVM = new HeaderViewModel(this);
            PlayerVM = new PlayerViewModel(this);

            CurrentPageViewModel = new HomeViewModel(this);
            CurrentNavigationItem = NavigationItem.Home;
        }

        partial void OnIsPanelOpenChanged(bool value)
        {
            OnPropertyChanged(nameof(PanelWidth));
        }

        public void NavigateTo(ObservableObject vm, NavigationItem navItem = NavigationItem.None)
        {
            CurrentPageViewModel = vm;
            CurrentNavigationItem = navItem;
        }

        [RelayCommand]
        public void TogglePanel()
        {
            IsPanelOpen = !IsPanelOpen;
        }
    }
}
