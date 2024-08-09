using YIT.__Domain.Entities.Bases;
using YIT.__Domain.Entities.Models._03Akaun;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities.Models._01Jadual
{
    public class JKW : GenericFields
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Kod Diperlukan")]
        public string? Kod { get; set; }
        public string? Perihal { get; set; }
        public ICollection<JKWPTJBahagian> JKWPTJBahagian { get; set; } = new List<JKWPTJBahagian>();
        public ICollection<AkAkaun> AkAkaun { get; set; } = new List<AkAkaun>();
        public ICollection<AkTerima> AkTerima { get; set; } = new List<AkTerima>();
        public ICollection<AbBukuVot> AbBukuVot { get; set; } = new List<AbBukuVot>();
        public ICollection<AbWaran> AbWaran { get; set; } = new List<AbWaran>();
        public ICollection<AkPO> AkPO { get; set; } = new List<AkPO>();
        public ICollection<AkInden> AkInden { get; set; } = new List<AkInden>();
        public ICollection<AkNotaDebitKreditDiterima> AkNotaDebitKreditDiterima { get; set; } = new List<AkNotaDebitKreditDiterima>();

        public virtual ICollection<AkInvois>? AkInvois { get; set; }
    }
}
