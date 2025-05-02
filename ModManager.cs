using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace GameModBrowser
{
    public class Mod
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string GameId { get; set; }
        public string ThumbnailUrl { get; set; }
        public Image ThumbnailImage { get; set; }
        public string DownloadUrl { get; set; }
        public int Downloads { get; set; }
        public DateTime UpdateTime { get; set; }
        public string Category { get; set; }
        public double Rating { get; set; }
        public string Version { get; set; }
        public string LocalPath { get; set; }
        public bool IsInstalled { get; set; }
        public string GameCategories { get; set; } // 游戏分类（RPG、射击等）
        public string Characters { get; set; } // 角色列表

        public Mod(string id, string name, string description, string author, string gameId, 
                  string thumbnailUrl, string downloadUrl, string category)
        {
            Id = id;
            Name = name;
            Description = description;
            Author = author;
            GameId = gameId;
            ThumbnailUrl = thumbnailUrl;
            DownloadUrl = downloadUrl;
            Downloads = 0;
            UpdateTime = DateTime.Now;
            Category = category;
            Rating = 0;
            Version = "1.0";
            IsInstalled = false;
            GameCategories = "";
            Characters = "";
        }
        
        // 获取游戏分类列表
        public List<string> GetGameCategoriesList()
        {
            if (string.IsNullOrEmpty(GameCategories))
                return new List<string>();
            
            return GameCategories.Split(',').ToList();
        }
        
        // 获取角色列表
        public List<string> GetCharactersList()
        {
            if (string.IsNullOrEmpty(Characters))
                return new List<string>();
            
            return Characters.Split(',').ToList();
        }
    }

    public class ModManager
    {
        private List<Mod> mods;
        private string modDataFolder;
        private string downloadFolder;
        
        // 添加全局游戏分类和角色列表
        private List<string> globalGameCategories;
        private List<string> globalCharacters;
        
        public ModManager()
        {
            mods = new List<Mod>();
            globalGameCategories = new List<string>();
            globalCharacters = new List<string>();
            
            // 设置Mod数据和下载文件夹路径
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GameModBrowser"
            );
            
            modDataFolder = Path.Combine(appDataPath, "ModData");
            downloadFolder = Path.Combine(appDataPath, "Downloads");
            
            // 确保文件夹存在
            if (!Directory.Exists(modDataFolder))
            {
                Directory.CreateDirectory(modDataFolder);
            }
            
            if (!Directory.Exists(downloadFolder))
            {
                Directory.CreateDirectory(downloadFolder);
            }

            // 清空所有游戏分类和角色，确保没有预设选项
            DeleteAllGameCategories();
            DeleteAllCharacters();
            
            // 加载自定义游戏分类和角色
            LoadGameCategoriesAndCharacters();
            
            // 加载Mod列表
            LoadMods();
            
            // 不再自动添加示例Mod
            // 如果需要示例Mod，可以手动调用 AddSampleMods()
        }

        // 添加清空所有游戏分类的方法
        public void DeleteAllGameCategories()
        {
            globalGameCategories.Clear();
            SaveGameCategories();
            Console.WriteLine("已清空所有游戏分类");
        }

        // 添加清空所有角色的方法
        public void DeleteAllCharacters()
        {
            globalCharacters.Clear();
            SaveCharacters();
            Console.WriteLine("已清空所有角色");
        }

        // 添加加载游戏分类和角色的方法
        private void LoadGameCategoriesAndCharacters()
        {
            string categoriesFile = Path.Combine(modDataFolder, "categories.json");
            string charactersFile = Path.Combine(modDataFolder, "characters.json");
            
            try
            {
                globalGameCategories = new List<string>();
                globalCharacters = new List<string>();
                
                // 加载游戏分类
                if (File.Exists(categoriesFile))
                {
                    try {
                        string categoriesJson = File.ReadAllText(categoriesFile);
                        var categories = JsonSerializer.Deserialize<List<string>>(categoriesJson);
                        if (categories != null)
                        {
                            globalGameCategories = categories;
                            Console.WriteLine($"成功加载 {globalGameCategories.Count} 个游戏分类");
                        }
                    } 
                    catch {
                        // 如果加载失败，清空并创建一个新的空文件
                        globalGameCategories = new List<string>();
                        SaveGameCategories();
                    }
                }
                else
                {
                    // 无需初始默认分类
                    SaveGameCategories();
                }
                
                // 加载角色
                if (File.Exists(charactersFile))
                {
                    try {
                        string charactersJson = File.ReadAllText(charactersFile);
                        var characters = JsonSerializer.Deserialize<List<string>>(charactersJson);
                        if (characters != null)
                        {
                            globalCharacters = characters;
                            Console.WriteLine($"成功加载 {globalCharacters.Count} 个角色");
                        }
                    }
                    catch {
                        // 如果加载失败，清空并创建一个新的空文件
                        globalCharacters = new List<string>();
                        SaveCharacters();
                    }
                }
                else
                {
                    // 无需初始默认角色
                    SaveCharacters();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载游戏分类和角色失败: {ex.Message}");
                // 确保列表不为null
                globalGameCategories = new List<string>();
                globalCharacters = new List<string>();
            }
        }
        
        // 保存游戏分类
        private void SaveGameCategories()
        {
            try
            {
                string categoriesFile = Path.Combine(modDataFolder, "categories.json");
                string json = JsonSerializer.Serialize(globalGameCategories, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(categoriesFile, json);
                Console.WriteLine($"游戏分类已保存, 共 {globalGameCategories.Count} 个");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存游戏分类失败: {ex.Message}");
            }
        }
        
        // 保存角色
        private void SaveCharacters()
        {
            try
            {
                string charactersFile = Path.Combine(modDataFolder, "characters.json");
                string json = JsonSerializer.Serialize(globalCharacters, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(charactersFile, json);
                Console.WriteLine($"角色已保存, 共 {globalCharacters.Count} 个");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存角色失败: {ex.Message}");
            }
        }
        
        // 添加游戏分类
        public bool AddGameCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category) || globalGameCategories.Contains(category))
            {
                return false;
            }
            
            globalGameCategories.Add(category);
            SaveGameCategories();
            return true;
        }
        
        // 添加角色
        public bool AddCharacter(string character)
        {
            if (string.IsNullOrWhiteSpace(character) || globalCharacters.Contains(character))
            {
                return false;
            }
            
            globalCharacters.Add(character);
            SaveCharacters();
            return true;
        }
        
        // 获取所有游戏分类
        public List<string> GetAllGameCategories()
        {
            return new List<string>(globalGameCategories);
        }
        
        // 获取所有角色
        public List<string> GetAllCharacters()
        {
            return new List<string>(globalCharacters);
        }

        // 添加多个游戏分类
        public void AddGameCategories(IEnumerable<string> categories)
        {
            bool hasNew = false;
            foreach (var category in categories)
            {
                if (!string.IsNullOrWhiteSpace(category) && !globalGameCategories.Contains(category))
                {
                    globalGameCategories.Add(category);
                    hasNew = true;
                }
            }
            
            if (hasNew)
            {
                SaveGameCategories();
            }
        }
        
        // 添加多个角色
        public void AddCharacters(IEnumerable<string> characters)
        {
            bool hasNew = false;
            foreach (var character in characters)
            {
                if (!string.IsNullOrWhiteSpace(character) && !globalCharacters.Contains(character))
                {
                    globalCharacters.Add(character);
                    hasNew = true;
                }
            }
            
            if (hasNew)
            {
                SaveCharacters();
            }
        }

        private void LoadMods()
        {
            mods.Clear();
            
            string modDataFile = Path.Combine(modDataFolder, "mods.json");
            
            // 从JSON文件加载Mod列表
            if (File.Exists(modDataFile))
            {
                try
                {
                    string json = File.ReadAllText(modDataFile);
                    
                    // 解析JSON数据（简化实现）
                    var simplifiedMods = JsonSerializer.Deserialize<List<JsonElement>>(json);
                    
                    if (simplifiedMods != null)
                    {
                        foreach (var item in simplifiedMods)
                        {
                            try
                            {
                                string id = item.GetProperty("Id").GetString();
                                string name = item.GetProperty("Name").GetString();
                                string description = item.GetProperty("Description").GetString();
                                string author = item.GetProperty("Author").GetString();
                                string gameId = item.GetProperty("GameId").GetString();
                                string thumbnailUrl = item.GetProperty("ThumbnailUrl").GetString();
                                string downloadUrl = item.GetProperty("DownloadUrl").GetString();
                                string category = item.GetProperty("Category").GetString();
                                
                                Mod mod = new Mod(
                                    id,
                                    name,
                                    description,
                                    author,
                                    gameId,
                                    thumbnailUrl,
                                    downloadUrl,
                                    category
                                );
                                
                                // 加载其他属性
                                try { mod.Downloads = item.GetProperty("Downloads").GetInt32(); } catch { }
                                try { mod.Rating = item.GetProperty("Rating").GetDouble(); } catch { }
                                try { mod.Version = item.GetProperty("Version").GetString(); } catch { }
                                try { mod.LocalPath = item.GetProperty("LocalPath").GetString(); } catch { }
                                try { mod.IsInstalled = item.GetProperty("IsInstalled").GetBoolean(); } catch { }
                                try { mod.GameCategories = item.GetProperty("GameCategories").GetString(); } catch { }
                                try { mod.Characters = item.GetProperty("Characters").GetString(); } catch { }
                                
                                // 加载图片
                                if (!string.IsNullOrEmpty(thumbnailUrl) && File.Exists(thumbnailUrl))
                                {
                                    try
                                    {
                                        mod.ThumbnailImage = Image.FromFile(thumbnailUrl);
                                    }
                                    catch
                                    {
                                        // 如果图片加载失败，使用默认图片
                                        mod.ThumbnailImage = null;
                                    }
                                }
                                
                                mods.Add(mod);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"解析Mod项失败: {ex.Message}");
                            }
                        }
                    }
                    
                    Console.WriteLine($"成功加载 {mods.Count} 个Mod");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"加载Mod列表失败: {ex.Message}");
                }
            }
            
            // 不再添加任何样例数据
            // 如果没有加载到任何Mod，添加样例数据
            // if (mods.Count == 0)
            // {
            //     AddSampleMods();
            // }
        }
        
        private void SaveMods()
        {
            try
            {
                // 确保目录存在
                string modDataFile = Path.Combine(modDataFolder, "mods.json");
                
                // 序列化Mod列表为JSON（简化实现）
                using (StreamWriter file = File.CreateText(modDataFile))
                {
                    // 由于要序列化Image对象，这里使用简单方式保存关键数据
                    var simplifiedMods = mods.Select(m => new 
                    {
                        m.Id,
                        m.Name,
                        m.Description,
                        m.Author,
                        m.GameId,
                        m.ThumbnailUrl,
                        m.DownloadUrl,
                        m.Downloads,
                        m.UpdateTime,
                        m.Category,
                        m.Rating,
                        m.Version,
                        m.LocalPath,
                        m.IsInstalled,
                        m.GameCategories,
                        m.Characters
                    }).ToList();
                    
                    string json = JsonSerializer.Serialize(simplifiedMods, 
                        new JsonSerializerOptions { WriteIndented = true });
                    file.Write(json);
                }
                
                Console.WriteLine($"成功保存 {mods.Count} 个Mod到 {modDataFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存Mod列表失败: {ex.Message}");
            }
        }

        private void AddSampleMods()
        {
            // 注释掉，不再添加示例Mod
            // AddSampleModsForGame("wuthering-waves");
        }

        private void AddSampleModsForGame(string gameId)
        {
            string[] categories = { };
            string[] modNames = { 
                "增强角色模型", "高清武器纹理", "新地图扩展", "现代UI界面", "音乐替换包",
                "角色皮肤包", "武器特效增强", "地形优化", "深色界面主题", "环境音效包"
            };
            string[] gameCategories = { };
            
            Random random = new Random();
            
            for (int i = 0; i < 20; i++)
            {
                string category = categories.Length > 0 ? categories[random.Next(categories.Length)] : "其他";
                string name = modNames[random.Next(modNames.Length)] + " " + (i + 1);
                
                // 创建示例Mod缩略图
                Bitmap thumbnail = new Bitmap(220, 300);
                using (Graphics g = Graphics.FromImage(thumbnail))
                {
                    // 设置随机背景色
                    Color backgroundColor = Color.FromArgb(
                        random.Next(50, 100), 
                        random.Next(50, 200), 
                        random.Next(50, 200),
                        random.Next(50, 200)
                    );
                    g.Clear(backgroundColor);
                    
                    // 添加Mod名称
                    using (Font font = new Font("Arial", 16, FontStyle.Bold))
                    using (Brush brush = new SolidBrush(Color.White))
                    using (StringFormat format = new StringFormat { Alignment = StringAlignment.Center })
                    {
                        g.DrawString(name, font, brush, 
                            new RectangleF(10, 100, 200, 100), format);
                    }
                    
                    // 添加类别标签
                    using (Font font = new Font("Arial", 12))
                    using (Brush brush = new SolidBrush(Color.FromArgb(240, 240, 240)))
                    using (StringFormat format = new StringFormat { Alignment = StringAlignment.Center })
                    {
                        g.DrawString(category, font, brush, 
                            new RectangleF(10, 150, 200, 50), format);
                    }
                    
                    // 添加作者标签
                    using (Font font = new Font("Arial", 10))
                    using (Brush brush = new SolidBrush(Color.FromArgb(230, 230, 230)))
                    using (StringFormat format = new StringFormat { Alignment = StringAlignment.Center })
                    {
                        g.DrawString("作者: 示例作者" + (i % 5 + 1), font, brush, 
                            new RectangleF(10, 200, 200, 50), format);
                    }
                }
                
                // 为随机Mod指定预览图保存路径
                string appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "GameModBrowser", "ModImages"
                );
                
                if (!Directory.Exists(appDataPath))
                {
                    Directory.CreateDirectory(appDataPath);
                }
                
                string thumbnailPath = Path.Combine(appDataPath, $"sample-mod-{i}.png");
                thumbnail.Save(thumbnailPath);
                
                // 随机选择游戏分类
                List<string> selectedGameCategories = new List<string>();
                if (gameCategories.Length > 0)
                {
                    int numCategories = random.Next(1, Math.Min(4, gameCategories.Length + 1));
                    for (int j = 0; j < numCategories; j++)
                    {
                        string gameCategory = gameCategories[random.Next(gameCategories.Length)];
                        if (!selectedGameCategories.Contains(gameCategory))
                        {
                            selectedGameCategories.Add(gameCategory);
                        }
                    }
                }
                
                // 随机选择角色
                List<string> characters = new List<string>();
                List<string> selectedCharacters = new List<string>();
                
                // 创建Mod
                Mod mod = new Mod(
                    "mod-" + i,
                    name,
                    "这是一个示例Mod描述，用于展示Mod浏览器的功能。该Mod提供了高质量的游戏内容，可以增强游戏体验，让您获得全新的游戏乐趣。",
                    "示例作者" + (i % 5 + 1),
                    gameId,
                    thumbnailPath, // 实际路径
                    "http://example.com/mods/download/" + i, // 添加虚拟下载URL
                    category
                )
                {
                    Downloads = random.Next(100, 10000),
                    UpdateTime = DateTime.Now.AddDays(-random.Next(1, 30)),
                    Rating = Math.Round(3 + random.NextDouble() * 2, 1),
                    Version = $"1.{random.Next(10)}",
                    ThumbnailImage = thumbnail,
                    GameCategories = string.Join(",", selectedGameCategories),
                    Characters = string.Join(",", selectedCharacters)
                };
                
                mods.Add(mod);
            }
            
            SaveMods();
        }

        public List<Mod> GetMods(string gameId, string searchQuery = "", string category = "", string sortBy = "")
        {
            // 输出调试信息，帮助定位问题
            Console.WriteLine($"查询Mod - 游戏: {gameId}, 搜索: {searchQuery}, 分类: {category}, 排序: {sortBy}");
            Console.WriteLine($"当前Mod总数: {mods.Count}");
            
            // 先获取指定游戏的所有Mod
            var filteredMods = mods.Where(m => m.GameId == gameId).ToList();
            Console.WriteLine($"该游戏的Mod数量: {filteredMods.Count}");
            
            // 应用搜索查询
            if (!string.IsNullOrEmpty(searchQuery) && searchQuery != "搜索Mod...")
            {
                filteredMods = filteredMods.Where(m => 
                    (m.Name != null && m.Name.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0) || 
                    (m.Description != null && m.Description.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (m.Author != null && m.Author.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                ).ToList();
                Console.WriteLine($"搜索后的Mod数量: {filteredMods.Count}");
            }
            
            // 应用分类过滤
            if (!string.IsNullOrEmpty(category))
            {
                filteredMods = filteredMods.Where(m => m.Category == category).ToList();
                Console.WriteLine($"分类过滤后的Mod数量: {filteredMods.Count}");
            }
            
            // 检查是否所有Mod都有缩略图
            foreach (var mod in filteredMods)
            {
                if (mod.ThumbnailImage == null && !string.IsNullOrEmpty(mod.ThumbnailUrl) && File.Exists(mod.ThumbnailUrl))
                {
                    try
                    {
                        mod.ThumbnailImage = Image.FromFile(mod.ThumbnailUrl);
                        Console.WriteLine($"加载图片: {mod.Id} - {mod.ThumbnailUrl}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"加载图片失败: {mod.Id} - {ex.Message}");
                    }
                }
            }
            
            // 应用排序
            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy)
                {
                    case "按热度排序":
                        filteredMods = filteredMods.OrderByDescending(m => m.Downloads).ToList();
                        break;
                    case "按最新排序":
                        filteredMods = filteredMods.OrderByDescending(m => m.UpdateTime).ToList();
                        break;
                    case "按下载量排序":
                        filteredMods = filteredMods.OrderByDescending(m => m.Downloads).ToList();
                        break;
                    case "按评分排序":
                        filteredMods = filteredMods.OrderByDescending(m => m.Rating).ToList();
                        break;
                }
            }
            else
            {
                // 默认按热度排序
                filteredMods = filteredMods.OrderByDescending(m => m.Downloads).ToList();
            }
            
            // 输出最终结果的调试信息
            Console.WriteLine($"最终返回的Mod数量: {filteredMods.Count}");
            foreach (var mod in filteredMods)
            {
                Console.WriteLine($"Mod信息: ID={mod.Id}, 名称={mod.Name}, 游戏={mod.GameId}, 分类={mod.Category}");
            }
            
            return filteredMods;
        }

        public Mod GetModById(string id)
        {
            return mods.FirstOrDefault(m => m.Id == id);
        }

        public bool DownloadMod(Mod mod)
        {
            try
            {
                // 检查下载链接是否为空
                if (string.IsNullOrEmpty(mod.DownloadUrl))
                {
                    Console.WriteLine($"Mod '{mod.Name}' 没有下载链接");
                    return false;
                }

                Console.WriteLine($"开始下载Mod: {mod.Name}, URL: {mod.DownloadUrl}");
                
                // 使用系统浏览器打开下载链接
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = mod.DownloadUrl,
                    UseShellExecute = true
                });

                // 更新下载计数
                mod.Downloads++;
                SaveMods();
                
                // 更新本地状态
                mod.IsInstalled = true;
                string downloadPath = Path.Combine(downloadFolder, $"{mod.Id}.zip");
                mod.LocalPath = downloadPath;
                
                Console.WriteLine($"Mod '{mod.Name}' 下载请求已发送到浏览器");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"下载Mod失败: {ex.Message}");
                return false;
            }
        }

        public bool InstallMod(Mod mod, string gamePath)
        {
            // 实际应用中，应该将下载的Mod解压并安装到游戏目录
            // 这里简化处理，只是返回成功
            
            if (!mod.IsInstalled || string.IsNullOrEmpty(mod.LocalPath))
            {
                return false;
            }
            
            try
            {
                // 模拟安装过程
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"安装Mod失败: {ex.Message}");
                return false;
            }
        }

        public bool UninstallMod(Mod mod, string gamePath)
        {
            // 实际应用中，应该从游戏目录移除已安装的Mod
            // 这里简化处理，只是返回成功
            
            if (!mod.IsInstalled)
            {
                return false;
            }
            
            try
            {
                // 模拟卸载过程
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"卸载Mod失败: {ex.Message}");
                return false;
            }
        }

        public bool RateMod(Mod mod, double rating)
        {
            // 为Mod评分
            if (rating < 1 || rating > 5)
            {
                return false;
            }
            
            // 这里简化处理，实际应用中应该考虑用户已有评分、平均分计算等
            mod.Rating = rating;
            SaveMods();
            
            return true;
        }
        
        public bool AddCustomMod(Mod mod)
        {
            // 检查ID是否已存在
            if (mods.Any(m => m.Id == mod.Id))
            {
                Console.WriteLine($"添加Mod失败: ID已存在 - {mod.Id}");
                return false;
            }
            
            try
            {
                // 设置图片路径，确保可以在界面上显示
                if (mod.ThumbnailImage != null && string.IsNullOrEmpty(mod.ThumbnailUrl))
                {
                    // 保存预览图片到应用数据目录
                    string appDataPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "GameModBrowser", "ModImages"
                    );
                    
                    if (!Directory.Exists(appDataPath))
                    {
                        Directory.CreateDirectory(appDataPath);
                    }
                    
                    string imagePath = Path.Combine(appDataPath, $"{mod.Id}.png");
                    mod.ThumbnailImage.Save(imagePath);
                    mod.ThumbnailUrl = imagePath;
                    Console.WriteLine($"缩略图已保存到: {imagePath}");
                }
                else if (mod.ThumbnailImage == null && !string.IsNullOrEmpty(mod.ThumbnailUrl) && File.Exists(mod.ThumbnailUrl))
                {
                    // 如果只有路径没有图像，尝试加载图像
                    try
                    {
                        mod.ThumbnailImage = Image.FromFile(mod.ThumbnailUrl);
                        Console.WriteLine($"已从路径加载缩略图: {mod.ThumbnailUrl}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"加载缩略图失败: {ex.Message}");
                    }
                }
                
                // 添加到Mod列表
                mods.Add(mod);
                
                // 确保保存数据
                SaveMods();
                
                // 保存新的游戏分类和角色
                if (!string.IsNullOrEmpty(mod.GameCategories))
                {
                    AddGameCategories(mod.GetGameCategoriesList());
                }
                
                if (!string.IsNullOrEmpty(mod.Characters))
                {
                    AddCharacters(mod.GetCharactersList());
                }
                
                Console.WriteLine($"成功添加Mod: {mod.Name}, ID: {mod.Id}, 分类: {mod.Category}, 游戏ID: {mod.GameId}");
                Console.WriteLine($"缩略图路径: {mod.ThumbnailUrl}, 缩略图状态: {(mod.ThumbnailImage != null ? "已加载" : "未加载")}");
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"添加Mod失败: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        public bool DeleteMod(string modId)
        {
            try
            {
                // 找到要删除的Mod
                Mod modToDelete = mods.FirstOrDefault(m => m.Id == modId);
                
                if (modToDelete == null)
                {
                    Console.WriteLine($"找不到要删除的Mod: {modId}");
                    return false;
                }
                
                Console.WriteLine($"准备删除Mod: {modToDelete.Name} (ID: {modId})");
                
                // 删除Mod缩略图
                if (!string.IsNullOrEmpty(modToDelete.ThumbnailUrl) && File.Exists(modToDelete.ThumbnailUrl))
                {
                    try
                    {
                        // 如果图片正在使用中，可能会抛出异常
                        if (modToDelete.ThumbnailImage != null)
                        {
                            modToDelete.ThumbnailImage.Dispose();
                            modToDelete.ThumbnailImage = null;
                        }
                        
                        File.Delete(modToDelete.ThumbnailUrl);
                        Console.WriteLine($"已删除Mod缩略图: {modToDelete.ThumbnailUrl}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"删除Mod缩略图失败: {ex.Message}");
                        // 继续删除Mod，即使图片删除失败
                    }
                }
                
                // 从列表中移除Mod
                mods.Remove(modToDelete);
                
                // 保存更改
                SaveMods();
                
                Console.WriteLine($"成功删除Mod: {modToDelete.Name}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"删除Mod时出错: {ex.Message}");
                return false;
            }
        }

        public bool DeleteAllSampleMods()
        {
            try
            {
                Console.WriteLine("开始删除所有示例Mod...");
                int deletedCount = 0;
                
                // 找出所有非自定义Mod（不是以"mod-custom-"开头的）
                var samplesToDelete = mods.Where(m => !m.Id.StartsWith("mod-custom-")).ToList();
                
                foreach (var mod in samplesToDelete)
                {
                    // 删除Mod缩略图
                    if (!string.IsNullOrEmpty(mod.ThumbnailUrl) && File.Exists(mod.ThumbnailUrl))
                    {
                        try
                        {
                            // 释放图片资源
                            if (mod.ThumbnailImage != null)
                            {
                                mod.ThumbnailImage.Dispose();
                                mod.ThumbnailImage = null;
                            }
                            
                            File.Delete(mod.ThumbnailUrl);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"删除示例Mod缩略图失败: {ex.Message}");
                            // 继续处理，即使图片删除失败
                        }
                    }
                    
                    // 从列表中移除
                    mods.Remove(mod);
                    deletedCount++;
                }
                
                // 保存更改
                SaveMods();
                
                Console.WriteLine($"成功删除 {deletedCount} 个示例Mod");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"删除示例Mod时出错: {ex.Message}");
                return false;
            }
        }

        // 添加一个公共方法来保存所有Mod
        public void SaveAllMods()
        {
            try
            {
                Console.WriteLine("正在保存所有Mod数据...");
                SaveMods();
                Console.WriteLine("所有Mod数据已保存");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存所有Mod数据失败: {ex.Message}");
            }
        }
    }
} 