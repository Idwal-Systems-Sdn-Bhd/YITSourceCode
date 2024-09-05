using YIT.__Domain.Entities.Bases;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT.__Domain.Entities.Models._01Jadual
{
    public class JPTJ : GenericFields
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(3)]
        public string? Kod { get; set; }
        [Required]
        [MaxLength(100)]
        public string? Perihal { get; set; }
        public ICollection<JKWPTJBahagian> JKWPTJBahagian { get; set; } = new List<JKWPTJBahagian>();
        public ICollection<AkAkaun> AkAkaun { get; set; } = new List<AkAkaun>();
        public ICollection<AbBukuVot> AbBukuVot { get; set; } = new List<AbBukuVot>();
        public ICollection<AkPenyataPemungut>? AkPenyataPemungut { get; set; }

    }
}
