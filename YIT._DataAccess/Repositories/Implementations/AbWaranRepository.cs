using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class AbWaranRepository : _GenericRepository<AbWaran>, IAbWaranRepository
    {
        private readonly ApplicationDbContext _context;

        public AbWaranRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public List<AbWaran> GetAllDetails()
        {
            return _context.AbWaran
                .Include(a => a.DPengesah)
                .Include(a => a.DPenyemak)
                .Include(a => a.DPelulus)
                .Include(a => a.AbWaranObjek)!
                .ThenInclude(to => to.AkCarta)
                .Include(a => a.AbWaranObjek)!
                    .ThenInclude(a => a.JKWPTJBahagian)
                        .ThenInclude(a => a!.JKW)
                .Include(a => a.AbWaranObjek)!
                    .ThenInclude(a => a.JKWPTJBahagian)
                        .ThenInclude(a => a!.JPTJ)
                .Include(a => a.AbWaranObjek)!
                    .ThenInclude(a => a.JKWPTJBahagian)
                        .ThenInclude(a => a!.JBahagian)
                .Include(a => a.JKW)
                .ToList();
        }

        public AbWaran GetAllDetailsById(int id)
        {

            return _context.AbWaran
                .Include(a => a.DPengesah)
                .Include(a => a.DPenyemak)
                .Include(a => a.DPelulus)
                .Include(a => a.AbWaranObjek)!
                .ThenInclude(to => to.AkCarta)
                .Include(a => a.AbWaranObjek)!
                    .ThenInclude(a => a.JKWPTJBahagian)
                        .ThenInclude(a => a!.JKW)
                .Include(a => a.AbWaranObjek)!
                    .ThenInclude(a => a.JKWPTJBahagian)
                        .ThenInclude(a => a!.JPTJ)
                .Include(a => a.AbWaranObjek)!
                    .ThenInclude(a => a.JKWPTJBahagian)
                        .ThenInclude(a => a!.JBahagian)
                .Include(a => a.JKW)
                .Where(a => a.Id == id).FirstOrDefault() ?? new AbWaran();
        }

        public AbWaran GetDetailsById(int id)
        {
            return _context.AbWaran
                .IgnoreQueryFilters()
                .Include(t => t.JKW)
                .Include(t => t.DPekerjaPosting)
                .Include(t => t.DPengesah)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPenyemak)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPelulus)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.AbWaranObjek)!
                    .ThenInclude(to => to.AkCarta)
                .Include(t => t.AbWaranObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JKW)
                .Include(t => t.AbWaranObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JPTJ)
                .Include(t => t.AbWaranObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JBahagian)
                .FirstOrDefault(pp => pp.Id == id) ?? new AbWaran();
        }

        public List<AbWaran> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang)
        {
            if (searchString == null && dateFrom == null && dateTo == null && orderBy == null)
            {
                return new List<AbWaran>();
            }

            var abWaranList = _context.AbWaran
                .IgnoreQueryFilters()
                .Include(t => t.JKW)
                .Include(t => t.DPekerjaPosting)
                .Include(t => t.DPengesah)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPenyemak)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPelulus)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.AbWaranObjek)!
                    .ThenInclude(to => to.AkCarta)
                .Include(t => t.AbWaranObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JKW)
                .Include(t => t.AbWaranObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JPTJ)
                .Include(t => t.AbWaranObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JBahagian)
                .ToList();

            // searchstring filters
            if (searchString != null)
            {
                abWaranList = abWaranList.Where(t =>
                t.NoRujukan!.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                || t.DPengesah!.DPekerja!.Nama!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            // searchString filters end

            // date filters
            if (dateFrom != null && dateTo != null)
            {
                abWaranList = abWaranList.Where(t => t.Tarikh >= dateFrom && t.Tarikh <= dateTo.Value.AddHours(23.99)).ToList();
            }
            // date filters end

            // status borang filters
            switch (enStatusBorang)
            {
                case EnStatusBorang.None:
                    abWaranList = abWaranList.Where(pp => pp.EnStatusBorang == EnStatusBorang.None).ToList();
                    break;
                case EnStatusBorang.Sah:
                    abWaranList = abWaranList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Sah).ToList();
                    break;
                case EnStatusBorang.Semak:
                    abWaranList = abWaranList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Semak).ToList();
                    break;
                case EnStatusBorang.Lulus:
                    abWaranList = abWaranList.Where(pp => pp.EnStatusBorang == EnStatusBorang.Lulus).ToList();
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
                        abWaranList = abWaranList.OrderBy(t => t.DPengesah!.DPekerja!.Nama).ToList();
                        break;
                    case "Tarikh":
                        abWaranList = abWaranList.OrderBy(t => t.Tarikh).ToList(); break;
                    default:
                        abWaranList = abWaranList.OrderBy(t => t.NoRujukan).ToList();
                        break;
                }

            }
            // order by filters end

            return abWaranList;
        }

        public List<AbWaran> GetResultsByDPekerjaIdFromDKonfigKelulusan(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang, int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModul)
        {

            // get all data with details
            List<AbWaran> abWaranList = GetResults(searchString, dateFrom, dateTo, orderBy, enStatusBorang);

            var filterings = FilterByComparingJBahagianAbWaranObjekWithJBahagianDKonfigKelulusan(dPekerjaId, enKategoriKelulusan, enJenisModul, abWaranList);

            //var results = FilterByComparingJumlahAkPenilaianPerolehanWithMinAmountMaxAmountDKonfigKelulusan(dPekerjaId, enKategoriKelulusan, enJenisModul, filterings);

            return filterings;
        }

        public List<AbWaran> FilterByComparingJBahagianAbWaranObjekWithJBahagianDKonfigKelulusan(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModul, List<AbWaran> abWaranList)
        {
            // initialize result sets
            List<AbWaran> results = new List<AbWaran>();

            //get all pengesah/penyemak/pelulus with same dpekerjaId, group by pekerjaId and bahagianId

            var konfigKelulusanBahagianGrouped = _context.DKonfigKelulusan
                 .Include(kk => kk.DPekerja)
                 .Include(kk => kk.JBahagian)
                .Where(b => b.EnKategoriKelulusan == enKategoriKelulusan
                && b.DPekerjaId == dPekerjaId
                && b.EnJenisModul == enJenisModul)
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

                var abWaranGroup = new List<AbWaranObjek>().GroupBy(objek => objek.JKWPTJBahagianId);
                if (abWaranList != null && abWaranList.Count > 0)
                {
                    foreach (var abWaran in abWaranList)
                    {
                        var waranObjekBahagianList = new List<JBahagian>();

                        // group akPPObjek by bahagian
                        if (abWaran.AbWaranObjek != null && abWaran.AbWaranObjek.Count > 0)
                        {
                            foreach (var item in abWaran.AbWaranObjek)
                            {
                                waranObjekBahagianList.Add(item.JKWPTJBahagian?.JBahagian ?? new JBahagian());
                            }

                        }
                        // if konfigKelulusan bahagian null, bypass all, add to results
                        if (konfigKelulusanBahagianList.Count == 0)
                        {
                            results.Add(abWaran);
                            continue;
                        }

                        // compare each lists, if its equal then insert to results
                        var items = waranObjekBahagianList.All(konfigKelulusanBahagianList.Contains);
                        if (konfigKelulusanBahagianList.OrderBy(kk => kk.Kod).SequenceEqual(waranObjekBahagianList.OrderBy(pp => pp.Kod))
                            || waranObjekBahagianList.All(konfigKelulusanBahagianList.Contains))
                        {

                            results.Add(abWaran);
                            continue;
                        };
                    }
                }
            }


            return results;
        }

        public async Task<bool> IsSahAsync(int id)
        {
            bool isSah = await _context.AbWaran.AnyAsync(t => t.Id == id && t.EnStatusBorang == EnStatusBorang.Sah || t.EnStatusBorang == EnStatusBorang.Semak || t.EnStatusBorang == EnStatusBorang.Lulus);
            if (isSah)
            {
                return true;
            }

            return false;
        }

        public void Sah(int id, int? pengesahId, string? userId)
        {
            var data = _context.AbWaran.FirstOrDefault(pp => pp.Id == id);
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

        public void BatalSah(int id, string? tindakan, string? userId)
        {
            var data = _context.AbWaran.FirstOrDefault(pp => pp.Id == id);

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

        //public void Semak(int id, int penyemakId, string? userId)
        //{
        //    var data = _context.AbWaran.FirstOrDefault(pp => pp.Id == id);
        //    var penyemak = _context.DKonfigKelulusan.FirstOrDefault(kk => kk.DPekerjaId == penyemakId);
        //    if (data != null)
        //    {
        //        data.EnStatusBorang = EnStatusBorang.Semak;
        //        data.DPenyemakId = penyemak!.Id;
        //        data.TarikhSemak = DateTime.Now;

        //        data.UserIdKemaskini = userId ?? "";
        //        data.TarKemaskini = DateTime.Now;
        //        data.Tindakan = "";

        //        _context.Update(data);

        //    }
        //}

        //public void BatalSemak(int id, string? tindakan, string? userId)
        //{
        //    var data = _context.AbWaran.FirstOrDefault(pp => pp.Id == id);

        //    if (data != null)
        //    {
        //        data.EnStatusBorang = EnStatusBorang.None;
        //        data.DPengesahId = null;
        //        data.TarikhSah = null;

        //        data.DPenyemakId = null;
        //        data.TarikhSemak = null;

        //        data.Tindakan = tindakan;
        //        data.UserIdKemaskini = userId ?? "";
        //        data.TarKemaskini = DateTime.Now;

        //        _context.Update(data);
        //    }
        //}

    }
}
