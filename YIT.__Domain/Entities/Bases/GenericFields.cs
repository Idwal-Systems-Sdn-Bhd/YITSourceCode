using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities.Bases
{
    public class GenericFields : IGenericFields
    {
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
    }
}
