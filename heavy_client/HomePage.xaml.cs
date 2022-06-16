using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=234238

namespace heavy_client
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        private readonly string _connectionString = (App.Current as App).ConnectionString;
        private bool _changeIsSuspendedTo = true;

        private ObservableCollection<User> _users = new ObservableCollection<User>();
        public ObservableCollection<User> Users { get => _users; set => _users = value; }

        public HomePage()
        {
            InitializeComponent();


            //ApplicationView.SetPreferredMinSize(new Size(515, 330));

            //ApplicationView.PreferredLaunchViewSize = new Size(1280, 720);
            //ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }

        // Define this method within your main page class.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Instead of hard coded items, the data could be pulled
            // asynchronously from a database or the internet.
            const string GetUsersQuery = "SELECT Users.id, lastname, firstname," +
                                         " email, isSuspended, UserTypes.type " +
                                         " from Users inner join UserTypes on Users.id_UserTypes = UserTypes.id ";
            GetUsers(_connectionString, GetUsersQuery);
        }

        public void GetUsers(string connectionString, string GetUsersQuery)
        {
            Users.Clear();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetUsersQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var user = new User
                                    {
                                        UserID = reader.GetInt32(0),
                                        LastName = reader.GetString(1),
                                        FirstName = reader.GetString(2),
                                        Email = reader.GetString(3),
                                        IsSuspended = reader.GetBoolean(4),
                                        UserType = reader.GetString(5)
                                    };
                                    Users.Add(user);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                _ = new MessageDialog("Exception: " + eSql.Message).ShowAsync();
            }
        }

        private void UsersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListView).SelectedItem != null)
            {
                User user = (sender as ListView).SelectedItem as User;
                Disable_Button.IsEnabled = true;
                Edit_Button.IsEnabled = true;
                Delete_Button.IsEnabled = true;
                _changeIsSuspendedTo = user.IsSuspended;

                if (user.IsSuspended == true)
                {
                    Disable_Button.Content = "Enable";
                    _changeIsSuspendedTo = false;
                }
                else if (user.IsSuspended == false)
                {
                    Disable_Button.Content = "Disable";
                    _changeIsSuspendedTo = true;
                }
            }
            else
            {
                Disable_Button.IsEnabled = false;
            }
        }

        private void Disable_Button_Click(object sender, RoutedEventArgs e)
        {
            User user = UsersListView.SelectedItem as User;
            Disable_Button.Focus(FocusState.Pointer); //made the button get focus.
            string SetIsSuspendedQuery = String.Format("SET XACT_ABORT ON;BEGIN TRANSACTION;UPDATE Users SET Users.isSuspended = {0} WHERE Users.id = {1};COMMIT;", _changeIsSuspendedTo ? 1 : 0, user.UserID);
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = SetIsSuspendedQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    _ = new MessageDialog(reader.GetString(0)).ShowAsync();
                                }
                            }
                        }
                    }
                }
                user.IsSuspended = _changeIsSuspendedTo;
                UsersListView_SelectionChanged(UsersListView, null);
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                _ = new MessageDialog("Exception: " + eSql.Message).ShowAsync();
            }
        }

        private void add_Button_Click(object sender, RoutedEventArgs e)
        {
            Users.Add(new User
            {
                UserID = 5,
                LastName = "Test",
                FirstName = "test48856262",
                Email = "test@gmail.test",
                IsSuspended = true,
                UserType = "Livreur"
            });
        }

        private async void Export_Button_Click(object sender, RoutedEventArgs e)
        {
            const string end_line = ";\n";

            string csv = "ID,Lastname,Firstname,Email Address,Is suspended,Type;\n";

            foreach (User user in Users)
            {
                string last = user.GetType().GetProperties().Last().GetValue(user, null).ToString();
                foreach (PropertyInfo prop in user.GetType().GetProperties())
                {
                    csv += prop.GetValue(user, null).ToString();
                    if (prop.GetValue(user, null).ToString() != last)
                    {
                        csv += ",";
                    }
                }

                csv += end_line;
            }
            Debug.WriteLine(csv);

            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("CSV File", new List<string>() { ".csv" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "Users list";

            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                Windows.Storage.CachedFileManager.DeferUpdates(file);
                // write to file
                await Windows.Storage.FileIO.WriteTextAsync(file, csv);
                // Let Windows know that we're finished changing the file so
                // the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                Windows.Storage.Provider.FileUpdateStatus status = await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
                //if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                //{
                //    this.textBlock.Text = "File " + file.Name + " was saved.";
                //}
                //else
                //{
                //    this.textBlock.Text = "File " + file.Name + " couldn't be saved.";
                //}
            }
            //else
            //{
            //    this.textBlock.Text = "Operation cancelled.";
            //}
        }

        private void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            User user = UsersListView.SelectedItem as User;
            Disable_Button.Focus(FocusState.Pointer); //made the button get focus.
            string SetDeleteQuery = "SET XACT_ABORT ON;" +
                                    "BEGIN TRANSACTION;" +
                                    String.Format("DELETE FROM BillingAddress WHERE BillingAddress.id_Users = {0};", user.UserID) +
                                    String.Format("DELETE FROM DeliveryAddress WHERE DeliveryAddress.id_Users = {0};", user.UserID) +
                                    String.Format("DELETE FROM paypalAddress WHERE paypalAddress.id_Users = {0};", user.UserID) +
                                    String.Format("DELETE FROM Users WHERE Users.id = {0};", user.UserID) +
                                    "COMMIT;";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = SetDeleteQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    _ = new MessageDialog(reader.GetString(0)).ShowAsync();
                                }
                            }
                        }
                    }
                }
                Users.Remove(user);
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                _ = new MessageDialog("Exception: " + eSql.Message).ShowAsync();
            }
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (SearchBar.Text != null)
            //{
            //    const string SetSearchQuery = "SELECT Users.id, lastname, firstname," +
            //                             " email, isSuspended, UserTypes.type " +
            //                             " from Users inner join UserTypes on Users.id_UserTypes = UserTypes.id ";
            //}
            string SetSearchQuery = "SELECT Users.id, Users.lastname, Users.firstname, Users.email, Users.isSuspended, UserTypes.type " +
                                    "FROM Users INNER JOIN UserTypes ON Users.id_UserTypes = UserTypes.id " +
                                    String.Format("WHERE Users.lastname LIKE '%{0}%' OR Users.firstname LIKE '%{0}%' OR Users.email LIKE '%{0}%'", SearchBar.Text);
            GetUsers(_connectionString, SetSearchQuery);
        }

        private void Edit_Button_Click(object sender, RoutedEventArgs e)
        {
            User userselected = UsersListView.SelectedItem as User;
            Frame.Navigate(typeof(UserPage), userselected.UserID);
        }
    }
}
