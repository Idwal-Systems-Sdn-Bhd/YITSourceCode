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
    public class AkAkaunRepository : IAkAkaunRepository<AkAkaun>
    {
        private readonly ApplicationDbContext _context;

        public AkAkaunRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<AkAkaun>> GetResults(int? KW, int? Carta1Id, DateTime? DateFrom, DateTime? DateTo)
        {
            var akaunList = await _context.AkAkaun
                                    .Include(ak => ak.JKW)
                                    .Include(ak => ak.JPTJ)
                                    .Include(ak => ak.JBahagian)
                                    .Include(ak => ak.AkCarta1)
                                    .Include(ak => ak.AkCarta2)
                                    .Where(ak => ak.AkCarta1Id == 4)
                                    .ToListAsync();

            if (DateFrom != null && DateTo != null)
            {
                akaunList = akaunList.Where(ak =>
                                            ak.Tarikh >= DateFrom
                                            && ak.Tarikh <= DateTo).ToList();  
            }
            if (KW  != null)
            {
                akaunList = akaunList.Where(ak => ak.JKWId == KW).ToList();
            }

            if (Carta1Id != null)
            {
                akaunList = akaunList.Where(ak => ak.AkCarta1Id == Carta1Id).ToList();
            }

            return akaunList;
        }
    }
}
