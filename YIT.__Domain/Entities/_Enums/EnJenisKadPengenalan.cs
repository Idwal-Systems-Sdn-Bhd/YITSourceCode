using System.ComponentModel.DataAnnotations;

namespace YIT.__Domain.Entities._Enums
{
    public enum EnJenisKadPengenalan
    {
        [Display(Name = "")]
        None = 0,

        [Display(Name = "No KP Lama")]
        KPLama = 0,

        [Display(Name = "No KP Baru")]
        KPBaru = 0,

        [Display(Name = "No Passport")]
        Passport = 0,
    }
}