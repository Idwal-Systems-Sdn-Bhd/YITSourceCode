﻿using Microsoft.AspNetCore.Identity;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._00Sistem;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._02Daftar;

namespace YIT._DataAccess.Data.DataConfigurations
{
    public static class SeedData
    {
        public static void SeedUsers(UserManager<IdentityUser> userManager, ApplicationDbContext db)
        {
            var results = userManager.FindByEmailAsync(Init.superAdminEmail).Result;

            if (results == null)
            {
                var user = new ApplicationUser
                {
                    UserName = Init.superAdminEmail,
                    Email = Init.superAdminEmail,
                    Nama = Init.superAdminName
                };

                IdentityResult result = userManager.CreateAsync(user, Init.superAdminPassword).Result;

                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(user, Init.superAdminName).Wait();
                }
            }
            else
            {
                userManager.AddToRoleAsync(results, Init.superAdminName).Wait();

                if (db.UserClaims.FirstOrDefault(uc => uc.UserId == results.Id) == null)
                {
                    List<IdentityUserClaim<string>> claimForUser = new List<IdentityUserClaim<string>>()
                    {
                        new IdentityUserClaim<string> { UserId = results.Id, ClaimType = "JD000", ClaimValue = "JD000 Jadual" },
                        new IdentityUserClaim<string> { UserId = results.Id, ClaimType = "DF001", ClaimValue = "DF001 Daftar Awam" },
                        new IdentityUserClaim<string> { UserId = results.Id, ClaimType = "PR001", ClaimValue = "PR001 Resit Rasmi" },
                        new IdentityUserClaim<string> { UserId = results.Id, ClaimType = "LP000", ClaimValue = "LP000 Laporan" }
                    };

                    db.UserClaims.AddRangeAsync(claimForUser).Wait();

                    db.SaveChanges();
                }
            }
        }

        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // -- First Layer Insert
            if (context.SiAppInfo.Any())
            {

            }
            else
            {
                CompanyDetails company = new CompanyDetails();

                context.SiAppInfo.AddRange(
                    new SiAppInfo
                    {
                        TarVersi = DateTime.Today,
                        KodSistem = Init.CompSystemCode,
                        NamaSyarikat = Init.CompName,
                        NoPendaftaran = Init.CompRegNo,
                        AlamatSyarikat1 = Init.CompAddress1,
                        AlamatSyarikat2 = Init.CompAddress2,
                        AlamatSyarikat3 = Init.CompAddress3,
                        Bandar = Init.CompCity,
                        Poskod = Init.CompPoscode,
                        Daerah = Init.CompDistrict,
                        Negeri = Init.CompState,
                        TelSyarikat = Init.CompTel,
                        FaksSyarikat = Init.CompFax,
                        EmelSyarikat = Init.CompEmail,
                        CompanyLogoWeb = Init.CompWebLogo,
                        CompanyLogoPrintPDF = Init.CompPrintLogo

                    }
                );
            }

            if (context.JCaraBayar.Any())
            {
                //return;
            }
            else
            {
                context.JCaraBayar.AddRange(
                    new JCaraBayar
                    {
                        Kod = "T",
                        Perihal = "TUNAI"
                    },
                    new JCaraBayar
                    {
                        Kod = "C",
                        Perihal = "CEK / WANG POS"
                    },
                    new JCaraBayar
                    {
                        Kod = "M",
                        Perihal = "MAKLUMAN KREDIT"
                    },
                    new JCaraBayar
                    {
                        Kod = "E",
                        Perihal = "EFT"
                    },
                    new JCaraBayar
                    {
                        Kod = "I",
                        Perihal = "IBG"
                    },
                    new JCaraBayar
                    {
                        Kod = "K",
                        Perihal = "KAD KREDIT"
                    },
                    new JCaraBayar
                    {
                        Kod = "JP",
                        Perihal = "JOMPAY"
                    }
                );
            }
            
