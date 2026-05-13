using System.Configuration;
using System.Data;
using System.Windows;
using Vibra_DesktopApp.Services;
using Vibra_DesktopApp.ViewModels;
using Vibra_DesktopApp.Views;

namespace Vibra_DesktopApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var user = SessionManager.LoadUserAsync().Result;

            if(user != null)
            {
                MainWindow mainWindow = new MainWindow()
                {
                    DataContext = new MainViewModel()
                };
                mainWindow.Show();
            }
            else
            {
                var indexVM = new IndexViewModel();

                var indexWindow = new IndexWindow (indexVM)
                {
                    DataContext = indexVM
                };

                indexWindow.Show();
            }

        }
    }

}
