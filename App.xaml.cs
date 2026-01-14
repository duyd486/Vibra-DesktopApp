using System.Configuration;
using System.Data;
using System.Windows;
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

            var indexVM = new IndexViewModel();

            var indexWindow = new IndexWindow
            {
                DataContext = indexVM
            };

            indexWindow.Show();
        }
    }

}
