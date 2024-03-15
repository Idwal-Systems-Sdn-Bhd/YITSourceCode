using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Bases;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._02Daftar;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkCV : GenericTransactionFields
    {
        public int Id { get; set; }
        [Display(Name = "No Rujukan")]
        public string? NoRujukan { get; set; }
        public string? Tahun { get; set; }
        public DateTime Tarikh { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Jumlah { get; set; }
        [Display(Name = "Panjar")]
        public int DPanjarId { get; set; }
        public DPanjar? DPanjar { get; set; }
        [Display(Name = "Kategori Penerima")]
        public EnKategoriDaftarAwam EnKategoriPenerima { get; set; } // jenis: pekerja / lain-lain
        [Display(Name = "Anggota")]
        public int? DPekerjaId { get; set; }
        public DPekerja? DPekerja { get; set; }
        [Display(Name = "No Pendaftaran / IC")]
        public string? NoPendaftaranPenerima { get; set; }
        [Display(Name = "Nama Penerima")]
        public string? NamaPenerima { get; set; }
        public string? Catatan { get; set; }
        [Display(Name = "Alamat")]
        public string? Alamat1 { get; set; }
        public string? Alamat2 { get; set; }
        public string? Alamat3 { get; set; }
        public ICollection<AkCVObjek>? AkCVObjek { get; set; }
    }
}