using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Bases;

namespace YIT.__Domain.Entities.Models._01Jadual
{
    public class JKonfigPerubahanEkuiti : GenericFields
    {
        public int Id { get; set; }
        [Display(Name = "Kump. Wang")]
        public int JKWId { get; set; }
        public JKW? JKW { get; set; }
        public string? Tahun { get; set; }
        [Display(Name = "Baki pada 1 Januari RM")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal BakiAwal { get; set; }
        [Display(Name = "Pelarasan RM")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Pelarasan { get; set; }
        [Display(Name = "Lebihan Bersih RM")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal LebihanBersih { get; set; }
        [Display(Name = "Baki pada 31 Disember RM")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal BakiAkhir { get; set; }
    }
}
