using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class AkPenilaianPerolehanRepository : _GenericRepository<AkPenilaianPerolehan>, IAkPenilaianPerolehanRepository
    {
        private readonly ApplicationDbContext _context;

        public AkPenilaianPerolehanRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public AkPenilaianPerolehan GetDetailsById(int id)
        {
            throw new NotImplementedException();
        }

        public List<AkPenilaianPerolehan> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy)
        {
            if (searchString == null && dateFrom == null && dateTo == null && orderBy == null)
            {
                return new List<AkPenilaianPerolehan>();
            }

            var akPP = _context.AkPenilaianPerolehan
                .IgnoreQueryFilters()
                .Include(t => t.JKW)
                .Include(t => t.DPemohon)
                .Include(t => t.DDaftarAwam)
                .Include(t => t.DPekerjaPosting)
                .Include(t => t.DPengesah)
                .Include(t => t.DPenyemak)
                .Include(t => t.DPelulus)
                .Include(t => t.AkPenilaianPerolehanObjek)!
                    .ThenInclude(to => to.AkCarta)
                .Include(t => t.AkPenilaianPerolehanObjek)!
                    .ThenInclude(to => to.JBahagian)
                        .ThenInclude(b => b!.JPTJ)
                            .ThenInclude(b => b!.JKW)
                .ToList();

            // searchstring filters
            if (searchString != null)
            {
                akPP = akPP.Where(t =>
                t.NoRujukan!.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                || t.DPemohon!.Nama!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            // searchString filters end

            // date filters
            if (dateFrom != null && dateTo != null)
            {
                akPP = akPP.Where(t => t.Tarikh >= dateFrom && t.Tarikh <= dateTo.Value.AddHours(23.99)).ToList();
            }
            // date filters end

            // order by filters
            if (orderBy != null)
            {
                switch (orderBy)
                {
                    case "Nama":
                        akPP = akPP.OrderBy(t => t.DPemohon!.Nama).ToList();
                        break;
                    case "Tarikh":
                        akPP = akPP.OrderBy(t => t.Tarikh).ToList(); break;
                    default:
                        akPP = akPP.OrderBy(t => t.NoRujukan).ToList();
                        break;
                }

            }
            // order by filters end

            return akPP;
        }

        public async Task<bool> IsSahAsync(int id)
        {
            bool isSah = await _context.AkPenilaianPerolehan.AnyAsync(t => t.Id == id && t.EnStatusBorang == EnStatusBorang.Sah);
            if (isSah)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsSemakAsync(int id)
        {
            bool isSemak = await _context.AkPenilaianPerolehan.AnyAsync(t => t.Id == id && t.EnStatusBorang == EnStatusBorang.Semak);
            if (isSemak)
            {
                return true;
            }

            return false;
        }


        public async Task<bool> IsLulusAsync(int id)
        {
            bool isLulus = await _context.AkPenilaianPerolehan.AnyAsync(t => t.Id == id && t.EnStatusBorang == EnStatusBorang.Lulus);
            if (isLulus)
            {
                return true;
            }

            return false;
        }

        public async void SahAsync(int id, int pengesahId, string? userId)
        {
            var data = await _context.AkPenilaianPerolehan.FirstOrDefaultAsync(pp => pp.Id == id);

            if (data != null)
            {
                data.EnStatusBorang = EnStatusBorang.Sah;
                data.DPengesahId = pengesahId;
                data.TarikhSah = DateTime.Now;

                data.UserIdKemaskini = userId ?? "";
                data.TarKemaskini = DateTime.Now;

                _context.Update(data);

            }
        }

        public async void BatalSahAsync(int id, string? tindakan, string? userId)
        {
            var data = await _context.AkPenilaianPerolehan.FirstOrDefaultAsync(pp => pp.Id == id);

            if (data != null)
            {
                data.EnStatusBorang = EnStatusBorang.None;
                data.DPengesahId = null;
                data.TarikhSah = null;

                data.Tindakan = tindakan;
                data.UserIdKemaskini = userId ?? "";
                data.TarKemaskini = DateTime.Now;

                _context.Update(data);
            }
        }

        public async void SemakAsync(int id, int penyemakId, string? userId)
        {
            var data = await _context.AkPenilaianPerolehan.FirstOrDefaultAsync(pp => pp.Id == id);

            if (data != null)
            {
                data.EnStatusBorang = EnStatusBorang.Semak;
                data.DPenyemakId = penyemakId;
                data.TarikhSemak = DateTime.Now;

                data.UserIdKemaskini = userId ?? "";
                data.TarKemaskini = DateTime.Now;

                _context.Update(data);

            }
        }

        public async void BatalSemakAsync(int id, string? tindakan, string? userId)
        {
            var data = await _context.AkPenilaianPerolehan.FirstOrDefaultAsync(pp => pp.Id == id);

            if (data != null)
            {
                data.EnStatusBorang = EnStatusBorang.None;
                data.DPengesahId = null;
                data.TarikhSah = null;

                data.DPenyemakId = null;
                data.TarikhSemak = null;

                data.Tindakan = tindakan;
                data.UserIdKemaskini = userId ?? "";
                data.TarKemaskini = DateTime.Now;

                _context.Update(data);
            }
        }

        public async void LulusAsync(int id, int pelulusId, string? userId)
        {
            var data = await _context.AkPenilaianPerolehan.FirstOrDefaultAsync(pp => pp.Id == id);

            if (data != null)
            {
                data.EnStatusBorang = EnStatusBorang.Lulus;
                data.DPelulusId = pelulusId;
                data.TarikhLulus = DateTime.Now;

                data.FlPosting = 1;
                data.DPekerjaPostingId = pelulusId;
                data.TarikhPosting = DateTime.Now;

                data.UserIdKemaskini = userId ?? "";
                data.TarKemaskini = DateTime.Now;

                _context.Update(data);

            }
        }

        public async void BatalLulusAsync(int id, string? tindakan, string? userId)
        {
            var data = await _context.AkPenilaianPerolehan.FirstOrDefaultAsync(pp => pp.Id == id);

            if (data != null)
            {
                data.EnStatusBorang = EnStatusBorang.None;
                data.DPengesahId = null;
                data.TarikhSah = null;

                data.DPenyemakId = null;
                data.TarikhSemak = null;

                data.DPelulusId = null;
                data.TarikhLulus = null;

                data.DPekerjaPostingId = null;
                data.TarikhPosting = null;

                data.Tindakan = tindakan;
                data.UserIdKemaskini = userId ?? "";
                data.TarKemaskini = DateTime.Now;

                _context.Update(data);
            }
        }

        public async Task<bool> IsBatalAsync(int id)
        {
            bool isBatal = await _context.AkPenilaianPerolehan.AnyAsync(t => t.Id == id && t.FlBatal == 1);
            if (isBatal)
            {
                return true;
            }

            return false;
        }

        public async void BatalAsync(int id, string? sebabBatal, string? userId)
        {
            var data = await _context.AkPenilaianPerolehan.FirstOrDefaultAsync(pp => pp.Id == id);

            if (data != null)
            {
                data.FlBatal = 1;
                data.TarBatal = DateTime.Now;
                data.SebabBatal = sebabBatal;

                data.UserIdKemaskini = userId ?? "";
                data.TarKemaskini = DateTime.Now;

                _context.Update(data);
            }
        }

        public async Task<bool> IsInBudgetAsync(string? tahun, int jBahagianId, int akCartaId, decimal amaun)
        {
            // check if enough budget in abBukuVot
            var sql = (from tbl in await _context.AbBukuVot
                       .Include(x => x.Vot)
            .Include(x => x.JBahagian)
                       .Where(x => x.Tahun == tahun && x.VotId == akCartaId && x.JBahagianId == jBahagianId)
                       .ToListAsync()
                       select new
                       {
                           Id = tbl.VotId,
                           Tahun = tbl.Tahun,
                           Bahagian = tbl.JBahagian?.Kod,
                           KodAkaun = tbl.Vot?.Kod,
                           Debit = tbl.Debit,
                           Kredit = tbl.Kredit,
                           Tanggungan = tbl.Tanggungan,
                           Liabiliti = tbl.Liabiliti,
                           Baki = tbl.Baki
                       }).GroupBy(x => new { x.Tahun, x.KodAkaun, x.Bahagian }).FirstOrDefault();

            return amaun < sql?.Select(t => t.Baki + t.Kredit - t.Debit - t.Tanggungan).Sum();
        }

        public async Task<bool> IsBudgetAvailableAsync(string? tahun, int jBahagianId, int akCartaId)
        {
            return await _context.AbBukuVot.AnyAsync(pp => pp.Tahun == tahun && pp.JBahagianId == jBahagianId && pp.VotId == akCartaId);
        }

        public string GetMaxRefNo(string initNoRujukan, string tahun)
        {
            var max = _context.AkPenilaianPerolehan.Where(pp => pp.Tahun == tahun).OrderByDescending(pp => pp.NoRujukan).ToList();

            if (max != null)
            {
                var refNo = max.FirstOrDefault()?.NoRujukan?.Substring(8,5);
                return refNo ?? "";
            }
            else
            {
                return "";
            }
        }
    }
}
