using DMCW.Repository.Data.DataService;
using DMCW.Repository.Data.Entities;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DMCW.Service.Helper
{
    public static class Utility
    {
        public static async Task<int> GetNextSequenceValue(string sequenceName, string clientId, MongoDBContext _context)
        {
            var filter = string.IsNullOrEmpty(clientId) ? Builders<Sequence>.Filter.Eq("_id", $"{sequenceName}") : Builders<Sequence>.Filter.Eq("_id", $"{sequenceName}_{clientId}");
            var update = Builders<Sequence>.Update.Inc("sequence_value", 1);
            var options = new FindOneAndUpdateOptions<Sequence>
            {
                ReturnDocument = ReturnDocument.After,
                IsUpsert = true // Create the sequence document if it doesn't exist
            };

            var result = await _context.Sequences.FindOneAndUpdateAsync(filter, update, options);
            return result.SequenceValue;
        }

        public static string GetUserEmailFromClaims(IHttpContextAccessor httpContextAccessor)
        {
            var claimsIdentity = httpContextAccessor.HttpContext?.User.Identity as ClaimsIdentity;
            return claimsIdentity?.FindFirst(ClaimTypes.Email)?.Value;
        }
        public static string GetUserIdFromClaims(IHttpContextAccessor httpContextAccessor)
        {
            // Initialize userId as object type since TryGetValue uses out object parameter
            object userId = null;

            // Check if HttpContext is not null
            if (httpContextAccessor.HttpContext != null)
            {
                // Try to get the ClientId from HttpContext.Items
                if (httpContextAccessor.HttpContext.Items.TryGetValue("ClientId", out userId))
                {
                    // If found, return it as string
                    return userId as string;
                }
            }

            // Return null if not found or HttpContext is null
            return null;
        }

        public static decimal ConvertToDecimal(BsonDecimal128 number)
        {
            return Convert.ToDecimal(number.Value);
        }

        public static BsonDecimal128 ConvertToBsonDecimal(decimal number)
        {
            var value = new Decimal128(number);
            return new BsonDecimal128(value);
        }

    }
}
