using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Vibra_DesktopApp.ViewModels
{
    public partial class IndexViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableObject currentViewModel;

        public LoginViewModel LoginVM { get; }
        public SignUpViewModel SignUpVM { get; }


        public event EventHandler? OnWindowClose;



        public IndexViewModel()
        {
            LoginVM = new LoginViewModel(this);
            SignUpVM = new SignUpViewModel(this);

            CurrentViewModel = LoginVM;
        }


        public void CloseWindow()
        {
            OnWindowClose?.Invoke(this, EventArgs.Empty);
        }


        public void ShowSignUp()
            => CurrentViewModel = SignUpVM;

        public void ShowLogin()
            => CurrentViewModel = LoginVM;
    }
}
