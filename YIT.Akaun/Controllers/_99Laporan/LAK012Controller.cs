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
    public class LAK012Controller : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodLPenyataAkaunPembekal;
        public const string namamodul = Modules.namaLPenyataAkaunPembekal;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly UserServices _userServices;

        public LAK012Controller(
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
            PopulateSelectList(model.dDaftarAwamId);
            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> ExportExcel(PrintFormModel model)
        {
            LAK012PrintModel printModel = await PrepareData(model.kodLaporan, model.tarikhDari, model.tarikhHingga, model.dDaftarAwamId);

            // Generate a new unique identifier against which the file can be stored
            string handle = string.Format("attachment;" + model.kodLaporan + ".xlsx;", string.IsNullOrEmpty(model.kodLaporan) ? Guid.NewGuid().ToString() : WebUtility.UrlEncode(model.kodLaporan));

            // save viewmodel into workbook
            if (model.kodLaporan == "LAK00201")
            {
                // construct and insert data into dataTable 
                var excelData = GenerateDataTableLAK00201(printModel, model.tarikhDari, model.tarikhHingga, model.dDaftarAwamId);

                // insert dataTable into Workbook
                RunWorkBookLAK00201(printModel, excelData, handle);
            }
            // save viewmodel into workbook

            return Json(new { FileGuid = handle, FileName = model.kodLaporan + ".xlsx" });
        }

        private async Task<LAK012PrintModel> PrepareData(string? kodLaporan, string? tarikhDari, string? tarikhHingga, int? dDaftarAwamId)
        {
            LAK012PrintModel reportModel = new LAK012PrintModel();

            var user = await _userManager.GetUserAsync(User);
            var namaUser = await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.Email == user!.Email);
            reportModel.CommonModels.Username = namaUser?.Nama;

            reportModel.CommonModels.KodLaporan = kodLaporan;
            reportModel.CommonModels.CompanyDetails = new CompanyDetails();

            DateTime? date1 = null;
            DateTime? date2 = null;

            if (!string.IsNullOrEmpty(tarikhDari) && !string.IsNullOrEmpty(tarikhHingga))
            {
                date1 = DateTime.Parse(tarikhDari);
                date2 = DateTime.Parse(tarikhHingga);
            }

            string? selectedKod = "";
            string? selectedNama = "";

            if (dDaftarAwamId.HasValue)
            {
                var selectedItem = await _unitOfWork.DDaftarAwamRepo.GetByIdAsync(dDaftarAwamId.Value);
                if (selectedItem != null)
                {
                    selectedKod = selectedItem.Kod;
                    selectedNama = selectedItem.Nama;
                }
            }

            if (kodLaporan == "LAK00201")
            {
                reportModel.CommonModels.Tajuk1 = $"Penyata Akaun Pembekal Dari Tarikh : {date1?.ToString("dd/MM/yyyy")} Hingga {date2?.ToString("dd/MM/yyyy")}, Kod Pembekal: {selectedKod}, Nama: {selectedKod} - {selectedNama}";

                reportModel.AkPV = _unitOfWork.AkPVRepo.GetResults1("", date1, date2, null, EnStatusBorang.Semua, null, null, null, dDaftarAwamId);
                reportModel.AkBelian = _unitOfWork.AkBelianRepo.GetResults1("", date1, date2, dDaftarAwamId);

                var kredit = await _unitOfWork.AkBelianRepo.GetKredit(tarikhDari, tarikhHingga, dDaftarAwamId);

                if (kredit > 0)
                {
                    kredit = -kredit;
                }

                reportModel.Kredit = kredit;
                reportModel.Baki = kredit;
            }

            return reportModel;
        }

        private DataTable GenerateDataTableLAK00201(LAK012PrintModel printModel, string? tarikhDari, string? tarikhHingga, int? dDaftarAwamId)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Penyata Akaun Pembekal";
            dt.Columns.Add("Bil", typeof(int));
            dt.Columns.Add("No Invois / No Baucer", typeof(string));
            dt.Columns.Add("Tarikh", typeof(DateTime));
            dt.Columns.Add("Perkara", typeof(string));
            dt.Columns.Add("Debit", typeof(decimal));
            dt.Columns.Add("Kredit", typeof(decimal));
            dt.Columns.Add("Baki", typeof(decimal));

            DateTime date1 = DateTime.MinValue;
            DateTime date2 = DateTime.MaxValue;

            if (!string.IsNullOrEmpty(tarikhDari) && !string.IsNullOrEmpty(tarikhHingga))
            {
                date1 = DateTime.Parse(tarikhDari).Date;
                date2 = DateTime.Parse(tarikhHingga).Date.AddDays(1).AddTicks(-1);
            }

            decimal totalDebit = 0m;
            decimal totalKredit = printModel.Kredit;
            decimal runningBalance = printModel.Baki;

            int bil = 1;
            dt.Rows.Add(bil,
                        string.Empty,
                        date1,
                        "Baki Awal",
                        0m,
                        totalKredit,
                        runningBalance
                       );
            bil++;

            var akBelianList = _context.AkBelian
                .Include(a => a.AkBelianPerihal)
                .Where(a => a.Tarikh >= date1 && a.Tarikh <= date2 && a.DDaftarAwamId == dDaftarAwamId)
                .OrderBy(a => a.Tarikh)
                .ThenBy(a => a.NoRujukan)
                .ToList();

            foreach (var akBelian in akBelianList)
            {
                runningBalance += akBelian.Jumlah;
                totalKredit += akBelian.Jumlah;

                string perihal = akBelian.AkBelianPerihal.FirstOrDefault()?.Perihal?.Trim() ?? "Unknown Perihal";

                dt.Rows.Add(bil,
                            akBelian.NoRujukan,
                            akBelian.Tarikh,
                            perihal,
                            0m,
                            akBelian.Jumlah,
                            runningBalance
                           );
                bil++;
            }

            var matchingPvIds = _context.AkPVPenerima
                .Where(p => p.DDaftarAwamId == dDaftarAwamId)
                .Select(p => p.AkPVId)
                .ToList();

            var akPvList = _context.AkPV
                .Include(a => a.AkPVInvois)
                .ThenInclude(b => b.AkBelian)
                .Where(a => a.Tarikh >= date1 && a.Tarikh <= date2 && matchingPvIds.Contains(a.Id))
                .Where(a => a.AkPVInvois.Any(b => b.AkBelian.DDaftarAwamId == dDaftarAwamId && b.AkBelian.NoRujukan != null))
                .ToList();

            foreach (var akPv in akPvList)
            {
                decimal totalDebitEntry = akPv.AkPVInvois.Sum(x => x.AkBelian.Jumlah);
                runningBalance -= totalDebitEntry;
                totalDebit += totalDebitEntry;

                string cleanedRingkasan = akPv.Ringkasan.Trim();

                dt.Rows.Add(bil,
                            akPv.NoRujukan,
                            akPv.Tarikh,
                            cleanedRingkasan,
                            totalDebitEntry,
                            0m,
                            runningBalance
                           );
                bil++;
            }

            var grandTotalRow = dt.NewRow();
            grandTotalRow["Debit"] = totalDebit;
            grandTotalRow["Kredit"] = totalKredit;
            dt.Rows.Add(grandTotalRow);

            return dt;
        }

        private void RunWorkBookLAK00201(LAK012PrintModel printModel, DataTable excelData, string handle)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Penyata Akaun Pembekal");
                ws.Cell("A1").Value = printModel.CommonModels.CompanyDetails?.NamaSyarikat;
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A2").Value = printModel.CommonModels.Tajuk1;
                ws.Cell("A3").Value = printModel.CommonModels.Tajuk2;

                ws.ColumnWidth = 5;
                ws.Cell("A5").InsertTable(excelData)
                    .Theme = XLTableTheme.TableStyleMedium1;

                ws.Column(2).AdjustToContents();
                ws.Column(3).Style.DateFormat.Format = "dd/MM/yyyy";
                ws.Column(3).AdjustToContents();
                ws.Column(4).AdjustToContents();
                ws.Column(5).Style.NumberFormat.Format = " #,##0.00";
                ws.Column(5).AdjustToContents();
                ws.Column(6).Style.NumberFormat.Format = " #,##0.00";
                ws.Column(6).AdjustToContents();
                ws.Column(7).Style.NumberFormat.Format = " #,##0.00";
                ws.Column(7).AdjustToContents();

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

        private void PopulateSelectList(int? dDaftarAwamId)
        {
            var dDAList = _unitOfWork.DDaftarAwamRepo.GetAllDetails();
            var daSelect = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "-- SEMUA PEMBEKAL --",
                    Value = "0"
                }
            };

            if (dDAList != null && dDAList.Any())
            {
                daSelect.AddRange(dDAList.Select(item => new SelectListItem
                {
                    Text = item.Kod + " - " + item.Nama,
                    Value = item.Id.ToString()
                }));
            }
            else
            {
                daSelect.Add(new SelectListItem
                {
                    Text = "-- Tiada Pembekal Berdaftar --",
                    Value = ""
                });
            }

            ViewBag.DDaftarAwam = new SelectList(daSelect, "Value", "Text");

            if (dDaftarAwamId.HasValue && dDaftarAwamId.Value != 0)
            {
                var selectedItem = dDAList.FirstOrDefault(x => x.Id == dDaftarAwamId.Value);
                if (selectedItem != null)
                {
                    ViewBag.SelectedKod = selectedItem.Kod;
                    ViewBag.SelectedNama = selectedItem.Nama;
                }
                else
                {
                    ViewBag.SelectedKod = "";
                    ViewBag.SelectedNama = "";
                }
            }
        }

        // printing List of Laporan
        [AllowAnonymous]
        public async Task<IActionResult> Print(string? kodLaporan, string? tarikhDari, string? tarikhHingga, int? dDaftarAwamId)
        {
            PopulateSelectList(dDaftarAwamId);

            var reportModel = await PrepareData(kodLaporan, tarikhDari, tarikhHingga, dDaftarAwamId);
            var company = await _userServices.GetCompanyDetails();

            if (kodLaporan == "LAK00201")
            {
                var akpv = await _unitOfWork.AkPVRepo.GetResultsGroupByTarikh1(tarikhDari, tarikhHingga, dDaftarAwamId);
                var akbelian = await _unitOfWork.AkBelianRepo.GetResultsGroupByTarikh1(tarikhDari, tarikhHingga, dDaftarAwamId);

                var kredit = await _unitOfWork.AkBelianRepo.GetKredit(tarikhDari, tarikhHingga, dDaftarAwamId);

                if (kredit > 0)
                {
                    kredit = -kredit;
                }

                var baki = kredit;

                var combinedData = akbelian
                    .Select(ab => new CombinedData
                    {
                        Tarikh = ab.Tarikh,
                        NoRujukan = ab.NoRujukan,
                        Perihal = ab.AkBelianPerihal?.FirstOrDefault()?.Perihal ?? "",
                        Jumlah = ab.Jumlah,
                        Type = "Kredit"
                    })
                    .Concat(
                        akpv.Select(apv => new CombinedData
                        {
                            Tarikh = apv.Tarikh,
                            NoRujukan = apv.NoRujukan,
                            Perihal = apv.Ringkasan,
                            Jumlah = apv.Jumlah,
                            Type = "Debit"
                        })
                    )
                    .OrderBy(cd => cd.Tarikh)
                    .ThenBy(cd => cd.NoRujukan)
                    .ToList();

                reportModel = new LAK012PrintModel
                {
                    AkPV = akpv.ToList(),
                    AkBelian = akbelian.ToList(),
                    CombinedData = combinedData,
                    CommonModels = new CommonPrintModel
                    {
                        CompanyDetails = company,
                    },
                    Kredit = kredit,
                    Baki = baki,
                };

                tarikhDari = DateTime.Parse(tarikhDari).ToString("dd/MM/yyyy");
                tarikhHingga = DateTime.Parse(tarikhHingga).ToString("dd/MM/yyyy");

                return new ViewAsPdf("LAK00201PDF", reportModel, new ViewDataDictionary(ViewData) {
                    { "NamaSyarikat", company.NamaSyarikat },
                    { "AlamatSyarikat1", company.AlamatSyarikat1 },
                    { "AlamatSyarikat2", company.AlamatSyarikat2 },
                    { "AlamatSyarikat3", company.AlamatSyarikat3 },
                    { "TarikhDari", tarikhDari },
                    { "TarikhHingga", tarikhHingga },
                })
                {
                    PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                    PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                    CustomSwitches = "--footer-center \"[page]/[toPage]\"" +
                        " --footer-line --footer-font-size \"7\" --footer-spacing 1 --footer-font-name \"Segoe UI\"",
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                };
            }

            //string customSwitches = "--page-offset 0 --footer-center [page] / [toPage] --footer-font-size 6";

            return new ViewAsPdf(modul + EnJenisFail.PDF, reportModel,
            new ViewDataDictionary(ViewData) {
                    { "NamaSyarikat", company.NamaSyarikat },
                    { "AlamatSyarikat1", company.AlamatSyarikat1 },
                    { "AlamatSyarikat2", company.AlamatSyarikat2 },
                    { "AlamatSyarikat3", company.AlamatSyarikat3 }
            })

            {
                PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--footer-center \"[page]/[to  Page]\"" +
                    " --footer-line --footer-font-size \"7\" --footer-spacing 1 --footer-font-name \"Segoe UI\"",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
            };
        }
        // printing List of Laporan end

    }

}