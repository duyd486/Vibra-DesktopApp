using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Vibra_DesktopApp.Singleton;
using Vibra_DesktopApp.Views;

namespace Vibra_DesktopApp.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IndexViewModel _indexVM;

        [ObservableProperty] private string? emailText;
        [ObservableProperty] private string? passwordText;
        [ObservableProperty] private string? rePasswordText;



        public LoginViewModel(IndexViewModel indexVM)
        {
            _indexVM = indexVM;
        }



        [RelayCommand]
        private async Task LoginAsync()
        {
            //if(EmailText == null || PasswordText == null)
            //{
            //    MessageBox.Show("Vui lòng điền đủ tài khoản và mật khẩu");
            //    return;
            //}

            //bool result = await ApiManager.GetInstance().LoginAsync(EmailText, PasswordText);

            //if (result && ApiManager.GetInstance().GetCurrentUser() != null)
            //{
            //    MainWindow mainWindow = new MainWindow();
            //    mainWindow.Show();
            //    //loginView?.Close();
            //}


            bool result = await ApiManager.GetInstance().LoginAsync("adele@gmail.com", "12345678");
            if (result && ApiManager.GetInstance().GetCurrentUser() != null)
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                _indexVM.CloseWindow();
            }
        }



        [RelayCommand]
        private void OpenSignUp()
        {
            EmailText = "";
            PasswordText = "";
            _indexVM.ShowSignUp();
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
