using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities._Enums
{
    public enum EnInitNoRujukan
    {
        [Display(Name = "RR")]
        RR = 1, // terimaan
        [Display(Name = "DI")]
        DI = 2, // invois dikeluarkan
        [Display(Name = "ND")]
        ND = 3, // nota debit 
        [Display(Name = "NK")]
        NK = 4, // nota kredit 
        [Display(Name = "PM")]
        PM = 5, // Permohonan Perolehan // PM/2023/00001
        [Display(Name = "PN")]
        PN = 6, // Penilaian Perolehan // PN/2023/00001
        [Display(Name = "NM")]
        NM = 7, // nota minta // NM/2023/00001
        [Display(Name = "PO")]
        PO = 8, // Pesanan tempatan
        [Display(Name = "IK")]
        IK = 9, // Inden kerja
        [Display(Name = "PX")]
        PX = 10, // Pelarasan Pesanan tempatan
        [Display(Name = "IX")]
        IX = 11, // Pelarasan Inden Kerja
        [Display(Name = "IN")]
        IN = 12, // Invois pembekal
        [Display(Name = "SW")]
        SW = 13, // invois dikeluarkan (sewaan)
        [Display(Name = "SP")]
        SP = 14, // invois dikeluarkan (pinjaman)
        [Display(Name = "JU")]
        JU = 15, // pelarasan baucer jurnal
        [Display(Name = "WR")]
        WR = 16, // waran
        [Display(Name = "CV")]
        CV = 17, // baucer runcit
        [Display(Name = "RK")]
        RK = 18, // rekup
        [Display(Name = "GJ")]
        GJ = 19, // janaan gaji
        [Display(Name = "EFT")]
        EF = 20, // janaan EFT
        [Display(Name = "PV")]
        PV = 21, // baucer pembayaran

    }
}
