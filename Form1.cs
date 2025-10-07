using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ISREC
{
    public partial class FormMenu : Form
    {

        private const int MAX_PRICE_FOR_TRACKBARS = 1000000;
        private const int SMALL_CHANGE_FOR_TRACKBARS = 100000;
        private const string CONNECTION_STRING = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\DBISREC.mdf;Integrated Security=True";

        private bool _dragging = false;
        private Point _startPoint = new Point(0, 0);
        private string subCategoryType = "";
        private string subCategoryType1 = "";
        private string selectedImagePath;

        private readonly Func<int, int> roundValue = value => (value / 10000) * 10000;
        private readonly Func<string, int, int> parseOrDefault = (input, defaultValue) =>
            int.TryParse(input.Replace(" ", ""), out int val) ? val : defaultValue;

        public FormMenu()
        {
            InitializeComponent();
            InitializeControls();
        }

        private void InitializeControls()
        {
            guna2ComboBoxSubcategoryPropertyType.Visible = false;
            guna2PanelPage2.Visible = false;

            guna2TrackBarPrice_1.Maximum = MAX_PRICE_FOR_TRACKBARS;
            guna2TrackBarPrice_1.SmallChange = SMALL_CHANGE_FOR_TRACKBARS;
            guna2TrackBarPrice_2.Maximum = MAX_PRICE_FOR_TRACKBARS;
            guna2TrackBarPrice_2.SmallChange = SMALL_CHANGE_FOR_TRACKBARS;
        }

        public class CustomFlowLayoutPanel : FlowLayoutPanel
        {
            public CustomFlowLayoutPanel()
            {
                this.DoubleBuffered = true;
            }
        }

        public class Property
        {
            public int Id { get; set; }
            public string Heading { get; set; }
            public decimal Price { get; set; }
            public int Area { get; set; }
            public string Place { get; set; }
            public byte[] Image { get; set; }
            public string Type { get; set; }
        }

        private void guna2PanelUp_MouseDown(object sender, MouseEventArgs e)
        {
            _dragging = true;
            _startPoint = new Point(e.X, e.Y);
        }

        private void guna2PanelUp_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }

        private void guna2PanelUp_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - _startPoint.X, p.Y - _startPoint.Y);
            }
        }

        private void gunaButtonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void guna2ButtonCollapse_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }


        private void guna2ComboBoxPropertyType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (guna2ComboBoxPropertyType.SelectedItem == null) return;

            guna2ComboBoxSubcategoryPropertyType.Items.Clear();
            guna2ComboBoxSubcategoryPropertyType.Visible = true;

            string selectedCategory = guna2ComboBoxPropertyType.SelectedItem.ToString();
            labelTypeProperty.Text = $"{selectedCategory}: ";

            PopulateSubcategories(selectedCategory, guna2ComboBoxSubcategoryPropertyType);
            guna2ComboBoxSubcategoryPropertyType.DroppedDown = true;
        }

        private void guna2ComboBoxSubcategoryPropertyType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (guna2ComboBoxSubcategoryPropertyType.SelectedItem == null) return;

            string mainCategory = guna2ComboBoxPropertyType.SelectedItem.ToString();
            subCategoryType = guna2ComboBoxSubcategoryPropertyType.SelectedItem.ToString();

            labelTypeProperty.Text = $"{mainCategory}: {subCategoryType}";
            guna2ComboBoxSubcategoryPropertyType.Visible = false;
        }

        private void guna2ComboBoxCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (guna2ComboBoxCategory.SelectedItem == null) return;

            guna2ComboBoxSubCategory.Items.Clear();
            guna2ComboBoxSubCategory.Visible = true;

            string selectedCategory = guna2ComboBoxCategory.SelectedItem.ToString();
            labelTP.Text = $"{selectedCategory}: ";

            PopulateSubcategories(selectedCategory, guna2ComboBoxSubCategory);
            guna2ComboBoxSubCategory.DroppedDown = true;
        }

        private void guna2ComboBoxSubCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (guna2ComboBoxSubCategory.SelectedItem == null) return;

            string mainCategory = guna2ComboBoxCategory.SelectedItem.ToString();
            subCategoryType1 = guna2ComboBoxSubCategory.SelectedItem.ToString();

            labelTP.Text = $"{mainCategory}: {subCategoryType1}";
            guna2ComboBoxSubCategory.Visible = false;
        }

        private void guna2ComboBoxLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateLocationLabel(guna2ComboBoxLocation, labelLocation);
        }

        private void guna2ComboBoxPlace_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateLocationLabel(guna2ComboBoxPlace, labelLC);
        }

        private void UpdateLocationLabel(Guna.UI2.WinForms.Guna2ComboBox comboBox, Label label)
        {
            string location = "Расположение";
            label.Text = location;

            if (comboBox.SelectedItem == null) return;

            string selectedLocation = comboBox.SelectedItem.ToString();
            label.Text = $"{location}: {selectedLocation}";
        }

        private void guna2TrackBarPrice_1_Scroll(object sender, ScrollEventArgs e)
        {
            int roundedPrice = roundValue(guna2TrackBarPrice_1.Value);
            guna2TextBoxPrice_1.Text = roundedPrice.ToString("N0");
        }

        private void guna2TrackBarPrice_2_Scroll(object sender, ScrollEventArgs e)
        {
            int roundedPrice = roundValue(guna2TrackBarPrice_2.Value);
            guna2TextBoxPrice_2.Text = roundedPrice.ToString("N0");
        }

        private void guna2TextBoxPrice_1_TextChanged(object sender, EventArgs e)
        {
            guna2TrackBarPrice_1.Value = parseOrDefault(guna2TextBoxPrice_1.Text, guna2TrackBarPrice_1.Value);
        }

        private void guna2TextBoxPrice_2_TextChanged(object sender, EventArgs e)
        {
            guna2TrackBarPrice_2.Value = parseOrDefault(guna2TextBoxPrice_2.Text, guna2TrackBarPrice_2.Value);
        }

        private void guna2ButtonClients_Click(object sender, EventArgs e)
        {
            if (guna2PanelPage2.Visible) return;

            ShowPropertiesPage();
            LoadProperties(25);
        }

        private void guna2Button_Search_1_Click(object sender, EventArgs e)
        {
            ShowSearchPage();
        }

        private void guna2ButtonCreates_Click(object sender, EventArgs e)
        {
            HideAllPages();
        }

        private void gunaButtonSearch_2_Click(object sender, EventArgs e)
        {
            PerformSearch();
        }

        private void guna2ButtonChoose_Click(object sender, EventArgs e)
        {
            SelectImage();
        }

        private void guna2ButtonCreate_Click(object sender, EventArgs e)
        {
            CreateProperty();
        }

        private List<Property> GetRandomProperties(int count, string propertyType = null)
        {
            var properties = new List<Property>();

            using (var connection = new SqlConnection(CONNECTION_STRING))
            {
                try
                {
                    connection.Open();
                    string query = BuildPropertyQuery(count, propertyType);

                    using (var command = new SqlCommand(query, connection))
                    {
                        if (propertyType != null)
                            command.Parameters.AddWithValue("@Type", propertyType);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                properties.Add(ReadProperty(reader));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
            return properties;
        }

        private string BuildPropertyQuery(int count, string propertyType)
        {
            return propertyType == null
                ? $"SELECT TOP {count} * FROM [Table] ORDER BY NEWID()"
                : $"SELECT TOP {count} * FROM [Table] WHERE Type = @Type ORDER BY NEWID()";
        }

        private Property ReadProperty(SqlDataReader reader)
        {
            return new Property
            {
                Id = reader.GetInt32(0),
                Heading = reader.GetString(1),
                Price = reader.GetDecimal(2),
                Area = reader.GetInt32(3),
                Place = reader.GetString(6),
                Image = reader[7] as byte[],
                Type = reader.GetString(4)
            };
        }

        private void ShowPropertiesPage()
        {
            guna2PanelPage2.Visible = true;
            guna2PanelPage1.Visible = false;
            flowLayoutPanelCards.Controls.Clear();
        }

        private void ShowSearchPage()
        {
            guna2PanelPage1.Visible = true;
            guna2PanelPage2.Visible = false;
        }

        private void HideAllPages()
        {
            guna2PanelPage1.Visible = false;
            guna2PanelPage2.Visible = false;
        }

        private void LoadProperties(int count)
        {
            var properties = GetRandomProperties(count);
            DisplayProperties(properties);
        }

        private void DisplayProperties(List<Property> properties)
        {
            foreach (var property in properties)
            {
                var card = CreatePropertyCard(property);
                flowLayoutPanelCards.Controls.Add(card);
            }
        }

        private Guna2Panel CreatePropertyCard(Property property)
        {
            var card = new Guna2Panel
            {
                Size = new Size(300, 148),
                BorderRadius = 4,
                FillColor = Color.White,
                BorderColor = Color.Gray,
                BorderThickness = 1,
                Margin = new Padding(17)
            };

            AddCardControls(card, property);
            return card;
        }

        private void AddCardControls(Guna2Panel card, Property property)
        {
            // Заголовок
            card.Controls.Add(new Guna2HtmlLabel
            {
                Text = property.Heading,
                Location = new Point(10, 10),
                Font = new Font("Century Gothic", 12, FontStyle.Bold),
                AutoSize = true
            });

            // Площадь
            card.Controls.Add(new Guna2HtmlLabel
            {
                Text = $"Площадь {property.Area} м²",
                Location = new Point(10, 35),
                Font = new Font("Century Gothic", 9),
                AutoSize = true
            });

            // Место
            card.Controls.Add(new Guna2HtmlLabel
            {
                Text = property.Place,
                Location = new Point(card.Size.Width - 155, card.Size.Height - 20),
                Font = new Font("Century Gothic", 8, FontStyle.Bold),
                AutoSize = false,
                Size = new Size(150, 20),
                TextAlignment = ContentAlignment.TopRight
            });

            // Цена
            card.Controls.Add(new Guna2HtmlLabel
            {
                Text = $"{property.Price:N0} Р в мес",
                Location = new Point(10, 60),
                Font = new Font("Century Gothic", 11, FontStyle.Bold),
                AutoSize = true
            });

            // Изображение
            card.Controls.Add(new Guna2PictureBox
            {
                Image = ByteArrayToImage(property.Image),
                Size = new Size(130, 100),
                Location = new Point(150, 35),
                BorderRadius = 4,
                SizeMode = PictureBoxSizeMode.StretchImage
            });

            // Кнопка типо чата
            var btnChat = new Guna2Button
            {
                Text = "Связаться",
                Size = new Size(120, 30),
                Location = new Point(10, 110),
                FillColor = Color.FromArgb(65, 88, 112),
                ForeColor = Color.White,
                BorderRadius = 4
            };
            btnChat.Click += (s, e) => MessageBox.Show($"ID {property.Id}");
            card.Controls.Add(btnChat);
        }

        private void PopulateSubcategories(string category, Guna.UI2.WinForms.Guna2ComboBox comboBox)
        {
            var subcategories = GetSubcategoriesForCategory(category);
            if (subcategories != null)
            {
                comboBox.Items.AddRange(subcategories);
                comboBox.Visible = true;
            }
        }

        private object[] GetSubcategoriesForCategory(string category)
        {
            switch (category)
            {
                case "Жилая":
                    return new object[] { "Квартира", "Частный дом", "Апартаменты" };
                case "Коммерческая":
                    return new object[] { "Офисные помещения", "Торговая недвижимость", "Склады" };
                case "Земельные участки":
                    return new object[] { "Для жилой застройки", "Для коммерции", "Сельхоз земли" };
                case "Специальные":
                    return new object[] { "Гаражи и парковки", "Промышленные объекты", "Социальные объекты" };
                case "Другие":
                    return new object[] { "Короткая аренда", "Инвестиционные объекты", "Доли и совместная собственность" };
                default:
                    return null;
            }
        }

        private Image ByteArrayToImage(byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length == 0) return null;

            using (var ms = new MemoryStream(byteArray))
            {
                return Image.FromStream(ms);
            }
        }

        private void PerformSearch()
        {
            string place = guna2ComboBoxLocation.Text;
            string price_1 = guna2TextBoxPrice_1.Text;
            string price_2 = guna2TextBoxPrice_2.Text;

            if (!ValidateSearchCriteria(place))
                return;

            ShowPropertiesPage();

            var properties = GetFilteredProperties(place, price_1, price_2);

            if (properties.Count == 0)
            {
                MessageBox.Show("По вашему запросу ничего не найдено", "Результаты поиска",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DisplayProperties(properties);
        }

        private bool ValidateSearchCriteria(string place)
        {
            if (string.IsNullOrWhiteSpace(subCategoryType) || string.IsNullOrWhiteSpace(place))
            {
                return false;
            }
            return true;
        }

        private List<Property> GetFilteredProperties(string place, string price_1, string price_2)
        {
            var properties = GetRandomProperties(25);

            // Фильтрация по месту и типу
            properties = properties.Where(p => p.Place.Contains(place)).ToList();
            properties = properties.Where(p => p.Type.Contains(subCategoryType)).ToList();

            // Фильтрация по цене
            if (!string.IsNullOrWhiteSpace(price_1) || !string.IsNullOrWhiteSpace(price_2))
            {
                properties = FilterByPrice(properties, price_1, price_2);
            }

            return properties;
        }

        private List<Property> FilterByPrice(List<Property> properties, string minPriceStr, string maxPriceStr)
        {
            decimal minPrice = 0;
            decimal maxPrice = decimal.MaxValue;

            if (!string.IsNullOrWhiteSpace(minPriceStr))
            {
                if (!decimal.TryParse(minPriceStr, out minPrice))
                {
                    MessageBox.Show("Неверный формат минимальной цены", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return new List<Property>();
                }
            }

            if (!string.IsNullOrWhiteSpace(maxPriceStr))
            {
                if (!decimal.TryParse(maxPriceStr, out maxPrice))
                {
                    MessageBox.Show("Неверный формат максимальной цены", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return new List<Property>();
                }
            }

            if (minPrice > maxPrice)
            {
                MessageBox.Show("Минимальная цена не может быть больше максимальной", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return new List<Property>();
            }

            return properties.Where(p => p.Price >= minPrice && p.Price <= maxPrice).ToList();
        }

        private void SelectImage()
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Выберите изображение";
                openFileDialog.Filter = "Файлы изображений|*.jpg;*.jpeg;*.png;*.gif;*.bmp|Все файлы|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        selectedImagePath = openFileDialog.FileName;
                        guna2PictureBox5.Image = Image.FromFile(openFileDialog.FileName);
                        guna2PictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}",
                                      "Ошибка",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void CreateProperty()
        {
            if (!ValidatePropertyFields())
            {
                MessageBox.Show("Нужно заполнить все поля!", "Предупреждение",
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            byte[] imageBytes = GetImageBytes();
            if (imageBytes == null && !string.IsNullOrEmpty(selectedImagePath))
            {
                return;
            }

            SavePropertyToDatabase(imageBytes);
        }

        private bool ValidatePropertyFields()
        {
            string place = guna2ComboBoxPlace.Text;

            return !string.IsNullOrWhiteSpace(guna2TextBoxCreateName.Text) &&
                   !string.IsNullOrWhiteSpace(guna2TextBoxCreatePrice.Text) &&
                   !string.IsNullOrWhiteSpace(guna2TextBoxCreateArea.Text) &&
                   !string.IsNullOrWhiteSpace(subCategoryType1) &&
                   !string.IsNullOrWhiteSpace(place);
        }

        private byte[] GetImageBytes()
        {
            if (string.IsNullOrEmpty(selectedImagePath))
                return null;

            try
            {
                return File.ReadAllBytes(selectedImagePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при чтении изображения: {ex.Message}",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void SavePropertyToDatabase(byte[] imageBytes)
        {
            using (var connection = new SqlConnection(CONNECTION_STRING))
            {
                try
                {
                    connection.Open();

                    string query = @"INSERT INTO [Table] (Heading, Price, Area, Type, Place, Image) 
                                   VALUES (@Heading, @Price, @Area, @Type, @Place, @Image)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        AddPropertyParameters(command, imageBytes);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Данные успешно сохранены", "Успех",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearFields();
                        }
                    }
                }
                catch (FormatException)
                {
                    MessageBox.Show("Неправильный формат",
                                  "Ошибка формата", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Ошибка базы данных: {ex.Message}",
                                  "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}",
                                  "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void AddPropertyParameters(SqlCommand command, byte[] imageBytes)
        {
            string place = guna2ComboBoxPlace.Text;

            command.Parameters.AddWithValue("@Heading", guna2TextBoxCreateName.Text);
            command.Parameters.AddWithValue("@Price", Convert.ToDecimal(guna2TextBoxCreatePrice.Text));
            command.Parameters.AddWithValue("@Area", Convert.ToInt32(guna2TextBoxCreateArea.Text));
            command.Parameters.AddWithValue("@Type", subCategoryType1);
            command.Parameters.AddWithValue("@Place", place);

            if (imageBytes != null)
                command.Parameters.AddWithValue("@Image", imageBytes);
            else
                command.Parameters.AddWithValue("@Image", DBNull.Value);
        }

        private void ClearFields()
        {
            guna2TextBoxCreateName.Clear();
            guna2TextBoxCreatePrice.Clear();
            guna2TextBoxCreateArea.Clear();
            guna2ComboBoxPlace.SelectedIndex = -1;
            guna2PictureBox5.Image = null;
            selectedImagePath = null;
        }
    }
}
