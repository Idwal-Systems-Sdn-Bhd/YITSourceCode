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
using YIT.__Domain.Entities._Statics;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using YIT._DataAccess.Services;

namespace YIT.Akaun.Controllers._99Laporan
{
    [Authorize]
    public class LAK011Controller : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodLDaftarPoInden;
        public const string namamodul = Modules.namaLDaftarPoInden;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly UserServices _userServices;

        public LAK011Controller(
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
            PopulateSelectList(model.enJenisPerolehan);
            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> ExportExcel(PrintFormModel model)
        {
            LAK011PrintModel printModel = await PrepareData(model.kodLaporan, model.tarikhDari, model.tarikhHingga, model.enJenisPerolehan);

            // Generate a new unique identifier against which the file can be stored
            string handle = string.Format("attachment;" + model.kodLaporan + ".xlsx;", string.IsNullOrEmpty(model.kodLaporan) ? Guid.NewGuid().ToString() : WebUtility.UrlEncode(model.kodLaporan));

            // save viewmodel into workbook
            if (model.kodLaporan == "LAK00201")
            {
                // construct and insert data into dataTable 
                var excelData = GenerateDataTableLAK00201(printModel, model.tarikhDari, model.tarikhHingga, model.enJenisPerolehan);

                // insert dataTable into Workbook
                RunWorkBookLAK00201(printModel, excelData, handle);
            }
            // save viewmodel into workbook
            else if (model.kodLaporan == "LAK00202")
            {
                //construct and insert data into dataTable 
                var excelData1 = GenerateDataTableLAK00202(printModel, model.tarikhDari, model.tarikhHingga);

                //insert dataTable into Workbook
                RunWorkBookLAK00202(printModel, excelData1, handle);
            }

            return Json(new { FileGuid = handle, FileName = model.kodLaporan + ".xlsx" });
        }

        private async Task<LAK011PrintModel> PrepareData(string? kodLaporan, string? tarikhDari, string? tarikhHingga, EnJenisPerolehan enJenisPerolehan)
        {
            LAK011PrintModel reportModel = new LAK011PrintModel();

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
                reportModel.CommonModels.Tajuk1 = $"Daftar Pesanan Tempatan Dari {@Convert.ToDateTime(tarikhDari).ToString("dd/MM/yyyy")} Hingga {@Convert.ToDateTime(tarikhHingga):dd/MM/yyyy}";
                reportModel.AkPO = _unitOfWork.AkPORepo.GetResults1(date1, date2, enJenisPerolehan, null);
            }
            else if (kodLaporan == "LAK00202")
            {
                reportModel.CommonModels.Tajuk1 = $"Daftar Inden Kerja Dari {@Convert.ToDateTime(tarikhDari).ToString("dd/MM/yyyy")} Hingga {@Convert.ToDateTime(tarikhHingga):dd/MM/yyyy}";
                reportModel.AkInden = _unitOfWork.AkIndenRepo.GetResults1(date1, date2);
            }

            return reportModel;
        }

        private DataTable GenerateDataTableLAK00201(LAK011PrintModel printModel, string? tarikhDari, string? tarikhHingga, EnJenisPerolehan enJenisPerolehan)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Daftar Pesanan Tempatan";
            dt.Columns.Add("Bil", typeof(string));
            dt.Columns.Add("No Pesanan", typeof(string));
            dt.Columns.Add("Tarikh", typeof(string));
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

                var akp = _context.AkPO
                    .Include(b => b.AkPOObjek)
                    .Where(p => p.Tarikh >= date1 && p.Tarikh <= date2);

                switch (enJenisPerolehan)
                {
                    case EnJenisPerolehan.Bekalan:
                        akp = akp.Where(pp => pp.EnJenisPerolehan == EnJenisPerolehan.Bekalan);
                        break;
                    case EnJenisPerolehan.Perkhidmatan:
                        akp = akp.Where(pp => pp.EnJenisPerolehan == EnJenisPerolehan.Perkhidmatan);
                        break;
                    case EnJenisPerolehan.Kerja:
                        akp = akp.Where(pp => pp.EnJenisPerolehan == EnJenisPerolehan.Kerja);
                        break;
                    case EnJenisPerolehan.Semua:
                        break;
                }

                var akpo = akp.OrderBy(p => p.Tarikh).ThenBy(p => p.NoRujukan).ToList();

                foreach (var group in akpo)
                {
                    bool isFirstRow = true;

                    foreach (var akPOObjek in group.AkPOObjek!)
                    {
                        string combinedDDaftarAwam = isFirstRow ? $"{group.DDaftarAwam?.Kod} {group.DDaftarAwam?.Nama}" : "";
                        string cleanedKodPerihal = akPOObjek.AkCarta?.Kod!.Trim() + " " + akPOObjek.AkCarta?.Perihal!.Trim();

                        dt.Rows.Add(
                            isFirstRow ? bil.ToString() : "",
                            isFirstRow ? group.NoRujukan : "",
                            isFirstRow ? group.Tarikh.ToString("dd/MM/yyyy") : "",
                            combinedDDaftarAwam,
                            isFirstRow ? group.Jumlah : 0,
                            cleanedKodPerihal,
                            akPOObjek.Amaun
                        );

                        if (isFirstRow)
                        {
                            bil++;
                            isFirstRow = false;
                        }

                        grandTotalAmaun += akPOObjek.Amaun;
                    }

                    grandTotalJumlah += group.Jumlah;
                }