            if (context.Roles.Any())
            {

            }
            else
            {
                context.Roles.AddRange(
                    new IdentityRole { Name = "SuperAdmin", NormalizedName = "SuperAdmin".ToUpper() },
                   new IdentityRole { Name = "Admin", NormalizedName = "Admin".ToUpper() },
                    new IdentityRole { Name = "Supervisor", NormalizedName = "Supervisor".ToUpper() },
                    new IdentityRole { Name = "User", NormalizedName = "User".ToUpper() }
                    );
            }
            //kalau dlm database, nama table J Negeri ada isi
            if (context.JNegeri.Any())
            {
                //return;   // DB has been seeded
            }
            else
            {
                context.JNegeri.AddRange(
                    new JNegeri
                    {
                        Kod = "01",
                        Perihal = "JOHOR"
                    },
                    new JNegeri
                    {
                        Kod = "02",
                        Perihal = "KEDAH"
                    },
                    new JNegeri
                    {
                        Kod = "03",
                        Perihal = "KELANTAN"
                    },
                    new JNegeri
                    {
                        Kod = "04",
                        Perihal = "MELAKA"
                    },
                    new JNegeri
                    {
                        Kod = "05",
                        Perihal = "NEGERI SEMBILAN"
                    },
                    new JNegeri
                    {
                        Kod = "06",
                        Perihal = "PAHANG"
                    },
                    new JNegeri
                    {
                        Kod = "07",
                        Perihal = "PULAU PINANG"
                    },
                    new JNegeri
                    {
                        Kod = "08",
                        Perihal = "PERAK"
                    },
                    new JNegeri
                    {
                        Kod = "09",
                        Perihal = "PERLIS"
                    },
                    new JNegeri
                    {
                        Kod = "10",
                        Perihal = "SELANGOR"
                    },
                    new JNegeri
                    {
                        Kod = "11",
                        Perihal = "TERENGGANU"
                    },
                    new JNegeri
                    {
                        Kod = "12",
                        Perihal = "SABAH"
                    },
                    new JNegeri
                    {
                        Kod = "13",
                        Perihal = "SARAWAK"
                    },
                    new JNegeri
                    {
                        Kod = "14",
                        Perihal = "WILAYAH PERSEKUTUAN (KUALA LUMPUR)"
                    },
                    new JNegeri
                    {
                        Kod = "15",
                        Perihal = "WILAYAH PERSEKUTUAN (LABUAN)"
                    },
                    new JNegeri
                    {
                        Kod = "16",
                        Perihal = "WILAYAH PERSEKUTUAN (PUTRAJAYA)"
                    }
                );
            }

            if (context.JAgama.Any())
            {
                //return;   // DB has been seeded
            }
            else
            {
                context.JAgama.AddRange(
                    new JAgama
                    {
                        Perihal = "ISLAM"
                    },

                    new JAgama
                    {
                        Perihal = "BUDDHA"
                    },

                    new JAgama
                    {
                        Perihal = "KRISTIAN"
                    },
                    new JAgama
                    {
                        Perihal = "HINDU"
                    },
                    new JAgama
                    {
                        Perihal = "TIADA AGAMA"
                    },
                    new JAgama
                    {
                        Perihal = "LAIN-LAIN"
                    }

                );
            }
            //seed tuk jbank, jbank ada 5, 1 maybank, 02 bank islam, 03 affin, 04 hong leong
            if (context.JBangsa.Any())
            {
                //return;   // DB has been seeded
            }
            else
            {
                context.JBangsa.AddRange(
                    new JBangsa
                    {
                        Perihal = "MELAYU",
                    },

                    new JBangsa
                    {
                        Perihal = "CINA"
                    },

                    new JBangsa
                    {
                        Perihal = "INDIA"
                    },
                    new JBangsa
                    {
                        Perihal = "LAIN-LAIN"
                    }

                );
            }

            if (context.JKW.Any())
            {

            }
            else
            {
                // ** Ubah di sini
                context.JKW.AddRange(
                    new JKW
                    {
                        Kod = "1",
                        Perihal = "YAYASAN ISLAM TERENGGANU"
                    });
                
            }

