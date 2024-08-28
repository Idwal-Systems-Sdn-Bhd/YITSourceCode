using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT._DataAccess.Data;

namespace YIT.Tests.DataAccess.Repositories
{
    public interface AkPVRepositoryISeeds
    {
        Task GetDbContext(ApplicationDbContext context);
    }
}
