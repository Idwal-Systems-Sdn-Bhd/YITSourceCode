using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Bases;
using YIT.__Domain.Entities.Models._01Jadual;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities.Models._02Daftar
{
    public class DDaftarAwam : GenericFields
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Kod Syarikat Diperlukan")]
        [DisplayName("Kod Syarikat")]
        public string? KodSyarikat { get; set; }
        
        [Required(ErrorMessage = "Nama Syarikat Diperlukan")]
        [DisplayName("Nama Syarikat")]
        public string? NamaSyarikat { get; set; }
        [DisplayName("Negeri")]
        public int? JNegeriId { get; set; }
        public JNegeri? JNegeri { get; set; }
        [DisplayName("Bank")]
        public int? JBankId { get; set; }
        public JBank? JBank { get; set; }
        [DisplayName("No Pendaftaran")] // no IC // No Pendaftaran
        public string? NoPendaftaran { get; set; }
        [DisplayName("Alamat")]
        public string? Alamat1 { get; set; }
        public string? Alamat2 { get; set; }
        public string? Alamat3 { get; set; }
        public string? Poskod { get; set; }
        public string? Bandar { get; set; }
        [DisplayName("No Telefon")]
        public string? Telefon1 { get; set; }
        public string? Emel { get; set; }
        [DisplayName("No Akaun Bank")]
        public string? NoAkaunBank { get; set; }
        [DisplayName("Kategori")]
        public EnKategoriDaftarAwam EnKategoriDaftarAwam { get; set; }

    }
}
