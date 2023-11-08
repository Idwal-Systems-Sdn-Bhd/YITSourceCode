using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities._Enums
{
    public enum EnStatusKahwin
    {
        [Display(Name = "Bujang")]
        Bujang = 1,
        [Display(Name = "Kahwin")]
        Kahwin = 2,
        [Display(Name = "Duda")]
        Duda = 3,
        [Display(Name = "Janda")]
        Janda = 4
    }
}
