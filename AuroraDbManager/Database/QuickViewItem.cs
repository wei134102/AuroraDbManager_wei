using System;
using System.Data;

namespace AuroraDbManager.Database {
    internal class QuickViewItem {
        public QuickViewItem(DataRow row) { DataRow = row; }

        internal DataRow DataRow { get; private set; }

        public bool Changed { get; set; }

        public int Id { get { return int.Parse(DataRow["Id"].ToString()); } }
        
        public string DisplayName { 
            get { return DataRow["DisplayName"].ToString(); }
            set {
                DataRow["DisplayName"] = value;
                Changed = true;
            }
        }
        
        public string SortMethod { 
            get { return DataRow["SortMethod"].ToString(); }
            set {
                DataRow["SortMethod"] = value;
                Changed = true;
            }
        }
        
        public string FilterMethod { 
            get { return DataRow["FilterMethod"].ToString(); }
            set {
                DataRow["FilterMethod"] = value;
                Changed = true;
            }
        }
        
        public int Flags { 
            get { return int.Parse(DataRow["Flags"].ToString()); }
            set {
                DataRow["Flags"] = value;
                Changed = true;
            }
        }
        
        public string CreatorXUID { 
            get { return DataRow["CreatorXUID"].ToString(); }
            set {
                DataRow["CreatorXUID"] = value;
                Changed = true;
            }
        }
        
        public int OrderIndex { 
            get { return int.Parse(DataRow["OrderIndex"].ToString()); }
            set {
                DataRow["OrderIndex"] = value;
                Changed = true;
            }
        }
        
        public string IconHash { 
            get { return DataRow["IconHash"].ToString(); }
            set {
                DataRow["IconHash"] = value;
                Changed = true;
            }
        }
    }
}