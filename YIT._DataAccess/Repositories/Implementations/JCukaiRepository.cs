using YIT.__Domain.Entities.Models._01Jadual;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class JCukaiRepository : _GenericRepository<JCukai>, IJCukaiRepository
    {
        public JCukaiRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}