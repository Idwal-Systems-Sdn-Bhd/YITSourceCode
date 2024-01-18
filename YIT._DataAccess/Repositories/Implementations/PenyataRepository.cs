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

namespace YIT._DataAccess.Repositories.Implementations
{
    public class PenyataRepository : IPenyataRepository
    {
        public readonly ApplicationDbContext context;
        public PenyataRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

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
    }
}
