using System.ComponentModel.DataAnnotations;

namespace YIT.__Domain.Entities._Enums
{
    public enum EnJenisTerimaan
    {
        [Display(Name = "Am")]
        Am = 0,
        [Display(Name = "Invois")]
        Invois = 1,
        [Display(Name = "Gaji")]
        Gaji = 2
    }
}