            if (context.JBank.Any())
            {

            }
            else
            {
                // ** Ubah di sini
                context.JBank.AddRange(
                    new JBank
                    {
                        Kod = "01",
                        Perihal = "AFFIN BANK BERHAD",
                        KodBNMEFT = "PHBMMYKL",
                        Length1 = 12
                    },
                    new JBank
                    {
                        Kod = "02",
                        Perihal = "AFFIN ISLAMIC BANK",
                        KodBNMEFT = "PHBMMYKL",
                        Length2 = 12
                    },
                     new JBank
                     {
                         Kod = "03",
                         Perihal = "ALLIANCE BANK",
                         KodBNMEFT = "MFBBMYKL",
                         Length1 = 15
                     },
                      new JBank
                      {
                          Kod = "04",
                          Perihal = "ALLIANCE ISLAMIC BANK",
                          KodBNMEFT = "MFBBMYKL",
                          Length1 = 15
                      },
                      new JBank
                      {
                          Kod = "05",
                          Perihal = "AL-RAJHI BANKING INVESTMENT",
                          KodBNMEFT = "RJHIMYKL",
                          Length1 = 15

                      },
                      new JBank
                      {
                          Kod = "06",
                          Perihal = "AMBANK",
                          KodBNMEFT = "ARBKMYKL",
                          Length1 = 13,
                          Length2 = 12
                      },
                      new JBank
                      {
                          Kod = "07",
                          Perihal = "AMBANK ISLAMIC",
                          KodBNMEFT = "ARBKMYKL",
                          Length1 = 13
                      },
                      new JBank
                      {
                          Kod = "08",
                          Perihal = "BANK ISLAM",
                          KodBNMEFT = "BIMBMYKL",
                          Length1 = 14
                      },
                      new JBank
                      {
                          Kod = "09",
                          Perihal = "BANK MUAMALAT",
                          KodBNMEFT = "BMMBMYKL",
                          Length1 = 14
                      },
                      new JBank
                      {
                          Kod = "10",
                          Perihal = "BANK RAKYAT",
                          KodBNMEFT = "BKRMMYKL",
                          Length1 = 12,
                          Length2 = 10
                      },
                      new JBank
                      {
                          Kod = "11",
                          Perihal = "BANK SIMPANAN NASIONAL",
                          KodBNMEFT = "BSNAMYK1",
                          Length1 = 16,
                          Length2 = 21
                      },
                      new JBank
                      {
                          Kod = "12",
                          Perihal = "BANK SIMPANAN NASIONAL - SPI",
                          KodBNMEFT = "BSNAMYK1",
                          Length1 = 16,
                          Length2 = 21
                      },
                      new JBank
                      {
                          Kod = "13",
                          Perihal = "BANK PERTANIAN MALAYSIA",
                          KodBNMEFT = "AGOBMYK1",
                          Length1 = 16
                      },
                      new JBank
                      {
                          Kod = "14",
                          Perihal = "CIMB (BCB)",
                          KodBNMEFT = "CIBBMYKL",
                          Length1 = 14,
                          Length2 = 10
                      },
                      new JBank
                      {
                          Kod = "15",
                          Perihal = "CIMB ISLAMIC",
                          KodBNMEFT = "CIBBMYKL",
                          Length1 = 14,
                          Length2 = 10
                      },
                      new JBank
                      {
                          Kod = "16",
                          Perihal = "CIMB (SOUTHERN)",
                          KodBNMEFT = "CIBBMYKL",
                          Length1 = 14,
                          Length2 = 10
                      },
                      new JBank
                      {
                          Kod = "17",
                          Perihal = "EON BANK BHD",
                          KodBNMEFT = "HLBBMYKL",
                          Length1 = 17 //kiv
                      },
                      new JBank
                      {
                          Kod = "18",
                          Perihal = "EONCAP ISLAMIC BANK",
                          KodBNMEFT = "HLBBMYKL",
                          Length1 = 18 //kiv
                      },
                      new JBank
                      {
                          Kod = "19",
                          Perihal = "HONG LEONG",
                          KodBNMEFT = "HLBBMYKL",
                          Length1 = 13,
                          Length2 = 11
                      },
                      new JBank
                      {
                          Kod = "20",
                          Perihal = "HONG LEONG ISLAMIC BANK",
                          KodBNMEFT = "HLBBMYKL",
                          Length1 = 13
                      },
                      new JBank
                      {
                          Kod = "21",
                          Perihal = "HSBC AMANAH",
                          KodBNMEFT = "HBMBMYKL",
                          Length1 = 17,
                          Length2 = 12
                      },
                      new JBank
                      {
                          Kod = "22",
                          Perihal = "HSBC - ISLAMIC",
                          KodBNMEFT = "HBMBMYKL",
                          Length1 = 17
                      },
                      new JBank
                      {
                          Kod = "23",
                          Perihal = "HSBC",
                          KodBNMEFT = "HBMBMYKL",
                          Length1 = 17
                      },
                      new JBank
                      {
                          Kod = "24",
                          Perihal = "MAYBANK",
                          KodBNMEFT = "MBBEMYKL",
                          Length1 = 12
                      },
                      new JBank
                      {
                          Kod = "25",
                          Perihal = "MAYBANK ISLAMIC",
                          KodBNMEFT = "MBBEMYKL",
                          Length1 = 12
                      },
                      new JBank
                      {
                          Kod = "26",
                          Perihal = "OCBC BANK BHD",
                          KodBNMEFT = "OCBCMYKL",
                          Length1 = 10
                      },
                      new JBank
                      {
                          Kod = "27",
                          Perihal = "OCBC AL AMIN BANK",
                          KodBNMEFT = "OCBCMYKL",
                          Length1 = 10
                      },
                      new JBank
                      {
                          Kod = "28",
                          Perihal = "PUBLIC BANK BERHAD",
                          KodBNMEFT = "PBBEMYKL",
                          Length1 = 10
                      },
                      new JBank
                      {
                          Kod = "29",
                          Perihal = "PUBLIC ISLAMIC BANK BERHAD",
                          KodBNMEFT = "PBBEMYKL",
                          Length1 = 10
                      },
                      new JBank
                      {
                          Kod = "30",
                          Perihal = "RHB BANK BERHAD",
                          KodBNMEFT = "RHBBMYKL",
                          Length1 = 14
                      },
                      new JBank
                      {
                          Kod = "31",
                          Perihal = "RHB ISLAMIC BANK",
                          KodBNMEFT = "RHBBMYKL",
                          Length1 = 14
                      },
                      new JBank
                      {
                          Kod = "32",
                          Perihal = "STANDARD CHARTERED",
                          KodBNMEFT = "SCBLMYKX",
                          Length1 = 17
                      },
                      new JBank
                      {
                          Kod = "33",
                          Perihal = "UNITED OVERSEAS BANK",
                          KodBNMEFT = "UOVBMYKL",
                          Length1 = 17
                      },
                      new JBank
                      {
                          Kod = "34",
                          Perihal = "CITI BANK BHD",
                          KodBNMEFT = "CITIMYKL",
                          Length1 = 16
                      },
                      new JBank
                      {
                          Kod = "75",
                          Perihal = "MBSB BANK BHD",
                          KodBNMEFT = "AFBQMYKL",
                          Length1 = 16
                      }
                    );

            }

