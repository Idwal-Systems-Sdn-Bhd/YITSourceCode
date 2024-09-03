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
    public class LAK018Controller : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodLWaranPeruntukan;
        public const string namamodul = Modules.namaLWaranPeruntukan;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly UserServices _userServices;

        public LAK018Controller(
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
            PopulateSelectList(model.jKWId, model.jBahagianId);
            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> ExportExcel(PrintFormModel model)
        {
            LAK018PrintModel printModel = await PrepareData(model.kodLaporan, model.jKWId, model.jBahagianId, model.tahun, model.enJenisPeruntukan);

            // Generate a new unique identifier against which the file can be stored
            string handle = string.Format("attachment;" + model.kodLaporan + ".xlsx;", string.IsNullOrEmpty(model.kodLaporan) ? Guid.NewGuid().ToString() : WebUtility.UrlEncode(model.kodLaporan));

            // save viewmodel into workbook
            if (model.kodLaporan == "LAK00201")
            {
                // construct and insert data into dataTable 
                var excelData = GenerateDataTableLAK00201(printModel, model.jKWId, model.jBahagianId, model.tahun);

                // insert dataTable into Workbook
                RunWorkBookLAK00201(printModel, excelData, handle);
            }
            // save viewmodel into workbook
            if (model.kodLaporan == "LAK00202")
            {
                //construct and insert data into dataTable
                var excelData1 = GenerateDataTableLAK00202(printModel, model.jKWId, model.jBahagianId, model.tahun);

                //insert dataTable into Workbook
                RunWorkBookLAK00202(printModel, excelData1, handle);
            }

            return Json(new { FileGuid = handle, FileName = model.kodLaporan + ".xlsx" });
        }

        private async Task<LAK018PrintModel> PrepareData(string? kodLaporan, int? jKWId, int? jBahagianId, string? tahun, EnJenisPeruntukan? enJenisPeruntukan)
        {
            LAK018PrintModel reportModel = new LAK018PrintModel();

            var user = await _userManager.GetUserAsync(User);
            var namaUser = await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.Email == user!.Email);

            reportModel.CommonModels.Username = namaUser?.Nama;

            reportModel.CommonModels.KodLaporan = kodLaporan;
            CompanyDetails company = new CompanyDetails();
            reportModel.CommonModels.CompanyDetails = company;

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

            string? jBahagianKod = "";
            string? jBahagianPerihal = "";

            if (jBahagianId.HasValue)
            {
                var jBahagianDetails = await _context.JBahagian
                                            .Where(j => j.Id == jBahagianId)
                                            .Select(j => new { j.Kod, j.Perihal })
                                            .FirstOrDefaultAsync();

                if (jBahagianDetails != null)
                {
                    jBahagianKod = jBahagianDetails.Kod;
                    jBahagianPerihal = jBahagianDetails.Perihal;
                }
            }

            if (kodLaporan == "LAK00201")
            {
                reportModel.CommonModels.Tajuk1 = $"LAPORAN WARAN PERUNTUKAN PADA TAHUN {tahun} MENGIKUT KUMPULAN WANG {jKWKod} - {jKWPerihal} DAN PTJ {jBahagianKod} - {jBahagianPerihal} ";
                reportModel.AbWaran = _unitOfWork.AbWaranRepo.GetResults1(jKWId, jBahagianId, tahun, enJenisPeruntukan);
            }
            else if (kodLaporan == "LAK00202")
            {
                reportModel.CommonModels.Tajuk1 = $"LAPORAN PINDAHAN PERUNTUKAN PADA TAHUN {tahun} MENGIKUT KUMPULAN WANG {jKWKod} - {jKWPerihal} DAN PTJ {jBahagianKod} - {jBahagianPerihal} ";
                reportModel.AbWaran = _unitOfWork.AbWaranRepo.GetResults1(jKWId, jBahagianId, tahun, EnJenisPeruntukan.Viremen);
            }

            return reportModel;
        }

        private DataTable GenerateDataTableLAK00201(LAK018PrintModel printModel, int? jKWId, int? jBahagianId, string? tahun)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Laporan Waran Peruntukan";
            dt.Columns.Add("Bil", typeof(int));
            dt.Columns.Add("No Rujukan", typeof(string));
            dt.Columns.Add("Tarikh", typeof(DateTime));
            dt.Columns.Add("Objek", typeof(string));
            dt.Columns.Add("Nama Akaun", typeof(string));
            dt.Columns.Add("Asal RM", typeof(decimal));
            dt.Columns.Add("Tambah RM", typeof(decimal));
            dt.Columns.Add("Pindah RM", typeof(decimal));
            dt.Columns.Add("Jumlah RM", typeof(decimal));

            decimal totalAsal = 0;
            decimal totalTambah = 0;
            decimal totalPindah = 0;
            decimal grandTotalJumlah = 0;

            if (printModel.AbWaran != null)
            {
                var bil = 1;

                var abwaranList1 = _context.AbWaran
                    .Include(t => t.AbWaranObjek)
                     .Where(w => w.Tahun == tahun);

                if (jKWId != 0)
                {
                    abwaranList1 = abwaranList1.Where(w => w.JKWId == jKWId);
                }

                if (jBahagianId != 0)
                {
                    abwaranList1 = abwaranList1.Where(w => w.AbWaranObjek.Any(obj => obj.JKWPTJBahagian.JBahagianId == jBahagianId));
                }

                var abwaranList = abwaranList1.ToList();

                var distinctObjeks = abwaranList
                    .SelectMany(g => g.AbWaranObjek)
                    .GroupBy(obj => new { obj.AbWaran.NoRujukan, obj.AbWaran.Tarikh, obj.AkCarta.Kod, obj.Amaun }) 
                    .Select(g => g.First());

                foreach (var objek in distinctObjeks)
                {
                    decimal asal = objek.Amaun; 
                    decimal tambah = 0; 
                    decimal pindah = 0; 
                    decimal jumlahRecord = asal + tambah + pindah;

                    string cleanedPerihal = objek.AkCarta?.Perihal.Trim();

                    dt.Rows.Add(
                        bil,
                        objek.AbWaran?.NoRujukan,
                        objek.AbWaran?.Tarikh.ToString("dd/MM/yyyy"),
                        objek.AkCarta?.Kod,
                        cleanedPerihal,
                        asal,
                        tambah,
                        pindah,
                        jumlahRecord
                    );

                    totalAsal += asal;
                    totalTambah += tambah;
                    totalPindah += pindah;
                    grandTotalJumlah += jumlahRecord;

                    bil++;
                }

                var grandTotalRow = dt.NewRow();
                grandTotalRow["Nama Akaun"] = "JUMLAH RM";
                grandTotalRow["Asal RM"] = totalAsal;
                grandTotalRow["Tambah RM"] = totalTambah;
                grandTotalRow["Pindah RM"] = totalPindah;
                grandTotalRow["Jumlah RM"] = grandTotalJumlah;
                dt.Rows.Add(grandTotalRow);
            }

            return dt;
        }

        private void RunWorkBookLAK00201(LAK018PrintModel printModel, DataTable excelData, string handle)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Laporan Waran Peruntukan");
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

        private DataTable GenerateDataTableLAK00202(LAK018PrintModel printModel, int? jKWId, int? jBahagianId, string? tahun)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Laporan Pindahan Peruntukan";
            dt.Columns.Add("Bil", typeof(int));
            dt.Columns.Add("No Rujukan", typeof(string));
            dt.Columns.Add("Tarikh", typeof(DateTime));
            dt.Columns.Add("Objek", typeof(string));
            dt.Columns.Add("Nama Akaun", typeof(string));
            dt.Columns.Add("Asal RM", typeof(decimal));
            dt.Columns.Add("Tambah RM", typeof(decimal));
            dt.Columns.Add("Pindah RM", typeof(decimal));
            dt.Columns.Add("Jumlah RM", typeof(decimal));

            decimal totalAsal = 0;
            decimal totalTambah = 0;
            decimal totalPindah = 0;
            decimal grandTotalJumlah = 0;

            if (printModel.AbWaran != null)
            {
                var bil = 1;

                var abwaranList1 = _context.AbWaran
                    .Include(t => t.AbWaranObjek)
                     .Where(w => w.Tahun == tahun && w.EnJenisPeruntukan == EnJenisPeruntukan.Viremen);

                if (jKWId != 0)
                {
                    abwaranList1 = abwaranList1.Where(w => w.JKWId == jKWId);
                }

                if (jBahagianId != 0)
                {
                    abwaranList1 = abwaranList1.Where(w => w.AbWaranObjek.Any(obj => obj.JKWPTJBahagian.JBahagianId == jBahagianId));
                }

                var abwaranList = abwaranList1.ToList();

                var distinctObjeks = abwaranList
                    .SelectMany(g => g.AbWaranObjek)
                    .GroupBy(obj => new { obj.AbWaran.NoRujukan, obj.AbWaran.Tarikh, obj.AkCarta.Kod, obj.Amaun }) 
                    .Select(g => g.First());

                foreach (var objek in distinctObjeks)
                {
                    decimal asal = 0; 
                    decimal tambah = 0; 
                    decimal pindah = objek.Amaun; 
                    decimal jumlahRecord = asal + tambah + pindah;

                    dt.Rows.Add(
                        bil,
                        objek.AbWaran?.NoRujukan,
                        objek.AbWaran?.Tarikh.ToString("dd/MM/yyyy"),
                        objek.AkCarta?.Kod,
                        objek.AkCarta?.Perihal,
                        asal,
                        tambah,
                        pindah,
                        jumlahRecord
                    );

                    totalAsal += asal;
                    totalTambah += tambah;
                    totalPindah += pindah;
                    grandTotalJumlah += jumlahRecord;

                    bil++;
                }

                var grandTotalRow = dt.NewRow();
                grandTotalRow["Nama Akaun"] = "JUMLAH RM";
                grandTotalRow["Asal RM"] = totalAsal;
                grandTotalRow["Tambah RM"] = totalTambah;
                grandTotalRow["Pindah RM"] = totalPindah;
                grandTotalRow["Jumlah RM"] = grandTotalJumlah; 
                dt.Rows.Add(grandTotalRow);
            }

            return dt;
        }

        private void RunWorkBookLAK00202(LAK018PrintModel printModel, DataTable excelData1, string handle)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Laporan Pindahan Peruntukan");
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
        private void PopulateSelectList(int? jKWId, int? jBahagianId)
        {
            var jKWList = _unitOfWork.JKWRepo.GetAllDetails();
            var kwSelect = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "-- SEMUA KW --",
                    Value = "0"
                }
            };

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

            ViewBag.JKW = selectList;

            var jBahagianList = _unitOfWork.JBahagianRepo.GetAllDetails();
            var bahagianSelect = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "-- SEMUA BAHAGIAN --",
                    Value = "0"
                }
            };

            if (jBahagianList != null && jBahagianList.Any())
            {
                bahagianSelect.AddRange(jBahagianList.Select(item => new SelectListItem
                {
                    Text = item.Kod + " - " + item.Perihal,
                    Value = item.Id.ToString()
                }));
            }
            else
            {
                bahagianSelect.Add(new SelectListItem
                {
                    Text = "-- Tiada Bahagian Berdaftar --",
                    Value = ""
                });
            }

            var selectList1 = new SelectList(bahagianSelect, "Value", "Text");

            if (jBahagianId.HasValue && jBahagianId.Value != 0)
            {
                var selectedItem = selectList.FirstOrDefault(x => x.Value == jBahagianId.ToString());
                if (selectedItem != null)
                {
                    selectedItem.Selected = true;
                }
            }

            ViewBag.JBahagian = selectList1;
        }

        // printing List of Laporan
        [AllowAnonymous]
        public async Task<IActionResult> Print(string? kodLaporan, int? jKWId, int? jBahagianId, string? tahun, EnJenisPeruntukan enJenisPeruntukan)
        {
            var abwaran = new List<AbWaran>();

            var reportModel = await PrepareData(kodLaporan, jKWId, jBahagianId, tahun, enJenisPeruntukan);
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

            if (jBahagianId.HasValue)
            {
                var jBahagianDetails = await _context.JBahagian
                                    .Where(j => j.Id == jBahagianId)
                                    .Select(j => new { j.Kod, j.Perihal })
                                    .FirstOrDefaultAsync();

                if (jBahagianDetails != null)
                {
                    ViewBag.jBahagianKod = jBahagianDetails.Kod;
                    ViewBag.jBahagianPerihal = jBahagianDetails.Perihal;
                }
            }

            if (kodLaporan == "LAK00201")
            {
                var reportModel1 = await PrepareData(kodLaporan, jKWId, jBahagianId, tahun, null);
                
                if (jKWId == 0)
                {
                    reportModel1 = await PrepareData(kodLaporan, null, jBahagianId, tahun, null);
                }
                if (jBahagianId == 0)
                {
                    reportModel1 = await PrepareData(kodLaporan, jKWId, null, tahun, null);
                }
                if (jKWId == 0 && jBahagianId == 0)
                {
                    reportModel1 = await PrepareData(kodLaporan, null, null, tahun, null);
                }

                return new ViewAsPdf("LAK00201PDF", reportModel1, new ViewDataDictionary(ViewData)
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
                var reportModel2 = await PrepareData(kodLaporan, jKWId, jBahagianId, tahun, EnJenisPeruntukan.Viremen);
                
                if (jKWId == 0)
                {
                    reportModel2 = await PrepareData(kodLaporan, null, jBahagianId, tahun, EnJenisPeruntukan.Viremen);
                }
                if (jBahagianId == 0)
                {
                    reportModel2 = await PrepareData(kodLaporan, jKWId, null, tahun, EnJenisPeruntukan.Viremen);
                }
                if (jKWId == 0 && jBahagianId == 0)
                {
                    reportModel2 = await PrepareData(kodLaporan, null, null, tahun, EnJenisPeruntukan.Viremen);
                }

                return new ViewAsPdf("LAK00202PDF", reportModel2, new ViewDataDictionary(ViewData)
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
                return new ViewAsPdf(modul + EnJenisFail.PDF, abwaran, new ViewDataDictionary(ViewData)
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