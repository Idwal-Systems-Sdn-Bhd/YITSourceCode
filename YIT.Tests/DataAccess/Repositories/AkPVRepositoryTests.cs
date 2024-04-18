using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Implementations;

namespace YIT.Tests.DataAccess.Repositories
{
    public class AkPVRepositoryTests
    {
        public async Task<ApplicationDbContext> GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();
            if (await context.AkPV.CountAsync() <= 0)
            {
                // Jadual
                var jkw = new JKW()
                {
                    Kod = "1",
                    Perihal = "YAYASAN ISLAM TERENGGANU"
                };

                context.Add(jkw);

                var ptj = new JPTJ()
                {
                    Kod = "01",
                    Perihal = "CARUMAN KERAJAAN NEGERI"
                };

                context.Add(ptj);

                var bahagian = new JBahagian()
                {
                    Kod = "01",
                    Perihal = "PENTADBIRAN"
                };

                context.Add(bahagian);

                var caraBayar = new JCaraBayar()
                {
                    Kod = "T",
                    Perihal = "TUNAI"
                };

                context.Add(caraBayar);

                var akCartaBank = new AkCarta()
                {
                    Kod = "A11201",
                    Perihal = "MAYBANK  - 563019249274 (PTD)",
                    DebitKredit = "D",
                    UmumDetail = "D",
                    Baki = decimal.Parse("1233672.61"),
                    EnJenis = EnJenisCarta.Aset,
                    EnParas = EnParas.Paras4
                };
                context.Add(akCartaBank);

                var akCartaBelanja1 = new AkCarta()
                {
                    Kod = "B23102",
                    Perihal = "BAYARAN POS UNTUK SURAT-SURAT",
                    DebitKredit = "D",
                    UmumDetail = "D",
                    Baki = 0,
                    EnJenis = EnJenisCarta.Belanja,
                    EnParas = EnParas.Paras4
                };

                context.Add(akCartaBelanja1);

                var akCartaLiabiliti = new AkCarta()
                {
                    Kod = "L13101",
                    Perihal = "PEMIUTANG PELBAGAI",
                    DebitKredit = "K",
                    UmumDetail = "D",
                    Baki = 0,
                    EnJenis = EnJenisCarta.Liabiliti,
                    EnParas = EnParas.Paras4
                };

                context.Add(akCartaLiabiliti);

                var akCartaBelanja2 = new AkCarta()
                {
                    Kod = "B27101",
                    Perihal = "BEKALAN PEJABAT",
                    DebitKredit = "D",
                    UmumDetail = "D",
                    Baki = 0,
                    EnJenis = EnJenisCarta.Belanja,
                    EnParas = EnParas.Paras4
                };

                context.Add(akCartaBelanja2);

                var jBank = new JBank()
                {
                    Kod = "01",
                    Perihal = "AFFIN BANK BERHAD",
                    KodBNMEFT = "PHBMMYKL",
                    Length1 = 12,

                };

                context.Add(jBank);

                var jNegeri = new JNegeri()
                {
                    Kod = "01",
                    Perihal = "SELANGOR"
                };

                context.Add(jNegeri);

                await context.SaveChangesAsync();
                //

                // Jadual Linked
                var jkwptjbahagian = new JKWPTJBahagian()
                {
                    JKWId = 1,
                    JPTJId = 1,
                    JBahagianId = 1
                };

                context.Add(jkwptjbahagian);

                var akBank = new AkBank()
                {
                    Kod = "01",
                    Perihal = "MAYBANK 563019249274",
                    NoAkaun = "563019249274",
                    JKWId = 1,
                    AkCartaId = 1,
                    JBankId = 1,

                };
                context.Add(akBank);

                await context.SaveChangesAsync();

                var jCawangan = new JCawangan()
                {
                    Kod = "01",
                    Perihal = "TADIKA KUALA TERENGGANU",
                    AkBankId = 1,
                };
                context.Add(jCawangan);

                await context.SaveChangesAsync();
                //

                // Daftar Awam
                var daftarAwam = new DDaftarAwam()
                {
                    Kod = "I0001",
                    Nama = "IDWAL SYSTEMS SYDN BHD",
                    JNegeriId = 1,
                    JBankId = 1,
                    NoPendaftaran = "187842-T",
                    Alamat1 = "605G KOMPLEKS DIAMOND, BANGI BUSINESS PARK,",
                    Alamat2 = "JALAN MEDAN BANGI, OFF PERSIARAN BANDAR",
                    Poskod = "43650",
                    Bandar = "BANDAR BARU BANGI",
                    Telefon1 = "0382100155",
                    Emel = "far@idwal.com.my",
                    EnKategoriDaftarAwam = EnKategoriDaftarAwam.Pembekal,
                    EnKategoriAhli = EnKategoriAhli.Tiada,
                    IsBekalan = true,
                    IsPerkhidmatan = true,
                    KodM2E = "P0001"
                };

                context.Add(daftarAwam);

                await context.SaveChangesAsync();
                //

                // PenilaianPerolehan
                // PenilaianPerolehan For PVWithOneInvoisAkruWithOnePOAndWithoutInden
                var akPenilaianPerolehanOnePO = new AkPenilaianPerolehan()
                {
                    Tahun = "2024",
                    NoRujukan = "PN/2024/00001",
                    NoSebutHarga = "00001",
                    Tarikh = 1.January(2024),
                    TarikhPerlu = 5.January(2024),
                    EnKaedahPerolehan = EnKaedahPerolehan.None,
                    HargaTawaran = decimal.Parse("6200.00"),
                    Jumlah = decimal.Parse("6200.00"),
                    Sebab = "Membeli Sebuah Laptop Baru",
                    BilSebutharga = 1,
                    MaklumatSebutHarga = "SB/0001",
                    JKWId = 1,
                    Jawatan = "Eksekutif Teknologi Maklumat",
                    FlPOInden = 0,
                    DDaftarAwamId = 1,
                    EnStatusBorang = EnStatusBorang.Lulus,
                    FlPosting = 1

                };

                context.Add(akPenilaianPerolehanOnePO);
                await context.SaveChangesAsync();

                var akPenilaianPerolehanObjekOnePO = new AkPenilaianPerolehanObjek()
                {
                    AkPenilaianPerolehanId = 1,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("6200.00")
                };

                context.Add(akPenilaianPerolehanObjekOnePO);

                var akPenilaianPerolehanPerihalOnePO = new AkPenilaianPerolehanPerihal()
                {
                    AkPenilaianPerolehanId = 1,
                    Bil = 1,
                    Perihal = "Membeli sebuah laptop (akru)",
                    Kuantiti = 1,
                    Unit = "Unit",
                    Harga = decimal.Parse("6200.00"),
                    Amaun = decimal.Parse("6200.00")
                };

                context.Add(akPenilaianPerolehanPerihalOnePO);

                await context.SaveChangesAsync();
                // PenilaianPerolehan For PVWithOneInvoisAkruWithOnePOAndWithoutInden End

                // PenilaianPerolehan For PVWithOneInvoisAkruWithOneIndenAndWithoutPO
                var akPenilaianPerolehanOneInden = new AkPenilaianPerolehan()
                {
                    Tahun = "2024",
                    NoRujukan = "PN/2024/00002",
                    NoSebutHarga = "00002",
                    Tarikh = 1.January(2024),
                    TarikhPerlu = 7.January(2024),
                    EnKaedahPerolehan = EnKaedahPerolehan.None,
                    HargaTawaran = decimal.Parse("800.00"),
                    Jumlah = decimal.Parse("800.00"),
                    Sebab = "Membaiki paip sinki dapur",
                    BilSebutharga = 1,
                    MaklumatSebutHarga = "SB/0002",
                    JKWId = 1,
                    Jawatan = "Eksekutif Teknologi Maklumat",
                    FlPOInden = 1,
                    DDaftarAwamId = 1,
                    EnStatusBorang = EnStatusBorang.Lulus,
                    FlPosting = 1

                };

                context.Add(akPenilaianPerolehanOneInden);
                await context.SaveChangesAsync();

                var akPenilaianPerolehanObjekOneInden = new AkPenilaianPerolehanObjek()
                {
                    AkPenilaianPerolehanId = 2,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("800.00")
                };

                context.Add(akPenilaianPerolehanObjekOneInden);

                var akPenilaianPerolehanPerihalOneInden = new AkPenilaianPerolehanPerihal()
                {
                    AkPenilaianPerolehanId = 2,
                    Bil = 1,
                    Perihal = "Membaiki paip sinki dapur (akru)",
                    Kuantiti = 1,
                    Unit = "Unit",
                    Harga = decimal.Parse("800.00"),
                    Amaun = decimal.Parse("800.00")
                };

                context.Add(akPenilaianPerolehanPerihalOneInden);

                await context.SaveChangesAsync();
                // PenilaianPerolehan For PVWithOneInvoisAkruWithOnePOAndWithoutInden End

                // PenilaianPerolehan For PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek
                var akPenilaianPerolehanMultiplePOWithEachHaveOneSameObjek1 = new AkPenilaianPerolehan()
                {
                    Tahun = "2024",
                    NoRujukan = "PN/2024/00003",
                    NoSebutHarga = "00003",
                    Tarikh = 1.January(2024),
                    TarikhPerlu = 5.January(2024),
                    EnKaedahPerolehan = EnKaedahPerolehan.None,
                    HargaTawaran = decimal.Parse("100.50"),
                    Jumlah = decimal.Parse("100.50"),
                    Sebab = "Membeli 1 unit kertas printer",
                    BilSebutharga = 1,
                    MaklumatSebutHarga = "SB/0003",
                    JKWId = 1,
                    Jawatan = "Eksekutif Teknologi Maklumat",
                    FlPOInden = 0,
                    DDaftarAwamId = 1,
                    EnStatusBorang = EnStatusBorang.Lulus,
                    FlPosting = 1

                };

                context.Add(akPenilaianPerolehanMultiplePOWithEachHaveOneSameObjek1);
                await context.SaveChangesAsync();

                var akPenilaianPerolehanObjekMultiplePOWithEachHaveOneSameObjek1 = new AkPenilaianPerolehanObjek()
                {
                    AkPenilaianPerolehanId = 2,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("100.50")
                };

                context.Add(akPenilaianPerolehanObjekMultiplePOWithEachHaveOneSameObjek1);

                var akPenilaianPerolehanPerihalMultiplePOWithEachHaveOneSameObjek1 = new AkPenilaianPerolehanPerihal()
                {
                    AkPenilaianPerolehanId = 2,
                    Bil = 1,
                    Perihal = "Membeli sebuah laptop (akru)",
                    Kuantiti = 1,
                    Unit = "Unit",
                    Harga = decimal.Parse("100.50"),
                    Amaun = decimal.Parse("100.50")
                };

                context.Add(akPenilaianPerolehanPerihalMultiplePOWithEachHaveOneSameObjek1);

                await context.SaveChangesAsync();

                var akPenilaianPerolehanMultiplePOWithEachHaveOneSameObjek2 = new AkPenilaianPerolehan()
                {
                    Tahun = "2024",
                    NoRujukan = "PN/2024/00004",
                    NoSebutHarga = "00004",
                    Tarikh = 1.January(2024),
                    TarikhPerlu = 6.January(2024),
                    EnKaedahPerolehan = EnKaedahPerolehan.None,
                    HargaTawaran = decimal.Parse("600.00"),
                    Jumlah = decimal.Parse("600.00"),
                    Sebab = "Membeli 1 unit whiteboard",
                    BilSebutharga = 1,
                    MaklumatSebutHarga = "SB/0004",
                    JKWId = 1,
                    Jawatan = "Eksekutif Teknologi Maklumat",
                    FlPOInden = 0,
                    DDaftarAwamId = 1,
                    EnStatusBorang = EnStatusBorang.Lulus,
                    FlPosting = 1

                };

                context.Add(akPenilaianPerolehanMultiplePOWithEachHaveOneSameObjek2);
                await context.SaveChangesAsync();

                var akPenilaianPerolehanObjekMultiplePOWithEachHaveOneSameObjek2 = new AkPenilaianPerolehanObjek()
                {
                    AkPenilaianPerolehanId = 3,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("600.00")
                };

                context.Add(akPenilaianPerolehanObjekMultiplePOWithEachHaveOneSameObjek2);

                var akPenilaianPerolehanPerihalMultiplePOWithEachHaveOneSameObjek2 = new AkPenilaianPerolehanPerihal()
                {
                    AkPenilaianPerolehanId = 3,
                    Bil = 1,
                    Perihal = "Membeli whiteboard (akru)",
                    Kuantiti = 1,
                    Unit = "Unit",
                    Harga = decimal.Parse("600.00"),
                    Amaun = decimal.Parse("600.00")
                };

                context.Add(akPenilaianPerolehanPerihalMultiplePOWithEachHaveOneSameObjek2);

                await context.SaveChangesAsync();
                // PenilaianPerolehan For PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek End

                // PenilaianPerolehan For PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneDifferentObjek
                var akPenilaianPerolehanMultiplePOWithEachHaveOneDifferentObjek1 = new AkPenilaianPerolehan()
                {
                    Tahun = "2024",
                    NoRujukan = "PN/2024/00005",
                    NoSebutHarga = "00005",
                    Tarikh = 2.January(2024),
                    TarikhPerlu = 8.January(2024),
                    EnKaedahPerolehan = EnKaedahPerolehan.None,
                    HargaTawaran = decimal.Parse("120.00"),
                    Jumlah = decimal.Parse("120.00"),
                    Sebab = "Membeli 1 unit bekas pensel",
                    BilSebutharga = 1,
                    MaklumatSebutHarga = "SB/0005",
                    JKWId = 1,
                    Jawatan = "Eksekutif Teknologi Maklumat",
                    FlPOInden = 0,
                    DDaftarAwamId = 1,
                    EnStatusBorang = EnStatusBorang.Lulus,
                    FlPosting = 1

                };

                context.Add(akPenilaianPerolehanMultiplePOWithEachHaveOneDifferentObjek1);
                await context.SaveChangesAsync();

                var akPenilaianPerolehanObjekMultiplePOWithEachHaveOneDifferentObjek1 = new AkPenilaianPerolehanObjek()
                {
                    AkPenilaianPerolehanId = 4,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("120.00")
                };

                context.Add(akPenilaianPerolehanObjekMultiplePOWithEachHaveOneDifferentObjek1);

                var akPenilaianPerolehanPerihalMultiplePOWithEachHaveOneDifferentObjek1 = new AkPenilaianPerolehanPerihal()
                {
                    AkPenilaianPerolehanId = 4,
                    Bil = 1,
                    Perihal = "Membeli bekas pensel (akru)",
                    Kuantiti = 1,
                    Unit = "Unit",
                    Harga = decimal.Parse("120.00"),
                    Amaun = decimal.Parse("120.00")
                };

                context.Add(akPenilaianPerolehanPerihalMultiplePOWithEachHaveOneDifferentObjek1);

                await context.SaveChangesAsync();

                var akPenilaianPerolehanMultiplePOWithEachHaveOneDifferentObjek2 = new AkPenilaianPerolehan()
                {
                    Tahun = "2024",
                    NoRujukan = "PN/2024/00006",
                    NoSebutHarga = "00006",
                    Tarikh = 2.January(2024),
                    TarikhPerlu = 9.January(2024),
                    EnKaedahPerolehan = EnKaedahPerolehan.None,
                    HargaTawaran = decimal.Parse("150.00"),
                    Jumlah = decimal.Parse("150.00"),
                    Sebab = "Membeli 1 unit majalah pejabat",
                    BilSebutharga = 1,
                    MaklumatSebutHarga = "SB/0006",
                    JKWId = 1,
                    Jawatan = "Eksekutif Teknologi Maklumat",
                    FlPOInden = 0,
                    DDaftarAwamId = 1,
                    EnStatusBorang = EnStatusBorang.Lulus,
                    FlPosting = 1

                };

                context.Add(akPenilaianPerolehanMultiplePOWithEachHaveOneDifferentObjek2);
                await context.SaveChangesAsync();

                var akPenilaianPerolehanObjekMultiplePOWithEachHaveOneDifferentObjek2 = new AkPenilaianPerolehanObjek()
                {
                    AkPenilaianPerolehanId = 5,
                    AkCartaId = 4,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("150.00")
                };

                context.Add(akPenilaianPerolehanObjekMultiplePOWithEachHaveOneDifferentObjek2);

                var akPenilaianPerolehanPerihalMultiplePOWithEachHaveOneDifferentObjek2 = new AkPenilaianPerolehanPerihal()
                {
                    AkPenilaianPerolehanId = 5,
                    Bil = 1,
                    Perihal = "Membeli majalah pejabat (akru)",
                    Kuantiti = 1,
                    Unit = "Unit",
                    Harga = decimal.Parse("150.00"),
                    Amaun = decimal.Parse("150.00")
                };

                context.Add(akPenilaianPerolehanPerihalMultiplePOWithEachHaveOneDifferentObjek2);

                await context.SaveChangesAsync();
                // PenilaianPerolehan For PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneDifferentObjek End
                //

                // PO
                // PO For PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek
                var akPO = new AkPO()
                {
                    Tahun = "2024",
                    NoRujukan = "PO/2024/00001",
                    Tarikh = 1.January(2024),
                    AkPenilaianPerolehanId = 1,
                    Jumlah = decimal.Parse("6200.00"),
                    JKWId = 1,
                    DDaftarAwamId = 1,
                    FlPosting = 1,
                    EnStatusBorang = EnStatusBorang.Lulus
                };
                context.Add(akPO);
                await context.SaveChangesAsync();

                var akPOObjek = new AkPOObjek()
                {
                    AkPOId = 1,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("6200.00")
                };

                context.Add(akPOObjek);

                var akPOPerihal = new AkPOPerihal()
                {
                    AkPOId = 1,
                    Bil = 1,
                    Perihal = "Membeli sebuah laptop (akru)",
                    Kuantiti = 1,
                    Unit = "Unit",
                    Harga = decimal.Parse("6200.00"),
                    Amaun = decimal.Parse("6200.00")
                };

                context.Add(akPOPerihal);
                await context.SaveChangesAsync();
                // PO For PVWithOneInvoisAkruWithOnePOAndWithoutInden End

                // PO For PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek
                var akPOWithEachHaveOneSameObjek1 = new AkPO()
                {
                    Tahun = "2024",
                    NoRujukan = "PO/2024/00002",
                    Tarikh = 2.January(2024),
                    AkPenilaianPerolehanId = 2,
                    Jumlah = decimal.Parse("100.50"),
                    JKWId = 1,
                    DDaftarAwamId = 1,
                    FlPosting = 1,
                    EnStatusBorang = EnStatusBorang.Lulus
                };
                context.Add(akPOWithEachHaveOneSameObjek1);
                await context.SaveChangesAsync();

                var akPOObjekWithEachHaveOneSameObjek1 = new AkPOObjek()
                {
                    AkPOId = 2,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("100.50")
                };

                context.Add(akPOObjekWithEachHaveOneSameObjek1);

                var akPOPerihalWithEachHaveOneSameObjek1 = new AkPOPerihal()
                {
                    AkPOId = 2,
                    Bil = 1,
                    Perihal = "Membeli kertas printer (akru)",
                    Kuantiti = 1,
                    Unit = "Unit",
                    Harga = decimal.Parse("100.50"),
                    Amaun = decimal.Parse("100.50")
                };

                context.Add(akPOPerihalWithEachHaveOneSameObjek1);
                await context.SaveChangesAsync();

                var akPOWithEachHaveOneSameObjek2 = new AkPO()
                {
                    Tahun = "2024",
                    NoRujukan = "PO/2024/00003",
                    Tarikh = 1.January(2024),
                    AkPenilaianPerolehanId = 3,
                    Jumlah = decimal.Parse("600.00"),
                    JKWId = 1,
                    DDaftarAwamId = 1,
                    FlPosting = 1,
                    EnStatusBorang = EnStatusBorang.Lulus
                };
                context.Add(akPOWithEachHaveOneSameObjek2);
                await context.SaveChangesAsync();

                var akPOObjekWithEachHaveOneSameObjek2 = new AkPOObjek()
                {
                    AkPOId = 3,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("600.00")
                };

                context.Add(akPOObjekWithEachHaveOneSameObjek2);

                var akPOPerihalWithEachHaveOneSameObjek2 = new AkPOPerihal()
                {
                    AkPOId = 3,
                    Bil = 1,
                    Perihal = "Membeli whiteboard (akru)",
                    Kuantiti = 1,
                    Unit = "Unit",
                    Harga = decimal.Parse("600.00"),
                    Amaun = decimal.Parse("600.00")
                };

                context.Add(akPOPerihalWithEachHaveOneSameObjek2);
                await context.SaveChangesAsync();
                // PO For PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek End

                // PO For PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneDifferentObjek
                var akPOWithEachHaveOneDifferentObjek1 = new AkPO()
                {
                    Tahun = "2024",
                    NoRujukan = "PO/2024/00004",
                    Tarikh = 5.January(2024),
                    AkPenilaianPerolehanId = 4,
                    Jumlah = decimal.Parse("120.00"),
                    JKWId = 1,
                    DDaftarAwamId = 1,
                    FlPosting = 1,
                    EnStatusBorang = EnStatusBorang.Lulus
                };
                context.Add(akPOWithEachHaveOneDifferentObjek1);
                await context.SaveChangesAsync();

                var akPOObjekWithEachHaveOneDifferentObjek1 = new AkPOObjek()
                {
                    AkPOId = 4,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("100.50")
                };

                context.Add(akPOObjekWithEachHaveOneDifferentObjek1);

                var akPOPerihalWithEachHaveOneDifferentObjek1 = new AkPOPerihal()
                {
                    AkPOId = 4,
                    Bil = 1,
                    Perihal = "Membeli bekas pensel (akru)",
                    Kuantiti = 1,
                    Unit = "Unit",
                    Harga = decimal.Parse("100.50"),
                    Amaun = decimal.Parse("100.50")
                };

                context.Add(akPOPerihalWithEachHaveOneDifferentObjek1);
                await context.SaveChangesAsync();

                var akPOWithEachHaveOneDifferentObjek2 = new AkPO()
                {
                    Tahun = "2024",
                    NoRujukan = "PO/2024/00005",
                    Tarikh = 3.January(2024),
                    AkPenilaianPerolehanId = 5,
                    Jumlah = decimal.Parse("150.00"),
                    JKWId = 1,
                    DDaftarAwamId = 1,
                    FlPosting = 1,
                    EnStatusBorang = EnStatusBorang.Lulus
                };
                context.Add(akPOWithEachHaveOneDifferentObjek2);
                await context.SaveChangesAsync();

                var akPOObjekWithEachHaveOneDifferentObjek2 = new AkPOObjek()
                {
                    AkPOId = 5,
                    AkCartaId = 4,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("150.00")
                };

                context.Add(akPOObjekWithEachHaveOneDifferentObjek2);

                var akPOPerihalWithEachHaveOneDifferentObjek2 = new AkPOPerihal()
                {
                    AkPOId = 5,
                    Bil = 1,
                    Perihal = "Membeli majalah pejabat (akru)",
                    Kuantiti = 1,
                    Unit = "Unit",
                    Harga = decimal.Parse("150.00"),
                    Amaun = decimal.Parse("150.00")
                };

                context.Add(akPOPerihalWithEachHaveOneDifferentObjek2);
                await context.SaveChangesAsync();
                // PO For PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneDifferentObjek End

                //

                // Inden
                // Inden For PVWithOneInvoisAkruWithOneIndenAndWithoutPO
                var akInden = new AkInden()
                {
                    Tahun = "2024",
                    NoRujukan = "IK/2024/00001",
                    Tarikh = 2.January(2024),
                    AkPenilaianPerolehanId = 2,
                    Jumlah = decimal.Parse("800.00"),
                    JKWId = 1,
                    DDaftarAwamId = 1,
                    FlPosting = 1,
                    EnStatusBorang = EnStatusBorang.Lulus
                };
                context.Add(akInden);
                await context.SaveChangesAsync();

                var akIndenObjek = new AkIndenObjek()
                {
                    AkIndenId = 1,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("6200.00")
                };

                context.Add(akIndenObjek);

                var akIndenPerihal = new AkIndenPerihal()
                {
                    AkIndenId = 1,
                    Bil = 1,
                    Perihal = "Membaiki paip sinki dapur (akru)",
                    Kuantiti = 1,
                    Unit = "Unit",
                    Harga = decimal.Parse("800.00"),
                    Amaun = decimal.Parse("800.00")
                };

                context.Add(akIndenPerihal);
                await context.SaveChangesAsync();
                // Inden For PVWithOneInvoisAkruWithOneIndenAndWithoutPO End

                //


                // Belian
                // Belian For PVNotAkru
                var akBelianNotAkru = new AkBelian()
                {
                    Tahun = "2024",
                    NoRujukan = "IN/A0001/001",
                    Tarikh = 1.January(2024),
                    TarikhTerimaBahagian = 1.January(2024),
                    TarikhAkuanKewangan = 1.January(2024),
                    EnJenisBayaranBelian = EnJenisBayaranBelian.LainLain,
                    DDaftarAwamId = 1,
                    Jumlah = decimal.Parse("250.00"),
                    JKWId = 1,
                    EnStatusBorang = EnStatusBorang.Lulus,
                    FlPosting = 1
                };

                context.Add(akBelianNotAkru);

                await context.SaveChangesAsync();

                var akBelianObjekNotAkru = new AkBelianObjek()
                {
                    AkBelianId = 1,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("250.00")
                };

                context.Add(akBelianObjekNotAkru);

                var akBelianPerihalNotAkru = new AkBelianPerihal()
                {
                    AkBelianId = 1,
                    Bil = 1,
                    Perihal = "Membayar Bil Elektrik (tidak akru)",
                    Kuantiti = 1,
                    Unit = "Unit",
                    Harga = decimal.Parse("250.00"),
                    Amaun = decimal.Parse("250.00")
                };

                context.Add(akBelianPerihalNotAkru);

                await context.SaveChangesAsync();
                // Belian For PVNotAkru End

                // Belian For PVAkruWithoutPOOrInden
                var akBelianAkruWithoutPOOrInden = new AkBelian()
                {
                    Tahun = "2024",
                    NoRujukan = "IN/A0001/002",
                    Tarikh = 2.January(2024),
                    TarikhTerimaBahagian = 2.January(2024),
                    TarikhAkuanKewangan = 2.January(2024),
                    EnJenisBayaranBelian = EnJenisBayaranBelian.LainLain,
                    DDaftarAwamId = 1,
                    Jumlah = decimal.Parse("190.00"),
                    JKWId = 1,
                    EnStatusBorang = EnStatusBorang.Lulus,
                    FlPosting = 1,
                    AkAkaunAkruId = 3
                };

                context.Add(akBelianAkruWithoutPOOrInden);


                await context.SaveChangesAsync();

                var akBelianObjekAkruWithoutPOOrInden = new AkBelianObjek()
                {
                    AkBelianId = 2,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("190.00")
                };

                context.Add(akBelianObjekAkruWithoutPOOrInden);

                var akBelianPerihalAkruWithoutPOOrInden = new AkBelianPerihal()
                {
                    AkBelianId = 2,
                    Bil = 1,
                    Perihal = "Membayar Hutang Perniagaan (akru)",
                    Kuantiti = 1,
                    Unit = "Unit",
                    Harga = decimal.Parse("190.00"),
                    Amaun = decimal.Parse("190.00")
                };

                context.Add(akBelianPerihalAkruWithoutPOOrInden);

                await context.SaveChangesAsync();
                // Belian For PVAkruWithoutPOOrInden End

                // Belian For PVAkruWithOnePOAndWithoutInden
                var akBelianAkruWithOnePOAndWithoutInden = new AkBelian()
                {
                    Tahun = "2024",
                    NoRujukan = "IN/A0001/003",
                    Tarikh = 3.January(2024),
                    TarikhTerimaBahagian = 3.January(2024),
                    TarikhAkuanKewangan = 3.January(2024),
                    EnJenisBayaranBelian = EnJenisBayaranBelian.LainLain,
                    DDaftarAwamId = 1,
                    Jumlah = decimal.Parse("6200.00"),
                    JKWId = 1,
                    EnStatusBorang = EnStatusBorang.Lulus,
                    FlPosting = 1,
                    AkAkaunAkruId = 3,
                    AkPOId = 1
                };

                context.Add(akBelianAkruWithOnePOAndWithoutInden);


                await context.SaveChangesAsync();

                var akBelianObjekAkruWithOnePOAndWithoutInden = new AkBelianObjek()
                {
                    AkBelianId = 3,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("6200.00")
                };

                context.Add(akBelianObjekAkruWithOnePOAndWithoutInden);

                var akBelianPerihalAkruWithOnePOAndWithoutInden = new AkBelianPerihal()
                {
                    AkBelianId = 3,
                    Bil = 1,
                    Perihal = "Membeli sebuah laptop (akru)",
                    Kuantiti = 1,
                    Unit = "Unit",
                    Harga = decimal.Parse("6200.00"),
                    Amaun = decimal.Parse("6200.00")
                };

                context.Add(akBelianPerihalAkruWithOnePOAndWithoutInden);

                await context.SaveChangesAsync();
                // Belian For PVAkruWithOnePOAndWithoutInden End

                // Belian For PVAkruWithOneIndenAndWithoutPO
                var akBelianAkruWithOneIndenAndWithoutPO = new AkBelian()
                {
                    Tahun = "2024",
                    NoRujukan = "IN/A0001/004",
                    Tarikh = 4.January(2024),
                    TarikhTerimaBahagian = 4.January(2024),
                    TarikhAkuanKewangan = 4.January(2024),
                    EnJenisBayaranBelian = EnJenisBayaranBelian.LainLain,
                    DDaftarAwamId = 1,
                    Jumlah = decimal.Parse("800.00"),
                    JKWId = 1,
                    EnStatusBorang = EnStatusBorang.Lulus,
                    FlPosting = 1,
                    AkAkaunAkruId = 3,
                    AkIndenId = 1
                };

                context.Add(akBelianAkruWithOneIndenAndWithoutPO);


                await context.SaveChangesAsync();

                var akBelianObjekAkruWithOneIndenAndWithoutPO = new AkBelianObjek()
                {
                    AkBelianId = 4,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("800.00")
                };

                context.Add(akBelianObjekAkruWithOneIndenAndWithoutPO);

                var akBelianPerihalAkruWithOneIndenAndWithoutPO = new AkBelianPerihal()
                {
                    AkBelianId = 4,
                    Bil = 1,
                    Perihal = "Membaiki paip sinki dapur (akru)",
                    Kuantiti = 1,
                    Unit = "Unit",
                    Harga = decimal.Parse("800.00"),
                    Amaun = decimal.Parse("800.00")
                };

                context.Add(akBelianPerihalAkruWithOneIndenAndWithoutPO);

                await context.SaveChangesAsync();
                // Belian For PVAkruWithOneIndenAndWithoutPO End

                // Belian For PVWithMultipleInvoisNotAkru
                var akBelianNotAkru1 = new AkBelian()
                {
                    Tahun = "2024",
                    NoRujukan = "IN/A0001/005",
                    Tarikh = 4.January(2024),
                    TarikhTerimaBahagian = 4.January(2024),
                    TarikhAkuanKewangan = 4.January(2024),
                    EnJenisBayaranBelian = EnJenisBayaranBelian.LainLain,
                    DDaftarAwamId = 1,
                    Jumlah = decimal.Parse("100.00"),
                    JKWId = 1,
                    EnStatusBorang = EnStatusBorang.Lulus,
                    FlPosting = 1
                };

                context.Add(akBelianNotAkru1);

                await context.SaveChangesAsync();

                var akBelianObjekNotAkru1 = new AkBelianObjek()
                {
                    AkBelianId = 5,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("100.00")
                };

                context.Add(akBelianObjekNotAkru1);

                var akBelianPerihalNotAkru1 = new AkBelianPerihal()
                {
                    AkBelianId = 5,
                    Bil = 1,
                    Perihal = "Membayar Bil Air pertama (tidak akru)",
                    Kuantiti = 1,
                    Unit = "Unit",
                    Harga = decimal.Parse("100.00"),
                    Amaun = decimal.Parse("100.00")
                };

                context.Add(akBelianPerihalNotAkru1);

                var akBelianNotAkru2 = new AkBelian()
                {
                    Tahun = "2024",
                    NoRujukan = "IN/A0001/006",
                    Tarikh = 3.January(2024),
                    TarikhTerimaBahagian = 3.January(2024),
                    TarikhAkuanKewangan = 3.January(2024),
                    EnJenisBayaranBelian = EnJenisBayaranBelian.LainLain,
                    DDaftarAwamId = 1,
                    Jumlah = decimal.Parse("150.00"),
                    JKWId = 1,
                    EnStatusBorang = EnStatusBorang.Lulus,
                    FlPosting = 1
                };

                context.Add(akBelianNotAkru2);

                await context.SaveChangesAsync();

                var akBelianObjekNotAkru2 = new AkBelianObjek()
                {
                    AkBelianId = 6,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("150.00")
                };

                context.Add(akBelianObjekNotAkru2);

                var akBelianPerihalNotAkru2 = new AkBelianPerihal()
                {
                    AkBelianId = 6,
                    Bil = 1,
                    Perihal = "Membayar Bil Air kedua (tidak akru)",
                    Kuantiti = 1,
                    Unit = "Unit",
                    Harga = decimal.Parse("150.00"),
                    Amaun = decimal.Parse("150.00")
                };

                context.Add(akBelianPerihalNotAkru2);

                var akBelianNotAkru3 = new AkBelian()
                {
                    Tahun = "2024",
                    NoRujukan = "IN/A0001/007",
                    Tarikh = 2.January(2024),
                    TarikhTerimaBahagian = 2.January(2024),
                    TarikhAkuanKewangan = 2.January(2024),
                    EnJenisBayaranBelian = EnJenisBayaranBelian.LainLain,
                    DDaftarAwamId = 1,
                    Jumlah = decimal.Parse("250.00"),
                    JKWId = 1,
                    EnStatusBorang = EnStatusBorang.Lulus,
                    FlPosting = 1
                };

                context.Add(akBelianNotAkru3);

                await context.SaveChangesAsync();

                var akBelianObjekNotAkru3 = new AkBelianObjek()
                {
                    AkBelianId = 7,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("250.00")
                };

                context.Add(akBelianObjekNotAkru3);

                var akBelianPerihalNotAkru3 = new AkBelianPerihal()
                {
                    AkBelianId = 7,
                    Bil = 1,
                    Perihal = "Membayar Bil Air ketiga (tidak akru)",
                    Kuantiti = 1,
                    Unit = "Unit",
                    Harga = decimal.Parse("250.00"),
                    Amaun = decimal.Parse("250.00")
                };

                context.Add(akBelianPerihalNotAkru3);

                await context.SaveChangesAsync();
                // Belian For PVWithMultipleInvoisNotAkru End

                // Belian For PVWithMultipleInvoisAkruWithoutPOOrInden
                var akBelianAkruWithoutPOOrInden1 = new AkBelian()
                {
                    Tahun = "2024",
                    NoRujukan = "IN/A0001/008",
                    Tarikh = 5.January(2024),
                    TarikhTerimaBahagian = 5.January(2024),
                    TarikhAkuanKewangan = 5.January(2024),
                    EnJenisBayaranBelian = EnJenisBayaranBelian.LainLain,
                    DDaftarAwamId = 1,
                    Jumlah = decimal.Parse("500.00"),
                    JKWId = 1,
                    EnStatusBorang = EnStatusBorang.Lulus,
                    FlPosting = 1,
                    AkAkaunAkruId = 3
                };

                context.Add(akBelianAkruWithoutPOOrInden1);

                await context.SaveChangesAsync();

                var akBelianObjekAkruWithoutPOOrInden1 = new AkBelianObjek()
                {
                    AkBelianId = 8,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("500.00")
                };

                context.Add(akBelianObjekAkruWithoutPOOrInden1);

                var akBelianPerihalAkruWithoutPOOrInden1 = new AkBelianPerihal()
                {
                    AkBelianId = 8,
                    Bil = 1,
                    Perihal = "Membayar hutang perniagaan pertama (akru)",
                    Kuantiti = 1,
                    Unit = "Unit",
                    Harga = decimal.Parse("500.00"),
                    Amaun = decimal.Parse("500.00")
                };

                context.Add(akBelianPerihalAkruWithoutPOOrInden1);

                var akBelianAkruWithoutPOOrInden2 = new AkBelian()
                {
                    Tahun = "2024",
                    NoRujukan = "IN/A0001/009",
                    Tarikh = 3.January(2024),
                    TarikhTerimaBahagian = 4.January(2024),
                    TarikhAkuanKewangan = 4.January(2024),
                    EnJenisBayaranBelian = EnJenisBayaranBelian.LainLain,
                    DDaftarAwamId = 1,
                    Jumlah = decimal.Parse("250.00"),
                    JKWId = 1,
                    EnStatusBorang = EnStatusBorang.Lulus,
                    FlPosting = 1,
                    AkAkaunAkruId = 3
                };

                context.Add(akBelianAkruWithoutPOOrInden2);

                await context.SaveChangesAsync();

                var akBelianObjekAkruWithoutPOOrInden2 = new AkBelianObjek()
                {
                    AkBelianId = 9,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("250.00")
                };

                context.Add(akBelianObjekAkruWithoutPOOrInden2);

                var akBelianPerihalAkruWithoutPOOrInden2 = new AkBelianPerihal()
                {
                    AkBelianId = 9,
                    Bil = 1,
                    Perihal = "Membayar hutang perniagaan kedua (akru)",
                    Kuantiti = 1,
                    Unit = "Unit",
                    Harga = decimal.Parse("250.00"),
                    Amaun = decimal.Parse("250.00")
                };

                context.Add(akBelianPerihalAkruWithoutPOOrInden2);

                var akBelianAkruWithoutPOOrInden3 = new AkBelian()
                {
                    Tahun = "2024",
                    NoRujukan = "IN/A0001/010",
                    Tarikh = 3.January(2024),
                    TarikhTerimaBahagian = 4.January(2024),
                    TarikhAkuanKewangan = 4.January(2024),
                    EnJenisBayaranBelian = EnJenisBayaranBelian.LainLain,
                    DDaftarAwamId = 1,
                    Jumlah = decimal.Parse("250.00"),
                    JKWId = 1,
                    EnStatusBorang = EnStatusBorang.Lulus,
                    FlPosting = 1,
                    AkAkaunAkruId = 3
                };

                context.Add(akBelianAkruWithoutPOOrInden3);

                await context.SaveChangesAsync();

                var akBelianObjekAkruWithoutPOOrInden3 = new AkBelianObjek()
                {
                    AkBelianId = 10,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("250.00")
                };

                context.Add(akBelianObjekAkruWithoutPOOrInden3);

                var akBelianPerihalAkruWithoutPOOrInden3 = new AkBelianPerihal()
                {
                    AkBelianId = 10,
                    Bil = 1,
                    Perihal = "Membayar hutang perniagaan ketiga (akru)",
                    Kuantiti = 1,
                    Unit = "Unit",
                    Harga = decimal.Parse("250.00"),
                    Amaun = decimal.Parse("250.00")
                };

                context.Add(akBelianPerihalAkruWithoutPOOrInden3);

                await context.SaveChangesAsync();
                // Belian For PVWithMultipleInvoisNotAkru End

                // Belian For PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek
                var akBelianAkruWithMultiplePOWithEachHaveOneSameObjek1 = new AkBelian()
                {
                    Tahun = "2024",
                    NoRujukan = "IN/A0001/011",
                    Tarikh = 4.January(2024),
                    TarikhTerimaBahagian = 5.January(2024),
                    TarikhAkuanKewangan = 5.January(2024),
                    EnJenisBayaranBelian = EnJenisBayaranBelian.LainLain,
                    DDaftarAwamId = 1,
                    Jumlah = decimal.Parse("100.50"),
                    JKWId = 1,
                    EnStatusBorang = EnStatusBorang.Lulus,
                    FlPosting = 1,
                    AkAkaunAkruId = 3,
                    AkPOId = 2
                };

                context.Add(akBelianAkruWithMultiplePOWithEachHaveOneSameObjek1);

                await context.SaveChangesAsync();

                var akBelianObjekAkruWithMultiplePOWithEachHaveOneSameObjek1 = new AkBelianObjek()
                {
                    AkBelianId = 11,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("100.50")
                };

                context.Add(akBelianObjekAkruWithMultiplePOWithEachHaveOneSameObjek1);

                var akBelianPerihalAkruWithMultiplePOWithEachHaveOneSameObjek1 = new AkBelianPerihal()
                {
                    AkBelianId = 11,
                    Bil = 1,
                    Perihal = "Membeli kertas printer (akru)",
                    Kuantiti = 1,
                    Unit = "Unit",
                    Harga = decimal.Parse("100.50"),
                    Amaun = decimal.Parse("100.50")
                };

                context.Add(akBelianPerihalAkruWithMultiplePOWithEachHaveOneSameObjek1);

                var akBelianAkruWithMultiplePOWithEachHaveOneSameObjek2 = new AkBelian()
                {
                    Tahun = "2024",
                    NoRujukan = "IN/A0001/012",
                    Tarikh = 3.January(2024),
                    TarikhTerimaBahagian = 4.January(2024),
                    TarikhAkuanKewangan = 4.January(2024),
                    EnJenisBayaranBelian = EnJenisBayaranBelian.LainLain,
                    DDaftarAwamId = 1,
                    Jumlah = decimal.Parse("600.00"),
                    JKWId = 1,
                    EnStatusBorang = EnStatusBorang.Lulus,
                    FlPosting = 1,
                    AkAkaunAkruId = 3,
                    AkPOId = 3
                };

                context.Add(akBelianAkruWithMultiplePOWithEachHaveOneSameObjek2);

                await context.SaveChangesAsync();

                var akBelianObjekAkruWithMultiplePOWithEachHaveOneSameObjek2 = new AkBelianObjek()
                {
                    AkBelianId = 12,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("600.00")
                };

                context.Add(akBelianObjekAkruWithMultiplePOWithEachHaveOneSameObjek2);

                var akBelianPerihalAkruWithMultiplePOWithEachHaveOneSameObjek2 = new AkBelianPerihal()
                {
                    AkBelianId = 12,
                    Bil = 1,
                    Perihal = "Membeli whiteboard (akru)",
                    Kuantiti = 1,
                    Unit = "Unit",
                    Harga = decimal.Parse("600.00"),
                    Amaun = decimal.Parse("600.00")
                };

                context.Add(akBelianPerihalAkruWithMultiplePOWithEachHaveOneSameObjek2);

                await context.SaveChangesAsync();

                // Belian For PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek End

                // Belian For PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneDifferentObjek
                var akBelianAkruWithMultiplePOWithEachHaveOneDifferentObjek1 = new AkBelian()
                {
                    Tahun = "2024",
                    NoRujukan = "IN/A0001/013",
                    Tarikh = 7.January(2024),
                    TarikhTerimaBahagian = 8.January(2024),
                    TarikhAkuanKewangan = 8.January(2024),
                    EnJenisBayaranBelian = EnJenisBayaranBelian.LainLain,
                    DDaftarAwamId = 1,
                    Jumlah = decimal.Parse("120.00"),
                    JKWId = 1,
                    EnStatusBorang = EnStatusBorang.Lulus,
                    FlPosting = 1,
                    AkAkaunAkruId = 3,
                    AkPOId = 4
                };

                context.Add(akBelianAkruWithMultiplePOWithEachHaveOneDifferentObjek1);


                await context.SaveChangesAsync();

                var akBelianObjekAkruWithMultiplePOWithEachHaveOneDifferentObjek1 = new AkBelianObjek()
                {
                    AkBelianId = 13,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("100.50")
                };

                context.Add(akBelianObjekAkruWithMultiplePOWithEachHaveOneDifferentObjek1);

                var akBelianPerihalAkruWithMultiplePOWithEachHaveOneDifferentObjek1 = new AkBelianPerihal()
                {
                    AkBelianId = 13,
                    Bil = 1,
                    Perihal = "Membeli bekas pensel (akru)",
                    Kuantiti = 1,
                    Unit = "Unit",
                    Harga = decimal.Parse("120.00"),
                    Amaun = decimal.Parse("120.00")
                };

                context.Add(akBelianPerihalAkruWithMultiplePOWithEachHaveOneDifferentObjek1);

                var akBelianAkruWithMultiplePOWithEachHaveOneDifferentObjek2 = new AkBelian()
                {
                    Tahun = "2024",
                    NoRujukan = "IN/A0001/014",
                    Tarikh = 5.January(2024),
                    TarikhTerimaBahagian = 6.January(2024),
                    TarikhAkuanKewangan = 6.January(2024),
                    EnJenisBayaranBelian = EnJenisBayaranBelian.LainLain,
                    DDaftarAwamId = 1,
                    Jumlah = decimal.Parse("150.00"),
                    JKWId = 1,
                    EnStatusBorang = EnStatusBorang.Lulus,
                    FlPosting = 1,
                    AkAkaunAkruId = 3,
                    AkPOId = 5
                };

                context.Add(akBelianAkruWithMultiplePOWithEachHaveOneDifferentObjek2);

                await context.SaveChangesAsync();

                var akBelianObjekAkruWithMultiplePOWithEachHaveOneDifferentObjek2 = new AkBelianObjek()
                {
                    AkBelianId = 14,
                    AkCartaId = 4,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("150.00")
                };

                context.Add(akBelianObjekAkruWithMultiplePOWithEachHaveOneDifferentObjek2);

                var akBelianPerihalAkruWithMultiplePOWithEachHaveOneDifferentObjek2 = new AkBelianPerihal()
                {
                    AkBelianId = 14,
                    Bil = 1,
                    Perihal = "Membeli majalah pejabat (akru)",
                    Kuantiti = 1,
                    Unit = "Unit",
                    Harga = decimal.Parse("150.00"),
                    Amaun = decimal.Parse("150.00")
                };

                context.Add(akBelianPerihalAkruWithMultiplePOWithEachHaveOneDifferentObjek2);

                await context.SaveChangesAsync();

                // Belian For PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneDifferentObjek End
                //

                // PV
                // -PVWithoutInvois
                var akPVWithoutInvois = new AkPV()
                {
                    Tahun = "2024",
                    NoRujukan = "PV/2024/00001",
                    Tarikh = 1.January(2024),
                    JCawanganId = 1,
                    Jumlah = decimal.Parse("290.00"),
                    AkBankId = 1,
                    JKWId = 1,
                    Ringkasan = "Bayaran Baucer Tanpa Invois",
                    NamaPenerima = "John Doe",
                    EnJenisBayaran = EnJenisBayaran.Invois,

                };
                context.Add(akPVWithoutInvois);

                await context.SaveChangesAsync();

                var akPVObjekWithoutInvois = new AkPVObjek()
                {
                    AkPVId = 1,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("290.00")
                };

                context.Add(akPVObjekWithoutInvois);

                var akPVPenerimaWithoutInvois = new AkPVPenerima()
                {
                    AkPVId = 1,
                    EnKategoriDaftarAwam = EnKategoriDaftarAwam.LainLain,
                    NoPendaftaranPenerima = "700420115144",
                    NamaPenerima = "John Doe",
                    NoPendaftaranPemohon = "700420115144",
                    JCaraBayarId = 1,
                    JBankId = 1,
                    NoAkaunBank = "35951000257",
                    Alamat1 = "1194H  KAMPUNG SEBERANG BAROH KIRI",
                    Alamat2 = "KUALA TERENGGANU",
                    Emel = "tuanrabiahtest@yit.gov.my",
                    KodM2E = "G0017",
                    Amaun = decimal.Parse("290.00"),
                    Bil = 1,
                    EnJenisId = EnJenisId.KPBaru

                };

                context.Add(akPVPenerimaWithoutInvois);

                await context.SaveChangesAsync();
                // -PVWithoutInvois end

                // -PVWithOneInvoisNotAkru

                var akPVWithOneInvoisNotAkru = new AkPV()
                {
                    Tahun = "2024",
                    NoRujukan = "PV/2024/00002",
                    Tarikh = 2.January(2024),
                    JCawanganId = 1,
                    Jumlah = decimal.Parse("250.00"),
                    AkBankId = 1,
                    JKWId = 1,
                    Ringkasan = "Bayaran Baucer dengan 1 Invois tidak akru",
                    NamaPenerima = "Rebecca Winston",
                    EnJenisBayaran = EnJenisBayaran.Invois,
                    IsInvois = true
                };
                context.Add(akPVWithOneInvoisNotAkru);

                await context.SaveChangesAsync();

                var akPVObjekWithOneInvoisNotAkru = new AkPVObjek()
                {
                    AkPVId = 2,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("250.00")
                };

                context.Add(akPVObjekWithOneInvoisNotAkru);

                var akPVPenerimaWithOneInvoisNotAkru = new AkPVPenerima()
                {
                    AkPVId = 2,
                    EnKategoriDaftarAwam = EnKategoriDaftarAwam.LainLain,
                    NoPendaftaranPenerima = "720420115246",
                    NamaPenerima = "Rebecca Winston",
                    NoPendaftaranPemohon = "720420115246",
                    JCaraBayarId = 1,
                    JBankId = 1,
                    NoAkaunBank = "35951100327",
                    Alamat1 = "11A Persiaran Selangor",
                    Alamat2 = "Shah Alam",
                    Emel = "tuanrabiahtest@yit.gov.my",
                    KodM2E = "G0017",
                    Amaun = decimal.Parse("250.00"),
                    Bil = 1,
                    EnJenisId = EnJenisId.KPBaru

                };

                context.Add(akPVPenerimaWithOneInvoisNotAkru);

                var akPVInvoisWithOneInvoisNotAkru = new AkPVInvois()
                {
                    AkPVId = 2,
                    IsTanggungan = false,
                    AkBelianId = 1,
                    Amaun = decimal.Parse("250.00")
                };
                context.Add(akPVInvoisWithOneInvoisNotAkru);

                await context.SaveChangesAsync();

                // -PVWithOneInvoisNotAkru end

                // -PVWithOneInvoisAkruWithoutPOOrInden
                var akPVWithOneInvoisAkruWithoutPOOrInden = new AkPV()
                {
                    Tahun = "2024",
                    NoRujukan = "PV/2024/00003",
                    Tarikh = 3.January(2024),
                    JCawanganId = 1,
                    Jumlah = decimal.Parse("190.00"),
                    AkBankId = 1,
                    JKWId = 1,
                    Ringkasan = "Bayaran Baucer dengan 1 Invois akru tanpa PO atau Inden (Tanpa Tanggungan)",
                    NamaPenerima = "John F. Kennedy",
                    EnJenisBayaran = EnJenisBayaran.Invois,
                    IsInvois = true,
                    IsAkru = true

                };
                context.Add(akPVWithOneInvoisAkruWithoutPOOrInden);

                await context.SaveChangesAsync();


                var akPVObjekWithOneInvoisAkruWithoutPOOrInden = new AkPVObjek()
                {
                    AkPVId = 3,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("190.00")
                };

                context.Add(akPVObjekWithOneInvoisAkruWithoutPOOrInden);

                var akPVPenerimaWithOneInvoisAkruWithoutPOOrInden = new AkPVPenerima()
                {
                    AkPVId = 3,
                    EnKategoriDaftarAwam = EnKategoriDaftarAwam.LainLain,
                    NoPendaftaranPenerima = "820420215255",
                    NamaPenerima = "John F. Kennedy",
                    NoPendaftaranPemohon = "820420215255",
                    JCaraBayarId = 1,
                    JBankId = 1,
                    NoAkaunBank = "15951110527",
                    Alamat1 = "139, jalan bandar,",
                    Alamat2 = "Kuala Terengganu",
                    Emel = "johnkennedy@gmail.com",
                    KodM2E = "G0007",
                    Amaun = decimal.Parse("190.00"),
                    Bil = 1,
                    EnJenisId = EnJenisId.KPBaru

                };

                context.Add(akPVPenerimaWithOneInvoisAkruWithoutPOOrInden);

                var akPVInvoisWithOneInvoisAkruWithoutPOOrInden = new AkPVInvois()
                {
                    AkPVId = 3,
                    IsTanggungan = false,
                    AkBelianId = 2,
                    Amaun = decimal.Parse("190.00")
                };
                context.Add(akPVInvoisWithOneInvoisAkruWithoutPOOrInden);

                await context.SaveChangesAsync();

                // -PVWithOneInvoisAkruWithoutPOOrInden end

                // -PVWithOneInvoisAkruWithOnePOAndWithoutInden
                var akPVWithOneInvoisAkruWithOnePOAndWithoutInden = new AkPV()
                {
                    Tahun = "2024",
                    NoRujukan = "PV/2024/00004",
                    Tarikh = 4.January(2024),
                    JCawanganId = 1,
                    Jumlah = decimal.Parse("6200.00"),
                    AkBankId = 1,
                    JKWId = 1,
                    Ringkasan = "Bayaran Baucer dengan 1 Invois akru dan ada PO (Dengan Tanggungan)",
                    NamaPenerima = "Maria Sharapova",
                    EnJenisBayaran = EnJenisBayaran.Invois,
                    IsInvois = true,
                    IsAkru = true,
                    IsTanggungan = true

                };
                context.Add(akPVWithOneInvoisAkruWithOnePOAndWithoutInden);

                await context.SaveChangesAsync();


                var akPVObjekWithOneInvoisAkruWithOnePOAndWithoutInden = new AkPVObjek()
                {
                    AkPVId = 4,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("6200.00")
                };

                context.Add(akPVObjekWithOneInvoisAkruWithOnePOAndWithoutInden);

                var akPVPenerimaWithOneInvoisAkruWithOnePOAndWithoutInden = new AkPVPenerima()
                {
                    AkPVId = 4,
                    EnKategoriDaftarAwam = EnKategoriDaftarAwam.LainLain,
                    NoPendaftaranPenerima = "790222017238",
                    NamaPenerima = "Maria Sharapova",
                    NoPendaftaranPemohon = "790222017238",
                    JCaraBayarId = 1,
                    JBankId = 1,
                    NoAkaunBank = "65971183502",
                    Alamat1 = "11A Persiaran Selangor",
                    Alamat2 = "Shah Alam",
                    Emel = "mariasharap10@gmail.com",
                    KodM2E = "G0002",
                    Amaun = decimal.Parse("6200.00"),
                    Bil = 1,
                    EnJenisId = EnJenisId.KPBaru

                };

                context.Add(akPVPenerimaWithOneInvoisAkruWithOnePOAndWithoutInden);

                var akPVInvoisWithOneInvoisAkruWithOnePOAndWithoutInden = new AkPVInvois()
                {
                    AkPVId = 4,
                    IsTanggungan = true,
                    AkBelianId = 3,
                    Amaun = decimal.Parse("6200.00")
                };
                context.Add(akPVInvoisWithOneInvoisAkruWithOnePOAndWithoutInden);

                await context.SaveChangesAsync();

                // -PVWithOneInvoisAkruWithOnePOAndWithoutInden end

                // -PVWithOneInvoisAkruWithOneIndenAndWithoutPO
                var akPVWithOneInvoisAkruWithOneIndenAndWithoutPO = new AkPV()
                {
                    Tahun = "2024",
                    NoRujukan = "PV/2024/00005",
                    Tarikh = 5.January(2024),
                    JCawanganId = 1,
                    Jumlah = decimal.Parse("800.00"),
                    AkBankId = 1,
                    JKWId = 1,
                    Ringkasan = "Bayaran Baucer dengan 1 Invois akru dan ada Inden (Dengan Tanggungan)",
                    NamaPenerima = "Kevin Hart",
                    EnJenisBayaran = EnJenisBayaran.Invois,
                    IsInvois = true,
                    IsAkru = true,
                    IsTanggungan = true

                };
                context.Add(akPVWithOneInvoisAkruWithOneIndenAndWithoutPO);

                await context.SaveChangesAsync();


                var akPVObjekWithOneInvoisAkruWithOneIndenAndWithoutPO = new AkPVObjek()
                {
                    AkPVId = 5,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("800.00")
                };

                context.Add(akPVObjekWithOneInvoisAkruWithOneIndenAndWithoutPO);

                var akPVPenerimaWithOneInvoisAkruWithOneIndenAndWithoutPO = new AkPVPenerima()
                {
                    AkPVId = 5,
                    EnKategoriDaftarAwam = EnKategoriDaftarAwam.LainLain,
                    NoPendaftaranPenerima = "901102047211",
                    NamaPenerima = "Kevin Hart",
                    NoPendaftaranPemohon = "901102047211",
                    JCaraBayarId = 1,
                    JBankId = 1,
                    NoAkaunBank = "15371182509",
                    Alamat1 = "A 76 Jln Harimau Tarum Taman Abad",
                    Alamat2 = "Johor Bahru",
                    Emel = "kevinHart@gmail.com",
                    KodM2E = "G0022",
                    Amaun = decimal.Parse("800.00"),
                    Bil = 1,
                    EnJenisId = EnJenisId.KPBaru

                };

                context.Add(akPVPenerimaWithOneInvoisAkruWithOneIndenAndWithoutPO);

                var akPVInvoisWithOneInvoisAkruWithOneIndenAndWithoutPO = new AkPVInvois()
                {
                    AkPVId = 5,
                    IsTanggungan = true,
                    AkBelianId = 4,
                    Amaun = decimal.Parse("800.00")
                };
                context.Add(akPVInvoisWithOneInvoisAkruWithOneIndenAndWithoutPO);

                await context.SaveChangesAsync();

                // -PVWithOneInvoisAkruWithoutPOOrInden end

                // -PVWithMultipleInvoisNotAkru
                var akPVWithMultipleInvoisNotAkru = new AkPV()
                {
                    Tahun = "2024",
                    NoRujukan = "PV/2024/00006",
                    Tarikh = 5.January(2024),
                    JCawanganId = 1,
                    Jumlah = decimal.Parse("500.00"),
                    AkBankId = 1,
                    JKWId = 1,
                    Ringkasan = "Bayaran Baucer dengan multiple Invois tak akru (cash basis)",
                    NamaPenerima = "Syed Ahmad Dani",
                    EnJenisBayaran = EnJenisBayaran.Invois,
                    IsInvois = true,
                    IsAkru = false,
                    IsTanggungan = false

                };
                context.Add(akPVWithMultipleInvoisNotAkru);

                await context.SaveChangesAsync();


                var akPVObjekWithMultipleInvoisNotAkru = new AkPVObjek()
                {
                    AkPVId = 6,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("500.00")
                };

                context.Add(akPVObjekWithMultipleInvoisNotAkru);

                var akPVPenerimaWithMultipleInvoisNotAkru = new AkPVPenerima()
                {
                    AkPVId = 6,
                    EnKategoriDaftarAwam = EnKategoriDaftarAwam.LainLain,
                    NoPendaftaranPenerima = "950813115655",
                    NamaPenerima = "Syed Ahmad Dani",
                    NoPendaftaranPemohon = "950813115655",
                    JCaraBayarId = 1,
                    JBankId = 1,
                    NoAkaunBank = "35978182560",
                    Alamat1 = "No. 27 Jln Padi Ria 13",
                    Alamat2 = "Bandar Baru Uda",
                    Alamat3 = "Johor Bahru",
                    Emel = "syedDani001@gmail.com",
                    KodM2E = "G0005",
                    Amaun = decimal.Parse("100.00"),
                    Bil = 1,
                    EnJenisId = EnJenisId.KPBaru

                };

                context.Add(akPVPenerimaWithMultipleInvoisNotAkru);

                var akPVInvoisWithMultipleInvoisNotAkru1 = new AkPVInvois()
                {
                    AkPVId = 6,
                    IsTanggungan = false,
                    AkBelianId = 5,
                    Amaun = decimal.Parse("100.00")
                };
                context.Add(akPVInvoisWithMultipleInvoisNotAkru1);

                var akPVInvoisWithMultipleInvoisNotAkru2 = new AkPVInvois()
                {
                    AkPVId = 6,
                    IsTanggungan = false,
                    AkBelianId = 6,
                    Amaun = decimal.Parse("150.00")
                };
                context.Add(akPVInvoisWithMultipleInvoisNotAkru2);

                var akPVInvoisWithMultipleInvoisNotAkru3 = new AkPVInvois()
                {
                    AkPVId = 6,
                    IsTanggungan = false,
                    AkBelianId = 7,
                    Amaun = decimal.Parse("250.00")
                };
                context.Add(akPVInvoisWithMultipleInvoisNotAkru3);

                await context.SaveChangesAsync();

                // -PVWithMultipleInvoisNotAkru end

                // -PVWithMultipleInvoisAkruWithoutPOOrInden
                var akPVWithMultipleInvoisAkruWithoutPOOrInden = new AkPV()
                {
                    Tahun = "2024",
                    NoRujukan = "PV/2024/00007",
                    Tarikh = 6.January(2024),
                    JCawanganId = 1,
                    Jumlah = decimal.Parse("1000.00"),
                    AkBankId = 1,
                    JKWId = 1,
                    Ringkasan = "Bayaran Baucer dengan multiple Invois akru",
                    NamaPenerima = "Michael Jackson",
                    EnJenisBayaran = EnJenisBayaran.Invois,
                    IsInvois = true,
                    IsAkru = true,
                    IsTanggungan = false

                };
                context.Add(akPVWithMultipleInvoisAkruWithoutPOOrInden);

                await context.SaveChangesAsync();


                var akPVObjekWithMultipleInvoisAkruWithoutPOOrInden = new AkPVObjek()
                {
                    AkPVId = 7,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("500.00")
                };

                context.Add(akPVObjekWithMultipleInvoisAkruWithoutPOOrInden);

                var akPVPenerimaWithMultipleInvoisAkruWithoutPOOrInden = new AkPVPenerima()
                {
                    AkPVId = 7,
                    EnKategoriDaftarAwam = EnKategoriDaftarAwam.LainLain,
                    NoPendaftaranPenerima = "900303034211",
                    NamaPenerima = "Michael Jackson",
                    NoPendaftaranPemohon = "900303034211",
                    JCaraBayarId = 1,
                    JBankId = 1,
                    NoAkaunBank = "65253132661",
                    Alamat1 = "47 Jln Sultan Yussof",
                    Alamat2 = "Ipoh",
                    Alamat3 = "Perak",
                    Emel = "michaelJackson@yahoo.com",
                    KodM2E = "G0015",
                    Amaun = decimal.Parse("1000.00"),
                    Bil = 1,
                    EnJenisId = EnJenisId.KPBaru

                };

                context.Add(akPVPenerimaWithMultipleInvoisAkruWithoutPOOrInden);

                var akPVInvoisWithMultipleInvoisAkruWithoutPOOrInden1 = new AkPVInvois()
                {
                    AkPVId = 7,
                    IsTanggungan = false,
                    AkBelianId = 8,
                    Amaun = decimal.Parse("500.00")
                };
                context.Add(akPVInvoisWithMultipleInvoisAkruWithoutPOOrInden1);

                var akPVInvoisWithMultipleInvoisAkruWithoutPOOrInden2 = new AkPVInvois()
                {
                    AkPVId = 7,
                    IsTanggungan = false,
                    AkBelianId = 9,
                    Amaun = decimal.Parse("250.00")
                };
                context.Add(akPVInvoisWithMultipleInvoisAkruWithoutPOOrInden2);

                var akPVInvoisWithMultipleInvoisAkruWithoutPOOrInden3 = new AkPVInvois()
                {
                    AkPVId = 7,
                    IsTanggungan = false,
                    AkBelianId = 10,
                    Amaun = decimal.Parse("250.00")
                };
                context.Add(akPVInvoisWithMultipleInvoisAkruWithoutPOOrInden3);

                await context.SaveChangesAsync();

                // -PVWithMultipleInvoisAkruWithoutPOOrInden end

                // -PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek
                var akPVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek = new AkPV()
                {
                    Tahun = "2024",
                    NoRujukan = "PV/2024/00008",
                    Tarikh = 7.January(2024),
                    JCawanganId = 1,
                    Jumlah = decimal.Parse("700.50"),
                    AkBankId = 1,
                    JKWId = 1,
                    Ringkasan = "Bayaran Baucer dengan multiple Invois akru dan multiple PO yang setiap po sama objek (dengan tanggungan)",
                    NamaPenerima = "Utada Hikaru",
                    EnJenisBayaran = EnJenisBayaran.Invois,
                    IsInvois = true,
                    IsAkru = true,
                    IsTanggungan = true

                };
                context.Add(akPVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek);

                await context.SaveChangesAsync();


                var akPVObjekWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek1 = new AkPVObjek()
                {
                    AkPVId = 8,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("100.50")
                };

                context.Add(akPVObjekWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek1);

                var akPVObjekWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek2 = new AkPVObjek()
                {
                    AkPVId = 8,
                    AkCartaId = 4,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("600.00")
                };

                context.Add(akPVObjekWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek2);

                var akPVPenerimaWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek = new AkPVPenerima()
                {
                    AkPVId = 8,
                    EnKategoriDaftarAwam = EnKategoriDaftarAwam.LainLain,
                    NoPendaftaranPenerima = "870613051516",
                    NamaPenerima = "Utada Hikaru",
                    NoPendaftaranPemohon = "870613051516",
                    JCaraBayarId = 1,
                    JBankId = 1,
                    NoAkaunBank = "45152192001",
                    Alamat1 = "No. 229 Jln Tun Razak",
                    Alamat2 = "Kuala Lumpur",
                    Emel = "utadahikaru@gmail.com",
                    KodM2E = "G0011",
                    Amaun = decimal.Parse("700.50"),
                    Bil = 1,
                    EnJenisId = EnJenisId.KPBaru

                };

                context.Add(akPVPenerimaWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek);

                var akPVInvoisWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek1 = new AkPVInvois()
                {
                    AkPVId = 8,
                    IsTanggungan = true,
                    AkBelianId = 11,
                    Amaun = decimal.Parse("100.50")
                };
                context.Add(akPVInvoisWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek1);

                var akPVInvoisWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek2 = new AkPVInvois()
                {
                    AkPVId = 8,
                    IsTanggungan = true,
                    AkBelianId = 12,
                    Amaun = decimal.Parse("600.00")
                };
                context.Add(akPVInvoisWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek2);

                await context.SaveChangesAsync();

                // -PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek end

                // -PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneDifferentObjek
                var akPVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneDifferentObjek = new AkPV()
                {
                    Tahun = "2024",
                    NoRujukan = "PV/2024/00009",
                    Tarikh = 9.January(2024),
                    JCawanganId = 1,
                    Jumlah = decimal.Parse("270.00"),
                    AkBankId = 1,
                    JKWId = 1,
                    Ringkasan = "Bayaran Baucer dengan multiple Invois akru dan multiple PO yang setiap po berbeza objek (dengan tanggungan)",
                    NamaPenerima = "Alan Walker",
                    EnJenisBayaran = EnJenisBayaran.Invois,
                    IsInvois = true,
                    IsAkru = true,
                    IsTanggungan = true

                };
                context.Add(akPVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneDifferentObjek);

                await context.SaveChangesAsync();


                var akPVObjekWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneDifferentObjek1 = new AkPVObjek()
                {
                    AkPVId = 9,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("120.00")
                };

                context.Add(akPVObjekWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneDifferentObjek1);

                var akPVObjekWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneDifferentObjek2 = new AkPVObjek()
                {
                    AkPVId = 8,
                    AkCartaId = 4,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("150.00")
                };

                context.Add(akPVObjekWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneDifferentObjek2);

                var akPVPenerimaWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneDifferentObjek = new AkPVPenerima()
                {
                    AkPVId = 9,
                    EnKategoriDaftarAwam = EnKategoriDaftarAwam.LainLain,
                    NoPendaftaranPenerima = "841209042911",
                    NamaPenerima = "Alan Walker",
                    NoPendaftaranPemohon = "841209042911",
                    JCaraBayarId = 1,
                    JBankId = 1,
                    NoAkaunBank = "44122732082",
                    Alamat1 = "No. 29 Jln Mahameru",
                    Alamat2 = "Kuala Lumpur",
                    Emel = "alanWalker@gmail.com",
                    KodM2E = "G0019",
                    Amaun = decimal.Parse("270.00"),
                    Bil = 1,
                    EnJenisId = EnJenisId.KPBaru

                };

                context.Add(akPVPenerimaWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneDifferentObjek);

                var akPVInvoisWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneDifferentObjek1 = new AkPVInvois()
                {
                    AkPVId = 9,
                    IsTanggungan = true,
                    AkBelianId = 13,
                    Amaun = decimal.Parse("120.00")
                };
                context.Add(akPVInvoisWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneDifferentObjek1);

                var akPVInvoisWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneDifferentObjek2 = new AkPVInvois()
                {
                    AkPVId = 9,
                    IsTanggungan = true,
                    AkBelianId = 14,
                    Amaun = decimal.Parse("150.00")
                };
                context.Add(akPVInvoisWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneDifferentObjek2);

                await context.SaveChangesAsync();

                // -PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek end

            }


