using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using Vibra_DesktopApp.ViewModels.Components;
using Vibra_DesktopApp.Views.Pages;

namespace Vibra_DesktopApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public SidebarViewModel SidebarVM { get; }
        public HeaderViewModel HeaderVM { get; }
        public PlayerViewModel PlayerVM { get; }


        [ObservableProperty] private ObservableObject currentPageViewModel;
        [ObservableProperty] private bool isPanelOpen = false;


        public GridLength PanelWidth => IsPanelOpen ? new GridLength(1, GridUnitType.Star) : new GridLength(0);


        public MainViewModel()
        {
            SidebarVM = new SidebarViewModel(this);
            HeaderVM = new HeaderViewModel(this);
            PlayerVM = new PlayerViewModel(this);

            currentPageViewModel = new HomeViewModel(this);
        }

        partial void OnIsPanelOpenChanged(bool value)
        {
            OnPropertyChanged(nameof(PanelWidth));
        }

        public void NavigateTo(ObservableObject vm)
        {
            CurrentPageViewModel = vm;
        }

        [RelayCommand]
        public void TogglePanel()
        {
            IsPanelOpen = !IsPanelOpen;
        }
    }
}
