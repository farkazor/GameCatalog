using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyCatalog
{
    public partial class GalleryForm : Form
    {
        private MediaForm mediaFormInstance = null;
        public GalleryForm()
        {
            InitializeComponent();
        }
        private string connectionString = "Data Source=catalog.db;Version=3;";
        public class GameCoverItem
        {
            public int ItemId { get; set; }
            public string Title { get; set; }
            public string CoverPath { get; set; }
            public string MediaPath { get; set; }    
            public string HtmlDescPath { get; set; } 
        }
        private List<GameCoverItem> coverList = new List<GameCoverItem>();
        private int currentIndex = 0;
        private const int ThumbnailCount = 6;
        private void GalleryForm_Load(object sender, EventArgs e)
        {
            LoadCoversFromDb();

            if (coverList.Count > 0)
            {
                ShowMainImage(0);
                UpdateThumbnailStrip();
            }
            else
            {
                MessageBox.Show("В базе нет загруженных обложек!", "Инфо", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void LoadCoversFromDb()
        {
            coverList.Clear();
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT item_id, title, cover_image_path, media_path, html_desc_path FROM ITEMS WHERE cover_image_path IS NOT NULL AND cover_image_path != ''";
                SQLiteCommand cmd = new SQLiteCommand(query, conn);

                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        coverList.Add(new GameCoverItem
                        {
                            ItemId = Convert.ToInt32(reader["item_id"]),
                            Title = reader["title"].ToString(),
                            CoverPath = reader["cover_image_path"]?.ToString() ?? "",
                            MediaPath = reader["media_path"]?.ToString() ?? "",
                            HtmlDescPath = reader["html_desc_path"]?.ToString() ?? ""
                        });
                    }
                }
            }
        }
        private void ShowMainImage(int index)
        {
            if (index < 0 || index >= coverList.Count) return;
            currentIndex = index;
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, coverList[currentIndex].CoverPath);
            if (File.Exists(fullPath))
            {
                picMain.ImageLocation = fullPath;
                picMain.SizeMode = PictureBoxSizeMode.Zoom;
            }
            else
            {
                picMain.Image = null;
            }
            UpdateThumbnailStrip(); 
        }
        private void UpdateThumbnailStrip()
        {
            pnlThumbnails.Controls.Clear();
            if (coverList.Count == 0) return;
            int startIndex = Math.Max(0, currentIndex - (ThumbnailCount / 2));
            if (startIndex + ThumbnailCount > coverList.Count)
            {
                startIndex = Math.Max(0, coverList.Count - ThumbnailCount);
            }
            int endIndex = Math.Min(coverList.Count, startIndex + ThumbnailCount);
            for (int i = startIndex; i < endIndex; i++)
            {
                int itemIndex = i; 
                GameCoverItem item = coverList[i];
                PictureBox thumb = new PictureBox
                {
                    Width = 90,
                    Height = 110,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Margin = new Padding(5),
                    Cursor = Cursors.Hand,
                    Tag = itemIndex
                };
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, item.CoverPath);
                if (File.Exists(fullPath))
                {
                    thumb.ImageLocation = fullPath;
                }
                if (itemIndex == currentIndex)
                {
                    thumb.BorderStyle = BorderStyle.Fixed3D;
                    thumb.BackColor = Color.LightSkyBlue;
                }
                else
                {
                    thumb.BorderStyle = BorderStyle.FixedSingle;
                    thumb.BackColor = Color.Transparent;
                }
                thumb.Click += (s, e) => ShowMainImage(itemIndex);
                pnlThumbnails.Controls.Add(thumb);
            }
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (coverList.Count == 0) return;
            int newIndex = (currentIndex - 1 + coverList.Count) % coverList.Count;
            ShowMainImage(newIndex);
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (coverList.Count == 0) return;
            int newIndex = (currentIndex + 1) % coverList.Count;
            ShowMainImage(newIndex);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            btnNext_Click(sender, e);
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {
                timer1.Stop();
                btnPlay.Text = "Старт Слайд-шоу";
            }
            else
            {
                timer1.Start();
                btnPlay.Text = "Пауза";
            }
        }
       
        private void picMain_Click(object sender, EventArgs e)
        {
            if (coverList.Count == 0 || currentIndex >= coverList.Count) return;
            int selectedGameId = coverList[currentIndex].ItemId;
            if (mediaFormInstance != null && !mediaFormInstance.IsDisposed)
            {
                mediaFormInstance.Close();
            }
            string videoPath = coverList[currentIndex].MediaPath;
            string htmlPath = coverList[currentIndex].HtmlDescPath;
            mediaFormInstance = new MediaForm(videoPath, htmlPath);
            mediaFormInstance.Show();
        }
    }
}
