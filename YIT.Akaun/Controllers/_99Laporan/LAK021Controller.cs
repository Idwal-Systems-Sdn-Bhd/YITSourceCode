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
    public class LAK021Controller : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodLPerolehanBelumBayarBatal;
        public const string namamodul = Modules.namaLPerolehanBelumBayarBatal;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly UserServices _userServices;

        public LAK021Controller(
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
            LAK021PrintModel printModel = await PrepareData(model.kodLaporan, model.tarikhDari, model.tarikhHingga, model.enStatusBorang, model.jKWId);

            // Generate a new unique identifier against which the file can be stored
            string handle = string.Format("attachment;" + model.kodLaporan + ".xlsx;", string.IsNullOrEmpty(model.kodLaporan) ? Guid.NewGuid().ToString() : WebUtility.UrlEncode(model.kodLaporan));

            // save viewmodel into workbook
            if (model.kodLaporan == "LAK00201")
            {
                // construct and insert data into dataTable 
                var excelData = GenerateDataTableLAK00201(printModel, model.tarikhDari, model.tarikhHingga, model.enStatusBorang, model.jKWId);

                // insert dataTable into Workbook
                RunWorkBookLAK00201(printModel, excelData, handle);
            }
            // save viewmodel into workbook
            else if (model.kodLaporan == "LAK00202")
            {
                //construct and insert data into dataTable 
                var excelData = GenerateDataTableLAK00202(printModel, model.tarikhDari, model.tarikhHingga, model.enStatusBorang, model.jKWId);

                //insert dataTable into Workbook
                RunWorkBookLAK00202(printModel, excelData, handle);
            }

            return Json(new { FileGuid = handle, FileName = model.kodLaporan + ".xlsx" });
        }

        private async Task<LAK021PrintModel> PrepareData(string? kodLaporan, string? tarikhDari, string? tarikhHingga, EnStatusBorang enStatusBorang, int? jKWId)
        {
            LAK021PrintModel reportModel = new LAK021PrintModel();

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
                reportModel.CommonModels.Tajuk1 = $"Laporan Perolehan Belum Bayar Dari {Convert.ToDateTime(tarikhDari):dd/MM/yyyy} Hingga {Convert.ToDateTime(tarikhHingga):dd/MM/yyyy}";
                reportModel.CommonModels.Tajuk2 = $"JKW: {jKWKod} - {jKWPerihal}";
                reportModel.AkPenilaianPerolehan = _unitOfWork.AkPenilaianPerolehanRepo.GetResults1("", date1, date2, null, EnStatusBorang.Semua, jKWId);
            }
            else if (kodLaporan == "LAK00202")
            {
                reportModel.CommonModels.Tajuk1 = $"Laporan Perolehan Batal Dari {Convert.ToDateTime(tarikhDari):dd/MM/yyyy} Hingga {Convert.ToDateTime(tarikhHingga):dd/MM/yyyy}";
                reportModel.CommonModels.Tajuk2 = $"JKW: {jKWKod} - {jKWPerihal}";
                reportModel.AkPenilaianPerolehan = _unitOfWork.AkPenilaianPerolehanRepo.GetResults1("", date1, date2, null, EnStatusBorang.Semua, jKWId);
            }

            reportModel.AkPenilaianPerolehan = _unitOfWork.AkPenilaianPerolehanRepo.GetResults1("", date1, date2, null, EnStatusBorang.Semua, jKWId);

            return reportModel;
        }

        private DataTable GenerateDataTableLAK00201(LAK021PrintModel printModel, string? tarikhDari, string? tarikhHingga, EnStatusBorang enStatusBorang, int? jKWId)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Laporan Perolehan Batal";
            dt.Columns.Add("Bil", typeof(int));
            dt.Columns.Add("No Rujukan", typeof(string));
            dt.Columns.Add("Tarikh", typeof(DateTime));
            dt.Columns.Add("Pembekal", typeof(string));
            dt.Columns.Add("Jumlah RM", typeof(decimal));

            DateTime date1 = DateTime.MinValue;
            DateTime date2 = DateTime.MaxValue;

            if (!string.IsNullOrEmpty(tarikhDari) && !string.IsNullOrEmpty(tarikhHingga))
            {
                date1 = DateTime.Parse(tarikhDari).Date;
                date2 = DateTime.Parse(tarikhHingga).Date.AddDays(1).AddTicks(-1);
            }

            decimal grandTotalJumlah = 0;

            if (printModel.AkPenilaianPerolehan != null)
            {
                var bil = 1;

                var akPP = _context.AkPenilaianPerolehan
                    .Where(a => (a.FlPOInden == 1) && (a.FlBatal == 0) && a.Tarikh >= date1 && a.Tarikh <= date2 && a.JKWId == jKWId)
                    .ToList();

                var akPORecords = _context.AkPO
                    .Where(p => akPP.Select(a => a.Id).Contains(p.AkPenilaianPerolehanId))
                    .ToList();

                var akIndenRecords = _context.AkInden
                    .Where(k => akPP.Select(a => a.Id).Contains(k.AkPenilaianPerolehanId))
                    .ToList();

                var akBelianRecords = _context.AkBelian
                    .Where(b => (b.AkPOId == null && b.AkIndenId == null) || 
                          (b.AkPOId != null && akPORecords.Select(po => po.Id).Contains(b.AkPOId.Value)) ||
                          (b.AkIndenId != null && akIndenRecords.Select(inden => inden.Id).Contains(b.AkIndenId.Value)))
                    .ToList();

                var akPVInvoisRecords = _context.AkPVInvois
                    .Where(v => akBelianRecords.Select(b => b.Id).Contains(v.AkBelianId))
                    .ToList();

                var belianWithoutPVInvois = akBelianRecords
                    .Where(b => !akPVInvoisRecords.Select(v => v.AkBelianId).Contains(b.Id)) 
                    .ToList();

                var filteredAkPP = akPP
                    .Where(pp => belianWithoutPVInvois.Any(b =>
                        (b.AkPOId != null && b.AkPOId == akPORecords.FirstOrDefault(po => po.AkPenilaianPerolehanId == pp.Id)?.Id) ||
                        (b.AkIndenId != null && b.AkIndenId == akIndenRecords.FirstOrDefault(inden => inden.AkPenilaianPerolehanId == pp.Id)?.Id)) ||
                        akIndenRecords.Any(inden => inden.AkPenilaianPerolehanId == pp.Id && (inden.FlBatal == 0) && !akBelianRecords.Any(b => b.AkIndenId == inden.Id)) ||
                        akIndenRecords.Any(inden => inden.AkPenilaianPerolehanId == pp.Id && inden.FlBatal == 1 && !akBelianRecords.Any(b => b.AkIndenId == inden.Id)) ||
                        akPORecords.Any(po => po.AkPenilaianPerolehanId == pp.Id && (po.FlBatal == 0) && !akBelianRecords.Any(b => b.AkPOId == po.Id)) ||
                        akPORecords.Any(po => po.AkPenilaianPerolehanId == pp.Id && po.FlBatal == 1 && !akBelianRecords.Any(b => b.AkPOId == po.Id))
                      )
                    .ToList();

                foreach (var akPPL in filteredAkPP)
                {
                    decimal Jumlah = akPPL.Jumlah;

                    dt.Rows.Add(bil,
                                akPPL.NoRujukan,
                                akPPL.Tarikh,
                                akPPL.DDaftarAwam?.Nama,
                                akPPL.Jumlah
                            );

                    bil++;
                    grandTotalJumlah += Jumlah;
                }

                var grandTotalRow = dt.NewRow();
                grandTotalRow["Pembekal"] = "JUMLAH RM";
                grandTotalRow["Jumlah RM"] = grandTotalJumlah;

                dt.Rows.Add(grandTotalRow);
            }

            return dt;
        }

        private void RunWorkBookLAK00201(LAK021PrintModel printModel, DataTable excelData, string handle)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Laporan Perolehan Belum Bayar");
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
                ws.Column(5)
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

        private DataTable GenerateDataTableLAK00202(LAK021PrintModel printModel, string? tarikhDari, string? tarikhHingga, EnStatusBorang enStatusBorang, int? jKWId)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Laporan Perolehan Batal";
            dt.Columns.Add("Bil", typeof(int));
            dt.Columns.Add("No Rujukan", typeof(string));
            dt.Columns.Add("Tarikh", typeof(DateTime));
            dt.Columns.Add("Pembekal", typeof(string));
            dt.Columns.Add("Jumlah RM", typeof(decimal));

            DateTime date1 = DateTime.MinValue;
            DateTime date2 = DateTime.MaxValue;

            if (!string.IsNullOrEmpty(tarikhDari) && !string.IsNullOrEmpty(tarikhHingga))
            {
                date1 = DateTime.Parse(tarikhDari).Date;
                date2 = DateTime.Parse(tarikhHingga).Date.AddDays(1).AddTicks(-1);
            }

            decimal grandTotalJumlah = 0;

            if (printModel.AkPenilaianPerolehan != null)
            {
                var bil = 1;

                var akPP = _context.AkPenilaianPerolehan
                   .Where(a => a.FlBatal == 1 && a.Tarikh >= date1 && a.Tarikh <= date2 && a.JKWId == jKWId)
                   .ToList();

                var akPORecords = _context.AkPO
                    .Where(p => akPP.Select(a => a.Id).Contains(p.AkPenilaianPerolehanId) && (p.FlBatal == 0))
                    .ToList();

                var akIndenRecords = _context.AkInden
                    .Where(k => akPP.Select(a => a.Id).Contains(k.AkPenilaianPerolehanId) && (k.FlBatal == 0))
                    .ToList();

                var akBelianRecords = _context.AkBelian
                    .Where(b => (b.AkPOId != null && akPORecords.Select(po => po.Id).Contains(b.AkPOId.Value)) ||
                          (b.AkIndenId != null && akIndenRecords.Select(inden => inden.Id).Contains(b.AkIndenId.Value))
                    )
                    .ToList();

                var akPVInvoisRecords = _context.AkPVInvois
                    .Where(v => akBelianRecords.Select(b => b.Id).Contains(v.AkBelianId))
                    .ToList();

                var excludedAkPPIds = akPORecords
                    .Where(po => akPVInvoisRecords.Any(v => akBelianRecords.Any(b => b.AkPOId == po.Id && b.Id == v.AkBelianId)))
                    .Select(po => po.AkPenilaianPerolehanId)
                    .Union(akIndenRecords
                        .Where(inden => akPVInvoisRecords.Any(v => akBelianRecords.Any(b => b.AkIndenId == inden.Id && b.Id == v.AkBelianId)))
                        .Select(inden => inden.AkPenilaianPerolehanId))
                    .Distinct()
                    .ToList();

                var filteredAkPP = akPP.Where(pp => !excludedAkPPIds.Contains(pp.Id)).ToList();

                foreach (var akPPL in filteredAkPP)
                {
                    decimal Jumlah = akPPL.Jumlah;

                    dt.Rows.Add(bil,
                                akPPL.NoRujukan,
                                akPPL.Tarikh,
                                akPPL.DDaftarAwam?.Nama,
                                akPPL.Jumlah
                            );

                    bil++;
                    grandTotalJumlah += Jumlah;
                }

                var grandTotalRow = dt.NewRow();
                grandTotalRow["Pembekal"] = "JUMLAH RM";
                grandTotalRow["Jumlah RM"] = grandTotalJumlah;

                dt.Rows.Add(grandTotalRow);
            }

            return dt;
        }

        private void RunWorkBookLAK00202(LAK021PrintModel printModel, DataTable excelData, string handle)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Laporan Perolehan Batal");
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
                ws.Column(5)
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

            var selectList = new SelectList(kwSelect, "Value", "Text");

            if (jKWId.HasValue && jKWId.Value != 0)
            {
                var selectedItem = selectList.FirstOrDefault(x => x.Value == jKWId.ToString());
                if (selectedItem != null)
                {
                    selectedItem.Selected = true;
                }
            }
            else if (kwSelect.Any())
            {
                kwSelect.First().Selected = true;
            }

            ViewBag.JKW = selectList;
        }

        // printing List of Laporan
        [AllowAnonymous]
        public async Task<IActionResult> Print(string? kodLaporan, string? tarikhDari, string? tarikhHingga, EnStatusBorang enStatusBorang, int? jKWId)
        {
            var reportModel = await PrepareData(kodLaporan, tarikhDari, tarikhHingga, EnStatusBorang.Semua, jKWId);

            var company = await _userServices.GetCompanyDetails();

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

            var viewDataDictionary = new ViewDataDictionary(ViewData)
            {
                { "NamaSyarikat", company.NamaSyarikat },
                { "AlamatSyarikat1", company.AlamatSyarikat1 },
                { "AlamatSyarikat2", company.AlamatSyarikat2 },
                { "AlamatSyarikat3", company.AlamatSyarikat3 }
            };

            string viewName;

            switch (kodLaporan)
            {
                case "LAK00201":

                    var akpp = await _unitOfWork.AkPenilaianPerolehanRepo.GetResultsGroupByBelumBayar(tarikhDari, tarikhHingga, jKWId);

                    reportModel.AkPenilaianPerolehanResult = akpp;

                    viewName = "LAK00201PDF";
                    var TarikhDariLAK00201 = DateTime.Parse(tarikhDari!).ToString("dd/MM/yyyy");
                    var TarikhHinggaLAK00201 = DateTime.Parse(tarikhHingga!).ToString("dd/MM/yyyy");
                    viewDataDictionary["TarikhDari"] = TarikhDariLAK00201;
                    viewDataDictionary["TarikhHingga"] = TarikhHinggaLAK00201;
                    break;

                case "LAK00202":

                    akpp = await _unitOfWork.AkPenilaianPerolehanRepo.GetResultsGroupByBatal(tarikhDari, tarikhHingga, jKWId);

                    reportModel.AkPenilaianPerolehanResult = akpp;

                    viewName = "LAK00202PDF";
                    var TarikhDariLAK00203 = DateTime.Parse(tarikhDari!).ToString("dd/MM/yyyy");
                    var TarikhHinggaLAK00203 = DateTime.Parse(tarikhHingga!).ToString("dd/MM/yyyy");
                    viewDataDictionary["TarikhDari"] = TarikhDariLAK00203;
                    viewDataDictionary["TarikhHingga"] = TarikhHinggaLAK00203;
                    break;

                default:
                    viewName = modul + EnJenisFail.PDF;
                    break;
            }

            return new ViewAsPdf(viewName, reportModel, viewDataDictionary)
            {
                PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--footer-center \"[page]/[toPage]\"" +
                    " --footer-line --footer-font-size \"7\" --footer-spacing 1 --footer-font-name \"Segoe UI\"",
                PageSize = Rotativa.AspNetCore.Options.Size.A4
            };
        }
        // printing List of Laporan end
    }

}