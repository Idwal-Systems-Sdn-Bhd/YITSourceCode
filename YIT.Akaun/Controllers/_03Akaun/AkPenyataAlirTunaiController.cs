using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services.Math;
using YIT.Akaun.Infrastructure;
using YIT.Akaun.Models.ViewModels.Forms;

namespace YIT.Akaun.Controllers._03Akaun
{
    [Authorize(Roles = Init.allExceptAdminRole)]
    public class AkPenyataAlirTunaiController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodPAkAlirTunaiBulanan;
        public const string namamodul = Modules.namaPAkAlirTunaiBulanan;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly UserServices _userService;
        private readonly IPenyataRepository _penyata;
        private readonly _IUnitOfWork _unitOfWork;

        public AkPenyataAlirTunaiController(ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            UserServices userService,
            IPenyataRepository penyata,
            _IUnitOfWork unitOfWork)
        {
            _context = context;
            _userManager = userManager;
            _userService = userService;
            _penyata = penyata;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(PenyataFormModel form)
        {

            var alirTunai = new List<_AkAlirTunai>();

            if (form.Tahun1 == null)
            {
                form.Tahun1 = DateTime.Now.Year.ToString();
            }

            PopulateSelectList(form.AkBankId, form.JKWId, form.JPTJId, form.TarDari1, form.Tahun1);

            if (form.AkBankId != null)
            {

                alirTunai = await _penyata.GetAkAlirTunai((int)form.AkBankId,form.JKWId,form.JPTJId,form.Tahun1 ?? DateTime.Now.Year.ToString(), 1);
            }

            return View(alirTunai);
        }

        private void PopulateSelectList(int? akBankId, int? JKWId, int? JPTJId, DateTime? tarDari1, string? tahun1)
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

                    foreach(var item2 in jkwptjBahagianList)
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

            // populate tahun
            if (String.IsNullOrWhiteSpace(tahun1))
                ViewData["Tahun1"] = DateTime.Now.Year.ToString();
            else
                ViewData["Tahun1"] = tahun1;
        }

        public async Task<IActionResult> PrintPDF(PenyataFormModel form)
        {
            var alirTunai = new List<_AkAlirTunai>();

            PopulateSelectList(form.AkBankId, form.JKWId, form.JPTJId, form.TarDari1, form.Tahun1);

            ViewData["Tahun"] = DateTime.Now.Year.ToString();

            if (form.AkBankId != null)
            {

                alirTunai = await _penyata.GetAkAlirTunai((int)form.AkBankId, form.JKWId, form.JPTJId, form.Tahun1 ?? DateTime.Now.Year.ToString(), 1);

                var bank = await _context.AkBank
                    .Include(b => b.AkCarta)
                    .FirstOrDefaultAsync(b => b.Id == form.AkBankId);

                var jkw = await _context.JKW.FirstOrDefaultAsync(jkw => jkw.Id == form.JKWId);

                var jptj = await _context.JPTJ.FirstOrDefaultAsync(ptj => ptj.Id == form.JPTJId);

                var company = await _userService.GetCompanyDetails();

                ViewData["Tahun"] = form.Tahun1;

                return new ViewAsPdf("AlirTunaiPecahanBulanPDF", alirTunai,
                    new ViewDataDictionary(ViewData)
                    {
                        { "NamaKW", BelanjawanFormatter.ConvertToKW(jkw?.Kod) + " - " + jkw?.Perihal },
                        { "NamaPTJ", BelanjawanFormatter.ConvertToPTJ(jkw?.Kod,jptj?.Kod) + " - " + jptj?.Perihal },
                        { "NamaBank", bank?.NoAkaun + " (" + bank?.AkCarta?.Kod + " - " + bank?.AkCarta?.Perihal +") "},
                        { "NamaSyarikat", company.NamaSyarikat },
                        { "AlamatSyarikat1", company.AlamatSyarikat1 },
                        { "AlamatSyarikat2", company.AlamatSyarikat2 },
                        { "AlamatSyarikat3", company.AlamatSyarikat3 }
                    })
                {
                    PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                    PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                    CustomSwitches = "--footer-center \"[page]/[toPage]\"" +
                        " --footer-line --footer-font-size \"7\" --footer-spacing 1 --footer-font-name \"Segoe UI\"",
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                };
            }
            else
            {
                ViewData["Tahun"] = DateTime.Now.Year.ToString();

                PopulateSelectList(form.AkBankId, form.JKWId, form.JPTJId, form.TarDari1, form.Tahun1);

                TempData[SD.Error] = "Akaun Bank Tidak Wujud.";

                return View(alirTunai.OrderBy(b => b.KeluarMasuk).ToList());
            }

        }
    }
}
