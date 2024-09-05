using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities._Statics
{
    public static class ClaimStore
    {
        public static List<Claim> claimList = new List<Claim>()
        {
            // Sistem (SI)
            
            //

            // Jadual (JD)
            new Claim(Modules.kodJAgama,Modules.namaJAgama),
            new Claim(Modules.kodJAgama + "C",Modules.namaJAgama + " : Tambah"),
            new Claim(Modules.kodJAgama + "E",Modules.namaJAgama + " : Ubah"),
            new Claim(Modules.kodJAgama + "D",Modules.namaJAgama + " : Hapus"),
            new Claim(Modules.kodJAgama + "R",Modules.namaJAgama + " : Rollback"),

            new Claim(Modules.kodJBahagian,Modules.namaJBahagian),
            new Claim(Modules.kodJBahagian + "C",Modules.namaJBahagian + " : Tambah"),
            new Claim(Modules.kodJBahagian + "E",Modules.namaJBahagian + " : Ubah"),
            new Claim(Modules.kodJBahagian + "D",Modules.namaJBahagian + " : Hapus"),
            new Claim(Modules.kodJBahagian + "R",Modules.namaJBahagian + " : Rollback"),

            new Claim(Modules.kodJBangsa,Modules.namaJBangsa),
            new Claim(Modules.kodJBangsa + "C",Modules.namaJBangsa + " : Tambah"),
            new Claim(Modules.kodJBangsa + "E",Modules.namaJBangsa + " : Ubah"),
            new Claim(Modules.kodJBangsa + "D",Modules.namaJBangsa + " : Hapus"),
            new Claim(Modules.kodJBangsa + "R",Modules.namaJBangsa + " : Rollback"),

            new Claim(Modules.kodJBank,Modules.namaJBank),
            new Claim(Modules.kodJBank + "C",Modules.namaJBank + " : Tambah"),
            new Claim(Modules.kodJBank + "E",Modules.namaJBank + " : Ubah"),
            new Claim(Modules.kodJBank + "D",Modules.namaJBank + " : Hapus"),
            new Claim(Modules.kodJBank + "R",Modules.namaJBank + " : Rollback"),

            new Claim(Modules.kodJCaraBayar,Modules.namaJCaraBayar),
            new Claim(Modules.kodJCaraBayar + "C",Modules.namaJCaraBayar + " : Tambah"),
            new Claim(Modules.kodJCaraBayar + "E",Modules.namaJCaraBayar + " : Ubah"),
            new Claim(Modules.kodJCaraBayar + "D",Modules.namaJCaraBayar + " : Hapus"),
            new Claim(Modules.kodJCaraBayar + "R",Modules.namaJCaraBayar + " : Rollback"),

            new Claim(Modules.kodJCawangan,Modules.namaJCawangan),
            new Claim(Modules.kodJCawangan + "C",Modules.namaJCawangan + " : Tambah"),
            new Claim(Modules.kodJCawangan + "E",Modules.namaJCawangan + " : Ubah"),
            new Claim(Modules.kodJCawangan + "D",Modules.namaJCawangan + " : Hapus"),
            new Claim(Modules.kodJCawangan + "R",Modules.namaJCawangan + " : Rollback"),

            new Claim(Modules.kodJKonfigPenyata,Modules.namaJKonfigPenyata),
            new Claim(Modules.kodJKonfigPenyata + "C",Modules.namaJKonfigPenyata + " : Tambah"),
            new Claim(Modules.kodJKonfigPenyata + "E",Modules.namaJKonfigPenyata + " : Ubah"),
            new Claim(Modules.kodJKonfigPenyata + "D",Modules.namaJKonfigPenyata + " : Hapus"),
            new Claim(Modules.kodJKonfigPenyata + "R",Modules.namaJKonfigPenyata + " : Rollback"),

            new Claim(Modules.kodJKonfigPerubahanEkuiti,Modules.namaJKonfigPerubahanEkuiti),
            new Claim(Modules.kodJKonfigPerubahanEkuiti + "C",Modules.namaJKonfigPerubahanEkuiti + " : Tambah"),
            new Claim(Modules.kodJKonfigPerubahanEkuiti + "E",Modules.namaJKonfigPerubahanEkuiti + " : Ubah"),
            new Claim(Modules.kodJKonfigPerubahanEkuiti + "D",Modules.namaJKonfigPerubahanEkuiti + " : Hapus"),
            new Claim(Modules.kodJKonfigPerubahanEkuiti + "R",Modules.namaJKonfigPerubahanEkuiti + " : Rollback"),

            new Claim(Modules.kodJKW,Modules.namaJKW),
            new Claim(Modules.kodJKW + "C",Modules.namaJKW + " : Tambah"),
            new Claim(Modules.kodJKW + "E",Modules.namaJKW + " : Ubah"),
            new Claim(Modules.kodJKW + "D",Modules.namaJKW + " : Hapus"),
            new Claim(Modules.kodJKW + "R",Modules.namaJKW + " : Rollback"),

            new Claim(Modules.kodJNegeri,Modules.namaJNegeri),
            new Claim(Modules.kodJNegeri + "C",Modules.namaJNegeri + " : Tambah"),
            new Claim(Modules.kodJNegeri + "E",Modules.namaJNegeri + " : Ubah"),
            new Claim(Modules.kodJNegeri + "D",Modules.namaJNegeri + " : Hapus"),
            new Claim(Modules.kodJNegeri + "R",Modules.namaJNegeri + " : Rollback"),

            new Claim(Modules.kodJPTJ,Modules.namaJPTJ),
            new Claim(Modules.kodJPTJ + "C",Modules.namaJPTJ + " : Tambah"),
            new Claim(Modules.kodJPTJ + "E",Modules.namaJPTJ + " : Ubah"),
            new Claim(Modules.kodJPTJ + "D",Modules.namaJPTJ + " : Hapus"),
            new Claim(Modules.kodJPTJ + "R",Modules.namaJPTJ + " : Rollback"),
            //

            // Daftar (DF)
            new Claim(Modules.kodDDaftarAwam,Modules.namaDDaftarAwam),
            new Claim(Modules.kodDDaftarAwam + "C",Modules.namaDDaftarAwam + " : Tambah"),
            new Claim(Modules.kodDDaftarAwam + "E",Modules.namaDDaftarAwam + " : Ubah"),
            new Claim(Modules.kodDDaftarAwam + "D",Modules.namaDDaftarAwam + " : Hapus"),
            new Claim(Modules.kodDDaftarAwam + "R",Modules.namaDDaftarAwam + " : Rollback"),

            new Claim(Modules.kodDKonfigKelulusan,Modules.namaDKonfigKelulusan),
            new Claim(Modules.kodDKonfigKelulusan + "C",Modules.namaDKonfigKelulusan + " : Tambah"),
            new Claim(Modules.kodDKonfigKelulusan + "E",Modules.namaDKonfigKelulusan + " : Ubah"),
            new Claim(Modules.kodDKonfigKelulusan + "D",Modules.namaDKonfigKelulusan + " : Hapus"),
            new Claim(Modules.kodDKonfigKelulusan + "R",Modules.namaDKonfigKelulusan + " : Rollback"),

            new Claim(Modules.kodDPanjar,Modules.namaDPanjar),
            new Claim(Modules.kodDPanjar + "C",Modules.namaDPanjar + " : Tambah"),
            new Claim(Modules.kodDPanjar + "E",Modules.namaDPanjar + " : Ubah"),
            new Claim(Modules.kodDPanjar + "D",Modules.namaDPanjar + " : Hapus"),
            new Claim(Modules.kodDPanjar + "R",Modules.namaDPanjar + " : Rollback"),

            new Claim(Modules.kodDPekerja,Modules.namaDPekerja),
            new Claim(Modules.kodDPekerja + "C",Modules.namaDPekerja + " : Tambah"),
            new Claim(Modules.kodDPekerja + "E",Modules.namaDPekerja + " : Ubah"),
            new Claim(Modules.kodDPekerja + "D",Modules.namaDPekerja + " : Hapus"),
            new Claim(Modules.kodDPekerja + "R",Modules.namaDPekerja + " : Rollback"),

            new Claim(Modules.kodDPenerimaCekGaji,Modules.namaDPenerimaCekGaji),
            new Claim(Modules.kodDPenerimaCekGaji + "C",Modules.namaDPenerimaCekGaji + " : Tambah"),
            new Claim(Modules.kodDPenerimaCekGaji + "E",Modules.namaDPenerimaCekGaji + " : Ubah"),
            new Claim(Modules.kodDPenerimaCekGaji + "D",Modules.namaDPenerimaCekGaji + " : Hapus"),
            new Claim(Modules.kodDPenerimaCekGaji + "R",Modules.namaDPenerimaCekGaji + " : Rollback"),
            //

            // Akaun (AK)
            new Claim(Modules.kodAkAnggar,Modules.namaAkAnggar),
            new Claim(Modules.kodAkAnggar + "C",Modules.namaAkAnggar + " : Tambah"),
            new Claim(Modules.kodAkAnggar + "E",Modules.namaAkAnggar + " : Ubah"),
            new Claim(Modules.kodAkAnggar + "D",Modules.namaAkAnggar + " : Hapus"),
            new Claim(Modules.kodAkAnggar + "R",Modules.namaAkAnggar + " : Rollback"),
            new Claim(Modules.kodAkAnggar + "L",Modules.namaAkAnggar + " : Lulus"),
            new Claim(Modules.kodAkAnggar + "BL",Modules.namaAkAnggar + " : Batal Lulus"),

            new Claim(Modules.kodAbWaran,Modules.namaAbWaran),
            new Claim(Modules.kodAbWaran + "C",Modules.namaAbWaran + " : Tambah"),
            new Claim(Modules.kodAbWaran + "E",Modules.namaAbWaran + " : Ubah"),
            new Claim(Modules.kodAbWaran + "D",Modules.namaAbWaran + " : Hapus"),
            new Claim(Modules.kodAbWaran + "R",Modules.namaAbWaran + " : Rollback"),
            new Claim(Modules.kodAbWaran + "L",Modules.namaAbWaran + " : Lulus"),
            new Claim(Modules.kodAbWaran + "BL",Modules.namaAbWaran + " : Batal Lulus"),

            new Claim(Modules.kodAkBank,Modules.namaAkBank),
            new Claim(Modules.kodAkBank + "C",Modules.namaAkBank + " : Tambah"),
            new Claim(Modules.kodAkBank + "E",Modules.namaAkBank + " : Ubah"),
            new Claim(Modules.kodAkBank + "D",Modules.namaAkBank + " : Hapus"),
            new Claim(Modules.kodAkBank + "R",Modules.namaAkBank + " : Rollback"),

            new Claim(Modules.kodAkBelian,Modules.namaAkBelian),
            new Claim(Modules.kodAkBelian + "C",Modules.namaAkBelian + " : Tambah"),
            new Claim(Modules.kodAkBelian + "E",Modules.namaAkBelian + " : Ubah"),
            new Claim(Modules.kodAkBelian + "D",Modules.namaAkBelian + " : Hapus"),
            new Claim(Modules.kodAkBelian + "R",Modules.namaAkBelian + " : Rollback"),
            new Claim(Modules.kodAkBelian + "L",Modules.namaAkBelian + " : Lulus"),
            new Claim(Modules.kodAkBelian + "BL",Modules.namaAkBelian + " : Batal Lulus"),

            new Claim(Modules.kodAkCV,Modules.namaAkCV),
            new Claim(Modules.kodAkCV + "C",Modules.namaAkCV + " : Tambah"),
            new Claim(Modules.kodAkCV + "E",Modules.namaAkCV + " : Ubah"),
            new Claim(Modules.kodAkCV + "D",Modules.namaAkCV + " : Hapus"),
            new Claim(Modules.kodAkCV + "R",Modules.namaAkCV + " : Rollback"),
            new Claim(Modules.kodAkCV + "L",Modules.namaAkCV + " : Lulus"),
            new Claim(Modules.kodAkCV + "BL",Modules.namaAkCV + " : Batal Lulus"),

            new Claim(Modules.kodAkEFTMaybank2E,Modules.namaAkEFTMaybank2E),
            new Claim(Modules.kodAkEFTMaybank2E + "C",Modules.namaAkEFTMaybank2E + " : Tambah"),
            new Claim(Modules.kodAkEFTMaybank2E + "E",Modules.namaAkEFTMaybank2E + " : Ubah"),
            new Claim(Modules.kodAkEFTMaybank2E + "D",Modules.namaAkEFTMaybank2E + " : Hapus"),
            new Claim(Modules.kodAkEFTMaybank2E + "R",Modules.namaAkEFTMaybank2E + " : Rollback"),

            new Claim(Modules.kodAkInden,Modules.namaAkInden),
            new Claim(Modules.kodAkInden + "C",Modules.namaAkInden + " : Tambah"),
            new Claim(Modules.kodAkInden + "E",Modules.namaAkInden + " : Ubah"),
            new Claim(Modules.kodAkInden + "D",Modules.namaAkInden + " : Hapus"),
            new Claim(Modules.kodAkInden + "R",Modules.namaAkInden + " : Rollback"),
            new Claim(Modules.kodAkInden + "L",Modules.namaAkInden + " : Lulus"),
            new Claim(Modules.kodAkInden + "BL",Modules.namaAkInden + " : Batal Lulus"),

            new Claim(Modules.kodAkJanaanProfil,Modules.namaAkJanaanProfil),
            new Claim(Modules.kodAkJanaanProfil + "C",Modules.namaAkJanaanProfil + " : Tambah"),
            new Claim(Modules.kodAkJanaanProfil + "E",Modules.namaAkJanaanProfil + " : Ubah"),
            new Claim(Modules.kodAkJanaanProfil + "D",Modules.namaAkJanaanProfil + " : Hapus"),
            new Claim(Modules.kodAkJanaanProfil + "R",Modules.namaAkJanaanProfil + " : Rollback"),

            new Claim(Modules.kodAkJurnal,Modules.namaAkJurnal),
            new Claim(Modules.kodAkJurnal + "C",Modules.namaAkJurnal + " : Tambah"),
            new Claim(Modules.kodAkJurnal + "E",Modules.namaAkJurnal + " : Ubah"),
            new Claim(Modules.kodAkJurnal + "D",Modules.namaAkJurnal + " : Hapus"),
            new Claim(Modules.kodAkJurnal + "R",Modules.namaAkJurnal + " : Rollback"),
            new Claim(Modules.kodAkJurnal + "L",Modules.namaAkJurnal + " : Lulus"),
            new Claim(Modules.kodAkJurnal + "BL",Modules.namaAkJurnal + " : Batal Lulus"),

            new Claim(Modules.kodAkNotaMinta,Modules.namaAkNotaMinta),
            new Claim(Modules.kodAkNotaMinta + "C",Modules.namaAkNotaMinta + " : Tambah"),
            new Claim(Modules.kodAkNotaMinta + "E",Modules.namaAkNotaMinta + " : Ubah"),
            new Claim(Modules.kodAkNotaMinta + "D",Modules.namaAkNotaMinta + " : Hapus"),
            new Claim(Modules.kodAkNotaMinta + "R",Modules.namaAkNotaMinta + " : Rollback"),

            new Claim(Modules.kodAkPelarasanInden,Modules.namaAkPelarasanInden),
            new Claim(Modules.kodAkPelarasanInden + "C",Modules.namaAkPelarasanInden + " : Tambah"),
            new Claim(Modules.kodAkPelarasanInden + "E",Modules.namaAkPelarasanInden + " : Ubah"),
            new Claim(Modules.kodAkPelarasanInden + "D",Modules.namaAkPelarasanInden + " : Hapus"),
            new Claim(Modules.kodAkPelarasanInden + "R",Modules.namaAkPelarasanInden + " : Rollback"),
            new Claim(Modules.kodAkPelarasanInden + "L",Modules.namaAkPelarasanInden + " : Lulus"),
            new Claim(Modules.kodAkPelarasanInden + "BL",Modules.namaAkPelarasanInden + " : Batal Lulus"),

            new Claim(Modules.kodAkPelarasanPO,Modules.namaAkPelarasanPO),
            new Claim(Modules.kodAkPelarasanPO + "C",Modules.namaAkPelarasanPO + " : Tambah"),
            new Claim(Modules.kodAkPelarasanPO + "E",Modules.namaAkPelarasanPO + " : Ubah"),
            new Claim(Modules.kodAkPelarasanPO + "D",Modules.namaAkPelarasanPO + " : Hapus"),
            new Claim(Modules.kodAkPelarasanPO + "R",Modules.namaAkPelarasanPO + " : Rollback"),
            new Claim(Modules.kodAkPelarasanPO + "L",Modules.namaAkPelarasanPO + " : Lulus"),
            new Claim(Modules.kodAkPelarasanPO + "BL",Modules.namaAkPelarasanPO + " : Batal Lulus"),

            new Claim(Modules.kodAkPenilaianPerolehan,Modules.namaAkPenilaianPerolehan),
            new Claim(Modules.kodAkPenilaianPerolehan + "C",Modules.namaAkPenilaianPerolehan + " : Tambah"),
            new Claim(Modules.kodAkPenilaianPerolehan + "E",Modules.namaAkPenilaianPerolehan + " : Ubah"),
            new Claim(Modules.kodAkPenilaianPerolehan + "D",Modules.namaAkPenilaianPerolehan + " : Hapus"),
            new Claim(Modules.kodAkPenilaianPerolehan + "R",Modules.namaAkPenilaianPerolehan + " : Rollback"),
            new Claim(Modules.kodAkPenilaianPerolehan + "L",Modules.namaAkPenilaianPerolehan + " : Lulus"),
            new Claim(Modules.kodAkPenilaianPerolehan + "BL",Modules.namaAkPenilaianPerolehan + " : Batal Lulus"),

            new Claim(Modules.kodAkPO,Modules.namaAkPO),
            new Claim(Modules.kodAkPO + "C",Modules.namaAkPO + " : Tambah"),
            new Claim(Modules.kodAkPO + "E",Modules.namaAkPO + " : Ubah"),
            new Claim(Modules.kodAkPO + "D",Modules.namaAkPO + " : Hapus"),
            new Claim(Modules.kodAkPO + "R",Modules.namaAkPO + " : Rollback"),
            new Claim(Modules.kodAkPO + "L",Modules.namaAkPO + " : Lulus"),
            new Claim(Modules.kodAkPO + "BL",Modules.namaAkPO + " : Batal Lulus"),

            new Claim(Modules.kodAkPV,Modules.namaAkPV),
            new Claim(Modules.kodAkPV + "C",Modules.namaAkPV + " : Tambah"),
            new Claim(Modules.kodAkPV + "E",Modules.namaAkPV + " : Ubah"),
            new Claim(Modules.kodAkPV + "D",Modules.namaAkPV + " : Hapus"),
            new Claim(Modules.kodAkPV + "R",Modules.namaAkPV + " : Rollback"),
            new Claim(Modules.kodAkPV + "L",Modules.namaAkPV + " : Lulus"),
            new Claim(Modules.kodAkPV + "BL",Modules.namaAkPV + " : Batal Lulus"),

            new Claim(Modules.kodAkTerima,Modules.namaAkTerima),
            new Claim(Modules.kodAkTerima + "C",Modules.namaAkTerima + " : Tambah"),
            new Claim(Modules.kodAkTerima + "E",Modules.namaAkTerima + " : Ubah"),
            new Claim(Modules.kodAkTerima + "D",Modules.namaAkTerima + " : Hapus"),
            new Claim(Modules.kodAkTerima + "R",Modules.namaAkTerima + " : Rollback"),
            new Claim(Modules.kodAkTerima + "L",Modules.namaAkTerima + " : Lulus"),
            new Claim(Modules.kodAkTerima + "BL",Modules.namaAkTerima + " : Batal Lulus"),

            new Claim(Modules.kodAkNotaDebitKreditDiterima,Modules.namaAkNotaDebitKreditDiterima),
            new Claim(Modules.kodAkNotaDebitKreditDiterima + "C",Modules.namaAkNotaDebitKreditDiterima + " : Tambah"),
            new Claim(Modules.kodAkNotaDebitKreditDiterima + "E",Modules.namaAkNotaDebitKreditDiterima + " : Ubah"),
            new Claim(Modules.kodAkNotaDebitKreditDiterima + "D",Modules.namaAkNotaDebitKreditDiterima + " : Hapus"),
            new Claim(Modules.kodAkNotaDebitKreditDiterima + "R",Modules.namaAkNotaDebitKreditDiterima + " : Rollback"),
            new Claim(Modules.kodAkNotaDebitKreditDiterima + "L",Modules.namaAkNotaDebitKreditDiterima + " : Lulus"),
            new Claim(Modules.kodAkNotaDebitKreditDiterima + "BL",Modules.namaAkNotaDebitKreditDiterima + " : Batal Lulus"),

            new Claim(Modules.kodAkInvois,Modules.namaAkInvois),
            new Claim(Modules.kodAkInvois + "C",Modules.namaAkInvois + " : Tambah"),
            new Claim(Modules.kodAkInvois + "E",Modules.namaAkInvois + " : Ubah"),
            new Claim(Modules.kodAkInvois + "D",Modules.namaAkInvois + " : Hapus"),
            new Claim(Modules.kodAkInvois + "R",Modules.namaAkInvois + " : Rollback"),
            new Claim(Modules.kodAkInvois + "L",Modules.namaAkInvois + " : Lulus"),
            new Claim(Modules.kodAkInvois + "BL",Modules.namaAkInvois + " : Batal Lulus"),

            new Claim(Modules.kodAkNotaDebitKreditDikeluarkan,Modules.namaAkNotaDebitKreditDikeluarkan),
            new Claim(Modules.kodAkNotaDebitKreditDikeluarkan + "C",Modules.namaAkNotaDebitKreditDikeluarkan + " : Tambah"),
            new Claim(Modules.kodAkNotaDebitKreditDikeluarkan + "E",Modules.namaAkNotaDebitKreditDikeluarkan + " : Ubah"),
            new Claim(Modules.kodAkNotaDebitKreditDikeluarkan + "D",Modules.namaAkNotaDebitKreditDikeluarkan + " : Hapus"),
            new Claim(Modules.kodAkNotaDebitKreditDikeluarkan + "R",Modules.namaAkNotaDebitKreditDikeluarkan + " : Rollback"),
            new Claim(Modules.kodAkNotaDebitKreditDikeluarkan + "L",Modules.namaAkNotaDebitKreditDikeluarkan + " : Lulus"),
            new Claim(Modules.kodAkNotaDebitKreditDikeluarkan + "BL",Modules.namaAkNotaDebitKreditDikeluarkan + " : Batal Lulus"),

            new Claim(Modules.kodAkPenyataPemungut,Modules.namaAkPenyataPemungut),
            new Claim(Modules.kodAkPenyataPemungut + "C",Modules.namaAkPenyataPemungut + " : Tambah"),
            new Claim(Modules.kodAkPenyataPemungut + "E",Modules.namaAkPenyataPemungut + " : Ubah"),
            new Claim(Modules.kodAkPenyataPemungut + "D",Modules.namaAkPenyataPemungut + " : Hapus"),
            new Claim(Modules.kodAkPenyataPemungut + "R",Modules.namaAkPenyataPemungut + " : Rollback"),
            new Claim(Modules.kodAkPenyataPemungut + "L",Modules.namaAkPenyataPemungut + " : Lulus"),
            new Claim(Modules.kodAkPenyataPemungut + "BL",Modules.namaAkPenyataPemungut + " : Batal Lulus"),
            //

            // Pemprosesan (PP)
            new Claim(Modules.kodLulusAbWaran + "L",Modules.namaLulusAbWaran),
            new Claim(Modules.kodSahAbWaran + "S",Modules.namaSahAbWaran),
            new Claim(Modules.kodSemakAbWaran + "S",Modules.namaSemakAbWaran),
            new Claim(Modules.kodLulusAbWaran + "E",Modules.namaLulusAbWaran + " : Hantar Semula"),

            new Claim(Modules.kodLulusAkBelian + "L",Modules.namaLulusAkBelian),
            new Claim(Modules.kodLulusAkBelian + "E",Modules.namaLulusAkBelian + " : Hantar Semula"),

            new Claim(Modules.kodLulusAkInden + "L",Modules.namaLulusAkInden),
            new Claim(Modules.kodLulusAkInden + "E",Modules.namaLulusAkInden + " : Hantar Semula"),

            new Claim(Modules.kodLulusAkJurnal + "L",Modules.namaLulusAkJurnal),
            new Claim(Modules.kodSahAkJurnal + "S",Modules.namaSahAkJurnal),
            new Claim(Modules.kodSemakAkJurnal + "S",Modules.namaSemakAkJurnal),
            new Claim(Modules.kodLulusAkJurnal + "E",Modules.namaLulusAkJurnal + " : Hantar Semula"),

            new Claim(Modules.kodLulusAkPelarasanInden + "L",Modules.namaLulusAkPelarasanInden),
            new Claim(Modules.kodLulusAkPelarasanInden + "E",Modules.namaLulusAkPelarasanInden + " : Hantar Semula"),

            new Claim(Modules.kodLulusAkPelarasanPO + "L",Modules.namaLulusAkPelarasanPO),
            new Claim(Modules.kodLulusAkPelarasanPO + "E",Modules.namaLulusAkPelarasanPO + " : Hantar Semula"),

            new Claim(Modules.kodLulusAkPenilaianPerolehan + "L",Modules.namaLulusAkPenilaianPerolehan),
            new Claim(Modules.kodSahAkPenilaianPerolehan + "S",Modules.namaSahAkPenilaianPerolehan),
            new Claim(Modules.kodSemakAkPenilaianPerolehan + "S",Modules.namaSemakAkPenilaianPerolehan),
            new Claim(Modules.kodLulusAkPenilaianPerolehan + "E",Modules.namaLulusAkPenilaianPerolehan + " : Hantar Semula"),

            new Claim(Modules.kodLulusAkNotaMinta + "L",Modules.namaLulusAkNotaMinta),
            new Claim(Modules.kodSahAkNotaMinta + "S",Modules.namaSahAkNotaMinta),
            new Claim(Modules.kodSemakAkNotaMinta + "S",Modules.namaSemakAkNotaMinta),
            new Claim(Modules.kodLulusAkNotaMinta + "E",Modules.namaLulusAkNotaMinta + " : Hantar Semula"),

            new Claim(Modules.kodLulusAkPO + "L",Modules.namaLulusAkPO),
            new Claim(Modules.kodLulusAkPO + "E",Modules.namaLulusAkPO + " : Hantar Semula"),

            new Claim(Modules.kodLulusAkPV + "L",Modules.namaLulusAkPV),
            new Claim(Modules.kodLulusAkPV + "E",Modules.namaLulusAkPV + " : Hantar Semula"),

            new Claim(Modules.kodLulusAkNotaDebitKreditDiterima + "L",Modules.namaLulusAkNotaDebitKreditDiterima),
            new Claim(Modules.kodLulusAkNotaDebitKreditDiterima + "E",Modules.namaLulusAkNotaDebitKreditDiterima + " : Hantar Semula"),

            new Claim(Modules.kodLulusAkInvois + "L",Modules.namaLulusAkInvois),
            new Claim(Modules.kodLulusAkInvois + "E",Modules.namaLulusAkInvois + " : Hantar Semula"),

            new Claim(Modules.kodLulusAkNotaDebitKreditDikeluarkan + "L",Modules.namaLulusAkNotaDebitKreditDikeluarkan),
            new Claim(Modules.kodLulusAkNotaDebitKreditDikeluarkan + "E",Modules.namaLulusAkNotaDebitKreditDikeluarkan + " : Hantar Semula"),
            //

            //

            // Laporan Akaun (PAK)
            //

        };
    }
}
