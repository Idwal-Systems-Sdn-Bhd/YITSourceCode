using Microsoft.EntityFrameworkCore;
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
    public class AkBankRepository : _GenericRepository<AkBank>, IAkBankRepository
    {
        private readonly ApplicationDbContext _context;

        public AkBankRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public List<AkBank> GetAllDetails()
        {
            return _context.AkBank.Include(b => b.JBank).Include(b => b.AkCarta).Include(b => b.JKW).ToList();
        }

        public AkBank GetAllDetailsById(int id)
        {
            return _context.AkBank.Include(b => b.JBank).Include(b => b.AkCarta).Include(b => b.JKW).FirstOrDefault(b => b.Id == id) ?? new AkBank();
        }
    }
}
