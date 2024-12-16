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
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT.Akaun.Infrastructure;
using YIT.Akaun.Models.ViewModels.Forms;
using YIT.Akaun.Models.ViewModels.Prints;

namespace YIT.Akaun.Controllers._99Laporan
{
    [Authorize]
    public class LAK014Controller : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodLPoBelumDibayar;
        public const string namamodul = Modules.namaLPoBelumDibayar;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly UserServices _userServices;

        public LAK014Controller(
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
            string searchString1 = model.searchString1!;
            string searchString2 = model.searchString2!;

            LAK014PrintModel printModel = await PrepareData(model.kodLaporan, model.tarikhDari, model.tarikhHingga, model.enStatusBorang, model.jKWId);

            // Generate a new unique identifier against which the file can be stored
            string handle = string.Format("attachment;" + model.kodLaporan + ".xlsx;", string.IsNullOrEmpty(model.kodLaporan) ? Guid.NewGuid().ToString() : WebUtility.UrlEncode(model.kodLaporan));

            // save viewmodel into workbook
            if (model.kodLaporan == "LAK014")
            {
                // construct and insert data into dataTable 
                var excelData = GenerateDataTableLAK014(printModel, model.tarikhDari, model.tarikhHingga, model.enStatusBorang, model.jKWId);

                // insert dataTable into Workbook
                RunWorkBookLAK014(printModel, excelData, handle);
            }

            return Json(new { FileGuid = handle, FileName = model.kodLaporan + ".xlsx" });
        }

        private async Task<LAK014PrintModel> PrepareData(string? kodLaporan, string? tarikhDari, string? tarikhHingga, EnStatusBorang enStatusBorang, int? jKWId)
        {
            LAK014PrintModel reportModel = new LAK014PrintModel();

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

            if (kodLaporan == "LAK014")
            {
                reportModel.CommonModels.Tajuk1 = $"Laporan Pesanan Tempatan Belum Dibayar Dari {Convert.ToDateTime(tarikhDari):dd/MM/yyyy} Hingga {Convert.ToDateTime(tarikhHingga):dd/MM/yyyy}";
                reportModel.AkPO = _unitOfWork.AkPORepo.GetResults1(date1, date2, null, jKWId);
            }

            reportModel.AkPO = _unitOfWork.AkPORepo.GetResults1(date1, date2, null, jKWId);

            return reportModel;
        }

        private DataTable GenerateDataTableLAK014(LAK014PrintModel printModel, string? tarikhDari, string? tarikhHingga, EnStatusBorang enStatusBorang, int? jKWId)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Laporan Pesanan Tempatan Belum Dibayar";
            dt.Columns.Add("Bil", typeof(int));
            dt.Columns.Add("Tarikh", typeof(DateTime));
            dt.Columns.Add("No Rujukan", typeof(string));
            dt.Columns.Add("Pembekal", typeof(string));
            dt.Columns.Add("Jumlah RM", typeof(decimal));
            dt.Columns.Add("Vot", typeof(string));
            dt.Columns.Add("Amaun RM", typeof(decimal));

            DateTime date1 = DateTime.MinValue;
            DateTime date2 = DateTime.MaxValue;

            if (!string.IsNullOrEmpty(tarikhDari) && !string.IsNullOrEmpty(tarikhHingga))
            {
                date1 = DateTime.Parse(tarikhDari).Date;
                date2 = DateTime.Parse(tarikhHingga).Date.AddDays(1).AddTicks(-1);
            }

            decimal grandTotalJumlah = 0;
            decimal grandTotalAmaun = 0;

            if (printModel.AkPO != null)
            {
                var bil = 1;

                var groupedSums = _context.AkPO
                .GroupBy(p => p.NoRujukanLama!.Substring(0, 7))
                .Select(g => new
                {
                    NOPO = g.Key, 
                    nJumlah = g.Sum(x => x.Jumlah)
                })
                .ToList();

                var akpoRecords = _context.AkPO
                    .Where(b => b.Tarikh >= date1 && b.Tarikh <= date2 && b.JKWId == jKWId)
                    .Where(a => !_context.AkBelian.Any(b => b.AkPOId == a.Id) || (_context.AkBelian.Any(b => b.AkPOId == a.Id) 
                     && !_context.AkPVInvois.Any(v => v.AkBelian!.AkPOId == a.Id)))
                    .Select(a => new _AkPOResult
                    {
                        Tarikh = a.Tarikh,
                        NoRujukan = a.NoRujukan,
                        DNama = a.DDaftarAwam != null ? a.DDaftarAwam.Nama : null,
                        Jumlah = a.Jumlah,
                        AkCartaKod = a.AkPOObjek != null && a.AkPOObjek.Any() ? a.AkPOObjek.FirstOrDefault()!.AkCarta!.Kod : null,
                        AkCartaPerihal = a.AkPOObjek != null && a.AkPOObjek.Any() ? a.AkPOObjek.FirstOrDefault()!.AkCarta!.Perihal : null,
                        Amaun = a.AkPOObjek != null && a.AkPOObjek.Any() ? a.AkPOObjek.FirstOrDefault()!.Amaun : 0
                    })
                    .ToList();

                var result = new List<_AkPOResult>();

                foreach (var akpo in akpoRecords)
                {
                    var noRujukanPrefix = akpo.NoRujukan!.Substring(0, 7);

                    var group = groupedSums.FirstOrDefault(g => g.NOPO == noRujukanPrefix);

                    if (group != null && group.nJumlah != 0.00m)
                    {
                        result.Add(akpo);
                    }
                }

                var finalResult = result
               .OrderBy(r => r.NoRujukan)
               .ThenBy(r => r.Tarikh)
               .ToList();

                foreach (var akp in finalResult)
                {
                    string cleanedKodPerihal = akp.AkCartaKod?.Trim() + " " + akp.AkCartaPerihal?.Trim();

                    dt.Rows.Add(bil,
                                akp.Tarikh,
                                akp.NoRujukan,
                                akp.DNama,
                                akp.Jumlah,
                                cleanedKodPerihal,
                                akp.Amaun
                            );

                    bil++;
                    grandTotalJumlah += akp.Jumlah;
                    grandTotalAmaun += akp.Amaun;
                }

                var grandTotalRow = dt.NewRow();
                grandTotalRow["Pembekal"] = "JUMLAH RM";
                grandTotalRow["Jumlah RM"] = grandTotalJumlah;
                grandTotalRow["Amaun RM"] = grandTotalAmaun;
                dt.Rows.Add(grandTotalRow);
            }

            return dt;
        }

        private void RunWorkBookLAK014(LAK014PrintModel printModel, DataTable excelData, string handle)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Pesanan Tempatan Belum Dibayar");
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
                ws.Column(6).AdjustToContents();
                ws.Column(7).AdjustToContents();
                ws.Column(7)
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

            var akpo = await _unitOfWork.AkPORepo.GetResultsGroupByTarikh(tarikhDari, tarikhHingga, jKWId);

            reportModel.AkPOResult = akpo;

            tarikhDari = DateTime.Parse(tarikhDari!).ToString("dd/MM/yyyy");
            tarikhHingga = DateTime.Parse(tarikhHingga!).ToString("dd/MM/yyyy");

            return new ViewAsPdf("LAK014PDF", reportModel, new ViewDataDictionary(ViewData)
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
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                CustomSwitches = "--footer-center \"[page]/[toPage]\"" +
                    " --footer-line --footer-font-size \"7\" --footer-spacing 1 --footer-font-name \"Segoe UI\"",
                PageSize = Rotativa.AspNetCore.Options.Size.A4
            };
        }
        // printing List of Laporan end
    }

}