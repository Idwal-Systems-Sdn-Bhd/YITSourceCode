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
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Rotativa.AspNetCore;
using System.Dynamic;
using YIT.__Domain.Entities._Enums;
using YIT.Akaun.Infrastructure;
using DocumentFormat.OpenXml.Drawing.Charts;
using DataTable = System.Data.DataTable;
using YIT.__Domain.Entities.Models._01Jadual;
using Microsoft.AspNetCore.Mvc.Rendering;
using YIT._DataAccess.Services.Math;
using YIT.__Domain.Entities._Statics;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using YIT._DataAccess.Services;
using System.Collections.Generic;
using System.Reflection.Metadata;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using System.Linq.Expressions;
using DocumentFormat.OpenXml.Spreadsheet;

namespace YIT.Akaun.Controllers._99Laporan
{
    [Authorize]
    public class LAK004Controller : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodLBelanjawanTerkini;
        public const string namamodul = Modules.namaLBelanjawanTerkini;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly ILaporanRepository _laporanRepository;
        private readonly UserServices _userServices;

        public LAK004Controller(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            _IUnitOfWork unitOfWork,
            IMemoryCache cache,
            ILaporanRepository laporanRepository,
            UserServices userServices)
        {
            _context = context;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _cache = cache;
            _laporanRepository = laporanRepository;
            _userServices = userServices;
        }
        public async Task<IActionResult> Index(PrintFormModel model)
        {
            PopulateSelectList(model.jKWId, model.JPTJId, model.Tahun1, model.Bulan, model.jBahagianId);

            var abWaran = new List<LAK004PrintModel>();

            if (model.Tahun1 == null)
            {
                model.Tahun1 = DateTime.Now.Year.ToString();
            }

            if (model.Bulan == null)
            {
                model.Bulan = DateTime.Now.Month.ToString();
            }


            if (model.jKWId != null && model.jBahagianId != null)
            {

                abWaran = await _laporanRepository.GetAbWaranBasedOnYear(model.jKWId, model.JPTJId, model.Tahun1, model.jBahagianId);
            }
            //
            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> ExportExcel(PrintFormModel model)
        {
            // Fetch AbWaran data based on the provided model parameters
            List<LAK004PrintModel> abWaranList = await PrepareData(model);

            // Generate a new unique identifier against which the file can be stored
            string handle = string.Format("attachment;" + model.kodLaporan + ".xlsx;", string.IsNullOrEmpty(model.kodLaporan) ? Guid.NewGuid().ToString() : WebUtility.UrlEncode(model.kodLaporan));

            // save viewmodel into workbook
            if (model.kodLaporan == "LAK00401")
            {
                // construct and insert data into dataTable 
                var excelData = GenerateDataTableLAK00401(abWaranList);

                // insert dataTable into Workbook
                RunWorkBookLAK00401(model, excelData, handle);
            }

            // save viewmodel into workbook
            if (model.kodLaporan == "LAK00402")
            {
                // construct and insert data into dataTable 
                var excelData = GenerateDataTableLAK00402(abWaranList);

                // insert dataTable into Workbook
                RunWorkBookLAK00402(model, excelData, handle);
            }


            return Json(new { FileGuid = handle, FileName = model.kodLaporan + ".xlsx" });
        }


        private async Task<List<LAK004PrintModel>> PrepareData(PrintFormModel form)
        {
            //LAK004PrintModel reportModel = new LAK004PrintModel();

            List<LAK004PrintModel> abWaranList = new List<LAK004PrintModel>();

            if (form.kodLaporan == "LAK00401")
            {

                form.Tajuk1 = $"Laporan Belanjawan Terkini";
                abWaranList = await _laporanRepository.GetAbWaranBasedOnYear(form.jKWId, form.JPTJId, form.Tahun1, form.jBahagianId);

            }
            if (form.kodLaporan == "LAK00402")
            {

                form.Tajuk1 = $"Laporan Belanjawan Mengikut Bulan";
                abWaranList = await _laporanRepository.GetAbWaranBasedOnYearAndMonth(form.jKWId, form.JPTJId, form.Tahun1, form.Bulan, form.jBahagianId);

            }

            var user = await _userManager.GetUserAsync(User);
            var namaUser = await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.Email == user!.Email);

            form.Username = namaUser?.Nama;

            var KodLaporan = form.kodLaporan;
            CompanyDetails company = new CompanyDetails();
            form.CompanyDetails = company;

            //DateTime? date1 = null;
            //DateTime? date2 = null;

            //if (!string.IsNullOrEmpty(tarikhDari) && !string.IsNullOrEmpty(tarikhHingga))
            //{
            //    date1 = DateTime.Parse(tarikhDari);
            //    date2 = DateTime.Parse(tarikhHingga);
            //}

            //reportModel.AbWaran = _unitOfWork.AbWaranRepo.GetResults(form.kodLaporan, date1, date2, form.enJenisPeruntukan);


            return abWaranList;
        }



