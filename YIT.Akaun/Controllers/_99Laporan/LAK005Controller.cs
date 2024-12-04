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
using YIT.__Domain.Entities.Models._01Jadual;
using YIT._DataAccess.Repositories.Implementations;
using Microsoft.AspNetCore.Mvc.Rendering;
using YIT._DataAccess.Services.Math;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Rotativa.AspNetCore;
using System.Dynamic;
using YIT.Akaun.Infrastructure;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using System.ComponentModel;
using YIT.__Domain.Entities._Statics;

namespace YIT.Akaun.Controllers._99Laporan
{
    [Authorize]
    public class LAK005Controller : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodLHasilBulanan;
        public const string namamodul = Modules.namaLHasilBulanan;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly UserServices _userServices;

        public LAK005Controller(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            _IUnitOfWork unitOfWork,
            IMemoryCache cache,
            UserServices userServices)
        {
            _context = context;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _cache = cache;
            _userServices = userServices;
        }
        public async Task<IActionResult> Index(PrintFormModel model)
        {
            PopulateSelectList(model.jKWId, model.JPTJId, model.jBahagianId, model.Tahun1, model.Bulan, model.JKWPTJBahagianId);


            model.Tahun1 ??= DateTime.Now.Year.ToString();
            model.Bulan ??= DateTime.Now.Month.ToString();


            var abWaran = new List<LAK005PrintModel>();

            if (model.jKWId != null || model.JPTJId != null || model.jBahagianId != null)
            {
                abWaran = await _unitOfWork.AkAnggarRepo.GetAkAnggarLejarToHasil(
                    model.Tahun1,
                    model.Bulan,
                    model.JPTJId,
                    model.jKWId,
                    model.jBahagianId,
                    model.JKWPTJBahagianId
                );
            }

            return View(model);
        }

