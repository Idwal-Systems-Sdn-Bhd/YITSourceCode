using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace YIT.__Domain.Entities._Enums
{
    public enum EnJenisPerolehan
    {
        [Display(Name = "")]
        None = 0,
        [Display(Name = "Bekalan")]
        Bekalan = 1,
        [Display(Name = "Perkhidmatan")]
        Perkhidmatan = 2,
        [Display(Name = "Kerja")]
        Kerja = 3,
        [Display(Name = "Semua")]
        Semua = 99,
    }
}