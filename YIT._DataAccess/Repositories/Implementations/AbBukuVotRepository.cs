using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities.Models._01Jadual;
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
    public class AbBukuVotRepository : IAbBukuVotRepository<AbBukuVot>
    {
        private readonly ApplicationDbContext _context;

        public AbBukuVotRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public List<AbBukuVot> GetResults(string? Tahun, string? Carta1Id, string? Carta2Id)
        {
            //Ringkasan Debit group by kod Bank AkTerima
            var sql = (from tbl in _context.AbBukuVot.Include(x => x.Vot)
                       .Include(x => x.JKW)
                       .Include(x => x.JPTJ)
                       .Include(x => x.JBahagian)
                       .Where(x => x.Tahun == Tahun)
                       .ToList()
                       select new
                       {
                           Id = tbl.VotId,
                           Tahun = tbl.Tahun,
                           JKWId = tbl.JKWId,
                           JKW = tbl.JKW,
                           JPTJId = tbl.JPTJId,
                           JPTJ = tbl.JPTJ,
                           JBahagianId = tbl.JBahagianId,
                           JBahagian = tbl.JBahagian,
                           VotId = tbl.VotId,
                           Vot = tbl.Vot,
                           Debit = tbl.Debit,
                           Kredit = tbl.Kredit,
                           Tanggungan = tbl.Tanggungan,
                           Liabiliti = tbl.Liabiliti,
                           Belanja = tbl.Belanja,
                           Baki = tbl.Baki

                       }).GroupBy(x => new { x.Tahun, x.VotId, x.JBahagianId }).ToList();

            List<AbBukuVot> vot = sql.Select(l => new AbBukuVot
            {
                Id = l.First().Id,
                Tahun = l.Select(x => x.Tahun).FirstOrDefault(),
                JKW = l.Select(x => x.JKW).FirstOrDefault(),
                JKWId = l.Select(x => x.JKWId).FirstOrDefault(),
                JPTJ = l.Select(x => x.JPTJ).FirstOrDefault(),
                JPTJId = l.Select(x => x.JPTJId).FirstOrDefault(),
                JBahagian = l.Select(x => x.JBahagian).FirstOrDefault(),
                JBahagianId = l.Select(x => x.JBahagianId).FirstOrDefault(),
                VotId = l.Select(x => x.VotId).FirstOrDefault(),
                Vot = l.Select(x => x.Vot).FirstOrDefault(),
                Debit = l.Sum(c => c.Debit),
                Kredit = l.Sum(c => c.Kredit),
                Tanggungan = l.Sum(c => c.Tanggungan),
                Liabiliti = l.Sum(c => c.Liabiliti),
                Belanja = l.Sum(c => c.Belanja),
                Baki = l.Sum(c => c.Baki)
            }).OrderBy(b => b.Vot!.Kod).ToList();

            if (!string.IsNullOrEmpty(Carta1Id) && !string.IsNullOrEmpty(Carta2Id))
            {
                Tuple<string, string> range = Tuple.Create(Carta1Id, Carta2Id);

                vot = vot.Where(s =>
                        range.Item1.CompareTo(s.Vot?.Kod?.Substring(0, range.Item1.Length)) <= 0 &&
                        s.Vot?.Kod?.Substring(0, range.Item2.Length).CompareTo(range.Item2) <= 0)
                        .OrderBy(x => x.Vot!.Kod).ToList();
            }

            return vot;
            

        }

        public async Task<IEnumerable<AbBukuVot>> GetResultsByDateRangeAsync(int? AkCartaId, string? Tahun, int? JKWId, int? JPTJId, int? JBahagianId, string? TarikhDari, string? TarikhHingga)
        {
            var abBukuVot = await _context.AbBukuVot
                .Include(x => x.Vot)
                .Include(x => x.JKW)
            .Include(x => x.JBahagian)
            .Where(x => x.Tahun == Tahun
                && x.JKWId == JKWId
                && x.JPTJId == JPTJId
                && x.JBahagianId == JBahagianId
                && x.VotId == AkCartaId
                )
                .ToListAsync();

            if (TarikhDari != null && TarikhHingga != null)
            {
                DateTime date1 = DateTime.Parse(TarikhDari);
                DateTime date2 = DateTime.Parse(TarikhHingga).AddHours(23.99);

                abBukuVot = abBukuVot.Where(bv => bv.Tarikh >= date1 && bv.Tarikh <= date2).ToList();
            }

            return abBukuVot.OrderBy(bv => bv.Tarikh).ToList();
        }

        public async Task<bool> IsBudgetExistAsync(string? tahun, int jBahagianId, int akCartaId)
        {
            return await _context.AbBukuVot.AnyAsync(pp => pp.Tahun == tahun && pp.JBahagianId == jBahagianId && pp.VotId == akCartaId);
        }

        public async Task<bool> IsInBudgetAsync(string? tahun, int jBahagianId, int akCartaId, decimal amaun)
        {
            // check if enough budget in abBukuVot
            var sql = (from tbl in await _context.AbBukuVot
                       .Include(x => x.Vot)
            .Include(x => x.JBahagian)
                       .Where(x => x.Tahun == tahun && x.VotId == akCartaId && x.JBahagianId == jBahagianId)
                       .ToListAsync()
                       select new
                       {
                           Id = tbl.VotId,
                           Tahun = tbl.Tahun,
                           Bahagian = tbl.JBahagian?.Kod,
                           KodAkaun = tbl.Vot?.Kod,
                           Debit = tbl.Debit,
                           Kredit = tbl.Kredit,
                           Tanggungan = tbl.Tanggungan,
                           Liabiliti = tbl.Liabiliti,
                           Baki = tbl.Baki
                       }).GroupBy(x => new { x.Tahun, x.KodAkaun, x.Bahagian }).FirstOrDefault();

            return amaun < sql?.Select(t => t.Baki + t.Kredit - t.Debit - t.Tanggungan).Sum();
        }

    }
}
