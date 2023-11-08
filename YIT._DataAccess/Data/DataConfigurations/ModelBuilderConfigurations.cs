﻿using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT.__Domain.Entities.Models._03Akaun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            //

            // Daftar
            modelBuilder.Entity<DPekerja>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<DPenyemak>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<DPelulus>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<DDaftarAwam>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            //

            // Akaun
            modelBuilder.Entity<AkAkaun>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);

            modelBuilder.Entity<AkCarta>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            modelBuilder.Entity<AkBank>().HasQueryFilter(m => EF.Property<int>(m, "FlHapus") == 0);
            //
        }

        public static void SeedEntitiesProperties(this ModelBuilder modelBuilder)
        {
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

            modelBuilder.Entity<AkTerimaObjek>()
                    .HasOne(m => m.AkCarta)
                    .WithMany(t => t.AkTerimaObjek)
                    .HasForeignKey(m => m.AkCartaId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkTerimaObjek>()
                    .HasOne(m => m.JBahagian)
                    .WithMany(t => t.AkTerimaObjek)
                    .HasForeignKey(m => m.JBahagianId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);

            modelBuilder.Entity<AkTerimaCaraBayar>()
                    .HasOne(m => m.JCaraBayar)
                    .WithMany(t => t.AkTerimaCaraBayar)
                    .HasForeignKey(m => m.JCaraBayarId)
                    .OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        }
    }
}