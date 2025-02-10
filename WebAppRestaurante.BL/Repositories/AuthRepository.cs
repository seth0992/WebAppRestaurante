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
        Task<UserModel> GetUserByLogin(string username, string password);

        Task AddRefreshTokenModel(RefreshTokenModel refreshTokenModel);
        Task RemoveRefreshTokenByUserID(int userID);
        Task<RefreshTokenModel> GetRefreshTokenModel(string refreshToken);
    }
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _appDbContext;

        public AuthRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task AddRefreshTokenModel(RefreshTokenModel refreshTokenModel)
        {
            await _appDbContext.RefreshTokens.AddAsync(refreshTokenModel);
            await _appDbContext.SaveChangesAsync(); 
        }

        public async Task<RefreshTokenModel> GetRefreshTokenModel(string refreshToken)
        {
            var newRefreshToken = await _appDbContext.RefreshTokens.Include(n => n.User).ThenInclude(n => n.UserRoles).ThenInclude(n => n.Role).FirstOrDefaultAsync(n => n.RefreshToken == refreshToken);

            return newRefreshToken;
        }

        public async Task<UserModel> GetUserByLogin(string username, string password)
        {
            return await _appDbContext.Users
                    .Include(n => n.UserRoles)
                    .ThenInclude(n => n.Role)
                    .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);
        }

        public async Task RemoveRefreshTokenByUserID(int userID)
        {
            var refreshToken = _appDbContext.RefreshTokens.FirstOrDefault(n => n.UserID == userID);
            if (refreshToken != null) { 
                _appDbContext.RemoveRange(refreshToken);
                await _appDbContext.SaveChangesAsync();
            }
        }
    }
}
