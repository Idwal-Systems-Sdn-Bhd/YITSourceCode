using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities._Enums
{
    public enum EnJenisModul
    {
        [Display(Name = "Perolehan")]
        Perolehan = 1,
        [Display(Name = "Anggaran Bajet")]
        Bajet = 2,
        [Display(Name = "Baucer")]
        Baucer = 3,
        [Display(Name = "Tanggungan")]
        Tanggungan = 4,
        [Display(Name = "Jurnal")]
        Jurnal = 5

    }
}
