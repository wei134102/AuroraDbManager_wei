// 
// 	UserSettingItem.cs
// 	AuroraDbManager
// 
// 	Created by Swizzy on 25/05/2015
// 	Copyright (c) 2015 Swizzy. All rights reserved.

namespace AuroraDbManager.Database {
    using System.Data;

    public class UserSettingItem {
        public UserSettingItem(DataRow row) { DataRow = row; }

        internal DataRow DataRow { get; private set; }

        public bool Changed { get; set; }

        public int Id { get { return (int)((long)DataRow["Id"]); } }

        public string Name {
            get { return (string)DataRow["Name"]; }
            set {
                Changed = true;
                DataRow["Name"] = value;
            }
        }

        public string Value {
            get { return (string)DataRow["Value"]; }
            set {
                Changed = true;
                DataRow["Value"] = value;
            }
        }

        public string ProfileId {
            get { return (string)DataRow["ProfileId"]; }
            set {
                Changed = true;
                DataRow["ProfileId"] = value;
            }
        }
    }
}