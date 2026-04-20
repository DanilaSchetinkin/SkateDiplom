using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using SchetinkinDemo.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchetinkinDemo
{
    public partial class ClientCatalogView : UserControl
    {
        private int _selectedCategoryId = 0; // 0 = все товары
        private Button? _activeButton;

        public ClientCatalogView()
        {
            InitializeComponent();
            Loaded += ClientCatalogView_Loaded;
        }

        private async void ClientCatalogView_Loaded(object? sender, RoutedEventArgs e)
        {
            await LoadCategories();
        }

        private async Task LoadCategories()
        {
            using var context = new SkateshopDbContext();
            var categories = await context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            CategoriesPanel.Children.Clear();

            // Кнопка "Все товары"
            AddCategoryButton(new Category { Id = 0, Name = "Все товары" }, isFirst: true);

            foreach (var cat in categories)
                AddCategoryButton(cat);

            // Загружаем товары для "Все"
            await LoadProducts();
        }

        private void AddCategoryButton(Category category, bool isFirst = false)
        {
            var btn = new Button
            {
                Content = category.Name,
                Tag = category.Id,
                Classes = { isFirst ? "category-btn-active" : "category-btn" }
            };

            if (isFirst)
                _activeButton = btn;

            btn.Click += async (_, _) =>
            {
                // Снимаем активный стиль с предыдущей кнопки
                if (_activeButton != null)
                {
                    _activeButton.Classes.Remove("category-btn-active");
                    _activeButton.Classes.Add("category-btn");
                }

                // Ставим активный стиль на нажатую
                btn.Classes.Remove("category-btn");
                btn.Classes.Add("category-btn-active");
                _activeButton = btn;

                _selectedCategoryId = (int)(btn.Tag ?? 0);
                await LoadProducts();
            };

            CategoriesPanel.Children.Add(btn);
        }

        private async Task LoadProducts()
        {
            using var context = new SkateshopDbContext();
            IQueryable<Product> query = context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Where(p => p.IsActive);

            // Фильтр по категории
            if (_selectedCategoryId != 0)
            {
                query = query.Where(p => p.CategoryId == _selectedCategoryId);
                CategoryTitleTextBlock.Text = (_activeButton?.Content as string) ?? "";
            }
            else
            {
                CategoryTitleTextBlock.Text = "Все товары";
            }

            // Поиск
            var search = SearchTextBox.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));

            // Сортировка
            var sortTag = (SortComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "NameAsc";
            query = sortTag switch
            {
                "NameAsc"   => query.OrderBy(p => p.Name),
                "NameDesc"  => query.OrderByDescending(p => p.Name),
                "PriceAsc"  => query.OrderBy(p => p.Price),
                "PriceDesc" => query.OrderByDescending(p => p.Price),
                _           => query.OrderBy(p => p.Name)
            };

            var products = await query.Select(p => new ProductCatalogViewModel
            {
                Id            = p.Id,
                Sku           = p.Sku,
                Name          = p.Name,
                Price         = p.Price,
                StockQuantity = p.StockQuantity,
                BrandName     = p.Brand != null ? p.Brand.Name : "---",
                CategoryName  = p.Category != null ? p.Category.Name : "---",
                ImagePath     = p.Productimage
            }).ToListAsync();

            ProductsItemsControl.ItemsSource = products;
        }

        private async void SearchButton_Click(object? sender, RoutedEventArgs e)
        {
            await LoadProducts();
        }

        private async void SortComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            await LoadProducts();
        }

        private void AddToCartButton_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int productId) return;

            var product = (ProductsItemsControl.ItemsSource as IEnumerable<ProductCatalogViewModel>)
                ?.FirstOrDefault(p => p.Id == productId);

            if (product != null)
                CartManager.AddItem(product.Id, product.Name, product.Price, product.ImagePath);
        }
    }

    public class ProductCatalogViewModel
    {
        public int Id { get; set; }
        public string Sku { get; set; } = "";
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string BrandName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public string? ImagePath { get; set; }
    }
}
