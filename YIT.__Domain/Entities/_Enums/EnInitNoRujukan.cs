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
        NM = 5, // nota minta
        PO = 6, // Pesanan tempatan
        IK = 7, // Inden kerja
        PX = 8, // Pelarasan Pesanan tempatan
        IX = 9, // Pelarasan Inden Kerja
        IN = 10, // Invois pembekal
        SW = 11, // invois dikeluarkan (sewaan)
        SP = 12, // invois dikeluarkan (pinjaman)
        JU = 13, // pelarasan baucer jurnal
        
    }
}
