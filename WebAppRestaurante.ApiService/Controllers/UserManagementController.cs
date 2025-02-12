using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAppRestaurante.BL.Services;
using WebAppRestaurante.Models.Models;

namespace WebAppRestaurante.ApiService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {

        private readonly IUserService _userService;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(IUserService userService, ILogger<UserManagementController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Obtener todos los usuarios (Solo SuperAdmin y Admin)
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet]
        public async Task<ActionResult<BaseResponseModel>> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                
                return Ok(new BaseResponseModel { Success = true, Data = users });


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
    }
}
