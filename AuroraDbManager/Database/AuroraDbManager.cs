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
                System.Diagnostics.Debug.WriteLine($"ExecuteNonQueryContent: Attempting to execute SQL: {sql}");

                // 使用事务确保数据一致性
                using (var transaction = _content.BeginTransaction()) {
                    var cmd = new SQLiteCommand(sql, _content, transaction);
                    var result = cmd.ExecuteNonQuery();
                    transaction.Commit(); // 显式提交事务

                    // 添加调试日志，帮助诊断问题
                    System.Diagnostics.Debug.WriteLine($"ExecuteNonQueryContent: SQL={sql}, Result={result}");

                    return result;
                }
            }
            catch (Exception ex) {
                // 记录详细的错误信息
                var errorMsg = $"ExecuteNonQueryContent failed. SQL: {sql}, Error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(errorMsg);
                App.SaveException(new Exception(errorMsg, ex));

                // 抛出异常，让上层知道发生了错误
                throw new Exception($"数据库操作失败: {ex.Message}", ex);
            }
        }

        private int ExecuteNonQuerySettings(string sql) {
            try {
                System.Diagnostics.Debug.WriteLine($"ExecuteNonQuerySettings: Attempting to execute SQL: {sql}");
                
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
            System.Diagnostics.Debug.WriteLine("SaveContentChanges: Starting to save changes");

            var sql = new StringBuilder();
            var changedItems = new List<string>();

            // 检查ContentItems中的更改项并生成SQL
            System.Diagnostics.Debug.WriteLine($"SaveContentChanges: Checking {_contentItems.Count()} ContentItem items");
            foreach (var item in _contentItems.Where(x => x.Changed)) {
                sql.AppendLine($"UPDATE ContentItems SET " +
                    $"TitleId={item.TitleId}, " +
                    $"MediaId={item.MediaId}, " +
                    $"BaseVersion={item.BaseVersion}, " +
                    $"DiscNum={item.DiscNum}, " +
                    $"DiscsInSet={item.DiscsInSet}, " +
                    $"TitleName='{item.TitleName.Replace("'", "''")}', " +
                    $"Description='{item.Description.Replace("'", "''")}', " +
                    $"Publisher='{item.Publisher.Replace("'", "''")}', " +
                    $"Developer='{item.Developer.Replace("'", "''")}', " +
                    $"LiveRating={item.LiveRating}, " +
                    $"LiveRaters={item.LiveRaters}, " +
                    $"ReleaseDate='{item.ReleaseDate.Replace("'", "''")}', " +
                    $"GenreFlag={(int)item.GenreFlag}, " +
                    $"ContentFlags={(int)item.ContentFlags}, " +
                    $"Hash='{item.Hash.Replace("'", "''")}', " +
                    $"GameCapsOnline={((long)item.DataRow["GameCapsOnline"])}, " +
                    $"GameCapsOffline={((long)item.DataRow["GameCapsOffline"])}, " +
                    $"GameCapsFlags={(int)item.GameCapsFlags}, " +
                    $"FileType={(int)item.FileType}, " +
                    $"ContentType={(int)item.ContentType}, " +
                    $"ContentGroup={(int)item.ContentGroup}, " +
                    $"DefaultGroup={(int)item.DefaultGroup}, " +
                    $"DateAdded={item.DateAdded.ToFileTime()}, " +
                    $"FoundAtDepth={item.FoundAtDepth}, " +
                    $"SystemLink={(item.SystemLink ? 1 : 0)}, " +
                    $"ScanPathId={item.ScanPathId}, " +
                    $"Directory='{item.Directory.Replace("'", "''")}', " +
                    $"Executable='{item.Executable.Replace("'", "''")}' " +
                    $"WHERE Id={item.Id};");
                changedItems.Add($"ContentItem Id={item.Id}");
                System.Diagnostics.Debug.WriteLine($"ContentItem UPDATE: Id={item.Id}, TitleName={item.TitleName}");
            }

            // 检查TitleUpdateItems中的更改项并生成SQL
            System.Diagnostics.Debug.WriteLine($"SaveContentChanges: Checking {_titleUpdateItems.Count()} TitleUpdateItem items");
            foreach (var item in _titleUpdateItems.Where(x => x.Changed)) {
                sql.AppendLine($"UPDATE TitleUpdates SET " +
                    $"DisplayName='{item.DisplayName.Replace("'", "''")}', " +
                    $"FileName='{item.FileName.Replace("'", "''")}', " +
                    $"LiveDeviceId='{item.LiveDeviceId.Replace("'", "''")}', " +
                    $"LivePath='{item.LivePath.Replace("'", "''")}', " +
                    $"TitleId={item.TitleId}, " +
                    $"MediaId={item.MediaId}, " +
                    $"BaseVersion={item.BaseVersion}, " +
                    $"Version={item.Version}, " +
                    $"Hash='{item.Hash.Replace("'", "''")}', " +
                    $"BackupPath='{item.BackupPath.Replace("'", "''")}', " +
                    $"FileSize='{item.FileSize}' " +
                    $"WHERE Id={item.Id};");
                changedItems.Add($"TitleUpdateItem Id={item.Id}");
                System.Diagnostics.Debug.WriteLine($"TitleUpdateItem UPDATE: Id={item.Id}, DisplayName={item.DisplayName}");
            }

            // 执行SQL
            System.Diagnostics.Debug.WriteLine($"SaveContentChanges: Found {changedItems.Count} changed items");
            if (sql.Length > 0) {
                System.Diagnostics.Debug.WriteLine($"Executing {changedItems.Count} updates: {string.Join(", ", changedItems)}");
                System.Diagnostics.Debug.WriteLine($"SQL to execute: {sql}");
                var result = ExecuteNonQueryContent(sql.ToString());
                System.Diagnostics.Debug.WriteLine($"SaveContentChanges: {result} rows affected");

                // 只清除已保存项的更改标记
                foreach (var item in _contentItems.Where(x => x.Changed)) item.Changed = false;
                foreach (var item in _titleUpdateItems.Where(x => x.Changed)) item.Changed = false;

                System.Diagnostics.Debug.WriteLine("SaveContentChanges completed successfully");
            }
            else {
                System.Diagnostics.Debug.WriteLine("SaveContentChanges: No changes detected");
            }
        }

        public void SaveSettingsChanges() {
            System.Diagnostics.Debug.WriteLine("SaveSettingsChanges: Starting to save changes");
            
            var sql = new StringBuilder();
            var changedItems = new List<string>();
            
            // 检查每个集合中的更改项并生成SQL
            System.Diagnostics.Debug.WriteLine($"SaveSettingsChanges: Checking {_systemSettingItems.Count()} SystemSetting items");
            foreach(var item in _systemSettingItems.Where(x => x.Changed)) {
                sql.AppendLine($"UPDATE SystemSettings SET Name='{item.Name.Replace("'", "''")}', Value='{item.Value.Replace("'", "''")}' WHERE Id={item.Id};");
                changedItems.Add($"SystemSetting Id={item.Id}");
                System.Diagnostics.Debug.WriteLine($"SystemSetting UPDATE: Id={item.Id}, Name={item.Name}, Value={item.Value}");
            }
            
            System.Diagnostics.Debug.WriteLine($"SaveSettingsChanges: Checking {_userSettingItems.Count()} UserSetting items");
            foreach(var item in _userSettingItems.Where(x => x.Changed)) {
                sql.AppendLine($"UPDATE UserSettings SET Name='{item.Name.Replace("'", "''")}', Value='{item.Value.Replace("'", "''")}', ProfileId='{item.ProfileId.Replace("'", "''")}' WHERE Id={item.Id};");
                changedItems.Add($"UserSetting Id={item.Id}");
                System.Diagnostics.Debug.WriteLine($"UserSetting UPDATE: Id={item.Id}, Name={item.Name}, Value={item.Value}");
            }
            
            System.Diagnostics.Debug.WriteLine($"SaveSettingsChanges: Checking {_scanPathItems.Count()} ScanPath items");
            foreach(var item in _scanPathItems.Where(x => x.Changed)) {
                sql.AppendLine($"UPDATE ScanPaths SET Path='{item.Path.Replace("'", "''")}', DeviceId='{item.DeviceID.Replace("'", "''")}', Depth={item.Depth}, ScriptData='{item.ScriptData.Replace("'", "''")}', OptionsFlag={item.OptionsFlag} WHERE Id={item.Id};");
                changedItems.Add($"ScanPath Id={item.Id}");
                System.Diagnostics.Debug.WriteLine($"ScanPath UPDATE: Id={item.Id}, Path={item.Path}");
            }
            
            System.Diagnostics.Debug.WriteLine($"SaveSettingsChanges: Checking {_profileItems.Count()} Profile items");
            foreach(var item in _profileItems.Where(x => x.Changed)) {
                sql.AppendLine($"UPDATE Profiles SET Gametag='{item.GameTag.Replace("'", "''")}', Xuid='{item.Xuid.Replace("'", "''")}' WHERE Id={item.Id};");
                changedItems.Add($"Profile Id={item.Id}");
                System.Diagnostics.Debug.WriteLine($"Profile UPDATE: Id={item.Id}, Gametag={item.GameTag}");
            }
            
            System.Diagnostics.Debug.WriteLine($"SaveSettingsChanges: Checking {_quickViewItems.Count()} QuickView items");
            foreach(var item in _quickViewItems.Where(x => x.Changed)) {
                sql.AppendLine($"UPDATE QuickViews SET DisplayName='{item.DisplayName.Replace("'", "''")}', SortMethod='{item.SortMethod.Replace("'", "''")}', FilterMethod='{item.FilterMethod.Replace("'", "''")}', Flags={item.Flags}, CreatorXUID='{item.CreatorXUID.Replace("'", "''")}', OrderIndex={item.OrderIndex}, IconHash='{item.IconHash.Replace("'", "''")}' WHERE Id={item.Id};");
                changedItems.Add($"QuickView Id={item.Id}");
                System.Diagnostics.Debug.WriteLine($"QuickView UPDATE: Id={item.Id}, DisplayName={item.DisplayName}");
            }
            
            System.Diagnostics.Debug.WriteLine($"SaveSettingsChanges: Checking {_userFavoriteItems.Count()} UserFavorite items");
            foreach(var item in _userFavoriteItems.Where(x => x.Changed)) {
                sql.AppendLine($"UPDATE UserFavorites SET ContentId={item.ContentId}, ProfileId='{item.ProfileId.Replace("'", "''")}' WHERE Id={item.Id};");
                changedItems.Add($"UserFavorite Id={item.Id}");
                System.Diagnostics.Debug.WriteLine($"UserFavorite UPDATE: Id={item.Id}, ContentId={item.ContentId}");
            }
            
            System.Diagnostics.Debug.WriteLine($"SaveSettingsChanges: Checking {_userHiddenItems.Count()} UserHidden items");
            foreach(var item in _userHiddenItems.Where(x => x.Changed)) {
                sql.AppendLine($"UPDATE UserHidden SET ContentId={item.ContentId}, ProfileId='{item.ProfileId.Replace("'", "''")}' WHERE Id={item.Id};");
                changedItems.Add($"UserHidden Id={item.Id}");
                System.Diagnostics.Debug.WriteLine($"UserHidden UPDATE: Id={item.Id}, ContentId={item.ContentId}");
            }
            
            System.Diagnostics.Debug.WriteLine($"SaveSettingsChanges: Checking {_trainerItems.Count()} Trainer items");
            foreach(var item in _trainerItems.Where(x => x.Changed)) {
                sql.AppendLine($"UPDATE Trainers SET TitleId='{item.TitleId.Replace("'", "''")}', MediaId='{item.MediaId.Replace("'", "''")}', TrainerPath='{item.TrainerPath.Replace("'", "''")}', TrainerName='{item.TrainerName.Replace("'", "''")}', TrainerVersion={item.TrainerVersion}, TrainerData='{item.TrainerData.Replace("'", "''")}', TrainerInfo='{item.TrainerInfo.Replace("'", "''")}', TrainerAuthor='{item.TrainerAuthor.Replace("'", "''")}', TrainerRating={item.TrainerRating}, TrainerFlags={item.TrainerFlags}, CreatorXUID='{item.CreatorXUID.Replace("'", "''")}' WHERE Id={item.Id};");
                changedItems.Add($"Trainer Id={item.Id}");
                System.Diagnostics.Debug.WriteLine($"Trainer UPDATE: Id={item.Id}, TrainerName={item.TrainerName}");
            }
            
            // 执行SQL
            System.Diagnostics.Debug.WriteLine($"SaveSettingsChanges: Found {changedItems.Count} changed items");
            if(sql.Length > 0) {
                System.Diagnostics.Debug.WriteLine($"Executing {changedItems.Count} updates: {string.Join(", ", changedItems)}");
                System.Diagnostics.Debug.WriteLine($"SQL to execute: {sql}");
                var result = ExecuteNonQuerySettings(sql.ToString());
                System.Diagnostics.Debug.WriteLine($"SaveSettingsChanges: {result} rows affected");
                
                // 只清除已保存项的更改标记
                foreach(var item in _systemSettingItems.Where(x => x.Changed)) item.Changed = false;
                foreach(var item in _userSettingItems.Where(x => x.Changed)) item.Changed = false;
                foreach(var item in _scanPathItems.Where(x => x.Changed)) item.Changed = false;
                foreach(var item in _profileItems.Where(x => x.Changed)) item.Changed = false;
                foreach(var item in _trainerItems.Where(x => x.Changed)) item.Changed = false;
                foreach(var item in _userFavoriteItems.Where(x => x.Changed)) item.Changed = false;
                foreach(var item in _userHiddenItems.Where(x => x.Changed)) item.Changed = false;
                foreach(var item in _quickViewItems.Where(x => x.Changed)) item.Changed = false;
                
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

        public void DeleteContentItem(ContentItem item) {
            try {
                var sql = $"DELETE FROM ContentItems WHERE Id={item.Id}";
                ExecuteNonQueryContent(sql);
                
                var list = _contentItems.ToList();
                list.RemoveAll(x => x.Id == item.Id);
                _contentItems = list.ToArray();
                
                SendStatusChanged("Content item deleted successfully");
            }
            catch (Exception ex) {
                App.SaveException(ex);
                throw new Exception($"Failed to delete content item: {ex.Message}", ex);
            }
        }

        public void DeleteTitleUpdateItem(TitleUpdateItem item) {
            try {
                var sql = $"DELETE FROM TitleUpdates WHERE Id={item.Id}";
                ExecuteNonQueryContent(sql);
                
                var list = _titleUpdateItems.ToList();
                list.RemoveAll(x => x.Id == item.Id);
                _titleUpdateItems = list.ToArray();
                
                SendStatusChanged("Title update item deleted successfully");
            }
            catch (Exception ex) {
                App.SaveException(ex);
                throw new Exception($"Failed to delete title update item: {ex.Message}", ex);
            }
        }

        public void AddContentItem(ContentItem item) {
            try {
                var sql = $"INSERT INTO ContentItems (" +
                    $"Directory, Executable, TitleId, MediaId, BaseVersion, DiscNum, DiscsInSet, TitleName, Description, " +
                    $"Publisher, Developer, LiveRating, LiveRaters, ReleaseDate, GenreFlag, ContentFlags, Hash, " +
                    $"GameCapsOnline, GameCapsOffline, GameCapsFlags, FileType, ContentType, ContentGroup, DefaultGroup, " +
                    $"DateAdded, FoundAtDepth, SystemLink, ScanPathId) VALUES (" +
                    $"'{item.Directory.Replace("'", "''")}', " +
                    $"'{item.Executable.Replace("'", "''")}', " +
                    $"{item.TitleId}, " +
                    $"{item.MediaId}, " +
                    $"{item.BaseVersion}, " +
                    $"{item.DiscNum}, " +
                    $"{item.DiscsInSet}, " +
                    $"'{item.TitleName.Replace("'", "''")}', " +
                    $"'{item.Description.Replace("'", "''")}', " +
                    $"'{item.Publisher.Replace("'", "''")}', " +
                    $"'{item.Developer.Replace("'", "''")}', " +
                    $"{item.LiveRating}, " +
                    $"{item.LiveRaters}, " +
                    $"'{item.ReleaseDate.Replace("'", "''")}', " +
                    $"{(int)item.GenreFlag}, " +
                    $"{(int)item.ContentFlags}, " +
                    $"'{item.Hash.Replace("'", "''")}', " +
                    $"{((long)item.DataRow["GameCapsOnline"])}, " +
                    $"{((long)item.DataRow["GameCapsOffline"])}, " +
                    $"{(int)item.GameCapsFlags}, " +
                    $"{(int)item.FileType}, " +
                    $"{(int)item.ContentType}, " +
                    $"{(int)item.ContentGroup}, " +
                    $"{(int)item.DefaultGroup}, " +
                    $"{item.DateAdded.ToFileTime()}, " +
                    $"{item.FoundAtDepth}, " +
                    $"{(item.SystemLink ? 1 : 0)}, " +
                    $"{item.ScanPathId}" +
                    $")";
                
                ExecuteNonQueryContent(sql);
                
                var list = _contentItems.ToList();
                list.Add(item);
                _contentItems = list.ToArray();
                
                SendStatusChanged("Content item added successfully");
            }
            catch (Exception ex) {
                App.SaveException(ex);
                throw new Exception($"Failed to add content item: {ex.Message}", ex);
            }
        }

        public void AddTitleUpdateItem(TitleUpdateItem item) {
            try {
                var sql = $"INSERT INTO TitleUpdates (" +
                    $"DisplayName, FileName, LiveDeviceId, LivePath, TitleId, MediaId, BaseVersion, Version, Hash, BackupPath, FileSize" +
                    $") VALUES (" +
                    $"'{item.DisplayName.Replace("'", "''")}', " +
                    $"'{item.FileName.Replace("'", "''")}', " +
                    $"'{item.LiveDeviceId.Replace("'", "''")}', " +
                    $"'{item.LivePath.Replace("'", "''")}', " +
                    $"{item.TitleId}, " +
                    $"{item.MediaId}, " +
                    $"{item.BaseVersion}, " +
                    $"{item.Version}, " +
                    $"'{item.Hash.Replace("'", "''")}', " +
                    $"'{item.BackupPath.Replace("'", "''")}', " +
                    $"'{item.FileSize}'" +
                    $")";
                
                ExecuteNonQueryContent(sql);
                
                var list = _titleUpdateItems.ToList();
                list.Add(item);
                _titleUpdateItems = list.ToArray();
                
                SendStatusChanged("Title update item added successfully");
            }
            catch (Exception ex) {
                App.SaveException(ex);
                throw new Exception($"Failed to add title update item: {ex.Message}", ex);
            }
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