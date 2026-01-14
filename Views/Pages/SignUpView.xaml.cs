using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Vibra_DesktopApp.ViewModels;

namespace Vibra_DesktopApp.Views
{
    /// <summary>
    /// Interaction logic for SignUpView.xaml
    /// </summary>
    public partial class SignUpView : UserControl
    {
        public SignUpView()
        {
            InitializeComponent();
        }
        private void PasswordBox_RePasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is SignUpViewModel vm && sender is PasswordBox box)
            {
                vm.SetRePassword(box.Password);
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is SignUpViewModel vm && sender is PasswordBox box)
            {
                vm.SetPassword(box.Password);
            }
        }
    }
}
