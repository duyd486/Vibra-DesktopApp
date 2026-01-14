using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Vibra_DesktopApp.Singleton;

namespace Vibra_DesktopApp.ViewModels
{
    public partial class SignUpViewModel : ObservableObject
    {
        private readonly IndexViewModel _indexVM;

        [ObservableProperty] private string? emailText;
        [ObservableProperty] private string? passwordText;
        [ObservableProperty] private string? rePasswordText;


        public SignUpViewModel(IndexViewModel indexVM)
        {
            _indexVM = indexVM;
        }

        [RelayCommand]
        private async Task SignUp()
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

            bool result = await ApiManager.GetInstance().SignUpAsync(EmailText, PasswordText);

            if (result)
            {
                OpenLogin();
            }
        }

        [RelayCommand]
        private void OpenLogin()
        {
            EmailText = "";
            PasswordText = "";
            RePasswordText = "";
            _indexVM.ShowLogin();
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
