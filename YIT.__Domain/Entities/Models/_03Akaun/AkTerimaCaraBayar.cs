using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._01Jadual;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities.Models._03Akaun
{
    public class AkTerimaCaraBayar
    {
        public int Id { get; set; }
        public int AkTerimaId { get; set; }
        public AkTerima? AkTerima { get; set; }
        public int JCaraBayarId { get; set; }
        public JCaraBayar? JCaraBayar { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amaun { get; set; }
        [DisplayName("No Cek / Mak. Kredit")]
        public string? NoCekMK { get; set; } // no cek / no maklumat kredit
        public EnJenisCek EnJenisCek { get; set; }
        public string? KodBankCek { get; set; }
        public string? TempatCek { get; set; }
        public string? NoSlip { get; set; } 
        public DateTime? TarikhSlip { get; set; }
        public DateTime? ReconTarikhTunai { get; set; }
        public bool ReconIsAutoMatch { get; set; }
    }
}
