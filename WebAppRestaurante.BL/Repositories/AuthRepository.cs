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
    public interface IAuthRepository {
        Task<UserModel> GetUserByUsername(string username);
        Task<UserModel> GetUserById(int id);
        Task<UserModel> CreateUser(UserModel user);
        Task UpdateUser(UserModel user);
        Task<RefreshTokenModel> GetRefreshTokenModel(string refreshToken);
        Task AddRefreshTokenModel(RefreshTokenModel refreshTokenModel);
        Task RemoveRefreshTokenByUserID(int userID);
        Task<bool> UpdateLastLogin(int userId);
        Task<List<UserModel>> GetAllUsers();
        Task<bool> DeactivateUser(int userId);
        Task<bool> UpdateUserRoles(int userId, List<int> roleIds);
    }
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _appDbContext;

        public AuthRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<UserModel> GetUserByUsername(string username)
        {
            // Obtenemos el usuario incluyendo sus roles y la información del rol
            return await _appDbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
        }

        public async Task<UserModel> GetUserById(int id)
        {
            // Obtenemos el usuario por ID incluyendo sus roles
            return await _appDbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.ID == id && u.IsActive);
        }

        public async Task<UserModel> CreateUser(UserModel user)
        {
            // Verificamos si ya existe un usuario con el mismo username o email
            if (await _appDbContext.Users.AnyAsync(u => u.Username == user.Username))
            {
                throw new InvalidOperationException("Username already exists");
            }

            if (await _appDbContext.Users.AnyAsync(u => u.Email == user.Email))
            {
                throw new InvalidOperationException("Email already exists");
            }

            // Establecemos los valores por defecto
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;

            // Agregamos el usuario a la base de datos
            await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            return user;
        }

        public async Task UpdateUser(UserModel user)
        {
            // Verificamos si existe otro usuario con el mismo email (excluyendo el usuario actual)
            if (await _appDbContext.Users.AnyAsync(u => u.Email == user.Email && u.ID != user.ID))
            {
                throw new InvalidOperationException("Email already exists");
            }

            // Actualizamos el usuario
            _appDbContext.Entry(user).State = EntityState.Modified;

            // No actualizamos la contraseña ni la fecha de creación
            _appDbContext.Entry(user).Property(x => x.PasswordHash).IsModified = false;
            _appDbContext.Entry(user).Property(x => x.CreatedAt).IsModified = false;

            await _appDbContext.SaveChangesAsync();
        }

        public async Task<RefreshTokenModel> GetRefreshTokenModel(string refreshToken)
        {
            return await _appDbContext.RefreshTokens
                .Include(n => n.User)
                .ThenInclude(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(n => n.RefreshToken == refreshToken);
        }

        public async Task AddRefreshTokenModel(RefreshTokenModel refreshTokenModel)
        {
            await _appDbContext.RefreshTokens.AddAsync(refreshTokenModel);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task RemoveRefreshTokenByUserID(int userID)
        {
            var tokens = await _appDbContext.RefreshTokens
                .Where(n => n.UserID == userID)
                .ToListAsync();

            if (tokens.Any())
            {
                _appDbContext.RefreshTokens.RemoveRange(tokens);
                await _appDbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> UpdateLastLogin(int userId)
        {
            var user = await _appDbContext.Users.FindAsync(userId);
            if (user != null)
            {
                user.LastLogin = DateTime.UtcNow;
                await _appDbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<UserModel>> GetAllUsers()
        {
            return await _appDbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.IsActive)
                .OrderBy(u => u.Username)
                .ToListAsync();
        }

        public async Task<bool> DeactivateUser(int userId)
        {
            var user = await _appDbContext.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsActive = false;
                await _appDbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateUserRoles(int userId, List<int> roleIds)
        {
            var user = await _appDbContext.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.ID == userId);

            if (user == null)
                return false;

            // Eliminamos los roles actuales
            _appDbContext.UserRoles.RemoveRange(user.UserRoles);

            // Agregamos los nuevos roles
            foreach (var roleId in roleIds)
            {
                user.UserRoles.Add(new UserRolModel
                {
                    UserID = userId,
                    RoleID = roleId
                });
            }

            await _appDbContext.SaveChangesAsync();
            return true;
        }
    }
}
