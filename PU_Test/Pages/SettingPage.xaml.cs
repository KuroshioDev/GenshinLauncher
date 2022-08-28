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

namespace PU_Test.Pages
{
    /// <summary>
    /// SettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingPage : Page
    {
        ViewModel.SettingPage vm = new ViewModel.SettingPage();
        public SettingPage()
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void CloseDialog(object sender, RoutedEventArgs e)
        {
            GlobalValues.frame.Visibility = Visibility.Collapsed;
        }

        private void GoToBroswer(object sender, MouseButtonEventArgs e)
        {
            dynamic control = sender;
            var url = control.Tag.ToString();
            Process.Start("explorer.exe", url);
        }
    }

}
