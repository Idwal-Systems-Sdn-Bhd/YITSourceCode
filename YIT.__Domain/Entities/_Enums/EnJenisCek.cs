using System.ComponentModel.DataAnnotations;

namespace YIT.__Domain.Entities._Enums
{
    public enum EnJenisCek
    {
        [Display(Name = "Cawangan")]
        Cawangan = 1,
        [Display(Name = "Tempatan")]
        Tempatan = 2,
        [Display(Name = "Luar")]
        Luar = 3,
        [Display(Name = "Lain-lain")]
        LainLain = 0
    }
}