// 
// 	ContentItem.cs
// 	AuroraDbManager
// 
// 	Created by Swizzy on 25/05/2015
// 	Copyright (c) 2015 Swizzy. All rights reserved.

namespace AuroraDbManager.Database {
    using System;
    using System.Data;

    public class ContentItem {
        private long _offlineFlag;
        private long _onlineFlag;

        public ContentItem(DataRow row) {
            DataRow = row;
            _onlineFlag = (long)DataRow["GameCapsOnline"];
            _offlineFlag = (long)DataRow["GameCapsOffline"];
        }

        internal DataRow DataRow { get; private set; }

        public bool Changed { get; set; }

        public int BaseVersion {
            get { return (int)(long)DataRow["BaseVersion"]; }
            set {
                Changed = true;
                DataRow["BaseVersion"] = value;
            }
        }

        public DateTime DateAdded {
            get { 
                var value = DataRow["DateAdded"];
                if (value is long longValue) {
                    return DateTime.FromFileTime(longValue);
                }
                return DateTime.FromFileTime(Convert.ToInt64(value));
            }
            set {
                Changed = true;
                DataRow["DateAdded"] = value.ToFileTime();
            }
        }

        public string Directory {
            get { 
                var value = DataRow["Directory"];
                return value?.ToString() ?? string.Empty;
            }
            set {
                Changed = true;
                DataRow["Directory"] = value;
            }
        }

        public int DiscsInSet {
            get {
                var value = DataRow["DiscsInSet"];
                var ret = value is long ? (int)(long)value : Convert.ToInt32(value);
                return ret <= 0 ? 1 : ret;
            }
            set {
                Changed = true;
                DataRow["DiscsInSet"] = value;
            }
        }

        public int DiscNum {
            get {
                var value = DataRow["DiscNum"];
                var ret = value is long ? (int)(long)value : Convert.ToInt32(value);
                return ret <= 0 ? 1 : ret;
            }
            set {
                Changed = true;
                DataRow["DiscNum"] = value;
            }
        }

        public string DiscInfo { get { return string.Format("{0}/{1}", DiscNum, DiscsInSet); } }

        public string Executable { 
            get { 
                var value = DataRow["Executable"];
                return value?.ToString() ?? string.Empty;
            } 
        }

        public DbFlags.FileTypes FileType { 
            get { 
                var value = DataRow["FileType"];
                if (value is long longValue) {
                    return (DbFlags.FileTypes)longValue;
                }
                return (DbFlags.FileTypes)Convert.ToInt32(value);
            } 
        }

        public int FoundAtDepth { 
            get { 
                var value = DataRow["FoundAtDepth"];
                return value is long ? (int)(long)value : Convert.ToInt32(value);
            } 
        }

        public string Hash { 
            get { 
                var value = DataRow["Hash"];
                return value?.ToString() ?? string.Empty;
            } 
        }

        public int Id { 
            get { 
                var value = DataRow["Id"];
                return value is long ? (int)(long)value : Convert.ToInt32(value);
            } 
        }

        public int MediaId {
            get { 
                var value = DataRow["MediaId"];
                return value is long ? (int)(long)value : Convert.ToInt32(value);
            }
            set {
                Changed = true;
                DataRow["MediaId"] = value;
            }
        }

        public int ScanPathId { 
            get { 
                var value = DataRow["ScanPathId"];
                return value is long ? (int)(long)value : Convert.ToInt32(value);
            } 
        }

        public bool SystemLink {
            get { 
                var value = DataRow["SystemLink"];
                if (value is long longValue) {
                    return longValue == 1;
                }
                return Convert.ToInt64(value) == 1;
            }
            set {
                Changed = true;
                DataRow["SystemLink"] = value ? 1 : 0;
            }
        }

        public int TitleId {
            get { 
                var value = DataRow["TitleId"];
                return value is long ? (int)(long)value : Convert.ToInt32(value);
            }
            set {
                Changed = true;
                DataRow["TitleId"] = value;
            }
        }

