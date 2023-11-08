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
            return _context.JBahagian.Include(b => b.JPTJ).ThenInclude(ptj => ptj!.JKW).ToList();
        }

        public JBahagian GetAllDetailsById(int id)
        {
            return _context.JBahagian.Include(b => b.JPTJ).ThenInclude(ptj => ptj!.JKW).Where(ptj => ptj.Id == id).FirstOrDefault() ?? new JBahagian();
        }
    }
}