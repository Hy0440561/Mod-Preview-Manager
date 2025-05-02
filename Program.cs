using System;
using System.IO;
using System.Windows.Forms;

namespace GameModBrowser
{
    static class Program
    {
        public static string AppDataPath { get; private set; }
        public static string ConfigFilePath { get; private set; }
        public static string ModsDirectoryPath { get; private set; }
        public static string ImagesDirectoryPath { get; private set; }
        
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                
                // 初始化应用程序路径
                InitializeAppPaths();
                
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"程序启动异常: {ex.Message}\n\n{ex.StackTrace}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private static void InitializeAppPaths()
        {
            // 应用数据目录
            AppDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GameModBrowser");
                
            // 确保目录存在
            if (!Directory.Exists(AppDataPath))
            {
                Directory.CreateDirectory(AppDataPath);
            }
            
            // 配置文件路径
            ConfigFilePath = Path.Combine(AppDataPath, "config.json");
            
            // Mods数据目录
            ModsDirectoryPath = Path.Combine(AppDataPath, "Mods");
            if (!Directory.Exists(ModsDirectoryPath))
            {
                Directory.CreateDirectory(ModsDirectoryPath);
            }
            
            // 图片存储目录
            ImagesDirectoryPath = Path.Combine(AppDataPath, "Images");
            if (!Directory.Exists(ImagesDirectoryPath))
            {
                Directory.CreateDirectory(ImagesDirectoryPath);
            }
        }
    }
} 