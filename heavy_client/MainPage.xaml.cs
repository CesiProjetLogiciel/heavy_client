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
using System.Diagnostics;
using System.Threading.Tasks;


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
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            connectButton.Visibility = Visibility.Collapsed;
            connectionProgress.Visibility = Visibility.Visible;

            string server_address = server_addressTextBox.Text;
            string username = usernameTextBox.Text;
            string password = passwordBox.Password;

            string connectionString = string.Format(@"Data Source={0};Initial Catalog=cesieats;User ID={1};Password={2}",
                server_address,
                username,
                password);

            try
            {
                await Task.Run(() => Connect(connectionString));
                (Application.Current as App).ConnectionString = connectionString;
                Dictionary<string, List<string>> permissions = null;

                if (username == "sa")
                    UserPermissions.Permissions = await Task.Run(() => UserPermissions.PopulatePermissionsSA());
                else
                    UserPermissions.Permissions = await Task.Run(() => UserPermissions.PopulatePermissions(connectionString, UserPermissions.query));
                
                Frame.Navigate(typeof(HomePage));
            }
            catch (Exception exc)
            {
                _ = new MessageDialog(exc.Message).ShowAsync();
                connectButton.Visibility = Visibility.Visible;
                connectionProgress.Visibility = Visibility.Collapsed;
            }
            //if (connect)
            //{
            //    (Application.Current as App).ConnectionString = connectionString;
            //    Frame.Navigate(typeof(HomePage));
            //}
            //else
            //{
            //    connectButton.Visibility = Visibility.Visible;
            //    connectionProgress.Visibility = Visibility.Collapsed;
            //}
        }

        private bool Connect(string connectionString)
        {
            bool EndWithoutError = false;

            SqlConnection cnn = new SqlConnection(connectionString);
            cnn.Open();
            if (cnn.State == System.Data.ConnectionState.Open)
            {
                cnn.Close();
            }
            return EndWithoutError;
        }
    }
}
