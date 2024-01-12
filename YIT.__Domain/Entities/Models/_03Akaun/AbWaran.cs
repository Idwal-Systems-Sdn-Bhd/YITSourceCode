using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Bases;
using YIT.__Domain.Entities.Models._02Daftar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._01Jadual;
using System.ComponentModel.DataAnnotations;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AbWaran : GenericTransactionFields
    {
        public int Id { get; set; }
        [MaxLength(4)]
        [RegularExpression(@"^[\d+]*$", ErrorMessage = "Nombor sahaja dibenarkan")]
        public string? Tahun { get; set; }
        [DisplayName("No Rujukan")]
        public string? NoRujukan { get; set; }
        public DateTime Tarikh { get; set; }
        [DisplayName("Kumpulan Wang")]
        public int JKWId { get; set; }
        public JKW? JKW { get; set; }
        [DisplayName("Jenis Peruntukan")]
        public EnJenisPeruntukan EnJenisPeruntukan { get; set; }
        [DisplayName("Jenis Pindahan")]
        public int FlJenisPindahan { get; set; } // 0 = dalam bahagian; 1 = antara bahagian
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Jumlah { get; set; }
        public string? Sebab { get; set; }
        public string? NoRujukanLama { get; set; } // dummy
        public ICollection<AbWaranObjek>? AbWaranObjek { get; set; }

    }
}
