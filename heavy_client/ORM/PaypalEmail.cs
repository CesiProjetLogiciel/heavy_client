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
    public static class PaypalEmailORMResources
    {

    }

    public class PaypalEmail : INotifyPropertyChanged
    {
        private string _email;

        public int? PaypalEmailID { get; set; }
        public bool IsRegistered { get; set; } = false;
        public string Email
        {
            get { return _email; }
            set { if (_email != value) { _email = value; NotifyPropertyChanged(); } }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}