using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;

namespace MyCatalog
{
    public partial class MainForm : Form
    {
        private MediaForm mediaFormInstance = null;
        private GalleryForm galleryFormInstance = null;
        private string connectionString = "Data Source=catalog.db;Version=3;";
        public MainForm()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            LoadGamesData();
            LoadCategories();
        }
        private void LoadGamesData()
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT 
                                        i.item_id AS 'ID', 
                                        i.title AS 'Название', 
                                        i.release_year AS 'Год', 
                                        c.category_name AS 'Жанр',
                                        i.cover_image_path,
                                        i.media_path,
                                        i.html_desc_path
                                     FROM ITEMS i
                                     LEFT JOIN CATEGORIES c ON i.category_id = c.category_id";
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvGames.DataSource = dt;
                    if (dgvGames.Columns["cover_image_path"] != null) dgvGames.Columns["cover_image_path"].Visible = false;
                    if (dgvGames.Columns["media_path"] != null) dgvGames.Columns["media_path"].Visible = false;
                    if (dgvGames.Columns["html_desc_path"] != null) dgvGames.Columns["html_desc_path"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке базы данных: " + ex.Message);
            }
        }
        private void LoadCategories()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT category_id, category_name FROM CATEGORIES";
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                DataRow dr = dt.NewRow();
                dr["category_id"] = 0;
                dr["category_name"] = "Все жанры";
                dt.Rows.InsertAt(dr, 0);
                cmbCategory.DataSource = dt;
                cmbCategory.DisplayMember = "category_name"; 
                cmbCategory.ValueMember = "category_id";   
            }
        }
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }
        private void ApplyFilters()
        {
            DataTable dt = dgvGames.DataSource as DataTable;
            if (dt == null) return;
            List<string> filters = new List<string>();
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                string searchText = txtSearch.Text.Replace("'", "''");
                filters.Add($"Название LIKE '%{searchText}%'");
            }

            if (cmbCategory.SelectedValue != null)
            {
                if (int.TryParse(cmbCategory.SelectedValue.ToString(), out int categoryId) && categoryId > 0)
                {
                    DataRowView selectedRow = cmbCategory.SelectedItem as DataRowView;
                    if (selectedRow != null)
                    {
                        string categoryName = selectedRow["category_name"].ToString().Replace("'", "''");
                        filters.Add($"Жанр = '{categoryName}'");
                    }
                }
            }
            dt.DefaultView.RowFilter = string.Join(" AND ", filters);
        }

        private void cmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show("Точно закрыть программу?", "Выход", MessageBoxButtons.YesNo);
            if (result == DialogResult.No) e.Cancel = true; 
        }

        private void dgvGames_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvGames.Rows.Count) return;
            var yearCell = dgvGames.Rows[e.RowIndex].Cells["Год"].Value;
            if (yearCell != null && int.TryParse(yearCell.ToString(), out int year))
            {
                if (year < 2015)
                {
                    e.CellStyle.BackColor = Color.MistyRose;
                    e.CellStyle.SelectionBackColor = Color.LightCoral; 
                }
                else
                {
                    e.CellStyle.BackColor = Color.Honeydew;
                    e.CellStyle.SelectionBackColor = Color.LightGreen; 
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            EditForm editForm = new EditForm();
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadGamesData(); 
            }
        }

        private void dgvGames_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int selectedId = Convert.ToInt32(dgvGames.Rows[e.RowIndex].Cells["ID"].Value);
            EditForm editForm = new EditForm(selectedId);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadGamesData(); 
            }
        }

        private void buttonGallery_Click(object sender, EventArgs e)
        { 
            if (galleryFormInstance != null && !galleryFormInstance.IsDisposed)
            {
                galleryFormInstance.BringToFront();
                galleryFormInstance.Focus();
                return;
            }
            galleryFormInstance = new GalleryForm();
            galleryFormInstance.Show(); 
        }
        private void OpenMediaForm(int gameId)
        {
            string videoPath = "";
            string htmlPath = "";
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT media_path, html_desc_path FROM ITEMS WHERE item_id = @id";

                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", gameId);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            videoPath = reader["media_path"]?.ToString() ?? "";
                            htmlPath = reader["html_desc_path"]?.ToString() ?? "";
                        }
                    }
                }
            }
            if (mediaFormInstance != null && !mediaFormInstance.IsDisposed)
            {
                mediaFormInstance.Close();
            }
            mediaFormInstance = new MediaForm(videoPath, htmlPath);
            mediaFormInstance.Show(); 
        }

        private void btnOpenMedia_Click(object sender, EventArgs e)
        {
            if (dgvGames.CurrentRow == null || dgvGames.CurrentRow.Index < 0)
            {
                MessageBox.Show("Сначала выберите игру из таблицы!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int selectedGameId = Convert.ToInt32(dgvGames.CurrentRow.Cells["ID"].Value);
            OpenMediaForm(selectedGameId);
        }
    }
}
