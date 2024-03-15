using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class JKonfigPenyataRepository : _GenericRepository<JKonfigPenyata>, IJKonfigPenyataRepository
    {
        private readonly ApplicationDbContext _context;

        public JKonfigPenyataRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public JKonfigPenyata GetAllDetailsById(int id)
        {
            var result = _context.JKonfigPenyata.Include(kp => kp.JKonfigPenyataBaris)!.ThenInclude(b => b.JKonfigPenyataBarisFormula)
                .FirstOrDefault(p => p.Id == id);

            return result ?? new JKonfigPenyata();
        }

        public JKonfigPenyata GetAllDetailsByTahunOrKod(string? tahun, string? kod)
        {
            var result = new JKonfigPenyata();
            result = _context.JKonfigPenyata.Include(pe => pe.JKonfigPenyataBaris)!.ThenInclude(b => b.JKonfigPenyataBarisFormula).FirstOrDefault(pe => pe.Tahun == tahun);

            if (!string.IsNullOrEmpty(kod))
            {
                result = _context.JKonfigPenyata.Include(pe => pe.JKonfigPenyataBaris)!.ThenInclude(b => b.JKonfigPenyataBarisFormula).FirstOrDefault(pe => pe.Tahun == tahun && pe.Kod == kod);
            }

            if (result != null && result.JKonfigPenyataBaris != null && result.JKonfigPenyataBaris.Count > 0)
            {
                result.JKonfigPenyataBaris = result.JKonfigPenyataBaris.OrderBy(b => b.EnKategoriTajuk).ThenBy(b => b.Susunan).ToList();
            }

            return result ?? new JKonfigPenyata();
        }
    }

    
}
