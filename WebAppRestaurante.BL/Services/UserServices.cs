using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAppRestaurante.BL.Repositories;
using WebAppRestaurante.Common.Security;
using WebAppRestaurante.Models.Entities.Products;
using WebAppRestaurante.Models.Entities.Users;

namespace WebAppRestaurante.BL.Services
{
    public interface IUserService
    {
        Task<List<UserModel>> GetAllUsersAsync();
        Task<UserModel> GetUserById(int id);
        Task<UserModel> CreateUser(UserModel user, string password);
        Task UpdateUser(UserModel user);
        Task<bool> ChangePassword(int userId, string currentPassword, string newPassword);
        Task<bool> UpdateUserRoles(int userId, List<int> roleIds);
        Task<bool> DeactivateUser(int userId);               

    }

    public class UserServices(IUserRepository userRepository, ILogger<UserModel> logger) : IUserService
    {
        public async Task<UserModel> CreateUser(UserModel user, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(password))
                    throw new ArgumentException("La contraseña no puede estar vacía");

                user.PasswordHash = PasswordHasher.HashPassword(password);
                return await userRepository.CreateUser(user);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al crear usuario {Username}", user.Username);
                throw;
            }
        }
        public async Task<bool> ChangePassword(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await userRepository.GetUserById(userId);
                if (user != null && PasswordHasher.VerifyPassword(currentPassword, user.PasswordHash))
                {
                    user.PasswordHash = PasswordHasher.HashPassword(newPassword);
                    await userRepository.UpdateUser(user);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al cambiar contraseña para el usuario {UserId}", userId);
                throw;
            }
        }

        public async Task<UserModel> GetUserById(int id)
        {
            try
            {
                return await userRepository.GetUserById(id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener usuario por ID {UserId}", id);
                throw;
            }
        }

        public async Task<bool> DeactivateUser(int userId)
        {
            try
            {
                return await userRepository.DeactivateUser(userId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al desactivar usuario {UserId}", userId);
                throw;
            }
        }

        public Task<List<UserModel>> GetAllUsersAsync()
        {
           return userRepository.GetAllUsersAsync();
        }


        public async Task UpdateUser(UserModel user)
        {
            try
            {
                await userRepository.UpdateUser(user);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al actualizar usuario {UserId}", user.ID);
                throw;
            }
        }


        public async Task<bool> UpdateUserRoles(int userId, List<int> roleIds)
        {
            try
            {
                return await userRepository.UpdateUserRoles(userId, roleIds);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al actualizar roles para usuario {UserId}", userId);
                throw;
            }
        }
    }
}
