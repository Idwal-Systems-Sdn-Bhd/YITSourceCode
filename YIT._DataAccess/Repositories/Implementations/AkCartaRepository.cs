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

        public async Task<List<_AkCartaResult>> GetResults(int? akCartaId, int? akCartaId1, string? tahun)
        {
            if (akCartaId == null || akCartaId1 == null || string.IsNullOrEmpty(tahun))
            {
                return new List<_AkCartaResult>();
            }

            int year = int.Parse(tahun);

            var akCartaList = await _context.AkCarta
                .Include(a => a.AkAkaun1)
                .Where(a => a.Id >= akCartaId && a.Id <= akCartaId1 && a.EnParas == EnParas.Paras4)
                .ToListAsync();

            var akCartaResults = akCartaList.Select(a =>
            {
                var akAkaun1List = a.AkAkaun1?.ToList() ?? new List<AkAkaun>();
                var akAkaun2List = a.AkAkaun2?.ToList() ?? new List<AkAkaun>();

                decimal totalBakiAwal = 0;
                decimal totalJumlah = 0;
                decimal totalBakiAwalH2 = 0;
                decimal totalJumlahH1 = 0;
                decimal totalJumlahH2 = 0;

                decimal jan = 0, feb = 0, mac = 0, apr = 0, mei = 0, jun = 0;
                decimal jul = 0, ogo = 0, sep = 0, okt = 0, nov = 0, dis = 0;

                foreach (var b in akAkaun1List)
                {
                    int month = b.Tarikh.Month;

                    if (b.Tarikh.Year < year)
                    {
                        totalBakiAwal += b.Debit - b.Kredit;
                        totalBakiAwalH2 += b.Debit - b.Kredit;
                    }
                    else if (b.Tarikh.Year == year)
                    {
                        totalJumlah += b.Debit - b.Kredit;

                        if (month >= 1 && month <= 6)
                        {
                            totalBakiAwalH2 += b.Debit - b.Kredit;
                            totalJumlahH1 += b.Debit - b.Kredit;
                        }

                        if (month >= 7 && month <= 12)
                        {
                            totalJumlahH2 += b.Debit - b.Kredit;
                        }

                        switch (month)
                        {
                            case 1:
                                jan += b.Debit - b.Kredit;
                                break;
                            case 2:
                                feb += b.Debit - b.Kredit;
                                break;
                            case 3:
                                mac += b.Debit - b.Kredit;
                                break;
                            case 4:
                                apr += b.Debit - b.Kredit;
                                break;
                            case 5:
                                mei += b.Debit - b.Kredit;
                                break;
                            case 6:
                                jun += b.Debit - b.Kredit;
                                break;
                            case 7:
                                jul += b.Debit - b.Kredit;
                                break;
                            case 8:
                                ogo += b.Debit - b.Kredit;
                                break;
                            case 9:
                                sep += b.Debit - b.Kredit;
                                break;
                            case 10:
                                okt += b.Debit - b.Kredit;
                                break;
                            case 11:
                                nov += b.Debit - b.Kredit;
                                break;
                            case 12:
                                dis += b.Debit - b.Kredit;
                                break;
                        }
                    }
                }

                foreach (var b in akAkaun2List)
                {
                    int month = b.Tarikh.Month;

                    if (b.Tarikh.Year < year)
                    {
                        totalBakiAwal += b.Debit - b.Kredit;
                        totalBakiAwalH2 += b.Debit - b.Kredit;
                    }
                    else if (b.Tarikh.Year == year)
                    {
                        totalJumlah += b.Debit - b.Kredit;

                        if (month >= 1 && month <= 6)
                        {
                            totalBakiAwalH2 += b.Debit - b.Kredit;
                            totalJumlahH1 += b.Debit - b.Kredit;
                        }

                        if (month >= 7 && month <= 12)
                        {
                            totalJumlahH2 += b.Debit - b.Kredit;
                        }

                        switch (month)
                        {
                            case 1:
                                jan += b.Debit - b.Kredit;
                                break;
                            case 2:
                                feb += b.Debit - b.Kredit;
                                break;
                            case 3:
                                mac += b.Debit - b.Kredit;
                                break;
                            case 4:
                                apr += b.Debit - b.Kredit;
                                break;
                            case 5:
                                mei += b.Debit - b.Kredit;
                                break;
                            case 6:
                                jun += b.Debit - b.Kredit;
                                break;
                            case 7:
                                jul += b.Debit - b.Kredit;
                                break;
                            case 8:
                                ogo += b.Debit - b.Kredit;
                                break;
                            case 9:
                                sep += b.Debit - b.Kredit;
                                break;
                            case 10:
                                okt += b.Debit - b.Kredit;
                                break;
                            case 11:
                                nov += b.Debit - b.Kredit;
                                break;
                            case 12:
                                dis += b.Debit - b.Kredit;
                                break;
                        }
                    }
                }

                var akCartaResult = new _AkCartaResult
                {
                    Kod = a.Kod,
                    Perihal = a.Perihal,
                    BakiAwal = totalBakiAwal,
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
                    Jumlah = totalJumlah,
                    JumlahH1 = totalJumlahH1,
                    JumlahH2 = totalJumlahH2,
                    BakiAwalH2 = totalBakiAwalH2,
                    BakiAkhir = totalBakiAwal + totalJumlah,
                    BakiAkhirH1 = totalBakiAwal + totalJumlahH1,
                    BakiAkhirH2 = totalBakiAwal + totalJumlahH1 + totalJumlahH2,
                };

                return akCartaResult;
            }).ToList();

            return akCartaResults;
        }
    }
}