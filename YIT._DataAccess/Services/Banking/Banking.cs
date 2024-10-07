using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;

namespace YIT._DataAccess.Services.Banking
{
    public class Banking : _IBanking
    {
        private readonly ApplicationDbContext _context;
        public Banking(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AkPenyesuaianBankPenyataBank>?> ConvertToAkPenyesuaianPenyataBankList(string? jsonData,int akPenyesuaianBankId, int akBankId)
        {
            List<AkPenyesuaianBankPenyataBank>? result = new List<AkPenyesuaianBankPenyataBank> ();

            List<string> list = JsonConvert.DeserializeObject<List<string>>(jsonData);

            var bankCode = await GetBNMCodeByAkBankIdAsync(akBankId);

            if (list != null && list.Any())
            {
                switch (bankCode)
                {
                    case "BIMBMYKL":
                        result = ConvertBIMBFormat(list, akPenyesuaianBankId);
                        break;
                    default:
                        result = null;
                        break;
                }

            }

            return result;
        }

        public async Task<string> GetBNMCodeByAkBankIdAsync(int akBankId)
        {
            var akBank = await _context.AkBank.Include(b => b.JBank).FirstOrDefaultAsync(b => b.Id == akBankId);

            return akBank?.JBank?.KodBNMEFT ?? "";
        }

        public List<AkPenyesuaianBankPenyataBank> ConvertBIMBFormat(List<string> list, int akPenyesuaianBankId)
        {
            List<AkPenyesuaianBankPenyataBank> results = new List<AkPenyesuaianBankPenyataBank>();
            var bil = 1;

            foreach (var item in list)
            {
                var penyata = new AkPenyesuaianBankPenyataBank();
                //{
                //    Id = 0,
                //    AkPenyesuaianBankId = akPenyesuaianBankId,
                //    Indeks = item.Substring(0, 6),
                //    NoAkaunBank = item.Substring(20,14),
                //    Tarikh = new DateTime(int.Parse(item.Substring(15,4)),int.Parse(item.Substring(11,2)),int.Parse(item.Substring(8,2))),
                //    KodCawanganBank = item.Substring(73,3),
                //    KodTransaksi = item.Substring(35,4),
                //    PerihalTransaksi = item.Substring(40,20),
                //    NoDokumen = item.Substring(61,11),
                //    NoDokumenTambahan1 = item.Substring(145),
                //    Debit = Convert.ToDecimal(item.Substring(77,18)),
                //    Kredit = Convert.ToDecimal(item.Substring(96,18)),
                //    Baki = Convert.ToDecimal(item.Substring(125,18)),
                //    SignDebitKredit = item.Substring(143,1),
                //    IsPadan = false
                //};

                penyata.Id = 0;
                penyata.Bil = bil;
                penyata.AkPenyesuaianBankId = akPenyesuaianBankId;
                penyata.Indeks = item.Substring(0, 6);
                penyata.NoAkaunBank = item.Substring(18, 14);
                int year = int.Parse(item.Substring(13, 4));
                int month = int.Parse(item.Substring(10, 2));
                int day = int.Parse(item.Substring(7, 2));
                penyata.Tarikh = new DateTime(year, month, day);
                penyata.KodCawanganBank = item.Substring(71, 3);
                penyata.KodTransaksi = item.Substring(33, 4);
                penyata.PerihalTransaksi = item.Substring(38, 20);
                penyata.NoDokumen = item.Substring(59, 11);
                penyata.NoDokumenTambahan1 = item.Substring(133);
                var debit = string.IsNullOrWhiteSpace(item.Substring(75, 18)) ? "0" : item.Substring(75, 18);
                var kredit = string.IsNullOrWhiteSpace(item.Substring(94, 18)) ? "0" : item.Substring(94, 18);
                var baki = string.IsNullOrWhiteSpace(item.Substring(113, 18)) ? "0" : item.Substring(113, 18);
                penyata.Debit = Convert.ToDecimal(debit);
                penyata.Kredit = Convert.ToDecimal(kredit);
                penyata.Baki = Convert.ToDecimal(baki);
                penyata.SignDebitKredit = item.Substring(131, 1);
                penyata.IsPadan = false;

                results.Add(penyata);
                bil++;
            }
            
            return results;
        }
    }
}
