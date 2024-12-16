using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._02Daftar;
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
            return _context.AkPenilaianPerolehan
                .IgnoreQueryFilters()
                //.Include(t => t.AkPV)
                .Include(t => t.JKW)
                .Include(t => t.LHDNMSIC)
                .Include(t => t.DPemohon)
                .Include(t => t.DDaftarAwam)
                .Include(t => t.DPekerjaPosting)
                .Include(t => t.DPengesah)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPenyemak)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPelulus)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.AkPenilaianPerolehanObjek)!
                    .ThenInclude(to => to.AkCarta)
                .Include(t => t.AkPenilaianPerolehanObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JKW)
                .Include(t => t.AkPenilaianPerolehanObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JPTJ)
                .Include(t => t.AkPenilaianPerolehanObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JBahagian)
                .Include(t => t.AkPenilaianPerolehanPerihal)!
                    .ThenInclude(t => t.LHDNUnitUkuran)
                .FirstOrDefault(pp => pp.Id == id) ?? new AkPenilaianPerolehan();
        }

        public List<AkPenilaianPerolehan> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang)
        {
            if (searchString == null && dateFrom == null && dateTo == null && orderBy == null)
            {
                return new List<AkPenilaianPerolehan>();
            }

            var akPPList = _context.AkPenilaianPerolehan
                .IgnoreQueryFilters()
                //.Include(t => t.AkPV)
                .Include(t => t.LHDNMSIC)
                .Include(t => t.JKW)
                .Include(t => t.DPemohon)
                .Include(t => t.DDaftarAwam)
                .Include(t => t.DPekerjaPosting)
                .Include(t => t.DPengesah)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPenyemak)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPelulus)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.AkPenilaianPerolehanObjek)!
                    .ThenInclude(to => to.AkCarta)
                .Include(t => t.AkPenilaianPerolehanObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JKW)
                .Include(t => t.AkPenilaianPerolehanObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JPTJ)
                .Include(t => t.AkPenilaianPerolehanObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JBahagian)
                        .Where(t => t.Tarikh >= dateFrom && t.Tarikh <= dateTo!.Value.AddHours(23.99)).ToList();

            // searchstring filters
            if (searchString != null)
            {
                akPPList = akPPList.Where(t =>
                t.NoRujukan!.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                || t.DPemohon!.Nama!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            // searchString filters end

            // status borang filters
            switch (enStatusBorang)
            {
                case EnStatusBorang.None:
                    akPPList = akPPList.Where(pp => pp.EnStatusBorang == EnStatusBorang.None).ToList();
                    break;
                case EnStatusBorang.Sah:
                    akPPList = akPPList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Sah).ToList();
                    break;
                case EnStatusBorang.Semak:
                    akPPList = akPPList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Semak).ToList();
                    break;
                case EnStatusBorang.Lulus:
                    akPPList = akPPList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Lulus).ToList();
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
                        akPPList = akPPList.OrderBy(t => t.DPemohon!.Nama).ToList();
                        break;
                    case "Tarikh":
                        akPPList = akPPList.OrderBy(t => t.Tarikh).ToList(); break;
                    default:
                        akPPList = akPPList.OrderBy(t => t.NoRujukan).ToList();
                        break;
                }

            }
            // order by filters end

            return akPPList;
        }

        public List<AkPenilaianPerolehan> GetResults1(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang, int? jKWId)
        {
            if (searchString == null && dateFrom == null && dateTo == null && orderBy == null && jKWId == null)
            {
                return new List<AkPenilaianPerolehan>();
            }

            var akPPList = _context.AkPenilaianPerolehan
                .IgnoreQueryFilters()
                //.Include(t => t.AkPV)
                .Include(t => t.LHDNMSIC)
                .Include(t => t.JKW)
                .Include(t => t.DPemohon)
                .Include(t => t.DDaftarAwam)
                .Include(t => t.DPekerjaPosting)
                .Include(t => t.DPengesah)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPenyemak)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPelulus)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.AkPenilaianPerolehanObjek)!
                    .ThenInclude(to => to.AkCarta)
                .Include(t => t.AkPenilaianPerolehanObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JKW)
                .Include(t => t.AkPenilaianPerolehanObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JPTJ)
                .Include(t => t.AkPenilaianPerolehanObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JBahagian)
                        .Where(t => t.Tarikh >= dateFrom && t.Tarikh <= dateTo!.Value.AddHours(23.99)).ToList();

            // searchstring filters
            if (searchString != null)
            {
                akPPList = akPPList.Where(t =>
                t.NoRujukan!.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                || t.DPemohon!.Nama!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            // searchString filters end

            // status borang filters
            switch (enStatusBorang)
            {
                case EnStatusBorang.None:
                    akPPList = akPPList.Where(pp => pp.EnStatusBorang == EnStatusBorang.None).ToList();
                    break;
                case EnStatusBorang.Sah:
                    akPPList = akPPList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Sah).ToList();
                    break;
                case EnStatusBorang.Semak:
                    akPPList = akPPList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Semak).ToList();
                    break;
                case EnStatusBorang.Lulus:
                    akPPList = akPPList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Lulus).ToList();
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
                        akPPList = akPPList.OrderBy(t => t.DPemohon!.Nama).ToList();
                        break;
                    case "Tarikh":
                        akPPList = akPPList.OrderBy(t => t.Tarikh).ToList(); break;
                    default:
                        akPPList = akPPList.OrderBy(t => t.NoRujukan).ToList();
                        break;
                }

            }
            // order by filters end

            if (jKWId != null)
            {
                akPPList = akPPList.Where(wr => wr.JKWId == jKWId).ToList();
            }

            return akPPList;
        }

        public List<AkPenilaianPerolehan> GetResultsByDPekerjaIdFromDKonfigKelulusan(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang, int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan)
        {
            
            // get all data with details
            List<AkPenilaianPerolehan> akPPList = GetResults(searchString, dateFrom, dateTo, orderBy, enStatusBorang);

            var filterings = FilterByComparingJBahagianAkPenilaianObjekWithJBahagianDKonfigKelulusan(dPekerjaId,enKategoriKelulusan,enJenisModulKelulusan, akPPList);

            var results = FilterByComparingJumlahAkPenilaianPerolehanWithMinAmountMaxAmountDKonfigKelulusan(dPekerjaId, enKategoriKelulusan, enJenisModulKelulusan, filterings);

            return results;
        }

        public List<AkPenilaianPerolehan> FilterByComparingJBahagianAkPenilaianObjekWithJBahagianDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AkPenilaianPerolehan> akPPList)
        {
            // initialize result sets
            List<AkPenilaianPerolehan> results = new List<AkPenilaianPerolehan>();

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
                
                foreach(var item in konfigKelulusanBahagianGrouped)
                {
                    if (item.JBahagian != null) konfigKelulusanBahagianList.Add(item.JBahagian);
                }
                
                var akPPGroup = new List<AkPenilaianPerolehanObjek>().GroupBy(objek => objek.JKWPTJBahagianId);
                if (akPPList != null && akPPList.Count > 0)
                {
                    foreach (var akPP in akPPList)
                    {
                        var penilaianPerolehanObjekBahagianList = new List<JBahagian>();

                        // group akPPObjek by bahagian
                        if (akPP.AkPenilaianPerolehanObjek != null && akPP.AkPenilaianPerolehanObjek.Count > 0)
                        {
                            foreach(var item in akPP.AkPenilaianPerolehanObjek)
                            {
                                penilaianPerolehanObjekBahagianList.Add(item.JKWPTJBahagian?.JBahagian ?? new JBahagian());
                            }

                        }
                        // if konfigKelulusan bahagian null, bypass all, add to results
                        if (konfigKelulusanBahagianList.Count == 0)
                        {
                            results.Add(akPP);
                            continue;
                        }

                        // compare each lists, if its equal then insert to results
                        var items = penilaianPerolehanObjekBahagianList.All(konfigKelulusanBahagianList.Contains);
                        if (konfigKelulusanBahagianList.OrderBy(kk => kk.Kod).SequenceEqual(penilaianPerolehanObjekBahagianList.OrderBy(pp => pp.Kod)) 
                            || penilaianPerolehanObjekBahagianList.All(konfigKelulusanBahagianList.Contains))
                        {

                            results.Add(akPP); 
                            continue;
                        };
                    }
                }
            }

           
            return results;
        }


        public List<AkPenilaianPerolehan> FilterByComparingJumlahAkPenilaianPerolehanWithMinAmountMaxAmountDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, List<AkPenilaianPerolehan> filterings)
        {
            //initialize new list akPP
            List<AkPenilaianPerolehan> results = new List<AkPenilaianPerolehan>();

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

        public async Task<List<_AkPenilaianPerolehanResult>> GetResultsGroupByBelumBayar(string? tarikhDari, string? tarikhHingga, int? jKWId)
        {
            if (string.IsNullOrEmpty(tarikhDari) || string.IsNullOrEmpty(tarikhHingga) || jKWId == null)
            {
                return new List<_AkPenilaianPerolehanResult>();
            }

            DateTime date1 = DateTime.Parse(tarikhDari).Date;
            DateTime date2 = DateTime.Parse(tarikhHingga).Date;

            var akPP = await _context.AkPenilaianPerolehan
                .Where(a => (a.FlPOInden == 1) && (a.FlBatal == 0) && a.Tarikh >= date1 && a.Tarikh <= date2 && a.JKWId == jKWId)
                .ToListAsync();

            var akPORecords = await _context.AkPO
                .Where(p => akPP.Select(a => a.Id).Contains(p.AkPenilaianPerolehanId))
                .ToListAsync();

            var akIndenRecords = await _context.AkInden
                .Where(k => akPP.Select(a => a.Id).Contains(k.AkPenilaianPerolehanId))
                .ToListAsync();

            var akBelianRecords = await _context.AkBelian
                .Where(b => (b.AkPOId == null && b.AkIndenId == null) ||  
                      (b.AkPOId != null && akPORecords.Select(po => po.Id).Contains(b.AkPOId.Value)) ||
                      (b.AkIndenId != null && akIndenRecords.Select(inden => inden.Id).Contains(b.AkIndenId.Value)))
                .ToListAsync();

            var akPVInvoisRecords = await _context.AkPVInvois
                .Where(v => akBelianRecords.Select(b => b.Id).Contains(v.AkBelianId))
                .ToListAsync();

            var belianWithoutPVInvois = akBelianRecords
                .Where(b => !akPVInvoisRecords.Select(v => v.AkBelianId).Contains(b.Id)) 
                .ToList();

            var filteredAkPP = akPP
                .Where(pp => belianWithoutPVInvois.Any(b =>
                    (b.AkPOId != null && b.AkPOId == akPORecords.FirstOrDefault(po => po.AkPenilaianPerolehanId == pp.Id)?.Id) ||
                    (b.AkIndenId != null && b.AkIndenId == akIndenRecords.FirstOrDefault(inden => inden.AkPenilaianPerolehanId == pp.Id)?.Id)) || 
                    akIndenRecords.Any(inden => inden.AkPenilaianPerolehanId == pp.Id && (inden.FlBatal == 0) && !akBelianRecords.Any(b => b.AkIndenId == inden.Id)) || 
                    akIndenRecords.Any(inden => inden.AkPenilaianPerolehanId == pp.Id && inden.FlBatal == 1 && !akBelianRecords.Any(b => b.AkIndenId == inden.Id)) || 
                    akPORecords.Any(po => po.AkPenilaianPerolehanId == pp.Id && (po.FlBatal == 0) && !akBelianRecords.Any(b => b.AkPOId == po.Id)) || 
                    akPORecords.Any(po => po.AkPenilaianPerolehanId == pp.Id && po.FlBatal == 1 && !akBelianRecords.Any(b => b.AkPOId == po.Id))
                      )
                .ToList();

            var results = filteredAkPP
                .GroupBy(a => new
                {
                    a.NoRujukanLama,
                    a.Jumlah,
                    a.Tarikh,
                    a.FlBatal,
                    DNama = a.DDaftarAwam?.Nama
                })
                .Select(g => new _AkPenilaianPerolehanResult
                {
                    NoRujukan = g.Key.NoRujukanLama,
                    Jumlah = g.Key.Jumlah,
                    Tarikh = g.Key.Tarikh,
                    FlBatal = g.Key.FlBatal,
                    DNama = g.Key.DNama,
                    Total = g.Sum(x => x.Jumlah)
                })
                .ToList();

            return results;
        }

        public async Task<List<_AkPenilaianPerolehanResult>> GetResultsGroupByBatal(string? tarikhDari, string? tarikhHingga, int? jKWId)
        {
            if (string.IsNullOrEmpty(tarikhDari) || string.IsNullOrEmpty(tarikhHingga) || jKWId == null)
            {
                return new List<_AkPenilaianPerolehanResult>();
            }

            DateTime date1 = DateTime.Parse(tarikhDari).Date;
            DateTime date2 = DateTime.Parse(tarikhHingga).Date;

            var akPP = await _context.AkPenilaianPerolehan
                .Where(a => a.FlBatal == 1 && a.Tarikh >= date1 && a.Tarikh <= date2 && a.JKWId == jKWId)
                .ToListAsync();

            var akPORecords = await _context.AkPO
                .Where(p => akPP.Select(a => a.Id).Contains(p.AkPenilaianPerolehanId) && (p.FlBatal == 0))
                .ToListAsync();

            var akIndenRecords = await _context.AkInden
                .Where(k => akPP.Select(a => a.Id).Contains(k.AkPenilaianPerolehanId) && (k.FlBatal == 0))
                .ToListAsync();

            var akBelianRecords = await _context.AkBelian
                .Where(b => (b.AkPOId != null && akPORecords.Select(po => po.Id).Contains(b.AkPOId.Value)) ||
                      (b.AkIndenId != null && akIndenRecords.Select(inden => inden.Id).Contains(b.AkIndenId.Value))
                )
                .ToListAsync();

            var akPVInvoisRecords = await _context.AkPVInvois
                .Where(v => akBelianRecords.Select(b => b.Id).Contains(v.AkBelianId))
                .ToListAsync();

            var excludedAkPPIds = akPORecords
                .Where(po => akPVInvoisRecords.Any(v => akBelianRecords.Any(b => b.AkPOId == po.Id && b.Id == v.AkBelianId)))
                .Select(po => po.AkPenilaianPerolehanId)
                .Union(akIndenRecords
                    .Where(inden => akPVInvoisRecords.Any(v => akBelianRecords.Any(b => b.AkIndenId == inden.Id && b.Id == v.AkBelianId)))
                    .Select(inden => inden.AkPenilaianPerolehanId))
                .Distinct()
                .ToList();

            var filteredAkPP = akPP.Where(pp => !excludedAkPPIds.Contains(pp.Id)).ToList();

            var results = filteredAkPP
                .GroupBy(a => new
                {
                    a.NoRujukanLama,
                    a.Jumlah,
                    a.Tarikh,
                    a.FlBatal,
                    DNama = a.DDaftarAwam?.Nama
                })
                .Select(g => new _AkPenilaianPerolehanResult
                {
                    NoRujukan = g.Key.NoRujukanLama,
                    Jumlah = g.Key.Jumlah,
                    Tarikh = g.Key.Tarikh,
                    FlBatal = g.Key.FlBatal,
                    DNama = g.Key.DNama,
                    Total = g.Sum(x => x.Jumlah)
                })
                .ToList();

            return results; 
        }

        public async Task<bool> IsSahAsync(int id)
        {
            bool isSah = await _context.AkPenilaianPerolehan.AnyAsync(t => t.Id == id && (t.EnStatusBorang == EnStatusBorang.Sah || t.EnStatusBorang == EnStatusBorang.Semak || t.EnStatusBorang == EnStatusBorang.Lulus));
            if (isSah)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsSemakAsync(int id)
        {
            bool isSemak = await _context.AkPenilaianPerolehan.AnyAsync(t => t.Id == id && (t.EnStatusBorang == EnStatusBorang.Semak || t.EnStatusBorang == EnStatusBorang.Lulus));
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

        public void Sah(int id, int? pengesahId, string? userId)
        {
            var data = _context.AkPenilaianPerolehan.FirstOrDefault(pp => pp.Id == id);
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
            var data = _context.AkPenilaianPerolehan.FirstOrDefault(pp => pp.Id == id);
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
            var data = _context.AkPenilaianPerolehan.FirstOrDefault(pp => pp.Id == id);
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

            }
        }

        public  void HantarSemula(int id, string? tindakan, string? userId)
        {
            var data = _context.AkPenilaianPerolehan.FirstOrDefault(pp => pp.Id == id);

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
            bool isBatal = await _context.AkPenilaianPerolehan.AnyAsync(t => t.Id == id && t.FlBatal == 1);
            if (isBatal)
            {
                return true;
            }

            return false;
        }

        public  void Batal(int id, string? sebabBatal, string? userId)
        {
            var data =  _context.AkPenilaianPerolehan.FirstOrDefault(pp => pp.Id == id);

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

        public List<AkPenilaianPerolehan> GetAllByJenis(int flPOInden)
        {
            return _context.AkPenilaianPerolehan.Where(pp => pp.EnStatusBorang == EnStatusBorang.Lulus && pp.FlPOInden == flPOInden).ToList();
        }

        public void BatalLulus(int id, string? tindakan, string? userId)
        {
            HantarSemula(id, tindakan, userId);

        }
        public async Task<bool> IsPostedAsync(int id, string noRujukan)
        {
            bool isPosted = await _context.AkPenilaianPerolehan.AnyAsync(t => t.Id == id && t.FlPosting == 1);
            if (isPosted)
            {
                return true;
            }

            bool isExistInAkPO = await _context.AkPO.AnyAsync(b => b.AkPenilaianPerolehanId == id);

            if (isExistInAkPO)
            {
                return true;
            }

            bool isExistInAkInden = await _context.AkInden.AnyAsync(b => b.AkPenilaianPerolehanId == id);

            if (isExistInAkInden)
            {
                return true;
            }
            return false;

        }

        public void BatalPos(int id, string? tindakan, string? userId)
        {
            var data = _context.AkPenilaianPerolehan.FirstOrDefault(pp => pp.Id == id);

            if (data != null)
            {
                data.EnStatusBorang = EnStatusBorang.Kemaskini;
                data.Tindakan = tindakan;

                data.UserIdKemaskini = userId ?? "";
                data.TarKemaskini = DateTime.Now;

                data.FlPosting = 0;
                data.TarikhPosting = null;

                _context.Update(data);


            }
        }
    }
}
