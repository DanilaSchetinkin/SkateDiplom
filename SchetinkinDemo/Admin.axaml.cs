// Admin.axaml.cs
using Avalonia.Controls;
using Avalonia.Interactivity;
using SchetinkinDemo.Models;

namespace SchetinkinDemo
{
    public partial class Admin : Window
    {
        // Поля для хранения данных о пользователе, если они нужны
        private readonly int _userId;
        private readonly string _userRole;

        // Конструктор по умолчанию для дизайнера
        public Admin()
        {
            InitializeComponent();
        }

        // Главный конструктор, который вы будете вызывать из окна входа
        // ПРИМЕР: new Admin(user.Id, user.Role.Name, user.FirstName + " " + user.LastName);
        public Admin(int userId, string userRole, string userFio)
        {
            InitializeComponent();
            _userId = userId;
            _userRole = userRole;

            // Заполняем информацию о пользователе
            FioTextBlock.Text = userFio;
            RoleTextBlock.Text = userRole;

            // Настраиваем видимость кнопок в зависимости от роли
            SetupInterfaceForRole();

            // При запуске сразу открываем экран с товарами
            ProductsButton_Click(null, null);
        }

        private void SetupInterfaceForRole()
        {
            // Если роль не "admin", скрываем кнопку управления пользователями
            if (_userRole.ToLower() != "admin")
            {
                UsersButton.IsVisible = false;
            }
        }

        // --- Обработчики кнопок навигации ---

        private void ProductsButton_Click(object? sender, RoutedEventArgs e)
        {
            // Создаем экземпляр нашего UserControl-а и помещаем его в ContentControl
            MainContentControl.Content = new ProductsView(); // Используем ProductsView, а не ProductsWindow
        }

        private void OrdersButton_Click(object? sender, RoutedEventArgs e)
        {
            // TODO: Заменить на ваш UserControl для заказов
            // MainContentControl.Content = new OrdersView(); 
            MainContentControl.Content = new TextBlock { Text = "Экран управления заказами (в разработке)", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
        }

        private void UsersButton_Click(object? sender, RoutedEventArgs e)
        {
            // TODO: Заменить на ваш UserControl для пользователей
            // MainContentControl.Content = new UsersView();
            MainContentControl.Content = new TextBlock { Text = "Экран управления пользователями (в разработке)", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
        }

        private void LogoutButton_Click(object? sender, RoutedEventArgs e)
        {
            // Закрываем это окно и открываем окно входа
            var loginWindow = new MainWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}