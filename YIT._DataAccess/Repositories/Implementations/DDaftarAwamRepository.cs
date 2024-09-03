using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Bases;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class DDaftarAwamRepository : _GenericRepository<DDaftarAwam>, IDDaftarAwamRepository
    {
        private readonly ApplicationDbContext _context;

        public DDaftarAwamRepository(ApplicationDbContext context) :base(context)
        {
            _context = context;
        }

        public List<DDaftarAwam> GetAllDetails()
        {
            return _context.DDaftarAwam
                .Include(df => df.JBank)
                .Include(df => df.JNegeri)
                .ToList();
        }

        public List<SelectItemList> GetAllDetailsGroupByKod()
        {
            var daftarAwam = _context.DDaftarAwam
                .Include(df => df.JBank)
                .Include(df => df.JNegeri)
                .ToList();

            var result = daftarAwam.GroupBy(df => df.Kod)
                .Select(l => new SelectItemList
                {
                    Value = l.First().Kod,
                    TextValue = l.First().Nama
                }).OrderBy(df => df.TextValue).ToList();

            return result;
        }

        public IEnumerable<DDaftarAwam> GetAllDetailsByKategori(EnKategoriDaftarAwam kategoriDaftarAwam)
        {
            var result = _context.DDaftarAwam
                .Where(df => df.EnKategoriDaftarAwam == kategoriDaftarAwam).AsQueryable();

            if (result.Any())
            {
                foreach (var item in result)
                {
                    item.JNegeri = _context.JNegeri.Find(item.JNegeri);
                    item.JBank = _context.JBank.Find(item.JBank);
                }
            }
            return result;
        }

        public DDaftarAwam GetAllDetailsById(int id)
        {
            return _context.DDaftarAwam
                .Include(df => df.JBank)
                .Include(df => df.JNegeri)
                .FirstOrDefault(df => df.Id == id) ?? new DDaftarAwam();
        }

        public string GetMaxRefNo(string initial)
        {
            var max = _context.DDaftarAwam.Where(df => df.Kod!.Substring(0,1) == initial.Substring(0,1)).OrderByDescending(df => df.Kod).ToList();

            if (max != null)
            {

                var refNo = max.FirstOrDefault()?.Kod?.Substring(1,4);
                return refNo ?? "";
            }
            else
            {
                return "";
            }
            
        }
        public List<DDaftarAwam> GetResults(string? searchString, string? orderBy)
        {
            if (searchString == null && orderBy == null)
            {
                return new List<DDaftarAwam>();
            }

            var dDAList = _context.DDaftarAwam
                .IgnoreQueryFilters()
                .Include(df => df.JBank)
                .Include(df => df.JNegeri)
                .ToList();

            // searchstring filters
            if (searchString != null)
            {
                dDAList = dDAList.Where(t =>
                t.Kod!.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                t.Nama!.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                t.JBank!.Perihal!.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                t.Alamat1!.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                t.NoAkaunBank!.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                t.KodM2E!.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                t.EnKategoriDaftarAwam!.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)
                )
                .ToList();

                //                dDAList = dDAList.Where(t =>
                //    t?.Kod?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true ||
                //    t?.Nama?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true ||
                //    t?.JBank?.Perihal?.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase) == true ||
                //    t?.Alamat1?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true ||
                //    t?.NoAkaunBank?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true ||
                //    t?.KodM2E?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true ||
                //    t?.EnKategoriDaftarAwam.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase) == true
                //)
                //.ToList();


            }
            // searchString filters end

            // order by filters
            if (orderBy != null)
            {
                switch (orderBy)
                {
                    default:
                        dDAList = dDAList.OrderBy(t => t.Kod).ToList();
                        break;
                }

            }
            // order by filters end

            return dDAList;
        }

        public async Task<IEnumerable<DDaftarAwam>> GetResultsAsync(string? searchString, string? orderBy)
        {
            //if (searchString == null && orderBy == null)
            //{
            //    return new List<DDaftarAwam>();
            //}

            var dDAList = await _context.DDaftarAwam
                .IgnoreQueryFilters()
                .Include(df => df.JBank)
                .Include(df => df.JNegeri)
                .ToListAsync();

            // searchstring filters
            if (searchString != null)
            {
                dDAList = dDAList.Where(t =>
                t.Kod!.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                t.Nama!.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                t.JBank!.Perihal!.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                t.Alamat1!.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                t.NoAkaunBank!.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                t.KodM2E!.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                t.EnKategoriDaftarAwam!.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)
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
                        dDAList = dDAList.OrderBy(t => t.Kod).ToList();
                        break;
                }

            }
            // order by filters end

            return dDAList;
        }
    }
}