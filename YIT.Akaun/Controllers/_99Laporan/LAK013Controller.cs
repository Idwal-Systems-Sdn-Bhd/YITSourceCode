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
using System.Globalization;
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
    public class LAK013Controller : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodLDaftarBaucerBayaran;
        public const string namamodul = Modules.namaLDaftarBaucerBayaran;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly UserServices _userServices;

        public LAK013Controller(
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
            string searchString1 = model.searchString1;
            string searchString2 = model.searchString2;

            LAK013PrintModel printModel = await PrepareData(model.kodLaporan, model.tarikhDari, model.tarikhHingga, model.enStatusBorang, model.jKWId, searchString1, searchString2);

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
            if (model.kodLaporan == "LAK00202")
            {
                // construct and insert data into dataTable 
                var excelData1 = GenerateDataTableLAK00202(printModel, model.enStatusBorang, model.jKWId, searchString1, searchString2);

                // insert dataTable into Workbook
                RunWorkBookLAK00202(printModel, excelData1, handle, searchString1, searchString2);
            }
            // save viewmodel into workbook
            if (model.kodLaporan == "LAK00203")
            {
                //construct and insert data into dataTable 
                var excelData2 = GenerateDataTableLAK00203(printModel, model.tarikhDari, model.tarikhHingga, model.enStatusBorang, model.jKWId);

                //insert dataTable into Workbook
                RunWorkBookLAK00203(printModel, excelData2, handle);
            }

            return Json(new { FileGuid = handle, FileName = model.kodLaporan + ".xlsx" });
        }

        private async Task<LAK013PrintModel> PrepareData(string? kodLaporan, string? tarikhDari, string? tarikhHingga, EnStatusBorang enStatusBorang, int? jKWId, string? searchString1, string? searchString2)
        {
            LAK013PrintModel reportModel = new LAK013PrintModel();

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

            if (kodLaporan == "LAK00201")
            {
                reportModel.CommonModels.Tajuk1 = $"Daftar Baucer Bayaran Dari {Convert.ToDateTime(tarikhDari):dd/MM/yyyy} Hingga {Convert.ToDateTime(tarikhHingga):dd/MM/yyyy}";
                reportModel.AkPV = _unitOfWork.AkPVRepo.GetResults1("", date1, date2, null, EnStatusBorang.Semua, null, null, jKWId, null);
            }
            else if (kodLaporan == "LAK00202")
            {
                var upperSearchString1 = searchString1?.ToUpper() ?? string.Empty;
                var upperSearchString2 = searchString2?.ToUpper() ?? string.Empty;

                reportModel.CommonModels.Tajuk1 = $"Daftar Baucer Bayaran Dari {upperSearchString1} dan {upperSearchString2}";
                reportModel.AkPV = await _unitOfWork.AkPVRepo.GetResultsGroupBySearchString(jKWId, searchString1, searchString2);
            }
            else if (kodLaporan == "LAK00203")
            {
                reportModel.CommonModels.Tajuk1 = $"Laporan Daftar Baucer Bayaran Yang Dibatalkan Dari {Convert.ToDateTime(tarikhDari):dd/MM/yyyy} Hingga {Convert.ToDateTime(tarikhHingga):dd/MM/yyyy}";
                reportModel.AkPV = _unitOfWork.AkPVRepo.GetResults1("", date1, date2, null, EnStatusBorang.Semua, null, null, jKWId, null);
            }

            reportModel.AkPV = _unitOfWork.AkPVRepo.GetResults1("", date1, date2, null, enStatusBorang, null, null, jKWId, null);

            return reportModel;
        }

        private DataTable GenerateDataTableLAK00201(LAK013PrintModel printModel, string? tarikhDari, string? tarikhHingga, EnStatusBorang enStatusBorang, int? jKWId)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Daftar Baucer Bayaran dari Tarikh";
            dt.Columns.Add("Bil", typeof(int));
            dt.Columns.Add("Nama Penerima / Alamat", typeof(string));
            dt.Columns.Add("No KP Baru", typeof(string));
            dt.Columns.Add("No Baucer / Tarikh", typeof(string));
            dt.Columns.Add("No Cek", typeof(string));
            dt.Columns.Add("Jumlah RM", typeof(decimal));

            DateTime date1 = DateTime.MinValue;
            DateTime date2 = DateTime.MaxValue;

            if (!string.IsNullOrEmpty(tarikhDari) && !string.IsNullOrEmpty(tarikhHingga))
            {
                date1 = DateTime.Parse(tarikhDari).Date;
                date2 = DateTime.Parse(tarikhHingga).Date.AddDays(1).AddTicks(-1);
            }

            decimal grandTotalJumlah = 0;

            if (printModel.AkPV != null)
            {
                var bil = 1;

                var akPvList = _context.AkPV
                    .Include(b => b.AkPVPenerima)
                    .Where(b => b.Tarikh >= date1 && b.Tarikh <= date2 && b.JKWId == jKWId)
                    .ToList();

                var allPenerima = akPvList
                    .SelectMany(pv => pv.AkPVPenerima, (pv, penerima) => new
                    {
                        AkPV = pv,
                        AkPVPenerima = penerima
                    })
                    .OrderBy(x => x.AkPVPenerima.NoRujukanCaraBayar)
                    .ToList();

                foreach (var item in allPenerima)
                {
                    var akPV = item.AkPV;
                    var akPVPenerima = item.AkPVPenerima;
                    decimal Jumlah = akPVPenerima.Amaun;

                    string penerimaAlamat = $"{akPVPenerima.NamaPenerima}\r\n{akPVPenerima.Alamat1}\r\n{akPVPenerima.Alamat2}\r\n{akPVPenerima.Alamat3}\r\n Telefon: {akPVPenerima.DDaftarAwam?.Telefon1}";
                    string rujukanTarikh = $"{akPV.NoRujukan}\r\n{akPV.Tarikh.ToString("dd/MM/yyyy")}";

                    dt.Rows.Add(bil,
                                penerimaAlamat,
                                akPVPenerima.NoPendaftaranPenerima,
                                rujukanTarikh,
                                akPVPenerima.NoRujukanCaraBayar,
                                akPVPenerima.Amaun
                            );

                    bil++;
                    grandTotalJumlah += Jumlah;
                }

                var grandTotalRow = dt.NewRow();
                grandTotalRow["No Cek"] = "JUMLAH RM";
                grandTotalRow["Jumlah RM"] = grandTotalJumlah;

                dt.Rows.Add(grandTotalRow);
            }

            return dt;
        }

        private void RunWorkBookLAK00201(LAK013PrintModel printModel, DataTable excelData, string handle)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Daftar Baucer Bayaran (Tarikh)");
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

        private DataTable GenerateDataTableLAK00202(LAK013PrintModel printModel, EnStatusBorang enStatusBorang, int? jKWId, string? searchString1, string? searchString2)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Daftar Baucer Bayaran Dari No Baucer";
            dt.Columns.Add("Bil", typeof(int));
            dt.Columns.Add("Nama Penerima / Alamat", typeof(string));
            dt.Columns.Add("No KP Baru", typeof(string));
            dt.Columns.Add("No Baucer / Tarikh", typeof(string));
            dt.Columns.Add("No Cek", typeof(string));
            dt.Columns.Add("Jumlah RM", typeof(decimal));

            decimal grandTotalJumlah = 0;

            if (printModel.AkPV != null)
            {
                var bil = 1;

                var akPvList = _context.AkPV
                    .Include(b => b.AkPVPenerima)
                    .Where(b => b.JKWId == jKWId)
                    .ToList();

                var allPenerima = akPvList
                    .SelectMany(pv => pv.AkPVPenerima, (pv, penerima) => new
                    {
                        AkPV = pv,
                        AkPVPenerima = penerima
                    })
                    .Where(x => string.Compare(x.AkPV.NoRujukan.ToLower(), searchString1.ToLower()) >= 0 &&
                                string.Compare(x.AkPV.NoRujukan.ToLower(), searchString2.ToLower()) <= 0)
                    .OrderBy(x => x.AkPVPenerima.NoRujukanCaraBayar)
                    .ToList();

                foreach (var item in allPenerima)
                {
                    var akPV = item.AkPV;
                    var akPVPenerima = item.AkPVPenerima;
                    decimal Jumlah = akPVPenerima.Amaun;

                    string penerimaAlamat = $"{akPVPenerima.NamaPenerima}\r\n{akPVPenerima.Alamat1}\r\n{akPVPenerima.Alamat2}\r\n{akPVPenerima.Alamat3}\r\n Telefon: {akPVPenerima.DDaftarAwam?.Telefon1}";
                    string rujukanTarikh = $"{akPV.NoRujukan}\r\n{akPV.Tarikh.ToString("dd/MM/yyyy")}";

                    dt.Rows.Add(bil,
                                penerimaAlamat,
                                akPVPenerima.NoPendaftaranPenerima,
                                rujukanTarikh,
                                akPVPenerima.NoRujukanCaraBayar,
                                akPVPenerima.Amaun
                            );

                    bil++;
                    grandTotalJumlah += Jumlah;
                }

                var grandTotalRow = dt.NewRow();
                grandTotalRow["No Cek"] = "JUMLAH RM";
                grandTotalRow["Jumlah RM"] = grandTotalJumlah;

                dt.Rows.Add(grandTotalRow);
            }

            return dt;
        }

        private void RunWorkBookLAK00202(LAK013PrintModel printModel, DataTable excelData1, string handle, string? searchString1, string? searchString2)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Daftar Baucer Bayaran (Baucer)");
                ws.Cell("A1").Value = printModel.CommonModels.CompanyDetails?.NamaSyarikat;
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A2").Value = printModel.CommonModels.Tajuk1;
                ws.Cell("A3").Value = printModel.CommonModels.Tajuk2;

                ws.ColumnWidth = 5;
                ws.Cell("A5").InsertTable(excelData1)
                    .Theme = XLTableTheme.TableStyleMedium1;

                ws.Column(2).AdjustToContents();
                ws.Column(3).AdjustToContents();
                ws.Column(4).AdjustToContents();
                ws.Column(5).AdjustToContents();
                ws.Column(6).AdjustToContents();
                ws.Column(6)
                   .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(7).AdjustToContents();

                int grandTotalRowIndex = excelData1.Rows.Count + 5;

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

        private DataTable GenerateDataTableLAK00203(LAK013PrintModel printModel, string? tarikhDari, string? tarikhHingga, EnStatusBorang enStatusBorang, int? jKWId)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Laporan Daftar Baucer Bayaran Yang Dibatalkan";
            dt.Columns.Add("Bil", typeof(int));
            dt.Columns.Add("Nama Penerima / Alamat", typeof(string));
            dt.Columns.Add("No KP Baru", typeof(string));
            dt.Columns.Add("No Baucer / Tarikh", typeof(string));
            dt.Columns.Add("No Cek", typeof(string));
            dt.Columns.Add("Jumlah RM", typeof(decimal));

            DateTime date1 = DateTime.MinValue;
            DateTime date2 = DateTime.MaxValue;

            if (!string.IsNullOrEmpty(tarikhDari) && !string.IsNullOrEmpty(tarikhHingga))
            {
                date1 = DateTime.Parse(tarikhDari).Date;
                date2 = DateTime.Parse(tarikhHingga).Date.AddDays(1).AddTicks(-1);
            }

            decimal grandTotalJumlah = 0;

            if (printModel.AkPV != null)
            {
                var bil = 1;

                var akPvList = _context.AkPV
                    .Include(b => b.AkPVPenerima)
                    .Where(b => b.Tarikh >= date1 && b.Tarikh <= date2 && b.JKWId == jKWId && b.FlBatal == 1)
                    .ToList();

                var allPenerima = akPvList
                    .SelectMany(pv => pv.AkPVPenerima, (pv, penerima) => new
                    {
                        AkPV = pv,
                        AkPVPenerima = penerima
                    })
                    .OrderBy(x => x.AkPVPenerima.NoRujukanCaraBayar)
                    .ToList();

                foreach (var item in allPenerima)
                {
                    var akPV = item.AkPV;
                    var akPVPenerima = item.AkPVPenerima;
                    decimal Jumlah = akPVPenerima.Amaun;

                    string penerimaAlamat = $"{akPVPenerima.NamaPenerima}\r\n{akPVPenerima.Alamat1}\r\n{akPVPenerima.Alamat2}\r\n{akPVPenerima.Alamat3}\r\n Telefon: {akPVPenerima.DDaftarAwam?.Telefon1}";
                    string rujukanTarikh = $"{akPV.NoRujukan}\r\n{akPV.Tarikh.ToString("dd/MM/yyyy")}";

                    dt.Rows.Add(bil,
                                penerimaAlamat,
                                akPVPenerima.NoPendaftaranPenerima,
                                rujukanTarikh,
                                akPVPenerima.NoRujukanCaraBayar,
                                akPVPenerima.Amaun
                            );

                    bil++;
                    grandTotalJumlah += Jumlah;
                }

                var grandTotalRow = dt.NewRow();
                grandTotalRow["No Cek"] = "JUMLAH RM";
                grandTotalRow["Jumlah RM"] = grandTotalJumlah;

                dt.Rows.Add(grandTotalRow);
            }

            return dt;
        }

        private void RunWorkBookLAK00203(LAK013PrintModel printModel, DataTable excelData2, string handle)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Daftar Baucer Bayaran (Batal)");
                ws.Cell("A1").Value = printModel.CommonModels.CompanyDetails?.NamaSyarikat;
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A2").Value = printModel.CommonModels.Tajuk1;
                ws.Cell("A3").Value = printModel.CommonModels.Tajuk2;

                ws.ColumnWidth = 5;
                ws.Cell("A5").InsertTable(excelData2)
                    .Theme = XLTableTheme.TableStyleMedium1;

                ws.Column(2).AdjustToContents();
                ws.Column(3).AdjustToContents();
                ws.Column(4).AdjustToContents();
                ws.Column(5).AdjustToContents();
                ws.Column(6).AdjustToContents();
                ws.Column(6)
                   .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(7).AdjustToContents();

                int grandTotalRowIndex = excelData2.Rows.Count + 5;

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
        public async Task<IActionResult> Print(string? kodLaporan, string? tarikhDari, string? tarikhHingga, EnStatusBorang enStatusBorang, int? jKWId, string? searchString1, string? searchString2)
        {
            var reportModel = await PrepareData(kodLaporan, tarikhDari, tarikhHingga, EnStatusBorang.Semua, jKWId, searchString1, searchString2);

            var company = await _userServices.GetCompanyDetails();

            var viewDataDictionary = new ViewDataDictionary(ViewData)
            {
                { "NamaSyarikat", company.NamaSyarikat },
                { "AlamatSyarikat1", company.AlamatSyarikat1 },
                { "AlamatSyarikat2", company.AlamatSyarikat2 },
                { "AlamatSyarikat3", company.AlamatSyarikat3 },
                { "SearchString1", searchString1 },
                { "SearchString2", searchString2 }
            };

            string viewName;

            switch (kodLaporan)
            {
                case "LAK00201":

                    var akpv = await _unitOfWork.AkPVRepo.GetResultsGroupByTarikh(tarikhDari, tarikhHingga, jKWId);

                    reportModel.AkPV = akpv;

                    viewName = "LAK00201PDF";
                    var TarikhDariLAK00201 = DateTime.Parse(tarikhDari).ToString("dd/MM/yyyy");
                    var TarikhHinggaLAK00201 = DateTime.Parse(tarikhHingga).ToString("dd/MM/yyyy");
                    viewDataDictionary["TarikhDari"] = TarikhDariLAK00201;
                    viewDataDictionary["TarikhHingga"] = TarikhHinggaLAK00201;
                    break;

                case "LAK00202":

                    akpv = await _unitOfWork.AkPVRepo.GetResultsGroupBySearchString(jKWId, searchString1, searchString2);

                    reportModel.AkPV = akpv;

                    viewName = "LAK00202PDF";
                    break;

                case "LAK00203":

                    akpv = await _unitOfWork.AkPVRepo.GetResultsGroupByTarikh(tarikhDari, tarikhHingga, jKWId);

                    akpv = akpv.Where(pv => pv.FlBatal == 1).ToList();

                    reportModel.AkPV = akpv;

                    viewName = "LAK00203PDF";
                    var TarikhDariLAK00203 = DateTime.Parse(tarikhDari).ToString("dd/MM/yyyy");
                    var TarikhHinggaLAK00203 = DateTime.Parse(tarikhHingga).ToString("dd/MM/yyyy");
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