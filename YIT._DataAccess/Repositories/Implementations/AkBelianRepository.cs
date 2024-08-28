using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class AkBelianRepository : _GenericRepository<AkBelian>, IAkBelianRepository
    {
        private readonly ApplicationDbContext _context;

        public AkBelianRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public AkBelian GetDetailsById(int id)
        {
            return _context.AkBelian
                .IgnoreQueryFilters()
                .Include(t => t.JKW)
                .Include(t => t.LHDNMSIC)
                .Include(t => t.LHDNEInvois)
                .Include(t => t.DDaftarAwam)
                .Include(t => t.DPekerjaPosting)
                .Include(t => t.AkAkaunAkru)
                .Include(t => t.AkInden)
                .Include(t => t.AkPO)
                .Include(t => t.AkNotaMinta)
                .Include(t => t.DPengesah)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPenyemak)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPelulus)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.AkBelianObjek)!
                    .ThenInclude(to => to.AkCarta)
                .Include(t => t.AkBelianObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JKW)
                .Include(t => t.AkBelianObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JPTJ)
                .Include(t => t.AkBelianObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JBahagian)
                .Include(t => t.AkBelianPerihal)
                .FirstOrDefault(pp => pp.Id == id) ?? new AkBelian();
        }

        public List<AkBelian> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang)
        {
            if (searchString == null && dateFrom == null && dateTo == null && orderBy == null)
            {
                return new List<AkBelian>();
            }

            var akBelianList = _context.AkBelian
                .IgnoreQueryFilters()
                .Include(t => t.JKW)
                .Include(t => t.LHDNMSIC)
                .Include(t => t.LHDNEInvois)
                .Include(t => t.DDaftarAwam)
                .Include(t => t.DPekerjaPosting)
                .Include(t => t.AkAkaunAkru)
                .Include(t => t.AkInden)
                .Include(t => t.AkPO)
                .Include(t => t.AkNotaMinta)
                .Include(t => t.DPengesah)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPenyemak)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPelulus)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.AkBelianObjek)!
                    .ThenInclude(to => to.AkCarta)
                .Include(t => t.AkBelianObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JKW)
                .Include(t => t.AkBelianObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JPTJ)
                .Include(t => t.AkBelianObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JBahagian)
                        .Where(t => t.Tarikh >= dateFrom && t.Tarikh <= dateTo!.Value.AddHours(23.99))
                .ToList();

            // searchstring filters
            if (searchString != null)
            {
                akBelianList = akBelianList.Where(t =>
                t.NoRujukan!.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                || t.DDaftarAwam!.Nama!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            // searchString filters end

            // status borang filters
            switch (enStatusBorang)
            {
                case EnStatusBorang.None:
                    akBelianList = akBelianList.Where(pp => pp.EnStatusBorang == EnStatusBorang.None).ToList();
                    break;
                case EnStatusBorang.Sah:
                    akBelianList = akBelianList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Sah).ToList();
                    break;
                case EnStatusBorang.Semak:
                    akBelianList = akBelianList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Semak).ToList();
                    break;
                case EnStatusBorang.Lulus:
                    akBelianList = akBelianList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Lulus).ToList();
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
                        akBelianList = akBelianList.OrderBy(t => t.DDaftarAwam!.Nama).ToList();
                        break;
                    case "Tarikh":
                        akBelianList = akBelianList.OrderBy(t => t.Tarikh).ToList(); break;
                    default:
                        akBelianList = akBelianList.OrderBy(t => t.NoRujukan).ToList();
                        break;
                }

            }
            // order by filters end

            return akBelianList;
        }

        public List<AkBelian> GetResultsByDPekerjaIdFromDKonfigKelulusan(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang, int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan)
        {

            // get all data with details
            List<AkBelian> akBelianList = GetResults(searchString, dateFrom, dateTo, orderBy, enStatusBorang);

            var filterings = FilterByComparingJBahagianAkPenilaianObjekWithJBahagianDKonfigKelulusan(dPekerjaId, enKategoriKelulusan, enJenisModulKelulusan, akBelianList);

            var results = FilterByComparingJumlahAkBelianWithMinAmountMaxAmountDKonfigKelulusan(dPekerjaId, enKategoriKelulusan, enJenisModulKelulusan, filterings);

            return results;
        }

        public List<AkBelian> FilterByComparingJBahagianAkPenilaianObjekWithJBahagianDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AkBelian> akBelianList)
        {
            // initialize result sets
            List<AkBelian> results = new List<AkBelian>();

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

                var akBelianGroup = new List<AkBelianObjek>().GroupBy(objek => objek.JKWPTJBahagianId);
                if (akBelianList != null && akBelianList.Count > 0)
                {
                    foreach (var akBelian in akBelianList)
                    {
                        var penilaianPerolehanObjekBahagianList = new List<JBahagian>();

                        // group akBelianObjek by bahagian
                        if (akBelian.AkBelianObjek != null && akBelian.AkBelianObjek.Count > 0)
                        {
                            foreach (var item in akBelian.AkBelianObjek)
                            {
                                penilaianPerolehanObjekBahagianList.Add(item.JKWPTJBahagian?.JBahagian ?? new JBahagian());
                            }

                        }
                        // if konfigKelulusan bahagian null, bypass all, add to results
                        if (konfigKelulusanBahagianList.Count == 0)
                        {
                            results.Add(akBelian);
                            continue;
                        }

                        // compare each lists, if its equal then insert to results
                        var items = penilaianPerolehanObjekBahagianList.All(konfigKelulusanBahagianList.Contains);
                        if (konfigKelulusanBahagianList.OrderBy(kk => kk.Kod).SequenceEqual(penilaianPerolehanObjekBahagianList.OrderBy(pp => pp.Kod))
                            || penilaianPerolehanObjekBahagianList.All(konfigKelulusanBahagianList.Contains))
                        {

                            results.Add(akBelian);
                            continue;
                        };
                    }
                }
            }


            return results;
        }


        public List<AkBelian> FilterByComparingJumlahAkBelianWithMinAmountMaxAmountDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AkBelian> filterings)
        {
            //initialize new list akBelian
            List<AkBelian> results = new List<AkBelian>();

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
            bool isSah = await _context.AkBelian.AnyAsync(t => t.Id == id && (t.EnStatusBorang == EnStatusBorang.Sah || t.EnStatusBorang == EnStatusBorang.Semak || t.EnStatusBorang == EnStatusBorang.Lulus));
            if (isSah)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsSemakAsync(int id)
        {
            bool isSemak = await _context.AkBelian.AnyAsync(t => t.Id == id && (t.EnStatusBorang == EnStatusBorang.Semak || t.EnStatusBorang == EnStatusBorang.Lulus));
            if (isSemak)
            {
                return true;
            }

            return false;
        }


        public async Task<bool> IsLulusAsync(int id)
        {
            bool isLulus = await _context.AkBelian.AnyAsync(t => t.Id == id && t.EnStatusBorang == EnStatusBorang.Lulus);
            if (isLulus)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsPostedAsync(int id, string noRujukan)
        {
            bool isPosted = await _context.AkBelian.AnyAsync(t => t.Id == id && t.FlPosting == 1);
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

        public void Sah(int id, int? pengesahId, string? userId)
        {
            var data = _context.AkBelian.FirstOrDefault(pp => pp.Id == id);
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
            var data = _context.AkBelian.FirstOrDefault(pp => pp.Id == id);
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
            bool isDenganTanggungan = false;

            if (data != null)
            {
                if (data.EnJenisBayaranBelian == EnJenisBayaranBelian.PO || data.EnJenisBayaranBelian == EnJenisBayaranBelian.Inden) isDenganTanggungan = true;

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

        public async Task<bool> IsBatalAsync(int id)
        {
            bool isBatal = await _context.AkBelian.AnyAsync(t => t.Id == id && t.FlBatal == 1);
            if (isBatal)
            {
                return true;
            }

            return false;
        }

        public void Batal(int id, string? sebabBatal, string? userId)
        {
            var data = _context.AkBelian.FirstOrDefault(pp => pp.Id == id);

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

        public string GetMaxRefNo(string initNoRujukan, string tahun)
        {
            var max = _context.AkBelian.Where(pp => pp.Tahun == tahun).OrderByDescending(pp => pp.NoRujukan).ToList();

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

        public void PostingToAbBukuVot(AkBelian akBelian, bool isDenganTanggungan)
        {
            List<AbBukuVot> abBukuVotList = new List<AbBukuVot>();

            if (akBelian.AkBelianObjek != null && akBelian.AkBelianObjek.Count > 0)
            {

                foreach (var item in akBelian.AkBelianObjek)
                {
                    if (isDenganTanggungan)
                    {
                        AbBukuVot abBukuVot = new AbBukuVot()
                        {
                            Tahun = akBelian.Tahun,
                            JKWId = item.JKWPTJBahagian?.JKWId ?? akBelian.JKWId,
                            JPTJId = (int)item.JKWPTJBahagian!.JPTJId,
                            JBahagianId = item.JKWPTJBahagian.JBahagianId,
                            Tarikh = akBelian.Tarikh,
                            DDaftarAwamId = akBelian.DDaftarAwamId,
                            VotId = item.AkCartaId,
                            NoRujukan = akBelian.NoRujukan,
                            Debit = item.Amaun,
                            Liabiliti = item.Amaun,
                            Belanja = item.Amaun
                            // + BakiLiabiliti
                        };

                        abBukuVotList.Add(abBukuVot);
                    }
                    else
                    {
                        if (akBelian.AkAkaunAkruId != null)
                        {
                            AbBukuVot abBukuVot = new AbBukuVot()
                            {
                                Tahun = akBelian.Tahun,
                                JKWId = item.JKWPTJBahagian?.JKWId ?? akBelian.JKWId,
                                JPTJId = (int)item.JKWPTJBahagian!.JPTJId,
                                JBahagianId = item.JKWPTJBahagian.JBahagianId,
                                Tarikh = akBelian.Tarikh,
                                DDaftarAwamId = akBelian.DDaftarAwamId,
                                VotId = item.AkCartaId,
                                NoRujukan = akBelian.NoRujukan,
                                Debit = item.Amaun,
                                Liabiliti = item.Amaun,
                                Belanja = item.Amaun,
                                Baki = -item.Amaun
                                // +BakiLiabiliti
                            };

                            abBukuVotList.Add(abBukuVot);
                        }

                    }

                }
            }

            _context.AbBukuVot.AddRange(abBukuVotList);
        }

        public void PostingToAkAkaun(AkBelian akBelian)
        {
            List<AkAkaun> akAkaunList = new List<AkAkaun>();

            if (akBelian.AkBelianObjek != null && akBelian.AkBelianObjek.Count > 0)
            {

                if (akBelian.AkAkaunAkruId != null)
                {

                    foreach (var item in akBelian.AkBelianObjek)
                    {
                        AkAkaun akAkaun1 = new AkAkaun()
                        {
                            JKWId = akBelian.JKWId,
                            JPTJId = item.JKWPTJBahagian?.JPTJId,
                            JBahagianId = item.JKWPTJBahagian?.JBahagianId,
                            NoRujukan = akBelian.NoRujukan,
                            Tarikh = akBelian.Tarikh,
                            AkCarta1Id = (int)akBelian.AkAkaunAkruId,
                            AkCarta2Id = item.AkCartaId,
                            Kredit = item.Amaun
                        };
                        akAkaunList.Add(akAkaun1);

                        AkAkaun akAkaun2 = new AkAkaun()
                        {
                            JKWId = akBelian.JKWId,
                            JPTJId = item.JKWPTJBahagian?.JPTJId,
                            JBahagianId = item.JKWPTJBahagian?.JBahagianId,
                            NoRujukan = akBelian.NoRujukan,
                            Tarikh = akBelian.Tarikh,
                            AkCarta1Id = item.AkCartaId,
                            AkCarta2Id = (int)akBelian.AkAkaunAkruId,
                            Debit = item.Amaun
                        };

                        akAkaunList.Add(akAkaun2);
                    }
                }


                _context.AkAkaun.AddRange(akAkaunList);
            }
        }
        public void RemovePostingFromAbBukuVot(AkBelian akBelian, string userId)
        {
            var abBukuVotList = _context.AbBukuVot.Where(b => b.NoRujukan == akBelian.NoRujukan).ToList();

            if (abBukuVotList != null && abBukuVotList.Count > 0)
            {
                _context.RemoveRange(abBukuVotList);
            }

        }

        public void RemovePostingFromAkAkaun(AkBelian akBelian)
        {
            var akAkaunList = _context.AkAkaun.Where(b => b.NoRujukan == akBelian.NoRujukan).ToList();

            if (akAkaunList != null && akAkaunList.Count > 0)
            {
                _context.RemoveRange(akAkaunList);
            }
        }

        public List<AkBelian> GetAllByStatus(EnStatusBorang enStatusBorang)
        {
            var result = _context.AkBelian.Include(b => b.AkPVInvois).Where(pp => pp.EnStatusBorang == enStatusBorang && pp.FlPosting == 1 && pp.FlBatal != 1).ToList();

            result = result.Where(pp => pp.AkPVInvois != null && pp.AkPVInvois.Count == 0).ToList();
            return result;
        }

        public void BatalPos(int id, string? tindakan, string? userId)
        {
            var data = _context.AkBelian.FirstOrDefault(pp => pp.Id == id);

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

        public void HantarSemula(int id, string? tindakan, string? userId)
        {
            var data = _context.AkBelian.FirstOrDefault(pp => pp.Id == id);

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
            var data = _context.AkBelian.FirstOrDefault(pp => pp.Id == id);

            if (data != null)
            {
                HantarSemula(id, tindakan, userId);

                RemovePostingFromAbBukuVot(data, userId ?? "");
                RemovePostingFromAkAkaun(data);

            }
        }

        public AkBelian GetBalanceAdjustmentFromAkDebitKreditDiterima(AkBelian akBelian)
        {
            var akNotaDebitKreditDiterimaObjekAkBelianObjekGrouped = new List<AkBelianObjek>();

            var akNotaDebitKreditDiterima = _context.AkNotaDebitKreditDiterima
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
                .FirstOrDefault(pp => pp.AkBelianId == akBelian.Id);

            if (akNotaDebitKreditDiterima != null)
            {
                if (akNotaDebitKreditDiterima.FlDebitKredit == 0)
                {
                    akBelian.Jumlah += akNotaDebitKreditDiterima.Jumlah;
                }
                else
                {
                    akBelian.Jumlah -= akNotaDebitKreditDiterima.Jumlah;
                }

                if (akNotaDebitKreditDiterima.AkNotaDebitKreditDiterimaObjek != null && akNotaDebitKreditDiterima.AkNotaDebitKreditDiterimaObjek.Count > 0 && akBelian.AkBelianObjek != null && akBelian.AkBelianObjek.Count > 0)
                {

                    foreach (var objek in akBelian.AkBelianObjek)
                    {
                        akNotaDebitKreditDiterimaObjekAkBelianObjekGrouped.Add(objek);
                    }

                    foreach (var objek in akNotaDebitKreditDiterima.AkNotaDebitKreditDiterimaObjek)
                    {
                        var akNotaDebitKreditDiterimaObjek = new AkBelianObjek
                        {
                            AkBelianId = akBelian.Id,
                            AkBelian = akBelian,
                            JKWPTJBahagianId = objek.JKWPTJBahagianId,
                            JKWPTJBahagian = objek.JKWPTJBahagian,
                            AkCartaId = objek.AkCartaId,
                            AkCarta = objek.AkCarta,
                            Amaun = objek.Amaun
                        };
                        akNotaDebitKreditDiterimaObjekAkBelianObjekGrouped.Add(akNotaDebitKreditDiterimaObjek);
                    }

                    akNotaDebitKreditDiterimaObjekAkBelianObjekGrouped = akNotaDebitKreditDiterimaObjekAkBelianObjekGrouped.GroupBy(b => new { b.AkCartaId, b.JKWPTJBahagianId }).Select(l => new AkBelianObjek
                    {
                        Id = l.First().Id,
                        AkBelianId = l.First().AkBelianId,
                        AkBelian = l.First().AkBelian,
                        JKWPTJBahagianId = l.First().JKWPTJBahagianId,
                        JKWPTJBahagian = l.First().JKWPTJBahagian,
                        AkCartaId = l.First().AkCartaId,
                        AkCarta = l.First().AkCarta,
                        Amaun = l.Sum(l => l.Amaun)
                    }).ToList();
                }

                akBelian.AkBelianObjek = akNotaDebitKreditDiterimaObjekAkBelianObjekGrouped;

            }

            return akBelian;
        }
    }
}