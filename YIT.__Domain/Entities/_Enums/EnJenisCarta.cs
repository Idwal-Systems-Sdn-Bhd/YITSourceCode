using System.ComponentModel.DataAnnotations;

namespace YIT.__Domain.Entities._Enums
{
    public enum EnJenisCarta
    {
        [Display(Name = "Liabiliti")]
        Liabiliti = 1,
        [Display(Name = "Ekuiti")]
        Ekuiti = 2,
        [Display(Name = "Belanja")]
        Belanja = 3,
        [Display(Name = "Aset")]
        Aset = 4,
        [Display(Name = "Hasil")]
        Hasil = 5
    }
}