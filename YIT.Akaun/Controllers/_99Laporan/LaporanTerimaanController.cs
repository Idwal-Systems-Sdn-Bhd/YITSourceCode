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

namespace YIT.Akaun.Controllers._99Laporan
{
    [Authorize]
    public class LaporanTerimaanController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = "LP0001";
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;

        public LaporanTerimaanController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            _IUnitOfWork unitOfWork,
            IMemoryCache cache)
        {
            _context = context;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _cache = cache;
        }
        public IActionResult Index(UserFormModel model)
        {

            //
            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> ExportExcel(UserFormModel model)
        {
            LP0001PrintModel printModel = await PrepareData(model.kodLaporan, model.tarikhDari, model.tarikhHingga, model.susunan);

            // Generate a new unique identifier against which the file can be stored
            string handle = string.Format("attachment;" + model.kodLaporan + ".xlsx;", string.IsNullOrEmpty(model.kodLaporan) ? Guid.NewGuid().ToString() : WebUtility.UrlEncode(model.kodLaporan));

            // save viewmodel into workbook
            if (model.kodLaporan == "LP000101")
            {
                // construct and insert data into dataTable 
                var excelData = GenerateDataTableLP000101(printModel);

                // insert dataTable into Workbook
                RunWorkBookLP000101(printModel, excelData, handle);
            }

            return Json(new { FileGuid = handle, FileName = model.kodLaporan + ".xlsx" });
        }

        private async Task<LP0001PrintModel> PrepareData(string? kodLaporan, string? tarikhDari, string? tarikhHingga, string? susunan)
        {
            LP0001PrintModel reportModel = new LP0001PrintModel();

            if (kodLaporan == "LP000101")
            {
                
                reportModel.Tajuk1 = $"Laporan Daftar Resit Am Dari {@Convert.ToDateTime(tarikhDari).ToString("dd/MM/yyyy")} Hingga {@Convert.ToDateTime(tarikhHingga):dd/MM/yyyy}";
            }

            var user = await _userManager.GetUserAsync(User);
            var namaUser = await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.Email == user!.Email);

            reportModel.Username = namaUser?.Nama;

            reportModel.KodLaporan = kodLaporan;
            CompanyDetails company = new CompanyDetails();
            reportModel.CompanyDetails = company;

            DateTime? date1 = null;
            DateTime? date2 = null;

            if (!string.IsNullOrEmpty(tarikhDari) && !string.IsNullOrEmpty(tarikhHingga))
            {
                date1 = DateTime.Parse(tarikhDari);
                date2 = DateTime.Parse(tarikhHingga);
            }

            reportModel.AkTerima = _unitOfWork.AkTerimaRepo.GetResults("", date1, date2, susunan);

            return reportModel;
        }

        private DataTable GenerateDataTableLP000101(LP0001PrintModel printModel)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Laporan Terimaan";
            dt.Columns.Add("Bil", typeof(int));
            dt.Columns.Add("Tarikh", typeof(DateTime));
            dt.Columns.Add("No Resit", typeof(string));
            dt.Columns.Add("Pembayar", typeof(string));
            dt.Columns.Add("Amaun RM", typeof(decimal));

            if (printModel.AkTerima != null)
            {
                var bil = 1;
                foreach (var item in printModel.AkTerima)
                {

                    dt.Rows.Add(bil,
                        item.Tarikh.ToString("dd/MM/yyyy"),
                        item.NoRujukan,
                        item.Nama?.ToUpper(),
                        item.Jumlah
                        );
                    bil++;
                }

            }
            return dt;
        }
        private void RunWorkBookLP000101(LP0001PrintModel printModel, DataTable excelData, string handle)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Laporan Terimaan");
                ws.Cell("A1").Value = printModel.CompanyDetails?.NamaSyarikat;
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A2").Value = printModel.Tajuk1;
                ws.Cell("A3").Value = printModel.Tajuk2;

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

                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveAs(ms);

                    //This is an equivalent to tempdata, but requires manual cleanup
                    _cache.Set(handle, ms.ToArray(),
                                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));


                }
            }
        }

        

        
    }
}
