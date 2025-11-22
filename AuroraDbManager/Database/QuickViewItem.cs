using System;
using System.Data;

namespace AuroraDbManager.Database {
    internal class QuickViewItem {
        private readonly DataRow _row;

        public QuickViewItem(DataRow row) {
            _row = row;
        }

        public int Id { get { return int.Parse(_row["Id"].ToString()); } }
        
        public string DisplayName { 
            get { return _row["DisplayName"].ToString(); }
            set {
                _row["DisplayName"] = value;
                Changed = true;
            }
        }
        
        public string SortMethod { 
            get { return _row["SortMethod"].ToString(); }
            set {
                _row["SortMethod"] = value;
                Changed = true;
            }
        }
        
        public string FilterMethod { 
            get { return _row["FilterMethod"].ToString(); }
            set {
                _row["FilterMethod"] = value;
                Changed = true;
            }
        }
        
        public int Flags { 
            get { return int.Parse(_row["Flags"].ToString()); }
            set {
                _row["Flags"] = value;
                Changed = true;
            }
        }
        
        public string CreatorXUID { 
            get { return _row["CreatorXUID"].ToString(); }
            set {
                _row["CreatorXUID"] = value;
                Changed = true;
            }
        }
        
        public int OrderIndex { 
            get { return int.Parse(_row["OrderIndex"].ToString()); }
            set {
                _row["OrderIndex"] = value;
                Changed = true;
            }
        }
        
        public string IconHash { 
            get { return _row["IconHash"].ToString(); }
            set {
                _row["IconHash"] = value;
                Changed = true;
            }
        }
        
        public bool Changed { get; set; }
    }
}