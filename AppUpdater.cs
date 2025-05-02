using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameModBrowser
{
    public class AppUpdater : IDisposable
    {
        private readonly string _currentVersion;
        private readonly string _updateServerUrl;
        private readonly string _updateInfoFileName = "version.json";
        private readonly HttpClient _httpClient;
        private bool _disposed;
        
        public event EventHandler<string> UpdateAvailable;
        public event EventHandler<Exception> UpdateCheckFailed;
        
        public AppUpdater(string currentVersion, string updateServerUrl)
        {
            _currentVersion = currentVersion;
            _updateServerUrl = updateServerUrl;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }
        
        public async Task CheckForUpdatesAsync()
        {
            try
            {
                string latestVersion = await GetLatestVersionAsync();
                
                if (!string.IsNullOrEmpty(latestVersion) && IsNewerVersion(latestVersion, _currentVersion))
                {
                    UpdateAvailable?.Invoke(this, latestVersion);
                }
            }
            catch (Exception ex)
            {
                UpdateCheckFailed?.Invoke(this, ex);
            }
        }
        
        private async Task<string> GetLatestVersionAsync()
        {
            string updateUrl = $"{_updateServerUrl}/{_updateInfoFileName}";
            
            HttpResponseMessage response = await _httpClient.GetAsync(updateUrl);
            response.EnsureSuccessStatusCode();
            
            string json = await response.Content.ReadAsStringAsync();
            
            // 简单解析JSON
            if (json.Contains("version"))
            {
                int start = json.IndexOf("version") + 10; // "version":"
                int end = json.IndexOf("\"", start);
                return json.Substring(start, end - start);
            }
            
            return string.Empty;
        }
        
        private bool IsNewerVersion(string latestVersion, string currentVersion)
        {
            Version latest = Version.Parse(latestVersion);
            Version current = Version.Parse(currentVersion);
            
            return latest > current;
        }
        
        public string GetDownloadUrl(string version)
        {
            // 构建下载URL
            return $"{_updateServerUrl}/downloads/GameModBrowser-v{version}.zip";
        }
        
        public void DownloadAndInstallUpdate(string version)
        {
            try
            {
                string downloadUrl = GetDownloadUrl(version);
                
                // 提示用户，将开始下载更新
                DialogResult result = MessageBox.Show(
                    $"发现新版本: v{version}\n当前版本: v{_currentVersion}\n\n是否立即下载并安装更新?",
                    "更新可用",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);
                    
                if (result == DialogResult.Yes)
                {
                    // 使用浏览器下载
                    Process.Start(downloadUrl);
                    
                    MessageBox.Show(
                        "下载已开始。请在下载完成后关闭当前应用并安装新版本。",
                        "正在更新",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"下载更新失败: {ex.Message}",
                    "更新错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        
        // 释放资源
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // 释放托管资源
                    _httpClient?.Dispose();
                }
                
                // 释放非托管资源
                _disposed = true;
            }
        }
        
        // 析构函数
        ~AppUpdater()
        {
            Dispose(false);
        }
    }
} 