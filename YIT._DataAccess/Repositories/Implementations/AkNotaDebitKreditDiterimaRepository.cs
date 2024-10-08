using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class AkNotaDebitKreditDiterimaRepository : _GenericRepository<AkNotaDebitKreditDiterima>, IAkNotaDebitKreditDiterimaRepository
    {
        private readonly ApplicationDbContext _context;

        public AkNotaDebitKreditDiterimaRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public string GetMaxRefNo(string initNoRujukan, string tahun)
        {
            var max = _context.AkNotaDebitKreditDiterima.Where(pp => pp.Tahun == tahun).OrderByDescending(pp => pp.NoRujukan).ToList();

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

        public List<AkNotaDebitKreditDiterima> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang)
        {
            if (searchString == null && dateFrom == null && dateTo == null && orderBy == null)
            {
                return new List<AkNotaDebitKreditDiterima>();
            }

            var akNotaDebitKreditDiterimaList = _context.AkNotaDebitKreditDiterima
                .IgnoreQueryFilters()
                .Include(t => t.JKW)
                .Include(t => t.LHDNMSIC)
                .Include(t => t.LHDNEInvois)
                .Include(t => t.AkBelian)
                    .ThenInclude(b => b!.DDaftarAwam)
                .Include(t => t.DPekerjaPosting)
                //.Include(t => t.DPengesah)
                //    .ThenInclude(t => t!.DPekerja)
                //.Include(t => t.DPenyemak)
                //    .ThenInclude(t => t!.DPekerja)
                //.Include(t => t.DPelulus)
                //    .ThenInclude(t => t!.DPekerja)
                //.Include(t => t.AkNotaDebitKreditDiterimaObjek)!
                //    .ThenInclude(to => to.AkCarta)
                //.Include(t => t.AkNotaDebitKreditDiterimaObjek)!
                //    .ThenInclude(to => to.JKWPTJBahagian)
                //        .ThenInclude(b => b!.JKW)
                //.Include(t => t.AkNotaDebitKreditDiterimaObjek)!
                //    .ThenInclude(to => to.JKWPTJBahagian)
                //        .ThenInclude(b => b!.JPTJ)
                //.Include(t => t.AkNotaDebitKreditDiterimaObjek)!
                //    .ThenInclude(to => to.JKWPTJBahagian)
                //        .ThenInclude(b => b!.JBahagian)
                        .Where(t => t.Tarikh >= dateFrom && t.Tarikh <= dateTo!.Value.AddHours(23.99))
                .ToList();

            // searchstring filters
            if (searchString != null)
            {
                akNotaDebitKreditDiterimaList = akNotaDebitKreditDiterimaList.Where(t =>
                t.NoRujukan!.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                || t.AkBelian!.DDaftarAwam!.Nama!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            // searchString filters end

            // status borang filters
            switch (enStatusBorang)
            {
                case EnStatusBorang.None:
                    akNotaDebitKreditDiterimaList = akNotaDebitKreditDiterimaList.Where(pp => pp.EnStatusBorang == EnStatusBorang.None).ToList();
                    break;
                case EnStatusBorang.Sah:
                    akNotaDebitKreditDiterimaList = akNotaDebitKreditDiterimaList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Sah).ToList();
                    break;
                case EnStatusBorang.Semak:
                    akNotaDebitKreditDiterimaList = akNotaDebitKreditDiterimaList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Semak).ToList();
                    break;
                case EnStatusBorang.Lulus:
                    akNotaDebitKreditDiterimaList = akNotaDebitKreditDiterimaList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Lulus).ToList();
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
                        akNotaDebitKreditDiterimaList = akNotaDebitKreditDiterimaList.OrderBy(t => t.AkBelian!.DDaftarAwam!.Nama).ToList();
                        break;
                    case "Tarikh":
                        akNotaDebitKreditDiterimaList = akNotaDebitKreditDiterimaList.OrderBy(t => t.Tarikh).ToList(); break;
                    default:
                        akNotaDebitKreditDiterimaList = akNotaDebitKreditDiterimaList.OrderBy(t => t.NoRujukan).ToList();
                        break;
                }

            }
            // order by filters end

            return akNotaDebitKreditDiterimaList;
        }

        public AkNotaDebitKreditDiterima GetDetailsById(int id)
        {
            return _context.AkNotaDebitKreditDiterima
                .IgnoreQueryFilters()
                .Include(t => t.JKW)
                .Include(t => t.LHDNMSIC)
                .Include(t => t.LHDNEInvois)
                .Include(t => t.AkBelian)
                    .ThenInclude(b => b!.DDaftarAwam)
                .Include(t => t.DPekerjaPosting)
                .Include(t => t.DPengesah)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPenyemak)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPelulus)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.AkNotaDebitKreditDiterimaObjek)!
                    .ThenInclude(to => to.AkCarta)
                .Include(t => t.AkNotaDebitKreditDiterimaObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JKW)
                .Include(t => t.AkNotaDebitKreditDiterimaObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JPTJ)
                .Include(t => t.AkNotaDebitKreditDiterimaObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JBahagian)
                .Include(t => t.AkNotaDebitKreditDiterimaPerihal)
                .FirstOrDefault(pp => pp.Id == id) ?? new AkNotaDebitKreditDiterima();
        }

        public async Task<bool> IsLulusAsync(int id)
        {
            bool isLulus = await _context.AkNotaDebitKreditDiterima.AnyAsync(t => t.Id == id && t.EnStatusBorang == EnStatusBorang.Lulus);
            if (isLulus)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsPostedAsync(int id, string noRujukan)
        {
            bool isPosted = await _context.AkNotaDebitKreditDiterima.AnyAsync(t => t.Id == id && t.FlPosting == 1);
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
            bool isDenganTanggungan = false;

            if (data != null && data.AkBelian != null)
            {
                if (data.AkBelian.EnJenisBayaranBelian == EnJenisBayaranBelian.PO || data.AkBelian.EnJenisBayaranBelian == EnJenisBayaranBelian.Inden) isDenganTanggungan = true;

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

                PostingToAbBukuVot(data, isDenganTanggungan);

                PostingToAkAkaun(data);
            }
        }

        public void PostingToAbBukuVot(AkNotaDebitKreditDiterima akNotaDebitKreditDiterima, bool isDenganTanggungan)
        {
            List<AbBukuVot> abBukuVotList = new List<AbBukuVot>();

            if (akNotaDebitKreditDiterima.AkNotaDebitKreditDiterimaObjek != null && akNotaDebitKreditDiterima.AkNotaDebitKreditDiterimaObjek.Count > 0)
            {

                foreach (var item in akNotaDebitKreditDiterima.AkNotaDebitKreditDiterimaObjek)
                {
                    decimal amaunDebit = 0;
                    decimal amaunKredit = 0;

                    if (akNotaDebitKreditDiterima.FlDebitKredit == 0)
                    {
                        amaunDebit = item.Amaun;
                    }
                    else
                    {
                        amaunKredit = item.Amaun;
                    };

                    if (isDenganTanggungan)
                    {
                        AbBukuVot abBukuVot = new AbBukuVot()
                        {
                            Tahun = akNotaDebitKreditDiterima.Tahun,
                            JKWId = item.JKWPTJBahagian?.JKWId ?? akNotaDebitKreditDiterima.JKWId,
                            JPTJId = (int)item.JKWPTJBahagian!.JPTJId,
                            JBahagianId = item.JKWPTJBahagian.JBahagianId,
                            Tarikh = akNotaDebitKreditDiterima.Tarikh,
                            DDaftarAwamId = akNotaDebitKreditDiterima.AkBelian?.DDaftarAwamId,
                            VotId = item.AkCartaId,
                            NoRujukan = akNotaDebitKreditDiterima.NoRujukan,
                            //Debit = amaunDebit,
                            //Kredit = amaunKredit,
                            Tbs = amaunDebit - amaunKredit,
                            Liabiliti = amaunDebit - amaunKredit,
                            Belanja = amaunDebit - amaunKredit
                            // + BakiLiabiliti
                        };

                        abBukuVotList.Add(abBukuVot);
                    }
                    else
                    {
                        if (akNotaDebitKreditDiterima.AkBelian?.AkAkaunAkruId != null)
                        {
                            AbBukuVot abBukuVot = new AbBukuVot()
                            {
                                Tahun = akNotaDebitKreditDiterima.Tahun,
                                JKWId = item.JKWPTJBahagian?.JKWId ?? akNotaDebitKreditDiterima.JKWId,
                                JPTJId = (int)item.JKWPTJBahagian!.JPTJId,
                                JBahagianId = item.JKWPTJBahagian.JBahagianId,
                                Tarikh = akNotaDebitKreditDiterima.Tarikh,
                                DDaftarAwamId = akNotaDebitKreditDiterima.AkBelian?.DDaftarAwamId,
                                VotId = item.AkCartaId,
                                NoRujukan = akNotaDebitKreditDiterima.NoRujukan,
                                //Debit = amaunDebit,
                                //Kredit = amaunKredit,
                                Liabiliti = amaunDebit - amaunKredit,
                                Belanja = amaunDebit - amaunKredit,
                                Baki = amaunDebit - amaunKredit
                                // +BakiLiabiliti
                            };

                            abBukuVotList.Add(abBukuVot);
                        }

                    }

                }
            }

            _context.AbBukuVot.AddRange(abBukuVotList);
        }

        public void PostingToAkAkaun(AkNotaDebitKreditDiterima akNotaDebitKreditDiterima)
        {
            List<AkAkaun> akAkaunList = new List<AkAkaun>();

            if (akNotaDebitKreditDiterima.AkNotaDebitKreditDiterimaObjek != null && akNotaDebitKreditDiterima.AkNotaDebitKreditDiterimaObjek.Count > 0)
            {

                if (akNotaDebitKreditDiterima.AkBelian != null && akNotaDebitKreditDiterima.AkBelian.AkAkaunAkruId != null)
                {

                    foreach (var item in akNotaDebitKreditDiterima.AkNotaDebitKreditDiterimaObjek)
                    {
                        var kodDebit = 0;
                        var kodKredit = 0;

                        if (akNotaDebitKreditDiterima.FlDebitKredit == 0)
                        {
                            kodDebit = (int)akNotaDebitKreditDiterima.AkBelian.AkAkaunAkruId;
                            kodKredit = item.AkCartaId;
                        }
                        else
                        {
                            kodDebit = item.AkCartaId;
                            kodKredit = (int)akNotaDebitKreditDiterima.AkBelian.AkAkaunAkruId;
                        };

                        AkAkaun akAkaun1 = new AkAkaun()
                        {
                            JKWId = akNotaDebitKreditDiterima.JKWId,
                            JPTJId = item.JKWPTJBahagian?.JPTJId,
                            JBahagianId = item.JKWPTJBahagian?.JBahagianId,
                            NoRujukan = akNotaDebitKreditDiterima.NoRujukan,
                            Tarikh = akNotaDebitKreditDiterima.Tarikh,
                            AkCarta1Id = kodDebit,
                            AkCarta2Id = kodKredit,
                            Kredit = item.Amaun
                        };
                        akAkaunList.Add(akAkaun1);

                        AkAkaun akAkaun2 = new AkAkaun()
                        {
                            JKWId = akNotaDebitKreditDiterima.JKWId,
                            JPTJId = item.JKWPTJBahagian?.JPTJId,
                            JBahagianId = item.JKWPTJBahagian?.JBahagianId,
                            NoRujukan = akNotaDebitKreditDiterima.NoRujukan,
                            Tarikh = akNotaDebitKreditDiterima.Tarikh,
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
            var data = _context.AkNotaDebitKreditDiterima.FirstOrDefault(pp => pp.Id == id);

            if (data != null)
            {
                HantarSemula(id, tindakan, userId);

                RemovePostingFromAbBukuVot(data, userId ?? "");
                RemovePostingFromAkAkaun(data);

            }
        }

        public void BatalPos(int id, string? tindakan, string? userId)
        {
            var data = _context.AkNotaDebitKreditDiterima.FirstOrDefault(pp => pp.Id == id);

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
                RemovePostingFromAkAkaun(data);

            }
        }

        public void RemovePostingFromAbBukuVot(AkNotaDebitKreditDiterima akNotaDebitKreditDiterima, string userId)
        {
            var abBukuVotList = _context.AbBukuVot.Where(b => b.NoRujukan == akNotaDebitKreditDiterima.NoRujukan).ToList();

            if (abBukuVotList != null && abBukuVotList.Count > 0)
            {
                _context.RemoveRange(abBukuVotList);
            }

        }

        public void RemovePostingFromAkAkaun(AkNotaDebitKreditDiterima akNotaDebitKreditDiterima)
        {
            var akAkaunList = _context.AkAkaun.Where(b => b.NoRujukan == akNotaDebitKreditDiterima.NoRujukan).ToList();

            if (akAkaunList != null && akAkaunList.Count > 0)
            {
                _context.RemoveRange(akAkaunList);
            }
        }


        public void HantarSemula(int id, string? tindakan, string? userId)
        {
            var data = _context.AkNotaDebitKreditDiterima.FirstOrDefault(pp => pp.Id == id);

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


        public List<AkNotaDebitKreditDiterima> GetResultsByDPekerjaIdFromDKonfigKelulusan(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang, int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan)
        {

            // get all data with details
            List<AkNotaDebitKreditDiterima> akNotaDebitKreditDiterimaList = GetResults(searchString, dateFrom, dateTo, orderBy, enStatusBorang);

            var filterings = FilterByComparingJBahagianAkNotaDebitKreditDiterimaObjekWithJBahagianDKonfigKelulusan(dPekerjaId, enKategoriKelulusan, enJenisModulKelulusan, akNotaDebitKreditDiterimaList);

            var results = FilterByComparingJumlahAkNotaDebitKreditDiterimaWithMinAmountMaxAmountDKonfigKelulusan(dPekerjaId, enKategoriKelulusan, enJenisModulKelulusan, filterings);

            return results;
        }

        public List<AkNotaDebitKreditDiterima> FilterByComparingJBahagianAkNotaDebitKreditDiterimaObjekWithJBahagianDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AkNotaDebitKreditDiterima> akNotaDebitKreditDiterimaList)
        {
            // initialize result sets
            List<AkNotaDebitKreditDiterima> results = new List<AkNotaDebitKreditDiterima>();

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

                var akNotaDebitKreditDiterimaGroup = new List<AkNotaDebitKreditDiterimaObjek>().GroupBy(objek => objek.JKWPTJBahagianId);
                if (akNotaDebitKreditDiterimaList != null && akNotaDebitKreditDiterimaList.Count > 0)
                {
                    foreach (var akNotaDebitKreditDiterima in akNotaDebitKreditDiterimaList)
                    {
                        var akNotaDebitKreditDiterimaObjekBahagianList = new List<JBahagian>();

                        // group akNotaDebitKreditDiterimaObjek by bahagian
                        if (akNotaDebitKreditDiterima.AkNotaDebitKreditDiterimaObjek != null && akNotaDebitKreditDiterima.AkNotaDebitKreditDiterimaObjek.Count > 0)
                        {
                            foreach (var item in akNotaDebitKreditDiterima.AkNotaDebitKreditDiterimaObjek)
                            {
                                akNotaDebitKreditDiterimaObjekBahagianList.Add(item.JKWPTJBahagian?.JBahagian ?? new JBahagian());
                            }

                        }
                        // if konfigKelulusan bahagian null, bypass all, add to results
                        if (konfigKelulusanBahagianList.Count == 0)
                        {
                            results.Add(akNotaDebitKreditDiterima);
                            continue;
                        }

                        // compare each lists, if its equal then insert to results
                        var items = akNotaDebitKreditDiterimaObjekBahagianList.All(konfigKelulusanBahagianList.Contains);
                        if (konfigKelulusanBahagianList.OrderBy(kk => kk.Kod).SequenceEqual(akNotaDebitKreditDiterimaObjekBahagianList.OrderBy(pp => pp.Kod))
                            || akNotaDebitKreditDiterimaObjekBahagianList.All(konfigKelulusanBahagianList.Contains))
                        {

                            results.Add(akNotaDebitKreditDiterima);
                            continue;
                        };
                    }
                }
            }


            return results;
        }


        public List<AkNotaDebitKreditDiterima> FilterByComparingJumlahAkNotaDebitKreditDiterimaWithMinAmountMaxAmountDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AkNotaDebitKreditDiterima> filterings)
        {
            //initialize new list akNotaDebitKreditDiterima
            List<AkNotaDebitKreditDiterima> results = new List<AkNotaDebitKreditDiterima>();

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