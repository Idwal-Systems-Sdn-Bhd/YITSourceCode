using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class DPekerjaRepository : _GenericRepository<DPekerja>, IDPekerjaRepository
    {
        private readonly ApplicationDbContext _context;

        public DPekerjaRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public IEnumerable<DPekerja> GetAllByStatus(string? statusKerja)
        {

            var result = from pekerja in _context.DPekerja.AsQueryable() select pekerja;

            if (statusKerja != null && statusKerja == "Aktif" )
            {
                result = result.Where(p => p.TarikhBerhentiKerja >= DateTime.Now || p.TarikhBerhentiKerja == null);
            }

            return result.OrderBy(p => p.KodPekerja);
        }

        public List<DPekerja> GetAllDetails()
        {
            return _context.DPekerja
                .Include(p => p.JBank)
                .Include(p => p.JPTJ)
                .Include(p => p.JBahagian)
                .Include(p => p.JNegeri)
                .Include(p => p.JBangsa)
                .Include(p => p.JCawangan)
                .ToList();
        }

        public DPekerja GetAllDetailsById(int id)
        {
            return _context.DPekerja
                .Include(p => p.JBank)
                .Include(p => p.JBahagian)
                .Include(p => p.JPTJ)
                .Include(p => p.JNegeri)
                .Include(p => p.JBangsa)
                .Include(p => p.JCawangan)
                .FirstOrDefault(p => p.Id == id) ?? new DPekerja();
        }
        public string GetMaxRefNo()
        {
            return _context.DPekerja.Max(p => p.NoGaji) ?? "0";
        }
        public List<DPekerja> GetResults(string? searchString, string? orderBy)
        {
            if (searchString == null && orderBy == null)
            {
                return new List<DPekerja>();
            }

            var dPList = _context.DPekerja
                .IgnoreQueryFilters()
                .Include(p => p.JBank)
                .Include(p => p.JBahagian)
                .Include(p => p.JPTJ)
                .Include(p => p.JNegeri)
                .Include(p => p.JBangsa)
                .Include(p => p.JCawangan)
                .ToList();

            // searchstring filters
            if (searchString != null)
            {
                dPList = dPList.Where(t =>
                t.NoGaji!.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                t.NoKp!.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                t.Nama!.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                t.Alamat1!.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                t.JNegeri!.Perihal!.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                t.JBank!.Perihal!.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                )
                .ToList();
            }
            // searchString filters end

            // order by filters
            if (orderBy != null)
            {
                switch (orderBy)
                {
                    default:
                        dPList = dPList.OrderBy(t => t.NoGaji).ToList();
                        break;
                }

            }
            // order by filters end

            return dPList;
        }
    }
}
