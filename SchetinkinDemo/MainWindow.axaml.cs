using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using SchetinkinDemo.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SchetinkinDemo
{
    public partial class MainWindow : Window
    {
        private int _failedAttempts = 0;

        /// <summary>Роли, для которых открывается клиентское окно магазина (остальные — сотрудники).</summary>
        private static bool IsClientPortalRole(string? roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return true;
            return roleName.Trim().ToLowerInvariant() switch
            {
                "customer" or "client" or "user" or "клиент" or "покупатель" or "guest" => true,
                _ => false
            };
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object? sender, RoutedEventArgs e)
        {
            // Сбрасываем старую ошибку
            ErrorMessageTextBlock.Text = string.Empty;

            // Проверка на пустые поля
            string email = LoginBox.Text;
            string password = PasswordBox.Text;
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessageTextBlock.Text = "Email и пароль не могут быть пустыми.";
                return;
            }

            // Проверка на превышение количества попыток
            if (_failedAttempts >= 3)
            {
                ErrorMessageTextBlock.Text = "Слишком много неудачных попыток. Попробуйте через минуту.";
                LoginButton.IsEnabled = false;
                await Task.Delay(60000);
                LoginButton.IsEnabled = true;
                _failedAttempts = 0;
                return;
            }

            using var context = new SkateshopDbContext();

            // Ищем пользователя по email и паролю (в открытом виде)
            var user = await context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == password);

            if (user != null)
            {
                // Успешный вход
                _failedAttempts = 0; // сбрасываем счётчик

                string userFio = $"{user.FirstName} {user.LastName}".Trim();

                // Клиентский портал — только для ролей «клиент»; остальные (админ, менеджер, сотрудник) — панель сотрудника с поддержкой
                if (IsClientPortalRole(user.Role?.Name))
                {
                    var clientWindow = new ClientWindow(user.Id, userFio);
                    clientWindow.Show();
                }
                else
                {
                    var roleLabel = user.Role?.Name ?? "Сотрудник";
                    var adminWindow = new Admin(user.Id, roleLabel, userFio);
                    adminWindow.Show();
                }

                // Закрываем окно входа
                this.Close();
            }
            else
            {
                // Неудачный вход
                _failedAttempts++;
                ErrorMessageTextBlock.Text = "Неверный email или пароль.";

                // Очищаем поле пароля для безопасности
                PasswordBox.Text = string.Empty;

                // Если после этой попытки достигнут лимит, блокируем кнопку на минуту
                if (_failedAttempts >= 3)
                {
                    ErrorMessageTextBlock.Text = "Неверный email или пароль. Доступ заблокирован на 1 минуту.";
                    LoginButton.IsEnabled = false;
                    await Task.Delay(60000);
                    LoginButton.IsEnabled = true;
                    _failedAttempts = 0;
                }
            }
        }
    }
}