using CommunityToolkit.Mvvm.ComponentModel;

namespace Vibra_DesktopApp.ViewModels.Pages
{
    public partial class CategoriesViewModel : ObservableObject
    {
        private readonly MainViewModel _mainVM;

        public CategoriesViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
        }
    }
}
