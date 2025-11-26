// 
// GameDirectoryManagerView.xaml.cs
// AuroraDbManager
//
// Created by Assistant on 2025/11/26
// Copyright © 2025 All rights reserved.
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AuroraDbManager.Database;
using Microsoft.Win32;
using Forms = System.Windows.Forms;
using DbManager = AuroraDbManager.Database.AuroraDbManager;

namespace AuroraDbManager.Views
{
    /// <summary>
    /// GameDirectoryManagerView.xaml 的交互逻辑
    /// </summary>
    public partial class GameDirectoryManagerView : System.Windows.Controls.UserControl
    {
        private DbManager _dbManager;
        private List<GameDirectoryItem> _sourceGames;
        private List<GameDirectoryItem> _destinationGames;
        private string _currentDbPath;

        public GameDirectoryManagerView()
        {
            InitializeComponent();
            _dbManager = new DbManager();
            _sourceGames = new List<GameDirectoryItem>();
            _destinationGames = new List<GameDirectoryItem>();
            
            // 自动加载程序目录下的xbox_games.db文件
            Loaded += GameDirectoryManagerView_Loaded;
        }

        private void GameDirectoryManagerView_Loaded(object sender, RoutedEventArgs e)
        {
            // 获取程序目录路径
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _currentDbPath = Path.Combine(appDirectory, "xbox_games.db");
            System.Diagnostics.Debug.WriteLine($"=== 数据库加载信息 ===");
            System.Diagnostics.Debug.WriteLine($"尝试查找数据库文件: {_currentDbPath}");
            System.Diagnostics.Debug.WriteLine($"当前程序目录: {appDirectory}");

            // 检查数据库文件是否存在
            if (File.Exists(_currentDbPath))
            {
                System.Diagnostics.Debug.WriteLine($"成功找到数据库文件: {_currentDbPath}");
                System.Diagnostics.Debug.WriteLine($"数据库文件大小: {new FileInfo(_currentDbPath).Length} 字节");
                
                // 尝试打开数据库验证是否有效
                try
                {
                    var testGames = _dbManager.GetXboxGames(_currentDbPath);
                    System.Diagnostics.Debug.WriteLine($"数据库加载成功，包含 {testGames.Count} 条游戏记录");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"数据库加载失败: {ex.Message}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"未找到数据库文件: {_currentDbPath}");
                // 文件不存在，显示提示弹窗
                MessageBox.Show("未找到xbox_games.db数据库文件。请确保该文件存在于程序目录中。", 
                    "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// 浏览源目录
        /// </summary>
        private void BrowseSourceButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == Forms.DialogResult.OK)
            {
                SourceDirectoryTextBox.Text = dialog.SelectedPath;
            }
        }

        /// <summary>
        /// 浏览目标目录
        /// </summary>
        private void BrowseDestinationButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == Forms.DialogResult.OK)
            {
                DestinationDirectoryTextBox.Text = dialog.SelectedPath;
            }
        }

        /// <summary>
        /// 扫描目录
        /// </summary>
        private async void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            string sourcePath = SourceDirectoryTextBox.Text;
            string destinationPath = DestinationDirectoryTextBox.Text;

            System.Diagnostics.Debug.WriteLine($"开始扫描目录");
            System.Diagnostics.Debug.WriteLine($"源目录: {sourcePath}");
            System.Diagnostics.Debug.WriteLine($"目标目录: {destinationPath}");

