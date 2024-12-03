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
using YIT.__Domain.Entities._Statics;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Rotativa.AspNetCore;
using YIT.__Domain.Entities._Enums;
using YIT.Akaun.Infrastructure;
using YIT.__Domain.Entities.Models._01Jadual;
using Microsoft.AspNetCore.Mvc.Rendering;
using YIT._DataAccess.Services.Math;
using System.Collections.Generic;
using DocumentFormat.OpenXml.ExtendedProperties;
using System.Dynamic;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using System.Security.Cryptography;

namespace YIT.Akaun.Controllers._99Laporan
{
    [Authorize]
    public class LAK003Controller : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodLPenerimaanPembayaran;
        public const string namamodul = Modules.namaLPenerimaanPembayaran;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly ILaporanRepository _laporan;
        private readonly UserServices _userServices;

        public LAK003Controller(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            _IUnitOfWork unitOfWork,
            IMemoryCache cache,
            ILaporanRepository laporan,
            UserServices userServices)
        {
            _context = context;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _cache = cache;
            _laporan = laporan;
            _userServices = userServices;
        }
        public async Task<IActionResult> Index(PrintFormModel form)
        {

            var bukuTunai = new List<_AkBukuTunai>();

            if (form.tarDari1 == null && form.tarHingga1 == null)
            {
                form.tarDari1 = new DateTime(DateTime.Now.Year, 1, 1);
                form.tarHingga1 = DateTime.Now;
            }

            PopulateSelectList(form.AkBankId, form.jKWId, form.JPTJId, form.tarDari1, form.tarHingga1);

            if (form.AkBankId != null)
            {

                bukuTunai = await _laporan.GetAkBukuTunaiBasedOnBank((int)form.AkBankId, form.jKWId, form.JPTJId, form.tarDari1, form.tarHingga1, form.Tahun1);
            }

            if (form.jKWId != null)
            {
                bukuTunai = await _laporan.GetAkBukuTunaiBasedOnKW(form.jKWId, form.JPTJId, form.tarDari1, form.tarHingga1, form.Tahun1);
            }



            return View(form);
        }

        [HttpPost]
        public async Task<JsonResult> ExportExcel(PrintFormModel model)
        {

            List<_AkBukuTunai> printModel = await PrepareData(model);

            // Generate a new unique identifier against which the file can be stored
            string handle = string.Format("attachment;" + model.kodLaporan + ".xlsx;", string.IsNullOrEmpty(model.kodLaporan) ? Guid.NewGuid().ToString() : WebUtility.UrlEncode(model.kodLaporan));

            // save viewmodel into workbook
            if (model.kodLaporan == "LAK00301" && model.AkBankId != null)
            {

                //construct and insert data into dataTable 
                var excelData = await GenerateDataTableLAK00301(model);

                // insert dataTable into Workbook
                RunWorkBookLAK00301(model, excelData, handle);
            }

            if (model.kodLaporan == "LAK00302")
            {

                //construct and insert data into dataTable 
                var excelData = await GenerateDataTableLAK00302(model);

                // insert dataTable into Workbook
                RunWorkBookLAK00302(model, excelData, handle);
            }

            return Json(new { FileGuid = handle, FileName = model.kodLaporan + ".xlsx" });
        }

