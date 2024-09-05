using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Bases;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._02Daftar;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkTerimaTunggal : GenericTransactionFields
    {
        public int Id { get; set; }
        public string? Tahun { get; set; }
        [DisplayName("No Rujukan")]
        public string? NoRujukan { get; set; }
        public DateTime Tarikh { get; set; }
        [DisplayName("Cawangan")]
        public int? JCawanganId { get; set; }
        public JCawangan? JCawangan { get; set; }
        [DisplayName("Jenis Terimaan")]
        public EnJenisTerimaan EnJenisTerimaan { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Jumlah { get; set; }
        [DisplayName("PV")]
        public int? AkPVId { get; set; }
        public AkPV? AkPV { get; set; }
        [DisplayName("Bank")]
        public int? AkBankId { get; set; }
        public AkBank? AkBank { get; set; }
        [DisplayName("Kump. Wang")]
        public int JKWId { get; set; }
        public JKW? JKW { get; set; }
        public string? Ringkasan { get; set; }
        [DisplayName("Kategori")]
        public EnKategoriDaftarAwam EnKategoriDaftarAwam { get; set; }
        [DisplayName("Daftar Awam")]
        public int? DDaftarAwamId { get; set; }
        public DDaftarAwam? DDaftarAwam { get; set; }
        [DisplayName("Anggota")]
        public int? DPekerjaId { get; set; }
        public DPekerja? DPekerja { get; set; }
        [DisplayName("No Pendaftaran")]
        public string? NoPendaftaranPembayar { get; set; }
        public string? Nama { get; set; }
        [DisplayName("Alamat")]
        public string? Alamat1 { get; set; }
        public string? Alamat2 { get; set; }
        public string? Alamat3 { get; set; }
        public string? Emel { get; set; }
        [DisplayName("No Telefon")]
        public string? Telefon1 { get; set; }

        public int FlCetak { get; set; }

        [DisplayName("Negeri")]
        public int? JNegeriId { get; set; }
        public JNegeri? JNegeri { get; set; }
        [DisplayName("Cara Bayar")]
        public int JCaraBayarId { get; set; }
        public JCaraBayar? JCaraBayar { get; set; }
        [DisplayName("No Cek / Mak. Kredit")]
        public string? NoCekMK { get; set; } // no cek / no maklumat kredit
        [DisplayName("Jenis Cek")]
        public EnJenisCek EnJenisCek { get; set; }
        [DisplayName("Kod Bank Cek")]
        public string? KodBankCek { get; set; }
        [DisplayName("Tempat Cek")]
        public string? TempatCek { get; set; }
        [DisplayName("No Slip")]
        public string? NoSlip { get; set; }
        [DisplayName("Tarikh Slip")]
        public DateTime? TarikhSlip { get; set; }
        public string? NoRujukanLama { get; set; } // dummy

        public ICollection<AkTerimaTunggalObjek>? AkTerimaTunggalObjek { get; set; }
        public ICollection<AkTerimaTunggalInvois>? AkTerimaTunggalInvois { get; set; }
        public ICollection<AkPenyataPemungutObjek>? AkPenyataPemungutObjek { get; set; }
    }
}
