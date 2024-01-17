using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Bases;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT.__Domain.Entities.Models._02Daftar
{
    public class DPenerimaCekGajiViewModel : GenericFields
    {
        public IEnumerable<DPenerimaCekGaji>? DPenerimaCekGaji { get; set; }
        public IEnumerable<AkJanaanProfil>? AkJanaanProfil { get; set; }
    }
}
