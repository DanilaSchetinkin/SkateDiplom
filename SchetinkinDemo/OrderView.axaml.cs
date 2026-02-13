using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using SchetinkinDemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchetinkinDemo
{
    // ИЗМЕНЕНИЕ №1: Оставляем имя класса 'OrderView' (в единственном числе)
    public partial class OrderView : UserControl
    {
        public OrderView()
        {
            InitializeComponent();
        }

        // ИЗМЕНЕНИЕ №2: Переименовываем метод, чтобы он соответствовал XAML
        // Было: OrdersView_Loaded
        // Стало: OrderView_Loaded
        private async void OrderView_Loaded(object? sender, RoutedEventArgs e)
        {
            await LoadOrders();
        }

        private async Task LoadOrders()
        {
            using var context = new SkateshopDbContext();

            var orderViewModels = await context.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderViewModel
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber ?? "БЕЗ НОМЕРА",
                    CustomerName = (o.Customer != null)
                                   ? (o.Customer.FirstName ?? "") + " " + (o.Customer.LastName ?? "")
                                   : "Неизвестный клиент",
                    OrderDate = o.CreatedAt,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status ?? "Без статуса"
                })
                .ToListAsync();

            OrdersListBox.ItemsSource = orderViewModels;
        }

        private async void OrdersListBox_DoubleTapped(object? sender, TappedEventArgs e)
        {
            // Получаем OrderViewModel, по которому кликнули
            var selectedOrder = OrdersListBox.SelectedItem as OrderViewModel;
            if (selectedOrder == null) return;

            // Создаем и открываем окно деталей, передавая ему ID заказа
            var detailsWindow = new OrderDetailsWindow(selectedOrder.Id);

            var parentWindow = (Window)this.VisualRoot;
            await detailsWindow.ShowDialog(parentWindow);
        }

        private void StatusComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            var orderVM = comboBox?.DataContext as OrderViewModel;
            var selectedStatusItem = comboBox?.SelectedItem as ComboBoxItem;

            if (orderVM == null || selectedStatusItem == null) return;

            var newStatus = selectedStatusItem.Content.ToString();

            if (orderVM.Status == newStatus) return;

            using var context = new SkateshopDbContext();
            var orderToUpdate = context.Orders.Find(orderVM.Id);
            if (orderToUpdate != null)
            {
                orderToUpdate.Status = newStatus;
                context.SaveChanges();
                orderVM.Status = newStatus;
            }
        }
    }
}