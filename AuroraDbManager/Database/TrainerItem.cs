using System;
using System.Data;

namespace AuroraDbManager.Database {
    /// <summary>
    /// 表示一个训练器项
    /// </summary>
    public class TrainerItem {
        public TrainerItem(DataRow row) { DataRow = row; }

        internal DataRow DataRow { get; private set; }

        public bool Changed { get; set; }

        public int Id {
            get { return (int)(long)DataRow["Id"]; }
            set { DataRow["Id"] = value; }
        }
        
        public string TitleId { 
            get { return DataRow["TitleId"].ToString(); }
            set {
                DataRow["TitleId"] = value;
                Changed = true;
            }
        }
        
        public string MediaId { 
            get { return DataRow["MediaId"].ToString(); }
            set {
                DataRow["MediaId"] = value;
                Changed = true;
            }
        }
        
        public string TrainerPath { 
            get { return DataRow["TrainerPath"].ToString(); }
            set {
                DataRow["TrainerPath"] = value;
                Changed = true;
            }
        }
        
        public string TrainerName { 
            get { return DataRow["TrainerName"].ToString(); }
            set {
                DataRow["TrainerName"] = value;
                Changed = true;
            }
        }
        
        public int TrainerVersion { 
            get { return int.Parse(DataRow["TrainerVersion"].ToString()); }
            set {
                DataRow["TrainerVersion"] = value;
                Changed = true;
            }
        }
        
        public string TrainerData { 
            get { return DataRow["TrainerData"].ToString(); }
            set {
                DataRow["TrainerData"] = value;
                Changed = true;
            }
        }
        
        public string TrainerInfo { 
            get { return DataRow["TrainerInfo"].ToString(); }
            set {
                DataRow["TrainerInfo"] = value;
                Changed = true;
            }
        }
        
        public string TrainerAuthor { 
            get { return DataRow["TrainerAuthor"].ToString(); }
            set {
                DataRow["TrainerAuthor"] = value;
                Changed = true;
            }
        }
        
        public int TrainerRating { 
            get { return int.Parse(DataRow["TrainerRating"].ToString()); }
            set {
                DataRow["TrainerRating"] = value;
                Changed = true;
            }
        }
        
        public int TrainerFlags { 
            get { return int.Parse(DataRow["TrainerFlags"].ToString()); }
            set {
                DataRow["TrainerFlags"] = value;
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
    }
}