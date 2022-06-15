using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;


// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace heavy_client
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            ApplicationView.PreferredLaunchViewSize = new Size(520, 390);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Connect();
            Frame.Navigate(typeof(HomePage));

        }

        private void Connect()
        {
            string data_source = server_addressTextBox.Text;
            string username = usernameTextBox.Text;
            string password = passwordBox.Password;

            string connectionString = String.Format(@"Data Source={0};Initial Catalog=cesieats;User ID={1};Password={2}",
                data_source, username, password);
            SqlConnection cnn = new SqlConnection(connectionString);
            try
            {
                cnn.Open();
                if (cnn.State == System.Data.ConnectionState.Open)
                {
                    _ = new MessageDialog("Connected").ShowAsync();
                    Frame.Navigate(typeof(HomePage));
                    (App.Current as App).ConnectionString = connectionString;
                    cnn.Close();
                }
            }
            catch (Exception exc)
            {
                _ = new MessageDialog(exc.Message).ShowAsync();
            }
        }
    }
}
