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

namespace YIT.Tests.DataAccess.Repositories
{
    public class AkPVRepositorySeeds : AkPVRepositoryISeeds
    {
        public async Task GetDbContext(ApplicationDbContext context)
        {
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

                var akCartaObjek = new AkCarta()
                {
                    Kod = "B23102",
                    Perihal = "BAYARAN POS UNTUK SURAT-SURAT",
                    DebitKredit = "D",
                    UmumDetail = "D",
                    Baki = 0,
                    EnJenis = EnJenisCarta.Belanja,
                    EnParas = EnParas.Paras4
                };

                context.Add(akCartaObjek);

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

                // Belian
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

                var akBelianNotAkruObjek = new AkBelianObjek()
                {
                    AkBelianId = 1,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("250.00")
                };

                context.Add(akBelianNotAkruObjek);

                var akBelianNotAkruPerihal = new AkBelianPerihal()
                {
                    AkBelianId = 1,
                    Bil = 1,
                    Perihal = "Membayar Bil Elektrik (tidak akru)",
                    Kuantiti = 1,
                    Unit = "Unit",
                    Harga = decimal.Parse("250.00"),
                    Amaun = decimal.Parse("250.00")
                };

                context.Add(akBelianNotAkruPerihal);

                await context.SaveChangesAsync();
                //

                // PV

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

                var akPVObjekWithoutInvois = new AkPVObjek()
                {
                    AkPVId = 1,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("290.00")
                };

                context.Add(akPVObjekWithoutInvois);

                var akPVObjekWithOneInvoisNotAkru = new AkPVObjek()
                {
                    AkPVId = 2,
                    AkCartaId = 2,
                    JKWPTJBahagianId = 1,
                    Amaun = decimal.Parse("250.00")
                };

                context.Add(akPVObjekWithOneInvoisNotAkru);

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
            }

        }
    }
}
