using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class DPelulusRepository : _GenericRepository<DPelulus>, IDPelulusRepository
    {
        private readonly ApplicationDbContext _context;

        public DPelulusRepository(ApplicationDbContext context) :base(context)
        {
            _context = context;
        }

        public List<DPelulus> GetAllDetails()
        {
            return _context.DPelulus.Include(p => p.DPekerja).ToList();
        }

        public DPelulus GetAllDetailsById(int id)
        {
            return _context.DPelulus.Include(p => p.DPekerja).FirstOrDefault(p => p.Id == id) ?? new DPelulus();
        }
    }
}