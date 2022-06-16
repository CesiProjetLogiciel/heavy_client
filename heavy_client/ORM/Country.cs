using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace heavy_client
{
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

        public Country(int countryID, string phoneCountryCode, string name)
        {
            PhoneCountryCode = phoneCountryCode;
            Name = name;
            CountryID = countryID;
            PhoneCountryCode = phoneCountryCode;
            Name = name;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}