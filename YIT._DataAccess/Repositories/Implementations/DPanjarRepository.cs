using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class DPanjarRepository : _GenericRepository<DPanjar>, IDPanjarRepository
    {
        private readonly ApplicationDbContext _context;

        public DPanjarRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public List<DPanjar> GetResults(string? searchString, string? orderBy)
        {
            var result = new List<DPanjar>();

            if (searchString == null && orderBy == null)
            {
                return result;
            }
            else
            {
                result = _context.DPanjar
                    .IgnoreQueryFilters()
                    .Include(p => p.JCawangan)
                    .Include(p => p.JKWPTJBahagian)
                        .ThenInclude(p => p!.JKW)
                    .Include(p => p.JKWPTJBahagian)
                        .ThenInclude(p => p!.JPTJ)
                    .Include(p => p.JKWPTJBahagian)
                        .ThenInclude(p => p!.JBahagian)
                    .Include(p => p.AkCarta)
                    .Include(p => p.DPanjarPemegang)!
                        .ThenInclude(pp => pp.DPekerja)
                    .ToList();

                if (searchString != null) 
                {
                    result = result.Where(r => 
                    r.JCawangan!.Perihal!.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    r.AkCarta!.Kod!.ToString().Contains(searchString,StringComparison.OrdinalIgnoreCase) ||
                    r.AkCarta!.Perihal!.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    )
                        .ToList();
                }

                if (orderBy != null)
                {
                    switch (orderBy)
                    {
                        default:
                            result.OrderBy(r => r.JCawangan!.Kod).ToList(); break;
                    }
                }

                return result;
            }
        }
        public List<DPanjar> GetAllDetails()
        {
            return _context.DPanjar
                .Include(p => p.JCawangan)
                .Include(p => p.JKWPTJBahagian)
                        .ThenInclude(p => p!.JKW)
                    .Include(p => p.JKWPTJBahagian)
                        .ThenInclude(p => p!.JPTJ)
                    .Include(p => p.JKWPTJBahagian)
                        .ThenInclude(p => p!.JBahagian)
                .Include(p => p.AkCarta)
                .Include(p => p.DPanjarPemegang)!
                    .ThenInclude(pp => pp.DPekerja)
                .ToList();
        }

        public DPanjar GetAllDetailsById(int id)
        {
            return _context.DPanjar
                .Include(p => p.JCawangan)
                .Include(p => p.JKWPTJBahagian)
                        .ThenInclude(p => p!.JKW)
                    .Include(p => p.JKWPTJBahagian)
                        .ThenInclude(p => p!.JPTJ)
                    .Include(p => p.JKWPTJBahagian)
                        .ThenInclude(p => p!.JBahagian)
                .Include(p => p.AkCarta)
                .Include(p => p.DPanjarPemegang)!
                    .ThenInclude(pp => pp.DPekerja)
                .FirstOrDefault(p => p.Id == id) ?? new DPanjar();
        }

        public string GetMaxRefNo()
        {
            return _context.DPanjar.Max(p => p.Kod) ?? "0";
        }
    }
}