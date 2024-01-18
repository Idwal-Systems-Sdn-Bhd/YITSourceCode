using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class JKWPTJBahagianRepository : _GenericRepository<JKWPTJBahagian>, IJKWPTJBahagianRepository
    {
        private readonly ApplicationDbContext _context;

        public JKWPTJBahagianRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public List<JKWPTJBahagian> GetAllDetails()
        {
            return _context.JKWPTJBahagian
                .Include(b => b.JKW)
                .Include(b => b.JPTJ)
                .Include(b => b.JBahagian)
                .ToList();
        }

        public JKWPTJBahagian GetAllDetailsById(int id)
        {
            return _context.JKWPTJBahagian
                .Include(b => b.JKW)
                .Include(b => b.JPTJ)
                .Include(b => b.JBahagian)
                .FirstOrDefault(b => b.Id == id) ?? new JKWPTJBahagian();
        }

        public List<JKWPTJBahagian> GetAllDetailsByJKWId(int JKWId)
        {
            return _context.JKWPTJBahagian
                .Include(b => b.JKW)
                .Include(b => b.JPTJ)
                .Include(b => b.JBahagian).Where(b => b.JKWId == JKWId)
                .ToList();
        }

        public List<JKWPTJBahagian> GetAllDetailsByJPTJId(int JPTJId)
        {
            return _context.JKWPTJBahagian
                .Include(b => b.JKW)
                .Include(b => b.JPTJ)
                .Include(b => b.JBahagian).Where(b => b.JPTJId == JPTJId)
                .ToList();
        }
    }
}