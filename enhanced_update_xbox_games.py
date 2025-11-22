import json
import os

def update_xbox_games_with_chinese_titles():
    # 定义文件路径
    games_file = 'xbox360_games.json'
    translations_file = 'xbox_translations.json'
    output_file = 'xbox360_games_updated.json'
    
    # 检查文件是否存在
    if not os.path.exists(games_file):
        print(f"错误: 找不到游戏数据文件 {games_file}")
        return
    
    if not os.path.exists(translations_file):
        print(f"错误: 找不到翻译数据文件 {translations_file}")
        return
    
    try:
        # 读取游戏数据
        print("正在读取游戏数据...")
        with open(games_file, 'r', encoding='utf-8') as f:
            games_data = json.load(f)
        
        # 读取翻译数据
        print("正在读取翻译数据...")
        with open(translations_file, 'r', encoding='utf-8') as f:
            translations_data = json.load(f)
        
        # 统计信息
        total_games = len(games_data)
        updated_count = 0
        unchanged_count = 0
        
        # 为每个游戏添加中文标题
        print("正在处理游戏数据...")
        for game in games_data:
            english_title = game.get('Title')
            if english_title in translations_data:
                game['Title_cn'] = translations_data[english_title]
                updated_count += 1
            else:
                # 如果没有找到翻译，则使用英文标题作为默认值
                game['Title_cn'] = english_title
                unchanged_count += 1
        
        # 保存更新后的数据
        print("正在保存更新后的数据...")
        with open(output_file, 'w', encoding='utf-8') as f:
            json.dump(games_data, f, ensure_ascii=False, indent=4)
        
        # 输出统计信息
        print("=" * 50)
        print("处理完成！")
        print(f"总共处理了 {total_games} 个游戏")
        print(f"其中 {updated_count} 个游戏添加了中文标题")
        print(f"其中 {unchanged_count} 个游戏未找到对应翻译，使用英文标题")
        print(f"更新后的文件已保存为: {output_file}")
        print("=" * 50)
        
        # 显示前5个处理结果示例
        print("\n前5个处理结果示例:")
        for i, game in enumerate(games_data[:5]):
            print(f"{i+1}. {game['Title']} -> {game['Title_cn']}")
            
    except Exception as e:
        print(f"处理过程中出现错误: {str(e)}")
        return

def create_backup():
    """创建原始文件的备份"""
    import shutil
    games_file = 'xbox360_games.json'
    backup_file = 'xbox360_games_backup.json'
    
    if os.path.exists(games_file):
        shutil.copy2(games_file, backup_file)
        print(f"已创建原始文件备份: {backup_file}")
    else:
        print(f"找不到原始文件 {games_file}，无法创建备份")

if __name__ == "__main__":
    print("Xbox 360 游戏数据更新工具")
    print("=" * 30)
    
    # 询问是否需要创建备份
    choice = input("是否需要创建原始文件备份? (y/n): ")
    if choice.lower() == 'y':
        create_backup()
    
    # 执行更新操作
    update_xbox_games_with_chinese_titles()