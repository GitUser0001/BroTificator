using Notifications.Wpf;
using NotifierServiece;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
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
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            UpdateManager.GetUpdateManager.onCollectionChaned += UpdateUI;
            UpdateManager.GetUpdateManager.StartWathcer();
        }

        private void UpdateUI(IEnumerable<MediaContent> mediaContents)
        {
            Action<List<MediaContent>> action = (x) => { lboxMediaContent.ItemsSource = x; };
            Application.Current.Dispatcher.BeginInvoke(action, mediaContents);
        }

        private void TestBtn_Click(object sender, RoutedEventArgs e)
        {
            var notificationManager = new NotificationManager();
            notificationManager.Show(new NotificationContent
            {
                Title = "Sample notification",
                Message = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
                Type = NotificationType.Information
            });
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Navigate(new AddResourcePage());
        }

        private void BtnDel_Click(object sender, RoutedEventArgs e)
        {
            var result = false;
            var mediaContent = GetSelectedProject();
            
            if (mediaContent != null)
                result = UpdateManager.GetUpdateManager.TryToRemoveMediaContent(mediaContent);

            if (!result)
                MessageBox.Show("Oops?");
        }

        private MediaContent GetSelectedProject()
        {
            return (lboxMediaContent.SelectedItem as MediaContent);
        }
    }
}
