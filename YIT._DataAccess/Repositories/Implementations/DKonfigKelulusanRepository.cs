using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class DKonfigKelulusanRepository : _GenericRepository<DKonfigKelulusan>, IDKonfigKelulusanRepository
    {
        private readonly ApplicationDbContext _context;

        public DKonfigKelulusanRepository(ApplicationDbContext context) : base(context) 
        {
            _context = context;
        }

        public List<DKonfigKelulusan> GetAllDetails()
        {
            return _context.DKonfigKelulusan.Include(p => p.DPekerja).ToList();
        }

        public DKonfigKelulusan GetAllDetailsById(int id)
        {
            return _context.DKonfigKelulusan.Include(p => p.DPekerja).FirstOrDefault(p => p.Id == id) ?? new DKonfigKelulusan();
        }
    }
}