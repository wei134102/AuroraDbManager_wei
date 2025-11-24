// 
// 	SettingsDbView.xaml.cs
// 	AuroraDbManager
// 
// 	Created by Swizzy on 23/05/2015
// 	Copyright (c) 2015 Swizzy. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using AuroraDbManager.Classes;
using Microsoft.Win32;
using AuroraDbManager.Database;

namespace AuroraDbManager.Views {
    /// <summary>
    ///     Interaction logic for SettingsDbView.xaml
    /// </summary>
    public partial class SettingsDbView : UserControl {
        public SettingsDbView() { InitializeComponent(); }

        public void OpenDb(string filename = null) {
            try {
                if(string.IsNullOrWhiteSpace(filename)) {
                    var ofd = new OpenFileDialog();
                    if(ofd.ShowDialog() != true)
                        return;
                    filename = ofd.FileName;
                }
                if(!File.Exists(filename)) {
                    SendStatusChanged("ERROR: {0} Does not exist", filename);
                    return;
                }
                SendStatusChanged("Loading {0}...", filename);
                App.DbManager.ConnectToSettings(filename);
                Dispatcher.Invoke(new Action(() => {
                    ProfilesBox.ItemsSource = App.DbManager.GetProfiles();
                    ScanPathsBox.ItemsSource = App.DbManager.GetScanPaths();
                    SystemSettingsBox.ItemsSource = App.DbManager.GetSystemSettings();
                    UserSettingsBox.ItemsSource = App.DbManager.GetUserSettings();
                    QuickViewsBox.ItemsSource = App.DbManager.GetQuickViews();
                    UserFavoritesBox.ItemsSource = App.DbManager.GetUserFavorites();
                    UserHiddenBox.ItemsSource = App.DbManager.GetUserHidden();
                    TrainersBox.ItemsSource = App.DbManager.GetTrainers();
                }));
                SendStatusChanged("Finished loading Settings DB...");
            }
            catch(Exception ex) {
                App.SaveException(ex);
                SendStatusChanged("Failed loading Settings DB... Check error.log for more information...");
            }
        }

        private void SendStatusChanged(string msg, params object[] param) {
            var handler = App.StatusChanged;
            if(handler != null)
                handler(this, new StatusEventArgs(string.Format(msg, param)));
        }

        private void ReloadDataGrid(System.Windows.Controls.DataGrid dataGrid) {
            try {
                // 重新加载对应的数据集合
                if(dataGrid == SystemSettingsBox) {
                    SystemSettingsBox.ItemsSource = null;
                    SystemSettingsBox.ItemsSource = App.DbManager.GetSystemSettings();
                }
                else if(dataGrid == UserSettingsBox) {
                    UserSettingsBox.ItemsSource = null;
                    UserSettingsBox.ItemsSource = App.DbManager.GetUserSettings();
                }
                else if(dataGrid == ScanPathsBox) {
                    ScanPathsBox.ItemsSource = null;
                    ScanPathsBox.ItemsSource = App.DbManager.GetScanPaths();
                }
                else if(dataGrid == ProfilesBox) {
                    ProfilesBox.ItemsSource = null;
                    ProfilesBox.ItemsSource = App.DbManager.GetProfiles();
                }
                else if(dataGrid == TrainersBox) {
                    TrainersBox.ItemsSource = null;
                    TrainersBox.ItemsSource = App.DbManager.GetTrainers();
                }
                else if(dataGrid == UserFavoritesBox) {
                    UserFavoritesBox.ItemsSource = null;
                    UserFavoritesBox.ItemsSource = App.DbManager.GetUserFavorites();
                }
                else if(dataGrid == UserHiddenBox) {
                    UserHiddenBox.ItemsSource = null;
                    UserHiddenBox.ItemsSource = App.DbManager.GetUserHidden();
                }
                else if(dataGrid == QuickViewsBox) {
                    QuickViewsBox.ItemsSource = null;
                    QuickViewsBox.ItemsSource = App.DbManager.GetQuickViews();
                }
            }
            catch(Exception ex) {
                App.SaveException(ex);
                System.Diagnostics.Debug.WriteLine($"ReloadDataGrid failed: {ex.Message}");
            }
        }

