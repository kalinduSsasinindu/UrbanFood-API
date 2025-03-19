using Microsoft.AspNetCore.Http;

namespace DMCW.Repository.Helper
{
    internal class Utility
    {
        public static string GetUserIdFromClaims(IHttpContextAccessor httpContextAccessor)
        {
            var clientId = httpContextAccessor.HttpContext.Items.TryGetValue("ClientId", out var userId) ? userId as string : null;
            return clientId;
        }
    }
}
