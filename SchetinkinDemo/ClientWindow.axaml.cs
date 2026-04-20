using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using SchetinkinDemo.Models;

namespace SchetinkinDemo
{
    public partial class ClientWindow : Window
    {
        private readonly int _userId;
        private readonly string _userFio;
        private ClientCatalogView _catalogView;
        private CartView _cartView;
        private ClientOrdersView _ordersView;

        public ClientWindow(int userId, string userFio)
        {
            InitializeComponent();
            _userId = userId;
            _userFio = userFio;
            FioTextBlock.Text = userFio;

            // Создаём View один раз
            _catalogView = new ClientCatalogView();
            _cartView = new CartView(userId);
            _ordersView = new ClientOrdersView(userId);

            // По умолчанию показываем каталог
            MainContentControl.Content = _catalogView;
        }

        private void CatalogButton_Click(object? sender, RoutedEventArgs e)
        {
            MainContentControl.Content = _catalogView;
        }

        private void CartButton_Click(object? sender, RoutedEventArgs e)
        {
            MainContentControl.Content = _cartView;
        }

        private void OrdersButton_Click(object? sender, RoutedEventArgs e)
        {
            MainContentControl.Content = _ordersView;
        }

        private void LogoutButton_Click(object? sender, RoutedEventArgs e)
        {
            var loginWindow = new MainWindow();
            loginWindow.Show();
            Close();
        }

        private ChatView? _supportChatView; // Может быть nullable, если не всегда инициализируем

        private async void SupportChatButton_Click(object? sender, RoutedEventArgs e)
        {
            var dbContext = App.ServiceProvider.GetRequiredService<SkateshopDbContext>();
            var conversationId = await SupportChatHelper.GetOrCreateCustomerSupportAsync(
                dbContext, _userId, _userFio);

            _supportChatView = new ChatView(_userId, conversationId);
            MainContentControl.Content = _supportChatView;
        }

    }
}