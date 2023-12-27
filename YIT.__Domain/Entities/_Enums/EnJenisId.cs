using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities._Enums
{
    public enum EnJenisId
    {
        [Display(Name = "Tiada")]
        None = 0,
        [Display(Name = "KP Baru")]
        KPBaru = 1,
        [Display(Name = "KP Lama")]
        KPLama = 2,
        [Display(Name = "Passport")]
        Passport = 3,
        [Display(Name = "Kod Pembekal")]
        KodPembekal = 4,
        [Display(Name = "No Tentera")]
        NoTentera = 4,
    }
}