            return context;
        }

        [Fact]
        public async void AkPVRepository_PVWithoutInvois_ReturnsBool()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var akPVRepo = new AkPVRepository(dbContext);

            // Act
            var akPV = akPVRepo.GetDetailsById(1);
            var result = akPVRepo.PVWithoutInvois(akPV);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async void AkPVRepository_PVWithOneInvoisNotAkru_ReturnsBool()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var akPVRepo = new AkPVRepository(dbContext);

            // Act
            var akPV = akPVRepo.GetDetailsById(2);
            var result = akPVRepo.PVWithOneInvoisNotAkru(akPV);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async void AkPVRepository_PVWithOneInvoisAkruWithoutPOOrInden_ReturnsBool()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var akPVRepo = new AkPVRepository(dbContext);

            // Act
            var akPV = akPVRepo.GetDetailsById(3); // tak buat pv dia lagi
            var result = akPVRepo.PVWithOneInvoisAkruWithoutPOOrInden(akPV);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async void AkPVRepository_PVWithOneInvoisAkruWithOnePOAndWithoutInden_ReturnsBool()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var akPVRepo = new AkPVRepository(dbContext);

            // Act
            var akPV = akPVRepo.GetDetailsById(4);
            var result = akPVRepo.PVWithOneInvoisAkruWithOnePOAndWithoutInden(akPV);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async void AkPVRepository_PVWithOneInvoisAkruWithOneIndenAndWithoutPO_ReturnsBool()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var akPVRepo = new AkPVRepository(dbContext);

