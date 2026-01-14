using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vibra_DesktopApp.ViewModels.Pages
{
    public partial class SearchViewModel : ObservableObject
    {
        private readonly MainViewModel _mainVM;

        public SearchViewModel(MainViewModel mainVM, string searchText)
        {
            _mainVM = mainVM;
        }
    }
}
