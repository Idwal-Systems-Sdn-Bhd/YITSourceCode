using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class AkPenyesuaianBankRepository : _GenericRepository<AkPenyesuaianBank>, IAkPenyesuaianPenyataBankRepository
    {
        private readonly ApplicationDbContext _context;

        public AkPenyesuaianBankRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public List<AkPenyesuaianBank> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang)
        {
            if (searchString == null && dateFrom == null && dateTo == null && orderBy == null)
            {
                return new List<AkPenyesuaianBank>();
            }

            var akPenyesuaianBankList = _context.AkPenyesuaianBank
                .IgnoreQueryFilters()
                .Include(t => t.AkBank)
                .Include(t => t.DPengesah)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPenyemak)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPelulus)
                    .ThenInclude(t => t!.DPekerja)
                        .Where(t => t.Tarikh >= dateFrom && t.Tarikh <= dateTo!.Value.AddHours(23.99))
                .ToList();

            // searchstring filters
            if (searchString != null)
            {
                akPenyesuaianBankList = akPenyesuaianBankList.Where(t =>
                t.NoRujukan!.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                || t.NamaFail!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            // searchString filters end

            // status borang filters
            switch (enStatusBorang)
            {
                case EnStatusBorang.None:
                    akPenyesuaianBankList = akPenyesuaianBankList.Where(pp => pp.EnStatusBorang == EnStatusBorang.None).ToList();
                    break;
                case EnStatusBorang.Sah:
                    akPenyesuaianBankList = akPenyesuaianBankList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Sah).ToList();
                    break;
                case EnStatusBorang.Semak:
                    akPenyesuaianBankList = akPenyesuaianBankList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Semak).ToList();
                    break;
                case EnStatusBorang.Lulus:
                    akPenyesuaianBankList = akPenyesuaianBankList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Lulus).ToList();
                    break;
                case EnStatusBorang.Semua:
                    break;
            }
            // status borang filters end

            // order by filters
            if (orderBy != null)
            {
                switch (orderBy)
                {
                    case "Nama":
                        akPenyesuaianBankList = akPenyesuaianBankList.OrderBy(t => t.NamaFail).ToList();
                        break;
                    case "Tarikh":
                        akPenyesuaianBankList = akPenyesuaianBankList.OrderBy(t => t.Tarikh).ToList(); break;
                    default:
                        akPenyesuaianBankList = akPenyesuaianBankList.OrderBy(t => t.NoRujukan).ToList();
                        break;
                }

            }
            // order by filters end

            return akPenyesuaianBankList;


        }

        public string GetMaxRefNo(string initNoRujukan, string tahun)
        {
            var max = _context.AkPenyesuaianBank.Where(pp => pp.Tahun == tahun).OrderByDescending(pp => pp.NoRujukan).ToList();

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

        public AkPenyesuaianBank GetDetailsById(int id)
        {
            return _context.AkPenyesuaianBank
                .IgnoreQueryFilters()
                .Include(t => t.AkBank)
                .Include(t => t.DPengesah)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPenyemak)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPelulus)
                    .ThenInclude(t => t!.DPekerja)
                .FirstOrDefault(pp => pp.Id == id) ?? new AkPenyesuaianBank();
        }

        public async Task<bool> IsPostedAsync(int id, string noRujukan)
        {
            bool isPosted = await _context.AkPenyesuaianBank.AnyAsync(t => t.Id == id && t.FlPosting == 1);
            if (isPosted)
            {
                return true;
            }

            var akPenyesuaianBank = await _context.AkPenyesuaianBank.Include(pb => pb.AkPenyesuaianBankPenyataBank).FirstOrDefaultAsync(b => b.Id == id);

            if (akPenyesuaianBank != null && akPenyesuaianBank.AkPenyesuaianBankPenyataBank != null && akPenyesuaianBank.AkPenyesuaianBankPenyataBank.Any())
            {
                foreach (var item in akPenyesuaianBank.AkPenyesuaianBankPenyataBank)
                {
                    bool isExistInAkAkaunPenyataBank = await _context.AkPenyesuaianBankPenyataBank.AnyAsync(pb => pb.IsPadan == true);

                    if (isExistInAkAkaunPenyataBank)
                    {
                        return true;
                    }
                }
            }
            return false;

        }

        public List<AkPenyesuaianBankPenyataBank> GetAkPenyesuaianBankPenyataBankByAkPenyesuaianBankId(int Id)
        {
            List<AkPenyesuaianBankPenyataBank> data = _context.AkPenyesuaianBankPenyataBank.Include(ppo => ppo.AkAkaunPenyataBank).Where(ppo => ppo.AkPenyesuaianBankId == Id)
                .OrderBy(ppo => ppo.Tarikh).ToList();

            return data;
        }

        public bool IsAkPenyesuaianBankPenyataBankExist(Expression<Func<AkPenyesuaianBankPenyataBank, bool>> predicate)
        {
            return _context.AkPenyesuaianBankPenyataBank.Include(ppo => ppo.AkAkaunPenyataBank).Any(predicate);
        }
    }
}