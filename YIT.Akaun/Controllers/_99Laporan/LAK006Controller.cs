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
using YIT.Akaun.Controllers._03Akaun;
using System.Collections.Generic;

namespace YIT.Akaun.Controllers._99Laporan
{
    [Authorize]
    public class LAK006Controller : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodLPendapatanTahunan;
        public const string namamodul = Modules.namaLPendapatanTahunan;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly ILaporanRepository _laporanRepository;
        private readonly UserServices _userServices;

        public LAK006Controller(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            _IUnitOfWork unitOfWork,
            IMemoryCache cache,
            ILaporanRepository laporanRepository,
            UserServices userServices
            )
        {
            _context = context;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _cache = cache;
            _laporanRepository = laporanRepository;
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
            
            List<LAK006PrintModel> printModel = await PrepareData(model);

            // Generate a new unique identifier against which the file can be stored
            string handle = string.Format("attachment;" + model.kodLaporan + ".xlsx;", string.IsNullOrEmpty(model.kodLaporan) ? Guid.NewGuid().ToString() : WebUtility.UrlEncode(model.kodLaporan));


            if (model.kodLaporan == "LAK006")
            {
                // construct and insert data into dataTable 
                var excelData = await GenerateDataTableLAK006(model);

                // insert dataTable into Workbook
                RunWorkBookLAK006(model, excelData, handle);
            }


            return Json(new { FileGuid = handle, FileName = model.kodLaporan + ".xlsx" });
        }

