import json

# 读取更新后的游戏数据
with open('xbox360_games_updated.json', 'r', encoding='utf-8') as f:
    games_data = json.load(f)

print('Total games:', len(games_data))
print('\nFirst 10 games with Developer info:')
for i in range(min(10, len(games_data))):
    game = games_data[i]
    print(f'Title: {game["Title"]} | Developer: {game["Developer"]}')

# 查找一些特定游戏验证Developer字段
print('\nSpecific game examples:')
game_titles = ['Air Conflicts: Secret Wars', 'Zoids Assault', 'Operation Darkness(NA)']
for game in games_data:
    if game["Title"] in game_titles:
        developer = game.get("Developer", "N/A")
        category = game.get("Category", "N/A")
        year = game.get("Year", "N/A")
        print(f'Title: {game["Title"]} | Developer: {developer} | Category: {category} | Year: {year}')
