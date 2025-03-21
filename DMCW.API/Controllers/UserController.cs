using DMCW.ServiceInterface.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMCW.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public UserController(ILogger<UserController> logger, IUserService userService, IConfiguration configuration)
        {
            _logger = logger;
            _userService = userService;
            _configuration = configuration;
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var user = await _userService.GetUserByEmail();
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile");
                return StatusCode(500, "An error occurred while retrieving user profile");
            }
        }

       /* [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileUpdateDto profile)
        {
            try
            {
                var success = await _userService.UpdateUserProfile(profile);
                return success
                    ? Ok(new { Message = "Profile updated successfully" })
                    : StatusCode(500, "Failed to update profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return StatusCode(500, "An error occurred while updating user profile");
            }
        }
       */
        [Authorize]
        [HttpPost("become-seller")]
        public async Task<IActionResult> BecomeSeller()
        {
            try
            {
                var user = await _userService.GetUserByEmail();
                if (user.UserRole == "Seller")
                {
                    return BadRequest("User is already a seller");
                }

                var success = await _userService.UpdateUserRole(user.Id, "Seller");
                return success
                    ? Ok(new { Message = "Successfully updated to seller role" })
                    : StatusCode(500, "Failed to update user role");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user to seller role");
                return StatusCode(500, "An error occurred while updating user role");
            }
        }
        /*
                [Authorize]
                [HttpPut("seller-profile")]
                public async Task<IActionResult> UpdateSellerProfile([FromBody] SellerProfileUpdateDto sellerProfile)
                {
                    try
                    {
                        var user = await _userService.GetUserByEmail();
                        if (user.UserRole != "Seller")
                        {
                            return BadRequest("User is not a seller");
                        }

                        var success = await _userService.UpdateSellerProfile(user.Id, sellerProfile);
                        return success
                            ? Ok(new { Message = "Seller profile updated successfully" })
                            : StatusCode(500, "Failed to update seller profile");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating seller profile");
                        return StatusCode(500, "An error occurred while updating seller profile");
                    }
                }
        
              [Authorize(Roles = "Admin")]
                [HttpPut("verify-seller/{userId}")]
                public async Task<IActionResult> VerifySeller(string userId, [FromQuery] bool isVerified)
                {
                    try
                    {
                        var success = await _userService.VerifySeller(userId, isVerified);
                        return success
                            ? Ok(new { Message = $"Seller verification status updated to {isVerified}" })
                            : StatusCode(500, "Failed to update seller verification status");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating seller verification status");
                        return StatusCode(500, "An error occurred while updating seller verification status");
                    }
                }*/



        [Authorize(Roles = "Admin")]
        [HttpPut("deactivate/{userId}")]
        public async Task<IActionResult> DeactivateUser(string userId)
        {
            try
            {
                var success = await _userService.DeactivateUser(userId);
                return success
                    ? Ok(new { Message = "User deactivated successfully" })
                    : StatusCode(500, "Failed to deactivate user");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user");
                return StatusCode(500, "An error occurred while deactivating user");
            }
        }
    }
}