                var grandTotalRow = dt.NewRow();
                grandTotalRow["No Pesanan"] = "JUMLAH";
                grandTotalRow["Jumlah RM"] = grandTotalJumlah;
                grandTotalRow["Amaun RM"] = grandTotalAmaun;
                dt.Rows.Add(grandTotalRow);
            }

            return dt;
        }

        private void RunWorkBookLAK00201(LAK011PrintModel printModel, DataTable excelData, string handle)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Daftar Pesanan Tempatan");
                ws.Cell("A1").Value = printModel.CommonModels.CompanyDetails?.NamaSyarikat;
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A2").Value = printModel.CommonModels.Tajuk1;
                ws.Cell("A3").Value = printModel.CommonModels.Tajuk2;

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
                ws.Column(6).AdjustToContents();
                ws.Column(7)
                    .Style.NumberFormat.Format = " #,##0.00";
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

        private DataTable GenerateDataTableLAK00202(LAK011PrintModel printModel, string? tarikhDari, string? tarikhHingga)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Daftar Inden Kerja";
            dt.Columns.Add("Bil", typeof(int));
            dt.Columns.Add("No Pesanan", typeof(string));
            dt.Columns.Add("Tarikh", typeof(DateTime));
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

            if (printModel.AkInden != null)
            {
                var bil = 1;

                var aki = _context.AkInden
                    .Include(b => b.AkIndenObjek)
                    .Where(p => p.Tarikh >= date1 && p.Tarikh <= date2);

                var akinden = aki.OrderBy(p => p.Tarikh).ThenBy(p => p.NoRujukan).ToList();

                foreach (var group in akinden)
                {
                    foreach (var akIndenObjek in group.AkIndenObjek!)
                    {
                        string combinedDDaftarAwam = $"{group.DDaftarAwam?.Kod} {group.DDaftarAwam?.Nama}";
                        string cleanedKodPerihal = akIndenObjek.AkCarta?.Kod!.Trim() + " " + akIndenObjek.AkCarta?.Perihal!.Trim();

                        dt.Rows.Add(bil,
                                    group.NoRujukan,
                                    group.Tarikh,
                                    combinedDDaftarAwam,
                                    group.Jumlah,
                                    cleanedKodPerihal,
                                    akIndenObjek.Amaun
                                   );
                        bil++;

                        grandTotalAmaun += akIndenObjek.Amaun;
                    }

                    grandTotalJumlah += group.Jumlah;
                }

                var grandTotalRow = dt.NewRow();
                grandTotalRow["No Pesanan"] = "JUMLAH";
                grandTotalRow["Jumlah RM"] = grandTotalJumlah;
                grandTotalRow["Amaun RM"] = grandTotalAmaun;
                dt.Rows.Add(grandTotalRow);
            }

            return dt;
        }

        private void RunWorkBookLAK00202(LAK011PrintModel printModel, DataTable excelData1, string handle)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Daftar Inden Kerja");
                ws.Cell("A1").Value = printModel.CommonModels.CompanyDetails?.NamaSyarikat;
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A2").Value = printModel.CommonModels.Tajuk1;
                ws.Cell("A3").Value = printModel.CommonModels.Tajuk2;

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
                ws.Column(7)
                    .Style.NumberFormat.Format = " #,##0.00";
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
        private void PopulateSelectList(EnJenisPerolehan? selectedValue = null)
        {
            var enumValues = Enum.GetValues(typeof(EnJenisPerolehan)).Cast<EnJenisPerolehan>();

            var selectList = enumValues.Select(e => new SelectListItem
            {
                Text = e.GetDisplayName(),
                Value = ((int)e).ToString(),
                Selected = e == selectedValue
            }).ToList();

            ViewBag.EnJenisPerolehan = new SelectList(selectList, "Value", "Text", selectedValue);
        }

        // printing List of Laporan
        [AllowAnonymous]
        public async Task<IActionResult> Print(string? kodLaporan, string? tarikhDari, string? tarikhHingga, EnJenisPerolehan enJenisPerolehan)
        {
            var akpo = new List<AkPO>();
            var akinden = new List<AkInden>();
            var reportModel = await PrepareData(kodLaporan, tarikhDari, tarikhHingga, enJenisPerolehan);
            var company = await _userServices.GetCompanyDetails();

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
                    viewName = "LAK00201PDF";
                    var TarikhDariLAK00201 = DateTime.Parse(tarikhDari!).ToString("dd/MM/yyyy");
                    var TarikhHinggaLAK00201 = DateTime.Parse(tarikhHingga!).ToString("dd/MM/yyyy");
                    viewDataDictionary["TarikhDari"] = TarikhDariLAK00201;
                    viewDataDictionary["TarikhHingga"] = TarikhHinggaLAK00201;
                    break;

                case "LAK00202":
                    viewName = "LAK00202PDF";
                    var TarikhDariLAK00202 = DateTime.Parse(tarikhDari!).ToString("dd/MM/yyyy");
                    var TarikhHinggaLAK00202 = DateTime.Parse(tarikhHingga!).ToString("dd/MM/yyyy");
                    viewDataDictionary["TarikhDari"] = TarikhDariLAK00202;
                    viewDataDictionary["TarikhHingga"] = TarikhHinggaLAK00202;
                    break;

                default:
                    viewName = modul + EnJenisFail.PDF;
                    break;
            }

            return new ViewAsPdf(viewName, reportModel, viewDataDictionary)
            {
                PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                PageOrientation = viewName == modul + EnJenisFail.PDF ? Rotativa.AspNetCore.Options.Orientation.Portrait : Rotativa.AspNetCore.Options.Orientation.Landscape,
                CustomSwitches = "--footer-center \"[page]/[toPage]\"" +
                    " --footer-line --footer-font-size \"7\" --footer-spacing 1 --footer-font-name \"Segoe UI\"",
                PageSize = Rotativa.AspNetCore.Options.Size.A4
            };
        }
        // printing List of Laporan end
    }

}