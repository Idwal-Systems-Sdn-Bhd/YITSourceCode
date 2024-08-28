using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    internal class AkInvoisRepository : _GenericRepository<AkInvois>, IAkInvoisRepository
    {
        private readonly ApplicationDbContext _context;

        public AkInvoisRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public string GetMaxRefNo(string initNoRujukan, string tahun)
        {
            var max = _context.AkInvois.Where(pp => pp.Tahun == tahun).OrderByDescending(pp => pp.NoRujukan).ToList();

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

        public List<AkInvois> GetResultsByDPekerjaIdFromDKonfigKelulusan(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang, int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan)
        {

            // get all data with details
            List<AkInvois> akInvoisList = GetResults(searchString, dateFrom, dateTo, orderBy, enStatusBorang);

            var filterings = FilterByComparingJBahagianAkInvoisObjekWithJBahagianDKonfigKelulusan(dPekerjaId, enKategoriKelulusan, enJenisModulKelulusan, akInvoisList);

            var results = FilterByComparingJumlahAkInvoisWithMinAmountMaxAmountDKonfigKelulusan(dPekerjaId, enKategoriKelulusan, enJenisModulKelulusan, filterings);

            return results;
        }

        public List<AkInvois> FilterByComparingJBahagianAkInvoisObjekWithJBahagianDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AkInvois> akInvoisList)
        {
            // initialize result sets
            List<AkInvois> results = new List<AkInvois>();

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

                var akInvoisGroup = new List<AkInvoisObjek>().GroupBy(objek => objek.JKWPTJBahagianId);
                if (akInvoisList != null && akInvoisList.Count > 0)
                {
                    foreach (var akInvois in akInvoisList)
                    {
                        var akInvoisObjekBahagianList = new List<JBahagian>();

                        // group akInvoisObjek by bahagian
                        if (akInvois.AkInvoisObjek != null && akInvois.AkInvoisObjek.Count > 0)
                        {
                            foreach (var item in akInvois.AkInvoisObjek)
                            {
                                akInvoisObjekBahagianList.Add(item.JKWPTJBahagian?.JBahagian ?? new JBahagian());
                            }

                        }
                        // if konfigKelulusan bahagian null, bypass all, add to results
                        if (konfigKelulusanBahagianList.Count == 0)
                        {
                            results.Add(akInvois);
                            continue;
                        }

                        // compare each lists, if its equal then insert to results
                        var items = akInvoisObjekBahagianList.All(konfigKelulusanBahagianList.Contains);
                        if (konfigKelulusanBahagianList.OrderBy(kk => kk.Kod).SequenceEqual(akInvoisObjekBahagianList.OrderBy(pp => pp.Kod))
                            || akInvoisObjekBahagianList.All(konfigKelulusanBahagianList.Contains))
                        {

                            results.Add(akInvois);
                            continue;
                        };
                    }
                }
            }


            return results;
        }


        public List<AkInvois> FilterByComparingJumlahAkInvoisWithMinAmountMaxAmountDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AkInvois> filterings)
        {
            //initialize new list akInvois
            List<AkInvois> results = new List<AkInvois>();

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

        public List<AkInvois> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang)
        {
            if (searchString == null && dateFrom == null && dateTo == null && orderBy == null)
            {
                return new List<AkInvois>();
            }

            var akInvoisList = _context.AkInvois
                .IgnoreQueryFilters()
                .Include(t => t.JKW)
                .Include(t => t.LHDNMSIC)
                .Include(t => t.LHDNEInvois)
                .Include(t => t.DDaftarAwam)
                .Include(t => t.DPekerjaPosting)
                .Include(t => t.AkAkaunAkru)
                .Include(t => t.DPengesah)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPenyemak)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPelulus)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.AkInvoisObjek)!
                    .ThenInclude(to => to.AkCarta)
                .Include(t => t.AkInvoisObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JKW)
                .Include(t => t.AkInvoisObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JPTJ)
                .Include(t => t.AkInvoisObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JBahagian)
                        .Where(t => t.Tarikh >= dateFrom && t.Tarikh <= dateTo!.Value.AddHours(23.99))
                .ToList();

            // searchstring filters
            if (searchString != null)
            {
                akInvoisList = akInvoisList.Where(t =>
                t.NoRujukan!.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                || t.DDaftarAwam!.Nama!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            // searchString filters end

            // status borang filters
            switch (enStatusBorang)
            {
                case EnStatusBorang.None:
                    akInvoisList = akInvoisList.Where(pp => pp.EnStatusBorang == EnStatusBorang.None).ToList();
                    break;
                case EnStatusBorang.Sah:
                    akInvoisList = akInvoisList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Sah).ToList();
                    break;
                case EnStatusBorang.Semak:
                    akInvoisList = akInvoisList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Semak).ToList();
                    break;
                case EnStatusBorang.Lulus:
                    akInvoisList = akInvoisList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Lulus).ToList();
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
                        akInvoisList = akInvoisList.OrderBy(t => t.DDaftarAwam!.Nama).ToList();
                        break;
                    case "Tarikh":
                        akInvoisList = akInvoisList.OrderBy(t => t.Tarikh).ToList(); break;
                    default:
                        akInvoisList = akInvoisList.OrderBy(t => t.NoRujukan).ToList();
                        break;
                }

            }
            // order by filters end

            return akInvoisList;
        }


        public AkInvois GetDetailsById(int id)
        {
            return _context.AkInvois
                .IgnoreQueryFilters()
                .Include(t => t.JKW)
                .Include(t => t.DDaftarAwam)
                .Include(t => t.DPekerjaPosting)
                .Include(t => t.AkAkaunAkru)
                .Include(t => t.LHDNMSIC)
                .Include(t => t.DPengesah)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPenyemak)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPelulus)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.AkInvoisObjek)!
                    .ThenInclude(to => to.AkCarta)
                .Include(t => t.AkInvoisObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JKW)
                .Include(t => t.AkInvoisObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JPTJ)
                .Include(t => t.AkInvoisObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JBahagian)
                .Include(t => t.AkInvoisPerihal)
                .FirstOrDefault(pp => pp.Id == id) ?? new AkInvois();
        }

        public async Task<bool> IsPostedAsync(int id, string noRujukan)
        {
            bool isPosted = await _context.AkInvois.AnyAsync(t => t.Id == id && t.FlPosting == 1);
            if (isPosted)
            {
                return true;
            }

            bool isExistInAbBukuVot = await _context.AbBukuVot.AnyAsync(b => b.NoRujukan == noRujukan);

            if (isExistInAbBukuVot)
            {
                return true;
            }

            bool isExistInAkAkaun = await _context.AkAkaun.AnyAsync(b => b.NoRujukan == noRujukan);

            if (isExistInAkAkaun)
            {
                return true;
            }

            return false;

        }

        public async Task<bool> IsLulusAsync(int id)
        {
            bool isLulus = await _context.AkInvois.AnyAsync(t => t.Id == id && t.EnStatusBorang == EnStatusBorang.Lulus);
            if (isLulus)
            {
                return true;
            }

            return false;
        }

        public void BatalLulus(int id, string? tindakan, string? userId)
        {
            var data = _context.AkInvois.FirstOrDefault(pp => pp.Id == id);

            if (data != null)
            {
                HantarSemula(id, tindakan, userId);

                RemovePostingFromAkAkaun(data);

            }
        }

        public void HantarSemula(int id, string? tindakan, string? userId)
        {
            var data = _context.AkInvois.FirstOrDefault(pp => pp.Id == id);

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

        public void RemovePostingFromAkAkaun(AkInvois akInvois)
        {
            var akAkaunList = _context.AkAkaun.Where(b => b.NoRujukan == akInvois.NoRujukan).ToList();

            if (akAkaunList != null && akAkaunList.Count > 0)
            {
                _context.RemoveRange(akAkaunList);
            }
        }

        public void BatalPos(int id, string? tindakan, string? userId)
        {
            var data = _context.AkInvois.FirstOrDefault(pp => pp.Id == id);

            if (data != null)
            {
                data.EnStatusBorang = EnStatusBorang.Kemaskini;
                data.Tindakan = tindakan;

                data.UserIdKemaskini = userId ?? "";
                data.TarKemaskini = DateTime.Now;

                data.FlPosting = 0;
                data.TarikhPosting = null;

                _context.Update(data);

                RemovePostingFromAkAkaun(data);

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

                PostingToAkAkaun(data);
            }
        }

        public void PostingToAkAkaun(AkInvois akInvois)
        {
            List<AkAkaun> akAkaunList = new List<AkAkaun>();

            if (akInvois.AkInvoisObjek != null && akInvois.AkInvoisObjek.Count > 0)
            {

                if (akInvois.AkAkaunAkruId != null)
                {

                    foreach (var item in akInvois.AkInvoisObjek)
                    {
                        AkAkaun akAkaun1 = new AkAkaun()
                        {
                            JKWId = akInvois.JKWId,
                            JPTJId = item.JKWPTJBahagian?.JPTJId,
                            JBahagianId = item.JKWPTJBahagian?.JBahagianId,
                            NoRujukan = akInvois.NoRujukan,
                            Tarikh = akInvois.Tarikh,
                            AkCarta1Id = (int)akInvois.AkAkaunAkruId,
                            AkCarta2Id = item.AkCartaId,
                            Debit = item.Amaun
                        };
                        akAkaunList.Add(akAkaun1);

                        AkAkaun akAkaun2 = new AkAkaun()
                        {
                            JKWId = akInvois.JKWId,
                            JPTJId = item.JKWPTJBahagian?.JPTJId,
                            JBahagianId = item.JKWPTJBahagian?.JBahagianId,
                            NoRujukan = akInvois.NoRujukan,
                            Tarikh = akInvois.Tarikh,
                            AkCarta1Id = item.AkCartaId,
                            AkCarta2Id = (int)akInvois.AkAkaunAkruId,
                            Kredit = item.Amaun
                        };

                        akAkaunList.Add(akAkaun2);
                    }
                }


                _context.AkAkaun.AddRange(akAkaunList);
            }
        }
    }
}