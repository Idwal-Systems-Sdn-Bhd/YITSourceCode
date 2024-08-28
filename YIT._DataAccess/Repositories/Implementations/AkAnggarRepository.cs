using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT.__Domain.Entities._Enums;
using System.Runtime.Intrinsics.X86;
using System.Runtime.InteropServices;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Linq;
using YIT._DataAccess.Services;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class AkAnggarRepository : _GenericRepository<AkAnggar>, IAkAnggarRepository
    {
        private readonly ApplicationDbContext _context;

        public AkAnggarRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public AkAnggar GetDetailsById(int id)
        {
            return _context.AkAnggar
                .IgnoreQueryFilters()
                .Include(t => t.DPekerjaPosting)
                .Include(t => t.DPengesah)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPenyemak)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPelulus)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.AkAnggarObjek)!
                    .ThenInclude(to => to.AkCarta)
                .Include(t => t.AkAnggarObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JKW)
                .Include(t => t.AkAnggarObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JPTJ)
                .Include(t => t.AkAnggarObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JBahagian)
                .FirstOrDefault(pp => pp.Id == id) ?? new AkAnggar();
        }

        public List<AkAnggar> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang)
        {
            if (searchString == null && dateFrom == null && dateTo == null && orderBy == null)
            {
                return new List<AkAnggar>();
            }

            var akAnggarList = _context.AkAnggar
                .IgnoreQueryFilters()
                .Include(t => t.DPekerjaPosting)
                .Include(t => t.DPengesah)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPenyemak)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPelulus)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.AkAnggarObjek)!
                    .ThenInclude(to => to.AkCarta)
                .Include(t => t.AkAnggarObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JKW)
                .Include(t => t.AkAnggarObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JPTJ)
                .Include(t => t.AkAnggarObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JBahagian)
                        .Where(t => t.Tarikh >= dateFrom && t.Tarikh <= dateTo!.Value.AddHours(23.99))
                        .ToList();

            // searchstring filters
            if (searchString != null)
            {
                akAnggarList = akAnggarList.Where(t => t.NoRujukan!.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();

            }
            // searchString filters end

            // status borang filters
            switch (enStatusBorang)
            {
                case EnStatusBorang.None:
                    akAnggarList = akAnggarList.Where(pp => pp.EnStatusBorang == EnStatusBorang.None).ToList();
                    break;
                case EnStatusBorang.Sah:
                    akAnggarList = akAnggarList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Sah).ToList();
                    break;
                case EnStatusBorang.Semak:
                    akAnggarList = akAnggarList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Semak).ToList();
                    break;
                case EnStatusBorang.Lulus:
                    akAnggarList = akAnggarList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Lulus).ToList();
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
                        akAnggarList = akAnggarList.OrderBy(t => t.Tarikh).ToList(); break;
                    default:
                        akAnggarList = akAnggarList.OrderBy(t => t.NoRujukan).ToList();
                        break;
                }

            }
            // order by filters end

            return akAnggarList;
        }

        public List<AkAnggar> GetResultsByDPekerjaIdFromDKonfigKelulusan(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang, int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan)
        {

            // get all data with details
            List<AkAnggar> akAnggarList = GetResults(searchString, dateFrom, dateTo, orderBy, enStatusBorang);

            var filterings = FilterByComparingJBahagianAbWaranObjekWithJBahagianDKonfigKelulusan(dPekerjaId, enKategoriKelulusan, enJenisModulKelulusan, akAnggarList);

            var results = FilterByComparingJumlahAbWaranWithMinAmountMaxAmountDKonfigKelulusan(dPekerjaId, enKategoriKelulusan, enJenisModulKelulusan, filterings);

            return results;
        }

        public List<AkAnggar> FilterByComparingJBahagianAbWaranObjekWithJBahagianDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AkAnggar> akAnggarList)
        {
            // initialize result sets
            List<AkAnggar> results = new List<AkAnggar>();

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

                var akAnggarGroup = new List<AkAnggarObjek>().GroupBy(objek => objek.JKWPTJBahagianId);
                if (akAnggarList != null && akAnggarList.Count > 0)
                {
                    foreach (var akAnggar in akAnggarList)
                    {
                        var anggarObjekBahagianList = new List<JBahagian>();

                        // group abWaranObjek by bahagian
                        if (akAnggar.AkAnggarObjek != null && akAnggar.AkAnggarObjek.Count > 0)
                        {
                            foreach (var item in akAnggar.AkAnggarObjek)
                            {
                                anggarObjekBahagianList.Add(item.JKWPTJBahagian?.JBahagian ?? new JBahagian());
                            }

                        }
                        // if konfigKelulusan bahagian null, bypass all, add to results
                        if (konfigKelulusanBahagianList.Count == 0)
                        {
                            results.Add(akAnggar);
                            continue;
                        }

                        // compare each lists, if its equal then insert to results
                        var items = anggarObjekBahagianList.All(konfigKelulusanBahagianList.Contains);
                        if (konfigKelulusanBahagianList.OrderBy(kk => kk.Kod).SequenceEqual(anggarObjekBahagianList.OrderBy(pp => pp.Kod))
                            || anggarObjekBahagianList.All(konfigKelulusanBahagianList.Contains))
                        {

                            results.Add(akAnggar);
                            continue;
                        };
                    }
                }
            }


            return results;
        }

        public List<AkAnggar> FilterByComparingJumlahAbWaranWithMinAmountMaxAmountDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AkAnggar> filterings)
        {
            //initialize new list akPP
            List<AkAnggar> results = new List<AkAnggar>();

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
            bool isSah = await _context.AkAnggar.AnyAsync(t => t.Id == id && (t.EnStatusBorang == EnStatusBorang.Sah || t.EnStatusBorang == EnStatusBorang.Semak || t.EnStatusBorang == EnStatusBorang.Lulus));
            if (isSah)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsSemakAsync(int id)
        {
            bool isSemak = await _context.AkAnggar.AnyAsync(t => t.Id == id && (t.EnStatusBorang == EnStatusBorang.Semak || t.EnStatusBorang == EnStatusBorang.Lulus));

            if (isSemak)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsLulusAsync(int id)
        {
            bool isLulus = await _context.AkAnggar.AnyAsync(t => t.Id == id && t.EnStatusBorang == EnStatusBorang.Lulus);
            if (isLulus)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsPostedAsync(int id, string noRujukan)
        {
            bool isPosted = await _context.AkAnggar.AnyAsync(t => t.Id == id && t.FlPosting == 1);
            if (isPosted)
            {
                return true;
            }

            bool isExistInAkAnggarLejar = await _context.AkAnggarLejar.AnyAsync(b => b.NoRujukan == noRujukan);

            if (isExistInAkAnggarLejar)
            {
                return true;
            }

            return false;

        }

        public void Sah(int id, int? pengesahId, string? userId)
        {
            var data = _context.AkAnggar.FirstOrDefault(pp => pp.Id == id);
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
            var data = _context.AkAnggar.FirstOrDefault(pp => pp.Id == id);
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

                PostingToAkAnggarLejar(data);

            }
        }

        public async Task<bool> IsBatalAsync(int id)
        {
            bool isBatal = await _context.AkAnggar.AnyAsync(t => t.Id == id && t.FlBatal == 1);
            if (isBatal)
            {
                return true;
            }

            return false;
        }

        public void Batal(int id, string? sebabBatal, string? userId)
        {
            var data = _context.AkAnggar.FirstOrDefault(pp => pp.Id == id);

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

        public void PostingToAkAnggarLejar(AkAnggar akAnggar)
        {
            List<AkAnggarLejar> AkAnggarLejarList = new List<AkAnggarLejar>();

            if (akAnggar.AkAnggarObjek != null && akAnggar.AkAnggarObjek.Count > 0)
            {


                foreach (var item in akAnggar.AkAnggarObjek)
                {
                    AkAnggarLejar AkAnggarLejar = new AkAnggarLejar()
                    {
                        Tahun = akAnggar.Tahun,
                        JKWPTJBahagian = item.JKWPTJBahagian,
                        AkCarta = item.AkCarta,
                        Tarikh = akAnggar.Tarikh,
                        Amaun = item.Amaun,
                        NoRujukan = akAnggar.NoRujukan,
                        Baki = 0,
                    };

                    AkAnggarLejarList.Add(AkAnggarLejar);
                }
            }

            _context.AkAnggarLejar.AddRange(AkAnggarLejarList);
        }

        public void RemovePostingFromAkAnggarLejar(AkAnggar akAnggar, string userId)
        {
            var AkAnggarLejarList = _context.AkAnggarLejar.Where(b => b.NoRujukan == akAnggar.NoRujukan).ToList();

            if (AkAnggarLejarList != null && AkAnggarLejarList.Count > 0)
            {
                _context.RemoveRange(AkAnggarLejarList);
            }

        }

        public void HantarSemula(int id, string? tindakan, string? userId)
        {
            var data = _context.AkAnggar.FirstOrDefault(pp => pp.Id == id);

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
            var data = _context.AkAnggar.FirstOrDefault(pp => pp.Id == id);

            if (data != null)
            {
                HantarSemula(id, tindakan, userId);

                RemovePostingFromAkAnggarLejar(data, userId ?? "");

            }

        }

        public void BatalPos(int id, string? tindakan, string? userId)
        {
            var data = _context.AkAnggar.FirstOrDefault(pp => pp.Id == id);

            if (data != null)
            {
                data.EnStatusBorang = EnStatusBorang.Kemaskini;
                data.Tindakan = tindakan;

                data.UserIdKemaskini = userId ?? "";
                data.TarKemaskini = DateTime.Now;

                data.FlPosting = 0;
                data.TarikhPosting = null;

                _context.Update(data);

                RemovePostingFromAkAnggarLejar(data, userId ?? "");

            }
        }


        public string GetMaxRefNo(string initNoRujukan, string tahun)
        {
            var max = _context.AkAnggar.Where(pp => pp.Tahun == tahun).OrderByDescending(pp => pp.NoRujukan).ToList();

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


        //public async Task<List<LAK005PrintModel>> GetAkAnggarLejarToHasil(string? Tahun1, string? Bulan, int? JPTJId, int? JKWId, int? JbahagianId, int? JKWPTJBahagianId)
        //{
        //    // Parse the year and month from the input strings
        //    int year = int.Parse(Tahun1 ?? DateTime.Now.Year.ToString());
        //    int month = string.IsNullOrEmpty(Bulan) ? 12 : int.Parse(Bulan); // Default to December if month is not specified

        //    List<LAK005PrintModel> akAkaunResult = new List<LAK005PrintModel>();

        //    if (Tahun1 != null)
        //    {
        //        var results = await _context.AkAnggarLejar
        //            .Include(b => b.AkAnggar)
        //            .ThenInclude(b => b.AkAnggarObjek!)
        //            .ThenInclude(b => b.AkCarta)
        //            .Where(b => b.Tahun == Tahun1 && b.Tarikh.Month == int.Parse(Bulan!))
        //            .ToListAsync();

        //        if (JPTJId != null)
        //        {
        //            results = results.Where(ak => ak.JKWPTJBahagianId == JPTJId).ToList();
        //        }

        //        if (JKWId != null)
        //        {
        //            results = results.Where(ak => ak.JKWPTJBahagianId == JKWId).ToList();
        //        }

        //        if (JbahagianId != null)
        //        {
        //            results = results.Where(ak => ak.JKWPTJBahagianId == JbahagianId).ToList();
        //        }



        //        var groupedResults = results
        //            .GroupBy(r => new { r.AkCarta?.Kod, r.AkCarta?.Perihal })
        //            .Select(g => new
        //            {
        //                Kod = g.Key.Kod,
        //                Perihal = g.Key.Perihal,
        //                akAnggarLejarList = g.ToList()
        //            }).ToList();

        //        int Bulan1 = int.Parse(Bulan);

        //        foreach (var group in groupedResults)
        //        {
        //            decimal cumulativeSum = 0;
        //            foreach (var akAnggarLejar in group.akAnggarLejarList)
        //            {
        //                decimal hasilBulanan = akAnggarLejar.Tarikh.Month == Bulan1 ? akAnggarLejar.Amaun : 0;
        //                decimal bakiAnggaran = akAnggarLejar.Baki;

        //                decimal hasilTerkumpul = 0;

        //                // Calculate HasilTerkumpul based on the selected month
        //                switch (Bulan1)
        //                {
        //                    case 1:
        //                        hasilTerkumpul = hasilBulanan;
        //                        break;
        //                    case 2:
        //                        hasilTerkumpul = group.akAnggarLejarList
        //                            .Where(x => x.Tarikh.Month <= 2)
        //                            .Sum(x => x.Amaun);  // Corrected to sum up 'Amaun'
        //                        break;
        //                    case 3:
        //                        hasilTerkumpul = group.akAnggarLejarList
        //                            .Where(x => x.Tarikh.Month <= 3)
        //                            .Sum(x => x.Amaun);
        //                        break;
        //                    case 4:
        //                        hasilTerkumpul = group.akAnggarLejarList
        //                            .Where(x => x.Tarikh.Month <= 4)
        //                            .Sum(x => x.Amaun);
        //                        break;
        //                    case 5:
        //                        hasilTerkumpul = group.akAnggarLejarList
        //                            .Where(x => x.Tarikh.Month <= 5)
        //                            .Sum(x => x.Amaun);
        //                        break;
        //                    case 6:
        //                        hasilTerkumpul = group.akAnggarLejarList
        //                            .Where(x => x.Tarikh.Month <= 6)
        //                            .Sum(x => x.Amaun);
        //                        break;
        //                        // Continue for other months as needed
        //                }


        //                akAkaunResult.Add(new LAK005PrintModel
        //                {
        //                    Kod = akAnggarLejar.AkCarta?.Kod,
        //                    Perihal = akAnggarLejar.AkCarta?.Perihal,
        //                    AnggaranHasil = akAnggarLejar.Amaun,
        //                    HasilBulanan = hasilBulanan,
        //                    HasilTerkumpul = hasilTerkumpul,
        //                    BakiAnggaran = bakiAnggaran
        //                });
        //            }
        //        }
        //    }

                       

        //         return akAkaunResult
        //        .GroupBy(b => new { b.Kod, b.Perihal })
        //        .Select(g =>
        //        {
        //            var firstItem = g.FirstOrDefault();
        //            decimal anggar = (decimal)g.First().AnggaranHasil;

        //            decimal hasilBulanan = (decimal)g.Sum(x => x.HasilBulanan);

        //            decimal hasilTerkumpul = (decimal)g.First().HasilTerkumpul;



        //            return new LAK005PrintModel
        //            {
        //                Kod = g.Key.Kod,
        //                Perihal = g.Key.Perihal,
        //                AnggaranHasil = anggar,
        //                HasilBulanan = hasilBulanan,
        //                HasilTerkumpul = hasilTerkumpul,
        //                BakiAnggaran = anggar - hasilTerkumpul
        //            };
        //        }).OrderBy(b => b.Kod)
        //        .ToList();
        //   }
        }
    }










