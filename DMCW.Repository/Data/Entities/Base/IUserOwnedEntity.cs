using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCW.Repository.Data.Entities.Base
{
    public interface IUserOwnedEntity
    {
        string ClientId { get; set; }
    }
}
