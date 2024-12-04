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
using YIT.Akaun.Microservices;
using YIT.Akaun.Models.ViewModels.Common;
using YIT._DataAccess.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace YIT.Akaun.Controllers._99Laporan
{
    [Authorize]
    public class LAK007Controller : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodLPendapatanBulanan;
        public const string namamodul = Modules.namaLPendapatanBulanan;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly ILaporanRepository _laporanRepository;
        private readonly UserServices _userServices;

        public LAK007Controller(
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

            PopulateSelectList(model.AkBankId, model.jKWId, model.tarDari1, model.tarHingga1, model.EnParas);


            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> ExportExcel(PrintFormModel model)
        {
            
            List<LAK007PrintModel> printModel = await PrepareData(model);

            // Generate a new unique identifier against which the file can be stored
            string handle = string.Format("attachment;" + model.kodLaporan + ".xlsx;", string.IsNullOrEmpty(model.kodLaporan) ? Guid.NewGuid().ToString() : WebUtility.UrlEncode(model.kodLaporan));


            if (model.kodLaporan == "LAK00701")
            {
                // construct and insert data into dataTable 
                var excelData = GenerateDataTableLAK00701(printModel);

                // insert dataTable into Workbook
                RunWorkBookLAK00701(model, excelData, handle);
            }

            if (model.kodLaporan == "LAK00702")
            {
                // construct and insert data into dataTable 
                var excelData = await GenerateDataTableLAK00702(model);

                // insert dataTable into Workbook
                RunWorkBookLAK00702(model, excelData, handle);
            }


            return Json(new { FileGuid = handle, FileName = model.kodLaporan + ".xlsx" });
        }

        private async Task<List<LAK007PrintModel>> PrepareData(PrintFormModel form)
        {
            List<LAK007PrintModel> reportModel = new List<LAK007PrintModel>();

            if (form.kodLaporan == "LAK00701")
            {

                form.Tajuk1 = $"Laporan Pendapatan dan Perbelanjaan Pada Tahun {form.Tahun1} Bagi Bulan {form.Bulan}";
                reportModel = await _laporanRepository.GetResultPendapatanBulananByJumlahTerkumpul(form.Tahun1, form.Bulan, form.jKWId);

            }

            if (form.kodLaporan == "LAK00702")
            {

                form.Tajuk1 = $"Laporan Pendapatan dan Perbelanjaan Pada Tahun {form.Tahun1} Bagi Bulan {form.Bulan}";
                reportModel = await _laporanRepository.GetResultPendapatanBulananByParas(form.Tahun1, form.Bulan, form.jKWId, form.EnParas);

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

        private void PopulateSelectList(int? AkBankId, int? JKWId, DateTime? tarDari1, DateTime? tarHingga1, EnParas enParas)
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

            // populate paras
            List<ListItemViewModel> parasList = EnumHelper<EnParas>.GetList();
            var parasSelect = new List<SelectListItem>();

            foreach (var item in parasList)
            {
                parasSelect.Add(new SelectListItem()
                {
                    Text = item.perihal,
                    Value = item.id.ToString()
                });

            }
            ViewBag.EnParas = new SelectList(parasSelect, "Value", "Text", enParas.GetDisplayCode());
            // populate paras end
        }


        private DataTable GenerateDataTableLAK00701(List<LAK007PrintModel> model)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Laporan Pendapatan";
            dt.Columns.Add("Jenis", typeof(string));
            dt.Columns.Add("Kod Akaun", typeof(string));
            dt.Columns.Add("Nama Akaun", typeof(string));
            dt.Columns.Add("Jumlah", typeof(decimal));


            decimal? Jumlah = 0;


            if (model != null && model.Any())
            {
                string currentJenis = model.First().Jenis!;

                dt.Rows.Add(
                DBNull.Value,
                DBNull.Value,
                currentJenis == "H" ? "PENDAPATAN" : "PERBELANJAAN",
                DBNull.Value
            );

                foreach (var item in model)
                {
                  

                    if (item.Jenis != currentJenis)
                    {

                        dt.Rows.Add(
                          DBNull.Value,
                          DBNull.Value,
                          currentJenis == "H" ? "JUMLAH PENDAPATAN RM" : "JUMLAH PERBELANJAAN RM",
                          Jumlah


                          );

                        Jumlah = 0;
                        currentJenis = item.Jenis!;

                        // Add the title row
                        dt.Rows.Add(
                            DBNull.Value,
                            DBNull.Value,
                            currentJenis == "H" ? "PENDAPATAN" : "PERBELANJAAN",  
                            DBNull.Value

                        );

                    }

                    dt.Rows.Add(
                            item.Jenis,
                            item.KodAkaun,
                            item.NamaAkaun,
                            item.Jumlah

                        );
                    Jumlah += item.Jumlah;

                }
                dt.Rows.Add(
                    DBNull.Value,
                    DBNull.Value,
                    currentJenis == "H" ? "JUMLAH PENDAPATAN RM" : "JUMLAH PERBELANJAAN RM",
                    Jumlah
                );

                dt.Columns.Remove("Jenis");
            }
             
            return dt;
        }


        private void RunWorkBookLAK00701(PrintFormModel printModel, DataTable excelData, string handle)
        {
            List<JKW> jkwList = _unitOfWork.JKWRepo.GetAllDetails();
            List<AkBank> akbankList = _unitOfWork.AkBankRepo.GetAllDetails(); 

            using (XLWorkbook wb = new XLWorkbook())
            {
                var jkw = jkwList.FirstOrDefault(j => j.Id == printModel.jKWId);
               
                var ws = wb.AddWorksheet("Laporan Pendapatan");
                ws.Cell("A1").Value = printModel.CompanyDetails?.NamaSyarikat;
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A2").Value = printModel.Tajuk1;
                if (printModel.jKWId != null && jkw != null)
                {
                    ws.Cell("A3").Value = $"{BelanjawanFormatter.ConvertToKW(jkw?.Kod) + " - " + jkw?.Perihal}";
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
               


                foreach (var row in ws.RowsUsed())
                {
                    if (row.Cell(2).Value.ToString().StartsWith("JUMLAH PENDAPATAN RM"))
                    {
                        // Change background color
                        row.Cells(1, 3).Style.Fill.BackgroundColor = XLColor.LightSkyBlue;


                        // Make the row bold
                        row.Cells(1, 3).Style.Font.Bold = true;
                    }
                }

                foreach (var row in ws.RowsUsed())
                {
                    if (row.Cell(2).Value.ToString().StartsWith("JUMLAH PERBELANJAAN RM"))
                    {
                        // Change background color
                        row.Cells(1, 3).Style.Fill.BackgroundColor = XLColor.LightSkyBlue;


                        // Make the row bold
                        row.Cells(1, 3).Style.Font.Bold = true;
                    }
                }

                using MemoryStream ms = new MemoryStream();
                wb.SaveAs(ms);

                
                _cache.Set(handle, ms.ToArray(),
                            new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));
            }
        }



        public async Task<DataTable> GenerateDataTableLAK00702(PrintFormModel model)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Laporan Pendapatan";
            dt.Columns.Add("Jenis", typeof(string));
            dt.Columns.Add("Kod Akaun", typeof(string));
            dt.Columns.Add("Nama Akaun", typeof(string));
            dt.Columns.Add("Tahun lepas Bulan", typeof(decimal));
            dt.Columns.Add("Tahun Semasa Bulan", typeof(decimal));
            dt.Columns.Add("Tahun lepas Kumpul", typeof(decimal));
            dt.Columns.Add("Tahun Semasa Kumpul", typeof(decimal));
            dt.Columns.Add("Terkumpul %", typeof(decimal));
            dt.Columns.Add("Tahun Semasa Peruntukan", typeof(decimal));
            dt.Columns.Add("Peruntukan %", typeof(decimal));


            decimal? tahunLpsBulan = 0;
            decimal? tahunSmsBulan = 0;
            decimal? tahunLpsKumpul = 0;
            decimal? tahunSmsKumpul = 0;
            decimal? KumpulPercentage = 0;
            decimal? tahunSmsPeruntukan = 0;
            decimal? PeruntukanPercentage = 0;



            var resultByParas = await _laporanRepository.GetResultPendapatanBulananByParas(model.Tahun1, model.Bulan, model.jKWId, model.EnParas);

            if (resultByParas != null && resultByParas.Any())
            {
                string currentJenis = resultByParas.First().Jenis!;
               
                // Add the title row 
                dt.Rows.Add(
                    DBNull.Value,
                    DBNull.Value,
                    currentJenis == "H" ? "PENDAPATAN" : "PERBELANJAAN", 
                    DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value,
                    DBNull.Value, DBNull.Value
                );

                foreach (var item in resultByParas)
                {
                    KumpulPercentage = item.TahunLps_Kumpul != 0 ? (item.TahunSms_Kumpul - item.TahunLps_Kumpul) / item.TahunLps_Kumpul * 100 : 0;
                    PeruntukanPercentage = item.TahunSms_Peruntukan != 0 ? (item.TahunSms_Kumpul / item.TahunSms_Peruntukan) * 100 : 0;
                    // If Jenis changes, add a total row for the previous Jenis
                    if (item.Jenis != currentJenis)
                    {

                        dt.Rows.Add(
                            DBNull.Value,
                            DBNull.Value,
                            currentJenis == "H" ? "JUMLAH PENDAPATAN RM" : "JUMLAH PERBELANJAAN RM",
                            tahunLpsBulan,
                            tahunSmsBulan,
                            tahunLpsKumpul,
                            tahunSmsKumpul,
                            KumpulPercentage,
                            tahunSmsPeruntukan,
                            PeruntukanPercentage
                            
                        );


                        // Reset totals for the new group
                        tahunLpsBulan = tahunSmsBulan = tahunLpsKumpul = tahunSmsKumpul = KumpulPercentage = tahunSmsPeruntukan = 0;
                        currentJenis = item.Jenis!;

                        // Add the title row
                        dt.Rows.Add(
                            DBNull.Value,
                            DBNull.Value,
                            currentJenis == "H" ? "PENDAPATAN" : "PERBELANJAAN",
                            DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value,
                            DBNull.Value,DBNull.Value
                        );
                    }

                    dt.Rows.Add(
                           item.Jenis,
                           item.KodAkaun,
                           item.NamaAkaun,
                           item.TahunLps_BulanSMS,
                           item.TahunSms_BulanSMS,
                           item.TahunLps_Kumpul,
                           item.TahunSms_Kumpul,
                           KumpulPercentage,
                           item.TahunSms_Peruntukan,
                           PeruntukanPercentage
                        );


                    // Update running totals
                    tahunLpsBulan += item.TahunLps_BulanSMS;
                    tahunSmsBulan += item.TahunSms_BulanSMS;
                    tahunLpsKumpul += item.TahunLps_Kumpul;
                    tahunSmsKumpul += item.TahunSms_Kumpul;
                    KumpulPercentage += item.TerKumpulPercentage;
                    tahunSmsPeruntukan += item.TahunSms_Peruntukan;
                    PeruntukanPercentage += item.TerKumpulPercentage;
                    
                }

                // Add the final total row
                dt.Rows.Add(
                    DBNull.Value,
                    DBNull.Value,
                    currentJenis == "H" ? "JUMLAH PENDAPATAN RM" : "JUMLAH PERBELANJAAN RM",
                    tahunLpsBulan,
                    tahunSmsBulan,
                    tahunLpsKumpul,
                    tahunSmsKumpul,
                    KumpulPercentage,
                    tahunSmsPeruntukan,
                    PeruntukanPercentage
                    
                );

                dt.Columns.Remove("Jenis");

            }

            return dt;
        }


        private void RunWorkBookLAK00702(PrintFormModel printModel, DataTable excelData, string handle)
        {
            List<JKW> jkwList = _unitOfWork.JKWRepo.GetAllDetails();
            List<AkBank> akbankList = _unitOfWork.AkBankRepo.GetAllDetails();
            List<ListItemViewModel> parasList = EnumHelper<EnParas>.GetList();


            using (XLWorkbook wb = new XLWorkbook())
            {
                var jkw = jkwList.FirstOrDefault(j => j.Id == printModel.jKWId);
                
                var ws = wb.AddWorksheet("Laporan Pendapatan");
                ws.Cell("A1").Value = printModel.CompanyDetails?.NamaSyarikat;
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A2").Value = printModel.Tajuk1;
                if (printModel.jKWId != null && jkw != null)
                {
                    ws.Cell("A3").Value = $"{BelanjawanFormatter.ConvertToKW(jkw?.Kod) + " - " + jkw?.Perihal}";
                }

                if (printModel.EnParas != 0)
                {
                    ws.Cell("A4").Value = printModel.EnParas.GetDisplayName();
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




                foreach (var row in ws.RowsUsed())
                {
                    if (row.Cell(2).Value.ToString().StartsWith("JUMLAH PENDAPATAN RM"))
                    {
                        // Change background color
                        row.Cells(1, 8).Style.Fill.BackgroundColor = XLColor.LightSkyBlue;


                        // Make the row bold
                        row.Cells(1, 8).Style.Font.Bold = true;
                    }
                }

                foreach (var row in ws.RowsUsed())
                {
                    if (row.Cell(2).Value.ToString().StartsWith("JUMLAH PERBELANJAAN RM"))
                    {
                        // Change background color
                        row.Cells(1, 8).Style.Fill.BackgroundColor = XLColor.LightSkyBlue;


                        // Make the row bold
                        row.Cells(1, 8).Style.Font.Bold = true;
                    }
                }

                using MemoryStream ms = new MemoryStream();
                wb.SaveAs(ms);

                
                _cache.Set(handle, ms.ToArray(),
                            new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));
            }
        }


        // printing List of Laporan
        [AllowAnonymous]
        public async Task<IActionResult> PrintPDF(PrintFormModel form)
        {
            var abWaran = new List<LAK007PrintModel>();
            var company = await _userServices.GetCompanyDetails();
            


            var reportModel = await PrepareData(form);

            if (form.kodLaporan == "LAK00701")
            {
                abWaran = await _laporanRepository.GetResultPendapatanBulananByJumlahTerkumpul(form.Tahun1, form.Bulan, form.jKWId);

                dynamic dyModel = new ExpandoObject();
                dyModel.ReportModel = abWaran;
                dyModel.reportModelGrouped = abWaran.GroupBy(b => b.Jenis);
                var jkw = await _context.JKW.FirstOrDefaultAsync(b => b.Id == form.jKWId);

                return new ViewAsPdf("LAK00701PDF", dyModel, new ViewDataDictionary(ViewData)
                    {
                        { "NamaSyarikat", company.NamaSyarikat },
                        { "AlamatSyarikat1", company.AlamatSyarikat1 },
                        { "AlamatSyarikat2", company.AlamatSyarikat2 },
                        { "AlamatSyarikat3", company.AlamatSyarikat3 },
                        { "Tahun", form.Tahun1 },
                        { "Bulan", int.Parse(form.Bulan!).ToString("D2") },
                        { "NamaKW", BelanjawanFormatter.ConvertToKW(jkw?.Kod) + " - " + jkw?.Perihal }
                    })
                            {
                                PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                            };
                        }

            if (form.kodLaporan == "LAK00702" && form.EnParas != 0)
            {
                abWaran = await _laporanRepository.GetResultPendapatanBulananByParas(form.Tahun1, form.Bulan, form.jKWId, form.EnParas);

                dynamic dyModel = new ExpandoObject();
                dyModel.ReportModel = abWaran;
                dyModel.reportModelGrouped = abWaran.GroupBy(b => b.Jenis);
                var jkw = await _context.JKW.FirstOrDefaultAsync(b => b.Id == form.jKWId);

                return new ViewAsPdf("LAK00702PDF", dyModel, new ViewDataDictionary(ViewData)
                    {
                        { "NamaSyarikat", company.NamaSyarikat },
                        { "AlamatSyarikat1", company.AlamatSyarikat1 },
                        { "AlamatSyarikat2", company.AlamatSyarikat2 },
                        { "AlamatSyarikat3", company.AlamatSyarikat3 },
                        { "Tahun", form.Tahun1 },
                        { "Bulan", int.Parse(form.Bulan!).ToString("D2") },
                        { "Paras", form.EnParas.GetDisplayName().ToUpper() },
                        { "NamaKW", BelanjawanFormatter.ConvertToKW(jkw?.Kod) + " - " + jkw?.Perihal }
                    })
                            {
                                PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                            };
                        }

            // Default case if no valid report type is found
            return BadRequest("Invalid report type.");
        }

    }
}
