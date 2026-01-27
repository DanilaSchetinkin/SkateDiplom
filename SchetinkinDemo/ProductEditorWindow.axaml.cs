using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using SchetinkinDemo.Models;
using System.Linq;

namespace SchetinkinDemo
{
    public partial class ProductEditorWindow : Window
    {
        private readonly int? _productId;
        // DbContext теперь не нужен как поле класса

        // Конструктор для НОВОГО товара
        public ProductEditorWindow()
        {
            InitializeComponent();
            _productId = null;
            Title = "Создание нового товара";
            LoadComboBoxes();
        }

        // Конструктор для РЕДАКТИРОВАНИЯ
        public ProductEditorWindow(int productId)
        {
            InitializeComponent();
            _productId = productId;
            Title = $"Редактирование товара (ID: {productId})";
            LoadComboBoxes();
            LoadProductData();
        }

        private void LoadComboBoxes()
        {
            using var context = new SkateshopDbContext();
            BrandComboBox.ItemsSource = context.Brands.ToList();
            CategoryComboBox.ItemsSource = context.Categories.ToList();
        }

        private void LoadProductData()
        {
            using var context = new SkateshopDbContext();
            var product = context.Products.Find(_productId.Value);
            if (product == null) return;

            NameTextBox.Text = product.Name;
            SkuTextBox.Text = product.Sku;
            PriceTextBox.Text = product.Price.ToString("F2");
            StockTextBox.Text = product.StockQuantity.ToString();

            BrandComboBox.SelectedItem = (BrandComboBox.ItemsSource as System.Collections.IEnumerable)
                .OfType<Brand>().FirstOrDefault(b => b.Id == product.BrandId);

            CategoryComboBox.SelectedItem = (CategoryComboBox.ItemsSource as System.Collections.IEnumerable)
                .OfType<Category>().FirstOrDefault(c => c.Id == product.CategoryId);
        }

        // --- ГЛАВНОЕ ИЗМЕНЕНИЕ ---
        private void SaveButton_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) || BrandComboBox.SelectedItem == null || CategoryComboBox.SelectedItem == null)
            {
                return;
            }

            // Создаем "чистый" объект Product с данными из формы
            var productData = new Product
            {
                Name = NameTextBox.Text,
                Sku = SkuTextBox.Text,
                Price = decimal.TryParse(PriceTextBox.Text, out var price) ? price : 0,
                StockQuantity = int.TryParse(StockTextBox.Text, out var stock) ? stock : 0,
                BrandId = (BrandComboBox.SelectedItem as Brand)?.Id,
                CategoryId = (CategoryComboBox.SelectedItem as Category)?.Id ?? 0
            };

            // Создаем новый DbContext ТОЛЬКО для операции сохранения
            using var context = new SkateshopDbContext();

            if (_productId == null) // СЦЕНАРИЙ: Создание
            {
                // Просто добавляем новый объект
                context.Products.Add(productData);
            }
            else // СЦЕНАРИЙ: Редактирование
            {
                // Присваиваем ID нашему "чистому" объекту
                productData.Id = _productId.Value;

                // ЯВНО говорим Entity Framework, что этот объект нужно ОБНОВИТЬ, а не создать
                context.Products.Update(productData);
            }

            try
            {
                context.SaveChanges();
                this.Close();
            }
            catch (DbUpdateException ex)
            {
                // Если ошибка все еще здесь, мы ее поймаем и увидим
                System.Diagnostics.Debug.WriteLine($"ОШИБКА СОХРАНЕНИЯ: {ex.InnerException?.Message}");
                // TODO: Показать сообщение пользователю
            }
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}