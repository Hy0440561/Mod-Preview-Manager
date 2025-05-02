using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GameModBrowser
{
    public class ColorScheme
    {
        public Color BackgroundColor { get; set; }
        public Color BackgroundGradientStart { get; set; }
        public Color BackgroundGradientEnd { get; set; }
        public Color PrimaryColor { get; set; }
        public Color SecondaryColor { get; set; }
        public Color AccentColor { get; set; }
        public Color AccentColorHover { get; set; }
        public Color TextColor { get; set; }
        public Color AccentTextColor { get; set; }
        public Color CardColor { get; set; }
        public Color CardBorderColor { get; set; }
        public int CardBorderRadius { get; set; }
        public bool UseGradientBackground { get; set; }
    }

    public class ThemeManager
    {
        // 夜店风格的黑色主题 - 配合粉色主题
        private readonly ColorScheme darkTheme = new ColorScheme
        {
            BackgroundColor = Color.FromArgb(18, 18, 25), // 略微带蓝的黑色
            BackgroundGradientStart = Color.FromArgb(25, 10, 35), // 暗紫色渐变开始
            BackgroundGradientEnd = Color.FromArgb(15, 10, 30), // 暗紫色渐变结束
            PrimaryColor = Color.FromArgb(35, 25, 40), // 深紫标题栏
            SecondaryColor = Color.FromArgb(30, 20, 35), // 深紫侧边栏
            AccentColor = Color.FromArgb(219, 112, 147), // 深粉色按钮 - 与亮色主题匹配
            AccentColorHover = Color.FromArgb(240, 128, 170), // 深粉色按钮悬停 - 与亮色主题匹配
            TextColor = Color.FromArgb(240, 240, 245), // 亮色文本
            AccentTextColor = Color.White, // 按钮文本
            CardColor = Color.FromArgb(40, 30, 45), // 卡片背景
            CardBorderColor = Color.FromArgb(70, 50, 80), // 卡片边框
            CardBorderRadius = 10, // 与亮色主题一致
            UseGradientBackground = true
        };

        // 明亮主题 - 粉色系
        private readonly ColorScheme lightTheme = new ColorScheme
        {
            BackgroundColor = Color.FromArgb(255, 245, 250), // 浅粉色背景
            BackgroundGradientStart = Color.FromArgb(255, 240, 250), // 渐变开始色
            BackgroundGradientEnd = Color.FromArgb(250, 230, 245), // 渐变结束色
            PrimaryColor = Color.FromArgb(255, 182, 193), // 浅粉红色标题栏
            SecondaryColor = Color.FromArgb(255, 210, 220), // 粉色侧边栏
            AccentColor = Color.FromArgb(219, 112, 147), // 深粉色按钮
            AccentColorHover = Color.FromArgb(240, 128, 170), // 深粉色按钮悬停
            TextColor = Color.FromArgb(75, 50, 70), // 深色文本
            AccentTextColor = Color.White, // 按钮文本
            CardColor = Color.FromArgb(255, 250, 252), // 卡片背景色
            CardBorderColor = Color.FromArgb(245, 200, 220), // 卡片边框色
            CardBorderRadius = 10, // 稍微增加圆角
            UseGradientBackground = true
        };

        public ColorScheme GetColorScheme(bool isDark)
        {
            return isDark ? darkTheme : lightTheme;
        }

        // 获取对应主题的按钮样式
        public void ApplyButtonStyle(Button button, bool isDark, bool isAccentButton = false)
        {
            ColorScheme scheme = GetColorScheme(isDark);
            
            if (isAccentButton)
            {
                button.BackColor = scheme.AccentColor;
                button.ForeColor = scheme.AccentTextColor;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                
                // 添加鼠标悬停效果
                button.MouseEnter += (s, e) => { button.BackColor = scheme.AccentColorHover; };
                button.MouseLeave += (s, e) => { button.BackColor = scheme.AccentColor; };
            }
            else
            {
                button.BackColor = scheme.PrimaryColor;
                button.ForeColor = scheme.TextColor;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 1;
                button.FlatAppearance.BorderColor = scheme.SecondaryColor;
                
                // 添加鼠标悬停效果
                Color hoverColor = isDark ? 
                    Color.FromArgb(Math.Min(scheme.PrimaryColor.R + 15, 255), 
                                 Math.Min(scheme.PrimaryColor.G + 15, 255), 
                                 Math.Min(scheme.PrimaryColor.B + 15, 255)) :
                    Color.FromArgb(Math.Max(scheme.PrimaryColor.R - 15, 0), 
                                 Math.Max(scheme.PrimaryColor.G - 15, 0), 
                                 Math.Max(scheme.PrimaryColor.B - 15, 0));
                
                button.MouseEnter += (s, e) => { button.BackColor = hoverColor; };
                button.MouseLeave += (s, e) => { button.BackColor = scheme.PrimaryColor; };
            }
        }

        // 获取对应主题的文本框样式
        public void ApplyTextBoxStyle(TextBox textBox, bool isDark)
        {
            ColorScheme scheme = GetColorScheme(isDark);
            
            textBox.BackColor = isDark ? Color.FromArgb(45, 45, 55) : Color.White;
            textBox.ForeColor = scheme.TextColor;
            textBox.BorderStyle = BorderStyle.FixedSingle;
        }

        // 获取对应主题的下拉框样式
        public void ApplyComboBoxStyle(ComboBox comboBox, bool isDark)
        {
            ColorScheme scheme = GetColorScheme(isDark);
            
            comboBox.BackColor = isDark ? Color.FromArgb(45, 45, 55) : Color.White;
            comboBox.ForeColor = scheme.TextColor;
            comboBox.FlatStyle = FlatStyle.Flat;
        }
        
        // 创建渐变背景
        public void CreateGradientBackground(Panel panel, bool isDark)
        {
            ColorScheme scheme = GetColorScheme(isDark);
            
            // 清除之前添加的Paint事件处理器
            string handlerKey = "GradientPaintHandler";
            if (panel.Controls.ContainsKey(handlerKey))
            {
                Label handlerLabel = panel.Controls[handlerKey] as Label;
                if (handlerLabel != null && handlerLabel.Tag is PaintEventHandler oldHandler)
                {
                    panel.Paint -= oldHandler;
                }
                panel.Controls.Remove(handlerLabel);
            }
            
            if (scheme.UseGradientBackground)
            {
                // 创建新的Paint事件处理器
                PaintEventHandler paintHandler = (s, e) => 
                {
                    using (LinearGradientBrush brush = new LinearGradientBrush(
                        panel.ClientRectangle,
                        scheme.BackgroundGradientStart,
                        scheme.BackgroundGradientEnd,
                        LinearGradientMode.ForwardDiagonal))
                    {
                        e.Graphics.FillRectangle(brush, panel.ClientRectangle);
                    }
                };
                
                // 使用隐藏的Label存储处理器引用
                Label handlerStore = new Label
                {
                    Name = handlerKey,
                    Visible = false,
                    Tag = paintHandler
                };
                panel.Controls.Add(handlerStore);
                
                // 添加事件处理器
                panel.Paint += paintHandler;
                panel.Invalidate(); // 强制重绘
            }
            else
            {
                panel.BackColor = scheme.BackgroundColor;
            }
        }
        
        // 设置卡片样式，包括圆角和阴影效果
        public void ApplyCardStyle(Panel card, bool isDark)
        {
            ColorScheme scheme = GetColorScheme(isDark);
            
            card.BackColor = scheme.CardColor;
            card.BorderStyle = BorderStyle.None;
            
            // 清除之前添加的Paint事件处理器
            string handlerKey = "CardPaintHandler";
            if (card.Controls.ContainsKey(handlerKey))
            {
                Label handlerLabel = card.Controls[handlerKey] as Label;
                if (handlerLabel != null && handlerLabel.Tag is PaintEventHandler oldHandler)
                {
                    card.Paint -= oldHandler;
                }
                card.Controls.Remove(handlerLabel);
            }
            
            // 创建新的Paint事件处理器
            PaintEventHandler paintHandler = (s, e) => 
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                
                // 绘制圆角背景
                using (GraphicsPath path = new GraphicsPath())
                {
                    int radius = scheme.CardBorderRadius;
                    Rectangle rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                    
                    path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
                    path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
                    path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
                    path.CloseAllFigures();
                    
                    using (SolidBrush brush = new SolidBrush(scheme.CardColor))
                    {
                        e.Graphics.FillPath(brush, path);
                    }
                    
                    using (Pen pen = new Pen(scheme.CardBorderColor, 1))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            };
            
            // 使用隐藏的Label存储处理器引用
            Label handlerStore = new Label
            {
                Name = handlerKey,
                Visible = false,
                Tag = paintHandler
            };
            card.Controls.Add(handlerStore);
            
            // 添加事件处理器
            card.Paint += paintHandler;
            card.Invalidate(); // 强制重绘
        }
    }
} 