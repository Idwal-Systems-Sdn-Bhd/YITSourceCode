using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Bases;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._02Daftar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkTerima : GenericFields
    {
        public int Id { get; set; }
        [DisplayName("No Rujukan")]
        public string? NoRujukan { get; set; }
        public DateTime Tarikh { get; set; }
        public int FlPosting { get; set; }
        public int? DPekerjaPostingId { get; set; }
        public DPekerja? DPekerjaPosting { get; set; }
        public DateTime? TarikhPosting { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Jumlah { get; set; }
        [DisplayName("Kod Pembayar")]
        public string? KodPembayar { get; set; }
        public string? Nama { get; set; }
        [DisplayName("Alamat")]
        public string? Alamat1 { get; set; }
        public string? Alamat2 { get; set; }
        public string? Alamat3 { get; set; }
        public string? Poskod { get; set; }
        public string? Bandar { get; set; }
        [DisplayName("No Telefon")]
        public string? Telefon1 { get; set; }
        public string? Emel { get; set; }
        public string? Perihal { get; set; }
        [DisplayName("Jenis Terimaan")]
        public EnJenisTerimaan? EnJenisTerimaan { get; set; }
        [DisplayName("Kategori")]
        public EnKategoriDaftarAwam EnKategoriDaftarAwam { get; set; }
        public int FlCetak { get; set; }
        [DisplayName("Kump. Wang")]
        public int JKWId { get; set; }
        public JKW? JKW { get; set; }

        [DisplayName("Neger")]
        public int? JNegeriId { get; set; }
        public JNegeri? JNegeri { get; set; }
        [DisplayName("Bank")]
        public int? AkBankId { get; set; }
        public AkBank? AkBank { get; set; }
        [DisplayName("Daftar Awam")]
        public int? DDaftarAwamId { get; set; }
        public DDaftarAwam? DDaftarAwam { get; set; }
        public string? NoRujukanLama { get; set; } // dummy
        [DisplayName("Cawangan")]
        public int JCawanganId { get; set; }
        public JCawangan? JCawangan { get; set; }

        public ICollection<AkTerimaObjek>? AkTerimaObjek { get; set; }
        public ICollection<AkTerimaCaraBayar>? AkTerimaCaraBayar { get; set; }
    }
}
