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
        public static string GenerateRunningNumber(string prefix,string latestNoRujukan,int subString)
        {
            int x = 1;
            string noRujukan = prefix + "00000";

            if (string.IsNullOrEmpty(latestNoRujukan))
            {
                noRujukan = string.Format("{0:" + prefix + "00000}", x);
            }
            else
            {
                x = int.Parse(latestNoRujukan.Substring(subString));
                x++;
                noRujukan = string.Format("{0:" + prefix.Substring(subString).ToUpper() + "00000}", x);
            }
            return noRujukan;
        }
    }
}
