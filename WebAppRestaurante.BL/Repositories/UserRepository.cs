using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAppRestaurante.Database.Data;
using WebAppRestaurante.Models.Entities.Users;

namespace WebAppRestaurante.BL.Repositories
{
    public interface IUserRepository {

        Task<List<UserModel>> GetAllUsersAsync();
    }

    public class UserRepository(AppDbContext appDbContext) : IUserRepository
    {
        public async Task<List<UserModel>> GetAllUsersAsync()
        {
           return await appDbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(u => u.Role)
                .ToListAsync();
        }
    }
}
