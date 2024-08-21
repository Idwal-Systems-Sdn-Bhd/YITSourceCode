using YIT.__Domain.Entities.Models._50LHDN;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class LHDNMSICRepository : _GenericRepository<LHDNMSIC>, ILHDNMSICRepository
    {
        public LHDNMSICRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}