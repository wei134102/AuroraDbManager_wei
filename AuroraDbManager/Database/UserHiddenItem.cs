using System;
using System.Data;

namespace AuroraDbManager.Database {
    /// <summary>
    /// 表示一个用户隐藏项
    /// </summary>
    public class UserHiddenItem {
        public UserHiddenItem(DataRow row) { DataRow = row; }

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