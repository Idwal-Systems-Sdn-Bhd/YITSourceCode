using System.ComponentModel.DataAnnotations;

namespace YIT.__Domain.Entities._Enums
{
    public enum EnKaedahPerolehan
    {
        [Display(Name = "")]
        None = 0,
        [Display(Name = "Pembelian Terus")]
        PembelianTerus = 1,
        [Display(Name = "Sebutharga")]
        Sebutharga = 2,
        [Display(Name = "Tender")]
        Tender = 3,
        [Display(Name = "Perjanjian")]
        Perjanjian = 4,
        [Display(Name = "Rundingan Terus")]
        RundinganTerus = 5

    }
}