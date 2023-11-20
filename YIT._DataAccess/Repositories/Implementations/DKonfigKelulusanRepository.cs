using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Enums;
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
            return _context.DKonfigKelulusan.Include(p => p.DPekerja).Include(p => p.JBahagian).ToList();
        }

        public DKonfigKelulusan GetAllDetailsById(int id)
        {
            return _context.DKonfigKelulusan.Include(p => p.DPekerja).Include(p => p.JBahagian).FirstOrDefault(p => p.Id == id) ?? new DKonfigKelulusan();
        }
        public List<DKonfigKelulusan> GetResultsByKategori(EnKategoriKelulusan enKategoriKelulusan)
        {
            return _context.DKonfigKelulusan.Where(p => p.EnKategoriKelulusan == enKategoriKelulusan).ToList();
        }
    }
}