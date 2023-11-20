using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._00Sistem;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT.__Domain.Entities.Models._03Akaun;
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

        // 00Sistem (Si)
        public DbSet<SiAppInfo> SiAppInfo { get; set; }
        //

        // 01Jadual (J)
        public DbSet<JNegeri> JNegeri { get; set;}
        public DbSet<JAgama> JAgama { get; set;}
        public DbSet<JBangsa> JBangsa { get; set;}
        public DbSet<JBank> JBank { get; set;}
        public DbSet<JCaraBayar> JCaraBayar { get; set;}
        public DbSet<JKW> JKW { get; set;}
        public DbSet<JPTJ> JPTJ { get; set; }
        public DbSet<JBahagian> JBahagian { get; set; }
        public DbSet<JCawangan> JCawangan { get; set; }
        //

        // 02Daftar (D)
        public DbSet<DPekerja> DPekerja { get; set;}
        public DbSet<DDaftarAwam> DDaftarAwam { get; set; }
        public DbSet<DKonfigKelulusan> DKonfigKelulusan { get; set; }
        //

        // 03Kewangan (AK)
        public DbSet<AkCarta> AkCarta { get;set; }  
        public DbSet<AkBank> AkBank { get; set; }
        public DbSet<AkAkaun> AkAkaun { get; set; }
        public DbSet<AbWaran> AbWaran { get; set; }
        public DbSet<AbWaranObjek> AbWaranObjek { get; set; }
        public DbSet<AbBukuVot> AbBukuVot { get; set; }

        //

        // 04Penerimaan (TR)
        public DbSet<AkTerima> AkTerima { get; set; }
        public DbSet<AkTerimaCaraBayar> AkTerimaCaraBayar { get; set; }
        public DbSet<AkTerimaObjek> AkTerimaObjek { get; set; }
        //

        // 05Perolehan (PR)
        public DbSet<AkPenilaianPerolehan> AkPenilaianPerolehan { get; set; }
        public DbSet<AkPenilaianPerolehanObjek> AkPenilaianPerolehanObjek { get; set; }
        public DbSet<AkPenilaianPerolehanPerihal> AkPenilaianPerolehanPerihal { get; set; }
        public DbSet<AkNotaMinta> AkNotaMinta { get; set; }
        public DbSet<AkNotaMintaObjek> AkNotaMintaObjek { get; set; }
        public DbSet<AkNotaMintaPerihal> AkNotaMintaPerihal { get; set; }
        //

        // 06Tanggungan (TG)
        public DbSet<AkPO> AkPO { get; set; }
        public DbSet<AkPOObjek> AkPOObjek { get; set; }
        public DbSet<AkPOPerihal> AkPOPerihal { get; set; }
        public DbSet<AkInden> AkInden { get; set; }
        public DbSet<AkIndenObjek> AkIndenObjek { get; set; }
        public DbSet<AkIndenPerihal> AkIndenPerihal { get; set; }
        //

        // 07Pembayaran (PV)

        //

        // 08Pelarasan (PT)

        //

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.FilteringSoftDeleteQuery();
            builder.SeedEntitiesProperties();
        }
    }
}
