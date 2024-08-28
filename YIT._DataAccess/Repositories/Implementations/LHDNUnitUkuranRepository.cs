using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities.Models._50LHDN;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class LHDNUnitUkuranRepository : _GenericRepository<LHDNUnitUkuran>, ILHDNUnitUkuranRepository
    {
        private readonly ApplicationDbContext _context;

        public LHDNUnitUkuranRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<LHDNUnitUkuran> GetByCodeAsync(string code)
        {
            return await _context.LHDNUnitUkuran.FirstOrDefaultAsync(uu => uu.Code == code) ?? new LHDNUnitUkuran();
        }
    }
}