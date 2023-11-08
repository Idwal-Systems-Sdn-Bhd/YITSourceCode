using YIT.__Domain.Entities.Models._01Jadual;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class JBangsaRepository : _GenericRepository<JBangsa>, IJBangsaRepository
    {
        public JBangsaRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}