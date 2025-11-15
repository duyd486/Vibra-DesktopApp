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
    /// Interaction logic for SignUpWindow.xaml
    /// </summary>
    public partial class SignUpWindow : Window
    {
        LoginViewModel viewModel;
        public SignUpWindow(LoginViewModel vm)
        {
            InitializeComponent();
            viewModel = vm;
            DataContext = viewModel;
        }

        private void PasswordBox_RePasswordChanged(object sender, RoutedEventArgs e)
        {
            var senderBox = sender as PasswordBox;
            viewModel.SetRePassword(senderBox.Password);
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var senderBox = sender as PasswordBox;
            viewModel.SetPassword(senderBox.Password);
        }
    }
}
