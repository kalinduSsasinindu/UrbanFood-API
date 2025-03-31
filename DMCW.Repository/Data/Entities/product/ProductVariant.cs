using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCW.Repository.Data.Entities.product
{
    public class ProductVariant
    {
        public int VariantId { get; set; }
        public string SKU { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public int? AvailableQuantity { get; set; }
        public int? CommittedQuantity { get; set; }
        public bool IsActive { get; set; }
        public int? OnHandQuantity
        {
            get
            {
                return (AvailableQuantity ?? 0) + (CommittedQuantity ?? 0);
            }
        }
    }
}