        private void PopulateSelectList(int? JKWId, int? JPTJId, int? JBahagianId, string? Tahun1, string? Bulan, int? JKWPTJBahagianId)
        {

            // populate list JKW 
            List<JKW> jKWList = _unitOfWork.JKWRepo.GetAllDetails();

            var jkwSelect = new List<SelectListItem>();

            if (jKWList != null)
            {
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
            // populate list JPTJ 
            List<JPTJ> jptjList = _unitOfWork.JPTJRepo.GetAllDetails();

            var jptjSelect = new List<SelectListItem>();

            if (jptjList != null)
            {
                jptjSelect.Add(new SelectListItem()
                {
                    Text = "-- SEMUA PTJ --",
                    Value = ""
                });

                foreach (var item in jptjList)
                {
                    List<JKWPTJBahagian> jkwptjBahagianList = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsByJPTJId(item.Id);

                    var kodKW = "";

                    foreach (var item2 in jkwptjBahagianList)
                    {
                        kodKW = item2.JKW?.Kod;
                    }
                    jptjSelect.Add(new SelectListItem()
                    {
                        Text = BelanjawanFormatter.ConvertToPTJ(kodKW, item.Kod) + " - " + item.Perihal,
                        Value = item.Id.ToString()
                    });

                }
                ViewBag.JPTJ = new SelectList(jptjSelect, "Value", "Text", JPTJId);

            }
            else
            {
                jptjSelect.Add(new SelectListItem()
                {
                    Text = "-- Tiada PTJ Berdaftar --",
                    Value = ""
                });

                ViewBag.JPTJ = new SelectList(jptjSelect, "Value", "Text", null);
            }
            // populate list JPTJ end


            // populate list JBahagian 
            List<JBahagian> jBahagianList = _unitOfWork.JBahagianRepo.GetAllDetails();

            var jbahagianSelect = new List<SelectListItem>();

            if (jBahagianList != null)
            {
                foreach (var item in jBahagianList)
                {
                    jbahagianSelect.Add(new SelectListItem()
                    {
                        Text = item.Kod + " - " + item.Perihal,
                        Value = item.Id.ToString()
                    });
                }
                ViewBag.JBahagian = new SelectList(jbahagianSelect, "Value", "Text", JBahagianId);
            }
            else
            {
                jbahagianSelect.Add(new SelectListItem()
                {
                    Text = "-- Tiada Bahagian Berdaftar --",
                    Value = ""
                });

                ViewBag.JBahagian = new SelectList(jbahagianSelect, "Value", "Text", null);
            }
            // populate JBahagian end


            // populate  tarikhHingga
            if (Tahun1 != null)
            {
                ViewData["DateTo"] = Tahun1;
            }

            if (Bulan != null)
            {
                ViewData["Month"] = Bulan;
            }
        }

        [HttpPost]
        public async Task<JsonResult> ExportExcel(PrintFormModel model)
        {
            List<LAK005PrintModel> printModel = await PrepareData(model);

            // Generate a new unique identifier against which the file can be stored
            string handle = string.Format("attachment;" + model.kodLaporan + ".xlsx;", string.IsNullOrEmpty(model.kodLaporan) ? Guid.NewGuid().ToString() : WebUtility.UrlEncode(model.kodLaporan));

            // save viewmodel into workbook
            if (model.kodLaporan == "LAK005")
            {
                // construct and insert data into dataTable 
                var excelData = GenerateDataTableLAK005(printModel);

                // insert dataTable into Workbook
                RunWorkBookLAK005(model, excelData, handle);
            }

            return Json(new { FileGuid = handle, FileName = model.kodLaporan + ".xlsx" });
        }

        private async Task<List<LAK005PrintModel>> PrepareData(PrintFormModel form)
        {
            List<LAK005PrintModel> reportModel = new List<LAK005PrintModel>();

            if (form.kodLaporan == "LAK005")
            {
                form.Tajuk1 = $"Laporan Hasil Bulanan";
                reportModel = await _unitOfWork.AkAnggarRepo.GetAkAnggarLejarToHasil(form.Tahun1, form.Bulan, form.JPTJId, form.jKWId, form.jBahagianId, form.JKWPTJBahagianId);

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

        private DataTable GenerateDataTableLAK005(List<LAK005PrintModel> printModel)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Laporan Hasil Bulanan";
            dt.Columns.Add("Kod", typeof(string));
            dt.Columns.Add("Perihal", typeof(string));
            dt.Columns.Add("Anggaran Hasil", typeof(decimal));
            dt.Columns.Add("Hasil Bulanan", typeof(decimal));
            dt.Columns.Add("Hasil Terkumpul", typeof(decimal));
            dt.Columns.Add("Baki Anggaran", typeof(decimal));

            if (printModel != null && printModel.Any())
            {
                var groupedData = printModel.GroupBy(b => new { b.Kod, b.Perihal })
                    .Select(g =>
                    {
                        var AnggaranHasil = g.Sum(b => b.AnggaranHasil);
                        var HasilBulanan = g.Sum(b => b.HasilBulanan);
                        var HasilTerkumpul = g.Sum(b => b.HasilTerkumpul);
                        var BakiAnggaran = g.Sum(b => b.BakiAnggaran);

                        return new LAK005PrintModel
                        {
                            Kod = g.First().Kod,
                            Perihal = g.First().Perihal,
                            AnggaranHasil = AnggaranHasil,
                            HasilBulanan = HasilBulanan,
                            HasilTerkumpul = HasilTerkumpul,
                            BakiAnggaran = BakiAnggaran
                        };
                    })
                    .OrderBy(b => b.Kod);

                foreach (var akAkaun in groupedData)
                {
                    dt.Rows.Add(
                        akAkaun.Kod ?? string.Empty,
                        akAkaun.Perihal ?? string.Empty,
                        akAkaun.AnggaranHasil ?? 0,
                        akAkaun.HasilBulanan ?? 0,
                        akAkaun.HasilTerkumpul ?? 0,
                        akAkaun.BakiAnggaran ?? 0
                    );
                }

                // Calculate JUMLAH
                var JumlahAnggaranHasil = groupedData.Sum(x => x.AnggaranHasil);
                var JumlahHasilBulanan = groupedData.Sum(x => x.HasilBulanan);
                var JumlahHasilTerkumpul = groupedData.Sum(x => x.HasilTerkumpul);
                var JumlahBakiAnggaran = groupedData.Sum(x => x.BakiAnggaran);

                // Add JUMLAH
                dt.Rows.Add(
                    string.Empty, // Kod
                    "JUMLAH BESAR",  // Perihal
                    JumlahAnggaranHasil,
                    JumlahHasilBulanan,
                    JumlahHasilTerkumpul,
                    JumlahBakiAnggaran
                );
            }


            return dt;
        }



        private void RunWorkBookLAK005(PrintFormModel printModel, DataTable excelData, string handle)
        {
            List<JKW> jkwList = _unitOfWork.JKWRepo.GetAllDetails();
            List<JPTJ> jptjList = _unitOfWork.JPTJRepo.GetAllDetails();
            List<JBahagian> jBahagianList = _unitOfWork.JBahagianRepo.GetAllDetails();

            using (XLWorkbook wb = new XLWorkbook())
            {
                var jkw = jkwList.FirstOrDefault(j => j.Id == printModel.jKWId);
                var jptj = jptjList.FirstOrDefault(j => j.Id == printModel.JPTJId);
                var jbahagian = jBahagianList.FirstOrDefault(j => j.Id == printModel.jBahagianId);

                var ws = wb.AddWorksheet("Laporan Hasil Bulanan");
                ws.Cell("A1").Value = printModel.CompanyDetails?.NamaSyarikat;
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A2").Value = printModel.Tajuk1;
                if (printModel.jKWId != null && jkw != null)
                {
                    ws.Cell("A3").Value = $"{BelanjawanFormatter.ConvertToKW(jkw?.Kod) + " - " + jkw?.Perihal}";
                }
                if (printModel.JPTJId != null && jptj != null)
                {
                    ws.Cell("A4").Value = $"{BelanjawanFormatter.ConvertToPTJ(jkw?.Kod, jptj?.Kod) + " - " + jptj?.Perihal}";
                }
                if (printModel.jBahagianId != null && jbahagian != null)
                {
                    ws.Cell("A5").Value = $"{jbahagian?.Kod + " - " + jbahagian?.Perihal}";
                }

                ws.ColumnWidth = 5;
                ws.Cell("A8").InsertTable(excelData)
                    .Theme = XLTableTheme.TableStyleMedium1;

                ws.Column(1).AdjustToContents();
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

                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveAs(ms);

                    //This is an equivalent to tempdata, but requires manual cleanup
                    _cache.Set(handle, ms.ToArray(),
                                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));


                }
            }
        }

