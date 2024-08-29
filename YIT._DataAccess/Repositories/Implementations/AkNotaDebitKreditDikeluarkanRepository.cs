using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class AkNotaDebitKreditDikeluarkanRepository : _GenericRepository<AkNotaDebitKreditDikeluarkan>, IAkNotaDebitKreditDikeluarkanRepository
    {
        private readonly ApplicationDbContext _context;

        public AkNotaDebitKreditDikeluarkanRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public string GetMaxRefNo(string initNoRujukan, string tahun)
        {
            var max = _context.AkNotaDebitKreditDikeluarkan.Where(pp => pp.Tahun == tahun).OrderByDescending(pp => pp.NoRujukan).ToList();

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

        public List<AkNotaDebitKreditDikeluarkan> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang)
        {
            if (searchString == null && dateFrom == null && dateTo == null && orderBy == null)
            {
                return new List<AkNotaDebitKreditDikeluarkan>();
            }

            var akNotaDebitKreditDikeluarkanList = _context.AkNotaDebitKreditDikeluarkan
                .IgnoreQueryFilters()
                .Include(t => t.JKW)
                .Include(t => t.LHDNMSIC)
                .Include(t => t.LHDNEInvois)
                .Include(t => t.AkInvois)
                    .ThenInclude(b => b!.DDaftarAwam)
                .Include(t => t.DPekerjaPosting)
                .Where(t => t.Tarikh >= dateFrom && t.Tarikh <= dateTo!.Value.AddHours(23.99))
                .ToList();

            // searchstring filters
            if (searchString != null)
            {
                akNotaDebitKreditDikeluarkanList = akNotaDebitKreditDikeluarkanList.Where(t =>
                t.NoRujukan!.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                || t.AkInvois!.DDaftarAwam!.Nama!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            // searchString filters end

            // status borang filters
            switch (enStatusBorang)
            {
                case EnStatusBorang.None:
                    akNotaDebitKreditDikeluarkanList = akNotaDebitKreditDikeluarkanList.Where(pp => pp.EnStatusBorang == EnStatusBorang.None).ToList();
                    break;
                case EnStatusBorang.Sah:
                    akNotaDebitKreditDikeluarkanList = akNotaDebitKreditDikeluarkanList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Sah).ToList();
                    break;
                case EnStatusBorang.Semak:
                    akNotaDebitKreditDikeluarkanList = akNotaDebitKreditDikeluarkanList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Semak).ToList();
                    break;
                case EnStatusBorang.Lulus:
                    akNotaDebitKreditDikeluarkanList = akNotaDebitKreditDikeluarkanList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Lulus).ToList();
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
                        akNotaDebitKreditDikeluarkanList = akNotaDebitKreditDikeluarkanList.OrderBy(t => t.AkInvois!.DDaftarAwam!.Nama).ToList();
                        break;
                    case "Tarikh":
                        akNotaDebitKreditDikeluarkanList = akNotaDebitKreditDikeluarkanList.OrderBy(t => t.Tarikh).ToList(); break;
                    default:
                        akNotaDebitKreditDikeluarkanList = akNotaDebitKreditDikeluarkanList.OrderBy(t => t.NoRujukan).ToList();
                        break;
                }

            }
            // order by filters end

            return akNotaDebitKreditDikeluarkanList;
        }

        public AkNotaDebitKreditDikeluarkan GetDetailsById(int id)
        {
            return _context.AkNotaDebitKreditDikeluarkan
                .IgnoreQueryFilters()
                .Include(t => t.JKW)
                .Include(t => t.LHDNMSIC)
                .Include(t => t.LHDNEInvois)
                .Include(t => t.AkInvois)
                    .ThenInclude(b => b!.DDaftarAwam)
                .Include(t => t.DPekerjaPosting)
                .Include(t => t.DPengesah)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPenyemak)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPelulus)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.AkNotaDebitKreditDikeluarkanObjek)!
                    .ThenInclude(to => to.AkCarta)
                .Include(t => t.AkNotaDebitKreditDikeluarkanObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JKW)
                .Include(t => t.AkNotaDebitKreditDikeluarkanObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JPTJ)
                .Include(t => t.AkNotaDebitKreditDikeluarkanObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JBahagian)
                .Include(t => t.AkNotaDebitKreditDikeluarkanPerihal)
                .FirstOrDefault(pp => pp.Id == id) ?? new AkNotaDebitKreditDikeluarkan();
        }

        public async Task<bool> IsLulusAsync(int id)
        {
            bool isLulus = await _context.AkNotaDebitKreditDikeluarkan.AnyAsync(t => t.Id == id && t.EnStatusBorang == EnStatusBorang.Lulus);
            if (isLulus)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsPostedAsync(int id, string noRujukan)
        {
            bool isPosted = await _context.AkNotaDebitKreditDikeluarkan.AnyAsync(t => t.Id == id && t.FlPosting == 1);
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

        public void Lulus(int id, int? pelulusId, string? userId)
        {
            var data = GetDetailsById(id);
            var pelulus = _context.DKonfigKelulusan.FirstOrDefault(kk => kk.DPekerjaId == pelulusId);

            if (data != null && data.AkInvois != null)
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

        public void PostingToAkAkaun(AkNotaDebitKreditDikeluarkan akNotaDebitKreditDikeluarkan)
        {
            List<AkAkaun> akAkaunList = new List<AkAkaun>();

            if (akNotaDebitKreditDikeluarkan.AkNotaDebitKreditDikeluarkanObjek != null && akNotaDebitKreditDikeluarkan.AkNotaDebitKreditDikeluarkanObjek.Count > 0)
            {

                if (akNotaDebitKreditDikeluarkan.AkInvois != null && akNotaDebitKreditDikeluarkan.AkInvois.AkAkaunAkruId != null)
                {

                    foreach (var item in akNotaDebitKreditDikeluarkan.AkNotaDebitKreditDikeluarkanObjek)
                    {
                        var kodDebit = 0;
                        var kodKredit = 0;

                        if (akNotaDebitKreditDikeluarkan.FlDebitKredit == 0)
                        {
                            kodDebit = (int)akNotaDebitKreditDikeluarkan.AkInvois.AkAkaunAkruId;
                            kodKredit = item.AkCartaId;
                        }
                        else
                        {
                            kodDebit = item.AkCartaId;
                            kodKredit = (int)akNotaDebitKreditDikeluarkan.AkInvois.AkAkaunAkruId;
                        };

                        AkAkaun akAkaun1 = new AkAkaun()
                        {
                            JKWId = akNotaDebitKreditDikeluarkan.JKWId,
                            JPTJId = item.JKWPTJBahagian?.JPTJId,
                            JBahagianId = item.JKWPTJBahagian?.JBahagianId,
                            NoRujukan = akNotaDebitKreditDikeluarkan.NoRujukan,
                            Tarikh = akNotaDebitKreditDikeluarkan.Tarikh,
                            AkCarta1Id = kodDebit,
                            AkCarta2Id = kodKredit,
                            Kredit = item.Amaun
                        };
                        akAkaunList.Add(akAkaun1);

                        AkAkaun akAkaun2 = new AkAkaun()
                        {
                            JKWId = akNotaDebitKreditDikeluarkan.JKWId,
                            JPTJId = item.JKWPTJBahagian?.JPTJId,
                            JBahagianId = item.JKWPTJBahagian?.JBahagianId,
                            NoRujukan = akNotaDebitKreditDikeluarkan.NoRujukan,
                            Tarikh = akNotaDebitKreditDikeluarkan.Tarikh,
                            AkCarta1Id = kodKredit,
                            AkCarta2Id = kodDebit,
                            Debit = item.Amaun
                        };

                        akAkaunList.Add(akAkaun2);
                    }
                }


                _context.AkAkaun.AddRange(akAkaunList);
            }
        }

        public void BatalLulus(int id, string? tindakan, string? userId)
        {
            var data = _context.AkNotaDebitKreditDikeluarkan.FirstOrDefault(pp => pp.Id == id);

            if (data != null)
            {
                HantarSemula(id, tindakan, userId);

                RemovePostingFromAkAkaun(data);

            }
        }

        public void BatalPos(int id, string? tindakan, string? userId)
        {
            var data = _context.AkNotaDebitKreditDikeluarkan.FirstOrDefault(pp => pp.Id == id);

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

        public void RemovePostingFromAkAkaun(AkNotaDebitKreditDikeluarkan akNotaDebitKreditDikeluarkan)
        {
            var akAkaunList = _context.AkAkaun.Where(b => b.NoRujukan == akNotaDebitKreditDikeluarkan.NoRujukan).ToList();

            if (akAkaunList != null && akAkaunList.Count > 0)
            {
                _context.RemoveRange(akAkaunList);
            }
        }


        public void HantarSemula(int id, string? tindakan, string? userId)
        {
            var data = _context.AkNotaDebitKreditDikeluarkan.FirstOrDefault(pp => pp.Id == id);

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


        public List<AkNotaDebitKreditDikeluarkan> GetResultsByDPekerjaIdFromDKonfigKelulusan(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang, int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan)
        {

            // get all data with details
            List<AkNotaDebitKreditDikeluarkan> akNotaDebitKreditDikeluarkanList = GetResults(searchString, dateFrom, dateTo, orderBy, enStatusBorang);

            var filterings = FilterByComparingJBahagianAkNotaDebitKreditDikeluarkanObjekWithJBahagianDKonfigKelulusan(dPekerjaId, enKategoriKelulusan, enJenisModulKelulusan, akNotaDebitKreditDikeluarkanList);

            var results = FilterByComparingJumlahAkNotaDebitKreditDikeluarkanWithMinAmountMaxAmountDKonfigKelulusan(dPekerjaId, enKategoriKelulusan, enJenisModulKelulusan, filterings);

            return results;
        }

        public List<AkNotaDebitKreditDikeluarkan> FilterByComparingJBahagianAkNotaDebitKreditDikeluarkanObjekWithJBahagianDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AkNotaDebitKreditDikeluarkan> akNotaDebitKreditDikeluarkanList)
        {
            // initialize result sets
            List<AkNotaDebitKreditDikeluarkan> results = new List<AkNotaDebitKreditDikeluarkan>();

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

                var akNotaDebitKreditDikeluarkanGroup = new List<AkNotaDebitKreditDikeluarkanObjek>().GroupBy(objek => objek.JKWPTJBahagianId);
                if (akNotaDebitKreditDikeluarkanList != null && akNotaDebitKreditDikeluarkanList.Count > 0)
                {
                    foreach (var akNotaDebitKreditDikeluarkan in akNotaDebitKreditDikeluarkanList)
                    {
                        var akNotaDebitKreditDikeluarkanObjekBahagianList = new List<JBahagian>();

                        // group akNotaDebitKreditDikeluarkanObjek by bahagian
                        if (akNotaDebitKreditDikeluarkan.AkNotaDebitKreditDikeluarkanObjek != null && akNotaDebitKreditDikeluarkan.AkNotaDebitKreditDikeluarkanObjek.Count > 0)
                        {
                            foreach (var item in akNotaDebitKreditDikeluarkan.AkNotaDebitKreditDikeluarkanObjek)
                            {
                                akNotaDebitKreditDikeluarkanObjekBahagianList.Add(item.JKWPTJBahagian?.JBahagian ?? new JBahagian());
                            }

                        }
                        // if konfigKelulusan bahagian null, bypass all, add to results
                        if (konfigKelulusanBahagianList.Count == 0)
                        {
                            results.Add(akNotaDebitKreditDikeluarkan);
                            continue;
                        }

                        // compare each lists, if its equal then insert to results
                        var items = akNotaDebitKreditDikeluarkanObjekBahagianList.All(konfigKelulusanBahagianList.Contains);
                        if (konfigKelulusanBahagianList.OrderBy(kk => kk.Kod).SequenceEqual(akNotaDebitKreditDikeluarkanObjekBahagianList.OrderBy(pp => pp.Kod))
                            || akNotaDebitKreditDikeluarkanObjekBahagianList.All(konfigKelulusanBahagianList.Contains))
                        {

                            results.Add(akNotaDebitKreditDikeluarkan);
                            continue;
                        };
                    }
                }
            }


            return results;
        }


        public List<AkNotaDebitKreditDikeluarkan> FilterByComparingJumlahAkNotaDebitKreditDikeluarkanWithMinAmountMaxAmountDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AkNotaDebitKreditDikeluarkan> filterings)
        {
            //initialize new list akNotaDebitKreditDikeluarkan
            List<AkNotaDebitKreditDikeluarkan> results = new List<AkNotaDebitKreditDikeluarkan>();

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

    }
}