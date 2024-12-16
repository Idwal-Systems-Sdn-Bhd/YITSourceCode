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
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT.Akaun.Infrastructure;
using YIT.Akaun.Models.ViewModels.Forms;
using YIT.Akaun.Models.ViewModels.Prints;

namespace YIT.Akaun.Controllers._99Laporan
{
    [Authorize]
    public class LAK010Controller : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodLDaftarBil;
        public const string namamodul = Modules.namaLDaftarBil;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly UserServices _userServices;

        public LAK010Controller(
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
            PopulateSelectList(model.jKWId);
            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> ExportExcel(PrintFormModel model)
        {
            LAK010PrintModel printModel = await PrepareData(model.kodLaporan, model.tarikhDari, model.tarikhHingga, model.jKWId);

            // Generate a new unique identifier against which the file can be stored
            string handle = string.Format("attachment;" + model.kodLaporan + ".xlsx;", string.IsNullOrEmpty(model.kodLaporan) ? Guid.NewGuid().ToString() : WebUtility.UrlEncode(model.kodLaporan));

            // save viewmodel into workbook
            if (model.kodLaporan == "LAK010")
            {
                // construct and insert data into dataTable 
                var excelData = GenerateDataTableLAK010(printModel, model.tarikhDari, model.tarikhHingga, model.jKWId);

                // insert dataTable into Workbook
                RunWorkBookLAK010(printModel, excelData, handle);
            }
            // save viewmodel into workbook

            return Json(new { FileGuid = handle, FileName = model.kodLaporan + ".xlsx" });
        }

        private async Task<LAK010PrintModel> PrepareData(string? kodLaporan, string? tarikhDari, string? tarikhHingga, int? jKWId)
        {
            LAK010PrintModel reportModel = new LAK010PrintModel();

            var user = await _userManager.GetUserAsync(User);
            var namaUser = await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.Email == user!.Email);
            reportModel.CommonModels.Username = namaUser?.Nama;

            reportModel.CommonModels.KodLaporan = kodLaporan;
            reportModel.CommonModels.CompanyDetails = new CompanyDetails();

            DateTime? date1 = null;
            DateTime? date2 = null;

            if (!string.IsNullOrEmpty(tarikhDari) && !string.IsNullOrEmpty(tarikhHingga))
            {
                date1 = DateTime.Parse(tarikhDari);
                date2 = DateTime.Parse(tarikhHingga);
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

            if (kodLaporan == "LAK010")
            {
                reportModel.CommonModels.Tajuk1 = $"Daftar Bil Dari Tarikh : {date1?.ToString("dd/MM/yyyy")} Hingga {date2?.ToString("dd/MM/yyyy")}";
                reportModel.CommonModels.Tajuk2 = $"JKW: {jKWKod} - {jKWPerihal}";
                reportModel.AkBelian = _unitOfWork.AkBelianRepo.GetResults1("", date1, date2, null, jKWId);
            }

            return reportModel;
        }

        private DataTable GenerateDataTableLAK010(LAK010PrintModel printModel, string? tarikhDari, string? tarikhHingga, int? jKWId)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Daftar Bil";
            dt.Columns.Add("Bil", typeof(int));
            dt.Columns.Add("No Invois/Tar Kew Terima", typeof(string));
            dt.Columns.Add("Pembekal Perihal Bayaran Jumlah", typeof(string));
            dt.Columns.Add("Pengesahan Kew. Tar Pengesahan Pembatalan", typeof(string));
            dt.Columns.Add("Tarikh Baucer Disediakan, Objek, No Baucer, No Pesanan", typeof(string));
            dt.Columns.Add("No Cek, Tarikh Cek, Jumlah Tunggakan Hari", typeof(string));

            DateTime date1 = DateTime.MinValue;
            DateTime date2 = DateTime.MaxValue;

            if (!string.IsNullOrEmpty(tarikhDari) && !string.IsNullOrEmpty(tarikhHingga))
            {
                date1 = DateTime.Parse(tarikhDari).Date;
                date2 = DateTime.Parse(tarikhHingga).Date.AddDays(1).AddTicks(-1);
            }

            decimal grandTotalJumlah = 0;

            if (printModel.AkBelian != null)
            {
                var bil = 1;

                var akBelianList = _context.AkBelian
                .Include(a => a.AkBelianObjek)!
                    .ThenInclude(b => b.AkCarta)
                .Include(a => a.AkBelianPerihal)
                .Include(a => a.DDaftarAwam)
                .Include(a => a.AkPVInvois)!
                    .ThenInclude(a => a.AkPV)
                        .ThenInclude(a => a!.AkPVPenerima)
                .Where(a => a.Tarikh >= date1 && a.Tarikh <= date2 && a.JKWId == jKWId)
                .OrderBy(a => a.Tarikh)
                .ThenBy(a => a.NoRujukan)
                .ToList();

                foreach (var akBelian in akBelianList)
                {
                    decimal Jumlah = akBelian.Jumlah;

                    string combinedTTerimaKewangan = $"{akBelian.NoRujukan}\r\n" +
                                                     $"{akBelian.TarikhTerimaKewangan?.ToString("dd/MM/yyyy") ?? string.Empty}";

                    string combinedDDaftarAwam = $"{akBelian.DDaftarAwam?.Kod} {akBelian.DDaftarAwam?.Nama}\r\n" + 
                                                 $"{(akBelian.AkBelianPerihal?.Any() == true ? string.Join(", ", akBelian.AkBelianPerihal.Select(obj => obj.Perihal).Where(perihal => !string.IsNullOrEmpty(perihal))) : string.Empty)}\r\n" + 
                                                 $"{akBelian.Jumlah}";

                    string formattedTAkuanKewangan = akBelian.TarikhAkuanKewangan?.ToString("dd/MM/yyyy") != null ? $"✓\r\n{akBelian.TarikhAkuanKewangan:dd/MM/yyyy}" : string.Empty;

                    string noRujukan = akBelian.AkPO?.NoRujukan! ?? akBelian.AkInden?.NoRujukan!;

                    string combinedDDaftarAwam1 = $"{(akBelian.AkPVInvois?.Any() == true ? string.Join(", ", akBelian.AkPVInvois.Select(obj => obj.AkPV?.Tarikh.ToString("dd/MM/yyyy"))) : string.Empty)}\r\n" +
                                                  $"{(akBelian.AkBelianObjek?.Any() == true ? string.Join(", ", akBelian.AkBelianObjek.Select(obj => obj.AkCarta?.Kod)) : string.Empty)}\r\n" +
                                                  $"{(akBelian.AkPVInvois?.Any() == true ? string.Join(", ", akBelian.AkPVInvois.Select(obj => obj.AkPV?.NoRujukan)) : string.Empty)}\r\n" +
                                                  $"{noRujukan}";

                    string batal = akBelian.FlBatal == 1 ? "Batal" : 
                                   $"{(akBelian.AkPVInvois?.Any() == true ? string.Join(", ", akBelian.AkPVInvois.SelectMany(obj => obj.AkPV?.AkPVPenerima ?? Enumerable.Empty<AkPVPenerima>()).Select(akPVPenerima => akPVPenerima.NoRujukanCaraBayar)) : string.Empty)}\r\n" +
                                   $"{(akBelian.AkPVInvois?.Any() == true ? string.Join("\r\n", akBelian.AkPVInvois.SelectMany(obj => obj.AkPV?.AkPVPenerima ?? Enumerable.Empty<AkPVPenerima>()).Where(akPVPenerima => akPVPenerima.TarikhCaraBayar.HasValue).Select(akPVPenerima =>
                                   $"{akPVPenerima.TarikhCaraBayar!.Value.ToString("dd/MM/yyyy")}\r\n{CalculateTunggakanHari(akBelian)} Hari")) : string.Empty)}";

                    dt.Rows.Add(bil,
                                combinedTTerimaKewangan,
                                combinedDDaftarAwam,
                                formattedTAkuanKewangan,
                                combinedDDaftarAwam1,
                                batal
                            );

                    bil++;
                    grandTotalJumlah += Jumlah;
                }
            }

            var grandTotalRow = dt.NewRow();
            grandTotalRow["Tarikh Baucer Disediakan, Objek, No Baucer, No Pesanan"] = "JUMLAH KESELURUHAN RM";
            grandTotalRow["No Cek, Tarikh Cek, Jumlah Tunggakan Hari"] = grandTotalJumlah;

            dt.Rows.Add(grandTotalRow);

            return dt;
        }

        private void RunWorkBookLAK010(LAK010PrintModel printModel, DataTable excelData, string handle)
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Daftar Bil");
                ws.Cell("A1").Value = printModel.CommonModels.CompanyDetails?.NamaSyarikat;
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A2").Value = printModel.CommonModels.Tajuk1;
                ws.Cell("A3").Value = printModel.CommonModels.Tajuk2;

                ws.ColumnWidth = 5;
                ws.Cell("A5").InsertTable(excelData)
                    .Theme = XLTableTheme.TableStyleMedium1;

                ws.Column(2).AdjustToContents();
                ws.Column(3).Width = 85;
                ws.Column(4).Width = 15;
                ws.Column(5).Width = 24;
                ws.Column(6).Width = 15;

                int grandTotalRowIndex = excelData.Rows.Count + 5;

                ws.Row(grandTotalRowIndex).Style.Font.Bold = true;

                using (MemoryStream ms = new MemoryStream())
                {
                    wb.SaveAs(ms);

                    //This is an equivalent to tempdata, but requires manual cleanup
                    _cache.Set(handle, ms.ToArray(),
                                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));
                }
            }
        }

        private void PopulateSelectList(int? jKWId)
        {
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

            var selectList = new SelectList(kwSelect, "Value", "Text");

            if (jKWId.HasValue && jKWId.Value != 0)
            {
                var selectedItem = selectList.FirstOrDefault(x => x.Value == jKWId.ToString());
                if (selectedItem != null)
                {
                    selectedItem.Selected = true;
                }
            }
            else if (kwSelect.Any())
            {
                kwSelect.First().Selected = true;
            }

            ViewBag.JKW = selectList;
        }

        private int CalculateTunggakanHari(AkBelian akB)
        {
            int tunggakanHari = 0;

            if (akB.AkPVInvois != null && akB.AkPVInvois.Any())
            {
                foreach (var akPVInvois in akB.AkPVInvois)
                {
                    if (akPVInvois.AkPV != null && akPVInvois.AkPV.AkPVPenerima != null && akPVInvois.AkPV.AkPVPenerima.Any())
                    {
                        foreach (var akPVPenerima in akPVInvois.AkPV.AkPVPenerima)
                        {
                            if (akB.TarikhAkuanKewangan.HasValue && akPVPenerima.TarikhCaraBayar.HasValue)
                            {
                                DateTime tarikhAkB = akPVPenerima.TarikhCaraBayar.Value;
                                DateTime tarikhAkPI = akB.TarikhAkuanKewangan.Value;

                                TimeSpan timeSpan = tarikhAkB - tarikhAkPI;
                                tunggakanHari = timeSpan.Days;
                            }
                        }
                    }
                }
            }

            return tunggakanHari;
        }

        public List<AkBelianWithTunggakan> GetAkBelianWithTunggakanList(List<AkBelian> akBelianList)
        {
            var akBelianWithTunggakanList = akBelianList.Select(akB =>
            {
                var perihal = akB.AkBelianPerihal?.Select(obj => obj.Perihal).Where(perihal => !string.IsNullOrEmpty(perihal)).ToList();
                var tarikh = akB.AkPVInvois?.Select(obj => obj.AkPV?.Tarikh.ToString("dd/MM/yyyy")).Where(tarikh => !string.IsNullOrEmpty(tarikh)).ToList();
                var kod = akB.AkBelianObjek?.Select(obj => obj.AkCarta?.Kod).Where(kod => !string.IsNullOrEmpty(kod)).ToList();
                var rujukan = akB.AkPVInvois?.Select(obj => obj.AkPV?.NoRujukan).Where(rujukan => !string.IsNullOrEmpty(rujukan)).ToList();
                var noRujukan = akB.AkPO?.NoRujukan ?? akB.AkInden?.NoRujukan;
                var penerimaList = akB.AkPVInvois?.SelectMany(obj => obj.AkPV?.AkPVPenerima ?? Enumerable.Empty<AkPVPenerima>()).ToList();
                var noRujukanCaraBayarList = penerimaList!.Select(p => p.NoRujukanCaraBayar).Where(r => !string.IsNullOrEmpty(r)).ToList();
                var formattedDatesAndDaysList = penerimaList?.Where(p => p.TarikhCaraBayar.HasValue).Select(p => $"{p.TarikhCaraBayar!.Value:dd/MM/yyyy}<br><br> {CalculateTunggakanHari(akB)} Hari").ToList();

                return new AkBelianWithTunggakan
                {
                    AkBelian = akB,
                    TunggakanHari = CalculateTunggakanHari(akB),
                    TTerimaKewangan = akB.TarikhTerimaKewangan?.ToString("dd/MM/yyyy"),
                    akBPerihal = perihal?.Any() == true ? string.Join(", ", perihal) : null,
                    FormattedTAkuanKewangan = akB.TarikhAkuanKewangan?.ToString("dd/MM/yyyy") != null
                    ? $"<span class='checkmark'>&#10003;</span><br><br>{akB.TarikhAkuanKewangan:dd/MM/yyyy}"
                    : "<span></span>",
                    TarikhPVInvois = tarikh?.Any() == true ? string.Join(", ", tarikh) : null,
                    KodBelianObjek = kod?.Any() == true ? string.Join(", ", kod) : null,
                    NoRujukanPVInvois = rujukan?.Any() == true ? string.Join(", ", rujukan) : null,
                    NoRujukan = noRujukan,
                    Batal = akB.FlBatal == 1
                        ? "<strong>Batal</strong>"
                        : $"{(noRujukanCaraBayarList?.Any() == true ? string.Join(", ", noRujukanCaraBayarList) : string.Empty)}" +
                          $"{(formattedDatesAndDaysList?.Any() == true ? "<br><br>" + string.Join("<br><br>", formattedDatesAndDaysList) : string.Empty)}",
                };
            }).ToList();

            return akBelianWithTunggakanList;
        }

        // printing List of Laporan
        [AllowAnonymous]
        public async Task<IActionResult> Print(string? kodLaporan, string? tarikhDari, string? tarikhHingga, int? jKWId)
        {
            PopulateSelectList(jKWId);

            var reportModel = await PrepareData(kodLaporan, tarikhDari, tarikhHingga, jKWId);
            var company = await _userServices.GetCompanyDetails();

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

                var akbelian = await _unitOfWork.AkBelianRepo.GetResultsGroupByTarikh1(tarikhDari, tarikhHingga, jKWId);

                tarikhDari = DateTime.Parse(tarikhDari!).ToString("dd/MM/yyyy");
                tarikhHingga = DateTime.Parse(tarikhHingga!).ToString("dd/MM/yyyy");

                var akBelianWithTunggakanList = GetAkBelianWithTunggakanList(akbelian);

                reportModel.AkBelianWithTunggakanList = akBelianWithTunggakanList;
                reportModel.Jumlah = akBelianWithTunggakanList.Sum(item => item.AkBelian?.Jumlah ?? 0);

                return new ViewAsPdf("LAK010PDF", reportModel, new ViewDataDictionary(ViewData) {
                    { "NamaSyarikat", company.NamaSyarikat },
                    { "AlamatSyarikat1", company.AlamatSyarikat1 },
                    { "AlamatSyarikat2", company.AlamatSyarikat2 },
                    { "AlamatSyarikat3", company.AlamatSyarikat3 },
                    { "TarikhDari", tarikhDari },
                    { "TarikhHingga", tarikhHingga },
                })
                {
                    PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                    PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                    CustomSwitches = "--footer-center \"[page]/[toPage]\"" +
                        " --footer-line --footer-font-size \"7\" --footer-spacing 1 --footer-font-name \"Segoe UI\"",
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                };
        }
        // printing List of Laporan end

    }

}