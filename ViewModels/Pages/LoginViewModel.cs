using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Duende.IdentityModel.OidcClient;
using Duende.IdentityModel.OidcClient.Browser;
using Google.Apis.Auth;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Vibra_DesktopApp.Models;
using Vibra_DesktopApp.Services;
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

        [ObservableProperty] private bool _isSubmitting;



        public LoginViewModel(IndexViewModel indexVM)
        {
            _indexVM = indexVM;
        }



        [RelayCommand]
        private async Task LoginAsync()
        {
            if (IsSubmitting)
                return;

            IsSubmitting = true;
            try
            {
                if (EmailText == null || PasswordText == null)
                {
                    MessageBox.Show("Vui lòng điền đủ tài khoản và mật khẩu");
                    return;
                }
                bool result = await ApiManager.GetInstance().LoginAsync(EmailText, PasswordText);
                //bool result = await ApiManager.GetInstance().LoginAsync("adele@gmail.com", "12345678");

                if (result && ApiManager.GetInstance().GetCurrentUser() != null)
                {
                    await OpenAfterLoginAsync();
                }
            }
            finally
            {
                IsSubmitting = false;
            }
        }




        [RelayCommand]
        private async Task LoginWithGoogle()
        {
            if (IsSubmitting)
                return;

            IsSubmitting = true;
            try
            {
                bool success = await ApiManager.GetInstance().LoginWithGoogleAsync();

                if (success && ApiManager.GetInstance().GetCurrentUser() != null)
                {
                    await OpenAfterLoginAsync();
                }
            }
            finally
            {
                IsSubmitting = false;
            }
        }

        [RelayCommand]
        private async Task LoginWithFacebook()
        {
            MessageBox.Show("Tính năng đăng nhập bằng Facebook đang được phát triển. Vui lòng thử lại sau!");
            //bool success = await ApiManager.GetInstance().LoginWithFacebookAsync();

            //if (success && ApiManager.GetInstance().GetCurrentUser() != null)
            //{
            //    MainWindow mainWindow = new MainWindow();
            //    mainWindow.Show();
            //    _indexVM.CloseWindow();
            //}
        }

        [RelayCommand]
        private void Test()
        {
            MessageBox.Show("Test command executed!");
        }

        [RelayCommand]
        private void OpenSignUp()
        {
            EmailText = "";
            PasswordText = "";
            _indexVM.ShowSignUp();
        }

        private async Task OpenAfterLoginAsync()
        {
            List<Song>? listRecentRotation;
            var recent = await ApiManager.GetInstance()
                .HttpGetAsync<List<Song>>("home/recent-rotation?limit=5");

            listRecentRotation = recent?.Take(1).ToList();

            //MessageBox.Show(listRecentRotation?.Count.ToString());

            if (listRecentRotation != null && listRecentRotation.Count > 0)
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                _indexVM.CloseWindow();
            }
            else
            {
                //MessageBox.Show("Đăng nhập thành công, nhưng không có bài hát nào trong phần Recent Rotation. Chuyển sang chọn interest");
                _indexVM.ShowInterest();
            }

        }

        [RelayCommand]
        private void CloseWindow()
        {
            Application.Current?.Shutdown();
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
