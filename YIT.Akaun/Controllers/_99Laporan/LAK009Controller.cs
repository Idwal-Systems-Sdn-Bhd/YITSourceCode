using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Rotativa.AspNetCore;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Net;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services.Math;
using YIT.Akaun.Infrastructure;
using YIT.Akaun.Models.ViewModels.Forms;
using YIT.Akaun.Models.ViewModels.Prints;

namespace YIT.Akaun.Controllers._99Laporan
{
    [Authorize]
    public class LAK009Controller : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodLPembayaranIkutJulatTertentu;
        public const string namamodul = Modules.namaLPembayaranIkutJulatTertentu;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly UserServices _userServices;

        public LAK009Controller(
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

            PopulateSelectList(model.AkBankId, model.jKWId, model.tarDari1, model.tarHingga1);


            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> ExportExcel(PrintFormModel model)
        {
            //int akBankId = 0;
            List<LAK009PrintModel> printModel = await PrepareData(model);

            // Generate a new unique identifier against which the file can be stored
            string handle = string.Format("attachment;" + model.kodLaporan + ".xlsx;", string.IsNullOrEmpty(model.kodLaporan) ? Guid.NewGuid().ToString() : WebUtility.UrlEncode(model.kodLaporan));


            if (model.kodLaporan == "LAK009")
            {
                // construct and insert data into dataTable 
                var excelData = GenerateDataTableLAK009(printModel);

                // insert dataTable into Workbook
                RunWorkBookLAK009(model, excelData, printModel, handle);
            }


            return Json(new { FileGuid = handle, FileName = model.kodLaporan + ".xlsx" });
        }

        private async Task<List<LAK009PrintModel>> PrepareData(PrintFormModel form)
        {
            List<LAK009PrintModel> reportModel = new List<LAK009PrintModel>();


            if (form.kodLaporan == "LAK009")
            {

                form.Tajuk1 = $"Laporan Pembayaran Mengikut Julat Tertentu Dari {@Convert.ToDateTime(form.tarDari1).ToString("dd/MM/yyyy")} Hingga {@Convert.ToDateTime(form.tarHingga1):dd/MM/yyyy}";
                reportModel = await _unitOfWork.AkPVRepo.GetResultLAK009(form.tarDari1, form.tarHingga1, form.jKWId, (int)form.AkBankId!);
            }


            var user = await _userManager.GetUserAsync(User);
            var namaUser = await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.Email == user!.Email);

            form.Username = namaUser?.Nama;

            var KodLaporan = form.kodLaporan;
            CompanyDetails company = new CompanyDetails();
            form.CompanyDetails = company;

            DateTime? date1 = null;
            DateTime? date2 = null;

            if (!string.IsNullOrEmpty(form.tarikhDari) && !string.IsNullOrEmpty(form.tarikhHingga))
            {
                date1 = DateTime.Parse(form.tarikhDari);
                date2 = DateTime.Parse(form.tarikhHingga);
            }


            return reportModel;
        }

        private void PopulateSelectList(int? AkBankId, int? JKWId, DateTime? tarDari1, DateTime? tarHingga1)
        {
            // populate list bank 
            List<AkBank> akBankList = _unitOfWork.AkBankRepo.GetAllDetails();

            var bankSelect = new List<SelectListItem>();

            if (akBankList != null)
            {
                foreach (var item in akBankList)
                {
                    bankSelect.Add(new SelectListItem()
                    {
                        Text = item.NoAkaun + " (" + item.AkCarta?.Kod + " - " + item.AkCarta?.Perihal + ")",
                        Value = item.Id.ToString()
                    });
                }
                ViewBag.AkBank = new SelectList(bankSelect, "Value", "Text", AkBankId);
            }
            else
            {
                bankSelect.Add(new SelectListItem()
                {
                    Text = "-- Tiada Bank Berdaftar --",
                    Value = ""
                });

                ViewBag.AkBank = new SelectList(bankSelect, "Value", "Text", null);
            }
            // populate list bank end

            // populate list JKW 
            List<JKW> jKWList = _unitOfWork.JKWRepo.GetAllDetails();

            var jkwSelect = new List<SelectListItem>();

            if (jKWList != null)
            {
                jkwSelect.Add(new SelectListItem()
                {
                    Text = "-- SEMUA KW --",
                    Value = ""
                });

                foreach (var item in jKWList)
                {
                    jkwSelect.Add(new SelectListItem()
                    {
                        Text = BelanjawanFormatter.ConvertToKW(item.Kod) + " - " + item.Perihal,
                        Value = item.Id.ToString()
                    });
                }
                ViewBag.JKW = new SelectList(jkwSelect, "Value", "Text", JKWId);
            }
            else
            {
                jkwSelect.Add(new SelectListItem()
                {
                    Text = "-- Tiada Kump. Wang Berdaftar --",
                    Value = ""
                });

                ViewBag.JKW = new SelectList(jkwSelect, "Value", "Text", null);
            }
            // populate list jkw end



            // populate tarikhDari and tarikhHingga
            if (tarDari1 != null)
            {
                ViewData["DateFrom"] = tarDari1?.ToString("yyyy-MM-dd");
                ViewData["DateTo"] = tarHingga1?.ToString("yyyy-MM-dd");
            }
        }


