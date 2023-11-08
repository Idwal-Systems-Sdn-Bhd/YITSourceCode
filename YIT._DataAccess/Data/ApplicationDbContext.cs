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
        //

        // 02Daftar (D)
        public DbSet<DPekerja> DPekerja { get; set;}
        public DbSet<DDaftarAwam> DDaftarAwam { get; set; }
        public DbSet<DPenyemak> DPenyemak { get; set; }
        public DbSet<DPelulus> DPelulus { get; set; }
        //

        // 03Kewangan (AK)
        public DbSet<AkCarta> AkCarta { get;set; }  
        public DbSet<AkBank> AkBank { get; set; }
        public DbSet<AkAkaun> AkAkaun { get; set; }
        //

        // 04Penerimaan (PR)
        public DbSet<AkTerima> AkTerima { get; set; }
        public DbSet<AkTerimaCaraBayar> AkTerimaCaraBayar { get; set; }
        public DbSet<AkTerimaObjek> AkTerimaObjek { get; set; }
        //

        // 05Tanggungan (TG)

        //

        // 06Pembayaran (PV)

        //

        // 07Pelarasan (PT)

        //

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.FilteringSoftDeleteQuery();
            builder.SeedEntitiesProperties();
        }
    }
}
