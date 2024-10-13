using AutoProxy.Helpers;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AutoProxy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = App.MainViewModel;
            new ConsoleOutputRedirector(txtOutput,sclOutput);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            App.MainViewModel.proxy.auto.AutoMode = true;
        }

        private void AboutMe_Click(object sender, RoutedEventArgs e)
        {
            var resault = MessageBox.Show(
                "Yasin Ebrahim Nezhadian. " +
                "\r\nhttps://github.com/nezhadian/AutoProxy" +
                "\r\nClick OK to Copy to Clipboard.");
            if(resault == MessageBoxResult.OK)
            {
                Clipboard.SetText("https://github.com/nezhadian/AutoProxy");
            }
        }
    }
}
