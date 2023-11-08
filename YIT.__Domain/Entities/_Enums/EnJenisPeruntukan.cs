using System.ComponentModel.DataAnnotations;

namespace YIT.__Domain.Entities._Enums
{
    public enum EnJenisPeruntukan
    {
        [Display(Name = "Peruntukan Asal")]
        PeruntukanAsal = 1,
        [Display(Name = "Peruntukan Tambahan")]
        PeruntukanTambahan = 2,
        [Display(Name = "Viremen")]
        Viremen = 3
    }
}