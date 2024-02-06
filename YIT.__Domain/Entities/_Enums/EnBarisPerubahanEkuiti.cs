using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities._Enums
{
    public enum EnBarisPerubahanEkuiti
    {
        [Display(Name = "1. Baki Pada 1 Januari")]
        BakiAwal = 0,
        [Display(Name = "2. Pelarasan Tahun")]
        Pelarasan = 1,
        [Display(Name = "3. Lebihan Tahun")]
        Lebihan = 2
    }
}
