using System;
using System.Data;

namespace AuroraDbManager.Database {
    internal class TrainerItem {
        private readonly DataRow _row;

        public TrainerItem(DataRow row) {
            _row = row;
        }

        public int Id { get { return int.Parse(_row["Id"].ToString()); } }
        
        public string TitleId { 
            get { return _row["TitleId"].ToString(); }
            set {
                _row["TitleId"] = value;
                Changed = true;
            }
        }
        
        public string MediaId { 
            get { return _row["MediaId"].ToString(); }
            set {
                _row["MediaId"] = value;
                Changed = true;
            }
        }
        
        public string TrainerPath { 
            get { return _row["TrainerPath"].ToString(); }
            set {
                _row["TrainerPath"] = value;
                Changed = true;
            }
        }
        
        public string TrainerName { 
            get { return _row["TrainerName"].ToString(); }
            set {
                _row["TrainerName"] = value;
                Changed = true;
            }
        }
        
        public int TrainerVersion { 
            get { return int.Parse(_row["TrainerVersion"].ToString()); }
            set {
                _row["TrainerVersion"] = value;
                Changed = true;
            }
        }
        
        public string TrainerData { 
            get { return _row["TrainerData"].ToString(); }
            set {
                _row["TrainerData"] = value;
                Changed = true;
            }
        }
        
        public string TrainerInfo { 
            get { return _row["TrainerInfo"].ToString(); }
            set {
                _row["TrainerInfo"] = value;
                Changed = true;
            }
        }
        
        public string TrainerAuthor { 
            get { return _row["TrainerAuthor"].ToString(); }
            set {
                _row["TrainerAuthor"] = value;
                Changed = true;
            }
        }
        
        public int TrainerRating { 
            get { return int.Parse(_row["TrainerRating"].ToString()); }
            set {
                _row["TrainerRating"] = value;
                Changed = true;
            }
        }
        
        public int TrainerFlags { 
            get { return int.Parse(_row["TrainerFlags"].ToString()); }
            set {
                _row["TrainerFlags"] = value;
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
        
        public bool Changed { get; set; }
    }
}