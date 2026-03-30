using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using SchetinkinDemo.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SchetinkinDemo
{
    public partial class ClientCatalogView : UserControl
    {
        public ClientCatalogView()
        {
            InitializeComponent();
            Loaded += ClientCatalogView_Loaded;
        }

        private async void ClientCatalogView_Loaded(object? sender, RoutedEventArgs e)
        {
            await LoadCategories();
            await LoadProducts();
        }

        private async Task LoadCategories()
        {
            using var context = new SkateshopDbContext();
            var categories = await context.Categories.ToListAsync();
            categories.Insert(0, new Category { Id = 0, Name = "Все категории" });
            CategoryComboBox.ItemsSource = categories;
            CategoryComboBox.SelectedIndex = 0;
        }

        private async Task LoadProducts()
        {
            using var context = new SkateshopDbContext();
            IQueryable<Product> query = context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Where(p => p.IsActive == true);

            // Фильтр по категории
            var selectedCategory = CategoryComboBox.SelectedItem as Category;
            if (selectedCategory != null && selectedCategory.Id != 0)
            {
                query = query.Where(p => p.CategoryId == selectedCategory.Id);
            }

            // Поиск
            string search = SearchTextBox.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));
            }

            // Сортировка
            var sortItem = SortComboBox.SelectedItem as ComboBoxItem;
            string sortTag = sortItem?.Tag?.ToString() ?? "NameAsc";
            query = sortTag switch
            {
                "NameAsc" => query.OrderBy(p => p.Name),
                "NameDesc" => query.OrderByDescending(p => p.Name),
                "PriceAsc" => query.OrderBy(p => p.Price),
                "PriceDesc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderBy(p => p.Name)
            };

            var products = await query.Select(p => new ProductCatalogViewModel
            {
                Id = p.Id,
                Sku = p.Sku,
                Name = p.Name,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                BrandName = p.Brand != null ? p.Brand.Name : "---",
                CategoryName = p.Category != null ? p.Category.Name : "---"
            }).ToListAsync();

            ProductsListBox.ItemsSource = products;
        }

        private async void SearchButton_Click(object? sender, RoutedEventArgs e)
        {
            await LoadProducts();
        }

        private async void CategoryComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            await LoadProducts();
        }

        private async void SortComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            await LoadProducts();
        }

        private void AddToCartButton_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int productId)
            {
                // Найдём товар в текущем списке
                var product = ProductsListBox.ItemsSource
                    .Cast<ProductCatalogViewModel>()
                    .FirstOrDefault(p => p.Id == productId);
                if (product != null)
                {
                    CartManager.AddItem(product.Id, product.Name, product.Price, product.ImagePath);
                    // Можно показать уведомление
                }
            }
        }
    }

    public class ProductCatalogViewModel
    {
        public int Id { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string BrandName { get; set; }
        public string CategoryName { get; set; }
        public string? ImagePath { get; set; }
    }
}