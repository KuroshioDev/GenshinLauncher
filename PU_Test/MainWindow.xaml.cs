using PU_Test.Common;
using PU_Test.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace PU_Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal ViewModel.MainWindow vm;
        public MainWindow()
        {
            InitializeComponent();
            GlobalValues.mainWindow = this;
            vm= new ViewModel.MainWindow();
            DataContext = vm;
            GlobalValues.frame = dialog_frame;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            GlobalValues.frame.Visibility = Visibility.Visible;
            GlobalValues.frame.Navigate(new Pages.SettingPage());
        }

        private void WindowMinize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void GoToBroswer(object sender, MouseButtonEventArgs e)
        {
            dynamic control = sender;
            var url=control.Tag.ToString();
            Process.Start("explorer.exe", url);
        }

        private void RefreshServerInfo(object sender, MouseButtonEventArgs e)
        {
                vm.UpdateSI();

        }
    }
}
