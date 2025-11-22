import json

def analyze_xbox_data():
    # 读取更新后的游戏数据
    with open('xbox360_games_updated.json', 'r', encoding='utf-8') as f:
        games_data = json.load(f)
    
    # 读取翻译数据
    with open('xbox_translations.json', 'r', encoding='utf-8') as f:
        translations_data = json.load(f)
    
    # 统计信息
    total_games = len(games_data)
    games_with_chinese = 0
    games_without_chinese = 0
    
    # 平台统计
    platform_count = {}
    
    # 开发商统计
    developer_count = {}
    
    # 标题ID统计
    title_id_count = {}
    
    # 分析游戏数据
    for game in games_data:
        # 统计中文标题
        if 'Title_cn' in game and game['Title_cn'] != game['Title']:
            games_with_chinese += 1
        else:
            games_without_chinese += 1
        
        # 统计平台
        platform = game.get('Platform', 'Unknown')
        platform_count[platform] = platform_count.get(platform, 0) + 1
        
        # 统计开发商
        developer = game.get('Developer', 'Unknown')
        if developer != '???':
            developer_count[developer] = developer_count.get(developer, 0) + 1
            
        # 统计标题ID
        title_id = game.get('TitleID', 'Unknown')
        title_id_count[title_id] = title_id_count.get(title_id, 0) + 1
    
    # 输出统计报告
    print("=" * 60)
    print("Xbox 360 游戏数据统计报告")
    print("=" * 60)
    print(f"总游戏数量: {total_games}")
    print(f"有中文标题的游戏: {games_with_chinese}")
    print(f"无中文标题的游戏: {games_without_chinese}")
    print(f"中文标题覆盖率: {games_with_chinese/total_games*100:.2f}%")
    
    print("\n平台分布:")
    print("-" * 30)
    for platform, count in sorted(platform_count.items(), key=lambda x: x[1], reverse=True):
        print(f"{platform}: {count}")
    
    print("\n主要开发商 (Top 20):")
    print("-" * 30)
    sorted_developers = sorted(developer_count.items(), key=lambda x: x[1], reverse=True)
    for developer, count in sorted_developers[:20]:
        print(f"{developer}: {count}")
    
    print("\n重复标题ID:")
    print("-" * 30)
    duplicate_ids = {tid: count for tid, count in title_id_count.items() if count > 1}
    if duplicate_ids:
        for tid, count in sorted(duplicate_ids.items(), key=lambda x: x[1], reverse=True):
            print(f"{tid}: {count} 次出现")
    else:
        print("未发现重复的标题ID")
    
    # 检查翻译数据使用情况
    print("\n翻译数据使用情况:")
    print("-" * 30)
    used_translations = 0
    unused_translations = []
    
    game_titles = {game['Title'] for game in games_data}
    for english_title in translations_data:
        if english_title in game_titles:
            used_translations += 1
        else:
            unused_translations.append(english_title)
    
    print(f"翻译数据总数: {len(translations_data)}")
    print(f"已使用的翻译: {used_translations}")
    print(f"未使用的翻译: {len(unused_translations)}")
    
    if unused_translations:
        print("\n部分未使用的翻译:")
        for title in unused_translations[:10]:
            print(f"- {title}")

if __name__ == "__main__":
    analyze_xbox_data()