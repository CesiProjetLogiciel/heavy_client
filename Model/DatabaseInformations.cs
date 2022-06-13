using System;

namespace Model {
	public class DatabaseInformations {
		private string login;
		private string password;
		private string dbAddress;
		private int dbPort;

		public void StoreCredentials(ref string login, ref string password) {
			throw new System.NotImplementedException("Not implemented");
		}
		public string GetConnectionURI() {
			throw new System.NotImplementedException("Not implemented");
		}
		public DatabaseInformations() {
			throw new System.NotImplementedException("Not implemented");
		}

	}

}