        private DataTable GenerateDataTableLAK00401(List<LAK004PrintModel> abWaranList)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Laporan Belanjawan Terkini";
            dt.Columns.Add("Kod", typeof(string));
            dt.Columns.Add("Perihal", typeof(string));
            dt.Columns.Add("Peruntukan Asal", typeof(decimal));
            dt.Columns.Add("Peruntukan Tambahan", typeof(decimal));
            dt.Columns.Add("Pindahan Peruntukan", typeof(decimal));
            dt.Columns.Add("Jumlah Peruntukan", typeof(decimal));
            dt.Columns.Add("Peruntukan Telah Guna", typeof(decimal));
            dt.Columns.Add("Peruntukan Telah Guna %", typeof(decimal));
            dt.Columns.Add("Tanggungan Belum Selesai", typeof(decimal));
            dt.Columns.Add("Tanggungan Belum Selesai %", typeof(decimal));
            dt.Columns.Add("Perbelanjaan Bersih", typeof(decimal));
            dt.Columns.Add("Perbelanjaan Bersih %", typeof(decimal));
            dt.Columns.Add("Baki", typeof(decimal));

            decimal jumAsal = 0;
            decimal jumTambahan = 0;
            decimal jumPindahan = 0;
            decimal jumJumlah = 0;
            decimal jumtelahGuna = 0;
            decimal jumtelahGunaPercent = 0;
            decimal jumtbs = 0;
            decimal jumtbsPercent = 0;
            decimal jumbelanja = 0;
            decimal jumbelanjaPercent = 0;
            decimal jumbaki = 0;


