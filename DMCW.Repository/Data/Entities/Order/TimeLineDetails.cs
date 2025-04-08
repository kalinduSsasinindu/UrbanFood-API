using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCW.Repository.Data.Entities.Order
{
    public class TimeLineDetails
    {
        public DateTime CreatedAt { get; set; }
        public string Comment { get; set; }
        public List<string> Images { get; set; }
        public List<string> ImgUrls { get; set; }
    }
}
