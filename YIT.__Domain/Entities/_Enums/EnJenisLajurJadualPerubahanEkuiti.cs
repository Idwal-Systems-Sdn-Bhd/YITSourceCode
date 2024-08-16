using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Statics;

namespace YIT.__Domain.Entities._Enums
{
    public enum EnJenisLajurJadualPerubahanEkuiti
    {
        [Display(Name = Init.CompInitial)]
        KumpWang = 0,
        [Display(Name = "Rizab")]
        Rizab = 1,
        [Display(Name = "Anak Syarikat")]
        AnakSyarikat = 2,
        [Display(Name = "Kepentingan Bukan Kawalan")]
        KepentinganBukanKawalan = 3,
    }
}
