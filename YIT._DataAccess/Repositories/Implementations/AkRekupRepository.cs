using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class AkRekupRepository : _GenericRepository<AkRekup>, IAkRekupRepository
    {
        private readonly ApplicationDbContext _context;

        public AkRekupRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public List<AkRekup> GetAllDetails()
        {
            return _context.AkRekup
                .Include(r => r.DPanjar)
                    .ThenInclude(p => p!.JCawangan)
                .ToList();
        }
        public AkRekup GetDetailsById(int id)
        {
            return _context.AkRekup
                .Include(r => r.DPanjar)
                    .ThenInclude(p => p!.JCawangan)
                .FirstOrDefault(r => r.Id == id) ?? new AkRekup();
        }


    }
}