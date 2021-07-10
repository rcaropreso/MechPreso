using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

//Para tratar o OnClose da janela principal
using Microsoft.Xaml.Behaviors; //https://stackoverflow.com/questions/8360209/how-to-add-system-windows-interactivity-to-project


namespace WpfApp1
{
    /// <summary>
    /// Lógica interna para MainWindow1.xaml
    /// </summary>
    public partial class MainWindow1 : Window
    {
        public MainWindow1()
        {
            InitializeComponent();
        }

        private void textChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            this.txtEventLog.ScrollToEnd();
        }
    }
}
