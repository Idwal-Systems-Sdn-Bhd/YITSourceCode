using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities._Enums
{
    public enum EnJenisModulProfil
    {
        [Display(Name = "Tiada")]
        Tiada = 0,
        [Display(Name = "Gaji")]
        Gaji = 1,
    }
}
