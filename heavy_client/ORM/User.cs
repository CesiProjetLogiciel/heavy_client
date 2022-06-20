using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace heavy_client
{
    public static class UserORMResources
    {
        private static ObservableCollection<User> _users = new ObservableCollection<User>();
        public static ObservableCollection<User> Users { get => _users; set => _users = value; }
    }

    public class User : INotifyPropertyChanged
    {
        private string _lastName;
        private string _firstName;
        private string _email;
        private bool _isSuspended;
        private UserType _userType;
        private PaypalEmail _paypalEmail = new PaypalEmail();

        public int UserID { get; set; }
        public PaypalEmail PaypalEmail
        {
            get { return _paypalEmail; }
            set { if (_paypalEmail != value) { _paypalEmail = value; NotifyPropertyChanged(); } }
        }
        public string LastName
        {
            get { return _lastName; }
            set { if (_lastName != value) { _lastName = value; NotifyPropertyChanged(); } }
        }
        public string FirstName
        {
            get { return _firstName; }
            set { if (_firstName != value) { _firstName = value; NotifyPropertyChanged(); } }
        }
        public string Email
        {
            get { return _email; }
            set { if (_email != value) { _email = value; NotifyPropertyChanged(); } }
        }
        public bool IsSuspended 
        {
            get { return _isSuspended; }
            set { if (_isSuspended != value) { _isSuspended = value; NotifyPropertyChanged(); } }
        }
        public UserType UserType
        {
            get { return _userType; }
            set { if (_userType != value) { _userType = value; NotifyPropertyChanged(); } }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}