        private async Task<List<_AkBukuTunai>> PrepareData(PrintFormModel form)
        {

            List<_AkBukuTunai> bukuTunai = new List<_AkBukuTunai>();


            if (form.kodLaporan == "LAK00301" && form.AkBankId != null)
            {

                form.Tajuk1 = $"Laporan Penerimaan dan Pembayaran Mengikut Bank Dari {@Convert.ToDateTime(form.tarDari1).ToString("dd/MM/yyyy")} Hingga {@Convert.ToDateTime(form.tarHingga1):dd/MM/yyyy}";
                bukuTunai = await _laporan.GetAkBukuTunaiBasedOnBank((int)form.AkBankId, form.jKWId, form.JPTJId, form.tarDari1, form.tarHingga1, form.Tahun1);
            }

            if (form.kodLaporan == "LAK00302")
            {

                form.Tajuk1 = $"Laporan Penerimaan dan Pembayaran Mengikut Kumpulan Wang Dari {@Convert.ToDateTime(form.tarDari1).ToString("dd/MM/yyyy")} Hingga {@Convert.ToDateTime(form.tarHingga1):dd/MM/yyyy}";
                bukuTunai = await _laporan.GetAkBukuTunaiBasedOnKW(form.jKWId, form.JPTJId, form.tarDari1, form.tarHingga1, form.Tahun1);
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

            if (form.AkBankId != null)
            {

                //bukuTunai = await _unitOfWork.AkPVRepo.GetAkBukuTunaiBasedOnBank((int)form.AkBankId!, form.JKWId, form.JPTJId, form.tarDari1, form.tarHingga1, form.Tahun1);
            }



            return bukuTunai;
        }



        public async Task<Tuple<DataTable, DataTable>> GenerateDataTableLAK00301(PrintFormModel model)
        {
            DataTable dtMasuk = new DataTable();
            dtMasuk.TableName = "Laporan Buku Tunai Masuk";
            dtMasuk.Columns.Add("Tarikh Masuk", typeof(DateTime));
            dtMasuk.Columns.Add("Nama Akaun Masuk", typeof(string));
            dtMasuk.Columns.Add("No Rujukan Masuk", typeof(string));
            dtMasuk.Columns.Add("Amaun Masuk", typeof(decimal));
            dtMasuk.Columns.Add("Jumlah Masuk", typeof(decimal));

            DataTable dtKeluar = new DataTable();
            dtKeluar.TableName = "Laporan Buku Tunai Keluar";
            dtKeluar.Columns.Add("Tarikh Keluar", typeof(DateTime));
            dtKeluar.Columns.Add("Nama Akaun Keluar", typeof(string));
            dtKeluar.Columns.Add("No Rujukan Keluar", typeof(string));
            dtKeluar.Columns.Add("Amaun Keluar", typeof(decimal));
            dtKeluar.Columns.Add("Jumlah Keluar", typeof(decimal));


            var bukuTunaiList = await _laporan.GetAkBukuTunaiBasedOnBank(
                (int)model.akBankId!,
                model.jKWId,
                model.JPTJId,
                model.tarDari1,
                model.tarHingga1,
                model.Tahun1
            );

            decimal previousBalance = 0;
            
            if (bukuTunaiList != null && bukuTunaiList.Any())
            {
                
                previousBalance = bukuTunaiList
                    .Where(x => x.TarikhMasuk == null && x.TarikhKeluar == null)
                    .Sum(x => x.KeluarMasuk);
            }

           
            dtMasuk.Rows.Add(
                null, 
                "BAKI BAWA KE HADAPAN",
                string.Empty,
                DBNull.Value,
                previousBalance 
            );

            dtKeluar.Rows.Add(
                null, 
                string.Empty,
                string.Empty,
                DBNull.Value,
                previousBalance
            );

            decimal jumlahMasuk = 0;
            decimal jumlahKeluar = 0;

            if (bukuTunaiList != null)
            {   
                foreach (var item in bukuTunaiList)
                {
                    jumlahMasuk += item.AmaunMasuk;

                    if (item.TarikhMasuk != null)
                    {
                        dtMasuk.Rows.Add(
                            item.TarikhMasuk,
                            item.NamaAkaunMasuk ?? string.Empty,
                            item.NoRujukanMasuk ?? string.Empty,
                            item.AmaunMasuk,
                            jumlahMasuk
                        );
                    }

                    jumlahKeluar += item.AmaunKeluar;

                    if (item.TarikhKeluar != null)
                    {
                        dtKeluar.Rows.Add(
                            item.TarikhKeluar,
                            item.NamaAkaunKeluar ?? string.Empty,
                            item.NoRujukanKeluar ?? string.Empty,
                            item.AmaunKeluar,
                            jumlahKeluar
                        );
                    }
                }

                
                if (dtMasuk.Rows.Count > 0)
                {
                    var AmaunMasuk = bukuTunaiList.Sum(x => x.AmaunMasuk);
                    dtMasuk.Rows.Add(DBNull.Value,
                        "JUMLAH BESAR", 
                        string.Empty,
                        DBNull.Value,
                        AmaunMasuk);
                }

                
                if (dtKeluar.Rows.Count > 0)
                {
                    var AmaunKeluar = bukuTunaiList.Sum(x => x.AmaunKeluar);
                    dtKeluar.Rows.Add(DBNull.Value, 
                        "JUMLAH BESAR", 
                        string.Empty, 
                        DBNull.Value,
                        AmaunKeluar);
                }
            }

            return new Tuple<DataTable, DataTable>(dtMasuk, dtKeluar);
        }



        private void RunWorkBookLAK00301(PrintFormModel printModel, Tuple<DataTable, DataTable> excelData, string handle)
        {
            List<AkBank> akbankList = _unitOfWork.AkBankRepo.GetAllDetails();

            using (XLWorkbook wb = new XLWorkbook())
            {
                var akbank = akbankList.FirstOrDefault(j => j.Id == printModel.AkBankId);

                var ws = wb.AddWorksheet("Laporan LAK00301");
                ws.Cell("A1").Value = printModel.CompanyDetails?.NamaSyarikat;
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A2").Value = printModel.Tajuk1;

                if (printModel.AkBankId != null && akbankList != null)
                {
                    ws.Cell("A3").Value = $"{akbank?.NoAkaun + " (" + akbank?.AkCarta?.Kod + " - " + akbank?.AkCarta?.Perihal}";
                }

                var dtMasuk = excelData.Item1; 
                var dtKeluar = excelData.Item2;

                var tableMasuk = ws.Cell("A5").InsertTable(dtMasuk);
                tableMasuk.Theme = XLTableTheme.TableStyleMedium1;


                var tableKeluar = ws.Cell("G5").InsertTable(dtKeluar);
                tableKeluar.Theme = XLTableTheme.TableStyleMedium1;



                ws.Column(2)
              .Style.DateFormat.Format = "dd/MM/yyyy";
                ws.Column(2).AdjustToContents();
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
                 .Style.DateFormat.Format = "dd/MM/yyyy";
                ws.Column(7).AdjustToContents();
                ws.Column(8).AdjustToContents();
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
                ws.Column(13).AdjustToContents();

                foreach (var row in tableMasuk.DataRange.Rows())
                {
                    if (row.Cell(2).Value.ToString().StartsWith("JUMLAH BESAR"))
                    {
                        // Change background color
                        row.Cells(1, 5).Style.Fill.BackgroundColor = XLColor.LightSkyBlue;


                        // Make the row bold
                        row.Cells(1, 5).Style.Font.Bold = true;
                    }
                }

                foreach (var row in tableKeluar.DataRange.Rows())
                {
                    if (row.Cell(2).Value.ToString().StartsWith("JUMLAH BESAR"))
                    {
                        // Change background color
                        row.Cells(1, 5).Style.Fill.BackgroundColor = XLColor.LightSkyBlue;


                        // Make the row bold
                        row.Cells(1, 5).Style.Font.Bold = true;
                    }
                }


                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    ms.Position = 0;

                    //This is an equivalent to tempdata, but requires manual cleanup
                    _cache.Set(handle, ms.ToArray(),
                                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));


                }
            }
        }