            if (string.IsNullOrWhiteSpace(sourcePath) && string.IsNullOrWhiteSpace(destinationPath))
            {
                MessageBox.Show("请至少选择一个目录进行扫描。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                ScanButton.IsEnabled = false;
                StatusTextBlock.Text = "正在扫描目录...";

                // 异步扫描目录
                await Task.Run(() =>
                {
                    // 扫描源目录
                    if (!string.IsNullOrWhiteSpace(sourcePath))
                    {
                        if (Directory.Exists(sourcePath))
                        {
                            System.Diagnostics.Debug.WriteLine($"开始扫描源目录: {sourcePath}");
                            _sourceGames = ScanGameDirectory(sourcePath);
                            System.Diagnostics.Debug.WriteLine($"源目录扫描完成，找到 {_sourceGames.Count} 个游戏");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"源目录不存在: {sourcePath}");
                            MessageBox.Show($"源目录不存在: {sourcePath}", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }

                    // 扫描目标目录
                    if (!string.IsNullOrWhiteSpace(destinationPath))
                    {
                        if (Directory.Exists(destinationPath))
                        {
                            System.Diagnostics.Debug.WriteLine($"开始扫描目标目录: {destinationPath}");
                            _destinationGames = ScanGameDirectory(destinationPath);
                            System.Diagnostics.Debug.WriteLine($"目标目录扫描完成，找到 {_destinationGames.Count} 个游戏");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"目标目录不存在: {destinationPath}");
                            MessageBox.Show($"目标目录不存在: {destinationPath}", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                });

                // 更新UI
                UpdateGameLists();
                StatusTextBlock.Text = "目录扫描完成";
                System.Diagnostics.Debug.WriteLine($"UI更新完成，源游戏数: {_sourceGames.Count}，目标游戏数: {_destinationGames.Count}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"扫描目录时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = "扫描失败";
                System.Diagnostics.Debug.WriteLine($"扫描目录时出错: {ex.Message}");
            }
            finally
            {
                ScanButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// 扫描游戏目录
        /// </summary>
        /// <param name="directoryPath">目录路径</param>
        /// <returns>游戏列表</returns>
        private List<GameDirectoryItem> ScanGameDirectory(string directoryPath)
        {
            var games = new List<GameDirectoryItem>();
            
            try
            {
                // 检查目录是否存在
                if (!Directory.Exists(directoryPath))
                {
                    System.Diagnostics.Debug.WriteLine($"目录不存在: {directoryPath}");
                    return games;
                }

                // 获取第一级目录（游戏ID目录）
                var gameDirectories = Directory.GetDirectories(directoryPath);
                System.Diagnostics.Debug.WriteLine($"在目录 {directoryPath} 中找到 {gameDirectories.Length} 个游戏目录");

                foreach (var gameDir in gameDirectories)
                {
                    var gameDirInfo = new DirectoryInfo(gameDir);
                    var gameId = gameDirInfo.Name; // 第一级目录名作为游戏ID
                    
                    System.Diagnostics.Debug.WriteLine($"=== 处理游戏目录 ===");
                    System.Diagnostics.Debug.WriteLine($"游戏ID: {gameId}");
                    System.Diagnostics.Debug.WriteLine($"完整路径: {gameDir}");
                    
                    // 检查是否存在00007000子目录
                    var subDirPath = Path.Combine(gameDir, "00007000");
                    if (!Directory.Exists(subDirPath))
                    {
                        System.Diagnostics.Debug.WriteLine($"游戏 {gameId} 缺少 00007000 子目录");
                        continue;
                    }
                    
                    // 获取游戏大小（计算整个游戏目录的大小）
                    long size = CalculateDirectorySize(gameDir);
                    System.Diagnostics.Debug.WriteLine($"游戏大小: {FormatBytes(size)} ({size} 字节)");
                    
                    // 创建游戏项
                    var gameItem = new GameDirectoryItem
                    {
                        GameId = gameId,
                        FullPath = gameDir,
                        Size = size
                    };
                    
                    // 从数据库获取游戏标题信息（如果数据库存在）
                    if (File.Exists(_currentDbPath))
                    {
                        try
                        {
                            System.Diagnostics.Debug.WriteLine($"开始查询数据库: {_currentDbPath}");
                            XboxGameItem gameInfo = null;
                            
                            // 尝试多种大小写格式
                            var formatsToTry = new List<string> 
                            { 
                                gameId,           // 原始格式
                                gameId.ToUpper(), // 大写格式
                                gameId.ToLower(), // 小写格式
                                ToTitleCase(gameId) // 首字母大写格式
                            };

                            // 去重并尝试每种格式
                            var uniqueFormats = formatsToTry.Distinct().ToList();
                            System.Diagnostics.Debug.WriteLine($"将尝试以下格式进行查询: {string.Join(", ", uniqueFormats)}");
                            
                            foreach (var format in uniqueFormats)
                            {
                                System.Diagnostics.Debug.WriteLine($"尝试使用格式 '{format}' 查询数据库");
                                gameInfo = _dbManager.GetXboxGameByTitleId(_currentDbPath, format);
                                if (gameInfo != null)
                                {
                                    System.Diagnostics.Debug.WriteLine($"使用格式 '{format}' 成功找到游戏信息");
                                    System.Diagnostics.Debug.WriteLine($"  数据库ID: {gameInfo.TitleId}");
                                    System.Diagnostics.Debug.WriteLine($"  英文标题: {gameInfo.Title}");
                                    System.Diagnostics.Debug.WriteLine($"  中文标题: {gameInfo.TitleCn}");
                                    break;
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"使用格式 '{format}' 未找到游戏");
                                }
                            }
                            
                            // 处理查询结果
                            if (gameInfo != null)
                            {
                                gameItem.Title = !string.IsNullOrEmpty(gameInfo.Title) ? gameInfo.Title : gameId;
                                gameItem.TitleCn = !string.IsNullOrEmpty(gameInfo.TitleCn) ? gameInfo.TitleCn : gameId;
                                System.Diagnostics.Debug.WriteLine($"最终使用标题 - 英文: {gameItem.Title}, 中文: {gameItem.TitleCn}");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"未在数据库中找到游戏ID: {gameId} (已尝试所有大小写变体)");
                                gameItem.Title = gameId; // 使用游戏ID作为标题
                                gameItem.TitleCn = gameId; // 使用游戏ID作为中文标题
                            }
                        }
                        catch (Exception dbEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"查询数据库时出错: {dbEx.Message}");
                            System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {dbEx.StackTrace}");
                            gameItem.Title = gameId; // 使用游戏ID作为标题
                            gameItem.TitleCn = gameId; // 使用游戏ID作为中文标题
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"数据库文件不存在: {_currentDbPath}");
                        gameItem.Title = gameId; // 使用游戏ID作为标题
                        gameItem.TitleCn = gameId; // 使用游戏ID作为中文标题
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"添加游戏项: ID={gameItem.GameId}, Title={gameItem.Title}, TitleCn={gameItem.TitleCn}");
                    games.Add(gameItem);
                }
            }
            catch (Exception ex)
            {
                // 记录异常但不中断扫描过程
                System.Diagnostics.Debug.WriteLine($"扫描目录 {directoryPath} 时出错: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                MessageBox.Show($"扫描目录 {directoryPath} 时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            System.Diagnostics.Debug.WriteLine($"扫描完成，共找到 {games.Count} 个游戏");
            return games;
        }

        /// <summary>
        /// 计算目录大小
        /// </summary>
        /// <param name="directoryPath">目录路径</param>
        /// <returns>目录大小（字节）</returns>
        private long CalculateDirectorySize(string directoryPath)
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);
                return dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 更新游戏列表显示
        /// </summary>
        private void UpdateGameLists()
        {
            // 更新源目录游戏列表
            SourceGamesDataGrid.ItemsSource = null;
            // 为源游戏列表中的每个游戏项添加属性变更监听
            foreach (var game in _sourceGames)
            {
                game.PropertyChanged -= Game_PropertyChanged;
                game.PropertyChanged += Game_PropertyChanged;
            }
            SourceGamesDataGrid.ItemsSource = _sourceGames;
            SourceDirectoryInfoTextBlock.Text = $"源目录: {_sourceGames.Count} 个游戏";

            // 更新目标目录游戏列表
            DestinationGamesDataGrid.ItemsSource = null;
            // 为目标游戏列表中的每个游戏项添加属性变更监听
            foreach (var game in _destinationGames)
            {
                game.PropertyChanged -= Game_PropertyChanged;
                game.PropertyChanged += Game_PropertyChanged;
            }
            DestinationGamesDataGrid.ItemsSource = _destinationGames;
            DestinationDirectoryInfoTextBlock.Text = $"目标目录: {_destinationGames.Count} 个游戏";

            // 更新状态栏
            UpdateStatusText();
        }

        /// <summary>
        /// 游戏项属性变更事件处理
        /// </summary>
        private void Game_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
            {
                UpdateStatusText();
            }
        }

        /// <summary>
        /// DataGrid选择改变事件处理
        /// </summary>
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 不再需要此方法，因为我们在Game_PropertyChanged中处理了IsSelected的变化
        }

        /// <summary>
        /// 源目录全选
        /// </summary>
        private void SourceSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var game in _sourceGames)
            {
                game.IsSelected = true;
            }
            UpdateStatusText();
        }

