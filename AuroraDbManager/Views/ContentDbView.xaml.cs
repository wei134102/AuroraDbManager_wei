// 
// 	ContentDbView.xaml.cs
// 	AuroraDbManager
// 
// 	Created by Swizzy on 23/05/2015
// 	Copyright (c) 2015 Swizzy. All rights reserved.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AuroraDbManager.Classes;
using AuroraDbManager.Database;
using Microsoft.Win32;

namespace AuroraDbManager.Views {
    /// <summary>
    ///     Interaction logic for ContentDbView.xaml
    /// </summary>
    public partial class ContentDbView : UserControl {
        public ContentDbView() { InitializeComponent(); }

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
                Dispatcher.Invoke(new Action(() => ContentDbViewBox.ItemsSource = App.DbManager.GetContentItems()));
                Dispatcher.Invoke(new Action(() => TitleUpdatesDbViewBox.ItemsSource = App.DbManager.GetTitleUpdateItems()));
                SendStatusChanged("Finished loading Content DB...");
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

        private DataGridRow FindParentDataGridRow(DependencyObject child) {
            while (child != null && !(child is DataGridRow)) {
                child = VisualTreeHelper.GetParent(child);
            }
            return child as DataGridRow;
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