        public async Task<Tuple<DataTable, DataTable>> GenerateDataTableLAK00302(PrintFormModel model)
        {
            DataTable dtMasuk = new DataTable();
            dtMasuk.TableName = "Laporan Buku Tunai Masuk";
            dtMasuk.Columns.Add("Tarikh Masuk", typeof(DateTime));
            dtMasuk.Columns.Add("Nama Akaun Masuk", typeof(string));
            dtMasuk.Columns.Add("No Rujukan Masuk", typeof(string));
            dtMasuk.Columns.Add("Amaun Masuk", typeof(decimal));
            dtMasuk.Columns.Add("Jumlah Masuk", typeof(decimal));

            DataTable dtKeluar = new DataTable();
            dtKeluar.TableName = "Laporan Buku Tunai Keluar";
            dtKeluar.Columns.Add("Tarikh Keluar", typeof(DateTime));
            dtKeluar.Columns.Add("Nama Akaun Keluar", typeof(string));
            dtKeluar.Columns.Add("No Rujukan Keluar", typeof(string));
            dtKeluar.Columns.Add("Amaun Keluar", typeof(decimal));
            dtKeluar.Columns.Add("Jumlah Keluar", typeof(decimal));


            var bukuTunaiList = await _laporan.GetAkBukuTunaiBasedOnKW(
                model.jKWId,
                model.JPTJId,
                model.tarDari1,
                model.tarHingga1,
                model.Tahun1
            );

            decimal previousBalance = 0;

            if (bukuTunaiList != null && bukuTunaiList.Any())
            {

                previousBalance = bukuTunaiList
                    .Where(x => x.TarikhMasuk == null && x.TarikhKeluar == null)
                    .Sum(x => x.KeluarMasuk);
            }


            dtMasuk.Rows.Add(
                null,
                "BAKI BAWA KE HADAPAN",
                string.Empty,
                DBNull.Value,
                previousBalance
            );

            dtKeluar.Rows.Add(
                null,
                string.Empty,
                string.Empty,
                DBNull.Value,
                previousBalance
            );

            decimal jumlahMasuk = 0;
            decimal jumlahKeluar = 0;

            if (bukuTunaiList != null)
            {
                foreach (var item in bukuTunaiList)
                {
                    jumlahMasuk += item.AmaunMasuk;

                    if (item.TarikhMasuk != null)
                    {
                        dtMasuk.Rows.Add(
                            item.TarikhMasuk,
                            item.NamaAkaunMasuk ?? string.Empty,
                            item.NoRujukanMasuk ?? string.Empty,
                            item.AmaunMasuk,
                            jumlahMasuk
                        );
                    }

                    jumlahKeluar += item.AmaunKeluar;

                    if (item.TarikhKeluar != null)
                    {
                        dtKeluar.Rows.Add(
                            item.TarikhKeluar,
                            item.NamaAkaunKeluar ?? string.Empty,
                            item.NoRujukanKeluar ?? string.Empty,
                            item.AmaunKeluar,
                            jumlahKeluar
                        );
                    }
                }


                if (dtMasuk.Rows.Count > 0)
                {
                    var AmaunMasuk = bukuTunaiList.Sum(x => x.AmaunMasuk);
                    dtMasuk.Rows.Add(DBNull.Value,
                        "JUMLAH BESAR",
                        string.Empty,
                        DBNull.Value,
                        AmaunMasuk);
                }


                if (dtKeluar.Rows.Count > 0)
                {
                    var AmaunKeluar = bukuTunaiList.Sum(x => x.AmaunKeluar);
                    dtKeluar.Rows.Add(DBNull.Value,
                        "JUMLAH BESAR",
                        string.Empty,
                        DBNull.Value,
                        AmaunKeluar);
                }
            }

            return new Tuple<DataTable, DataTable>(dtMasuk, dtKeluar);
        }



