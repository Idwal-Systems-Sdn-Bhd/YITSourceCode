using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Rotativa.AspNetCore;
using System.Data;
using System.Net;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT.Akaun.Infrastructure;
using YIT.Akaun.Models.ViewModels.Forms;
using YIT.Akaun.Models.ViewModels.Prints;

namespace YIT.Akaun.Controllers._99Laporan
{
    [Authorize]
    public class LAK015Controller : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodLSiBerhutang;
        public const string namamodul = Modules.namaLSiBerhutang;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly UserServices _userServices;

        public LAK015Controller(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            _IUnitOfWork unitOfWork,
            IMemoryCache cache,
            UserServices userServices
            )
        {
            _context = context;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _cache = cache;
            _userServices = userServices;
        }
        public IActionResult Index(PrintFormModel model)
        {
            PopulateSelectList(model.AkCartaId);
            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> ExportExcel(PrintFormModel model)
        {
            LAK015PrintModel printModel = await PrepareData(model.kodLaporan, model.AkCartaId, model.tahun);

            // Generate a new unique identifier against which the file can be stored
            string handle = string.Format("attachment;" + model.kodLaporan + ".xlsx;", string.IsNullOrEmpty(model.kodLaporan) ? Guid.NewGuid().ToString() : WebUtility.UrlEncode(model.kodLaporan));

            // save viewmodel into workbook
            if (model.kodLaporan == "LAK00201")
            {
                // construct and insert data into dataTable 
                var excelData = GenerateDataTableLAK00201(printModel, model.AkCartaId, model.tahun);

                // insert dataTable into Workbook
                RunWorkBookLAK00201(printModel, excelData, handle);
            }
            // save viewmodel into workbook
            else if (model.kodLaporan == "LAK00202")
            {
                //construct and insert data into dataTable
                var excelData = GenerateDataTableLAK00202(printModel, model.AkCartaId, model.tahun);

                //insert dataTable into Workbook
                RunWorkBookLAK00202(printModel, excelData, handle);
            }
            else if (model.kodLaporan == "LAK00203")
            {
                //construct and insert data into dataTable
                var excelData = GenerateDataTableLAK00203(printModel, model.AkCartaId, model.tahun);

                //insert dataTable into Workbook
                RunWorkBookLAK00203(printModel, excelData, handle);
            }

            return Json(new { FileGuid = handle, FileName = model.kodLaporan + ".xlsx" });
        }

        private async Task<LAK015PrintModel> PrepareData(string? kodLaporan, int? akCartaId, string? tahun)
        {
            LAK015PrintModel reportModel = new LAK015PrintModel();

            var user = await _userManager.GetUserAsync(User);
            var namaUser = await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.Email == user!.Email);

            reportModel.CommonModels.Username = namaUser?.Nama;

            reportModel.CommonModels.KodLaporan = kodLaporan;
            CompanyDetails company = new CompanyDetails();
            reportModel.CommonModels.CompanyDetails = company;

            string? selectedKod = "";
            string? selectedPerihal = "";

            if (akCartaId.HasValue)
            {
                var selectedItem = await _unitOfWork.AkCartaRepo.GetByIdAsync(akCartaId.Value);
                if (selectedItem != null)
                {
                    selectedKod = selectedItem.Kod;
                    selectedPerihal = selectedItem.Perihal;
                }
            }

            var akCartaList = await _unitOfWork.AkCartaRepo.GetResults(akCartaId, tahun);
            reportModel.JumlahJan = akCartaList.Sum(r => r.Jan);
            reportModel.JumlahFeb = akCartaList.Sum(r => r.Feb);
            reportModel.JumlahMac = akCartaList.Sum(r => r.Mac);
            reportModel.JumlahApr = akCartaList.Sum(r => r.Apr);
            reportModel.JumlahMei = akCartaList.Sum(r => r.Mei);
            reportModel.JumlahJun = akCartaList.Sum(r => r.Jun);
            reportModel.JumlahJul = akCartaList.Sum(r => r.Jul);
            reportModel.JumlahOgo = akCartaList.Sum(r => r.Ogo);
            reportModel.JumlahSep = akCartaList.Sum(r => r.Sep);
            reportModel.JumlahOkt = akCartaList.Sum(r => r.Okt);
            reportModel.JumlahNov = akCartaList.Sum(r => r.Nov);
            reportModel.JumlahDis = akCartaList.Sum(r => r.Dis);

            reportModel.AkCartaResult = akCartaList;

            if (kodLaporan == "LAK00201")
            {
                reportModel.CommonModels.Tajuk1 = $"Laporan SiBerhutang Pada Tahun {tahun} Mengikut Kod Akaun {selectedKod} - {selectedPerihal} ";

                reportModel.JumlahBaki = akCartaList.Sum(r => r.BakiAwal); 
                reportModel.JumlahAkhir = akCartaList.Sum(r => r.Jumlah);
                reportModel.Jumlah1 = akCartaList.Sum(r => r.BakiAwal + r.Jan + r.Feb + r.Mac + r.Apr + r.Mei + r.Jun + r.Jul + r.Ogo + r.Sep + r.Okt + r.Nov + r.Dis); 
            }
            else if (kodLaporan == "LAK00202")
            {
                reportModel.CommonModels.Tajuk1 = $"Laporan SiBerhutang Setengah Tahun Pertama Pada Tahun {tahun} Mengikut Kod Akaun {selectedKod} - {selectedPerihal} ";

                reportModel.JumlahBaki = akCartaList.Sum(r => r.BakiAwal); 
                reportModel.JumlahAkhir = akCartaList.Sum(r => r.JumlahH1);
                reportModel.Jumlah1 = akCartaList.Sum(r => r.BakiAwal + r.Jan + r.Feb + r.Mac + r.Apr + r.Mei + r.Jun); 
            }
            else if (kodLaporan == "LAK00203")
            {
                reportModel.CommonModels.Tajuk1 = $"Laporan Siberhutang Setengah Tahun Kedua Pada Tahun {tahun} Mengikut Kod Akaun {selectedKod} - {selectedPerihal} ";

                reportModel.JumlahBaki = akCartaList.Sum(r => r.BakiAwalH2);  
                reportModel.JumlahAkhir = akCartaList.Sum(r => r.JumlahH2);
                reportModel.Jumlah1 = akCartaList.Sum(r => r.BakiAwalH2 + r.Jul + r.Ogo + r.Sep + r.Okt + r.Nov + r.Dis); 
            }

            return reportModel;
        }

