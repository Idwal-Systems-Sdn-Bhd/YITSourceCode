using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace YIT._DataAccess.Services.Math
{
    public class RunningNumberFormatter
    {
        public static string GenerateRunningNumber(string prefix,string latestNoRujukan,string digits)
        {
            int x = 1;
            string noRujukan = prefix + digits;

            if (string.IsNullOrEmpty(latestNoRujukan))
            {
                noRujukan = string.Format("{0:" + prefix + digits + "}", x);
            }
            else
            {
                x = int.Parse(latestNoRujukan.Substring(0));
                x++;
                noRujukan = string.Format("{0:" + prefix.Substring(0).ToUpper() + digits + "}", x);
            }
            return noRujukan;
        }
    }
}
