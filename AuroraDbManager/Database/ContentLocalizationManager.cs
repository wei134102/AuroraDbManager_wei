// 
// ContentLocalizationManager.cs
// AuroraDbManager
//
// Created to handle localization of Content database with Chinese titles
//

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace AuroraDbManager.Database
{
    /// <summary>
    /// 管理Content数据库的本地化功能
    /// </summary>
    public class ContentLocalizationManager
    {
        private readonly string _xboxGamesDbPath;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="xboxGamesDbPath">Xbox游戏数据库路径</param>
        public ContentLocalizationManager(string xboxGamesDbPath)
        {
            _xboxGamesDbPath = xboxGamesDbPath;
        }
        
        /// <summary>
        /// 检查Xbox游戏数据库是否存在
        /// </summary>
        /// <returns>是否存在</returns>
        public bool IsXboxGamesDbAvailable()
        {
            return File.Exists(_xboxGamesDbPath);
        }
        
        /// <summary>
        /// 根据TitleId获取中文标题
        /// </summary>
        /// <param name="titleId">游戏标题ID（十六进制字符串）</param>
        /// <returns>中文标题，如果未找到则返回null</returns>
        public string GetChineseTitleByTitleId(string titleId)
        {
            if (!IsXboxGamesDbAvailable())
                return null;
                
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={_xboxGamesDbPath};Version=3;"))
                {
                    connection.Open();
                    
                    // 将十六进制字符串转换为整数进行比较
                    if (!int.TryParse(titleId, System.Globalization.NumberStyles.HexNumber, null, out int titleIdInt))
                        return null;
                    
                    using (var command = new SQLiteCommand(
                        "SELECT Title_cn FROM ContentItems WHERE TitleId = @titleId LIMIT 1", connection))
                    {
                        command.Parameters.AddWithValue("@titleId", titleIdInt.ToString("x8")); // 转换为8位小写十六进制
                        
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var titleCn = reader["Title_cn"]?.ToString();
                                // 只有当中文标题与英文标题不同时才返回
                                var title = reader["Title"]?.ToString();
                                if (!string.IsNullOrEmpty(titleCn) && titleCn != title)
                                {
                                    return titleCn;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录异常但不中断程序
                System.Diagnostics.Debug.WriteLine($"获取中文标题时出错: {ex.Message}");
            }
            
            return null;
        }
        
        /// <summary>
        /// 批量获取中文标题映射
        /// </summary>
        /// <param name="titleIds">标题ID列表</param>
        /// <returns>标题ID到中文标题的映射字典</returns>
        public Dictionary<string, string> GetChineseTitlesByTitleIds(IEnumerable<string> titleIds)
        {
            var result = new Dictionary<string, string>();
            
            if (!IsXboxGamesDbAvailable() || titleIds == null)
                return result;
                
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={_xboxGamesDbPath};Version=3;"))
                {
                    connection.Open();
                    
                    // 构建IN查询语句
                    var titleIdList = titleIds.ToList();
                    if (titleIdList.Count == 0)
                        return result;
                        
                    var parameters = new List<string>();
                    using (var command = new SQLiteCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "SELECT TitleId, Title, Title_cn FROM ContentItems WHERE TitleId IN (" + 
                            string.Join(",", titleIdList.Select((_, i) => "@titleId" + i)) + ")";
                        
                        for (int i = 0; i < titleIdList.Count; i++)
                        {
                            // 转换为8位小写十六进制格式
                            if (int.TryParse(titleIdList[i], System.Globalization.NumberStyles.HexNumber, null, out int titleIdInt))
                            {
                                command.Parameters.AddWithValue("@titleId" + i, titleIdInt.ToString("x8"));
                            }
                        }
                        
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var dbTitleId = reader["TitleId"]?.ToString();
                                var titleCn = reader["Title_cn"]?.ToString();
                                var title = reader["Title"]?.ToString();
                                
                                // 只有当中文标题与英文标题不同时才添加
                                if (!string.IsNullOrEmpty(titleCn) && titleCn != title && !result.ContainsKey(dbTitleId))
                                {
                                    result[dbTitleId] = titleCn;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录异常但不中断程序
                System.Diagnostics.Debug.WriteLine($"批量获取中文标题时出错: {ex.Message}");
            }
            
            return result;
        }
        
        /// <summary>
        /// 本地化ContentItems集合
        /// </summary>
        /// <param name="contentItems">ContentItems集合</param>
        /// <returns>已本地化的ContentItems数量</returns>
        public int LocalizeContentItems(IEnumerable<ContentItem> contentItems)
        {
            if (contentItems == null)
                return 0;
                
            var itemsList = contentItems.ToList();
            if (itemsList.Count == 0)
                return 0;
                
            // 提取所有唯一的TitleId
            var titleIds = itemsList
                .Where(item => item != null && item.TitleId != 0)
                .Select(item => item.TitleId.ToString("x8")) // 转换为8位小写十六进制格式
                .Distinct()
                .ToList();
                
            // 批量获取中文标题映射
            var titleToChineseMap = GetChineseTitlesByTitleIds(titleIds);
            
            // 应用本地化
            int localizedCount = 0;
            foreach (var item in itemsList)
            {
                if (item != null && item.TitleId != 0)
                {
                    var hexTitleId = item.TitleId.ToString("x8");
                    if (titleToChineseMap.TryGetValue(hexTitleId, out string chineseTitle))
                    {
                        // 保存原始英文标题
                        var originalTitle = item.TitleName;
                        
                        // 更新标题为中文标题
                        item.TitleName = chineseTitle;
                        localizedCount++;
                        
                        System.Diagnostics.Debug.WriteLine($"已本地化: {originalTitle} -> {chineseTitle} (TitleId: {hexTitleId})");
                    }
                }
            }
            
            return localizedCount;
        }
    }
}