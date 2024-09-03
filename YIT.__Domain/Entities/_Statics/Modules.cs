using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities._Statics
{
    public static class Modules
    {
        // Sistem (SI)
        public const string kodSiAppInfo = "SI0001";
        public const string namaSiAppInfo = "Sistem Info Syarikat";
        //

        // Jadual (JD)
        public const string kodJKW = "JD0001";
        public const string namaJKW = "Jadual Kumpulan Wang";
        
        public const string kodJPTJ = "JD0002";
        public const string namaJPTJ = "Jadual Pusat Tanggungjawab";

        public const string kodJBahagian = "JD0003";
        public const string namaJBahagian = "Jadual Bahagian";

        public const string kodJBank = "JD0004";
        public const string namaJBank = "Jadual Bank";

        public const string kodJCaraBayar = "JD0005";
        public const string namaJCaraBayar = "Jadual Cara Bayar";

        public const string kodJAgama = "JD0006";
        public const string namaJAgama = " Jadual Agama";

        public const string kodJBangsa= "JD0007";
        public const string namaJBangsa = " Jadual Bangsa";

        public const string kodJNegeri = "JD0008";
        public const string namaJNegeri = "Jadual Negeri";

        public const string kodJCawangan = "JD0009";
        public const string namaJCawangan = "Jadual Cawangan";

        public const string kodJKonfigPerubahanEkuiti = "JD0010";
        public const string namaJKonfigPerubahanEkuiti = "Jadual Konfigurasi Perubahan Ekuiti";

        public const string kodJKonfigPenyata = "JD0011";
        public const string namaJKonfigPenyata = "Jadual Konfigurasi Penyata";
        //

        // Daftar (DF)
        public const string kodDPekerja = "DF0001";
        public const string namaDPekerja = "Daftar Anggota";

        public const string kodDDaftarAwam = "DF0002";
        public const string namaDDaftarAwam = "Daftar Awam";

        public const string kodDKonfigKelulusan = "DF0003";
        public const string namaDKonfigKelulusan = "Daftar Konfigurasi Kelulusan";

        public const string kodDPenerimaCekGaji = "DF0004";
        public const string namaDPenerimaCekGaji = "Daftar Penerima Cek Gaji";

        public const string kodDPanjar = "DF0005";
        public const string namaDPanjar = "Daftar Pemegang Panjar";

        // Akaun (AK)
        public const string kodAkCarta = "AK0001";
        public const string namaAkCarta = "Carta Akaun";

        public const string kodAkBank = "AK0002";
        public const string namaAkBank = " Akaun Bank";

        public const string kodAkAkaun = "AK0003";
        public const string namaAkAkaun = "Akaun Bank Utama";

        public const string kodAbBukuVot = "AK0004";
        public const string namaAbBukuVot = "Belanjawan Buku Vot";

        public const string kodAbWaran = "AK0005";
        public const string namaAbWaran = "Belanjawan Waran";

        public const string kodAkPenilaianPerolehan = "AK0006";
        public const string namaAkPenilaianPerolehan = "Perolehan Penilaian Perolehan";

        public const string kodAkNotaMinta = "AK0007";
        public const string namaAkNotaMinta = "Perolehan Nota Minta";

        public const string kodAkPO = "AK0008";
        public const string namaAkPO = "Tanggungan Pesanan Tempatan";

        public const string kodAkPelarasanPO = "AK0009";
        public const string namaAkPelarasanPO = "Pelarasan Pesanan Tempatan";

        public const string kodAkInden = "AK0010";
        public const string namaAkInden = "Tanggungan Inden Kerja";

        public const string kodAkPelarasanInden = "AK0011";
        public const string namaAkPelarasanInden = "Pelarasan Inden Kerja";

        public const string kodAkBelian = "AK0012";
        public const string namaAkBelian = "Tanggungan Invois Pembekal";

        public const string kodAkPV = "AK0013";
        public const string namaAkPV = "Baucer Pembayaran";

        public const string kodAkJanaanProfil = "AK0014";
        public const string namaAkJanaanProfil = "Janaan Profil";

        public const string kodAkEFTMaybank2E = "AK0015";
        public const string namaAkEFTMaybank2E = "Janaan EFT Maybank2E";

        public const string kodAkJurnal = "AK0016";
        public const string namaAkJurnal = "Pelarasan Baucer Jurnal";

        public const string kodAkCV = "AK0017";
        public const string namaAkCV = "Baucer Tunai Keluar";

        public const string kodAkTerima = "AK0018";
        public const string namaAkTerima = "Terimaan Resit Rasmi";

        public const string kodAkAnggar = "AK0019";
        public const string namaAkAnggar = "Anggaran Hasil";

        public const string kodAkNotaDebitKreditDiterima = "AK0020";
        public const string namaAkNotaDebitKreditDiterima = "Pelarasan Nota Debit Kredit Diterima";

        public const string kodAkInvois = "AK0021";
        public const string namaAkInvois = "Terimaan Invois Dikeluarkan";

        public const string kodAkNotaDebitKreditDikeluarkan = "AK0022";
        public const string namaAkNotaDebitKreditDikeluarkan = "Pelarasan Nota Debit Kredit Dikeluarkan";
        //

        // Pemprosesan (PP)
        public const string kodSahAbWaran = "PP0001";
        public const string namaSahAbWaran = "Pemprosesan Sah Waran";

        public const string kodSemakAbWaran = "PP0002";
        public const string namaSemakAbWaran = "Pemprosesan Semak Waran";

        public const string kodLulusAbWaran = "PP0003";
        public const string namaLulusAbWaran = "Pemprosesan Lulus Waran";

        public const string kodSahAkPenilaianPerolehan = "PP0004";
        public const string namaSahAkPenilaianPerolehan = "Pemprosesan Sah Penilaian Perolehan";

        public const string kodSemakAkPenilaianPerolehan = "PP0005";
        public const string namaSemakAkPenilaianPerolehan = "Pemprosesan Semak Penilaian Perolehan";

        public const string kodLulusAkPenilaianPerolehan = "PP0006";
        public const string namaLulusAkPenilaianPerolehan = "Pemprosesan Lulus Penilaian Perolehan";

        public const string kodSahAkNotaMinta = "PP0007";
        public const string namaSahAkNotaMinta = "Pemprosesan Sah Nota Minta";

        public const string kodSemakAkNotaMinta = "PP0008";
        public const string namaSemakAkNotaMinta = "Pemprosesan Semak Nota Minta";

        public const string kodLulusAkNotaMinta = "PP0009";
        public const string namaLulusAkNotaMinta = "Pemprosesan Lulus Nota Minta";

        public const string kodSahAkPO = "PP0010";
        public const string namaSahAkPO = "Pemprosesan Sah Pesanan Tempatan";

        public const string kodSemakAkPO = "PP0011";
        public const string namaSemakAkPO = "Pemprosesan Semak Pesanan Tempatan";

        public const string kodLulusAkPO = "PP0012";
        public const string namaLulusAkPO = "Pemprosesan Lulus Pesanan Tempatan";

        public const string kodSahAkInden = "PP0013";
        public const string namaSahAkInden = "Pemprosesan Sah Inden Kerja";

        public const string kodSemakAkInden = "PP0014";
        public const string namaSemakAkInden = "Pemprosesan Semak Inden Kerja";

        public const string kodLulusAkInden = "PP0015";
        public const string namaLulusAkInden = "Pemprosesan Lulus Inden Kerja";

        public const string kodSahAkPelarasanPO = "PP0016";
        public const string namaSahAkPelarasanPO = "Pemprosesan Sah Pelarasan Pesanan Tempatan";

        public const string kodSemakAkPelarasanPO = "PP0017";
        public const string namaSemakAkPelarasanPO = "Pemprosesan Semak Pelarasan Pesanan Tempatan";

        public const string kodLulusAkPelarasanPO = "PP0018";
        public const string namaLulusAkPelarasanPO = "Pemprosesan Lulus Pelarasan Pesanan Tempatan";

        public const string kodSahAkPelarasanInden = "PP0019";
        public const string namaSahAkPelarasanInden = "Pemprosesan Sah Pelarasan Inden Kerja";

        public const string kodSemakAkPelarasanInden = "PP0020";
        public const string namaSemakAkPelarasanInden = "Pemprosesan Semak Pelarasan Inden Kerja";

        public const string kodLulusAkPelarasanInden = "PP0021";
        public const string namaLulusAkPelarasanInden = "Pemprosesan Lulus Pelarasan Inden Kerja";

        public const string kodLulusAkBelian = "PP0022";
        public const string namaLulusAkBelian = "Pemprosesan Lulus Invois Pembekal";

        public const string kodLulusAkPV = "PP0023";
        public const string namaLulusAkPV = "Pemprosesan Lulus Baucer Pembayaran";

        public const string kodSahAkJurnal = "PP0024";
        public const string namaSahAkJurnal = "Pemprosesan Sah Baucer Jurnal";

        public const string kodSemakAkJurnal = "PP0025";
        public const string namaSemakAkJurnal = "Pemprosesan Semak Baucer Jurnal";

        public const string kodLulusAkJurnal = "PP0026";
        public const string namaLulusAkJurnal = "Pemprosesan Lulus Baucer Jurnal";

        public const string kodLulusAkNotaDebitKreditDiterima = "PP0027";
        public const string namaLulusAkNotaDebitKreditDiterima = "Pemprosesan Lulus Nota Debit Kredit Diterima";

        public const string kodLulusAkInvois = "PP0028";
        public const string namaLulusAkInvois = "Pemprosesan Lulus Invois Dikeluarkan";

        public const string kodLulusAkNotaDebitKreditDikeluarkan = "PP0029";
        public const string namaLulusAkNotaDebitKreditDikeluarkan = "Pemprosesan Lulus Nota Debit Kredit Dikeluarkan";
        //

        // Laporan Penyata (PAK)
        public const string kodPAkBukuTunai = "PAK001";
        public const string namaPAkBukuTunai = "Penyata Buku Tunai";

        public const string kodPAkAlirTunaiBulanan = "PAK002";
        public const string namaPAkAlirTunaiBulanan = "Penyata Alir Tunai Bulanan";

        public const string kodPAkAlirTunaiTahunan = "PAK003";
        public const string namaPAkAlirTunaiTahunan = "Penyata Alir Tunai";

        public const string kodPAkPerubahanEkuiti = "PAK004";
        public const string namaPAkPerubahanEkuti = "Penyata Perubahan Ekuiti";

        public const string kodPAkTimbangDuga = "PAK005";
        public const string namaPAkTimbangDuga = "Penyata Timbang Duga";

        public const string kodPAkUntungRugi = "PAK006";
        public const string namaPAkUntungRugi = "Penyata Untung Rugi";

        public const string kodPAkKunciKiraKira = "PAK007";
        public const string namaPAkKunciKiraKira = "Penyata Kunci Kira - Kira";
        //

        // Laporan Akaun (LAK)
        public const string kodLCekBelumTunai = "LAK001";
        public const string namaLCekBelumTunai = "Laporan Cek Belum Tunai";

        public const string kodLCekYangBerkaitan = "LAK002";
        public const string namaLCekYangBerkaitan = "Laporan Cek Yang Berkaitan";

        public const string kodLPenerimaanPembayaran = "LAK003";
        public const string namaLPenerimaanPembayaran = "Laporan Penerimaan Dan Pembayaran";

        public const string kodLBelanjawanTerkini = "LAK004";
        public const string namaLBelanjawanTerkini = "Laporan Belanjawan";

        public const string kodLDaftarPoInden = "LAK011";
        public const string namaLDaftarPoInden = "Daftar Pesanan Tempatan Dan Inden Kerja";

        public const string kodLPenyataAkaunPembekal = "LAK012";
        public const string namaLPenyataAkaunPembekal = "Penyata Akaun Pembekal";

        public const string kodLDaftarBaucerBayaran = "LAK013";
        public const string namaLDaftarBaucerBayaran = "Daftar Baucer Bayaran";

        public const string kodLWaranPeruntukan = "LAK018";
        public const string namaLWaranPeruntukan = "Laporan Waran Peruntukan";

        //
    }
}
