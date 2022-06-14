using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace heavy_client.ORM
{
    public class User : INotifyPropertyChanged
    {
        public int UserID { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string email { get; set; }
        public bool isSuspended { get; set; }
        public int UserTypeId { get; set; }
        public string UserType { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
