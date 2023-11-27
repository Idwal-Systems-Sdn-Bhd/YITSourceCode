using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YIT._DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class CreateDbInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AkCarta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Perihal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DebitKredit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UmumDetail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Catatan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Baki = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EnJenis = table.Column<int>(type: "int", nullable: false),
                    EnParas = table.Column<int>(type: "int", nullable: false),
                    DPekerjaMasukId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarMasuk = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DPekerjaKemaskiniId = table.Column<int>(type: "int", nullable: true),
                    UserIdKemaskini = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarKemaskini = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlHapus = table.Column<int>(type: "int", nullable: false),
                    TarHapus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabHapus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AkCarta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExceptionLogger",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UrlRequest = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ControllerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExceptionType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExceptionMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExceptionStackTrace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TraceIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExceptionLogger", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JAgama",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Perihal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DPekerjaMasukId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarMasuk = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DPekerjaKemaskiniId = table.Column<int>(type: "int", nullable: true),
                    UserIdKemaskini = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarKemaskini = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlHapus = table.Column<int>(type: "int", nullable: false),
                    TarHapus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabHapus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JAgama", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JBahagian",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Perihal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DPekerjaMasukId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarMasuk = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DPekerjaKemaskiniId = table.Column<int>(type: "int", nullable: true),
                    UserIdKemaskini = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarKemaskini = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlHapus = table.Column<int>(type: "int", nullable: false),
                    TarHapus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabHapus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JBahagian", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JBangsa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Perihal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DPekerjaMasukId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarMasuk = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DPekerjaKemaskiniId = table.Column<int>(type: "int", nullable: true),
                    UserIdKemaskini = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarKemaskini = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlHapus = table.Column<int>(type: "int", nullable: false),
                    TarHapus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabHapus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JBangsa", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JBank",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Perihal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KodBNMEFT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Length1 = table.Column<int>(type: "int", nullable: false),
                    Length2 = table.Column<int>(type: "int", nullable: true),
                    DPekerjaMasukId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarMasuk = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DPekerjaKemaskiniId = table.Column<int>(type: "int", nullable: true),
                    UserIdKemaskini = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarKemaskini = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlHapus = table.Column<int>(type: "int", nullable: false),
                    TarHapus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabHapus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JBank", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JCaraBayar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Perihal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DPekerjaMasukId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarMasuk = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DPekerjaKemaskiniId = table.Column<int>(type: "int", nullable: true),
                    UserIdKemaskini = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarKemaskini = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlHapus = table.Column<int>(type: "int", nullable: false),
                    TarHapus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabHapus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JCaraBayar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JKW",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Perihal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DPekerjaMasukId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarMasuk = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DPekerjaKemaskiniId = table.Column<int>(type: "int", nullable: true),
                    UserIdKemaskini = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarKemaskini = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlHapus = table.Column<int>(type: "int", nullable: false),
                    TarHapus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabHapus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JKW", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JNegeri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Perihal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DPekerjaMasukId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarMasuk = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DPekerjaKemaskiniId = table.Column<int>(type: "int", nullable: true),
                    UserIdKemaskini = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarKemaskini = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlHapus = table.Column<int>(type: "int", nullable: false),
                    TarHapus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabHapus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JNegeri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JPTJ",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kod = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Perihal = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DPekerjaMasukId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarMasuk = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DPekerjaKemaskiniId = table.Column<int>(type: "int", nullable: true),
                    UserIdKemaskini = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarKemaskini = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlHapus = table.Column<int>(type: "int", nullable: false),
                    TarHapus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabHapus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JPTJ", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SiAppInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KodSistem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarVersi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NamaSyarikat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoPendaftaran = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlamatSyarikat1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlamatSyarikat2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlamatSyarikat3 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Bandar = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Poskod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Daerah = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Negeri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TelSyarikat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FaksSyarikat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmelSyarikat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarMula = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompanyLogoWeb = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyLogoPrintPDF = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FlMaintainance = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiAppInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AkBank",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Perihal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoAkaun = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JKWId = table.Column<int>(type: "int", nullable: false),
                    AkCartaId = table.Column<int>(type: "int", nullable: false),
                    JBankId = table.Column<int>(type: "int", nullable: false),
                    DPekerjaMasukId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarMasuk = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DPekerjaKemaskiniId = table.Column<int>(type: "int", nullable: true),
                    UserIdKemaskini = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarKemaskini = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlHapus = table.Column<int>(type: "int", nullable: false),
                    TarHapus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabHapus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AkBank", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AkBank_AkCarta_AkCartaId",
                        column: x => x.AkCartaId,
                        principalTable: "AkCarta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AkBank_JBank_JBankId",
                        column: x => x.JBankId,
                        principalTable: "JBank",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AkBank_JKW_JKWId",
                        column: x => x.JKWId,
                        principalTable: "JKW",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DDaftarAwam",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JNegeriId = table.Column<int>(type: "int", nullable: true),
                    JBankId = table.Column<int>(type: "int", nullable: true),
                    NoPendaftaran = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoKPLama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Alamat1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Alamat2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Alamat3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Poskod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bandar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefon1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefon2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefon3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Handphone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Emel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoAkaunBank = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnKategoriDaftarAwam = table.Column<int>(type: "int", nullable: false),
                    EnKategoriAhli = table.Column<int>(type: "int", nullable: false),
                    Faks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsBekalan = table.Column<bool>(type: "bit", nullable: false),
                    IsPerkhidmatan = table.Column<bool>(type: "bit", nullable: false),
                    IsKerja = table.Column<bool>(type: "bit", nullable: false),
                    JangkaMasaDari = table.Column<DateTime>(type: "datetime2", nullable: true),
                    JangkaMasaHingga = table.Column<DateTime>(type: "datetime2", nullable: true),
                    KodM2E = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KodLama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DPekerjaMasukId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarMasuk = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DPekerjaKemaskiniId = table.Column<int>(type: "int", nullable: true),
                    UserIdKemaskini = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarKemaskini = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlHapus = table.Column<int>(type: "int", nullable: false),
                    TarHapus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabHapus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DDaftarAwam", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DDaftarAwam_JBank_JBankId",
                        column: x => x.JBankId,
                        principalTable: "JBank",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DDaftarAwam_JNegeri_JNegeriId",
                        column: x => x.JNegeriId,
                        principalTable: "JNegeri",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AkAkaun",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JKWId = table.Column<int>(type: "int", nullable: false),
                    JPTJId = table.Column<int>(type: "int", nullable: true),
                    JBahagianId = table.Column<int>(type: "int", nullable: true),
                    AkCarta1Id = table.Column<int>(type: "int", nullable: false),
                    AkCarta2Id = table.Column<int>(type: "int", nullable: true),
                    NoRujukan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tarikh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Kredit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NoRujukanLama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DPekerjaMasukId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarMasuk = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DPekerjaKemaskiniId = table.Column<int>(type: "int", nullable: true),
                    UserIdKemaskini = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarKemaskini = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlHapus = table.Column<int>(type: "int", nullable: false),
                    TarHapus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabHapus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AkAkaun", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AkAkaun_AkCarta_AkCarta1Id",
                        column: x => x.AkCarta1Id,
                        principalTable: "AkCarta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AkAkaun_AkCarta_AkCarta2Id",
                        column: x => x.AkCarta2Id,
                        principalTable: "AkCarta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AkAkaun_JBahagian_JBahagianId",
                        column: x => x.JBahagianId,
                        principalTable: "JBahagian",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AkAkaun_JKW_JKWId",
                        column: x => x.JKWId,
                        principalTable: "JKW",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AkAkaun_JPTJ_JPTJId",
                        column: x => x.JPTJId,
                        principalTable: "JPTJ",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JKWPTJBahagian",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JKWId = table.Column<int>(type: "int", nullable: false),
                    JPTJId = table.Column<int>(type: "int", nullable: false),
                    JBahagianId = table.Column<int>(type: "int", nullable: false),
                    Kod = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JKWPTJBahagian", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JKWPTJBahagian_JBahagian_JBahagianId",
                        column: x => x.JBahagianId,
                        principalTable: "JBahagian",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JKWPTJBahagian_JKW_JKWId",
                        column: x => x.JKWId,
                        principalTable: "JKW",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JKWPTJBahagian_JPTJ_JPTJId",
                        column: x => x.JPTJId,
                        principalTable: "JPTJ",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JCawangan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Perihal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AkBankId = table.Column<int>(type: "int", nullable: true),
                    DPekerjaMasukId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarMasuk = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DPekerjaKemaskiniId = table.Column<int>(type: "int", nullable: true),
                    UserIdKemaskini = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarKemaskini = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlHapus = table.Column<int>(type: "int", nullable: false),
                    TarHapus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabHapus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JCawangan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JCawangan_AkBank_AkBankId",
                        column: x => x.AkBankId,
                        principalTable: "AkBank",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AbBukuVot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tahun = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JKWId = table.Column<int>(type: "int", nullable: false),
                    JPTJId = table.Column<int>(type: "int", nullable: false),
                    JBahagianId = table.Column<int>(type: "int", nullable: true),
                    Tarikh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DDaftarAwamId = table.Column<int>(type: "int", nullable: true),
                    VotId = table.Column<int>(type: "int", nullable: false),
                    NoRujukan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Kredit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tanggungan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tbs = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Baki = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rizab = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Liabiliti = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Belanja = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbBukuVot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbBukuVot_AkCarta_VotId",
                        column: x => x.VotId,
                        principalTable: "AkCarta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AbBukuVot_DDaftarAwam_DDaftarAwamId",
                        column: x => x.DDaftarAwamId,
                        principalTable: "DDaftarAwam",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AbBukuVot_JBahagian_JBahagianId",
                        column: x => x.JBahagianId,
                        principalTable: "JBahagian",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AbBukuVot_JKW_JKWId",
                        column: x => x.JKWId,
                        principalTable: "JKW",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AbBukuVot_JPTJ_JPTJId",
                        column: x => x.JPTJId,
                        principalTable: "JPTJ",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DPekerja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoGaji = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KodPekerja = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoKp = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoKpLama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarikhLahir = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Alamat1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Alamat2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Alamat3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Poskod = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Bandar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JNegeriId = table.Column<int>(type: "int", nullable: false),
                    TelefonRumah = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TelefonBimbit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Emel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TarikhMasukKerja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TarikhBerhentiKerja = table.Column<DateTime>(type: "datetime2", nullable: true),
                    JBankId = table.Column<int>(type: "int", nullable: true),
                    JBangsaId = table.Column<int>(type: "int", nullable: true),
                    Jawatan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoAkaunBank = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    KodM2E = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoCukai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoPerkeso = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoKWAP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoKWSP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JBahagianId = table.Column<int>(type: "int", nullable: false),
                    JCawanganId = table.Column<int>(type: "int", nullable: false),
                    EnJenisKadPengenalan = table.Column<int>(type: "int", nullable: false),
                    JPTJId = table.Column<int>(type: "int", nullable: false),
                    DPekerjaMasukId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarMasuk = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DPekerjaKemaskiniId = table.Column<int>(type: "int", nullable: true),
                    UserIdKemaskini = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarKemaskini = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlHapus = table.Column<int>(type: "int", nullable: false),
                    TarHapus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabHapus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DPekerja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DPekerja_JBahagian_JBahagianId",
                        column: x => x.JBahagianId,
                        principalTable: "JBahagian",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DPekerja_JBangsa_JBangsaId",
                        column: x => x.JBangsaId,
                        principalTable: "JBangsa",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DPekerja_JBank_JBankId",
                        column: x => x.JBankId,
                        principalTable: "JBank",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DPekerja_JCawangan_JCawanganId",
                        column: x => x.JCawanganId,
                        principalTable: "JCawangan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DPekerja_JNegeri_JNegeriId",
                        column: x => x.JNegeriId,
                        principalTable: "JNegeri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DPekerja_JPTJ_JPTJId",
                        column: x => x.JPTJId,
                        principalTable: "JPTJ",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AkTerima",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoRujukan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tarikh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FlPosting = table.Column<int>(type: "int", nullable: false),
                    DPekerjaPostingId = table.Column<int>(type: "int", nullable: true),
                    TarikhPosting = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Jumlah = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KodPembayar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Alamat1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Alamat2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Alamat3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Poskod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bandar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefon1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Emel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Perihal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnJenisTerimaan = table.Column<int>(type: "int", nullable: true),
                    EnKategoriDaftarAwam = table.Column<int>(type: "int", nullable: false),
                    FlCetak = table.Column<int>(type: "int", nullable: false),
                    JKWId = table.Column<int>(type: "int", nullable: false),
                    JNegeriId = table.Column<int>(type: "int", nullable: true),
                    AkBankId = table.Column<int>(type: "int", nullable: true),
                    DDaftarAwamId = table.Column<int>(type: "int", nullable: true),
                    NoRujukanLama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JCawanganId = table.Column<int>(type: "int", nullable: false),
                    DPekerjaMasukId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarMasuk = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DPekerjaKemaskiniId = table.Column<int>(type: "int", nullable: true),
                    UserIdKemaskini = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarKemaskini = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlHapus = table.Column<int>(type: "int", nullable: false),
                    TarHapus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabHapus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AkTerima", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AkTerima_AkBank_AkBankId",
                        column: x => x.AkBankId,
                        principalTable: "AkBank",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkTerima_DDaftarAwam_DDaftarAwamId",
                        column: x => x.DDaftarAwamId,
                        principalTable: "DDaftarAwam",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkTerima_DPekerja_DPekerjaPostingId",
                        column: x => x.DPekerjaPostingId,
                        principalTable: "DPekerja",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkTerima_JCawangan_JCawanganId",
                        column: x => x.JCawanganId,
                        principalTable: "JCawangan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AkTerima_JKW_JKWId",
                        column: x => x.JKWId,
                        principalTable: "JKW",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AkTerima_JNegeri_JNegeriId",
                        column: x => x.JNegeriId,
                        principalTable: "JNegeri",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LgDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LgModule = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LgOperation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LgNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoRujukan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdRujukan = table.Column<int>(type: "int", nullable: false),
                    Jumlah = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SysCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DPekerjaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppLog_DPekerja_DPekerjaId",
                        column: x => x.DPekerjaId,
                        principalTable: "DPekerja",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tandatangan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DPekerjaId = table.Column<int>(type: "int", nullable: true),
                    JKWPTJBahagianList = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_DPekerja_DPekerjaId",
                        column: x => x.DPekerjaId,
                        principalTable: "DPekerja",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DKonfigKelulusan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DPekerjaId = table.Column<int>(type: "int", nullable: false),
                    JBahagianId = table.Column<int>(type: "int", nullable: true),
                    EnKategoriKelulusan = table.Column<int>(type: "int", nullable: false),
                    EnJenisModulKelulusan = table.Column<int>(type: "int", nullable: false),
                    MinAmaun = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaksAmaun = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KataLaluan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tandatangan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KodLama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAktif = table.Column<bool>(type: "bit", nullable: false),
                    DPekerjaMasukId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarMasuk = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DPekerjaKemaskiniId = table.Column<int>(type: "int", nullable: true),
                    UserIdKemaskini = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarKemaskini = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlHapus = table.Column<int>(type: "int", nullable: false),
                    TarHapus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabHapus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DKonfigKelulusan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DKonfigKelulusan_DPekerja_DPekerjaId",
                        column: x => x.DPekerjaId,
                        principalTable: "DPekerja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DKonfigKelulusan_JBahagian_JBahagianId",
                        column: x => x.JBahagianId,
                        principalTable: "JBahagian",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DPenyelia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JCawanganId = table.Column<int>(type: "int", nullable: false),
                    DPekerjaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DPenyelia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DPenyelia_DPekerja_DPekerjaId",
                        column: x => x.DPekerjaId,
                        principalTable: "DPekerja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DPenyelia_JCawangan_JCawanganId",
                        column: x => x.JCawanganId,
                        principalTable: "JCawangan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AkTerimaCaraBayar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AkTerimaId = table.Column<int>(type: "int", nullable: false),
                    JCaraBayarId = table.Column<int>(type: "int", nullable: false),
                    Amaun = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NoCekMK = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnJenisCek = table.Column<int>(type: "int", nullable: false),
                    KodBankCek = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TempatCek = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoSlip = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TarikhSlip = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReconTarikhTunai = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReconIsAutoMatch = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AkTerimaCaraBayar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AkTerimaCaraBayar_AkTerima_AkTerimaId",
                        column: x => x.AkTerimaId,
                        principalTable: "AkTerima",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AkTerimaCaraBayar_JCaraBayar_JCaraBayarId",
                        column: x => x.JCaraBayarId,
                        principalTable: "JCaraBayar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AkTerimaObjek",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AkTerimaId = table.Column<int>(type: "int", nullable: false),
                    AkCartaId = table.Column<int>(type: "int", nullable: false),
                    JKWPTJBahagianId = table.Column<int>(type: "int", nullable: false),
                    Amaun = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AkTerimaObjek", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AkTerimaObjek_AkCarta_AkCartaId",
                        column: x => x.AkCartaId,
                        principalTable: "AkCarta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AkTerimaObjek_AkTerima_AkTerimaId",
                        column: x => x.AkTerimaId,
                        principalTable: "AkTerima",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AkTerimaObjek_JKWPTJBahagian_JKWPTJBahagianId",
                        column: x => x.JKWPTJBahagianId,
                        principalTable: "JKWPTJBahagian",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbWaran",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tahun = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoRujukan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tarikh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JKWId = table.Column<int>(type: "int", nullable: false),
                    EnJenisPeruntukan = table.Column<int>(type: "int", nullable: false),
                    FlJenisPindahan = table.Column<int>(type: "int", nullable: false),
                    Jumlah = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Sebab = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoRujukanLama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DPekerjaMasukId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarMasuk = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DPekerjaKemaskiniId = table.Column<int>(type: "int", nullable: true),
                    UserIdKemaskini = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarKemaskini = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlHapus = table.Column<int>(type: "int", nullable: false),
                    TarHapus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabHapus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FlBatal = table.Column<int>(type: "int", nullable: false),
                    TarBatal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabBatal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DPengesahId = table.Column<int>(type: "int", nullable: true),
                    TarikhSah = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DPenyemakId = table.Column<int>(type: "int", nullable: true),
                    TarikhSemak = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DPelulusId = table.Column<int>(type: "int", nullable: true),
                    TarikhLulus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DPekerjaPostingId = table.Column<int>(type: "int", nullable: true),
                    FlPosting = table.Column<int>(type: "int", nullable: false),
                    TarikhPosting = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EnStatusBorang = table.Column<int>(type: "int", nullable: false),
                    Tindakan = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbWaran", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbWaran_DKonfigKelulusan_DPelulusId",
                        column: x => x.DPelulusId,
                        principalTable: "DKonfigKelulusan",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AbWaran_DKonfigKelulusan_DPengesahId",
                        column: x => x.DPengesahId,
                        principalTable: "DKonfigKelulusan",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AbWaran_DKonfigKelulusan_DPenyemakId",
                        column: x => x.DPenyemakId,
                        principalTable: "DKonfigKelulusan",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AbWaran_DPekerja_DPekerjaPostingId",
                        column: x => x.DPekerjaPostingId,
                        principalTable: "DPekerja",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AbWaran_JKW_JKWId",
                        column: x => x.JKWId,
                        principalTable: "JKW",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AkNotaMinta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoRujukan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tahun = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tarikh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TarikhPerlu = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EnKaedahPerolehan = table.Column<int>(type: "int", nullable: false),
                    Jumlah = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Sebab = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JKWId = table.Column<int>(type: "int", nullable: false),
                    DPemohonId = table.Column<int>(type: "int", nullable: true),
                    Jawatan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DDaftarAwamId = table.Column<int>(type: "int", nullable: false),
                    DPekerjaMasukId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarMasuk = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DPekerjaKemaskiniId = table.Column<int>(type: "int", nullable: true),
                    UserIdKemaskini = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarKemaskini = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlHapus = table.Column<int>(type: "int", nullable: false),
                    TarHapus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabHapus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FlBatal = table.Column<int>(type: "int", nullable: false),
                    TarBatal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabBatal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DPengesahId = table.Column<int>(type: "int", nullable: true),
                    TarikhSah = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DPenyemakId = table.Column<int>(type: "int", nullable: true),
                    TarikhSemak = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DPelulusId = table.Column<int>(type: "int", nullable: true),
                    TarikhLulus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DPekerjaPostingId = table.Column<int>(type: "int", nullable: true),
                    FlPosting = table.Column<int>(type: "int", nullable: false),
                    TarikhPosting = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EnStatusBorang = table.Column<int>(type: "int", nullable: false),
                    Tindakan = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AkNotaMinta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AkNotaMinta_DDaftarAwam_DDaftarAwamId",
                        column: x => x.DDaftarAwamId,
                        principalTable: "DDaftarAwam",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AkNotaMinta_DKonfigKelulusan_DPelulusId",
                        column: x => x.DPelulusId,
                        principalTable: "DKonfigKelulusan",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkNotaMinta_DKonfigKelulusan_DPengesahId",
                        column: x => x.DPengesahId,
                        principalTable: "DKonfigKelulusan",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkNotaMinta_DKonfigKelulusan_DPenyemakId",
                        column: x => x.DPenyemakId,
                        principalTable: "DKonfigKelulusan",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkNotaMinta_DPekerja_DPekerjaPostingId",
                        column: x => x.DPekerjaPostingId,
                        principalTable: "DPekerja",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkNotaMinta_DPekerja_DPemohonId",
                        column: x => x.DPemohonId,
                        principalTable: "DPekerja",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkNotaMinta_JKW_JKWId",
                        column: x => x.JKWId,
                        principalTable: "JKW",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AkPenilaianPerolehan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoRujukan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tahun = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoSebutHarga = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tarikh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TarikhPerlu = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EnKaedahPerolehan = table.Column<int>(type: "int", nullable: false),
                    HargaTawaran = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Jumlah = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Sebab = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BilSebutharga = table.Column<int>(type: "int", nullable: true),
                    MaklumatSebutHarga = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JKWId = table.Column<int>(type: "int", nullable: false),
                    DPemohonId = table.Column<int>(type: "int", nullable: true),
                    Jawatan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FlPOInden = table.Column<int>(type: "int", nullable: false),
                    DDaftarAwamId = table.Column<int>(type: "int", nullable: false),
                    NoRujukanLama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DPekerjaMasukId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarMasuk = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DPekerjaKemaskiniId = table.Column<int>(type: "int", nullable: true),
                    UserIdKemaskini = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarKemaskini = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlHapus = table.Column<int>(type: "int", nullable: false),
                    TarHapus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabHapus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FlBatal = table.Column<int>(type: "int", nullable: false),
                    TarBatal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabBatal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DPengesahId = table.Column<int>(type: "int", nullable: true),
                    TarikhSah = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DPenyemakId = table.Column<int>(type: "int", nullable: true),
                    TarikhSemak = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DPelulusId = table.Column<int>(type: "int", nullable: true),
                    TarikhLulus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DPekerjaPostingId = table.Column<int>(type: "int", nullable: true),
                    FlPosting = table.Column<int>(type: "int", nullable: false),
                    TarikhPosting = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EnStatusBorang = table.Column<int>(type: "int", nullable: false),
                    Tindakan = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AkPenilaianPerolehan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AkPenilaianPerolehan_DDaftarAwam_DDaftarAwamId",
                        column: x => x.DDaftarAwamId,
                        principalTable: "DDaftarAwam",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AkPenilaianPerolehan_DKonfigKelulusan_DPelulusId",
                        column: x => x.DPelulusId,
                        principalTable: "DKonfigKelulusan",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkPenilaianPerolehan_DKonfigKelulusan_DPengesahId",
                        column: x => x.DPengesahId,
                        principalTable: "DKonfigKelulusan",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkPenilaianPerolehan_DKonfigKelulusan_DPenyemakId",
                        column: x => x.DPenyemakId,
                        principalTable: "DKonfigKelulusan",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkPenilaianPerolehan_DPekerja_DPekerjaPostingId",
                        column: x => x.DPekerjaPostingId,
                        principalTable: "DPekerja",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkPenilaianPerolehan_DPekerja_DPemohonId",
                        column: x => x.DPemohonId,
                        principalTable: "DPekerja",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkPenilaianPerolehan_JKW_JKWId",
                        column: x => x.JKWId,
                        principalTable: "JKW",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbWaranObjek",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AbWaranId = table.Column<int>(type: "int", nullable: false),
                    AkCartaId = table.Column<int>(type: "int", nullable: false),
                    JKWPTJBahagianId = table.Column<int>(type: "int", nullable: false),
                    Amaun = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TK = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbWaranObjek", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbWaranObjek_AbWaran_AbWaranId",
                        column: x => x.AbWaranId,
                        principalTable: "AbWaran",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AbWaranObjek_AkCarta_AkCartaId",
                        column: x => x.AkCartaId,
                        principalTable: "AkCarta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AbWaranObjek_JKWPTJBahagian_JKWPTJBahagianId",
                        column: x => x.JKWPTJBahagianId,
                        principalTable: "JKWPTJBahagian",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AkNotaMintaObjek",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AkNotaMintaId = table.Column<int>(type: "int", nullable: false),
                    AkCartaId = table.Column<int>(type: "int", nullable: false),
                    JKWPTJBahagianId = table.Column<int>(type: "int", nullable: false),
                    Amaun = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AkNotaMintaObjek", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AkNotaMintaObjek_AkCarta_AkCartaId",
                        column: x => x.AkCartaId,
                        principalTable: "AkCarta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AkNotaMintaObjek_AkNotaMinta_AkNotaMintaId",
                        column: x => x.AkNotaMintaId,
                        principalTable: "AkNotaMinta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AkNotaMintaObjek_JKWPTJBahagian_JKWPTJBahagianId",
                        column: x => x.JKWPTJBahagianId,
                        principalTable: "JKWPTJBahagian",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AkNotaMintaPerihal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AkNotaMintaId = table.Column<int>(type: "int", nullable: false),
                    Bil = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Perihal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Kuantiti = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Harga = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amaun = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AkNotaMintaPerihal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AkNotaMintaPerihal_AkNotaMinta_AkNotaMintaId",
                        column: x => x.AkNotaMintaId,
                        principalTable: "AkNotaMinta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AkInden",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tahun = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoRujukan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tarikh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AkPenilaianPerolehanId = table.Column<int>(type: "int", nullable: false),
                    JangkaMasaDari = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JangkaMasaHingga = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Jumlah = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    JKWId = table.Column<int>(type: "int", nullable: false),
                    DDaftarAwamId = table.Column<int>(type: "int", nullable: false),
                    Ringkasan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoRujukanLama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DPekerjaMasukId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarMasuk = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DPekerjaKemaskiniId = table.Column<int>(type: "int", nullable: true),
                    UserIdKemaskini = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarKemaskini = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlHapus = table.Column<int>(type: "int", nullable: false),
                    TarHapus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabHapus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FlBatal = table.Column<int>(type: "int", nullable: false),
                    TarBatal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabBatal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DPengesahId = table.Column<int>(type: "int", nullable: true),
                    TarikhSah = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DPenyemakId = table.Column<int>(type: "int", nullable: true),
                    TarikhSemak = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DPelulusId = table.Column<int>(type: "int", nullable: true),
                    TarikhLulus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DPekerjaPostingId = table.Column<int>(type: "int", nullable: true),
                    FlPosting = table.Column<int>(type: "int", nullable: false),
                    TarikhPosting = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EnStatusBorang = table.Column<int>(type: "int", nullable: false),
                    Tindakan = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AkInden", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AkInden_AkPenilaianPerolehan_AkPenilaianPerolehanId",
                        column: x => x.AkPenilaianPerolehanId,
                        principalTable: "AkPenilaianPerolehan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AkInden_DDaftarAwam_DDaftarAwamId",
                        column: x => x.DDaftarAwamId,
                        principalTable: "DDaftarAwam",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AkInden_DKonfigKelulusan_DPelulusId",
                        column: x => x.DPelulusId,
                        principalTable: "DKonfigKelulusan",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkInden_DKonfigKelulusan_DPengesahId",
                        column: x => x.DPengesahId,
                        principalTable: "DKonfigKelulusan",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkInden_DKonfigKelulusan_DPenyemakId",
                        column: x => x.DPenyemakId,
                        principalTable: "DKonfigKelulusan",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkInden_DPekerja_DPekerjaPostingId",
                        column: x => x.DPekerjaPostingId,
                        principalTable: "DPekerja",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkInden_JKW_JKWId",
                        column: x => x.JKWId,
                        principalTable: "JKW",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AkPenilaianPerolehanObjek",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AkPenilaianPerolehanId = table.Column<int>(type: "int", nullable: false),
                    AkCartaId = table.Column<int>(type: "int", nullable: false),
                    JKWPTJBahagianId = table.Column<int>(type: "int", nullable: false),
                    Amaun = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AkPenilaianPerolehanObjek", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AkPenilaianPerolehanObjek_AkCarta_AkCartaId",
                        column: x => x.AkCartaId,
                        principalTable: "AkCarta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AkPenilaianPerolehanObjek_AkPenilaianPerolehan_AkPenilaianPerolehanId",
                        column: x => x.AkPenilaianPerolehanId,
                        principalTable: "AkPenilaianPerolehan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AkPenilaianPerolehanObjek_JKWPTJBahagian_JKWPTJBahagianId",
                        column: x => x.JKWPTJBahagianId,
                        principalTable: "JKWPTJBahagian",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AkPenilaianPerolehanPerihal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AkPenilaianPerolehanId = table.Column<int>(type: "int", nullable: false),
                    Bil = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Perihal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Kuantiti = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Harga = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amaun = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AkPenilaianPerolehanPerihal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AkPenilaianPerolehanPerihal_AkPenilaianPerolehan_AkPenilaianPerolehanId",
                        column: x => x.AkPenilaianPerolehanId,
                        principalTable: "AkPenilaianPerolehan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AkPO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tahun = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoRujukan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tarikh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AkPenilaianPerolehanId = table.Column<int>(type: "int", nullable: false),
                    EnJenisPerolehan = table.Column<int>(type: "int", nullable: false),
                    Jumlah = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    JKWId = table.Column<int>(type: "int", nullable: false),
                    DDaftarAwamId = table.Column<int>(type: "int", nullable: false),
                    NoRujukanLama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DPekerjaMasukId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarMasuk = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DPekerjaKemaskiniId = table.Column<int>(type: "int", nullable: true),
                    UserIdKemaskini = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarKemaskini = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlHapus = table.Column<int>(type: "int", nullable: false),
                    TarHapus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabHapus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FlBatal = table.Column<int>(type: "int", nullable: false),
                    TarBatal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabBatal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DPengesahId = table.Column<int>(type: "int", nullable: true),
                    TarikhSah = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DPenyemakId = table.Column<int>(type: "int", nullable: true),
                    TarikhSemak = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DPelulusId = table.Column<int>(type: "int", nullable: true),
                    TarikhLulus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DPekerjaPostingId = table.Column<int>(type: "int", nullable: true),
                    FlPosting = table.Column<int>(type: "int", nullable: false),
                    TarikhPosting = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EnStatusBorang = table.Column<int>(type: "int", nullable: false),
                    Tindakan = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AkPO", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AkPO_AkPenilaianPerolehan_AkPenilaianPerolehanId",
                        column: x => x.AkPenilaianPerolehanId,
                        principalTable: "AkPenilaianPerolehan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AkPO_DDaftarAwam_DDaftarAwamId",
                        column: x => x.DDaftarAwamId,
                        principalTable: "DDaftarAwam",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AkPO_DKonfigKelulusan_DPelulusId",
                        column: x => x.DPelulusId,
                        principalTable: "DKonfigKelulusan",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkPO_DKonfigKelulusan_DPengesahId",
                        column: x => x.DPengesahId,
                        principalTable: "DKonfigKelulusan",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkPO_DKonfigKelulusan_DPenyemakId",
                        column: x => x.DPenyemakId,
                        principalTable: "DKonfigKelulusan",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkPO_DPekerja_DPekerjaPostingId",
                        column: x => x.DPekerjaPostingId,
                        principalTable: "DPekerja",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkPO_JKW_JKWId",
                        column: x => x.JKWId,
                        principalTable: "JKW",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AkIndenObjek",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AkIndenId = table.Column<int>(type: "int", nullable: false),
                    AkCartaId = table.Column<int>(type: "int", nullable: false),
                    JKWPTJBahagianId = table.Column<int>(type: "int", nullable: false),
                    Amaun = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AkIndenObjek", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AkIndenObjek_AkCarta_AkCartaId",
                        column: x => x.AkCartaId,
                        principalTable: "AkCarta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AkIndenObjek_AkInden_AkIndenId",
                        column: x => x.AkIndenId,
                        principalTable: "AkInden",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AkIndenObjek_JKWPTJBahagian_JKWPTJBahagianId",
                        column: x => x.JKWPTJBahagianId,
                        principalTable: "JKWPTJBahagian",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AkIndenPerihal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AkIndenId = table.Column<int>(type: "int", nullable: false),
                    Bil = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Perihal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Kuantiti = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Harga = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amaun = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AkIndenPerihal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AkIndenPerihal_AkInden_AkIndenId",
                        column: x => x.AkIndenId,
                        principalTable: "AkInden",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AkPelarasanInden",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tahun = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoRujukan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tarikh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AkIndenId = table.Column<int>(type: "int", nullable: false),
                    Jumlah = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    JKWId = table.Column<int>(type: "int", nullable: false),
                    Ringkasan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoRujukanLama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DPekerjaMasukId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarMasuk = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DPekerjaKemaskiniId = table.Column<int>(type: "int", nullable: true),
                    UserIdKemaskini = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarKemaskini = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlHapus = table.Column<int>(type: "int", nullable: false),
                    TarHapus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabHapus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FlBatal = table.Column<int>(type: "int", nullable: false),
                    TarBatal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabBatal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DPengesahId = table.Column<int>(type: "int", nullable: true),
                    TarikhSah = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DPenyemakId = table.Column<int>(type: "int", nullable: true),
                    TarikhSemak = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DPelulusId = table.Column<int>(type: "int", nullable: true),
                    TarikhLulus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DPekerjaPostingId = table.Column<int>(type: "int", nullable: true),
                    FlPosting = table.Column<int>(type: "int", nullable: false),
                    TarikhPosting = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EnStatusBorang = table.Column<int>(type: "int", nullable: false),
                    Tindakan = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AkPelarasanInden", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AkPelarasanInden_AkInden_AkIndenId",
                        column: x => x.AkIndenId,
                        principalTable: "AkInden",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AkPelarasanInden_DKonfigKelulusan_DPelulusId",
                        column: x => x.DPelulusId,
                        principalTable: "DKonfigKelulusan",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkPelarasanInden_DKonfigKelulusan_DPengesahId",
                        column: x => x.DPengesahId,
                        principalTable: "DKonfigKelulusan",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkPelarasanInden_DKonfigKelulusan_DPenyemakId",
                        column: x => x.DPenyemakId,
                        principalTable: "DKonfigKelulusan",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkPelarasanInden_DPekerja_DPekerjaPostingId",
                        column: x => x.DPekerjaPostingId,
                        principalTable: "DPekerja",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkPelarasanInden_JKW_JKWId",
                        column: x => x.JKWId,
                        principalTable: "JKW",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AkPelarasanPO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tahun = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoRujukan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tarikh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AkPOId = table.Column<int>(type: "int", nullable: false),
                    Jumlah = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    JKWId = table.Column<int>(type: "int", nullable: false),
                    Ringkasan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoRujukanLama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DPekerjaMasukId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarMasuk = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DPekerjaKemaskiniId = table.Column<int>(type: "int", nullable: true),
                    UserIdKemaskini = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarKemaskini = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlHapus = table.Column<int>(type: "int", nullable: false),
                    TarHapus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabHapus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FlBatal = table.Column<int>(type: "int", nullable: false),
                    TarBatal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SebabBatal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DPengesahId = table.Column<int>(type: "int", nullable: true),
                    TarikhSah = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DPenyemakId = table.Column<int>(type: "int", nullable: true),
                    TarikhSemak = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DPelulusId = table.Column<int>(type: "int", nullable: true),
                    TarikhLulus = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DPekerjaPostingId = table.Column<int>(type: "int", nullable: true),
                    FlPosting = table.Column<int>(type: "int", nullable: false),
                    TarikhPosting = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EnStatusBorang = table.Column<int>(type: "int", nullable: false),
                    Tindakan = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AkPelarasanPO", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AkPelarasanPO_AkPO_AkPOId",
                        column: x => x.AkPOId,
                        principalTable: "AkPO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AkPelarasanPO_DKonfigKelulusan_DPelulusId",
                        column: x => x.DPelulusId,
                        principalTable: "DKonfigKelulusan",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkPelarasanPO_DKonfigKelulusan_DPengesahId",
                        column: x => x.DPengesahId,
                        principalTable: "DKonfigKelulusan",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkPelarasanPO_DKonfigKelulusan_DPenyemakId",
                        column: x => x.DPenyemakId,
                        principalTable: "DKonfigKelulusan",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkPelarasanPO_DPekerja_DPekerjaPostingId",
                        column: x => x.DPekerjaPostingId,
                        principalTable: "DPekerja",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AkPelarasanPO_JKW_JKWId",
                        column: x => x.JKWId,
                        principalTable: "JKW",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AkPOObjek",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AkPOId = table.Column<int>(type: "int", nullable: false),
                    AkCartaId = table.Column<int>(type: "int", nullable: false),
                    JKWPTJBahagianId = table.Column<int>(type: "int", nullable: false),
                    Amaun = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AkPOObjek", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AkPOObjek_AkCarta_AkCartaId",
                        column: x => x.AkCartaId,
                        principalTable: "AkCarta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AkPOObjek_AkPO_AkPOId",
                        column: x => x.AkPOId,
                        principalTable: "AkPO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AkPOObjek_JKWPTJBahagian_JKWPTJBahagianId",
                        column: x => x.JKWPTJBahagianId,
                        principalTable: "JKWPTJBahagian",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AkPOPerihal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AkPOId = table.Column<int>(type: "int", nullable: false),
                    Bil = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Perihal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Kuantiti = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Harga = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amaun = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AkPOPerihal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AkPOPerihal_AkPO_AkPOId",
                        column: x => x.AkPOId,
                        principalTable: "AkPO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AkPelarasanIndenObjek",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AkPelarasanIndenId = table.Column<int>(type: "int", nullable: false),
                    AkCartaId = table.Column<int>(type: "int", nullable: false),
                    JKWPTJBahagianId = table.Column<int>(type: "int", nullable: false),
                    Amaun = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AkPelarasanIndenObjek", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AkPelarasanIndenObjek_AkCarta_AkCartaId",
                        column: x => x.AkCartaId,
                        principalTable: "AkCarta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AkPelarasanIndenObjek_AkPelarasanInden_AkPelarasanIndenId",
                        column: x => x.AkPelarasanIndenId,
                        principalTable: "AkPelarasanInden",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AkPelarasanIndenObjek_JKWPTJBahagian_JKWPTJBahagianId",
                        column: x => x.JKWPTJBahagianId,
                        principalTable: "JKWPTJBahagian",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AkPelarasanIndenPerihal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AkPelarasanIndenId = table.Column<int>(type: "int", nullable: false),
                    Bil = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Perihal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Kuantiti = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Harga = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amaun = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AkPelarasanIndenPerihal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AkPelarasanIndenPerihal_AkPelarasanInden_AkPelarasanIndenId",
                        column: x => x.AkPelarasanIndenId,
                        principalTable: "AkPelarasanInden",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AkPelarasanPOObjek",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AkPelarasanPOId = table.Column<int>(type: "int", nullable: false),
                    AkCartaId = table.Column<int>(type: "int", nullable: false),
                    JKWPTJBahagianId = table.Column<int>(type: "int", nullable: false),
                    Amaun = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AkPelarasanPOObjek", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AkPelarasanPOObjek_AkCarta_AkCartaId",
                        column: x => x.AkCartaId,
                        principalTable: "AkCarta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AkPelarasanPOObjek_AkPelarasanPO_AkPelarasanPOId",
                        column: x => x.AkPelarasanPOId,
                        principalTable: "AkPelarasanPO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AkPelarasanPOObjek_JKWPTJBahagian_JKWPTJBahagianId",
                        column: x => x.JKWPTJBahagianId,
                        principalTable: "JKWPTJBahagian",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AkPelarasanPOPerihal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AkPelarasanPOId = table.Column<int>(type: "int", nullable: false),
                    Bil = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Perihal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Kuantiti = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Harga = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amaun = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AkPelarasanPOPerihal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AkPelarasanPOPerihal_AkPelarasanPO_AkPelarasanPOId",
                        column: x => x.AkPelarasanPOId,
                        principalTable: "AkPelarasanPO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AbBukuVot_DDaftarAwamId",
                table: "AbBukuVot",
                column: "DDaftarAwamId");

            migrationBuilder.CreateIndex(
                name: "IX_AbBukuVot_JBahagianId",
                table: "AbBukuVot",
                column: "JBahagianId");

            migrationBuilder.CreateIndex(
                name: "IX_AbBukuVot_JKWId",
                table: "AbBukuVot",
                column: "JKWId");

            migrationBuilder.CreateIndex(
                name: "IX_AbBukuVot_JPTJId",
                table: "AbBukuVot",
                column: "JPTJId");

            migrationBuilder.CreateIndex(
                name: "IX_AbBukuVot_VotId",
                table: "AbBukuVot",
                column: "VotId");

            migrationBuilder.CreateIndex(
                name: "IX_AbWaran_DPekerjaPostingId",
                table: "AbWaran",
                column: "DPekerjaPostingId");

            migrationBuilder.CreateIndex(
                name: "IX_AbWaran_DPelulusId",
                table: "AbWaran",
                column: "DPelulusId");

            migrationBuilder.CreateIndex(
                name: "IX_AbWaran_DPengesahId",
                table: "AbWaran",
                column: "DPengesahId");

            migrationBuilder.CreateIndex(
                name: "IX_AbWaran_DPenyemakId",
                table: "AbWaran",
                column: "DPenyemakId");

            migrationBuilder.CreateIndex(
                name: "IX_AbWaran_JKWId",
                table: "AbWaran",
                column: "JKWId");

            migrationBuilder.CreateIndex(
                name: "IX_AbWaranObjek_AbWaranId",
                table: "AbWaranObjek",
                column: "AbWaranId");

            migrationBuilder.CreateIndex(
                name: "IX_AbWaranObjek_AkCartaId",
                table: "AbWaranObjek",
                column: "AkCartaId");

            migrationBuilder.CreateIndex(
                name: "IX_AbWaranObjek_JKWPTJBahagianId",
                table: "AbWaranObjek",
                column: "JKWPTJBahagianId");

            migrationBuilder.CreateIndex(
                name: "IX_AkAkaun_AkCarta1Id",
                table: "AkAkaun",
                column: "AkCarta1Id");

            migrationBuilder.CreateIndex(
                name: "IX_AkAkaun_AkCarta2Id",
                table: "AkAkaun",
                column: "AkCarta2Id");

            migrationBuilder.CreateIndex(
                name: "IX_AkAkaun_JBahagianId",
                table: "AkAkaun",
                column: "JBahagianId");

            migrationBuilder.CreateIndex(
                name: "IX_AkAkaun_JKWId",
                table: "AkAkaun",
                column: "JKWId");

            migrationBuilder.CreateIndex(
                name: "IX_AkAkaun_JPTJId",
                table: "AkAkaun",
                column: "JPTJId");

            migrationBuilder.CreateIndex(
                name: "IX_AkBank_AkCartaId",
                table: "AkBank",
                column: "AkCartaId");

            migrationBuilder.CreateIndex(
                name: "IX_AkBank_JBankId",
                table: "AkBank",
                column: "JBankId");

            migrationBuilder.CreateIndex(
                name: "IX_AkBank_JKWId",
                table: "AkBank",
                column: "JKWId");

            migrationBuilder.CreateIndex(
                name: "IX_AkInden_AkPenilaianPerolehanId",
                table: "AkInden",
                column: "AkPenilaianPerolehanId");

            migrationBuilder.CreateIndex(
                name: "IX_AkInden_DDaftarAwamId",
                table: "AkInden",
                column: "DDaftarAwamId");

            migrationBuilder.CreateIndex(
                name: "IX_AkInden_DPekerjaPostingId",
                table: "AkInden",
                column: "DPekerjaPostingId");

            migrationBuilder.CreateIndex(
                name: "IX_AkInden_DPelulusId",
                table: "AkInden",
                column: "DPelulusId");

            migrationBuilder.CreateIndex(
                name: "IX_AkInden_DPengesahId",
                table: "AkInden",
                column: "DPengesahId");

            migrationBuilder.CreateIndex(
                name: "IX_AkInden_DPenyemakId",
                table: "AkInden",
                column: "DPenyemakId");

            migrationBuilder.CreateIndex(
                name: "IX_AkInden_JKWId",
                table: "AkInden",
                column: "JKWId");

            migrationBuilder.CreateIndex(
                name: "IX_AkIndenObjek_AkCartaId",
                table: "AkIndenObjek",
                column: "AkCartaId");

            migrationBuilder.CreateIndex(
                name: "IX_AkIndenObjek_AkIndenId",
                table: "AkIndenObjek",
                column: "AkIndenId");

            migrationBuilder.CreateIndex(
                name: "IX_AkIndenObjek_JKWPTJBahagianId",
                table: "AkIndenObjek",
                column: "JKWPTJBahagianId");

            migrationBuilder.CreateIndex(
                name: "IX_AkIndenPerihal_AkIndenId",
                table: "AkIndenPerihal",
                column: "AkIndenId");

            migrationBuilder.CreateIndex(
                name: "IX_AkNotaMinta_DDaftarAwamId",
                table: "AkNotaMinta",
                column: "DDaftarAwamId");

            migrationBuilder.CreateIndex(
                name: "IX_AkNotaMinta_DPekerjaPostingId",
                table: "AkNotaMinta",
                column: "DPekerjaPostingId");

            migrationBuilder.CreateIndex(
                name: "IX_AkNotaMinta_DPelulusId",
                table: "AkNotaMinta",
                column: "DPelulusId");

            migrationBuilder.CreateIndex(
                name: "IX_AkNotaMinta_DPemohonId",
                table: "AkNotaMinta",
                column: "DPemohonId");

            migrationBuilder.CreateIndex(
                name: "IX_AkNotaMinta_DPengesahId",
                table: "AkNotaMinta",
                column: "DPengesahId");

            migrationBuilder.CreateIndex(
                name: "IX_AkNotaMinta_DPenyemakId",
                table: "AkNotaMinta",
                column: "DPenyemakId");

            migrationBuilder.CreateIndex(
                name: "IX_AkNotaMinta_JKWId",
                table: "AkNotaMinta",
                column: "JKWId");

            migrationBuilder.CreateIndex(
                name: "IX_AkNotaMintaObjek_AkCartaId",
                table: "AkNotaMintaObjek",
                column: "AkCartaId");

            migrationBuilder.CreateIndex(
                name: "IX_AkNotaMintaObjek_AkNotaMintaId",
                table: "AkNotaMintaObjek",
                column: "AkNotaMintaId");

            migrationBuilder.CreateIndex(
                name: "IX_AkNotaMintaObjek_JKWPTJBahagianId",
                table: "AkNotaMintaObjek",
                column: "JKWPTJBahagianId");

            migrationBuilder.CreateIndex(
                name: "IX_AkNotaMintaPerihal_AkNotaMintaId",
                table: "AkNotaMintaPerihal",
                column: "AkNotaMintaId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPelarasanInden_AkIndenId",
                table: "AkPelarasanInden",
                column: "AkIndenId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPelarasanInden_DPekerjaPostingId",
                table: "AkPelarasanInden",
                column: "DPekerjaPostingId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPelarasanInden_DPelulusId",
                table: "AkPelarasanInden",
                column: "DPelulusId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPelarasanInden_DPengesahId",
                table: "AkPelarasanInden",
                column: "DPengesahId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPelarasanInden_DPenyemakId",
                table: "AkPelarasanInden",
                column: "DPenyemakId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPelarasanInden_JKWId",
                table: "AkPelarasanInden",
                column: "JKWId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPelarasanIndenObjek_AkCartaId",
                table: "AkPelarasanIndenObjek",
                column: "AkCartaId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPelarasanIndenObjek_AkPelarasanIndenId",
                table: "AkPelarasanIndenObjek",
                column: "AkPelarasanIndenId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPelarasanIndenObjek_JKWPTJBahagianId",
                table: "AkPelarasanIndenObjek",
                column: "JKWPTJBahagianId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPelarasanIndenPerihal_AkPelarasanIndenId",
                table: "AkPelarasanIndenPerihal",
                column: "AkPelarasanIndenId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPelarasanPO_AkPOId",
                table: "AkPelarasanPO",
                column: "AkPOId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPelarasanPO_DPekerjaPostingId",
                table: "AkPelarasanPO",
                column: "DPekerjaPostingId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPelarasanPO_DPelulusId",
                table: "AkPelarasanPO",
                column: "DPelulusId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPelarasanPO_DPengesahId",
                table: "AkPelarasanPO",
                column: "DPengesahId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPelarasanPO_DPenyemakId",
                table: "AkPelarasanPO",
                column: "DPenyemakId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPelarasanPO_JKWId",
                table: "AkPelarasanPO",
                column: "JKWId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPelarasanPOObjek_AkCartaId",
                table: "AkPelarasanPOObjek",
                column: "AkCartaId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPelarasanPOObjek_AkPelarasanPOId",
                table: "AkPelarasanPOObjek",
                column: "AkPelarasanPOId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPelarasanPOObjek_JKWPTJBahagianId",
                table: "AkPelarasanPOObjek",
                column: "JKWPTJBahagianId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPelarasanPOPerihal_AkPelarasanPOId",
                table: "AkPelarasanPOPerihal",
                column: "AkPelarasanPOId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPenilaianPerolehan_DDaftarAwamId",
                table: "AkPenilaianPerolehan",
                column: "DDaftarAwamId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPenilaianPerolehan_DPekerjaPostingId",
                table: "AkPenilaianPerolehan",
                column: "DPekerjaPostingId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPenilaianPerolehan_DPelulusId",
                table: "AkPenilaianPerolehan",
                column: "DPelulusId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPenilaianPerolehan_DPemohonId",
                table: "AkPenilaianPerolehan",
                column: "DPemohonId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPenilaianPerolehan_DPengesahId",
                table: "AkPenilaianPerolehan",
                column: "DPengesahId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPenilaianPerolehan_DPenyemakId",
                table: "AkPenilaianPerolehan",
                column: "DPenyemakId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPenilaianPerolehan_JKWId",
                table: "AkPenilaianPerolehan",
                column: "JKWId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPenilaianPerolehanObjek_AkCartaId",
                table: "AkPenilaianPerolehanObjek",
                column: "AkCartaId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPenilaianPerolehanObjek_AkPenilaianPerolehanId",
                table: "AkPenilaianPerolehanObjek",
                column: "AkPenilaianPerolehanId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPenilaianPerolehanObjek_JKWPTJBahagianId",
                table: "AkPenilaianPerolehanObjek",
                column: "JKWPTJBahagianId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPenilaianPerolehanPerihal_AkPenilaianPerolehanId",
                table: "AkPenilaianPerolehanPerihal",
                column: "AkPenilaianPerolehanId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPO_AkPenilaianPerolehanId",
                table: "AkPO",
                column: "AkPenilaianPerolehanId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPO_DDaftarAwamId",
                table: "AkPO",
                column: "DDaftarAwamId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPO_DPekerjaPostingId",
                table: "AkPO",
                column: "DPekerjaPostingId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPO_DPelulusId",
                table: "AkPO",
                column: "DPelulusId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPO_DPengesahId",
                table: "AkPO",
                column: "DPengesahId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPO_DPenyemakId",
                table: "AkPO",
                column: "DPenyemakId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPO_JKWId",
                table: "AkPO",
                column: "JKWId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPOObjek_AkCartaId",
                table: "AkPOObjek",
                column: "AkCartaId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPOObjek_AkPOId",
                table: "AkPOObjek",
                column: "AkPOId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPOObjek_JKWPTJBahagianId",
                table: "AkPOObjek",
                column: "JKWPTJBahagianId");

            migrationBuilder.CreateIndex(
                name: "IX_AkPOPerihal_AkPOId",
                table: "AkPOPerihal",
                column: "AkPOId");

            migrationBuilder.CreateIndex(
                name: "IX_AkTerima_AkBankId",
                table: "AkTerima",
                column: "AkBankId");

            migrationBuilder.CreateIndex(
                name: "IX_AkTerima_DDaftarAwamId",
                table: "AkTerima",
                column: "DDaftarAwamId");

            migrationBuilder.CreateIndex(
                name: "IX_AkTerima_DPekerjaPostingId",
                table: "AkTerima",
                column: "DPekerjaPostingId");

            migrationBuilder.CreateIndex(
                name: "IX_AkTerima_JCawanganId",
                table: "AkTerima",
                column: "JCawanganId");

            migrationBuilder.CreateIndex(
                name: "IX_AkTerima_JKWId",
                table: "AkTerima",
                column: "JKWId");

            migrationBuilder.CreateIndex(
                name: "IX_AkTerima_JNegeriId",
                table: "AkTerima",
                column: "JNegeriId");

            migrationBuilder.CreateIndex(
                name: "IX_AkTerimaCaraBayar_AkTerimaId",
                table: "AkTerimaCaraBayar",
                column: "AkTerimaId");

            migrationBuilder.CreateIndex(
                name: "IX_AkTerimaCaraBayar_JCaraBayarId",
                table: "AkTerimaCaraBayar",
                column: "JCaraBayarId");

            migrationBuilder.CreateIndex(
                name: "IX_AkTerimaObjek_AkCartaId",
                table: "AkTerimaObjek",
                column: "AkCartaId");

            migrationBuilder.CreateIndex(
                name: "IX_AkTerimaObjek_AkTerimaId",
                table: "AkTerimaObjek",
                column: "AkTerimaId");

            migrationBuilder.CreateIndex(
                name: "IX_AkTerimaObjek_JKWPTJBahagianId",
                table: "AkTerimaObjek",
                column: "JKWPTJBahagianId");

            migrationBuilder.CreateIndex(
                name: "IX_AppLog_DPekerjaId",
                table: "AppLog",
                column: "DPekerjaId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DPekerjaId",
                table: "AspNetUsers",
                column: "DPekerjaId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DDaftarAwam_JBankId",
                table: "DDaftarAwam",
                column: "JBankId");

            migrationBuilder.CreateIndex(
                name: "IX_DDaftarAwam_JNegeriId",
                table: "DDaftarAwam",
                column: "JNegeriId");

            migrationBuilder.CreateIndex(
                name: "IX_DKonfigKelulusan_DPekerjaId",
                table: "DKonfigKelulusan",
                column: "DPekerjaId");

            migrationBuilder.CreateIndex(
                name: "IX_DKonfigKelulusan_JBahagianId",
                table: "DKonfigKelulusan",
                column: "JBahagianId");

            migrationBuilder.CreateIndex(
                name: "IX_DPekerja_JBahagianId",
                table: "DPekerja",
                column: "JBahagianId");

            migrationBuilder.CreateIndex(
                name: "IX_DPekerja_JBangsaId",
                table: "DPekerja",
                column: "JBangsaId");

            migrationBuilder.CreateIndex(
                name: "IX_DPekerja_JBankId",
                table: "DPekerja",
                column: "JBankId");

            migrationBuilder.CreateIndex(
                name: "IX_DPekerja_JCawanganId",
                table: "DPekerja",
                column: "JCawanganId");

            migrationBuilder.CreateIndex(
                name: "IX_DPekerja_JNegeriId",
                table: "DPekerja",
                column: "JNegeriId");

            migrationBuilder.CreateIndex(
                name: "IX_DPekerja_JPTJId",
                table: "DPekerja",
                column: "JPTJId");

            migrationBuilder.CreateIndex(
                name: "IX_DPenyelia_DPekerjaId",
                table: "DPenyelia",
                column: "DPekerjaId");

            migrationBuilder.CreateIndex(
                name: "IX_DPenyelia_JCawanganId",
                table: "DPenyelia",
                column: "JCawanganId");

            migrationBuilder.CreateIndex(
                name: "IX_JCawangan_AkBankId",
                table: "JCawangan",
                column: "AkBankId");

            migrationBuilder.CreateIndex(
                name: "IX_JKWPTJBahagian_JBahagianId",
                table: "JKWPTJBahagian",
                column: "JBahagianId");

            migrationBuilder.CreateIndex(
                name: "IX_JKWPTJBahagian_JKWId",
                table: "JKWPTJBahagian",
                column: "JKWId");

            migrationBuilder.CreateIndex(
                name: "IX_JKWPTJBahagian_JPTJId",
                table: "JKWPTJBahagian",
                column: "JPTJId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbBukuVot");

            migrationBuilder.DropTable(
                name: "AbWaranObjek");

            migrationBuilder.DropTable(
                name: "AkAkaun");

            migrationBuilder.DropTable(
                name: "AkIndenObjek");

            migrationBuilder.DropTable(
                name: "AkIndenPerihal");

            migrationBuilder.DropTable(
                name: "AkNotaMintaObjek");

            migrationBuilder.DropTable(
                name: "AkNotaMintaPerihal");

            migrationBuilder.DropTable(
                name: "AkPelarasanIndenObjek");

            migrationBuilder.DropTable(
                name: "AkPelarasanIndenPerihal");

            migrationBuilder.DropTable(
                name: "AkPelarasanPOObjek");

            migrationBuilder.DropTable(
                name: "AkPelarasanPOPerihal");

            migrationBuilder.DropTable(
                name: "AkPenilaianPerolehanObjek");

            migrationBuilder.DropTable(
                name: "AkPenilaianPerolehanPerihal");

            migrationBuilder.DropTable(
                name: "AkPOObjek");

            migrationBuilder.DropTable(
                name: "AkPOPerihal");

            migrationBuilder.DropTable(
                name: "AkTerimaCaraBayar");

            migrationBuilder.DropTable(
                name: "AkTerimaObjek");

            migrationBuilder.DropTable(
                name: "AppLog");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "DPenyelia");

            migrationBuilder.DropTable(
                name: "ExceptionLogger");

            migrationBuilder.DropTable(
                name: "JAgama");

            migrationBuilder.DropTable(
                name: "SiAppInfo");

            migrationBuilder.DropTable(
                name: "AbWaran");

            migrationBuilder.DropTable(
                name: "AkNotaMinta");

            migrationBuilder.DropTable(
                name: "AkPelarasanInden");

            migrationBuilder.DropTable(
                name: "AkPelarasanPO");

            migrationBuilder.DropTable(
                name: "JCaraBayar");

            migrationBuilder.DropTable(
                name: "AkTerima");

            migrationBuilder.DropTable(
                name: "JKWPTJBahagian");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "AkInden");

            migrationBuilder.DropTable(
                name: "AkPO");

            migrationBuilder.DropTable(
                name: "AkPenilaianPerolehan");

            migrationBuilder.DropTable(
                name: "DDaftarAwam");

            migrationBuilder.DropTable(
                name: "DKonfigKelulusan");

            migrationBuilder.DropTable(
                name: "DPekerja");

            migrationBuilder.DropTable(
                name: "JBahagian");

            migrationBuilder.DropTable(
                name: "JBangsa");

            migrationBuilder.DropTable(
                name: "JCawangan");

            migrationBuilder.DropTable(
                name: "JNegeri");

            migrationBuilder.DropTable(
                name: "JPTJ");

            migrationBuilder.DropTable(
                name: "AkBank");

            migrationBuilder.DropTable(
                name: "AkCarta");

            migrationBuilder.DropTable(
                name: "JBank");

            migrationBuilder.DropTable(
                name: "JKW");
        }
    }
}
