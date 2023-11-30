using System.ComponentModel.DataAnnotations;

namespace YIT.__Domain.Entities._Enums
{
    public enum EnStatusBorang
    {
        [Display(Name = "")]
        None = 0,
        [Display(Name = "Sah")]
        Sah = 1,
        [Display(Name = "Semak")]
        Semak = 2,
        [Display(Name = "Lulus")]
        Lulus = 3,
        [Display(Name = "Kemaskini")]
        Kemaskini = 4,
        [Display(Name = "Batal")]
        Batal = 5,
        [Display(Name = "Semua")]
        Semua = 99
    }
}