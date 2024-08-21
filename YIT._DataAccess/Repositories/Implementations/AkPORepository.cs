using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class AkPORepository : _GenericRepository<AkPO>, IAkPORepository
    {
        private readonly ApplicationDbContext _context;

        public AkPORepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public AkPO GetDetailsById(int id)
        {
            return _context.AkPO
                .IgnoreQueryFilters()
                .Include(t => t.LHDNMSIC)
                .Include(t => t.JKW)
                .Include(t => t.DDaftarAwam)
                .Include(t => t.DPekerjaPosting)
                .Include(t => t.AkPenilaianPerolehan)
                .Include(t => t.DPengesah)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPenyemak)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPelulus)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.AkPOObjek)!
                    .ThenInclude(to => to.AkCarta)
                .Include(t => t.AkPOObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JKW)
                .Include(t => t.AkPOObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JPTJ)
                .Include(t => t.AkPOObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JBahagian)
                .Include(t => t.AkPOPerihal)
                .FirstOrDefault(pp => pp.Id == id) ?? new AkPO();
        }

        public List<AkPO> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang)
        {
            if (searchString == null && dateFrom == null && dateTo == null && orderBy == null)
            {
                return new List<AkPO>();
            }

            var akPOList = _context.AkPO
                .IgnoreQueryFilters()
                .Include(t => t.LHDNMSIC)
                .Include(t => t.JKW)
                .Include(t => t.DDaftarAwam)
                .Include(t => t.DPekerjaPosting)
                .Include(t => t.AkPenilaianPerolehan)
                .Include(t => t.DPengesah)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPenyemak)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPelulus)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.AkPOObjek)!
                    .ThenInclude(to => to.AkCarta)
                .Include(t => t.AkPOObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JKW)
                .Include(t => t.AkPOObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JPTJ)
                .Include(t => t.AkPOObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JBahagian)
                        .Where(t => t.Tarikh >= dateFrom && t.Tarikh <= dateTo!.Value.AddHours(23.99))
                        .ToList();


            // searchstring filters
            if (searchString != null)
            {
                akPOList = akPOList.Where(t =>
                t.NoRujukan!.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                || t.DDaftarAwam!.Nama!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            // searchString filters end

            // status borang filters
            switch (enStatusBorang)
            {
                case EnStatusBorang.None:
                    akPOList = akPOList.Where(pp => pp.EnStatusBorang == EnStatusBorang.None).ToList();
                    break;
                case EnStatusBorang.Sah:
                    akPOList = akPOList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Sah).ToList();
                    break;
                case EnStatusBorang.Semak:
                    akPOList = akPOList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Semak).ToList();
                    break;
                case EnStatusBorang.Lulus:
                    akPOList = akPOList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Lulus).ToList();
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
                        akPOList = akPOList.OrderBy(t => t.DDaftarAwam!.Nama).ToList();
                        break;
                    case "Tarikh":
                        akPOList = akPOList.OrderBy(t => t.Tarikh).ToList(); break;
                    default:
                        akPOList = akPOList.OrderBy(t => t.NoRujukan).ToList();
                        break;
                }

            }
            // order by filters end

            return akPOList;
        }

        public List<AkPO> GetResultsByDPekerjaIdFromDKonfigKelulusan(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang, int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan)
        {

            // get all data with details
            List<AkPO> akPOList = GetResults(searchString, dateFrom, dateTo, orderBy, enStatusBorang);

            var filterings = FilterByComparingJBahagianAkPOObjekWithJBahagianDKonfigKelulusan(dPekerjaId, enKategoriKelulusan, enJenisModulKelulusan, akPOList);

            var results = FilterByComparingJumlahAkPOWithMinAmountMaxAmountDKonfigKelulusan(dPekerjaId, enKategoriKelulusan, enJenisModulKelulusan, filterings);

            return results;
        }

        public List<AkPO> FilterByComparingJBahagianAkPOObjekWithJBahagianDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AkPO> akPOList)
        {
            // initialize result sets
            List<AkPO> results = new List<AkPO>();

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

                var akPOGroup = new List<AkPOObjek>().GroupBy(objek => objek.JKWPTJBahagianId);
                if (akPOList != null && akPOList.Count > 0)
                {
                    foreach (var akPO in akPOList)
                    {
                        var penilaianPerolehanObjekBahagianList = new List<JBahagian>();

                        // group akPOObjek by bahagian
                        if (akPO.AkPOObjek != null && akPO.AkPOObjek.Count > 0)
                        {
                            foreach (var item in akPO.AkPOObjek)
                            {
                                penilaianPerolehanObjekBahagianList.Add(item.JKWPTJBahagian?.JBahagian ?? new JBahagian());
                            }

                        }
                        // if konfigKelulusan bahagian null, bypass all, add to results
                        if (konfigKelulusanBahagianList.Count == 0)
                        {
                            results.Add(akPO);
                            continue;
                        }

                        // compare each lists, if its equal then insert to results
                        var items = penilaianPerolehanObjekBahagianList.All(konfigKelulusanBahagianList.Contains);
                        if (konfigKelulusanBahagianList.OrderBy(kk => kk.Kod).SequenceEqual(penilaianPerolehanObjekBahagianList.OrderBy(pp => pp.Kod))
                            || penilaianPerolehanObjekBahagianList.All(konfigKelulusanBahagianList.Contains))
                        {

                            results.Add(akPO);
                            continue;
                        };
                    }
                }
            }


            return results;
        }


        public List<AkPO> FilterByComparingJumlahAkPOWithMinAmountMaxAmountDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AkPO> filterings)
        {
            //initialize new list akPO
            List<AkPO> results = new List<AkPO>();

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
            bool isSah = await _context.AkPO.AnyAsync(t => t.Id == id && (t.EnStatusBorang == EnStatusBorang.Sah || t.EnStatusBorang == EnStatusBorang.Semak || t.EnStatusBorang == EnStatusBorang.Lulus));
            if (isSah)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsSemakAsync(int id)
        {
            bool isSemak = await _context.AkPO.AnyAsync(t => t.Id == id && (t.EnStatusBorang == EnStatusBorang.Semak || t.EnStatusBorang == EnStatusBorang.Lulus));
            if (isSemak)
            {
                return true;
            }

            return false;
        }


        public async Task<bool> IsLulusAsync(int id)
        {
            bool isLulus = await _context.AkPO.AnyAsync(t => t.Id == id && t.EnStatusBorang == EnStatusBorang.Lulus);
            if (isLulus)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsPostedAsync(int id, string noRujukan)
        {
            bool isPosted = await _context.AkPO.AnyAsync(t => t.Id == id && t.FlPosting == 1);
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
            var data = _context.AkPO.FirstOrDefault(pp => pp.Id == id);
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
            var data = _context.AkPO.FirstOrDefault(pp => pp.Id == id);
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
            bool isBatal = await _context.AkPO.AnyAsync(t => t.Id == id && t.FlBatal == 1);
            if (isBatal)
            {
                return true;
            }

            return false;
        }

        public void Batal(int id, string? sebabBatal, string? userId)
        {
            var data = _context.AkPO.FirstOrDefault(pp => pp.Id == id);

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
            var max = _context.AkPO.Where(pp => pp.Tahun == tahun).OrderByDescending(pp => pp.NoRujukan).ToList();

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

        public void PostingToAbBukuVot(AkPO akPO)
        {
            List<AbBukuVot> abBukuVotList = new List<AbBukuVot>();

            if (akPO.AkPOObjek != null && akPO.AkPOObjek.Count > 0)
            {


                foreach (var item in akPO.AkPOObjek)
                {
                    AbBukuVot abBukuVot = new AbBukuVot()
                    {
                        Tahun = akPO.Tahun,
                        JKWId = item.JKWPTJBahagian?.JKWId ?? akPO.JKWId,
                        JPTJId = (int)item.JKWPTJBahagian!.JPTJId,
                        JBahagianId = item.JKWPTJBahagian.JBahagianId,
                        Tarikh = akPO.Tarikh,
                        DDaftarAwamId = akPO.DDaftarAwamId,
                        VotId = item.AkCartaId,
                        NoRujukan = akPO.NoRujukan,
                        Tanggungan = item.Amaun,
                        Tbs = item.Amaun,
                        Baki = -item.Amaun
                    };

                    abBukuVotList.Add(abBukuVot);
                }
            }

            _context.AbBukuVot.AddRange(abBukuVotList);
        }

        public void RemovePostingFromAbBukuVot(AkPO akPO, string userId)
        {
            var abBukuVotList = _context.AbBukuVot.Where(b => b.NoRujukan == akPO.NoRujukan).ToList();

            if (abBukuVotList != null && abBukuVotList.Count > 0)
            {
                _context.RemoveRange(abBukuVotList);
            }

        }

        public List<AkPO> GetAllByStatus(EnStatusBorang enStatusBorang)
        {
            return _context.AkPO.Where(pp => pp.EnStatusBorang == enStatusBorang).ToList();
        }

        public void HantarSemula(int id, string? tindakan, string? userId)
        {
            var data = _context.AkPO.FirstOrDefault(pp => pp.Id == id);

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
            var data = _context.AkPO.FirstOrDefault(pp => pp.Id == id);

            if (data != null)
            {
                HantarSemula(id, tindakan, userId);

                RemovePostingFromAbBukuVot(data, userId ?? "");

            }
            
        }

        public void BatalPos(int id, string? tindakan, string? userId)
        {
            var data = _context.AkPO.FirstOrDefault(pp => pp.Id == id);

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

        public AkPO GetBalanceAdjustmentFromAkPelarasanPO(AkPO akPO)
        {

            var akPelarasanPOObjekAkPOObjekGrouped = new List<AkPOObjek>();
            var akPelarasanPOPerihalAkPOPerihalGrouped = new List<AkPOPerihal>();

            var akPelarasanPO = _context.AkPelarasanPO
                .Include(t => t.AkPelarasanPOObjek)!
                    .ThenInclude(to => to.AkCarta)
                .Include(t => t.AkPelarasanPOObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JKW)
                .Include(t => t.AkPelarasanPOObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JPTJ)
                .Include(t => t.AkPelarasanPOObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JBahagian)
                .Include(t => t.AkPelarasanPOPerihal)
                .FirstOrDefault(pp => pp.AkPOId == akPO.Id);

            if (akPelarasanPO != null)
            {
                akPO.Jumlah += akPelarasanPO.Jumlah;

                if (akPelarasanPO.AkPelarasanPOObjek != null && akPelarasanPO.AkPelarasanPOObjek.Count > 0 && akPO.AkPOObjek != null && akPO.AkPOObjek.Count > 0)
                {

                    foreach (var objek in akPO.AkPOObjek)
                    {
                        akPelarasanPOObjekAkPOObjekGrouped.Add(objek);
                    }

                    foreach (var objek in akPelarasanPO.AkPelarasanPOObjek)
                    {
                        var akPelarasanPOObjek = new AkPOObjek
                        {
                            AkPOId = akPO.Id,
                            AkPO = akPO,
                            JKWPTJBahagianId = objek.JKWPTJBahagianId,
                            JKWPTJBahagian = objek.JKWPTJBahagian,
                            AkCartaId = objek.AkCartaId,
                            AkCarta = objek.AkCarta,
                            Amaun = objek.Amaun
                        };
                        akPelarasanPOObjekAkPOObjekGrouped.Add(akPelarasanPOObjek);
                    }

                    akPelarasanPOObjekAkPOObjekGrouped = akPelarasanPOObjekAkPOObjekGrouped.GroupBy(b => new {b.AkCartaId, b.JKWPTJBahagianId}).Select(l => new AkPOObjek
                    {
                        Id =l.First().Id,
                        AkPOId = l.First().AkPOId,
                        AkPO = l.First().AkPO,
                        JKWPTJBahagianId = l.First().JKWPTJBahagianId,
                        JKWPTJBahagian = l.First().JKWPTJBahagian,
                        AkCartaId = l.First().AkCartaId,
                        AkCarta = l.First().AkCarta,
                        Amaun = l.Sum(l => l.Amaun)
                    }).ToList();
                    
                }

                if (akPelarasanPO.AkPelarasanPOPerihal != null && akPelarasanPO.AkPelarasanPOPerihal.Count > 0 && akPO.AkPOPerihal != null && akPO.AkPOPerihal.Count > 0)
                {

                    decimal bil = 1;
                    foreach (var perihal in akPO.AkPOPerihal)
                    {
                        akPelarasanPOPerihalAkPOPerihalGrouped.Add(perihal);
                        bil++;
                    }

                    foreach (var perihal in akPelarasanPO.AkPelarasanPOPerihal)
                    {
                        var akPelarasanPOPerihal = new AkPOPerihal
                        {
                            AkPOId = akPO.Id,
                            AkPO = akPO,
                            Bil = bil,
                            Perihal = perihal.Perihal,
                            Kuantiti = perihal.Kuantiti,
                            Unit = perihal.Unit,
                            Harga = perihal.Harga,
                            Amaun = perihal.Amaun
                        };
                        bil++;
                        akPelarasanPOPerihalAkPOPerihalGrouped.Add(akPelarasanPOPerihal);
                    }

                }

                akPO.AkPOObjek = akPelarasanPOObjekAkPOObjekGrouped;
                akPO.AkPOPerihal = akPelarasanPOPerihalAkPOPerihalGrouped;

            }

            return akPO;
        }
    }
}