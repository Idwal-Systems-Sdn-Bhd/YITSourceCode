using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT.Akaun.Models.ViewModels.Forms;
using YIT.Akaun.Models.ViewModels.Prints;
using System.Data;
using System.Net;
using YIT.__Domain.Entities._Enums;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Rotativa.AspNetCore;
using YIT.Akaun.Infrastructure;
using DocumentFormat.OpenXml.Office2010.Excel;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Models._01Jadual;
using DocumentFormat.OpenXml;
using YIT.Akaun.Views.LAK002;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using DocumentFormat.OpenXml.Wordprocessing;
using System.ComponentModel;
using Microsoft.CodeAnalysis.Elfie.Model;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Dynamic;
using MoreLinq;
using DocumentFormat.OpenXml.Drawing;
using System.Linq;

namespace YIT.Akaun.Controllers._99Laporan
{
    [Authorize]
    public class LAK002Controller : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodLCekBelumTunai;
        public const string namamodul = Modules.namaLCekBelumTunai;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly UserServices _userServices;

        public LAK002Controller(
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
            //
            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> ExportExcel(PrintFormModel model)
        {
            int akBankId = 0;
            LAK002PrintModel printModel = await PrepareData(model.kodLaporan, model.tarikhDari, model.tarikhHingga, model.susunan, EnStatusBorang.Lulus, akBankId);

            // Generate a new unique identifier against which the file can be stored
            string handle = string.Format("attachment;" + model.kodLaporan + ".xlsx;", string.IsNullOrEmpty(model.kodLaporan) ? Guid.NewGuid().ToString() : WebUtility.UrlEncode(model.kodLaporan));

            // save viewmodel into workbook
            if (model.kodLaporan == "LAK00201")
            {
                
                // construct and insert data into dataTable 
                var excelData = GenerateDataTableLAK00201(printModel);

                // insert dataTable into Workbook
                RunWorkBookLAK00201(printModel, excelData, handle);
            }
            // save viewmodel into workbook
            if (model.kodLaporan == "LAK00202")
            {
                // construct and insert data into dataTable 
                var excelData1 = GenerateDataTableLAK00202(printModel);

                // insert dataTable into Workbook
                RunWorkBookLAK00202(printModel, excelData1, handle);
            }


            return Json(new { FileGuid = handle, FileName = model.kodLaporan + ".xlsx" });
        }

        private async Task <LAK002PrintModel> PrepareData(string? kodLaporan, string? tarikhDari, string? tarikhHingga, string? susunan, EnStatusBorang enStatusBorang, int akBankId)
        {
            LAK002PrintModel reportModel = new LAK002PrintModel();
           

            if (kodLaporan == "LAK00201")
            {

                reportModel.Tajuk1 = $"Laporan Cek Yang Belum Ditunai Dari {@Convert.ToDateTime(tarikhDari).ToString("dd/MM/yyyy")} Hingga {@Convert.ToDateTime(tarikhHingga):dd/MM/yyyy}";
            }
            if (kodLaporan == "LAK00202")
            {

                reportModel.Tajuk1 = $"Laporan Cek Yang Dibatalkan Dari {@Convert.ToDateTime(tarikhDari).ToString("dd/MM/yyyy")} Hingga {@Convert.ToDateTime(tarikhHingga):dd/MM/yyyy}";
            }

            var user = await _userManager.GetUserAsync(User);
            var namaUser = await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.Email == user!.Email);

            reportModel.Username = namaUser?.Nama;

            reportModel.KodLaporan = kodLaporan;
            CompanyDetails company = new CompanyDetails();
            reportModel.CompanyDetails = company;

            DateTime? date1 = null;
            DateTime? date2 = null;

            if (!string.IsNullOrEmpty(tarikhDari) && !string.IsNullOrEmpty(tarikhHingga))
            {
                date1 = DateTime.Parse(tarikhDari);
                date2 = DateTime.Parse(tarikhHingga);
            }

            reportModel.AkPV = _unitOfWork.AkPVRepo.GetResults("", date1, date2, susunan, enStatusBorang, null);


            return reportModel;
        }