        private DataTable GenerateDataTableLAK009(List<LAK009PrintModel> printModel)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Laporan Pembayaran Mengikut Julat Tertentu";
            dt.Columns.Add("Bil", typeof(int));
            dt.Columns.Add("Tarikh Cek", typeof(DateTime));
            dt.Columns.Add("No Baucer", typeof(string));
            dt.Columns.Add("Perihal", typeof(string));
            dt.Columns.Add("Amaun RM", typeof(decimal));
            dt.Columns.Add("No Cek", typeof(string));
            dt.Columns.Add("Penerima Cek", typeof(string));


            decimal? jumlah = 0; 
            decimal? jumlahEFT = 0; 
            decimal? jumlahCek = 0;

            if (printModel != null)
            {

                var bil = 1;
                DateTime? lastTarikhPV = null;
                string? lastNoRujukanPV = null;
                string? lastPerihal = null;


                var groupedData = printModel
                    .GroupBy(b => new { b.TarikhPV, b.NoRujukanPV, b.Perihal })
                    .Select(group => new
                    {
                        Date = group.Key.TarikhPV,
                        NoRujukanPV = group.Key.NoRujukanPV,
                        Perihal = group.Key.Perihal,
                        GroupItems = group.ToList(),
                        TotalAmount = group.Sum(item => item.Amaun)
                    })
                    .OrderBy(group => group.Date)
                    .ThenBy(group => group.NoRujukanPV)
                    .ThenBy(group => group.Perihal);

                foreach (var group in groupedData)
                {
                    foreach (var item in group.GroupItems)
                    {
                        if (item.Amaun != 0)
                        {

                            // Check if NoRujukanPV or Perihal has changed, otherwise set DBNull.Value
                            object? tarikhPVToShow = lastTarikhPV != item.TarikhPV ? item.TarikhPV : DBNull.Value;
                            object? noRujukanToShow = lastNoRujukanPV != item.NoRujukanPV ? item.NoRujukanPV : DBNull.Value;
                            object? perihalToShow = lastPerihal != item.Perihal ? item.Perihal : DBNull.Value;


                            dt.Rows.Add(
                                bil,
                                tarikhPVToShow,
                                noRujukanToShow,
                                perihalToShow,
                                item.Amaun,
                                item.NoRujukanCek,
                                item.PenerimaCek
                            );

                            // Update the last shown values
                            lastTarikhPV = item.TarikhPV;
                            lastNoRujukanPV = item.NoRujukanPV;
                            lastPerihal = item.Perihal;

                            bil++;

                            jumlah += item.Amaun;
                            if (item.NoRujukanCek == "EFT")
                            {
                                jumlahEFT += item.Amaun;
                            }
                            else
                            {
                                jumlahCek += item.Amaun;
                            }
                        }
                    }
                }
            }

            // Add summary rows to the DataTable
            dt.Rows.Add(
                DBNull.Value, 
                DBNull.Value,
                DBNull.Value,
                "JUMLAH KESELURUHAN",
                jumlah,
                DBNull.Value,
                DBNull.Value
            );

            dt.Rows.Add(
                DBNull.Value,
                DBNull.Value,
                DBNull.Value,
                "JUMLAH PEMBAYARAN MENGGUNAKAN ELECTRONIC FUND TRANSFER (EFT)",
                jumlahEFT,
                DBNull.Value,
                DBNull.Value
            );

            dt.Rows.Add(
                DBNull.Value,
                DBNull.Value,
                DBNull.Value,
                "JUMLAH PEMBAYARAN MENGGUNAKAN CEK",
                jumlahCek,
                DBNull.Value,
                DBNull.Value
            );

