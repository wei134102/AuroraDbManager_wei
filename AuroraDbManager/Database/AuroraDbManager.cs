// 
// 	AuroraDbManager.cs
// 	AuroraDbManager
// 
// 	Created by Swizzy on 14/05/2015
// 	Copyright (c) 2015 Swizzy. All rights reserved.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using AuroraDbManager.Database;

namespace AuroraDbManager.Database {

    internal class AuroraDbManager {
        private SQLiteConnection _content;
        private ContentItem[] _contentItems;
        private SQLiteConnection _settings;
        private TitleUpdateItem[] _titleUpdateItems;
        private SystemSettingItem[] _systemSettingItems;
        private UserSettingItem[] _userSettingItems;
        private ScanPathItem[] _scanPathItems;
        private ProfileItem[] _profileItems;
        private QuickViewItem[] _quickViewItems;
        private UserFavoriteItem[] _userFavoriteItems;
        private UserHiddenItem[] _userHiddenItems;
        private TrainerItem[] _trainerItems;

        public bool IsContentOpen { get { return _content != null; } }

        public bool IsSettingsOpen { get { return _settings != null; } }

        public void ConnectToContent(string path) {
            if(_content != null)
                _content.Close();
            _content = new SQLiteConnection("Data Source=\"" + path + "\";Version=3;");
            _content.Open();
            _contentItems = GetContentDataTable("SELECT * FROM ContentItems").Select().Select(row => new ContentItem(row)).ToArray();
            _titleUpdateItems = GetContentDataTable("SELECT CAST(FileSize AS TEXT) AS FileSize, * FROM TitleUpdates").Select().Select(row => new TitleUpdateItem(row)).ToArray();
        }

        public void ConnectToSettings(string path) {
            if(_settings != null)
                _settings.Close();
            _settings = new SQLiteConnection("Data Source=\"" + path + "\";Version=3;");
            _settings.Open();
            LoadSettingsData();
        }

        private void LoadSettingsData() {
            _systemSettingItems = GetSettingsDataTable("SELECT * FROM SystemSettings").Select().Select(row => new SystemSettingItem(row)).ToArray();
            _userSettingItems = GetSettingsDataTable("SELECT * FROM UserSettings").Select().Select(row => new UserSettingItem(row)).ToArray();
            _scanPathItems = GetSettingsDataTable("SELECT * FROM ScanPaths").Select().Select(row => new ScanPathItem(row)).ToArray();
            _profileItems = GetSettingsDataTable("SELECT * FROM Profiles").Select().Select(row => new ProfileItem(row)).ToArray();
            _quickViewItems = GetSettingsDataTable("SELECT * FROM QuickViews").Select().Select(row => new QuickViewItem(row)).ToArray();
            _userFavoriteItems = GetSettingsDataTable("SELECT * FROM UserFavorites").Select().Select(row => new UserFavoriteItem(row)).ToArray();
            _userHiddenItems = GetSettingsDataTable("SELECT * FROM UserHidden").Select().Select(row => new UserHiddenItem(row)).ToArray();
            _trainerItems = GetSettingsDataTable("SELECT * FROM Trainers").Select().Select(row => new TrainerItem(row)).ToArray();
        }

        private DataTable GetContentDataTable(string sql) {
            var dt = new DataTable();
            try {
                var cmd = new SQLiteCommand(sql, _content);
                using(var reader = cmd.ExecuteReader())
                    dt.Load(reader);
            }
            catch(Exception ex) {
                App.SaveException(ex);
            }
            return dt;
        }

        private DataTable GetSettingsDataTable(string sql) {
            var dt = new DataTable();
            try {
                var cmd = new SQLiteCommand(sql, _settings);
                using(var reader = cmd.ExecuteReader())
                    dt.Load(reader);
            }
            catch(Exception ex) {
                App.SaveException(ex);
            }
            return dt;
        }

        private int ExecuteNonQueryContent(string sql) {
            try {
                var cmd = new SQLiteCommand(sql, _content);
                return cmd.ExecuteNonQuery();
            }
            catch(Exception ex) {
                App.SaveException(ex);
                return 0;
            }
        }

