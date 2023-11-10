using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities._Enums
{
    public enum EnInitNoRujukan
    {
        RR = 1, // terimaan
        DI = 2, // invois dikeluarkan
        ND = 3, // nota debit 
        NK = 4, // nota kredit 
        PM = 5, // Permohonan Perolehan // PM/2023/00001
        PN = 6, // Penilaian Perolehan // PN/2023/00001
        NM = 7, // nota minta // NM/2023/00001
        PO = 8, // Pesanan tempatan
        IK = 9, // Inden kerja
        PX = 10, // Pelarasan Pesanan tempatan
        IX = 11, // Pelarasan Inden Kerja
        IN = 12, // Invois pembekal
        SW = 13, // invois dikeluarkan (sewaan)
        SP = 14, // invois dikeluarkan (pinjaman)
        JU = 15, // pelarasan baucer jurnal
        
    }
}
