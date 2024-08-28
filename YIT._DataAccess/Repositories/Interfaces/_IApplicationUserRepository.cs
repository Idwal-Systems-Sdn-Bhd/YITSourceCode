using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Administrations;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface _IApplicationUserRepository
    {
        ApplicationUser GetApplicationUser(string id);
    }
}
