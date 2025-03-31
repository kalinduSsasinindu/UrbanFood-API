namespace DMCW.API.Dtos
{
    public class MediaWebDto
    {
        public string productId { get; set; }
        public List<string> newMediaBase64 { get; set; }
        public List<MediaUpdateRequestWebDto> mediaUpdates { get; set; }
    }
}
