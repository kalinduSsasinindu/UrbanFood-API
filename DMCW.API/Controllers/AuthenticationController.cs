using DMCW.API.Dtos;
using DMCW.API.Dtos.Configuration;
using DMCW.API.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DMCW.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AuthenticationSettings _authSettings;

        public AuthenticationController(IHttpClientFactory httpClientFactory, IOptions<AuthenticationSettings> authSettings)
        {
            _httpClientFactory = httpClientFactory;
            _authSettings = authSettings.Value;
        }

        [Authorize]
        [HttpGet("login")]
        public IActionResult Login()
        {
            var clientId = _authSettings.Google.ClientId;
            var redirectUri = _authSettings.Google.WebRedirectUrl;

            var codeVerifier = PkceHelper.GenerateCodeVerifier();
            var codeChallenge = PkceHelper.GenerateCodeChallenge(codeVerifier);

            var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                          $"response_type=code&" +
                          $"client_id={clientId}&" +
                          $"redirect_uri={redirectUri}&" +
                          $"scope=openid%20profile%20email&" +
                          $"code_challenge={codeChallenge}" +
                          $"code_challenge_method=S256";

            return Redirect(authUrl);
        }

        [HttpPost("exchange-code")]
        public async Task<IActionResult> ExchangeCodeForToken([FromBody] ExchangeCodeRequest request)
        {
            var codeVerifier = request.CodeVerifier; // Get from request
            var authCode = request.AuthCode; // Get from request

            var clientId = _authSettings.Google.ClientId;

            //var redirectUri = string.Empty;       
            //if (HttpContext.Connection.LocalIpAddress != null) {
            //    redirectUri = _authSettings.Google.WebRedirectLocalUrl;
            //}
            //else
            //{
            var redirectUri = _authSettings.Google.WebRedirectUrl;
            // } 

            var clientSecret = _authSettings.Google.ClientSecret;

            if (request.IsMobile)
            {
                redirectUri = _authSettings.Google.MobileRedictUrl;
            }

            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token");
            var requestBody = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", authCode),
            new KeyValuePair<string, string>("redirect_uri", redirectUri),
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("code_verifier", codeVerifier),
            new KeyValuePair<string, string>("client_secret", clientSecret)
        };

            tokenRequest.Content = new FormUrlEncodedContent(requestBody);
            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(tokenRequest);

            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = await response.Content.ReadAsStringAsync();
                return Ok(tokenResponse);
            }

            return BadRequest("Token exchange failed");
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var clientId = _authSettings.Google.ClientId;
            var clientSecret = _authSettings.Google.ClientSecret;

            var redirectUri = request.IsMobile ? _authSettings.Google.MobileRedictUrl : _authSettings.Google.WebRedirectUrl;

            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token");
            var requestBody = new List<KeyValuePair<string, string>>
    {
        new KeyValuePair<string, string>("grant_type", "refresh_token"),
        new KeyValuePair<string, string>("refresh_token", request.RefreshToken),
        new KeyValuePair<string, string>("client_id", clientId),
        new KeyValuePair<string, string>("client_secret", clientSecret)
    };

            tokenRequest.Content = new FormUrlEncodedContent(requestBody);
            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(tokenRequest);

            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = await response.Content.ReadAsStringAsync();
                return Ok(tokenResponse);
            }

            return BadRequest("Token refresh failed");
        }
    }

}