        private DataTable GenerateDataTableLAK00201(LAK015PrintModel printModel, int? akCartaId, string? tahun)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Laporan SiBerhutang";
            dt.Columns.Add("Kod", typeof(string));
            dt.Columns.Add("Nama Akaun", typeof(string));
            dt.Columns.Add("Baki Pada 01/01", typeof(decimal));
            dt.Columns.Add("Jan", typeof(decimal));
            dt.Columns.Add("Feb", typeof(decimal));
            dt.Columns.Add("Mac", typeof(decimal));
            dt.Columns.Add("Apr", typeof(decimal));
            dt.Columns.Add("Mei", typeof(decimal));
            dt.Columns.Add("Jun", typeof(decimal));
            dt.Columns.Add("Jul", typeof(decimal));
            dt.Columns.Add("Ogo", typeof(decimal));
            dt.Columns.Add("Sep", typeof(decimal));
            dt.Columns.Add("Okt", typeof(decimal));
            dt.Columns.Add("Nov", typeof(decimal));
            dt.Columns.Add("Dis", typeof(decimal));
            dt.Columns.Add("Jumlah RM", typeof(decimal));
            dt.Columns.Add("Baki Pada 31/12", typeof(decimal));

            if (akCartaId != null && !string.IsNullOrEmpty(tahun))
            {
                int year = int.Parse(tahun);

                var akCartaList = _context.AkCarta
                    .Include(a => a.AkAkaun1)
                    .Where(a => a.Id == akCartaId)
                    .ToList(); 

                decimal totalBakiAwal = 0;
                decimal totalJan = 0, totalFeb = 0, totalMac = 0, totalApr = 0;
                decimal totalMei = 0, totalJun = 0, totalJul = 0, totalOgo = 0;
                decimal totalSep = 0, totalOkt = 0, totalNov = 0, totalDis = 0;
                decimal totalJumlah = 0;
                decimal totalBakiAkhir = 0;

                foreach (var akCarta in akCartaList)
                {
                    var akAkaun1List = akCarta.AkAkaun1!.ToList();

                    var bakiAwal = akAkaun1List
                        .Where(b => b.Tarikh.Year < year)
                        .Sum(b => b.Debit - b.Kredit);

                    var jumlah = akAkaun1List
                        .Where(b => b.Tarikh.Year == year)
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

                    var bakiAkhir = bakiAwal + jumlah;

                    string cleanedKod = akCarta.Kod!.Trim();
                    string cleanedPerihal = akCarta.Perihal!.Trim();

                    dt.Rows.Add(
                        cleanedKod,
                        cleanedPerihal,
                        bakiAwal,
                        jan, feb, mac, apr, mei, jun, jul, ogo, sep, okt, nov, dis,
                        jumlah,
                        bakiAkhir
                    );

                    totalBakiAwal += bakiAwal;
                    totalJan += jan; totalFeb += feb; totalMac += mac; totalApr += apr;
                    totalMei += mei; totalJun += jun; totalJul += jul; totalOgo += ogo;
                    totalSep += sep; totalOkt += okt; totalNov += nov; totalDis += dis;
                    totalJumlah += jumlah;
                    totalBakiAkhir += bakiAkhir;
                }

                var grandTotalRow = dt.NewRow();
                grandTotalRow["Nama Akaun"] = "JUMLAH RM";
                grandTotalRow["Baki Pada 01/01"] = totalBakiAwal;
                grandTotalRow["Jan"] = totalJan;
                grandTotalRow["Feb"] = totalFeb;
                grandTotalRow["Mac"] = totalMac;
                grandTotalRow["Apr"] = totalApr;
                grandTotalRow["Mei"] = totalMei;
                grandTotalRow["Jun"] = totalJun;
                grandTotalRow["Jul"] = totalJul;
                grandTotalRow["Ogo"] = totalOgo;
                grandTotalRow["Sep"] = totalSep;
                grandTotalRow["Okt"] = totalOkt;
                grandTotalRow["Nov"] = totalNov;
                grandTotalRow["Dis"] = totalDis;
                grandTotalRow["Jumlah RM"] = totalJumlah;
                grandTotalRow["Baki Pada 31/12"] = totalBakiAkhir;

                dt.Rows.Add(grandTotalRow);
            }

