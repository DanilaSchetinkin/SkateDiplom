using Avalonia.Controls;
using Avalonia.Interactivity;
using SchetinkinDemo.Models;
using System.Diagnostics;
using System.Linq;

namespace SchetinkinDemo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }
        private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Сбрасываем старую ошибку
            ErrorMessageTextBlock.Text = string.Empty;

            // Используем ваш DbContext
            using var context = new SkateshopDbContext();// <--- ЗАМЕНИТЕ НА ИМЯ ВАШЕГО DbContext

            string email = LoginBox.Text;
            string plainTextPassword = PasswordBox.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(plainTextPassword))
            {
                ErrorMessageTextBlock.Text = "Email и пароль не могут быть пустыми.";
                return;
            }

            // ====================================================================
            // ==       НЕБЕЗОПАСНАЯ ПРОВЕРКА ПАРОЛЯ (КАК ВЫ ПРОСИЛИ)           ==
            // ====================================================================
            // Код ищет пользователя, у которого email и пароль (в открытом виде)
            // совпадают с введенными данными.
            // Замените .Users и .PasswordHash на названия вашей таблицы и колонки

            var user = context.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == plainTextPassword);


            if (user != null)
            {
                // УСПЕХ! Пользователь с таким email и паролем найден.

                var admin = new Admin();
                admin.Show();
                this.Close();
            }
            else
            {
                // НЕУДАЧА! Либо email, либо пароль неверный.
                ErrorMessageTextBlock.Text = "Неверный email или пароль.";
            }
        }
    }
}