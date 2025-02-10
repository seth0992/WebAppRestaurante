using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAppRestaurante.Models.Entities.Users
{
    public class UserModel
    {
        public UserModel() { 
            UserRoles = new List<UserRolModel>();
        }

        public int ID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty ;

        public virtual ICollection<UserRolModel> UserRoles { get; set; } 
    }
}
