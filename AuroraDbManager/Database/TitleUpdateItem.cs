// 
// 	TitleUpdateItem.cs
// 	AuroraDbManager
// 
// 	Created by Swizzy on 25/05/2015
// 	Copyright (c) 2015 Swizzy. All rights reserved.

namespace AuroraDbManager.Database {
    using System.Data;

    public class TitleUpdateItem {
        public TitleUpdateItem(DataRow row) { DataRow = row; }

        internal DataRow DataRow { get; private set; }

        internal bool Changed { get; set; }

        public int Id { 
            get { 
                var value = DataRow["Id"];
                return value is long ? (int)(long)value : System.Convert.ToInt32(value);
            } 
        }

        public string DisplayName {
            get { 
                var value = DataRow["DisplayName"];
                return value?.ToString() ?? string.Empty;
            }
            set {
                Changed = true;
                DataRow["DisplayName"] = value;
            }
        }

        public int TitleId {
            get { 
                var value = DataRow["TitleId"];
                return value is long ? (int)(long)value : System.Convert.ToInt32(value);
            }
            set {
                Changed = true;
                DataRow["TitleId"] = value;
            }
        }

        public int MediaId {
            get { 
                var value = DataRow["MediaId"];
                return value is long ? (int)(long)value : System.Convert.ToInt32(value);
            }
            set {
                Changed = true;
                DataRow["MediaId"] = value;
            }
        }

        public int BaseVersion {
            get { 
                var value = DataRow["BaseVersion"];
                return value is long ? (int)(long)value : System.Convert.ToInt32(value);
            }
            set {
                Changed = true;
                DataRow["BaseVersion"] = value;
            }
        }

        public int Version {
            get { 
                var value = DataRow["Version"];
                return value is long ? (int)(long)value : System.Convert.ToInt32(value);
            }
            set {
                Changed = true;
                DataRow["Version"] = value;
            }
        }

        public string FileName {
            get { 
                var value = DataRow["FileName"];
                return value?.ToString() ?? string.Empty;
            }
            set {
                Changed = true;
                DataRow["FileName"] = value;
            }
        }

        public string FileSize { 
            get { 
                var value = DataRow["FileSize"];
                return value?.ToString() ?? string.Empty;
            } 
        }

        public string LiveDeviceId { 
            get { 
                var value = DataRow["LiveDeviceId"];
                return value?.ToString() ?? string.Empty;
            } 
        }

        public string LivePath {
            get { 
                var value = DataRow["LivePath"];
                return value?.ToString() ?? string.Empty;
            }
            set {
                Changed = true;
                DataRow["LivePath"] = value;
            }
        }

        public string BackupPath { 
            get { 
                var value = DataRow["BackupPath"];
                return value?.ToString() ?? string.Empty;
            } 
        }

        public string Hash { 
            get { 
                var value = DataRow["Hash"];
                return value?.ToString() ?? string.Empty;
            } 
        }
    }
}