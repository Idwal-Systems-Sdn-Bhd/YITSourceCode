using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class DPenyemakRepository : _GenericRepository<DPenyemak>, IDPenyemakRepository
    {
        private readonly ApplicationDbContext _context;

        public DPenyemakRepository(ApplicationDbContext context) : base(context) 
        {
            _context = context;
        }

        public List<DPenyemak> GetAllDetails()
        {
            return _context.DPenyemak.Include(p => p.DPekerja).ToList();
        }

        public DPenyemak GetAllDetailsById(int id)
        {
            return _context.DPenyemak.Include(p => p.DPekerja).FirstOrDefault(p => p.Id == id) ?? new DPenyemak();
        }
    }
}