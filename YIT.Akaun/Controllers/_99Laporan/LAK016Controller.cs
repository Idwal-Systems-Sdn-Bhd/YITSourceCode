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
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT.Akaun.Infrastructure;
using YIT.Akaun.Models.ViewModels.Forms;
using YIT.Akaun.Models.ViewModels.Prints;

namespace YIT.Akaun.Controllers._99Laporan
{
    [Authorize]
    public class LAK016Controller : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodLResitDibatalkan;
        public const string namamodul = Modules.namaLResitDibatalkan;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly UserServices _userServices;

        public LAK016Controller(
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
            model.dPekerjaId1 = 80; 
            model.dPekerjaId2 = null; 
            model.dPekerjaId3 = 606; 

            PopulateSelectList(model.jCawanganId, model.jKWId, model.dPekerjaId1, model.dPekerjaId2, model.dPekerjaId3,
            new List<string> { }, new List<string> { "Kew" }, new List<string> { "Pelaburan"},                                       
            new List<string> { }, new List<string> { "Kew" }, new List<string> { "Pelaburan" },                                      
            new List<string> { }, new List<string> { "Pengarah", "KPP" }, new List<string> { },                                      

            new List<string> { }, new List<string> { }, new List<string> { },                                                        
            new List<string> { }, new List<string> { }, new List<string> { },                                                        
            new List<string> { }, new List<string> { "Jusa", "KAFA", "Latihan", "Pendidikan", "Pembangunan" }, new List<string> { } 
            );

            return View(model);
        }


        [HttpPost]
        public async Task<JsonResult> ExportExcel(PrintFormModel model)
        {
            LAK016PrintModel printModel = await PrepareData(model.kodLaporan, model.tarikhDari, model.tarikhHingga, model.jCawanganId, model.jKWId, model.dPekerjaId1, model.dPekerjaId2, model.dPekerjaId3);

            // Generate a new unique identifier against which the file can be stored
            string handle = string.Format("attachment;" + model.kodLaporan + ".xlsx;", string.IsNullOrEmpty(model.kodLaporan) ? Guid.NewGuid().ToString() : WebUtility.UrlEncode(model.kodLaporan));

            // save viewmodel into workbook
            if (model.kodLaporan == "LAK016")
            {
                // construct and insert data into dataTable 
                var excelData = GenerateDataTableLAK016(printModel, model.tarikhDari, model.tarikhHingga, model.jCawanganId, model.jKWId);

                var additionalInfoData = GenerateAdditionalInfoTable(printModel, model.dPekerjaId1, model.dPekerjaId2, model.dPekerjaId3);

                // insert dataTable into Workbook
                RunWorkBookLAK016(printModel, excelData, additionalInfoData, handle, model.dPekerjaId1, model.dPekerjaId2, model.dPekerjaId3);
            }

            return Json(new { FileGuid = handle, FileName = model.kodLaporan + ".xlsx" });
        }

        private async Task<LAK016PrintModel> PrepareData(string? kodLaporan, string? tarikhDari, string? tarikhHingga, int? jCawanganId, int? jKWId, int? dPekerjaId1,
        int? dPekerjaId2, int? dPekerjaId3)
        {
            LAK016PrintModel reportModel = new LAK016PrintModel();

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

            string? jCWKod = "";
            string? jCWPerihal = "";

            if (jCawanganId.HasValue)
            {
                var jCWDetails = await _context.JCawangan
                                    .Where(j => j.Id == jCawanganId)
                                    .Select(j => new { j.Kod, j.Perihal })
                                    .FirstOrDefaultAsync();

                if (jCWDetails != null)
                {
                    jCWKod = jCWDetails.Kod;
                    jCWPerihal = jCWDetails.Perihal;
                }
            }

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

            if (dPekerjaId1.HasValue)
            {
                var pekerja1 = await _context.DPekerja.FindAsync(dPekerjaId1);
                reportModel.NamaDisedia = pekerja1?.Nama;
                reportModel.JawatanDisedia = pekerja1?.Jawatan;
            }

            if (dPekerjaId2.HasValue)
            {
                var pekerja2 = await _context.DPekerja.FindAsync(dPekerjaId2);
                reportModel.NamaSemak = pekerja2?.Nama;
                reportModel.JawatanSemak = pekerja2?.Jawatan;
            }

            if (dPekerjaId3.HasValue)
            {
                var pekerja3 = await _context.DPekerja.FindAsync(dPekerjaId3);
                reportModel.NamaDiluluskan = pekerja3?.Nama;
                reportModel.JawatanDiluluskan = pekerja3?.Jawatan;
            }

            if (kodLaporan == "LAK016")
            {
                reportModel.CommonModels.Tajuk1 = $"Laporan Resit Yang Dibatalkan Untuk Tarikh {Convert.ToDateTime(tarikhDari):dd/MM/yyyy} Hingga {Convert.ToDateTime(tarikhHingga):dd/MM/yyyy} Bagi Cawangan {jCWKod} - {jCWPerihal}";
                reportModel.CommonModels.Tajuk2 = $"JKW: {jKWKod} - {jKWPerihal}";
                reportModel.AkTerimaTunggal = _unitOfWork.AkTerimaTunggalRepo.GetResults1(date1, date2, jCawanganId, jKWId);
            }

            reportModel.AkTerimaTunggal = _unitOfWork.AkTerimaTunggalRepo.GetResults1(date1, date2, jCawanganId, jKWId);

            return reportModel;
        }