        public DbFlags.ContentFlags ContentFlags {
            get { 
                var value = DataRow["ContentFlags"];
                if (value is long longValue) {
                    return (DbFlags.ContentFlags)longValue;
                }
                return (DbFlags.ContentFlags)Convert.ToInt32(value);
            }
            set {
                Changed = true;
                DataRow["ContentFlags"] = (int)value;
            }
        }

        public DbFlags.ContentGroups ContentGroup {
            get { 
                var value = DataRow["ContentGroup"];
                if (value is long longValue) {
                    return (DbFlags.ContentGroups)longValue;
                }
                return (DbFlags.ContentGroups)Convert.ToInt32(value);
            }
            set {
                Changed = true;
                DataRow["ContentGroup"] = (int)value;
            }
        }

        public DbFlags.ContentTypes ContentType {
            get { 
                var value = DataRow["ContentType"];
                if (value is long longValue) {
                    return (DbFlags.ContentTypes)longValue;
                }
                return (DbFlags.ContentTypes)Convert.ToInt32(value);
            }
            set {
                Changed = true;
                DataRow["ContentType"] = (int)value;
            }
        }

        public DbFlags.ContentGroups DefaultGroup {
            get { 
                var value = DataRow["DefaultGroup"];
                if (value is long longValue) {
                    return (DbFlags.ContentGroups)longValue;
                }
                return (DbFlags.ContentGroups)Convert.ToInt32(value);
            }
            set {
                Changed = true;
                DataRow["DefaultGroup"] = (int)value;
            }
        }

        public string Description {
            get { 
                var value = DataRow["Description"];
                return value?.ToString() ?? string.Empty;
            }
            set {
                Changed = true;
                DataRow["Description"] = value;
            }
        }

        public string Developer {
            get { 
                var value = DataRow["Developer"];
                return value?.ToString() ?? string.Empty;
            }
            set {
                Changed = true;
                DataRow["Developer"] = value;
            }
        }

        public DbFlags.GameCapsFlags GameCapsFlags {
            get { 
                var value = DataRow["GameCapsFlags"];
                if (value is long longValue) {
                    return (DbFlags.GameCapsFlags)longValue;
                }
                return (DbFlags.GameCapsFlags)Convert.ToInt32(value);
            }
            set {
                Changed = true;
                DataRow["GameCapsFlags"] = (int)value;
            }
        }

        public DbFlags.GenreFlags GenreFlag {
            get { 
                var value = DataRow["GenreFlag"];
                if (value is long longValue) {
                    return (DbFlags.GenreFlags)longValue;
                }
                return (DbFlags.GenreFlags)Convert.ToInt32(value);
            }
            set {
                Changed = true;
                DataRow["GenreFlag"] = (int)value;
            }
        }

        public int LiveRaters {
            get { 
                var value = DataRow["LiveRaters"];
                return value is long ? (int)(long)value : Convert.ToInt32(value);
            }
            set {
                Changed = true;
                DataRow["LiveRaters"] = value;
            }
        }

        public double LiveRating {
            get { 
                var value = DataRow["LiveRating"];
                if (value is double doubleValue) {
                    return doubleValue;
                }
                return Convert.ToDouble(value);
            }
            set {
                Changed = true;
                DataRow["LiveRating"] = value;
            }
        }

        public string Publisher {
            get { 
                var value = DataRow["Publisher"];
                return value?.ToString() ?? string.Empty;
            }
            set {
                Changed = true;
                DataRow["Publisher"] = value;
            }
        }

        public string ReleaseDate {
            get { 
                var value = DataRow["ReleaseDate"];
                return value?.ToString() ?? string.Empty;
            }
            set {
                Changed = true;
                DataRow["ReleaseDate"] = value;
            }
        }