        private DataTable GenerateDataTableLAK00201(LAK002PrintModel printModel)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Laporan Cek Yang Belum Ditunai";
            dt.Columns.Add("Bil", typeof(int));
            //dt.Columns.Add("No Baucer", typeof(string));
            dt.Columns.Add("No Cek", typeof(string));
            dt.Columns.Add("Tarikh Cek", typeof(DateTime));
            dt.Columns.Add("Amaun RM", typeof(decimal));
            //dt.Columns.Add("Jumlah Mengikut Tarikh", typeof(decimal));

            if (printModel.AkPV != null)
            {
                var bil = 1;
                var groupedAkPVPenerima = printModel.AkPV
                    .SelectMany(akPV => akPV.AkPVPenerima)
                    .Where(akPVPenerima => akPVPenerima.JCaraBayarId == 2 && akPVPenerima.IsCekDitunaikan != true) //2= Cek
                    .GroupBy(akPVPenerima => akPVPenerima.TarikhCaraBayar)
                    .Select(group => new
                    {
                        Date = group.Key,
                        AkPvPenerima = group.ToList(),
                        TotalAmountByDate = group.Sum(akPVPenerima => akPVPenerima.Amaun)
                    })
                    .OrderBy(group => group.Date); 

                decimal Jumlah = 0; 

                foreach (var group in groupedAkPVPenerima)
                {
                    foreach (var akPVPenerima in group.AkPvPenerima)
                    {
                        dt.Rows.Add(bil,
                           //akPVPenerima.AkPV?.NoRujukan,
                           akPVPenerima.NoRujukanCaraBayar,
                           akPVPenerima.TarikhCaraBayar,
                           akPVPenerima.Amaun
                        //DBNull.Value // Placeholder for detail rows; summary will be added after each group
                        );
                        bil++;
                    }

                    // After adding all detail rows for a group, add a summary row
                    var summaryRow = dt.NewRow();
                    summaryRow["No Cek"] = "Jumlah Pada " + ((DateTime)group.Date).ToString("dd/MM/yyyy");
                    summaryRow["Amaun RM"] = group.TotalAmountByDate;
                    dt.Rows.Add(summaryRow);

                    Jumlah += group.TotalAmountByDate; // Add the group total to the grand total
                }

                // Add a row for the grand total
                var grandTotalRow = dt.NewRow();
                grandTotalRow["No Cek"] = "JUMLAH";
                grandTotalRow["Amaun RM"] = Jumlah;
                dt.Rows.Add(grandTotalRow);
            }

