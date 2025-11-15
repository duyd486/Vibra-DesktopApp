using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Vibra_DesktopApp.Singleton;
using Vibra_DesktopApp.Views;

namespace Vibra_DesktopApp.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty] private string? emailText;
        [ObservableProperty] private string? passwordText;



        [RelayCommand]
        private void Login()
        {
            if(EmailText == null || PasswordText == null)
            {
                MessageBox.Show("Please enter both email and password.");
                return;
            }

            ApiManager.GetInstance().LoginAsync(EmailText, PasswordText);
        }

        [RelayCommand]
        private void OpenSignUp()
        {
            SignUpWindow signUpWindow = new SignUpWindow(this);
            signUpWindow.ShowDialog();
        }



        public void SetPassword(string password)
        {
            PasswordText = password;
        }
    }
}