        public string TitleName {
            get { 
                var value = DataRow["TitleName"];
                return value?.ToString() ?? string.Empty;
            }
            set {
                Changed = true;
                DataRow["TitleName"] = value;
            }
        }

        public string OnlineMultiplayerPlayers { get { return string.Format("{0} - {1}", MinimumOnlineMultiplayerPlayers, MaximumOnlineMultiplayerPlayers); } }

        public string OnlineCoOpPlayers { get { return string.Format("{0} - {1}", MinimumOnlineCoOpPlayers, MaximumOnlineCoOpPlayers); } }

        public string OfflinePlayers { get { return string.Format("{0} - {1}", MinimumOfflinePlayers, MaximumOfflinePlayers); } }

        public string OfflineCoOpPlayers { get { return string.Format("{0} - {1}", MinimumOfflineCoOpPlayers, MaximumOfflineCoOpPlayers); } }

        public string OfflineSystemLinkPlayers { get { return string.Format("{0} - {1}", MinimumOfflineSystemLinkPlayers, MaximumOfflineSystemLinkPlayers); } }

        public byte MaximumOnlineCoOpPlayers { get { return (byte)(_onlineFlag & 0xFF); } set { SetOnlineFlag(MaximumOnlineCoOpPlayers, value); } }

        public byte MinimumOnlineCoOpPlayers { get { return (byte)((_onlineFlag >> 8) & 0xFF); } set { SetOnlineFlag(MinimumOnlineCoOpPlayers, (long)value << 8); } }

        public byte MaximumOnlineMultiplayerPlayers { get { return (byte)((_onlineFlag >> 24) & 0xFF); } set { SetOnlineFlag(MaximumOnlineMultiplayerPlayers, (long)value << 24); } }

        public byte MinimumOnlineMultiplayerPlayers { get { return (byte)((_onlineFlag >> 16) & 0xFF); } set { SetOnlineFlag(MinimumOnlineMultiplayerPlayers, (long)value << 16); } }

        public byte MinimumOfflineSystemLinkPlayers { get { return (byte)((_offlineFlag >> 40) & 0xFF); } set { SetOfflineFlag(MinimumOfflineSystemLinkPlayers, (long)value << 40); } }

        public byte MaximumOfflineSystemLinkPlayers { get { return (byte)((_offlineFlag >> 40) & 0xFF); } set { SetOfflineFlag(MaximumOfflineSystemLinkPlayers, (long)value << 40); } }

        public byte MinimumOfflineCoOpPlayers { get { return (byte)((_offlineFlag >> 16) & 0xFF); } set { SetOfflineFlag(MinimumOfflineCoOpPlayers, (long)value << 16); } }

        public byte MaximumOfflineCoOpPlayers { get { return (byte)((_offlineFlag >> 24) & 0xFF); } set { SetOfflineFlag(MaximumOfflineCoOpPlayers, (long)value << 24); } }

        public byte MinimumOfflinePlayers {
            get {
                var ret = _offlineFlag & 0xFF;
                return (byte)(ret <= 0 ? 1 : ret);
            }
            set { SetOfflineFlag(MinimumOfflinePlayers, value); }
        }

        public byte MaximumOfflinePlayers {
            get {
                var ret = (_offlineFlag >> 8) & 0xFF;
                return (byte)(ret <= 0 ? 1 : ret);
            }
            set { SetOfflineFlag(MaximumOfflinePlayers, (long)value << 8); }
        }

        private void SetOnlineFlag(long removeMask, long addMask) {
            Changed = true;
            _onlineFlag &= ~removeMask; // Remove old data
            DataRow["GameCapsOnline"] = _onlineFlag |= addMask; // Set new data;
        }

        private void SetOfflineFlag(long removeMask, long addMask) {
            Changed = true;
            _offlineFlag &= ~removeMask; // Remove old data
            DataRow["GameCapsOffline"] = _offlineFlag |= addMask; // Set new data;
        }
    }
}