using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT._DataAccess.Services;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class AkCartaRepository : _GenericRepository<AkCarta>, IAkCartaRepository
    {
        private readonly ApplicationDbContext _context;

        public AkCartaRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public List<AkCarta> GetResultsByJenis(EnJenisCarta jenis, EnParas paras)
        {
            return _context.AkCarta.Where(c => c.EnJenis == jenis && c.EnParas == paras).ToList();
        }

        public List<AkCarta> GetResultsByParas(EnParas paras)
        {
            return _context.AkCarta.Where(c => c.EnParas == paras).ToList();
        }

        public string GetSetOfCartaStringList(bool isPukal, string? enJenisCartaList, bool isKecuali, string? kodList)
        {
            List<string> setKodList = new List<string>();

            List<string> arrKodList = kodList?.Split(',').ToList() ?? new List<string>();

            if (isPukal)
            {
                List<string> arrJenisCartaList = enJenisCartaList?.Split(',').ToList() ?? new List<string>();
                foreach (var jenisCarta in arrJenisCartaList)
                {

                    var akCartaList = GetCartaListByJenisCarta((EnJenisCarta)int.Parse(jenisCarta), isKecuali, arrKodList);
                    setKodList = akCartaList;
                }
            }
            else
            {
                setKodList = arrKodList;
            }

            return string.Join(',', setKodList);
        }

        private List<string> GetCartaListByJenisCarta(EnJenisCarta jenisCartaId, bool isKecuali, List<string>? arrKodList)
        {
            var cartaList = _context.AkCarta
                .Where(a => a.EnJenis.Equals(jenisCartaId) && (!isKecuali || !arrKodList!.Contains(a.Id.ToString())))
                .Select(c => c.Id.ToString())
                .ToList();

            return cartaList ?? new List<string>();
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
    }
}
