using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._02Daftar;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public  class AkJanaanProfilPenerima
    {
        public int Id { get; set; }
        public int? Bil { get; set; }
        public int AkJanaanProfilId { get; set; }
        public AkJanaanProfil? AkJanaanProfil { get; set; }
        public EnKategoriDaftarAwam EnKategoriDaftarAwam { get; set; }
        public int? DPenerimaZakatId { get; set; }
        public DPenerimaZakat? DPenerimaZakat { get; set; }
        public int? DDaftarAwamId { get; set; }
        public DDaftarAwam? DDaftarAwam { get; set; }
        public int? DPekerjaId { get; set; }
        public DPekerja? DPekerja { get; set; }
        public string? NoPendaftaranPenerima { get; set; }
        public string? NamaPenerima { get; set; }
        public string? NoPendaftaranPemohon { get; set; }
        public string? Catatan { get; set; }
        public int JCaraBayarId { get; set; }
        public JCaraBayar? JCaraBayar { get; set; }
        public int? JBankId { get; set; }
        public JBank? JBank { get; set; }
        public string? NoAkaunBank { get; set; }
        public string? Alamat1 { get; set; }
        public string? Alamat2 { get; set; }
        public string? Alamat3 { get; set; }
        public string? Emel { get; set; }
        public string? KodM2E { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amaun { get; set; }
        public string? NoRujukanMohon { get; set; }
        public int? AkRekupId { get; set; }
        public AkRekup? AkRekup { get; set; }
        [Display(Name = "Jenis Id")]
        public EnJenisId EnJenisId { get; set; }
        public ICollection<AkPVPenerima>? AkPVPenerima { get; set; }
    }
}
