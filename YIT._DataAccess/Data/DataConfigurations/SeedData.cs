using Microsoft.AspNetCore.Identity;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._00Sistem;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._03Akaun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Data.DataConfigurations
{
    public static class SeedData
    {
        public static void SeedUsers(UserManager<IdentityUser> userManager, ApplicationDbContext db)
        {
            var results = userManager.FindByEmailAsync(Init.superAdminPassword).Result;

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
                        Perihal = "CARUMAN KERAJAAN NEGERI"
                    },
                    new JKW
                    {
                        Kod = "2",
                        Perihal = "JAKIM"
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
                        Perihal = "MAYBANK"
                    },
                    new JBank
                    {
                        Kod = "02",
                        Perihal = "BANK ISLAM"
                    },
                     new JBank
                     {
                         Kod = "03",
                         Perihal = "AFFIN"
                     },
                      new JBank
                      {
                          Kod = "04",
                          Perihal = "HONG LEONG"
                      }
                    );

            }

            if (context.AkCarta.Any())
            {

            }
            else
            {
                {
                    context.AkCarta.AddRange(
                        new AkCarta
                        {
                            Kod = "A10000",
                            Perihal = "ASET SEMASA",
                            DebitKredit = "D",
                            UmumDetail = "U",
                            Baki = 0,
                            EnJenis = EnJenisCarta.Aset,
                            EnParas = EnParas.Paras1
                        },
                        new AkCarta
                        {
                            Kod = "A11000",
                            Perihal = "WANG TUNAI DAN BAKI BANK",
                            DebitKredit = "D",
                            UmumDetail = "U",
                            Baki = 0,
                            EnJenis = EnJenisCarta.Aset,
                            EnParas = EnParas.Paras2
                        },
                    new AkCarta
                    {
                        Kod = "A11100",
                        Perihal = "WANG TUNAI DAN BAKI BANK",
                        DebitKredit = "D",
                        UmumDetail = "U",
                        Baki = 0,
                        EnJenis = EnJenisCarta.Aset,
                        EnParas = EnParas.Paras3
                    },
                    new AkCarta
                    {
                        Kod = "A11101",
                        Perihal = "AKAUN BANK UTAMA",
                        DebitKredit = "D",
                        UmumDetail = "D",
                        Baki = 0,
                        EnJenis = EnJenisCarta.Aset,
                        EnParas = EnParas.Paras4
                    },
                    //
                    // Belanja
                    new AkCarta
                    {
                        Kod = "B10000",
                        Perihal = "GAJI DAN UPAH",
                        DebitKredit = "D",
                        UmumDetail = "U",
                        Baki = 0,
                        EnJenis = EnJenisCarta.Belanja,
                        EnParas = EnParas.Paras1
                    },
                    new AkCarta
                    {
                        Kod = "B11000",
                        Perihal = "GAJI DAN UPAH",
                        DebitKredit = "D",
                        UmumDetail = "U",
                        Baki = 0,
                        EnJenis = EnJenisCarta.Belanja,
                        EnParas = EnParas.Paras2
                    },
                    new AkCarta
                    {
                        Kod = "B11100",
                        Perihal = "GAJI DAN UPAH KAKITANGAN",
                        DebitKredit = "D",
                        UmumDetail = "U",
                        Baki = 0,
                        EnJenis = EnJenisCarta.Belanja,
                        EnParas = EnParas.Paras3
                    },
                    new AkCarta
                    {
                        Kod = "B11101",
                        Perihal = "GAJI DAN UPAH - KAKITANGAN",
                        DebitKredit = "D",
                        UmumDetail = "D",
                        Baki = 0,
                        EnJenis = EnJenisCarta.Belanja,
                        EnParas = EnParas.Paras4
                    },
                    // LIABILITI
                    new AkCarta
                    {
                        Kod = "L10000",
                        Perihal = "LIABILITI SEMASA",
                        DebitKredit = "K",
                        UmumDetail = "U",
                        Baki = 0,
                        EnJenis = EnJenisCarta.Liabiliti,
                        EnParas = EnParas.Paras1
                    },
                    new AkCarta
                    {
                        Kod = "L11000",
                        Perihal = "AKAUN BELUM BAYAR",
                        DebitKredit = "K",
                        UmumDetail = "U",
                        Baki = 0,
                        EnJenis = EnJenisCarta.Liabiliti,
                        EnParas = EnParas.Paras2
                    },
                    new AkCarta
                    {
                        Kod = "L11100",
                        Perihal = "AKAUN BELUM BAYAR",
                        DebitKredit = "K",
                        UmumDetail = "U",
                        Baki = 0,
                        EnJenis = EnJenisCarta.Liabiliti,
                        EnParas = EnParas.Paras3
                    },
                    new AkCarta
                    {
                        Kod = "L11101",
                        Perihal = "AKAUN BELUM BAYAR",
                        DebitKredit = "K",
                        UmumDetail = "D",
                        Baki = 0,
                        EnJenis = EnJenisCarta.Liabiliti,
                        EnParas = EnParas.Paras4
                    },
                    // EKUITI
                    new AkCarta
                    {
                        Kod = "E10000",
                        Perihal = "EKUITI",
                        DebitKredit = "K",
                        UmumDetail = "U",
                        Baki = 0,
                        EnJenis = EnJenisCarta.Ekuiti,
                        EnParas = EnParas.Paras1
                    },
                    new AkCarta
                    {
                        Kod = "E11000",
                        Perihal = "RIZAB",
                        DebitKredit = "K",
                        UmumDetail = "U",
                        Baki = 0,
                        EnJenis = EnJenisCarta.Ekuiti,
                        EnParas = EnParas.Paras2
                    },
                    new AkCarta
                    {
                        Kod = "E11100",
                        Perihal = "RIZAB",
                        DebitKredit = "K",
                        UmumDetail = "U",
                        Baki = 0,
                        EnJenis = EnJenisCarta.Ekuiti,
                        EnParas = EnParas.Paras3
                    },
                    new AkCarta
                    {
                        Kod = "E11101",
                        Perihal = "RIZAB PENILAIAN SEMULA TANAH",
                        DebitKredit = "K",
                        UmumDetail = "D",
                        Baki = 0,
                        EnJenis = EnJenisCarta.Ekuiti,
                        EnParas = EnParas.Paras4
                    },
                    // HASIL
                    new AkCarta
                    {
                        Kod = "H70000",
                        Perihal = "HASIL",
                        DebitKredit = "K",
                        UmumDetail = "U",
                        Baki = 0,
                        EnJenis = EnJenisCarta.Hasil,
                        EnParas = EnParas.Paras1
                    },
                    new AkCarta
                    {
                        Kod = "H71000",
                        Perihal = "PELBAGAI TERIMAAN UNTUK PERKHIDMATAN",
                        DebitKredit = "K",
                        UmumDetail = "U",
                        Baki = 0,
                        EnJenis = EnJenisCarta.Hasil,
                        EnParas = EnParas.Paras2
                    },
                    new AkCarta
                    {
                        Kod = "H71100",
                        Perihal = "KOMISEN ATAS SUMBANGAN",
                        DebitKredit = "K",
                        UmumDetail = "U",
                        Baki = 0,
                        EnJenis = EnJenisCarta.Hasil,
                        EnParas = EnParas.Paras3
                    },
                    new AkCarta
                    {
                        Kod = "H71101",
                        Perihal = "KOMISEN ATAS SUMBANGAN",
                        DebitKredit = "K",
                        UmumDetail = "D",
                        Baki = 0,
                        EnJenis = EnJenisCarta.Hasil,
                        EnParas = EnParas.Paras4
                    }
                        );
                }
            }

            context.SaveChanges();
            // -- First Layer Insert END

            // -- Second Layer Insert
            if (context.JPTJ.Any())
            {

            } else
            {
                context.JPTJ.AddRange(
                    new JPTJ
                    {
                        Kod = "01",
                        Perihal = "CARUMAN KERAJAAN NEGERI",
                        JKWId = 1
                    },
                    new JPTJ
                    {
                        Kod = "02",
                        Perihal = "JAKIM",
                        JKWId = 2
                    });
            }

            context.SaveChanges();
            // -- Second Layer Insert END

            // -- Third Layer Insert
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
                        Perihal = "PENTADBIRAN",
                        JPTJId = 1
                    },
                    new JBahagian
                    {
                        Kod = "02",
                        Perihal = "TADIKA",
                        JPTJId = 1
                    }, 
                    new JBahagian
                    {
                        Kod = "03",
                        Perihal = "PELABURAN",
                        JPTJId = 1
                    }, new JBahagian
                    {
                        Kod = "04",
                        Perihal = "KHIDMAT MASYARAKAT",
                        JPTJId = 1
                    }, new JBahagian
                    {
                        Kod = "05",
                        Perihal = "DAKWAH",
                        JPTJId = 1
                    }, new JBahagian
                    {
                        Kod = "06",
                        Perihal = "PENERBITAN",
                        JPTJId = 1
                    }, new JBahagian
                    {
                        Kod = "07",
                        Perihal = "INSPI/PDI",
                        JPTJId = 1
                    }
                    , new JBahagian
                    {
                        Kod = "08",
                        Perihal = "SRAYIT",
                        JPTJId = 1
                    }, 
                    new JBahagian
                    {
                        Kod = "09",
                        Perihal = "KAFA",
                        JPTJId = 1
                    },
                    new JBahagian
                    {
                        Kod = "01",
                        Perihal = "PENTADBIRAN",
                        JPTJId = 2
                    },
                    new JBahagian
                    {
                        Kod = "02",
                        Perihal = "TADIKA",
                        JPTJId = 2
                    },
                    new JBahagian
                    {
                        Kod = "03",
                        Perihal = "PELABURAN",
                        JPTJId = 2
                    }, 
                    new JBahagian
                    {
                        Kod = "04",
                        Perihal = "KHIDMAT MASYARAKAT",
                        JPTJId = 2
                    }, 
                    new JBahagian
                    {
                        Kod = "05",
                        Perihal = "DAKWAH",
                        JPTJId = 2
                    }, 
                    new JBahagian
                    {
                        Kod = "06",
                        Perihal = "PENERBITAN",
                        JPTJId = 2
                    }, 
                    new JBahagian
                    {
                        Kod = "07",
                        Perihal = "INSPI/PDI",
                        JPTJId = 2
                    }
                    , 
                    new JBahagian
                    {
                        Kod = "08",
                        Perihal = "SRAYIT",
                        JPTJId = 2
                    },
                    new JBahagian
                    {
                        Kod = "09",
                        Perihal = "KAFA",
                        JPTJId = 2
                    }
                    // ** Tambah di sini
                    );
            }

            context.SaveChanges();

            // -- Third Layer Insert END
        }
    }
}
