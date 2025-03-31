using DMCW.Repository.Data.Entities.product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCW.Repository.Data.Entities.Search
{
    public class ProductSearchResponse
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public List<string> ImgUrls { get; set; }
        public List<ProductVariant> Variants { get; set; }


    }
}
