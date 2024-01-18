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
    [Authorize]
    public class AkPenyataBukuTunaiController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodPAkBukuTunai;
        public const string namamodul = Modules.namaPAkBukuTunai;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly UserServices _userService;
        private readonly IPenyataRepository _penyata;
        private readonly _IUnitOfWork _unitOfWork;

        public AkPenyataBukuTunaiController(ApplicationDbContext context,
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

            var bukuTunai = new List<_AkBukuTunai>();

            if (form.TarDari1 ==  null && form.TarHingga1 == null)
            {
                form.TarDari1 = new DateTime(DateTime.Now.Year,1,1);
                form.TarHingga1 = DateTime.Now;
            }

            PopulateSelectList(form.AkBankId,form.JKWId,form.JPTJId, form.TarDari1, form.TarHingga1);

            if (form.AkBankId != null)
            {

                bukuTunai = await _penyata.GetAkBukuTunai((int)form.AkBankId!, form.JKWId, form.JPTJId, form.TarDari1, form.TarHingga1?.AddHours(23.99));
            }

            return View(bukuTunai);
        }

        private void PopulateSelectList(int? akBankId,int? JKWId, int? JPTJId, DateTime? tarDari1, DateTime? tarHingga1)
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
                foreach (var item in jptjList)
                {
                    List<JKWPTJBahagian> jkwPtjBahagianList = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsByJPTJId(item.Id);

                    foreach (var item2 in jkwPtjBahagianList)
                    {
                        jptjSelect.Add(new SelectListItem()
                        {
                            Text = BelanjawanFormatter.ConvertToPTJ(item2.JKW?.Kod,item.Kod) + " - " + item.Perihal,
                        Value = item.Id.ToString()
                    });
                    }
                    
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

        public async Task<IActionResult> PrintPDF(PenyataFormModel form)
        {
            var bukuTunai = new List<_AkBukuTunai>();

            if (form.AkBankId != 0)
            {

                bukuTunai = await _penyata.GetAkBukuTunai((int)form.AkBankId!, form.JKWId, form.JPTJId, form.TarDari1, form.TarHingga1?.AddHours(23.99));

                var bank = await _context.AkBank
                    .Include(b => b.AkCarta)
                    .FirstOrDefaultAsync(b => b.Id == form.AkBankId);

                var company = await _userService.GetCompanyDetails();

                return new ViewAsPdf("BukuTunaiPDF", bukuTunai,
                    new ViewDataDictionary(ViewData)
                    {
                        { "TarDari", form.TarDari1?.ToString("dd/MM/yyyy hh:mm:ss tt") },
                        { "TarHingga", form.TarHingga1?.AddHours(23.99).ToString("dd/MM/yyyy hh:mm:ss tt") },
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

                var date1 = DateTime.Now.Year.ToString() + "-01-01T00:00:01";
                var date2 = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                ViewData["DateFrom"] = date1;
                ViewData["DateTo"] = date2;

                PopulateSelectList(form.AkBankId, form.JKWId, form.JPTJId, form.TarDari1, form.TarHingga1);

                TempData[SD.Error] = "Akaun Bank Tidak Wujud.";

                return View(bukuTunai);
            }

        }
    }
}
