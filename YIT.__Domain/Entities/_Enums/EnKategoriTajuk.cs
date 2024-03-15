using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities._Enums
{
    public enum EnKategoriTajuk
    {
        [Display(Name = "Tajuk Utama")]
        TajukUtama = 0,
        [Display(Name = "Tajuk Kecil")]
        TajukKecil = 1,
        [Display(Name = "Perihalan")]
        Perihalan = 2
    }
}
