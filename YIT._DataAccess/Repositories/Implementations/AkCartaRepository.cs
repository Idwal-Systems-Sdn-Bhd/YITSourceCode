using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
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

        public async Task<List<_AkCartaResult>> GetResults(int? akCartaId, string? tahun)
        {
            if (akCartaId == null || string.IsNullOrEmpty(tahun))
            {
                return new List<_AkCartaResult>();
            }

            int year = int.Parse(tahun);

            var akCartaList = await _context.AkCarta
                .Include(a => a.AkAkaun1)
                .Where(a => a.Id == akCartaId)
                .ToListAsync();

            var akCartaResults = akCartaList.Select(a =>
            {
                var akAkaun1List = a.AkAkaun1!.ToList();

                var bakiAwal = akAkaun1List
                    .Where(b => b.Tarikh.Year < year)
                    .Sum(b => b.Debit - b.Kredit);

                var bakiAwalH2 = akAkaun1List
                .Where(b => (b.Tarikh.Year == year && b.Tarikh.Month >= 1 && b.Tarikh.Month <= 6) || 
                 b.Tarikh.Year < year) 
                .Sum(b => b.Debit - b.Kredit);

                var jumlah = akAkaun1List
                    .Where(b => b.Tarikh.Year == year)
                    .Sum(b => b.Debit - b.Kredit);

                var jumlahH1 = akAkaun1List
                    .Where(b => b.Tarikh.Year == year && b.Tarikh.Month >= 1 && b.Tarikh.Month <= 6)
                    .Sum(b => b.Debit - b.Kredit);

                var jumlahH2 = akAkaun1List
                    .Where(b => b.Tarikh.Year == year && b.Tarikh.Month >= 7 && b.Tarikh.Month <= 12)
                    .Sum(b => b.Debit - b.Kredit);

                var jan = akAkaun1List
                    .Where(b => b.Tarikh.Year == year && b.Tarikh.Month == 1)
                    .Sum(b => b.Debit - b.Kredit);

                var feb = akAkaun1List
                    .Where(b => b.Tarikh.Year == year && b.Tarikh.Month == 2)
                    .Sum(b => b.Debit - b.Kredit);

                var mac = akAkaun1List
                    .Where(b => b.Tarikh.Year == year && b.Tarikh.Month == 3)
                    .Sum(b => b.Debit - b.Kredit);

                var apr = akAkaun1List
                    .Where(b => b.Tarikh.Year == year && b.Tarikh.Month == 4)
                    .Sum(b => b.Debit - b.Kredit);

                var mei = akAkaun1List
                    .Where(b => b.Tarikh.Year == year && b.Tarikh.Month == 5)
                    .Sum(b => b.Debit - b.Kredit);

                var jun = akAkaun1List
                    .Where(b => b.Tarikh.Year == year && b.Tarikh.Month == 6)
                    .Sum(b => b.Debit - b.Kredit);

                var jul = akAkaun1List
                    .Where(b => b.Tarikh.Year == year && b.Tarikh.Month == 7)
                    .Sum(b => b.Debit - b.Kredit);

                var ogo = akAkaun1List
                    .Where(b => b.Tarikh.Year == year && b.Tarikh.Month == 8)
                    .Sum(b => b.Debit - b.Kredit);

                var sep = akAkaun1List
                    .Where(b => b.Tarikh.Year == year && b.Tarikh.Month == 9)
                    .Sum(b => b.Debit - b.Kredit);

                var okt = akAkaun1List
                    .Where(b => b.Tarikh.Year == year && b.Tarikh.Month == 10)
                    .Sum(b => b.Debit - b.Kredit);

                var nov = akAkaun1List
                    .Where(b => b.Tarikh.Year == year && b.Tarikh.Month == 11)
                    .Sum(b => b.Debit - b.Kredit);

                var dis = akAkaun1List
                    .Where(b => b.Tarikh.Year == year && b.Tarikh.Month == 12)
                    .Sum(b => b.Debit - b.Kredit);

                var akCartaResult = new _AkCartaResult
                {
                    Kod = a.Kod,
                    Perihal = a.Perihal,
                    BakiAwal = bakiAwal,
                    Jan = jan,
                    Feb = feb,
                    Mac = mac,
                    Apr = apr,
                    Mei = mei,
                    Jun = jun,
                    Jul = jul,
                    Ogo = ogo,
                    Sep = sep,
                    Okt = okt,
                    Nov = nov,
                    Dis = dis,
                    Jumlah = jumlah,
                    JumlahH1 = jumlahH1, 
                    JumlahH2 = jumlahH2, 
                    BakiAwalH2 = bakiAwalH2,
                    BakiAkhir = bakiAwal + jumlah,
                    BakiAkhirH1 = bakiAwal + jumlahH1,
                    BakiAkhirH2 = bakiAwal + jumlahH1 + jumlahH2,
                };

                return akCartaResult;
            }).ToList();

            return akCartaResults;
        }

    }
}
