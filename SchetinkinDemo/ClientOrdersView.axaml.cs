using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using SchetinkinDemo.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SchetinkinDemo
{
    public partial class ClientOrdersView : UserControl
    {
        private int _currentUserId;

        // Конструктор по умолчанию для дизайнера
        public ClientOrdersView()
        {
            InitializeComponent();
            _currentUserId = 0;
            Loaded += ClientOrdersView_Loaded; // или можно вызвать LoadOrders() напрямую
        }

        public ClientOrdersView(int userId)
        {
            InitializeComponent();
            _currentUserId = userId;
            Loaded += ClientOrdersView_Loaded;
        }

        private async void ClientOrdersView_Loaded(object? sender, RoutedEventArgs e)
        {
            await LoadOrders();
        }

        private async Task LoadOrders()
        {
            using var context = new SkateshopDbContext();
            var orders = await context.Orders
                .Where(o => o.CustomerId == _currentUserId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new ClientOrderViewModel
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    CreatedAt = o.CreatedAt ?? DateTime.UtcNow,
                    Status = o.Status
                })
                .ToListAsync();
            OrdersListBox.ItemsSource = orders;
        }

        private async void ViewDetails_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int orderId)
            {
                var detailsWindow = new OrderDetailsWindow(orderId);
                await detailsWindow.ShowDialog((Window)this.VisualRoot);
            }
        }
    }

    // Вспомогательный класс для отображения
    public class ClientOrderViewModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
    }
}