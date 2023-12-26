using System.ComponentModel.DataAnnotations;

namespace YIT.__Domain.Entities._Enums
{
    public enum EnJenisBayaran
    {
        [Display(Name = "Am")]
        Am = 0,
        [Display(Name = "Invois")]
        Invois = 1,
        [Display(Name = "Gaji")]
        Gaji = 2,
        [Display(Name = "Panjar")]
        Panjar = 3,
        [Display(Name = "Janaan Profil")]
        JanaanProfil = 4
    }
}