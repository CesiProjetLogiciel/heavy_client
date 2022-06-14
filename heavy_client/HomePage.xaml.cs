using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        public HomePage()
        {
            InitializeComponent();
            ApplicationView.PreferredLaunchViewSize = new Size(1280, 720);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }

        public ObservableCollection<ORM.User> GetProducts(SqlConnection conn)
        {
            const string GetProductsQuery = "select id, lastname, firstname," +
               " email, isSuspended, UserTypes.id, UserTypes.type " +
               " from Users inner join UserTypes on Users.id_UserTypes = UserTypes.id ";

            var users = new ObservableCollection<ORM.User>();
            try
            {
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = GetProductsQuery;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var user = new ORM.User();
                                user.UserID = reader.GetInt32(0);
                                user.LastName = reader.GetString(1);
                                user.FirstName = reader.GetString(2);
                                user.email = reader.GetString(3);
                                user.isSuspended = reader.GetBoolean(4);
                                user.UserTypeId = reader.GetInt32(5);
                                user.UserType = reader.GetString(6);
                                users.Add(user);
                            }
                        }
                    }
                }
                
                return users;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
            }
            return null;
        }
    }
}
