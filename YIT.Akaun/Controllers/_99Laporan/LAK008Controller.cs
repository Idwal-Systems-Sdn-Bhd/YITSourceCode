using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Rotativa.AspNetCore;
using System.Data;
using System.Dynamic;
using System.Net;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Implementations;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services;
using YIT._DataAccess.Services.Math;
using YIT.Akaun.Infrastructure;
using YIT.Akaun.Microservices;
using YIT.Akaun.Models.ViewModels.Common;
using YIT.Akaun.Models.ViewModels.Forms;
using YIT.Akaun.Models.ViewModels.Prints;

namespace YIT.Akaun.Controllers._99Laporan
{
    public class LAK008Controller : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodLPenerimaanIkutJulatTertentu;
        public const string namamodul = Modules.namaLPenerimaanIkutJulatTertentu;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly ILaporanRepository _laporanRepository;
        private readonly UserServices _userServices;

        public LAK008Controller(
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
        public IActionResult Index(PrintFormModel form, int? dPekerjaId1, int? dPekerjaId2, int? dPekerjaId3)
        {
            PopulateSelectList(form.akBankId, form.jKWId, form.tarDari1, form.tarHingga1, form.jCawanganId, dPekerjaId1, dPekerjaId2, dPekerjaId3);
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> ExportExcel(PrintFormModel model, int? dPekerjaId1, int? dPekerjaId2, int? dPekerjaId3)
        {
            List<LAK008PrintModel> printModel = await PrepareData(model, dPekerjaId1, dPekerjaId2, dPekerjaId3);

            // Generate a new unique identifier against which the file can be stored
            string handle = string.Format("attachment;" + model.kodLaporan + ".xlsx;", string.IsNullOrEmpty(model.kodLaporan) ? Guid.NewGuid().ToString() : WebUtility.UrlEncode(model.kodLaporan));


            if (!string.IsNullOrEmpty(model.kodLaporan) && !string.IsNullOrEmpty(model.susunan))
            {
                DataTable excelData = null;

                switch (model.kodLaporan)
                {
                    case "LAK00801A_Tarikh":
                        excelData = await GenerateDataTableLAK00801A(model);
                        RunWorkBookLAK00801A(model, excelData, handle);
                        break;

                    case "LAK00801A_Akaun":
                        excelData = await GenerateDataTableLAK00801AA(model);
                        RunWorkBookLAK00801AA(model, excelData, handle);
                        break;

                    default:
                       
                        break;
                }
            }


            if (model.kodLaporan == "LAK00802")
            {
                var excelData = await GenerateDataTableLAK00802(model);
                RunWorkBookLAK00802(model, excelData, handle);
            }

            if (model.kodLaporan == "LAK00803")
            {
                var excelData = await GenerateDataTableLAK00803(model);
                RunWorkBookLAK00803(model, excelData, handle);
            }

            return Json(new { FileGuid = handle, FileName = model.kodLaporan + ".xlsx" });
        }

        private async Task<List<LAK008PrintModel>> PrepareData(PrintFormModel form, int? dPekerjaId1, int? dPekerjaId2, int? dPekerjaId3)
        {
            List<LAK008PrintModel> reportModel = new List<LAK008PrintModel>();

            if (dPekerjaId1.HasValue)
            {
                var pekerja1 = await _context.DPekerja.FindAsync(dPekerjaId1);
                if (pekerja1 != null)
                {
                    reportModel.Add(new LAK008PrintModel
                    {
                        NamaDisedia = pekerja1.Nama,
                        JawatanDisedia = pekerja1.Jawatan
                    });
                }
            }

            if (dPekerjaId2.HasValue)
            {
                var pekerja2 = await _context.DPekerja.FindAsync(dPekerjaId2);
                if (pekerja2 != null)
                {
                    reportModel.Add(new LAK008PrintModel
                    {
                        NamaDisemak = pekerja2.Nama,
                        JawatanDisemak = pekerja2.Jawatan
                    });
                }
            }

            if (dPekerjaId3.HasValue)
            {
                var pekerja3 = await _context.DPekerja.FindAsync(dPekerjaId3);
                if (pekerja3 != null)
                {
                    reportModel.Add(new LAK008PrintModel
                    {
                        NamaDilulus = pekerja3.Nama,
                        JawatanDilulus = pekerja3.Jawatan
                    });
                }
            }

            if (form.kodLaporan == "LAK00801A_Tarikh" && form.susunan == "Tarikh")
            {

                form.Tajuk1 = $"Laporan Penerimaan Mengikut Tarikh {@Convert.ToDateTime(form.tarDari1).ToString("dd/MM/yyyy")} Hingga {@Convert.ToDateTime(form.tarHingga1):dd/MM/yyyy}";
                reportModel = await _laporanRepository.AkterimaByTarikh(form.tarDari1, form.tarHingga1, form.jCawanganId, form.susunan, form.akBankId);
                reportModel = reportModel.OrderBy(b => b.Tarikh).ToList();
            }

            if (form.kodLaporan == "LAK00801A_Akaun" && form.susunan == "Akaun")
            {
                form.Tajuk1 = $"Laporan Penerimaan Mengikut Tarikh {@Convert.ToDateTime(form.tarDari1).ToString("dd/MM/yyyy")} Hingga {@Convert.ToDateTime(form.tarHingga1):dd/MM/yyyy}";
                reportModel = await _laporanRepository.AkterimaByAkaun(form.tarDari1, form.tarHingga1, form.jCawanganId, form.susunan, form.akBankId);
                reportModel = reportModel.OrderBy(b => b.Kod).ToList();
            }

            if (form.kodLaporan == "LAK00802")
            {
                form.Tajuk1 = $"Laporan Penerimaan Bagi Daerah";
                reportModel = await _laporanRepository.AkTerimaByCawangan(form.tarDari1, form.tarHingga1, form.jCawanganId, form.akBankId);
            }

            if (form.kodLaporan == "LAK00803")
            {
                form.Tajuk1 = $"Laporan Harian Kemasukan Ke Bank";
                reportModel = await _laporanRepository.AkTerimaByBank(form.tarDari1, form.tarHingga1, form.jCawanganId, form.akBankId);
            }

            var user = await _userManager.GetUserAsync(User);
            var namaUser = await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.Email == user!.Email);

            form.Username = namaUser?.Nama;

            var kodLaporan = form.kodLaporan;
            CompanyDetails company = new CompanyDetails();
            form.CompanyDetails = company;

            DateTime? date1 = null;
            DateTime? date2 = null;

            //if (!string.IsNullOrEmpty(form.tarDari1) && !string.IsNullOrEmpty(form.tarHingga1))
            //{
            //    date1 = DateTime.Parse(tarikhDari);
            //    date2 = DateTime.Parse(tarikhHingga);
            //}    

            return reportModel;
        }

        public async Task<DataTable> GenerateDataTableLAK00801A(PrintFormModel printmodel)
        {
            // Create a new DataTable
            DataTable dt = new DataTable();
            dt.TableName = "Laporan Penerimaan";

            // Define the columns for the DataTable
            dt.Columns.Add("Bil", typeof(int)); 
            dt.Columns.Add("Tarikh", typeof(DateTime));
            dt.Columns.Add("No Resit", typeof(string));
            dt.Columns.Add("Daripada", typeof(string));
            dt.Columns.Add("Tunai", typeof(decimal));
            dt.Columns.Add("Cek", typeof(decimal));
            dt.Columns.Add("Maklumat Kredit", typeof(decimal));
            dt.Columns.Add("Jumlah", typeof(decimal));
            dt.Columns.Add("Posting", typeof(int));

            
            var terimaList = await _laporanRepository.AkterimaByTarikh(printmodel.tarDari1, printmodel.tarHingga1, printmodel.jCawanganId, printmodel.susunan, printmodel.akBankId);

           
            if (terimaList != null && terimaList.Any() )
            {
                int bil = 1; 

                foreach (var item in terimaList)
                {
                   
                    dt.Rows.Add(
                        bil++,               
                        item.Tarikh,                
                        item.NoRujukan,             
                        item.Daripada,              
                        item.Tunai,                 
                        item.Cek,                   
                        item.MaklumatKredit,        
                        item.Jumlah,
                        item.FlPosting
                    );
                }
            }

            return dt;
        }


        private void RunWorkBookLAK00801A(PrintFormModel printModel, DataTable excelData, string handle)
        {
            List<JCawangan> jcawanganList = _unitOfWork.JCawanganRepo.GetAllDetails();
            List<AkBank> akbankList = _unitOfWork.AkBankRepo.GetAllDetails();

            using (XLWorkbook wb = new XLWorkbook())
            {
                var jcawangan = jcawanganList.FirstOrDefault(j => j.Id == printModel.jCawanganId);
                var akbank = akbankList.FirstOrDefault(j => j.Id == printModel.AkBankId);

                var ws = wb.AddWorksheet("LAK00801A");
                ws.Cell("A1").Value = printModel.CompanyDetails?.NamaSyarikat;
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A2").Value = printModel.Tajuk1;
                if (printModel.jCawanganId != null && jcawanganList != null)
                {
                    ws.Cell("A3").Value = $"{jcawangan?.Kod + " - " + jcawangan?.Perihal}";
                }
                if (printModel.AkBankId != null && akbankList != null)
                {
                    ws.Cell("A4").Value = $"{akbank?.NoAkaun + " (" + akbank?.AkCarta?.Kod + " - " + akbank?.AkCarta?.Perihal}";
                }

                ws.ColumnWidth = 5;
                ws.Cell("A7").InsertTable(excelData)
                    .Theme = XLTableTheme.TableStyleMedium1;

                ws.Column(2)
                   .Style.DateFormat.Format = "dd/MM/yyyy";
                ws.Column(2).AdjustToContents();
                ws.Column(3).AdjustToContents();
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
                ws.Column(9).AdjustToContents();

                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveAs(ms);

                    //This is an equivalent to tempdata, but requires manual cleanup
                    _cache.Set(handle, ms.ToArray(),
                                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));


                }
            }
        }

