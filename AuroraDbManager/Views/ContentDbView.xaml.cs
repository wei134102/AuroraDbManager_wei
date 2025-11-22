// 
// 	ContentDbView.xaml.cs
// 	AuroraDbManager
// 
// 	Created by Swizzy on 23/05/2015
// 	Copyright (c) 2015 Swizzy. All rights reserved.

using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AuroraDbManager.Classes;
using AuroraDbManager.Database;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;

namespace AuroraDbManager.Views {
    /// <summary>
    ///     Interaction logic for ContentDbView.xaml
    /// </summary>
    public partial class ContentDbView : UserControl {
        // 添加本地化管理器
        private ContentLocalizationManager _localizationManager;
        private bool _isLocalizedEnabled = false;
        
        public ContentDbView() { 
            InitializeComponent(); 
            
            // 初始化本地化管理器
            string xboxGamesDbPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), 
                "xbox_games.db");
            _localizationManager = new ContentLocalizationManager(xboxGamesDbPath);
            
            // 更新UI状态
            UpdateLocalizationUI();
        }
        
        /// <summary>
        /// 本地化功能是否可用
        /// </summary>
        public bool IsLocalizedEnabled
        {
            get { return _isLocalizedEnabled; }
            set 
            { 
                _isLocalizedEnabled = value;
                UpdateLocalizationUI();
            }
        }

        public void OpenDb(string filename = null) {
            try {
                if(string.IsNullOrWhiteSpace(filename)) {
                    var ofd = new OpenFileDialog();
                    if(ofd.ShowDialog() != true)
                        return;
                    filename = ofd.FileName;
                }
                SendStatusChanged("Loading {0}...", filename);
                App.DbManager.ConnectToContent(filename);
                var contentItems = App.DbManager.GetContentItems();
                
                // 应用本地化（如果可用）
                if (_localizationManager.IsXboxGamesDbAvailable()) {
                    SendStatusChanged("正在本地化内容数据库...");
                    int localizedCount = _localizationManager.LocalizeContentItems(contentItems);
                    SendStatusChanged($"已完成本地化 {localizedCount} 个项目");
                }
                
                Dispatcher.Invoke(new Action(() => ContentDbViewBox.ItemsSource = contentItems));
                Dispatcher.Invoke(new Action(() => TitleUpdatesDbViewBox.ItemsSource = App.DbManager.GetTitleUpdateItems()));
                SendStatusChanged("Finished loading Content DB...");
                
                // 更新本地化功能状态
                IsLocalizedEnabled = (contentItems.Count() > 0) && _localizationManager.IsXboxGamesDbAvailable();
            }
            catch(Exception ex) {
                App.SaveException(ex);
                SendStatusChanged("Failed loading Content DB... Check error.log for more information...");
            }
        }

        private void DbViewChanged(object sender, SelectionChangedEventArgs e) {
            //TODO: Open Editor
        }

        private void SendStatusChanged(string msg, params object[] param) {
            var handler = App.StatusChanged;
            if(handler != null)
                handler(this, new StatusEventArgs(string.Format(msg, param)));
        }
        
        /// <summary>
        /// 更新本地化UI状态
        /// </summary>
        private void UpdateLocalizationUI()
        {
            if (LocalizeButton != null)
            {
                LocalizeButton.IsEnabled = _isLocalizedEnabled;
            }
            
            if (LocalizationStatusText != null)
            {
                if (!_localizationManager.IsXboxGamesDbAvailable())
                {
                    LocalizationStatusText.Text = "本地化数据库不可用";
                }
                else if (!_isLocalizedEnabled)
                {
                    LocalizationStatusText.Text = "无内容可本地化";
                }
                else
                {
                    LocalizationStatusText.Text = "本地化功能就绪";
                }
            }
        }
        
        /// <summary>
        /// 本地化按钮点击事件
        /// </summary>
        private void LocalizeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ContentDbViewBox.ItemsSource is IEnumerable<ContentItem> contentItems)
                {
                    SendStatusChanged("正在本地化内容数据库...");
                    int localizedCount = _localizationManager.LocalizeContentItems(contentItems);
                    
                    // 刷新UI
                    ContentDbViewBox.Items.Refresh();
                    
                    SendStatusChanged($"已完成本地化 {localizedCount} 个项目");
                }
                else
                {
                    SendStatusChanged("没有可本地化的内容");
                }
            }
            catch (Exception ex)
            {
                App.SaveException(ex);
                SendStatusChanged($"本地化失败: {ex.Message}");
            }
        }

        private void ContentDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e) {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null || e.Row.Item == null)
                return;

            try {
                // 添加调试日志
                System.Diagnostics.Debug.WriteLine($"ContentDataGrid_RowEditEnding: Editing item of type {e.Row.Item.GetType().Name}");

                // 延迟保存操作，确保所有编辑都已完成
                Dispatcher.BeginInvoke(new Action(() => {
                    try {
                        // 保存所有更改到数据库
                        App.DbManager.SaveContentChanges();
                        
                        // 显示成功消息
                        SendStatusChanged("Content item updated successfully");
                        System.Diagnostics.Debug.WriteLine("ContentDataGrid_RowEditEnding: Completed successfully");
                    }
                    catch (Exception ex) {
                        System.Diagnostics.Debug.WriteLine($"ContentDataGrid_RowEditEnding (Delayed): Exception occurred: {ex.Message}");
                        App.SaveException(ex);
                        SendStatusChanged("Failed to update content item: " + ex.Message);
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"ContentDataGrid_RowEditEnding: Exception occurred: {ex.Message}");
                App.SaveException(ex);
                SendStatusChanged("Failed to update content item: " + ex.Message);
            }
        }

        private void TitleUpdateDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e) {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null || e.Row.Item == null)
                return;

            try {
                // 添加调试日志
                System.Diagnostics.Debug.WriteLine($"TitleUpdateDataGrid_RowEditEnding: Editing item of type {e.Row.Item.GetType().Name}");

                // 延迟保存操作，确保所有编辑都已完成
                Dispatcher.BeginInvoke(new Action(() => {
                    try {
                        // 保存所有更改到数据库
                        App.DbManager.SaveContentChanges();
                        
                        // 显示成功消息
                        SendStatusChanged("Title update item updated successfully");
                        System.Diagnostics.Debug.WriteLine("TitleUpdateDataGrid_RowEditEnding: Completed successfully");
                    }
                    catch (Exception ex) {
                        System.Diagnostics.Debug.WriteLine($"TitleUpdateDataGrid_RowEditEnding (Delayed): Exception occurred: {ex.Message}");
                        App.SaveException(ex);
                        SendStatusChanged("Failed to update title update item: " + ex.Message);
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"TitleUpdateDataGrid_RowEditEnding: Exception occurred: {ex.Message}");
                App.SaveException(ex);
                SendStatusChanged("Failed to update title update item: " + ex.Message);
            }
        }

        private void DataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null)
                return;

            // 获取右键点击的行
            var hitTestResult = VisualTreeHelper.HitTest(dataGrid, e.GetPosition(dataGrid));
            if (hitTestResult != null) {
                var dataGridRow = FindParentDataGridRow(hitTestResult.VisualHit);
                if (dataGridRow != null) {
                    dataGridRow.IsSelected = true;
                }
            }
        }

        private void DataGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e) {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null)
                return;

            // 获取当前选中的项
            var selectedItem = dataGrid.SelectedItem;
            if (selectedItem == null) {
                e.Handled = true; // 如果没有选中项，不显示删除菜单
            }
        }

        private DataGridRow FindParentDataGridRow(System.Windows.DependencyObject child) {
            System.Windows.DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            if (parentObject is DataGridRow row) {
                return row;
            }
            else {
                return FindParentDataGridRow(parentObject);
            }
        }

        private void DeleteContentItem_Click(object sender, RoutedEventArgs e) {
            var selectedItem = ContentDbViewBox.SelectedItem as ContentItem;
            if (selectedItem == null) {
                SendStatusChanged("No content item selected for deletion");
                return;
            }

            try {
                // 确认删除操作
                var result = MessageBox.Show($"Are you sure you want to delete content item '{selectedItem.TitleName}' (ID: {selectedItem.Id})?", 
                    "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes) {
                    // 执行删除操作
                    App.DbManager.DeleteContentItem(selectedItem);
                    SendStatusChanged("Content item deleted successfully");
                    
                    // 更新UI
                    var items = App.DbManager.GetContentItems();
                    Dispatcher.Invoke(new Action(() => ContentDbViewBox.ItemsSource = items));
                }
            }
            catch (Exception ex) {
                App.SaveException(ex);
                SendStatusChanged("Failed to delete content item: " + ex.Message);
            }
        }

        private void DeleteTitleUpdateItem_Click(object sender, RoutedEventArgs e) {
            var selectedItem = TitleUpdatesDbViewBox.SelectedItem as TitleUpdateItem;
            if (selectedItem == null) {
                SendStatusChanged("No title update item selected for deletion");
                return;
            }

            try {
                // 确认删除操作
                var result = MessageBox.Show($"Are you sure you want to delete title update item '{selectedItem.DisplayName}' (ID: {selectedItem.Id})?", 
                    "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes) {
                    // 执行删除操作
                    App.DbManager.DeleteTitleUpdateItem(selectedItem);
                    SendStatusChanged("Title update item deleted successfully");
                    
                    // 更新UI
                    var items = App.DbManager.GetTitleUpdateItems();
                    Dispatcher.Invoke(new Action(() => TitleUpdatesDbViewBox.ItemsSource = items));
                }
            }
            catch (Exception ex) {
                App.SaveException(ex);
                SendStatusChanged("Failed to delete title update item: " + ex.Message);
            }
        }

        private void AddContentItem_Click(object sender, RoutedEventArgs e) {
            try {
                // 注意：这里应该打开一个对话框让用户输入新项的数据
                // 为简化起见，我们创建一个默认的新项
                MessageBox.Show("添加新内容项功能需要实现一个对话框来输入数据", "功能提示", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // 示例代码（需要根据实际需求调整）：
                // var newItem = new ContentItem(/* 初始化DataRow */);
                // App.DbManager.AddContentItem(newItem);
                // SendStatusChanged("New content item added successfully");
                // 
                // // 更新UI
                // var items = App.DbManager.GetContentItems();
                // Dispatcher.Invoke(new Action(() => ContentDbViewBox.ItemsSource = items));
            }
            catch (Exception ex) {
                App.SaveException(ex);
                SendStatusChanged("Failed to add new content item: " + ex.Message);
            }
        }

        private void AddTitleUpdateItem_Click(object sender, RoutedEventArgs e) {
            try {
                // 注意：这里应该打开一个对话框让用户输入新项的数据
                // 为简化起见，我们创建一个默认的新项
                MessageBox.Show("添加新标题更新项功能需要实现一个对话框来输入数据", "功能提示", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // 示例代码（需要根据实际需求调整）：
                // var newItem = new TitleUpdateItem(/* 初始化DataRow */);
                // App.DbManager.AddTitleUpdateItem(newItem);
                // SendStatusChanged("New title update item added successfully");
                //
                // // 更新UI
                // var items = App.DbManager.GetTitleUpdateItems();
                // Dispatcher.Invoke(new Action(() => TitleUpdatesDbViewBox.ItemsSource = items));
            }
            catch (Exception ex) {
                App.SaveException(ex);
                SendStatusChanged("Failed to add new title update item: " + ex.Message);
            }
        }
    }
}