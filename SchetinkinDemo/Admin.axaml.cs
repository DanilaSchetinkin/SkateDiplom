// Admin.axaml.cs
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchetinkinDemo.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

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
            Title = userRole.ToLower() == "admin" ? "Панель администратора" : "Панель сотрудника";

            // Настраиваем видимость кнопок в зависимости от роли
            SetupInterfaceForRole();

            // При запуске сразу открываем экран с товарами
            ProductsButton_Click(null, null);
        }

        private void SetupInterfaceForRole()
        {
            var isAdmin = _userRole.ToLower() == "admin";
            UsersButton.IsVisible = isAdmin;
            CustomerSupportInboxButton.IsVisible = isAdmin;
            StaffSupportInboxButton.IsVisible = isAdmin;
            StaffTechSupportButton.IsVisible = !isAdmin;
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
            MainContentControl.Content = new OrderView();
        }

        private void UsersButton_Click(object? sender, RoutedEventArgs e)
        {
            // TODO: Заменить на ваш UserControl для пользователей
            // MainContentControl.Content = new UsersView();
            MainContentControl.Content = new UsersView();
        }

        private void LogoutButton_Click(object? sender, RoutedEventArgs e)
        {
            // Закрываем это окно и открываем окно входа
            var loginWindow = new MainWindow();
            loginWindow.Show();
            this.Close();
        }


        private void CustomerSupportInboxButton_Click(object? sender, RoutedEventArgs e)
        {
            MainContentControl.Content = new SupportInboxView(
                SupportChatHelper.CustomerSupportConversationType,
                "Обращения клиентов",
                OpenChatFromInboxAsync,
                () => ProductsButton_Click(null, null));
        }

        private void StaffSupportInboxButton_Click(object? sender, RoutedEventArgs e)
        {
            MainContentControl.Content = new SupportInboxView(
                SupportChatHelper.StaffSupportConversationType,
                "Обращения сотрудников в техподдержку",
                OpenChatFromInboxAsync,
                () => ProductsButton_Click(null, null));
        }

        private async void StaffTechSupportButton_Click(object? sender, RoutedEventArgs e)
        {
            var db = App.ServiceProvider.GetRequiredService<SkateshopDbContext>();
            var fio = FioTextBlock.Text ?? "";
            var conversationId = await SupportChatHelper.GetOrCreateStaffSupportAsync(db, _userId, fio);
            MainContentControl.Content = new ChatView(_userId, conversationId);
        }

        private async void OpenChatFromInboxAsync(int conversationId)
        {
            var db = App.ServiceProvider.GetRequiredService<SkateshopDbContext>();
            var alreadyMember = await db.ConversationParticipants
                .AnyAsync(p => p.ConversationId == conversationId && p.UserId == _userId);

            if (!alreadyMember)
            {
                db.ConversationParticipants.Add(new ConversationParticipant
                {
                    ConversationId = conversationId,
                    UserId = _userId
                });
                await db.SaveChangesAsync();
            }

            MainContentControl.Content = new ChatView(_userId, conversationId);
        }
    }
}