            if (abWaranList != null && abWaranList.Any())
            {
                var groupedData = abWaranList.GroupBy(b => new { b.Kod, b.Perihal, b.enJenisPeruntukan })
                    .Select(g =>
                    {
                        var totalPeruntukanAsal = g.Sum(b => b.PeruntukanAsal);
                        var totalPeruntukanTambahan = g.Sum(b => b.PeruntukanTambahan);
                        var totalPindahanPeruntukan = g.Sum(b => b.PindahanPeruntukan);
                        var jumlahPeruntukan = totalPeruntukanAsal + totalPeruntukanTambahan + totalPindahanPeruntukan;

                        return new LAK004PrintModel

                        {

                            Kod = g.First().Kod,
                            Perihal = g.First().Perihal,
                            PeruntukanAsal = totalPeruntukanAsal,
                            PeruntukanTambahan = totalPeruntukanTambahan,
                            PindahanPeruntukan = totalPindahanPeruntukan,
                            JumlahPeruntukan = jumlahPeruntukan,
                            PeruntukanTelahGuna = g.Sum(b => b.PeruntukanTelahGuna),
                            TelahGunaPercentage = jumlahPeruntukan != 0 ? (g.Sum(b => b.PeruntukanTelahGuna) / jumlahPeruntukan) * 100 : 0,
                            TBS = g.Sum(b => b.TBS),
                            TBSPercentage = jumlahPeruntukan != 0 ? (g.Sum(b => b.TBS) / jumlahPeruntukan) * 100 : 0,
                            PerbelanjaanBersih = g.Sum(b => b.PerbelanjaanBersih),
                            BelanjaBersihPercentage = jumlahPeruntukan != 0 ? (g.Sum(b => b.PerbelanjaanBersih) / jumlahPeruntukan) * 100 : 0,
                            Baki = g.First().Baki
                        };
                    })
                    .OrderBy(b => b.Kod);

                foreach (var abWaran in groupedData)
                {
                    dt.Rows.Add(
                        abWaran.Kod ?? string.Empty,
                        abWaran.Perihal ?? string.Empty,
                        abWaran.PeruntukanAsal ?? 0,
                        abWaran.PeruntukanTambahan ?? 0,
                        abWaran.PindahanPeruntukan ?? 0,
                        abWaran.JumlahPeruntukan ?? 0,
                        abWaran.PeruntukanTelahGuna ?? 0,
                        abWaran.TelahGunaPercentage ?? 0,
                        abWaran.TBS ?? 0,
                        abWaran.TBSPercentage ?? 0,
                        abWaran.PerbelanjaanBersih ?? 0,
                        abWaran.BelanjaBersihPercentage ?? 0,
                        abWaran.Baki ?? 0


                );

                    jumAsal += abWaran.PeruntukanAsal ?? 0;
                    jumTambahan += abWaran.PeruntukanTambahan ?? 0;
                    jumPindahan += abWaran.PindahanPeruntukan ?? 0;
                    jumJumlah += abWaran.JumlahPeruntukan ?? 0;
                    jumtelahGuna += abWaran.PeruntukanTelahGuna ?? 0;
                    jumtelahGunaPercent += abWaran.TelahGunaPercentage ?? 0;
                    jumtbs += abWaran.TBS ?? 0;
                    jumtbsPercent += abWaran.TBSPercentage ?? 0;
                    jumbelanja += abWaran.PerbelanjaanBersih ?? 0;
                    jumbelanjaPercent += abWaran.BelanjaBersihPercentage ?? 0;
                    jumbaki += abWaran.Baki ?? 0;
                   

                } 

                // Add JUMLAH
                dt.Rows.Add(
                    string.Empty, // Kod
                    "JUMLAH BESAR",  // Perihal
                    jumAsal,
                    jumTambahan,
                    jumPindahan,
                    jumJumlah,
                    jumtelahGuna,
                    jumtelahGunaPercent,
                    jumtbs,
                    jumtbsPercent,
                    jumbelanja,
                    jumbelanjaPercent,
                    jumbaki
                );
            }

