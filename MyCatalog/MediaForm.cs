using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyCatalog
{
    public partial class MediaForm : Form
    {
        private string videoPath;
        private string htmlPath;
        public MediaForm()
        {
            InitializeComponent();
            
        }
        public MediaForm(string videoPath, string htmlPath) : this()
        {
            this.videoPath = videoPath;
            this.htmlPath = htmlPath;
        }
        private async void MediaForm_Load(object sender, EventArgs e)
        {
            string fullVideoPath = GetFullPath(videoPath);
            string fullHtmlPath = GetFullPath(htmlPath);
            if (!string.IsNullOrEmpty(fullVideoPath) && File.Exists(fullVideoPath))
            {
                player.Visible = true;
                player.URL = fullVideoPath;
                player.Ctlcontrols.play();
            }
            else
            {
                player.Visible = false;
                webView.Dock = DockStyle.Fill;
            }
            if (!string.IsNullOrEmpty(fullHtmlPath) && File.Exists(fullHtmlPath))
            {
                await webView.EnsureCoreWebView2Async(null);
                webView.CoreWebView2.Navigate(fullHtmlPath);
            }
        }
        private string GetFullPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return null;
            string binPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath.TrimStart('\\', '/'));
            if (File.Exists(binPath)) return binPath;
            if (File.Exists(relativePath)) return relativePath;
            return null;
        }
        private void MediaForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (player.Visible)
            {
                player.Ctlcontrols.stop();
            }
        }
    }
}
