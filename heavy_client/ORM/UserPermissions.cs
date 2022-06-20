using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace heavy_client
{
    public static class UserPermissions
    {
        private static readonly List<string> _keys = new List<string>(){ "Users", "DeliveryAddress", "BillingAddress", "Countries", 
            "paypalAddress", "UserTypes", "Users.isSuspended" };

        private static readonly List<string> _permissions = new List<string>(){ "SELECT", "DELETE", "UPDATE", "INSERT" };

        public const string query = "EXEC dbo.GetPermissions;";

        public static Dictionary<string, List<string>> Permissions = new Dictionary<string, List<string>>();

        public static Dictionary<string, List<string>> PopulatePermissions(string connectionString, string query)
        {
            Dictionary<string, List<string>> permissions = new Dictionary<string, List<string>>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
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
                                    string key = reader.IsDBNull(8) ? reader.GetString(7) : reader.GetString(7) + "." + reader.GetString(8);
                                    if (!permissions.ContainsKey(key))
                                        permissions.Add(key, new List<string>());
                                    permissions[key].Add(reader.GetString(4));
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
            return permissions;
        }

        public static Dictionary<string, List<string>> PopulatePermissionsSA()
        {
            Dictionary<string, List<string>> permissions = new Dictionary<string, List<string>>();
            foreach (string key in _keys)
            {
                permissions.Add(key, new List<string>());
                foreach (string permission in _permissions)
                    permissions[key].Add(permission);
            }
            return permissions;
        }

        public static bool IsAllowed(string key, string value)
        {
            return Permissions.ContainsKey(key) && Permissions[key].Contains(value);
        }
    }
}
