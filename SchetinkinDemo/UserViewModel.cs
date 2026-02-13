using SchetinkinDemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchetinkinDemo
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        // Свойство для хранения ТЕКУЩЕЙ роли пользователя (объект Role)
        public Role UserRole { get; set; }

        // Свойство для хранения ВСЕХ возможных ролей для ComboBox
        public List<Role> AllRoles { get; set; }
    }
}
