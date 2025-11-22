// 
// 	MainWindow.xaml.cs
// 	AuroraDbManager
// 
// 	Created by Swizzy on 14/05/2015
// 	Copyright (c) 2015 Swizzy. All rights reserved.

namespace AuroraDbManager {
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using Classes;
    using Database;
    using Microsoft.Win32;
    using System.Linq;
    using System.Collections.Generic;
    using System.Runtime.Serialization.Json;
    using Views;

    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        private readonly BackgroundWorker _bwLoadContent = new BackgroundWorker();
        private readonly BackgroundWorker _bwLoadSettings = new BackgroundWorker();

        public MainWindow() {
            InitializeComponent();
            _bwLoadContent.DoWork += BwLoadContentOnDoWork;
            _bwLoadContent.RunWorkerCompleted += BwLoadContentOnRunWorkerCompleted;
            _bwLoadSettings.DoWork += BwLoadSettingsOnDoWork;
            _bwLoadSettings.RunWorkerCompleted += BwLoadSettingsOnRunWorkerCompleted;
            App.StatusChanged += AppOnStatusChanged;
            DataContext = App.DbManager;
        }

        private void AppOnStatusChanged(object sender, StatusEventArgs e) {
            Dispatcher.Invoke(() => StatusText.Text = e.Status);
        }