        private void RunWorkBookLAK00302(PrintFormModel printModel, Tuple<DataTable, DataTable> excelData, string handle)
        {
            List<JKW> jkwlist = _unitOfWork.JKWRepo.GetAllDetails();

            using (XLWorkbook wb = new XLWorkbook())
            {
                var jkw = jkwlist.FirstOrDefault(j => j.Id == printModel.jKWId);

                var ws = wb.AddWorksheet("Laporan LAK00302");
                ws.Cell("A1").Value = printModel.CompanyDetails?.NamaSyarikat;
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A2").Value = printModel.Tajuk1;

                if (printModel.jKWId != null && jkw != null)
                {
                    ws.Cell("A3").Value = $"{BelanjawanFormatter.ConvertToKW(jkw?.Kod) + " - " + jkw?.Perihal}";
                }

                var dtMasuk = excelData.Item1;
                var dtKeluar = excelData.Item2;

                var tableMasuk = ws.Cell("A5").InsertTable(dtMasuk);
                tableMasuk.Theme = XLTableTheme.TableStyleMedium1;


                var tableKeluar = ws.Cell("G5").InsertTable(dtKeluar);
                tableKeluar.Theme = XLTableTheme.TableStyleMedium1;



                ws.Column(2)
              .Style.DateFormat.Format = "dd/MM/yyyy";
                ws.Column(2).AdjustToContents();
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
                 .Style.DateFormat.Format = "dd/MM/yyyy";
                ws.Column(7).AdjustToContents();
                ws.Column(8).AdjustToContents();
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
                ws.Column(13).AdjustToContents();

                foreach (var row in tableMasuk.DataRange.Rows())
                {
                    if (row.Cell(2).Value.ToString().StartsWith("JUMLAH BESAR"))
                    {
                        // Change background color
                        row.Cells(1, 5).Style.Fill.BackgroundColor = XLColor.LightSkyBlue;


                        // Make the row bold
                        row.Cells(1, 5).Style.Font.Bold = true;
                    }
                }

                foreach (var row in tableKeluar.DataRange.Rows())
                {
                    if (row.Cell(2).Value.ToString().StartsWith("JUMLAH BESAR"))
                    {
                        // Change background color
                        row.Cells(1, 5).Style.Fill.BackgroundColor = XLColor.LightSkyBlue;


                        // Make the row bold
                        row.Cells(1, 5).Style.Font.Bold = true;
                    }
                }


                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    ms.Position = 0;

                    //This is an equivalent to tempdata, but requires manual cleanup
                    _cache.Set(handle, ms.ToArray(),
                                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));


                }
            }
        }

        private void PopulateSelectList(int? akBankId, int? JKWId, int? JPTJId, DateTime? tarDari1, DateTime? tarHingga1)
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
                ViewBag.AkBank = new SelectList(bankSelect, "Value", "Text", akBankId);
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

            // populate tarikhDari and tarikhHingga
            if (tarDari1 != null)
            {
                ViewData["DateFrom"] = tarDari1?.ToString("yyyy-MM-dd");
                ViewData["DateTo"] = tarHingga1?.ToString("yyyy-MM-dd");
            }
        }



        // printing List of Laporan
        [AllowAnonymous]
        public async Task<IActionResult> PrintPDF(PrintFormModel form)
        {
            var bukuTunai = new List<_AkBukuTunai>();

            if (form.kodLaporan == "LAK00301" && form.AkBankId != 0)
            {
                bukuTunai = await _laporan.GetAkBukuTunaiBasedOnBank((int)form.AkBankId!, form.jKWId, form.JPTJId, form.tarDari1, form.tarHingga1, form.Tahun1);

                var bank = await _context.AkBank
                    .Include(b => b.AkCarta)
                    .FirstOrDefaultAsync(b => b.Id == form.AkBankId);

                var company = await _userServices.GetCompanyDetails();

                return new ViewAsPdf("LAK00301PDF", bukuTunai,
                    new ViewDataDictionary(ViewData)
                    {
                { "TarDari", form.tarDari1?.ToString("dd/MM/yyyy hh:mm:ss tt") },
                { "TarHingga", form.tarHingga1?.AddHours(23.99).ToString("dd/MM/yyyy hh:mm:ss tt") },
                { "NamaBank", bank?.NoAkaun + " (" + bank?.AkCarta?.Kod + " - " + bank?.AkCarta?.Perihal + ")" },
                { "NamaSyarikat", company.NamaSyarikat },
                { "AlamatSyarikat1", company.AlamatSyarikat1 },
                { "AlamatSyarikat2", company.AlamatSyarikat2 },
                { "AlamatSyarikat3", company.AlamatSyarikat3 }
                    })
                {
                    PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                    PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                    CustomSwitches = "--footer-center \"[page]/[toPage]\" " +
                                    "--footer-line --footer-font-size \"7\" --footer-spacing 1 --footer-font-name \"Segoe UI\"",
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                };

            }

            if (form.kodLaporan == "LAK00302")
            {
                bukuTunai = await _laporan.GetAkBukuTunaiBasedOnKW(form.jKWId, form.JPTJId, form.tarDari1, form.tarHingga1, form.Tahun1);

                var company = await _userServices.GetCompanyDetails();

                var jkw = await _context.JKW.FirstOrDefaultAsync(b => b.Id == form.jKWId);

                return new ViewAsPdf("LAK00302PDF", bukuTunai,
                    new ViewDataDictionary(ViewData)
                    {
                { "TarDari", form.tarDari1?.ToString("dd/MM/yyyy hh:mm:ss tt") },
                { "TarHingga", form.tarHingga1?.AddHours(23.99).ToString("dd/MM/yyyy hh:mm:ss tt") },
                { "NamaKW", BelanjawanFormatter.ConvertToKW(jkw?.Kod) + " - " + jkw?.Perihal },
                { "NamaSyarikat", company.NamaSyarikat },
                { "AlamatSyarikat1", company.AlamatSyarikat1 },
                { "AlamatSyarikat2", company.AlamatSyarikat2 },
                { "AlamatSyarikat3", company.AlamatSyarikat3 }
                    })
                {
                    PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                    PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                    CustomSwitches = "--footer-center \"[page]/[toPage]\" " +
                                    "--footer-line --footer-font-size \"7\" --footer-spacing 1 --footer-font-name \"Segoe UI\"",
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                };

            }
            return View (form);
        }
    }
}
  





            // printing List of Laporan end


