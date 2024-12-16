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
    public class LAK017Controller : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodLAkaunKenaBayar;
        public const string namamodul = Modules.namaLAkaunKenaBayar;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly UserServices _userServices;

        public LAK017Controller(
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
            PopulateSelectList(model.jKWId);
            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> ExportExcel(PrintFormModel model)
        {
            LAK017PrintModel printModel = await PrepareData(model.kodLaporan, model.tarikhDari, model.tarikhHingga, model.enStatusBorang, model.tahun, model.jKWId);

            // Generate a new unique identifier against which the file can be stored
            string handle = string.Format("attachment;" + model.kodLaporan + ".xlsx;", string.IsNullOrEmpty(model.kodLaporan) ? Guid.NewGuid().ToString() : WebUtility.UrlEncode(model.kodLaporan));

            // save viewmodel into workbook
            if (model.kodLaporan == "LAK00201")
            {
                // construct and insert data into dataTable 
                var excelData = GenerateDataTableLAK00201(printModel, model.tahun, model.tarikhDari, model.tarikhHingga, model.jKWId);

                // insert dataTable into Workbook
                RunWorkBookLAK00201(printModel, excelData, handle);
            }
            // save viewmodel into workbook
            else if (model.kodLaporan == "LAK00202")
            {
                //construct and insert data into dataTable
                var excelData = GenerateDataTableLAK00202(printModel, model.tahun, model.tarikhDari, model.tarikhHingga, model.jKWId);

                //insert dataTable into Workbook
                RunWorkBookLAK00202(printModel, excelData, handle);
            }

            return Json(new { FileGuid = handle, FileName = model.kodLaporan + ".xlsx" });
        }

        private async Task<LAK017PrintModel> PrepareData(string? kodLaporan, string? tarikhDari, string? tarikhHingga, EnStatusBorang enStatusBorang, string? tahun, int? jKWId)
        {
            LAK017PrintModel reportModel = new LAK017PrintModel();

            var user = await _userManager.GetUserAsync(User);
            var namaUser = await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.Email == user!.Email);

            reportModel.CommonModels.Username = namaUser?.Nama;

            reportModel.CommonModels.KodLaporan = kodLaporan;
            CompanyDetails company = new CompanyDetails();
            reportModel.CommonModels.CompanyDetails = company;

            DateTime? date1 = null;
            DateTime? date2 = null;

            if (!string.IsNullOrEmpty(tarikhDari) && !string.IsNullOrEmpty(tarikhHingga))
            {
                date1 = DateTime.Parse(tarikhDari);
                date2 = DateTime.Parse(tarikhHingga);
            }

            string? jKWKod = "";
            string? jKWPerihal = "";

            if (jKWId.HasValue)
            {
                var jKWDetails = await _context.JKW
                                       .Where(j => j.Id == jKWId)
                                       .Select(j => new { j.Kod, j.Perihal })
                                       .FirstOrDefaultAsync();

                if (jKWDetails != null)
                {
                    jKWKod = jKWDetails.Kod;
                    jKWPerihal = jKWDetails.Perihal;
                }
            }

            if (kodLaporan == "LAK00201")
            {
                reportModel.CommonModels.Tajuk1 = $"Laporan Akaun Kena Bayar Untuk Tarikh {Convert.ToDateTime(tarikhDari):dd/MM/yyyy} Hingga {Convert.ToDateTime(tarikhHingga):dd/MM/yyyy} Bagi Tahun {tahun}";
                reportModel.CommonModels.Tajuk2 = $"JKW: {jKWKod} - {jKWPerihal}";
                reportModel.AkJurnal = _unitOfWork.AkJurnalRepo.GetResults1("", date1, date2, null, EnStatusBorang.Semua, tahun, jKWId);
            }
            else if (kodLaporan == "LAK00202")
            {
                reportModel.CommonModels.Tajuk1 = $"Laporan Akaun Kena Bayar Untuk Tarikh {Convert.ToDateTime(tarikhDari):dd/MM/yyyy} Hingga {Convert.ToDateTime(tarikhHingga):dd/MM/yyyy} Bagi Tahun {tahun}";
                reportModel.CommonModels.Tajuk2 = $"JKW: {jKWKod} - {jKWPerihal}";
                reportModel.AkJurnal = _unitOfWork.AkJurnalRepo.GetResults1("", date1, date2, null, EnStatusBorang.Semua, tahun, jKWId);
            }

            return reportModel;
        }

        private DataTable GenerateDataTableLAK00201(LAK017PrintModel printModel, string? tahun, string? tarikhDari, string? tarikhHingga, int? jKWId)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Laporan AKB Dengan Tanggungan";
            dt.Columns.Add("Bil", typeof(int));
            dt.Columns.Add("Tarikh", typeof(DateTime));
            dt.Columns.Add("No Rujukan", typeof(string));
            dt.Columns.Add("Kod", typeof(string));
            dt.Columns.Add("Nama Akaun", typeof(string));
            dt.Columns.Add("Amaun RM", typeof(decimal));

            DateTime date1 = DateTime.MinValue;
            DateTime date2 = DateTime.MaxValue;

            if (!string.IsNullOrEmpty(tarikhDari) && !string.IsNullOrEmpty(tarikhHingga))
            {
                date1 = DateTime.Parse(tarikhDari).Date;
                date2 = DateTime.Parse(tarikhHingga).Date.AddDays(1).AddTicks(-1);
            }

            decimal grandTotalJumlah = 0;

            if (printModel.AkJurnal != null & !string.IsNullOrEmpty(tahun))
            {
                int year = int.Parse(tahun!);

                var bil = 1;

                var akJurnals = _context.AkJurnal
                    .Where(j => j.Tarikh >= date1 && j.Tarikh <= date2 && j.JKWId == jKWId)
                    .OrderBy(j => j.NoRujukan)
                    .ToList();

                var akJurnalObjekList = akJurnals
                    .SelectMany(j => j.AkJurnalObjek!)
                    .Where(jo =>
                        jo.AkJurnal != null &&
                        (
                            jo.AkJurnal.AkPVId == null ||
                            (
                                jo.AkJurnal.AkPV != null &&
                                jo.AkJurnal.AkPV.AkPVInvois != null &&
                                jo.AkJurnal.AkPV.AkPVInvois
                                    .Any(inv =>
                                        inv.AkBelian != null &&
                                        inv.AkBelian.AkPO != null &&
                                        !string.IsNullOrEmpty(inv.AkBelian.AkPO.NoRujukanLama) &&
                                        inv.AkBelian.AkPO.NoRujukanLama.Substring(0, 8) == inv.AkBelian.AkPO.NoRujukanLama.Substring(0, 8)
                                    )
                            )
                        ) &&
                        jo.AkJurnal.AkPV?.AkPVInvois != null &&
                        jo.AkJurnal.AkPV.AkPVInvois
                            .Any(inv => inv.AkBelian?.AkPO != null)
                    )
                    .ToList();

                var groupedResults = akJurnalObjekList
                    .GroupBy(obj => new
                    {
                        NoRujukan = obj.AkJurnal!.NoRujukan,
                        JKWId = obj.AkJurnal!.JKWId,
                        Kod = obj.AkCartaDebit?.Kod ?? obj.AkCartaKredit?.Kod,
                        Perihal = obj.AkCartaDebit?.Perihal ?? obj.AkCartaKredit?.Perihal
                    })
                    .Select(group => new
                    {
                        group.Key.NoRujukan,
                        group.Key.JKWId,
                        group.Key.Kod,
                        group.Key.Perihal,
                        Amaun = group.Sum(o => o.Amaun),
                        Tarikh = group.FirstOrDefault()!.AkJurnal!.Tarikh
                    })
                    .ToList();

                foreach (var group in groupedResults)
                {
                    string? AkCartaKod = group.Kod;
                    string? cleanedAkCartaPerihal = group.Perihal?.Trim();
                    decimal Amaun = group.Amaun;

                    dt.Rows.Add(
                        bil,
                        group.Tarikh,
                        group.NoRujukan,
                        AkCartaKod,
                        cleanedAkCartaPerihal,
                        Amaun
                    );

                    bil++;
                    grandTotalJumlah += Amaun;
                }

                var grandTotalRow = dt.NewRow();
                grandTotalRow["Nama Akaun"] = "JUMLAH RM";
                grandTotalRow["Amaun RM"] = grandTotalJumlah;

                dt.Rows.Add(grandTotalRow);
            }

            return dt;
        }

        private void RunWorkBookLAK00201(LAK017PrintModel printModel, DataTable excelData, string handle)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Laporan AKB Dengan Tanggungan");
                ws.Cell("A1").Value = printModel.CommonModels.CompanyDetails?.NamaSyarikat;
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A2").Value = printModel.CommonModels.Tajuk1;
                ws.Cell("A3").Value = printModel.CommonModels.Tajuk2;

                ws.ColumnWidth = 5;
                ws.Cell("A5").InsertTable(excelData)
                    .Theme = XLTableTheme.TableStyleMedium1;

                ws.Column(2).AdjustToContents();
                ws.Column(3).AdjustToContents();
                ws.Column(4).AdjustToContents();
                ws.Column(5).AdjustToContents();
                ws.Column(6).AdjustToContents();
                ws.Column(6)
                   .Style.NumberFormat.Format = " #,##0.00";

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

        private DataTable GenerateDataTableLAK00202(LAK017PrintModel printModel, string? tahun, string? tarikhDari, string? tarikhHingga, int? jKWId)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Laporan AKB Tanpa Tanggungan";
            dt.Columns.Add("Bil", typeof(int));
            dt.Columns.Add("Tarikh", typeof(DateTime));
            dt.Columns.Add("No Rujukan", typeof(string));
            dt.Columns.Add("Kod", typeof(string));
            dt.Columns.Add("Nama Akaun", typeof(string));
            dt.Columns.Add("Amaun RM", typeof(decimal));

            DateTime date1 = DateTime.MinValue;
            DateTime date2 = DateTime.MaxValue;

            if (!string.IsNullOrEmpty(tarikhDari) && !string.IsNullOrEmpty(tarikhHingga))
            {
                date1 = DateTime.Parse(tarikhDari).Date;
                date2 = DateTime.Parse(tarikhHingga).Date.AddDays(1).AddTicks(-1);
            }

            decimal grandTotalJumlah = 0;

            if (printModel.AkJurnal != null & !string.IsNullOrEmpty(tahun))
            {
                int year = int.Parse(tahun!);
                
                var bil = 1;

                var akJurnal = _context.AkJurnal
                 .Include(b => b.AkJurnalObjek)!
                     .ThenInclude(b => b.AkCartaDebit)
                 .Include(b => b.AkJurnalObjek)!
                     .ThenInclude(b => b.AkCartaKredit)
                 .Where(b => b.Tarikh.Year == year && b.Tarikh >= date1 && b.Tarikh <= date2 && b.JKWId == jKWId && b.IsAKB == false &&
                     !b.AkJurnalObjek!.Any(jo =>
                     (jo.AkCartaDebit != null && jo.AkCartaDebit.Kod == "L14101") || (jo.AkCartaKredit != null && jo.AkCartaKredit.Kod == "L14101")
                     ))
                 .OrderBy(b => b.NoRujukan)
                 .ToList();

                foreach (var akJ in akJurnal)
                {
                    foreach (var jo in akJ.AkJurnalObjek!)
                    {
                        string? AkCartaKod = jo.AkCartaDebit?.Kod ?? jo.AkCartaKredit?.Kod;
                        string? cleanedAkCartaPerihal = jo.AkCartaDebit?.Perihal!.Trim() ?? jo.AkCartaKredit?.Perihal!.Trim();
                        decimal Amaun = jo.Amaun;

                        dt.Rows.Add(
                            bil,                      
                            akJ.Tarikh,               
                            akJ.NoRujukan,            
                            AkCartaKod,
                            cleanedAkCartaPerihal,           
                            Amaun.ToString("#,##0.00")
                        );

                        bil++;
                        grandTotalJumlah += Amaun;
                    }
                }

                var grandTotalRow = dt.NewRow();
                grandTotalRow["Nama Akaun"] = "JUMLAH RM";
                grandTotalRow["Amaun RM"] = grandTotalJumlah;

                dt.Rows.Add(grandTotalRow);
            }

            return dt;
        }

        private void RunWorkBookLAK00202(LAK017PrintModel printModel, DataTable excelData, string handle)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Laporan AKB Tanpa Tanggungan");
                ws.Cell("A1").Value = printModel.CommonModels.CompanyDetails?.NamaSyarikat;
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A2").Value = printModel.CommonModels.Tajuk1;
                ws.Cell("A3").Value = printModel.CommonModels.Tajuk2;

                ws.ColumnWidth = 5;
                ws.Cell("A5").InsertTable(excelData)
                    .Theme = XLTableTheme.TableStyleMedium1;

                ws.Column(2).AdjustToContents();
                ws.Column(3).AdjustToContents();
                ws.Column(4).AdjustToContents();
                ws.Column(5).AdjustToContents();
                ws.Column(6).AdjustToContents();
                ws.Column(6)
                   .Style.NumberFormat.Format = " #,##0.00";

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

        private void PopulateSelectList(int? jKWId)
        {
            var jKWList = _unitOfWork.JKWRepo.GetAllDetails();
            var kwSelect = new List<SelectListItem>();

            if (jKWList != null && jKWList.Any())
            {
                kwSelect.AddRange(jKWList.Select(item => new SelectListItem
                {
                    Text = item.Kod + " - " + item.Perihal,
                    Value = item.Id.ToString()
                }));
            }
            else
            {
                kwSelect.Add(new SelectListItem
                {
                    Text = "-- Tiada Kumpulan Wang Berdaftar --",
                    Value = ""
                });
            }

            var selectList1 = new SelectList(kwSelect, "Value", "Text");

            if (jKWId.HasValue && jKWId.Value != 0)
            {
                var selectedItem = selectList1.FirstOrDefault(x => x.Value == jKWId.ToString());
                if (selectedItem != null)
                {
                    selectedItem.Selected = true;
                }
            }
            else if (kwSelect.Any())
            {
                kwSelect.First().Selected = true;
            }

            ViewBag.JKW = selectList1;

        }

        // printing List of Laporan
        [AllowAnonymous]
        public async Task<IActionResult> Print(string? kodLaporan, string? tarikhDari, string? tarikhHingga, string? tahun, int? jKWId)
        {
            var reportModel = await PrepareData(kodLaporan, tarikhDari, tarikhHingga, EnStatusBorang.Semua, tahun, jKWId);
            var company = await _userServices.GetCompanyDetails();

            ViewBag.Tahun = tahun;

            if (jKWId.HasValue)
            {
                var jKWDetails = await _context.JKW
                                    .Where(j => j.Id == jKWId)
                                    .Select(j => new { j.Kod, j.Perihal })
                                    .FirstOrDefaultAsync();

                if (jKWDetails != null)
                {
                    ViewBag.jKWKod = jKWDetails.Kod;
                    ViewBag.jKWPerihal = jKWDetails.Perihal;
                }
            }

            if (kodLaporan == "LAK00201")
            {
                var akjurnal = await _unitOfWork.AkJurnalRepo.GetResultsGroupWithTanggungan(tahun, tarikhDari, tarikhHingga, jKWId);

                tarikhDari = DateTime.Parse(tarikhDari!).ToString("dd/MM/yyyy");
                tarikhHingga = DateTime.Parse(tarikhHingga!).ToString("dd/MM/yyyy");

                reportModel.AkJurnalResult = akjurnal;

                return new ViewAsPdf("LAK00201PDF", reportModel, new ViewDataDictionary(ViewData)
                {
                    { "NamaSyarikat", company.NamaSyarikat },
                    { "AlamatSyarikat1", company.AlamatSyarikat1 },
                    { "AlamatSyarikat2", company.AlamatSyarikat2 },
                    { "AlamatSyarikat3", company.AlamatSyarikat3 },
                    { "TarikhDari", tarikhDari },
                    { "TarikhHingga", tarikhHingga },
                })
                {
                    PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                    PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                    CustomSwitches = "--footer-center \"[page]/[toPage]\"" +
                        " --footer-line --footer-font-size \"7\" --footer-spacing 1 --footer-font-name \"Segoe UI\"",
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                };
            }
            else if (kodLaporan == "LAK00202")
            {
                var akjurnal = await _unitOfWork.AkJurnalRepo.GetResultsGroupWithoutTanggungan(tahun, tarikhDari, tarikhHingga, jKWId);

                tarikhDari = DateTime.Parse(tarikhDari!).ToString("dd/MM/yyyy");
                tarikhHingga = DateTime.Parse(tarikhHingga!).ToString("dd/MM/yyyy");

                reportModel.AkJurnalResult = akjurnal;            

                return new ViewAsPdf("LAK00202PDF", reportModel, new ViewDataDictionary(ViewData)
                {
                    { "NamaSyarikat", company.NamaSyarikat },
                    { "AlamatSyarikat1", company.AlamatSyarikat1 },
                    { "AlamatSyarikat2", company.AlamatSyarikat2 },
                    { "AlamatSyarikat3", company.AlamatSyarikat3 },
                    { "TarikhDari", tarikhDari },
                    { "TarikhHingga", tarikhHingga },
                })
                {
                    PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                    PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
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
                    { "AlamatSyarikat3", company.AlamatSyarikat3 },
                    { "TarikhDari", tarikhDari },
                    { "TarikhHingga", tarikhHingga },
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