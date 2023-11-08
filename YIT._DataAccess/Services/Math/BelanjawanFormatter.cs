using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Services.Math
{
    public class BelanjawanFormatter
    {
        public static string ConvertToKW(string? kodKW)
        {
            return kodKW + "00" ?? "";
        }

        public static string ConvertToPTJ(string? kodKW,string? kodPTJ)
        {
            return kodKW + kodPTJ ?? "";
        }

        public static string ConvertToBahagian(string? kodKW, string? kodPTJ, string? kodBahagian)
        {
            return kodKW + kodPTJ + kodBahagian ?? "";
        }
    }
}
