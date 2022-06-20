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
        private readonly string _connectionString = (Application.Current as App).ConnectionString;
        private bool _changeIsSuspendedTo = true;

        const string GetUsersQuery = "SELECT Users.id, lastname, firstname," +
                                         " email, isSuspended, id_UserTypes, paypalAddress.id, paypalAddress.paypal" +
                                         " FROM Users LEFT JOIN paypalAddress ON Users.id=paypalAddress.id_Users";

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
            const string GetUserTypesQuery = "SELECT UserTypes.id, UserTypes.type"+
                                            " FROM UserTypes;";
            GetUserTypes(_connectionString, GetUserTypesQuery);
            GetUsers(_connectionString, GetUsersQuery);
        }

        public void GetUserTypes(string connectionString, string GetUsersQuery)
        {
            UserTypeORMResources.UserTypes.Clear();
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
                                    var userType = new UserType
                                    {
                                        UserTypeID = reader.GetInt32(0),
                                        Type = reader.GetString(1)
                                    };
                                    UserTypeORMResources.UserTypes.Add(userType);
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

        public void GetUsers(string connectionString, string GetUsersQuery)
        {
            UserORMResources.Users.Clear();
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
                                        UserType = UserTypeORMResources.UserTypes.Where(x => x.UserTypeID == reader.GetInt32(5)).First()
                                    };
                                    user.PaypalEmail.PaypalEmailID = reader.IsDBNull(6) ? null : (int?)reader.GetInt32(6);
                                    user.PaypalEmail.Email = reader.IsDBNull(6) ? null : reader.GetString(7);
                                    user.PaypalEmail.IsRegistered = reader.IsDBNull(6) ? false : true;
                                    UserORMResources.Users.Add(user);
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
                if (UserPermissions.IsAllowed("Users.isSuspended", "UPDATE"))
                    Disable_Button.IsEnabled = true;

                if (UserPermissions.IsAllowed("Users", "UPDATE"))
                    Edit_Button.IsEnabled = true;
                Edit_Button.IsEnabled = true;

                if (UserPermissions.IsAllowed("Users", "DELETE")
                    && UserPermissions.IsAllowed("BillingAddress", "DELETE")
                    && UserPermissions.IsAllowed("DeliveryAddress", "DELETE")
                    && UserPermissions.IsAllowed("paypalAddress", "DELETE"))
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
                Disable_Button.IsEnabled = false;
        }

        private void Disable_Button_Click(object sender, RoutedEventArgs e)
        {
            User user = UsersListView.SelectedItem as User;
            Disable_Button.Focus(FocusState.Pointer); //made the button get focus.
            string SetIsSuspendedQuery = string.Format("SET XACT_ABORT ON;BEGIN TRANSACTION;UPDATE Users SET Users.isSuspended = {0} WHERE Users.id = {1};COMMIT;", _changeIsSuspendedTo ? 1 : 0, user.UserID);
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

        private async void Export_Button_Click(object sender, RoutedEventArgs e)
        {
            const string end_line = ";\n";

            string csv = "ID,Lastname,Firstname,Email Address,Is suspended,Type,Paypal address;\n";

            foreach (User user in UserORMResources.Users)
            {
                csv += string.Format("{0},{1},{2},{3},{4},{5},{6}",
                    user.UserID, 
                    user.LastName, 
                    user.FirstName, 
                    user.Email, 
                    user.IsSuspended, 
                    user.UserType.Type, 
                    user.PaypalEmail.Email 
                );
                //string last = user.GetType().GetProperties().Last().GetValue(user, null).ToString();
                //foreach (PropertyInfo prop in user.GetType().GetProperties())
                //{
                //    csv += prop.GetValue(user, null).ToString();
                //    if (prop.GetValue(user, null).ToString() != last)
                //    {
                //        csv += ",";
                //    }
                //}

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
                if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                {
                    _ = new MessageDialog("File " + file.Name + " was saved successfully.").ShowAsync();
                }
                else
                {
                    _ = new MessageDialog("File " + file.Name + " couldn't be saved.").ShowAsync();
                }
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
                                    string.Format("DELETE FROM BillingAddress WHERE BillingAddress.id_Users = {0};", user.UserID) +
                                    string.Format("DELETE FROM DeliveryAddress WHERE DeliveryAddress.id_Users = {0};", user.UserID) +
                                    string.Format("DELETE FROM paypalAddress WHERE paypalAddress.id_Users = {0};", user.UserID) +
                                    string.Format("DELETE FROM Users WHERE Users.id = {0};", user.UserID) +
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
                UserORMResources.Users.Remove(user);
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
            string SetSearchQuery = GetUsersQuery +
                                    string.Format(" WHERE Users.lastname LIKE '%{0}%' OR Users.firstname LIKE '%{0}%' OR Users.email LIKE '%{0}%'", ((TextBox)sender).Text);
            GetUsers(_connectionString, SetSearchQuery);
        }

        private void Edit_Button_Click(object sender, RoutedEventArgs e) => 
            Frame.Navigate(typeof(UserPage), UsersListView.SelectedItem);

    }
}
