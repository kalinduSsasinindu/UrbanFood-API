using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCW.ServiceInterface.Dtos
{
    public class MediaServiceDto
    {
        public string productId { get; set; }
        public List<string> newMediaBase64 { get; set; }
        public List<MediaUpdateRequestServiceDto> mediaUpdates { get; set; }
    }
}
