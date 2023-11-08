using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Services.Math
{
    public class DateTimeFormatter
    {
        // change string(month) type MM to MMM in Malay
        public static string BulanSingkatanBahasaMelayu(string d)
        {
            string result = "";

            switch (d)
            {
                case "01":
                    result = "Jan";
                    break;
                case "02":
                    result = "Feb";
                    break;
                case "03":
                    result = "Mac";
                    break;
                case "04":
                    result = "Apr";
                    break;
                case "05":
                    result = "Mei";
                    break;
                case "06":
                    result = "Jun";
                    break;
                case "07":
                    result = "Jul";
                    break;
                case "08":
                    result = "Ogo";
                    break;
                case "09":
                    result = "Sep";
                    break;
                case "10":
                    result = "Okt";
                    break;
                case "11":
                    result = "Nov";
                    break;
                case "12":
                    result = "Dis";
                    break;
                default:
                    break;
            }

            return result;
        }

        public static string HariBahasaMelayu(string d)
        {
            string result = "";

            switch (d)
            {
                case "Monday":
                    result = "Isnin";
                    break;
                case "Tuesday":
                    result = "Selasa";
                    break;
                case "Wednesday":
                    result = "Rabu";
                    break;
                case "Thursday":
                    result = "Khamis";
                    break;
                case "Friday":
                    result = "Jumaat";
                    break;
                case "Saturday":
                    result = "Sabtu";
                    break;
                case "Sunday":
                    result = "Ahad";
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}
