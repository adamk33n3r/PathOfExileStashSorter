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
using POEStashSorterModels;
using System.Threading;
using System.Windows.Threading;
using PoeStashSorterModels.Exceptions;
using PoeStashSorterModels.Servers;

namespace POEStashSorter
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private Server server;
        private double originalHight;
        public LoginWindow()
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(Settings.Instance.Username))
            {
                txtEmail.Text = Settings.Instance.Username;
                txtPassword.Password = Settings.Instance.Password.Decrypt();
            }
            else if (!string.IsNullOrEmpty(Settings.Instance.SessionID))
            {
                txtSessionID.Text = Settings.Instance.SessionID;
                chkUseSessionID.IsChecked = true;
            }
            
            List<Server> servers=new List<Server>();
            servers.Add(new GeneralServer());
            servers.Add(new GarenaCisServer());
            servers.Add(new GarenaThServer());
            servers.Add(new GarenaSgServer());
            servers.Add(new GarenaTWServer());
            CbComboBox.ItemsSource = servers;
            CbComboBox.DisplayMemberPath = "Name";
            CbComboBox.SelectedIndex = Settings.Instance.ServerID;
            originalHight = Height;
        }


        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlay();
            String username = null;
            String password = null;
            bool useSessionID = chkUseSessionID.IsChecked == true;

            Settings.Instance.Username = null;
            Settings.Instance.Password = null;
            Settings.Instance.SessionID = null;

            username = txtEmail.Text;
            password = txtPassword.Password;

            if (chkRememberMe.IsChecked == true && !useSessionID)
            {
                Settings.Instance.Username = txtEmail.Text;
                Settings.Instance.Password = txtPassword.Password.Encrypt();
            }
            else if (chkRememberMe.IsChecked == true)
            {
                Settings.Instance.SessionID = txtSessionID.Text;
                password = txtSessionID.Text;
            }
            Settings.Instance.ServerID = CbComboBox.SelectedIndex;
            
            try
            {
                PoeConnector.Connect(server, username, password, useSessionID);
                Settings.Instance.SaveChanges();
                ErrorText.Content = string.Empty;
                var main = new MainWindow();
                main.Show();
                Application.Current.MainWindow = main;
                this.Close();
            }
            catch (Exception ex)
            {
                Height = originalHight + 25;
                HideOverlay();
                ErrorText.Content = ex.Message;
            }

        }

        private void ShowOverlay()
        {
            overlayBg.Visibility = overlayTxt.Visibility = Visibility.Visible;
            Wait(0.5);
        }
        private void HideOverlay()
        {
            overlayBg.Visibility = overlayTxt.Visibility = Visibility.Hidden;
            Wait(0.5);
        }

        private void chkUseSessionID_Checked(object sender, RoutedEventArgs e)
        {
            Visibility emailVisibility = chkUseSessionID.IsChecked == true ? Visibility.Hidden : Visibility.Visible;
            Visibility sessionVisibility = chkUseSessionID.IsChecked == true ? Visibility.Visible : Visibility.Hidden;
            lblEmail.Visibility = emailVisibility;
            txtEmail.Visibility = emailVisibility;
            lblPassword.Visibility = emailVisibility;
            txtPassword.Visibility = emailVisibility;
            lblSessionID.Visibility = sessionVisibility;
            txtSessionID.Visibility = sessionVisibility;
        }

        private void Wait(double seconds)
        {
            var frame = new DispatcherFrame();
            new Thread((ThreadStart)(() =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(seconds));
                frame.Continue = false;
            })).Start();
            Dispatcher.PushFrame(frame);
        }

        private void CbComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            server = (Server)CbComboBox.SelectedItem;
            lblEmail.Content = server.EmailLoginName;
            
        }

    }
}
