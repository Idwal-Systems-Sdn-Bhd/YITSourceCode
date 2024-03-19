using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities.Models._02Daftar;
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

        public List<AkRekup> GetAllFilteredBy(bool isLinked)
        {
            return _context.AkRekup.Include(r => r.DPanjar).Where(r => r.IsLinked == isLinked).ToList();
        }

        public AkRekup GetDetailsById(int id)
        {
            return _context.AkRekup
                .Include(r => r.DPanjar)
                    .ThenInclude(p => p!.JCawangan)
                .Include(r => r.DPanjar)
                    .ThenInclude(p => p!.DPanjarPemegang)!
                        .ThenInclude(pp => pp.DPekerja)
                .FirstOrDefault(r => r.Id == id) ?? new AkRekup();
        }

        public List<AkRekup> GetAllExceptBakiAwalByDPanjarId(int dPanjarId)
        {
            return _context.AkRekup
                .Where(r => r.NoRujukan != "BAKI AWAL" && r.DPanjarId == dPanjarId).ToList();
        }

        public AkRekup GetDetailsLastByDPanjarId(int dPanjarId)
        {
            var rekup = new AkRekup();

            List<AkRekup> rekupList = _context.AkRekup.Where(r => r.DPanjarId == dPanjarId && r.NoRujukan != "BAKI AWAL").OrderBy(r => r.NoRujukan).ToList();

            if (rekupList.Any())
            {
                foreach (var item in rekupList) rekup = item;
            }
            return rekup;
        }

        public AkRekup GetDetailsByBakiAwalAndDPanjarId(string noRujukan, int dPanjarId, bool isLinked)
        {
            return _context.AkRekup
                .Include(r => r.DPanjar)
                    .ThenInclude(p => p!.JCawangan)
                .FirstOrDefault(r => r.NoRujukan == noRujukan && r.DPanjarId == dPanjarId && r.IsLinked == isLinked) ?? new AkRekup();
        }

        public string GetMaxRefNoByDPanjarId(string initNoRujukan, int dPanjarId)
        {
            return _context.AkRekup.Where(r => r.DPanjarId == dPanjarId && r.NoRujukan != "BAKI AWAL").Max(r => r.NoRujukan)?.Substring(5,4) ?? "";
        }
    }
}