using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using SchetinkinDemo.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SchetinkinDemo
{
    public partial class CartView : UserControl
    {
        private int _currentUserId; // задаётся из окна

        // Конструктор по умолчанию для дизайнера
        public CartView()
        {
            InitializeComponent();
            // Можно установить значения по умолчанию, чтобы дизайнер что-то показывал
            _currentUserId = 0;
            RefreshCart(); // вызовется, но в дизайнере корзина пустая
        }

        public CartView(int userId)
        {
            InitializeComponent();
            _currentUserId = userId;
            Loaded += CartView_Loaded;
            Unloaded += CartView_Unloaded;
            CartManager.CartChanged += RefreshCart;
        }

        private void CartView_Loaded(object? sender, RoutedEventArgs e)
        {
            RefreshCart();
        }

        private void CartView_Unloaded(object? sender, RoutedEventArgs e)
        {
            CartManager.CartChanged -= RefreshCart;
        }

        private void RefreshCart()
        {
            CartItemsListBox.ItemsSource = CartManager.Items.ToList();
            CheckoutButton.IsEnabled = CartManager.Items.Any();
            ClearCartButton.IsEnabled = CartManager.Items.Any();
            TotalTextBlock.Text = $"Итого: {CartManager.Items.Sum(i => i.Total):c}";
        }

        private void Quantity_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            if (sender is NumericUpDown nud && nud.Tag is int productId && nud.Value.HasValue)
            {
                CartManager.UpdateQuantity(productId, (int)nud.Value.Value);
                RefreshCart();
            }
        }

        private void RemoveItem_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int productId)
            {
                CartManager.RemoveItem(productId);
                RefreshCart();
            }
        }

        private void ClearCartButton_Click(object? sender, RoutedEventArgs e)
        {
            CartManager.Clear();
            RefreshCart();
        }

        private async void CheckoutButton_Click(object? sender, RoutedEventArgs e)
        {
            using var context = new SkateshopDbContext();

            var total = CartManager.Items.Sum(i => i.Total);

            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(context),
                CustomerId = _currentUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = "Новый",
                TotalAmount = total
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            foreach (var item in CartManager.Items)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    PricePerUnit = item.Price
                };
                context.OrderItems.Add(orderItem);
            }
            await context.SaveChangesAsync();

            CartManager.Clear();
            RefreshCart();

            var messageBox = new MessageBox($"Заказ оформлен! Номер заказа: {order.OrderNumber}");
            await messageBox.ShowDialog((Window)this.VisualRoot);
        }

        private string GenerateOrderNumber(SkateshopDbContext context)
        {
            // Используем случайный суффикс чтобы избежать коллизий при одновременных заказах
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var random = new Random().Next(1000, 9999);
            return $"ORD-{datePart}-{random}";
        }
    }
}

