using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GameModBrowser
{
    public partial class ModUploadForm : Form
    {
        private string selectedGameId;
        private string previewImagePath;
        private List<string> characters = new List<string>();
        private List<string> gameCategories = new List<string>();
        private ModManager modManager;
        private GameManager gameManager;
        
        // 静态列表存储最近使用的下载链接
        private static List<string> recentDownloadLinks = new List<string>();
        // 最大保存的链接数量
        private const int MaxRecentLinks = 4;
        
        // 链接历史菜单
        private ContextMenuStrip linkHistoryMenu;

        public Mod CreatedMod { get; private set; }

        public ModUploadForm(string gameId, string gameName, ModManager modManager)
        {
            this.modManager = modManager;
            this.gameManager = new GameManager();
            this.selectedGameId = gameId;
            
            InitializeComponent();
            this.Text = $"上传Mod - {gameName}";
            
            // 确保组件已经创建
            if (Controls["gameCategoryComboBox"] == null)
            {
                Console.WriteLine("警告: gameCategoryComboBox未找到");
            }
            
            // 加载分类和角色数据
            LoadCategories();
            
            // 初始化链接历史菜单
            InitializeLinkHistoryMenu();
            
            // 添加游戏分类选择事件
            ComboBox gameCategoryComboBox = (ComboBox)Controls["gameCategoryComboBox"];
            if (gameCategoryComboBox != null)
            {
                gameCategoryComboBox.SelectedIndexChanged += GameCategoryComboBox_SelectedIndexChanged;
                gameCategoryComboBox.DoubleClick += GameCategoryComboBox_DoubleClick; // 添加双击事件
            }
            
            // 输出调试信息
            Console.WriteLine($"ModUploadForm已初始化 - 游戏ID: {gameId}, 游戏名称: {gameName}");
            Console.WriteLine($"表单控件数量: {this.Controls.Count}");
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // 基本设置
            this.ClientSize = new Size(600, 750); // 增加高度以容纳新组件
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "上传Mod";

            // 标题
            Label titleLabel = new Label
            {
                Text = "上传新Mod",
                Font = new Font(Font.FontFamily, 16, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(200, 30)
            };
            this.Controls.Add(titleLabel);

            // Mod名称
            Label nameLabel = new Label
            {
                Text = "Mod名称:",
                Location = new Point(20, 70),
                Size = new Size(100, 20)
            };
            this.Controls.Add(nameLabel);

            TextBox nameTextBox = new TextBox
            {
                Location = new Point(150, 70),
                Size = new Size(400, 20),
                Name = "nameTextBox",
                Text = "" // 确保文本框初始为空
            };
            this.Controls.Add(nameTextBox);

            // 游戏分类（新添加）
            Label gameCategoryLabel = new Label
            {
                Text = "游戏分类:",
                Location = new Point(20, 110),
                Size = new Size(100, 20)
            };
            this.Controls.Add(gameCategoryLabel);

            ComboBox gameCategoryComboBox = new ComboBox
            {
                Location = new Point(150, 110),
                Size = new Size(200, 20),
                Name = "gameCategoryComboBox"
            };
            this.Controls.Add(gameCategoryComboBox);

            Button addGameCategoryButton = new Button
            {
                Text = "添加分类",
                Location = new Point(360, 110),
                Size = new Size(100, 25),
                Name = "addGameCategoryButton"
            };
            addGameCategoryButton.Click += AddGameCategoryButton_Click;
            this.Controls.Add(addGameCategoryButton);

            ListBox gameCategoryListBox = new ListBox
            {
                Location = new Point(150, 140),
                Size = new Size(200, 60),
                Name = "gameCategoryListBox"
            };
            this.Controls.Add(gameCategoryListBox);

            Button removeGameCategoryButton = new Button
            {
                Text = "移除所选",
                Location = new Point(360, 155),
                Size = new Size(100, 25),
                Name = "removeGameCategoryButton"
            };
            removeGameCategoryButton.Click += RemoveGameCategoryButton_Click;
            this.Controls.Add(removeGameCategoryButton);

            // Mod分类
            Label categoryLabel = new Label
            {
                Text = "Mod分类:",
                Location = new Point(20, 210),
                Size = new Size(100, 20)
            };
            this.Controls.Add(categoryLabel);

            ComboBox categoryComboBox = new ComboBox
            {
                Location = new Point(150, 210),
                Size = new Size(200, 20),
                DropDownStyle = ComboBoxStyle.DropDown,
                Name = "categoryComboBox"
            };
            this.Controls.Add(categoryComboBox);

            // 角色
            Label characterLabel = new Label
            {
                Text = "角色:",
                Location = new Point(20, 250),
                Size = new Size(100, 20)
            };
            this.Controls.Add(characterLabel);

            ComboBox characterComboBox = new ComboBox
            {
                Location = new Point(150, 250),
                Size = new Size(200, 20),
                Name = "characterComboBox"
            };
            this.Controls.Add(characterComboBox);

            Button addCharacterButton = new Button
            {
                Text = "添加角色",
                Location = new Point(360, 250),
                Size = new Size(100, 25),
                Name = "addCharacterButton"
            };
            addCharacterButton.Click += AddCharacterButton_Click;
            this.Controls.Add(addCharacterButton);

            ListBox characterListBox = new ListBox
            {
                Location = new Point(150, 280),
                Size = new Size(200, 80),
                Name = "characterListBox"
            };
            this.Controls.Add(characterListBox);

            Button removeCharacterButton = new Button
            {
                Text = "移除所选",
                Location = new Point(360, 310),
                Size = new Size(100, 25),
                Name = "removeCharacterButton"
            };
            removeCharacterButton.Click += RemoveCharacterButton_Click;
            this.Controls.Add(removeCharacterButton);

            // 预览图片
            Label previewLabel = new Label
            {
                Text = "预览图片:",
                Location = new Point(20, 380),
                Size = new Size(100, 20)
            };
            this.Controls.Add(previewLabel);

            PictureBox previewPictureBox = new PictureBox
            {
                Location = new Point(150, 380),
                Size = new Size(200, 150),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                Name = "previewPictureBox"
            };
            this.Controls.Add(previewPictureBox);

            Button selectImageButton = new Button
            {
                Text = "选择图片",
                Location = new Point(360, 450),
                Size = new Size(100, 25),
                Name = "selectImageButton"
            };
            selectImageButton.Click += SelectImageButton_Click;
            this.Controls.Add(selectImageButton);

            // 下载链接
            Label downloadLinkLabel = new Label
            {
                Text = "下载链接:",
                Location = new Point(20, 550),
                Size = new Size(100, 20)
            };
            this.Controls.Add(downloadLinkLabel);

            TextBox downloadLinkTextBox = new TextBox
            {
                Location = new Point(150, 550),
                Size = new Size(400, 20),
                Name = "downloadLinkTextBox"
            };
            this.Controls.Add(downloadLinkTextBox);

            // 作者信息
            Label authorLabel = new Label
            {
                Text = "作者:",
                Location = new Point(20, 590),
                Size = new Size(100, 20)
            };
            this.Controls.Add(authorLabel);

            TextBox authorTextBox = new TextBox
            {
                Location = new Point(150, 590),
                Size = new Size(200, 20),
                Name = "authorTextBox"
            };
            this.Controls.Add(authorTextBox);

            // Mod描述
            Label descriptionLabel = new Label
            {
                Text = "Mod描述:",
                Location = new Point(20, 630),
                Size = new Size(100, 20)
            };
            this.Controls.Add(descriptionLabel);

            TextBox descriptionTextBox = new TextBox
            {
                Location = new Point(150, 630),
                Size = new Size(400, 60),
                Multiline = true,
                Name = "descriptionTextBox"
            };
            this.Controls.Add(descriptionTextBox);

            // 提交按钮
            Button submitButton = new Button
            {
                Text = "提交",
                Location = new Point(200, 710),
                Size = new Size(80, 30),
                Name = "submitButton"
            };
            submitButton.Click += SubmitButton_Click;
            this.Controls.Add(submitButton);

            // 取消按钮
            Button cancelButton = new Button
            {
                Text = "取消",
                Location = new Point(300, 710),
                Size = new Size(80, 30),
                DialogResult = DialogResult.Cancel,
                Name = "cancelButton"
            };
            this.Controls.Add(cancelButton);

            this.CancelButton = cancelButton;
            this.ResumeLayout(false);
        }

        private void LoadCategories()
        {
            // 加载Mod分类
            ComboBox categoryComboBox = (ComboBox)Controls["categoryComboBox"];
            categoryComboBox.Items.Clear();
            
            // 添加预设Mod分类选项
            categoryComboBox.Items.Add("正常");
            categoryComboBox.Items.Add("R18");
            categoryComboBox.Items.Add("猎奇");
            
            // 默认选择"正常"分类
            categoryComboBox.SelectedIndex = 0;
            
            // 允许用户输入自定义分类
            categoryComboBox.DropDownStyle = ComboBoxStyle.DropDown;

            // 加载游戏分类 - 从GameManager获取游戏列表
            ComboBox gameCategoryComboBox = (ComboBox)Controls["gameCategoryComboBox"];
            gameCategoryComboBox.Items.Clear(); // 清空已有项
            
            // 添加所有可用游戏
            foreach (var game in gameManager.GetGames())
            {
                gameCategoryComboBox.Items.Add(game.Name);
            }
            
            // 如果下拉框为空，则添加默认提示文本
            if (gameCategoryComboBox.Items.Count == 0)
            {
                gameCategoryComboBox.Text = "输入游戏分类";
            }
            else
            {
                // 设置选中游戏
                for (int i = 0; i < gameCategoryComboBox.Items.Count; i++)
                {
                    var game = gameManager.GetGames()[i];
                    if (game.Id == selectedGameId)
                    {
                        gameCategoryComboBox.SelectedIndex = i;
                        break;
                    }
                }
                
                // 如果没有匹配的游戏，默认选择第一个
                if (gameCategoryComboBox.SelectedIndex == -1 && gameCategoryComboBox.Items.Count > 0)
                {
                    gameCategoryComboBox.SelectedIndex = 0;
                }
            }
            
            gameCategoryComboBox.DropDownStyle = ComboBoxStyle.DropDownList; // 修改为只能从列表选择
            
            // 添加提示工具提示而不是标签
            ToolTip gameCategoryTooltip = new ToolTip();
            gameCategoryTooltip.SetToolTip(gameCategoryComboBox, "选择后请点击\"添加分类\"按钮或双击快速添加");
            gameCategoryTooltip.SetToolTip(Controls["addGameCategoryButton"], "点击添加所选分类或直接双击下拉框选项");

            // 添加右侧提示标签（替换原有的下方提示）
            Label tipLabel = new Label
            {
                Text = "(双击即可快速添加)",
                Location = new Point(360, 90),
                Size = new Size(150, 20),
                ForeColor = Color.Gray,
                Font = new Font(this.Font.FontFamily, 8),
                Name = "gameSelectTipLabel"
            };

            // 删除旧标签（如果存在）
            if (Controls["gameSelectTipLabel"] != null)
            {
                Controls.Remove(Controls["gameSelectTipLabel"]);
            }

            // 添加新的提示标签
            this.Controls.Add(tipLabel);

            // 加载当前游戏对应的角色列表
            UpdateCharactersList();
        }

        private void AddGameCategoryButton_Click(object sender, EventArgs e)
        {
            ComboBox gameCategoryComboBox = (ComboBox)Controls["gameCategoryComboBox"];
            ListBox gameCategoryListBox = (ListBox)Controls["gameCategoryListBox"];
            
            string category = gameCategoryComboBox.Text.Trim();
            
            if (!string.IsNullOrEmpty(category) && !gameCategories.Contains(category))
            {
                gameCategories.Add(category);
                gameCategoryListBox.Items.Add(category);
                
                // 如果是新分类，添加到下拉列表中，同时添加到全局分类中
                if (!gameCategoryComboBox.Items.Contains(category))
                {
                    gameCategoryComboBox.Items.Add(category);
                    // 添加到全局游戏分类
                    modManager.AddGameCategory(category);
                }
                
                gameCategoryComboBox.Text = "";
            }
        }

        private void RemoveGameCategoryButton_Click(object sender, EventArgs e)
        {
            ListBox gameCategoryListBox = (ListBox)Controls["gameCategoryListBox"];
            
            if (gameCategoryListBox.SelectedIndex != -1)
            {
                string category = gameCategoryListBox.SelectedItem.ToString();
                gameCategories.Remove(category);
                gameCategoryListBox.Items.Remove(category);
            }
        }

        private void SelectImageButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "图片文件|*.jpg;*.jpeg;*.png;*.gif;*.bmp|所有文件|*.*";
                openFileDialog.Title = "选择预览图片";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    previewImagePath = openFileDialog.FileName;
                    PictureBox previewPictureBox = (PictureBox)Controls["previewPictureBox"];
                    
                    try
                    {
                        previewPictureBox.Image = Image.FromFile(previewImagePath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"加载图片时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void AddCharacterButton_Click(object sender, EventArgs e)
        {
            ComboBox characterComboBox = (ComboBox)Controls["characterComboBox"];
            ListBox characterListBox = (ListBox)Controls["characterListBox"];
            
            string character = characterComboBox.Text.Trim();
            
            if (!string.IsNullOrEmpty(character) && !characters.Contains(character))
            {
                characters.Add(character);
                characterListBox.Items.Add(character);
                
                // 如果是新角色，添加到下拉列表中，同时添加到全局角色中
                if (!characterComboBox.Items.Contains(character))
                {
                    characterComboBox.Items.Add(character);
                    // 添加到全局角色
                    modManager.AddCharacter(character);
                }
                
                characterComboBox.Text = "";
            }
        }

        private void RemoveCharacterButton_Click(object sender, EventArgs e)
        {
            ListBox characterListBox = (ListBox)Controls["characterListBox"];
            
            if (characterListBox.SelectedIndex != -1)
            {
                string character = characterListBox.SelectedItem.ToString();
                characters.Remove(character);
                characterListBox.Items.Remove(character);
            }
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            // 获取所有字段
            TextBox nameTextBox = (TextBox)Controls["nameTextBox"];
            ComboBox categoryComboBox = (ComboBox)Controls["categoryComboBox"];
            ComboBox gameCategoryComboBox = (ComboBox)Controls["gameCategoryComboBox"];
            ListBox gameCategoryListBox = (ListBox)Controls["gameCategoryListBox"];
            TextBox downloadLinkTextBox = (TextBox)Controls["downloadLinkTextBox"];
            TextBox authorTextBox = (TextBox)Controls["authorTextBox"];
            TextBox descriptionTextBox = (TextBox)Controls["descriptionTextBox"];
            PictureBox previewPictureBox = (PictureBox)Controls["previewPictureBox"];

            // 验证必填字段
            if (string.IsNullOrEmpty(nameTextBox.Text))
            {
                MessageBox.Show("请输入Mod名称", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(downloadLinkTextBox.Text))
            {
                MessageBox.Show("请输入下载链接", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(authorTextBox.Text))
            {
                MessageBox.Show("请输入作者信息", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (previewPictureBox.Image == null)
            {
                MessageBox.Show("请选择预览图片", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 如果游戏分类列表为空，但已选择了游戏分类，则自动添加
            if (gameCategoryListBox.Items.Count == 0 && gameCategoryComboBox.SelectedIndex != -1)
            {
                string selectedCategory = gameCategoryComboBox.SelectedItem.ToString();
                if (!string.IsNullOrEmpty(selectedCategory) && !gameCategories.Contains(selectedCategory))
                {
                    gameCategories.Add(selectedCategory);
                    gameCategoryListBox.Items.Add(selectedCategory);
                }
            }

            // 再次检查游戏分类列表是否为空
            if (gameCategoryListBox.Items.Count == 0)
            {
                MessageBox.Show("请至少添加一个游戏分类", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 保存下载链接到历史记录
                AddLinkToHistory(downloadLinkTextBox.Text);
                
                // 生成新的Mod ID
                string modId = $"mod-custom-{DateTime.Now.Ticks}";
                
                Console.WriteLine($"开始创建Mod, ID: {modId}, 游戏ID: {selectedGameId}");

                // 保存预览图片到应用数据目录
                string appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "GameModBrowser", "ModImages"
                );
                
                if (!Directory.Exists(appDataPath))
                {
                    Directory.CreateDirectory(appDataPath);
                }
                
                string imagePath = Path.Combine(appDataPath, $"{modId}.png");
                previewPictureBox.Image.Save(imagePath);
                Console.WriteLine($"图片已保存到: {imagePath}");

                // 准备游戏分类和角色数据
                string gameCategories = string.Join(",", this.gameCategories);
                string charactersList = string.Join(",", this.characters);
                
                Console.WriteLine($"游戏分类: {gameCategories}");
                Console.WriteLine($"角色: {charactersList}");

                // 创建Mod对象
                Mod newMod = new Mod(
                    modId,
                    nameTextBox.Text.Trim(),
                    descriptionTextBox.Text.Trim(),
                    authorTextBox.Text.Trim(),
                    selectedGameId, // 确保使用正确的游戏ID
                    imagePath,
                    downloadLinkTextBox.Text.Trim(),
                    categoryComboBox.SelectedItem?.ToString() ?? "正常"
                )
                {
                    ThumbnailImage = new Bitmap(previewPictureBox.Image),
                    UpdateTime = DateTime.Now,
                    GameCategories = gameCategories,
                    Characters = charactersList,
                    Downloads = 0,
                    Rating = 5.0,
                    Version = "1.0"
                };
                
                // 确保设置了CreatedMod属性
                this.CreatedMod = newMod;
                
                Console.WriteLine($"Mod创建成功: {CreatedMod.Name}, ID: {CreatedMod.Id}, 游戏ID: {CreatedMod.GameId}, 分类: {CreatedMod.Category}");
                Console.WriteLine($"缩略图路径: {CreatedMod.ThumbnailUrl}, 图片状态: {(CreatedMod.ThumbnailImage != null ? "已加载" : "未加载")}");

                // 返回结果
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"创建Mod失败: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"创建Mod失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 游戏分类选择改变事件处理程序
        private void GameCategoryComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox gameCategoryComboBox = (ComboBox)sender;
            if (gameCategoryComboBox.SelectedIndex >= 0)
            {
                string selectedGameName = gameCategoryComboBox.SelectedItem.ToString();
                
                // 根据游戏名称查找游戏ID
                foreach (var game in gameManager.GetGames())
                {
                    if (game.Name == selectedGameName)
                    {
                        selectedGameId = game.Id;
                        // 更新窗体标题
                        this.Text = $"上传Mod - {selectedGameName}";
                        // 更新角色列表
                        UpdateCharactersList();
                        break;
                    }
                }
            }
        }
        
        // 更新角色列表方法
        private void UpdateCharactersList()
        {
            ComboBox characterComboBox = (ComboBox)Controls["characterComboBox"];
            characterComboBox.Items.Clear();
            
            // 从GameManager获取当前游戏的角色列表
            List<string> gameCharacters = gameManager.GetCharactersByGameId(selectedGameId);
            if (gameCharacters.Count > 0)
            {
                // 确保坎特雷拉在鸣潮游戏中可用
                if (selectedGameId == "wuthering-waves" && !gameCharacters.Contains("坎特雷拉"))
                {
                    gameCharacters.Add("坎特雷拉");
                    Console.WriteLine("已添加坎特雷拉角色");
                }
                
                characterComboBox.Items.AddRange(gameCharacters.ToArray());
                Console.WriteLine($"为游戏 {selectedGameId} 加载了 {gameCharacters.Count} 个角色");
                
                // 如果是鸣潮并且有坎特雷拉角色，则默认选择坎特雷拉
                if (selectedGameId == "wuthering-waves" && gameCharacters.Contains("坎特雷拉"))
                {
                    int kanteilaIndex = characterComboBox.Items.IndexOf("坎特雷拉");
                    if (kanteilaIndex >= 0)
                    {
                        characterComboBox.SelectedIndex = kanteilaIndex;
                        
                        // 如果没有角色被选择，自动添加坎特雷拉到已选择列表
                        if (characters.Count == 0)
                        {
                            string character = "坎特雷拉";
                            characters.Add(character);
                            
                            // 更新已选择角色列表框
                            ListBox characterListBox = (ListBox)Controls["characterListBox"];
                            if (characterListBox != null && !characterListBox.Items.Contains(character))
                            {
                                characterListBox.Items.Add(character);
                            }
                            
                            Console.WriteLine("已自动添加坎特雷拉到选择列表");
                        }
                        
                        return;
                    }
                }
                
                // 如果没有特殊选择，则默认选择第一个角色
                if (characterComboBox.Items.Count > 0)
                {
                    characterComboBox.SelectedIndex = 0;
                }
            }
            else
            {
                characterComboBox.Text = "选择或输入角色名";
            }
        }

        // 初始化链接历史菜单
        private void InitializeLinkHistoryMenu()
        {
            linkHistoryMenu = new ContextMenuStrip();
            
            // 获取下载链接输入框
            TextBox downloadLinkTextBox = (TextBox)Controls["downloadLinkTextBox"];
            
            // 为输入框添加右键菜单
            downloadLinkTextBox.ContextMenuStrip = linkHistoryMenu;
            
            // 添加菜单项
            UpdateLinkHistoryMenu();
            
            // 添加按钮显示历史记录
            Button showHistoryButton = new Button
            {
                Text = "历史",
                Size = new Size(50, 20),
                Location = new Point(downloadLinkTextBox.Right + 5, downloadLinkTextBox.Top),
                Cursor = Cursors.Hand
            };
            showHistoryButton.Click += (s, e) => 
            {
                if (linkHistoryMenu.Items.Count > 0)
                {
                    linkHistoryMenu.Show(showHistoryButton, new Point(0, showHistoryButton.Height));
                }
                else
                {
                    MessageBox.Show("没有历史记录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
            this.Controls.Add(showHistoryButton);
        }
        
        // 更新链接历史菜单
        private void UpdateLinkHistoryMenu()
        {
            linkHistoryMenu.Items.Clear();
            
            foreach (string link in recentDownloadLinks)
            {
                // 如果链接太长，显示截断版本
                string displayText = link.Length > 40 ? link.Substring(0, 37) + "..." : link;
                
                ToolStripMenuItem item = new ToolStripMenuItem(displayText);
                item.Tag = link; // 使用Tag存储完整链接
                
                item.Click += (s, e) => 
                {
                    TextBox downloadLinkTextBox = (TextBox)Controls["downloadLinkTextBox"];
                    downloadLinkTextBox.Text = (string)((ToolStripMenuItem)s).Tag;
                };
                
                linkHistoryMenu.Items.Add(item);
            }
        }
        
        // 添加链接到历史记录
        private void AddLinkToHistory(string link)
        {
            // 如果链接已存在，先移除它
            recentDownloadLinks.Remove(link);
            
            // 添加到最前面
            recentDownloadLinks.Insert(0, link);
            
            // 保持列表长度不超过最大值
            while (recentDownloadLinks.Count > MaxRecentLinks)
            {
                recentDownloadLinks.RemoveAt(recentDownloadLinks.Count - 1);
            }
            
            // 更新菜单
            UpdateLinkHistoryMenu();
        }

        // 双击游戏分类下拉框时自动添加分类
        private void GameCategoryComboBox_DoubleClick(object sender, EventArgs e)
        {
            // 手动触发添加分类按钮点击
            AddGameCategoryButton_Click(Controls["addGameCategoryButton"], EventArgs.Empty);
        }
    }
} 