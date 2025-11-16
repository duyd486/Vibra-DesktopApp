using CommunityToolkit.Mvvm.ComponentModel;
using Vibra_DesktopApp.Views.Pages;

namespace Vibra_DesktopApp.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        public static MainWindowViewModel? Instance { get; private set; }

        public MainWindowViewModel()
        {
            currentPage = new HomePage();
        }

        [ObservableProperty] public object? currentPage;

        public static MainWindowViewModel GetInstance()
        {
            if (Instance == null)
            {
                Instance = new MainWindowViewModel();
            }
            return Instance;
        }
    }
}
