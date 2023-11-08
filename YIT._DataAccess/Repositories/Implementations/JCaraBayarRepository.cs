using YIT.__Domain.Entities.Models._01Jadual;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    internal class JCaraBayarRepository : _GenericRepository<JCaraBayar>, IJCaraBayarRepository
    {
        public JCaraBayarRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}