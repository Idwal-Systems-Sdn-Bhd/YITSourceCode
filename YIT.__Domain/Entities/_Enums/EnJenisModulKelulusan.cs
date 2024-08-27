using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities._Enums
{
    public enum EnJenisModulKelulusan
    {
        [Display(Name = "Waran")]
        Waran = 1,
        [Display(Name = "Permohonan Perolehan")]
        Permohonan = 2,
        [Display(Name = "Penilaian Perolehan")]
        Penilaian = 3,
        [Display(Name = "Nota Minta")]
        NotaMinta = 4,
        [Display(Name = "Pesanan Tempatan")]
        PO = 5,
        [Display(Name = "Inden Kerja")]
        Inden = 6,
        [Display(Name = "Pel. Pesanan Tempatan")]
        PelarasanPO = 7,
        [Display(Name = "Pel. Inden Kerja")]
        PelarasanInden = 8,
        [Display(Name = "Invois Pembekal")]
        Belian = 9,
        [Display(Name = "Nota Debit/Kredit Pembekal")]
        NotaDebitKreditDiterima = 10,
        [Display(Name = "Baucer Bayaran")]
        PV = 11,
        [Display(Name = "Jurnal")]
        Jurnal = 12,
        [Display(Name = "Nota Debit/Kredit Dikeluarkan")]
        NotaDebitKreditDikeluarkan = 13,

    }
}
