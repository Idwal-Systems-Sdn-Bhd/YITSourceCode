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

        public List<DDaftarAwam> GetAllDetailsByKategori(EnKategoriDaftarAwam kategoriDaftarAwam)
        {
            return _context.DDaftarAwam
                .Include(df => df.JBank)
                .Include(df => df.JNegeri)
                .Where(df => df.EnKategoriDaftarAwam == kategoriDaftarAwam)
                .ToList();
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

                var refNo = max.FirstOrDefault()?.Kod?.Substring(1,5);
                return refNo ?? "";
            }
            else
            {
                return "";
            }
            
        }
    }
}