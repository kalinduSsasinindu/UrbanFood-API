using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCW.ServiceInterface.Dtos
{
    public class MediaUpdateRequestServiceDto
    {
        public string Url { get; set; }
        public bool IsDeleted { get; set; }
    }
}
