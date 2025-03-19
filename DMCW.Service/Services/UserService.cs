using AutoMapper;
using DMCW.Repository.Data.DataService;
using DMCW.Repository.Data.Entities.User;
using DMCW.Service.Helper;
using DMCW.ServiceInterface.Dtos.User.KlzTEch.Service.Interface.Dto;
using DMCW.ServiceInterface.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace DMCW.Service.Services
{



    public class UserService : IUserService
    {
        private readonly MongoDBContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public UserService(MongoDBContext mongoDBContext, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IMapper mapper)
        {
            _context = mongoDBContext;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _mapper = mapper;
        }

        public async Task<UserServiceDto> GetUserByEmail()
        {
            var userEmail = Utility.GetUserEmailFromClaims(_httpContextAccessor);

            if (string.IsNullOrEmpty(userEmail))
            {
                throw new InvalidOperationException("User email claim not found.");
            }

            var filter = Builders<User>.Filter.Eq("Email", userEmail);
            var existingUser = await _context.Users.Find(filter).FirstOrDefaultAsync();

            if (existingUser != null)
            {
                return _mapper.Map<UserServiceDto>(existingUser);
            }

            var newUser = await CreateNewUserAsync(userEmail);
            return _mapper.Map<UserServiceDto>(newUser);
        }

        public async Task<UserServiceDto> GetUserByUserId(HttpContext context)
        {
            if (context.Items.TryGetValue("ClientId", out var clientId))
            {
                if (string.IsNullOrEmpty((string?)clientId))
                {
                    throw new InvalidOperationException("User ID claim not found.");
                }

                var filter = Builders<User>.Filter.Eq("ClientId", clientId);
                var existingUser = await _context.Users.Find(filter).FirstOrDefaultAsync();

                if (existingUser != null)
                {
                    return _mapper.Map<UserServiceDto>(existingUser);
                }
                else
                {
                    throw new Exception("User record not found");
                }
            }
            throw new Exception("User record not found");
        }

        public async Task<bool> UpdateUserProfile(UserProfileUpdateDto profileUpdate)
        {
            var userEmail = Utility.GetUserEmailFromClaims(_httpContextAccessor);

            if (string.IsNullOrEmpty(userEmail))
            {
                throw new InvalidOperationException("User email claim not found.");
            }

            var filter = Builders<User>.Filter.Eq("Email", userEmail);
            var update = Builders<User>.Update
                .Set(u => u.Name, profileUpdate.Name)
                .Set(u => u.Phone, profileUpdate.Phone)
                .Set(u => u.Address, _mapper.Map<Address>(profileUpdate.Address))
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _context.Users.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateUserRole(string userId, string newRole)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty");
            }

            if (newRole != "Customer" && newRole != "Seller" && newRole != "Admin")
            {
                throw new ArgumentException("Invalid role. Role must be Customer, Seller, or Admin.");
            }

            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update
                .Set(u => u.UserRole, newRole)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _context.Users.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeactivateUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty");
            }

            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update
                .Set(u => u.IsActive, false)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _context.Users.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateSellerProfile(string userId, SellerProfileUpdateDto sellerProfile)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty");
            }

            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update
                .Set(u => u.StoreName, sellerProfile.StoreName)
                .Set(u => u.StoreDescription, sellerProfile.StoreDescription)
                .Set(u => u.BusinessHours, _mapper.Map<Dictionary<string, BusinessHours>>(sellerProfile.BusinessHours))
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _context.Users.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> VerifySeller(string userId, bool isVerified)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty");
            }

            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update
                .Set(u => u.IsVerifiedSeller, isVerified)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _context.Users.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        #region Private Methods
        private async Task<User> CreateNewUserAsync(string userEmail)
        {
            int newUserId = await Utility.GetNextSequenceValue("userid", null, _context);
            var newUser = new User
            {
                ClientId = newUserId.ToString(),
                Email = userEmail,
                UserRole = "Customer",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _context.Users.InsertOneAsync(newUser);
            return newUser;
        }
        #endregion
    }

}
