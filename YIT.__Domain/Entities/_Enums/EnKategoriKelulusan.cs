using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities._Enums
{
    public enum EnKategoriKelulusan
    {
        [Display(Name = "Pengesah")]
        Pengesah = 1,
        [Display(Name = "Penyemak")]
        Penyemak = 2,
        [Display(Name = "Pelulus")]
        Pelulus = 3,
    }
}
