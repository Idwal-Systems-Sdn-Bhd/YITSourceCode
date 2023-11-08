using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class AkCartaRepository : _GenericRepository<AkCarta>, IAkCartaRepository
    {
        private readonly ApplicationDbContext _context;

        public AkCartaRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public List<AkCarta> GetResultsByJenis(EnJenisCarta jenis, EnParas paras)
        {
            return _context.AkCarta.Where(c => c.EnJenis == jenis && c.EnParas == paras).ToList();
        }

        public List<AkCarta> GetResultsByParas(EnParas paras)
        {
            return _context.AkCarta.Where(c => c.EnParas == paras).ToList();
        }
    }
}
