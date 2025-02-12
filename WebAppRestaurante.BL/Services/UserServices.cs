using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAppRestaurante.BL.Repositories;
using WebAppRestaurante.Models.Entities.Products;
using WebAppRestaurante.Models.Entities.Users;

namespace WebAppRestaurante.BL.Services
{
    public interface IUserService
    {
        Task<List<UserModel>> GetAllUsersAsync();
    }
    public class UserServices(IUserRepository userRepository) : IUserService
    {
        public Task<List<UserModel>> GetAllUsersAsync()
        {
           return userRepository.GetAllUsersAsync();
        }
    }
}
