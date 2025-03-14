namespace DMCW.API.Dtos.Configuration
{
    public class AuthenticationSettings
    {
        public GoogleSettings Google { get; set; }
    }

    public class GoogleSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string MobileRedictUrl { get; set; }
        public string WebRedirectUrl { get; set; }
        public string WebRedirectLocalUrl { get; set; }
        public string APIRedirectURL { get; set; }
    }
}
