using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    internal class AkInvoisRepository : _GenericRepository<AkInvois>, IAkInvoisRepository
    {
        private readonly ApplicationDbContext _context;

        public AkInvoisRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public AkInvois GetDetailsById(int id)
        {
            return _context.AkInvois
                .IgnoreQueryFilters()
                .Include(t => t.JKW)
                .Include(t => t.DDaftarAwam)
                .Include(t => t.DPekerjaPosting)
                .Include(t => t.AkAkaunAkru)
                .Include(t => t.DPengesah)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPenyemak)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.DPelulus)
                    .ThenInclude(t => t!.DPekerja)
                .Include(t => t.AkInvoisObjek)!
                    .ThenInclude(to => to.AkCarta)
                .Include(t => t.AkInvoisObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JKW)
                .Include(t => t.AkInvoisObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JPTJ)
                .Include(t => t.AkInvoisObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JBahagian)
                .Include(t => t.AkInvoisPerihal)
                .FirstOrDefault(pp => pp.Id == id) ?? new AkInvois();
        }
    }
}