            return dt;
        }

        private void RunWorkBookLAK00201(LAK015PrintModel printModel, DataTable excelData, string handle)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Laporan SiBerhutang");
                ws.Cell("A1").Value = printModel.CommonModels.CompanyDetails?.NamaSyarikat;
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A2").Value = printModel.CommonModels.Tajuk1;
                ws.Cell("A3").Value = printModel.CommonModels.Tajuk2;

                ws.ColumnWidth = 5;
                ws.Cell("A5").InsertTable(excelData)
                    .Theme = XLTableTheme.TableStyleMedium1;

                ws.Column(1).Width = 8; 
                ws.Column(2).AdjustToContents();
                ws.Column(3)
                   .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(3).AdjustToContents();
                ws.Column(4)
                   .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(4).AdjustToContents();
                ws.Column(5)
                   .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(5).AdjustToContents();
                ws.Column(6)
                   .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(6).AdjustToContents();
                ws.Column(7)
                    .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(7).AdjustToContents();
                ws.Column(8)
                    .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(8).AdjustToContents();
                ws.Column(9)
                    .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(9).AdjustToContents();
                ws.Column(10)
                    .Style.NumberFormat.Format = "#,##0.00";
                ws.Column(10).AdjustToContents();
                ws.Column(11)
                    .Style.NumberFormat.Format = "#,##0.00";
                ws.Column(11).AdjustToContents();
                ws.Column(12)
                    .Style.NumberFormat.Format = "#,##0.00";
                ws.Column(12).AdjustToContents();
                ws.Column(13)
                    .Style.NumberFormat.Format = "#,##0.00";
                ws.Column(13).AdjustToContents();
                ws.Column(14)
                    .Style.NumberFormat.Format = "#,##0.00";
                ws.Column(14).AdjustToContents();
                ws.Column(15)
                    .Style.NumberFormat.Format = "#,##0.00";
                ws.Column(15).AdjustToContents();
                ws.Column(16)
                    .Style.NumberFormat.Format = "#,##0.00";
                ws.Column(16).AdjustToContents();
                ws.Column(17)
                    .Style.NumberFormat.Format = "#,##0.00";
                ws.Column(17).AdjustToContents();

                int grandTotalRowIndex = excelData.Rows.Count + 5;

                ws.Row(grandTotalRowIndex).Style.Font.Bold = true;

                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveAs(ms);

                    //This is an equivalent to tempdata, but requires manual cleanup
                    _cache.Set(handle, ms.ToArray(),
                                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));
                }
            }
        }

        private DataTable GenerateDataTableLAK00202(LAK015PrintModel printModel, int? akCartaId, string? tahun)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Laporan SiBerhutang (Pertama)";
            dt.Columns.Add("Kod", typeof(string));
            dt.Columns.Add("Nama Akaun", typeof(string));
            dt.Columns.Add("Baki Pada 01/01", typeof(decimal));
            dt.Columns.Add("Jan", typeof(decimal));
            dt.Columns.Add("Feb", typeof(decimal));
            dt.Columns.Add("Mac", typeof(decimal));
            dt.Columns.Add("Apr", typeof(decimal));
            dt.Columns.Add("Mei", typeof(decimal));
            dt.Columns.Add("Jun", typeof(decimal));
            dt.Columns.Add("Jumlah RM", typeof(decimal));
            dt.Columns.Add("Baki Pada 30/06", typeof(decimal));

            if (akCartaId != null && !string.IsNullOrEmpty(tahun))
            {
                int year = int.Parse(tahun);

                var akCartaList = _context.AkCarta
                    .Include(a => a.AkAkaun1)
                    .Where(a => a.Id == akCartaId)
                    .ToList(); 

                decimal totalBakiAwal = 0;
                decimal totalJan = 0, totalFeb = 0, totalMac = 0, totalApr = 0;
                decimal totalMei = 0, totalJun = 0;
                decimal totalJumlah = 0;
                decimal totalBakiAkhir = 0;

                foreach (var akCarta in akCartaList)
                {
                    var akAkaun1List = akCarta.AkAkaun1!.ToList();

                    var bakiAwal = akAkaun1List
                        .Where(b => b.Tarikh.Year < year)
                        .Sum(b => b.Debit - b.Kredit);

                    var jumlah = akAkaun1List
                    .Where(b => b.Tarikh.Year == year && b.Tarikh.Month >= 1 && b.Tarikh.Month <= 6)
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

                    var bakiAkhir = bakiAwal + jumlah;

                    string cleanedKod = akCarta.Kod!.Trim();
                    string cleanedPerihal = akCarta.Perihal!.Trim();

                    dt.Rows.Add(
                        cleanedKod,
                        cleanedPerihal,
                        bakiAwal,
                        jan, feb, mac, apr, mei, jun,
                        jumlah,
                        bakiAkhir
                    );

                    totalBakiAwal += bakiAwal;
                    totalJan += jan; totalFeb += feb; totalMac += mac; totalApr += apr;
                    totalMei += mei; totalJun += jun;
                    totalJumlah += jumlah;
                    totalBakiAkhir += bakiAkhir;
                }

                var grandTotalRow = dt.NewRow();
                grandTotalRow["Nama Akaun"] = "JUMLAH RM";
                grandTotalRow["Baki Pada 01/01"] = totalBakiAwal;
                grandTotalRow["Jan"] = totalJan;
                grandTotalRow["Feb"] = totalFeb;
                grandTotalRow["Mac"] = totalMac;
                grandTotalRow["Apr"] = totalApr;
                grandTotalRow["Mei"] = totalMei;
                grandTotalRow["Jun"] = totalJun;
                grandTotalRow["Jumlah RM"] = totalJumlah;
                grandTotalRow["Baki Pada 30/06"] = totalBakiAkhir;

                dt.Rows.Add(grandTotalRow);
            }

            return dt;
        }

        private void RunWorkBookLAK00202(LAK015PrintModel printModel, DataTable excelData, string handle)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Laporan Siberhutang (Pertama)");
                ws.Cell("A1").Value = printModel.CommonModels.CompanyDetails?.NamaSyarikat;
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A2").Value = printModel.CommonModels.Tajuk1;
                ws.Cell("A3").Value = printModel.CommonModels.Tajuk2;

                ws.ColumnWidth = 5;
                ws.Cell("A5").InsertTable(excelData)
                    .Theme = XLTableTheme.TableStyleMedium1;

                ws.Column(1).Width = 8; 
                ws.Column(2).AdjustToContents();
                ws.Column(3)
                   .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(3).AdjustToContents();
                ws.Column(4)
                   .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(4).AdjustToContents();
                ws.Column(5)
                   .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(5).AdjustToContents();
                ws.Column(6)
                   .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(6).AdjustToContents();
                ws.Column(7)
                    .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(7).AdjustToContents();
                ws.Column(8)
                    .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(8).AdjustToContents();
                ws.Column(9)
                    .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(9).AdjustToContents();
                ws.Column(10)
                    .Style.NumberFormat.Format = "#,##0.00";
                ws.Column(10).AdjustToContents();
                ws.Column(11)
                    .Style.NumberFormat.Format = "#,##0.00";
                ws.Column(11).AdjustToContents();

                int grandTotalRowIndex = excelData.Rows.Count + 5;

                ws.Row(grandTotalRowIndex).Style.Font.Bold = true;

                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveAs(ms);

                    //This is an equivalent to tempdata, but requires manual cleanup
                    _cache.Set(handle, ms.ToArray(),
                                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));
                }
            }
        }

        private DataTable GenerateDataTableLAK00203(LAK015PrintModel printModel, int? akCartaId, string? tahun)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Laporan SiBerhutang (Kedua)";
            dt.Columns.Add("Kod", typeof(string));
            dt.Columns.Add("Nama Akaun", typeof(string));
            dt.Columns.Add("Baki Pada 01/07", typeof(decimal));
            dt.Columns.Add("Jul", typeof(decimal));
            dt.Columns.Add("Ogo", typeof(decimal));
            dt.Columns.Add("Sep", typeof(decimal));
            dt.Columns.Add("Okt", typeof(decimal));
            dt.Columns.Add("Nov", typeof(decimal));
            dt.Columns.Add("Dis", typeof(decimal));
            dt.Columns.Add("Jumlah RM", typeof(decimal));
            dt.Columns.Add("Baki Pada 31/12", typeof(decimal));

            if (akCartaId != null && !string.IsNullOrEmpty(tahun))
            {
                int year = int.Parse(tahun);

                var akCartaList = _context.AkCarta
                    .Include(a => a.AkAkaun1)
                    .Where(a => a.Id == akCartaId)
                    .ToList();  

                decimal totalBakiAwal = 0;
                decimal totalJul = 0, totalOgo = 0;
                decimal totalSep = 0, totalOkt = 0, totalNov = 0, totalDis = 0;
                decimal totalJumlah = 0;
                decimal totalBakiAkhir = 0;

                foreach (var akCarta in akCartaList)
                {
                    var akAkaun1List = akCarta.AkAkaun1!.ToList();

                    var bakiAwal = akAkaun1List
                    .Where(b => (b.Tarikh.Year == year && b.Tarikh.Month >= 1 && b.Tarikh.Month <= 6) ||  b.Tarikh.Year < year)
                    .Sum(b => b.Debit - b.Kredit);

                    var jumlah = akAkaun1List
                    .Where(b => b.Tarikh.Year == year && b.Tarikh.Month >= 7 && b.Tarikh.Month <= 12)
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

                    var bakiAkhir = bakiAwal + jumlah;

                    string cleanedKod = akCarta.Kod!.Trim();
                    string cleanedPerihal = akCarta.Perihal!.Trim();

                    dt.Rows.Add(
                        cleanedKod,
                        cleanedPerihal,
                        bakiAwal,
                        jul, ogo, sep, okt, nov, dis,
                        jumlah,
                        bakiAkhir
                    );

                    totalBakiAwal += bakiAwal;
                    totalJul += jul; totalOgo += ogo;
                    totalSep += sep; totalOkt += okt; totalNov += nov; totalDis += dis;
                    totalJumlah += jumlah;
                    totalBakiAkhir += bakiAkhir;
                }

                var grandTotalRow = dt.NewRow();
                grandTotalRow["Nama Akaun"] = "JUMLAH RM";
                grandTotalRow["Baki Pada 01/07"] = totalBakiAwal;
                grandTotalRow["Jul"] = totalJul;
                grandTotalRow["Ogo"] = totalOgo;
                grandTotalRow["Sep"] = totalSep;
                grandTotalRow["Okt"] = totalOkt;
                grandTotalRow["Nov"] = totalNov;
                grandTotalRow["Dis"] = totalDis;
                grandTotalRow["Jumlah RM"] = totalJumlah;
                grandTotalRow["Baki Pada 31/12"] = totalBakiAkhir;

                dt.Rows.Add(grandTotalRow);
            }

            return dt;
        }

        private void RunWorkBookLAK00203(LAK015PrintModel printModel, DataTable excelData, string handle)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Laporan SiBerhutang (Kedua)");
                ws.Cell("A1").Value = printModel.CommonModels.CompanyDetails?.NamaSyarikat;
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A2").Value = printModel.CommonModels.Tajuk1;
                ws.Cell("A3").Value = printModel.CommonModels.Tajuk2;

                ws.ColumnWidth = 5;
                ws.Cell("A5").InsertTable(excelData)
                    .Theme = XLTableTheme.TableStyleMedium1;

                ws.Column(1).Width = 8; 
                ws.Column(2).AdjustToContents();
                ws.Column(3)
                   .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(3).AdjustToContents();
                ws.Column(4)
                   .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(4).AdjustToContents();
                ws.Column(5)
                   .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(5).AdjustToContents();
                ws.Column(6)
                   .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(6).AdjustToContents();
                ws.Column(7)
                    .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(7).AdjustToContents();
                ws.Column(8)
                    .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(8).AdjustToContents();
                ws.Column(9)
                    .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(9).AdjustToContents();
                ws.Column(10)
                    .Style.NumberFormat.Format = "#,##0.00";
                ws.Column(10).AdjustToContents();
                ws.Column(11)
                    .Style.NumberFormat.Format = "#,##0.00";
                ws.Column(11).AdjustToContents();

                int grandTotalRowIndex = excelData.Rows.Count + 5;

                ws.Row(grandTotalRowIndex).Style.Font.Bold = true;

                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveAs(ms);

                    //This is an equivalent to tempdata, but requires manual cleanup
                    _cache.Set(handle, ms.ToArray(),
                                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));
                }
            }
        }

        private void PopulateSelectList(int? akCartaId)
        {
            var cartaList = _unitOfWork.AkCartaRepo.GetResultsByParas(EnParas.Paras4);
            var cartaSelect = new List<SelectListItem>();

            if (cartaList != null)
            {
                cartaSelect.AddRange(cartaList.Select(item => new SelectListItem
                {
                    Text = item.Kod + " - " + item.Perihal,
                    Value = item.Id.ToString()
                }));
            }
            else
            {
                cartaSelect.Add(new SelectListItem
                {
                    Text = "-- Tiada Kumpulan Wang Berdaftar --",
                    Value = ""
                });
            }

            ViewBag.AkCarta = new SelectList(cartaSelect, "Value", "Text");

            if (akCartaId.HasValue && akCartaId.Value != 0)
            {
                var selectedItem = cartaList!.FirstOrDefault(x => x.Id == akCartaId.Value);
                if (selectedItem != null)
                {
                    ViewBag.SelectedKod = selectedItem.Kod;
                    ViewBag.SelectedPerihal = selectedItem.Perihal;
                }
                else
                {
                    ViewBag.SelectedKod = "";
                    ViewBag.SelectedPerihal = "";
                }
            }

        }

        // printing List of Laporan
        [AllowAnonymous]
        public async Task<IActionResult> Print(string? kodLaporan, int? akCartaId, string? tahun)
        {
            var reportModel = await PrepareData(kodLaporan, akCartaId, tahun);
            var company = await _userServices.GetCompanyDetails();

            ViewBag.Tahun = tahun;

            if (akCartaId.HasValue)
            {
                var akCartaDetails = await _context.AkCarta
                                    .Where(j => j.Id == akCartaId)
                                    .Select(j => new { j.Kod, j.Perihal })
                                    .FirstOrDefaultAsync();

                if (akCartaDetails != null)
                {
                    ViewBag.SelectedKod = akCartaDetails.Kod;
                    ViewBag.SelectedPerihal = akCartaDetails.Perihal;
                }
            }

            if (kodLaporan == "LAK00201")
            {
                reportModel = await PrepareData(kodLaporan, akCartaId, tahun);

                return new ViewAsPdf("LAK00201PDF", reportModel, new ViewDataDictionary(ViewData)
                {
                    { "NamaSyarikat", company.NamaSyarikat },
                    { "AlamatSyarikat1", company.AlamatSyarikat1 },
                    { "AlamatSyarikat2", company.AlamatSyarikat2 },
                    { "AlamatSyarikat3", company.AlamatSyarikat3 }
                })
                {
                    PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                    PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                    CustomSwitches = "--footer-center \"[page]/[toPage]\"" +
                        " --footer-line --footer-font-size \"7\" --footer-spacing 1 --footer-font-name \"Segoe UI\"",
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                };
            }
            else if (kodLaporan == "LAK00202")
            {
                reportModel = await PrepareData(kodLaporan, akCartaId, tahun);

                return new ViewAsPdf("LAK00202PDF", reportModel, new ViewDataDictionary(ViewData)
                {
                    { "NamaSyarikat", company.NamaSyarikat },
                    { "AlamatSyarikat1", company.AlamatSyarikat1 },
                    { "AlamatSyarikat2", company.AlamatSyarikat2 },
                    { "AlamatSyarikat3", company.AlamatSyarikat3 }
                })
                {
                    PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                    PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                    CustomSwitches = "--footer-center \"[page]/[toPage]\"" +
                        " --footer-line --footer-font-size \"7\" --footer-spacing 1 --footer-font-name \"Segoe UI\"",
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                };
            }
            else if (kodLaporan == "LAK00203")
            {
                reportModel = await PrepareData(kodLaporan, akCartaId, tahun);

                return new ViewAsPdf("LAK00203PDF", reportModel, new ViewDataDictionary(ViewData)
                {
                    { "NamaSyarikat", company.NamaSyarikat },
                    { "AlamatSyarikat1", company.AlamatSyarikat1 },
                    { "AlamatSyarikat2", company.AlamatSyarikat2 },
                    { "AlamatSyarikat3", company.AlamatSyarikat3 }
                })
                {
                    PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                    PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                    CustomSwitches = "--footer-center \"[page]/[toPage]\"" +
                        " --footer-line --footer-font-size \"7\" --footer-spacing 1 --footer-font-name \"Segoe UI\"",
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                };
            }
            else
            {
                return new ViewAsPdf(modul + EnJenisFail.PDF, reportModel, new ViewDataDictionary(ViewData)
                {
                    { "NamaSyarikat", company.NamaSyarikat },
                    { "AlamatSyarikat1", company.AlamatSyarikat1 },
                    { "AlamatSyarikat2", company.AlamatSyarikat2 },
                    { "AlamatSyarikat3", company.AlamatSyarikat3 }
                })
                {
                    PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                    PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                    CustomSwitches = "--footer-center \"[page]/[toPage]\"" +
                        " --footer-line --footer-font-size \"7\" --footer-spacing 1 --footer-font-name \"Segoe UI\"",
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                };
            }
        }
        // printing List of Laporan end
    }

}