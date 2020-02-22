using NotifierServiece;
using System;
using System.Collections.Generic;
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

namespace notification_gui
{
    /// <summary>
    /// Interaction logic for AddResourcePage.xaml
    /// </summary>
    public partial class AddResourcePage : Page
    {
        public AddResourcePage()
        {
            InitializeComponent();
            tbUrl.Focus();
        }

        private void BtnSab_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;

            var url = tbUrl.Text;

            Task.Factory.StartNew(() =>
            {
                if (UpdateManager.GetUpdateManager.TryToAddUrl(url))
                {
                    Action navigateBackAction = () => { MainWindow.NavigateBack(); };
                    Application.Current.Dispatcher.BeginInvoke(navigateBackAction);
                }
                else
                {
                    MessageBox.Show("OOOOOops? Check ULR..");
                    Action enableButtoonAction = () => { (sender as Button).IsEnabled = true; };
                    Application.Current.Dispatcher.BeginInvoke(enableButtoonAction);
                }
            });
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.NavigateBack();
        }
    }
}
