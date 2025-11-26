// 
// GameDirectoryItem.cs
// AuroraDbManager
//
// Created by Assistant on 2025/11/26
// Copyright © 2025 All rights reserved.
//

using System;
using System.ComponentModel;
using System.IO;

namespace AuroraDbManager.Database
{
    /// <summary>
    /// 表示游戏目录中的游戏项
    /// </summary>
    public class GameDirectoryItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        /// <summary>
        /// 游戏ID（目录名）
        /// </summary>
        public string GameId { get; set; }

        /// <summary>
        /// 游戏英文标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 游戏中文标题
        /// </summary>
        public string TitleCn { get; set; }

        /// <summary>
        /// 游戏完整路径
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// 游戏大小（字节）
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 游戏大小的可读格式
        /// </summary>
        public string FormattedSize
        {
            get
            {
                return FormatBytes(Size);
            }
        }

        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}