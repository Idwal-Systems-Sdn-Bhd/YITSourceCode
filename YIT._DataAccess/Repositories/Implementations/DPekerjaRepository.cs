using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class DPekerjaRepository : _GenericRepository<DPekerja>, IDPekerjaRepository
    {
        private readonly ApplicationDbContext _context;

        public DPekerjaRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public List<DPekerja> GetAllDetails()
        {
            return _context.DPekerja
                .Include(p => p.JBank)
                .Include(p => p.JKWPTJBahagian)
                    .ThenInclude(p => p!.JBahagian)
                .Include(p => p.JNegeri)
                .Include(p => p.JBangsa)
                .ToList();
        }

        public DPekerja GetAllDetailsById(int id)
        {
            return _context.DPekerja
                .Include(p => p.JBank)
                .Include(p => p.JKWPTJBahagian)
                    .ThenInclude(p => p!.JBahagian)
                .Include(p => p.JNegeri)
                .Include(p => p.JBangsa)
                .FirstOrDefault(p => p.Id == id) ?? new DPekerja();
        }

        public string GetMaxRefNo()
        {
            return _context.DPekerja.Max(p => p.NoGaji) ?? "0";
        }
    }
}
