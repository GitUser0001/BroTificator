using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using NotifierServiece;
using Notifications.Wpf;
using System.Diagnostics;

namespace notification_gui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private System.Windows.Forms.NotifyIcon _notifyIcon;

        private bool _isExit;

        protected override void OnStartup(StartupEventArgs e)
        {
            var thisProcessName = Process.GetCurrentProcess().ProcessName;
            if (Process.GetProcesses().Count(p => p.ProcessName == thisProcessName) > 1) {
                MessageBox.Show("You have another instance of the applicaiton");
                Application.Current.Shutdown();
            }

            base.OnStartup(e);

            UpdateManager.GetUpdateManager.onMediaContentChanged += ShowNotification;

            MainWindow = new MainWindow();
            MainWindow.Closing += MainWindow_Closing;

            MainWindow.Show();

            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.DoubleClick += (s, args) => ShowMainWindow();
            _notifyIcon.Icon = notification_gui.Properties.Resources.Icon;
            _notifyIcon.Visible = true;

            CreateContextMenu();
        }

        private void ShowNotification(MediaContent mediaContent)
        {
            var notificationManager = new NotificationManager();
            notificationManager.Show(new NotificationContent
            {
                Title = mediaContent.Name,
                Message = $"{mediaContent.Name} was update, currnet status {mediaContent.Status}",
                Type = NotificationType.Information
            });
        }

        private void CreateContextMenu()
        {
            _notifyIcon.ContextMenuStrip =
              new System.Windows.Forms.ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("MainWindow...").Click += (s, e) => ShowMainWindow();
            _notifyIcon.ContextMenuStrip.Items.Add("Exit").Click += (s, e) => ExitApplication();
        }

        private void ExitApplication()
        {
            _isExit = true;
            MainWindow.Close();
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }

        private void ShowMainWindow()
        {
            if (MainWindow.IsVisible)
            {
                if (MainWindow.WindowState == WindowState.Minimized)
                {
                    MainWindow.WindowState = WindowState.Normal;
                }
                MainWindow.Activate();
            }
            else 
            {
                MainWindow.Show();
            }           
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!_isExit)
            {
                e.Cancel = true;
                MainWindow.Hide();
            }
        }
    }
}
