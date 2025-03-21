
using DMCW.ServiceInterface.Dtos.User;
using Microsoft.AspNetCore.Http;


namespace DMCW.ServiceInterface.Interfaces
{

    public interface IUserService
    {
        // Core user methods
        Task<UserServiceDto> GetUserByEmail();
        Task<UserServiceDto> GetUserByUserId(HttpContext context);

        // Account management
        //Task<bool> UpdateUserProfile(UserProfileUpdateDto profileUpdate);
        Task<bool> UpdateUserRole(string userId, string newRole);
        Task<bool> DeactivateUser(string userId);

        // Seller-specific methods
        //Task<bool> UpdateSellerProfile(string userId, SellerProfileUpdateDto sellerProfile);
        //Task<bool> VerifySeller(string userId, bool isVerified);
    }
}

