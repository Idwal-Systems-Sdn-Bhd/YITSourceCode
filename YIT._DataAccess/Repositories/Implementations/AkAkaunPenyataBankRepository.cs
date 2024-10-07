using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class AkAkaunPenyataBankRepository : IAkAkaunPenyataBankRepository<AkAkaunPenyataBank>
    {
        private readonly ApplicationDbContext _context;

        public AkAkaunPenyataBankRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<AkAkaunPenyataBank>> GetPadananPenyataListByAkPenyesuaianBankIdAsync(int akPenyesuaianBankId)
        {
            var padananPenyata = await _context.AkAkaunPenyataBank
                .Include(ap => ap.AkPenyesuaianBankPenyataBank)
                .Where(ap => ap.AkPenyesuaianBankPenyataBank!.AkPenyesuaianBankId == akPenyesuaianBankId).ToListAsync();

            return padananPenyata;
        }

        public void Add(AkAkaunPenyataBank akAkaunPenyataBank)
        {
            _context.Add(akAkaunPenyataBank);
        }

        public void Remove(int id)
        {
            var penyata = _context.AkAkaunPenyataBank.Find(id);
            if (penyata != null)
            {
                _context.AkAkaunPenyataBank.Remove(penyata);
            }
            
        }

        public void Update(AkAkaunPenyataBank akAkaunPenyataBank)
        {
            _context.Update(akAkaunPenyataBank);
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public async Task<IEnumerable<AkAkaunPenyataBank>> GetListByIdAsync(int id)
        {
            var padananPenyata = await _context.AkAkaunPenyataBank
                                    .Include(ap => ap.AkPVPenerima)
                                        .ThenInclude(p => p!.AkPV)
                                    .Include(ap => ap.AkAkaun)
                                    .Where(ap => ap.Id == id).ToListAsync();

            return padananPenyata;
        }
    }
}
