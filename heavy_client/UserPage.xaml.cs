using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Input;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=234238

namespace heavy_client
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class UserPage : Page
    {

        private readonly string _connectionString = (Application.Current as App).ConnectionString;
        private static readonly string[] _isPaypalAllowed = { "restaurant", "delivery man" };

        public User UserSelected { get; set; }

        public static bool IsPaypalAllowed(User user)
        {
            return _isPaypalAllowed.Contains(user.UserType.Type.ToLower());
        }

        public static Visibility IsVisible(User user)
        {
            return IsPaypalAllowed(user) ? Visibility.Visible : Visibility.Collapsed;
        }

        string GetAddressQuery (string table)
            {
                return string.Format("SELECT zipcode, city, address, state, " +
                "additionnalInfo, t1.lastname, t1.firstname, phonenumber, t1.phonecountrycode, t1.name, t1.id " +
                "FROM (SELECT {0}.id, zipcode, city, address, state, additionnalInfo, {0}.lastname, {0}.firstname, " +
                "phonenumber, Countries.name, {0}.phonecountrycode, {0}.id_Users " +
                "FROM {0} INNER JOIN Countries ON  {0}.id_Countries = Countries.id) " +
                "AS t1 INNER JOIN Users ON t1.id_Users = Users.id WHERE Users.id = {1} ", table, UserSelected.UserID);
            }

        public UserPage()
        {
            InitializeComponent();
        }

        // Define this method within your main page class.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            UserSelected = e.Parameter as User;

            if (!UserPermissions.IsAllowed("DeliveryAddress", "INSERT"))
                AddDeliveryAddress.IsEnabled = false;
            if (!UserPermissions.IsAllowed("BillingAddress", "INSERT"))
                AddDeliveryAddress.IsEnabled = false;

            if (!UserPermissions.IsAllowed("Users", "UPDATE"))
            {
                LastnameTextBox.IsEnabled = false;
                FirstnameTextBox.IsEnabled = false;
                EmailTextBox.IsEnabled = false;
                ClientTypeComboBox.IsEnabled = false;
            }

            if (!(UserPermissions.IsAllowed("paypalAddress", "UPDATE")
                && UserPermissions.IsAllowed("paypalAddress", "INSERT")))
                PaypalEmailTextBox.IsEnabled = false;

            if (!(UserPermissions.IsAllowed("Users", "UPDATE")
                    && UserPermissions.IsAllowed("BillingAddress", "UPDATE")
                    && UserPermissions.IsAllowed("DeliveryAddress", "UPDATE")
                    && UserPermissions.IsAllowed("BillingAddress", "INSERT")
                    && UserPermissions.IsAllowed("DeliveryAddress", "INSERT")))
                Save_Button.IsEnabled = false;

            // Instead of hard coded items, the data could be pulled
            // asynchronously from a database or the internet.
            GetCountries(_connectionString);
            GetAddress(_connectionString);
        }

        public void GetCountries(string connectionString)
        {
            CountryORMResources.Countries.Clear();
            const string CountriesQuery = "SELECT id, name, phonecountrycode FROM Countries";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = CountriesQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var country = new Country
                                    {
                                        CountryID = reader.GetInt32(0),
                                        Name = reader.GetString(1),
                                        PhoneCountryCode = reader.GetString(2),
                                    };
                                    CountryORMResources.Countries.Add(country);
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

        public void GetAddress(string connectionString)
        {
            AddressORMResources.DeliveryAddresses.Clear();
            AddressORMResources.BillingAddresses.Clear();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetAddressQuery("DeliveryAddress");
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var address = new Address
                                    {
                                        Zipcode = !reader.IsDBNull(0) ? reader.GetString(0) : "",
                                        City = !reader.IsDBNull(1) ? reader.GetString(1) : "",
                                        Street = !reader.IsDBNull(2) ? reader.GetString(2) : "",
                                        State = !reader.IsDBNull(3) ? reader.GetString(3) : "",
                                        AdditionnalInfo = !reader.IsDBNull(4) ? reader.GetString(4) : "",
                                        Lastname = !reader.IsDBNull(5) ? reader.GetString(5) : "",
                                        Firstname = !reader.IsDBNull(6) ? reader.GetString(6) : "",
                                        PhoneNumber = !reader.IsDBNull(7) ? reader.GetString(7) : "",
                                        PhoneCountryCode = CountryORMResources.Countries.Where(x => x.PhoneCountryCode == reader.GetString(8)).First(),
                                        CountryName = CountryORMResources.Countries.Where(x => x.Name == reader.GetString(9)).First(),
                                        AddressID = reader.GetInt32(10)
                                    };
                                    string r = reader.GetString(8);
                                    AddressORMResources.DeliveryAddresses.Add(address);
                                }
                            }
                        }
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetAddressQuery("BillingAddress");
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var address = new Address
                                    {
                                        Zipcode = !reader.IsDBNull(0) ? reader.GetString(0) : "",
                                        City = !reader.IsDBNull(1) ? reader.GetString(1) : "",
                                        Street = !reader.IsDBNull(2) ? reader.GetString(2) : "",
                                        State = !reader.IsDBNull(3) ? reader.GetString(3) : "",
                                        AdditionnalInfo = !reader.IsDBNull(4) ? reader.GetString(4) : "",
                                        Lastname = !reader.IsDBNull(5) ? reader.GetString(5) : "",
                                        Firstname = !reader.IsDBNull(6) ? reader.GetString(6) : "",
                                        PhoneNumber = !reader.IsDBNull(7) ? reader.GetString(7) : "",
                                        PhoneCountryCode = CountryORMResources.Countries.Where(x => x.PhoneCountryCode == reader.GetString(8)).First(),
                                        CountryName = CountryORMResources.Countries.Where(x => x.Name == reader.GetString(9)).First(),
                                        AddressID = reader.GetInt32(10)
                                    };
                                    AddressORMResources.BillingAddresses.Add(address);
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

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            const string queryStart = "SET XACT_ABORT ON;BEGIN TRANSACTION;";
            const string queryEnd = "COMMIT;";
            string query = "" + queryStart;

            query += string.Format("UPDATE Users SET Users.firstname = '{0}'," +
                "Users.lastname = '{1}'," +
                "Users.email = '{2}'," +
                "Users.id_UserTypes = {3}" +
                " WHERE Users.id = {4};", UserSelected.FirstName, UserSelected.LastName, 
                UserSelected.Email, UserSelected.UserType.UserTypeID, UserSelected.UserID);

            query = IsPaypalAllowed(UserSelected) ? query + GetQueryPaypal(UserSelected) : query;

            foreach(var address in AddressORMResources.DeliveryAddresses)
            {
                query += GetQueryAddresses("DeliveryAddress", address);
            }
            foreach (var address in AddressORMResources.BillingAddresses)
            {
                query += GetQueryAddresses("BillingAddress", address);
            }
            query += queryEnd;

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = query;
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
                _ = new MessageDialog("Changes saved successfully.").ShowAsync();
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                _ = new MessageDialog("Exception: " + eSql.Message).ShowAsync();
            }
        }

        private string GetQueryPaypal(User user)
        {
            if (user.PaypalEmail.IsRegistered)
            {
                return GetUpdatePaypal(user);
            }
            else
            {
                return GetInsertPaypal(user);
            }
        }

        private string GetInsertPaypal(User user)
        {
            string query = string.Format("INSERT INTO paypalAddress VALUES ('{0}',{1});", 
                user.PaypalEmail.Email, user.UserID);
            return query;
        }

        private string GetUpdatePaypal(User user)
        {
            string query = string.Format("UPDATE paypalAddress SET paypal = '{0}' WHERE id_Users = {1};",
                user.PaypalEmail.Email, user.UserID);
            return query;
        }

        private string GetQueryAddresses(string table, Address address)
        {
            if (address.IsRegistered)
            {
                return GetUpdateQueryAddresses(table, address);
            }
            else
            {
                return GetInsertQueryAddresses(table, address);
            }
        }

        private string GetUpdateQueryAddresses(string table, Address address)
        {
            string query = string.Format("UPDATE {0} SET {0}.zipcode = '{1}', " +
                    "{0}.id_Countries = {2}," +
                    "{0}.city = '{3}'," +
                    "{0}.address = '{4}'," +
                    "{0}.state = '{5}'," +
                    "{0}.additionnalInfo = '{6}'," +
                    "{0}.lastname = '{7}'," +
                    "{0}.firstname = '{8}'," +
                    "{0}.phonenumber = '{9}'," +
                    "{0}.phonecountrycode = '{10}'" +
                    " WHERE {0}.id = {11};",
                    table,
                    address.Zipcode,
                    address.CountryName.CountryID,
                    address.City,
                    address.Street,
                    address.State,
                    address.AdditionnalInfo,
                    address.Lastname,
                    address.Firstname,
                    address.PhoneNumber,
                    address.PhoneCountryCode.PhoneCountryCode,
                    address.AddressID);
            return query;
        }

        private string GetInsertQueryAddresses(string table, Address address)
        {
            string query = string.Format("INSERT INTO {0} VALUES ('{1}','{2}','{3}','{4}','{5}'," +
                    "'{6}','{7}','{8}','{9}',{10},{11});",
                    table,
                    address.Zipcode,
                    address.City,
                    address.Street,
                    address.State,
                    address.AdditionnalInfo,
                    address.Lastname,
                    address.Firstname,
                    address.PhoneNumber,
                    address.PhoneCountryCode.PhoneCountryCode,
                    address.CountryName.CountryID,
                    address.UserID);
            return query;
        }

        private void Quit_Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HomePage));
        }

        private void Add_Address_Button_Click(object sender, RoutedEventArgs e)
        {
            Address new_address = new Address
            {
                UserID = UserSelected.UserID,
                IsRegistered = false
            };
            if (((Button)sender).Name == "AddDeliveryAddress")
            {
                AddressORMResources.DeliveryAddresses.Add(new_address);
            }
            else if (((Button)sender).Name == "AddBillingAddress")
            {
                AddressORMResources.BillingAddresses.Add(new_address);
            }
        }

        private void Delete_Address_Button_Click(object sender, RoutedEventArgs e)
        {
            string table = null;
            Address address = null;
            ObservableCollection<Address> collection = null;
            if (((Button)sender).Name == "DeleteDeliveryAddress")
            {
                table = "DeliveryAddress";
                address = DeliveryAddressListView.SelectedItem as Address;
                collection = AddressORMResources.DeliveryAddresses;
            }
            else if (((Button)sender).Name == "DeleteBillingAddress")
            {
                table = "BillingAddress";
                address = BillingAddressListView.SelectedItem as Address;
                collection = AddressORMResources.BillingAddresses;
            }
            ((Button)sender).Focus(FocusState.Pointer); //made the button get focus.
            string SetDeleteQuery = "SET XACT_ABORT ON;" +
                                    "BEGIN TRANSACTION;" +
                                    string.Format("DELETE FROM {0} WHERE {0}.id = {1};", 
                                    table, address.AddressID)+
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
                collection.Remove(address);
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                _ = new MessageDialog("Exception: " + eSql.Message).ShowAsync();
            }
        }

        private void AddressListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listview = sender as ListView;
            if (listview.Name == "DeliveryAddressListView")
                DeleteDeliveryAddress.IsEnabled = listview.SelectedItem != null 
                    && UserPermissions.IsAllowed("DeliveryAddress", "DELETE");
            else if (listview.Name == "BillingAddressListView")
                DeleteBillingAddress.IsEnabled = listview.SelectedItem != null
                    && UserPermissions.IsAllowed("BillingAddress", "DELETE");
        }
    }
}
