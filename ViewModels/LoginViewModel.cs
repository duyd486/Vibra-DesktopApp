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
        private LoginWindow? loginWindow;
        private SignUpWindow? signUpWindow;

        [ObservableProperty] private string? emailText;
        [ObservableProperty] private string? passwordText;
        [ObservableProperty] private string? rePasswordText;


        public LoginViewModel(LoginWindow login)
        {
            loginWindow = login;
        }



        [RelayCommand]
        private void Login()
        {
            if(EmailText == null || PasswordText == null)
            {
                MessageBox.Show("Vui lòng điền đủ tài khoản và mật khẩu");
                return;
            }

            _ = ApiManager.GetInstance().LoginAsync(EmailText, PasswordText, () =>
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                loginWindow.Close();
            });
        }

        [RelayCommand]
        private void SignUp()
        {
            if (EmailText == null || PasswordText == null || RePasswordText == null)
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin");
                return;
            }
            if (PasswordText != RePasswordText)
            {
                MessageBox.Show("Mật khẩu không trùng khớp");
                return;
            }

            _ = ApiManager.GetInstance().SignUpAsync(EmailText, PasswordText, CloseSignUp);
        }

        [RelayCommand]
        private void OpenSignUp()
        {
            EmailText = "";
            signUpWindow = new SignUpWindow(this);
            signUpWindow.ShowDialog();
        }

        [RelayCommand]
        private void CloseSignUp()
        {
            EmailText = "";
            signUpWindow?.Close();
        }




        public void SetPassword(string password)
        {
            PasswordText = password;
        }
        public void SetRePassword(string rePassword)
        {
            RePasswordText = rePassword;
        }
    }
}