        private int ExecuteNonQuerySettings(string sql) {
            try {
                // 使用事务确保数据一致性
                using(var transaction = _settings.BeginTransaction()) {
                    var cmd = new SQLiteCommand(sql, _settings, transaction);
                    var result = cmd.ExecuteNonQuery();
                    transaction.Commit(); // 显式提交事务
                    
                    // 添加调试日志，帮助诊断问题
                    System.Diagnostics.Debug.WriteLine($"ExecuteNonQuerySettings: SQL={sql}, Result={result}");
                    
                    return result;
                }
            }
            catch(Exception ex) {
                // 记录详细的错误信息
                var errorMsg = $"ExecuteNonQuerySettings failed. SQL: {sql}, Error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(errorMsg);
                App.SaveException(new Exception(errorMsg, ex));
                
                // 抛出异常，让上层知道发生了错误
                throw new Exception($"数据库操作失败: {ex.Message}", ex);
            }
        }

        public IEnumerable<ContentItem> GetContentItems() { return _contentItems; }

        public IEnumerable<TitleUpdateItem> GetTitleUpdateItems() { return _titleUpdateItems; }

        public void UpdateTitleUpdateItem(TitleUpdateItem tuItem) {
            for(var i = 0; i < _titleUpdateItems.Length; i++) {
                if(_titleUpdateItems[i].Id != tuItem.Id)
                    continue;
                _titleUpdateItems[i] = tuItem;
                return;
            }
        }

        public void UpdateContentItem(ContentItem contentItem) {
            for(var i = 0; i < _contentItems.Length; i++) {
                if(_contentItems[i].Id != contentItem.Id)
                    continue;
                _contentItems[i] = contentItem;
                return;
            }
        }

        public void SaveContentChanges() {
            foreach(var contentItem in _contentItems) {
                if (!contentItem.Changed)
                    continue;
                //TODO: Make it save content Changes
            }
            foreach(var titleUpdateItem in _titleUpdateItems) {
                if (!titleUpdateItem.Changed)
                    continue;
                //TODO: Make it save titleupdate Changes
            }
        }