        private DataTable GenerateDataTableLAK016(LAK016PrintModel printModel, string? tarikhDari, string? tarikhHingga, int? jCawanganId, int? jKWId)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Laporan Resit Yang Dibatalkan";
            dt.Columns.Add("Bil", typeof(int));
            dt.Columns.Add("No Rujukan", typeof(string));
            dt.Columns.Add("Nama Penerima", typeof(string));
            dt.Columns.Add("Tarikh", typeof(string));
            dt.Columns.Add("Cara Bayar", typeof(string));
            dt.Columns.Add("Sebab Batal", typeof(string));
            dt.Columns.Add("Tarikh Batal", typeof(string));
            dt.Columns.Add("Jumlah RM", typeof(decimal));

            DateTime date1 = DateTime.MinValue;
            DateTime date2 = DateTime.MaxValue;

            if (!string.IsNullOrEmpty(tarikhDari) && !string.IsNullOrEmpty(tarikhHingga))
            {
                date1 = DateTime.Parse(tarikhDari).Date;
                date2 = DateTime.Parse(tarikhHingga).Date.AddDays(1).AddTicks(-1);
            }

            decimal grandTotalJumlah = 0;

            if (printModel.AkTerimaTunggal != null)
            {
                var bil = 1;

                var akTT = _context.AkTerimaTunggal
                .Where(b => b.FlBatal == 1 && b.Tarikh >= date1 && b.Tarikh <= date2 && b.JCawanganId == jCawanganId && b.JKWId == jKWId)
                .OrderBy(a => a.Tarikh)
                .ThenBy(a => a.NoRujukan)
                .ToList();

                foreach (var akT in akTT)
                {
                    decimal Jumlah = akT.Jumlah;

                    dt.Rows.Add(bil,
                        akT.NoRujukan,
                        akT.Nama,
                        akT.Tarikh.ToString("dd/MM/yyyy"),
                        akT.JCaraBayar?.Perihal,
                        akT.SebabBatal,
                        akT.TarBatal?.ToString("dd/MM/yyyy"),
                        akT.Jumlah.ToString("#,##0.00")
                    );

                    bil++;
                    grandTotalJumlah += Jumlah;
                }

                var grandTotalRow = dt.NewRow();
                grandTotalRow["Tarikh Batal"] = "JUMLAH RM";
                grandTotalRow["Jumlah RM"] = grandTotalJumlah;

                dt.Rows.Add(grandTotalRow);
            }

