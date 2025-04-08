using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCW.ServiceInterface.Dtos.product
{
    namespace DMCW.API.Dtos.Product
    {
        namespace DMCW.API.Dtos
        {
            public class CreateProductReviewDto
            {
                public int? Rating { get; set; }
                public string? Comment { get; set; }
                public List<string>? ReviewImages { get; set; }
            }

            public class ProductReviewDto
            {
                public string? Id { get; set; }
                public string? ProductId { get; set; }
                public string? ReviewerId { get; set; }
                public string? ReviewerName { get; set; }
                public string? ReviewerProfilePicture { get; set; }
                public int? Rating { get; set; }
                public string? Comment { get; set; }
                public List<string>? ReviewImages { get; set; }
                public int? LikesCount { get; set; }
                public bool? IsFeatured { get; set; }
                public DateTime? CreatedAt { get; set; }
                public bool? IsVerified { get; set; }
            }
        }
    }

}
