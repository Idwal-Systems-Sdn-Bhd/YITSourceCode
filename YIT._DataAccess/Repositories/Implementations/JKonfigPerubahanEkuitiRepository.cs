using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class JKonfigPerubahanEkuitiRepository : _GenericRepository<JKonfigPerubahanEkuiti>, IJKonfigPerubahanEkuitiRepository
    {
        private readonly ApplicationDbContext _context;

        public JKonfigPerubahanEkuitiRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public List<JKonfigPerubahanEkuiti> GetAllDetails()
        {
            return _context.JKonfigPerubahanEkuiti.Include(pe => pe.JKW).ToList();
        }

        public JKonfigPerubahanEkuiti GetAllDetailsById(int id)
        {
            return _context.JKonfigPerubahanEkuiti.Include(pe => pe.JKW).FirstOrDefault(pe => pe.Id == id) ?? new JKonfigPerubahanEkuiti();
        }

        public JKonfigPerubahanEkuiti GetAllDetailsByTahunAndJKW(string? tahun, int? JKWId)
        {
            return _context.JKonfigPerubahanEkuiti.Include(pe => pe.JKW).FirstOrDefault(pe => pe.Tahun == tahun && pe.JKWId == JKWId) ?? new JKonfigPerubahanEkuiti();
        }
    }
}
