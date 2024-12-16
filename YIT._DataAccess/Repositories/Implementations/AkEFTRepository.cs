using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class AkEFTRepository : _GenericRepository<AkEFT>, IAkEFTRepository
    {
        private readonly ApplicationDbContext _context;

        public AkEFTRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public AkEFT GetDetailsById(int id)
        {
            return _context.AkEFT
                .IgnoreQueryFilters()
                .Include(e => e.AkBank)
                    .ThenInclude(b => b!.JBank)
                .Include(e => e.AkEFTPenerima)!
                    .ThenInclude(ep => ep.AkPV)
                        .ThenInclude(ep => ep!.AkPVPenerima)!
                .Include(e => e.AkEFTPenerima)!
                    .ThenInclude(ep => ep.AkPV)
                        .ThenInclude(ep => ep!.AkPVPenerima)!
                .FirstOrDefault(e => e.Id == id) ?? new AkEFT();
        }

        public string GetMaxRefNo(string initNoRujukan, string tahun)
        {
            var max = _context.AkEFT.Where(pp => pp.Tarikh.Year == int.Parse(tahun)).OrderByDescending(pp => pp.NoRujukan).ToList();

            if (max != null)
            {
                var refNo = max.FirstOrDefault()?.NoRujukan?.Substring(8, 5);
                return refNo ?? "";
            }
            else
            {
                return "";
            }
        }

        public List<AkEFT> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy)
        {
            if (searchString == null && dateFrom == null && dateTo == null && orderBy == null)
            {
                return new List<AkEFT>();
            }

            var akEFTList = _context.AkEFT
                .IgnoreQueryFilters()
                .Include(e => e.AkBank)
                    .ThenInclude(b => b!.JBank)
                .Include(e => e.AkEFTPenerima)!
                    .ThenInclude(ep => ep.AkPV)
                        .ThenInclude(ep => ep!.AkPVPenerima)!
                .Include(e => e.AkEFTPenerima)!
                    .ThenInclude(ep => ep.AkPV)
                        .ThenInclude(ep => ep!.AkPVPenerima)!
                .ToList();

            // searchstring filters
            if (searchString != null)
            {
                akEFTList = akEFTList.Where(t =>
                t.NoRujukan!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            // searchString filters end

            // order by filters
            if (orderBy != null)
            {
                switch (orderBy)
                {
                    case "Tarikh":
                        akEFTList = akEFTList.OrderBy(t => t.Tarikh).ToList(); break;
                    default:
                        akEFTList = akEFTList.OrderBy(t => t.NoRujukan).ToList();
                        break;
                }
            }
            // order by filters end

            return akEFTList;
        }

        public async Task<List<AkEFT>> GetResultsGroupByTarikh(string? tarikhDari, string? tarikhHingga)
        {
            if (tarikhDari == null || tarikhHingga == null)
            {
                return new List<AkEFT>();
            }

            DateTime date1 = DateTime.Parse(tarikhDari).Date;
            DateTime date2 = DateTime.Parse(tarikhHingga).Date.AddDays(1).AddTicks(-1);

            var akEft = await _context.AkEFT
                .Include(b => b.AkEFTPenerima)
                .Where(b => b.Tarikh >= date1 && b.Tarikh <= date2)
                .OrderBy(a => a.Tarikh)
                .ThenBy(a => a.NoRujukan)
                .ToListAsync();

            return akEft;
        }

        public async Task<List<AkEFT>> GetResultsGroupBySearchString(string? searchString1, string? searchString2)
        {
            if (searchString1 == null || searchString2 == null)
            {
                return new List<AkEFT>();
            }

            var lowerSearchString1 = searchString1.ToLower();
            var lowerSearchString2 = searchString2.ToLower();

            bool isOrderCorrect = string.Compare(lowerSearchString1, lowerSearchString2, StringComparison.Ordinal) <= 0;

            if (!isOrderCorrect)
            {
                return new List<AkEFT>();
            }

            var akEft = await _context.AkEFT
                .Include(b => b.AkEFTPenerima)
                .Where(b => b.NoRujukan!.ToLower().CompareTo(lowerSearchString1) >= 0 && b.NoRujukan!.ToLower().CompareTo(lowerSearchString2) <= 0)
                .ToListAsync();

            return akEft;
        }
    }
}