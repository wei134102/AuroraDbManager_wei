// 
// XboxGameItem.cs
// AuroraDbManager
//
// Created by Assistant on 2025/11/22
// Copyright © 2025 All rights reserved.
//

using System;
using System.Data;

namespace AuroraDbManager.Database {
    /// <summary>
    /// 表示一个Xbox 360游戏项
    /// </summary>
    public class XboxGameItem {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="row">数据行</param>
        public XboxGameItem(DataRow row) {
            DataRow = row;
        }

        /// <summary>
        /// 数据行
        /// </summary>
        internal DataRow DataRow { get; private set; }

        /// <summary>
        /// 是否已更改
        /// </summary>
        public bool Changed { get; set; }

        /// <summary>
        /// 游戏ID
        /// </summary>
        public int Id {
            get {
                var value = DataRow["Id"];
                return value is long ? (int)(long)value : Convert.ToInt32(value);
            }
        }

        /// <summary>
        /// 标题ID
        /// </summary>
        public string TitleId {
            get {
                return DataRow["TitleId"]?.ToString() ?? string.Empty;
            }
            set {
                DataRow["TitleId"] = value;
                Changed = true;
            }
        }

        /// <summary>
        /// 英文标题
        /// </summary>
        public string Title {
            get {
                return DataRow["Title"]?.ToString() ?? string.Empty;
            }
            set {
                DataRow["Title"] = value;
                Changed = true;
            }
        }

        /// <summary>
        /// 中文标题
        /// </summary>
        public string TitleCn {
            get {
                return DataRow["Title_cn"]?.ToString() ?? string.Empty;
            }
            set {
                DataRow["Title_cn"] = value;
                Changed = true;
            }
        }

        /// <summary>
        /// 开发商
        /// </summary>
        public string Developer {
            get {
                return DataRow["Developer"]?.ToString() ?? string.Empty;
            }
            set {
                DataRow["Developer"] = value;
                Changed = true;
            }
        }

        /// <summary>
        /// 发行商
        /// </summary>
        public string Publisher {
            get {
                return DataRow["Publisher"]?.ToString() ?? string.Empty;
            }
            set {
                DataRow["Publisher"] = value;
                Changed = true;
            }
        }

        /// <summary>
        /// 平台
        /// </summary>
        public string Platform {
            get {
                return DataRow["Platform"]?.ToString() ?? string.Empty;
            }
            set {
                DataRow["Platform"] = value;
                Changed = true;
            }
        }

        /// <summary>
        /// 文件夹标题
        /// </summary>
        public string FolderTitle {
            get {
                return DataRow["FolderTitle"]?.ToString() ?? string.Empty;
            }
            set {
                DataRow["FolderTitle"] = value;
                Changed = true;
            }
        }
    }
}