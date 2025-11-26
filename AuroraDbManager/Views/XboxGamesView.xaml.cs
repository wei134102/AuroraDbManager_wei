// 
// XboxGamesView.xaml.cs
// AuroraDbManager
//
// Created by Assistant on 2025/11/22
// Copyright © 2025 All rights reserved.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using AuroraDbManager.Database;
using Microsoft.Win32;
using DbManager = AuroraDbManager.Database.AuroraDbManager;

namespace AuroraDbManager.Views
{
    /// <summary>
    /// XboxGamesView.xaml 的交互逻辑
    /// </summary>
    public partial class XboxGamesView : UserControl
    {
        private DbManager _dbManager;
        private string _currentDbPath;
        private List<XboxGameItem> _allGames;
        
        public XboxGamesView()
        {
            InitializeComponent();
            _dbManager = new DbManager();
            _allGames = new List<XboxGameItem>();
            SearchTextBox.GotFocus += SearchTextBox_GotFocus;
            SearchTextBox.LostFocus += SearchTextBox_LostFocus;
            UpdatePlaceholderText();
            
            // 自动加载程序目录下的xbox_games.db文件
            Loaded += XboxGamesView_Loaded;
        }

        private void XboxGamesView_Loaded(object sender, RoutedEventArgs e)
        {
            // 获取程序目录路径
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string dbPath = Path.Combine(appDirectory, "xbox_games.db");
            
            // 检查数据库文件是否存在
            if (File.Exists(dbPath))
            {
                LoadDatabase(dbPath);
            }
            else
            {
                // 文件不存在，显示提示弹窗
                MessageBox.Show("未找到xbox_games.db数据库文件。请确保该文件存在于程序目录中，或使用'浏览数据库...'按钮手动选择数据库文件。", 
                    "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == "输入游戏名称进行搜索...")
            {
                SearchTextBox.Text = "";
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderText();
        }

        private void UpdatePlaceholderText()
        {
            if (string.IsNullOrEmpty(SearchTextBox.Text))
            {
                SearchTextBox.Text = "输入游戏标题ID、英文标题或中文标题进行搜索...";
            }
        }

        /// <summary>
        /// 浏览并加载数据库文件
        /// </summary>
        private void BrowseDbButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "SQLite数据库文件 (*.db)|*.db|所有文件 (*.*)|*.*",
                Title = "选择Xbox游戏数据库文件"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                LoadDatabase(openFileDialog.FileName);
            }
        }

        /// <summary>
        /// 加载数据库文件
        /// </summary>
        /// <param name="dbPath">数据库文件路径</param>
        private void LoadDatabase(string dbPath)
        {
            try
            {
                _currentDbPath = dbPath;
                _allGames = _dbManager.GetXboxGames(dbPath);
                
                // 更新UI
                GamesDataGrid.ItemsSource = _allGames;
                GameCountTextBlock.Text = $"游戏总数: {_allGames.Count}";
                
                // 如果有数据，自动选择第一项
                if (_allGames.Count > 0)
                {
                    GamesDataGrid.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载数据库时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 搜索文本框内容变化事件
        /// </summary>
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PerformSearch();
        }

        /// <summary>
        /// 搜索选项变化事件
        /// </summary>
        private void SearchOptionsChanged(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        /// <summary>
        /// 执行搜索
        /// </summary>
        private void PerformSearch()
        {
            if (_allGames == null || _allGames.Count == 0 || string.IsNullOrEmpty(_currentDbPath))
                return;

            string searchTerm = SearchTextBox.Text;
            
            // 如果是占位符文本，则不进行搜索
            if (searchTerm == "输入游戏标题ID、英文标题或中文标题进行搜索...")
            {
                GamesDataGrid.ItemsSource = _allGames;
                return;
            }

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                // 如果搜索词为空，显示所有游戏
                GamesDataGrid.ItemsSource = _allGames;
                GameCountTextBlock.Text = $"游戏总数: {_allGames.Count}";
            }
            else
            {
                // 获取选中的搜索类型
                ComboBoxItem selectedItem = SearchTypeComboBox.SelectedItem as ComboBoxItem;
                string searchType = selectedItem?.Tag?.ToString() ?? "all";
                
                // 执行搜索
                var searchResults = _dbManager.SearchXboxGames(_currentDbPath, searchTerm, searchType);
                GamesDataGrid.ItemsSource = searchResults;
                GameCountTextBlock.Text = $"搜索结果: {searchResults.Count} / {_allGames.Count}";
            }
        }
    }
}