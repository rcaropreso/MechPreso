using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WpfApp1.View;
using WpfApp1.ViewModel;

namespace WpfApp1
{
    /// <summary>
    /// Interação lógica para App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //MainWindow app = new MainWindow();
            MainWindow1 app = new MainWindow1();
            MainWindowViewModel context = new MainWindowViewModel();
            app.DataContext = context;
            app.Show();
        }
    }
}
