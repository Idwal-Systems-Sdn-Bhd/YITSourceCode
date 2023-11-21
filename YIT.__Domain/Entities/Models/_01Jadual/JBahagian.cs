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
        public ICollection<JKWPTJBahagian>? JKWPTJBahagian { get; set; }
        public ICollection<AkAkaun>? AkAkaun { get; set; }

    }
}
