using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class AkPVRepository : _GenericRepository<AkPV>, IAkPVRepository
    {
        ApplicationDbContext _context;
        public AkPVRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Batal(int id, string? sebabBatal, string? userId)
        {
            var data = _context.AkPV.FirstOrDefault(pp => pp.Id == id);

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

        public void BatalLulus(int id, string? tindakan, string? userId)
        {
            var data = _context.AkPV.FirstOrDefault(pp => pp.Id == id);

            if (data != null)
            {
                HantarSemula(id, tindakan, userId);

                RemovePostingFromAbBukuVot(data);
                RemovePostingFromAkAkaun(data);


            }
        }

        public void BatalPos(int id, string? tindakan, string? userId)
        {
            var data = _context.AkPV.FirstOrDefault(pp => pp.Id == id);

            if (data != null)
            {
                data.EnStatusBorang = EnStatusBorang.Kemaskini;
                data.Tindakan = tindakan;

                data.UserIdKemaskini = userId ?? "";
                data.TarKemaskini = DateTime.Now;

                data.FlPosting = 0;
                data.TarikhPosting = null;

                _context.Update(data);

                RemovePostingFromAbBukuVot(data);
                RemovePostingFromAkAkaun(data);
                RemovePostingFromAkPanjarLejar(data);


            }
        }

        public List<AkPV> GetAllByStatus(EnStatusBorang enStatusBorang)
        {
            return _context.AkPV.Where(pp => pp.EnStatusBorang == enStatusBorang).ToList();
        }

        public AkPV GetDetailsById(int id)
        {
            return _context.AkPV
                .IgnoreQueryFilters()
                .Include(t => t.JKW)
                .Include(t => t.AkBank)
                    .ThenInclude(t => t!.JBank)
                .Include(t => t.JCawangan)
                .Include(t => t.DPekerjaPosting)
                .Include(t => t.DPengesah)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPenyemak)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPelulus)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.AkJanaanProfil)
                .Include(t => t.SuGajiBulanan)
                    .ThenInclude(t => t!.SuGajiBulananPekerja)!
                        .ThenInclude(t => t.SuGajiElaunPotongan)!
                            .ThenInclude(t => t.JElaunPotongan)
                .Include(t => t.AkPVObjek)!
                    .ThenInclude(to => to.AkCarta)
                .Include(t => t.AkPVObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JKW)
                .Include(t => t.AkPVObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JPTJ)
                .Include(t => t.AkPVObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JBahagian)
                .Include(t => t.AkPVObjek)!
                    .ThenInclude(t => t.JCukai)
                .Include(t => t.AkPVInvois)!
                    .ThenInclude(t => t.AkBelian)
                .Include(t => t.AkPVPenerima)!
                    .ThenInclude(t => t.DDaftarAwam)
                .Include(t => t.AkPVPenerima)!
                    .ThenInclude(t => t.DPekerja)
                .Include(t => t.AkPVPenerima)!
                    .ThenInclude(t => t.JCaraBayar)
                .Include(t => t.AkPVPenerima)!
                    .ThenInclude(t => t.JBank)
                .Include(t => t.AkPVPenerima)!
                    .ThenInclude(t => t.DPanjar)
                .Include(t => t.AkPVPenerima)!
                    .ThenInclude(t => t.AkRekup)
                .FirstOrDefault(pp => pp.Id == id) ?? new AkPV();
        }

        public string GetMaxRefNo(string initNoRujukan, string tahun)
        {
            var max = _context.AkPV.Where(pp => pp.Tahun == tahun).OrderByDescending(pp => pp.NoRujukan).ToList();

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

        public List<AkPV> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang, int? akBankId)
        {
            if (searchString == null && dateFrom == null && dateTo == null && orderBy == null)
            {
                return new List<AkPV>();
            }

            var akPVList = _context.AkPV
                .IgnoreQueryFilters()
                .Include(t => t.JKW)
                .Include(t => t.AkBank)
                    .ThenInclude(t => t!.JBank)
                .Include(t => t.JCawangan)
                .Include(t => t.DPekerjaPosting)
                .Include(t => t.DPengesah)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPenyemak)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPelulus)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.AkPVPenerima)!
                    .ThenInclude(pp => pp.JBank)
                .Include(t => t.AkPVPenerima)!
                    .ThenInclude(pp => pp.JCaraBayar)
                .Include(t => t.AkPVPenerima)!
                    .ThenInclude(pp => pp.DDaftarAwam)
                .Include(t => t.AkPVPenerima)!
                    .ThenInclude(pp => pp.DPekerja)
                    //.Include(t => t.SuGajiBulanan)
                    //    .ThenInclude(t => t!.SuGajiBulananPekerja)!
                    //        .ThenInclude(t => t.SuGajiElaunPotongan)!
                    //            .ThenInclude(t => t.JElaunPotongan)
                    //.Include(t => t.AkPVObjek)!
                    //    .ThenInclude(to => to.AkCarta)
                    //.Include(t => t.AkPVObjek)!
                    //    .ThenInclude(to => to.JKWPTJBahagian)
                    //        .ThenInclude(b => b!.JKW)
                    //.Include(t => t.AkPVObjek)!
                    //    .ThenInclude(to => to.JKWPTJBahagian)
                    //        .ThenInclude(b => b!.JPTJ)
                    //.Include(t => t.AkPVObjek)!
                    //    .ThenInclude(to => to.JKWPTJBahagian)
                    //        .ThenInclude(b => b!.JBahagian)
                    //.Include(t => t.AkPVObjek)!
                    //    .ThenInclude(t => t.JCukai)
                    //.Include(t => t.AkPVInvois)!
                    //    .ThenInclude(t => t.AkBelian)
                    .Where(t => t.Tarikh >= dateFrom && t.Tarikh <= dateTo!.Value.AddHours(23.99))
                .ToList();

            // searchstring filters
            if (searchString != null)
            {
                akPVList = akPVList.Where(t =>
                t.NoRujukan!.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                || t.NamaPenerima!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            // searchString filters end

            // status borang filters
            switch (enStatusBorang)
            {
                case EnStatusBorang.None:
                    akPVList = akPVList.Where(pp => pp.EnStatusBorang == EnStatusBorang.None).ToList();
                    break;
                case EnStatusBorang.Sah:
                    akPVList = akPVList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Sah).ToList();
                    break;
                case EnStatusBorang.Semak:
                    akPVList = akPVList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Semak).ToList();
                    break;
                case EnStatusBorang.Lulus:
                    akPVList = akPVList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Lulus).ToList();
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
                        akPVList = akPVList.OrderBy(t => t.NamaPenerima).ToList();
                        break;
                    case "Tarikh":
                        akPVList = akPVList.OrderBy(t => t.Tarikh).ToList(); break;
                    default:
                        akPVList = akPVList.OrderBy(t => t.NoRujukan).ToList();
                        break;
                }

            }
            // order by filters end

            if (akBankId != null)
            {
                akPVList = akPVList.Where(pv => pv.AkBankId == akBankId).ToList();
            }
            return akPVList;
        }

        public List<AkPV> GetResults1(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang, int? akBankId, int? tunai, int? jKWId, int? dDaftarAwamId)
        {
            if (searchString == null && dateFrom == null && dateTo == null && akBankId == null && tunai == null && jKWId == null && dDaftarAwamId == null)
            {
                return new List<AkPV>();
            }

            var query = _context.AkPV
                .IgnoreQueryFilters()
                .Include(t => t.JKW)
                .Include(t => t.AkBank)
                    .ThenInclude(t => t!.JBank)
                .Include(t => t.JCawangan)
                .Include(t => t.DPekerjaPosting)
                .Include(t => t.DPengesah)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPenyemak)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPelulus)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.AkPVPenerima)!
                    .ThenInclude(pp => pp.JBank)
                .Include(t => t.AkPVPenerima)!
                    .ThenInclude(pp => pp.JCaraBayar)
                .Include(t => t.AkPVPenerima)!
                    .ThenInclude(pp => pp.DDaftarAwam)
                .Include(t => t.AkPVPenerima)!
                    .ThenInclude(pp => pp.DPekerja)
                .AsQueryable();

            if (dateFrom != null && dateTo != null)
            {
                query = query.Where(t => t.Tarikh >= dateFrom && t.Tarikh <= dateTo.Value.AddHours(23.99));
            }

            if (searchString != null)
            {
                searchString = searchString.ToLower();
                query = query.Where(t =>
                    (t.NoRujukan != null && t.NoRujukan.ToLower().Contains(searchString)) ||
                    (t.NamaPenerima != null && t.NamaPenerima.ToLower().Contains(searchString)));
            }

            switch (enStatusBorang)
            {
                case EnStatusBorang.None:
                    query = query.Where(pp => pp.EnStatusBorang == EnStatusBorang.None);
                    break;
                case EnStatusBorang.Sah:
                    query = query.Where(pp => pp.EnStatusBorang == EnStatusBorang.Sah);
                    break;
                case EnStatusBorang.Semak:
                    query = query.Where(pp => pp.EnStatusBorang == EnStatusBorang.Semak);
                    break;
                case EnStatusBorang.Lulus:
                    query = query.Where(pp => pp.EnStatusBorang == EnStatusBorang.Lulus);
                    break;
                case EnStatusBorang.Semua:
                    break;
            }

            if (akBankId != null)
            {
                query = query.Where(pv => pv.AkBankId == akBankId);
            }

            if (jKWId != null)
            {
                query = query.Where(pv => pv.JKWId == jKWId);
            }

            if (dDaftarAwamId != null)
            {
                query = query.Where(a => a.AkPVPenerima!.Any(p => p.DDaftarAwamId == dDaftarAwamId));
            }

            if (orderBy != null)
            {
                switch (orderBy)
                {
                    case "Nama":
                        query = query.OrderBy(t => t.NamaPenerima);
                        break;
                    case "Tarikh":
                        query = query.OrderBy(t => t.Tarikh);
                        break;
                    default:
                        query = query.OrderBy(t => t.NoRujukan);
                        break;
                }
            }

            var akPVList = query.ToList();

            return akPVList;
        }

        public List<AkPV> GetResultsByDPekerjaIdFromDKonfigKelulusan(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang, int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan)
        {
            // get all data with details
            List<AkPV> akPVList = GetResults(searchString, dateFrom, dateTo, orderBy, enStatusBorang, null);

            var filterings = FilterByComparingJBahagianAkPVObjekWithJBahagianDKonfigKelulusan(dPekerjaId, enKategoriKelulusan, enJenisModulKelulusan, akPVList);

            var results = FilterByComparingJumlahAkPVWithMinAmountMaxAmountDKonfigKelulusan(dPekerjaId, enKategoriKelulusan, enJenisModulKelulusan, filterings);

            return results;
        }

        public List<AkPV> FilterByComparingJBahagianAkPVObjekWithJBahagianDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AkPV> akPVList)
        {
            // initialize result sets
            List<AkPV> results = new List<AkPV>();

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

                var akPVGroup = new List<AkPVObjek>().GroupBy(objek => objek.JKWPTJBahagianId);
                if (akPVList != null && akPVList.Count > 0)
                {
                    foreach (var akPV in akPVList)
                    {
                        var penilaianPerolehanObjekBahagianList = new List<JBahagian>();

                        // group akPVObjek by bahagian
                        if (akPV.AkPVObjek != null && akPV.AkPVObjek.Count > 0)
                        {
                            foreach (var item in akPV.AkPVObjek)
                            {
                                penilaianPerolehanObjekBahagianList.Add(item.JKWPTJBahagian?.JBahagian ?? new JBahagian());
                            }

                        }
                        // if konfigKelulusan bahagian null, bypass all, add to results
                        if (konfigKelulusanBahagianList.Count == 0)
                        {
                            results.Add(akPV);
                            continue;
                        }

                        // compare each lists, if its equal then insert to results
                        var items = penilaianPerolehanObjekBahagianList.All(konfigKelulusanBahagianList.Contains);
                        if (konfigKelulusanBahagianList.OrderBy(kk => kk.Kod).SequenceEqual(penilaianPerolehanObjekBahagianList.OrderBy(pp => pp.Kod))
                            || penilaianPerolehanObjekBahagianList.All(konfigKelulusanBahagianList.Contains))
                        {

                            results.Add(akPV);
                            continue;
                        };
                    }
                }
            }


            return results;
        }

        public List<AkPV> FilterByComparingJumlahAkPVWithMinAmountMaxAmountDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AkPV> filterings)
        {
            //initialize new list akPV
            List<AkPV> results = new List<AkPV>();

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

        public void HantarSemula(int id, string? tindakan, string? userId)
        {
            var data = _context.AkPV.FirstOrDefault(pp => pp.Id == id);

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

        public async Task<bool> IsBatalAsync(int id)
        {
            bool isBatal = await _context.AkPV.AnyAsync(t => t.Id == id && t.FlBatal == 1);
            if (isBatal)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsLulusAsync(int id)
        {
            bool isLulus = await _context.AkPV.AnyAsync(t => t.Id == id && t.EnStatusBorang == EnStatusBorang.Lulus);
            if (isLulus)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsPostedAsync(int id, string noRujukan)
        {
            bool isPosted = await _context.AkPV.AnyAsync(t => t.Id == id && t.FlPosting == 1);
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

        public async Task<bool> IsSahAsync(int id)
        {
            bool isSah = await _context.AkPV.AnyAsync(t => t.Id == id && (t.EnStatusBorang == EnStatusBorang.Sah || t.EnStatusBorang == EnStatusBorang.Semak || t.EnStatusBorang == EnStatusBorang.Lulus));
            if (isSah)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsSemakAsync(int id)
        {
            bool isSemak = await _context.AkPV.AnyAsync(t => t.Id == id && (t.EnStatusBorang == EnStatusBorang.Semak || t.EnStatusBorang == EnStatusBorang.Lulus));
            if (isSemak)
            {
                return true;
            }

            return false;
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

                PostingToAkAkaun(data);

                PostingToAkPanjarLejar(data);
            }
        }

        public void PostingToAkAkaun(AkPV akPV)
        {
            List<AkAkaun> akAkaunList = new List<AkAkaun>();

            if (akPV.AkPVObjek != null && akPV.AkPVObjek.Count > 0)
            {

                if (akPV.IsInvois)
                {
                    int? akAkaunAkruId = null;
                    if (akPV.AkPVInvois != null && akPV.AkPVInvois.Count > 0)
                    {
                        foreach (var item in akPV.AkPVInvois)
                        {
                            akAkaunAkruId = item.AkBelian?.AkAkaunAkruId;
                        }
                    }

                    foreach (var item in akPV.AkPVObjek)
                    {
                        if (akPV.IsAkru && akAkaunAkruId != null)
                        {
                            AkAkaun akAkaun1 = new AkAkaun()
                            {
                                JKWId = akPV.JKWId,
                                JPTJId = item.JKWPTJBahagian?.JPTJId,
                                JBahagianId = item.JKWPTJBahagian?.JBahagianId,
                                NoRujukan = akPV.NoRujukan,
                                Tarikh = akPV.Tarikh,
                                AkCarta1Id = akPV.AkBank!.AkCartaId,
                                AkCarta2Id = akAkaunAkruId,
                                Kredit = item.Amaun
                            };
                            akAkaunList.Add(akAkaun1);

                            AkAkaun akAkaun2 = new AkAkaun()
                            {
                                JKWId = akPV.JKWId,
                                JPTJId = item.JKWPTJBahagian?.JPTJId,
                                JBahagianId = item.JKWPTJBahagian?.JBahagianId,
                                NoRujukan = akPV.NoRujukan,
                                Tarikh = akPV.Tarikh,
                                AkCarta1Id = (int)akAkaunAkruId,
                                AkCarta2Id = akPV.AkBank?.AkCartaId,
                                Debit = item.Amaun
                            };

                            akAkaunList.Add(akAkaun2);
                        }
                        else
                        {
                            AkAkaun akAkaun1 = new AkAkaun()
                            {
                                JKWId = akPV.JKWId,
                                JPTJId = item.JKWPTJBahagian?.JPTJId,
                                JBahagianId = item.JKWPTJBahagian?.JBahagianId,
                                NoRujukan = akPV.NoRujukan,
                                Tarikh = akPV.Tarikh,
                                AkCarta1Id = akPV.AkBank!.AkCartaId,
                                AkCarta2Id = item.AkCartaId,
                                Kredit = item.Amaun
                            };
                            akAkaunList.Add(akAkaun1);

                            AkAkaun akAkaun2 = new AkAkaun()
                            {
                                JKWId = akPV.JKWId,
                                JPTJId = item.JKWPTJBahagian?.JPTJId,
                                JBahagianId = item.JKWPTJBahagian?.JBahagianId,
                                NoRujukan = akPV.NoRujukan,
                                Tarikh = akPV.Tarikh,
                                AkCarta1Id = item.AkCartaId,
                                AkCarta2Id = akPV.AkBank?.AkCartaId,
                                Debit = item.Amaun
                            };

                            akAkaunList.Add(akAkaun2);
                        }
                    }
                }
                else
                {
                    foreach (var item in akPV.AkPVObjek)
                    {
                        AkAkaun akAkaun1 = new AkAkaun()
                        {
                            JKWId = akPV.JKWId,
                            JPTJId = item.JKWPTJBahagian?.JPTJId,
                            JBahagianId = item.JKWPTJBahagian?.JBahagianId,
                            NoRujukan = akPV.NoRujukan,
                            Tarikh = akPV.Tarikh,
                            AkCarta1Id = akPV.AkBank!.AkCartaId,
                            AkCarta2Id = item.AkCartaId,
                            Kredit = item.Amaun
                        };
                        akAkaunList.Add(akAkaun1);

                        AkAkaun akAkaun2 = new AkAkaun()
                        {
                            JKWId = akPV.JKWId,
                            JPTJId = item.JKWPTJBahagian?.JPTJId,
                            JBahagianId = item.JKWPTJBahagian?.JBahagianId,
                            NoRujukan = akPV.NoRujukan,
                            Tarikh = akPV.Tarikh,
                            AkCarta1Id = item.AkCartaId,
                            AkCarta2Id = akPV.AkBank?.AkCartaId,
                            Debit = item.Amaun
                        };

                        akAkaunList.Add(akAkaun2);
                    }
                }

                _context.AkAkaun.AddRange(akAkaunList);
            }
        }
        public void PostingToAbBukuVot(AkPV akPV)
        {
            List<AbBukuVot> abBukuVotList = new List<AbBukuVot>();

            int? daftarAwamId = null;
            if (akPV.AkPVPenerima != null && akPV.AkPVPenerima.Count > 0)
            {
                foreach (var item in akPV.AkPVPenerima)
                {
                    daftarAwamId = item.DDaftarAwamId;
                }
            }

            // Cash Basis (PV yang tiada invois atau PV yang ada Invois tanpa akruan)
            if (PVWithoutInvois(akPV) || PVWithOneInvoisNotAkru(akPV) || PVWithMultipleInvoisNotAkru(akPV))
            {
                if (akPV.AkPVObjek != null && akPV.AkPVObjek.Count > 0)
                {
                    foreach (var item in akPV.AkPVObjek)
                    {
                        AbBukuVot abBukuVot = new AbBukuVot()
                        {
                            Tahun = akPV.Tahun,
                            JKWId = item.JKWPTJBahagian?.JKWId ?? akPV.JKWId,
                            JPTJId = (int)item.JKWPTJBahagian!.JPTJId,
                            JBahagianId = item.JKWPTJBahagian.JBahagianId,
                            Tarikh = akPV.Tarikh,
                            DDaftarAwamId = daftarAwamId,
                            VotId = item.AkCartaId,
                            NoRujukan = akPV.NoRujukan,
                            Debit = item.Amaun,
                            Belanja = item.Amaun,
                            Baki = -item.Amaun

                        };

                        abBukuVotList.Add(abBukuVot);
                    }
                }
            }

            // PV yang ada Invois tanpa tanggungan
            if (PVWithOneInvoisAkruWithoutPOOrInden(akPV) || PVWithMultipleInvoisAkruWithoutPOOrInden(akPV))
            {
                if (akPV.AkPVObjek != null && akPV.AkPVObjek.Count > 0)
                {
                    foreach (var item in akPV.AkPVObjek)
                    {
                        AbBukuVot abBukuVot = new AbBukuVot()
                        {
                            Tahun = akPV.Tahun,
                            JKWId = item.JKWPTJBahagian?.JKWId ?? akPV.JKWId,
                            JPTJId = (int)item.JKWPTJBahagian!.JPTJId,
                            JBahagianId = item.JKWPTJBahagian.JBahagianId,
                            Tarikh = akPV.Tarikh,
                            DDaftarAwamId = daftarAwamId,
                            VotId = item.AkCartaId,
                            NoRujukan = akPV.NoRujukan,
                            Debit = item.Amaun,
                            Liabiliti = -item.Amaun,
                            JumLiabiliti = -item.Amaun
                            // - BakiLiabiliti
                        };

                        abBukuVotList.Add(abBukuVot);
                    }
                }
            }

            // PV yang ada Invois dengan tanggungan
            if (PVWithOneInvoisAkruWithOnePOAndWithoutInden(akPV) ||
                PVWithOneInvoisAkruWithOneIndenAndWithoutPO(akPV) ||
                PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek(akPV) ||
                PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneDifferentObjek(akPV))
            {
                List<AkPOObjek> poList = new List<AkPOObjek>();

                if (akPV.AkPVInvois != null && akPV.AkPVInvois.Count > 0)
                {
                    foreach (var invois in akPV.AkPVInvois)
                    {
                        // each insert poObjekList into poList
                        poList.AddRange(invois.AkBelian!.AkPO!.AkPOObjek!);

                    }
                }

                if (poList != null && poList.Count > 0)
                {
                    foreach (var item in poList)
                    {
                        AbBukuVot abBukuVot = new AbBukuVot()
                        {
                            Tahun = akPV.Tahun,
                            JKWId = item.JKWPTJBahagian?.JKWId ?? akPV.JKWId,
                            JPTJId = (int)item.JKWPTJBahagian!.JPTJId,
                            JBahagianId = item.JKWPTJBahagian.JBahagianId,
                            Tarikh = akPV.Tarikh,
                            DDaftarAwamId = daftarAwamId,
                            VotId = item.AkCartaId,
                            NoRujukan = akPV.NoRujukan,
                            Debit = item.Amaun,
                            //Tanggungan = -item.Amaun,
                            Tbs = -item.Amaun,
                            Liabiliti = -item.Amaun,
                            JumLiabiliti = -item.Amaun
                            // - BakiLiabiliti
                        };

                        abBukuVotList.Add(abBukuVot);
                    }
                }
            }

            _context.AbBukuVot.AddRange(abBukuVotList);

        }

        public void PostingToAkPanjarLejar(AkPV akPV)
        {
            List<AkPanjarLejar> akPanjarLejarList = new List<AkPanjarLejar>();

            if (akPV.EnJenisBayaran == EnJenisBayaran.Panjar)
            {
                if (akPV.AkPVPenerima != null && akPV.AkPVPenerima.Count > 0)
                {
                    foreach (var penerima in akPV.AkPVPenerima)
                    {
                        if (penerima.EnKategoriDaftarAwam == EnKategoriDaftarAwam.LainLain && penerima.AkRekupId != null)
                        {
                            // update panjar lejar
                            var rekup = _context.AkRekup
                                .Include(r => r.DPanjar)
                                    .ThenInclude(p => p!.DPanjarPemegang)!
                                        .ThenInclude(pp => pp.DPekerja)
                                .FirstOrDefault(r => r.Id == penerima.AkRekupId);
                            if (rekup != null && rekup.DPanjar != null && rekup.DPanjar.DPanjarPemegang != null)
                            {

                                var LejarPanjarList = _context.AkPanjarLejar.Where(r => r.DPanjarId == rekup.DPanjarId).ToList();

                                decimal totalRekup = 0;

                                if (LejarPanjarList != null && LejarPanjarList.Count > 0)
                                {
                                    foreach (var item in LejarPanjarList)
                                    {
                                        totalRekup += item.Debit - item.Kredit;
                                    }
                                }
                                else
                                {
                                    totalRekup = rekup.Jumlah;
                                }

                                AkPanjarLejar akPanjarLejar = new AkPanjarLejar()
                                {
                                    JKWPTJBahagianId = (int)rekup.DPanjar.JKWPTJBahagianId!,
                                    DPanjarId = rekup.DPanjarId,
                                    AkCVId = null,
                                    AkPVId = akPV.Id,
                                    AkJurnalId = null,
                                    Tarikh = akPV.Tarikh,
                                    AkCartaId = rekup.DPanjar.AkCartaId,
                                    Debit = rekup.Jumlah < 0 ? 0 : rekup.Jumlah,
                                    Kredit = rekup.Jumlah < 0 ? -rekup.Jumlah : 0,
                                    AkRekupId = rekup.Id,
                                    NoRujukan = rekup.NoRujukan,
                                    Baki = totalRekup,
                                    IsPaid = true
                                };

                                _context.AkPanjarLejar.Add(akPanjarLejar);

                                rekup.IsLinked = true;

                                _context.AkRekup.Update(rekup);
                            }
                            // update panjar lejar end

                        }
                    }
                }
            }
        }

        public void RemovePostingFromAkAkaun(AkPV akPV)
        {
            var akAkaunList = _context.AkAkaun.Where(b => b.NoRujukan == akPV.NoRujukan).ToList();

            if (akAkaunList != null && akAkaunList.Count > 0)
            {
                _context.RemoveRange(akAkaunList);
            }
        }
        public void RemovePostingFromAbBukuVot(AkPV akPV)
        {
            var abBukuVotList = _context.AbBukuVot.Where(b => b.NoRujukan == akPV.NoRujukan).ToList();

            if (abBukuVotList != null && abBukuVotList.Count > 0)
            {
                _context.RemoveRange(abBukuVotList);
            }
        }

        public void RemovePostingFromAkPanjarLejar(AkPV akPV)
        {

            if (akPV.EnJenisBayaran == EnJenisBayaran.Panjar)
            {
                if (akPV.AkPVPenerima != null && akPV.AkPVPenerima.Count > 0)
                {
                    foreach (var penerima in akPV.AkPVPenerima)
                    {
                        if (penerima.EnKategoriDaftarAwam == EnKategoriDaftarAwam.LainLain && penerima.AkRekupId != null)
                        {
                            // update panjar lejar
                            var rekup = _context.AkRekup
                                .Include(r => r.DPanjar)
                                    .ThenInclude(p => p!.DPanjarPemegang)!
                                        .ThenInclude(pp => pp.DPekerja)
                                .FirstOrDefault(r => r.Id == penerima.AkRekupId);
                            if (rekup != null && rekup.DPanjar != null && rekup.DPanjar.DPanjarPemegang != null)
                            {

                                var LejarPanjarList = _context.AkPanjarLejar.Where(r => r.DPanjarId == rekup.DPanjarId && r.NoRujukan == rekup.NoRujukan).ToList();

                                if (LejarPanjarList != null && LejarPanjarList.Count > 0)
                                {
                                    _context.RemoveRange(LejarPanjarList);
                                }
                            }


                            // update panjar lejar end
                        }
                    }
                }
            }

        }

        public void Sah(int id, int? pengesahId, string? userId)
        {
            var data = _context.AkPV.FirstOrDefault(pp => pp.Id == id);
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
            var data = _context.AkPV.FirstOrDefault(pp => pp.Id == id);
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

        public bool HaveAkJanaanProfil(int akJanaanProfilId)
        {
            return _context.AkPV.Any(pv => pv.AkJanaanProfilId == akJanaanProfilId);
        }

        public List<AkPVPenerima> GetAkPVPenerimaByAkPVId(int Id)
        {
            List<AkPVPenerima> data = _context.AkPVPenerima.Include(ppo => ppo.JCaraBayar).Where(ppo => ppo.AkPVId == Id).ToList();

            return data;
        }

        public async Task<List<AkPVPenerima>> GetResultsGroupByTarikhCaraBayar(string? tarikhDari, string? tarikhHingga, int? akBankId, int? tunai)
        {
            List<AkPV> akPv = new List<AkPV>();
            List<AkPVPenerima> akPvResult = new List<AkPVPenerima>();

            List<AkPVPenerima> akPvPenerima = new List<AkPVPenerima>();

            if (tarikhDari != null && tarikhHingga != null)
            {
                DateTime date1 = DateTime.Parse(tarikhDari).Date;
                DateTime date2 = DateTime.Parse(tarikhHingga).Date.AddDays(1).AddTicks(-1);

                akPv = await _context.AkPV
                .Include(b => b.AkPVPenerima)
                .Where(b => b.Tarikh >= date1 && b.Tarikh <= date2 && (akBankId == 0 || b.AkBankId == akBankId))
                .ToListAsync();

                foreach (var a in akPv)
                {
                    if (a.AkPVPenerima != null && a.AkPVPenerima.Any())
                    {
                        foreach (var b in a.AkPVPenerima)
                        {
                            if (b != null && b.IsCekDitunaikan != true && tunai == 0) // 2 = cek

                            {
                                akPvResult.Add(new AkPVPenerima
                                {
                                    TarikhCaraBayar = b.TarikhCaraBayar,
                                    NoRujukanCaraBayar = b.NoRujukanCaraBayar,
                                    Amaun = b.Amaun,
                                    AkPV = b.AkPV,
                                    NamaPenerima = b.NamaPenerima,
                                    IsCekDitunaikan = b.IsCekDitunaikan
                                });
                            }

                            else if (b != null && b.IsCekDitunaikan == true && tunai == 1)
                            {
                                akPvResult.Add(new AkPVPenerima
                                {
                                    TarikhCaraBayar = b.TarikhCaraBayar,
                                    NoRujukanCaraBayar = b.NoRujukanCaraBayar,
                                    Amaun = b.Amaun,
                                    AkPV = b.AkPV,
                                    NamaPenerima = b.NamaPenerima,
                                    IsCekDitunaikan = b.IsCekDitunaikan
                                });
                            }

                            else if (b != null && tunai == 2)
                            {
                                akPvResult.Add(new AkPVPenerima
                                {
                                    TarikhCaraBayar = b.TarikhCaraBayar,
                                    NoRujukanCaraBayar = b.NoRujukanCaraBayar,
                                    Amaun = b.Amaun,
                                    AkPV = b.AkPV,
                                    NamaPenerima = b.NamaPenerima,
                                    IsCekDitunaikan = b.IsCekDitunaikan
                                });
                            }
                        }

                    }
                }
            }

            return akPvResult.GroupBy(b => new { b.TarikhCaraBayar, b.NoRujukanCaraBayar, b.Amaun, b.NamaPenerima })
            .Select(g => new AkPVPenerima
            {
                NoRujukanCaraBayar = g.First().NoRujukanCaraBayar,
                TarikhCaraBayar = g.First().TarikhCaraBayar,
                Amaun = g.Sum(b => b.Amaun),
                AkPV = g.First().AkPV,
                NamaPenerima = g.First().NamaPenerima,
                IsCekDitunaikan = g.First().IsCekDitunaikan
            }).OrderBy(b => b.TarikhCaraBayar)
                .ThenBy(b => b.NoRujukanCaraBayar)
                .ThenBy(b => b.AkPV?.Tarikh)
                .ToList();
        }

        public async Task<List<AkPVPenerima>> GetResultsGroupByTarikhCaraBayar1(string? tarikhDari, string? tarikhHingga)
        {
            List<AkPV> akPv = new List<AkPV>();
            List<AkPVPenerima> akPvResult = new List<AkPVPenerima>();

            if (tarikhDari != null && tarikhHingga != null)
            {
                akPv = await _context.AkPV
                .Include(b => b.AkPVPenerima)
                .Where(b => b.Tarikh >= DateTime.Parse(tarikhDari) && b.Tarikh <= DateTime.Parse(tarikhHingga))
                .ToListAsync();
            }

            foreach (var a in akPv)
            {
                if (a.AkPVPenerima != null && a.AkPVPenerima.Any())
                {
                    foreach (var b in a.AkPVPenerima)
                    {
                        if (b.JCaraBayarId == 2 && !b.IsCekDitunaikan && b.EnStatusEFT == EnStatusProses.Fail) // 2 = cek
                        {
                            akPvResult.Add(new AkPVPenerima
                            {
                                TarikhCaraBayar = b.TarikhCaraBayar,
                                NoRujukanCaraBayar = b.NoRujukanCaraBayar,
                                Amaun = b.Amaun,
                                AkPV = b.AkPV
                            });
                        }
                    }
                }
            }

            var groupedResults = akPvResult
                .GroupBy(b => new { b.TarikhCaraBayar, b.NoRujukanCaraBayar })
                .Select(g => new AkPVPenerima
                {
                    NoRujukanCaraBayar = g.Key.NoRujukanCaraBayar,
                    TarikhCaraBayar = g.Key.TarikhCaraBayar,
                    Amaun = g.Sum(b => b.Amaun),
                    AkPV = g.First().AkPV
                })
                .OrderBy(b => b.TarikhCaraBayar)
                .ThenBy(b => b.AkPV?.Tarikh)
                .ToList();

            return groupedResults;
        }

        public async Task<List<AkPV>> GetResultsGroupByTarikh(string? tarikhDari, string? tarikhHingga, int? jKWId)
        {
            if (tarikhDari == null || tarikhHingga == null || jKWId == null)
            {
                return new List<AkPV>();
            }

            DateTime date1 = DateTime.Parse(tarikhDari).Date;
            DateTime date2 = DateTime.Parse(tarikhHingga).Date.AddDays(1).AddTicks(-1);

            var akPv = await _context.AkPV
                .Include(b => b.AkPVPenerima)
                .Where(b => b.Tarikh >= date1 && b.Tarikh <= date2 && b.JKWId == jKWId)
                .ToListAsync();

            var flattenedAndSortedPenerima = akPv
                .SelectMany(pv => pv.AkPVPenerima!.Select(pvp => new { AkPV = pv, AkPVPenerima = pvp }))
                .OrderBy(item => item.AkPVPenerima.NoRujukanCaraBayar)
                .ToList();

            var groupedAndSortedResults = flattenedAndSortedPenerima
                .GroupBy(item => item.AkPVPenerima.NoRujukanCaraBayar)
                .Select(g => new AkPV
                {
                    Id = g.First().AkPV.Id,
                    NoRujukan = g.First().AkPV.NoRujukan,
                    Tarikh = g.First().AkPV.Tarikh,
                    JKWId = g.First().AkPV.JKWId,
                    NamaPenerima = g.First().AkPV.NamaPenerima,
                    FlBatal = g.First().AkPV.FlBatal,
                    AkPVPenerima = g.Select(x => x.AkPVPenerima).ToList()
                })
                .ToList();

            return groupedAndSortedResults;
        }

        public async Task<List<AkPV>> GetResultsGroupBySearchString(int? jKWId, string? searchString1, string? searchString2)
        {
            if (jKWId == null || searchString1 == null || searchString2 == null)
            {
                return new List<AkPV>();
            }

            var lowerSearchString1 = searchString1.ToLower();
            var lowerSearchString2 = searchString2.ToLower();

            bool isOrderCorrect = string.Compare(lowerSearchString1, lowerSearchString2, StringComparison.Ordinal) <= 0;

            if (!isOrderCorrect)
            {
                return new List<AkPV>();
            }

            var akPv = _context.AkPV
                .Include(b => b.AkPVPenerima)
                .Where(b => b.JKWId == jKWId && b.NoRujukan!.ToLower().CompareTo(lowerSearchString1) >= 0 &&
                            b.NoRujukan!.ToLower().CompareTo(lowerSearchString2) <= 0);

            var akPvResults = await akPv.ToListAsync();

            var flattenedPenerima = akPvResults
                .SelectMany(pv => pv.AkPVPenerima!.Select(pvp => new { AkPV = pv, AkPVPenerima = pvp }))
                .OrderBy(item => item.AkPVPenerima.NoRujukanCaraBayar)
                .ThenBy(item => item.AkPV.NoRujukan)
                .ToList();

            var groupedResults = flattenedPenerima
                .GroupBy(item => new { item.AkPV.NoRujukan, item.AkPV.Id })
                .Select(g => new AkPV
                {
                    Id = g.Key.Id,
                    NoRujukan = g.Key.NoRujukan,
                    Tarikh = g.First().AkPV.Tarikh,
                    JKWId = g.First().AkPV.JKWId,
                    NamaPenerima = g.First().AkPV.NamaPenerima,
                    FlBatal = g.First().AkPV.FlBatal,
                    AkPVPenerima = g.Select(x => x.AkPVPenerima).ToList()
                })
                .ToList();

            return groupedResults;
        }

        public async Task<List<AkPV>> GetResultsGroupByTarikh1(string? tarikhDari, string? tarikhHingga, int? dDaftarAwamId)
        {
            if (tarikhDari == null || tarikhHingga == null || dDaftarAwamId == null)
            {
                return new List<AkPV>();
            }

            DateTime date1 = DateTime.Parse(tarikhDari).Date;
            DateTime date2 = DateTime.Parse(tarikhHingga).Date.AddDays(1).AddTicks(-1);

            var matchingPvIds = await _context.AkPVPenerima
                .Where(p => p.DDaftarAwamId == dDaftarAwamId)
                .Select(p => p.AkPVId)
                .ToListAsync();

            var akPvQuery = _context.AkPV
                .Include(a => a.AkPVInvois)!
                .ThenInclude(b => b.AkBelian)
                .Where(a => a.Tarikh >= date1 && a.Tarikh <= date2 && matchingPvIds.Contains(a.Id))
                .Where(a => a.AkPVInvois!.Any(b => b.AkBelian!.DDaftarAwamId == dDaftarAwamId && b.AkBelian.NoRujukan != null));

            var akPv = await akPvQuery.ToListAsync();

            return akPv;
        }


        public async Task<List<LAK009PrintModel>> GetResultLAK009(DateTime? tarDari1, DateTime? tarHingga1, int? JKWId, int AkBankId)
        {
            List<AkPV> akPv = new List<AkPV>();
            List<LAK009PrintModel> akPvResult = new List<LAK009PrintModel>();

            if (tarDari1 != null && tarHingga1 != null)
            {
                akPv = await _context.AkPV
                .Include(b => b.AkPVObjek!)
                .ThenInclude(b => b.AkCarta)
                .Include(b => b.AkPVPenerima)
                .Where(b => b.Tarikh >= tarDari1 && b.Tarikh <= tarHingga1 && (AkBankId == 0 || b.AkBankId == AkBankId))
                .ToListAsync();
            }

            if (JKWId != null)
            {
                akPv = akPv.Where(ak => ak.JKWId == JKWId).ToList();
            }


            var akBank = await _context.AkBank.Include(b => b.AkCarta).FirstOrDefaultAsync(b => b.Id == AkBankId);
            if (akBank != null)
            {
                akPv = akPv.Where(ak => ak.AkBankId == AkBankId).ToList();
            }



            var jumlahByAkCartaKod = new Dictionary<string, decimal>();

            // accumulate the Jumlah for all AkPVObjek
            foreach (var a in akPv)
            {
                if (a.AkPVObjek != null && a.AkPVObjek.Any())
                {
                    foreach (var obj in a.AkPVObjek)
                    {
                        var akCartaKod = obj.AkCarta?.Kod;
                        var amaun = obj.Amaun;

                        // Update dictionary with Jumlah based on akCartaKod
                        if (akCartaKod != null)
                        {
                            if (!jumlahByAkCartaKod.ContainsKey(akCartaKod))
                            {
                                jumlahByAkCartaKod[akCartaKod] = 0;
                            }
                            jumlahByAkCartaKod[akCartaKod] += amaun;
                        }
                    }
                }
            }

            var AkCartaKodList = new SortedSet<string>();

            foreach (var a in akPv)
            {
                // Loop for AkPVPenerima without affecting Amaun
                if (a.AkPVPenerima != null && a.AkPVPenerima.Any())
                {
                    foreach (var b in a.AkPVPenerima)
                    {
                        var akcartaResult = new LAK009PrintModel
                        {
                            TarikhPV = a.Tarikh,
                            NoRujukanPV = a.NoRujukan,
                            Perihal = a.Ringkasan,
                            Amaun = b.Amaun,
                            NoRujukanCek = b.NoRujukanCaraBayar,
                            PenerimaCek = b.NamaPenerima
                        };
                        akPvResult.Add(akcartaResult);
                    }
                }

                    // Loop for AkPVObjek to link akCartaKod and Jumlah without duplication
                    if (a.AkPVObjek != null && a.AkPVObjek.Any())
                    {
                        var cartaObjek = a.AkPVObjek
                            .Where(obj => obj.AkCarta?.Kod != null)
                            .ToList();

                        foreach (var obj in cartaObjek)
                        {
                            var akCartaKod = obj.AkCarta?.Kod;
                            var akCartaPerihal = obj.AkCarta?.Perihal;

                            if (akCartaKod != null && !AkCartaKodList.Contains(akCartaKod))
                            {
                                var jumlah = jumlahByAkCartaKod.GetValueOrDefault(akCartaKod, 0);

                                // Find or create a matching entry in akPvResult based on NoRujukanPV, Perihal, and akCartaKod
                                var CartaResult = akPvResult
                                    .FirstOrDefault(x => x.NoRujukanPV == a.NoRujukan
                                    && x.Perihal == a.Ringkasan
                                    && x.akCartaKod == null);

                                if (CartaResult != null)
                                {
                                    // Assign akCarta details to the matched entry
                                    CartaResult.akCartaKod = akCartaKod;
                                    CartaResult.akCartaPerihal = akCartaPerihal;
                                    CartaResult.Jumlah = jumlah;

                                    AkCartaKodList.Add(akCartaKod);

                                }
                            }
                        }
                    }
                }


                var PvResult = akPvResult
                     .GroupBy(b => new { b.TarikhPV, b.NoRujukanPV, b.Perihal, b.Amaun, b.NoRujukanCek, b.akCartaKod })
                     .Select(g => new LAK009PrintModel
                     {
                         TarikhPV = g.First().TarikhPV,
                         NoRujukanPV = g.First().NoRujukanPV,
                         Perihal = g.First().Perihal,
                         Amaun = g.Sum(a => a.Amaun),
                         NoRujukanCek = g.First().NoRujukanCek,
                         PenerimaCek = g.First().PenerimaCek,
                         akCartaKod = null,
                         akCartaPerihal = null,
                         Jumlah = 0
                     })
                     .OrderBy(b => b.TarikhPV)
                     .ThenBy(b => b.NoRujukanPV)
                     .ThenBy(b => b.Perihal)
                     .ToList();


                var akCartaResult = akPvResult
                    .Where(b => b.akCartaKod != null)
                    .GroupBy(b => new { b.akCartaKod, b.akCartaPerihal })
                    .Select(g => new LAK009PrintModel
                    {
                        TarikhPV = null,
                        NoRujukanPV = null,
                        Perihal = null,
                        Amaun = 0,
                        NoRujukanCek = null,
                        PenerimaCek = null,
                        akCartaKod = g.Key.akCartaKod,
                        akCartaPerihal = g.First().akCartaPerihal,
                        Jumlah = g.Sum(a => a.Jumlah)
                    })
                    .OrderBy(g => g.akCartaKod)
                    .ToList();



                var finalPvResult = PvResult.Concat(akCartaResult)
                    .ToList();


                return finalPvResult;
            }



            // functions
            public bool PVWithoutInvois(AkPV akPV)
        {
            if (akPV.IsInvois == false && akPV.IsTanggungan == false && akPV.IsAkru == false && akPV.AkPVInvois != null && akPV.AkPVInvois.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool PVWithOneInvoisNotAkru(AkPV akPV)
        {
            if (akPV.IsInvois == true && akPV.IsTanggungan == false && akPV.IsAkru == false && akPV.AkPVInvois != null && akPV.AkPVInvois.Count == 1)
            {
                bool result = false;

                foreach (var invois in akPV.AkPVInvois)
                {
                    if (invois.IsTanggungan == false && invois.AkBelian != null && invois.AkBelian.AkAkaunAkruId == null)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }

                return result;
            }
            else
            {
                return false;
            }
        }

        public bool PVWithOneInvoisAkruWithoutPOOrInden(AkPV akPV)
        {
            if (akPV.IsInvois == true && akPV.IsTanggungan == false && akPV.IsAkru == true && akPV.AkPVInvois != null && akPV.AkPVInvois.Count > 0)
            {
                var result = false;

                foreach (var invois in akPV.AkPVInvois)
                {
                    if (invois.IsTanggungan == false && invois.AkBelian != null && invois.AkBelian.AkAkaunAkruId != null)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }

                return result;
            }
            else
            {
                return false;
            }
        }

        public bool PVWithOneInvoisAkruWithOnePOAndWithoutInden(AkPV akPV)
        {
            if (akPV.IsInvois == true && akPV.IsTanggungan == true && akPV.IsAkru == true && akPV.AkPVInvois != null && akPV.AkPVInvois.Count > 0)
            {
                bool result = false;
                foreach (var invois in akPV.AkPVInvois)
                {
                    if (invois.IsTanggungan == true && invois.AkBelian != null && invois.AkBelian.AkAkaunAkruId != null)
                    {
                        if (invois.AkBelian.AkPOId != null && invois.AkBelian.AkIndenId == null)
                        {
                            result = true;
                        }
                        else
                        {
                            result = false;
                        }
                    }
                }
                return result;
            }
            else { return false; }
        }

        public bool PVWithOneInvoisAkruWithOneIndenAndWithoutPO(AkPV akPV)
        {
            if (akPV.IsInvois == true && akPV.IsTanggungan == true && akPV.IsAkru == true && akPV.AkPVInvois != null && akPV.AkPVInvois.Count > 0)
            {
                bool result = false;
                foreach (var invois in akPV.AkPVInvois)
                {
                    if (invois.IsTanggungan == true && invois.AkBelian != null && invois.AkBelian.AkAkaunAkruId != null)
                    {
                        if (invois.AkBelian.AkIndenId != null && invois.AkBelian.AkPOId == null)
                        {
                            result = true;
                        }
                        else
                        {
                            result = false;
                        }
                    }
                }
                return result;
            }
            else { return false; }
        }

        public bool PVWithMultipleInvoisNotAkru(AkPV akPV)
        {
            if (akPV.IsInvois == true && akPV.IsTanggungan == false && akPV.IsAkru == false && akPV.AkPVInvois != null && akPV.AkPVInvois.Count > 1)
            {
                bool result = false;
                foreach (var invois in akPV.AkPVInvois)
                {
                    if (invois.IsTanggungan == false && invois.AkBelian != null && invois.AkBelian.AkAkaunAkruId == null)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                return result;
            }
            else
            {
                return false;
            }
        }

        public bool PVWithMultipleInvoisAkruWithoutPOOrInden(AkPV akPV)
        {
            if (akPV.IsInvois == true && akPV.IsTanggungan == false && akPV.IsAkru == true && akPV.AkPVInvois != null && akPV.AkPVInvois.Count > 1)
            {
                bool result = false;
                foreach (var invois in akPV.AkPVInvois)
                {
                    if (invois.IsTanggungan == false && invois.AkBelian != null && invois.AkBelian.AkAkaunAkruId != null)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                return result;
            }
            else
            {
                return false;
            }
        }

        public bool PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek(AkPV akPV)
        {
            if (akPV.IsInvois == true && akPV.IsTanggungan == true && akPV.IsAkru == true && akPV.AkPVInvois != null && akPV.AkPVInvois.Count > 1)
            {
                bool result = false;
                List<AkPOObjek> poList = new List<AkPOObjek>();
                foreach (var invois in akPV.AkPVInvois)
                {
                    if (invois.IsTanggungan == true && invois.AkBelian != null && invois.AkBelian.AkAkaunAkruId != null && invois.AkBelian.AkPOId != null && invois.AkBelian.AkIndenId == null && invois.AkBelian.AkPO != null && invois.AkBelian.AkPO.AkPOObjek != null && invois.AkBelian.AkPO.AkPOObjek.Count > 0)
                    {
                        // each insert poObjekList into poList
                        poList.AddRange(invois.AkBelian.AkPO.AkPOObjek);

                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }

                if (result == true)
                {
                    poList = poList.GroupBy(x => x.AkCartaId)
                        .Select(l => new AkPOObjek()
                        {
                            AkCartaId = l.First().AkCartaId,
                            JKWPTJBahagianId = l.First().JKWPTJBahagianId,
                            Counter = l.Count()
                        }).ToList();

                    if (poList != null && poList.Count > 0)
                    {
                        foreach (var po in poList)
                        {
                            if (po.Counter > 1)
                            {
                                result = true;
                            }
                            else
                            {
                                result = false;
                            }
                        }
                    }
                }

                return result;
            }
            else
            {
                return false;
            }
        }

        public bool PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneDifferentObjek(AkPV akPV)
        {
            if (akPV.IsInvois == true && akPV.IsTanggungan == true && akPV.IsAkru == true && akPV.AkPVInvois != null && akPV.AkPVInvois.Count > 1)
            {
                bool result = false;
                List<AkPOObjek> poList = new List<AkPOObjek>();
                foreach (var invois in akPV.AkPVInvois)
                {
                    if (invois.IsTanggungan == true && invois.AkBelian != null && invois.AkBelian.AkAkaunAkruId != null && invois.AkBelian.AkPOId != null && invois.AkBelian.AkIndenId == null && invois.AkBelian.AkPO != null && invois.AkBelian.AkPO.AkPOObjek != null && invois.AkBelian.AkPO.AkPOObjek.Count > 0)
                    {
                        // each insert poObjekList into poList
                        poList.AddRange(invois.AkBelian.AkPO.AkPOObjek);

                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }

                if (result == true)
                {
                    poList = poList.GroupBy(x => x.AkCartaId)
                        .Select(l => new AkPOObjek()
                        {
                            AkCartaId = l.First().AkCartaId,
                            JKWPTJBahagianId = l.First().JKWPTJBahagianId,
                            Counter = l.Count()
                        }).ToList();

                    if (poList != null && poList.Count > 0)
                    {
                        foreach (var po in poList)
                        {
                            if (po.Counter == 1)
                            {
                                result = true;
                                break;
                            }
                            else
                            {
                                result = false;
                            }
                        }
                    }
                }

                return result;
            }
            else
            {
                return false;
            }
        }
    }
}