using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class AkJurnalRepository : _GenericRepository<AkJurnal>, IAkJurnalRepository
    {
        private readonly ApplicationDbContext _context;

        public AkJurnalRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public AkJurnal GetDetailsById(int id)
        {
            return _context.AkJurnal
                .IgnoreQueryFilters()
                .Include(j => j.JKW)
                .Include(j => j.AkJurnalObjek)!
                    .ThenInclude(jo => jo.AkCartaDebit)
                .Include(j => j.AkJurnalObjek)!
                    .ThenInclude(jo => jo.AkCartaKredit)
                .Include(j => j.AkJurnalObjek)!
                    .ThenInclude(jo => jo.JKWPTJBahagianDebit)
                .Include(j => j.AkJurnalObjek)!
                    .ThenInclude(jo => jo.JKWPTJBahagianKredit)
                .Include(j => j.AkJurnalPenerimaCekBatal)!
                    .ThenInclude(jo => jo.AkPV)
                .FirstOrDefault(j => j.Id.Equals(id)) ?? new AkJurnal();
        }

        public List<AkJurnal> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang)
        {
            if (searchString == null && dateFrom == null && dateTo == null) return new List<AkJurnal>();

            var akJurnalList = _context.AkJurnal
                .IgnoreQueryFilters()
                .Include(j => j.JKW)
                .Where(j => j.Tarikh >= dateFrom && j.Tarikh <= dateTo!.Value.AddHours(23.99))
                .ToList();


            // searchstring filters
            if (searchString != null)
            {
                akJurnalList = akJurnalList.Where(t =>
                t.NoRujukan!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            // searchString filters end

            // status borang filters
            switch (enStatusBorang)
            {
                case EnStatusBorang.None:
                    akJurnalList = akJurnalList.Where(pp => pp.EnStatusBorang == EnStatusBorang.None).ToList();
                    break;
                case EnStatusBorang.Sah:
                    akJurnalList = akJurnalList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Sah).ToList();
                    break;
                case EnStatusBorang.Semak:
                    akJurnalList = akJurnalList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Semak).ToList();
                    break;
                case EnStatusBorang.Lulus:
                    akJurnalList = akJurnalList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Lulus).ToList();
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
                        akJurnalList = akJurnalList.OrderBy(t => t.Tarikh).ToList(); break;
                    default:
                        akJurnalList = akJurnalList.OrderBy(t => t.NoRujukan).ToList();
                        break;
                }

            }
            // order by filters end

            return akJurnalList;
        }

        public List<AkJurnal> GetResultsByDPekerjaIdFromDKonfigKelulusan(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang, int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan)
        {

            // get all data with details
            List<AkJurnal> akJurnalList = GetResults(searchString, dateFrom, dateTo, orderBy, enStatusBorang);

            var filterings = FilterByComparingJBahagianAkPenilaianObjekWithJBahagianDKonfigKelulusan(dPekerjaId, enKategoriKelulusan, enJenisModulKelulusan, akJurnalList);

            var results = FilterByComparingJumlahAkJurnalWithMinAmountMaxAmountDKonfigKelulusan(dPekerjaId, enKategoriKelulusan, enJenisModulKelulusan, filterings);

            return results;
        }

        public List<AkJurnal> FilterByComparingJBahagianAkPenilaianObjekWithJBahagianDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AkJurnal> akJurnalList)
        {
            // initialize result sets
            List<AkJurnal> results = new List<AkJurnal>();

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
                // bahagian debit
                if (akJurnalList != null && akJurnalList.Count > 0)
                {
                    foreach (var akJurnal in akJurnalList)
                    {
                        var jurnalObjekBahagianDebitList = new List<JBahagian>();

                        // group akJurnalObjek by bahagian
                        if (akJurnal.AkJurnalObjek != null && akJurnal.AkJurnalObjek.Count > 0)
                        {
                            foreach (var item in akJurnal.AkJurnalObjek)
                            {
                                jurnalObjekBahagianDebitList.Add(item.JKWPTJBahagianDebit?.JBahagian ?? new JBahagian());
                            }

                        }
                        // if konfigKelulusan bahagian null, bypass all, add to results
                        if (konfigKelulusanBahagianList.Count == 0)
                        {
                            results.Add(akJurnal);
                            continue;
                        }

                        // compare each lists, if its equal then insert to results
                        var items = jurnalObjekBahagianDebitList.All(konfigKelulusanBahagianList.Contains);
                        if (konfigKelulusanBahagianList.OrderBy(kk => kk.Kod).SequenceEqual(jurnalObjekBahagianDebitList.OrderBy(pp => pp.Kod))
                            || jurnalObjekBahagianDebitList.All(konfigKelulusanBahagianList.Contains))
                        {

                            results.Add(akJurnal);
                            continue;
                        };
                    }
                }
                //

                // bahagian kredit
                if (akJurnalList != null && akJurnalList.Count > 0)
                {
                    foreach (var akJurnal in akJurnalList)
                    {
                        var jurnalObjekBahagianKreditList = new List<JBahagian>();

                        // group akJurnalObjek by bahagian
                        if (akJurnal.AkJurnalObjek != null && akJurnal.AkJurnalObjek.Count > 0)
                        {
                            foreach (var item in akJurnal.AkJurnalObjek)
                            {
                                jurnalObjekBahagianKreditList.Add(item.JKWPTJBahagianKredit?.JBahagian ?? new JBahagian());
                            }

                        }
                        // if konfigKelulusan bahagian null, bypass all, add to results
                        if (konfigKelulusanBahagianList.Count == 0)
                        {
                            results.Add(akJurnal);
                            continue;
                        }

                        // compare each lists, if its equal then insert to results
                        var items = jurnalObjekBahagianKreditList.All(konfigKelulusanBahagianList.Contains);
                        if (konfigKelulusanBahagianList.OrderBy(kk => kk.Kod).SequenceEqual(jurnalObjekBahagianKreditList.OrderBy(pp => pp.Kod))
                            || jurnalObjekBahagianKreditList.All(konfigKelulusanBahagianList.Contains))
                        {

                            results.Add(akJurnal);
                            continue;
                        };
                    }
                }
                //
            }


            return results;
        }


        public List<AkJurnal> FilterByComparingJumlahAkJurnalWithMinAmountMaxAmountDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AkJurnal> filterings)
        {
            //initialize new list akJurnal
            List<AkJurnal> results = new List<AkJurnal>();

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
                            if (konfigKelulusan.MinAmaun <= filtering.JumlahDebit && filtering.JumlahDebit <= konfigKelulusan.MaksAmaun &&
                                konfigKelulusan.MinAmaun <= filtering.JumlahKredit && filtering.JumlahKredit <= konfigKelulusan.MaksAmaun)
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
            bool isSah = await _context.AkJurnal.AnyAsync(t => t.Id == id && (t.EnStatusBorang == EnStatusBorang.Sah || t.EnStatusBorang == EnStatusBorang.Semak || t.EnStatusBorang == EnStatusBorang.Lulus));
            if (isSah)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsSemakAsync(int id)
        {
            bool isSemak = await _context.AkJurnal.AnyAsync(t => t.Id == id && (t.EnStatusBorang == EnStatusBorang.Semak || t.EnStatusBorang == EnStatusBorang.Lulus));
            if (isSemak)
            {
                return true;
            }

            return false;
        }


        public async Task<bool> IsLulusAsync(int id)
        {
            bool isLulus = await _context.AkJurnal.AnyAsync(t => t.Id == id && t.EnStatusBorang == EnStatusBorang.Lulus);
            if (isLulus)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsPostedAsync(int id, string noRujukan)
        {
            bool isPosted = await _context.AkJurnal.AnyAsync(t => t.Id == id && t.FlPosting == 1);
            if (isPosted)
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
            var data = _context.AkJurnal.FirstOrDefault(pp => pp.Id == id);
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
            var data = _context.AkJurnal.FirstOrDefault(pp => pp.Id == id);
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

                PostingToAkAkaun(data);
            }
        }
        public void PostingToAkAkaun(AkJurnal akJurnal)
        {
            List<AkAkaun> akAkaunList = new List<AkAkaun>();

            if (akJurnal.AkJurnalObjek != null && akJurnal.AkJurnalObjek.Count > 0)
            {


                foreach (var item in akJurnal.AkJurnalObjek)
                {
                    AkAkaun akAkaun1 = new AkAkaun()
                    {
                        JKWId = akJurnal.JKWId,
                        JPTJId = item.JKWPTJBahagianKredit?.JPTJId,
                        JBahagianId = item.JKWPTJBahagianKredit?.JBahagianId,
                        NoRujukan = akJurnal.NoRujukan,
                        Tarikh = akJurnal.Tarikh,
                        AkCarta1Id = item.AkCartaKreditId,
                        AkCarta2Id = item.AkCartaDebitId,
                        Kredit = item.Amaun
                    };
                    akAkaunList.Add(akAkaun1);

                    AkAkaun akAkaun2 = new AkAkaun()
                    {
                        JKWId = akJurnal.JKWId,
                        JPTJId = item.JKWPTJBahagianDebit?.JPTJId,
                        JBahagianId = item.JKWPTJBahagianDebit?.JBahagianId,
                        NoRujukan = akJurnal.NoRujukan,
                        Tarikh = akJurnal.Tarikh,
                        AkCarta1Id = item.AkCartaDebitId,
                        AkCarta2Id = item.AkCartaKreditId,
                        Debit = item.Amaun
                    };

                    akAkaunList.Add(akAkaun2);
                }
                _context.AkAkaun.AddRange(akAkaunList);
            }
        }

        public void PostingToAbBukuVot(AkJurnal akJurnal)
        {
            List<AbBukuVot> abBukuVotList = new List<AbBukuVot>();

            if (akJurnal.AkJurnalObjek != null && akJurnal.AkJurnalObjek.Count > 0)
            {


                foreach (var item in akJurnal.AkJurnalObjek)
                {
                    if (item.IsDebitAbBukuVot)
                    {
                        AbBukuVot abBukuVot = new AbBukuVot()
                        {
                            Tahun = akJurnal.Tarikh.ToString("yyyy"),
                            JKWId = item.JKWPTJBahagianDebit?.JKWId ?? akJurnal.JKWId,
                            JPTJId = (int)item.JKWPTJBahagianDebit!.JPTJId,
                            JBahagianId = item.JKWPTJBahagianDebit.JBahagianId,
                            Tarikh = akJurnal.Tarikh,
                            VotId = item.AkCartaDebitId,
                            NoRujukan = akJurnal.NoRujukan,
                            Debit = item.Amaun
                        };

                        abBukuVotList.Add(abBukuVot);
                    }

                    if (item.IsKreditAbBukuVot)
                    {
                        AbBukuVot abBukuVot = new AbBukuVot()
                        {
                            Tahun = akJurnal.Tarikh.ToString("yyyy"),
                            JKWId = item.JKWPTJBahagianKredit?.JKWId ?? akJurnal.JKWId,
                            JPTJId = (int)item.JKWPTJBahagianKredit!.JPTJId,
                            JBahagianId = item.JKWPTJBahagianKredit.JBahagianId,
                            Tarikh = akJurnal.Tarikh,
                            VotId = item.AkCartaKreditId,
                            NoRujukan = akJurnal.NoRujukan,
                            Kredit = item.Amaun
                        };

                        abBukuVotList.Add(abBukuVot);
                    }
                    
                }
            }

            _context.AbBukuVot.AddRange(abBukuVotList);
        }
        public async Task<bool> IsBatalAsync(int id)
        {
            bool isBatal = await _context.AkJurnal.AnyAsync(t => t.Id == id && t.FlBatal == 1);
            if (isBatal)
            {
                return true;
            }

            return false;
        }

        public void Batal(int id, string? sebabBatal, string? userId)
        {
            var data = _context.AkJurnal.FirstOrDefault(pp => pp.Id == id);

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
            var max = _context.AkJurnal.Where(pp => pp.Tarikh.Year.ToString() == tahun).OrderByDescending(pp => pp.NoRujukan).ToList();

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

        public void RemovePostingFromAkAkaun(AkJurnal akJurnal)
        {
            var akAkaunList = _context.AkAkaun.Where(b => b.NoRujukan == akJurnal.NoRujukan).ToList();

            if (akAkaunList != null && akAkaunList.Count > 0)
            {
                _context.RemoveRange(akAkaunList);
            }
        }

        public List<AkJurnal> GetAllByStatus(EnStatusBorang enStatusBorang)
        {
            return _context.AkJurnal.Where(pp => pp.EnStatusBorang == enStatusBorang).ToList();
        }

        public void BatalPos(int id, string? tindakan, string? userId)
        {
            var data = _context.AkJurnal.FirstOrDefault(pp => pp.Id == id);

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

        public void HantarSemula(int id, string? tindakan, string? userId)
        {
            var data = _context.AkJurnal.FirstOrDefault(pp => pp.Id == id);

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
            var data = _context.AkJurnal.FirstOrDefault(pp => pp.Id == id);

            if (data != null)
            {
                HantarSemula(id, tindakan, userId);

                RemovePostingFromAkAkaun(data);

            }
        }
    }
}