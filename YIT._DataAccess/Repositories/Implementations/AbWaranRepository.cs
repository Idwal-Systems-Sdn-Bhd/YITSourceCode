using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT.__Domain.Entities._Enums;
using System.Runtime.Intrinsics.X86;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class AbWaranRepository : _GenericRepository<AbWaran>, IAbWaranRepository
    {
        private readonly ApplicationDbContext _context;

        public AbWaranRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public AbWaran GetDetailsById(int id)
        {
            return _context.AbWaran
                .IgnoreQueryFilters()
                .Include(t => t.JKW)
                .Include(t => t.DPekerjaPosting)
                .Include(t => t.DPengesah)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPenyemak)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPelulus)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.AbWaranObjek)!
                    .ThenInclude(to => to.AkCarta)
                .Include(t => t.AbWaranObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JKW)
                .Include(t => t.AbWaranObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JPTJ)
                .Include(t => t.AbWaranObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JBahagian)
                .FirstOrDefault(pp => pp.Id == id) ?? new AbWaran();
        }

        public List<AbWaran> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang)
        {
            if (searchString == null && dateFrom == null && dateTo == null && orderBy == null)
            {
                return new List<AbWaran>();
            }

            var abWaranList = _context.AbWaran
                .IgnoreQueryFilters()
                .Include(t => t.JKW)
                .Include(t => t.DPekerjaPosting)
                .Include(t => t.DPengesah)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPenyemak)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPelulus)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.AbWaranObjek)!
                    .ThenInclude(to => to.AkCarta)
                .Include(t => t.AbWaranObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JKW)
                .Include(t => t.AbWaranObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JPTJ)
                .Include(t => t.AbWaranObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JBahagian)
                        .Where(t => t.Tarikh >= dateFrom && t.Tarikh <= dateTo!.Value.AddHours(23.99))
                        .ToList();

            // searchstring filters
            if (searchString != null)
            {
                abWaranList = abWaranList.Where(t => t.NoRujukan!.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();

            }
            // searchString filters end

            // status borang filters
            switch (enStatusBorang)
            {
                case EnStatusBorang.None:
                    abWaranList = abWaranList.Where(pp => pp.EnStatusBorang == EnStatusBorang.None).ToList();
                    break;
                case EnStatusBorang.Sah:
                    abWaranList = abWaranList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Sah).ToList();
                    break;
                case EnStatusBorang.Semak:
                    abWaranList = abWaranList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Semak).ToList();
                    break;
                case EnStatusBorang.Lulus:
                    abWaranList = abWaranList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Lulus).ToList();
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
                    case "Tarikh":
                        abWaranList = abWaranList.OrderBy(t => t.Tarikh).ToList(); break;
                    default:
                        abWaranList = abWaranList.OrderBy(t => t.NoRujukan).ToList();
                        break;
                }

            }
            // order by filters end

            return abWaranList;
        }

        public List<AbWaran> GetResultsByDPekerjaIdFromDKonfigKelulusan(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang, int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan)
        {

            // get all data with details
            List<AbWaran> abWaranList = GetResults(searchString, dateFrom, dateTo, orderBy, enStatusBorang);

            var filterings = FilterByComparingJBahagianAbWaranObjekWithJBahagianDKonfigKelulusan(dPekerjaId, enKategoriKelulusan, enJenisModulKelulusan, abWaranList);

            var results = FilterByComparingJumlahAbWaranWithMinAmountMaxAmountDKonfigKelulusan(dPekerjaId, enKategoriKelulusan, enJenisModulKelulusan, filterings);

            return results;
        }

        public List<AbWaran> FilterByComparingJBahagianAbWaranObjekWithJBahagianDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AbWaran> abWaranList)
        {
            // initialize result sets
            List<AbWaran> results = new List<AbWaran>();

            //get all pengesah/penyemak/pelulus with same dpekerjaId, group by pekerjaId and bahagianId

            var konfigKelulusanBahagianGrouped = _context.DKonfigKelulusan
                 .Include(kk => kk.DPekerja)
                 .Include(kk => kk.JBahagian)
                .Where(b => b.EnKategoriKelulusan == enKategoriKelulusan
                && b.DPekerjaId == dPekerjaId
                && b.EnJenisModulKelulusan == enJenisModulKelulusan)
                .GroupBy(b => new { b.DPekerjaId, b.JBahagianId }).Select(l => new DKonfigKelulusan
                {
                    Id = l.First().DPekerjaId,
                    DPekerjaId = l.First().DPekerjaId,
                    DPekerja = l.First().DPekerja,
                    JBahagianId = l.First().JBahagianId,
                    JBahagian = l.First().JBahagian
                }).ToList();

            var konfigKelulusanBahagianList = new List<JBahagian>();


            if (konfigKelulusanBahagianGrouped != null && konfigKelulusanBahagianGrouped.Count > 0)
            {

                foreach (var item in konfigKelulusanBahagianGrouped)
                {
                    if (item.JBahagian != null) konfigKelulusanBahagianList.Add(item.JBahagian);
                }

                var abWaranGroup = new List<AbWaranObjek>().GroupBy(objek => objek.JKWPTJBahagianId);
                if (abWaranList != null && abWaranList.Count > 0)
                {
                    foreach (var abWaran in abWaranList)
                    {
                        var waranObjekBahagianList = new List<JBahagian>();

                        // group abWaranObjek by bahagian
                        if (abWaran.AbWaranObjek != null && abWaran.AbWaranObjek.Count > 0)
                        {
                            foreach (var item in abWaran.AbWaranObjek)
                            {
                                waranObjekBahagianList.Add(item.JKWPTJBahagian?.JBahagian ?? new JBahagian());
                            }

                        }
                        // if konfigKelulusan bahagian null, bypass all, add to results
                        if (konfigKelulusanBahagianList.Count == 0)
                        {
                            results.Add(abWaran);
                            continue;
                        }

                        // compare each lists, if its equal then insert to results
                        var items = waranObjekBahagianList.All(konfigKelulusanBahagianList.Contains);
                        if (konfigKelulusanBahagianList.OrderBy(kk => kk.Kod).SequenceEqual(waranObjekBahagianList.OrderBy(pp => pp.Kod))
                            || waranObjekBahagianList.All(konfigKelulusanBahagianList.Contains))
                        {

                            results.Add(abWaran);
                            continue;
                        };
                    }
                }
            }


            return results;
        }

        public List<AbWaran> FilterByComparingJumlahAbWaranWithMinAmountMaxAmountDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AbWaran> filterings)
        {
            //initialize new list akPP
            List<AbWaran> results = new List<AbWaran>();

            // get list of dKonfigKelulusan with same DPekerjaId, enKategoriKelulusan, enJenisModulKelulusan
            var konfigKelulusanList = _context.DKonfigKelulusan.Include(kk => kk.DPekerja)
                 .Include(kk => kk.JBahagian)
                .Where(b => b.EnKategoriKelulusan == enKategoriKelulusan
                && b.DPekerjaId == dPekerjaId
                && b.EnJenisModulKelulusan == enJenisModulKelulusan).ToList();

            if (filterings != null && filterings.Count > 0)
            {
                foreach (var filtering in filterings)
                {
                    if (konfigKelulusanList != null && konfigKelulusanList.Count > 0)
                    {
                        foreach (var konfigKelulusan in konfigKelulusanList)
                        {
                            if (konfigKelulusan.MinAmaun <= filtering.Jumlah && filtering.Jumlah <= konfigKelulusan.MaksAmaun)
                            {
                                results.Add(filtering);
                            }
                        }
                    }
                }
            }

            return results.GroupBy(b => b.Id).Select(grp => grp.First()).ToList();
        }

        public async Task<bool> IsSahAsync(int id)
        {
            bool isSah = await _context.AbWaran.AnyAsync(t => t.Id == id && (t.EnStatusBorang == EnStatusBorang.Sah || t.EnStatusBorang == EnStatusBorang.Semak || t.EnStatusBorang == EnStatusBorang.Lulus));
            if (isSah)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsSemakAsync(int id)
        {
            bool isSemak = await _context.AbWaran.AnyAsync(t => t.Id == id && (t.EnStatusBorang == EnStatusBorang.Semak || t.EnStatusBorang == EnStatusBorang.Lulus));

            if (isSemak)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsLulusAsync(int id)
        {
            bool isLulus = await _context.AbWaran.AnyAsync(t => t.Id == id && t.EnStatusBorang == EnStatusBorang.Lulus);
            if (isLulus)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsPostedAsync(int id, string noRujukan)
        {
            bool isPosted = await _context.AbWaran.AnyAsync(t => t.Id == id && t.FlPosting == 1);
            if (isPosted)
            {
                return true;
            }

            bool isExistInAbBukuVot = await _context.AbBukuVot.AnyAsync(b => b.NoRujukan == noRujukan);

            if (isExistInAbBukuVot)
            {
                return true;
            }

            return false;

        }

        public void Sah(int id, int? pengesahId, string? userId)
        {
            var data = _context.AbWaran.FirstOrDefault(pp => pp.Id == id);
            var pengesah = _context.DKonfigKelulusan.FirstOrDefault(kk => kk.DPekerjaId == pengesahId);
            if (data != null)
            {
                data.EnStatusBorang = EnStatusBorang.Sah;
                data.DPengesahId = pengesah!.Id;
                data.TarikhSah = DateTime.Now;

                data.UserIdKemaskini = userId ?? "";
                data.TarKemaskini = DateTime.Now;
                data.Tindakan = "";

                _context.Update(data);

            }
        }

        public void Semak(int id, int penyemakId, string? userId)
        {
            var data = _context.AbWaran.FirstOrDefault(pp => pp.Id == id);
            var penyemak = _context.DKonfigKelulusan.FirstOrDefault(kk => kk.DPekerjaId == penyemakId);
            if (data != null)
            {
                data.EnStatusBorang = EnStatusBorang.Semak;
                data.DPenyemakId = penyemak!.Id;
                data.TarikhSemak = DateTime.Now;

                data.UserIdKemaskini = userId ?? "";
                data.TarKemaskini = DateTime.Now;
                data.Tindakan = "";

                _context.Update(data);

            }
        }

        public void Lulus(int id, int? pelulusId, string? userId)
        {
            var data = GetDetailsById(id);
            var pelulus = _context.DKonfigKelulusan.FirstOrDefault(kk => kk.DPekerjaId == pelulusId);
            if (data != null)
            {
                if (data.EnStatusBorang != EnStatusBorang.Kemaskini)
                {
                    data.DPelulusId = pelulus!.Id;
                    data.TarikhLulus = DateTime.Now;
                }

                data.EnStatusBorang = EnStatusBorang.Lulus;
                data.FlPosting = 1;
                data.DPekerjaPostingId = pelulusId;
                data.TarikhPosting = DateTime.Now;

                data.UserIdKemaskini = userId ?? "";
                data.TarKemaskini = DateTime.Now;
                data.Tindakan = "";

                _context.Update(data);

                PostingToAbBukuVot(data);

            }
        }

        public async Task<bool> IsBatalAsync(int id)
        {
            bool isBatal = await _context.AbWaran.AnyAsync(t => t.Id == id && t.FlBatal == 1);
            if (isBatal)
            {
                return true;
            }

            return false;
        }

        public void Batal(int id, string? sebabBatal, string? userId)
        {
            var data = _context.AbWaran.FirstOrDefault(pp => pp.Id == id);

            if (data != null)
            {
                data.EnStatusBorang = EnStatusBorang.Batal;
                data.FlBatal = 1;
                data.TarBatal = DateTime.Now;
                data.SebabBatal = sebabBatal;

                data.UserIdKemaskini = userId ?? "";
                data.TarKemaskini = DateTime.Now;

                _context.Update(data);
            }
        }

        public void PostingToAbBukuVot(AbWaran abWaran)
        {
            List<AbBukuVot> abBukuVotList = new List<AbBukuVot>();

            if (abWaran.AbWaranObjek != null && abWaran.AbWaranObjek.Count > 0)
            {


                foreach (var item in abWaran.AbWaranObjek)
                {
                    AbBukuVot abBukuVot = new AbBukuVot()
                    {
                        Tahun = abWaran.Tahun,
                        JKWId = item.JKWPTJBahagian?.JKWId ?? abWaran.JKWId,
                        JPTJId = (int)item.JKWPTJBahagian!.JPTJId,
                        JBahagianId = item.JKWPTJBahagian.JBahagianId,
                        Tarikh = abWaran.Tarikh,
                        DDaftarAwamId = null,
                        VotId = item.AkCartaId,
                        NoRujukan = abWaran.NoRujukan,
                        Kredit = item.Amaun
                    };

                    abBukuVotList.Add(abBukuVot);
                }
            }

            _context.AbBukuVot.AddRange(abBukuVotList);
        }

        public void RemovePostingFromAbBukuVot(AbWaran abWaran, string userId)
        {
            var abBukuVotList = _context.AbBukuVot.Where(b => b.NoRujukan == abWaran.NoRujukan).ToList();

            if (abBukuVotList != null && abBukuVotList.Count > 0)
            {
                _context.RemoveRange(abBukuVotList);
            }

        }

        public void HantarSemula(int id, string? tindakan, string? userId)
        {
            var data = _context.AbWaran.FirstOrDefault(pp => pp.Id == id);

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

                data.FlPosting = 0;
                data.TarikhPosting = null;

                _context.Update(data);

            }
        }

        public void BatalLulus(int id, string? tindakan, string? userId)
        {
            var data = _context.AbWaran.FirstOrDefault(pp => pp.Id == id);

            if (data != null)
            {
                HantarSemula(id, tindakan, userId);

                RemovePostingFromAbBukuVot(data, userId ?? "");

            }

        }

        public void BatalPos(int id, string? tindakan, string? userId)
        {
            var data = _context.AbWaran.FirstOrDefault(pp => pp.Id == id);

            if (data != null)
            {
                data.EnStatusBorang = EnStatusBorang.Kemaskini;
                data.Tindakan = tindakan;

                data.UserIdKemaskini = userId ?? "";
                data.TarKemaskini = DateTime.Now;

                data.FlPosting = 0;
                data.TarikhPosting = null;

                _context.Update(data);

                RemovePostingFromAbBukuVot(data, userId ?? "");

            }
        }


        public string GetMaxRefNo(string initNoRujukan, string tahun)
        {
            var max = _context.AbWaran.Where(pp => pp.Tahun == tahun).OrderByDescending(pp => pp.NoRujukan).ToList();

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

    }
}
