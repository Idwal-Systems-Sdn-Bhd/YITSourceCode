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
            if (model.Tahun1 == null)
            {
                model.Tahun1 = DateTime.Now.Year.ToString();
            }

            PopulateSelectList(model.AkCartaId, model.Tahun1);
            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> ExportExcel(PrintFormModel model)
        {
            LAK015PrintModel printModel = await PrepareData(model.kodLaporan, model.AkCartaId, model.AkCartaId1, model.Tahun1);

            // Generate a new unique identifier against which the file can be stored
            string handle = string.Format("attachment;" + model.kodLaporan + ".xlsx;", string.IsNullOrEmpty(model.kodLaporan) ? Guid.NewGuid().ToString() : WebUtility.UrlEncode(model.kodLaporan));

            // save viewmodel into workbook
            if (model.kodLaporan == "LAK01501")
            {
                // construct and insert data into dataTable 
                var excelData = await GenerateDataTableLAK01501(printModel, model.AkCartaId, model.AkCartaId1, model.Tahun1);

                // insert dataTable into Workbook
                RunWorkBookLAK01501(printModel, excelData, handle);
            }
            // save viewmodel into workbook
            else if (model.kodLaporan == "LAK01502")
            {
                //construct and insert data into dataTable
                var excelData = await GenerateDataTableLAK01502(printModel, model.AkCartaId, model.AkCartaId1, model.Tahun1);

                //insert dataTable into Workbook
                RunWorkBookLAK01502(printModel, excelData, handle);
            }
            else if (model.kodLaporan == "LAK01503")
            {
                //construct and insert data into dataTable
                var excelData = await GenerateDataTableLAK01503(printModel, model.AkCartaId, model.AkCartaId1, model.Tahun1);

                //insert dataTable into Workbook
                RunWorkBookLAK01503(printModel, excelData, handle);
            }

            return Json(new { FileGuid = handle, FileName = model.kodLaporan + ".xlsx" });
        }

        private async Task<LAK015PrintModel> PrepareData(string? kodLaporan, int? akCartaId, int? akCartaId1, string? tahun1)
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
            string? selectedKod1 = "";
            string? selectedPerihal1 = "";

            if (akCartaId.HasValue)
            {
                var akCartaDetails = await _context.AkCarta
                                    .Where(j => j.Id == akCartaId)
                                    .Select(j => new { j.Kod, j.Perihal })
                                    .FirstOrDefaultAsync();

                if (akCartaDetails != null)
                {
                    selectedKod = akCartaDetails.Kod;
                    selectedPerihal = akCartaDetails?.Perihal?.Trim() ?? String.Empty;
                }
            }

            if (akCartaId1.HasValue)
            {
                var akCartaDetails1 = await _context.AkCarta
                                      .Where(j => j.Id == akCartaId1)
                                      .Select(j => new { j.Kod, j.Perihal })
                                      .FirstOrDefaultAsync();

                if (akCartaDetails1 != null)
                {
                    selectedKod1 = akCartaDetails1.Kod;
                    selectedPerihal1 = akCartaDetails1?.Perihal?.Trim() ?? string.Empty;
                }
            }

            var akCartaList = await _unitOfWork.AkCartaRepo.GetResults(akCartaId, akCartaId1, tahun1);
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

            if (kodLaporan == "LAK01501")
            {
                reportModel.CommonModels.Tajuk1 = $"Laporan SiBerhutang Pada Tahun {tahun1} Mengikut Kod Akaun {selectedKod} - {selectedPerihal} Hingga {selectedKod1} - {selectedPerihal1}";

                reportModel.JumlahBaki = akCartaList.Sum(r => r.BakiAwal); 
                reportModel.JumlahAkhir = akCartaList.Sum(r => r.Jumlah);
                reportModel.Jumlah1 = akCartaList.Sum(r => r.BakiAwal + r.Jan + r.Feb + r.Mac + r.Apr + r.Mei + r.Jun + r.Jul + r.Ogo + r.Sep + r.Okt + r.Nov + r.Dis); 
            }
            else if (kodLaporan == "LAK01502")
            {
                reportModel.CommonModels.Tajuk1 = $"Laporan SiBerhutang Setengah Tahun Pertama Pada Tahun {tahun1} Mengikut Kod Akaun {selectedKod} - {selectedPerihal} Hingga {selectedKod1} - {selectedPerihal1} ";

                reportModel.JumlahBaki = akCartaList.Sum(r => r.BakiAwal); 
                reportModel.JumlahAkhir = akCartaList.Sum(r => r.JumlahH1);
                reportModel.Jumlah1 = akCartaList.Sum(r => r.BakiAwal + r.Jan + r.Feb + r.Mac + r.Apr + r.Mei + r.Jun); 
            }
            else if (kodLaporan == "LAK01503")
            {
                reportModel.CommonModels.Tajuk1 = $"Laporan Siberhutang Setengah Tahun Kedua Pada Tahun {tahun1} Mengikut Kod Akaun {selectedKod} - {selectedPerihal} Hingga {selectedKod1} - {selectedPerihal1} ";

                reportModel.JumlahBaki = akCartaList.Sum(r => r.BakiAwalH2);  
                reportModel.JumlahAkhir = akCartaList.Sum(r => r.JumlahH2);
                reportModel.Jumlah1 = akCartaList.Sum(r => r.BakiAwalH2 + r.Jul + r.Ogo + r.Sep + r.Okt + r.Nov + r.Dis); 
            }

            return reportModel;
        }

        private async Task<DataTable> GenerateDataTableLAK01501(LAK015PrintModel printModel, int? akCartaId, int? akCartaId1, string? tahun1)
        {
            var akCartaList = await _unitOfWork.AkCartaRepo.GetResults(akCartaId, akCartaId1, tahun1);

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

            decimal totalBakiAwal = 0, totalJumlah = 0, totalBakiAkhir = 0;
            decimal totalJan = 0, totalFeb = 0, totalMac = 0, totalApr = 0, totalMei = 0, totalJun = 0;
            decimal totalJul = 0, totalOgo = 0, totalSep = 0, totalOkt = 0, totalNov = 0, totalDis = 0;

            foreach (var akCarta in akCartaList)
            {
                decimal bakiAwal = akCarta.BakiAwal;
                decimal jan = akCarta.Jan, feb = akCarta.Feb, mac = akCarta.Mac, apr = akCarta.Apr, mei = akCarta.Mei, jun = akCarta.Jun;
                decimal jul = akCarta.Jul, ogo = akCarta.Ogo, sep = akCarta.Sep, okt = akCarta.Okt, nov = akCarta.Nov, dis = akCarta.Dis;
                decimal jumlah = jan + feb + mac + apr + mei + jun + jul + ogo + sep + okt + nov + dis;
                decimal bakiAkhir = bakiAwal + jumlah;

                totalBakiAwal += bakiAwal;
                totalJan += jan; totalFeb += feb; totalMac += mac; totalApr += apr;
                totalMei += mei; totalJun += jun; totalJul += jul; totalOgo += ogo;
                totalSep += sep; totalOkt += okt; totalNov += nov; totalDis += dis;
                totalJumlah += jumlah;
                totalBakiAkhir += bakiAkhir;

                dt.Rows.Add(
                    akCarta?.Kod?.Trim(),
                    akCarta?.Perihal?.Trim(),
                    totalBakiAwal,
                    jan, feb, mac, apr, mei, jun, jul, ogo, sep, okt, nov, dis,
                    totalJumlah,
                    totalBakiAkhir
                );
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

            return dt;
        }

        private void RunWorkBookLAK01501(LAK015PrintModel printModel, DataTable excelData, string handle)
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

        private async Task<DataTable> GenerateDataTableLAK01502(LAK015PrintModel printModel, int? akCartaId, int? akCartaId1, string? tahun1)
        {
            var akCartaList = await _unitOfWork.AkCartaRepo.GetResults(akCartaId, akCartaId1, tahun1);

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

            decimal totalBakiAwal = 0;
            decimal totalJan = 0, totalFeb = 0, totalMac = 0, totalApr = 0;
            decimal totalMei = 0, totalJun = 0;
            decimal totalJumlah = 0;
            decimal totalBakiAkhir = 0;

            foreach (var akCarta in akCartaList)
            {
                decimal bakiAwal = akCarta.BakiAwal;
                decimal jan = akCarta.Jan, feb = akCarta.Feb, mac = akCarta.Mac, apr = akCarta.Apr, mei = akCarta.Mei, jun = akCarta.Jun;
                decimal jumlah = jan + feb + mac + apr + mei + jun;
                decimal bakiAkhir = bakiAwal + jumlah;

                dt.Rows.Add(
                akCarta?.Kod?.Trim(),
                akCarta?.Perihal?.Trim(),
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

            return dt;
        }

        private void RunWorkBookLAK01502(LAK015PrintModel printModel, DataTable excelData, string handle)
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

        private async Task <DataTable> GenerateDataTableLAK01503(LAK015PrintModel printModel, int? akCartaId, int? akCartaId1, string? tahun1)
        {
            var akCartaList = await _unitOfWork.AkCartaRepo.GetResults(akCartaId, akCartaId1, tahun1);

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

            decimal totalBakiAwal = 0;
            decimal totalJul = 0, totalOgo = 0;
            decimal totalSep = 0, totalOkt = 0, totalNov = 0, totalDis = 0;
            decimal totalJumlah = 0;
            decimal totalBakiAkhir = 0;

            foreach (var akCarta in akCartaList)
            {
                decimal bakiAwal = akCarta.BakiAwal;
                decimal jul = akCarta.Jul, ogo = akCarta.Ogo, sep = akCarta.Sep, okt = akCarta.Okt, nov = akCarta.Nov, dis = akCarta.Dis;
                decimal jumlah = jul + ogo + sep + okt + nov + dis;
                decimal bakiAkhir = bakiAwal + jumlah;

                dt.Rows.Add(
                    akCarta?.Kod?.Trim(),
                    akCarta?.Perihal?.Trim(),
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

            return dt;
        }

        private void RunWorkBookLAK01503(LAK015PrintModel printModel, DataTable excelData, string handle)
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

        private void PopulateSelectList(int? akCartaId, string? tahun1)
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

            if (String.IsNullOrWhiteSpace(tahun1))
            {
                ViewData["Tahun1"] = DateTime.Now.Year.ToString();
            }
            else
            {
                ViewData["Tahun1"] = tahun1;
            }

        }

        // printing List of Laporan
        [AllowAnonymous]
        public async Task<IActionResult> Print(string? kodLaporan, int? akCartaId, int? akCartaId1, string? tahun1)
        {
            var reportModel = await PrepareData(kodLaporan, akCartaId, akCartaId1, tahun1);
            var company = await _userServices.GetCompanyDetails();

            ViewBag.Tahun = tahun1;

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

            if (akCartaId1.HasValue)
            {
                var akCartaDetails1 = await _context.AkCarta
                                      .Where(j => j.Id == akCartaId1)
                                      .Select(j => new { j.Kod, j.Perihal })
                                      .FirstOrDefaultAsync();

                if (akCartaDetails1 != null)
                {
                    ViewBag.SelectedKod1 = akCartaDetails1.Kod;
                    ViewBag.SelectedPerihal1 = akCartaDetails1.Perihal;
                }
            }

            if (kodLaporan == "LAK01501")
            {
                reportModel = await PrepareData(kodLaporan, akCartaId, akCartaId1, tahun1);

                return new ViewAsPdf("LAK01501PDF", reportModel, new ViewDataDictionary(ViewData)
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
            else if (kodLaporan == "LAK01502")
            {
                reportModel = await PrepareData(kodLaporan, akCartaId, akCartaId1, tahun1);

                return new ViewAsPdf("LAK01502PDF", reportModel, new ViewDataDictionary(ViewData)
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
            else if (kodLaporan == "LAK01503")
            {
                reportModel = await PrepareData(kodLaporan, akCartaId, akCartaId1, tahun1);

                return new ViewAsPdf("LAK01503PDF", reportModel, new ViewDataDictionary(ViewData)
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