        public async Task<DataTable> GenerateDataTableLAK00801AA(PrintFormModel printmodel)
        {
            // Create a new DataTable
            DataTable dt = new DataTable();
            dt.TableName = "Laporan Penerimaan";

            // Define the columns for the DataTable
            dt.Columns.Add("Bil", typeof(int));
            dt.Columns.Add("Akaun", typeof(string));
            dt.Columns.Add("Perihalan Akaun", typeof(string));
            dt.Columns.Add("No Resit", typeof(string));
            dt.Columns.Add("Tarikh", typeof(DateTime));
            dt.Columns.Add("Amaun", typeof(decimal));
            dt.Columns.Add("Daripada", typeof(string));


            var terimaList = await _laporanRepository.AkterimaByAkaun(printmodel.tarDari1, printmodel.tarHingga1, printmodel.jCawanganId, printmodel.susunan, printmodel.akBankId);


            if (terimaList != null && terimaList.Any())
            {
                int bil = 1;

                var groupedData = terimaList.GroupBy(b => new { b.Kod, b.Perihal })
                    .Select(g =>
                    {
                        return new
                        {
                            Kod = g.Key.Kod,
                            Perihal = g.Key.Perihal,
                            groupItem = g.ToList(),
                            TotalAmaun = g.Sum(x => x.Amaun)
                        };
                    })
                    .OrderBy(b => b.Kod);

                foreach (var item in groupedData)
                {
                    bool isFirstRowInGroup = true;

                    foreach (var item2 in item.groupItem)
                    {
                        object? KodToShow = isFirstRowInGroup ? item.Kod : DBNull.Value;
                        object? PerihalToShow = isFirstRowInGroup ? item.Perihal : DBNull.Value;
                        int? bilToShow = isFirstRowInGroup ? bil++ : (int?)null;

                        dt.Rows.Add(
                            bilToShow,
                            KodToShow,
                            PerihalToShow,
                            item2.NoRujukan,
                            item2.Tarikh,
                            item2.Amaun,
                            item2.Daripada
                        );

                        isFirstRowInGroup = false;
                    }

                    dt.Rows.Add(
                        DBNull.Value,              
                        DBNull.Value,              
                        "JUMLAH",             
                        DBNull.Value,              
                        DBNull.Value,              
                        item.TotalAmaun,           
                        DBNull.Value               
                    );
                }
            }

            return dt;
        }




