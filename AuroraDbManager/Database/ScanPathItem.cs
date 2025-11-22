// 
// 	ScanPathItem.cs
// 	AuroraDbManager
// 
// 	Created by Swizzy on 25/05/2015
// 	Copyright (c) 2015 Swizzy. All rights reserved.

namespace AuroraDbManager.Database {
    using System.Data;

    public class ScanPathItem {
        public ScanPathItem(DataRow row) { DataRow = row; }

        internal DataRow DataRow { get; private set; }

        public bool Changed { get; set; }

        public int Id { get { return (int)((long)DataRow["Id"]); } }

        public string Path {
            get { return (string)DataRow["Path"]; }
            set {
                Changed = true;
                DataRow["Path"] = value;
            }
        }

        public string DeviceID {
            get { return (string)DataRow["DeviceId"]; }
            set {
                Changed = true;
                DataRow["DeviceId"] = value;
            }
        }

        public int Depth {
            get { return (int)((long)DataRow["Depth"]); }
            set {
                Changed = true;
                DataRow["Depth"] = value;
            }
        }

        public string ScriptData {
            get { return (string)DataRow["ScriptData"]; }
            set {
                Changed = true;
                DataRow["ScriptData"] = value;
            }
        }

        public long OptionsFlag {
            get { return (long)DataRow["OptionsFlag"]; }
            set {
                Changed = true;
                DataRow["OptionsFlag"] = value;
            }
        }
    }
}