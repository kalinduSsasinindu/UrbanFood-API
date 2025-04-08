using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCW.Repository.Data.Entities.Order
{
    public class PaymentInfo
    {
        public List<Payment> Payments { get; set; }

        public decimal TotalPaidAmount => Payments?.Sum(payment => payment.Amount) ?? 0;

    }
}