            //if (context.AkCarta.Any())
            //{
            //}
            //else
            //{
            //    {
            //        context.AkCarta.AddRange(
            //            new AkCarta
            //            {
            //                Kod = "A10000",
            //                Perihal = "ASET SEMASA",
            //                DebitKredit = "D",
            //                UmumDetail = "U",
            //                Baki = 0,
            //                EnJenis = EnJenisCarta.Aset,
            //                EnParas = EnParas.Paras1
            //            },
            //            new AkCarta
            //            {
            //                Kod = "A11000",
            //                Perihal = "WANG TUNAI DAN BAKI BANK",
            //                DebitKredit = "D",
            //                UmumDetail = "U",
            //                Baki = 0,
            //                EnJenis = EnJenisCarta.Aset,
            //                EnParas = EnParas.Paras2
            //            },
            //        new AkCarta
            //        {
            //            Kod = "A11100",
            //            Perihal = "WANG TUNAI DAN BAKI BANK",
            //            DebitKredit = "D",
            //            UmumDetail = "U",
            //            Baki = 0,
            //            EnJenis = EnJenisCarta.Aset,
            //            EnParas = EnParas.Paras3
            //        },
            //        new AkCarta
            //        {
            //            Kod = "A11101",
            //            Perihal = "AKAUN BANK UTAMA",
            //            DebitKredit = "D",
            //            UmumDetail = "D",
            //            Baki = 0,
            //            EnJenis = EnJenisCarta.Aset,
            //            EnParas = EnParas.Paras4
            //        },
            //        //
            //        // Belanja
            //        new AkCarta
            //        {
            //            Kod = "B10000",
            //            Perihal = "GAJI DAN UPAH",
            //            DebitKredit = "D",
            //            UmumDetail = "U",
            //            Baki = 0,
            //            EnJenis = EnJenisCarta.Belanja,
            //            EnParas = EnParas.Paras1
            //        },
            //        new AkCarta
            //        {
            //            Kod = "B11000",
            //            Perihal = "GAJI DAN UPAH",
            //            DebitKredit = "D",
            //            UmumDetail = "U",
            //            Baki = 0,
            //            EnJenis = EnJenisCarta.Belanja,
            //            EnParas = EnParas.Paras2
            //        },
            //        new AkCarta
            //        {
            //            Kod = "B11100",
            //            Perihal = "GAJI DAN UPAH KAKITANGAN",
            //            DebitKredit = "D",
            //            UmumDetail = "U",
            //            Baki = 0,
            //            EnJenis = EnJenisCarta.Belanja,
            //            EnParas = EnParas.Paras3
            //        },
            //        new AkCarta
            //        {
            //            Kod = "B11101",
            //            Perihal = "GAJI DAN UPAH - KAKITANGAN",
            //            DebitKredit = "D",
            //            UmumDetail = "D",
            //            Baki = 0,
            //            EnJenis = EnJenisCarta.Belanja,
            //            EnParas = EnParas.Paras4
            //        },
            //        // LIABILITI
            //        new AkCarta
            //        {
            //            Kod = "L10000",
            //            Perihal = "LIABILITI SEMASA",
            //            DebitKredit = "K",
            //            UmumDetail = "U",
            //            Baki = 0,
            //            EnJenis = EnJenisCarta.Liabiliti,
            //            EnParas = EnParas.Paras1
            //        },
            //        new AkCarta
            //        {
            //            Kod = "L11000",
            //            Perihal = "AKAUN BELUM BAYAR",
            //            DebitKredit = "K",
            //            UmumDetail = "U",
            //            Baki = 0,
            //            EnJenis = EnJenisCarta.Liabiliti,
            //            EnParas = EnParas.Paras2
            //        },
            //        new AkCarta
            //        {
            //            Kod = "L11100",
            //            Perihal = "AKAUN BELUM BAYAR",
            //            DebitKredit = "K",
            //            UmumDetail = "U",
            //            Baki = 0,
            //            EnJenis = EnJenisCarta.Liabiliti,
            //            EnParas = EnParas.Paras3
            //        },
            //        new AkCarta
            //        {
            //            Kod = "L11101",
            //            Perihal = "AKAUN BELUM BAYAR",
            //            DebitKredit = "K",
            //            UmumDetail = "D",
            //            Baki = 0,
            //            EnJenis = EnJenisCarta.Liabiliti,
            //            EnParas = EnParas.Paras4
            //        },
            //        // EKUITI
            //        new AkCarta
            //        {
            //            Kod = "E10000",
            //            Perihal = "EKUITI",
            //            DebitKredit = "K",
            //            UmumDetail = "U",
            //            Baki = 0,
            //            EnJenis = EnJenisCarta.Ekuiti,
            //            EnParas = EnParas.Paras1
            //        },
            //        new AkCarta
            //        {
            //            Kod = "E11000",
            //            Perihal = "RIZAB",
            //            DebitKredit = "K",
            //            UmumDetail = "U",
            //            Baki = 0,
            //            EnJenis = EnJenisCarta.Ekuiti,
            //            EnParas = EnParas.Paras2
            //        },
            //        new AkCarta
            //        {
            //            Kod = "E11100",
            //            Perihal = "RIZAB",
            //            DebitKredit = "K",
            //            UmumDetail = "U",
            //            Baki = 0,
            //            EnJenis = EnJenisCarta.Ekuiti,
            //            EnParas = EnParas.Paras3
            //        },
            //        new AkCarta
            //        {
            //            Kod = "E11101",
            //            Perihal = "RIZAB PENILAIAN SEMULA TANAH",
            //            DebitKredit = "K",
            //            UmumDetail = "D",
            //            Baki = 0,
            //            EnJenis = EnJenisCarta.Ekuiti,
            //            EnParas = EnParas.Paras4
            //        },
            //        // HASIL
            //        new AkCarta
            //        {
            //            Kod = "H70000",
            //            Perihal = "HASIL",
            //            DebitKredit = "K",
            //            UmumDetail = "U",
            //            Baki = 0,
            //            EnJenis = EnJenisCarta.Hasil,
            //            EnParas = EnParas.Paras1
            //        },
            //        new AkCarta
            //        {
            //            Kod = "H71000",
            //            Perihal = "PELBAGAI TERIMAAN UNTUK PERKHIDMATAN",
            //            DebitKredit = "K",
            //            UmumDetail = "U",
            //            Baki = 0,
            //            EnJenis = EnJenisCarta.Hasil,
            //            EnParas = EnParas.Paras2
            //        },
            //        new AkCarta
            //        {
            //            Kod = "H71100",
            //            Perihal = "KOMISEN ATAS SUMBANGAN",
            //            DebitKredit = "K",
            //            UmumDetail = "U",
            //            Baki = 0,
            //            EnJenis = EnJenisCarta.Hasil,
            //            EnParas = EnParas.Paras3
            //        },
            //        new AkCarta
            //        {
            //            Kod = "H71101",
            //            Perihal = "KOMISEN ATAS SUMBANGAN",
            //            DebitKredit = "K",
            //            UmumDetail = "D",
            //            Baki = 0,
            //            EnJenis = EnJenisCarta.Hasil,
            //            EnParas = EnParas.Paras4
            //        }
            //            );
            //    }
            //}

