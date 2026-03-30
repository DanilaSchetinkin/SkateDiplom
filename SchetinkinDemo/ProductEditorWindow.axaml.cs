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
        private Product _product; // Единый объект для работы

        // Конструктор для НОВОГО товара
        public ProductEditorWindow()
        {
            InitializeComponent();
            _productId = null;
            _product = new Product { IsActive = true }; // Новый товар сразу активен
            Title = "Создание нового товара";
            LoadComboBoxes();
            // Если в XAML есть привязки, можно установить DataContext = _product
            // DataContext = _product;
        }

        // Конструктор для РЕДАКТИРОВАНИЯ
        public ProductEditorWindow(int productId)
        {
            InitializeComponent();
            _productId = productId;
            Title = $"Редактирование товара (ID: {productId})";
            LoadComboBoxes();
            LoadProductData(); // Загружает _product из БД
            // DataContext = _product;
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
            _product = context.Products.Find(_productId.Value);
            if (_product == null) return;

            // Заполняем поля формы значениями из _product
            NameTextBox.Text = _product.Name;
            SkuTextBox.Text = _product.Sku;
            PriceTextBox.Text = _product.Price.ToString("F2");
            StockTextBox.Text = _product.StockQuantity.ToString();

            // Устанавливаем выбранные бренд и категорию
            BrandComboBox.SelectedItem = (BrandComboBox.ItemsSource as System.Collections.IEnumerable)
                .OfType<Brand>().FirstOrDefault(b => b.Id == _product.BrandId);
            CategoryComboBox.SelectedItem = (CategoryComboBox.ItemsSource as System.Collections.IEnumerable)
                .OfType<Category>().FirstOrDefault(c => c.Id == _product.CategoryId);

            // Если есть CheckBox для IsActive
            if (ActiveCheckBox != null)
                ActiveCheckBox.IsChecked = _product.IsActive;
        }

        private void SaveButton_Click(object? sender, RoutedEventArgs e)
        {
            // Валидация обязательных полей
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) || BrandComboBox.SelectedItem == null || CategoryComboBox.SelectedItem == null)
            {
                // TODO: показать сообщение пользователю
                return;
            }

            // Копируем данные из формы в _product
            _product.Name = NameTextBox.Text;
            _product.Sku = SkuTextBox.Text;
            _product.Price = decimal.TryParse(PriceTextBox.Text, out var price) ? price : 0;
            _product.StockQuantity = int.TryParse(StockTextBox.Text, out var stock) ? stock : 0;
            _product.BrandId = (BrandComboBox.SelectedItem as Brand)?.Id;
            _product.CategoryId = (CategoryComboBox.SelectedItem as Category)?.Id ?? 0;

            // Если CheckBox присутствует, используем его значение
            if (ActiveCheckBox != null)
                _product.IsActive = ActiveCheckBox.IsChecked == true;

            using var context = new SkateshopDbContext();

            if (_productId == null) // Создание
            {
                context.Products.Add(_product);
            }
            else // Редактирование
            {
                // Присоединяем объект к контексту и помечаем как изменённый
                context.Products.Update(_product);
            }

            try
            {
                context.SaveChanges();
                this.Close();
            }
            catch (DbUpdateException ex)
            {
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