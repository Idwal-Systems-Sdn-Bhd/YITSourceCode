using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._00Sistem;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT.__Domain.Entities.Models._04Sumber;
using YIT._DataAccess.Data.DataConfigurations;

namespace YIT._DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        // Administration
        public DbSet<ApplicationUser> ApplicationUsers {  get; set; }
        public DbSet<AppLog> AppLog { get; set; }
        public DbSet<ExceptionLogger> ExceptionLogger { get; set; }
        //

        // 00Sistem 
        public DbSet<SiAppInfo> SiAppInfo { get; set; }
        //

        // 01Jadual 
        public DbSet<JNegeri> JNegeri { get; set;}
        public DbSet<JAgama> JAgama { get; set;}
        public DbSet<JBangsa> JBangsa { get; set;}
        public DbSet<JBank> JBank { get; set;}
        public DbSet<JCaraBayar> JCaraBayar { get; set;}
        public DbSet<JKW> JKW { get; set;}
        public DbSet<JPTJ> JPTJ { get; set; }
        public DbSet<JBahagian> JBahagian { get; set; }
        public DbSet<JKWPTJBahagian> JKWPTJBahagian { get; set; }
        public DbSet<JCawangan> JCawangan { get; set; }
        public DbSet<JCukai> JCukai { get; set; }
        public DbSet<JElaunPotongan> JElaunPotongan { get; set; }
        public DbSet<JKonfigPerubahanEkuiti> JKonfigPerubahanEkuiti { get; set; }
        public DbSet<JKonfigPerubahanEkuitiBaris> JKonfigPerubahanEkuitiBaris { get; set; }
        public DbSet<JKonfigPenyata> JKonfigPenyata { get; set; }
        public DbSet<JKonfigPenyataBaris> JKonfigPenyataBaris { get; set; }
        public DbSet<JKonfigPenyataBarisFormula> JKonfigPenyataBarisFormula { get; set; }
        //

        // 02Daftar
        public DbSet<DPekerja> DPekerja { get; set;}
        public DbSet<DPekerjaElaunPotongan> DPekerjaElaunPotongan { get; set; }
        public DbSet<DDaftarAwam> DDaftarAwam { get; set; }
        public DbSet<DKonfigKelulusan> DKonfigKelulusan { get; set; }
        public DbSet<DPenerimaCekGaji> DPenerimaCekGaji { get; set; }
        public DbSet<DPanjar> DPanjar { get; set; }
        public DbSet<DPanjarPemegang> DPanjarPemegang { get; set; }
        //

        // 03Kewangan 
        public DbSet<AkCarta> AkCarta { get;set; }  
        public DbSet<AkBank> AkBank { get; set; }
        public DbSet<AkAkaun> AkAkaun { get; set; }
        public DbSet<AbWaran> AbWaran { get; set; }
        public DbSet<AbWaranObjek> AbWaranObjek { get; set; }
        public DbSet<AbBukuVot> AbBukuVot { get; set; }

        //

        // 04Penerimaan 
        public DbSet<AkTerima> AkTerima { get; set; }
        public DbSet<AkTerimaCaraBayar> AkTerimaCaraBayar { get; set; }
        public DbSet<AkTerimaObjek> AkTerimaObjek { get; set; }
        //

        // 05Perolehan 
        public DbSet<AkPenilaianPerolehan> AkPenilaianPerolehan { get; set; }
        public DbSet<AkPenilaianPerolehanObjek> AkPenilaianPerolehanObjek { get; set; }
        public DbSet<AkPenilaianPerolehanPerihal> AkPenilaianPerolehanPerihal { get; set; }
        public DbSet<AkNotaMinta> AkNotaMinta { get; set; }
        public DbSet<AkNotaMintaObjek> AkNotaMintaObjek { get; set; }
        public DbSet<AkNotaMintaPerihal> AkNotaMintaPerihal { get; set; }
        //

        // 06Tanggungan 
        public DbSet<AkPO> AkPO { get; set; }
        public DbSet<AkPOObjek> AkPOObjek { get; set; }
        public DbSet<AkPOPerihal> AkPOPerihal { get; set; }
        public DbSet<AkInden> AkInden { get; set; }
        public DbSet<AkIndenObjek> AkIndenObjek { get; set; }
        public DbSet<AkIndenPerihal> AkIndenPerihal { get; set; }
        public DbSet<AkBelian> AkBelian { get; set; }
        public DbSet<AkBelianObjek> AkBelianObjek { get; set; }
        public DbSet<AkBelianPerihal> AkBelianPerihal { get; set; }
        //

        // 07Pembayaran 
        public DbSet<AkPV> AkPV { get; set; }
        public DbSet<AkPVObjek> AkPVObjek { get; set; }
        public DbSet<AkPVInvois> AkPVInvois { get; set; }
        public DbSet<AkPVPenerima> AkPVPenerima { get; set; }
        public DbSet<AkJanaanProfil> AkJanaanProfil { get; set; }
        public DbSet<AkJanaanProfilPenerima> AkJanaanProfilPenerima { get; set; }
        //

        // 08EFT
        public DbSet<AkEFT> AkEFT { get; set; }
        public DbSet<AkEFTPenerima> AkEFTPenerima { get; set; }
        //

        // 09Panjar
        public DbSet<AkPanjarLejar> AkPanjarLejar { get; set; }
        public DbSet<AkRekup> AkRekup { get; set; }
        public DbSet<AkCV> AkCV { get; set; }
        public DbSet<AkCVObjek> AkCVObjek { get; set; }
        //
        // 10Pelarasan 
        public DbSet<AkPelarasanPO> AkPelarasanPO { get; set; }
        public DbSet<AkPelarasanPOObjek> AkPelarasanPOObjek { get; set; }
        public DbSet<AkPelarasanPOPerihal> AkPelarasanPOPerihal { get; set; }
        public DbSet<AkPelarasanInden> AkPelarasanInden { get; set; }
        public DbSet<AkPelarasanIndenObjek> AkPelarasanIndenObjek { get; set; }
        public DbSet<AkPelarasanIndenPerihal> AkPelarasanIndenPerihal { get; set; }
        public DbSet<AkNotaDebitKreditDiterima> AkNotaDebitKreditDiterima { get; set; }
        public DbSet<AkNotaDebitKreditDiterimaObjek> AkNotaDebitKreditDiterimaObjek { get; set; }
        public DbSet<AkNotaDebitKreditDiterimaPerihal> AkNotaDebitKreditDiterimaPerihal { get; set; }
        public DbSet<AkJurnal> AkJurnal { get; set; }
        public DbSet<AkJurnalObjek> AkJurnalObjek { get; set; }
        public DbSet<AkJurnalPenerimaCekBatal> AkJurnalPenerimaCekBatal { get; set; }
        //

        // 11Sumber
        public DbSet<SuGajiBulanan> SuGajiBulanan { get; set; }
        public DbSet<SuGajiBulananPekerja> SuGajiBulananPekerja { get; set; }
        public DbSet<SuGajiElaunPotongan> SuGajiElaunPotongan { get; set; }
        //

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.FilteringSoftDeleteQuery();
            builder.SeedEntitiesProperties();
        }
    }
}
