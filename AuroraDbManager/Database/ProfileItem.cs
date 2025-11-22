// 
// 	ProfileItem.cs
// 	AuroraDbManager
// 
// 	Created by Swizzy on 25/05/2015
// 	Copyright (c) 2015 Swizzy. All rights reserved.

namespace AuroraDbManager.Database {
    using System.Data;

    public class ProfileItem {
        public ProfileItem(DataRow row) { DataRow = row; }

        internal DataRow DataRow { get; private set; }

        public bool Changed { get; set; }

        public int Id { get { return (int)((long)DataRow["Id"]); } }

        public string GameTag {
            get { return (string)DataRow["Gametag"]; }
            set {
                Changed = true;
                DataRow["Gametag"] = value;
            }
        }

        public string Xuid {
            get { return (string)DataRow["Xuid"]; }
            set {
                Changed = true;
                DataRow["Xuid"] = value;
            }
        }
    }
}