        private void RunWorkBookLAK00801AA(PrintFormModel printModel, DataTable excelData,  string handle)
        {
            List<JCawangan> jcawanganList = _unitOfWork.JCawanganRepo.GetAllDetails();
            List<AkBank> akbankList = _unitOfWork.AkBankRepo.GetAllDetails();

            using (XLWorkbook wb = new XLWorkbook())
            {
                var jcawangan = jcawanganList.FirstOrDefault(j => j.Id == printModel.jCawanganId);
                var akbank = akbankList.FirstOrDefault(j => j.Id == printModel.AkBankId);

                var ws = wb.AddWorksheet("LAK00801AA");
                ws.Cell("A1").Value = printModel.CompanyDetails?.NamaSyarikat;
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A2").Value = printModel.Tajuk1;
                if (printModel.jCawanganId != null && jcawanganList != null)
                {
                    ws.Cell("A3").Value = $"{jcawangan?.Kod + " - " + jcawangan?.Perihal}";
                }
                if (printModel.AkBankId != null && akbankList != null)
                {
                    ws.Cell("A4").Value = $"{akbank?.NoAkaun + " (" + akbank?.AkCarta?.Kod + " - " + akbank?.AkCarta?.Perihal}";
                }

                ws.ColumnWidth = 5;
                ws.Cell("A7").InsertTable(excelData)
                    .Theme = XLTableTheme.TableStyleMedium1;

               
                ws.Column(2).AdjustToContents();
                ws.Column(3).AdjustToContents();
                ws.Column(4).AdjustToContents();
                ws.Column(5)
                  .Style.DateFormat.Format = "dd/MM/yyyy";
                ws.Column(5).AdjustToContents();
                ws.Column(6)
                    .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(6).AdjustToContents();
                ws.Column(7).AdjustToContents();



                foreach (var row in ws.RowsUsed())
                {
                    if (row.Cell(3).Value.ToString().Equals("JUMLAH", StringComparison.OrdinalIgnoreCase))
                    {
                        row.Cells(1, 7).Style.Fill.BackgroundColor = XLColor.LightSkyBlue;

                        row.Cells(1, 7).Style.Font.Bold = true;
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

        public async Task<DataTable> GenerateDataTableLAK00802(PrintFormModel printmodel)
        {
            // Create a new DataTable
            DataTable dt = new DataTable();
            dt.TableName = "Laporan Penerimaan";

            // Define the columns for the DataTable
            dt.Columns.Add("Bil", typeof(int));
            dt.Columns.Add("Tarikh", typeof(DateTime));
            dt.Columns.Add("No Resit", typeof(string));
            dt.Columns.Add("Daripada", typeof(string));
            dt.Columns.Add("No Dokumen", typeof(string));
            dt.Columns.Add("No Slip", typeof(string));
            dt.Columns.Add("Tarikh Slip", typeof(DateTime));
            dt.Columns.Add("Amaun", typeof(decimal));
            dt.Columns.Add("Bank", typeof(string));
            dt.Columns.Add("Jumlah", typeof(decimal));


            var terimaList = await _laporanRepository.AkTerimaByCawangan(printmodel.tarDari1, printmodel.tarHingga1, printmodel.jCawanganId, printmodel.akBankId);

            decimal? jumlahTunai = 0;
            decimal? jumlahMK = 0;
            decimal? jumlahCek = 0;

            if (terimaList != null && terimaList.Any())
            {
                int bil = 1;

                foreach (var item in terimaList)
                {

                    dt.Rows.Add(
                        bil++,
                        item.Tarikh,
                        item.NoRujukan,
                        item.Daripada,
                        item.NoDokumen,
                        item.NoSlip,
                        item.TarikhSlip,
                        item.Amaun,
                        item.Bank,
                        item.Jumlah
                    );

                    if (item.NoDokumen == "TUNAI")
                    {
                        jumlahTunai += item.Amaun;
                    }
                    if (item.NoDokumen == "MK")
                    {
                        jumlahMK += item.Amaun;
                    }
                    if (item.NoDokumen != "TUNAI" && item.NoDokumen != "MK")
                    {
                        jumlahCek += item.Amaun;
                    }
                }
            }

            dt.Rows.Add(
               DBNull.Value,
               DBNull.Value,
               DBNull.Value,
               "JUMLAH TUNAI",
               DBNull.Value,
               DBNull.Value,
               DBNull.Value,
               jumlahTunai,
               DBNull.Value,
               DBNull.Value
           );

            dt.Rows.Add(
                DBNull.Value,
                DBNull.Value,
                DBNull.Value,
                "JUMLAH MAKLUMAT KREDIT",
                DBNull.Value,
                DBNull.Value,
                DBNull.Value,
                jumlahMK,
                DBNull.Value,
                DBNull.Value
            );

            dt.Rows.Add(
                DBNull.Value,
                DBNull.Value,
                DBNull.Value,
                "JUMLAH CEK",
                DBNull.Value,
                DBNull.Value,
                DBNull.Value,
                jumlahCek,
                DBNull.Value,
                DBNull.Value
            );

            return dt;
        }

        private void RunWorkBookLAK00802(PrintFormModel printModel, DataTable excelData, string handle)
        {
            List<JCawangan> jcawanganList = _unitOfWork.JCawanganRepo.GetAllDetails();
            List<AkBank> akbankList = _unitOfWork.AkBankRepo.GetAllDetails();

            using (XLWorkbook wb = new XLWorkbook())
            {
                var jcawangan = jcawanganList.FirstOrDefault(j => j.Id == printModel.jCawanganId);
                var akbank = akbankList.FirstOrDefault(j => j.Id == printModel.AkBankId);

                var ws = wb.AddWorksheet("LAK00802");
                ws.Cell("A1").Value = printModel.CompanyDetails?.NamaSyarikat;
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A2").Value = printModel.Tajuk1;
                if (printModel.jCawanganId != null && jcawanganList != null)
                {
                    ws.Cell("A3").Value = $"{jcawangan?.Kod + " - " + jcawangan?.Perihal}";
                }
                if (printModel.AkBankId != null && akbankList != null)
                {
                    ws.Cell("A4").Value = $"{akbank?.NoAkaun + " (" + akbank?.AkCarta?.Kod + " - " + akbank?.AkCarta?.Perihal}";
                }

                ws.ColumnWidth = 5;
                ws.Cell("A7").InsertTable(excelData)
                    .Theme = XLTableTheme.TableStyleMedium1;

                ws.Column(2)
                   .Style.DateFormat.Format = "dd/MM/yyyy";
                ws.Column(2).AdjustToContents();
                ws.Column(3).AdjustToContents();
                ws.Column(4).AdjustToContents();
                ws.Column(5).AdjustToContents();
                ws.Column(6).AdjustToContents();
                ws.Column(7)
                    .Style.DateFormat.Format = "dd/MM/yyyy";
                ws.Column(7).AdjustToContents();
                ws.Column(8)
                   .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(8).AdjustToContents();
                ws.Column(9).AdjustToContents();
                ws.Column(10)
                   .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(10).AdjustToContents();

                foreach (var row in ws.RowsUsed())
                {
                    if (row.Cell(4).Value.ToString().Equals("JUMLAH TUNAI", StringComparison.OrdinalIgnoreCase))
                    {
                        // Change background color
                        row.Cells(4, 10).Style.Fill.BackgroundColor = XLColor.LightSkyBlue;

                        // Make the row bold
                        row.Cells(4, 10).Style.Font.Bold = true;
                    }
                }

                foreach (var row in ws.RowsUsed())
                {
                    if (row.Cell(4).Value.ToString().Equals("JUMLAH MAKLUMAT KREDIT", StringComparison.OrdinalIgnoreCase))
                    {
                        // Change background color
                        row.Cells(4, 10).Style.Fill.BackgroundColor = XLColor.Peach;

                        // Make the row bold
                        row.Cells(4, 10).Style.Font.Bold = true;
                    }
                }

                foreach (var row in ws.RowsUsed())
                {
                    if (row.Cell(4).Value.ToString().Equals("JUMLAH CEK", StringComparison.OrdinalIgnoreCase))
                    {
                        // Change background color
                        row.Cells(4, 10).Style.Fill.BackgroundColor = XLColor.LightGreen;

                        // Make the row bold
                        row.Cells(4, 10).Style.Font.Bold = true;
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


        public async Task<DataTable> GenerateDataTableLAK00803(PrintFormModel printmodel)
        {
            // Create a new DataTable
            DataTable dt = new DataTable();
            dt.TableName = "Laporan Penerimaan";

            // Define the columns for the DataTable
            dt.Columns.Add("Bil", typeof(int));
            dt.Columns.Add("No Dokumen", typeof(string));
            dt.Columns.Add("No Slip", typeof(string));
            dt.Columns.Add("Tarikh Slip", typeof(DateTime));
            dt.Columns.Add("Amaun", typeof(decimal));
            dt.Columns.Add("Bank", typeof(string));
            dt.Columns.Add("Jumlah", typeof(decimal));


            var terimaList = await _laporanRepository.AkTerimaByBank(printmodel.tarDari1, printmodel.tarHingga1, printmodel.jCawanganId, printmodel.akBankId);

            decimal? jumlah = 0;

            if (terimaList != null && terimaList.Any())
            {
                int bil = 1;

                foreach (var item in terimaList)
                {

                    dt.Rows.Add(
                        bil++,
                        item.NoDokumen,
                        item.NoSlip,
                        item.TarikhSlip,
                        item.Amaun,
                        item.Bank,
                        item.Jumlah
                    );

                    jumlah += item.Amaun;
                }
            }

            dt.Rows.Add(
                DBNull.Value,       
                DBNull.Value,       
                DBNull.Value,
                DBNull.Value,
                DBNull.Value,
                "JUMLAH",
                jumlah              
            );

            return dt;
        }

        private void RunWorkBookLAK00803(PrintFormModel printModel, DataTable excelData, string handle)
        {
            List<JCawangan> jcawanganList = _unitOfWork.JCawanganRepo.GetAllDetails();
            List<AkBank> akbankList = _unitOfWork.AkBankRepo.GetAllDetails();

            using (XLWorkbook wb = new XLWorkbook())
            {
                var jcawangan = jcawanganList.FirstOrDefault(j => j.Id == printModel.jCawanganId);
                var akbank = akbankList.FirstOrDefault(j => j.Id == printModel.AkBankId);

                var ws = wb.AddWorksheet("LAK00803");
                ws.Cell("A1").Value = printModel.CompanyDetails?.NamaSyarikat;
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A2").Value = printModel.Tajuk1;
                if (printModel.jCawanganId != null && jcawanganList != null)
                {
                    ws.Cell("A3").Value = $"{jcawangan?.Kod + " - " + jcawangan?.Perihal}";
                }
                if (printModel.AkBankId != null && akbankList != null)
                {
                    ws.Cell("A4").Value = $"{akbank?.NoAkaun + " (" + akbank?.AkCarta?.Kod + " - " + akbank?.AkCarta?.Perihal}";
                }

                ws.ColumnWidth = 5;
                ws.Cell("A7").InsertTable(excelData)
                    .Theme = XLTableTheme.TableStyleMedium1;

               
                ws.Column(2).AdjustToContents();
                ws.Column(3).AdjustToContents();
                ws.Column(4)
                    .Style.DateFormat.Format = "dd/MM/yyyy";
                ws.Column(4).AdjustToContents();
                ws.Column(5)
                   .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(5).AdjustToContents();
                ws.Column(6).AdjustToContents();
                ws.Column(7)
                   .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(7).AdjustToContents();
                ws.Column(8).AdjustToContents();

                foreach (var row in ws.RowsUsed())
                {
                    if (row.Cell(6).Value.ToString().Equals("JUMLAH", StringComparison.OrdinalIgnoreCase))
                    {
                        // Change background color
                        row.Cells(1, 7).Style.Fill.BackgroundColor = XLColor.LightSkyBlue;

                        // Make the row bold
                        row.Cells(1, 7).Style.Font.Bold = true;
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

        private void PopulateSelectList(int? AkBankId, int? JKWId, DateTime? tarDari1, DateTime? tarHingga1, int? jCawanganId, 
            int? dPekerjaId1, int? dPekerjaId2, int? dPekerjaId3,
            List<string>? namaKeywords1 = null, List<string>? jawatanKeywords1 = null, List<string>? bahagianKeywords1 = null,
            List<string>? namaKeywords2 = null, List<string>? jawatanKeywords2 = null, List<string>? bahagianKeywords2 = null,
            List<string>? namaKeywords3 = null, List<string>? jawatanKeywords3 = null, List<string>? bahagianKeywords3 = null,
            List<string>? excludeNama1 = null, List<string>? excludeJawatan1 = null, List<string>? excludeBahagian1 = null,
            List<string>? excludeNama2 = null, List<string>? excludeJawatan2 = null, List<string>? excludeBahagian2 = null,
            List<string>? excludeNama3 = null, List<string>? excludeJawatan3 = null, List<string>? excludeBahagian3 = null)
        {
            // populate list bank 
            List<AkBank> akBankList = _unitOfWork.AkBankRepo.GetAllDetails();

            var bankSelect = new List<SelectListItem>();

            if (akBankList != null)
            {
                bankSelect.Add(new SelectListItem()
                {
                    Text = "-- SEMUA BANK --",
                    Value = ""
                });

                foreach (var item in akBankList)
                {
                    bankSelect.Add(new SelectListItem()
                    {
                        Text = item.AkCarta?.Kod + " - " + item.AkCarta?.Perihal,
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


            // populate list JCawangan 
            List<JCawangan> jCawanganList = _unitOfWork.JCawanganRepo.GetAllDetails();

            var jCawanganSelect = new List<SelectListItem>();

            if (jCawanganList != null)
            {
                jCawanganSelect.Add(new SelectListItem()
                {
                    Text = "-- SEMUA Cawangan --",
                    Value = ""
                });

                foreach (var item in jCawanganList)
                {
                    jCawanganSelect.Add(new SelectListItem()
                    {
                        Text = item.Kod + " - " + item.Perihal,
                        Value = item.Id.ToString()
                    });
                }
                ViewBag.JCawangan = new SelectList(jCawanganSelect, "Value", "Text", jCawanganId);
            }
            else
            {
                jCawanganSelect.Add(new SelectListItem()
                {
                    Text = "-- Tiada Cawangan Berdaftar --",
                    Value = ""
                });

                ViewBag.JCawangan = new SelectList(jCawanganSelect, "Value", "Text", null);
            }
            // populate list jcawangan end



            // populate tarikhDari and tarikhHingga
            if (tarDari1 != null)
            {
                ViewData["DateFrom"] = tarDari1?.ToString("yyyy-MM-dd");
                ViewData["DateTo"] = tarHingga1?.ToString("yyyy-MM-dd");
            }

            // populate pekerja sedia, semak, lulus
            var dPList = _unitOfWork.DPekerjaRepo.GetAllDetails();

            var dropdownParams = new[]
                       {
                (dPekerjaId1, namaKeywords1, jawatanKeywords1, bahagianKeywords1, excludeNama1, excludeJawatan1, excludeBahagian1),
                (dPekerjaId2, namaKeywords2, jawatanKeywords2, bahagianKeywords2, excludeNama2, excludeJawatan2, excludeBahagian2),
                (dPekerjaId3, namaKeywords3, jawatanKeywords3, bahagianKeywords3, excludeNama3, excludeJawatan3, excludeBahagian3)
            };

            for (int i = 0; i < dropdownParams.Length; i++)
            {
                var (dPekerjaId, namaKeywords, jawatanKeywords, bahagianKeywords, excludeNama, excludeJawatan, excludeBahagian) = dropdownParams[i];

                var filteredList = dPList.Where(item =>
                 ((namaKeywords != null && namaKeywords.Any(keyword => item.Nama!.Contains(keyword, StringComparison.OrdinalIgnoreCase))) ||
                 (jawatanKeywords != null && jawatanKeywords.Any(keyword => item.Jawatan!.Contains(keyword, StringComparison.OrdinalIgnoreCase))) ||
                 (bahagianKeywords != null && item.JBahagian != null && bahagianKeywords.Any(keyword => item.JBahagian.Perihal!.Contains(keyword, StringComparison.OrdinalIgnoreCase)))) &&

                 (excludeNama == null || !excludeNama.Any(exclude => item.Nama!.Contains(exclude, StringComparison.OrdinalIgnoreCase))) &&
                 (excludeJawatan == null || !excludeJawatan.Any(exclude => item.Jawatan!.Contains(exclude, StringComparison.OrdinalIgnoreCase))) &&
                 (excludeBahagian == null || (item.JBahagian == null || !excludeBahagian.Any(exclude => item.JBahagian.Perihal!.Contains(exclude, StringComparison.OrdinalIgnoreCase))))
                 ).ToList();

                var dPSelect = filteredList.Select(item => new SelectListItem
                {
                    Text = item.Nama + " | " + item.Jawatan + " | " + item.JBahagian?.Perihal,
                    Value = item.Id.ToString()
                }).ToList();

                if (!dPSelect.Any())
                {
                    dPSelect.Add(new SelectListItem { Text = "-- Tiada Pekerja Berdaftar --", Value = "" });
                }

                if (dPekerjaId.HasValue && dPekerjaId.Value != 0)
                {
                    var specificItem = dPSelect.FirstOrDefault(x => x.Value == dPekerjaId.ToString());
                    if (specificItem != null)
                    {
                        dPSelect.Remove(specificItem);
                        dPSelect.Insert(0, specificItem);
                        specificItem.Selected = true;
                    }
                }

                switch (i)
                {
                    case 0:
                        ViewBag.DPekerja1 = new SelectList(dPSelect, "Value", "Text");
                        break;
                    case 1:
                        ViewBag.DPekerja2 = new SelectList(dPSelect, "Value", "Text");
                        break;
                    case 2:
                        ViewBag.DPekerja3 = new SelectList(dPSelect, "Value", "Text");
                        break;
                }
            }
        }


        // printing List of Laporan
        [AllowAnonymous]
        public async Task<IActionResult> PrintPDF(PrintFormModel form, int? dPekerjaId1, int? dPekerjaId2, int? dPekerjaId3)
        {
            var terima = new List<LAK008PrintModel>();
            var company = await _userServices.GetCompanyDetails();

            var reportModel = await PrepareData(form, dPekerjaId1, dPekerjaId2, dPekerjaId3);

            if (form.kodLaporan == "LAK00801A")
            {
                if (form.susunan == "Tarikh")
                {
                    terima = await _laporanRepository.AkterimaByTarikh(form.tarDari1, form.tarHingga1, form.jCawanganId, form.susunan, form.akBankId);

                    dynamic dyModel = new ExpandoObject();
                    dyModel.ReportModel = terima;
                    dyModel.reportModelGrouped = terima.GroupBy(b => b.Tarikh); // Group by Tarikh

                    var jkw = await _context.JKW.FirstOrDefaultAsync(b => b.Id == form.jKWId);
                    var cawangan = await _context.JCawangan.FirstOrDefaultAsync(b => b.Id == form.jCawanganId);

                    var bank = await _context.AkBank
                      .Include(b => b.AkCarta)
                      .FirstOrDefaultAsync(b => b.Id == form.AkBankId);

                    return new ViewAsPdf("LAK00801APDF", dyModel, new ViewDataDictionary(ViewData)
                    {
                        { "NamaSyarikat", company.NamaSyarikat },
                        { "AlamatSyarikat1", company.AlamatSyarikat1 },
                        { "AlamatSyarikat2", company.AlamatSyarikat2 },
                        { "AlamatSyarikat3", company.AlamatSyarikat3 },
                        { "TarDari", form.tarDari1?.ToString("dd/MM/yyyy") },
                        { "TarHingga", form.tarHingga1?.ToString("dd/MM/yyyy") },
                        { "NamaCawangan", cawangan?.Kod + "-" + cawangan?.Perihal },
                        { "NamaBank", bank?.AkCarta?.Kod + " - " + bank?.AkCarta?.Perihal }
                    })
                    {
                        PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                        PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                        PageSize = Rotativa.AspNetCore.Options.Size.A4,
                    };
                }
                else if (form.susunan == "Akaun")
                {
                    terima = await _laporanRepository.AkterimaByAkaun(form.tarDari1, form.tarHingga1, form.jCawanganId, form.susunan, form.akBankId);

                    dynamic dyModel = new ExpandoObject();
                    dyModel.ReportModel = terima;
                    dyModel.reportModelGrouped = terima.GroupBy(b => b.Kod); // Group by Akaun

                    var bank = await _context.AkBank
                   .Include(b => b.AkCarta)
                   .FirstOrDefaultAsync(b => b.Id == form.AkBankId);

                    var jkw = await _context.JKW.FirstOrDefaultAsync(b => b.Id == form.jKWId);
                    var cawangan = await _context.JCawangan.FirstOrDefaultAsync(b => b.Id == form.jCawanganId);

                    return new ViewAsPdf("LAK00801AAPDF", dyModel, new ViewDataDictionary(ViewData)
                    {
                        { "NamaSyarikat", company.NamaSyarikat },
                        { "AlamatSyarikat1", company.AlamatSyarikat1 },
                        { "AlamatSyarikat2", company.AlamatSyarikat2 },
                        { "AlamatSyarikat3", company.AlamatSyarikat3 },
                        { "TarDari", form.tarDari1?.ToString("dd/MM/yyyy") },
                        { "TarHingga", form.tarHingga1?.ToString("dd/MM/yyyy") },
                        { "NamaCawangan", cawangan?.Kod + "-" + cawangan?.Perihal },
                        { "NamaBank", bank?.AkCarta?.Kod + " - " + bank?.AkCarta?.Perihal }
                    })
                    {
                        PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                        PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                        PageSize = Rotativa.AspNetCore.Options.Size.A4,
                    };
                }
            }

            if (form.kodLaporan == "LAK00802")
            {
                terima = await _laporanRepository.AkTerimaByCawangan(form.tarDari1, form.tarHingga1, form.jCawanganId, form.akBankId);

                var bank = await _context.AkBank
                    .Include(b => b.AkCarta)
                    .FirstOrDefaultAsync(b => b.Id == form.AkBankId);

                dynamic dyModel = new ExpandoObject();
                dyModel.ReportModel = terima;
                dyModel.reportModelGrouped = terima.GroupBy(b => b.NoRujukan);

                var jkw = await _context.JKW.FirstOrDefaultAsync(b => b.Id == form.jKWId);
                var cawangan = await _context.JCawangan.FirstOrDefaultAsync(b => b.Id == form.jCawanganId);

                return new ViewAsPdf("LAK00802PDF", dyModel, new ViewDataDictionary(ViewData)
                    {
                        { "NamaSyarikat", company.NamaSyarikat },
                        { "AlamatSyarikat1", company.AlamatSyarikat1 },
                        { "AlamatSyarikat2", company.AlamatSyarikat2 },
                        { "AlamatSyarikat3", company.AlamatSyarikat3 },
                        { "TarDari", form.tarDari1?.ToString("dd/MM/yyyy") },
                        { "TarHingga", form.tarHingga1?.ToString("dd/MM/yyyy") },
                        { "NamaCawangan", cawangan?.Kod + "-" + cawangan?.Perihal },
                        { "NamaBank", bank?.AkCarta?.Kod + " - " + bank?.AkCarta?.Perihal }
                    })
                {
                    PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                    PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                };
            }


            if (form.kodLaporan == "LAK00803")
            {
                terima = await _laporanRepository.AkTerimaByBank(form.tarDari1, form.tarHingga1, form.jCawanganId, form.akBankId);

                dynamic dyModel = new ExpandoObject();
                dyModel.ReportModel = terima;
                dyModel.reportModelGrouped = terima.GroupBy(b => b.NoSlip);

                var jkw = await _context.JKW.FirstOrDefaultAsync(b => b.Id == form.jKWId);
                var cawangan = await _context.JCawangan.FirstOrDefaultAsync(b => b.Id == form.jCawanganId);
                var bank = await _context.AkBank
                    .Include(b => b.AkCarta)
                    .FirstOrDefaultAsync(b => b.Id == form.AkBankId);



                return new ViewAsPdf("LAK00803PDF", dyModel, new ViewDataDictionary(ViewData)
                    {
                        { "NamaSyarikat", company.NamaSyarikat },
                        { "AlamatSyarikat1", company.AlamatSyarikat1 },
                        { "AlamatSyarikat2", company.AlamatSyarikat2 },
                        { "AlamatSyarikat3", company.AlamatSyarikat3 },
                        { "TarDari", form.tarDari1?.ToString("dd/MM/yyyy") },
                        { "TarHingga", form.tarHingga1?.ToString("dd/MM/yyyy") },
                        { "NamaCawangan", cawangan?.Kod + "-" + cawangan?.Perihal },
                        { "NamaBank", bank?.AkCarta?.Kod + " - " + bank?.AkCarta?.Perihal }
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