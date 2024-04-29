using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using System.Dynamic;
using YIT.__Domain.Entities._Enums;
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
    public class AkPenyataPerubahanEkuitiController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodPAkPerubahanEkuiti;
        public const string namamodul = Modules.namaPAkPerubahanEkuti;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly UserServices _userService;
        private readonly IPenyataRepository _penyata;
        private readonly _IUnitOfWork _unitOfWork;

        public AkPenyataPerubahanEkuitiController(ApplicationDbContext context,
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

            var perubahanEkuitiKW = new _AkPerubahanEkuiti();
            var perubahanEkuitiRizab = new _AkPerubahanEkuiti();
            var perubahanEkuitiAnakSyarikat = new _AkPerubahanEkuiti();
            var perubahanEkuitiKepentinganBukanKawalan = new _AkPerubahanEkuiti();

            if (form.Tahun1 == null)
            {
                form.Tahun1 = DateTime.Now.Year.ToString();
            }

            PopulateSelectList(form.Tahun1);

            if (form.Tahun1 != null)
            {

                perubahanEkuitiKW = await _penyata.GetAkPerubahanEkuiti(EnJenisLajurJadualPerubahanEkuiti.KumpWang, 1, form.Tahun1 ?? DateTime.Now.Year.ToString());

                perubahanEkuitiRizab = await _penyata.GetAkPerubahanEkuiti(EnJenisLajurJadualPerubahanEkuiti.Rizab, null, form.Tahun1 ?? DateTime.Now.Year.ToString());

                perubahanEkuitiAnakSyarikat = await _penyata.GetAkPerubahanEkuiti(EnJenisLajurJadualPerubahanEkuiti.AnakSyarikat, null, form.Tahun1 ?? DateTime.Now.Year.ToString());

                perubahanEkuitiKepentinganBukanKawalan = await _penyata.GetAkPerubahanEkuiti(EnJenisLajurJadualPerubahanEkuiti.KepentinganBukanKawalan, null, form.Tahun1 ?? DateTime.Now.Year.ToString());

            }

            dynamic dyModel = new ExpandoObject();
            dyModel.PerubahanEkuitiKW = perubahanEkuitiKW;
            dyModel.PerubahanEkuitiRizab = perubahanEkuitiRizab;
            dyModel.PerubahanEkuitiAnakSyarikat = perubahanEkuitiAnakSyarikat;
            dyModel.PerubahanEkuitiKepentinganBukanKawalan = perubahanEkuitiKepentinganBukanKawalan;
            return View(dyModel);
        }

        private void PopulateSelectList(string tahun1)
        {

            // populate tahun
            if (String.IsNullOrWhiteSpace(tahun1))
                ViewData["Tahun1"] = DateTime.Now.Year.ToString();
            else
                ViewData["Tahun1"] = tahun1;
        }

        public async Task<IActionResult> PrintPDF(PenyataFormModel form)
        {
            var perubahanEkuitiKW = new _AkPerubahanEkuiti();
            var perubahanEkuitiRizab = new _AkPerubahanEkuiti();
            var perubahanEkuitiAnakSyarikat = new _AkPerubahanEkuiti();
            var perubahanEkuitiKepentinganBukanKawalan = new _AkPerubahanEkuiti();
            dynamic dyModel = new ExpandoObject();
            if (form.Tahun1 == null)
            {
                form.Tahun1 = DateTime.Now.Year.ToString();
            }

            PopulateSelectList(form.Tahun1);

            if (form.Tahun1 != null)
            {

                perubahanEkuitiKW = await _penyata.GetAkPerubahanEkuiti(EnJenisLajurJadualPerubahanEkuiti.KumpWang, 1, form.Tahun1 ?? DateTime.Now.Year.ToString());

                perubahanEkuitiRizab = await _penyata.GetAkPerubahanEkuiti(EnJenisLajurJadualPerubahanEkuiti.Rizab, null, form.Tahun1 ?? DateTime.Now.Year.ToString());

                perubahanEkuitiAnakSyarikat = await _penyata.GetAkPerubahanEkuiti(EnJenisLajurJadualPerubahanEkuiti.AnakSyarikat, null, form.Tahun1 ?? DateTime.Now.Year.ToString());

                perubahanEkuitiKepentinganBukanKawalan = await _penyata.GetAkPerubahanEkuiti(EnJenisLajurJadualPerubahanEkuiti.KepentinganBukanKawalan, null, form.Tahun1 ?? DateTime.Now.Year.ToString());

                dyModel.PerubahanEkuitiKW = perubahanEkuitiKW;
                dyModel.PerubahanEkuitiRizab = perubahanEkuitiRizab;
                dyModel.PerubahanEkuitiAnakSyarikat = perubahanEkuitiAnakSyarikat;
                dyModel.PerubahanEkuitiKepentinganBukanKawalan = perubahanEkuitiKepentinganBukanKawalan;
                var jkw = await _context.JKW.FirstOrDefaultAsync(jkw => jkw.Id == 1);

                var company = await _userService.GetCompanyDetails();

                return new ViewAsPdf("PerubahanEkuitiPDF", dyModel,
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
                ViewData["Tahun"] = DateTime.Now.Year.ToString();

                PopulateSelectList(form.Tahun1 ?? DateTime.Now.Year.ToString());

                TempData[SD.Error] = "Kump. Wang bagi tahun tersebut tidak wujud.";

                return View(dyModel);
            }

        }
    }
}