            if (context.JPTJ.Any())
            {

            }
            else
            {
                context.JPTJ.AddRange(
                    new JPTJ
                    {
                        Kod = "01",
                        Perihal = "CARUMAN KERAJAAN NEGERI"
                    },
                    new JPTJ
                    {
                        Kod = "02",
                        Perihal = "JAKIM"
                    }
                    );
            }

            if (context.JBahagian.Any())
            {

            }
            else
            {
                // ** Ubah di sini
                context.JBahagian.AddRange(
                    new JBahagian
                    {
                        Kod = "01",
                        Perihal = "PENTADBIRAN"
                    },
                    new JBahagian
                    {
                        Kod = "02",
                        Perihal = "TADIKA"
                    },
                    new JBahagian
                    {
                        Kod = "03",
                        Perihal = "PELABURAN"
                    }, new JBahagian
                    {
                        Kod = "04",
                        Perihal = "KHIDMAT MASYARAKAT"
                    }, new JBahagian
                    {
                        Kod = "05",
                        Perihal = "DAKWAH"
                    }, new JBahagian
                    {
                        Kod = "06",
                        Perihal = "PENERBITAN"
                    }, new JBahagian
                    {
                        Kod = "07",
                        Perihal = "INSPI/PDI"
                    }, new JBahagian
                    {
                        Kod = "08",
                        Perihal = "SRAYIT"
                    },
                    new JBahagian
                    {
                        Kod = "09",
                        Perihal = "KAFA"
                    },
                    new JBahagian
                    {
                        Kod = "10",
                        Perihal = "PONDOK DARUL IMAN"
                    },
                    new JBahagian
                    {
                        Kod = "11",
                        Perihal = "PUSAT DAKWAH UMMAH"
                    },
                    new JBahagian
                    {
                        Kod = "12",
                        Perihal = "PENDIDIKAN KHAS"
                    },
                    new JBahagian
                    {
                        Kod = "13",
                        Perihal = "AUDIT DALAM"
                    }
                    
                    // ** Tambah di sini
                    );
            }

