using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities._Enums
{
    public enum EnKategoriAhli
    {
        [Display(Name = "Ahli Baitulmal")]
        Baitulmal = 1,
        [Display(Name = "Ahli Majlis")]
        Majlis = 2,
        [Display(Name = "AJK")]
        AJK = 3,
        [Display(Name = "Tiada")]
        Tiada = 0
    }
}
