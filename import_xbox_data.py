import json
import sqlite3
import os

def create_content_table(cursor):
    """创建ContentItems表"""
    cursor.execute('''
        CREATE TABLE IF NOT EXISTS ContentItems (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            TitleId TEXT NOT NULL,
            Title TEXT,
            Title_cn TEXT,
            Developer TEXT,
            Publisher TEXT,
            Platform TEXT,
            FolderTitle TEXT
        )
    ''')

def import_xbox_games_to_db():
    """将Xbox游戏数据导入到SQLite数据库"""
    # 检查游戏数据文件是否存在
    if not os.path.exists('xbox360_games_updated.json'):
        print("错误: 找不到 xbox360_games_updated.json 文件")
        return
    
    # 读取游戏数据
    with open('xbox360_games_updated.json', 'r', encoding='utf-8') as f:
        games_data = json.load(f)
    
    # 连接到数据库（如果不存在会自动创建）
    conn = sqlite3.connect('xbox_games.db')
    cursor = conn.cursor()
    
    # 创建表
    create_content_table(cursor)
    
    # 插入数据
    inserted_count = 0
    for game in games_data:
        try:
            cursor.execute('''
                INSERT INTO ContentItems 
                (TitleId, Title, Title_cn, Developer, Publisher, Platform, FolderTitle)
                VALUES (?, ?, ?, ?, ?, ?, ?)
            ''', (
                game.get('TitleID', ''),
                game.get('Title', ''),
                game.get('Title_cn', ''),
                game.get('Developer', ''),
                game.get('Publisher', ''),
                game.get('Platform', ''),
                game.get('Folder Title', '')
            ))
            inserted_count += 1
        except Exception as e:
            print(f"插入游戏 '{game.get('Title', 'Unknown')}' 时出错: {e}")
    
    # 提交更改并关闭连接
    conn.commit()
    conn.close()
    
    print(f"成功导入 {inserted_count} 个游戏到数据库")
    print("数据库文件已保存为: xbox_games.db")

def query_sample_data():
    """查询并显示示例数据"""
    if not os.path.exists('xbox_games.db'):
        print("数据库文件不存在")
        return
    
    conn = sqlite3.connect('xbox_games.db')
    cursor = conn.cursor()
    
    # 查询前10个游戏
    cursor.execute('SELECT Title, Title_cn, Platform FROM ContentItems LIMIT 10')
    results = cursor.fetchall()
    
    print("\n数据库中的示例数据:")
    print("=" * 50)
    for title, title_cn, platform in results:
        print(f"英文标题: {title}")
        print(f"中文标题: {title_cn}")
        print(f"平台: {platform}")
        print("-" * 30)
    
    # 统计信息
    cursor.execute('SELECT COUNT(*) FROM ContentItems')
    total_count = cursor.fetchone()[0]
    
    cursor.execute('SELECT COUNT(*) FROM ContentItems WHERE Title_cn != Title')
    translated_count = cursor.fetchone()[0]
    
    print(f"\n数据库统计:")
    print(f"总游戏数: {total_count}")
    print(f"已翻译游戏数: {translated_count}")
    print(f"翻译覆盖率: {translated_count/total_count*100:.2f}%")
    
    conn.close()

if __name__ == "__main__":
    print("Xbox游戏数据导入工具")
    print("=" * 30)
    
    # 导入数据
    import_xbox_games_to_db()
    
    # 显示示例数据
    query_sample_data()