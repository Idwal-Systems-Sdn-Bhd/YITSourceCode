using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class JBahagianRepository : _GenericRepository<JBahagian>, IJBahagianRepository
    {
        private readonly ApplicationDbContext _context;

        public JBahagianRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public List<JBahagian> GetAllDetails()
        {
            return _context.JBahagian.Include(b => b.JKWPTJBahagian).ToList();
        }

        public JBahagian GetAllDetailsById(int id)
        {
            return _context.JBahagian.Include(b => b.JKWPTJBahagian).Where(b => b.Id == id).FirstOrDefault() ?? new JBahagian();
        }
    }
}