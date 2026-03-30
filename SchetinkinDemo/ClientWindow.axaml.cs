using Avalonia.Controls;
using Avalonia.Interactivity;

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
    }
}