            return dt;
        }
        private void RunWorkBookLAK00201(LAK002PrintModel printModel, DataTable excelData, string handle)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Laporan Cek Yang Belum Ditunai");
                ws.Cell("A1").Value = printModel.CompanyDetails?.NamaSyarikat;
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A2").Value = printModel.Tajuk1;
                ws.Cell("A3").Value = printModel.Tajuk2;

                ws.ColumnWidth = 5;
                ws.Cell("A5").InsertTable(excelData)
                    .Theme = XLTableTheme.TableStyleMedium1;

                ws.Column(2)
                   .Style.DateFormat.Format = "dd/MM/yyyy hh:mm:ss";
                ws.Column(2).AdjustToContents();
                ws.Column(3).AdjustToContents();
                ws.Column(4).AdjustToContents();
                ws.Column(5)
                    .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(5).AdjustToContents();



                foreach (var row in ws.RowsUsed())
                {
                    if (row.Cell(2).Value.ToString().StartsWith("Jumlah Pada"))
                    {
                        // Change background color
                        row.Cells(1, 4).Style.Fill.BackgroundColor = XLColor.LightBlue;

                        // Make the row bold
                        row.Cells(1, 4).Style.Font.Bold = true;
                    }
                }
              

                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveAs(ms);

                    //This is an equivalent to tempdata, but requires manual cleanup
                    _cache.Set(handle, ms.ToArray(),
                                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));


                }
            }
        }

        private DataTable GenerateDataTableLAK00202(LAK002PrintModel printModel)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Laporan Cek Yang Dibatalkan";
            dt.Columns.Add("Bil", typeof(int));
            dt.Columns.Add("No Cek", typeof(string));
            dt.Columns.Add("Tarikh Cek", typeof(DateTime));
            dt.Columns.Add("No Baucer", typeof(string));
            dt.Columns.Add("No Jurnal", typeof(string));
            dt.Columns.Add("Amaun Cek", typeof(decimal));
            dt.Columns.Add("Nama Penerima", typeof(string));
            //dt.Columns.Add("No Cek Ganti", typeof(string));
            //dt.Columns.Add("Tarikh Cek Ganti", typeof(DateTime));
            //dt.Columns.Add("No Baucer", typeof(string));
            //dt.Columns.Add("Nama Penerima", typeof(string));
            //dt.Columns.Add("Amaun Cek Ganti", typeof(decimal));
            dt.Columns.Add("Catatan", typeof(string));

            decimal TotalJumlah = 0;

            if (printModel.AkPV != null)
            {
                var bil = 1;


                foreach (var akPV in printModel.AkPV)
                {
                    
                    foreach (var akPVPenerima in akPV.AkPVPenerima.Where(akPVPenerima => akPVPenerima.JCaraBayarId == 2 && akPVPenerima.EnStatusEFT == EnStatusProses.Fail)) //2 = Cek
                    {
                        decimal Jumlah = 0;
                        // Check if there are any AkJurnalPenerimaCekBatal items
                        if (akPV.AkJurnalPenerimaCekBatal != null && akPV.AkJurnalPenerimaCekBatal.Any())
                        {
                            foreach (var akJurnal in akPV.AkJurnalPenerimaCekBatal)
                            {
                                // Now, for each akJurnal, you add a new row. Adjust the data accordingly
                                dt.Rows.Add(bil,
                                    akPVPenerima.NoRujukanCaraBayar,
                                    akPVPenerima.TarikhCaraBayar,
                                    akPV.NoRujukan,
                                    akJurnal.AkJurnal?.NoRujukan, // Using individual NoRujukan from akJurnal
                                    akPVPenerima.Amaun,
                                    akPVPenerima.NamaPenerima,
                                    akPVPenerima.Catatan
                                );

                                bil++;
                                Jumlah += akPVPenerima.Amaun;
                            }
                        }
                        else
                        {
                            // If there are no AkJurnalPenerimaCekBatal items, add a row with some default or empty value for that column
                            dt.Rows.Add(bil,
                                akPVPenerima.NoRujukanCaraBayar,
                                akPVPenerima.TarikhCaraBayar,
                                akPV.NoRujukan,
                                "",
                                akPVPenerima.Amaun,
                                akPVPenerima.NamaPenerima,
                                akPVPenerima.Catatan
                            );

                            bil++;
                            Jumlah += akPVPenerima.Amaun;
                        }
                        TotalJumlah += Jumlah;

                    }
                }
                // Add a row for the grand total
                var grandTotalRow = dt.NewRow();
                grandTotalRow["No Cek"] = "JUMLAH RM";
                grandTotalRow["Amaun Cek"] = TotalJumlah;
                dt.Rows.Add(grandTotalRow);

                DataView dv = new DataView(dt);
                dv.Sort = "No Cek";
                dt = dv.ToTable();
            }
            return dt;
        }
        private void RunWorkBookLAK00202(LAK002PrintModel printModel, DataTable excelData1, string handle)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Laporan Cek Yang Dibatalkan");
                ws.Cell("A1").Value = printModel.CompanyDetails?.NamaSyarikat;
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A2").Value = printModel.Tajuk1;
                ws.Cell("A3").Value = printModel.Tajuk2;

                ws.ColumnWidth = 5;
                ws.Cell("A5").InsertTable(excelData1)
                    .Theme = XLTableTheme.TableStyleMedium1;

                ws.Column(2)
                   .Style.DateFormat.Format = "dd/MM/yyyy hh:mm:ss";
                ws.Column(2).AdjustToContents();
                ws.Column(3).AdjustToContents();
                ws.Column(4).AdjustToContents();
                ws.Column(5)
                    .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(5).AdjustToContents();
                ws.Column(6).AdjustToContents();
                ws.Column(7).AdjustToContents();



                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveAs(ms);

                    //This is an equivalent to tempdata, but requires manual cleanup
                    _cache.Set(handle, ms.ToArray(),
                                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));


                }
            }
        }

        // printing List of Laporan
        [AllowAnonymous]
        public async Task<IActionResult> Print(string? kodLaporan, string? tarikhDari, string? tarikhHingga, string? susunan, EnStatusBorang enStatusBorang, int akBankId)
        { 
            var akpv = new List<AkPVPenerima>();
            var reportModel = await PrepareData(kodLaporan, tarikhDari, tarikhHingga, susunan, EnStatusBorang.Lulus, akBankId);
            var company = await _userServices.GetCompanyDetails();

            if (kodLaporan == "LAK00201")
            {
                var TarikhDari = DateTime.Parse(tarikhDari).ToString("dd/MM/yyyy");
                var TarikhHingga = DateTime.Parse(tarikhHingga).ToString("dd/MM/yyyy");
                await PrepareData(kodLaporan, tarikhDari, tarikhHingga, susunan, EnStatusBorang.Lulus, akBankId);


                akpv = await _unitOfWork.AkPVRepo.GetResultsGroupByTarikhCaraBayar(tarikhDari, tarikhHingga);

                dynamic dyModel = new ExpandoObject();
                dyModel.ReportModel = akpv;
                dyModel.reportModelGrouped = akpv.GroupBy(b => b.TarikhCaraBayar);
                

                return new ViewAsPdf("LAK00201PDF", dyModel, new ViewDataDictionary(ViewData) {
                    { "NamaSyarikat", company.NamaSyarikat },
                    { "AlamatSyarikat1", company.AlamatSyarikat1 },
                    { "AlamatSyarikat2", company.AlamatSyarikat2 },
                    { "AlamatSyarikat3", company.AlamatSyarikat3 },
                    { "TarikhDari", TarikhDari },
                    { "TarikhHingga", TarikhHingga },
                })
                {
                    PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                    PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                    CustomSwitches = "--footer-center \"[page]/[toPage]\"" +
                        " --footer-line --footer-font-size \"7\" --footer-spacing 1 --footer-font-name \"Segoe UI\"",
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                };
            }
            if (kodLaporan == "LAK00202")
            {
                
                var TarikhDari = DateTime.Parse(tarikhDari).ToString("dd/MM/yyyy");
                var TarikhHingga = DateTime.Parse(tarikhHingga).ToString("dd/MM/yyyy");

                var akpv1 = new List<AkPV>();
                var reportModel1 = await PrepareData(kodLaporan, tarikhDari, tarikhHingga, susunan, EnStatusBorang.Lulus, akBankId);
                var company1 = await _userServices.GetCompanyDetails();

                if (tarikhDari != null && tarikhHingga != null)
                {
                    List<AkPV> akPv1 = await _context.AkPV
                    .Include(b => b.AkPVPenerima)
                    .Where(b => b.Tarikh >= DateTime.Parse(tarikhDari) && b.Tarikh <= DateTime.Parse(tarikhHingga))
                    .ToListAsync();
                }

                return new ViewAsPdf("LAK00202PDF", reportModel1, new ViewDataDictionary(ViewData) {
                    { "NamaSyarikat", company.NamaSyarikat },
                    { "AlamatSyarikat1", company.AlamatSyarikat1 },
                    { "AlamatSyarikat2", company.AlamatSyarikat2 },
                    { "AlamatSyarikat3", company.AlamatSyarikat3 },
                    { "TarikhDari", TarikhDari },
                    { "TarikhHingga", TarikhHingga },
                })
                {
                    PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                    PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                    CustomSwitches = "--footer-center \"[page]/[toPage]\"" +
                        " --footer-line --footer-font-size \"7\" --footer-spacing 1 --footer-font-name \"Segoe UI\"",
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                };

            }
         
            //string customSwitches = "--page-offset 0 --footer-center [page] / [toPage] --footer-font-size 6";

            return new ViewAsPdf(modul + EnJenisFail.PDF, akpv,
                new ViewDataDictionary(ViewData) {
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
        // printing List of Laporan end
      
    }
}
