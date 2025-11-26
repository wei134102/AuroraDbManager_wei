import json
import os

def parse_txt_file(file_path):
    """Parse the txt file and return a dictionary with Title ID as key and game info as value"""
    data = {}
    if os.path.exists(file_path):
        with open(file_path, 'r', encoding='utf-8') as f:
            lines = f.readlines()
            
            # Skip header line
            for line in lines[1:]:
                # Skip empty lines
                if not line.strip():
                    continue
                    
                # Split the line by tab, filter out empty strings caused by consecutive tabs
                parts = [part.strip() for part in line.strip().split('\t') if part.strip()]
                if len(parts) >= 5:
                    title_id = parts[0].lower()  # Convert to lowercase for consistency with JSON
                    game_name = parts[1]
                    developer = parts[2]
                    category = parts[3]
                    year = parts[4]
                    
                    # Only add if Title ID is not already in the data (to avoid duplicates)
                    if title_id and title_id not in data:
                        data[title_id] = {
                            'Title': game_name,
                            'Developer': developer,
                            'Publisher': '',  # 文件中没有Publisher列，设为空
                            'Category': category,
                            'Year': year
                        }
                elif len(parts) >= 4:
                    # Handle cases where Year might be missing
                    title_id = parts[0].lower()
                    game_name = parts[1]
                    developer = parts[2]
                    category = parts[3]
                    year = ''
                    
                    if title_id and title_id not in data:
                        data[title_id] = {
                            'Title': game_name,
                            'Developer': developer,
                            'Publisher': '',
                            'Category': category,
                            'Year': year
                        }
    return data

def update_xbox_games_with_chinese_titles():
    # 定义文件路径
    games_file = 'xbox360_games.json'
    translations_file = 'xbox_translations.json'
    txt_file1 = 'xbox360.txt'
    txt_file2 = 'xbox360live.txt'
    output_file = 'xbox360_games_updated.json'
    
    # 读取游戏数据
    with open(games_file, 'r', encoding='utf-8') as f:
        games_data = json.load(f)
    
    # 读取翻译数据
    with open(translations_file, 'r', encoding='utf-8') as f:
        translations_data = json.load(f)
    
    # 读取txt文件中的游戏信息
    txt_data = {}
    txt_data.update(parse_txt_file(txt_file1))
    txt_data.update(parse_txt_file(txt_file2))
    
    # 创建一个以Title ID为键的现有游戏字典，方便查找
    existing_games = {}
    for game in games_data:
        title_id = game.get('Title ID')
        if title_id:
            existing_games[title_id.lower()] = game
    
    # 为每个游戏添加中文标题和额外信息
    updated_count = 0
    category_year_added_count = 0
    
    # 先处理已有的游戏
    for game in games_data:
        # 添加中文标题
        english_title = game.get('Title')
        if english_title in translations_data:
            game['Title_cn'] = translations_data[english_title]
            updated_count += 1
        else:
            # 如果没有找到翻译，则使用英文标题作为默认值
            game['Title_cn'] = english_title
            
        # 添加Developer、Category和Year信息（如果有）
        title_id = game.get('Title ID')
        if title_id:
            # Convert to lowercase for consistency with txt files
            title_id_lower = title_id.lower()
            if title_id_lower in txt_data:
                txt_game_info = txt_data[title_id_lower]
                developer = txt_game_info.get('Developer')
                category = txt_game_info.get('Category')
                year = txt_game_info.get('Year')
                
                # 检查是否需要更新Developer
                needs_developer = developer and (game.get('Developer') == '???' or not game.get('Developer'))
                # 检查是否需要添加Category
                needs_category = category and ('Category' not in game or not game.get('Category'))
                # 检查是否需要添加Year
                needs_year = year and ('Year' not in game or not game.get('Year'))
                
                # 更新Developer信息
                if needs_developer:
                    game['Developer'] = developer
                # 添加Category信息
                if needs_category:
                    game['Category'] = category
                # 添加Year信息
                if needs_year:
                    game['Year'] = year
                    
                # Count this as adding category/year info only if at least one was added
                if needs_developer or needs_category or needs_year:
                    category_year_added_count += 1
    
    # 添加txt文件中有但JSON中没有的游戏
    new_games_count = 0
    for title_id, txt_game_info in txt_data.items():
        if title_id not in existing_games:
            # 创建新游戏条目
            new_game = {
                'Platform': 'Xbox 360',
                'Title ID': title_id,  # Keep the lowercase format
                'Title': txt_game_info['Title'],
                'Developer': txt_game_info['Developer'],
                'Publisher': txt_game_info['Publisher']
            }
            
            # 添加中文标题（如果有）
            if txt_game_info['Title'] in translations_data:
                new_game['Title_cn'] = translations_data[txt_game_info['Title']]
                updated_count += 1
            else:
                new_game['Title_cn'] = txt_game_info['Title']
            
            # 添加Category和Year
            if txt_game_info['Category']:
                new_game['Category'] = txt_game_info['Category']
            if txt_game_info['Year']:
                new_game['Year'] = txt_game_info['Year']
            
            # 添加到游戏列表
            games_data.append(new_game)
            new_games_count += 1
    
    # Update count for category/year additions (we counted new games as having category/year added)
    category_year_added_count += new_games_count
    
    # 保存更新后的数据
    with open(output_file, 'w', encoding='utf-8') as f:
        json.dump(games_data, f, ensure_ascii=False, indent=4)
    
    print(f"处理完成！总共处理了 {len(games_data)} 个游戏，其中:")
    print(f"- 原始游戏数量: {len(existing_games)}")
    print(f"- 新增游戏数量: {new_games_count}")
    print(f"- 添加了中文标题的游戏: {updated_count} 个")
    print(f"- 添加了 Category 和/或 Year 信息的游戏: {category_year_added_count} 个")
    print(f"更新后的文件已保存为: {output_file}")

if __name__ == "__main__":
    update_xbox_games_with_chinese_titles()