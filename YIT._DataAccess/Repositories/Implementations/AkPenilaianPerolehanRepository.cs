using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                .Include(t => t.AkPenilaianPerolehanPerihal)
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
                .ToList();

            // searchstring filters
            if (searchString != null)
            {
                akPPList = akPPList.Where(t =>
                t.NoRujukan!.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                || t.DPemohon!.Nama!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            // searchString filters end

            // date filters
            if (dateFrom != null && dateTo != null)
            {
                akPPList = akPPList.Where(t => t.Tarikh >= dateFrom && t.Tarikh <= dateTo.Value.AddHours(23.99)).ToList();
            }
            // date filters end

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

        public List<AkPenilaianPerolehan> GetResultsByDPekerjaIdFromDKonfigKelulusan(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang, int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModul enJenisModul)
        {
            
            // get all data with details
            List<AkPenilaianPerolehan> akPPList = GetResults(searchString, dateFrom, dateTo, orderBy, enStatusBorang);

            var filterings = FilterByComparingJBahagianAkPenilaianObjekWithJBahagianDKonfigKelulusan(dPekerjaId,enKategoriKelulusan,enJenisModul, akPPList);

            var results = FilterByComparingJumlahAkPenilaianPerolehanWithMinAmountMaxAmountDKonfigKelulusan(dPekerjaId, enKategoriKelulusan, enJenisModul, filterings);

            return results;
        }

        public List<AkPenilaianPerolehan> FilterByComparingJBahagianAkPenilaianObjekWithJBahagianDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModul enJenisModul, List<AkPenilaianPerolehan> akPPList)
        {
            // initialize result sets
            List<AkPenilaianPerolehan> results = new List<AkPenilaianPerolehan>();

            //get all pengesah/penyemak/pelulus with same dpekerjaId, group by pekerjaId and bahagianId

            var konfigKelulusanBahagianGrouped = _context.DKonfigKelulusan
                 .Include(kk => kk.DPekerja)
                 .Include(kk => kk.JKWPTJBahagian)
                    .ThenInclude(kk => kk!.JBahagian)
                .Where(b => b.EnKategoriKelulusan == enKategoriKelulusan 
                && b.DPekerjaId == dPekerjaId
                && b.EnJenisModul == enJenisModul)
                .GroupBy(b => new { b.DPekerjaId, b.JKWPTJBahagianId }).Select(l => new DKonfigKelulusan
                {
                    Id = l.First().DPekerjaId,
                    DPekerjaId = l.First().DPekerjaId,
                    DPekerja = l.First().DPekerja,
                    JKWPTJBahagianId = l.First().JKWPTJBahagianId,
                    JKWPTJBahagian = l.First().JKWPTJBahagian
                }).ToList();

            var konfigKelulusanBahagianList = new List<JKWPTJBahagian>();
            

            if (konfigKelulusanBahagianGrouped != null && konfigKelulusanBahagianGrouped.Count > 0)
            {
                
                foreach(var item in konfigKelulusanBahagianGrouped)
                {
                    if (item.JKWPTJBahagian != null) konfigKelulusanBahagianList.Add(item.JKWPTJBahagian);
                }
                
                var akPPGroup = new List<AkPenilaianPerolehanObjek>().GroupBy(objek => objek.JKWPTJBahagianId);
                if (akPPList != null && akPPList.Count > 0)
                {
                    foreach (var akPP in akPPList)
                    {
                        var penilaianPerolehanObjekBahagianList = new List<JKWPTJBahagian>();

                        // group akPPObjek by bahagian
                        if (akPP.AkPenilaianPerolehanObjek != null && akPP.AkPenilaianPerolehanObjek.Count > 0)
                        {
                            foreach(var item in akPP.AkPenilaianPerolehanObjek)
                            {
                                penilaianPerolehanObjekBahagianList.Add(item.JKWPTJBahagian ?? new JKWPTJBahagian());
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


        public List<AkPenilaianPerolehan> FilterByComparingJumlahAkPenilaianPerolehanWithMinAmountMaxAmountDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModul enJenisModul, List<AkPenilaianPerolehan> filterings)
        {
            //initialize new list akPP
            List<AkPenilaianPerolehan> results = new List<AkPenilaianPerolehan>();

            // get list of dKonfigKelulusan with same DPekerjaId, enKategoriKelulusan, enJenisModul
            var konfigKelulusanList = _context.DKonfigKelulusan.Include(kk => kk.DPekerja)
                 .Include(kk => kk.JKWPTJBahagian)
                    .ThenInclude(kk => kk!.JBahagian)
                .Where(b => b.EnKategoriKelulusan == enKategoriKelulusan
                && b.DPekerjaId == dPekerjaId
                && b.EnJenisModul == enJenisModul).ToList();

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
            bool isSah = await _context.AkPenilaianPerolehan.AnyAsync(t => t.Id == id && t.EnStatusBorang == EnStatusBorang.Sah || t.EnStatusBorang == EnStatusBorang.Semak || t.EnStatusBorang == EnStatusBorang.Lulus);
            if (isSah)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsSemakAsync(int id)
        {
            bool isSemak = await _context.AkPenilaianPerolehan.AnyAsync(t => t.Id == id && t.EnStatusBorang == EnStatusBorang.Semak || t.EnStatusBorang == EnStatusBorang.Lulus);
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

        public  void BatalSah(int id, string? tindakan, string? userId)
        {
            var data = _context.AkPenilaianPerolehan.FirstOrDefault(pp => pp.Id == id);

            if (data != null)
            {
                data.EnStatusBorang = EnStatusBorang.None;
                data.DPengesahId = null;
                data.TarikhSah = null;

                data.Tindakan = tindakan;
                data.UserIdKemaskini = userId ?? "";
                data.TarKemaskini = DateTime.Now;

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

        public void BatalSemak(int id, string? tindakan, string? userId)
        {
            var data = _context.AkPenilaianPerolehan.FirstOrDefault(pp => pp.Id == id);

            if (data != null)
            {
                data.EnStatusBorang = EnStatusBorang.None;
                data.DPengesahId = null;
                data.TarikhSah = null;

                data.DPenyemakId = null;
                data.TarikhSemak = null;

                data.Tindakan = tindakan;
                data.UserIdKemaskini = userId ?? "";
                data.TarKemaskini = DateTime.Now;

                _context.Update(data);
            }
        }

        public void Lulus(int id, int pelulusId, string? userId)
        {
            var data = _context.AkPenilaianPerolehan.FirstOrDefault(pp => pp.Id == id);
            var pelulus = _context.DKonfigKelulusan.FirstOrDefault(kk => kk.DPekerjaId == pelulusId);
            if (data != null)
            {
                data.EnStatusBorang = EnStatusBorang.Lulus;
                data.DPelulusId = pelulus!.Id;
                data.TarikhLulus = DateTime.Now;

                data.FlPosting = 1;
                data.DPekerjaPostingId = pelulusId;
                data.TarikhPosting = DateTime.Now;

                data.UserIdKemaskini = userId ?? "";
                data.TarKemaskini = DateTime.Now;
                data.Tindakan = "";

                _context.Update(data);

            }
        }

        public  void BatalLulus(int id, string? tindakan, string? userId)
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
    }
}
