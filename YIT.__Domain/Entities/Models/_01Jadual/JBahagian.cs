using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using YIT.__Domain.Entities.Bases;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT.__Domain.Entities.Models._02Daftar;

namespace YIT.__Domain.Entities.Models._01Jadual
{
    public class JBahagian : GenericFields
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Kod Diperlukan")]
        public string? Kod { get; set; }
        [Required(ErrorMessage = "Perihal Diperlukan")]
        public string? Perihal { get; set; } 
        [ValidateNever]
        public JPTJ? JPTJ { get; set; }
        [DisplayName("PTJ")]
        public int? JPTJId { get; set; }
        public ICollection<AkAkaun>? AkAkaun { get; set; }
        public ICollection<AkTerimaObjek>? AkTerimaObjek { get; set; }
        public ICollection<AbWaranObjek>? AbWaranObjek { get; set; }
        public ICollection<AkPenilaianPerolehanObjek>? AkPenilaianPerolehanObjek { get; set; }
        public ICollection<AkNotaMintaObjek>? AkNotaMintaObjek { get; set; }
        public ICollection<AkIndenObjek>? AkIndenObjek { get; set; }
        public ICollection<AkPOObjek>? AkPOObjek { get; set; }
    }
}
