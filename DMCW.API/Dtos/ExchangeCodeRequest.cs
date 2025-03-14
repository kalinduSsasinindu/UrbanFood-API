namespace DMCW.API.Dtos
{
    public class ExchangeCodeRequest
    {
        public string AuthCode { get; set; }
        public string CodeVerifier { get; set; }
        public bool IsMobile { get; set; }
    }
}
