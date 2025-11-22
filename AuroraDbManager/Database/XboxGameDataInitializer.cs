// 
// XboxGameDataInitializer.cs
// AuroraDbManager
//
// Created to handle initialization of Xbox game data
//

using System;
using System.IO;
using System.Reflection;

namespace AuroraDbManager.Database
{
    /// <summary>
    /// 负责初始化Xbox游戏数据的工具类
    /// </summary>
    public static class XboxGameDataInitializer
    {
        /// <summary>
        /// 确保Xbox游戏数据库已初始化
        /// </summary>
        /// <returns>数据库文件路径</returns>
        public static string EnsureXboxGamesDbInitialized()
        {
            try
            {
                // 获取应用程序目录
                string appDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                
                // 数据库文件路径
                string dbPath = Path.Combine(appDirectory, "xbox_games.db");
                
                // 检查数据库文件是否已存在
                if (File.Exists(dbPath))
                {
                    return dbPath;
                }
                
                // 如果数据库文件不存在，尝试从资源或已知位置复制
                // 在此示例中，我们假设数据库文件应该已经存在
                // 实际应用中可能需要从网络或其他位置下载或复制
                
                return dbPath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"初始化Xbox游戏数据库时出错: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 检查是否所有必需的文件都存在
        /// </summary>
        /// <returns>是否所有文件都存在</returns>
        public static bool AreRequiredFilesPresent()
        {
            try
            {
                string appDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string dbPath = Path.Combine(appDirectory, "xbox_games.db");
                
                return File.Exists(dbPath);
            }
            catch
            {
                return false;
            }
        }
    }
}