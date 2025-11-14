using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Vibra_DesktopApp.ViewModels
{
    partial class LoginViewModel : ObservableObject
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

            Console.WriteLine("Login button clicked");
            MessageBox.Show("Login button clicked " + EmailText + " " + PasswordText);
        }


        public void SetPassword(string password)
        {
            PasswordText = password;
        }
    }
}
