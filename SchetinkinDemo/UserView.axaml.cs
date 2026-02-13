using Avalonia.Controls;
using Avalonia.Interactivity;
using SchetinkinDemo.Models;
using System.Threading.Tasks;
// --- ДОБАВЛЕНЫ НЕДОСТАЮЩИЕ USING ---
using System.Linq;
using Microsoft.EntityFrameworkCore;

// --- УБРАНА ЛИШНЯЯ СКОБКА ---
namespace SchetinkinDemo
{
    public partial class UsersView : UserControl
    {
        public UsersView()
        {
            // --- ИСПРАВЛЕНА ОПЕЧАТКА ---
            InitializeComponent();
        }

        private async void UsersView_Loaded(object? sender, RoutedEventArgs e)
        {
            await LoadUsers();
        }

        private async Task LoadUsers()
        {
            using var context = new SkateshopDbContext();

            var allRoles = await context.Roles.ToListAsync();

            var userViewModels = await context.Users
                .Include(u => u.Role)
                .OrderBy(u => u.Id)
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    Email = u.Email,
                    UserRole = u.Role,
                    AllRoles = allRoles
                })
                .ToListAsync();

            UsersListBox.ItemsSource = userViewModels;
        }

        private async void RoleComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            // Проверяем, был ли добавлен новый выбранный элемент
            if (e.AddedItems.Count == 0) return;

            var comboBox = sender as ComboBox;
            var userVM = comboBox?.DataContext as UserViewModel;

            // --- ИЗМЕНЕНИЕ ---
            // Берем новую роль напрямую из аргументов события. Это надежнее.
            var newRole = e.AddedItems[0] as Role;

            if (userVM == null || newRole == null || userVM.UserRole?.Id == newRole.Id)
            {
                return;
            }

            // ... остальной код сохранения остается таким же ...
            using var context = new SkateshopDbContext();
            var userToUpdate = await context.Users.FindAsync(userVM.Id);
            if (userToUpdate != null)
            {
                userToUpdate.RoleId = newRole.Id;
                await context.SaveChangesAsync();
                userVM.UserRole = newRole;
            }
        }
    }
}