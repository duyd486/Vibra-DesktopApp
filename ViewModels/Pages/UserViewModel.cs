using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vibra_DesktopApp.ViewModels.Pages
{
    public partial class UserViewModel : ObservableObject
    {
        private readonly MainViewModel _mainVM;

        public UserViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
        }
    }
}
