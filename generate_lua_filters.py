#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Xbox 360游戏分类Lua过滤器生成器
根据xbox360.txt和xbox360live.txt文件生成对应的Lua分类文件
"""

import os
import re
import json
from pathlib import Path

class Xbox360LuaGenerator:
    def __init__(self):
        self.games_data = []
        self.translations = {}
        
    def load_translations(self, translations_file):
        """加载游戏翻译数据"""
        try:
            with open(translations_file, 'r', encoding='utf-8') as f:
                self.translations = json.load(f)
            print(f"已加载 {len(self.translations)} 个游戏翻译")
        except Exception as e:
            print(f"加载翻译文件失败: {e}")
            self.translations = {}
    
    def parse_game_file(self, file_path):
        """解析游戏数据文件"""
        games = []
        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                lines = f.readlines()
            
            # 跳过标题行
            for line in lines[1:]:
                line = line.strip()
                if not line:
                    continue
                
                # 解析制表符分隔的数据
                parts = line.split('\t')
                if len(parts) >= 4:
                    title_id = parts[0].strip()
                    game_name = parts[1].strip()
                    developer = parts[2].strip()
                    category = parts[3].strip() if len(parts) > 3 else ''
                    year = parts[4].strip() if len(parts) > 4 else ''
                    
                    games.append({
                        'title_id': title_id,
                        'game_name': game_name,
                        'developer': developer,
                        'category': category,
                        'year': year,
                        'hex_id': self.convert_to_hex(title_id)
                    })
            
            print(f"从 {os.path.basename(file_path)} 解析了 {len(games)} 个游戏")
            return games
            
        except Exception as e:
            print(f"解析文件 {file_path} 失败: {e}")
            return []
    
    def convert_to_hex(self, title_id):
        """将Title ID转换为16进制格式"""
        try:
            # 移除可能的空格和特殊字符
            title_id = title_id.replace(' ', '').replace('®', '')
            # 转换为16进制，前面加0x
            if len(title_id) == 8 and all(c in '0123456789ABCDEFabcdef' for c in title_id):
                return '0x' + title_id.upper()
            else:
                return None
        except:
            return None
    
    def categorize_games(self, games):
        """按类别对游戏进行分类 - 直接使用原始分类名称"""
        categories = {}
        
        for game in games:
            category = game.get('category', 'Unknown')
            
            if category not in categories:
                categories[category] = []
            
            if game['hex_id']:
                categories[category].append(game)
        
        return categories
    
    def generate_lua_file(self, category, games, output_dir):
        """生成Lua分类文件 - 使用原始分类名称作为文件名和内容"""
        if not games:
            return

        # 直接使用原始分类名称
        file_name = f"{category}.lua"
        file_path = os.path.join(output_dir, file_name)

        # 生成Lua内容 - 使用原始分类名称
        lua_content = f"GameListFilterCategories.User[\"{category}\"] = function(Content)\nreturn ("

        # 添加游戏Title ID
        for i, game in enumerate(games):
            if i == 0:
                lua_content += f"Content.TitleId == {game['hex_id']}"
            else:
                lua_content += f"\nor Content.TitleId == {game['hex_id']}"

        lua_content += "\n)\nend\n"

        try:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(lua_content)
            print(f"生成文件: {file_name} ({len(games)} 个游戏)")
        except Exception as e:
            print(f"生成文件 {file_path} 失败: {e}")
    
    def generate_all_lua_files(self, xbox360_file, xboxlive_file, translations_file, output_dir):
        """生成所有Lua分类文件"""
        # 创建输出目录
        Path(output_dir).mkdir(parents=True, exist_ok=True)
        
        # 加载翻译
        self.load_translations(translations_file)
        
        # 解析游戏文件
        xbox360_games = self.parse_game_file(xbox360_file)
        xboxlive_games = []
        if xboxlive_file and os.path.exists(xboxlive_file):
            xboxlive_games = self.parse_game_file(xboxlive_file)
        
        all_games = xbox360_games + xboxlive_games
        print(f"总共处理了 {len(all_games)} 个游戏")
        
        # 按类别分类
        categories = self.categorize_games(all_games)
        
        # 生成Lua文件
        for category, games in categories.items():
            self.generate_lua_file(category, games, output_dir)
        
        # 生成统计信息
        self.generate_statistics(categories, output_dir)
        
        print(f"\nLua文件生成完成！文件保存在: {output_dir}")
    
    def extract_categories_to_genres(self, xbox360_file, xboxlive_file, genres_file):
        """第一步：提取所有分类到genres.txt文件，并翻译对应的中文名称"""
        categories = set()
        translations = {
            'Action': '动作',
            'Shooter': '射击',
            'Fighting': '格斗',
            'Sports': '体育',
            'Racing': '赛车',
            'Role Playing': '角色扮演',
            'Strategy': '策略',
            'Music': '音乐',
            'Family': '家庭',
            'Platformer': '平台',
            'Puzzle': '益智',
            'Flight': '飞行',
            'Kinect': '体感',
            'Arcade': '街机',
            'Other': '其他',
            'Unknown': '未知'
        }

        # 解析xbox360.txt
        if os.path.exists(xbox360_file):
            try:
                with open(xbox360_file, 'r', encoding='utf-8') as f:
                    lines = f.readlines()

                for line in lines[1:]:  # 跳过标题行
                    line = line.strip()
                    if not line:
                        continue

                    parts = line.split('\t')
                    if len(parts) > 3:  # 第4个元素是分类（索引3）
                        category = parts[3].strip()
                        if category:
                            categories.add(category)
            except Exception as e:
                print(f"解析xbox360.txt失败: {e}")

        # 解析xbox360live.txt
        if xboxlive_file and os.path.exists(xboxlive_file):
            try:
                with open(xboxlive_file, 'r', encoding='utf-8') as f:
                    lines = f.readlines()

                for line in lines[1:]:  # 跳过标题行
                    line = line.strip()
                    if not line:
                        continue

                    parts = line.split('\t')
                    if len(parts) > 3:  # 第4个元素是分类（索引3）
                        category = parts[3].strip()
                        if category:
                            categories.add(category)
            except Exception as e:
                print(f"解析xbox360live.txt失败: {e}")
        elif xboxlive_file:
            print(f"警告: 找不到文件 {xboxlive_file}, 将只处理xbox360.txt")

        # 保存分类到genres.txt文件
        try:
            with open(genres_file, 'w', encoding='utf-8') as f:
                f.write("Xbox 360游戏分类列表\n")
                f.write("=" * 30 + "\n")
                for category in sorted(categories):
                    translated_name = translations.get(category, '未翻译')
                    f.write(f"{category} | {translated_name}\n")
            print(f"✓ 已提取 {len(categories)} 个分类到 {genres_file}")
        except Exception as e:
            print(f"保存分类文件失败: {e}")

        return categories

    def generate_statistics(self, categories, output_dir):
        """生成统计信息"""
        stats_file = os.path.join(output_dir, "statistics.txt")
        
        with open(stats_file, 'w', encoding='utf-8') as f:
            f.write("Xbox 360游戏分类统计\n")
            f.write("=" * 40 + "\n\n")
            
            total_games = 0
            for category, games in sorted(categories.items()):
                count = len(games)
                total_games += count
                f.write(f"{category}: {count} 个游戏\n")
            
            f.write(f"\n总计: {total_games} 个游戏\n")
        
        print(f"生成统计文件: statistics.txt")

def main():
    # 配置路径 - 使用Python程序所在目录
    script_dir = os.path.dirname(os.path.abspath(__file__))
    base_dir = script_dir  # 当前目录就是根目录
    
    xbox360_file = os.path.join(base_dir, "xbox360.txt")
    xboxlive_file = os.path.join(base_dir, "xbox360live.txt")  
    translations_file = os.path.join(base_dir, "xbox_translations.json")
    genres_file = os.path.join(script_dir, "genres.txt")  # 分类文件在根目录，文件名为genres.txt
    output_dir = os.path.join(script_dir, "lua")  # Lua文件在py程序目录下的lua文件夹
    
    # 检查输入文件
    if not os.path.exists(xbox360_file):
        print(f"错误: 找不到文件 {xbox360_file}")
        return
    
    if not os.path.exists(xboxlive_file):
        print(f"警告: 找不到文件 {xboxlive_file}, 将只处理xbox360.txt")
        xboxlive_file = None  # 如果文件不存在就设为None
    
    print("=== Xbox 360 Lua过滤器生成器 ===")
    
    # 生成Lua文件
    generator = Xbox360LuaGenerator()
    
    # 第一步：提取分类到genres.txt
    print("\n第一步：提取游戏分类...")
    generator.extract_categories_to_genres(xbox360_file, xboxlive_file, genres_file)
    
    # 第二步：生成Lua文件
    print("\n第二步：生成Lua分类文件...")
    generator.generate_all_lua_files(
        xbox360_file, 
        xboxlive_file, 
        translations_file, 
        output_dir
    )
    
    print("\n✓ 所有任务完成！")

if __name__ == "__main__":
    main()