            if (context.JCukai.Any())
            {

            }
            else
            {
                context.JCukai.AddRange(
                    new JCukai
                    {
                        Kod = "AJP",
                        Perihal = "ANY ADJUSTMENT MADE TO INPUT TAX",
                        Peratus = 6
                    },
                    new JCukai
                    {
                        Kod = "AJS",
                        Perihal = "ANY ADJUSTMENT MADE TO OUTPUT TAX",
                        Peratus = 6
                    },
                    new JCukai
                    {
                        Kod = "BL",
                        Perihal = "PURCHASES WITH GST INCURRED BUT NOT CLAIMABLE (DISALLOWANCE OF INPUT TAX)",
                        Peratus = 0
                    },
                    new JCukai
                    {
                        Kod = "DS",
                        Perihal = "DEEMED SUPPLIES (EG. TRANSFER OR DISPOSAL OF BUSINESS ASSETS)",
                        Peratus = 0
                    },
                    new JCukai
                    {
                        Kod = "EP",
                        Perihal = "PURCAHSES EXEMPTED FROM GST",
                        Peratus = 0
                    },
                    new JCukai
                    {
                        Kod = "ES",
                        Perihal = "EXEMPT SUPPLIES UNDER GST",
                        Peratus = 0
                    },
                    new JCukai
                    {
                        Kod = "ES43",
                        Perihal = "INCIDENTAL EXEMPT SUPPLIES",
                        Peratus = 0
                    },
                    new JCukai
                    {
                        Kod = "GP",
                        Perihal = "PURCHASE TRANSACTIONS WHICH DISREGARDED UNDER GST LEGISLATION",
                        Peratus = 0
                    },
                    new JCukai
                    {
                        Kod = "GS",
                        Perihal = "DISREGARDED SUPPLIES",
                        Peratus = 0
                    },
                    new JCukai
                    {
                        Kod = "IM",
                        Perihal = "IMPORT OF GOODS WITH GST INCURRED",
                        Peratus = 0
                    },
                    new JCukai
                    {
                        Kod = "IS",
                        Perihal = "IMPORTS UNDER SPECIAL SCHEME WITH NO GST INCURRED",
                        Peratus = 0
                    },
                    new JCukai
                    {
                        Kod = "NR",
                        Perihal = "PURCHASE FROM NON GST-REGISTERED SUPPLIER WITH NO GST INCURRED",
                        Peratus = 0
                    },
                    new JCukai
                    {
                        Kod = "OP",
                        Perihal = "PURCHASE TRANSACTIONS WHICH IS OUT OF THE SCOPE OF GST LEGISLATION",
                        Peratus = 0
                    },
                    new JCukai
                    {
                        Kod = "OS",
                        Perihal = "OUT-OF-SCOPE SUPPLIES",
                        Peratus = 0
                    },
                    new JCukai
                    {
                        Kod = "RS",
                        Perihal = "RELIEF SUPPLY UNDER GST",
                        Peratus = 0
                    },
                    new JCukai
                    {
                        Kod = "SR",
                        Perihal = "STANDARD-RATED SUPPLIES WITH GST CHARGED",
                        Peratus = 0
                    },
                    new JCukai
                    {
                        Kod = "TX",
                        Perihal = "PURCHASE WITH GST INCURRED AT 6% AND DIRECTLY ATTIBUTABLE TO",
                        Peratus = 0
                    },
                    new JCukai
                    {
                        Kod = "TX-E43",
                        Perihal = "PURCHASE WITH GST INCURRED DIRECTLY ATTIBUTABLE TO",
                        Peratus = 0
                    },
                    new JCukai
                    {
                        Kod = "TX-n43",
                        Perihal = "PURCHASE WITH GST INCURRED DIRECTLY ATTIBUTABLE TO",
                        Peratus = 0
                    },
                    new JCukai
                    {
                        Kod = "TX-RE",
                        Perihal = "PURCHASE WITH GST INCURRED THAT IS NOT DIRECTLY ATTRIBUTABLE TO OR TAXABLE OR",
                        Peratus = 0
                    },
                    new JCukai
                    {
                        Kod = "ZP",
                        Perihal = "PURCHASE FROM GST-REGISTERED SUPPLIER WITH NO GST INCURRED",
                        Peratus = 0
                    },
                    new JCukai
                    {
                        Kod = "ZRE",
                        Perihal = "EXPORTATION OF GOODS OR SERVICES WHIC ARE SUBJECT TO ZERO RATED SUPPLIES",
                        Peratus = 0
                    },
                    new JCukai
                    {
                        Kod = "ZRL",
                        Perihal = "LOCAL SUPPLY OF GOODS OR SERVICES WHICH ARE SUBJET TO ZERO RATED SUPPLIES",
                        Peratus = 0
                    }

                    );
            }

