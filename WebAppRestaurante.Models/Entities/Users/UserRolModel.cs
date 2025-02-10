﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAppRestaurante.Models.Entities.Users
{
    public class UserRolModel
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public int RoleID { get; set; }

        public virtual RoleModel Role { get; set; }
        public virtual UserModel User { get; set; }
    }
}