        private async Task<List<LAK006PrintModel>> PrepareData(PrintFormModel form)
        {
            List<LAK006PrintModel> reportModel = new List<LAK006PrintModel>();


            if (form.kodLaporan == "LAK006")
            {

                form.Tajuk1 = $"Laporan Pendapatan dan Perbelanjaan Pada Tahun {form.Tahun1}";
                reportModel = await _laporanRepository.GetResultPendapatanTahunan(form.Tahun1, form.jKWId, (int)form.AkBankId!);

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


        public async Task<DataTable> GenerateDataTableLAK006(PrintFormModel model)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Laporan Pendapatan";
            dt.Columns.Add("Jenis", typeof(string));
            dt.Columns.Add("Kod Akaun", typeof(string));
            dt.Columns.Add("Nama Akaun", typeof(string));
            dt.Columns.Add("Jumlah", typeof(decimal));
            dt.Columns.Add("Januari", typeof(decimal));
            dt.Columns.Add("Februari", typeof(decimal));
            dt.Columns.Add("Mac", typeof(decimal));
            dt.Columns.Add("April", typeof(decimal));
            dt.Columns.Add("Mei", typeof(decimal));
            dt.Columns.Add("Jun", typeof(decimal));
            dt.Columns.Add("Julai", typeof(decimal));
            dt.Columns.Add("Ogos", typeof(decimal));
            dt.Columns.Add("September", typeof(decimal));
            dt.Columns.Add("Oktober", typeof(decimal));
            dt.Columns.Add("November", typeof(decimal));
            dt.Columns.Add("Disember", typeof(decimal));

            decimal? jan = 0;
            decimal? feb = 0;
            decimal? mac = 0;
            decimal? apr = 0;
            decimal? mei = 0;
            decimal? jun = 0;
            decimal? jul = 0;
            decimal? ogo = 0;
            decimal? sep = 0;
            decimal? okt = 0;
            decimal? nov = 0;
            decimal? dis = 0;
            decimal? jum = 0;



            var belanjaList = await _laporanRepository.GetResultPendapatanTahunan(model.Tahun1, model.jKWId, (int)model.AkBankId!);

            if (belanjaList != null && belanjaList.Any())
            {
                string currentJenis = belanjaList.First().Jenis!;

                // Add the title row for the first group at the start
                dt.Rows.Add(
                    DBNull.Value,
                    DBNull.Value,
                    currentJenis == "H" ? "PENDAPATAN" : "PERBELANJAAN",  // Title for the first group
                    DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value,
                    DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value,
                    DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value
                );

                foreach (var hasil in belanjaList)
                {
                    
                    // If Jenis changes, add a total row for the previous Jenis
                    if (hasil.Jenis != currentJenis)
                    {

                        dt.Rows.Add(
                            DBNull.Value,
                            DBNull.Value,
                            currentJenis == "H" ? "JUMLAH PENDAPATAN RM" : "JUMLAH PERBELANJAAN RM",
                            jum,
                            jan,
                            feb,
                            mac,
                            apr,
                            mei,
                            jun,
                            jul,
                            ogo,
                            sep,
                            okt,
                            nov,
                            dis
                        );


                        // Reset totals for the new group
                        jum = jan = feb = mac = apr = mei = jun = jul = ogo = sep = okt = nov = dis = 0;
                        currentJenis = hasil.Jenis!;

                        // Add the title row for the first group at the start
                        dt.Rows.Add(
                            DBNull.Value,
                            DBNull.Value,
                            currentJenis == "H" ? "PENDAPATAN" : "PERBELANJAAN",  // Title for the first group
                            DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value,
                            DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value,
                            DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value
                        );
                    }

                dt.Rows.Add(
                        hasil.Jenis,
                        hasil.KodAkaun,
                        hasil.NamaAkaun,
                        hasil.Jumlah,
                        hasil.Jan ?? 0,
                        hasil.Feb ?? 0,
                        hasil.Mac ?? 0,
                        hasil.Apr ?? 0,
                        hasil.Mei ?? 0,
                        hasil.Jun ?? 0,
                        hasil.Jul ?? 0,
                        hasil.Ogos ?? 0,
                        hasil.Sep ?? 0,
                        hasil.Okt ?? 0,
                        hasil.Nov ?? 0,
                        hasil.Dis ?? 0
                    );


                    // Update running totals
                    jum += hasil.Jumlah;
                    jan += hasil.Jan;
                    feb += hasil.Feb;
                    mac += hasil.Mac;
                    apr += hasil.Apr;
                    mei += hasil.Mei;
                    jun += hasil.Jun;
                    jul += hasil.Jul;
                    ogo += hasil.Ogos;
                    sep += hasil.Sep;
                    okt += hasil.Okt;
                    nov += hasil.Nov;
                    dis += hasil.Dis;
                }
               
                // Add the final total row for the last group
                dt.Rows.Add(
                    DBNull.Value,
                    DBNull.Value,
                    currentJenis == "H" ? "JUMLAH PENDAPATAN RM" : "JUMLAH PERBELANJAAN RM",
                    jum,
                    jan,
                    feb,
                    mac,
                    apr,
                    mei,
                    jun,
                    jul,
                    ogo,
                    sep,
                    okt,
                    nov,
                    dis
                );

                dt.Columns.Remove("Jenis");


               
            }

            return dt;
        }


            private void RunWorkBookLAK006(PrintFormModel printModel, DataTable excelData, string handle)
        {
            List<JKW> jkwList = _unitOfWork.JKWRepo.GetAllDetails();
            List<AkBank> akbankList = _unitOfWork.AkBankRepo.GetAllDetails();

            using (XLWorkbook wb = new XLWorkbook())
            {
                var jkw = jkwList.FirstOrDefault(j => j.Id == printModel.jKWId);
                var akbank = akbankList.FirstOrDefault(j => j.Id == printModel.AkBankId);

                var ws = wb.AddWorksheet("Laporan Pendapatan");
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

                ws.ColumnWidth = 8;
                ws.Cell("A7").InsertTable(excelData)
                    .Theme = XLTableTheme.TableStyleMedium1;

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
                     .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(10).AdjustToContents();
                ws.Column(11)
                     .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(11).AdjustToContents();
                ws.Column(12)
                     .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(12).AdjustToContents();
                ws.Column(13)
                     .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(13).AdjustToContents();
                ws.Column(14)
                     .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(14).AdjustToContents();
                ws.Column(15)
                     .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(15).AdjustToContents();
                ws.Column(16)
                    .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(16).AdjustToContents();
                ws.Column(17)
                    .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(17).AdjustToContents();


                //foreach (var row in ws.RowsUsed())
                //{

                //    // Make the row bold
                //    row.Cells(2, 2).Style.Font.Bold = true;

                //}

                foreach (var row in ws.RowsUsed())
                {
                    if (row.Cell(2).Value.ToString().StartsWith("JUMLAH PENDAPATAN RM"))
                    {
                        // Change background color
                        row.Cells(1, 15).Style.Fill.BackgroundColor = XLColor.LightSkyBlue;


                        // Make the row bold
                        row.Cells(1, 15).Style.Font.Bold = true;
                    }
                }

                foreach (var row in ws.RowsUsed())
                {
                    if (row.Cell(2).Value.ToString().StartsWith("JUMLAH PERBELANJAAN RM"))
                    {
                        // Change background color
                        row.Cells(1, 15).Style.Fill.BackgroundColor = XLColor.LightSkyBlue;


                        // Make the row bold
                        row.Cells(1, 15).Style.Font.Bold = true;
                    }
                }

                using MemoryStream ms = new MemoryStream();
                wb.SaveAs(ms);

                //This is an equivalent to tempdata, but requires manual cleanup
                _cache.Set(handle, ms.ToArray(),
                            new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));
            }
        }


        // printing List of Laporan
        [AllowAnonymous]
        public async Task<IActionResult> PrintPDF(PrintFormModel form)
        {
            var abWaran = new List<LAK006PrintModel>();
            var reportModel = await PrepareData(form);
            var company = await _userServices.GetCompanyDetails();


            await PrepareData(form);

            var jkw = await _context.JKW.FirstOrDefaultAsync(b => b.Id == form.jKWId);

            var bank = await _context.AkBank
                    .Include(b => b.AkCarta)
                    .FirstOrDefaultAsync(b => b.Id == form.AkBankId);


            abWaran = await _laporanRepository.GetResultPendapatanTahunan(form.Tahun1, form.jKWId, (int)form.AkBankId!);
            
            dynamic dyModel = new ExpandoObject();
            dyModel.ReportModel = abWaran;
            dyModel.reportModelGrouped = abWaran.GroupBy(b => b.Jenis);


            return new ViewAsPdf("LAK006PDF", dyModel, new ViewDataDictionary(ViewData) {
                { "NamaSyarikat", company.NamaSyarikat },
                { "AlamatSyarikat1", company.AlamatSyarikat1 },
                { "AlamatSyarikat2", company.AlamatSyarikat2 },
                { "AlamatSyarikat3", company.AlamatSyarikat3 },
                { "Tahun", form.Tahun1 },
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
