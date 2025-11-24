// 
// 	App.xaml.cs
// 	AuroraDbManager
// 
// 	Created by Swizzy on 14/05/2015
// 	Copyright (c) 2015 Swizzy. All rights reserved.

namespace AuroraDbManager {
    using System;
    using System.IO;
    using System.Windows;
    using AuroraDbManager.Classes;
    using AuroraDbManager.Database;

    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App {
        internal static readonly AuroraDbManager DbManager = new AuroraDbManager();
        internal static EventHandler<StatusEventArgs> StatusChanged;

        public static void SaveException(Exception ex) { File.AppendAllText("error.log", string.Format("[{0}]:{2}{1}{2}", DateTime.Now, ex, Environment.NewLine)); }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var loginWindow = new LoginWindow();
            var result = loginWindow.ShowDialog();

            // 检查登录窗口的返回结果
            if (result == true)
            {
                // 登录成功，显示主窗口
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
            else
            {
                // 登录失败或用户点击了取消，关闭应用程序
                Application.Current.Shutdown();
            }
        }
    }
}