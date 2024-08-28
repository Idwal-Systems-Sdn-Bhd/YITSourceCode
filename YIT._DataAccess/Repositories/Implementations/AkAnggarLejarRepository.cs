using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using static System.Runtime.InteropServices.JavaScript.JSType;
using YIT.__Domain.Entities.Models._01Jadual;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class AkAnggarLejarRepository : IAkAnggarLejarRepository<AkAnggarLejar>
    {
        private readonly ApplicationDbContext _context;

        public AkAnggarLejarRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        //public async Task<AkAnggarLejar> GetPreviousBalanceByStartingDate(int? JKWId, int? JPTJId, int? AkCarta1Id, DateTime? startingDate)
        //{
        //    AkAkaun bakiAwalAkAnggarLejar = new AkAnggarLejar();

        //    List<AkAnggarLejar> AkAnggarLejarList = new List<AkAnggarLejar>();

        //    if (JKWId != null && startingDate != null)
        //    {
        //        AkAnggarLejarList = await _context.AkAnggarLejar
        //                            .Where(ak => ak.JKWId == JKWId
        //                            && ak.Tarikh <= startingDate.Value.AddHours(23.99)
        //                            && ak.AkCarta1Id == AkCarta1Id)
        //                            .ToListAsync();

        //        if (JPTJId != null)
        //        {
        //            akaunList = akaunList.Where(ak => ak.JPTJId == JPTJId).ToList();
        //        }

        //        decimal bakiAwal = 0;

        //        foreach (var i in AkAnggarLejarList)
        //        {
        //            bakiAwal += i.Debit - i.Kredit;
        //        };

        //        bakiAwalAkAkaun.Tarikh = startingDate.Value;
        //        bakiAwalAkAkaun.NoRujukan = "Baki Awal";
        //        if (bakiAwal >= 0)
        //        {
        //            bakiAwalAkAkaun.Debit = bakiAwal;
        //        }
        //        else
        //        {
        //            bakiAwalAkAkaun.Kredit = 0 - bakiAwal;
        //        }

        //    }
        //    return bakiAwalAkAkaun;
        //}

        public async Task<IEnumerable<AkAnggarLejar>> GetResults(int? JKWPTJBahagianId, int? AkCartaId, DateTime? DateFrom, DateTime? DateTo)
        {
            List<AkAnggarLejar> AkAnggarLejarList = new List<AkAnggarLejar>();

            if (JKWPTJBahagianId != null && DateFrom != null && DateTo != null)
            {
                AkAnggarLejarList = await _context.AkAnggarLejar
                                    .Include(ak => ak.JKWPTJBahagian)
                                    .Where(ak => ak.JKWPTJBahagianId == JKWPTJBahagianId
                                    && ak.Tarikh >= DateFrom && ak.Tarikh <= DateTo.Value.AddHours(23.99)
                                    && ak.AkCartaId == AkCartaId)
                                    .ToListAsync();

                //if (JPTJId != null)
                //{
                //    AkAnggarLejarList = AkAnggarLejarList.Where(ak => ak.JKWPTJBahagian == JPTJId).ToList();
                //}
            }

            return AkAnggarLejarList;
        }

    }
}
