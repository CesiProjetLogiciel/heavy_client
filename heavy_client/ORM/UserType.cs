using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace heavy_client
{
    public static class UserTypeORMResources
    {
        private static ObservableCollection<UserType> _userTypes = new ObservableCollection<UserType>();
        public static ObservableCollection<UserType> UserTypes { get => _userTypes; set => _userTypes = value; }
    }

    public class UserType : INotifyPropertyChanged
    {
        private string _type;

        public int UserTypeID { get; set; }
        public string Type
        {
            get { return _type; }
            set { if (_type != value) { _type = value; NotifyPropertyChanged(); } }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
    public class UserTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
        object parameter, string language)
        {
            if (value != null)
            {
                if (value.GetType() == typeof(ObservableCollection<UserType>))
                {
                    ObservableCollection<string> userTypeString = new ObservableCollection<string>();
                    foreach (UserType userType in (ObservableCollection<UserType>)value)
                    {
                        userTypeString.Add(((UserType)userType).Type);
                    }
                    return userTypeString;
                }
                else
                {
                    return ((UserType)value).Type;
                }
            }
            else
            {
                return null;
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
                    return UserTypeORMResources.UserTypes.Where(x => x.Type == (string)value).First();
                }
            }
            else
            {
                return null;
            }
        }
    }
}