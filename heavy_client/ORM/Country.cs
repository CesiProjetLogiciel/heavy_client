using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace heavy_client
{
    public static class CountryORMResources
    {
        private static ObservableCollection<Country> _countries = new ObservableCollection<Country>();
        public static ObservableCollection<Country> Countries { get => _countries; set => _countries = value; }
    }

    public class Country : INotifyPropertyChanged
    {
        private string _phoneCountryCode;
        private string _name;

        public int CountryID { get; set; }
        public string PhoneCountryCode
        {
            get { return _phoneCountryCode; }
            set { if (_phoneCountryCode != value) { _phoneCountryCode = value; NotifyPropertyChanged(); } }
        }
        public string Name
        {
            get { return _name; }
            set { if (_name != value) { _name = value; NotifyPropertyChanged(); } }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }



    public class CountryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
        object parameter, string language)
        {
            if (value != null)
            {
                if (value.GetType() == typeof(ObservableCollection<Country>))
                {
                    ObservableCollection<string> countriesString = new ObservableCollection<string>();
                    foreach (Country country in (ObservableCollection<Country>)value)
                    {
                        countriesString.Add(CountrytoString(country, parameter));
                    }
                    return countriesString;
                }
                else
                {
                    return CountrytoString((Country)value, parameter);
                }
            }
            else
            {
                return null;
            }
        }

        private string CountrytoString(Country country, object parameter)
        {
            if (parameter.ToString().ToLower() == "name")
            {
                return country.Name;
            }
            else if (parameter.ToString().ToLower() == "phonecountrycode")
            {
                return country.PhoneCountryCode;
            }
            else
            {
                return string.Format("({0}) {1}", country.PhoneCountryCode, country.Name);
            }
        }

        private Country StringtoCountry(string country, object parameter)
        {
            if (parameter.ToString().ToLower() == "name")
            {
                return CountryORMResources.Countries.Where(x => x.Name == country).First();
            }
            else if (parameter.ToString().ToLower() == "phonecountrycode")
            {
                return CountryORMResources.Countries.Where(x => x.PhoneCountryCode == country).First();
            }
            else
            {
                string pattern = new Regex(@"(?<=\()(.*?)(?=\))").Match(country).Groups[0].Value;
                return StringtoCountry(pattern, "phonecountrycode");
            }
        }

        // ConvertBack is not implemented for a OneWay binding.
        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            if (value != null)
            {
                if (value.GetType() == typeof(ObservableCollection<string>))
                {
                    return CountryORMResources.Countries;
                }
                else
                {
                    return StringtoCountry((string)value, parameter);
                }
            }
            else
            {
                return null;
            }
        }
    }
}