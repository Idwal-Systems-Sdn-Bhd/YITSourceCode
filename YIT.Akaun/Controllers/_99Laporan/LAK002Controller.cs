using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MoreLinq;
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
    public class LAK002Controller : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodLCekBelumTunai;
        public const string namamodul = Modules.namaLCekBelumTunai;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly UserServices _userServices;

        public LAK002Controller(
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
            PopulateSelectList(model.akBankId, model.tunai);
            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> ExportExcel(PrintFormModel model)
        {
            int? akBankId = model.kodLaporan == "LAK00201" ? model.akBankId : null;
            int? tunai = model.kodLaporan == "LAK00201" ? model.tunai : null;

            LAK002PrintModel printModel = await PrepareData(model.kodLaporan,model.tarikhDari,model.tarikhHingga,model.susunan,EnStatusBorang.Lulus,akBankId,tunai);

            string handle = string.Format("attachment;" + model.kodLaporan + ".xlsx;", string.IsNullOrEmpty(model.kodLaporan) ? Guid.NewGuid().ToString() : WebUtility.UrlEncode(model.kodLaporan));

            // Generate and save the Excel file based on the report type
            if (model.kodLaporan == "LAK00201")
            {
                // Construct and insert data into DataTable for LAK00201
                var excelData = GenerateDataTableLAK00201(printModel, model.tarikhDari, model.tarikhHingga, akBankId, tunai);

                // Insert DataTable into Workbook for LAK00201
                RunWorkBookLAK00201(printModel, excelData, handle);
            }
            else if (model.kodLaporan == "LAK00202")
            {
                // Construct and insert data into DataTable for LAK00202
                var excelData1 = GenerateDataTableLAK00202(printModel);

                // Insert DataTable into Workbook for LAK00202
                RunWorkBookLAK00202(printModel, excelData1, handle);
            }

            return Json(new { FileGuid = handle, FileName = model.kodLaporan + ".xlsx" });
        }

        private async Task<LAK002PrintModel> PrepareData(string? kodLaporan, string? tarikhDari, string? tarikhHingga, string? susunan, EnStatusBorang enStatusBorang, int? akBankId, int? tunai)
        {
            LAK002PrintModel reportModel = new LAK002PrintModel();

            string? tunai1 = tunai == 1 ? "Sudah" : tunai == 0 ? "Belum" : tunai == 2 ? "Semua" : tunai.ToString();

            if (kodLaporan == "LAK00201")
            {
                reportModel.CommonModels.Tajuk1 = $"Laporan Penunaian Cek Dari Tarikh : {@Convert.ToDateTime(tarikhDari).ToString("dd/MM/yyyy")} Hingga {@Convert.ToDateTime(tarikhHingga):dd/MM/yyyy}";
                reportModel.CommonModels.Tajuk2 = $"Tunai : {tunai1}";
            }
            else if (kodLaporan == "LAK00202")
            {
                reportModel.CommonModels.Tajuk1 = $"Laporan Cek Yang Dibatalkan Dari {@Convert.ToDateTime(tarikhDari).ToString("dd/MM/yyyy")} Hingga {@Convert.ToDateTime(tarikhHingga):dd/MM/yyyy}";
            }

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

            reportModel.AkPV = _unitOfWork.AkPVRepo.GetResults1("", date1, date2, susunan, enStatusBorang, akBankId, tunai, null, null);

            return reportModel;
        }

        private List<DataTable> GenerateDataTableLAK00201(LAK002PrintModel printModel, string? tarikhDari, string? tarikhHingga, int? akBankId, int? tunai)
        {
            List<DataTable> dataTables = new List<DataTable>();

            DateTime date1 = DateTime.MinValue;
            DateTime date2 = DateTime.MaxValue;

            if (!string.IsNullOrEmpty(tarikhDari) && !string.IsNullOrEmpty(tarikhHingga))
            {
                date1 = DateTime.Parse(tarikhDari).Date;
                date2 = DateTime.Parse(tarikhHingga).Date.AddDays(1).AddTicks(-1);
            }

            if (printModel.AkPV != null)
            {
                var akpv = _context.AkPV
                .Include(b => b.AkPVPenerima)
                .Where(b => b.Tarikh >= date1 && b.Tarikh <= date2 && (akBankId == 0 || b.AkBankId == akBankId))
                .ToList();

                var AkBankId1 = akpv.Select(b => b.AkBankId).Distinct().ToList();

                var AkBank1 = _context.AkBank
                                    .Include(b => b.AkCarta)
                                    .Where(b => AkBankId1.Contains(b.Id))
                                    .ToList();

                int? tunaiValue = tunai;

                var groupedAkPVPenerima = akpv
                    .SelectMany(apv => apv.AkPVPenerima!)
                    .Where(apvp =>
                            (tunaiValue == 0 && apvp.IsCekDitunaikan != true) ||
                            (tunaiValue == 1 && apvp.IsCekDitunaikan == true) ||
                            (tunaiValue == 2)
                        )
                    .OrderBy(apvp => apvp.TarikhCaraBayar)
                    .GroupBy(apvp => new { apvp?.AkPV?.AkBankId, apvp?.AkPV?.AkBank?.Perihal })
                    .Select(group => new
                    {
                        AkBankId = group.Key.AkBankId,
                        AkBankName = group.Key.Perihal,
                        AkPvPenerima = group.GroupBy(item => item.TarikhCaraBayar?.Date)
                                            .OrderBy(dateGroup => dateGroup.Key)
                                            .Select(dateGroup => new
                                            {
                                                Date = dateGroup.Key,
                                                AkPvPenerima = dateGroup.OrderBy(item => item.NoRujukanCaraBayar).ToList(),
                                                TotalAmountByDate = dateGroup.Sum(apvp => apvp.Amaun)
                                            }),
                        AkCarta1 = AkBank1.FirstOrDefault(b => b.Id == group.Key.AkBankId)?.AkCarta
                    })
                    .OrderBy(item => AkBank1.FirstOrDefault(b => b.Id == item.AkBankId)?.AkCartaId)
                    .ToList();

                foreach (var group in groupedAkPVPenerima)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("Bil", typeof(int));
                    dt.Columns.Add("No Cek", typeof(string));
                    dt.Columns.Add("Tarikh Cek", typeof(DateTime));
                    dt.Columns.Add("Amaun RM", typeof(decimal));

                    if (tunai != null && tunai == 2)
                    {
                        dt.Columns.Add("Status Tunai", typeof(string));
                    }

                    decimal Jumlah = 0;
                    var bil = 1;

                    foreach (var dateGroup in group.AkPvPenerima)
                    {
                        foreach (var akPVPenerima in dateGroup.AkPvPenerima)
                        {
                            dt.Rows.Add(bil,
                                akPVPenerima.NoRujukanCaraBayar,
                                akPVPenerima.TarikhCaraBayar,
                                akPVPenerima.Amaun
                            );
                            bil++;

                            if (dt.Columns.Contains("Status Tunai"))
                            {
                                dt.Rows[dt.Rows.Count - 1]["Status Tunai"] = akPVPenerima.IsCekDitunaikan ? "Sudah" : "Belum";
                            }
                        }

                        var summaryRow = dt.NewRow();
                        summaryRow["No Cek"] = "Jumlah Pada " + dateGroup.Date?.ToString("dd/MM/yyyy");
                        summaryRow["Amaun RM"] = dateGroup.TotalAmountByDate;
                        dt.Rows.Add(summaryRow);

                        Jumlah += dateGroup.TotalAmountByDate;
                    }

                    var grandTotalRow = dt.NewRow();
                    grandTotalRow["No Cek"] = "JUMLAH";
                    grandTotalRow["Amaun RM"] = Jumlah;
                    dt.Rows.Add(grandTotalRow);

                    dt.ExtendedProperties.Add("BankName", group.AkBankName);
                    dt.ExtendedProperties.Add("AkCartaCode", group.AkCarta1?.Kod);

                    dataTables.Add(dt);
                }
            }

            return dataTables;
        }

        private void RunWorkBookLAK00201(LAK002PrintModel printModel, List<DataTable> dataTables, string handle)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Laporan Penunaian Cek");

                int startRow = 1;

                ws.Cell("A" + startRow).Value = printModel.CommonModels.CompanyDetails?.NamaSyarikat;
                ws.Cell("A" + (startRow + 1)).Value = printModel.CommonModels.Tajuk1;
                ws.Cell("A" + startRow).Style.Font.Bold = true;
                ws.Cell("A" + (startRow + 2)).Value = printModel.CommonModels.Tajuk2;
                startRow += 4;

                foreach (var dt in dataTables)
                {
                    string bankName = dt.ExtendedProperties["BankName"]?.ToString() ?? "Bank Name Not Available";
                    string akCartaCode = dt.ExtendedProperties["AkCartaCode"]?.ToString() ?? "AkCarta Kod Not Available";
                    ws.Cell("A" + startRow).Value = "Bank: " + akCartaCode + " - " + bankName;
                    ws.Cell("A" + startRow).Style.Font.Bold = true;
                    startRow++;
                    ws.Row(startRow).InsertRowsBelow(1);

                    var table = ws.Cell("A" + startRow).InsertTable(dt).Theme = XLTableTheme.TableStyleMedium1;
                    startRow += dt.Rows.Count + 2;
                }

                ws.Column(2).Style.DateFormat.Format = "dd/MM/yyyy hh:mm:ss";

                for (int i = 2; i <= ws.Columns().Count(); i++)
                {
                    ws.Column(i).AdjustToContents();
                }

                foreach (var row in ws.RowsUsed())
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        var cell = row.Cell(i);

                        if ((i == 2 || i == 4) && (cell.Value.ToString().StartsWith("Jumlah Pada") || cell.Value.ToString() == "JUMLAH"))
                        {
                            for (int j = 1; j <= 4; j++)
                            {
                                var targetCell = row.Cell(j);
                                targetCell.Style.Fill.BackgroundColor = XLColor.LightBlue;
                                targetCell.Style.Font.Bold = true;
                            }
                            break;
                        }
                    }
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveAs(ms);
                    _cache.Set(handle, ms.ToArray(),
                               new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));
                }
            }
        }

        private DataTable GenerateDataTableLAK00202(LAK002PrintModel printModel)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Laporan Cek Yang Dibatalkan";
            dt.Columns.Add("Bil", typeof(int));
            dt.Columns.Add("No Cek", typeof(string));
            dt.Columns.Add("Tarikh Cek", typeof(DateTime));
            dt.Columns.Add("No Baucer", typeof(string));
            dt.Columns.Add("No Jurnal", typeof(string));
            dt.Columns.Add("Amaun Cek", typeof(decimal));
            dt.Columns.Add("Nama Penerima", typeof(string));
            dt.Columns.Add("Catatan", typeof(string));

            decimal TotalJumlah = 0;

            if (printModel.AkPV != null)
            {
                var bil = 1;

                foreach (var akPV in printModel.AkPV)
                {
                    if (akPV.AkPVPenerima != null)
                    {
                        foreach (var akPVPenerima in akPV.AkPVPenerima.Where(akPVPenerima => akPVPenerima.JCaraBayarId == 2 && akPVPenerima.EnStatusEFT == EnStatusProses.Fail)) //2 = Cek
                        {
                            decimal Jumlah = 0;
                            // Check if there are any AkJurnalPenerimaCekBatal items
                            if (akPV.AkJurnalPenerimaCekBatal != null && akPV.AkJurnalPenerimaCekBatal.Any())
                            {
                                foreach (var akJurnal in akPV.AkJurnalPenerimaCekBatal)
                                {
                                    // Now, for each akJurnal, you add a new row. Adjust the data accordingly
                                    dt.Rows.Add(bil,
                                        akPVPenerima.NoRujukanCaraBayar,
                                        akPVPenerima.TarikhCaraBayar,
                                        akPV.NoRujukan,
                                        akJurnal.AkJurnal?.NoRujukan, // Using individual NoRujukan from akJurnal
                                        akPVPenerima.Amaun,
                                        akPVPenerima.NamaPenerima,
                                        akPVPenerima.Catatan
                                    );

                                    bil++;
                                    Jumlah += akPVPenerima.Amaun;
                                }
                            }
                            else
                            {
                                // If there are no AkJurnalPenerimaCekBatal items, add a row with some default or empty value for that column
                                dt.Rows.Add(bil,
                                    akPVPenerima.NoRujukanCaraBayar,
                                    akPVPenerima.TarikhCaraBayar,
                                    akPV.NoRujukan,
                                    "",
                                    akPVPenerima.Amaun,
                                    akPVPenerima.NamaPenerima,
                                    akPVPenerima.Catatan
                                );

                                bil++;
                                Jumlah += akPVPenerima.Amaun;
                            }
                            TotalJumlah += Jumlah;
                        }
                    }
                }
                // Add a row for the grand total
                var grandTotalRow = dt.NewRow();
                grandTotalRow["No Cek"] = "JUMLAH RM";
                grandTotalRow["Amaun Cek"] = TotalJumlah;
                dt.Rows.Add(grandTotalRow);

                DataView dv = new DataView(dt);
                dv.Sort = "No Cek";
                dt = dv.ToTable();
            }
            return dt;
        }
        private void RunWorkBookLAK00202(LAK002PrintModel printModel, DataTable excelData1, string handle)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Laporan Cek Yang Dibatalkan");
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
                ws.Column(5)
                    .Style.NumberFormat.Format = " #,##0.00";
                ws.Column(5).AdjustToContents();
                ws.Column(6).AdjustToContents();
                ws.Column(7).AdjustToContents();

                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveAs(ms);

                    //This is an equivalent to tempdata, but requires manual cleanup
                    _cache.Set(handle, ms.ToArray(),
                                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));
                }
            }
        }

        private void PopulateSelectList(int? akBankId, int? tunai)
        {
            var akBankList = _unitOfWork.AkBankRepo.GetAllDetails();
            var bankSelect = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "Semua",
                    Value = "0"
                }
            };

            if (akBankList != null && akBankList.Any())
            {
                bankSelect.AddRange(akBankList.Select(item => new SelectListItem
                {
                    Text = item.NoAkaun + " (" + item.AkCarta?.Kod + " - " + item.AkCarta?.Perihal + ")",
                    Value = item.Id.ToString()
                }));
            }
            else
            {
                bankSelect.Add(new SelectListItem
                {
                    Text = "-- Tiada Bank Berdaftar --",
                    Value = ""
                });
            }

            var selectList = new SelectList(bankSelect, "Value", "Text");

            if (akBankId.HasValue && akBankId.Value != 0)
            {
                var selectedItem = selectList.FirstOrDefault(x => x.Value == akBankId.ToString());
                if (selectedItem != null)
                {
                    selectedItem.Selected = true;
                }
            }

            ViewBag.AkBank = selectList;

            var tunaiSelectList = new List<SelectListItem>
            {
                new SelectListItem { Text = "Sudah", Value = "1" },
                new SelectListItem { Text = "Belum", Value = "0" },
                new SelectListItem { Text = "Semua", Value = "2" }
            };

            ViewBag.Tunai = new SelectList(tunaiSelectList, "Value", "Text");

        }

        [AllowAnonymous]
        public async Task<IActionResult> Print(PrintFormModel model, EnStatusBorang enStatusBorang)
        {
            var company = await _userServices.GetCompanyDetails();
            string viewName;
            string TarikhDari = "";
            string TarikhHingga = "";

            object? modelToPass = null;

            switch (model.kodLaporan)
            {
                case "LAK00201":
                    TarikhDari = DateTime.Parse(model.tarikhDari ?? DateTime.Now.ToString("dd/MM/yyyy")).ToString("dd/MM/yyyy");
                    TarikhHingga = DateTime.Parse(model.tarikhHingga ?? DateTime.Now.ToString("dd/MM/yyyy")).ToString("dd/MM/yyyy");

                    var akpv = await _unitOfWork.AkPVRepo.GetResultsGroupByTarikhCaraBayar(model.tarikhDari, model.tarikhHingga, model.akBankId, model.tunai);

                    var AkBankId1 = akpv.Select(b => b.AkPV?.AkBankId).Distinct().ToList();
                    var AkBank1 = await _context.AkBank
                                               .Include(b => b.AkCarta)
                                               .Where(b => AkBankId1.Contains(b.Id))
                                               .ToListAsync();

                    var groupedAkpv = akpv.GroupBy(b => b.AkPV?.AkBank?.Perihal)
                                          .Select(group => new GroupedReportModel
                                          {
                                              AkBank = group.Key,
                                              AkPVPenerima = group.ToList(),
                                              AkCarta1 = AkBank1.FirstOrDefault(b => b.Perihal == group.Key)?.AkCarta
                                          })
                                          .OrderBy(item => AkBank1.FirstOrDefault(b => b.Perihal == item.AkBank)?.AkCartaId)
                                          .ToList();

                    var reportviewModel = new ReportViewModel
                    {
                        NamaSyarikat = company.NamaSyarikat,
                        AlamatSyarikat1 = company.AlamatSyarikat1,
                        AlamatSyarikat2 = company.AlamatSyarikat2,
                        AlamatSyarikat3 = company.AlamatSyarikat3,
                        TarikhDari = TarikhDari,
                        TarikhHingga = TarikhHingga,
                        Tunai = model.tunai == 1 ? "SUDAH" : model.tunai == 0 ? "BELUM" : "SEMUA",
                        GroupedReportModel = groupedAkpv
                    };

                    // Populate AkPV and AkJurnal
                    var akPVList = new List<AkPV> { /* populate as needed */ };
                    var akJurnalList = new List<AkJurnal> { /* populate as needed */ };

                    // Initialize CommonPrintModel if needed
                    var commonPrintModel = new CommonPrintModel { /* populate as needed */ };

                    // Combine into LAK002PrintModel
                    var viewModel = new LAK002PrintModel
                    {
                        ReportViewModel = reportviewModel,
                        AkPV = akPVList,
                        AkJurnal = akJurnalList,
                        CommonModels = commonPrintModel
                    };

                    modelToPass = viewModel;
                    viewName = "LAK00201PDF";
                    break;

                case "LAK00202":
                    TarikhDari = DateTime.Parse(model.tarikhDari ?? DateTime.Now.ToString("dd/MM/yyyy")).ToString("dd/MM/yyyy");
                    TarikhHingga = DateTime.Parse(model.tarikhHingga ?? DateTime.Now.ToString("dd/MM/yyyy")).ToString("dd/MM/yyyy");

                    var reportModel1 = await PrepareData(model.kodLaporan, model.tarikhDari, model.tarikhHingga, model.susunan, EnStatusBorang.Lulus, null, null);

                    var company1 = await _userServices.GetCompanyDetails();

                    if (model.tarikhDari != null && model.tarikhHingga != null)
                    {
                        List<AkPV> akPv1 = await _context.AkPV
                            .Include(b => b.AkPVPenerima)
                            .Where(b => b.Tarikh >= DateTime.Parse(model.tarikhDari) && b.Tarikh <= DateTime.Parse(model.tarikhHingga))
                            .ToListAsync();
                    }

                    modelToPass = reportModel1;
                    viewName = "LAK00202PDF";
                    break;

                default:
                    viewName = modul + EnJenisFail.PDF;
                    break;
            }

            var viewDataDictionary = new ViewDataDictionary(ViewData)
            {
                { "NamaSyarikat", company.NamaSyarikat },
                { "AlamatSyarikat1", company.AlamatSyarikat1 },
                { "AlamatSyarikat2", company.AlamatSyarikat2 },
                { "AlamatSyarikat3", company.AlamatSyarikat3 },
                { "TarikhDari", TarikhDari },
                { "TarikhHingga", TarikhHingga },
            };

            return new ViewAsPdf(viewName, modelToPass, viewDataDictionary)
            {
                PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--footer-center \"[page]/[toPage]\"" +
                    " --footer-line --footer-font-size \"7\" --footer-spacing 1 --footer-font-name \"Segoe UI\"",
                PageSize = Rotativa.AspNetCore.Options.Size.A4
            };
        }

    }

}