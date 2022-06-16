using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace heavy_client
{
    public class Address : INotifyPropertyChanged
    {
        private string _zipcode;
        private string _city;
        private string _street;
        private string _state;
        private string _additionnalInfo;
        private string _lastname;
        private string _firstname;
        private string _phoneNumber;
        private string _countryName;
        private string _phoneCountryCode;

        public int UserID { get; set; }
        public string Zipcode
        {
            get { return _zipcode; }
            set { if (_zipcode != value) { _zipcode = value; NotifyPropertyChanged(); } }
        }
        public string City
        {
            get { return _city; }
            set { if (_city != value) { _city = value; NotifyPropertyChanged(); } }
        }
        public string Street
        {
            get { return _street; }
            set { if (_street != value) { _street = value; NotifyPropertyChanged(); } }
        }
        public string State 
        {
            get { return _state; }
            set { if (_state != value) { _state = value; NotifyPropertyChanged(); } }
        }
        public string AdditionnalInfo
        {
            get { return _additionnalInfo; }
            set { if (_additionnalInfo != value) { _additionnalInfo = value; NotifyPropertyChanged(); } }
        }
        public string Lastname
        {
            get { return _lastname; }
            set { if (_lastname != value) { _lastname = value; NotifyPropertyChanged(); } }
        }
        public string Firstname
        {
            get { return _firstname; }
            set { if (_firstname != value) { _firstname = value; NotifyPropertyChanged(); } }
        }
        public string PhoneNumber
        {
            get { return _phoneNumber; }
            set { if (_phoneNumber != value) { _phoneNumber = value; NotifyPropertyChanged(); } }
        }
        public string CountryName
        {
            get { return _countryName; }
            set { if (_countryName != value) { _countryName = value; NotifyPropertyChanged(); } }
        }
        public string PhoneCountryCode
        {
            get { return _phoneCountryCode; }
            set { if (_phoneCountryCode != value) { _phoneCountryCode = value; NotifyPropertyChanged(); } }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}