            return dt;
        }

        private void RunWorkBookLAK016(LAK016PrintModel printModel, DataTable excelData, DataTable additionalInfoData, string handle, int? dPekerjaId1, int? dPekerjaId2, int? dPekerjaId3)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Laporan Resit Yang Dibatalkan");

                ws.Cell("A1").Value = printModel.CommonModels.CompanyDetails?.NamaSyarikat;
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A2").Value = printModel.CommonModels.Tajuk1;
                ws.Cell("A3").Value = printModel.CommonModels.Tajuk2;

                ws.ColumnWidth = 5;

                ws.Cell("A5").InsertTable(excelData).Theme = XLTableTheme.TableStyleMedium1;
                for (int colIndex = 2; colIndex <= 8; colIndex++)
                {
                    ws.Column(colIndex).AdjustToContents();
                }
                ws.Column(6).Style.NumberFormat.Format = " #,##0.00";

                int grandTotalRowIndex = excelData.Rows.Count + 5;

                ws.Row(grandTotalRowIndex).Style.Font.Bold = true;

                int additionalInfoStartRow = excelData.Rows.Count + 7;

                for (int i = 0; i < additionalInfoData.Rows.Count; i++)
                {
                    for (int j = 0; j < additionalInfoData.Columns.Count; j++)
                    {
                        var cell = ws.Cell(additionalInfoStartRow + i, 3 + j);
                        cell.Value = additionalInfoData.Rows[i][j].ToString();

                        if (i == 0 || i == additionalInfoData.Rows.Count - 1)
                        {
                            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            cell.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        }
                        else if (j == 0 || j == additionalInfoData.Columns.Count - 1)
                        {
                            cell.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                            cell.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        }

                        cell.Style.Fill.BackgroundColor = XLColor.Transparent;
                        cell.Style.Font.FontColor = XLColor.Black;
                    }
                }

                if (additionalInfoData.Rows.Count > 0)
                {
                    ws.Row(additionalInfoStartRow).Style.Font.Bold = true;
                }

                int additionalInfoColumnCount = additionalInfoData.Columns.Count;
                for (int colIndex = 3; colIndex < 3 + additionalInfoColumnCount; colIndex++)
                {
                    ws.Column(colIndex).AdjustToContents();
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    _cache.Set(handle, ms.ToArray(), new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));
                }
            }
        }

        private DataTable GenerateAdditionalInfoTable(LAK016PrintModel printModel, int? dPekerjaId1, int? dPekerjaId2, int? dPekerjaId3)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Additional Information";

            dt.Columns.Add("Disedia", typeof(string));
            dt.Columns.Add("Disemak", typeof(string));
            dt.Columns.Add("Diluluskan", typeof(string));

            var headerRow = dt.NewRow();
            headerRow["Disedia"] = "Disedia";
            headerRow["Disemak"] = "Disemak";
            headerRow["Diluluskan"] = "Diluluskan";
            dt.Rows.Add(headerRow);  

            var emptyRow1 = dt.NewRow();
            var emptyRow2 = dt.NewRow();
            dt.Rows.Add(emptyRow1);  
            dt.Rows.Add(emptyRow2);  

            var row = dt.NewRow();
            row["Disedia"] = $"Nama: {printModel.NamaDisedia}{Environment.NewLine}Jawatan: {printModel.JawatanDisedia}";
            row["Disemak"] = $"Nama: {printModel.NamaSemak}{Environment.NewLine}Jawatan: {printModel.JawatanSemak}";
            row["Diluluskan"] = $"Nama: {printModel.NamaDiluluskan}{Environment.NewLine}Jawatan: {printModel.JawatanDiluluskan}";

            dt.Rows.Add(row);

            return dt;
        }

        private void PopulateSelectList(int? jCawanganId, int? jKWId, int? dPekerjaId1, int? dPekerjaId2, int? dPekerjaId3,
        List<string>? namaKeywords1 = null, List<string>? jawatanKeywords1 = null, List<string>? bahagianKeywords1 = null,
        List<string>? namaKeywords2 = null, List<string>? jawatanKeywords2 = null, List<string>? bahagianKeywords2 = null,
        List<string>? namaKeywords3 = null, List<string>? jawatanKeywords3 = null, List<string>? bahagianKeywords3 = null,
        List<string>? excludeNama1 = null,  List<string>? excludeJawatan1 = null, List<string>? excludeBahagian1 = null,
        List<string>? excludeNama2 = null,  List<string>? excludeJawatan2 = null, List<string>? excludeBahagian2 = null,
        List<string>? excludeNama3 = null,  List<string>? excludeJawatan3 = null, List<string>? excludeBahagian3 = null)
        {

            var jcawanganList = _unitOfWork.JCawanganRepo.GetAll();
            var cwSelect = new List<SelectListItem>();

            if (jcawanganList != null && jcawanganList.Any())
            {
                cwSelect.AddRange(jcawanganList.Select(item => new SelectListItem
                {
                    Text = item.Kod + " - " + item.Perihal,
                    Value = item.Id.ToString()
                }));
            }
            else
            {
                cwSelect.Add(new SelectListItem
                {
                    Text = "-- Tiada Cawangan Berdaftar --",
                    Value = ""
                });
            }

            var selectList = new SelectList(cwSelect, "Value", "Text");

            if (jCawanganId.HasValue && jCawanganId.Value != 0)
            {
                var selectedItem = selectList.FirstOrDefault(x => x.Value == jCawanganId.ToString());
                if (selectedItem != null)
                {
                    selectedItem.Selected = true;
                }
            }
            else if (cwSelect.Any())
            {
                cwSelect.First().Selected = true;
            }

            ViewBag.JCawangan = selectList;

            var jKWList = _unitOfWork.JKWRepo.GetAllDetails();
            var kwSelect = new List<SelectListItem>();

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

            var selectList1 = new SelectList(kwSelect, "Value", "Text");

            if (jKWId.HasValue && jKWId.Value != 0)
            {
                var selectedItem = selectList1.FirstOrDefault(x => x.Value == jKWId.ToString());
                if (selectedItem != null)
                {
                    selectedItem.Selected = true;
                }
            }
            else if (kwSelect.Any())
            {
                kwSelect.First().Selected = true;
            }

            ViewBag.JKW = selectList1;

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
        public async Task<IActionResult> Print(string? kodLaporan, string? tarikhDari, string? tarikhHingga, int? jCawanganId, int? jKWId,
        int? dPekerjaId1, int? dPekerjaId2, int? dPekerjaId3)
        {
            var reportModel = await PrepareData(kodLaporan, tarikhDari, tarikhHingga, jCawanganId, jKWId, dPekerjaId1, dPekerjaId2, dPekerjaId3);

            var company = await _userServices.GetCompanyDetails();

            if (jCawanganId.HasValue)
            {
                var jCWDetails = await _context.JCawangan
                                    .Where(j => j.Id == jCawanganId)
                                    .Select(j => new { j.Kod, j.Perihal })
                                    .FirstOrDefaultAsync();

                if (jCWDetails != null)
                {
                    ViewBag.jCWKod = jCWDetails.Kod;
                    ViewBag.jCWPerihal = jCWDetails.Perihal;
                }
            }

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

            var akt = await _unitOfWork.AkTerimaTunggalRepo.GetResultsGroupByTarikh1(tarikhDari, tarikhHingga, jCawanganId, jKWId);

            reportModel.AkTerimaTunggal= akt;

            tarikhDari = DateTime.Parse(tarikhDari!).ToString("dd/MM/yyyy");
            tarikhHingga = DateTime.Parse(tarikhHingga!).ToString("dd/MM/yyyy");

            return new ViewAsPdf("LAK016PDF", reportModel, new ViewDataDictionary(ViewData)
            {
                { "NamaSyarikat", company.NamaSyarikat },
                { "AlamatSyarikat1", company.AlamatSyarikat1 },
                { "AlamatSyarikat2", company.AlamatSyarikat2 },
                { "AlamatSyarikat3", company.AlamatSyarikat3 },
                { "TarikhDari", tarikhDari },
                { "TarikhHingga", tarikhHingga },
                { "NamaDisedia", reportModel.NamaDisedia },
                { "JawatanDisedia", reportModel.JawatanDisedia },
                { "NamaSemak", reportModel.NamaSemak },
                { "JawatanSemak", reportModel.JawatanSemak },
                { "NamaDiluluskan", reportModel.NamaDiluluskan },
                { "JawatanDiluluskan", reportModel.JawatanDiluluskan },
            })
            {
                PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                CustomSwitches = "--footer-center \"[page]/[toPage]\"" +
                    " --footer-line --footer-font-size \"7\" --footer-spacing 1 --footer-font-name \"Segoe UI\"",
                PageSize = Rotativa.AspNetCore.Options.Size.A4
            };
        }
        // printing List of Laporan end
    }

}