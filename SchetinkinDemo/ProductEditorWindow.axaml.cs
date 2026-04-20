using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Microsoft.EntityFrameworkCore;
using SchetinkinDemo.Models;
using System.IO;
using System.Linq;

namespace SchetinkinDemo
{
    public partial class ProductEditorWindow : Window
    {
        private readonly int? _productId;
        private Product _product;

        // Папка для хранения изображений товаров рядом с exe
        private static readonly string ImagesFolder =
            Path.Combine(System.AppContext.BaseDirectory, "ProductImages");

        public ProductEditorWindow()
        {
            InitializeComponent();
            _productId = null;
            _product = new Product { IsActive = true };
            Title = "Создание нового товара";
            LoadComboBoxes();
        }

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
            _product = context.Products.Find(_productId!.Value);
            if (_product == null) return;

            NameTextBox.Text = _product.Name;
            SkuTextBox.Text = _product.Sku;
            PriceTextBox.Text = _product.Price.ToString("F2");
            StockTextBox.Text = _product.StockQuantity.ToString();

            BrandComboBox.SelectedItem = (BrandComboBox.ItemsSource as System.Collections.IEnumerable)
                .OfType<Brand>().FirstOrDefault(b => b.Id == _product.BrandId);
            CategoryComboBox.SelectedItem = (CategoryComboBox.ItemsSource as System.Collections.IEnumerable)
                .OfType<Category>().FirstOrDefault(c => c.Id == _product.CategoryId);

            if (ActiveCheckBox != null)
                ActiveCheckBox.IsChecked = _product.IsActive;

            // Показываем текущее изображение
            if (!string.IsNullOrEmpty(_product.Productimage) && File.Exists(_product.Productimage))
            {
                SetImagePreview(_product.Productimage);
                ImagePathTextBlock.Text = Path.GetFileName(_product.Productimage);
            }
        }

        private async void PickImageButton_Click(object? sender, RoutedEventArgs e)
        {
            var topLevel = GetTopLevel(this);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Выберите изображение товара",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Изображения")
                    {
                        Patterns = new[] { "*.jpg", "*.jpeg", "*.png", "*.webp", "*.bmp" }
                    }
                }
            });

            if (files.Count == 0) return;

            var sourceFile = files[0].Path.LocalPath;

            // Копируем файл в папку приложения
            Directory.CreateDirectory(ImagesFolder);
            var destFileName = $"{System.Guid.NewGuid()}{Path.GetExtension(sourceFile)}";
            var destPath = Path.Combine(ImagesFolder, destFileName);
            File.Copy(sourceFile, destPath, overwrite: true);

            // Сохраняем путь в продукте
            _product.Productimage = destPath;

            // Показываем превью
            SetImagePreview(destPath);
            ImagePathTextBlock.Text = Path.GetFileName(destPath);
        }

        private void SetImagePreview(string path)
        {
            try
            {
                ProductImagePreview.Source = new Bitmap(path);
            }
            catch
            {
                ProductImagePreview.Source = null;
            }
        }

        private void SaveButton_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                BrandComboBox.SelectedItem == null ||
                CategoryComboBox.SelectedItem == null)
                return;

            _product.Name = NameTextBox.Text;
            _product.Sku = SkuTextBox.Text;
            _product.Price = decimal.TryParse(PriceTextBox.Text, out var price) ? price : 0;
            _product.StockQuantity = int.TryParse(StockTextBox.Text, out var stock) ? stock : 0;
            _product.BrandId = (BrandComboBox.SelectedItem as Brand)?.Id;
            _product.CategoryId = (CategoryComboBox.SelectedItem as Category)?.Id ?? 0;

            if (ActiveCheckBox != null)
                _product.IsActive = ActiveCheckBox.IsChecked == true;

            using var context = new SkateshopDbContext();

            if (_productId == null)
                context.Products.Add(_product);
            else
                context.Products.Update(_product);

            try
            {
                context.SaveChanges();
                this.Close();
            }
            catch (DbUpdateException ex)
            {
                System.Diagnostics.Debug.WriteLine($"ОШИБКА СОХРАНЕНИЯ: {ex.InnerException?.Message}");
            }
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