            context.SaveChanges();

            // -- First Layer Insert END

            // -- Second Layer Insert

            if (context.JKWPTJBahagian.Any())
            {

            }
            else
            {
                context.JKWPTJBahagian.AddRange(
                    new JKWPTJBahagian
                    {
                        JKWId = 1,
                        JPTJId = 1,
                        JBahagianId = 1,
                        Kod = "10101"
                    },
                    new JKWPTJBahagian
                    {
                        JKWId = 1,
                        JPTJId = 2,
                        JBahagianId = 1,
                        Kod = "10201"
                    },
                    new JKWPTJBahagian
                    {
                        JKWId = 1,
                        JPTJId = 1,
                        JBahagianId = 2,
                        Kod = "10102"
                    },
                    new JKWPTJBahagian
                    {
                        JKWId = 1,
                        JPTJId = 2,
                        JBahagianId = 2,
                        Kod = "10202"
                    },
                    new JKWPTJBahagian
                    {
                        JKWId = 1,
                        JPTJId = 1,
                        JBahagianId = 3,
                        Kod = "10103"
                    },
                    new JKWPTJBahagian
                    {
                        JKWId = 1,
                        JPTJId = 2,
                        JBahagianId = 3,
                        Kod = "10203"
                    },
                    new JKWPTJBahagian
                    {
                        JKWId = 1,
                        JPTJId = 1,
                        JBahagianId = 4,
                        Kod = "10104"
                    },
                    new JKWPTJBahagian
                    {
                        JKWId = 1,
                        JPTJId = 2,
                        JBahagianId = 4,
                        Kod = "10204"
                    },
                    new JKWPTJBahagian
                    {
                        JKWId = 1,
                        JPTJId = 1,
                        JBahagianId = 5,
                        Kod = "10105"
                    },
                    new JKWPTJBahagian
                    {
                        JKWId = 1,
                        JPTJId = 2,
                        JBahagianId = 5,
                        Kod = "10205"
                    },
                    new JKWPTJBahagian
                    {
                        JKWId = 1,
                        JPTJId = 1,
                        JBahagianId = 6,
                        Kod = "10106"
                    },
                    new JKWPTJBahagian
                    {
                        JKWId = 1,
                        JPTJId = 2,
                        JBahagianId = 6,
                        Kod = "10206"
                    },
                    new JKWPTJBahagian
                    {
                        JKWId = 1,
                        JPTJId = 1,
                        JBahagianId = 7,
                        Kod = "10107"
                    },
                    new JKWPTJBahagian
                    {
                        JKWId = 1,
                        JPTJId = 2,
                        JBahagianId = 7,
                        Kod = "10207"
                    },
                    new JKWPTJBahagian
                    {
                        JKWId = 1,
                        JPTJId = 1,
                        JBahagianId = 8,
                        Kod = "10108"
                    },
                    new JKWPTJBahagian
                    {
                        JKWId = 1,
                        JPTJId = 2,
                        JBahagianId = 8,
                        Kod = "10208"
                    },
                    new JKWPTJBahagian
                    {
                        JKWId = 1,
                        JPTJId = 1,
                        JBahagianId = 9,
                        Kod = "10109"
                    },
                    new JKWPTJBahagian
                    {
                        JKWId = 1,
                        JPTJId = 2,
                        JBahagianId = 9,
                        Kod = "10209"
                    },
                    new JKWPTJBahagian
                    {
                        JKWId = 1,
                        JPTJId = 1,
                        JBahagianId = 10,
                        Kod = "10110"
                    },
                    new JKWPTJBahagian
                    {
                        JKWId = 1,
                        JPTJId = 2,
                        JBahagianId = 10,
                        Kod = "10210"
                    },
                    new JKWPTJBahagian
                    {
                        JKWId = 1,
                        JPTJId = 1,
                        JBahagianId = 11,
                        Kod = "10111"
                    },
                    new JKWPTJBahagian
                    {
                        JKWId = 1,
                        JPTJId = 2,
                        JBahagianId = 11,
                        Kod = "10211"
                    },
                    new JKWPTJBahagian
                    {
                        JKWId = 1,
                        JPTJId = 1,
                        JBahagianId = 12,
                        Kod = "10112"
                    },
                    new JKWPTJBahagian
                    {
                        JKWId = 1,
                        JPTJId = 2,
                        JBahagianId = 12,
                        Kod = "10212"
                    },
                    new JKWPTJBahagian
                    {
                        JKWId = 1,
                        JPTJId = 1,
                        JBahagianId = 13,
                        Kod = "10113"
                    },
                    new JKWPTJBahagian
                    {
                        JKWId = 1,
                        JPTJId = 2,
                        JBahagianId = 13,
                        Kod = "10213"
                    }
                    );
            }
            if (context.DDaftarAwam.Any())
            {

            } else
            {
                //context.DDaftarAwam.AddRange(
                //    new DDaftarAwam
                //    {
                //        Kod = "I0001",
                //        Nama = "IDWAL SYSTEMS SDN BHD",
                //        JNegeriId = 10,
                //        JBankId = 8,
                //        NoPendaftaran = "187842-T",
                //        NoKPLama = "",
                //        Alamat1 = "Lot 605G, Kompleks Diamond",
                //        Alamat2 = "Bangi Business Park",
                //        Alamat3 = "Jalan Medan Bangi, Off Persiaran Bandar",
                //        Poskod = "43650",
                //        Bandar = "Bandar Baru Bangi",
                //        Telefon1 = "03-89663520",
                //        Telefon2 = "",
                //        Telefon3 = "",
                //        Handphone = "",
                //        Emel = "far@idwal.com.my",
                //        NoAkaunBank = "12029010003756",
                //        EnKategoriAhli = EnKategoriAhli.Tiada,
                //        EnKategoriDaftarAwam = EnKategoriDaftarAwam.Pembekal,
                //        Faks = "03-89663520",
                //        IsBekalan = false,
                //        IsPerkhidmatan = true,
                //        IsKerja = false,
                //        JangkaMasaDari = null,
                //        JangkaMasaHingga = null,
                //        KodM2E = "PI0001",
                //        DPekerjaMasukId = null,
                //        UserId = "superadmin@idwal.com.my",
                //        TarMasuk = DateTime.Now,
                //        DPekerjaKemaskiniId = null,
                //        TarKemaskini = null,
                //        FlHapus = 0,
                //        TarHapus = null,
                //        SebabHapus = ""
                //    });
            }
            context.SaveChanges();
            // -- Second Layer Insert END

            // -- Third Layer Insert


            context.SaveChanges();

            // -- Third Layer Insert END
        }
    
    }
}
