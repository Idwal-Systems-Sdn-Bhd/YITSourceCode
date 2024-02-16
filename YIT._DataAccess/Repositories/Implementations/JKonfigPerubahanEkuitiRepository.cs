using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services;
using YIT._DataAccess.Services.Math;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class JKonfigPerubahanEkuitiRepository : _GenericRepository<JKonfigPerubahanEkuiti>, IJKonfigPerubahanEkuitiRepository
    {
        private readonly ApplicationDbContext _context;

        public JKonfigPerubahanEkuitiRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public List<JKonfigPerubahanEkuiti> GetAllDetails()
        {
            return _context.JKonfigPerubahanEkuiti.Include(pe => pe.JKW).ToList();
        }

        public JKonfigPerubahanEkuiti GetAllDetailsById(int id)
        {
            var result = _context.JKonfigPerubahanEkuiti.Include(pe => pe.JKW).Include(pe => pe.JKonfigPerubahanEkuitiBaris).FirstOrDefault(pe => pe.Id == id);

            if (result != null && result.JKonfigPerubahanEkuitiBaris != null && result.JKonfigPerubahanEkuitiBaris.Count > 0)
            {
                string barisBefore = "";

                foreach (var baris in result.JKonfigPerubahanEkuitiBaris.OrderBy(b => b.EnBaris).ThenBy(b => b.EnJenisOperasi))
                {
                    string barisSentences = baris.EnBaris.GetDisplayName();
                    if (barisSentences == barisBefore)
                    {
                        barisSentences = "";
                    }
                    string sentence = FormulaInSentence(baris.EnJenisOperasi, baris.EnJenisCartaList, baris.IsKecuali, baris.KodList);

                    baris.BarisDescription = barisSentences;
                    baris.FormulaDescription = sentence;

                    barisBefore = baris.EnBaris.GetDisplayName();

                }

                result.JKonfigPerubahanEkuitiBaris = result.JKonfigPerubahanEkuitiBaris.OrderBy(b => b.EnBaris).ThenBy(b => b.EnJenisOperasi).ToList();
            }

            return result ?? new JKonfigPerubahanEkuiti();
        }

        public string FormulaInSentence(EnJenisOperasi jenisOperasi, string? jenisCarta, bool isKecuali, string? kodList)
        {
            string? txtexcept = "";
            string? txtcode = "";
            if (!string.IsNullOrEmpty(jenisCarta))
            {
                string[] jenisCartaArray = jenisCarta.Split(",");
                List<string> txtcodeList = new List<string>();
                foreach (var arr in jenisCartaArray)
                {
                    switch (arr[0])
                    {
                        case '1':
                            txtcodeList.Add(EnJenisCarta.Liabiliti.GetDisplayName());
                            break;
                        case '2':
                            txtcodeList.Add(EnJenisCarta.Ekuiti.GetDisplayName());
                            break;
                        case '3':
                            txtcodeList.Add(EnJenisCarta.Belanja.GetDisplayName());
                            break;
                        case '4':
                            txtcodeList.Add(EnJenisCarta.Aset.GetDisplayName());
                            break;
                        case '5':
                            txtcodeList.Add(EnJenisCarta.Hasil.GetDisplayName());
                            break;
                    }
                }
                txtcode = string.Join(",", txtcodeList);
                if (isKecuali && !string.IsNullOrEmpty(kodList))
                {

                    string[] kodListArray = kodList.Split(",");
                    List<string> txtexceptcodeList = new List<string>();
                    foreach (var arr in kodListArray)
                    {
                        var kodAkaun = _context.AkCarta.Find(int.Parse(arr))?.Kod ?? "";
                        txtexceptcodeList.Add(kodAkaun);
                    }
                    txtexcept = $" kecuali kod - kod({string.Join(",", txtexceptcodeList)})";
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(kodList))
                {
                    string[] kodListArray = kodList.Split(",");
                    List<string> txtcodeList = new List<string>();
                    foreach (var arr in kodListArray)
                    {
                        var kodAkaun = _context.AkCarta.Find(int.Parse(arr))?.Kod ?? "";
                        txtcodeList.Add(kodAkaun);
                    }
                    txtcode = string.Join(",", txtcodeList);
                }

            }

            string sentences = "";

            if (kodList != null || jenisCarta != null)
            {
                if (jenisOperasi == EnJenisOperasi.Tambah)
                {
                    sentences = $"Jumlah bagi kod - kod ({txtcode}){txtexcept}";
                }
                else
                {
                    sentences = $"ditolak dengan jumlah bagi kod - kod ({txtcode}){txtexcept}";
                }
            }
            else
            {
                if (jenisOperasi == EnJenisOperasi.Tambah)
                {
                    sentences = "Tiada formula operasi tambah";
                }
                else
                {
                    sentences = "Tiada formula operasi tolak";
                }
            }
            

            return sentences;
        }

        public JKonfigPerubahanEkuiti GetAllDetailsByTahunOrJenisEkuiti(string? tahun, EnJenisLajurJadualPerubahanEkuiti? enJenisEkuiti)
        {
            var result = new JKonfigPerubahanEkuiti();
            result = _context.JKonfigPerubahanEkuiti.Include(pe => pe.JKW).Include(pe => pe.JKonfigPerubahanEkuitiBaris).FirstOrDefault(pe => pe.Tahun == tahun);

            if (enJenisEkuiti != null)
            {
                result = _context.JKonfigPerubahanEkuiti.Include(pe => pe.JKW).Include(pe => pe.JKonfigPerubahanEkuitiBaris).FirstOrDefault(pe => pe.Tahun == tahun && pe.EnLajurJadual == enJenisEkuiti);
            }

            if (result != null && result.JKonfigPerubahanEkuitiBaris != null && result.JKonfigPerubahanEkuitiBaris.Count > 0)
            {
                string barisBefore = "";

                foreach (var baris in result.JKonfigPerubahanEkuitiBaris.OrderBy(b => b.EnBaris).ThenBy(b => b.EnJenisOperasi))
                {
                    string barisSentences = baris.EnBaris.GetDisplayName();
                    if (barisSentences == barisBefore)
                    {
                        barisSentences = "";
                    }
                    string sentence = FormulaInSentence(baris.EnJenisOperasi, baris.EnJenisCartaList, baris.IsKecuali, baris.KodList);

                    baris.BarisDescription = barisSentences;
                    baris.FormulaDescription = sentence;

                    barisBefore = baris.EnBaris.GetDisplayName();

                }

                result.JKonfigPerubahanEkuitiBaris = result.JKonfigPerubahanEkuitiBaris.OrderBy(b => b.EnBaris).ThenBy(b => b.EnJenisOperasi).ToList();
            }

            return result ?? new JKonfigPerubahanEkuiti();
        }
    }
}