            return dt;
        }

       
        private DataTable GenerateAkCartaDataTable(List<LAK009PrintModel> akPvResult)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Ringkasan";
            dt.Columns.Add("Bil", typeof(int));
            dt.Columns.Add("Kod", typeof(string));
            dt.Columns.Add("Perihal", typeof(string));
            dt.Columns.Add("Jumlah", typeof(decimal));

            decimal? jumlah = 0;

            if (akPvResult != null)
            {
                var bil = 1;

                // Group by akCartaKod to accumulate totals
                var groupedData = akPvResult
                    .Where(item => item.akCartaKod != null)
                    .GroupBy(item => new { item.akCartaKod, item.akCartaPerihal })
                    .Select(group => new
                    {
                        AkCartaKod = group.Key.akCartaKod,
                        AkCartaPerihal = group.Key.akCartaPerihal,
                        Jumlah = group.Sum(item => item.Jumlah)
                    })
                    .OrderBy(group => group.AkCartaKod);

                foreach (var group in groupedData)
                {
                    
                    dt.Rows.Add(
                        bil++, 
                        group.AkCartaKod,
                        group.AkCartaPerihal,
                        group.Jumlah
                    );

                    jumlah += group.Jumlah;
                }
            }

            dt.Rows.Add(
               DBNull.Value,
               DBNull.Value,
               "JUMLAH",
               jumlah
           );

            return dt;
        }


        private void RunWorkBookLAK009(PrintFormModel printModel, DataTable excelData, List<LAK009PrintModel> akPVResult, string handle)
        {
            List<JKW> jkwList = _unitOfWork.JKWRepo.GetAllDetails();
            List<AkBank> akbankList = _unitOfWork.AkBankRepo.GetAllDetails();

            using (XLWorkbook wb = new XLWorkbook())
            {
                var jkw = jkwList.FirstOrDefault(j => j.Id == printModel.jKWId);
                var akbank = akbankList.FirstOrDefault(j => j.Id == printModel.AkBankId);

                var ws = wb.AddWorksheet("Laporan Pembayaran Ikut Julat");
                ws.Cell("A1").Value = printModel.CompanyDetails?.NamaSyarikat;
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A2").Value = printModel.Tajuk1;
                if (printModel.jKWId != null && jkw != null)
                {
                    ws.Cell("A3").Value = $"{BelanjawanFormatter.ConvertToKW(jkw?.Kod) + " - " + jkw?.Perihal}";
                }
                if (printModel.AkBankId != null && akbankList != null)
                {
                    ws.Cell("A4").Value = $"{akbank?.NoAkaun + " (" + akbank?.AkCarta?.Kod + " - " + akbank?.AkCarta?.Perihal}";
                }

                ws.ColumnWidth = 5;
                ws.Cell("A7").InsertTable(excelData)
                    .Theme = XLTableTheme.TableStyleMedium1;

                ws.Column(2).AdjustToContents();
                ws.Column(3)
                   .Style.DateFormat.Format = "dd/MM/yyyy hh:mm:ss";
                ws.Column(3).AdjustToContents();
                ws.Column(4).AdjustToContents();
                ws.Column(5)
                    .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(5).AdjustToContents();
                ws.Column(6).AdjustToContents();
                ws.Column(7).AdjustToContents();


                foreach (var row in ws.RowsUsed())
                {
                    
                        // Make the row bold
                        row.Cells(2, 2).Style.Font.Bold = true;
                    
                }

                foreach (var row in ws.RowsUsed())
                {
                    if (row.Cell(4).Value.ToString().StartsWith("JUMLAH KESELURUHAN"))
                    {
                        // Change background color
                        row.Cells(4, 5).Style.Fill.BackgroundColor = XLColor.LightSkyBlue;

                        row.Cells(1, 3).Style.Fill.BackgroundColor = XLColor.Transparent;

                        row.Cells(6, 7).Style.Fill.BackgroundColor = XLColor.Transparent;

                        // Make the row bold
                        row.Cells(1, 5).Style.Font.Bold = true;
                    }
                }

                foreach (var row in ws.RowsUsed())
                {
                    if (row.Cell(4).Value.ToString().StartsWith("JUMLAH PEMBAYARAN MENGGUNAKAN ELECTRONIC FUND TRANSFER (EFT)"))
                    {
                        // Change background color
                        row.Cells(4, 5).Style.Fill.BackgroundColor = XLColor.Peach;

                        row.Cells(1, 3).Style.Fill.BackgroundColor = XLColor.Transparent;

                        row.Cells(6, 7).Style.Fill.BackgroundColor = XLColor.Transparent;

                        // Make the row bold
                        row.Cells(1, 5).Style.Font.Bold = true;
                    }
                }

                foreach (var row in ws.RowsUsed())
                {
                    if (row.Cell(4).Value.ToString().StartsWith("JUMLAH PEMBAYARAN MENGGUNAKAN CEK"))
                    {
                        // Change background color
                        row.Cells(4, 5).Style.Fill.BackgroundColor = XLColor.LightGreen;

                        row.Cells(1, 3).Style.Fill.BackgroundColor = XLColor.Transparent;

                        row.Cells(6, 7).Style.Fill.BackgroundColor = XLColor.Transparent;

                        // Make the row bold
                        row.Cells(1, 5).Style.Font.Bold = true;
                    }
                }



                
                DataTable akCartaDataTable = GenerateAkCartaDataTable(akPVResult);

                // Add new worksheet for AkCarta results
                var akCartaWs = wb.AddWorksheet("Ringkasan");
                akCartaWs.Cell("A1").Value = "RINGKASAN"; // Title for the new sheet
                akCartaWs.Cell("A1").Style.Font.Bold = true;

                akCartaWs.Cell("A2").Value = printModel.CompanyDetails?.NamaSyarikat;
                akCartaWs.Cell("A2").Style.Font.Bold = true;
                akCartaWs.Cell("A3").Value = printModel.Tajuk1;
                if (printModel.jKWId != null && jkw != null)
                {
                    akCartaWs.Cell("A4").Value = $"{BelanjawanFormatter.ConvertToKW(jkw?.Kod) + " - " + jkw?.Perihal}";
                }
                if (printModel.AkBankId != null && akbankList != null)
                {
                    akCartaWs.Cell("A5").Value = $"{akbank?.NoAkaun + " (" + akbank?.AkCarta?.Kod + " - " + akbank?.AkCarta?.Perihal}";
                }

                // Insert the AkCarta DataTable into the new worksheet
                akCartaWs.Cell("A7").InsertTable(akCartaDataTable)
                    .Theme = XLTableTheme.TableStyleMedium1;

                // Adjust column widths for the new worksheet
                akCartaWs.Column(2).AdjustToContents();
                akCartaWs.Column(3).AdjustToContents();
                akCartaWs.Column(4).AdjustToContents();
                akCartaWs.Column(4)
                     .Style.NumberFormat.Format = " #,##0.00";
                akCartaWs.Column(4).AdjustToContents();

                foreach (var row1 in akCartaWs.RowsUsed())
                {
                    if (row1.Cell(3).Value.ToString().StartsWith("JUMLAH"))
                    {
                        // Change background color
                        row1.Cells(2, 4).Style.Fill.BackgroundColor = XLColor.LightGreen;

                        row1.Cells(1, 2).Style.Fill.BackgroundColor = XLColor.Transparent;

                        // Make the row bold
                        row1.Cells(1, 4).Style.Font.Bold = true;
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

        // printing List of Laporan
        [AllowAnonymous]
        public async Task<IActionResult> PrintPDF(PrintFormModel form)
        {
            var akpv = new List<LAK009PrintModel>();
            var reportModel = await PrepareData(form);
            var company = await _userServices.GetCompanyDetails();

                
            await PrepareData(form);

            var jkw = await _context.JKW.FirstOrDefaultAsync(b => b.Id == form.jKWId);

            var bank = await _context.AkBank
                    .Include(b => b.AkCarta)
                    .FirstOrDefaultAsync(b => b.Id == form.AkBankId);


            akpv = await _unitOfWork.AkPVRepo.GetResultLAK009(form.tarDari1, form.tarHingga1, form.jKWId, (int)form.AkBankId!);

            dynamic dyModel = new ExpandoObject();
            dyModel.ReportModel = akpv;
            dyModel.reportModelGrouped = akpv.GroupBy(b => b.TarikhPV);


            return new ViewAsPdf("LAK009PDF", dyModel, new ViewDataDictionary(ViewData) {
                { "NamaSyarikat", company.NamaSyarikat },
                { "AlamatSyarikat1", company.AlamatSyarikat1 },
                { "AlamatSyarikat2", company.AlamatSyarikat2 },
                { "AlamatSyarikat3", company.AlamatSyarikat3 },
                { "TarikhDari", form.tarDari1?.ToString("dd/MM/yyyy hh:mm:ss tt") },
                { "TarikhHingga", form.tarHingga1?.AddHours(23.99).ToString("dd/MM/yyyy hh:mm:ss tt") },
                { "NamaKW", BelanjawanFormatter.ConvertToKW(jkw?.Kod) + " - " + jkw?.Perihal  },
                { "NamaBank", bank?.NoAkaun + " (" + bank?.AkCarta?.Kod + " - " + bank?.AkCarta?.Perihal +") " },

            })
                {
                    PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                    PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                    //CustomSwitches = "--footer-center \"[page]/[toPage]\"" +
                        //" --footer-line --footer-font-size \"7\" --footer-spacing 1 --footer-font-name \"Segoe UI\"",
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                };
            }
           
    }
}
