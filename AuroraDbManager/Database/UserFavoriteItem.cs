using System;
using System.Data;

namespace AuroraDbManager.Database {
    internal class UserFavoriteItem {
        private readonly DataRow _row;

        public UserFavoriteItem(DataRow row) {
            _row = row;
        }

        public int Id { get { return int.Parse(_row["Id"].ToString()); } }
        
        public int ContentId { 
            get { return int.Parse(_row["ContentId"].ToString()); }
            set {
                _row["ContentId"] = value;
                Changed = true;
            }
        }
        
        public string ProfileId { 
            get { return _row["ProfileId"].ToString(); }
            set {
                _row["ProfileId"] = value;
                Changed = true;
            }
        }
        
        public bool Changed { get; set; }
    }
}