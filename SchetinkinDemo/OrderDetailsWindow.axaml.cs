using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using SchetinkinDemo.Models;
using System.Linq;

namespace SchetinkinDemo
{
    public partial class OrderDetailsWindow : Window
    {
        private readonly int _orderId;

        // Конструктор по умолчанию нужен для дизайнера, его можно не использовать
        public OrderDetailsWindow()
        {
            InitializeComponent();
        }

        // НАШ ГЛАВНЫЙ КОНСТРУКТОР
        public OrderDetailsWindow(int orderId)
        {
            InitializeComponent();
            _orderId = orderId;
            LoadOrderDetails();
        }

        private void LoadOrderDetails()
        {
            using var context = new SkateshopDbContext();

            // Загружаем сам заказ, включая связанного клиента
            var order = context.Orders
                               .Include(o => o.Customer)
                               .FirstOrDefault(o => o.Id == _orderId);

            if (order == null)
            {
                // TODO: Показать сообщение, что заказ не найден
                this.Close();
                return;
            }

            // 1. Заполняем "шапку"
            OrderNumberTextBlock.Text = $"Заказ №{order.OrderNumber}";
            CustomerNameTextBlock.Text = $"Клиент: {order.Customer?.FirstName} {order.Customer?.LastName}";
            OrderDateTextBlock.Text = $"Дата: {order.CreatedAt:dd.MM.yyyy HH:mm}";
            StatusTextBlock.Text = $"Статус: {order.Status}";

            // 2. Загружаем список товаров в этом заказе
            var orderItems = context.OrderItems
                                    .Where(oi => oi.OrderId == _orderId)
                                    .Include(oi => oi.Product) // Подгружаем связанный товар
                                    .Select(oi => new OrderItemViewModel
                                    {
                                        ProductName = oi.Product.Name,
                                        Quantity = oi.Quantity,
                                        PricePerUnit = oi.PricePerUnit
                                    })
                                    .ToList();

            OrderItemsListBox.ItemsSource = orderItems;
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}