        private void DataGrid_RowEditEnding(object sender, System.Windows.Controls.DataGridRowEditEndingEventArgs e) {
            var dataGrid = sender as System.Windows.Controls.DataGrid;
            if(dataGrid == null || e.Row.Item == null)
                return;

            try {
                // 添加调试日志
                System.Diagnostics.Debug.WriteLine($"DataGrid_RowEditEnding: Editing item of type {e.Row.Item.GetType().Name}");
                // 在没有调试器的情况下也可以看到消息
                #if DEBUG
                // System.Windows.MessageBox.Show($"Editing item of type {e.Row.Item.GetType().Name}", "Debug Info", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                #endif
                
                // 延迟保存操作，确保所有编辑都已完成
                Dispatcher.BeginInvoke(new Action(() => {
                    try {
                        App.DbManager.SaveSettingsChanges(); // 保存所有更改到数据库
                        
                        // 显示成功消息
                        string statusMessage = "";
                        if(e.Row.Item is Database.ProfileItem) {
                            statusMessage = "Profile updated successfully";
                        }
                        else if(e.Row.Item is Database.ScanPathItem) {
                            statusMessage = "Scan path updated successfully";
                        }
                        else if(e.Row.Item is Database.SystemSettingItem) {
                            statusMessage = "System setting updated successfully";
                        }
                        else if(e.Row.Item is Database.UserSettingItem) {
                            statusMessage = "User setting updated successfully";
                        }
                        else if(e.Row.Item is Database.QuickViewItem) {
                            statusMessage = "Quick view updated successfully";
                        }
                        else if(e.Row.Item is Database.UserFavoriteItem) {
                            statusMessage = "User favorite updated successfully";
                        }
                        else if(e.Row.Item is Database.UserHiddenItem) {
                            statusMessage = "User hidden item updated successfully";
                        }
                        else if(e.Row.Item is Database.TrainerItem) {
                            statusMessage = "Trainer updated successfully";
                        }
                        
                        if (!string.IsNullOrEmpty(statusMessage)) {
                            SendStatusChanged(statusMessage);
                            System.Diagnostics.Debug.WriteLine($"DataGrid_RowEditEnding: {statusMessage}");
                            #if DEBUG
                            // System.Windows.MessageBox.Show(statusMessage, "Debug Info", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                            #endif
                        }
                        
                        System.Diagnostics.Debug.WriteLine("DataGrid_RowEditEnding: Completed successfully");
                    }
                    catch(Exception ex) {
                        System.Diagnostics.Debug.WriteLine($"DataGrid_RowEditEnding (Delayed): Exception occurred: {ex.Message}");
                        App.SaveException(ex);
                        SendStatusChanged("Failed to update item: " + ex.Message);
                        #if DEBUG
                        // System.Windows.MessageBox.Show("Failed to update item: " + ex.Message, "Debug Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        #endif
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
            catch(Exception ex) {
                System.Diagnostics.Debug.WriteLine($"DataGrid_RowEditEnding: Exception occurred: {ex.Message}");
                App.SaveException(ex);
                SendStatusChanged("Failed to update item: " + ex.Message);
                #if DEBUG
                // System.Windows.MessageBox.Show("Failed to update item: " + ex.Message, "Debug Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                #endif
            }
        }

        private void DataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            var dataGrid = sender as System.Windows.Controls.DataGrid;
            if(dataGrid == null)
                return;

            // 获取右键点击的行
            var hitTestResult = VisualTreeHelper.HitTest(dataGrid, e.GetPosition(dataGrid));
            if(hitTestResult != null) {
                var dataGridRow = FindParentDataGridRow(hitTestResult.VisualHit);
                if(dataGridRow != null) {
                    dataGridRow.IsSelected = true;
                }
            }
        }

        private void DataGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e) {
            var dataGrid = sender as System.Windows.Controls.DataGrid;
            if(dataGrid == null)
                return;

            // 获取当前选中的项
            var selectedItem = dataGrid.SelectedItem;
            if(selectedItem == null) {
                e.Handled = true; // 如果没有选中项，不显示删除菜单
                return;
            }
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e) {
            var menuItem = sender as System.Windows.Controls.MenuItem;
            if(menuItem == null)
                return;

            var contextMenu = menuItem.Parent as System.Windows.Controls.ContextMenu;
            if(contextMenu == null)
                return;

            var dataGrid = contextMenu.PlacementTarget as System.Windows.Controls.DataGrid;
            if(dataGrid == null || dataGrid.SelectedItem == null)
                return;

            try {
                if(dataGrid.SelectedItem is Database.QuickViewItem quickViewItem) {
                    App.DbManager.DeleteQuickView(quickViewItem);
                    // 刷新UI显示
                    dataGrid.ItemsSource = App.DbManager.GetQuickViews();
                    SendStatusChanged("Quick view deleted successfully");
                }
                else if(dataGrid.SelectedItem is Database.UserFavoriteItem userFavoriteItem) {
                    App.DbManager.DeleteUserFavorite(userFavoriteItem);
                    // 刷新UI显示
                    dataGrid.ItemsSource = App.DbManager.GetUserFavorites();
                    SendStatusChanged("User favorite deleted successfully");
                }
                else if(dataGrid.SelectedItem is Database.UserHiddenItem userHiddenItem) {
                    App.DbManager.DeleteUserHidden(userHiddenItem);
                    // 刷新UI显示
                    dataGrid.ItemsSource = App.DbManager.GetUserHidden();
                    SendStatusChanged("User hidden item deleted successfully");
                }
                else if(dataGrid.SelectedItem is Database.TrainerItem trainerItem) {
                    App.DbManager.DeleteTrainer(trainerItem);
                    // 刷新UI显示
                    dataGrid.ItemsSource = App.DbManager.GetTrainers();
                    SendStatusChanged("Trainer deleted successfully");
                }
            }
            catch(Exception ex) {
                App.SaveException(ex);
                SendStatusChanged("Failed to delete item: " + ex.Message);
            }
        }

        private void AddGenresButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 检查数据库连接
                if (App.DbManager == null || !App.DbManager.IsSettingsOpen)
                {
                    MessageBox.Show("请先加载设置数据库", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string luaFolderPath = Path.Combine(currentDirectory, "LUA");
                string genresCnPath = Path.Combine(currentDirectory, "genres_cn.txt");

                // 检查LUA文件夹是否存在
                if (!Directory.Exists(luaFolderPath))
                {
                    MessageBox.Show("LUA文件夹不存在", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 检查genres_cn.txt文件是否存在
                if (!File.Exists(genresCnPath))
                {
                    MessageBox.Show("genres_cn.txt文件不存在", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 读取genres_cn.txt文件，建立文件名到中文名称的映射
                Dictionary<string, string> nameMap = new Dictionary<string, string>();
                foreach (string line in File.ReadAllLines(genresCnPath, Encoding.UTF8))
                {
                    string[] parts = line.Split(new[] { '=' }, 2);
                    if (parts.Length == 2)
                    {
                        string fileName = parts[0].Trim();
                        string cnName = parts[1].Trim();
                        nameMap[fileName] = cnName;
                    }
                }

                // 获取第一个非空白的Creator XUID
                string creatorXuid = "";
                var quickViews = App.DbManager.GetQuickViews().ToList();
                if (quickViews != null && quickViews.Count > 0)
                {
                    foreach (var item in quickViews)
                    {
                        if (!string.IsNullOrEmpty(item.CreatorXUID))
                        {
                            creatorXuid = item.CreatorXUID;
                            break;
                        }
                    }
                }
                
                // 如果没有找到，使用默认值
                if (string.IsNullOrEmpty(creatorXuid))
                {
                    creatorXuid = "0000000000000000";
                }

                // 处理每个.lua文件
                int addedCount = 0;
                int skippedCount = 0;
                foreach (string luaFilePath in Directory.GetFiles(luaFolderPath, "*.lua"))
                {
                    string fileName = Path.GetFileNameWithoutExtension(luaFilePath);
                    string filterMethod = $"User.{fileName}";
                    string displayName = nameMap.ContainsKey(fileName) ? nameMap[fileName] : fileName;
                    
                    // 检查是否已经存在相同的FilterMethod
                    bool exists = false;
                    var allQuickViews = App.DbManager.GetQuickViews().ToList();
                    foreach (var item in allQuickViews)
                    {
                        if (item.FilterMethod == filterMethod)
                        {
                            exists = true;
                            break;
                        }
                    }
                    if (exists)
                    {
                        skippedCount++;
                        continue; // 跳过已存在的项目
                    }

                    // 创建新的QuickViewItem
                    DataTable tempTable = new DataTable();
                    tempTable.Columns.Add("Id", typeof(long));
                    tempTable.Columns.Add("DisplayName", typeof(string));
                    tempTable.Columns.Add("SortMethod", typeof(string));
                    tempTable.Columns.Add("FilterMethod", typeof(string));
                    tempTable.Columns.Add("Flags", typeof(int));
                    tempTable.Columns.Add("CreatorXUID", typeof(string));
                    tempTable.Columns.Add("OrderIndex", typeof(int));
                    tempTable.Columns.Add("IconHash", typeof(string));

                    DataRow row = tempTable.NewRow();
                    row["Id"] = 0;
                    row["DisplayName"] = displayName;
                    row["SortMethod"] = "Title Name";
                    row["FilterMethod"] = filterMethod;
                    row["Flags"] = 0;
                    row["CreatorXUID"] = creatorXuid;
                    row["OrderIndex"] = 0;
                    row["IconHash"] = "";
                    tempTable.Rows.Add(row);

                    QuickViewItem newItem = new QuickViewItem(row);
                    App.DbManager.AddQuickView(newItem);
                    addedCount++;
                }

                // 重新加载数据网格
                ReloadDataGrid(QuickViewsBox);
                
                // 显示结果
                MessageBox.Show($"已成功添加{addedCount}个游戏分类。跳过{skippedCount}个已存在的分类。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"添加游戏分类时发生错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddItem_Click(object sender, RoutedEventArgs e) {
            var menuItem = sender as System.Windows.Controls.MenuItem;
            if(menuItem == null)
                return;

            var contextMenu = menuItem.Parent as System.Windows.Controls.ContextMenu;
            if(contextMenu == null)
                return;

            var dataGrid = contextMenu.PlacementTarget as System.Windows.Controls.DataGrid;
            if(dataGrid == null)
                return;

            try {
                if(dataGrid.Name == "QuickViewsBox") {
                    // 创建一个新的DataTable来容纳新行
                    var table = new System.Data.DataTable("QuickViews");
                    // 添加必要的列
                    table.Columns.Add("Id", typeof(long));
                    table.Columns.Add("DisplayName", typeof(string));
                    table.Columns.Add("SortMethod", typeof(string));
                    table.Columns.Add("FilterMethod", typeof(string));
                    table.Columns.Add("Flags", typeof(long));
                    table.Columns.Add("CreatorXUID", typeof(string));
                    table.Columns.Add("OrderIndex", typeof(long));
                    table.Columns.Add("IconHash", typeof(string));
                    
                    // 创建新行
                    var newRow = table.NewRow();
                    newRow["Id"] = 0; // 临时ID，实际ID将在数据库中分配
                    newRow["DisplayName"] = "New Quick View";
                    newRow["SortMethod"] = "Default";
                    newRow["FilterMethod"] = "None";
                    newRow["Flags"] = 0;
                    newRow["CreatorXUID"] = "0";
                    newRow["OrderIndex"] = 0;
                    newRow["IconHash"] = "0";
                    
                    // 添加新的QuickView条目
                    var newItem = new Database.QuickViewItem(newRow);
                    
                    App.DbManager.AddQuickView(newItem);
                    // 刷新UI显示
                    dataGrid.ItemsSource = App.DbManager.GetQuickViews();
                    SendStatusChanged("Quick view added successfully");
                }
                else if(dataGrid.Name == "UserFavoritesBox") {
                    // 创建一个新的DataTable来容纳新行
                    var table = new System.Data.DataTable("UserFavorites");
                    // 添加必要的列
                    table.Columns.Add("Id", typeof(long));
                    table.Columns.Add("ContentId", typeof(long));
                    table.Columns.Add("ProfileId", typeof(string));
                    
                    // 创建新行
                    var newRow = table.NewRow();
                    newRow["Id"] = 0; // 临时ID，实际ID将在数据库中分配
                    newRow["ContentId"] = 0;
                    newRow["ProfileId"] = "0";
                    
                    // 添加新的UserFavorite条目
                    var newItem = new Database.UserFavoriteItem(newRow);
                    
                    App.DbManager.AddUserFavorite(newItem);
                    // 刷新UI显示
                    dataGrid.ItemsSource = App.DbManager.GetUserFavorites();
                    SendStatusChanged("User favorite added successfully");
                }
                else if(dataGrid.Name == "UserHiddenBox") {
                    // 创建一个新的DataTable来容纳新行
                    var table = new System.Data.DataTable("UserHidden");
                    // 添加必要的列
                    table.Columns.Add("Id", typeof(long));
                    table.Columns.Add("ContentId", typeof(long));
                    table.Columns.Add("ProfileId", typeof(string));
                    
                    // 创建新行
                    var newRow = table.NewRow();
                    newRow["Id"] = 0; // 临时ID，实际ID将在数据库中分配
                    newRow["ContentId"] = 0;
                    newRow["ProfileId"] = "0";
                    
                    // 添加新的UserHidden条目
                    var newItem = new Database.UserHiddenItem(newRow);
                    
                    App.DbManager.AddUserHidden(newItem);
                    // 刷新UI显示
                    dataGrid.ItemsSource = App.DbManager.GetUserHidden();
                    SendStatusChanged("User hidden item added successfully");
                }
                else if(dataGrid.Name == "TrainersBox") {
                    // 创建一个新的DataTable来容纳新行
                    var table = new System.Data.DataTable("Trainers");
                    // 添加必要的列
                    table.Columns.Add("Id", typeof(long));
                    table.Columns.Add("TitleId", typeof(string));
                    table.Columns.Add("MediaId", typeof(string));
                    table.Columns.Add("TrainerPath", typeof(string));
                    table.Columns.Add("TrainerName", typeof(string));
                    table.Columns.Add("TrainerVersion", typeof(long));
                    table.Columns.Add("TrainerData", typeof(string));
                    table.Columns.Add("TrainerInfo", typeof(string));
                    table.Columns.Add("TrainerAuthor", typeof(string));
                    table.Columns.Add("TrainerRating", typeof(double));
                    table.Columns.Add("TrainerFlags", typeof(long));
                    table.Columns.Add("CreatorXUID", typeof(string));
                    
                    // 创建新行
                    var newRow = table.NewRow();
                    newRow["Id"] = 0; // 临时ID，实际ID将在数据库中分配
                    newRow["TitleId"] = "0";
                    newRow["MediaId"] = "0";
                    newRow["TrainerPath"] = "";
                    newRow["TrainerName"] = "New Trainer";
                    newRow["TrainerVersion"] = 0;
                    newRow["TrainerData"] = "";
                    newRow["TrainerInfo"] = "";
                    newRow["TrainerAuthor"] = "";
                    newRow["TrainerRating"] = 0.0;
                    newRow["TrainerFlags"] = 0;
                    newRow["CreatorXUID"] = "0";
                    
                    // 添加新的Trainer条目
                    var newItem = new Database.TrainerItem(newRow);
                    
                    App.DbManager.AddTrainer(newItem);
                    // 刷新UI显示
                    dataGrid.ItemsSource = App.DbManager.GetTrainers();
                    SendStatusChanged("Trainer added successfully");
                }
            }
            catch(Exception ex) {
                App.SaveException(ex);
                SendStatusChanged("Failed to add item: " + ex.Message);
            }
        }

        private System.Windows.Controls.DataGridRow FindParentDataGridRow(DependencyObject element) {
            while(element != null && !(element is System.Windows.Controls.DataGridRow)) {
                element = VisualTreeHelper.GetParent(element);
            }
            return element as System.Windows.Controls.DataGridRow;
        }
    }
}