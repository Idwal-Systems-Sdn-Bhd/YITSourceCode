using YIT.__Domain.Entities.Models._01Jadual;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class JKWRepository : _GenericRepository<JKW>, IJKWRepository
    {
        private readonly ApplicationDbContext _context;

        public JKWRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public List<JKW> GetAllDetails()
        {
            return _context.JKW.Include(ptj => ptj.AbWaran).ToList();
        }
    }
}