        private void BwLoadContentOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            StatusText.Text = "Ready...";
        }

        private void BwLoadContentOnDoWork(object sender, DoWorkEventArgs e) {
            ContentDbView.OpenDb((string)e.Argument);
        }

        private void BwLoadSettingsOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            StatusText.Text = "Ready...";
        }

        private void BwLoadSettingsOnDoWork(object sender, DoWorkEventArgs e) {
            SettingsDbView.OpenDb((string)e.Argument);
        }

        private void LoadContentDb_OnClick(object sender, RoutedEventArgs e) {
            var ofd = new OpenFileDialog {
                Filter = "Aurora Content Database|Content.db|All Files|*.*",
                Title = "Select Aurora Content Database"
            };
            if(ofd.ShowDialog() == true) {
                StatusText.Text = "Loading Content Database...";
                _bwLoadContent.RunWorkerAsync(ofd.FileName);
            }
        }

        private void LoadSettingsDb_OnClick(object sender, RoutedEventArgs e) {
            var ofd = new OpenFileDialog {
                Filter = "Aurora Settings Database|Settings.db|All Files|*.*",
                Title = "Select Aurora Settings Database"
            };
            if(ofd.ShowDialog() == true) {
                StatusText.Text = "Loading Settings Database...";
                _bwLoadSettings.RunWorkerAsync(ofd.FileName);
            }
        }

        private void Exit_OnClick(object sender, RoutedEventArgs e) { Close(); }

        private void MainTabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if(e.AddedItems.Count > 0) {
                var tabItem = e.AddedItems[0] as TabItem;
                if(tabItem != null) {
                    if(tabItem.Header.ToString() == "Content Database")
                        ContentDbView.Focus();
                    else
                        SettingsDbView.Focus();
                }
            }
        }

        private void OpenXboxGamesDb_OnClick(object sender, RoutedEventArgs e) {
            // 创建并显示Xbox游戏数据库窗口
            var xboxGamesWindow = new Window {
                Title = "Xbox游戏数据库",
                Width = 1000,
                Height = 700,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var xboxGamesView = new XboxGamesView();
            xboxGamesWindow.Content = xboxGamesView;
            
            xboxGamesWindow.Show();
        }

        private void ExportContentData_OnClick(object sender, RoutedEventArgs e) {
            if (!App.DbManager.IsContentOpen) {
                MessageBox.Show("请先加载Content数据库", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var sfd = new SaveFileDialog {
                Filter = "CSV文件|*.csv|JSON文件|*.json|所有文件|*.*",
                Title = "导出Content数据"
            };

            if (sfd.ShowDialog() == true) {
                try {
                    StatusText.Text = "正在导出Content数据...";
                    var contentItems = App.DbManager.GetContentItems();
                    var titleUpdateItems = App.DbManager.GetTitleUpdateItems();

                    switch (sfd.FilterIndex) {
                        case 1: // CSV
                            ExportContentDataToCsv(sfd.FileName, contentItems, titleUpdateItems);
                            break;
                        case 2: // JSON
                            ExportContentDataToJson(sfd.FileName, contentItems, titleUpdateItems);
                            break;
                    }

                    StatusText.Text = "Content数据导出完成";
                    MessageBox.Show("数据导出完成", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex) {
                    StatusText.Text = "导出失败";
                    MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    App.SaveException(ex);
                }
            }
        }

        private void ExportSettingsData_OnClick(object sender, RoutedEventArgs e) {
            if (!App.DbManager.IsSettingsOpen) {
                MessageBox.Show("请先加载Settings数据库", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var sfd = new SaveFileDialog {
                Filter = "CSV文件|*.csv|JSON文件|*.json|所有文件|*.*",
                Title = "导出Settings数据"
            };

            if (sfd.ShowDialog() == true) {
                try {
                    StatusText.Text = "正在导出Settings数据...";
                    var systemSettings = App.DbManager.GetSystemSettings();
                    var userSettings = App.DbManager.GetUserSettings();
                    var scanPaths = App.DbManager.GetScanPaths();
                    var profiles = App.DbManager.GetProfiles();
                    var quickViews = App.DbManager.GetQuickViews();
                    var userFavorites = App.DbManager.GetUserFavorites();
                    var userHidden = App.DbManager.GetUserHidden();
                    var trainers = App.DbManager.GetTrainers();

                    switch (sfd.FilterIndex) {
                        case 1: // CSV
                            ExportSettingsDataToCsv(sfd.FileName, systemSettings, userSettings, scanPaths, profiles, quickViews, userFavorites, userHidden, trainers);
                            break;
                        case 2: // JSON
                            ExportSettingsDataToJson(sfd.FileName, systemSettings, userSettings, scanPaths, profiles, quickViews, userFavorites, userHidden, trainers);
                            break;
                    }

                    StatusText.Text = "Settings数据导出完成";
                    MessageBox.Show("数据导出完成", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex) {
                    StatusText.Text = "导出失败";
                    MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    App.SaveException(ex);
                }
            }
        }

        private void ExportContentDataToCsv(string filePath, IEnumerable<ContentItem> contentItems, IEnumerable<TitleUpdateItem> titleUpdateItems) {
            var csv = new StringBuilder();
            
            // 添加ContentItems表头
            csv.AppendLine("Content Items");
            csv.AppendLine("Id,TitleId,MediaId,BaseVersion,DiscNum,DiscsInSet,TitleName,Description,Publisher,Developer,LiveRating,LiveRaters,ReleaseDate,GenreFlag,ContentFlags,Hash,GameCapsFlags,FileType,ContentType,ContentGroup,DefaultGroup,DateAdded,FoundAtDepth,SystemLink,ScanPathId,Directory,Executable");
            
            // 添加ContentItems数据
            foreach (var item in contentItems) {
                csv.AppendLine($"{item.Id},{item.TitleId:X8},{item.MediaId:X8},{item.BaseVersion:X8},{item.DiscNum},{item.DiscsInSet}," +
                              $"\"{item.TitleName}\",\"{item.Description}\",\"{item.Publisher}\",\"{item.Developer}\"," +
                              $"{item.LiveRating},{item.LiveRaters},\"{item.ReleaseDate}\",\"{item.GenreFlag}\",\"{item.ContentFlags}\"," +
                              $"\"{item.Hash}\",{(int)item.GameCapsFlags},{(int)item.FileType},{(int)item.ContentType}," +
                              $"{(int)item.ContentGroup},{(int)item.DefaultGroup},{item.DateAdded.ToFileTime()},\"{item.FoundAtDepth}\"," +
                              $"{(item.SystemLink ? 1 : 0)},{item.ScanPathId},\"{item.Directory}\",\"{item.Executable}\"");
            }
            
            // 添加TitleUpdateItems表头
            csv.AppendLine();
            csv.AppendLine("Title Update Items");
            csv.AppendLine("Id,DisplayName,FileName,LiveDeviceId,LivePath,TitleId,MediaId,BaseVersion,Version,Hash,BackupPath,FileSize");
            
            // 添加TitleUpdateItems数据
            foreach (var item in titleUpdateItems) {
                csv.AppendLine($"{item.Id},\"{item.DisplayName}\",\"{item.FileName}\",\"{item.LiveDeviceId}\",\"{item.LivePath}\"," +
                              $"{item.TitleId:X8},{item.MediaId:X8},{item.BaseVersion:X8},{item.Version},\"{item.Hash}\"," +
                              $"\"{item.BackupPath}\",\"{item.FileSize}\"");
            }
            
            File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
        }

        private void ExportContentDataToJson(string filePath, IEnumerable<ContentItem> contentItems, IEnumerable<TitleUpdateItem> titleUpdateItems) {
            var contentData = new {
                ContentItems = contentItems.Select(item => new {
                    Id = item.Id,
                    TitleId = $"0x{item.TitleId:X8}",
                    MediaId = $"0x{item.MediaId:X8}",
                    BaseVersion = $"0x{item.BaseVersion:X8}",
                    DiscNum = item.DiscNum,
                    DiscsInSet = item.DiscsInSet,
                    TitleName = item.TitleName,
                    Description = item.Description,
                    Publisher = item.Publisher,
                    Developer = item.Developer,
                    LiveRating = item.LiveRating,
                    LiveRaters = item.LiveRaters,
                    ReleaseDate = item.ReleaseDate,
                    GenreFlag = item.GenreFlag.ToString(),
                    ContentFlags = item.ContentFlags.ToString(),
                    Hash = item.Hash,
                    GameCapsFlags = (int)item.GameCapsFlags,
                    FileType = (int)item.FileType,
                    ContentType = (int)item.ContentType,
                    ContentGroup = (int)item.ContentGroup,
                    DefaultGroup = (int)item.DefaultGroup,
                    DateAdded = item.DateAdded.ToString(),
                    FoundAtDepth = item.FoundAtDepth,
                    SystemLink = item.SystemLink,
                    ScanPathId = item.ScanPathId,
                    Directory = item.Directory,
                    Executable = item.Executable
                }),
                TitleUpdateItems = titleUpdateItems.Select(item => new {
                    Id = item.Id,
                    DisplayName = item.DisplayName,
                    FileName = item.FileName,
                    LiveDeviceId = item.LiveDeviceId,
                    LivePath = item.LivePath,
                    TitleId = $"0x{item.TitleId:X8}",
                    MediaId = $"0x{item.MediaId:X8}",
                    BaseVersion = $"0x{item.BaseVersion:X8}",
                    Version = item.Version,
                    Hash = item.Hash,
                    BackupPath = item.BackupPath,
                    FileSize = item.FileSize
                })
            };

            var json = SerializeToJson(contentData);
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }

        private void ExportSettingsDataToCsv(string filePath, 
            IEnumerable<SystemSettingItem> systemSettings,
            IEnumerable<UserSettingItem> userSettings,
            IEnumerable<ScanPathItem> scanPaths,
            IEnumerable<ProfileItem> profiles,
            IEnumerable<QuickViewItem> quickViews,
            IEnumerable<UserFavoriteItem> userFavorites,
            IEnumerable<UserHiddenItem> userHidden,
            IEnumerable<TrainerItem> trainers) {
            
            var csv = new StringBuilder();
            
            // SystemSettings
            csv.AppendLine("System Settings");
            csv.AppendLine("Id,Name,Value");
            foreach (var item in systemSettings) {
                csv.AppendLine($"{item.Id},\"{item.Name}\",\"{item.Value}\"");
            }
            
            csv.AppendLine();
            
            // UserSettings
            csv.AppendLine("User Settings");
            csv.AppendLine("Id,Name,Value,ProfileId");
            foreach (var item in userSettings) {
                csv.AppendLine($"{item.Id},\"{item.Name}\",\"{item.Value}\",\"{item.ProfileId}\"");
            }
            
            csv.AppendLine();
            
            // ScanPaths
            csv.AppendLine("Scan Paths");
            csv.AppendLine("Id,Path,DeviceId,Depth,ScriptData,OptionsFlag");
            foreach (var item in scanPaths) {
                csv.AppendLine($"{item.Id},\"{item.Path}\",\"{item.DeviceID}\",{item.Depth},\"{item.ScriptData}\",{item.OptionsFlag}");
            }
            
            csv.AppendLine();
            
            // Profiles
            csv.AppendLine("Profiles");
            csv.AppendLine("Id,Gametag,Xuid");
            foreach (var item in profiles) {
                csv.AppendLine($"{item.Id},\"{item.GameTag}\",\"{item.Xuid}\"");
            }
            
            csv.AppendLine();
            
            // QuickViews
            csv.AppendLine("Quick Views");
            csv.AppendLine("Id,DisplayName,SortMethod,FilterMethod,Flags,CreatorXUID,OrderIndex,IconHash");
            foreach (var item in quickViews) {
                csv.AppendLine($"{item.Id},\"{item.DisplayName}\",\"{item.SortMethod}\",\"{item.FilterMethod}\",{item.Flags},\"{item.CreatorXUID}\",{item.OrderIndex},\"{item.IconHash}\"");
            }
            
            csv.AppendLine();
            
            // UserFavorites
            csv.AppendLine("User Favorites");
            csv.AppendLine("Id,ContentId,ProfileId");
            foreach (var item in userFavorites) {
                csv.AppendLine($"{item.Id},{item.ContentId},\"{item.ProfileId}\"");
            }
            
            csv.AppendLine();
            
            // UserHidden
            csv.AppendLine("User Hidden");
            csv.AppendLine("Id,ContentId,ProfileId");
            foreach (var item in userHidden) {
                csv.AppendLine($"{item.Id},{item.ContentId},\"{item.ProfileId}\"");
            }
            
            csv.AppendLine();
            
            // Trainers
            csv.AppendLine("Trainers");
            csv.AppendLine("Id,TitleId,MediaId,TrainerPath,TrainerName,TrainerVersion,TrainerData,TrainerInfo,TrainerAuthor,TrainerRating,TrainerFlags,CreatorXUID");
            foreach (var item in trainers) {
                csv.AppendLine($"{item.Id},\"{item.TitleId}\",\"{item.MediaId}\",\"{item.TrainerPath}\",\"{item.TrainerName}\",{item.TrainerVersion}," +
                              $"\"{item.TrainerData}\",\"{item.TrainerInfo}\",\"{item.TrainerAuthor}\",{item.TrainerRating},{item.TrainerFlags},\"{item.CreatorXUID}\"");
            }
            
            File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
        }

        private void ExportSettingsDataToJson(string filePath,
            IEnumerable<SystemSettingItem> systemSettings,
            IEnumerable<UserSettingItem> userSettings,
            IEnumerable<ScanPathItem> scanPaths,
            IEnumerable<ProfileItem> profiles,
            IEnumerable<QuickViewItem> quickViews,
            IEnumerable<UserFavoriteItem> userFavorites,
            IEnumerable<UserHiddenItem> userHidden,
            IEnumerable<TrainerItem> trainers) {
            
            var settingsData = new {
                SystemSettings = systemSettings.Select(item => new {
                    Id = item.Id,
                    Name = item.Name,
                    Value = item.Value
                }),
                UserSettings = userSettings.Select(item => new {
                    Id = item.Id,
                    Name = item.Name,
                    Value = item.Value,
                    ProfileId = item.ProfileId
                }),
                ScanPaths = scanPaths.Select(item => new {
                    Id = item.Id,
                    Path = item.Path,
                    DeviceId = item.DeviceID,
                    Depth = item.Depth,
                    ScriptData = item.ScriptData,
                    OptionsFlag = item.OptionsFlag
                }),
                Profiles = profiles.Select(item => new {
                    Id = item.Id,
                    Gametag = item.GameTag,
                    Xuid = item.Xuid
                }),
                QuickViews = quickViews.Select(item => new {
                    Id = item.Id,
                    DisplayName = item.DisplayName,
                    SortMethod = item.SortMethod,
                    FilterMethod = item.FilterMethod,
                    Flags = item.Flags,
                    CreatorXUID = item.CreatorXUID,
                    OrderIndex = item.OrderIndex,
                    IconHash = item.IconHash
                }),
                UserFavorites = userFavorites.Select(item => new {
                    Id = item.Id,
                    ContentId = item.ContentId,
                    ProfileId = item.ProfileId
                }),
                UserHidden = userHidden.Select(item => new {
                    Id = item.Id,
                    ContentId = item.ContentId,
                    ProfileId = item.ProfileId
                }),
                Trainers = trainers.Select(item => new {
                    Id = item.Id,
                    TitleId = item.TitleId,
                    MediaId = item.MediaId,
                    TrainerPath = item.TrainerPath,
                    TrainerName = item.TrainerName,
                    TrainerVersion = item.TrainerVersion,
                    TrainerData = item.TrainerData,
                    TrainerInfo = item.TrainerInfo,
                    TrainerAuthor = item.TrainerAuthor,
                    TrainerRating = item.TrainerRating,
                    TrainerFlags = item.TrainerFlags,
                    CreatorXUID = item.CreatorXUID
                })
            };

            var json = SerializeToJson(settingsData);
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }

        private string SerializeToJson(object obj) {
            var serializer = new DataContractJsonSerializer(obj.GetType());
            using (var stream = new MemoryStream()) {
                serializer.WriteObject(stream, obj);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }
}