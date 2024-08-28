using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Services.Math
{
    public class PriceFormatter
    {
        static string[] satuan = new string[10] { "kosong", "satu", "dua", "tiga", "empat", "lima", "enam", "tujuh", "lapan", "sembilan" };
        static string[] belasan = new string[10] { "sepuluh", "sebelas", "dua belas", "tiga belas", "empat belas", "lima belas", "enam belas", "tujuh belas", "lapan belas", "sembilan belas" };
        static string[] puluhan = new string[10] { "", "", "dua puluh", "tiga puluh", "empat puluh", "lima puluh", "enam puluh", "tujuh puluh", "lapan puluh", "sembilan puluh" };
        static string[] ribuan = new string[5] { "", "ribu", "juta", "milion", "triliyun" };

        // change currency to words
        public static string JumlahDalamPerkataan(decimal d)
        {
            string result = wordString(d);
            string[] rs = result.Split(" ");

            if (rs[rs.Length - 1] != "sen")
            {
                result += " sahaja";
            }
            else
            {
                result += " sahaja";
            }
            return result;
        }

        public static string wordString(decimal d)
        {
            string strHasil = "";
            decimal frac = d - decimal.Truncate(d);

            if (decimal.Compare(frac, 0.0m) != 0)
                strHasil = "dan sen " + wordString(decimal.Round(frac * 100));
            else
                strHasil = "";
            int xDigit = 0;
            int xPosisi = 0;

            string strTemp = decimal.Truncate(d).ToString();
            for (int i = strTemp.Length; i > 0; i--)
            {
                string tmpx = "";
                xDigit = Convert.ToInt32(strTemp.Substring(i - 1, 1));
                xPosisi = (strTemp.Length - i) + 1;
                switch (xPosisi % 3)
                {
                    case 1:
                        bool allNull = false;
                        if (i == 1)
                            tmpx = satuan[xDigit] + " ";
                        else if (strTemp.Substring(i - 2, 1) == "1")
                            tmpx = belasan[xDigit] + " ";
                        else if (xDigit > 0)
                            tmpx = satuan[xDigit] + " ";
                        else
                        {
                            allNull = true;
                            if (i > 1)
                                if (strTemp.Substring(i - 2, 1) != "0")
                                    allNull = false;
                            if (i > 2)
                                if (strTemp.Substring(i - 3, 1) != "0")
                                    allNull = false;
                            tmpx = "";
                        }

                        if ((!allNull) && (xPosisi > 1))
                            tmpx = tmpx + ribuan[(int)decimal.Round(xPosisi / 3)] + " ";
                        strHasil = tmpx + strHasil;
                        break;
                    case 2:
                        if (xDigit > 0)
                            strHasil = puluhan[xDigit] + " " + strHasil;
                        break;
                    case 0:
                        if (xDigit > 0)
                            strHasil = satuan[xDigit] + " ratus " + strHasil;
                        break;
                }
            }
            strHasil = strHasil.Trim().ToLower();
            if (strHasil.Length > 0)
            {
                strHasil = strHasil.Substring(0, 1).ToUpper() +
                  strHasil.Substring(1, strHasil.Length - 1);
            }
            return strHasil;
        }
        // change currency to words end

        // get tax amount
        public static decimal GetTaxAmount(decimal harga, decimal kuantiti, decimal kadarCukai)
        {
            return harga * kuantiti * kadarCukai / 100;
        }

        // get total amount include tax
        public static decimal GetTotalPayableAmount(decimal harga, decimal kuantiti, decimal amaunCukai)
        {
            return (harga * kuantiti) + amaunCukai;
        }
    }
}
