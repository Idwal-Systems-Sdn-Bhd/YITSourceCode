using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class PenyataRepository : IPenyataRepository
    {
        public readonly ApplicationDbContext context;
        public PenyataRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        // Buku Tunai
        public async Task<List<_AkBukuTunai>> GetAkBukuTunai(int akBankId, int? JKWId, int? JPTJId, DateTime? TarMula, DateTime? TarHingga)
        {
            var bukuTunaiList = new List<_AkBukuTunai>();
            // cari baki bawa ke hadapan
            decimal previousBalance = await GetCarryPreviousBalanceBasedOnStartingDate(akBankId, JKWId, JPTJId, TarMula);

            bukuTunaiList.Add(new _AkBukuTunai()
            {
                TarikhMasuk = null,
                NamaAkaunMasuk = "BAKI BAWA KE HADAPAN",
                NoRujukanMasuk = "",
                AmaunMasuk = previousBalance,
                JumlahMasuk = 0,
                TarikhKeluar = null,
                NamaAkaunKeluar = "",
                AmaunKeluar = 0,
                NoRujukanKeluar = "",
                JumlahKeluar = 0,
                KeluarMasuk = 0
            });

            var bukuTunaiSemasaList = await GetListBukuTunaiBasedOnRangeDate(akBankId, JKWId, JPTJId, TarMula, TarHingga);

            bukuTunaiList.AddRange(bukuTunaiSemasaList);

            return bukuTunaiList.OrderBy(b => b.KeluarMasuk).ThenBy(b => b.TarikhMasuk).ThenBy(b => b.TarikhKeluar).ToList();
        }

        public async Task<decimal> GetCarryPreviousBalanceBasedOnStartingDate(int akBankId, int? JKWId, int? JPTJId, DateTime? TarMula)
        {
            decimal previousBalance = 0;

            if (TarMula != null)
            {
                var akBank = await context.AkBank.Include(b => b.AkCarta).FirstOrDefaultAsync(b => b.Id == akBankId);

                if (akBank != null)
                {

                    List<AkAkaun> akAkaun = await context.AkAkaun.Where(b => b.AkCarta1Id == akBank.AkCartaId && b.Tarikh < TarMula).ToListAsync();

                    if (JKWId != null)
                    {
                        akAkaun = akAkaun.Where(b => b.JKWId == JKWId).ToList();
                    }

                    if (JPTJId != null)
                    {
                        akAkaun = akAkaun.Where(b => b.JPTJId == JPTJId).ToList();
                    }



                    foreach (var item in akAkaun)
                    {
                        previousBalance = previousBalance + item.Debit - item.Kredit;
                    }
                }
            }

            return previousBalance;
        }

        public async Task<List<_AkBukuTunai>> GetListBukuTunaiBasedOnRangeDate(int akBankId, int? JKWId, int? JPTJId, DateTime? TarMula, DateTime? TarHingga)
        {
            var bukuTunai = new List<_AkBukuTunai>();

            if (TarMula != null && TarHingga != null)
            {
                // search CartaId from AkBankId
                var akBank = await context.AkBank.Where(b => b.Id == akBankId).FirstOrDefaultAsync();

                if (akBank != null)
                {
                    // PV
                    List<AkAkaun> bukuTunaiPV = await context.AkAkaun
                    .Include(b => b.AkCarta2)
                    .Where(b => b.NoRujukan!.Contains(EnInitNoRujukan.PV.GetDisplayName())
                    && b.Tarikh >= TarMula && b.Tarikh <= TarHingga
                    && b.AkCarta1Id == akBank.AkCartaId
                    && b.Kredit != 0).OrderBy(b => b.Tarikh).ToListAsync();

                    if (JKWId != null)
                    {
                        bukuTunaiPV = bukuTunaiPV.Where(b => b.JKWId == JKWId).ToList();
                    }

                    if (JPTJId != null)
                    {
                        bukuTunaiPV = bukuTunaiPV.Where(b => b.JBahagianId == JPTJId).ToList();
                    }

                    decimal jumlahKeluar = 0;
                    foreach (var item in bukuTunaiPV)
                    {
                        jumlahKeluar += item.Kredit;

                        bukuTunai.Add(new _AkBukuTunai()
                        {
                            TarikhMasuk = null,
                            NamaAkaunMasuk = "",
                            NoRujukanMasuk = "",
                            AmaunMasuk = 0,
                            JumlahMasuk = 0,
                            TarikhKeluar = item.Tarikh,
                            NamaAkaunKeluar = item.AkCarta2?.Kod + " - " + item.AkCarta2?.Perihal,
                            NoRujukanKeluar = item.NoRujukan,
                            AmaunKeluar = item.Kredit,
                            JumlahKeluar = jumlahKeluar,
                            KeluarMasuk = 1
                        });
                    }
                    // PV end
                    // Terima
                    List<AkAkaun> bukuTunaiResit = await context.AkAkaun
                        .Include(b => b.AkCarta2)
                        .Where(b => b.NoRujukan!.Contains(EnInitNoRujukan.RR.GetDisplayName())
                        && b.Tarikh >= TarMula && b.Tarikh <= TarHingga
                        && b.AkCarta1Id == akBank.AkCartaId
                        && b.Debit != 0).OrderBy(b => b.Tarikh).ToListAsync();

                    if (JKWId != null)
                    {
                        bukuTunaiResit = bukuTunaiResit.Where(b => b.JKWId == JKWId).ToList();
                    }

                    if (JPTJId != null)
                    {
                        bukuTunaiResit = bukuTunaiResit.Where(b => b.JBahagianId == JPTJId).ToList();
                    }

                    decimal jumlahMasuk = 0;
                    foreach (var item in bukuTunaiResit)
                    {
                        jumlahMasuk += item.Debit;

                        bukuTunai.Add(new _AkBukuTunai()
                        {
                            TarikhMasuk = item.Tarikh,
                            NamaAkaunMasuk = item.AkCarta2?.Kod + " - " + item.AkCarta2?.Perihal,
                            NoRujukanMasuk = item.NoRujukan,
                            AmaunMasuk = item.Debit,
                            JumlahMasuk = jumlahMasuk,
                            TarikhKeluar = null,
                            NamaAkaunKeluar = "",
                            NoRujukanKeluar = "",
                            JumlahKeluar = 0,
                            KeluarMasuk = 0
                        });
                    }
                    // Terima end
                    // Jurnal1
                    // refer AkBank, if debit = masuk, if kredit = keluar
                    List<AkAkaun> bukuTunaiJurnal = await context.AkAkaun
                        .Include(b => b.AkCarta2)
                        .Where(b => b.NoRujukan!.Contains(EnInitNoRujukan.JU.GetDisplayName())
                        && b.Tarikh >= TarMula && b.Tarikh <= TarHingga
                        && b.AkCarta1Id == akBank.AkCartaId).OrderBy(b => b.Tarikh).ToListAsync();

                    if (JKWId != null)
                    {
                        bukuTunaiJurnal = bukuTunaiJurnal.Where(b => b.JKWId == JKWId).ToList();
                    }

                    if (JPTJId != null)
                    {
                        bukuTunaiJurnal = bukuTunaiJurnal.Where(b => b.JBahagianId == JPTJId).ToList();
                    }

                    foreach (var item in bukuTunaiJurnal)
                    {

                        jumlahMasuk += item.Debit;
                        jumlahKeluar += item.Kredit;
                        if (item.Debit != 0)
                        {
                            bukuTunai.Add(new _AkBukuTunai()
                            {
                                TarikhMasuk = item.Tarikh,
                                NamaAkaunMasuk = item.AkCarta2?.Kod + " - " + item.AkCarta2?.Perihal,
                                NoRujukanMasuk = item.NoRujukan,
                                AmaunMasuk = item.Debit,
                                JumlahMasuk = jumlahMasuk,
                                TarikhKeluar = null,
                                NamaAkaunKeluar = "",
                                NoRujukanKeluar = "",
                                AmaunKeluar = 0,
                                JumlahKeluar = 0,
                                KeluarMasuk = 0
                            });
                        }
                        else
                        {
                            bukuTunai.Add(new _AkBukuTunai()
                            {
                                TarikhMasuk = null,
                                NamaAkaunMasuk = "",
                                NoRujukanMasuk = "",
                                AmaunMasuk = item.Debit,
                                JumlahMasuk = jumlahMasuk,
                                TarikhKeluar = item.Tarikh,
                                NamaAkaunKeluar = item.AkCarta2?.Kod + " - " + item.AkCarta2?.Perihal,
                                NoRujukanKeluar = item.NoRujukan,
                                AmaunKeluar = item.Kredit,
                                JumlahKeluar = jumlahKeluar,
                                KeluarMasuk = 1
                            });
                        }
                    }
                    // search CartaId from AkBankId

                    // jurnal1 end
                }

            }

            return bukuTunai.OrderBy(b => b.KeluarMasuk).ThenBy(b => b.TarikhMasuk).ThenBy(b => b.TarikhKeluar).ToList();
        }
        // Buku Tunai end

        // Alir Tunai
        public async Task<List<_AkAlirTunai>> GetAkAlirTunai(int akBankId, int? JKWId, int? JPTJId, string Tahun, int jenisAlirTunai)
        {
            var alirTunai = new List<_AkAlirTunai>();
            // find previous balance

            _AkAlirTunai bakiAwal = await GetCarryPreviousBalanceEachStartingMonth(akBankId, JKWId, JPTJId, Tahun);

            alirTunai.Add(bakiAwal);

            List<_AkAlirTunai> tunaiMasuk = await GetListAlirTunaiMasukBasedOnYear(akBankId, JKWId, JPTJId, Tahun, jenisAlirTunai);

            //alirTunai.AddRange(tunaiMasuk);

            List<_AkAlirTunai> tunaiKeluar = await GetListAlirTunaiKeluarBasedOnYear(akBankId, JKWId, JPTJId, Tahun, jenisAlirTunai);

            //alirTunai.AddRange(tunaiKeluar);

            List<_AkAlirTunai> tunaiGabung = new List<_AkAlirTunai>();

            tunaiGabung.AddRange(tunaiMasuk);
            tunaiGabung.AddRange(tunaiKeluar);

            tunaiGabung = tunaiGabung.GroupBy(b => new { b.NoAkaun, b.KeluarMasuk })
                .Select(l => new _AkAlirTunai
                {
                    NoAkaun = l.First().NoAkaun,
                    NamaAkaun = l.First().NamaAkaun,
                    KeluarMasuk = l.First().KeluarMasuk,
                    Jan = l.Sum(c => c.Jan),
                    Feb = l.Sum(c => c.Feb),
                    Mac = l.Sum(c => c.Mac),
                    Apr = l.Sum(c => c.Apr),
                    Mei = l.Sum(c => c.Mei),
                    Jun = l.Sum(c => c.Jun),
                    Jul = l.Sum(c => c.Jul),
                    Ogo = l.Sum(c => c.Ogo),
                    Sep = l.Sum(c => c.Sep),
                    Okt = l.Sum(c => c.Okt),
                    Nov = l.Sum(c => c.Nov),
                    Dis = l.Sum(c => c.Dis),
                    JumAkaun1 = l.Sum(c => c.JumAkaun1)
                }).OrderBy(b => b.NoAkaun).ToList();

            alirTunai.AddRange(tunaiGabung);

            _AkAlirTunai bakiAkhir = new _AkAlirTunai();

            if (bakiAwal != null)
            {
                bakiAkhir.NoAkaun = bakiAwal.NoAkaun;
                bakiAkhir.NamaAkaun = bakiAwal.NamaAkaun;
                bakiAkhir.KeluarMasuk = 3;
                bakiAkhir.Jan = bakiAwal.Feb;
                bakiAkhir.Feb = bakiAwal.Mac;
                bakiAkhir.Mac = bakiAwal.Apr;
                bakiAkhir.Apr = bakiAwal.Mei;
                bakiAkhir.Mei = bakiAwal.Jun;
                bakiAkhir.Jun = bakiAwal.Jul;
                bakiAkhir.Jul = bakiAwal.Ogo;
                bakiAkhir.Ogo = bakiAwal.Sep;
                bakiAkhir.Sep = bakiAwal.Okt;
                bakiAkhir.Okt = bakiAwal.Nov;
                bakiAkhir.Nov = bakiAwal.Dis;
                bakiAkhir.Dis = bakiAwal.JumAkaun1;
                bakiAkhir.JumAkaun1 = bakiAwal.JumAkaun1;

                alirTunai.Add(bakiAkhir);
            }

            return alirTunai;
        }

        public async Task<_AkAlirTunai> GetCarryPreviousBalanceEachStartingMonth(int akBankId, int? JKWId, int? JPTJId, string Tahun)
        {
            List<_AkAlirTunai> bakiAwal = new List<_AkAlirTunai>();

            var akBank = await context.AkBank.Where(b => b.Id == akBankId).FirstOrDefaultAsync();

            DateTime untilDate = new DateTime(int.Parse(Tahun), 12, 31, 23, 59, 59);

            List<AkAkaun> akAkaunList = new List<AkAkaun>();
            if (akBank != null)
            {
                akAkaunList = context.AkAkaun.Include(b => b.AkCarta1)
                .Where(b => b.AkCarta1Id == akBank.AkCartaId
                && b.Tarikh < untilDate
                && b.Debit != 0).ToList();

                decimal amaunJan = 0;
                decimal amaunFeb = 0;
                decimal amaunMac = 0;
                decimal amaunApr = 0;
                decimal amaunMei = 0;
                decimal amaunJun = 0;
                decimal amaunJul = 0;
                decimal amaunOgo = 0;
                decimal amaunSep = 0;
                decimal amaunOkt = 0;
                decimal amaunNov = 0;
                decimal amaunDis = 0;
                decimal amaunJan2 = 0;
                decimal amaunJum = 0;

                // Masuk
                if (JKWId != null)
                {
                    akAkaunList = akAkaunList.Where(b => b.JKWId == JKWId).ToList();
                }



                if (JPTJId != null)
                {
                    akAkaunList = akAkaunList.Where(b => b.JPTJId == JPTJId).ToList();
                }


                foreach (var a in akAkaunList)
                {
                    amaunJan = 0;
                    amaunFeb = 0;
                    amaunMac = 0;
                    amaunApr = 0;
                    amaunMei = 0;
                    amaunJun = 0;
                    amaunJul = 0;
                    amaunOgo = 0;
                    amaunSep = 0;
                    amaunOkt = 0;
                    amaunNov = 0;
                    amaunDis = 0;
                    amaunJan2 = 0;
                    amaunJum = 0;

                    DateTime jan = new DateTime(int.Parse(Tahun), 1, 1, 0, 0, 0);

                    if (a.Tarikh.Year < int.Parse(Tahun))
                    {
                        amaunJan = a.Debit;
                        amaunJum = a.Debit;

                        amaunFeb = a.Debit;
                        amaunMac = a.Debit;
                        amaunApr = a.Debit;
                        amaunMei = a.Debit;
                        amaunJun = a.Debit;
                        amaunJul = a.Debit;
                        amaunOgo = a.Debit;
                        amaunSep = a.Debit;
                        amaunOkt = a.Debit;
                        amaunNov = a.Debit;
                        amaunDis = a.Debit;
                        amaunJan2 = a.Debit;
                    }
                    DateTime feb = new DateTime(int.Parse(Tahun), 2, 1, 0, 0, 0);
                    if (a.Tarikh.Month < feb.Month && a.Tarikh.Year == int.Parse(Tahun))
                    {
                        amaunFeb = a.Debit;
                        amaunJum = a.Debit;

                        amaunMac = a.Debit;
                        amaunApr = a.Debit;
                        amaunMei = a.Debit;
                        amaunJun = a.Debit;
                        amaunJul = a.Debit;
                        amaunOgo = a.Debit;
                        amaunSep = a.Debit;
                        amaunOkt = a.Debit;
                        amaunNov = a.Debit;
                        amaunDis = a.Debit;
                        amaunJan2 = a.Debit;
                    }
                    DateTime mac = new DateTime(int.Parse(Tahun), 3, 1, 0, 0, 0);
                    if (a.Tarikh.Month < mac.Month && a.Tarikh.Year == int.Parse(Tahun))
                    {
                        amaunMac = a.Debit;
                        amaunJum = a.Debit;

                        amaunApr = a.Debit;
                        amaunMei = a.Debit;
                        amaunJun = a.Debit;
                        amaunJul = a.Debit;
                        amaunOgo = a.Debit;
                        amaunSep = a.Debit;
                        amaunOkt = a.Debit;
                        amaunNov = a.Debit;
                        amaunDis = a.Debit;
                        amaunJan2 = a.Debit;
                    }
                    DateTime apr = new DateTime(int.Parse(Tahun), 4, 1, 0, 0, 0);
                    if (a.Tarikh.Month < apr.Month && a.Tarikh.Year == int.Parse(Tahun))
                    {
                        amaunApr = a.Debit;
                        amaunJum = a.Debit;

                        amaunMei = a.Debit;
                        amaunJun = a.Debit;
                        amaunJul = a.Debit;
                        amaunOgo = a.Debit;
                        amaunSep = a.Debit;
                        amaunOkt = a.Debit;
                        amaunNov = a.Debit;
                        amaunDis = a.Debit;
                        amaunJan2 = a.Debit;
                    }
                    DateTime mei = new DateTime(int.Parse(Tahun), 5, 1, 0, 0, 0);
                    if (a.Tarikh.Month < mei.Month && a.Tarikh.Year == int.Parse(Tahun))
                    {
                        amaunMei = a.Debit;
                        amaunJum = a.Debit;

                        amaunJun = a.Debit;
                        amaunJul = a.Debit;
                        amaunOgo = a.Debit;
                        amaunSep = a.Debit;
                        amaunOkt = a.Debit;
                        amaunNov = a.Debit;
                        amaunDis = a.Debit;
                        amaunJan2 = a.Debit;
                    }
                    DateTime jun = new DateTime(int.Parse(Tahun), 6, 1, 0, 0, 0);
                    if (a.Tarikh.Month < jun.Month && a.Tarikh.Year == int.Parse(Tahun))
                    {
                        amaunJun = a.Debit;
                        amaunJum = a.Debit;

                        amaunJul = a.Debit;
                        amaunOgo = a.Debit;
                        amaunSep = a.Debit;
                        amaunOkt = a.Debit;
                        amaunNov = a.Debit;
                        amaunDis = a.Debit;
                        amaunJan2 = a.Debit;
                    }
                    DateTime jul = new DateTime(int.Parse(Tahun), 7, 1, 0, 0, 0);
                    if (a.Tarikh.Month < jul.Month && a.Tarikh.Year == int.Parse(Tahun))
                    {
                        amaunJul = a.Debit;
                        amaunJum = a.Debit;

                        amaunOgo = a.Debit;
                        amaunSep = a.Debit;
                        amaunOkt = a.Debit;
                        amaunNov = a.Debit;
                        amaunDis = a.Debit;
                        amaunJan2 = a.Debit;
                    }
                    DateTime ogo = new DateTime(int.Parse(Tahun), 8, 1, 0, 0, 0);
                    if (a.Tarikh.Month < ogo.Month && a.Tarikh.Year == int.Parse(Tahun))
                    {
                        amaunOgo = a.Debit;
                        amaunJum = a.Debit;

                        amaunSep = a.Debit;
                        amaunOkt = a.Debit;
                        amaunNov = a.Debit;
                        amaunDis = a.Debit;
                        amaunJan2 = a.Debit;
                    }
                    DateTime sep = new DateTime(int.Parse(Tahun), 9, 1, 0, 0, 0);
                    if (a.Tarikh.Month < sep.Month && a.Tarikh.Year == int.Parse(Tahun))
                    {
                        amaunSep = a.Debit;
                        amaunJum = a.Debit;

                        amaunOkt = a.Debit;
                        amaunNov = a.Debit;
                        amaunDis = a.Debit;
                        amaunJan2 = a.Debit;
                    }
                    DateTime okt = new DateTime(int.Parse(Tahun), 10, 1, 0, 0, 0);
                    if (a.Tarikh.Month < okt.Month && a.Tarikh.Year == int.Parse(Tahun))
                    {
                        amaunOkt = a.Debit;
                        amaunJum = a.Debit;

                        amaunNov = a.Debit;
                        amaunDis = a.Debit;
                        amaunJan2 = a.Debit;
                    }
                    DateTime nov = new DateTime(int.Parse(Tahun), 11, 1, 0, 0, 0);
                    if (a.Tarikh.Month < nov.Month && a.Tarikh.Year == int.Parse(Tahun))
                    {
                        amaunNov = a.Debit;
                        amaunJum = a.Debit;

                        amaunDis = a.Debit;
                        amaunJan2 = a.Debit;
                    }
                    DateTime dis = new DateTime(int.Parse(Tahun), 12, 1, 0, 0, 0);
                    if (a.Tarikh.Month < dis.Month && a.Tarikh.Year == int.Parse(Tahun))
                    {
                        amaunDis = a.Debit;
                        amaunJum = a.Debit;
                        amaunJan2 = a.Debit;
                    }
                    DateTime jan2 = new DateTime(int.Parse(Tahun) + 1, 1, 1, 0, 0, 0);
                    if (a.Tarikh.Year < int.Parse(Tahun) + 1)
                    {
                        amaunJan2 = a.Debit;
                        amaunJum = a.Debit;
                    }
                    bakiAwal.Add(new _AkAlirTunai
                    {
                        NoAkaun = a.AkCarta1?.Kod,
                        NamaAkaun = a.AkCarta1?.Perihal,
                        Jan = amaunJan,
                        Feb = amaunFeb,
                        Mac = amaunMac,
                        Apr = amaunApr,
                        Mei = amaunMei,
                        Jun = amaunJun,
                        Jul = amaunJul,
                        Ogo = amaunOgo,
                        Sep = amaunSep,
                        Okt = amaunOkt,
                        Nov = amaunNov,
                        Dis = amaunDis,
                        Jan2 = amaunJan2,
                        JumAkaun1 = amaunJum,
                        KeluarMasuk = 0
                    });
                }
                // Masuk END

                // Keluar
                List<AkAkaun> akAkaunK = context.AkAkaun.Include(b => b.AkCarta1)
                    .Where(b => b.AkCarta1Id == akBank.AkCartaId
                    && b.Tarikh < untilDate
                    && b.Kredit != 0).ToList();

                if (JKWId != null)
                {
                    akAkaunK = akAkaunK.Where(b => b.JKWId == JKWId).ToList();
                }



                if (JPTJId != null)
                {
                    akAkaunK = akAkaunK.Where(b => b.JBahagianId == JPTJId).ToList();
                }

                foreach (var a in akAkaunK)
                {
                    amaunJan = 0;
                    amaunFeb = 0;
                    amaunMac = 0;
                    amaunApr = 0;
                    amaunMei = 0;
                    amaunJun = 0;
                    amaunJul = 0;
                    amaunOgo = 0;
                    amaunSep = 0;
                    amaunOkt = 0;
                    amaunNov = 0;
                    amaunDis = 0;
                    amaunJan2 = 0;
                    amaunJum = 0;

                    DateTime jan = new DateTime(int.Parse(Tahun), 1, 1, 0, 0, 0);

                    if (a.Tarikh.Year < int.Parse(Tahun))
                    {
                        amaunJan = -a.Kredit;
                        amaunJum = -a.Kredit;

                        amaunFeb = -a.Kredit;
                        amaunMac = -a.Kredit;
                        amaunApr = -a.Kredit;
                        amaunMei = -a.Kredit;
                        amaunJun = -a.Kredit;
                        amaunJul = -a.Kredit;
                        amaunOgo = -a.Kredit;
                        amaunSep = -a.Kredit;
                        amaunOkt = -a.Kredit;
                        amaunNov = -a.Kredit;
                        amaunDis = -a.Kredit;
                        amaunJan2 = -a.Kredit;
                    }
                    DateTime feb = new DateTime(int.Parse(Tahun), 2, 1, 0, 0, 0);
                    if (a.Tarikh.Month < feb.Month && a.Tarikh.Year == int.Parse(Tahun))
                    {
                        amaunFeb = -a.Kredit;
                        amaunJum = -a.Kredit;

                        amaunMac = -a.Kredit;
                        amaunApr = -a.Kredit;
                        amaunMei = -a.Kredit;
                        amaunJun = -a.Kredit;
                        amaunJul = -a.Kredit;
                        amaunOgo = -a.Kredit;
                        amaunSep = -a.Kredit;
                        amaunOkt = -a.Kredit;
                        amaunNov = -a.Kredit;
                        amaunDis = -a.Kredit;
                        amaunJan2 = -a.Kredit;
                    }
                    DateTime mac = new DateTime(int.Parse(Tahun), 3, 1, 0, 0, 0);
                    if (a.Tarikh.Month < mac.Month && a.Tarikh.Year == int.Parse(Tahun))
                    {
                        amaunMac = -a.Kredit;
                        amaunJum = -a.Kredit;

                        amaunApr = -a.Kredit;
                        amaunMei = -a.Kredit;
                        amaunJun = -a.Kredit;
                        amaunJul = -a.Kredit;
                        amaunOgo = -a.Kredit;
                        amaunSep = -a.Kredit;
                        amaunOkt = -a.Kredit;
                        amaunNov = -a.Kredit;
                        amaunDis = -a.Kredit;
                        amaunJan2 = -a.Kredit;
                    }
                    DateTime apr = new DateTime(int.Parse(Tahun), 4, 1, 0, 0, 0);
                    if (a.Tarikh.Month < apr.Month && a.Tarikh.Year == int.Parse(Tahun))
                    {
                        amaunApr = -a.Kredit;
                        amaunJum = -a.Kredit;

                        amaunMei = -a.Kredit;
                        amaunJun = -a.Kredit;
                        amaunJul = -a.Kredit;
                        amaunOgo = -a.Kredit;
                        amaunSep = -a.Kredit;
                        amaunOkt = -a.Kredit;
                        amaunNov = -a.Kredit;
                        amaunDis = -a.Kredit;
                        amaunJan2 = -a.Kredit;
                    }
                    DateTime mei = new DateTime(int.Parse(Tahun), 5, 1, 0, 0, 0);
                    if (a.Tarikh.Month < mei.Month && a.Tarikh.Year == int.Parse(Tahun))
                    {
                        amaunMei = -a.Kredit;
                        amaunJum = -a.Kredit;

                        amaunJun = -a.Kredit;
                        amaunJul = -a.Kredit;
                        amaunOgo = -a.Kredit;
                        amaunSep = -a.Kredit;
                        amaunOkt = -a.Kredit;
                        amaunNov = -a.Kredit;
                        amaunDis = -a.Kredit;
                        amaunJan2 = -a.Kredit;
                    }
                    DateTime jun = new DateTime(int.Parse(Tahun), 6, 1, 0, 0, 0);
                    if (a.Tarikh.Month < jun.Month && a.Tarikh.Year == int.Parse(Tahun))
                    {
                        amaunJun = -a.Kredit;
                        amaunJum = -a.Kredit;

                        amaunJul = -a.Kredit;
                        amaunOgo = -a.Kredit;
                        amaunSep = -a.Kredit;
                        amaunOkt = -a.Kredit;
                        amaunNov = -a.Kredit;
                        amaunDis = -a.Kredit;
                        amaunJan2 = -a.Kredit;
                    }
                    DateTime jul = new DateTime(int.Parse(Tahun), 7, 1, 0, 0, 0);
                    if (a.Tarikh.Month < jul.Month && a.Tarikh.Year == int.Parse(Tahun))
                    {
                        amaunJul = -a.Kredit;
                        amaunJum = -a.Kredit;

                        amaunOgo = -a.Kredit;
                        amaunSep = -a.Kredit;
                        amaunOkt = -a.Kredit;
                        amaunNov = -a.Kredit;
                        amaunDis = -a.Kredit;
                        amaunJan2 = -a.Kredit;
                    }
                    DateTime ogo = new DateTime(int.Parse(Tahun), 8, 1, 0, 0, 0);
                    if (a.Tarikh.Month < ogo.Month && a.Tarikh.Year == int.Parse(Tahun))
                    {
                        amaunOgo = -a.Kredit;
                        amaunJum = -a.Kredit;

                        amaunSep = -a.Kredit;
                        amaunOkt = -a.Kredit;
                        amaunNov = -a.Kredit;
                        amaunDis = -a.Kredit;
                        amaunJan2 = -a.Kredit;
                    }
                    DateTime sep = new DateTime(int.Parse(Tahun), 9, 1, 0, 0, 0);
                    if (a.Tarikh.Month < sep.Month && a.Tarikh.Year == int.Parse(Tahun))
                    {
                        amaunSep = -a.Kredit;
                        amaunJum = -a.Kredit;

                        amaunOkt = -a.Kredit;
                        amaunNov = -a.Kredit;
                        amaunDis = -a.Kredit;
                        amaunJan2 = -a.Kredit;
                    }
                    DateTime okt = new DateTime(int.Parse(Tahun), 10, 1, 0, 0, 0);
                    if (a.Tarikh.Month < okt.Month && a.Tarikh.Year == int.Parse(Tahun))
                    {
                        amaunOkt = -a.Kredit;
                        amaunJum = -a.Kredit;

                        amaunNov = -a.Kredit;
                        amaunDis = -a.Kredit;
                        amaunJan2 = -a.Kredit;
                    }
                    DateTime nov = new DateTime(int.Parse(Tahun), 11, 1, 0, 0, 0);
                    if (a.Tarikh.Month < nov.Month && a.Tarikh.Year == int.Parse(Tahun))
                    {
                        amaunNov = -a.Kredit;
                        amaunJum = -a.Kredit;

                        amaunDis = -a.Kredit;
                        amaunJan2 = -a.Kredit;
                    }
                    DateTime dis = new DateTime(int.Parse(Tahun), 12, 1, 0, 0, 0);
                    if (a.Tarikh.Month < dis.Month && a.Tarikh.Year == int.Parse(Tahun))
                    {
                        amaunDis = -a.Kredit;
                        amaunJum = -a.Kredit;
                        amaunJan2 = -a.Kredit;
                    }
                    DateTime jan2 = new DateTime(int.Parse(Tahun) + 1, 1, 1, 0, 0, 0);
                    if (a.Tarikh.Year < int.Parse(Tahun) + 1)
                    {
                        amaunJan2 = -a.Kredit;
                        amaunJum = -a.Kredit;
                    }
                    bakiAwal.Add(new _AkAlirTunai
                    {
                        NoAkaun = a.AkCarta1?.Kod,
                        NamaAkaun = a.AkCarta1?.Perihal,
                        Jan = amaunJan,
                        Feb = amaunFeb,
                        Mac = amaunMac,
                        Apr = amaunApr,
                        Mei = amaunMei,
                        Jun = amaunJun,
                        Jul = amaunJul,
                        Ogo = amaunOgo,
                        Sep = amaunSep,
                        Okt = amaunOkt,
                        Nov = amaunNov,
                        Dis = amaunDis,
                        Jan2 = amaunJan2,
                        JumAkaun1 = amaunJum,
                        KeluarMasuk = 0
                    });
                }
                // Keluar END
            }


            return bakiAwal.GroupBy(b => new { b.NoAkaun, b.NamaAkaun, b.KeluarMasuk })
                .Select(l => new _AkAlirTunai
                {
                    NoAkaun = l.First().NoAkaun,
                    NamaAkaun = l.First().NamaAkaun,
                    KeluarMasuk = l.First().KeluarMasuk,
                    Jan = l.Sum(c => c.Jan),
                    Feb = l.Sum(c => c.Feb),
                    Mac = l.Sum(c => c.Mac),
                    Apr = l.Sum(c => c.Apr),
                    Mei = l.Sum(c => c.Mei),
                    Jun = l.Sum(c => c.Jun),
                    Jul = l.Sum(c => c.Jul),
                    Ogo = l.Sum(c => c.Ogo),
                    Sep = l.Sum(c => c.Sep),
                    Okt = l.Sum(c => c.Okt),
                    Nov = l.Sum(c => c.Nov),
                    Dis = l.Sum(c => c.Dis),
                    Jan2 = l.Sum(c => c.Jan2),
                    JumAkaun1 = l.Sum(c => c.JumAkaun1)
                }).OrderBy(b => b.NoAkaun).FirstOrDefault() ?? new _AkAlirTunai();
        }

        public async Task<List<_AkAlirTunai>> GetListAlirTunaiMasukBasedOnYear(int akBankId, int? JKWId, int? JPTJId, string Tahun, int jenisAlirTunai)
        {
            List<_AkAlirTunai> tunaiMasuk = new List<_AkAlirTunai>();

            var akBank = await context.AkBank.Where(b => b.Id == akBankId).FirstOrDefaultAsync();

            List<AkAkaun> akAkaunList = new List<AkAkaun>();
            if (akBank != null)
            {
                akAkaunList = context.AkAkaun.Include(b => b.AkCarta2)
                .Where(b => b.AkCarta1Id == akBank.AkCartaId
                && b.Tarikh.Year == int.Parse(Tahun)
                && b.Debit != 0).ToList();

                if (JKWId != null)
                {
                    akAkaunList = akAkaunList.Where(b => b.JKWId == JKWId).ToList();
                }

                if (JPTJId != null)
                {
                    akAkaunList = akAkaunList.Where(b => b.JPTJId == JPTJId).ToList();
                }

                decimal jan = 0;
                decimal feb = 0;
                decimal mac = 0;
                decimal apr = 0;
                decimal mei = 0;
                decimal jun = 0;
                decimal jul = 0;
                decimal ogo = 0;
                decimal sep = 0;
                decimal okt = 0;
                decimal nov = 0;
                decimal dis = 0;
                decimal jum = 0;

                foreach (var a in akAkaunList)
                {
                    jan = 0;
                    feb = 0;
                    mac = 0;
                    apr = 0;
                    mei = 0;
                    jun = 0;
                    jul = 0;
                    ogo = 0;
                    sep = 0;
                    okt = 0;
                    nov = 0;
                    dis = 0;
                    jum = a.Debit;

                    if (a.Tarikh.Year == int.Parse(Tahun))
                    {
                        switch (a.Tarikh.Month)
                        {
                            case 1:
                                jan = a.Debit;
                                break;
                            case 2:
                                feb = a.Debit;
                                break;
                            case 3:
                                mac = a.Debit;
                                break;
                            case 4:
                                apr = a.Debit;
                                break;
                            case 5:
                                mei = a.Debit;
                                break;
                            case 6:
                                jun = a.Debit;
                                break;
                            case 7:
                                jul = a.Debit;
                                break;
                            case 8:
                                ogo = a.Debit;
                                break;
                            case 9:
                                sep = a.Debit;
                                break;
                            case 10:
                                okt = a.Debit;
                                break;
                            case 11:
                                nov = a.Debit;
                                break;
                            case 12:
                                dis = a.Debit;
                                break;
                        }

                        int masuk = 1;
                        if (jenisAlirTunai == 1)
                        {
                            masuk = 4; // gabung jenis masuk keluar
                        }

                        tunaiMasuk.Add(
                            new _AkAlirTunai
                            {
                                NoAkaun = a.AkCarta2?.Kod ?? "",
                                NamaAkaun = a.AkCarta2?.Perihal ?? "BAKI AWAL",
                                KeluarMasuk = masuk,
                                Jan = jan,
                                Feb = feb,
                                Mac = mac,
                                Apr = apr,
                                Mei = mei,
                                Jun = jun,
                                Jul = jul,
                                Ogo = ogo,
                                Sep = sep,
                                Okt = okt,
                                Nov = nov,
                                Dis = dis,
                                JumAkaun1 = jum
                            });
                    }
                }
            }
            return tunaiMasuk.GroupBy(b => new { b.NoAkaun })
                    .Select(l => new _AkAlirTunai
                    {
                        NoAkaun = l.First().NoAkaun,
                        NamaAkaun = l.First().NamaAkaun,
                        KeluarMasuk = l.First().KeluarMasuk,
                        Jan = l.Sum(c => c.Jan),
                        Feb = l.Sum(c => c.Feb),
                        Mac = l.Sum(c => c.Mac),
                        Apr = l.Sum(c => c.Apr),
                        Mei = l.Sum(c => c.Mei),
                        Jun = l.Sum(c => c.Jun),
                        Jul = l.Sum(c => c.Jul),
                        Ogo = l.Sum(c => c.Ogo),
                        Sep = l.Sum(c => c.Sep),
                        Okt = l.Sum(c => c.Okt),
                        Nov = l.Sum(c => c.Nov),
                        Dis = l.Sum(c => c.Dis),
                        JumAkaun1 = l.Sum(c => c.JumAkaun1)
                    }).OrderBy(b => b.NoAkaun).ToList();

        }

        public async Task<List<_AkAlirTunai>> GetListAlirTunaiKeluarBasedOnYear(int akBankId, int? JKWId, int? JPTJId, string Tahun, int jenisAlirTunai)
        {
            List<_AkAlirTunai> tunaiKeluar = new List<_AkAlirTunai>();

            var akBank = await context.AkBank.Where(b => b.Id == akBankId).FirstOrDefaultAsync();

            List<AkAkaun> akAkaunList = new List<AkAkaun>();

            if (akBank != null)
            {
                akAkaunList = context.AkAkaun.Include(b => b.AkCarta2)
                .Where(b => b.AkCarta1Id == akBank.AkCartaId
                && b.Tarikh.Year == int.Parse(Tahun)
                && b.Kredit != 0).ToList();

                if (JKWId != null)
                {
                    akAkaunList = akAkaunList.Where(b => b.JKWId == JKWId).ToList();
                }

                if (JPTJId != null)
                {
                    akAkaunList = akAkaunList.Where(b => b.JBahagianId == JPTJId).ToList();
                }

                decimal jan = 0;
                decimal feb = 0;
                decimal mac = 0;
                decimal apr = 0;
                decimal mei = 0;
                decimal jun = 0;
                decimal jul = 0;
                decimal ogo = 0;
                decimal sep = 0;
                decimal okt = 0;
                decimal nov = 0;
                decimal dis = 0;
                decimal jum = 0;

                foreach (var a in akAkaunList)
                {
                    jan = 0;
                    feb = 0;
                    mac = 0;
                    apr = 0;
                    mei = 0;
                    jun = 0;
                    jul = 0;
                    ogo = 0;
                    sep = 0;
                    okt = 0;
                    nov = 0;
                    dis = 0;

                    jum = a.Kredit;

                    if (a.Tarikh.Year == int.Parse(Tahun))
                    {
                        switch (a.Tarikh.Month)
                        {
                            case 1:
                                jan = a.Kredit;
                                break;
                            case 2:
                                feb = a.Kredit;
                                break;
                            case 3:
                                mac = a.Kredit;
                                break;
                            case 4:
                                apr = a.Kredit;
                                break;
                            case 5:
                                mei = a.Kredit;
                                break;
                            case 6:
                                jun = a.Kredit;
                                break;
                            case 7:
                                jul = a.Kredit;
                                break;
                            case 8:
                                ogo = a.Kredit;
                                break;
                            case 9:
                                sep = a.Kredit;
                                break;
                            case 10:
                                okt = a.Kredit;
                                break;
                            case 11:
                                nov = a.Kredit;
                                break;
                            case 12:
                                dis = a.Kredit;
                                break;
                        }


                        int keluar = 2;
                        if (jenisAlirTunai == 1)
                        {
                            keluar = 4; // gabung jenis masuk keluar
                        }

                        if (keluar == 4)
                        {
                            tunaiKeluar.Add(
                                new _AkAlirTunai
                                {
                                    NoAkaun = a.AkCarta2?.Kod,
                                    NamaAkaun = a.AkCarta2?.Perihal,
                                    KeluarMasuk = keluar,
                                    Jan = -jan,
                                    Feb = -feb,
                                    Mac = -mac,
                                    Apr = -apr,
                                    Mei = -mei,
                                    Jun = -jun,
                                    Jul = -jul,
                                    Ogo = -ogo,
                                    Sep = -sep,
                                    Okt = -okt,
                                    Nov = -nov,
                                    Dis = -dis,
                                    JumAkaun1 = -jum
                                });
                        }
                        else
                        {
                            tunaiKeluar.Add(
                                new _AkAlirTunai
                                {
                                    NoAkaun = a.AkCarta2?.Kod,
                                    NamaAkaun = a.AkCarta2?.Perihal,
                                    KeluarMasuk = keluar,
                                    Jan = jan,
                                    Feb = feb,
                                    Mac = mac,
                                    Apr = apr,
                                    Mei = mei,
                                    Jun = jun,
                                    Jul = jul,
                                    Ogo = ogo,
                                    Sep = sep,
                                    Okt = okt,
                                    Nov = nov,
                                    Dis = dis,
                                    JumAkaun1 = jum
                                });
                        }

                    }
                }
            }


            return tunaiKeluar.GroupBy(b => new { b.NoAkaun })
                .Select(l => new _AkAlirTunai
                {
                    NoAkaun = l.First().NoAkaun,
                    NamaAkaun = l.First().NamaAkaun,
                    KeluarMasuk = l.First().KeluarMasuk,
                    Jan = l.Sum(c => c.Jan),
                    Feb = l.Sum(c => c.Feb),
                    Mac = l.Sum(c => c.Mac),
                    Apr = l.Sum(c => c.Apr),
                    Mei = l.Sum(c => c.Mei),
                    Jun = l.Sum(c => c.Jun),
                    Jul = l.Sum(c => c.Jul),
                    Ogo = l.Sum(c => c.Ogo),
                    Sep = l.Sum(c => c.Sep),
                    Okt = l.Sum(c => c.Okt),
                    Nov = l.Sum(c => c.Nov),
                    Dis = l.Sum(c => c.Dis),
                    JumAkaun1 = l.Sum(c => c.JumAkaun1)
                }).OrderBy(b => b.NoAkaun).ToList();
        }

        // Alir Tunai end
    }
}
