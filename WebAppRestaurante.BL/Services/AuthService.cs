using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAppRestaurante.BL.Repositories;
using WebAppRestaurante.Common.Security;
using WebAppRestaurante.Models.Entities.Users;

namespace WebAppRestaurante.BL.Services
{
    public interface IAuthService {
        // Métodos de autenticación básica
        Task<UserModel> GetUserByLogin(string username, string password);
        Task<UserModel> CreateUser(UserModel user, string password);
        Task<bool> ChangePassword(int userId, string currentPassword, string newPassword);

        // Métodos de gestión de tokens
        Task AddRefreshTokenModel(RefreshTokenModel refreshTokenModel);
        Task<RefreshTokenModel> GetRefreshTokenModel(string refreshToken);

        // Métodos de gestión de usuarios
        Task<UserModel> GetUserById(int id);
        Task<List<UserModel>> GetAllUsers();
        Task UpdateUser(UserModel user);
        Task<bool> DeactivateUser(int userId);
        Task<bool> UpdateLastLogin(int userId);

        // Métodos de gestión de roles
        Task<bool> UpdateUserRoles(int userId, List<int> roleIds);

    }

    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IAuthRepository authRepository, ILogger<AuthService> logger)
        {
            _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Implementación de métodos de autenticación básica
        public async Task<UserModel> GetUserByLogin(string username, string password)
        {
            try
            {
                var user = await _authRepository.GetUserByUsername(username);
                if (user != null && PasswordHasher.VerifyPassword(password, user.PasswordHash))
                {
                    await _authRepository.UpdateLastLogin(user.ID);
                    return user;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetUserByLogin para el usuario {Username}", username);
                throw;
            }
        }

        public async Task<UserModel> CreateUser(UserModel user, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(password))
                    throw new ArgumentException("La contraseña no puede estar vacía");

                user.PasswordHash = PasswordHasher.HashPassword(password);
                return await _authRepository.CreateUser(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario {Username}", user.Username);
                throw;
            }
        }

        public async Task<bool> ChangePassword(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _authRepository.GetUserById(userId);
                if (user != null && PasswordHasher.VerifyPassword(currentPassword, user.PasswordHash))
                {
                    user.PasswordHash = PasswordHasher.HashPassword(newPassword);
                    await _authRepository.UpdateUser(user);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña para el usuario {UserId}", userId);
                throw;
            }
        }

        // Implementación de métodos de gestión de tokens
        public async Task AddRefreshTokenModel(RefreshTokenModel refreshTokenModel)
        {
            try
            {
                await _authRepository.RemoveRefreshTokenByUserID(refreshTokenModel.UserID);
                await _authRepository.AddRefreshTokenModel(refreshTokenModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar refresh token para el usuario {UserId}", refreshTokenModel.UserID);
                throw;
            }
        }

        public async Task<RefreshTokenModel> GetRefreshTokenModel(string refreshToken)
        {
            try
            {
                return await _authRepository.GetRefreshTokenModel(refreshToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener refresh token");
                throw;
            }
        }

        // Implementación de métodos de gestión de usuarios
        public async Task<UserModel> GetUserById(int id)
        {
            try
            {
                return await _authRepository.GetUserById(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por ID {UserId}", id);
                throw;
            }
        }

        public async Task<List<UserModel>> GetAllUsers()
        {
            try
            {
                return await _authRepository.GetAllUsers();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los usuarios");
                throw;
            }
        }

        public async Task UpdateUser(UserModel user)
        {
            try
            {
                await _authRepository.UpdateUser(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario {UserId}", user.ID);
                throw;
            }
        }

        public async Task<bool> DeactivateUser(int userId)
        {
            try
            {
                return await _authRepository.DeactivateUser(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar usuario {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UpdateLastLogin(int userId)
        {
            try
            {
                return await _authRepository.UpdateLastLogin(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar último login para usuario {UserId}", userId);
                throw;
            }
        }

        // Implementación de métodos de gestión de roles
        public async Task<bool> UpdateUserRoles(int userId, List<int> roleIds)
        {
            try
            {
                return await _authRepository.UpdateUserRoles(userId, roleIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar roles para usuario {UserId}", userId);
                throw;
            }
        }
    }
}
