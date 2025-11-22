// 
// 	SettingsDbView.xaml.cs
// 	AuroraDbManager
// 
// 	Created by Swizzy on 23/05/2015
// 	Copyright (c) 2015 Swizzy. All rights reserved.

using System;
using System.IO;
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
                // DataGrid编辑已经直接修改了底层DataRow，并且属性setter已经设置了Changed=true
                // 我们只需要保存更改即可，不需要调用UpdateXxx方法
                App.DbManager.SaveSettingsChanges(); // 保存所有更改到数据库
                
                // 重新加载数据以确保界面显示最新数据
                ReloadDataGrid(dataGrid);
                
                // 显示成功消息
                if(e.Row.Item is Database.ProfileItem) {
                    SendStatusChanged("Profile updated successfully");
                }
                else if(e.Row.Item is Database.ScanPathItem) {
                    SendStatusChanged("Scan path updated successfully");
                }
                else if(e.Row.Item is Database.SystemSettingItem) {
                    SendStatusChanged("System setting updated successfully");
                }
                else if(e.Row.Item is Database.UserSettingItem) {
                    SendStatusChanged("User setting updated successfully");
                }
                else if(e.Row.Item is Database.QuickViewItem) {
                    SendStatusChanged("Quick view updated successfully");
                }
                else if(e.Row.Item is Database.UserFavoriteItem) {
                    SendStatusChanged("User favorite updated successfully");
                }
                else if(e.Row.Item is Database.UserHiddenItem) {
                    SendStatusChanged("User hidden item updated successfully");
                }
                else if(e.Row.Item is Database.TrainerItem) {
                    SendStatusChanged("Trainer updated successfully");
                }
            }
            catch(Exception ex) {
                App.SaveException(ex);
                SendStatusChanged("Failed to update item: " + ex.Message);
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
                    dataGrid.ItemsSource = App.DbManager.GetQuickViews();
                    SendStatusChanged("Quick view deleted successfully");
                }
                else if(dataGrid.SelectedItem is Database.UserFavoriteItem userFavoriteItem) {
                    App.DbManager.DeleteUserFavorite(userFavoriteItem);
                    dataGrid.ItemsSource = App.DbManager.GetUserFavorites();
                    SendStatusChanged("User favorite deleted successfully");
                }
                else if(dataGrid.SelectedItem is Database.UserHiddenItem userHiddenItem) {
                    App.DbManager.DeleteUserHidden(userHiddenItem);
                    dataGrid.ItemsSource = App.DbManager.GetUserHidden();
                    SendStatusChanged("User hidden item deleted successfully");
                }
                else if(dataGrid.SelectedItem is Database.TrainerItem trainerItem) {
                    App.DbManager.DeleteTrainer(trainerItem);
                    dataGrid.ItemsSource = App.DbManager.GetTrainers();
                    SendStatusChanged("Trainer deleted successfully");
                }
            }
            catch(Exception ex) {
                App.SaveException(ex);
                SendStatusChanged("Failed to delete item: " + ex.Message);
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
                // 使用反射调用私有方法插入数据，然后重新加载
                var method = typeof(AuroraDbManager.Database.AuroraDbManager).GetMethod("ExecuteNonQuerySettings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if(dataGrid.Name == "QuickViewsBox") {
                    var sql = "INSERT INTO QuickViews (DisplayName, SortMethod, FilterMethod, Flags, CreatorXUID, OrderIndex, IconHash) VALUES ('New Quick View', 'Default', 'None', 0, '0', 0, '0')";
                    method?.Invoke(App.DbManager, new object[] { sql });
                    SendStatusChanged("Quick view added successfully");
                }
                else if(dataGrid.Name == "UserFavoritesBox") {
                    var sql = "INSERT INTO UserFavorites (ContentId, ProfileId) VALUES (0, '0')";
                    method?.Invoke(App.DbManager, new object[] { sql });
                    SendStatusChanged("User favorite added successfully");
                }
                else if(dataGrid.Name == "UserHiddenBox") {
                    var sql = "INSERT INTO UserHidden (ContentId, ProfileId) VALUES (0, '0')";
                    method?.Invoke(App.DbManager, new object[] { sql });
                    SendStatusChanged("User hidden item added successfully");
                }
                else if(dataGrid.Name == "TrainersBox") {
                    var sql = "INSERT INTO Trainers (TitleId, MediaId, TrainerPath, TrainerName, TrainerVersion, TrainerData, TrainerInfo, TrainerAuthor, TrainerRating, TrainerFlags, CreatorXUID) VALUES ('0', '0', '', 'New Trainer', 1, '', '', '', 0, 0, '0')";
                    method?.Invoke(App.DbManager, new object[] { sql });
                    SendStatusChanged("Trainer added successfully");
                }

                // 重新加载数据 - 不显示文件对话框，直接刷新当前DataGrid的数据源
                if(dataGrid.Name == "QuickViewsBox") {
                    dataGrid.ItemsSource = App.DbManager.GetQuickViews();
                }
                else if(dataGrid.Name == "UserFavoritesBox") {
                    dataGrid.ItemsSource = App.DbManager.GetUserFavorites();
                }
                else if(dataGrid.Name == "UserHiddenBox") {
                    dataGrid.ItemsSource = App.DbManager.GetUserHidden();
                }
                else if(dataGrid.Name == "TrainersBox") {
                    dataGrid.ItemsSource = App.DbManager.GetTrainers();
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