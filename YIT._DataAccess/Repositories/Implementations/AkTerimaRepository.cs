using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class AkTerimaRepository : _GenericRepository<AkTerima>, IAkTerimaRepository
    {
        private readonly ApplicationDbContext _context;

        public AkTerimaRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }


        public List<AkTerima> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy)
        {
            if (searchString == null && dateFrom == null && dateTo == null && orderBy == null)
            {
                return new List<AkTerima>();
            }

            var akTerima = _context.AkTerima
                .IgnoreQueryFilters()
                .Include(t => t.JKW)
                .Include(t => t.DPekerjaPosting)
                .Include(t => t.JNegeri)
                .Include(t => t.AkBank)
                    .ThenInclude(b => b!.JBank)
                .Include(t => t.AkBank)
                    .ThenInclude(b => b!.JKW)
                .Include(t => t.AkBank)
                    .ThenInclude(b => b!.AkCarta)
                .Include(t => t.DDaftarAwam)
                .Include(t => t.AkTerimaCaraBayar)!
                    .ThenInclude(tcb => tcb.JCaraBayar)
                .Include(t => t.AkTerimaObjek)!
                    .ThenInclude(to => to.AkCarta)
                .Include(t => t.AkTerimaObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JKW)
                .Include(t => t.AkTerimaObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JPTJ)
                .Include(t => t.AkTerimaObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JBahagian)
                .ToList();

            // searchstring filters
            if (searchString != null)
            {
                akTerima = akTerima.Where(t => 
                t.Nama!.Contains(searchString, StringComparison.OrdinalIgnoreCase) 
                || t.NoRujukan!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            // searchString filters end

            // date filters
            if (dateFrom != null && dateTo != null)
            {
                akTerima = akTerima.Where(t => t.Tarikh >= dateFrom && t.Tarikh <= dateTo.Value.AddHours(23.99)).ToList();
            }
            // date filters end

            // order by filters
            if (orderBy != null)
            {
                switch (orderBy)
                {
                    case "Nama":
                        akTerima = akTerima.OrderBy(t => t.Nama).ToList();
                        break;
                    case "Tarikh":
                        akTerima = akTerima.OrderBy(t => t.Tarikh).ToList(); break;
                    default:
                        akTerima = akTerima.OrderBy(t => t.NoRujukan).ToList();
                        break;
                }
                    
            }
            // order by filters end

            return akTerima;
        }

        public AkTerima GetDetailsById(int id)
        {
            return _context.AkTerima.Include(t => t.JKW)
                .Include(t => t.DPekerjaPosting)
                .Include(t => t.JNegeri)
                .Include(t => t.AkBank)
                    .ThenInclude(b => b!.JBank)
                .Include(t => t.AkBank)
                    .ThenInclude(b => b!.JKW)
                .Include(t => t.AkBank)
                    .ThenInclude(b => b!.AkCarta)
                .Include(t => t.DDaftarAwam)
                .Include(t => t.AkTerimaCaraBayar)!
                    .ThenInclude(tcb => tcb.JCaraBayar)
                .Include(t => t.AkTerimaObjek)!
                    .ThenInclude(to => to.AkCarta)
                .Include(t => t.AkTerimaObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JKW)
                .Include(t => t.AkTerimaObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JPTJ)
                .Include(t => t.AkTerimaObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JBahagian)
                    .FirstOrDefault(t => t.Id == id) ?? new AkTerima();
        }

        public async Task<bool> IsPostedAsync(int id, string noRujukan)
        {
            bool isPosted = await _context.AkTerima.AnyAsync(t => t.Id == id && t.FlPosting == 1);
            if (isPosted)
            {
                return true;
            }

            bool isExistInAkAkaun = await _context.AkAkaun.AnyAsync(t => t.NoRujukan == noRujukan);

            if (isExistInAkAkaun)
            {
                return true;
            }

            return false;
        }

        public void PostingToAkAkaun(AkTerima akTerima, string userId, int? dPekerjaMasukId)
        {
            List<AkAkaun> akaunList = new List<AkAkaun>();
            if (akTerima.AkTerimaObjek != null)
            {
                foreach (var t in akTerima.AkTerimaObjek)
                {
                    var akAkaun1 = new AkAkaun();

                    akAkaun1.JKWId = t.JKWPTJBahagian?.JKWId ?? akTerima.JKWId;
                    akAkaun1.JPTJId = t.JKWPTJBahagian!.JPTJId;
                    akAkaun1.JBahagianId = t.JKWPTJBahagian.JBahagianId;
                    akAkaun1.AkCarta1Id = akTerima.AkBank!.AkCartaId;
                    akAkaun1.AkCarta2Id = t.AkCartaId;
                    akAkaun1.NoRujukan = akTerima.NoRujukan;
                    akAkaun1.Tarikh = akTerima.Tarikh;
                    akAkaun1.Debit = t.Amaun;
                    akAkaun1.Kredit = 0;
                    akAkaun1.UserId = userId;
                    akAkaun1.DPekerjaMasukId = dPekerjaMasukId;
                    akAkaun1.TarMasuk = DateTime.Now;

                    akaunList.Add(akAkaun1);

                    var akAkaun2 = new AkAkaun();

                    akAkaun2.JKWId = t.JKWPTJBahagian?.JKWId ?? akTerima.JKWId;
                    akAkaun2.JPTJId = t.JKWPTJBahagian!.JPTJId;
                    akAkaun2.JBahagianId = t.JKWPTJBahagian.JBahagianId;
                    akAkaun2.AkCarta1Id = t.AkCartaId; 
                    akAkaun2.AkCarta2Id = akTerima.AkBank!.AkCartaId;
                    akAkaun2.NoRujukan = akTerima.NoRujukan;
                    akAkaun2.Tarikh = akTerima.Tarikh;
                    akAkaun2.Debit = 0;
                    akAkaun2.Kredit = t.Amaun;
                    akAkaun2.UserId = userId;
                    akAkaun2.DPekerjaMasukId = dPekerjaMasukId;
                    akAkaun2.TarMasuk = DateTime.Now;

                    akaunList.Add(akAkaun2);
                }
                
            }

            _context.AkAkaun.AddRange(akaunList);

            // update akTerima posting fields
            akTerima.DPekerjaPostingId = dPekerjaMasukId;
            akTerima.TarikhPosting = DateTime.Now;
            akTerima.FlPosting = 1;

            _context.AkTerima.Update(akTerima);
        }

        public void RemovePostingFromAkAkaun(AkTerima akTerima, string userId)
        {
            List<AkAkaun> akaunList = _context.AkAkaun.Where(ak => ak.NoRujukan == akTerima.NoRujukan).ToList();

            if (akaunList.Count > 0)
            {
                _context.AkAkaun.RemoveRange(akaunList);
            }

            // update akTerima posting fields
            akTerima.DPekerjaPostingId = null;
            akTerima.TarikhPosting = null;
            akTerima.FlPosting = 0;

            akTerima.UserIdKemaskini = userId;
            akTerima.TarKemaskini = DateTime.Now;

            _context.AkTerima.Update(akTerima);
        }
    }
}