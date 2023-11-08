using YIT.__Domain.Entities.Models._01Jadual;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class JAgamaRepository : _GenericRepository<JAgama>, IJAgamaRepository
    {
        private readonly ApplicationDbContext _context;

        public JAgamaRepository(ApplicationDbContext context) : base(context) 
        {
            _context = context;
        }
    }
}