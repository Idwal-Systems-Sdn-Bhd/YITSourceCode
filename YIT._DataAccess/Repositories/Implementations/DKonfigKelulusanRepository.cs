using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._01Jadual;
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
            return _context.DKonfigKelulusan.Include(p => p.DPekerja)
                .Include(p => p.JBahagian)
                    .ToList();
        }

        public DKonfigKelulusan GetAllDetailsById(int id)
        {
            return _context.DKonfigKelulusan.Include(p => p.DPekerja).Include(p => p.JBahagian).FirstOrDefault(p => p.Id == id) ?? new DKonfigKelulusan();
        }

        public List<DKonfigKelulusan> GetResultsByCategoryGroupByDPekerja(EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModul)
        {
            var results = _context.DKonfigKelulusan
                 .Include(kk => kk.DPekerja)
                 .Include(kk => kk.JBahagian)
                .Where(b => b.EnKategoriKelulusan == enKategoriKelulusan)
                .Where(b => b.EnJenisModul == enJenisModul)
                .GroupBy(b => b.DPekerjaId).Select(l => new DKonfigKelulusan
            {
                    Id = l.First().DPekerjaId,
                DPekerjaId = l.First().DPekerjaId,
                DPekerja = l.First().DPekerja,
            }).ToList();

            return results ?? new List<DKonfigKelulusan>();
        }

        public bool IsPersonAvailable(EnJenisModulKelulusan enJenisModul, EnKategoriKelulusan enKategoriKelulusan, int jBahagianId, decimal jumlah)
        {
            return _context.DKonfigKelulusan.Any(kk => kk.EnJenisModul == enJenisModul && kk.EnKategoriKelulusan == enKategoriKelulusan && kk.JBahagianId == jBahagianId);
        }

        public bool IsValidUser(int dPekerjaId, string password, EnJenisModulKelulusan enJenisModul, EnKategoriKelulusan enKategoriKelulusan)
        {
            return _context.DKonfigKelulusan.Any(kk => kk.DPekerjaId == dPekerjaId && kk.KataLaluan == password && kk.EnJenisModul == enJenisModul && kk.EnKategoriKelulusan == enKategoriKelulusan);
        }
    }
}