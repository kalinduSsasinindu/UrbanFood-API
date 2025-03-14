using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace DMCW.API.Middleware
{
    public class GoogleTokenAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GoogleTokenAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IHttpClientFactory httpClientFactory)
            : base(options, logger, encoder)
        {
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Authorization header not found.");

            var authHeader = Request.Headers["Authorization"].ToString();
            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return AuthenticateResult.Fail("Bearer token not found.");

            var token = authHeader.Substring("Bearer ".Length).Trim();

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync($"https://oauth2.googleapis.com/tokeninfo?access_token={token}");

                if (!response.IsSuccessStatusCode)
                    return AuthenticateResult.Fail("Invalid token.");

                var content = await response.Content.ReadAsStringAsync();
                var tokenInfo = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, tokenInfo["sub"].GetString()),
                    new Claim(ClaimTypes.Email, tokenInfo["email"].GetString()),
                    // Add more claims as needed
                };

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail($"Authentication failed: {ex.Message}");
            }
        }
    }
}
