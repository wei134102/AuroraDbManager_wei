using System;
using System.Data;

namespace AuroraDbManager.Database {
    internal class UserFavoriteItem {
        public UserFavoriteItem(DataRow row) { DataRow = row; }

        internal DataRow DataRow { get; private set; }

        public bool Changed { get; set; }

        public int Id {
            get { return (int)(long)DataRow["Id"]; }
            set { DataRow["Id"] = value; }
        }
        
        public int ContentId { 
            get { return int.Parse(DataRow["ContentId"].ToString()); }
            set {
                DataRow["ContentId"] = value;
                Changed = true;
            }
        }
        
        public string ProfileId { 
            get { return DataRow["ProfileId"].ToString(); }
            set {
                DataRow["ProfileId"] = value;
                Changed = true;
            }
        }
    }
}