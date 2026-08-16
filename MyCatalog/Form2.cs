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
    public partial class EditForm : Form
    {
        public EditForm()
        {
            InitializeComponent();
        }
        private string SelectFile(string filter)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = filter;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    return ofd.FileName;
                }
            }
            return null;
        }

        private void btnBrowseCover_Click(object sender, EventArgs e)
        {
            string path = SelectFile("Изображения (*.jpg;*.png)|*.jpg;*.png");
            if (path != null) txtCoverPath.Text = path;
        }

        private void btnBrowseVideo_Click(object sender, EventArgs e)
        {
            string path = SelectFile("Видео файлы (*.mp4;*.avi;*.mkv)|*.mp4;*.avi;*.mkv");
            if (path != null) txtVideoPath.Text = path;
        }

        private void btnBrowseHtml_Click(object sender, EventArgs e)
        {
            string path = SelectFile("HTML документы (*.html;*.htm)|*.html;*.htm");
            if (path != null) txtHtmlPath.Text = path;
        }
        private string CopyToAssets(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                return sourcePath; 
            string assetsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
            if (!Directory.Exists(assetsFolder))
            {
                Directory.CreateDirectory(assetsFolder);
            }
            if (sourcePath.StartsWith(assetsFolder, StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFileName(sourcePath);
            }
            string extension = Path.GetExtension(sourcePath);
            string uniqueFileName = Guid.NewGuid().ToString() + extension;
            string destinationPath = Path.Combine(assetsFolder, uniqueFileName);
            File.Copy(sourcePath, destinationPath, true);
            return Path.Combine("Assets", uniqueFileName);
        }
        private int? gameId = null;

        public EditForm(int? id = null)
        {
            InitializeComponent();
            gameId = id;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Введите название игры!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }
            if (!int.TryParse(txtYear.Text, out int year) || year < 1950 || year > DateTime.Now.Year)
            {
                MessageBox.Show("Введите корректный год выпуска!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }
            if (cmbCategory.SelectedValue == null)
            {
                MessageBox.Show("Выберите жанр!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }
            string coverRelPath = CopyToAssets(txtCoverPath.Text);
            string videoRelPath = CopyToAssets(txtVideoPath.Text);
            string htmlRelPath = CopyToAssets(txtHtmlPath.Text);
            SaveGameToDb(txtTitle.Text.Trim(), year, Convert.ToInt32(cmbCategory.SelectedValue), coverRelPath, videoRelPath, htmlRelPath);
            this.DialogResult = DialogResult.OK;
        }

        private void SaveGameToDb(string title, int year, int categoryId, string cover, string video, string html)
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=catalog.db;Version=3;"))
            {
                conn.Open();
                SQLiteCommand cmd = conn.CreateCommand();
                if (gameId == null) 
                {
                    cmd.CommandText = @"INSERT INTO ITEMS (title, release_year, category_id, cover_image_path, media_path, html_desc_path)
                                VALUES (@title, @year, @cat, @cover, @video, @html)";
                }
                else 
                {
                    cmd.CommandText = @"UPDATE ITEMS 
                                SET title=@title, release_year=@year, category_id=@cat, 
                                    cover_image_path=@cover, media_path=@video, html_desc_path=@html
                                WHERE item_id=@id";
                    cmd.Parameters.AddWithValue("@id", gameId);
                }
                cmd.Parameters.AddWithValue("@title", title);
                cmd.Parameters.AddWithValue("@year", year);
                cmd.Parameters.AddWithValue("@cat", categoryId);
                cmd.Parameters.AddWithValue("@cover", cover ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@video", video ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@html", html ?? (object)DBNull.Value);

                cmd.ExecuteNonQuery();
            }
        }
        private void LoadCategories()
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=catalog.db;Version=3;"))
            {
                conn.Open();
                string query = "SELECT category_id, category_name FROM CATEGORIES";
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                cmbCategory.DataSource = dt;
                cmbCategory.DisplayMember = "category_name"; 
                cmbCategory.ValueMember = "category_id";     
            }
        }
        private void LoadGameData(int id)
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=catalog.db;Version=3;"))
            {
                conn.Open();
                string query = "SELECT * FROM ITEMS WHERE item_id = @id";
                SQLiteCommand cmd = new SQLiteCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        txtTitle.Text = reader["title"].ToString();
                        txtYear.Text = reader["release_year"].ToString();
                        cmbCategory.SelectedValue = Convert.ToInt32(reader["category_id"]);
                        txtCoverPath.Text = reader["cover_image_path"]?.ToString() ?? "";
                        txtVideoPath.Text = reader["media_path"]?.ToString() ?? "";
                        txtHtmlPath.Text = reader["html_desc_path"]?.ToString() ?? "";
                    }
                }
            }
        }
        private void EditForm_Load(object sender, EventArgs e)
        {
            LoadCategories();
            if (gameId != null)
            {
                LoadGameData(gameId.Value);
            }
        }
    }
}
