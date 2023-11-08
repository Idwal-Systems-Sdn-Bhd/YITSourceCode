using YIT.__Domain.Entities.Models._01Jadual;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class JBankRepository : _GenericRepository<JBank>, IJBankRepository
    {
        public JBankRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}