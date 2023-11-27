using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Bases;

namespace YIT.__Domain.Entities.Models._01Jadual
{
    public class JCukai : GenericFields
    {
        public int Id { get; set; }
        public string? Kod { get; set; }
        public string? Perihal { get; set; }
        [DisplayName("Kategori Cukai")]
        public EnKategoriCukai EnKategoriCukai { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Peratus { get; set; }
        [DisplayName("Kod Item")]
        public string? KodItem { get; set; }
    }
}