        /// <summary>
        /// 源目录取消全选
        /// </summary>
        private void SourceSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var game in _sourceGames)
            {
                game.IsSelected = false;
            }
            UpdateStatusText();
        }

        /// <summary>
        /// 目标目录全选
        /// </summary>
        private void DestinationSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var game in _destinationGames)
            {
                game.IsSelected = true;
            }
            UpdateStatusText();
        }

        /// <summary>
        /// 目标目录取消全选
        /// </summary>
        private void DestinationSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var game in _destinationGames)
            {
                game.IsSelected = false;
            }
            UpdateStatusText();
        }

        /// <summary>
        /// 更新状态文本
        /// </summary>
        private void UpdateStatusText()
        {
            int selectedSourceCount = _sourceGames.Count(g => g.IsSelected);
            long selectedSourceSize = _sourceGames.Where(g => g.IsSelected).Sum(g => g.Size);
            
            int selectedDestinationCount = _destinationGames.Count(g => g.IsSelected);
            long selectedDestinationSize = _destinationGames.Where(g => g.IsSelected).Sum(g => g.Size);
            
            long totalSelectedSize = selectedSourceSize + selectedDestinationSize;
            
            string sizeText = FormatBytes(totalSelectedSize);
            StatusTextBlock.Text = $"已选中 {selectedSourceCount + selectedDestinationCount} 个游戏，总容量 {sizeText}";
        }

        /// <summary>
        /// 格式化字节大小为可读格式
        /// </summary>
        /// <param name="bytes">字节数</param>
        /// <returns>格式化后的大小字符串</returns>
        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            // 根据大小决定小数位数
            if (order == 0)
                return $"{len:0} {sizes[order]}";
            else if (len >= 100)
                return $"{len:0} {sizes[order]}";
            else if (len >= 10)
                return $"{len:0.0} {sizes[order]}";
            else
                return $"{len:0.00} {sizes[order]}";
        }

        /// <summary>
        /// 复制选中的游戏到目标目录
        /// </summary>
        private async void CopySelectedButton_Click(object sender, RoutedEventArgs e)
        {
            string sourcePath = SourceDirectoryTextBox.Text;
            string destinationPath = DestinationDirectoryTextBox.Text;

            if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(destinationPath))
            {
                MessageBox.Show("请先选择源目录和目标目录。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Directory.Exists(sourcePath))
            {
                MessageBox.Show("源目录不存在。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Directory.Exists(destinationPath))
            {
                // 创建目标目录
                try
                {
                    Directory.CreateDirectory(destinationPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"创建目标目录失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            var selectedGames = _sourceGames.Where(g => g.IsSelected).ToList();
            if (selectedGames.Count == 0)
            {
                MessageBox.Show("请至少选择一个游戏进行复制。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                CopySelectedButton.IsEnabled = false;
                StatusTextBlock.Text = "正在复制游戏...";

                int copiedCount = 0;
                long totalSize = selectedGames.Sum(g => g.Size);
                long copiedSize = 0;

                foreach (var game in selectedGames)
                {
                    var sourceGamePath = Path.Combine(sourcePath, game.GameId);
                    var destinationGamePath = Path.Combine(destinationPath, game.GameId);

                    // 如果目标目录已存在该游戏，则跳过
                    if (Directory.Exists(destinationGamePath))
                    {
                        StatusTextBlock.Text = $"跳过 {game.GameId} (已存在)";
                        continue;
                    }

                    // 复制目录
                    StatusTextBlock.Text = $"正在复制 {game.GameId}...";
                    await Task.Run(() => CopyDirectory(sourceGamePath, destinationGamePath));
                    
                    copiedCount++;
                    copiedSize += game.Size;
                    
                    // 更新进度
                    string progressText = FormatBytes(copiedSize) + "/" + FormatBytes(totalSize);
                    StatusTextBlock.Text = $"已复制 {copiedCount}/{selectedGames.Count} 个游戏 ({progressText})";
                }

                StatusTextBlock.Text = $"复制完成，共复制 {copiedCount} 个游戏";
                MessageBox.Show($"复制完成，共复制 {copiedCount} 个游戏。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);

                // 重新扫描目标目录以更新显示
                _destinationGames = ScanGameDirectory(destinationPath);
                UpdateGameLists();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"复制游戏时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = "复制失败";
            }
            finally
            {
                CopySelectedButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// 复制目录
        /// </summary>
        /// <param name="sourceDir">源目录</param>
        /// <param name="destinationDir">目标目录</param>
        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            // 创建目标目录
            Directory.CreateDirectory(destinationDir);

            // 复制文件
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destinationDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            // 复制子目录
            foreach (string directory in Directory.GetDirectories(sourceDir))
            {
                string destDirectory = Path.Combine(destinationDir, Path.GetFileName(directory));
                CopyDirectory(directory, destDirectory);
            }
        }

        /// <summary>
        /// 将字符串转换为首字母大写格式
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns>首字母大写格式的字符串</returns>
        private string ToTitleCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var result = input.ToLower();
            return char.ToUpper(result[0]) + result.Substring(1);
        }

        // 实现INotifyPropertyChanged接口的属性变化通知
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}