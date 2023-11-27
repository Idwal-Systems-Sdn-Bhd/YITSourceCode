using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities._Enums
{
    public enum EnKategoriCukai
    {
        [Display(Name = "")]
        None = 0,
        [Display(Name = "Supply")]
        Supply = 1,
        [Display(Name = "Purchase")]
        Purchase = 2
    }
}
