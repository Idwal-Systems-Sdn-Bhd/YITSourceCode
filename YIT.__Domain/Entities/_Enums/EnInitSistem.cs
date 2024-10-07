using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities._Enums
{
    public enum EnInitSistem
    {
        // Global
        SI = 0, // sistem
        JD = 1,  // jadual
        DF = 2,  // daftar
        //

        AK = 3, // akaun
        SU = 4, // sumber
        SW = 5, // sewaan
        AS = 6, // aset
        AG = 7, // agihan
        ST = 8, // stor / stok
        PJ = 9, // pinjaman / PJD
        PB = 10, // pelaburan
        ED = 11, // EDMS
        SY = 12, // syarak
        KP = 13, // koperasi (caruman)
        GD = 14, // ar-Rahnu (gadaian)
        AB = 15, // belanjawan
        LP = 99 // laporan

    }
}
