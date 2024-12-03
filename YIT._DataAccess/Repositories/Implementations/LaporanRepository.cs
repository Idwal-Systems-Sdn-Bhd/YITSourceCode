using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
    public class LaporanRepository : ILaporanRepository
    {
        public readonly ApplicationDbContext _context;
        public LaporanRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        // Buku Tunai Based on Bank
        public async Task<List<_AkBukuTunai>> GetAkBukuTunaiBasedOnBank(int akBankId, int? JKWId, int? JPTJId, DateTime? TarMula, DateTime? TarHingga, string? Tahun1)
        {
            var bukuTunaiList = new List<_AkBukuTunai>();
            // cari baki bawa ke hadapan
            decimal previousBalance = await GetCarryPreviousBalanceBasedOnStartingDate(akBankId, JKWId, JPTJId, TarMula, Tahun1);

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

            var bukuTunaiSemasaList = await GetListBukuTunaiBasedOnRangeDateYear(akBankId, JKWId, JPTJId, TarMula, TarHingga, Tahun1);

            bukuTunaiList.AddRange(bukuTunaiSemasaList);

            return bukuTunaiList.OrderBy(b => b.KeluarMasuk).ThenBy(b => b.TarikhMasuk).ThenBy(b => b.TarikhKeluar).ToList();
        }

        public async Task<decimal> GetCarryPreviousBalanceBasedOnStartingDate(int akBankId, int? JKWId, int? JPTJId, DateTime? TarMula, string? Tahun1)
        {
            decimal previousBalance = 0;

            if (TarMula != null)
            {
                var akBank = await _context.AkBank.Include(b => b.AkCarta).FirstOrDefaultAsync(b => b.Id == akBankId);

                if (akBank != null)
                {

                    List<AkAkaun> akAkaun = await _context.AkAkaun.Where(b => b.AkCarta1Id == akBank.AkCartaId && b.Tarikh < TarMula).ToListAsync();

                    if (JKWId != null)
                    {
                        akAkaun = akAkaun.Where(b => b.JKWId == JKWId).ToList();
                    }

                    if (JPTJId != null)
                    {
                        akAkaun = akAkaun.Where(b => b.JPTJId == JPTJId).ToList();
                    }

                    if (Tahun1 != null)
                    {
                        akAkaun = akAkaun.Where(b => b.Tarikh.Year == int.Parse(Tahun1)).ToList();
                    }



                    foreach (var item in akAkaun)
                    {
                        previousBalance = previousBalance + item.Debit - item.Kredit;
                    }
                }
            }

            return previousBalance;
        }

        public async Task<List<_AkBukuTunai>> GetListBukuTunaiBasedOnRangeDateYear(int akBankId, int? JKWId, int? JPTJId, DateTime? TarMula, DateTime? TarHingga, string? Tahun1)
        {
            var bukuTunai = new List<_AkBukuTunai>();

            if (TarMula != null && TarHingga != null && Tahun1 != null)
            {
                // search CartaId from AkBankId
                var akBank = await _context.AkBank.Where(b => b.Id == akBankId).FirstOrDefaultAsync();

                if (akBank != null)
                {
                    // PV
                    List<AkAkaun> bukuTunaiPV = await _context.AkAkaun
                    .Include(b => b.AkCarta2)
                    .Where(b => b.NoRujukan!.Contains(EnInitNoRujukan.PV.GetDisplayName())
                    && b.Tarikh >= TarMula && b.Tarikh <= TarHingga
                    && b.Tarikh.Year == int.Parse(Tahun1)
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
                    List<AkAkaun> bukuTunaiResit = await _context.AkAkaun
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
                    List<AkAkaun> bukuTunaiJurnal = await _context.AkAkaun
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
        // Buku Tunai based on Bank end



        // Buku Tunai Based on JKW
        public async Task<List<_AkBukuTunai>> GetAkBukuTunaiBasedOnKW(int? JKWId, int? JPTJId, DateTime? TarMula, DateTime? TarHingga, string? Tahun1)
        {
            var bukuTunaiList = new List<_AkBukuTunai>();
            // cari baki bawa ke hadapan
            decimal previousBalance = await GetCarryPreviousBalanceBasedOnStartingDateKW(JKWId, JPTJId, TarMula, Tahun1);

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

            var bukuTunaiSemasaList = await GetListBukuTunaiBasedOnRangeDateYearKW(JKWId, JPTJId, TarMula, TarHingga, Tahun1);

            bukuTunaiList.AddRange(bukuTunaiSemasaList);

            return bukuTunaiList.OrderBy(b => b.KeluarMasuk).ThenBy(b => b.TarikhMasuk).ThenBy(b => b.TarikhKeluar).ToList();
        }

        public async Task<decimal> GetCarryPreviousBalanceBasedOnStartingDateKW(int? JKWId, int? JPTJId, DateTime? TarMula, string? Tahun1)
        {
            decimal previousBalance = 0;

            if (TarMula != null)
            {
                var jkw = await _context.JKW.FirstOrDefaultAsync(b => b.Id == JKWId);

                if (jkw != null)
                {

                    List<AkAkaun> akAkaun = await _context.AkAkaun.Where(b => b.Tarikh.Year == int.Parse(Tahun1!) && b.Tarikh < TarMula).ToListAsync();

                    if (JKWId != null)
                    {
                        akAkaun = akAkaun.Where(b => b.JKWId == JKWId).ToList();
                    }

                    if (JPTJId != null)
                    {
                        akAkaun = akAkaun.Where(b => b.JPTJId == JPTJId).ToList();
                    }

                    if (Tahun1 != null)
                    {
                        akAkaun = akAkaun.Where(b => b.Tarikh.Year == int.Parse(Tahun1)).ToList();
                    }



                    foreach (var item in akAkaun)
                    {
                        previousBalance = previousBalance + item.Debit - item.Kredit;
                    }
                }
            }

            return previousBalance;
        }

        public async Task<List<_AkBukuTunai>> GetListBukuTunaiBasedOnRangeDateYearKW(int? JKWId, int? JPTJId, DateTime? TarMula, DateTime? TarHingga, string? Tahun1)
        {
            var bukuTunai = new List<_AkBukuTunai>();

            if (TarMula != null && TarHingga != null && Tahun1 != null)
            {
                // search CartaId from AkBankId
                var jkw = await _context.JKW.Where(b => b.Id == JKWId).FirstOrDefaultAsync();

                if (jkw != null)
                {
                    // PV
                    List<AkAkaun> bukuTunaiPV = await _context.AkAkaun
                    .Include(b => b.AkCarta2)
                    .Where(b => b.NoRujukan!.Contains(EnInitNoRujukan.PV.GetDisplayName())
                    && b.Tarikh >= TarMula && b.Tarikh <= TarHingga
                    && b.Tarikh.Year == int.Parse(Tahun1)
                    && b.JKWId == JKWId
                    && b.Kredit != 0)
                    .OrderBy(b => b.Tarikh).ToListAsync();

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
                    List<AkAkaun> bukuTunaiResit = await _context.AkAkaun
                        .Include(b => b.AkCarta2)
                        .Where(b => b.NoRujukan!.Contains(EnInitNoRujukan.RR.GetDisplayName())
                        && b.Tarikh >= TarMula && b.Tarikh <= TarHingga
                        && b.Tarikh.Year == int.Parse(Tahun1)
                        && b.Debit != 0)
                        .OrderBy(b => b.Tarikh).ToListAsync();

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
                    List<AkAkaun> bukuTunaiJurnal = await _context.AkAkaun
                        .Include(b => b.AkCarta2)
                        .Where(b => b.NoRujukan!.Contains(EnInitNoRujukan.JU.GetDisplayName())
                        && b.Tarikh >= TarMula && b.Tarikh <= TarHingga
                        && b.Tarikh.Year == int.Parse(Tahun1))
                        .OrderBy(b => b.Tarikh).ToListAsync();

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
        // Buku Tunai based on JKW end


        public async Task<List<LAK004PrintModel>> GetAbWaranBasedOnYear(int? JKWId, int? JPTJId, string? Tahun1, int? JBahagianId)
        {

            List<LAK004PrintModel> abWaranResult = new List<LAK004PrintModel>();


            if (Tahun1 != null)
            {
                List<AbWaran> abWaran = await _context.AbWaran
                    .Include(a => a.AbWaranObjek!)
                    .ThenInclude(aw => aw.AkCarta)
                    .ThenInclude(ac => ac!.AbBukuVot!)
                    .Include(a => a.AbWaranObjek!)
                    .ThenInclude(a => a.JKWPTJBahagian)
                    .Where(a => a.Tahun == Tahun1)
                .ToListAsync();

                //if (!abWaran.Any())
                //{
                //    return abWaranResult;
                //}

                if (JKWId.HasValue)
                {
                    abWaran = abWaran.Where(a => a.JKWId == JKWId.Value).ToList();

                }

                if (JBahagianId.HasValue)
                {
                    abWaran = abWaran
                        .Where(a => a.AbWaranObjek!
                            .Any(obj => obj.JKWPTJBahagian != null && obj.JKWPTJBahagian.JBahagianId == JBahagianId.Value))
                        .ToList();

                }

                if (JPTJId.HasValue)
                {
                    abWaran = abWaran
                        .Where(a => a.AbWaranObjek!
                            .Any(obj => obj.JKWPTJBahagian != null && obj.JKWPTJBahagian.JPTJId == JPTJId.Value))
                        .ToList();

                }


                foreach (var a in abWaran)
                {
                    if (a.AbWaranObjek != null)
                    {
                        foreach (var b in a.AbWaranObjek)
                        {
                            if (b.AkCarta != null)
                            {
                                var bukuVot = _context.AbBukuVot
                                .Where(v => v.Id == b.Id)
                                .ToList();

                                var pindahanPeruntukan = 0m; // Default value
                                if (b.TK == "+")
                                {
                                    pindahanPeruntukan = b.Amaun;
                                }
                                else if (b.TK == "-")
                                {
                                    pindahanPeruntukan = -b.Amaun;
                                }

                                

                                switch (a.EnJenisPeruntukan)
                                {
                                    case EnJenisPeruntukan.PeruntukanAsal:
                                        abWaranResult.Add(new LAK004PrintModel
                                        {
                                            Kod = b.AkCarta?.Kod,
                                            Perihal = b.AkCarta?.Perihal,
                                            PeruntukanAsal = b.Amaun,
                                            PeruntukanTambahan = 0,
                                            PindahanPeruntukan = 0,
                                            JumlahPeruntukan = a.Jumlah,
                                            PeruntukanTelahGuna = bukuVot.Sum(v => v.Tbs) + bukuVot.Sum(v => v.Belanja),
                                            TelahGunaPercentage = a.Jumlah != 0 ? ((bukuVot.Sum(v => v.Tbs) + bukuVot.Sum(v => v.Belanja)) / a.Jumlah) * 100 : 0,
                                            TBS = bukuVot.Sum(v => v.Tbs),
                                            TBSPercentage = a.Jumlah != 0 ? (bukuVot.Sum(v => v.Tbs) / a.Jumlah) * 100 : 0,
                                            PerbelanjaanBersih = bukuVot.Sum(v => v.Belanja),
                                            BelanjaBersihPercentage = a.Jumlah != 0 ? (bukuVot.Sum(v => v.Belanja) / a.Jumlah) * 100 : 0,
                                            Baki = bukuVot.Sum(v => v.Baki)
                                        });
                                        break;

                                    case EnJenisPeruntukan.PeruntukanTambahan:
                                        abWaranResult.Add(new LAK004PrintModel
                                        {
                                            Kod = b.AkCarta?.Kod,
                                            Perihal = b.AkCarta?.Perihal,
                                            PeruntukanAsal = 0,
                                            PeruntukanTambahan = b.Amaun,
                                            PindahanPeruntukan = 0,
                                            JumlahPeruntukan = a.Jumlah,
                                            PeruntukanTelahGuna = bukuVot.Sum(v => v.Tbs) + bukuVot.Sum(v => v.Belanja),
                                            TelahGunaPercentage = a.Jumlah != 0 ? ((bukuVot.Sum(v => v.Tbs) + bukuVot.Sum(v => v.Belanja)) / a.Jumlah) * 100 : 0,
                                            TBS = bukuVot.Sum(v => v.Tbs),
                                            TBSPercentage = a.Jumlah != 0 ? (bukuVot.Sum(v => v.Tbs) / a.Jumlah) * 100 : 0,
                                            PerbelanjaanBersih = bukuVot.Sum(v => v.Belanja),
                                            BelanjaBersihPercentage = a.Jumlah != 0 ? (bukuVot.Sum(v => v.Belanja) / a.Jumlah) * 100 : 0,
                                            Baki = bukuVot.Sum(v => v.Baki)
                                        });
                                        break;

                                    case EnJenisPeruntukan.Viremen:
                                        abWaranResult.Add(new LAK004PrintModel
                                        {
                                            Kod = b.AkCarta?.Kod,
                                            Perihal = b.AkCarta?.Perihal,
                                            PeruntukanAsal = 0,
                                            PeruntukanTambahan = 0,
                                            PindahanPeruntukan = pindahanPeruntukan,
                                            JumlahPeruntukan = a.Jumlah,
                                            PeruntukanTelahGuna = bukuVot.Sum(v => v.Tbs) + bukuVot.Sum(v => v.Belanja),
                                            TelahGunaPercentage = a.Jumlah != 0 ? ((bukuVot.Sum(v => v.Tbs) + bukuVot.Sum(v => v.Belanja)) / a.Jumlah) * 100 : 0,
                                            TBS = bukuVot.Sum(v => v.Tbs),
                                            TBSPercentage = a.Jumlah != 0 ? (bukuVot.Sum(v => v.Tbs) / a.Jumlah) * 100 : 0,
                                            PerbelanjaanBersih = bukuVot.Sum(v => v.Belanja),
                                            BelanjaBersihPercentage = a.Jumlah != 0 ? (bukuVot.Sum(v => v.Belanja) / a.Jumlah) * 100 : 0,
                                            Baki = bukuVot.Sum(v => v.Baki)
                                        });
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            return abWaranResult.GroupBy(b => new { b.Kod, b.Perihal, b.enJenisPeruntukan })
                .Select(g =>
                {
                    var totalPeruntukanAsal = g.Sum(b => b.PeruntukanAsal);
                    var totalPeruntukanTambahan = g.Sum(b => b.PeruntukanTambahan);
                    var totalPindahanPeruntukan = g.Sum(b => b.PindahanPeruntukan);
                    var jumlahPeruntukan = totalPeruntukanAsal + totalPeruntukanTambahan + totalPindahanPeruntukan;

                    return new LAK004PrintModel

                    {
                        Kod = g.First().Kod,
                        Perihal = g.First().Perihal,
                        PeruntukanAsal = totalPeruntukanAsal,
                        PeruntukanTambahan = totalPeruntukanTambahan,
                        PindahanPeruntukan = totalPindahanPeruntukan,
                        JumlahPeruntukan = jumlahPeruntukan,
                        PeruntukanTelahGuna = g.Sum(b => b.PeruntukanTelahGuna),
                        TBS = g.Sum(b => b.TBS),
                        TBSPercentage = jumlahPeruntukan != 0 ? (g.Sum(b => b.TBS) / jumlahPeruntukan) * 100 : 0,
                        PerbelanjaanBersih = g.Sum(b => b.PerbelanjaanBersih),
                        BelanjaBersihPercentage = jumlahPeruntukan != 0 ? (g.Sum(b => b.PerbelanjaanBersih) / jumlahPeruntukan) * 100 : 0,
                        Baki = jumlahPeruntukan - g.Sum(b => b.PeruntukanTelahGuna)
                    };
                }).OrderBy(b => b.Kod).ToList();

        }



        public async Task<List<LAK004PrintModel>> GetAbWaranBasedOnYearAndMonth(int? JKWId, int? JPTJId, string? Tahun1, string? Bulan, int? JBahagianId)
        {

            List<LAK004PrintModel> abWaranResult = new List<LAK004PrintModel>();

            if (Tahun1 != null && Bulan != null)
            {
                List<AbBukuVot> abBukuVot = await _context.AbBukuVot
                    .Include(a => a.Vot)
                    .ThenInclude(a => a!.AbWaranObjek!)
                    .ThenInclude(a => a.AbWaran)
                    .ThenInclude(a => a!.AbWaranObjek!)
                    .ThenInclude(a => a.AkCarta)
                    .Where(a => a.Tahun == Tahun1 && a.Tarikh.Month == int.Parse(Bulan!))
                    .ToListAsync();

                if (JKWId != null)
                {
                    abBukuVot = abBukuVot.Where(a => a.JKWId == JKWId).ToList();
                }

                if (JPTJId != null)
                {
                    abBukuVot = abBukuVot.Where(a => a.JPTJId == JPTJId).ToList();
                }

                if (JBahagianId.HasValue)
                {
                    abBukuVot = abBukuVot.Where(a => a.JBahagianId == JBahagianId).ToList();

                }

                foreach (var a in abBukuVot)
                {
                    if (a.Vot?.AbWaranObjek != null)
                    {
                        foreach (var b in a.Vot?.AbWaranObjek!)
                        {
                            var filteredBukuVot = b.AkCarta?.AbBukuVot!
                                   .Where(c => c.Tarikh.Month == int.Parse(Bulan))
                                   .ToList();

                            var perbelanjaanTerkumpul = b.AkCarta?.AbBukuVot!
                                .Where(c => c.Tahun == Tahun1)
                                .Sum(c => c.Belanja);

                            var perbelanjaanBulanIni = filteredBukuVot?.Sum(c => c.Belanja);

                            if (b?.AkCarta != null && b.AbWaran != null)
                            {
                              
                                switch (b.AbWaran?.EnJenisPeruntukan)
                                {

                                    case EnJenisPeruntukan.PeruntukanAsal:
                                        abWaranResult.Add(new LAK004PrintModel
                                        {
                                            Kod = b.AkCarta?.Kod,
                                            Perihal = b.AkCarta?.Perihal,
                                            PeruntukanAsal = b.Amaun,
                                            PeruntukanTambahan = 0,
                                            PindahanPeruntukan = 0,
                                            JumlahPeruntukan = b.AbWaran?.Jumlah,
                                            PeruntukanTelahGuna = b.Amaun,
                                            TelahGunaPercentage = b.AbWaran?.Jumlah != 0 ? (b.Amaun / b.AbWaran?.Jumlah) * 100 : 0,
                                            TBS = a.Tanggungan,
                                            TBSPercentage = b.AbWaran?.Jumlah != 0 ? (a.Tanggungan / b.AbWaran?.Jumlah) * 100 : 0,
                                            PerbelanjaanBulanIni = perbelanjaanBulanIni,
                                            PerbelanjaanTerkumpul = perbelanjaanTerkumpul,
                                            BelanjaTerkumpulPercentage = b.AbWaran?.Jumlah != 0 ? (perbelanjaanTerkumpul / b.AbWaran?.Jumlah) * 100 : 0,
                                            Baki = a.Baki,
                                            BakiPercentage = b.AbWaran?.Jumlah != 0 ? (a.Baki / b.AbWaran?.Jumlah) * 100 : 0
                                        });
                                        break;

                                    case EnJenisPeruntukan.PeruntukanTambahan:
                                        abWaranResult.Add(new LAK004PrintModel
                                        {
                                            Kod = b.AkCarta?.Kod,
                                            Perihal = b.AkCarta?.Perihal,
                                            PeruntukanAsal = 0,
                                            PeruntukanTambahan = b.Amaun,
                                            PindahanPeruntukan = 0,
                                            JumlahPeruntukan = b.AbWaran?.Jumlah,
                                            PeruntukanTelahGuna = b.Amaun,
                                            TelahGunaPercentage = b.AbWaran?.Jumlah != 0 ? (b.Amaun / b.AbWaran?.Jumlah) * 100 : 0,
                                            TBS = a.Tanggungan,
                                            TBSPercentage = b.AbWaran?.Jumlah != 0 ? (a.Tanggungan / b.AbWaran?.Jumlah) * 100 : 0,
                                            PerbelanjaanBulanIni = perbelanjaanBulanIni,
                                            PerbelanjaanTerkumpul = perbelanjaanTerkumpul,
                                            BelanjaTerkumpulPercentage = b.AbWaran?.Jumlah != 0 ? (perbelanjaanTerkumpul / b.AbWaran?.Jumlah) * 100 : 0,
                                            Baki = a.Baki,
                                            BakiPercentage = b.AbWaran?.Jumlah != 0 ? (a.Baki / b.AbWaran?.Jumlah) * 100 : 0
                                        });
                                        break;

                                    case EnJenisPeruntukan.Viremen:
                                        abWaranResult.Add(new LAK004PrintModel
                                        {
                                            Kod = b.AkCarta?.Kod,
                                            Perihal = b.AkCarta?.Perihal,
                                            PeruntukanAsal = 0,
                                            PeruntukanTambahan = 0,
                                            PindahanPeruntukan = b.TK == "+" ? b.Amaun : (b.TK == "-" ? -b.Amaun : 0),
                                            JumlahPeruntukan = b.AbWaran?.Jumlah,
                                            PeruntukanTelahGuna = b.Amaun,
                                            TelahGunaPercentage = b.AbWaran?.Jumlah != 0 ? (b.Amaun / b.AbWaran?.Jumlah) * 100 : 0,
                                            TBS = a.Tanggungan,
                                            TBSPercentage = b.AbWaran?.Jumlah != 0 ? (a.Tanggungan / b.AbWaran?.Jumlah) * 100 : 0,
                                            PerbelanjaanBulanIni = perbelanjaanBulanIni,
                                            PerbelanjaanTerkumpul = perbelanjaanTerkumpul,
                                            BelanjaTerkumpulPercentage = b.AbWaran?.Jumlah != 0 ? (perbelanjaanTerkumpul / b.AbWaran?.Jumlah) * 100 : 0,
                                            Baki = a.Baki,
                                            BakiPercentage = b.AbWaran?.Jumlah != 0 ? (a.Baki / b.AbWaran?.Jumlah) * 100 : 0
                                        });
                                        break;

                                }
                            }
                        }
                    }
                }
            }

            return abWaranResult.GroupBy(b => new { b.Kod, b.Perihal, b.enJenisPeruntukan })
                .Select(g =>
                {
                    var totalPeruntukanAsal = g.Sum(b => b.PeruntukanAsal);
                    var totalPeruntukanTambahan = g.Sum(b => b.PeruntukanTambahan);
                    var totalPindahanPeruntukan = g.Sum(b => b.PindahanPeruntukan);
                    var jumlahPeruntukan = totalPeruntukanAsal + totalPeruntukanTambahan + totalPindahanPeruntukan;

                    return new LAK004PrintModel

                    {
                        Kod = g.First().Kod,
                        Perihal = g.First().Perihal,
                        PeruntukanAsal = totalPeruntukanAsal,
                        PeruntukanTambahan = totalPeruntukanTambahan,
                        PindahanPeruntukan = totalPindahanPeruntukan,
                        JumlahPeruntukan = jumlahPeruntukan,
                        PeruntukanTelahGuna = g.Sum(b => b.PeruntukanTelahGuna),
                        TBS = g.Sum(b => b.TBS),
                        TBSPercentage = jumlahPeruntukan != 0 ? (g.Sum(b => b.TBS) / jumlahPeruntukan) * 100 : 0,
                        PerbelanjaanBulanIni = g.Sum(b => b.PerbelanjaanBulanIni),
                        PerbelanjaanTerkumpul = g.Sum(b => b.PerbelanjaanTerkumpul),
                        BelanjaTerkumpulPercentage = jumlahPeruntukan != 0 ? (g.Sum(b => b.PerbelanjaanTerkumpul) / jumlahPeruntukan) * 100 : 0,
                        Baki = jumlahPeruntukan - g.Sum(b => b.PeruntukanTelahGuna),
                        BakiPercentage = jumlahPeruntukan != 0 ? (g.Sum(b => b.Baki) / jumlahPeruntukan) * 100 : 0
                    };
                }).OrderBy(b => b.Kod).ToList();
        }


        public async Task<List<LAK006PrintModel>> GetResultPendapatanTahunan(string? Tahun1, int? jKWId, int AkBankId)
        {
            var resultTahunan = new List<LAK006PrintModel>();

            List<LAK006PrintModel> hasil = await GetResultHasil(Tahun1, jKWId, AkBankId);

            List<LAK006PrintModel> belanja = await GetResultBelanja(Tahun1, jKWId, AkBankId);


            resultTahunan.AddRange(hasil);
            resultTahunan.AddRange(belanja);

            List<LAK006PrintModel> resultGabung = new List<LAK006PrintModel>();

            resultGabung = resultGabung.GroupBy(b => new { b.KodAkaun, b.Jenis })
                .Select(l => new LAK006PrintModel
                {
                    Jenis = l.First().Jenis,
                    KodAkaun = l.First().KodAkaun,
                    NamaAkaun = l.First().NamaAkaun,
                    Jan = l.Sum(c => c.Jan),
                    Feb = l.Sum(c => c.Feb),
                    Mac = l.Sum(c => c.Mac),
                    Apr = l.Sum(c => c.Apr),
                    Mei = l.Sum(c => c.Mei),
                    Jun = l.Sum(c => c.Jun),
                    Jul = l.Sum(c => c.Jul),
                    Ogos = l.Sum(c => c.Ogos),
                    Sep = l.Sum(c => c.Sep),
                    Okt = l.Sum(c => c.Okt),
                    Nov = l.Sum(c => c.Nov),
                    Dis = l.Sum(c => c.Dis),
                    Jumlah = l.Sum(c => c.Jumlah)
                }).OrderBy(b => b.KodAkaun).ToList();

            resultTahunan.AddRange(resultGabung);

            return resultTahunan;
        }

        public async Task<List<LAK006PrintModel>> GetResultHasil(string? Tahun1, int? jKWId, int AkBankId)
        {
            List<LAK006PrintModel> HasilResult = new List<LAK006PrintModel>();
            var akBank = await _context.AkBank.Where(b => b.Id == AkBankId).FirstOrDefaultAsync();

            List<AkAkaun> akAkaunList = new List<AkAkaun>();
            if (akBank != null)
            {
                akAkaunList = _context.AkAkaun.Include(b => b.AkCarta2)
                .Where(b => b.AkCarta1Id == akBank.AkCartaId
                && b.Tarikh.Year == int.Parse(Tahun1!)
                && b.Debit != 0).ToList();

                if (jKWId != null)
                {
                    akAkaunList = akAkaunList.Where(b => b.JKWId == jKWId).ToList();
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

                    if (a.Tarikh.Year == int.Parse(Tahun1!))
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


                        if (a.AkCarta2?.EnJenis == EnJenisCarta.Hasil)
                        {

                            var kodAkaun = a.AkCarta2?.Kod ?? "";
                            var namaAkaun = a.AkCarta2?.Perihal ?? "";


                            var jumlah = jan + feb + mac + apr + mei + jun + jul + ogo + sep + okt + nov + dis;


                            HasilResult.Add(
                                new LAK006PrintModel
                                {
                                    Jenis = "H",
                                    KodAkaun = kodAkaun,
                                    NamaAkaun = namaAkaun,
                                    Jan = jan,
                                    Feb = feb,
                                    Mac = mac,
                                    Apr = apr,
                                    Mei = mei,
                                    Jun = jun,
                                    Jul = jul,
                                    Ogos = ogo,
                                    Sep = sep,
                                    Okt = okt,
                                    Nov = nov,
                                    Dis = dis,
                                    Jumlah = jumlah
                                });
                        }
                    }
                }
            }


            return HasilResult
                .Where(b => !string.IsNullOrEmpty(b.KodAkaun))
                .GroupBy(b => new { b.KodAkaun, b.Jenis })
                .Select(l => new LAK006PrintModel
                {
                    Jenis = l.First().Jenis,
                    KodAkaun = l.First().KodAkaun,
                    NamaAkaun = l.First().NamaAkaun,
                    Jan = l.Sum(c => c.Jan),
                    Feb = l.Sum(c => c.Feb),
                    Mac = l.Sum(c => c.Mac),
                    Apr = l.Sum(c => c.Apr),
                    Mei = l.Sum(c => c.Mei),
                    Jun = l.Sum(c => c.Jun),
                    Jul = l.Sum(c => c.Jul),
                    Ogos = l.Sum(c => c.Ogos),
                    Sep = l.Sum(c => c.Sep),
                    Okt = l.Sum(c => c.Okt),
                    Nov = l.Sum(c => c.Nov),
                    Dis = l.Sum(c => c.Dis),
                    Jumlah = l.Sum(c => c.Jumlah)
                })
                .OrderBy(b => b.KodAkaun)
                .ToList();

        }

        public async Task<List<LAK006PrintModel>> GetResultBelanja(string? Tahun1, int? jKWId, int AkBankId)
        {
            List<LAK006PrintModel> BelanjaResult = new List<LAK006PrintModel>();
            var akBank = await _context.AkBank.Where(b => b.Id == AkBankId).FirstOrDefaultAsync();

            List<AkAkaun> akAkaunList = new List<AkAkaun>();
            if (akBank != null)
            {
                akAkaunList = _context.AkAkaun.Include(b => b.AkCarta2)
                .Where(b => b.AkCarta1Id == akBank.AkCartaId
                && b.Tarikh.Year == int.Parse(Tahun1!)
                && b.Kredit != 0).ToList();

                if (jKWId != null)
                {
                    akAkaunList = akAkaunList.Where(b => b.JKWId == jKWId).ToList();
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

                    if (a.Tarikh.Year == int.Parse(Tahun1!))
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


                        if (a.AkCarta2?.EnJenis == EnJenisCarta.Belanja)
                        {

                            var kodAkaun = a.AkCarta2?.Kod ?? "";
                            var namaAkaun = a.AkCarta2?.Perihal ?? "";


                            var jumlah = jan + feb + mac + apr + mei + jun + jul + ogo + sep + okt + nov + dis;


                            BelanjaResult.Add(
                                new LAK006PrintModel
                                {
                                    Jenis = "B",
                                    KodAkaun = kodAkaun,
                                    NamaAkaun = namaAkaun,
                                    Jan = jan,
                                    Feb = feb,
                                    Mac = mac,
                                    Apr = apr,
                                    Mei = mei,
                                    Jun = jun,
                                    Jul = jul,
                                    Ogos = ogo,
                                    Sep = sep,
                                    Okt = okt,
                                    Nov = nov,
                                    Dis = dis,
                                    Jumlah = jumlah
                                });
                        }
                    }
                }
            }


            return BelanjaResult
                .Where(b => !string.IsNullOrEmpty(b.KodAkaun))
                .GroupBy(b => new { b.KodAkaun, b.Jenis })
                .Select(l => new LAK006PrintModel
                {
                    Jenis = l.First().Jenis,
                    KodAkaun = l.First().KodAkaun,
                    NamaAkaun = l.First().NamaAkaun,
                    Jan = l.Sum(c => c.Jan),
                    Feb = l.Sum(c => c.Feb),
                    Mac = l.Sum(c => c.Mac),
                    Apr = l.Sum(c => c.Apr),
                    Mei = l.Sum(c => c.Mei),
                    Jun = l.Sum(c => c.Jun),
                    Jul = l.Sum(c => c.Jul),
                    Ogos = l.Sum(c => c.Ogos),
                    Sep = l.Sum(c => c.Sep),
                    Okt = l.Sum(c => c.Okt),
                    Nov = l.Sum(c => c.Nov),
                    Dis = l.Sum(c => c.Dis),
                    Jumlah = l.Sum(c => c.Jumlah)
                })
                .OrderBy(b => b.KodAkaun)
                .ToList();

        }


        public async Task<List<LAK007PrintModel>> GetResultPendapatanBulananByJumlahTerkumpul(string? Tahun1, string? Bulan, int? jKWId)
        {
            List<LAK007PrintModel> LAK007Result = new List<LAK007PrintModel>();

            if (Bulan != null)
            {
                List<AkAkaun> akAkaunList = await _context.AkAkaun.Include(b => b.AkCarta1)
                .Where(b => b.Tarikh.Year == int.Parse(Tahun1!)
                && b.Tarikh.Month == int.Parse(Bulan)).ToListAsync();

                if (jKWId != null)
                {
                    akAkaunList = akAkaunList.Where(b => b.JKWId == jKWId).ToList();
                }

                foreach (var a in akAkaunList)
                {
                    if (a.AkCarta1?.EnJenis == EnJenisCarta.Hasil)
                    {
                        LAK007Result.Add(new LAK007PrintModel
                        {
                            Jenis = "H",
                            KodAkaun = a.AkCarta1?.Kod,
                            NamaAkaun = a.AkCarta1?.Perihal,
                            Jumlah = a.Kredit
                        });

                    }
                    else if (a.AkCarta1?.EnJenis == EnJenisCarta.Belanja)
                    {
                        LAK007Result.Add(new LAK007PrintModel
                        {
                            Jenis = "B",
                            KodAkaun = a.AkCarta1?.Kod,
                            NamaAkaun = a.AkCarta1?.Perihal,
                            Jumlah = a.Debit
                        });
                    }

                }
            }
            return LAK007Result
               .Where(b => !string.IsNullOrEmpty(b.KodAkaun))
               .GroupBy(b => new { b.KodAkaun, b.Jenis })
               .Select(l => new LAK007PrintModel
               {
                   Jenis = l.First().Jenis,
                   KodAkaun = l.First().KodAkaun,
                   NamaAkaun = l.First().NamaAkaun,
                   Jumlah = l.Sum(x => x.Jumlah)
               })
               .OrderBy(b => b.KodAkaun)
               .ToList();
        }

        public async Task<List<LAK007PrintModel>> GetResultPendapatanBulananByParas(string? Tahun1, string? Bulan, int? jKWId, EnParas enParas)
        {
            List<LAK007PrintModel> LAK007Result = new List<LAK007PrintModel>();

            List<LAK007PrintModel> ParasHasil = await GetHasilPendapatanBulananByParas(Tahun1, Bulan, jKWId, enParas);
            List<LAK007PrintModel> ParasBelanja = await GetBelanjaPendapatanBulananByParas(Tahun1, Bulan, jKWId, enParas);

            LAK007Result.AddRange(ParasHasil);
            LAK007Result.AddRange(ParasBelanja);

            List<LAK007PrintModel> ParasGabung = new List<LAK007PrintModel>();


            ParasGabung = ParasGabung
             .GroupBy(b => new { b.KodAkaun, b.Jenis })
             .Select(l =>
             {

                 var tahunLpsKumpul = l.Sum(b => b.TahunLps_Kumpul);
                 var tahunSmsKumpul = l.Sum(b => b.TahunSms_Kumpul);
                 var tahunSmsPeruntukan = l.Sum(b => b.TahunSms_Peruntukan);

                 return new LAK007PrintModel
                 {
                     Jenis = l.First().Jenis,
                     KodAkaun = l.First().KodAkaun,
                     NamaAkaun = l.First().NamaAkaun,
                     TahunLps_BulanSMS = l.Sum(b => b.TahunLps_BulanSMS),
                     TahunSms_BulanSMS = l.Sum(b => b.TahunSms_BulanSMS),
                     TahunLps_Kumpul = l.Sum(b => b.TahunLps_Kumpul),
                     TahunSms_Kumpul = l.Sum(b => b.TahunSms_Kumpul),
                     TerKumpulPercentage = tahunLpsKumpul != 0 ? (tahunSmsKumpul - tahunLpsKumpul) / tahunLpsKumpul * 100 : 0,
                     TahunSms_Peruntukan = l.Sum(b => b.TahunSms_Peruntukan),
                     PeruntukanPercentage = tahunSmsPeruntukan != 0 ? (tahunSmsKumpul / tahunSmsPeruntukan) * 100 : 0
                 };
             })
             .OrderBy(b => b.KodAkaun)
             .ToList();

            LAK007Result.AddRange(ParasGabung);

            return LAK007Result;

        }

        public async Task<List<LAK007PrintModel>> GetHasilPendapatanBulananByParas(string? Tahun1, string? Bulan, int? jKWId, EnParas enParas)
        {
            List<LAK007PrintModel> LAK007Result = new List<LAK007PrintModel>();

            List<AkCarta> akCarta = await _context.AkCarta.OrderByDescending(a => a.EnParas).ToListAsync();

            List<AkAkaun> akAkaunList = new List<AkAkaun>();

            var currentMonth = int.Parse(Bulan!);
            var currentYear = int.Parse(Tahun1!);

            if (Bulan != null && Tahun1 != null)
            {

                akAkaunList = _context.AkAkaun
                    .Include(b => b.AkCarta1)
                    .ThenInclude(b => b!.AbBukuVot!)
                    .Where(b =>
                        (b.Tarikh.Year == currentYear || b.Tarikh.Year == currentYear - 1) &&
                        b.Tarikh.Month == currentMonth &&
                        b.Kredit != 0 || b.Kredit != 0)
                    .ToList();
            }

            if (jKWId != null)
            {
                akAkaunList = akAkaunList.Where(b => b.JKWId == jKWId).ToList();
            }


            if (akAkaunList != null)
            {
                foreach (var akaun in akAkaunList)
                {
                    var kodAkaun = akaun.AkCarta1?.Kod;
                    var namaAkaun = akaun.AkCarta1?.Perihal;
                    var akCartaItem = new AkCarta();


                    switch (enParas)
                    {
                        case EnParas.Paras1:
                            akCartaItem = akCarta.FirstOrDefault(c => c.Kod!.Substring(0, 2) == kodAkaun!.Substring(0, 2) && c.EnParas == enParas);
                            if (akCartaItem != null)
                            {
                                kodAkaun = akCartaItem.Kod;
                                namaAkaun = akCartaItem.Perihal;
                            }
                            break;
                        case EnParas.Paras2:
                            akCartaItem = akCarta.FirstOrDefault(c => c.Kod!.Substring(0, 3) == kodAkaun!.Substring(0, 3) && c.EnParas == enParas);
                            if (akCartaItem != null)
                            {
                                kodAkaun = akCartaItem.Kod;
                                namaAkaun = akCartaItem.Perihal;
                            }
                            break;
                        case EnParas.Paras3:
                            akCartaItem = akCarta.FirstOrDefault(c => c.Kod!.Substring(0, 4) == kodAkaun!.Substring(0, 4) && c.EnParas == enParas);
                            if (akCartaItem != null)
                            {
                                kodAkaun = akCartaItem.Kod;
                                namaAkaun = akCartaItem.Perihal;
                            }
                            break;
                        case EnParas.Paras4:
                            akCartaItem = akCarta.FirstOrDefault(c => c.Kod!.Substring(0, 6) == kodAkaun!.Substring(0, 6) && c.EnParas == enParas);
                            if (akCartaItem != null)
                            {
                                kodAkaun = akCartaItem.Kod;
                                namaAkaun = akCartaItem.Perihal;
                            }
                            break;
                    }


                    var bulanTahunLepas = akaun.Tarikh.Month == currentMonth && akaun.Tarikh.Year == currentYear - 1;
                    var bulanTahunSemasa = akaun.Tarikh.Month == currentMonth && akaun.Tarikh.Year == currentYear;

                    decimal tahunLpsKumpul = 0;
                    decimal tahunSmsKumpul = 0;


                    if (akaun.AkCarta1?.EnJenis == EnJenisCarta.Hasil)
                    {
                        if (akaun.Tarikh.Year == currentYear - 1 && akaun.Tarikh.Month <= currentMonth)
                        {
                            tahunLpsKumpul += akaun.Kredit;
                        }

                        if (akaun.Tarikh.Year == currentYear && akaun.Tarikh.Month <= currentMonth)
                        {
                            tahunSmsKumpul += akaun.Kredit;
                        }


                        decimal tahunSmsPeruntukan = 0;


                        if (akaun.AkCarta1?.AbBukuVot != null)
                        {
                            foreach (var vot in akaun.AkCarta1.AbBukuVot)
                            {
                                if (vot.Tahun == Tahun1 && vot.Tarikh.Month <= currentMonth)
                                {

                                    tahunSmsPeruntukan = vot.Kredit - vot.Debit;
                                    break;
                                }
                            }
                        }


                        LAK007Result.Add(new LAK007PrintModel
                        {
                            Jenis = "H",
                            KodAkaun = kodAkaun,
                            NamaAkaun = namaAkaun,
                            TahunLps_BulanSMS = bulanTahunLepas ? akaun.Kredit : 0,
                            TahunSms_BulanSMS = bulanTahunSemasa ? akaun.Kredit : 0,
                            TahunLps_Kumpul = tahunLpsKumpul,
                            TahunSms_Kumpul = tahunSmsKumpul,
                            TerKumpulPercentage = tahunLpsKumpul != 0 ? (tahunSmsKumpul - tahunLpsKumpul) / tahunLpsKumpul * 100 : 0,
                            TahunSms_Peruntukan = tahunSmsPeruntukan,
                            PeruntukanPercentage = tahunSmsPeruntukan != 0 ? (tahunSmsKumpul / tahunSmsPeruntukan) * 100 : 0
                        });
                    }


                }
            }

            //}
            //} 

            return LAK007Result
              .GroupBy(b => new { b.KodAkaun, b.Jenis })
              .Select(l =>
              {

                  var tahunLpsKumpul = l.Sum(b => b.TahunLps_Kumpul);
                  var tahunSmsKumpul = l.Sum(b => b.TahunSms_Kumpul);
                  var tahunSmsPeruntukan = l.Sum(b => b.TahunSms_Peruntukan);

                return new LAK007PrintModel
                {
                    Jenis = l.First().Jenis,
                    KodAkaun = l.First().KodAkaun,
                    NamaAkaun = l.First().NamaAkaun,
                    TahunLps_BulanSMS = l.Sum(b => b.TahunLps_BulanSMS),
                    TahunSms_BulanSMS = l.Sum(b => b.TahunSms_BulanSMS),
                    TahunLps_Kumpul = l.Sum(b => b.TahunLps_Kumpul),
                    TahunSms_Kumpul = l.Sum(b => b.TahunSms_Kumpul),
                    TerKumpulPercentage = tahunLpsKumpul != 0 ? (tahunSmsKumpul - tahunLpsKumpul) / tahunLpsKumpul * 100 : 0,
                    TahunSms_Peruntukan = l.Sum(b => b.TahunSms_Peruntukan),
                    PeruntukanPercentage = tahunSmsPeruntukan != 0 ? (tahunSmsKumpul / tahunSmsPeruntukan) * 100 : 0
                };
              })
              .OrderBy(b => b.KodAkaun)
              .ToList();
        }

        public async Task<List<LAK007PrintModel>> GetBelanjaPendapatanBulananByParas(string? Tahun1, string? Bulan, int? jKWId, EnParas enParas)
        {
            List<LAK007PrintModel> LAK007Result = new List<LAK007PrintModel>();

            List<AkCarta> akCarta = await _context.AkCarta.OrderByDescending(a => a.EnParas).ToListAsync();

            List<AkAkaun> akAkaunList = new List<AkAkaun>();

            var currentMonth = int.Parse(Bulan!);
            var currentYear = int.Parse(Tahun1!);

            if (Bulan != null && Tahun1 != null)
            {

                akAkaunList = _context.AkAkaun
                    .Include(b => b.AkCarta1)
                    .ThenInclude(b => b!.AbWaranObjek!)
                    .Where(b =>
                        (b.Tarikh.Year == currentYear || b.Tarikh.Year == currentYear - 1) &&
                        b.Tarikh.Month == currentMonth &&
                        b.Debit != 0 || b.Debit != 0)
                    .ToList();
            }

            if (jKWId != null)
            {
                akAkaunList = akAkaunList.Where(b => b.JKWId == jKWId).ToList();
            }


            if (akAkaunList != null)
            {
                foreach (var akaun in akAkaunList)
                {
                    var kodAkaun = akaun.AkCarta1?.Kod;
                    var namaAkaun = akaun.AkCarta1?.Perihal;
                    var akCartaItem = new AkCarta();


                    switch (enParas)
                    {
                        case EnParas.Paras1:
                            akCartaItem = akCarta.FirstOrDefault(c => c.Kod!.Substring(0, 2) == kodAkaun!.Substring(0, 2) && c.EnParas == enParas);
                            if (akCartaItem != null)
                            {
                                kodAkaun = akCartaItem.Kod;
                                namaAkaun = akCartaItem.Perihal;
                            }
                            break;
                        case EnParas.Paras2:
                            akCartaItem = akCarta.FirstOrDefault(c => c.Kod!.Substring(0, 3) == kodAkaun!.Substring(0, 3) && c.EnParas == enParas);
                            if (akCartaItem != null)
                            {
                                kodAkaun = akCartaItem.Kod;
                                namaAkaun = akCartaItem.Perihal;
                            }
                            break;
                        case EnParas.Paras3:
                            akCartaItem = akCarta.FirstOrDefault(c => c.Kod!.Substring(0, 4) == kodAkaun!.Substring(0, 4) && c.EnParas == enParas);
                            if (akCartaItem != null)
                            {
                                kodAkaun = akCartaItem.Kod;
                                namaAkaun = akCartaItem.Perihal;
                            }
                            break;
                        case EnParas.Paras4:
                            akCartaItem = akCarta.FirstOrDefault(c => c.Kod!.Substring(0, 6) == kodAkaun!.Substring(0, 6) && c.EnParas == enParas);
                            if (akCartaItem != null)
                            {
                                kodAkaun = akCartaItem.Kod;
                                namaAkaun = akCartaItem.Perihal;
                            }
                            break;
                    }


                    var bulanTahunLepas = akaun.Tarikh.Month == currentMonth && akaun.Tarikh.Year == currentYear - 1;
                    var bulanTahunSemasa = akaun.Tarikh.Month == currentMonth && akaun.Tarikh.Year == currentYear;

                    decimal tahunLpsKumpul = 0;
                    decimal tahunSmsKumpul = 0;


                    if (akaun.AkCarta1?.EnJenis == EnJenisCarta.Belanja)
                    {

                        if (akaun.Tarikh.Year == currentYear - 1 && akaun.Tarikh.Month <= currentMonth)
                        {
                            tahunLpsKumpul += akaun.Debit;
                        }
                        if (akaun.Tarikh.Year == currentYear && akaun.Tarikh.Month <= currentMonth)
                        {
                            tahunSmsKumpul += akaun.Debit;
                        }


                        decimal tahunSmsPeruntukan = 0;
                       

                        if (akaun.AkCarta1?.AbBukuVot != null)
                        {
                            foreach (var vot in akaun.AkCarta1.AbBukuVot)
                            {
                                if (vot.Tahun == Tahun1 && vot.Tarikh.Month <= currentMonth)
                                {

                                    tahunSmsPeruntukan = vot.Kredit - vot.Debit;
                                    break;
                                }
                            }
                        }

                        LAK007Result.Add(new LAK007PrintModel
                        {
                            Jenis = "B",
                            KodAkaun = kodAkaun,
                            NamaAkaun = namaAkaun,
                            TahunLps_BulanSMS = bulanTahunLepas ? akaun.Debit : 0,
                            TahunSms_BulanSMS = bulanTahunSemasa ? akaun.Debit : 0,
                            TahunLps_Kumpul = tahunLpsKumpul,
                            TahunSms_Kumpul = tahunSmsKumpul,
                            TerKumpulPercentage = tahunLpsKumpul != 0 ? (tahunSmsKumpul - tahunLpsKumpul) / tahunLpsKumpul * 100 : 0,
                            TahunSms_Peruntukan = tahunSmsPeruntukan,
                            PeruntukanPercentage = tahunSmsPeruntukan != 0 ? (tahunSmsKumpul / tahunSmsPeruntukan) * 100 : 0

                        });
                    }

                }
            }
            //}
            //}

            return LAK007Result
              .GroupBy(b => new { b.KodAkaun, b.Jenis })
              .Select(l =>
              {

                  var tahunLpsKumpul = l.Sum(b => b.TahunLps_Kumpul);
                  var tahunSmsKumpul = l.Sum(b => b.TahunSms_Kumpul);
                  var tahunSmsPeruntukan = l.Sum(b => b.TahunSms_Peruntukan);

                  return new LAK007PrintModel
                  {
                      Jenis = l.First().Jenis,
                      KodAkaun = l.First().KodAkaun,
                      NamaAkaun = l.First().NamaAkaun,
                      TahunLps_BulanSMS = l.Sum(b => b.TahunLps_BulanSMS),
                      TahunSms_BulanSMS = l.Sum(b => b.TahunSms_BulanSMS),
                      TahunLps_Kumpul = l.Sum(b => b.TahunLps_Kumpul),
                      TahunSms_Kumpul = l.Sum(b => b.TahunSms_Kumpul),
                      TerKumpulPercentage = tahunLpsKumpul != 0 ? (tahunSmsKumpul - tahunLpsKumpul) / tahunLpsKumpul * 100 : 0,
                      TahunSms_Peruntukan = l.Sum(b => b.TahunSms_Peruntukan),
                      PeruntukanPercentage = tahunSmsPeruntukan != 0 ? (tahunSmsKumpul / tahunSmsPeruntukan) * 100 : 0
                  };
              })
              .OrderBy(b => b.KodAkaun)
              .ToList();

        }
    }
}


