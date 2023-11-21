using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class JPTJRepository : _GenericRepository<JPTJ>, IJPTJRepository
    {
        private readonly ApplicationDbContext _context;

        public JPTJRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public List<JPTJ> GetAllDetails()
        {
            return _context.JPTJ.Include(ptj => ptj.JKWPTJBahagian).ToList();
        }

        public JPTJ GetAllDetailsById(int id)
        {
            return _context.JPTJ.Include(ptj => ptj.JKWPTJBahagian).Where(ptj => ptj.Id == id).FirstOrDefault() ?? new JPTJ();
        }
    }
}
