using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class AkCVRepository : _GenericRepository<AkCV>, IAkCVRepository
    {
        private readonly ApplicationDbContext _context;

        public AkCVRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public AkCV GetDetailsById(int id)
        {
            return _context.AkCV
                .IgnoreQueryFilters()
                .Include(cv => cv.DPekerja)
                .Include(cv => cv.DPanjar)
                    .ThenInclude(p => p!.JCawangan)
                .Include(cv => cv.DPanjar)
                    .ThenInclude(p => p!.DPanjarPemegang)!
                        .ThenInclude(pp => pp.DPekerja)
                .Include(cv => cv.AkCVObjek)!
                    .ThenInclude(o => o.JKWPTJBahagian)
                        .ThenInclude(b => b!.JBahagian)
                .FirstOrDefault(cv => cv.Id == id) ?? new AkCV();
        }

        public string GetMaxRefNo(string initNoRujukan, string tahun)
        {
            var max = _context.AkCV.Where(pp => pp.Tahun == tahun).OrderByDescending(pp => pp.NoRujukan).ToList();

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

        public List<AkCV> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang)
        {
            if (searchString == null && dateFrom == null && dateTo == null && orderBy == null)
            {
                return new List<AkCV>();
            }

            var akCVList = _context.AkCV
                .IgnoreQueryFilters()
                .Include(cv => cv.DPanjar)
                    .ThenInclude(p => p!.DPanjarPemegang)!
                        .ThenInclude(pp => pp.DPekerja)
                .Where(t => t.Tarikh >= dateFrom && t.Tarikh <= dateTo!.Value.AddHours(23.99))
                .ToList();

            // searchstring filters
            if (searchString != null)
            {
                akCVList = akCVList.Where(t =>
                t.NoRujukan!.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                || t.NamaPenerima!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            // searchString filters end

            // order by filters
            if (orderBy != null)
            {
                switch (orderBy)
                {
                    case "Nama":
                        akCVList = akCVList.OrderBy(t => t.NamaPenerima).ToList();
                        break;
                    case "Tarikh":
                        akCVList = akCVList.OrderBy(t => t.Tarikh).ToList(); break;
                    default:
                        akCVList = akCVList.OrderBy(t => t.NoRujukan).ToList();
                        break;
                }

            }
            // order by filters end

            return akCVList;
        }

        public void PostingToAkPanjarLejar(AkCV akCV)
        {
            
            if (akCV != null && akCV.AkCVObjek != null && akCV.AkCVObjek.Count() > 0)
            {
                List<AkPanjarLejar> akPanjarLejarList = new List<AkPanjarLejar>();
                foreach (var item in akCV.AkCVObjek)
                {
                    var akPanjarLejar = new AkPanjarLejar()
                    {
                        JKWPTJBahagianId = item.JKWPTJBahagianId,
                        DPanjarId = akCV.DPanjarId,
                        AkCVId = akCV.Id,
                        Tarikh = akCV.Tarikh,
                        AkCartaId = item.AkCartaId,
                        Kredit = item.Amaun,
                        NoRujukan = akCV.NoRujukan
                    };

                    akPanjarLejarList.Add(akPanjarLejar);
                }

                _context.AkPanjarLejar.AddRange(akPanjarLejarList);
            }
        }

        public void RemovePostingFromAkPanjarLejar(AkCV akCV)
        {
            var akPanjarLejarList = _context.AkPanjarLejar.Where(b => b.AkCVId == akCV.Id).ToList();

            if (akPanjarLejarList != null && akPanjarLejarList.Any())
            {
                _context.RemoveRange(akPanjarLejarList);
            }
        }

        public void Lulus(int id, int? DPekerjaId, string? userId)
        {
            var data = GetDetailsById(id);
            var pekerja = _context.DPekerja.FirstOrDefault(kk => kk.Id == DPekerjaId);
            if (data != null)
            {
                if (data.EnStatusBorang != EnStatusBorang.Kemaskini)
                {
                    data.DPelulusId = DPekerjaId;
                    data.TarikhLulus = DateTime.Now;
                }

                data.EnStatusBorang = EnStatusBorang.Lulus;
                data.FlPosting = 1;
                data.DPekerjaPostingId = DPekerjaId;
                data.TarikhPosting = DateTime.Now;

                data.UserIdKemaskini = userId ?? "";
                data.TarKemaskini = DateTime.Now;
                data.Tindakan = "";

                _context.Update(data);

                PostingToAkPanjarLejar(data);
            }
        }

        public void BatalPos(int id, string? tindakan, string? userId)
        {
            var data = _context.AkCV.FirstOrDefault(cv => cv.Id == id);

            if (data != null)
            {
                data.Tindakan = tindakan;
                data.EnStatusBorang = EnStatusBorang.None;

                data.UserIdKemaskini = userId ?? "";
                data.TarKemaskini = DateTime.Now;

                data.FlPosting = 0;
                data.TarikhPosting = null;

                _context.Update(data);

                RemovePostingFromAkPanjarLejar(data);
            }
        }

        public async Task<bool> IsLulusAsync(int id)
        {
            bool isLulus = await _context.AkCV.AnyAsync(t => t.Id == id && t.EnStatusBorang == EnStatusBorang.Lulus);
            if (isLulus)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsPostedAsync(int id, string noRujukan)
        {
            bool isPosted = await _context.AkCV.AnyAsync(t => t.Id == id && t.FlPosting == 1);
            if (isPosted)
            {
                return true;
            }

            bool isExistInAkPanjarLejar = await _context.AkPanjarLejar.AnyAsync(b => b.NoRujukan == noRujukan);

            if (isExistInAkPanjarLejar)
            {
                return true;
            }

            return false;

        }
    }
}