using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class AkPenyataPemungutRepository : _GenericRepository<AkPenyataPemungut>, IAkPenyataPemungutRepository
    {
        private readonly ApplicationDbContext _context;

        public AkPenyataPemungutRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public string GetMaxRefNo(string initNoRujukan, string tahun)
        {
            var max = _context.AkPenyataPemungut.Where(pp => pp.Tahun == tahun).OrderByDescending(pp => pp.NoRujukan).ToList();

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

        public List<AkPenyataPemungut> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang)
        {
            if (searchString == null && dateFrom == null && dateTo == null && orderBy == null)
            {
                return new List<AkPenyataPemungut>();
            }
            
            var akPenyataPemungutList = _context.AkPenyataPemungut
                .IgnoreQueryFilters()
                .Include(t => t.JPTJ)
                .Include(t => t.JCaraBayar)
                .Include(t => t.DPengesah)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPenyemak)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPelulus)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.AkPenyataPemungutObjek)!
                    .ThenInclude(to => to.AkTerimaTunggal)
                .Include(t => t.AkPenyataPemungutObjek)!
                    .ThenInclude(to => to.AkCarta)
                .Include(t => t.AkPenyataPemungutObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JKW)
                .Include(t => t.AkPenyataPemungutObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JPTJ)
                .Include(t => t.AkPenyataPemungutObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JBahagian)
                        .Where(t => t.Tarikh >= dateFrom && t.Tarikh <= dateTo!.Value.AddHours(23.99))
                .ToList();

            // searchstring filters
            if (searchString != null)
            {
                akPenyataPemungutList = akPenyataPemungutList.Where(t =>
                t.NoRujukan!.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                || t.JCawangan!.Perihal!.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    || t.JPTJ!.Perihal!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            // searchString filters end

            // status borang filters
            switch (enStatusBorang)
            {
                case EnStatusBorang.None:
                    akPenyataPemungutList = akPenyataPemungutList.Where(pp => pp.EnStatusBorang == EnStatusBorang.None).ToList();
                    break;
                case EnStatusBorang.Sah:
                    akPenyataPemungutList = akPenyataPemungutList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Sah).ToList();
                    break;
                case EnStatusBorang.Semak:
                    akPenyataPemungutList = akPenyataPemungutList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Semak).ToList();
                    break;
                case EnStatusBorang.Lulus:
                    akPenyataPemungutList = akPenyataPemungutList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Lulus).ToList();
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
                    case "Cawangan":
                        akPenyataPemungutList = akPenyataPemungutList.OrderBy(t => t.JCawangan!.Perihal).ToList();
                        break;
                    case "PTJ":
                        akPenyataPemungutList = akPenyataPemungutList.OrderBy(t => t.JPTJ!.Perihal).ToList();
                        break;
                    case "Tarikh":
                        akPenyataPemungutList = akPenyataPemungutList.OrderBy(t => t.Tarikh).ToList(); break;
                    default:
                        akPenyataPemungutList = akPenyataPemungutList.OrderBy(t => t.NoRujukan).ToList();
                        break;
                }

            }
            // order by filters end

            return akPenyataPemungutList;


        }

        public AkPenyataPemungut GetDetailsById(int id)
        {
            return _context.AkPenyataPemungut
                .IgnoreQueryFilters()
                .Include(t => t.JPTJ)
                .Include(t => t.JCaraBayar)
                .Include(t => t.JCawangan)
                .Include(t => t.AkBank)
                .Include(t => t.DPengesah)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPenyemak)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPelulus)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.AkPenyataPemungutObjek)!
                    .ThenInclude(to => to.AkTerimaTunggal)
                .Include(t => t.AkPenyataPemungutObjek)!
                    .ThenInclude(to => to.AkCarta)
                .Include(t => t.AkPenyataPemungutObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JKW)
                .Include(t => t.AkPenyataPemungutObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JPTJ)
                .Include(t => t.AkPenyataPemungutObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JBahagian)
                .FirstOrDefault(pp => pp.Id == id) ?? new AkPenyataPemungut();
        }

        public async Task<bool> IsPostedAsync(int id, string noRujukan)
        {
            bool isPosted = await _context.AkPenyataPemungut.AnyAsync(t => t.Id == id && t.FlPosting == 1);
            if (isPosted)
            {
                return true;
            }

            bool isExistInAkTerimaTunggal = await _context.AkTerimaTunggal.AnyAsync(b => b.NoSlip == noRujukan);

            if (isExistInAkTerimaTunggal)
            {
                return true;
            }

            return false;

        }

        public async Task<bool> IsLulusAsync(int id)
        {
            bool isLulus = await _context.AkPenyataPemungut.AnyAsync(t => t.Id == id && t.EnStatusBorang == EnStatusBorang.Lulus);
            if (isLulus)
            {
                return true;
            }

            return false;
        }

        public void RemoveNoSlipFromAkTerimaTunggalAkAkaun(AkPenyataPemungut akPenyataPemungut, string userId)
        {
            // update no slip AkTerimaTunggal
            var akTerimaTunggalList = _context.AkTerimaTunggal.Where(b => b.NoSlip == akPenyataPemungut.NoSlip).ToList();

            if (akTerimaTunggalList != null && akTerimaTunggalList.Count > 0)
            {
                foreach (var item in akTerimaTunggalList)
                {
                    item.NoSlip = "";
                    item.TarikhSlip = null;
                }

                _context.AkTerimaTunggal.UpdateRange(akTerimaTunggalList);

            }
            // update no slip AkAkaun
            var akAkaunList = _context.AkAkaun.Where(b => b.NoSlip == akPenyataPemungut.NoSlip).ToList();

            if (akAkaunList != null && akAkaunList.Count > 0)
            {
                foreach (var item in akAkaunList)
                {
                    item.NoSlip = "";
                    item.TarikhSlip = null;
                }

                _context.AkAkaun.UpdateRange(akAkaunList);

            }

            // update akPenyataPemungut posting fields
            akPenyataPemungut.DPekerjaPostingId = null;
            akPenyataPemungut.TarikhPosting = null;
            akPenyataPemungut.FlPosting = 0;

            akPenyataPemungut.UserIdKemaskini = userId;
            akPenyataPemungut.TarKemaskini = DateTime.Now;
            akPenyataPemungut.EnStatusBorang = EnStatusBorang.Kemaskini;
            akPenyataPemungut.DPelulusId = null;
            akPenyataPemungut.TarikhLulus = null;

            _context.AkPenyataPemungut.Update(akPenyataPemungut);
        }

        public void UpdateNoSlipAkTerimaTunggalAkAkaun(AkPenyataPemungut akPenyataPemungut, string userId, int? dPekerjaMasukId)
        {
            var akTerimaTunggalList = new List<AkTerimaTunggal>();
            if (akPenyataPemungut.AkPenyataPemungutObjek != null && akPenyataPemungut.AkPenyataPemungutObjek.Count > 0)
            {
                foreach (var item in akPenyataPemungut.AkPenyataPemungutObjek)
                {
                    var akTerimaTunggal = _context.AkTerimaTunggal.FirstOrDefault(tt => tt.Id == item.AkTerimaTunggalId);

                    if (akTerimaTunggal != null)
                    {
                        
                        akTerimaTunggal.NoSlip = akPenyataPemungut.NoSlip;
                        akTerimaTunggal.TarikhSlip = akPenyataPemungut.Tarikh;
                        akTerimaTunggalList.Add(akTerimaTunggal);
                        UpdateSlipAkAkaun(akTerimaTunggal.NoRujukan, akPenyataPemungut.NoSlip, akPenyataPemungut.Tarikh);
                    }
                }
                _context.AkTerimaTunggal.UpdateRange(akTerimaTunggalList);
            }

            // update akPenyataPemungut posting fields
            akPenyataPemungut.DPekerjaPostingId = dPekerjaMasukId;
            akPenyataPemungut.TarikhPosting = DateTime.Now;
            akPenyataPemungut.FlPosting = 1;
            akPenyataPemungut.EnStatusBorang = EnStatusBorang.Lulus;
            akPenyataPemungut.DPelulusId = dPekerjaMasukId;
            akPenyataPemungut.TarikhLulus = DateTime.Now;

            _context.AkPenyataPemungut.Update(akPenyataPemungut);
        }

        public void UpdateSlipAkAkaun(string? noRujukan,string? noSlip, DateTime? tarikhSlip)
        {
            if (noRujukan != null && noSlip != null && tarikhSlip != null)
            {
                    var akAkaun = _context.AkAkaun.Where(tt => tt.NoRujukan == noRujukan).ToList();

                if (akAkaun != null && akAkaun.Any())
                {
                    foreach (var item in akAkaun)
                    {
                        item.NoSlip = noSlip;
                        item.TarikhSlip = tarikhSlip;
                        
                    }
                    _context.AkAkaun.UpdateRange(akAkaun);
                    
                }
                
            }

        }

        public List<AkPenyataPemungutObjek> GetAkPenyataPemungutObjekByAkPenyataPemungutId(int Id)
        {
            List<AkPenyataPemungutObjek> data = _context.AkPenyataPemungutObjek.Include(ppo => ppo.AkPenyataPemungut).Where(ppo => ppo.AkPenyataPemungutId == Id).ToList();

            return data;
        }

        public bool IsAkPenyataPemungutObjekExist(Expression<Func<AkPenyataPemungutObjek,bool>> predicate)
        {
            return _context.AkPenyataPemungutObjek.Include(ppo => ppo.AkPenyataPemungut).Any(predicate);
        }

    }
}