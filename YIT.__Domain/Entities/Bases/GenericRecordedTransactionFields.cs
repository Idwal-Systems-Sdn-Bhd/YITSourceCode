using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._02Daftar;

namespace YIT.__Domain.Entities.Bases
{
    public class GenericRecordedTransactionFields : IGenericTransactionFields
    {
        // recorded transaction
        [DisplayName("No Pendaftaran / No. KP / No Tentera")] // no IC // No Pendaftaran
        public string? NoPendaftaran { get; set; }
        [Required(ErrorMessage = "Nama Diperlukan")]
        [DisplayName("Nama")]
        public string? Nama { get; set; }
        [DisplayName("Alamat")]
        public string? Alamat1 { get; set; }
        public string? Alamat2 { get; set; }
        public string? Alamat3 { get; set; }
        [EmailAddress(ErrorMessage = "Emel tidak sah"), MaxLength(100)]
        public string? Emel { get; set; }
        [DisplayName("No Akaun Bank")]
        [RegularExpression(@"^[\d+]*$", ErrorMessage = "Nombor sahaja dibenarkan")]
        public string? NoAkaunBank { get; set; }
        //
        // log
        public int? DPekerjaMasukId { get; set; }
        [ValidateNever]
        public string UserId { get; set; } = string.Empty;
        [DisplayName("Tarikh Masuk")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime TarMasuk { get; set; }
        public int? DPekerjaKemaskiniId { get; set; }
        [ValidateNever]
        public string UserIdKemaskini { get; set; } = string.Empty;
        [DisplayName("Tarikh Kemaskini")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? TarKemaskini { get; set; } = DateTime.Now;
        //log end
        public int FlHapus { get; set; }
        public DateTime? TarHapus { get; set; }
        public string? SebabHapus { get; set; }

        public int FlBatal { get; set; }
        public DateTime? TarBatal { get; set; }
        public string? SebabBatal { get; set; }

        public int? DPengesahId { get; set; }
        public DKonfigKelulusan? DPengesah { get; set; }
        public DateTime? TarikhSah { get; set; }
        public int? DPenyemakId { get; set; }
        public DKonfigKelulusan? DPenyemak { get; set; }
        public DateTime? TarikhSemak { get; set; }
        public int? DPelulusId { get; set; }
        public DKonfigKelulusan? DPelulus { get; set; }
        public DateTime? TarikhLulus { get; set; }

        public int? DPekerjaPostingId { get; set; }
        public DPekerja? DPekerjaPosting { get; set; }
        public int FlPosting { get; set; }
        public DateTime? TarikhPosting { get; set; }

        [DisplayName("Status")]
        public EnStatusBorang EnStatusBorang { get; set; }
        public string? Tindakan { get; set; }
    }
}
