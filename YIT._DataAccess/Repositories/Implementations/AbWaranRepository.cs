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
                .ThenInclude(a => a.JBahagian)
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
                .ThenInclude(a => a.JBahagian)
                .Include(a => a.JKW)
                .Where(a => a.Id == id).FirstOrDefault() ?? new AbWaran();
        }

    }
}
