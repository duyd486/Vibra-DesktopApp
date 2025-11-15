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
using System.Windows.Shapes;
using Vibra_DesktopApp.ViewModels;

namespace Vibra_DesktopApp.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        LoginViewModel viewModel;
        public LoginWindow()
        {
            InitializeComponent();
            DataContext = viewModel = new LoginViewModel(this);
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var senderBox = sender as PasswordBox;
            viewModel.SetPassword(senderBox.Password);
        }
    }
}