            // Act
            var akPV = akPVRepo.GetDetailsById(5);
            var result = akPVRepo.PVWithOneInvoisAkruWithOneIndenAndWithoutPO(akPV);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async void AkPVRepository_PVWithMultipleInvoisNotAkru()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var akPVRepo = new AkPVRepository(dbContext);

            // Act
            var akPV = akPVRepo.GetDetailsById(6);
            var result = akPVRepo.PVWithMultipleInvoisNotAkru(akPV);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async void AkPVRepository_PVWithMultipleInvoisAkruWithoutPOOrInden()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var akPVRepo = new AkPVRepository(dbContext);

            // Act
            var akPV = akPVRepo.GetDetailsById(7);
            var result = akPVRepo.PVWithMultipleInvoisAkruWithoutPOOrInden(akPV);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async void AkPVRepository_PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var akPVRepo = new AkPVRepository(dbContext);

            // Act
            var akPV = akPVRepo.GetDetailsById(8);
            var result = akPVRepo.PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneSameObjek(akPV);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async void AkPVRepository_PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneDifferentObjek()
        {
            // Arrange
            var dbContext = await GetDbContext();
            var akPVRepo = new AkPVRepository(dbContext);

            // Act
            var akPV = akPVRepo.GetDetailsById(9);
            var result = akPVRepo.PVWithMultipleInvoisAkruWithMultiplePOWithEachHaveOneDifferentObjek(akPV);

            // Assert
            result.Should().BeTrue();
        }
    }
}
