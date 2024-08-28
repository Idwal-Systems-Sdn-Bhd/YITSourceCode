using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT.__Domain.Entities.Models._03Akaun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._04Sumber;

namespace YIT._DataAccess.Data.DataConfigurations
{
    public static class ModelBuilderConfigurations
    {
        public static void FilteringSoftDeleteQuery(this ModelBuilder modelBuilder)
        {
            // Jadual
            modelBuilder.Entity<JNegeri>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<JAgama>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<JBangsa>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<JBank>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<JKW>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<JBahagian>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<JPTJ>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<JCaraBayar>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<JCawangan>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<JCukai>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<JElaunPotongan>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<JKonfigPerubahanEkuiti>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<JKonfigPenyata>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);

            //

            // Daftar
            modelBuilder.Entity<DPekerja>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0)
                .HasQueryFilter(m => EF.Property<bool>(m,"IsAdmin") == false);
            modelBuilder.Entity<DKonfigKelulusan>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<DDaftarAwam>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<DPenerimaZakat>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            //

            // Akaun
            modelBuilder.Entity<AkAkaun>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);

            modelBuilder.Entity<AkCarta>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<AkBank>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<AbWaran>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<AkPenilaianPerolehan>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<AkNotaMinta>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<AkPO>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<AkInden>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<AkPelarasanPO>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<AkPelarasanInden>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<AkBelian>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<AkNotaDebitKreditDiterima>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<AkPV>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<DPanjar>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<AkJanaanProfil>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<AkCV>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<AkEFT>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<AkJurnal>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<AkAnggar>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<AkNotaDebitKreditDikeluarkan>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);

            //

            // Sumber
            modelBuilder.Entity<SuGajiBulanan>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);

            //
        }

        public static void SeedEntitiesProperties(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JKWPTJBahagian>()
                    .HasOne(j => j.JKW)
                    .WithMany(j => j.JKWPTJBahagian)
                    .HasForeignKey(j => j.JKWId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<JKWPTJBahagian>()
                    .HasOne(j => j.JPTJ)
                    .WithMany(j => j.JKWPTJBahagian)
                    .HasForeignKey(j => j.JPTJId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<JKWPTJBahagian>()
                    .HasOne(j => j.JBahagian)
                    .WithMany(j => j.JKWPTJBahagian)
                    .HasForeignKey(j => j.JBahagianId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<DPenyelia>()
                    .HasOne(b => b.JCawangan)
                    .WithMany(b => b.DPenyelia)
                    .HasForeignKey(b => b.JCawanganId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<DPenyelia>()
                    .HasOne(b => b.DPekerja)
                    .WithMany(b => b.DPenyelia)
                    .HasForeignKey(b => b.DPekerjaId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkAkaun>()
                    .HasOne(m => m.AkCarta1)
                    .WithMany(t => t.AkAkaun1)
                    .HasForeignKey(m => m.AkCarta1Id)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

            modelBuilder.Entity<AkAkaun>()
                    .HasOne(m => m.AkCarta2)
                    .WithMany(t => t.AkAkaun2)
                    .HasForeignKey(m => m.AkCarta2Id)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkAkaun>()
                    .HasOne(m => m.JKW)
                    .WithMany(t => t.AkAkaun)
                    .HasForeignKey(m => m.JKWId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkAkaun>()
                    .HasOne(m => m.JPTJ)
                    .WithMany(t => t.AkAkaun)
                    .HasForeignKey(m => m.JPTJId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkAkaun>()
                    .HasOne(m => m.JBahagian)
                    .WithMany(t => t.AkAkaun)
                    .HasForeignKey(m => m.JBahagianId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkTerima>()
                    .HasOne(m => m.JKW)
                    .WithMany(t => t.AkTerima)
                    .HasForeignKey(m => m.JKWId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkTerima>()
                    .HasOne(m => m.JCawangan)
                    .WithMany(t => t.AkTerima)
                    .HasForeignKey(m => m.JCawanganId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkTerimaObjek>()
                    .HasOne(m => m.AkCarta)
                    .WithMany(t => t.AkTerimaObjek)
                    .HasForeignKey(m => m.AkCartaId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkTerimaObjek>()
                    .HasOne(m => m.JKWPTJBahagian)
                    .WithMany(t => t.AkTerimaObjek)
                    .HasForeignKey(m => m.JKWPTJBahagianId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkTerimaCaraBayar>()
                    .HasOne(m => m.JCaraBayar)
                    .WithMany(t => t.AkTerimaCaraBayar)
                    .HasForeignKey(m => m.JCaraBayarId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AbBukuVot>()
                    .HasOne(m => m.JKW)
                    .WithMany(t => t.AbBukuVot)
                    .HasForeignKey(m => m.JKWId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AbBukuVot>()
                    .HasOne(m => m.JPTJ)
                    .WithMany(t => t.AbBukuVot)
                    .HasForeignKey(m => m.JPTJId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AbBukuVot>()
                    .HasOne(m => m.Vot)
                    .WithMany(t => t.AbBukuVot)
                    .HasForeignKey(m => m.VotId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AbWaranObjek>()
                    .HasOne(m => m.AbWaran)
                    .WithMany(t => t.AbWaranObjek)
                    .HasForeignKey(m => m.AbWaranId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AbWaranObjek>()
                    .HasOne(m => m.AkCarta)
                    .WithMany(t => t.AbWaranObjek)
                    .HasForeignKey(m => m.AkCartaId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AbWaranObjek>()
                    .HasOne(m => m.JKWPTJBahagian)
                    .WithMany(t => t.AbWaranObjek)
                    .HasForeignKey(m => m.JKWPTJBahagianId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkAnggarObjek>()
                    .HasOne(m => m.AkAnggar)
                    .WithMany(t => t.AkAnggarObjek)
                    .HasForeignKey(m => m.AkAnggarId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkAnggarObjek>()
                    .HasOne(m => m.AkCarta)
                    .WithMany(t => t.AkAnggarObjek)
                    .HasForeignKey(m => m.AkCartaId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkAnggarObjek>()
                    .HasOne(m => m.JKWPTJBahagian)
                    .WithMany(t => t.AkAnggarObjek)
                    .HasForeignKey(m => m.JKWPTJBahagianId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);


            modelBuilder.Entity<AkAnggarLejar>()
                    .HasOne(m => m.AkCarta)
                    .WithMany(t => t.AkAnggarLejar)
                    .HasForeignKey(m => m.AkCartaId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

            modelBuilder.Entity<AkPenilaianPerolehanObjek>()
                    .HasOne(m => m.AkCarta)
                    .WithMany(t => t.AkPenilaianPerolehanObjek)
                    .HasForeignKey(m => m.AkCartaId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPenilaianPerolehanObjek>()
                    .HasOne(m => m.AkPenilaianPerolehan)
                    .WithMany(t => t.AkPenilaianPerolehanObjek)
                    .HasForeignKey(m => m.AkPenilaianPerolehanId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPenilaianPerolehanObjek>()
                    .HasOne(m => m.JKWPTJBahagian)
                    .WithMany(t => t.AkPenilaianPerolehanObjek)
                    .HasForeignKey(m => m.JKWPTJBahagianId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPenilaianPerolehanPerihal>()
                    .HasOne(m => m.AkPenilaianPerolehan)
                    .WithMany(t => t.AkPenilaianPerolehanPerihal)
                    .HasForeignKey(m => m.AkPenilaianPerolehanId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkNotaMintaObjek>()
                    .HasOne(m => m.AkNotaMinta)
                    .WithMany(t => t.AkNotaMintaObjek)
                    .HasForeignKey(m => m.AkNotaMintaId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkNotaMintaObjek>()
                    .HasOne(m => m.AkCarta)
                    .WithMany(t => t.AkNotaMintaObjek)
                    .HasForeignKey(m => m.AkCartaId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkNotaMintaObjek>()
                    .HasOne(m => m.JKWPTJBahagian)
                    .WithMany(t => t.AkNotaMintaObjek)
                    .HasForeignKey(m => m.JKWPTJBahagianId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkNotaMintaPerihal>()
                    .HasOne(m => m.AkNotaMinta)
                    .WithMany(t => t.AkNotaMintaPerihal)
                    .HasForeignKey(m => m.AkNotaMintaId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkInden>()
                    .HasOne(m => m.JKW)
                    .WithMany(t => t.AkInden)
                    .HasForeignKey(m => m.JKWId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkInden>()
                    .HasOne(m => m.DDaftarAwam)
                    .WithMany(t => t.AkInden)
                    .HasForeignKey(m => m.DDaftarAwamId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkIndenObjek>()
                    .HasOne(m => m.AkCarta)
                    .WithMany(t => t.AkIndenObjek)
                    .HasForeignKey(m => m.AkCartaId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkIndenObjek>()
                    .HasOne(m => m.JKWPTJBahagian)
                    .WithMany(t => t.AkIndenObjek)
                    .HasForeignKey(m => m.JKWPTJBahagianId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkIndenObjek>()
                    .HasOne(m => m.AkInden)
                    .WithMany(t => t.AkIndenObjek)
                    .HasForeignKey(m => m.AkIndenId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkIndenPerihal>()
                    .HasOne(m => m.AkInden)
                    .WithMany(t => t.AkIndenPerihal)
                    .HasForeignKey(m => m.AkIndenId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPO>()
                    .HasOne(m => m.JKW)
                    .WithMany(t => t.AkPO)
                    .HasForeignKey(m => m.JKWId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPO>()
                    .HasOne(m => m.DDaftarAwam)
                    .WithMany(t => t.AkPO)
                    .HasForeignKey(m => m.DDaftarAwamId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPOObjek>()
                    .HasOne(m => m.AkCarta)
                    .WithMany(t => t.AkPOObjek)
                    .HasForeignKey(m => m.AkCartaId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPOObjek>()
                    .HasOne(m => m.JKWPTJBahagian)
                    .WithMany(t => t.AkPOObjek)
                    .HasForeignKey(m => m.JKWPTJBahagianId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPOObjek>()
                    .HasOne(m => m.AkPO)
                    .WithMany(t => t.AkPOObjek)
                    .HasForeignKey(m => m.AkPOId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPOPerihal>()
                    .HasOne(m => m.AkPO)
                    .WithMany(t => t.AkPOPerihal)
                    .HasForeignKey(m => m.AkPOId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPelarasanPO>()
                   .HasOne(m => m.AkPO)
                   .WithMany(t => t.AkPelarasanPO)
                   .HasForeignKey(m => m.AkPOId)
                   .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPelarasanPOObjek>()
                    .HasOne(m => m.AkPelarasanPO)
                    .WithMany(t => t.AkPelarasanPOObjek)
                    .HasForeignKey(m => m.AkPelarasanPOId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPelarasanPOObjek>()
                    .HasOne(m => m.AkCarta)
                    .WithMany(t => t.AkPelarasanPOObjek)
                    .HasForeignKey(m => m.AkCartaId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPelarasanPOObjek>()
                    .HasOne(m => m.JKWPTJBahagian)
                    .WithMany(t => t.AkPelarasanPOObjek)
                    .HasForeignKey(m => m.JKWPTJBahagianId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPelarasanPOPerihal>()
                    .HasOne(m => m.AkPelarasanPO)
                    .WithMany(t => t.AkPelarasanPOPerihal)
                    .HasForeignKey(m => m.AkPelarasanPOId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPelarasanInden>()
                    .HasOne(m => m.AkInden)
                    .WithMany(t => t.AkPelarasanInden)
                    .HasForeignKey(m => m.AkIndenId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPelarasanIndenObjek>()
                    .HasOne(m => m.AkPelarasanInden)
                    .WithMany(t => t.AkPelarasanIndenObjek)
                    .HasForeignKey(m => m.AkPelarasanIndenId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPelarasanIndenObjek>()
                    .HasOne(m => m.AkCarta)
                    .WithMany(t => t.AkPelarasanIndenObjek)
                    .HasForeignKey(m => m.AkCartaId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPelarasanIndenObjek>()
                    .HasOne(m => m.JKWPTJBahagian)
                    .WithMany(t => t.AkPelarasanIndenObjek)
                    .HasForeignKey(m => m.JKWPTJBahagianId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPelarasanIndenPerihal>()
                    .HasOne(m => m.AkPelarasanInden)
                    .WithMany(t => t.AkPelarasanIndenPerihal)
                    .HasForeignKey(m => m.AkPelarasanIndenId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkBelianObjek>()
                    .HasOne(m => m.AkBelian)
                    .WithMany(t => t.AkBelianObjek)
                    .HasForeignKey(m => m.AkBelianId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkBelianObjek>()
                    .HasOne(m => m.AkCarta)
                    .WithMany(t => t.AkBelianObjek)
                    .HasForeignKey(m => m.AkCartaId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkBelianObjek>()
                    .HasOne(m => m.JKWPTJBahagian)
                    .WithMany(t => t.AkBelianObjek)
                    .HasForeignKey(m => m.JKWPTJBahagianId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkBelianPerihal>()
                    .HasOne(m => m.AkBelian)
                    .WithMany(t => t.AkBelianPerihal)
                    .HasForeignKey(m => m.AkBelianId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<SuGajiBulananPekerja>()
                    .HasOne(m => m.DPekerja)
                    .WithMany(t => t.SuGajiBulananPekerja)
                    .HasForeignKey(m => m.DPekerjaId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<SuGajiBulananPekerja>()
                    .HasOne(m => m.SuGajiBulanan)
                    .WithMany(t => t.SuGajiBulananPekerja)
                    .HasForeignKey(m => m.SuGajiBulananId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPVPenerima>()
                    .HasOne(m => m.JCaraBayar)
                    .WithMany(t => t.AkPVPenerima)
                    .HasForeignKey(m => m.JCaraBayarId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPVPenerima>()
                    .HasOne(m => m.AkJanaanProfilPenerima)
                    .WithMany(t => t.AkPVPenerima)
                    .HasForeignKey(m => m.AkJanaanProfilPenerimaId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPVPenerima>()
                    .HasOne(m => m.AkPV)
                    .WithMany(t => t.AkPVPenerima)
                    .HasForeignKey(m => m.AkPVId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPVObjek>()
                    .HasOne(m => m.AkCarta)
                    .WithMany(t => t.AkPVObjek)
                    .HasForeignKey(m => m.AkCartaId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPVObjek>()
                    .HasOne(m => m.AkPV)
                    .WithMany(t => t.AkPVObjek)
                    .HasForeignKey(m => m.AkPVId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPVInvois>()
                    .HasOne(m => m.AkBelian)
                    .WithMany(t => t.AkPVInvois)
                    .HasForeignKey(m => m.AkBelianId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPVInvois>()
                    .HasOne(m => m.AkPV)
                    .WithMany(t => t.AkPVInvois)
                    .HasForeignKey(m => m.AkPVId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPV>()
                    .HasOne(m => m.AkBank)
                    .WithMany(t => t.AkPV)
                    .HasForeignKey(m => m.AkBankId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkCVObjek>()
                    .HasOne(m => m.AkCarta)
                    .WithMany(t => t.AkCVObjek)
                    .HasForeignKey(m => m.AkCartaId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            //
            modelBuilder.Entity<AkJurnalObjek>()
                    .HasOne(m => m.AkJurnal)
                    .WithMany(t => t.AkJurnalObjek)
                    .HasForeignKey(m => m.AkJurnalId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkJurnalObjek>()
                    .HasOne(m => m.AkCartaDebit)
                    .WithMany(t => t.AkJurnalObjekDebit)
                    .HasForeignKey(m => m.AkCartaDebitId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkJurnalObjek>()
                    .HasOne(m => m.JKWPTJBahagianDebit)
                    .WithMany(t => t.AkJurnalObjekDebit)
                    .HasForeignKey(m => m.JKWPTJBahagianDebitId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkJurnalObjek>()
                    .HasOne(m => m.JKWPTJBahagianKredit)
                    .WithMany(t => t.AkJurnalObjekKredit)
                    .HasForeignKey(m => m.JKWPTJBahagianKreditId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkJurnalObjek>()
                    .HasOne(m => m.AkCartaKredit)
                    .WithMany(t => t.AkJurnalObjekKredit)
                    .HasForeignKey(m => m.AkCartaKreditId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkJurnalPenerimaCekBatal>()
                    .HasOne(m => m.AkJurnal)
                    .WithMany(t => t.AkJurnalPenerimaCekBatal)
                    .HasForeignKey(m => m.AkJurnalId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkJurnalPenerimaCekBatal>()
                    .HasOne(m => m.AkPV)
                    .WithMany(t => t.AkJurnalPenerimaCekBatal)
                    .HasForeignKey(m => m.AkPVId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);


            //
            modelBuilder.Entity<AkCVObjek>()
                    .HasOne(m => m.AkCV)
                    .WithMany(t => t.AkCVObjek)
                    .HasForeignKey(m => m.AkCVId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<DPekerjaElaunPotongan>()
                    .HasOne(m => m.DPekerja)
                    .WithMany(t => t.DPekerjaElaunPotongan)
                    .HasForeignKey(m => m.DPekerjaId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            
            modelBuilder.Entity<DPekerjaElaunPotongan>()
                    .HasOne(m => m.JElaunPotongan)
                    .WithMany(t => t.DPekerjaElaunPotongan)
                    .HasForeignKey(m => m.JElaunPotonganId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<SuGajiElaunPotongan>()
                    .HasOne(m => m.JElaunPotongan)
                    .WithMany(t => t.SuGajiElaunPotongan)
                    .HasForeignKey(m => m.JElaunPotonganId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkRekup>()
                    .HasOne(m => m.DPanjar)
                    .WithMany(t => t.AkRekup)
                    .HasForeignKey(m => m.DPanjarId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<DPanjarPemegang>()
                    .HasOne(m => m.DPanjar)
                    .WithMany(t => t.DPanjarPemegang)
                    .HasForeignKey(m => m.DPanjarId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<DPanjarPemegang>()
                    .HasOne(m => m.DPekerja)
                    .WithMany(t => t.DPanjarPemegang)
                    .HasForeignKey(m => m.DPekerjaId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPanjarLejar>()
                    .HasOne(m => m.DPanjar)
                    .WithMany(t => t.AkPanjarLejar)
                    .HasForeignKey(m => m.DPanjarId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkNotaDebitKreditDiterimaObjek>()
                    .HasOne(m => m.AkCarta)
                    .WithMany(t => t.AkNotaDebitKreditDiterimaObjek)
                    .HasForeignKey(m => m.AkCartaId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkNotaDebitKreditDiterimaObjek>()
                    .HasOne(m => m.AkNotaDebitKreditDiterima)
                    .WithMany(t => t.AkNotaDebitKreditDiterimaObjek)
                    .HasForeignKey(m => m.AkNotaDebitKreditDiterimaId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkNotaDebitKreditDiterimaPerihal>()
                    .HasOne(m => m.AkNotaDebitKreditDiterima)
                    .WithMany(t => t.AkNotaDebitKreditDiterimaPerihal)
                    .HasForeignKey(m => m.AkNotaDebitKreditDiterimaId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkNotaDebitKreditDiterima>()
                    .HasOne(m => m.JKW)
                    .WithMany(t => t.AkNotaDebitKreditDiterima)
                    .HasForeignKey(m => m.JKWId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkEFTPenerima>()
                    .HasOne(m => m.AkEFT)
                    .WithMany(t => t.AkEFTPenerima)
                    .HasForeignKey(m => m.AkEFTId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkJanaanProfilPenerima>()
                .HasOne(m => m.AkJanaanProfil)
                .WithMany(t => t.AkJanaanProfilPenerima)
                .HasForeignKey(m => m.AkJanaanProfilId)
                .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkJanaanProfilPenerima>()
                .HasOne(m => m.JCaraBayar)
                .WithMany(t => t.AkJanaanProfilPenerima)
                .HasForeignKey(m => m.JCaraBayarId)
                .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkJanaanProfil>()
                .HasOne(m => m.JCawangan)
                .WithMany(t => t.AkJanaanProfil)
                .HasForeignKey(m => m.JCawanganId)
                .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkEFT>()
                .HasOne(m => m.AkBank)
                .WithMany(t => t.AkEFT)
                .HasForeignKey(m => m.AkBankId)
                .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkEFTPenerima>()
                .HasOne(m => m.AkPV)
                .WithMany(t => t.AkEFTPenerima)
                .HasForeignKey(m => m.AkPVId)
                .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkEFTPenerima>()
                .HasOne(m => m.JCaraBayar)
                .WithMany(t => t.AkEFTPenerima)
                .HasForeignKey(m => m.JCaraBayarId)
                .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<DPenerimaCekGaji>()
                .HasOne(m => m.JCaraBayar)
                .WithMany(t => t.DPenerimaCekGaji)
                .HasForeignKey(m => m.JCaraBayarId)
                .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<JKonfigPerubahanEkuitiBaris>()
                .HasOne(m => m.JKonfigPerubahanEkuiti)
                .WithMany(t => t.JKonfigPerubahanEkuitiBaris)
                .HasForeignKey(m => m.JKonfigPerubahanEkuitiId)
                .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<JKonfigPenyataBarisFormula>()
                .HasOne(m => m.JKonfigPenyataBaris)
                .WithMany(t => t.JKonfigPenyataBarisFormula)
                .HasForeignKey(m => m.JKonfigPenyataBarisId)
                .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<JKonfigPenyataBaris>()
                .HasOne(m => m.JKonfigPenyata)
                .WithMany(t => t.JKonfigPenyataBaris)
                .HasForeignKey(m => m.JKonfigPenyataId)
                .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkPanjarLejar>()
                .HasOne(m => m.AkCarta)
                .WithMany(t => t.AkPanjarLejar)
                .HasForeignKey(m => m.AkCartaId)
                .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkInvoisObjek>()
                    .HasOne(m => m.AkInvois)
                    .WithMany(t => t.AkInvoisObjek)
                    .HasForeignKey(m => m.AkInvoisId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkInvoisObjek>()
                    .HasOne(m => m.AkCarta)
                    .WithMany(t => t.AkInvoisObjek)
                    .HasForeignKey(m => m.AkCartaId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkInvoisObjek>()
                    .HasOne(m => m.JKWPTJBahagian)
                    .WithMany(t => t.AkInvoisObjek)
                    .HasForeignKey(m => m.JKWPTJBahagianId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkInvoisPerihal>()
                    .HasOne(m => m.AkInvois)
                    .WithMany(t => t.AkInvoisPerihal)
                    .HasForeignKey(m => m.AkInvoisId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkInvois>()
                    .HasOne(m => m.DDaftarAwam)
                    .WithMany(t => t.AkInvois)
                    .HasForeignKey(m => m.DDaftarAwamId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkInvois>()
                    .HasOne(m => m.JKW)
                    .WithMany(t => t.AkInvois)
                    .HasForeignKey(m => m.JKWId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkNotaDebitKreditDikeluarkanObjek>()
                    .HasOne(m => m.AkCarta)
                    .WithMany(t => t.AkNotaDebitKreditDikeluarkanObjek)
                    .HasForeignKey(m => m.AkCartaId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkNotaDebitKreditDikeluarkanObjek>()
                    .HasOne(m => m.AkNotaDebitKreditDikeluarkan)
                    .WithMany(t => t.AkNotaDebitKreditDikeluarkanObjek)
                    .HasForeignKey(m => m.AkNotaDebitKreditDikeluarkanId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkNotaDebitKreditDikeluarkanPerihal>()
                    .HasOne(m => m.AkNotaDebitKreditDikeluarkan)
                    .WithMany(t => t.AkNotaDebitKreditDikeluarkanPerihal)
                    .HasForeignKey(m => m.AkNotaDebitKreditDikeluarkanId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkNotaDebitKreditDikeluarkan>()
                    .HasOne(m => m.JKW)
                    .WithMany(t => t.AkNotaDebitKreditDikeluarkan)
                    .HasForeignKey(m => m.JKWId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        }
    }
}