            return dt;
        }





        private void RunWorkBookLAK00401(PrintFormModel printModel, DataTable excelData, string handle)
        {
            List<JKW> jkwList = _unitOfWork.JKWRepo.GetAllDetails();
            List<JPTJ> jptjList = _unitOfWork.JPTJRepo.GetAllDetails();
            List<JBahagian> jBahagianList = _unitOfWork.JBahagianRepo.GetAllDetails();

            using (XLWorkbook wb = new XLWorkbook())
                {
                var jkw = jkwList.FirstOrDefault(j => j.Id == printModel.jKWId);
                var jptj = jptjList.FirstOrDefault(j => j.Id == printModel.JPTJId);
                var jbahagian = jBahagianList.FirstOrDefault(j => j.Id == printModel.jBahagianId);

                var ws = wb.AddWorksheet("Laporan Belanjawan Terkini");
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

                Console.WriteLine("Total Rows in Worksheet: " + ws.RowsUsed().Count());

                foreach (var item in ws.ColumnsUsed())
                {
                    item.AdjustToContents();

                    if (item.ColumnNumber() >= 3 && item.ColumnNumber() <= 14)
                    {
                        // Apply the desired number format
                        item.Style.NumberFormat.Format = "#,##0.00"; // Standard numeric format
                    }
                }

                foreach (var row in ws.RowsUsed())
                {
                    if (row.Cell(2).Value.ToString().StartsWith("JUMLAH BESAR"))
                    {
                        // Change background color
                        row.Cells(1, 13).Style.Fill.BackgroundColor = XLColor.LightSkyBlue;


                        // Make the row bold
                        row.Cells(1, 13).Style.Font.Bold = true;
                    }
                }


                using (MemoryStream ms = new MemoryStream())
                    {
                        wb.SaveAs(ms);
                    Console.WriteLine("MemoryStream Length: " + ms.Length);

                    //This is an equivalent to tempdata, but requires manual cleanup
                    _cache.Set(handle, ms.ToArray(),
                                    new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));


                    }
                }
            }


        private DataTable GenerateDataTableLAK00402(List<LAK004PrintModel> abBukuVotList)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Laporan Belanjawan Terkini";
            dt.Columns.Add("Kod", typeof(string));
            dt.Columns.Add("Perihal", typeof(string));
            dt.Columns.Add("Peruntukan Asal", typeof(decimal));
            dt.Columns.Add("Peruntukan Tambahan", typeof(decimal));
            dt.Columns.Add("Pindahan Peruntukan", typeof(decimal));
            dt.Columns.Add("Jumlah Peruntukan", typeof(decimal));
            dt.Columns.Add("Peruntukan Telah Guna", typeof(decimal));
            dt.Columns.Add("Peruntukan Telah Guna %", typeof(decimal));
            dt.Columns.Add("Tanggungan Belum Selesai", typeof(decimal));
            dt.Columns.Add("Tanggungan Belum Selesai %", typeof(decimal));
            dt.Columns.Add("Perbelanjaan Bulan Ini", typeof(decimal));
            dt.Columns.Add("Perbelanjaan Terkumpul", typeof(decimal));
            dt.Columns.Add("Perbelanjaan Terkumpul %", typeof(decimal));
            dt.Columns.Add("Baki", typeof(decimal));
            dt.Columns.Add("Baki %", typeof(decimal));

            decimal jumAsal = 0;
            decimal jumTambahan = 0;
            decimal jumPindahan = 0;
            decimal jumJumlah = 0;
            decimal jumtelahGuna = 0;
            decimal jumtelahGunaPercent = 0;
            decimal jumtbs = 0;
            decimal jumtbsPercent = 0;
            decimal jumbelanjaBulan = 0;
            decimal jumbelanjaKumpul = 0;
            decimal jumbelanjaKumpulPercent = 0;
            decimal jumbaki = 0;
            decimal jumbakiPercent = 0;


            if (abBukuVotList != null && abBukuVotList.Any())
            {
                var groupedData = abBukuVotList.GroupBy(b => new { b.Kod, b.Perihal, b.enJenisPeruntukan })
                    .Select(g =>
                    {
                        var totalPeruntukanAsal = g.Sum(b => b.PeruntukanAsal);
                        var totalPeruntukanTambahan = g.Sum(b => b.PeruntukanTambahan);
                        var totalPindahanPeruntukan = g.Sum(b => b.PindahanPeruntukan);
                        var jumlahPeruntukan = totalPeruntukanAsal + totalPeruntukanTambahan + totalPindahanPeruntukan;

                        return new LAK004PrintModel

                        {

                            Kod = g.First().Kod,
                            Perihal = g.First().Perihal,
                            PeruntukanAsal = totalPeruntukanAsal,
                            PeruntukanTambahan = totalPeruntukanTambahan,
                            PindahanPeruntukan = totalPindahanPeruntukan,
                            JumlahPeruntukan = jumlahPeruntukan,
                            PeruntukanTelahGuna = g.Sum(b => b.PeruntukanTelahGuna),
                            TelahGunaPercentage = jumlahPeruntukan != 0 ? (g.Sum(b => b.PeruntukanTelahGuna) / jumlahPeruntukan) * 100 : 0,
                            TBS = g.Sum(b => b.TBS),
                            TBSPercentage = jumlahPeruntukan != 0 ? (g.Sum(b => b.TBS) / jumlahPeruntukan) * 100 : 0,
                            PerbelanjaanBulanIni = g.Sum(b => b.PerbelanjaanBulanIni),
                            PerbelanjaanTerkumpul = g.Sum(b => b.PerbelanjaanTerkumpul),
                            BelanjaTerkumpulPercentage = jumlahPeruntukan != 0 ? (g.Sum(b => b.PerbelanjaanTerkumpul) / jumlahPeruntukan) * 100 : 0,
                            Baki = g.First().Baki,
                            BakiPercentage = jumlahPeruntukan != 0 ? (g.Sum(b => b.Baki) / jumlahPeruntukan) * 100 : 0
                        };
                    })
                    .OrderBy(b => b.Kod);

                foreach (var abBukuVot in groupedData)
                {
                    dt.Rows.Add(
                        abBukuVot.Kod ?? string.Empty,
                        abBukuVot.Perihal ?? string.Empty,
                        abBukuVot.PeruntukanAsal ?? 0,
                        abBukuVot.PeruntukanTambahan ?? 0,
                        abBukuVot.PindahanPeruntukan ?? 0,
                        abBukuVot.JumlahPeruntukan ?? 0,
                        abBukuVot.PeruntukanTelahGuna ?? 0,
                        abBukuVot.TelahGunaPercentage ?? 0,
                        abBukuVot.TBS ?? 0,
                        abBukuVot.TBSPercentage ?? 0,
                        abBukuVot.PerbelanjaanBulanIni ?? 0,
                        abBukuVot.PerbelanjaanTerkumpul ?? 0,
                        abBukuVot.BelanjaTerkumpulPercentage ?? 0,
                        abBukuVot.Baki ?? 0,
                        abBukuVot.BakiPercentage ?? 0


                );
                    jumAsal += abBukuVot.PeruntukanAsal ?? 0;
                    jumTambahan += abBukuVot.PeruntukanTambahan ?? 0;
                    jumPindahan += abBukuVot.PindahanPeruntukan ?? 0;
                    jumJumlah += abBukuVot.JumlahPeruntukan ?? 0;
                    jumtelahGuna += abBukuVot.PeruntukanTelahGuna ?? 0;
                    jumtelahGunaPercent += abBukuVot.TelahGunaPercentage ?? 0;
                    jumtbs += abBukuVot.TBS ?? 0;
                    jumtbsPercent += abBukuVot.TBSPercentage ?? 0;
                    jumbelanjaBulan += abBukuVot.PerbelanjaanBulanIni ?? 0;
                    jumbelanjaKumpul += abBukuVot.PerbelanjaanTerkumpul ?? 0;
                    jumbelanjaKumpulPercent += abBukuVot.BelanjaTerkumpulPercentage ?? 0;
                    jumbaki += abBukuVot.Baki ?? 0;
                    jumbakiPercent += abBukuVot.BakiPercentage ?? 0;


                }

                // Add JUMLAH
                dt.Rows.Add(
                    string.Empty, // Kod
                    "JUMLAH BESAR",  // Perihal
                    jumAsal,
                    jumTambahan,
                    jumPindahan,
                    jumJumlah,
                    jumtelahGuna,
                    jumtelahGunaPercent,
                    jumtbs,
                    jumtbsPercent,
                    jumbelanjaBulan,
                    jumbelanjaKumpul,
                    jumbelanjaKumpulPercent,
                    jumbaki,
                    jumbakiPercent
                );
            }

            return dt;
        }





        private void RunWorkBookLAK00402(PrintFormModel printModel, DataTable excelData, string handle)
        {
            List<JKW> jkwList = _unitOfWork.JKWRepo.GetAllDetails();
            List<JPTJ> jptjList = _unitOfWork.JPTJRepo.GetAllDetails();
            List<JBahagian> jBahagianList = _unitOfWork.JBahagianRepo.GetAllDetails();

            using (XLWorkbook wb = new XLWorkbook())
            {
                var jkw = jkwList.FirstOrDefault(j => j.Id == printModel.jKWId);
                var jptj = jptjList.FirstOrDefault(j => j.Id == printModel.JPTJId);
                var jbahagian = jBahagianList.FirstOrDefault(j => j.Id == printModel.jBahagianId);

                var ws = wb.AddWorksheet("Laporan Belanjawan Bulanan");
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

                Console.WriteLine("Total Rows in Worksheet: " + ws.RowsUsed().Count());

                foreach (var item in ws.ColumnsUsed())
                {
                    item.AdjustToContents();

                    if (item.ColumnNumber() >= 3 && item.ColumnNumber() <= 15)
                    {
                        // Apply the desired number format
                        item.Style.NumberFormat.Format = "#,##0.00"; // Standard numeric format
                    }
                }

                foreach (var row in ws.RowsUsed())
                {
                    if (row.Cell(2).Value.ToString().StartsWith("JUMLAH BESAR"))
                    {
                        // Change background color
                        row.Cells(1, 15).Style.Fill.BackgroundColor = XLColor.LightSkyBlue;


                        // Make the row bold
                        row.Cells(1, 15).Style.Font.Bold = true;
                    }
                }


                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    Console.WriteLine("MemoryStream Length: " + ms.Length);

                    //This is an equivalent to tempdata, but requires manual cleanup
                    _cache.Set(handle, ms.ToArray(),
                                    new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));


                }
            }
        }




        private void PopulateSelectList(int? JKWId, int? JPTJId, string? Tahun1, string? Bulan, int? JBahagianId)
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

        // printing List of Laporan
        public async Task<IActionResult> PrintPDF(PrintFormModel form)
        {
            var reportModel = await PrepareData(form);
            var company = await _userServices.GetCompanyDetails();
            var abWaran1 = new List<LAK004PrintModel>();

            var jkw = await _context.JKW.FirstOrDefaultAsync(b => b.Id == form.jKWId);

            var jptj = await _context.JPTJ.FirstOrDefaultAsync(ptj => ptj.Id == form.JPTJId);
            var jbahagian = await _context.JBahagian.FirstOrDefaultAsync(jbahagian => jbahagian.Id == form.jBahagianId);


            if (form.kodLaporan == "LAK00401")
            {
                var date1 = DateTime.Now.Year.ToString() + "-01-01";
                var date2 = DateTime.Now.ToString("yyyy-MM-dd");
                ViewData["DateFrom"] = date1;
                ViewData["DateTo"] = date2;


                abWaran1 = await _laporanRepository.GetAbWaranBasedOnYear(form.jKWId, form.JPTJId, form.Tahun1, form.jBahagianId);


                if (form.Tahun1 != null && abWaran1.Any())
                {
                    await _context.AbWaran
                        .Include(a => a.AbWaranObjek!)
                        .ThenInclude(aw => aw.AkCarta)
                        .ThenInclude(ac => ac!.AbBukuVot)
                        .Where(a => a.Tahun == form.Tahun1)
                        .ToListAsync();
                }

                dynamic dyModel = new ExpandoObject();
                dyModel.ReportModel = abWaran1;
                dyModel.reportModelGrouped = abWaran1.GroupBy(b => b.enJenisPeruntukan);

                
                PopulateSelectList(form.jKWId, form.JPTJId, form.Tahun1, form.Bulan, form.jBahagianId);

                return new ViewAsPdf("LAK00401PDF", dyModel, new ViewDataDictionary(ViewData) {
                     { "NamaSyarikat", company.NamaSyarikat },
                     { "AlamatSyarikat1", company.AlamatSyarikat1 },
                     { "AlamatSyarikat2", company.AlamatSyarikat2 },
                     { "AlamatSyarikat3", company.AlamatSyarikat3 },
                     { "Tahun", form.Tahun1 },
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

            if (form.kodLaporan == "LAK00402")
            {
                var date1 = DateTime.Now.Year.ToString() + "-01-01";
                var date2 = DateTime.Now.ToString("yyyy-MM-dd");
                var month = DateTime.Now.Month.ToString();
                ViewData["DateFrom"] = date1;
                ViewData["DateTo"] = date2;
                ViewData["Month"] = month;


                abWaran1 = await _laporanRepository.GetAbWaranBasedOnYearAndMonth(form.jKWId, form.JPTJId, form.Tahun1, form.Bulan, form.jBahagianId);

            }
            if (form.Tahun1 != null && abWaran1.Any())
            {
                await _context.AbWaran
                    .Include(a => a.AbWaranObjek!)
                    .ThenInclude(aw => aw.AkCarta)
                    .ThenInclude(ac => ac!.AbBukuVot)
                    .Where(a => a.Tahun == form.Tahun1 && a.Tarikh.Month == int.Parse(form.Bulan!))
                    .ToListAsync();
            }

            dynamic dyModel1 = new ExpandoObject();
            dyModel1.ReportModel = abWaran1;
            dyModel1.reportModelGrouped = abWaran1.GroupBy(b => b.enJenisPeruntukan);

           
            PopulateSelectList(form.jKWId, form.JPTJId, form.Tahun1, form.Bulan, form.jBahagianId);

            return new ViewAsPdf("LAK00402PDF", dyModel1, new ViewDataDictionary(ViewData) {
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
    

            // printing List of Laporan end
        

    



