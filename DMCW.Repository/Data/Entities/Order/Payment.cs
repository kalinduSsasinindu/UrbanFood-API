using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCW.Repository.Data.Entities.Order
{
    public class Payment
    {
        public string PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
