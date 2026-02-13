using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using SchetinkinDemo.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SchetinkinDemo
{
    public partial class ProductsView : UserControl
    {
        public ProductsView()
        {
            InitializeComponent();

        }

        private async void ProductsView_Loaded(object? sender, RoutedEventArgs e)
        {
            await LoadProducts();
        }

        private async Task LoadProducts(string? searchText = null)
        {
            using var context = new SkateshopDbContext();

            // 1. Начинаем строить запрос к таблице Products
            IQueryable<Product> query = context.Products;

            // 2. ФИЛЬТРУЕМ: оставляем только те товары, у которых IsActive = true
            query = query.Where(p => p.IsActive == true);

            // 3. Если в поиске есть текст, добавляем еще один фильтр по имени
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(p => p.Name.ToLower().Contains(searchText.ToLower()));
            }

            // 4. Преобразуем отфильтрованные "тяжелые" Product в "легкие" ProductViewModel
            //    и выполняем запрос к базе данных
            var productViewModels = await query
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Sku = p.Sku,
                    Name = p.Name,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    BrandName = (p.Brand != null) ? p.Brand.Name : "---",
                    CategoryName = (p.Category != null) ? p.Category.Name : "---"
                })
                .ToListAsync();

            // 5. Присваиваем результат нашему ListBox для отображения
            ProductsListBox.ItemsSource = productViewModels;
        }

        private async void SearchButton_Click(object? sender, RoutedEventArgs e)
        {
            await LoadProducts(SearchTextBox.Text);
        }

        private async void AddButton_Click(object? sender, RoutedEventArgs e)
        {
            var editorWindow = new ProductEditorWindow();
            var parentWindow = (Window)this.VisualRoot;
            await editorWindow.ShowDialog(parentWindow);
            await LoadProducts(SearchTextBox.Text);
        }

        private async void EditButton_Click(object? sender, RoutedEventArgs e)
        {
            // --- ИЗМЕНЕНИЕ: Получаем выбранный элемент из ListBox ---
            var selectedItem = ProductsListBox.SelectedItem as ProductViewModel;
            if (selectedItem == null) return;

            var editorWindow = new ProductEditorWindow(selectedItem.Id);
            var parentWindow = (Window)this.VisualRoot;
            await editorWindow.ShowDialog(parentWindow);
            await LoadProducts(SearchTextBox.Text);
        }

        private async void DeleteButton_Click(object? sender, RoutedEventArgs e)
        {
            var selectedItem = ProductsListBox.SelectedItem as ProductViewModel;
            if (selectedItem == null) return;

            // Сначала показываем диалог подтверждения
            var dialog = new ConfirmDialog($"Вы уверены, что хотите убрать из продажи товар '{selectedItem.Name}'?");
            var parentWindow = (Window)this.VisualRoot;
            var result = await dialog.ShowDialog<bool>(parentWindow);

            if (result == true) // Если пользователь нажал "Да"
            {
                // --- ВОТ ГЛАВНОЕ ИЗМЕНЕНИЕ ---
                // Мы больше не используем .Remove()
                using (var context = new SkateshopDbContext())
                {
                    var productToDeactivate = await context.Products.FindAsync(selectedItem.Id);
                    if (productToDeactivate != null)
                    {
                        // Вместо удаления, мы просто меняем флаг
                        productToDeactivate.IsActive = false;
                        await context.SaveChangesAsync();
                    }
                }

                // Обновляем список, чтобы товар исчез из видимости
                await LoadProducts(SearchTextBox.Text);
            }
        }
    }
}
