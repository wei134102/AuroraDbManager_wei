import json
import os

def update_xbox_games_with_chinese_titles():
    # 定义文件路径
    games_file = 'xbox360_games.json'
    translations_file = 'xbox_translations.json'
    output_file = 'xbox360_games_updated.json'
    
    # 读取游戏数据
    with open(games_file, 'r', encoding='utf-8') as f:
        games_data = json.load(f)
    
    # 读取翻译数据
    with open(translations_file, 'r', encoding='utf-8') as f:
        translations_data = json.load(f)
    
    # 为每个游戏添加中文标题
    updated_count = 0
    for game in games_data:
        english_title = game.get('Title')
        if english_title in translations_data:
            game['Title_cn'] = translations_data[english_title]
            updated_count += 1
        else:
            # 如果没有找到翻译，则使用英文标题作为默认值
            game['Title_cn'] = english_title
    
    # 保存更新后的数据
    with open(output_file, 'w', encoding='utf-8') as f:
        json.dump(games_data, f, ensure_ascii=False, indent=4)
    
    print(f"处理完成！总共处理了 {len(games_data)} 个游戏，其中 {updated_count} 个游戏添加了中文标题。")
    print(f"更新后的文件已保存为: {output_file}")

if __name__ == "__main__":
    update_xbox_games_with_chinese_titles()