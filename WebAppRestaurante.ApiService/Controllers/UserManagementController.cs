using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAppRestaurante.BL.Services;
using WebAppRestaurante.Models.Entities.Users;
using WebAppRestaurante.Models.Models.User;
using WebAppRestaurante.Models.Models;

namespace WebAppRestaurante.ApiService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(IAuthService authService, ILogger<UserManagementController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Obtener todos los usuarios (Solo SuperAdmin y Admin)
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet]
        public async Task<ActionResult<BaseResponseModel>> GetAllUsers()
        {
            try
            {
                var users = await _authService.GetAllUsers();
                return Ok(new BaseResponseModel
                {
                    Success = true,
                    Data = users.Select(u => new
                    {
                        u.ID,
                        u.Username,
                        u.FirstName,
                        u.LastName,
                        u.Email,
                        u.IsActive,
                        u.CreatedAt,
                        u.LastLogin,
                        Roles = u.UserRoles.Select(ur => ur.Role.RoleName).ToList()
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Internal server error while getting users"
                });
            }
        }

        // Crear nuevo usuario (Solo SuperAdmin)
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public async Task<ActionResult<BaseResponseModel>> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                // Validaciones básicas
                if (string.IsNullOrWhiteSpace(request.Password))
                    return BadRequest(new BaseResponseModel { Success = false, ErrorMessage = "Password is required" });

                // Crear el nuevo usuario
                var newUser = new UserModel
                {
                    Username = request.Username,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                // Crear usuario y asignar roles
                var createdUser = await _authService.CreateUser(newUser, request.Password);
                if (request.RoleIds.Any())
                {
                    await _authService.UpdateUserRoles(createdUser.ID, request.RoleIds);
                }

                return Ok(new BaseResponseModel { Success = true, Data = createdUser });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new BaseResponseModel { Success = false, ErrorMessage = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Internal server error while creating user"
                });
            }
        }

        // Actualizar usuario existente
        [Authorize(Roles = "SuperAdmin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<BaseResponseModel>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var user = await _authService.GetUserById(id);
                if (user == null)
                    return NotFound(new BaseResponseModel { Success = false, ErrorMessage = "User not found" });

                // Actualizar propiedades
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.Email = request.Email;

                await _authService.UpdateUser(user);

                // Actualizar roles si se proporcionaron
                if (request.RoleIds.Any())
                {
                    await _authService.UpdateUserRoles(id, request.RoleIds);
                }

                return Ok(new BaseResponseModel { Success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Internal server error while updating user"
                });
            }
        }

        // Desactivar usuario
        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<BaseResponseModel>> DeactivateUser(int id)
        {
            try
            {
                var result = await _authService.DeactivateUser(id);
                if (!result)
                    return NotFound(new BaseResponseModel { Success = false, ErrorMessage = "User not found" });

                return Ok(new BaseResponseModel { Success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user {UserId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Internal server error while deactivating user"
                });
            }
        }

        // Cambiar contraseña (usuario actual)
        [HttpPut("change-password")]
        public async Task<ActionResult<BaseResponseModel>> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                // Obtener el ID del usuario actual del token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    return BadRequest(new BaseResponseModel { Success = false, ErrorMessage = "Invalid user identification" });

                // Validar que las contraseñas coincidan
                if (request.NewPassword != request.ConfirmPassword)
                    return BadRequest(new BaseResponseModel { Success = false, ErrorMessage = "Passwords do not match" });

                var result = await _authService.ChangePassword(userId, request.CurrentPassword, request.NewPassword);
                if (!result)
                    return BadRequest(new BaseResponseModel { Success = false, ErrorMessage = "Current password is incorrect" });

                return Ok(new BaseResponseModel { Success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user");
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Internal server error while changing password"
                });
            }
        }

        // Obtener usuario por ID
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseModel>> GetUserById(int id)
        {
            try
            {
                var user = await _authService.GetUserById(id);
                if (user == null)
                    return NotFound(new BaseResponseModel { Success = false, ErrorMessage = "User not found" });

                return Ok(new BaseResponseModel
                {
                    Success = true,
                    Data = new
                    {
                        user.ID,
                        user.Username,
                        user.FirstName,
                        user.LastName,
                        user.Email,
                        user.IsActive,
                        user.CreatedAt,
                        user.LastLogin,
                        Roles = user.UserRoles.Select(ur => ur.Role.RoleName).ToList()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}", id);
                return StatusCode(500, new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Internal server error while getting user"
                });
            }
        }
    }
}

