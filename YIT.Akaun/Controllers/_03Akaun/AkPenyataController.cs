using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using System.Dynamic;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services.Math;
using YIT.Akaun.Infrastructure;
using YIT.Akaun.Models.ViewModels.Forms;

namespace YIT.Akaun.Controllers._03Akaun
{
    [Authorize(Roles = Init.allExceptAdminRole)]
    public class AkPenyataController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodPAkAlirTunaiTahunan;
        public const string namamodul = Modules.namaPAkAlirTunaiTahunan;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly UserServices _userService;
        private readonly IPenyataRepository _penyata;
        private readonly _IUnitOfWork _unitOfWork;

        public AkPenyataController(ApplicationDbContext context,
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
            List<_AkPenyataAlirTunai> penyataAlirTunai = new List<_AkPenyataAlirTunai>();

            if (form.Tahun1 == null)
            {
                form.Tahun1 = DateTime.Now.Year.ToString();
            }

            if (form.Tahun2 == null)
            {
                form.Tahun2 = DateTime.Now.AddYears(-1).Year.ToString();
            }

            PopulateSelectList(form.Tahun1,form.Tahun2);

            if (form.Tahun1 != null && form.Tahun2 != null)
            {
                penyataAlirTunai = await _penyata.GetAkPenyataAlirTunaiComparedByYears(modul,form.Tahun1, form.Tahun2);
            }

            return View(penyataAlirTunai.OrderBy(p => p.Susunan));
        }

        private void PopulateSelectList(string tahun1, string tahun2)
        {
            // populate tahun
            if (String.IsNullOrWhiteSpace(tahun1))
                ViewData["Tahun1"] = DateTime.Now.Year.ToString();
            else
                ViewData["Tahun1"] = tahun1;

            // populate tahun
            if (String.IsNullOrWhiteSpace(tahun2))
                ViewData["Tahun2"] = DateTime.Now.AddYears(-1).Year.ToString();
            else
                ViewData["Tahun2"] = tahun2;
        }

        public async Task<IActionResult> PrintPDF(PenyataFormModel form)
        {
            List<_AkPenyataAlirTunai> penyataAlirTunai = new List<_AkPenyataAlirTunai>();

            if (form.Tahun1 == null)
            {
                form.Tahun1 = DateTime.Now.Year.ToString();
            }

            if (form.Tahun2 == null)
            {
                form.Tahun2 = DateTime.Now.AddYears(-1).Year.ToString();
            }

            PopulateSelectList(form.Tahun1, form.Tahun2);

            if (form.Tahun1 != null && form.Tahun2 != null)
            {
                penyataAlirTunai = await _penyata.GetAkPenyataAlirTunaiComparedByYears(modul, form.Tahun1, form.Tahun2);

                var jkw = await _context.JKW.FirstOrDefaultAsync(jkw => jkw.Id == 1);

                var company = await _userService.GetCompanyDetails();

                return new ViewAsPdf("AlirTunaiPecahanTahunPDF", penyataAlirTunai.OrderBy(p => p.Susunan),
                    new ViewDataDictionary(ViewData)
                    {
                        { "NamaKW", BelanjawanFormatter.ConvertToKW(jkw?.Kod) + " - " + jkw?.Perihal },
                        { "Tahun", form.Tahun1 },
                        { "NamaSyarikat", company.NamaSyarikat },
                        { "AlamatSyarikat1", company.AlamatSyarikat1 },
                        { "AlamatSyarikat2", company.AlamatSyarikat2 },
                        { "AlamatSyarikat3", company.AlamatSyarikat3 }
                    })
                {
                    PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                    PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                    CustomSwitches = "--footer-center \"[page]/[toPage]\"" +
                        " --footer-line --footer-font-size \"7\" --footer-spacing 1 --footer-font-name \"Segoe UI\"",
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                };
            }
            else
            {
                PopulateSelectList(form.Tahun1 ?? DateTime.Now.Year.ToString(), form.Tahun2 ?? DateTime.Now.AddYears(-1).ToString());

                TempData[SD.Error] = "Kump. Wang bagi tahun tersebut tidak wujud.";

                return View(penyataAlirTunai);
            }
        }
    }
}