        public void SaveSettingsChanges() {
            var sql = new StringBuilder();
            var changedItems = new List<string>();
            
            // 检查每个集合中的更改项并生成SQL
            foreach(var item in _systemSettingItems.Where(x => x.Changed)) {
                sql.AppendLine($"UPDATE SystemSettings SET Name='{item.Name.Replace("'", "''")}', Value='{item.Value.Replace("'", "''")}' WHERE Id={item.Id};");
                changedItems.Add($"SystemSetting Id={item.Id}");
                System.Diagnostics.Debug.WriteLine($"SystemSetting UPDATE: Id={item.Id}, Name={item.Name}, Value={item.Value}");
            }
            
            foreach(var item in _userSettingItems.Where(x => x.Changed)) {
                sql.AppendLine($"UPDATE UserSettings SET Name='{item.Name.Replace("'", "''")}', Value='{item.Value.Replace("'", "''")}', ProfileId='{item.ProfileId.Replace("'", "''")}' WHERE Id={item.Id};");
                changedItems.Add($"UserSetting Id={item.Id}");
                System.Diagnostics.Debug.WriteLine($"UserSetting UPDATE: Id={item.Id}, Name={item.Name}, Value={item.Value}");
            }
            
            foreach(var item in _scanPathItems.Where(x => x.Changed)) {
                sql.AppendLine($"UPDATE ScanPaths SET Path='{item.Path.Replace("'", "''")}', DeviceId='{item.DeviceID.Replace("'", "''")}', Depth={item.Depth}, ScriptData='{item.ScriptData.Replace("'", "''")}', OptionsFlag={item.OptionsFlag} WHERE Id={item.Id};");
                changedItems.Add($"ScanPath Id={item.Id}");
                System.Diagnostics.Debug.WriteLine($"ScanPath UPDATE: Id={item.Id}, Path={item.Path}");
            }
            
            foreach(var item in _profileItems.Where(x => x.Changed)) {
                sql.AppendLine($"UPDATE Profiles SET Gametag='{item.GameTag.Replace("'", "''")}', Xuid='{item.Xuid.Replace("'", "''")}' WHERE Id={item.Id};");
                changedItems.Add($"Profile Id={item.Id}");
                System.Diagnostics.Debug.WriteLine($"Profile UPDATE: Id={item.Id}, Gametag={item.GameTag}");
            }
            
            foreach(var item in _quickViewItems.Where(x => x.Changed)) {
                sql.AppendLine($"UPDATE QuickViews SET DisplayName='{item.DisplayName.Replace("'", "''")}', SortMethod='{item.SortMethod.Replace("'", "''")}', FilterMethod='{item.FilterMethod.Replace("'", "''")}', Flags={item.Flags}, CreatorXUID='{item.CreatorXUID.Replace("'", "''")}', OrderIndex={item.OrderIndex}, IconHash='{item.IconHash.Replace("'", "''")}' WHERE Id={item.Id};");
                changedItems.Add($"QuickView Id={item.Id}");
                System.Diagnostics.Debug.WriteLine($"QuickView UPDATE: Id={item.Id}, DisplayName={item.DisplayName}");
            }
            
            foreach(var item in _userFavoriteItems.Where(x => x.Changed)) {
                sql.AppendLine($"UPDATE UserFavorites SET ContentId={item.ContentId}, ProfileId='{item.ProfileId.Replace("'", "''")}' WHERE Id={item.Id};");
                changedItems.Add($"UserFavorite Id={item.Id}");
                System.Diagnostics.Debug.WriteLine($"UserFavorite UPDATE: Id={item.Id}, ContentId={item.ContentId}");
            }
            
            foreach(var item in _userHiddenItems.Where(x => x.Changed)) {
                sql.AppendLine($"UPDATE UserHidden SET ContentId={item.ContentId}, ProfileId='{item.ProfileId.Replace("'", "''")}' WHERE Id={item.Id};");
                changedItems.Add($"UserHidden Id={item.Id}");
                System.Diagnostics.Debug.WriteLine($"UserHidden UPDATE: Id={item.Id}, ContentId={item.ContentId}");
            }
            
            foreach(var item in _trainerItems.Where(x => x.Changed)) {
                sql.AppendLine($"UPDATE Trainers SET TitleId='{item.TitleId.Replace("'", "''")}', MediaId='{item.MediaId.Replace("'", "''")}', TrainerPath='{item.TrainerPath.Replace("'", "''")}', TrainerName='{item.TrainerName.Replace("'", "''")}', TrainerVersion={item.TrainerVersion}, TrainerData='{item.TrainerData.Replace("'", "''")}', TrainerInfo='{item.TrainerInfo.Replace("'", "''")}', TrainerAuthor='{item.TrainerAuthor.Replace("'", "''")}', TrainerRating={item.TrainerRating}, TrainerFlags={item.TrainerFlags}, CreatorXUID='{item.CreatorXUID.Replace("'", "''")}' WHERE Id={item.Id};");
                changedItems.Add($"Trainer Id={item.Id}");
                System.Diagnostics.Debug.WriteLine($"Trainer UPDATE: Id={item.Id}, TrainerName={item.TrainerName}");
            }
            
            // 执行SQL
            if(sql.Length > 0) {
                System.Diagnostics.Debug.WriteLine($"Executing {changedItems.Count} updates: {string.Join(", ", changedItems)}");
                var result = ExecuteNonQuerySettings(sql.ToString());
                System.Diagnostics.Debug.WriteLine($"SaveSettingsChanges: {result} rows affected");
                
                // 清除所有更改标记
                foreach(var item in _systemSettingItems) item.Changed = false;
                foreach(var item in _userSettingItems) item.Changed = false;
                foreach(var item in _scanPathItems) item.Changed = false;
                foreach(var item in _profileItems) item.Changed = false;
                foreach(var item in _trainerItems) item.Changed = false;
                foreach(var item in _userFavoriteItems) item.Changed = false;
                foreach(var item in _userHiddenItems) item.Changed = false;
                foreach(var item in _quickViewItems) item.Changed = false;
                
                System.Diagnostics.Debug.WriteLine("SaveSettingsChanges completed successfully");
            }
            else {
                System.Diagnostics.Debug.WriteLine("SaveSettingsChanges: No changes detected");
            }
        }

        public void CloseContentDb() {
            if(!IsContentOpen)
                return;
            var isDisposed = false;
            _content.Disposed += (sender, args) => isDisposed = true;
            _content.Close();
            GC.Collect();
            while (!isDisposed)
                Thread.Sleep(10);
        }
        public IEnumerable<SystemSettingItem> GetSystemSettings() { return _systemSettingItems; }
        public IEnumerable<UserSettingItem> GetUserSettings() { return _userSettingItems; }
        public IEnumerable<ScanPathItem> GetScanPaths() { return _scanPathItems; }
        public IEnumerable<ProfileItem> GetProfiles() { return _profileItems; }
        public IEnumerable<QuickViewItem> GetQuickViews() { return _quickViewItems; }
        public IEnumerable<UserFavoriteItem> GetUserFavorites() { return _userFavoriteItems; }
        public IEnumerable<UserHiddenItem> GetUserHidden() { return _userHiddenItems; }
        public IEnumerable<TrainerItem> GetTrainers() { return _trainerItems; }

        public void UpdateSystemSetting(SystemSettingItem setting) {
            for(var i = 0; i < _systemSettingItems.Length; i++) {
                if(_systemSettingItems[i].Id != setting.Id)
                    continue;
                _systemSettingItems[i] = setting;
                return;
            }
        }

        public void UpdateUserSetting(UserSettingItem setting) {
            for(var i = 0; i < _userSettingItems.Length; i++) {
                if(_userSettingItems[i].Id != setting.Id)
                    continue;
                _userSettingItems[i] = setting;
                return;
            }
        }

        public void UpdateScanPath(ScanPathItem scanPath) {
            for(var i = 0; i < _scanPathItems.Length; i++) {
                if(_scanPathItems[i].Id != scanPath.Id)
                    continue;
                _scanPathItems[i] = scanPath;
                return;
            }
        }

        public void UpdateProfile(ProfileItem profile) {
            for(var i = 0; i < _profileItems.Length; i++) {
                if(_profileItems[i].Id != profile.Id)
                    continue;
                _profileItems[i] = profile;
                return;
            }
        }

        public void UpdateQuickView(QuickViewItem quickView) {
            for(var i = 0; i < _quickViewItems.Length; i++) {
                if(_quickViewItems[i].Id != quickView.Id)
                    continue;
                _quickViewItems[i] = quickView;
                return;
            }
        }

        public void UpdateUserFavorite(UserFavoriteItem userFavorite) {
            for(var i = 0; i < _userFavoriteItems.Length; i++) {
                if(_userFavoriteItems[i].Id != userFavorite.Id)
                    continue;
                _userFavoriteItems[i] = userFavorite;
                return;
            }
        }

        public void UpdateUserHidden(UserHiddenItem userHidden) {
            for(var i = 0; i < _userHiddenItems.Length; i++) {
                if(_userHiddenItems[i].Id != userHidden.Id)
                    continue;
                _userHiddenItems[i] = userHidden;
                return;
            }
        }

        public void UpdateTrainer(TrainerItem trainer) {
            for(var i = 0; i < _trainerItems.Length; i++) {
                if(_trainerItems[i].Id != trainer.Id)
                    continue;
                _trainerItems[i] = trainer;
                return;
            }
        }

                        public void DeleteQuickView(QuickViewItem quickView) {
            var sql = string.Format("DELETE FROM QuickViews WHERE Id={0}", quickView.Id);
            ExecuteNonQuerySettings(sql);
            var list = _quickViewItems.ToList();
            list.RemoveAll(x => x.Id == quickView.Id);
            _quickViewItems = list.ToArray();
        }

        public void DeleteUserFavorite(UserFavoriteItem userFavorite) {
            var sql = string.Format("DELETE FROM UserFavorites WHERE Id={0}", userFavorite.Id);
            ExecuteNonQuerySettings(sql);
            var list = _userFavoriteItems.ToList();
            list.RemoveAll(x => x.Id == userFavorite.Id);
            _userFavoriteItems = list.ToArray();
        }

        public void DeleteUserHidden(UserHiddenItem userHidden) {
            var sql = string.Format("DELETE FROM UserHidden WHERE Id={0}", userHidden.Id);
            ExecuteNonQuerySettings(sql);
            var list = _userHiddenItems.ToList();
            list.RemoveAll(x => x.Id == userHidden.Id);
            _userHiddenItems = list.ToArray();
        }

        public void DeleteTrainer(TrainerItem trainer) {
            var sql = string.Format("DELETE FROM Trainers WHERE Id={0}", trainer.Id);
            ExecuteNonQuerySettings(sql);
            var list = _trainerItems.ToList();
            list.RemoveAll(x => x.Id == trainer.Id);
            _trainerItems = list.ToArray();
        }

        public void AddQuickView(QuickViewItem quickView) {
            var sql = string.Format("INSERT INTO QuickViews (DisplayName, SortMethod, FilterMethod, Flags, CreatorXUID, OrderIndex, IconHash) VALUES ('{0}', '{1}', '{2}', {3}, '{4}', {5}, '{6}')",
                quickView.DisplayName.Replace("'", "''"),
                quickView.SortMethod.Replace("'", "''"),
                quickView.FilterMethod.Replace("'", "''"),
                quickView.Flags,
                quickView.CreatorXUID.Replace("'", "''"),
                quickView.OrderIndex,
                quickView.IconHash.Replace("'", "''"));
            ExecuteNonQuerySettings(sql);
            var list = _quickViewItems.ToList();
            list.Add(quickView);
            _quickViewItems = list.ToArray();
        }

        public void AddUserFavorite(UserFavoriteItem userFavorite) {
            var sql = string.Format("INSERT INTO UserFavorites (ContentId, ProfileId) VALUES ({0}, '{1}')",
                userFavorite.ContentId,
                userFavorite.ProfileId.Replace("'", "''"));
            ExecuteNonQuerySettings(sql);
            var list = _userFavoriteItems.ToList();
            list.Add(userFavorite);
            _userFavoriteItems = list.ToArray();
        }

        public void AddUserHidden(UserHiddenItem userHidden) {
            var sql = string.Format("INSERT INTO UserHidden (ContentId, ProfileId) VALUES ({0}, '{1}')",
                userHidden.ContentId,
                userHidden.ProfileId.Replace("'", "''"));
            ExecuteNonQuerySettings(sql);
            var list = _userHiddenItems.ToList();
            list.Add(userHidden);
            _userHiddenItems = list.ToArray();
        }

        public void AddTrainer(TrainerItem trainer) {
            var sql = string.Format("INSERT INTO Trainers (TitleId, MediaId, TrainerPath, TrainerName, TrainerVersion, TrainerData, TrainerInfo, TrainerAuthor, TrainerRating, TrainerFlags, CreatorXUID) VALUES ('{0}', '{1}', '{2}', '{3}', {4}, '{5}', '{6}', '{7}', {8}, {9}, '{10}')",
                trainer.TitleId.Replace("'", "''"),
                trainer.MediaId.Replace("'", "''"),
                trainer.TrainerPath.Replace("'", "''"),
                trainer.TrainerName.Replace("'", "''"),
                trainer.TrainerVersion,
                trainer.TrainerData.Replace("'", "''"),
                trainer.TrainerInfo.Replace("'", "''"),
                trainer.TrainerAuthor.Replace("'", "''"),
                trainer.TrainerRating,
                trainer.TrainerFlags,
                trainer.CreatorXUID.Replace("'", "''"));
            ExecuteNonQuerySettings(sql);
            var list = _trainerItems.ToList();
            list.Add(trainer);
            _trainerItems = list.ToArray();
        }

        public void CloseSettingsDb()
        {
            if (!IsSettingsOpen)
                return;
            var isDisposed = false;
            _settings.Disposed += (sender, args) => isDisposed = true;
            _settings.Close();
            GC.Collect();
            while (!isDisposed)
                Thread.Sleep(10);
        }
    }
}