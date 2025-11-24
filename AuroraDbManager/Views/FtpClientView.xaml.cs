using System;
using System.Collections.Generic;
using System.IO;
using System.Collections.ObjectModel; // Added for ObservableCollection
using System.Linq;
using System.Net.FtpClient;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AuroraDbManager.Views
{
    // Helper class for TreeView items
    public class FileSystemItem
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public bool IsDirectory { get; set; }
        public BitmapImage Icon { get; set; }
        public List<FileSystemItem> Children { get; set; } = new List<FileSystemItem>();
        public bool IsExpanded { get; set; } // Added for TreeView expansion
    }

    /// <summary>
    /// FtpClientView.xaml 的交互逻辑
    /// </summary>
    public partial class FtpClientView : UserControl
    {
        private FtpClient _ftpClient;
        private string _currentLocalPath = AppDomain.CurrentDomain.BaseDirectory; // Default to application directory
        private string _currentRemotePath = "/"; // Default to FTP root

        // Use ObservableCollection for data binding
        private ObservableCollection<FileSystemItem> _localFileSystemItems;
        private ObservableCollection<FileSystemItem> _remoteFileSystemItems;

        private FileSystemItem _selectedLocalItem;
        private FileSystemItem _selectedRemoteItem;

        public FtpClientView()
        {
            InitializeComponent();
            _localFileSystemItems = new ObservableCollection<FileSystemItem>();
            LocalTreeView.ItemsSource = _localFileSystemItems; // Bind ItemsSource once
            _remoteFileSystemItems = new ObservableCollection<FileSystemItem>();
            RemoteTreeView.ItemsSource = _remoteFileSystemItems; // Bind ItemsSource once
            InitializeFtpClient();
            PopulateLocalTreeView(_currentLocalPath);
            LocalPathTextBox.Text = _currentLocalPath;
        }

        private void InitializeFtpClient()
        {
            _ftpClient = new FtpClient();
            _ftpClient.ConnectTimeout = 10000; // 连接超时 10 秒
            _ftpClient.ReadTimeout = 10000;    // 读取超时 10 秒
            // DataConnectionTimeout 在您的库版本中不受支持，已移除
            _ftpClient.Encoding = Encoding.UTF8; // 默认使用 UTF8 编码，如果 Xbox 使用 GBK，请改为 Encoding.GetEncoding("GBK")
            _ftpClient.ValidateCertificate += (control, evt) => { evt.Accept = true; }; // 接受所有证书，用于 FTPS
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _ftpClient.Host = HostTextBox.Text;
                _ftpClient.Port = int.Parse(PortTextBox.Text);
                _ftpClient.Credentials = new System.Net.NetworkCredential(UserTextBox.Text, PasswordBox.Password);

                StatusTextBlock.Text = "正在连接...";
                await Task.Run(() => _ftpClient.Connect());

                StatusTextBlock.Text = "连接成功！";
                ConnectButton.IsEnabled = false;
                DisconnectButton.IsEnabled = true;
                RefreshRemoteButton.IsEnabled = true;
                UploadButton.IsEnabled = true; // Enable upload after connection
                DownloadButton.IsEnabled = true; // Enable download after connection
                DeleteButton.IsEnabled = true; // Enable delete after connection
                CreateFolderButton.IsEnabled = true; // Enable create folder after connection

                await PopulateRemoteTreeView(_currentRemotePath);
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"连接失败: {ex.Message}";
                MessageBox.Show($"连接失败: {ex.Message}\n\n请检查:\n1. FTP服务器是否已启动。\n2. IP地址和端口是否正确。\n3. 防火墙是否阻止了连接。", "FTP连接错误", MessageBoxButton.OK, MessageBoxImage.Error);
                DisconnectButton_Click(null, null); // Ensure UI is reset
            }
        }

        private async void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_ftpClient != null && _ftpClient.IsConnected)
                {
                    StatusTextBlock.Text = "正在断开连接...";
                    await Task.Run(() => _ftpClient.Disconnect());
                }
                StatusTextBlock.Text = "已断开连接";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"断开连接失败: {ex.Message}";
            }
            finally
            {
                ConnectButton.IsEnabled = true;
                DisconnectButton.IsEnabled = false;
                RefreshRemoteButton.IsEnabled = false;
                UploadButton.IsEnabled = false;
                DownloadButton.IsEnabled = false;
                DeleteButton.IsEnabled = false;
                CreateFolderButton.IsEnabled = false;
                RemoteTreeView.Items.Clear(); // Clear remote view
            }
        }

        private void PopulateLocalTreeView(string path) // Changed to private
        {
            _localFileSystemItems.Clear(); // Correct: Clear the bound collection
            try
            {
                var rootFileSystemItem = new FileSystemItem { Name = path, FullPath = path, IsDirectory = true, Icon = GetFolderIcon(), IsExpanded = true };
                AddLocalDirectoryChildren(rootFileSystemItem, path);
                _localFileSystemItems.Add(rootFileSystemItem);
                LocalPathTextBox.Text = path;
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"加载本地文件失败: {ex.Message}";
            }
        }

        private void AddLocalDirectoryChildren(FileSystemItem parent, string path) // Changed parent type to FileSystemItem
        {
            try
            {
                // Add parent directory (..)
                if (Directory.GetParent(path) != null)
                {
                    parent.Children.Add(new FileSystemItem
                    { Name = "..", FullPath = Directory.GetParent(path).FullName, IsDirectory = true, Icon = GetFolderIcon() }
                    );
                }

                foreach (var dir in Directory.GetDirectories(path))
                {
                    var dirInfo = new DirectoryInfo(dir); // Use dirInfo.Name for display
                    parent.Children.Add(new FileSystemItem { Name = dirInfo.Name, FullPath = dir, IsDirectory = true, Icon = GetFolderIcon() });
                }
                foreach (var file in Directory.GetFiles(path))
                {
                    var fileInfo = new FileInfo(file);
                    parent.Children.Add(new FileSystemItem { Name = fileInfo.Name, FullPath = file, IsDirectory = false, Icon = GetFileIcon() });
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Ignore access denied errors for directories we can't read
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"加载本地子目录失败: {ex.Message}";
            }
        }

        private async Task PopulateRemoteTreeView(string path) // Changed to private
        {
            _remoteFileSystemItems.Clear(); // Clear the bound collection
            if (!_ftpClient.IsConnected)
            {
                StatusTextBlock.Text = "未连接到FTP服务器。";
                return;
            }

            try
            {
                StatusTextBlock.Text = $"正在加载远程目录: {path}...";
                var listing = await Task.Run(() => _ftpClient.GetListing(path));
                
                string displayFolderName = path == "/" ? "/" : Path.GetFileName(path.TrimEnd('/'));
                if (string.IsNullOrEmpty(displayFolderName)) displayFolderName = path; // Fallback for root or weird paths

                var rootItem = new FileSystemItem { Name = displayFolderName, FullPath = path, IsDirectory = true, Icon = GetFolderIcon(), IsExpanded = true };

                // Add parent directory (..)
                if (path != "/")
                {
                    string parentPath = path.TrimEnd('/');
                    int lastSlash = parentPath.LastIndexOf('/');
                    parentPath = (lastSlash > 0) ? parentPath.Substring(0, lastSlash) : "/";
                    rootItem.Children.Add(new FileSystemItem
                        {
                            Name = "..",
                            FullPath = parentPath,
                            IsDirectory = true,
                            Icon = GetFolderIcon()
                        }
                    );
                }

                foreach (var item in listing.OrderByDescending(x => x.Type == FtpFileSystemObjectType.Directory).ThenBy(x => x.Name))
                {
                    var fsItem = new FileSystemItem
                    {
                        Name = item.Name,
                        FullPath = item.FullName,
                        IsDirectory = item.Type == FtpFileSystemObjectType.Directory,
                        Icon = item.Type == FtpFileSystemObjectType.Directory ? GetFolderIcon() : GetFileIcon()
                    };
                    rootItem.Children.Add(fsItem); // Add to children of FileSystemItem
                }
                _remoteFileSystemItems.Add(rootItem); // Add to the bound collection

                _currentRemotePath = path; // Update current remote path
                RemotePathTextBox.Text = path;
                StatusTextBlock.Text = $"远程目录加载完成: {path}";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"加载远程文件失败: {ex.Message}";
                MessageBox.Show($"加载远程文件失败: {ex.Message}", "FTP错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshLocalButton_Click(object sender, RoutedEventArgs e) // Changed to private
        {
            PopulateLocalTreeView(_currentLocalPath);
        }

        private async void RefreshRemoteButton_Click(object sender, RoutedEventArgs e) // Changed to private
        {
            await PopulateRemoteTreeView(_currentRemotePath);
        }

        private void LocalTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _selectedLocalItem = LocalTreeView.SelectedItem as FileSystemItem; // Direct cast
            if (_selectedLocalItem != null && _selectedLocalItem.IsDirectory)
            {
                _currentLocalPath = _selectedLocalItem.FullPath;
                PopulateLocalTreeView(_currentLocalPath); // Re-populate to show children
            }
        }

        private async void RemoteTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _selectedRemoteItem = RemoteTreeView.SelectedItem as FileSystemItem; // Direct cast
            if (_selectedRemoteItem != null && _selectedRemoteItem.IsDirectory)
            {
                await PopulateRemoteTreeView(_selectedRemoteItem.FullPath); // Navigate to selected remote directory
            }
        }

        private async void UploadButton_Click(object sender, RoutedEventArgs e) // Changed to private
        {
            _selectedRemoteItem = RemoteTreeView.SelectedItem as FileSystemItem; // Direct cast

            if (_selectedLocalItem == null || _selectedLocalItem.IsDirectory)
            {
                MessageBox.Show("请选择一个本地文件进行上传。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (!_ftpClient.IsConnected)
            {
                MessageBox.Show("请先连接到FTP服务器。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                StatusTextBlock.Text = $"正在上传 {_selectedLocalItem.Name} 到 {_currentRemotePath}...";
                TransferProgressBar.Visibility = Visibility.Visible;
                TransferProgressBar.Value = 0;

                await Task.Run(() => {
                    using (var localStream = File.OpenRead(_selectedLocalItem.FullPath))
                    using (var remoteStream = _ftpClient.OpenWrite($"{_currentRemotePath.TrimEnd('/')}/{_selectedLocalItem.Name}"))
                        localStream.CopyTo(remoteStream);
                });

                StatusTextBlock.Text = $"文件 {_selectedLocalItem.Name} 上传成功！";
                TransferProgressBar.Visibility = Visibility.Collapsed;
                await PopulateRemoteTreeView(_currentRemotePath); // Refresh remote view
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"上传失败: {ex.Message}";
                TransferProgressBar.Visibility = Visibility.Collapsed;
                MessageBox.Show($"上传失败: {ex.Message}", "FTP错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e) // Changed to private
        {
            var selectedTreeViewItem = RemoteTreeView.SelectedItem as TreeViewItem;
            _selectedRemoteItem = selectedTreeViewItem?.Header as FileSystemItem;

            if (_selectedRemoteItem == null || _selectedRemoteItem.IsDirectory)
            {
                MessageBox.Show("请选择一个远程文件进行下载。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (!_ftpClient.IsConnected)
            {
                MessageBox.Show("请先连接到FTP服务器。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                string localFilePath = Path.Combine(_currentLocalPath, _selectedRemoteItem.Name);
                StatusTextBlock.Text = $"正在下载 {_selectedRemoteItem.Name} 到 {localFilePath}...";
                TransferProgressBar.Visibility = Visibility.Visible;
                TransferProgressBar.Value = 0;

                await Task.Run(() => {
                    using (var remoteStream = _ftpClient.OpenRead(_selectedRemoteItem.FullPath))
                    using (var localStream = File.Create(localFilePath))
                        remoteStream.CopyTo(localStream);
                });

                StatusTextBlock.Text = $"文件 {_selectedRemoteItem.Name} 下载成功！";
                TransferProgressBar.Visibility = Visibility.Collapsed;
                PopulateLocalTreeView(_currentLocalPath); // Refresh local view
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"下载失败: {ex.Message}";
                TransferProgressBar.Visibility = Visibility.Collapsed;
                MessageBox.Show($"下载失败: {ex.Message}", "FTP错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e) // Changed to private
        {
            var selectedTreeViewItem = RemoteTreeView.SelectedItem as TreeViewItem;
            _selectedRemoteItem = selectedTreeViewItem?.Header as FileSystemItem;

            if (_selectedRemoteItem == null)
            {
                MessageBox.Show("请选择一个远程文件或文件夹进行删除。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (!_ftpClient.IsConnected)
            {
                MessageBox.Show("请先连接到FTP服务器。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"确定要删除 {_selectedRemoteItem.Name} 吗？", "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                StatusTextBlock.Text = $"正在删除 {_selectedRemoteItem.Name}...";
                if (_selectedRemoteItem.IsDirectory)
                {
                    await Task.Run(() => _ftpClient.DeleteDirectory(_selectedRemoteItem.FullPath, true)); // Recursive delete
                }
                else
                {
                    await Task.Run(() => _ftpClient.DeleteFile(_selectedRemoteItem.FullPath));
                }
                StatusTextBlock.Text = $"已删除 {_selectedRemoteItem.Name}。";
                await PopulateRemoteTreeView(_currentRemotePath); // Refresh remote view
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"删除失败: {ex.Message}";
                MessageBox.Show($"删除失败: {ex.Message}", "FTP错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CreateFolderButton_Click(object sender, RoutedEventArgs e) // Changed to private
        {
            if (!_ftpClient.IsConnected)
            {
                MessageBox.Show("请先连接到FTP服务器。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string folderName = ShowInputDialog("请输入新文件夹名称:", "新建文件夹");
            if (string.IsNullOrWhiteSpace(folderName)) return;

            try
            {
                string newFolderPath = $"{_currentRemotePath.TrimEnd('/')}/{folderName}";
                StatusTextBlock.Text = $"正在创建文件夹 {newFolderPath}...";
                await Task.Run(() => _ftpClient.CreateDirectory(newFolderPath));
                StatusTextBlock.Text = $"文件夹 {folderName} 创建成功。";
                await PopulateRemoteTreeView(_currentRemotePath); // Refresh remote view
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"创建文件夹失败: {ex.Message}";
                MessageBox.Show($"创建文件夹失败: {ex.Message}", "FTP错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string ShowInputDialog(string text, string caption)
        {
            Window inputDialog = new Window
            {
                Title = caption,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            StackPanel sp = new StackPanel { Margin = new Thickness(10) };
            sp.Children.Add(new TextBlock { Text = text, Margin = new Thickness(0, 0, 0, 5) });
            TextBox textBox = new TextBox { Width = 200 };
            sp.Children.Add(textBox);

            Button okButton = new Button { Content = "确定", IsDefault = true, Margin = new Thickness(0, 10, 0, 0) };
            string result = "";
            okButton.Click += (s, e) => { result = textBox.Text; inputDialog.Close(); };
            sp.Children.Add(okButton);

            inputDialog.Content = sp;
            inputDialog.ShowDialog();
            return result;
        }

        // Helper methods for icons (simple placeholders for now)
        private BitmapImage GetFolderIcon()
        {
            // You might want to use actual folder icons from your resources
            // return new BitmapImage(new Uri("pack://application:,,,/AuroraDbManager;component/Resources/folder.png")); // Placeholder
            return null; // Returning null for now to avoid another error if icons are not present
        }

        private BitmapImage GetFileIcon()
        {
            // You might want to use actual file icons from your resources
            // return new BitmapImage(new Uri("pack://application:,,,/AuroraDbManager;component/Resources/file.png")); // Placeholder
            return null; // Returning null for now to avoid another error if icons are not present
        }
    }
}