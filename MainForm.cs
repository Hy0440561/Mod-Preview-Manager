using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace GameModBrowser
{
    public partial class MainForm : Form, IDisposable
    {
        private ThemeManager themeManager;
        private GameManager gameManager;
        private ModManager modManager;
        private bool isDarkTheme = true;
        private const string AppVersion = "1.0.0"; // 当前应用版本

        // UI元素
        private Panel sidePanel;
        private Panel contentPanel;
        private Panel headerPanel;
        private FlowLayoutPanel modGridPanel;
        private Button themeToggleButton;
        private TextBox searchBox;
        private ComboBox gameSelectComboBox;
        private ComboBox charactersFilterComboBox;
        private string selectedGameId = "wuthering-waves"; // 默认选择鸣潮
        private StatusStrip statusStrip;
        private ToolStripStatusLabel versionStatusLabel;

        public MainForm()
        {
            InitializeComponent();
            SetupManagers();
            
            // 删除所有示例Mod
            modManager.DeleteAllSampleMods();
            
            InitializeUI();
            LoadGames();
            LoadMods();
            
            // 设置窗体标题包含版本信息
            this.Text = $"游戏Mod资源浏览器 v{AppVersion}";
            
            // 事件订阅
            this.Resize += MainForm_Resize;
            this.Load += (s, e) => {
                // 确保窗口加载完成后再次调整布局
                AdjustModCardLayout();
            };
            themeToggleButton.Click += ThemeToggleButton_Click;
            searchBox.TextChanged += SearchBox_TextChanged;
            gameSelectComboBox.SelectedIndexChanged += GameSelectComboBox_SelectedIndexChanged;
            charactersFilterComboBox.SelectedIndexChanged += CharactersFilterComboBox_SelectedIndexChanged;
            
            // 调整卡片布局
            AdjustModCardLayout();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(1280, 720);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "游戏Mod资源浏览器";
            this.ResumeLayout(false);
        }

        private void SetupManagers()
        {
            themeManager = new ThemeManager();
            gameManager = new GameManager();
            modManager = new ModManager();
        }

        private void InitializeUI()
        {
            // 设置窗体基本属性
            this.Text = "游戏Mod资源浏览器";  // 移除标题栏中的版本号
            this.Size = new Size(1280, 720);
            this.MinimumSize = new Size(800, 600);
            
            // 创建头部面板 - 先创建头部面板，后添加菜单
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60
            };
            
            // 创建主容器，使用TableLayoutPanel做精确布局
            TableLayoutPanel mainContainer = new TableLayoutPanel
            {
                RowCount = 1,
                ColumnCount = 2,
                Dock = DockStyle.Fill,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            
            // 设置列宽比例
            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F)); // 侧边栏宽度
            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));  // 内容区域
            
            // 创建侧边栏面板
            sidePanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 10, 10, 10)
            };
            mainContainer.Controls.Add(sidePanel, 0, 0);

            // 创建内容区域容器
            Panel contentContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15, 5, 5, 5) // 增加左侧padding
            };
            mainContainer.Controls.Add(contentContainer, 1, 0);
            
            // 创建Mod网格面板
            modGridPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(10, 10, 10, 10),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };
            
            // 添加事件处理
            modGridPanel.Resize += (s, e) => AdjustModCardLayout();
            contentContainer.Controls.Add(modGridPanel);

            // 替换之前的contentPanel引用
            contentPanel = contentContainer;
            
            // 按照正确的顺序添加控件到窗体
            this.Controls.Add(mainContainer);
            this.Controls.Add(headerPanel);
            
            // 添加菜单栏 - 放在最上层
            AddMenuStrip();
            
            // 添加应用标题
            Label titleLabel = new Label
            {
                Text = "游戏Mod资源浏览器",
                Font = new Font("Microsoft YaHei UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 18)
            };
            headerPanel.Controls.Add(titleLabel);
            
            // 恢复搜索框
            searchBox = new TextBox
            {
                Location = new Point(220, 15),
                Width = 300,
                Height = 30,
                Text = "搜索Mod...",
                BorderStyle = BorderStyle.FixedSingle
            };
            searchBox.GotFocus += (s, e) => {
                if (searchBox.Text == "搜索Mod...")
                    searchBox.Text = "";
            };
            searchBox.LostFocus += (s, e) => {
                if (string.IsNullOrWhiteSpace(searchBox.Text))
                    searchBox.Text = "搜索Mod...";
            };
            headerPanel.Controls.Add(searchBox);

            // 创建上传按钮
            Button uploadButton = new Button
            {
                Location = new Point(530, 15),
                Text = "上传Mod",
                Width = 100,
                Height = 30
            };
            uploadButton.Click += UploadButton_Click;
            themeManager.ApplyButtonStyle(uploadButton, isDarkTheme, true);
            headerPanel.Controls.Add(uploadButton);

            // 创建主题切换按钮
            themeToggleButton = new Button
            {
                Location = new Point(this.Width - 150, 15),
                Text = "切换主题",
                Width = 100,
                Height = 30
            };
            themeManager.ApplyButtonStyle(themeToggleButton, isDarkTheme, true);
            headerPanel.Controls.Add(themeToggleButton);

            // 添加"筛选条件"标题
            Label filterLabel = new Label
            {
                Text = "筛选条件",
                Font = new Font("Microsoft YaHei UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 10),
                Size = new Size(140, 20)
            };
            sidePanel.Controls.Add(filterLabel);

            // 添加游戏选择下拉框
            Label gameSelectLabel = new Label
            {
                Text = "游戏:",
                Location = new Point(10, 40),
                Size = new Size(50, 20),
                ForeColor = Color.White
            };
            sidePanel.Controls.Add(gameSelectLabel);

            gameSelectComboBox = new ComboBox
            {
                Location = new Point(10, 65),
                Width = 140,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            sidePanel.Controls.Add(gameSelectComboBox);

            // 添加角色筛选下拉框
            Label charactersLabel = new Label
            {
                Text = "角色:",
                Location = new Point(10, 95),
                Size = new Size(50, 20),
                ForeColor = Color.White
            };
            sidePanel.Controls.Add(charactersLabel);

            charactersFilterComboBox = new ComboBox
            {
                Location = new Point(10, 120),
                Width = 140,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            // 添加"全部"选项
            charactersFilterComboBox.Items.Add("全部");
            charactersFilterComboBox.SelectedIndex = 0;
            sidePanel.Controls.Add(charactersFilterComboBox);

            // 添加状态栏和版本信息
            AddStatusBar();
            
            // 应用初始主题
            ApplyTheme(isDarkTheme);
        }

        private void LoadGames()
        {
            Console.WriteLine("加载游戏列表...");
            var games = gameManager.GetGames();
            
            // 清空并重新填充游戏选择下拉框
            gameSelectComboBox.Items.Clear();
            foreach (var game in games)
            {
                gameSelectComboBox.Items.Add(game.Name);
            }
            
            // 选择默认游戏
            if (gameSelectComboBox.Items.Count > 0)
            {
                for (int i = 0; i < games.Count; i++)
                {
                    if (games[i].Id == selectedGameId)
                    {
                        gameSelectComboBox.SelectedIndex = i;
                        break;
                    }
                }
                
                // 如果没有找到匹配的游戏，选择第一个
                if (gameSelectComboBox.SelectedIndex == -1)
                {
                    gameSelectComboBox.SelectedIndex = 0;
                    selectedGameId = games[0].Id;
                }
                
                // 加载角色列表
                LoadCharactersByGameId(selectedGameId);
            }
            
            Console.WriteLine($"加载了 {games.Count} 个游戏");
        }
        
        // 根据游戏ID加载角色列表
        private void LoadCharactersByGameId(string gameId)
        {
            // 保存当前选择
            string currentSelection = charactersFilterComboBox.SelectedItem?.ToString();
            
            // 清空角色列表（保留"全部"选项）
            charactersFilterComboBox.Items.Clear();
            charactersFilterComboBox.Items.Add("全部");
            
            // 获取当前游戏的角色列表
            List<string> gameCharacters = gameManager.GetCharactersByGameId(gameId);
            
            // 添加角色到下拉框
            if (gameCharacters.Count > 0)
            {
                charactersFilterComboBox.Items.AddRange(gameCharacters.ToArray());
                Console.WriteLine($"为游戏 {gameId} 加载了 {gameCharacters.Count} 个角色");
            }
            
            // 如果是鸣潮并且有坎特雷拉角色，默认选择坎特雷拉
            if (gameId == "wuthering-waves" && gameCharacters.Contains("坎特雷拉") && 
                (string.IsNullOrEmpty(currentSelection) || currentSelection == "全部"))
            {
                int kanteilaIndex = charactersFilterComboBox.Items.IndexOf("坎特雷拉");
                if (kanteilaIndex >= 0)
                {
                    charactersFilterComboBox.SelectedIndex = kanteilaIndex;
                    return;
                }
            }
            
            // 正常选择逻辑
            int index = -1;
            if (!string.IsNullOrEmpty(currentSelection))
            {
                index = charactersFilterComboBox.Items.IndexOf(currentSelection);
            }
            
            // 如果找不到之前的选择，则选择"全部"
            if (index == -1)
            {
                index = 0; // "全部"的索引
            }
            
            charactersFilterComboBox.SelectedIndex = index;
        }

        private void LoadMods()
        {
            try
            {
                modGridPanel.Controls.Clear();
                Console.WriteLine("开始加载Mod列表...");
                
                // 获取筛选条件
                string searchQuery = searchBox.Text;
                if (searchQuery == "搜索Mod...") searchQuery = "";
                
                string category = ""; // 使用默认分类
                string sortBy = "按热度排序"; // 使用默认排序方式
                
                // 获取游戏和角色筛选
                string selectedCharacter = charactersFilterComboBox.SelectedItem?.ToString();
                if (selectedCharacter == "全部") selectedCharacter = "";
                
                Console.WriteLine($"当前筛选条件 - 游戏ID: {selectedGameId}, 搜索: {searchQuery}, 分类: {category}, 角色: {selectedCharacter}, 排序: {sortBy}");
                
                // 确保游戏ID有效
                if (string.IsNullOrEmpty(selectedGameId))
                {
                    Console.WriteLine("无效的游戏ID");
                    return;
                }
                
                // 先强制保存一次模组数据
                modManager.SaveAllMods();
                
                // 获取Mod列表（增加角色筛选参数）
                var mods = modManager.GetMods(selectedGameId, searchQuery, category, sortBy);
                Console.WriteLine($"获取到{mods.Count}个Mod");
                
                // 应用角色筛选
                if (!string.IsNullOrEmpty(selectedCharacter))
                {
                    Console.WriteLine($"正在按角色筛选: '{selectedCharacter}'");
                    
                    // 打印每个Mod的角色信息以进行调试
                    foreach (var mod in mods)
                    {
                        Console.WriteLine($"Mod '{mod.Name}' 的角色: '{mod.Characters}'");
                    }
                    
                    mods = mods.Where(m => 
                        !string.IsNullOrEmpty(m.Characters) && 
                        m.Characters.Split(',').Select(c => c.Trim()).Contains(selectedCharacter)
                    ).ToList();
                    
                    // 如果筛选后没有任何Mod，并且选择的是坎特雷拉角色，主动创建一个示例Mod
                    if (mods.Count == 0 && selectedCharacter == "坎特雷拉")
                    {
                        Console.WriteLine("没有找到坎特雷拉角色的Mod，创建示例Mod");
                        
                        // 创建一个示例Mod
                        Mod kanteilaExampleMod = CreateExampleMod(selectedGameId, "坎特雷拉");
                        
                        // 添加到ModManager并重新加载
                        if (modManager.AddCustomMod(kanteilaExampleMod))
                        {
                            Console.WriteLine("已创建坎特雷拉示例Mod");
                            
                            // 重新获取所有Mod
                            mods = modManager.GetMods(selectedGameId, searchQuery, category, sortBy);
                            
                            // 再次筛选
                            mods = mods.Where(m => 
                                !string.IsNullOrEmpty(m.Characters) && 
                                m.Characters.Split(',').Select(c => c.Trim()).Contains(selectedCharacter)
                            ).ToList();
                        }
                    }
                    
                    Console.WriteLine($"角色筛选后的Mod数量: {mods.Count}");
                }
                
                if (mods.Count == 0)
                {
                    Label noModsLabel = new Label
                    {
                        Text = "没有找到Mod，尝试上传一个吧！",
                        AutoSize = false,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Size = new Size(modGridPanel.Width - 20, 40),
                        Font = new Font(this.Font.FontFamily, 12),
                        Location = new Point(10, 30)
                    };
                    modGridPanel.Controls.Add(noModsLabel);
                    Console.WriteLine("显示'没有找到Mod'提示");
                    return;
                }
                
                foreach (var mod in mods)
                {
                    try
                    {
                        Console.WriteLine($"创建卡片: {mod.Id} - {mod.Name}");
                        
                        Panel modCard = CreateModCard(mod);
                        modGridPanel.Controls.Add(modCard);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"创建Mod卡片失败: {mod.Id} - {ex.Message}");
                    }
                }
                
                // 调整卡片布局
                AdjustModCardLayout();
                
                Console.WriteLine($"成功显示{modGridPanel.Controls.Count}个Mod卡片");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadMods异常: {ex.Message}\n{ex.StackTrace}");
                
                // 显示错误提示
                MessageBox.Show($"加载Mod列表时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Panel CreateModCard(Mod mod)
        {
            // 使用固定尺寸创建卡片
            int cardWidth = 200;  // 卡片默认宽度（将在布局调整中重新计算）
            
            // 创建图片卡片形式的面板
            Panel card = new Panel
            {
                Width = cardWidth,
                Height = 300,
                Margin = new Padding(10), // 对称边距
                Padding = new Padding(0),
                BorderStyle = BorderStyle.None,
                BackColor = Color.Transparent,
                Tag = mod.Id // 存储ID方便查找
            };
            
            // 应用当前主题的卡片样式
            themeManager.ApplyCardStyle(card, isDarkTheme);

            // 创建图片框，铺满整个卡片
            PictureBox thumbnail = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };
            
            // 设置图片 - 确保有图片可显示
            try
            {
                if (mod.ThumbnailImage != null)
                {
                    thumbnail.Image = mod.ThumbnailImage;
                }
                else if (!string.IsNullOrEmpty(mod.ThumbnailUrl) && File.Exists(mod.ThumbnailUrl))
                {
                    mod.ThumbnailImage = Image.FromFile(mod.ThumbnailUrl);
                    thumbnail.Image = mod.ThumbnailImage;
                }
                else
                {
                    thumbnail.Image = Properties.Resources.DefaultThumbnail;
                    Console.WriteLine($"使用默认缩略图: {mod.Id} - {mod.Name}");
                }
            }
            catch (Exception ex)
            {
                thumbnail.Image = Properties.Resources.DefaultThumbnail;
                Console.WriteLine($"加载Mod缩略图失败，使用默认图: {mod.Id} - {ex.Message}");
            }
            
            // 创建悬停时显示的信息面板
            Panel infoPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(150, 0, 0, 0),
                Visible = false
            };
            
            // 添加Mod名称标签
            Label nameLabel = new Label
            {
                Text = mod.Name,
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold),
                BackColor = Color.Transparent
            };
            infoPanel.Controls.Add(nameLabel);
            
            // 添加作者信息标签
            Label authorLabel = new Label
            {
                Text = $"作者: {mod.Author}",
                ForeColor = Color.FromArgb(220, 220, 220),
                Dock = DockStyle.Bottom,
                Height = 20,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(this.Font.FontFamily, 8),
                BackColor = Color.Transparent
            };
            infoPanel.Controls.Add(authorLabel);
            
            thumbnail.Controls.Add(infoPanel);
            
            // 添加鼠标悬停效果
            thumbnail.MouseEnter += (s, e) => 
            {
                // 卡片边框高亮效果
                card.Invalidate(); // 重绘卡片以显示高亮边框
                
                // 显示信息面板，添加淡入效果
                infoPanel.Visible = true;
                
                // 可以在这里添加更多动画效果
            };
            
            thumbnail.MouseLeave += (s, e) => 
            {
                // 移除卡片边框高亮
                card.Invalidate(); // 重绘卡片以恢复正常边框
                
                // 隐藏信息面板
                infoPanel.Visible = false;
            };

            // 点击显示详情
            thumbnail.Click += (s, e) => ShowModPreview(mod);
            
            card.Controls.Add(thumbnail);
            return card;
        }

        private void ShowModPreview(Mod mod)
        {
            // 创建预览对话框
            Form previewForm = new Form
            {
                Text = $"预览: {mod.Name}",
                Size = new Size(600, 500),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                StartPosition = FormStartPosition.CenterParent
            };

            // 添加缩略图
            PictureBox previewImage = new PictureBox
            {
                Location = new Point(20, 20),
                Size = new Size(300, 200),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle
            };
            
            // 设置图片 - 确保有图片可显示
            try
            {
                if (mod.ThumbnailImage != null)
                {
                    previewImage.Image = mod.ThumbnailImage;
                }
                else if (!string.IsNullOrEmpty(mod.ThumbnailUrl) && File.Exists(mod.ThumbnailUrl))
                {
                    mod.ThumbnailImage = Image.FromFile(mod.ThumbnailUrl);
                    previewImage.Image = mod.ThumbnailImage;
                }
                else
                {
                    previewImage.Image = Properties.Resources.DefaultThumbnail;
                }
            }
            catch (Exception ex)
            {
                previewImage.Image = Properties.Resources.DefaultThumbnail;
                Console.WriteLine($"加载预览图失败: {ex.Message}");
            }
            
            previewForm.Controls.Add(previewImage);

            // 添加详细信息
            Label nameLabel = new Label
            {
                Text = mod.Name,
                Location = new Point(340, 20),
                Size = new Size(240, 30),
                Font = new Font(this.Font.FontFamily, 14, FontStyle.Bold)
            };
            previewForm.Controls.Add(nameLabel);

            Label authorLabel = new Label
            {
                Text = $"作者: {mod.Author}",
                Location = new Point(340, 60),
                Size = new Size(240, 20)
            };
            previewForm.Controls.Add(authorLabel);

            Label versionLabel = new Label
            {
                Text = $"版本: {mod.Version}",
                Location = new Point(340, 90),
                Size = new Size(240, 20)
            };
            previewForm.Controls.Add(versionLabel);

            Label categoryLabel = new Label
            {
                Text = $"Mod类型: {mod.Category}",
                Location = new Point(340, 120),
                Size = new Size(240, 20)
            };
            previewForm.Controls.Add(categoryLabel);

            // 添加游戏分类信息
            if (!string.IsNullOrEmpty(mod.GameCategories))
            {
                Label gameCategoriesLabel = new Label
                {
                    Text = $"游戏分类: {mod.GameCategories}",
                    Location = new Point(340, 150),
                    Size = new Size(240, 20)
                };
                previewForm.Controls.Add(gameCategoriesLabel);
            }

            // 添加角色信息
            if (!string.IsNullOrEmpty(mod.Characters))
            {
                Label charactersLabel = new Label
                {
                    Text = $"相关角色: {mod.Characters}",
                    Location = new Point(340, 180),
                    Size = new Size(240, 20)
                };
                previewForm.Controls.Add(charactersLabel);
            }

            // 添加描述
            Label descriptionTitleLabel = new Label
            {
                Text = "描述:",
                Location = new Point(20, 240),
                Size = new Size(100, 20),
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold)
            };
            previewForm.Controls.Add(descriptionTitleLabel);

            TextBox descriptionTextBox = new TextBox
            {
                Text = mod.Description,
                Location = new Point(20, 270),
                Size = new Size(560, 120),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical
            };
            previewForm.Controls.Add(descriptionTextBox);

            // 添加下载按钮
            Button downloadButton = new Button
            {
                Text = "下载Mod",
                Location = new Point(150, 420),
                Size = new Size(140, 30)
            };
            downloadButton.Click += (sender, e) => 
            {
                downloadButton.Enabled = false;
                downloadButton.Text = "正在跳转...";
                
                modManager.DownloadMod(mod);
                
                downloadButton.Enabled = true;
                downloadButton.Text = "下载Mod";
            };
            previewForm.Controls.Add(downloadButton);

            // 添加删除按钮
            Button deleteButton = new Button
            {
                Text = "删除Mod",
                Location = new Point(310, 420),
                Size = new Size(140, 30),
                ForeColor = Color.White,
                BackColor = Color.Firebrick
            };
            deleteButton.FlatStyle = FlatStyle.Flat;
            
            deleteButton.Click += (sender, e) => 
            {
                // 确认删除
                DialogResult result = MessageBox.Show(
                    $"确定要删除 {mod.Name} 吗？此操作不可撤销。", 
                    "删除确认", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Warning);
                    
                if (result == DialogResult.Yes)
                {
                    deleteButton.Enabled = false;
                    deleteButton.Text = "正在删除...";
                    
                    // 执行删除
                    bool success = modManager.DeleteMod(mod.Id);
                    
                    if (success)
                    {
                        MessageBox.Show("Mod已成功删除", "删除成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        previewForm.Close();
                        
                        // 刷新Mod列表
                        LoadMods();
                    }
                    else
                    {
                        MessageBox.Show("删除Mod失败", "删除失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        deleteButton.Enabled = true;
                        deleteButton.Text = "删除Mod";
                    }
                }
            };
            
            // 只有用户上传的自定义Mod才显示删除按钮
            if (mod.Id.StartsWith("mod-custom-"))
            {
                previewForm.Controls.Add(deleteButton);
            }

            previewForm.ShowDialog();
        }

        private void ApplyTheme(bool isDark)
        {
            ColorScheme scheme = themeManager.GetColorScheme(isDark);
            
            // 应用颜色到窗体
            this.BackColor = scheme.BackgroundColor;
            this.ForeColor = scheme.TextColor;
            
            // 应用颜色到面板
            headerPanel.BackColor = scheme.PrimaryColor;
            sidePanel.BackColor = scheme.SecondaryColor;
            
            // 为内容面板创建渐变背景
            themeManager.CreateGradientBackground(contentPanel, isDark);
            
            // 按钮样式
            themeManager.ApplyButtonStyle(themeToggleButton, isDark, true);
            
            // 设置主题切换按钮文本
            themeToggleButton.Text = isDark ? "粉色主题" : "夜店主题";
            
            // 更新所有Mod卡片样式
            foreach (Control control in modGridPanel.Controls)
            {
                if (control is Panel card)
                {
                    themeManager.ApplyCardStyle(card, isDark);
                }
            }
            
            // 应用输入控件样式
            themeManager.ApplyTextBoxStyle(searchBox, isDark);
            themeManager.ApplyComboBoxStyle(gameSelectComboBox, isDark);
            themeManager.ApplyComboBoxStyle(charactersFilterComboBox, isDark);
            
            // 更新菜单栏样式
            if (this.MainMenuStrip != null)
            {
                MainMenuStrip.BackColor = isDark ? Color.FromArgb(35, 35, 45) : Color.FromArgb(255, 230, 240);
                MainMenuStrip.ForeColor = isDark ? Color.White : Color.FromArgb(60, 60, 60);
                MainMenuStrip.Renderer = new ToolStripProfessionalRenderer(new CustomColorTable(isDark));
            }
            
            // 更新状态栏样式
            if (statusStrip != null)
            {
                statusStrip.BackColor = isDark ? Color.FromArgb(30, 30, 40) : Color.FromArgb(245, 220, 230);
                versionStatusLabel.ForeColor = isDark ? Color.LightGray : Color.DimGray;
            }
        }

        // 调整Mod卡片布局，确保每行显示4张
        private void AdjustModCardLayout()
        {
            try
            {
                // 如果没有卡片，无需调整
                if (modGridPanel.Controls.Count == 0)
                    return;
                
                // 获取可用空间
                int availableWidth = modGridPanel.ClientSize.Width - modGridPanel.Padding.Left - modGridPanel.Padding.Right;
                Console.WriteLine($"卡片布局: 面板宽度 = {modGridPanel.Width}, 客户区宽度 = {modGridPanel.ClientSize.Width}, 可用宽度 = {availableWidth}");
                
                // 确保每行4张卡片
                int cardsPerRow = 4;
                int cardSpace = availableWidth / cardsPerRow;
                
                // 卡片边距
                int cardMargin = 10;
                
                // 计算卡片实际宽度
                int cardWidth = cardSpace - (cardMargin * 2);
                cardWidth = Math.Max(180, cardWidth); // 确保最小宽度
                
                Console.WriteLine($"卡片计算: 每卡片空间 = {cardSpace}, 实际宽度 = {cardWidth}");
                
                // 调整所有卡片
                foreach (Control control in modGridPanel.Controls)
                {
                    if (control is Panel card)
                    {
                        // 设置卡片尺寸
                        card.SuspendLayout();
                        card.Width = cardWidth;
                        card.Margin = new Padding(cardMargin);
                        card.ResumeLayout();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"调整布局错误: {ex.Message}");
            }
        }

        #region 事件处理
        private void MainForm_Resize(object sender, EventArgs e)
        {
            themeToggleButton.Location = new Point(this.Width - 150, 15);
            AdjustModCardLayout(); // 调整卡片布局
        }

        #pragma warning disable CS8622 // 禁用参数可空性不匹配的警告
        private void ThemeToggleButton_Click(object sender, EventArgs e)
        {
            isDarkTheme = !isDarkTheme;
            ApplyTheme(isDarkTheme);
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            if (searchBox.Text != "搜索Mod...")
                LoadMods();
        }

        // 游戏选择改变事件
        private void GameSelectComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (gameSelectComboBox.SelectedIndex != -1)
            {
                string selectedGameName = gameSelectComboBox.SelectedItem.ToString();
                
                // 查找游戏ID
                foreach (var game in gameManager.GetGames())
                {
                    if (game.Name == selectedGameName)
                    {
                        selectedGameId = game.Id;
                        
                        // 更新角色列表
                        LoadCharactersByGameId(selectedGameId);
                        
                        // 重新加载Mod
                        LoadMods();
                        break;
                    }
                }
            }
        }
        
        // 角色筛选改变事件
        private void CharactersFilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadMods();
        }
        #pragma warning restore CS8622 // 恢复警告

        private void UploadButton_Click(object sender, EventArgs e)
        {
            // Hard code the game selection since we removed gameSelector
            string gameName = "鸣潮";
            string gameId = "";
            
            try
            {
                // 获取游戏ID
                foreach (var game in gameManager.GetGames())
                {
                    if (game.Name == gameName)
                    {
                        gameId = game.Id;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(gameId))
                {
                    MessageBox.Show("找不到所选游戏", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Console.WriteLine($"打开Mod上传表单 - 游戏ID: {gameId}, 游戏名称: {gameName}");

                // 打开上传表单 - 传入ModManager实例
                using (ModUploadForm uploadForm = new ModUploadForm(gameId, gameName, modManager))
                {
                    DialogResult result = uploadForm.ShowDialog();
                    Console.WriteLine($"上传表单返回结果: {result}");
                    
                    if (result == DialogResult.OK && uploadForm.CreatedMod != null)
                    {
                        Mod createdMod = uploadForm.CreatedMod;
                        Console.WriteLine($"表单返回Mod: {createdMod.Name}, ID: {createdMod.Id}, 游戏ID: {createdMod.GameId}");
                        Console.WriteLine($"缩略图路径: {createdMod.ThumbnailUrl}, 图片状态: {(createdMod.ThumbnailImage != null ? "已加载" : "未加载")}");
                        
                        // 添加新Mod
                        if (modManager.AddCustomMod(createdMod))
                        {
                            MessageBox.Show($"Mod上传成功！\n保存路径: {createdMod.ThumbnailUrl}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            
                            // 显示上传的Mod在哪个分类下
                            string modCategory = createdMod.Category;
                            Console.WriteLine($"Mod已添加到分类: {modCategory}");
                            
                            // 确保保存数据
                            modManager.SaveAllMods();
                            
                            // 不再需要分类筛选相关代码，直接加载Mods
                            LoadMods();
                        }
                        else
                        {
                            MessageBox.Show("Mod上传失败，可能已存在相同ID", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else if (result == DialogResult.OK && uploadForm.CreatedMod == null)
                    {
                        MessageBox.Show("Mod创建过程出错，未返回有效的Mod数据", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"上传Mod时发生错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"上传Mod异常: {ex.Message}\n{ex.StackTrace}");
            }
        }
        #endregion

        // 添加一个查看版本信息的关于对话框
        private void ShowAboutDialog()
        {
            MessageBox.Show(
                $"游戏Mod资源浏览器 v{AppVersion}\n\n" +
                "一个用于浏览和下载游戏Mod资源的工具\n\n" +
                "© 2023 版权所有\n\n" +
                "最新版本请访问官方网站获取更新",
                "关于",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // 添加菜单栏
        private void AddMenuStrip()
        {
            // 创建菜单栏
            MenuStrip mainMenu = new MenuStrip();
            mainMenu.Dock = DockStyle.Top;
            mainMenu.BackColor = Color.FromArgb(35, 35, 45); // 暗色背景
            mainMenu.ForeColor = Color.White;
            mainMenu.Padding = new Padding(5, 2, 0, 2);
            mainMenu.Renderer = new ToolStripProfessionalRenderer(new CustomColorTable(isDarkTheme));
            
            // 文件菜单
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("文件");
            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("退出", null, (s, e) => { this.Close(); });
            fileMenu.DropDownItems.Add(exitMenuItem);
            
            // 帮助菜单
            ToolStripMenuItem helpMenu = new ToolStripMenuItem("帮助");
            ToolStripMenuItem aboutMenuItem = new ToolStripMenuItem("关于", null, (s, e) => { ShowAboutDialog(); });
            ToolStripMenuItem checkUpdateMenuItem = new ToolStripMenuItem("检查更新", null, (s, e) => { ShowCheckUpdateDialog(); });
            helpMenu.DropDownItems.Add(aboutMenuItem);
            helpMenu.DropDownItems.Add(checkUpdateMenuItem);
            
            // 添加菜单项到菜单栏
            mainMenu.Items.Add(fileMenu);
            mainMenu.Items.Add(helpMenu);
            
            // 添加到窗体
            this.Controls.Add(mainMenu);
            this.MainMenuStrip = mainMenu;
        }
        
        // 自定义菜单颜色
        private class CustomColorTable : ProfessionalColorTable
        {
            private bool _isDarkTheme;
            
            public CustomColorTable(bool isDarkTheme)
            {
                _isDarkTheme = isDarkTheme;
            }
            
            public override Color MenuItemSelected => _isDarkTheme ? Color.FromArgb(50, 50, 65) : base.MenuItemSelected;
            public override Color MenuItemBorder => _isDarkTheme ? Color.FromArgb(60, 60, 75) : base.MenuItemBorder;
            public override Color MenuBorder => _isDarkTheme ? Color.FromArgb(40, 40, 55) : base.MenuBorder;
            public override Color MenuItemPressedGradientBegin => _isDarkTheme ? Color.FromArgb(45, 45, 60) : base.MenuItemPressedGradientBegin;
            public override Color MenuItemPressedGradientEnd => _isDarkTheme ? Color.FromArgb(55, 55, 70) : base.MenuItemPressedGradientEnd;
            public override Color MenuItemSelectedGradientBegin => _isDarkTheme ? Color.FromArgb(45, 45, 60) : base.MenuItemSelectedGradientBegin;
            public override Color MenuItemSelectedGradientEnd => _isDarkTheme ? Color.FromArgb(55, 55, 70) : base.MenuItemSelectedGradientEnd;
        }
        
        // 显示检查更新对话框
        private void ShowCheckUpdateDialog()
        {
            DialogResult result = MessageBox.Show(
                $"当前版本: v{AppVersion}\n\n请访问以下地址获取最新版本:\nhttps://your-website.com/downloads\n\n是否现在访问?",
                "检查更新",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);
                
            if (result == DialogResult.Yes)
            {
                try
                {
                    System.Diagnostics.Process.Start("https://your-website.com/downloads");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"无法打开浏览器: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 添加状态栏
        private void AddStatusBar()
        {
            statusStrip = new StatusStrip
            {
                SizingGrip = false,
                BackColor = Color.FromArgb(35, 35, 45),
                ForeColor = Color.LightGray,
                Dock = DockStyle.Bottom
            };
            
            // 创建版本标签
            versionStatusLabel = new ToolStripStatusLabel($"版本 v{AppVersion}")
            {
                BorderSides = ToolStripStatusLabelBorderSides.Right,
                BorderStyle = Border3DStyle.Etched
            };
            
            // 添加版本标签到状态栏
            statusStrip.Items.Add(versionStatusLabel);
            
            // 添加状态栏到窗体
            this.Controls.Add(statusStrip);
        }

        // 创建示例Mod辅助方法
        private Mod CreateExampleMod(string gameId, string character)
        {
            try
            {
                Console.WriteLine($"创建示例Mod，角色: {character}，游戏ID: {gameId}");
                
                // 生成一个唯一ID
                string modId = $"mod-custom-{DateTime.Now.Ticks}";
                
                // 创建示例缩略图
                Bitmap thumbnail = new Bitmap(220, 300);
                using (Graphics g = Graphics.FromImage(thumbnail))
                {
                    // 设置背景色
                    Color backgroundColor = Color.FromArgb(80, 80, 180); // 蓝色背景
                    g.Clear(backgroundColor);
                    
                    // 添加Mod名称
                    using (Font titleFont = new Font("Microsoft YaHei UI", 16, FontStyle.Bold))
                    using (Brush titleBrush = new SolidBrush(Color.White))
                    using (StringFormat format = new StringFormat { Alignment = StringAlignment.Center })
                    {
                        g.DrawString($"{character}示例Mod", titleFont, titleBrush, 
                            new RectangleF(10, 80, 200, 50), format);
                    }
                    
                    // 添加角色名
                    using (Font characterFont = new Font("Microsoft YaHei UI", 14))
                    using (Brush characterBrush = new SolidBrush(Color.FromArgb(255, 255, 200)))
                    using (StringFormat format = new StringFormat { Alignment = StringAlignment.Center })
                    {
                        g.DrawString(character, characterFont, characterBrush, 
                            new RectangleF(10, 140, 200, 40), format);
                    }
                    
                    // 添加介绍文本
                    using (Font descFont = new Font("Microsoft YaHei UI", 10))
                    using (Brush descBrush = new SolidBrush(Color.FromArgb(230, 230, 230)))
                    using (StringFormat format = new StringFormat { Alignment = StringAlignment.Center })
                    {
                        g.DrawString("这是一个演示Mod\n用于展示角色筛选功能", descFont, descBrush, 
                            new RectangleF(10, 190, 200, 60), format);
                    }
                }
                
                // 保存示例缩略图
                string appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "GameModBrowser", "ModImages"
                );
                
                if (!Directory.Exists(appDataPath))
                {
                    Directory.CreateDirectory(appDataPath);
                }
                
                string thumbnailPath = Path.Combine(appDataPath, $"{modId}.png");
                thumbnail.Save(thumbnailPath);
                
                // 创建Mod对象
                Mod exampleMod = new Mod(
                    modId,
                    $"{character}角色示例Mod",
                    $"这是一个为{character}角色创建的示例Mod，用于展示角色筛选功能。",
                    "系统自动生成",
                    gameId,
                    thumbnailPath,
                    "https://example.com/download/sample-mod",
                    "正常"
                )
                {
                    ThumbnailImage = thumbnail,
                    UpdateTime = DateTime.Now,
                    Downloads = 100,
                    Rating = 5.0,
                    Version = "1.0",
                    GameCategories = "角色展示,演示",
                    Characters = character
                };
                
                Console.WriteLine($"示例Mod已创建: {exampleMod.Name}, ID: {exampleMod.Id}");
                return exampleMod;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"创建示例Mod失败: {ex.Message}");
                return null;
            }
        }

        // 覆盖Dispose方法以释放额外资源
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 释放托管资源
                // 这里可以添加自定义资源释放代码
            }
            base.Dispose(disposing);
        }
    }
} 