        public async Task<IActionResult> PrintPDF(PrintFormModel form)
        {
            var reportModel = await PrepareData(form);


            var company = await _userServices.GetCompanyDetails();
            var hasil = new List<LAK005PrintModel>();


            hasil = await _unitOfWork.AkAnggarRepo.GetAkAnggarLejarToHasil(form.Tahun1, form.Bulan, form.JPTJId, form.jKWId, form.jBahagianId, form.JKWPTJBahagianId);


            dynamic dyModel = new ExpandoObject();
            dyModel.ReportModel = hasil;
            dyModel.reportModelGrouped = hasil.GroupBy(b => b.Kod);

            var jkw = await _context.JKW.FirstOrDefaultAsync(b => b.Id == form.jKWId);

            var jptj = await _context.JPTJ.FirstOrDefaultAsync(ptj => ptj.Id == form.JPTJId);
            var jbahagian = await _context.JBahagian.FirstOrDefaultAsync(jbahagian => jbahagian.Id == form.jBahagianId);

            PopulateSelectList(form.jKWId, form.JPTJId, form.jBahagianId, form.Tahun1, form.Bulan, form.JKWPTJBahagianId);

            return new ViewAsPdf("LAK005PDF", dyModel, new ViewDataDictionary(ViewData) {
                     { "NamaSyarikat", company.NamaSyarikat },
                     { "AlamatSyarikat1", company.AlamatSyarikat1 },
                     { "AlamatSyarikat2", company.AlamatSyarikat2 },
                     { "AlamatSyarikat3", company.AlamatSyarikat3 },
                     { "Tahun", form.Tahun1 },
                     { "Bulan", form.Bulan },
                     { "NamaKW", BelanjawanFormatter.ConvertToKW(jkw?.Kod) + " - " + jkw?.Perihal },
                     { "NamaPTJ", BelanjawanFormatter.ConvertToPTJ(jkw?.Kod,jptj?.Kod) + " - " + jptj?.Perihal },
                     { "NamaBahagian", jbahagian?.Kod + " - " + jbahagian?.Perihal },

                 })
            {
                PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                CustomSwitches = "--footer-center \"[page]/[toPage]\"" +
                        " --footer-line --footer-font-size \"7\" --footer-spacing 1 --footer-font-name \"Segoe UI\"",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
            };

        }
    
        }
    }


