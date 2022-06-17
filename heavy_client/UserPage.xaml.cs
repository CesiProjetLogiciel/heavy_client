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

        private readonly string _connectionString = (App.Current as App).ConnectionString;
        private bool _changeIsSuspendedTo = true;

        public User UserSelected { get; set; }

        private ObservableCollection<Address> _deliveryAddresses = new ObservableCollection<Address>();
        private ObservableCollection<Address> _billingAddresses = new ObservableCollection<Address>();
        public ObservableCollection<Address> DeliveryAddresses { get => _deliveryAddresses; set => _deliveryAddresses = value; }
        public ObservableCollection<Address> BillingAddresses { get => _billingAddresses; set => _billingAddresses = value; }
        
        private ObservableCollection<Country> _countries = new ObservableCollection<Country>();
        public ObservableCollection<Country> Countries { get => _countries; set => _countries = value; }


        string GetAddressQuery (string table)
            {
                return String.Format("SELECT zipcode, city, address, state, " +
                "additionnalInfo, t1.lastname, t1.firstname, phonenumber, t1.phonecountrycode, t1.countryname " +
                "FROM (SELECT zipcode, city, address, state, additionnalInfo, {0}.lastname, {0}.firstname, " +
                "phonenumber, Countries.countryname, Countries.phonecountrycode, {0}.id_Users " +
                "FROM {0} INNER JOIN Countries ON  {0}.id_Countries = Countries.id) " +
                "AS t1 INNER JOIN Users ON {0}.id_Users = Users.id ", table);
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

            // Instead of hard coded items, the data could be pulled
            // asynchronously from a database or the internet.
            GetCountries(_connectionString);
        }

        public void GetCountries(string connectionString)
        {
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
                                    Countries.Add(country);
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
                                        Zipcode = reader.GetString(0),
                                        City = reader.GetString(1),
                                        Street = reader.GetString(2),
                                        State = reader.GetString(3),
                                        AdditionnalInfo = reader.GetString(4),
                                        Lastname = reader.GetString(5),
                                        Firstname = reader.GetString(6),
                                        PhoneNumber = reader.GetString(7),
                                        PhoneCountryCode = reader.GetString(8),
                                        CountryName = reader.GetString(9),
                                    };
                                    DeliveryAddresses.Add(address);
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
                                        Zipcode = reader.GetString(0),
                                        City = reader.GetString(1),
                                        Street = reader.GetString(2),
                                        State = reader.GetString(3),
                                        AdditionnalInfo = reader.GetString(4),
                                        Lastname = reader.GetString(5),
                                        Firstname = reader.GetString(6),
                                        PhoneNumber = reader.GetString(7),
                                        PhoneCountryCode = reader.GetString(8),
                                        CountryName = reader.GetString(9),
                                    };
                                    BillingAddresses.Add(address);
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

        private void DeliveryAddressListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }



    public class Customer : INotifyPropertyChanged
    {
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    this.OnPropertyChanged();
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
