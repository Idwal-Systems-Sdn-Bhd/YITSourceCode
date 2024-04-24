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
using System.Diagnostics;

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
                           Belanja = tbl.Debit,
                           Tbs = tbl.Tbs,
                           JumLiabiliti = tbl.Liabiliti,
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
                Tbs = l.Sum(c => c.Tbs),
                JumLiabiliti = l.Sum(c => c.Liabiliti),
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

            // add all PO, Belian, NotaDebitKreditDiterima, PV (not posting)


            List<AbBukuVot> votNotPosted = new List<AbBukuVot>();
            // add PO

            List<AkPOObjek> POObjekList = _context.AkPOObjek
                .Include(obj => obj.AkPO)
                .Include(obj => obj.AkCarta)
                .Include(obj => obj.JKWPTJBahagian)
                    .ThenInclude(obj => obj!.JKW)
                .Include(obj => obj.JKWPTJBahagian)
                    .ThenInclude(obj => obj!.JPTJ)
                .Include(obj => obj.JKWPTJBahagian)
                    .ThenInclude(obj => obj!.JBahagian)
                .Where(po => po.AkPO!.FlPosting == 0
                    && po.AkPO!.FlBatal != 1
                    && po.AkPO!.Tahun == Tahun
                    ).ToList();

            if (!string.IsNullOrEmpty(Carta1Id) && !string.IsNullOrEmpty(Carta2Id))
            {
                Tuple<string, string> range = Tuple.Create(Carta1Id, Carta2Id);

                POObjekList = POObjekList.Where(s =>
                        range.Item1.CompareTo(s.AkCarta?.Kod?.Substring(0, range.Item1.Length)) <= 0 &&
                        s.AkCarta?.Kod?.Substring(0, range.Item2.Length).CompareTo(range.Item2) <= 0)
                        .OrderBy(x => x.AkCarta!.Kod).ToList();
            }

            if (POObjekList != null && POObjekList.Count > 0)
            {
                foreach (var item in POObjekList)
                {
                    votNotPosted.Add(new AbBukuVot
                    {
                        Tahun = item.AkPO?.Tahun,
                        JKW = item.JKWPTJBahagian?.JKW,
                        JKWId = (int)item.JKWPTJBahagian?.JKWId!,
                        JPTJ = item.JKWPTJBahagian?.JPTJ,
                        JPTJId = (int)item.JKWPTJBahagian?.JPTJId!,
                        JBahagian = item.JKWPTJBahagian?.JBahagian,
                        JBahagianId = (int)item.JKWPTJBahagian?.JBahagianId!,
                        VotId = item.AkCartaId,
                        Vot = item.AkCarta,
                        Debit = 0,
                        Kredit = 0,
                        Tanggungan = item.Amaun,
                        Liabiliti = 0,
                        Belanja = 0,
                        Tbs = item.Amaun,
                        JumLiabiliti = 0,
                        Baki = -item.Amaun
                    });
                }
            }

            // add PO end

            // add Inden
            List<AkIndenObjek> IndenObjekList = _context.AkIndenObjek
                .Include(obj => obj.AkInden)
                .Include(obj => obj.AkCarta)
                .Include(obj => obj.JKWPTJBahagian)
                    .ThenInclude(obj => obj!.JKW)
                .Include(obj => obj.JKWPTJBahagian)
                    .ThenInclude(obj => obj!.JPTJ)
                .Include(obj => obj.JKWPTJBahagian)
                    .ThenInclude(obj => obj!.JBahagian)
                .Where(po => po.AkInden!.FlPosting == 0
                    && po.AkInden!.FlBatal != 1
                    && po.AkInden!.Tahun == Tahun
                    ).ToList();

            if (!string.IsNullOrEmpty(Carta1Id) && !string.IsNullOrEmpty(Carta2Id))
            {
                Tuple<string, string> range = Tuple.Create(Carta1Id, Carta2Id);

                IndenObjekList = IndenObjekList.Where(s =>
                        range.Item1.CompareTo(s.AkCarta?.Kod?.Substring(0, range.Item1.Length)) <= 0 &&
                        s.AkCarta?.Kod?.Substring(0, range.Item2.Length).CompareTo(range.Item2) <= 0)
                        .OrderBy(x => x.AkCarta!.Kod).ToList();
            }

            if (IndenObjekList != null && IndenObjekList.Count > 0)
            {
                foreach (var item in IndenObjekList)
                {
                    votNotPosted.Add(new AbBukuVot
                    {
                        Tahun = item.AkInden?.Tahun,
                        JKW = item.JKWPTJBahagian?.JKW,
                        JKWId = (int)item.JKWPTJBahagian?.JKWId!,
                        JPTJ = item.JKWPTJBahagian?.JPTJ,
                        JPTJId = (int)item.JKWPTJBahagian?.JPTJId!,
                        JBahagian = item.JKWPTJBahagian?.JBahagian,
                        JBahagianId = (int)item.JKWPTJBahagian?.JBahagianId!,
                        VotId = item.AkCartaId,
                        Vot = item.AkCarta,
                        Debit = 0,
                        Kredit = 0,
                        Tanggungan = item.Amaun,
                        Liabiliti = 0,
                        Belanja = 0,
                        Tbs = item.Amaun,
                        JumLiabiliti = 0,
                        Baki = -item.Amaun
                    });
                }
            }
            // add Inden end

            // add Pelarasan PO

            List<AkPelarasanPOObjek> PelPOObjekList = _context.AkPelarasanPOObjek
                .Include(obj => obj.AkPelarasanPO)
                .Include(obj => obj.AkCarta)
                .Include(obj => obj.JKWPTJBahagian)
                    .ThenInclude(obj => obj!.JKW)
                .Include(obj => obj.JKWPTJBahagian)
                    .ThenInclude(obj => obj!.JPTJ)
                .Include(obj => obj.JKWPTJBahagian)
                    .ThenInclude(obj => obj!.JBahagian)
                .Where(po => po.AkPelarasanPO!.FlPosting == 0
                    && po.AkPelarasanPO!.FlBatal != 1
                    && po.AkPelarasanPO!.Tahun == Tahun
                    ).ToList();

            if (!string.IsNullOrEmpty(Carta1Id) && !string.IsNullOrEmpty(Carta2Id))
            {
                Tuple<string, string> range = Tuple.Create(Carta1Id, Carta2Id);

                PelPOObjekList = PelPOObjekList.Where(s =>
                        range.Item1.CompareTo(s.AkCarta?.Kod?.Substring(0, range.Item1.Length)) <= 0 &&
                        s.AkCarta?.Kod?.Substring(0, range.Item2.Length).CompareTo(range.Item2) <= 0)
                        .OrderBy(x => x.AkCarta!.Kod).ToList();
            }

            if (PelPOObjekList != null && PelPOObjekList.Count > 0)
            {
                foreach (var item in PelPOObjekList)
                {
                    votNotPosted.Add(new AbBukuVot
                    {
                        Tahun = item.AkPelarasanPO?.Tahun,
                        JKW = item.JKWPTJBahagian?.JKW,
                        JKWId = (int)item.JKWPTJBahagian?.JKWId!,
                        JPTJ = item.JKWPTJBahagian?.JPTJ,
                        JPTJId = (int)item.JKWPTJBahagian?.JPTJId!,
                        JBahagian = item.JKWPTJBahagian?.JBahagian,
                        JBahagianId = (int)item.JKWPTJBahagian?.JBahagianId!,
                        VotId = item.AkCartaId,
                        Vot = item.AkCarta,
                        Debit = 0,
                        Kredit = 0,
                        Tanggungan = item.Amaun,
                        Liabiliti = 0,
                        Belanja = 0,
                        Tbs = item.Amaun,
                        JumLiabiliti = 0,
                        Baki = -item.Amaun
                    });
                }
            }

            // add Pelarasan PO end

            // add Pelarasan Inden
            List<AkPelarasanIndenObjek> PelIndenObjekList = _context.AkPelarasanIndenObjek
                .Include(obj => obj.AkPelarasanInden)
                .Include(obj => obj.AkCarta)
                .Include(obj => obj.JKWPTJBahagian)
                    .ThenInclude(obj => obj!.JKW)
                .Include(obj => obj.JKWPTJBahagian)
                    .ThenInclude(obj => obj!.JPTJ)
                .Include(obj => obj.JKWPTJBahagian)
                    .ThenInclude(obj => obj!.JBahagian)
                .Where(po => po.AkPelarasanInden!.FlPosting == 0
                    && po.AkPelarasanInden!.FlBatal != 1
                    && po.AkPelarasanInden!.Tahun == Tahun
                    ).ToList();

            if (!string.IsNullOrEmpty(Carta1Id) && !string.IsNullOrEmpty(Carta2Id))
            {
                Tuple<string, string> range = Tuple.Create(Carta1Id, Carta2Id);

                PelIndenObjekList = PelIndenObjekList.Where(s =>
                        range.Item1.CompareTo(s.AkCarta?.Kod?.Substring(0, range.Item1.Length)) <= 0 &&
                        s.AkCarta?.Kod?.Substring(0, range.Item2.Length).CompareTo(range.Item2) <= 0)
                        .OrderBy(x => x.AkCarta!.Kod).ToList();
            }

            if (PelIndenObjekList != null && PelIndenObjekList.Count > 0)
            {
                foreach (var item in PelIndenObjekList)
                {
                    votNotPosted.Add(new AbBukuVot
                    {
                        Tahun = item.AkPelarasanInden?.Tahun,
                        JKW = item.JKWPTJBahagian?.JKW,
                        JKWId = (int)item.JKWPTJBahagian?.JKWId!,
                        JPTJ = item.JKWPTJBahagian?.JPTJ,
                        JPTJId = (int)item.JKWPTJBahagian?.JPTJId!,
                        JBahagian = item.JKWPTJBahagian?.JBahagian,
                        JBahagianId = (int)item.JKWPTJBahagian?.JBahagianId!,
                        VotId = item.AkCartaId,
                        Vot = item.AkCarta,
                        Debit = 0,
                        Kredit = 0,
                        Tanggungan = item.Amaun,
                        Liabiliti = 0,
                        Belanja = 0,
                        Tbs = item.Amaun,
                        JumLiabiliti = 0,
                        Baki = -item.Amaun
                    });
                }
            }
            // add Pelarasan Inden end

            // add Belian
            List<AkBelianObjek> BelianList = _context.AkBelianObjek
                .Include(obj => obj.AkBelian)
                .Include(obj => obj.AkCarta)
                .Include(obj => obj.JKWPTJBahagian)
                    .ThenInclude(obj => obj!.JKW)
                .Include(obj => obj.JKWPTJBahagian)
                    .ThenInclude(obj => obj!.JPTJ)
                .Include(obj => obj.JKWPTJBahagian)
                    .ThenInclude(obj => obj!.JBahagian)
                .Where(po => po.AkBelian!.FlPosting == 0
                    && po.AkBelian!.FlBatal != 1
                    && po.AkBelian!.Tahun == Tahun
                    ).ToList();

            if (!string.IsNullOrEmpty(Carta1Id) && !string.IsNullOrEmpty(Carta2Id))
            {
                Tuple<string, string> range = Tuple.Create(Carta1Id, Carta2Id);

                BelianList = BelianList.Where(s =>
                        range.Item1.CompareTo(s.AkCarta?.Kod?.Substring(0, range.Item1.Length)) <= 0 &&
                        s.AkCarta?.Kod?.Substring(0, range.Item2.Length).CompareTo(range.Item2) <= 0)
                        .OrderBy(x => x.AkCarta!.Kod).ToList();
            }

            if (BelianList != null && BelianList.Count > 0)
            {
                foreach (var item in BelianList)
                {
                    if (item.AkBelian != null)
                    {
                        if (item.AkBelian.AkPOId != null || item.AkBelian.AkIndenId != null)
                        {
                            votNotPosted.Add(new AbBukuVot
                            {
                                Tahun = item.AkBelian.Tahun,
                                JKW = item.JKWPTJBahagian?.JKW,
                                JKWId = (int)item.JKWPTJBahagian?.JKWId!,
                                JPTJ = item.JKWPTJBahagian?.JPTJ,
                                JPTJId = (int)item.JKWPTJBahagian?.JPTJId!,
                                JBahagian = item.JKWPTJBahagian?.JBahagian,
                                JBahagianId = (int)item.JKWPTJBahagian?.JBahagianId!,
                                VotId = item.AkCartaId,
                                Vot = item.AkCarta,
                                Debit = item.Amaun,
                                Kredit = 0,
                                Tanggungan = 0,
                                Liabiliti = item.Amaun,
                                Belanja = item.Amaun,
                                Tbs = 0,
                                JumLiabiliti = item.Amaun,
                                Baki = 0
                            });
                        }
                        else
                        {
                            if (item.AkBelian.AkAkaunAkruId != null)
                            {
                                votNotPosted.Add(new AbBukuVot
                                {
                                    Tahun = item.AkBelian.Tahun,
                                    JKW = item.JKWPTJBahagian?.JKW,
                                    JKWId = (int)item.JKWPTJBahagian?.JKWId!,
                                    JPTJ = item.JKWPTJBahagian?.JPTJ,
                                    JPTJId = (int)item.JKWPTJBahagian?.JPTJId!,
                                    JBahagian = item.JKWPTJBahagian?.JBahagian,
                                    JBahagianId = (int)item.JKWPTJBahagian?.JBahagianId!,
                                    VotId = item.AkCartaId,
                                    Vot = item.AkCarta,
                                    Debit = item.Amaun,
                                    Kredit = 0,
                                    Tanggungan = 0,
                                    Liabiliti = item.Amaun,
                                    Belanja = item.Amaun,
                                    Tbs = 0,
                                    JumLiabiliti = item.Amaun,
                                    Baki = -item.Amaun
                                });
                            }
                        }
                    }


                }
            }
            // add Belian end

            // add NotaDebitKreditDiterima
            List<AkNotaDebitKreditDiterimaObjek> NDKObjekList = _context.AkNotaDebitKreditDiterimaObjek
                .Include(obj => obj.AkNotaDebitKreditDiterima)
                    .ThenInclude(obj => obj!.AkBelian)
                .Include(obj => obj.AkCarta)
                .Include(obj => obj.JKWPTJBahagian)
                    .ThenInclude(obj => obj!.JKW)
                .Include(obj => obj.JKWPTJBahagian)
                    .ThenInclude(obj => obj!.JPTJ)
                .Include(obj => obj.JKWPTJBahagian)
                    .ThenInclude(obj => obj!.JBahagian)
                .Where(po => po.AkNotaDebitKreditDiterima!.FlPosting == 0
                    && po.AkNotaDebitKreditDiterima!.FlBatal != 1
                    && po.AkNotaDebitKreditDiterima!.Tahun == Tahun
                    ).ToList();

            if (!string.IsNullOrEmpty(Carta1Id) && !string.IsNullOrEmpty(Carta2Id))
            {
                Tuple<string, string> range = Tuple.Create(Carta1Id, Carta2Id);

                NDKObjekList = NDKObjekList.Where(s =>
                        range.Item1.CompareTo(s.AkCarta?.Kod?.Substring(0, range.Item1.Length)) <= 0 &&
                        s.AkCarta?.Kod?.Substring(0, range.Item2.Length).CompareTo(range.Item2) <= 0)
                        .OrderBy(x => x.AkCarta!.Kod).ToList();
            }

            if (NDKObjekList != null && NDKObjekList.Count > 0)
            {
                foreach (var item in NDKObjekList)
                {
                    if (item.AkNotaDebitKreditDiterima != null && item.AkNotaDebitKreditDiterima.AkBelian != null && item.AkNotaDebitKreditDiterima.AkBelian.AkAkaunAkruId != null)
                    {
                        if (item.AkNotaDebitKreditDiterima.AkBelian.AkPOId != null || item.AkNotaDebitKreditDiterima.AkBelian.AkIndenId != null)
                        {
                            if (item.AkNotaDebitKreditDiterima.FlDebitKredit == 1) // kredit
                            {
                                votNotPosted.Add(new AbBukuVot
                                {
                                    Tahun = item.AkNotaDebitKreditDiterima.Tahun,
                                    JKW = item.JKWPTJBahagian?.JKW,
                                    JKWId = (int)item.JKWPTJBahagian?.JKWId!,
                                    JPTJ = item.JKWPTJBahagian?.JPTJ,
                                    JPTJId = (int)item.JKWPTJBahagian?.JPTJId!,
                                    JBahagian = item.JKWPTJBahagian?.JBahagian,
                                    JBahagianId = (int)item.JKWPTJBahagian?.JBahagianId!,
                                    VotId = item.AkCartaId,
                                    Vot = item.AkCarta,
                                    Debit = 0,
                                    Kredit = item.Amaun,
                                    Tanggungan = 0,
                                    Liabiliti = -item.Amaun,
                                    Belanja = -item.Amaun,
                                    Tbs = 0,
                                    JumLiabiliti = -item.Amaun,
                                    Baki = 0
                                });
                            }
                            else
                            {
                                votNotPosted.Add(new AbBukuVot
                                {
                                    Tahun = item.AkNotaDebitKreditDiterima.Tahun,
                                    JKW = item.JKWPTJBahagian?.JKW,
                                    JKWId = (int)item.JKWPTJBahagian?.JKWId!,
                                    JPTJ = item.JKWPTJBahagian?.JPTJ,
                                    JPTJId = (int)item.JKWPTJBahagian?.JPTJId!,
                                    JBahagian = item.JKWPTJBahagian?.JBahagian,
                                    JBahagianId = (int)item.JKWPTJBahagian?.JBahagianId!,
                                    VotId = item.AkCartaId,
                                    Vot = item.AkCarta,
                                    Debit = item.Amaun,
                                    Kredit = 0,
                                    Tanggungan = 0,
                                    Liabiliti = item.Amaun,
                                    Belanja = item.Amaun,
                                    Tbs = 0,
                                    JumLiabiliti = item.Amaun,
                                    Baki = 0
                                });
                            }

                        }
                    }

                }
            }
            // add NotaDebitKreditDiterima end

            // add PV
            List<AkPVObjek> akPVObjekList = _context.AkPVObjek
                .Include(obj => obj.AkPV)
                    .ThenInclude(pv => pv!.AkPVInvois)!
                        .ThenInclude(pv => pv!.AkBelian)
                            .ThenInclude(belian => belian!.AkPO)
                                .ThenInclude(po => po!.AkPOObjek)
                .Include(obj => obj.AkPV)
                    .ThenInclude(pv => pv!.AkPVInvois)!
                        .ThenInclude(pv => pv!.AkBelian)
                            .ThenInclude(belian => belian!.AkInden)
                                .ThenInclude(inden => inden!.AkIndenObjek)
                .Include(obj => obj.AkCarta)
                .Include(obj => obj.JKWPTJBahagian)
                    .ThenInclude(obj => obj!.JKW)
                .Include(obj => obj.JKWPTJBahagian)
                    .ThenInclude(obj => obj!.JPTJ)
                .Include(obj => obj.JKWPTJBahagian)
                    .ThenInclude(obj => obj!.JBahagian)
                .Where(po => po.AkPV!.FlPosting == 0
                    && po.AkPV!.FlBatal != 1
                    && po.AkPV!.Tahun == Tahun
                    ).ToList();

            if (!string.IsNullOrEmpty(Carta1Id) && !string.IsNullOrEmpty(Carta2Id))
            {
                Tuple<string, string> range = Tuple.Create(Carta1Id, Carta2Id);

                akPVObjekList = akPVObjekList.Where(s =>
                        range.Item1.CompareTo(s.AkCarta?.Kod?.Substring(0, range.Item1.Length)) <= 0 &&
                        s.AkCarta?.Kod?.Substring(0, range.Item2.Length).CompareTo(range.Item2) <= 0)
                        .OrderBy(x => x.AkCarta!.Kod).ToList();
            }

            if (akPVObjekList != null && akPVObjekList.Count > 0)
            {
                foreach (var item in akPVObjekList)
                {
                    // Cash Basis (PV yang tiada invois atau PV yang ada Invois tanpa akruan)
                    if (item.AkPV != null && (PVWithoutInvois(item.AkPV) ||
                        PVWithOneInvoisNotAkru(item.AkPV) ||
                        PVWithMultipleInvoisNotAkru(item.AkPV)))
                    {

                        votNotPosted.Add(new AbBukuVot
                        {
                            Tahun = item.AkPV?.Tahun,
                            JKW = item.JKWPTJBahagian?.JKW,
                            JKWId = (int)item.JKWPTJBahagian?.JKWId!,
                            JPTJ = item.JKWPTJBahagian?.JPTJ,
                            JPTJId = (int)item.JKWPTJBahagian?.JPTJId!,
                            JBahagian = item.JKWPTJBahagian?.JBahagian,
                            JBahagianId = (int)item.JKWPTJBahagian?.JBahagianId!,
                            VotId = item.AkCartaId,
                            Vot = item.AkCarta,
                            Debit = item.Amaun,
                            Kredit = 0,
                            Tanggungan = 0,
                            Liabiliti = 0,
                            Belanja = item.Amaun,
                            Tbs = 0,
                            JumLiabiliti = 0,
                            Baki = -item.Amaun
                        });
                    }


                    // PV yang ada Invois tanpa tanggungan
                    if (item.AkPV != null && (PVWithOneInvoisAkruWithoutPOOrInden(item.AkPV) ||
                        PVWithMultipleInvoisAkruWithoutPOOrInden(item.AkPV)))
                    {
                        votNotPosted.Add(new AbBukuVot
                        {
                            Tahun = item.AkPV?.Tahun,
                            JKW = item.JKWPTJBahagian?.JKW,
                            JKWId = (int)item.JKWPTJBahagian?.JKWId!,
                            JPTJ = item.JKWPTJBahagian?.JPTJ,
                            JPTJId = (int)item.JKWPTJBahagian?.JPTJId!,
                            JBahagian = item.JKWPTJBahagian?.JBahagian,
                            JBahagianId = (int)item.JKWPTJBahagian?.JBahagianId!,
                            VotId = item.AkCartaId,
                            Vot = item.AkCarta,
                            Debit = 0,
                            Kredit = 0,
                            Tanggungan = 0,
                            Liabiliti = -item.Amaun,
                            Belanja = 0,
                            Tbs = 0,
                            JumLiabiliti = -item.Amaun,
                            Baki = 0
                        });
                    }

                    // PV yang ada Invois dengan tanggungan
                    if (item.AkPV != null &&
                        (PVWithOneInvoisAkruWithOnePOAndWithoutInden(item.AkPV) ||
                        PVWithOneInvoisAkruWithOneIndenAndWithoutPO(item.AkPV) ||
                        PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek(item.AkPV) ||
                        PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneDifferentObjek(item.AkPV)))
                    {
                        votNotPosted.Add(new AbBukuVot
                        {
                            Tahun = item.AkPV?.Tahun,
                            JKW = item.JKWPTJBahagian?.JKW,
                            JKWId = (int)item.JKWPTJBahagian?.JKWId!,
                            JPTJ = item.JKWPTJBahagian?.JPTJ,
                            JPTJId = (int)item.JKWPTJBahagian?.JPTJId!,
                            JBahagian = item.JKWPTJBahagian?.JBahagian,
                            JBahagianId = (int)item.JKWPTJBahagian?.JBahagianId!,
                            VotId = item.AkCartaId,
                            Vot = item.AkCarta,
                            Debit = item.Amaun,
                            Kredit = 0,
                            Tanggungan = -item.Amaun,
                            Liabiliti = -item.Amaun,
                            Belanja = 0,
                            Tbs = -item.Amaun,
                            JumLiabiliti = -item.Amaun,
                            Baki = 0
                        });
                    }
                }
            }
            // add PV end

            vot.AddRange(votNotPosted);

            // add all end
            return vot.GroupBy(x => new { x.Tahun, x.VotId, x.JBahagianId })
                .Select(l => new AbBukuVot
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
                    Tbs = l.Sum(c => c.Tbs),
                    JumLiabiliti = l.Sum(c => c.Liabiliti),
                    Baki = l.Sum(c => c.Baki)
                }).OrderBy(b => b.Vot!.Kod).ToList();


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

                // add all PO, Belian, NotaDebitKreditDiterima, PV (not posting)

                List<AbBukuVot> votNotPosted = new List<AbBukuVot>();
                // add PO

                List<AkPOObjek> POObjekList = await _context.AkPOObjek
                    .Include(obj => obj.AkPO)
                    .Include(obj => obj.AkCarta)
                    .Include(obj => obj.JKWPTJBahagian)
                        .ThenInclude(obj => obj!.JKW)
                    .Include(obj => obj.JKWPTJBahagian)
                        .ThenInclude(obj => obj!.JPTJ)
                    .Include(obj => obj.JKWPTJBahagian)
                        .ThenInclude(obj => obj!.JBahagian)
                    .Where(x => x.AkPO!.FlPosting == 0
                        && x.AkPO!.FlBatal != 1
                        && x.AkPO!.Tahun == Tahun
                        && x.AkCartaId == AkCartaId
                        && x.JKWPTJBahagian!.JKWId == JKWId
                        && x.JKWPTJBahagian!.JPTJId == JPTJId
                        && x.JKWPTJBahagian!.JBahagianId == JBahagianId
                        && x.AkPO!.Tarikh >= date1 && x.AkPO!.Tarikh <= date2
                        ).ToListAsync();

                if (POObjekList != null && POObjekList.Count > 0)
                {
                    foreach (var item in POObjekList)
                    {
                        votNotPosted.Add(new AbBukuVot
                        {
                            IsPosted = false,
                            Tarikh = item.AkPO!.Tarikh,
                            NoRujukan = item.AkPO?.NoRujukan,
                            Tahun = item.AkPO?.Tahun,
                            JKW = item.JKWPTJBahagian?.JKW,
                            JKWId = (int)item.JKWPTJBahagian?.JKWId!,
                            JPTJ = item.JKWPTJBahagian?.JPTJ,
                            JPTJId = (int)item.JKWPTJBahagian?.JPTJId!,
                            JBahagian = item.JKWPTJBahagian?.JBahagian,
                            JBahagianId = (int)item.JKWPTJBahagian?.JBahagianId!,
                            VotId = item.AkCartaId,
                            Vot = item.AkCarta,
                            Debit = 0,
                            Kredit = 0,
                            Tanggungan = item.Amaun,
                            Liabiliti = 0,
                            Belanja = 0,
                            Tbs = item.Amaun,
                            JumLiabiliti = 0,
                            Baki = -item.Amaun
                        });
                    }
                }

                // add PO end

                // add Inden
                List<AkIndenObjek> IndenObjekList = await _context.AkIndenObjek
                    .Include(obj => obj.AkInden)
                    .Include(obj => obj.AkCarta)
                    .Include(obj => obj.JKWPTJBahagian)
                        .ThenInclude(obj => obj!.JKW)
                    .Include(obj => obj.JKWPTJBahagian)
                        .ThenInclude(obj => obj!.JPTJ)
                    .Include(obj => obj.JKWPTJBahagian)
                        .ThenInclude(obj => obj!.JBahagian)
                    .Where(x => x.AkInden!.FlPosting == 0
                        && x.AkInden!.FlBatal != 1
                        && x.AkInden!.Tahun == Tahun
                        && x.AkCartaId == AkCartaId
                        && x.JKWPTJBahagian!.JKWId == JKWId
                        && x.JKWPTJBahagian!.JPTJId == JPTJId
                        && x.JKWPTJBahagian!.JBahagianId == JBahagianId
                        && x.AkInden!.Tarikh >= date1 && x.AkInden!.Tarikh <= date2
                        ).ToListAsync();

                if (IndenObjekList != null && IndenObjekList.Count > 0)
                {
                    foreach (var item in IndenObjekList)
                    {
                        votNotPosted.Add(new AbBukuVot
                        {
                            IsPosted = false,
                            Tarikh = item.AkInden!.Tarikh,
                            NoRujukan = item.AkInden?.NoRujukan,
                            Tahun = item.AkInden?.Tahun,
                            JKW = item.JKWPTJBahagian?.JKW,
                            JKWId = (int)item.JKWPTJBahagian?.JKWId!,
                            JPTJ = item.JKWPTJBahagian?.JPTJ,
                            JPTJId = (int)item.JKWPTJBahagian?.JPTJId!,
                            JBahagian = item.JKWPTJBahagian?.JBahagian,
                            JBahagianId = (int)item.JKWPTJBahagian?.JBahagianId!,
                            VotId = item.AkCartaId,
                            Vot = item.AkCarta,
                            Debit = 0,
                            Kredit = 0,
                            Tanggungan = item.Amaun,
                            Liabiliti = 0,
                            Belanja = 0,
                            Tbs = item.Amaun,
                            JumLiabiliti = 0,
                            Baki = -item.Amaun
                        });
                    }
                }
                // add Inden end

                // add Pelarasan PO

                List<AkPelarasanPOObjek> PelPOObjekList = await _context.AkPelarasanPOObjek
                    .Include(obj => obj.AkPelarasanPO)
                    .Include(obj => obj.AkCarta)
                    .Include(obj => obj.JKWPTJBahagian)
                        .ThenInclude(obj => obj!.JKW)
                    .Include(obj => obj.JKWPTJBahagian)
                        .ThenInclude(obj => obj!.JPTJ)
                    .Include(obj => obj.JKWPTJBahagian)
                        .ThenInclude(obj => obj!.JBahagian)
                    .Where(x => x.AkPelarasanPO!.FlPosting == 0
                        && x.AkPelarasanPO!.FlBatal != 1
                        && x.AkPelarasanPO!.Tahun == Tahun
                        && x.AkCartaId == AkCartaId
                        && x.JKWPTJBahagian!.JKWId == JKWId
                        && x.JKWPTJBahagian!.JPTJId == JPTJId
                        && x.JKWPTJBahagian!.JBahagianId == JBahagianId
                        && x.AkPelarasanPO!.Tarikh >= date1 && x.AkPelarasanPO!.Tarikh <= date2
                        ).ToListAsync();

                if (PelPOObjekList != null && PelPOObjekList.Count > 0)
                {
                    foreach (var item in PelPOObjekList)
                    {
                        votNotPosted.Add(new AbBukuVot
                        {
                            IsPosted = false,
                            Tarikh = item.AkPelarasanPO!.Tarikh,
                            NoRujukan = item.AkPelarasanPO?.NoRujukan,
                            Tahun = item.AkPelarasanPO?.Tahun,
                            JKW = item.JKWPTJBahagian?.JKW,
                            JKWId = (int)item.JKWPTJBahagian?.JKWId!,
                            JPTJ = item.JKWPTJBahagian?.JPTJ,
                            JPTJId = (int)item.JKWPTJBahagian?.JPTJId!,
                            JBahagian = item.JKWPTJBahagian?.JBahagian,
                            JBahagianId = (int)item.JKWPTJBahagian?.JBahagianId!,
                            VotId = item.AkCartaId,
                            Vot = item.AkCarta,
                            Debit = 0,
                            Kredit = 0,
                            Tanggungan = item.Amaun,
                            Liabiliti = 0,
                            Belanja = 0,
                            Tbs = item.Amaun,
                            JumLiabiliti = 0,
                            Baki = -item.Amaun
                        });
                    }
                }

                // add Pelarasan PO end

                // add Pelarasan Inden
                List<AkPelarasanIndenObjek> PelIndenObjekList = await _context.AkPelarasanIndenObjek
                    .Include(obj => obj.AkPelarasanInden)
                    .Include(obj => obj.AkCarta)
                    .Include(obj => obj.JKWPTJBahagian)
                        .ThenInclude(obj => obj!.JKW)
                    .Include(obj => obj.JKWPTJBahagian)
                        .ThenInclude(obj => obj!.JPTJ)
                    .Include(obj => obj.JKWPTJBahagian)
                        .ThenInclude(obj => obj!.JBahagian)
                    .Where(x => x.AkPelarasanInden!.FlPosting == 0
                        && x.AkPelarasanInden!.FlBatal != 1
                        && x.AkPelarasanInden!.Tahun == Tahun
                        && x.AkCartaId == AkCartaId
                        && x.JKWPTJBahagian!.JKWId == JKWId
                        && x.JKWPTJBahagian!.JPTJId == JPTJId
                        && x.JKWPTJBahagian!.JBahagianId == JBahagianId
                        && x.AkPelarasanInden!.Tarikh >= date1 && x.AkPelarasanInden!.Tarikh <= date2
                        ).ToListAsync();

                if (PelIndenObjekList != null && PelIndenObjekList.Count > 0)
                {
                    foreach (var item in PelIndenObjekList)
                    {
                        votNotPosted.Add(new AbBukuVot
                        {
                            IsPosted = false,
                            Tarikh = item.AkPelarasanInden!.Tarikh,
                            NoRujukan = item.AkPelarasanInden?.NoRujukan,
                            Tahun = item.AkPelarasanInden?.Tahun,
                            JKW = item.JKWPTJBahagian?.JKW,
                            JKWId = (int)item.JKWPTJBahagian?.JKWId!,
                            JPTJ = item.JKWPTJBahagian?.JPTJ,
                            JPTJId = (int)item.JKWPTJBahagian?.JPTJId!,
                            JBahagian = item.JKWPTJBahagian?.JBahagian,
                            JBahagianId = (int)item.JKWPTJBahagian?.JBahagianId!,
                            VotId = item.AkCartaId,
                            Vot = item.AkCarta,
                            Debit = 0,
                            Kredit = 0,
                            Tanggungan = item.Amaun,
                            Liabiliti = 0,
                            Belanja = 0,
                            Tbs = item.Amaun,
                            JumLiabiliti = 0,
                            Baki = -item.Amaun
                        });
                    }
                }
                // add Pelarasan Inden end

                // add Belian
                List<AkBelianObjek> BelianList = await _context.AkBelianObjek
                    .Include(obj => obj.AkBelian)
                    .Include(obj => obj.AkCarta)
                    .Include(obj => obj.JKWPTJBahagian)
                        .ThenInclude(obj => obj!.JKW)
                    .Include(obj => obj.JKWPTJBahagian)
                        .ThenInclude(obj => obj!.JPTJ)
                    .Include(obj => obj.JKWPTJBahagian)
                        .ThenInclude(obj => obj!.JBahagian)
                    .Where(x => x.AkBelian!.FlPosting == 0
                        && x.AkBelian!.FlBatal != 1
                        && x.AkBelian!.Tahun == Tahun
                        && x.AkCartaId == AkCartaId
                        && x.JKWPTJBahagian!.JKWId == JKWId
                        && x.JKWPTJBahagian!.JPTJId == JPTJId
                        && x.JKWPTJBahagian!.JBahagianId == JBahagianId
                        && x.AkBelian!.Tarikh >= date1 && x.AkBelian!.Tarikh <= date2
                        ).ToListAsync();

                if (BelianList != null && BelianList.Count > 0)
                {
                    foreach (var item in BelianList)
                    {
                        if (item.AkBelian != null)
                        {
                            if (item.AkBelian.AkPOId != null || item.AkBelian.AkIndenId != null)
                            {
                                votNotPosted.Add(new AbBukuVot
                                {
                                    IsPosted = false,
                                    Tarikh = item.AkBelian.Tarikh,
                                    NoRujukan = item.AkBelian.NoRujukan,
                                    Tahun = item.AkBelian.Tahun,
                                    JKW = item.JKWPTJBahagian?.JKW,
                                    JKWId = (int)item.JKWPTJBahagian?.JKWId!,
                                    JPTJ = item.JKWPTJBahagian?.JPTJ,
                                    JPTJId = (int)item.JKWPTJBahagian?.JPTJId!,
                                    JBahagian = item.JKWPTJBahagian?.JBahagian,
                                    JBahagianId = (int)item.JKWPTJBahagian?.JBahagianId!,
                                    VotId = item.AkCartaId,
                                    Vot = item.AkCarta,
                                    Debit = item.Amaun,
                                    Kredit = 0,
                                    Tanggungan = 0,
                                    Liabiliti = item.Amaun,
                                    Belanja = item.Amaun,
                                    Tbs = 0,
                                    JumLiabiliti = item.Amaun,
                                    Baki = 0
                                });
                            }
                            else
                            {
                                if (item.AkBelian.AkAkaunAkruId != null)
                                {
                                    votNotPosted.Add(new AbBukuVot
                                    {
                                        IsPosted = false,
                                        Tarikh = item.AkBelian.Tarikh,
                                        NoRujukan = item.AkBelian.NoRujukan,
                                        Tahun = item.AkBelian.Tahun,
                                        JKW = item.JKWPTJBahagian?.JKW,
                                        JKWId = (int)item.JKWPTJBahagian?.JKWId!,
                                        JPTJ = item.JKWPTJBahagian?.JPTJ,
                                        JPTJId = (int)item.JKWPTJBahagian?.JPTJId!,
                                        JBahagian = item.JKWPTJBahagian?.JBahagian,
                                        JBahagianId = (int)item.JKWPTJBahagian?.JBahagianId!,
                                        VotId = item.AkCartaId,
                                        Vot = item.AkCarta,
                                        Debit = item.Amaun,
                                        Kredit = 0,
                                        Tanggungan = 0,
                                        Liabiliti = item.Amaun,
                                        Belanja = item.Amaun,
                                        Tbs = 0,
                                        JumLiabiliti = item.Amaun,
                                        Baki = -item.Amaun
                                    });
                                }
                            }
                        }


                    }
                }
                // add Belian end

                // add NotaDebitKreditDiterima
                List<AkNotaDebitKreditDiterimaObjek> NDKObjekList = await _context.AkNotaDebitKreditDiterimaObjek
                    .Include(obj => obj.AkNotaDebitKreditDiterima)
                        .ThenInclude(obj => obj!.AkBelian)
                    .Include(obj => obj.AkCarta)
                    .Include(obj => obj.JKWPTJBahagian)
                        .ThenInclude(obj => obj!.JKW)
                    .Include(obj => obj.JKWPTJBahagian)
                        .ThenInclude(obj => obj!.JPTJ)
                    .Include(obj => obj.JKWPTJBahagian)
                        .ThenInclude(obj => obj!.JBahagian)
                    .Where(x => x.AkNotaDebitKreditDiterima!.FlPosting == 0
                        && x.AkNotaDebitKreditDiterima!.FlBatal != 1
                        && x.AkNotaDebitKreditDiterima!.Tahun == Tahun
                        && x.AkCartaId == AkCartaId
                        && x.JKWPTJBahagian!.JKWId == JKWId
                        && x.JKWPTJBahagian!.JPTJId == JPTJId
                        && x.JKWPTJBahagian!.JBahagianId == JBahagianId
                        && x.AkNotaDebitKreditDiterima!.Tarikh >= date1 && x.AkNotaDebitKreditDiterima!.Tarikh <= date2
                        ).ToListAsync();

                if (NDKObjekList != null && NDKObjekList.Count > 0)
                {
                    foreach (var item in NDKObjekList)
                    {
                        if (item.AkNotaDebitKreditDiterima != null && item.AkNotaDebitKreditDiterima.AkBelian != null)
                        {
                            if (item.AkNotaDebitKreditDiterima.AkBelian.AkPOId != null || item.AkNotaDebitKreditDiterima.AkBelian.AkIndenId != null)
                            {
                                if (item.AkNotaDebitKreditDiterima.FlDebitKredit == 1) // kredit
                                {
                                    votNotPosted.Add(new AbBukuVot
                                    {
                                        IsPosted = false,
                                        Tarikh = item.AkNotaDebitKreditDiterima.Tarikh,
                                        NoRujukan = item.AkNotaDebitKreditDiterima.NoRujukan,
                                        Tahun = item.AkNotaDebitKreditDiterima.Tahun,
                                        JKW = item.JKWPTJBahagian?.JKW,
                                        JKWId = (int)item.JKWPTJBahagian?.JKWId!,
                                        JPTJ = item.JKWPTJBahagian?.JPTJ,
                                        JPTJId = (int)item.JKWPTJBahagian?.JPTJId!,
                                        JBahagian = item.JKWPTJBahagian?.JBahagian,
                                        JBahagianId = (int)item.JKWPTJBahagian?.JBahagianId!,
                                        VotId = item.AkCartaId,
                                        Vot = item.AkCarta,
                                        Debit = 0,
                                        Kredit = item.Amaun,
                                        Tanggungan = 0,
                                        Liabiliti = -item.Amaun,
                                        Belanja = -item.Amaun,
                                        Tbs = 0,
                                        JumLiabiliti = -item.Amaun,
                                        Baki = 0
                                    });
                                }
                                else
                                {
                                    votNotPosted.Add(new AbBukuVot
                                    {
                                        IsPosted = false,
                                        Tarikh = item.AkNotaDebitKreditDiterima.Tarikh,
                                        NoRujukan = item.AkNotaDebitKreditDiterima.NoRujukan,
                                        Tahun = item.AkNotaDebitKreditDiterima.Tahun,
                                        JKW = item.JKWPTJBahagian?.JKW,
                                        JKWId = (int)item.JKWPTJBahagian?.JKWId!,
                                        JPTJ = item.JKWPTJBahagian?.JPTJ,
                                        JPTJId = (int)item.JKWPTJBahagian?.JPTJId!,
                                        JBahagian = item.JKWPTJBahagian?.JBahagian,
                                        JBahagianId = (int)item.JKWPTJBahagian?.JBahagianId!,
                                        VotId = item.AkCartaId,
                                        Vot = item.AkCarta,
                                        Debit = item.Amaun,
                                        Kredit = 0,
                                        Tanggungan = 0,
                                        Liabiliti = item.Amaun,
                                        Belanja = item.Amaun,
                                        Tbs = 0,
                                        JumLiabiliti = item.Amaun,
                                        Baki = 0
                                    });
                                }

                            }
                        }

                    }
                }
                // add NotaDebitKreditDiterima end

                // add PV
                List<AkPVObjek> akPVObjekList = await _context.AkPVObjek
                    .Include(obj => obj.AkPV)
                        .ThenInclude(pv => pv!.AkPVInvois)!
                            .ThenInclude(pv => pv!.AkBelian)
                                .ThenInclude(belian => belian!.AkPO)
                                    .ThenInclude(po => po!.AkPOObjek)
                    .Include(obj => obj.AkPV)
                        .ThenInclude(pv => pv!.AkPVInvois)!
                            .ThenInclude(pv => pv!.AkBelian)
                                .ThenInclude(belian => belian!.AkInden)
                                    .ThenInclude(inden => inden!.AkIndenObjek)
                    .Include(obj => obj.AkCarta)
                    .Include(obj => obj.JKWPTJBahagian)
                        .ThenInclude(obj => obj!.JKW)
                    .Include(obj => obj.JKWPTJBahagian)
                        .ThenInclude(obj => obj!.JPTJ)
                    .Include(obj => obj.JKWPTJBahagian)
                        .ThenInclude(obj => obj!.JBahagian)
                    .Where(x => x.AkPV!.FlPosting == 0
                        && x.AkPV!.FlBatal != 1
                        && x.AkPV!.Tahun == Tahun
                        && x.AkCartaId == AkCartaId
                        && x.JKWPTJBahagian!.JKWId == JKWId
                        && x.JKWPTJBahagian!.JPTJId == JPTJId
                        && x.JKWPTJBahagian!.JBahagianId == JBahagianId
                        && x.AkPV!.Tarikh >= date1 && x.AkPV!.Tarikh <= date2
                        ).ToListAsync();

                if (akPVObjekList != null && akPVObjekList.Count > 0)
                {
                    foreach (var item in akPVObjekList)
                    {
                        // Cash Basis (PV yang tiada invois atau PV yang ada Invois tanpa akruan)
                        if (item.AkPV != null && (PVWithoutInvois(item.AkPV) ||
                            PVWithOneInvoisNotAkru(item.AkPV) ||
                            PVWithMultipleInvoisNotAkru(item.AkPV)))
                        {

                            votNotPosted.Add(new AbBukuVot
                            {
                                IsPosted = false,
                                Tarikh = item.AkPV.Tarikh,
                                NoRujukan = item.AkPV.NoRujukan,
                                Tahun = item.AkPV.Tahun,
                                JKW = item.JKWPTJBahagian?.JKW,
                                JKWId = (int)item.JKWPTJBahagian?.JKWId!,
                                JPTJ = item.JKWPTJBahagian?.JPTJ,
                                JPTJId = (int)item.JKWPTJBahagian?.JPTJId!,
                                JBahagian = item.JKWPTJBahagian?.JBahagian,
                                JBahagianId = (int)item.JKWPTJBahagian?.JBahagianId!,
                                VotId = item.AkCartaId,
                                Vot = item.AkCarta,
                                Debit = item.Amaun,
                                Kredit = 0,
                                Tanggungan = 0,
                                Liabiliti = 0,
                                Belanja = item.Amaun,
                                Tbs = 0,
                                JumLiabiliti = 0,
                                Baki = -item.Amaun
                            });
                        }


                        // PV yang ada Invois tanpa tanggungan
                        if (item.AkPV != null && (PVWithOneInvoisAkruWithoutPOOrInden(item.AkPV) ||
                            PVWithMultipleInvoisAkruWithoutPOOrInden(item.AkPV)))
                        {
                            votNotPosted.Add(new AbBukuVot
                            {
                                IsPosted = false,
                                Tarikh = item.AkPV.Tarikh,
                                NoRujukan = item.AkPV.NoRujukan,
                                Tahun = item.AkPV.Tahun,
                                JKW = item.JKWPTJBahagian?.JKW,
                                JKWId = (int)item.JKWPTJBahagian?.JKWId!,
                                JPTJ = item.JKWPTJBahagian?.JPTJ,
                                JPTJId = (int)item.JKWPTJBahagian?.JPTJId!,
                                JBahagian = item.JKWPTJBahagian?.JBahagian,
                                JBahagianId = (int)item.JKWPTJBahagian?.JBahagianId!,
                                VotId = item.AkCartaId,
                                Vot = item.AkCarta,
                                Debit = 0,
                                Kredit = 0,
                                Tanggungan = 0,
                                Liabiliti = -item.Amaun,
                                Belanja = 0,
                                Tbs = 0,
                                JumLiabiliti = -item.Amaun,
                                Baki = 0
                            });
                        }

                        // PV yang ada Invois dengan tanggungan
                        if (item.AkPV != null &&
                            (PVWithOneInvoisAkruWithOnePOAndWithoutInden(item.AkPV) ||
                            PVWithOneInvoisAkruWithOneIndenAndWithoutPO(item.AkPV) ||
                            PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek(item.AkPV) ||
                            PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneDifferentObjek(item.AkPV)))
                        {
                            votNotPosted.Add(new AbBukuVot
                            {
                                IsPosted = false,
                                Tarikh = item.AkPV.Tarikh,
                                NoRujukan = item.AkPV.NoRujukan,
                                Tahun = item.AkPV.Tahun,
                                JKW = item.JKWPTJBahagian?.JKW,
                                JKWId = (int)item.JKWPTJBahagian?.JKWId!,
                                JPTJ = item.JKWPTJBahagian?.JPTJ,
                                JPTJId = (int)item.JKWPTJBahagian?.JPTJId!,
                                JBahagian = item.JKWPTJBahagian?.JBahagian,
                                JBahagianId = (int)item.JKWPTJBahagian?.JBahagianId!,
                                VotId = item.AkCartaId,
                                Vot = item.AkCarta,
                                Debit = item.Amaun,
                                Kredit = 0,
                                Tanggungan = -item.Amaun,
                                Liabiliti = -item.Amaun,
                                Belanja = 0,
                                Tbs = -item.Amaun,
                                JumLiabiliti = -item.Amaun,
                                Baki = 0
                            });
                        }
                    }
                }
                // add PV end

                abBukuVot.AddRange(votNotPosted);

                // add all end
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

        public async Task<bool> IsExistByJKWPTJBahagianAkCartaId(int jKWId, int ptjId, int jBahagianId, int votId)
        {
            bool IsExist = await _context.AbBukuVot.AnyAsync(bv => bv.JKWId == jKWId && bv.JPTJId == ptjId && bv.JBahagianId == jBahagianId && bv.VotId == votId);
            return IsExist;
        }

        // functions
        public bool PVWithoutInvois(AkPV akPV)
        {
            if (akPV.IsInvois == false && akPV.IsTanggungan == false && akPV.IsAkru == false && akPV.AkPVInvois != null && akPV.AkPVInvois.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool PVWithOneInvoisNotAkru(AkPV akPV)
        {
            if (akPV.IsInvois == true && akPV.IsTanggungan == false && akPV.IsAkru == false && akPV.AkPVInvois != null && akPV.AkPVInvois.Count == 1)
            {
                bool result = false;

                foreach (var invois in akPV.AkPVInvois)
                {
                    if (invois.IsTanggungan == false && invois.AkBelian != null && invois.AkBelian.AkAkaunAkruId == null)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }

                return result;
            }
            else
            {
                return false;
            }
        }

        public bool PVWithOneInvoisAkruWithoutPOOrInden(AkPV akPV)
        {
            if (akPV.IsInvois == true && akPV.IsTanggungan == false && akPV.IsAkru == true && akPV.AkPVInvois != null && akPV.AkPVInvois.Count > 0)
            {
                var result = false;

                foreach (var invois in akPV.AkPVInvois)
                {
                    if (invois.IsTanggungan == false && invois.AkBelian != null && invois.AkBelian.AkAkaunAkruId != null)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }

                return result;
            }
            else
            {
                return false;
            }
        }

        public bool PVWithOneInvoisAkruWithOnePOAndWithoutInden(AkPV akPV)
        {
            if (akPV.IsInvois == true && akPV.IsTanggungan == true && akPV.IsAkru == true && akPV.AkPVInvois != null && akPV.AkPVInvois.Count > 0)
            {
                bool result = false;
                foreach (var invois in akPV.AkPVInvois)
                {
                    if (invois.IsTanggungan == true && invois.AkBelian != null && invois.AkBelian.AkAkaunAkruId != null)
                    {
                        if (invois.AkBelian.AkPOId != null && invois.AkBelian.AkIndenId == null)
                        {
                            result = true;
                        }
                        else
                        {
                            result = false;
                        }
                    }
                }
                return result;
            }
            else { return false; }
        }

        public bool PVWithOneInvoisAkruWithOneIndenAndWithoutPO(AkPV akPV)
        {
            if (akPV.IsInvois == true && akPV.IsTanggungan == true && akPV.IsAkru == true && akPV.AkPVInvois != null && akPV.AkPVInvois.Count > 0)
            {
                bool result = false;
                foreach (var invois in akPV.AkPVInvois)
                {
                    if (invois.IsTanggungan == true && invois.AkBelian != null && invois.AkBelian.AkAkaunAkruId != null)
                    {
                        if (invois.AkBelian.AkIndenId != null && invois.AkBelian.AkPOId == null)
                        {
                            result = true;
                        }
                        else
                        {
                            result = false;
                        }
                    }
                }
                return result;
            }
            else { return false; }
        }

        public bool PVWithMultipleInvoisNotAkru(AkPV akPV)
        {
            if (akPV.IsInvois == true && akPV.IsTanggungan == false && akPV.IsAkru == false && akPV.AkPVInvois != null && akPV.AkPVInvois.Count > 1)
            {
                bool result = false;
                foreach (var invois in akPV.AkPVInvois)
                {
                    if (invois.IsTanggungan == false && invois.AkBelian != null && invois.AkBelian.AkAkaunAkruId == null)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                return result;
            }
            else
            {
                return false;
            }
        }

        public bool PVWithMultipleInvoisAkruWithoutPOOrInden(AkPV akPV)
        {
            if (akPV.IsInvois == true && akPV.IsTanggungan == false && akPV.IsAkru == true && akPV.AkPVInvois != null && akPV.AkPVInvois.Count > 1)
            {
                bool result = false;
                foreach (var invois in akPV.AkPVInvois)
                {
                    if (invois.IsTanggungan == false && invois.AkBelian != null && invois.AkBelian.AkAkaunAkruId != null)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                return result;
            }
            else
            {
                return false;
            }
        }

        public bool PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek(AkPV akPV)
        {
            if (akPV.IsInvois == true && akPV.IsTanggungan == true && akPV.IsAkru == true && akPV.AkPVInvois != null && akPV.AkPVInvois.Count > 1)
            {
                bool result = false;
                List<AkPOObjek> poList = new List<AkPOObjek>();
                foreach (var invois in akPV.AkPVInvois)
                {
                    if (invois.IsTanggungan == true && invois.AkBelian != null && invois.AkBelian.AkAkaunAkruId != null && invois.AkBelian.AkPOId != null && invois.AkBelian.AkIndenId == null && invois.AkBelian.AkPO != null && invois.AkBelian.AkPO.AkPOObjek != null && invois.AkBelian.AkPO.AkPOObjek.Count > 0)
                    {
                        // each insert poObjekList into poList
                        poList.AddRange(invois.AkBelian.AkPO.AkPOObjek);

                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }

                if (result == true)
                {
                    poList = poList.GroupBy(x => x.AkCartaId)
                        .Select(l => new AkPOObjek()
                        {
                            AkCartaId = l.First().AkCartaId,
                            JKWPTJBahagianId = l.First().JKWPTJBahagianId,
                            Counter = l.Count()
                        }).ToList();

                    if (poList != null && poList.Count > 0)
                    {
                        foreach (var po in poList)
                        {
                            if (po.Counter > 1)
                            {
                                result = true;
                            }
                            else
                            {
                                result = false;
                            }
                        }
                    }
                }

                return result;
            }
            else
            {
                return false;
            }
        }

        public bool PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneDifferentObjek(AkPV akPV)
        {
            if (akPV.IsInvois == true && akPV.IsTanggungan == true && akPV.IsAkru == true && akPV.AkPVInvois != null && akPV.AkPVInvois.Count > 1)
            {
                bool result = false;
                List<AkPOObjek> poList = new List<AkPOObjek>();
                foreach (var invois in akPV.AkPVInvois)
                {
                    if (invois.IsTanggungan == true && invois.AkBelian != null && invois.AkBelian.AkAkaunAkruId != null && invois.AkBelian.AkPOId != null && invois.AkBelian.AkIndenId == null && invois.AkBelian.AkPO != null && invois.AkBelian.AkPO.AkPOObjek != null && invois.AkBelian.AkPO.AkPOObjek.Count > 0)
                    {
                        // each insert poObjekList into poList
                        poList.AddRange(invois.AkBelian.AkPO.AkPOObjek);

                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }

                if (result == true)
                {
                    poList = poList.GroupBy(x => x.AkCartaId)
                        .Select(l => new AkPOObjek()
                        {
                            AkCartaId = l.First().AkCartaId,
                            JKWPTJBahagianId = l.First().JKWPTJBahagianId,
                            Counter = l.Count()
                        }).ToList();

                    if (poList != null && poList.Count > 0)
                    {
                        foreach (var po in poList)
                        {
                            if (po.Counter == 1)
                            {
                                result = true;
                                break;
                            }
                            else
                            {
                                result = false;
                            }
                        }
                    }
                }

                return result;
            }
            else
            {
                return false;
            }
        }
    }
}
