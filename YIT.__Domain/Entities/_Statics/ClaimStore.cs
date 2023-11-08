using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace YIT.__Domain.Entities._Statics
{
    public static class ClaimStore
    {
        public static List<Claim> claimList = new List<Claim>()
        {
            // Jadual (JD)
            new Claim("JD000","Jadual"),
            //

            // Daftar (DF)
            new Claim("DF000","Daftar"),
            //

            // Penerimaan (PR)
            new Claim("PR000","Penerimaan"),
            //

            // Laporan (LP)
            new Claim("LP000","Laporan")
            //

        };
    }
}
