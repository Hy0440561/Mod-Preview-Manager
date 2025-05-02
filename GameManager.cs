using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace GameModBrowser
{
    public class Game
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FolderPath { get; set; }
        public string LogoPath { get; set; }
        public Image Logo { get; set; }

        public Game(string id, string name, string folderPath, string logoPath)
        {
            Id = id;
            Name = name;
            FolderPath = folderPath;
            LogoPath = logoPath;

            // 加载游戏Logo
            if (File.Exists(logoPath))
            {
                try
                {
                    Logo = Image.FromFile(logoPath);
                }
                catch (Exception ex)
                {
                    // 加载失败时使用默认图标
                    Console.WriteLine($"无法加载游戏图标: {id} - {ex.Message}");
                    Logo = null;
                }
            }
            
            Console.WriteLine($"创建游戏: ID={id}, 名称={name}");
        }
    }

    public class GameManager
    {
        private List<Game> games;
        private string configFilePath;
        private string gameDataFolder;
        
        // 添加游戏角色映射字典
        private Dictionary<string, List<string>> gameCharactersMap;
        
        public GameManager()
        {
            games = new List<Game>();
            gameCharactersMap = new Dictionary<string, List<string>>();
            
            // 设置配置文件和游戏数据文件夹路径
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GameModBrowser"
            );
            
            // 确保文件夹存在
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            configFilePath = Path.Combine(appDataPath, "games.json");
            gameDataFolder = Path.Combine(appDataPath, "GameData");
            
            if (!Directory.Exists(gameDataFolder))
            {
                Directory.CreateDirectory(gameDataFolder);
            }

            // 初始化预设的游戏角色映射
            InitializeGameCharacters();

            // 加载游戏列表
            LoadGames();
            
            // 如果没有游戏，添加默认游戏
            if (games.Count == 0)
            {
                AddDefaultGames();
            }
        }

        // 初始化游戏角色映射
        private void InitializeGameCharacters()
        {
            // 为鸣潮游戏添加角色，只保留长离、今夕、守岸人三个角色
            List<string> wutheringWavesCharacters = new List<string>
            {
                "长离", "今夕", "守岸人", "坎特雷拉",
                // 添加新角色
                "赞妮", "忌炎", "鉴心", "卡卡罗", "安可", "维里奈", "凌阳", 
                "漂泊者-男", "漂泊者-女", "秧秧", "白芷", "炽霞", "散华", 
                "秋水", "丹瑾", "莫特斐", "渊武", "桃祈"
            };
            gameCharactersMap["wuthering-waves"] = wutheringWavesCharacters;

            // 为星穹铁道添加角色
            List<string> starRailCharacters = new List<string>
            {
                "黄泉", "镜流", "停云", 
                // 添加新角色
                "姬子", "艾丝妲", "虎克", "三月七", "黑塔", "佩拉", "杰帕德", 
                "彦卿", "瑕蝶", "景元", "白露", "万敌", "卡芙卡", "希露瓦", 
                "那刻夏", "刃", "布洛妮娅", "丹恒", "桑博", "素裳", "克拉拉", 
                "娜塔莎", "希尔", "银狼", "青雀", "符玄", "罗刹", "瓦尔特"
            };
            gameCharactersMap["honkai-star-rail"] = starRailCharacters;

            // 为原神和绝区零创建空角色列表
            gameCharactersMap["genshin-impact"] = new List<string>();
            gameCharactersMap["zenless-zone-zero"] = new List<string>();
        }

        // 获取指定游戏的角色列表
        public List<string> GetCharactersByGameId(string gameId)
        {
            if (gameCharactersMap.ContainsKey(gameId))
            {
                return new List<string>(gameCharactersMap[gameId]);
            }
            return new List<string>(); // 返回空列表
        }

        // 为指定游戏添加角色
        public bool AddCharacterToGame(string gameId, string character)
        {
            if (!gameCharactersMap.ContainsKey(gameId))
            {
                gameCharactersMap[gameId] = new List<string>();
            }

            if (!gameCharactersMap[gameId].Contains(character))
            {
                gameCharactersMap[gameId].Add(character);
                return true;
            }
            return false;
        }

        private void LoadGames()
        {
            games.Clear();
            
            // 从配置文件加载游戏列表
            if (File.Exists(configFilePath))
            {
                try
                {
                    string json = File.ReadAllText(configFilePath);
                    // 这里简化处理，实际应该使用JSON序列化
                    // var gameList = JsonSerializer.Deserialize<List<Game>>(json);
                    // if (gameList != null)
                    // {
                    //     games = gameList;
                    // }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"加载游戏列表失败: {ex.Message}");
                }
            }
        }

        private void SaveGames()
        {
            try
            {
                // 保存游戏列表到配置文件
                // string json = JsonSerializer.Serialize(games);
                // File.WriteAllText(configFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存游戏列表失败: {ex.Message}");
            }
        }

        // 添加默认游戏方法
        private void AddDefaultGames()
        {
            // 添加鸣潮游戏
            AddDefaultGame("wuthering-waves", "鸣潮");
            
            // 添加星穹铁道
            AddDefaultGame("honkai-star-rail", "星穹铁道");
            
            // 添加原神
            AddDefaultGame("genshin-impact", "原神");
            
            // 添加绝区零
            AddDefaultGame("zenless-zone-zero", "绝区零");
        }

        // 添加单个默认游戏的辅助方法
        private void AddDefaultGame(string gameId, string gameName)
        {
            string logoPath = Path.Combine(gameDataFolder, $"{gameId}.png");
            
            // 如果没有图标，创建一个占位图标
            if (!File.Exists(logoPath))
            {
                try
                {
                    Bitmap placeholder = new Bitmap(100, 100);
                    using (Graphics g = Graphics.FromImage(placeholder))
                    {
                        // 不同游戏使用不同的颜色
                        Color backgroundColor = Color.Purple;
                        switch (gameId)
                        {
                            case "honkai-star-rail":
                                backgroundColor = Color.Blue;
                                break;
                            case "genshin-impact":
                                backgroundColor = Color.Green;
                                break;
                            case "zenless-zone-zero":
                                backgroundColor = Color.Orange;
                                break;
                        }
                        
                        g.Clear(backgroundColor);
                    }
                    placeholder.Save(logoPath);
                }
                catch
                {
                    // 忽略异常
                }
            }

            AddGame(gameId, gameName, Path.Combine(gameDataFolder, gameId), logoPath);
        }

        // 原来的AddDefaultGame方法
        private void AddDefaultGame()
        {
            // 保留此方法以向后兼容，现在只是调用新的方法
            AddDefaultGame("wuthering-waves", "鸣潮");
        }

        public List<Game> GetGames()
        {
            return games;
        }

        public Game GetGameById(string id)
        {
            return games.FirstOrDefault(g => g.Id == id);
        }

        public bool AddGame(string id, string name, string folderPath, string logoPath)
        {
            // 检查ID是否已存在
            if (games.Any(g => g.Id == id))
            {
                return false;
            }

            // 确保游戏文件夹存在
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // 添加游戏
            games.Add(new Game(id, name, folderPath, logoPath));
            
            // 保存游戏列表
            SaveGames();
            
            return true;
        }

        public bool RemoveGame(string id)
        {
            var game = GetGameById(id);
            if (game == null)
            {
                return false;
            }

            games.Remove(game);
            SaveGames();
            
            return true;
        }

        public bool UpdateGame(string id, string name, string folderPath, string logoPath)
        {
            var game = GetGameById(id);
            if (game == null)
            {
                return false;
            }

            game.Name = name;
            game.FolderPath = folderPath;
            
            if (logoPath != game.LogoPath)
            {
                game.LogoPath = logoPath;
                // 更新Logo
                if (File.Exists(logoPath))
                {
                    try
                    {
                        game.Logo = Image.FromFile(logoPath);
                    }
                    catch
                    {
                        // 加载失败时保持现有Logo
                    }
                }
            }

            SaveGames();
            
            return true;
        }
    }
} 