namespace DMCW.API.Dtos
{
    public class GoogleTokenResponse
    {
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }
        public string RefreshToken { get; set; }
        public string Scope { get; set; }
        public string TokenType { get; set; }
        public string IdToken { get; set; }
    }

}
