using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using System.Dynamic;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services.Math;
using YIT.Akaun.Infrastructure;
using YIT.Akaun.Microservices;
using YIT.Akaun.Models.ViewModels.Common;
using YIT.Akaun.Models.ViewModels.Forms;

namespace YIT.Akaun.Controllers._03Akaun
{
    [Authorize]
    public class AkPenyataUntungRugiController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodPAkUntungRugi;
        public const string namamodul = Modules.namaPAkUntungRugi;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly UserServices _userService;
        private readonly IPenyataRepository _penyata;
        private readonly _IUnitOfWork _unitOfWork;

        public AkPenyataUntungRugiController(ApplicationDbContext context,
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
            var untungRugi = new List<_AkUntungRugi>();

            PopulateSelectList(form.JKWId, form.JPTJId, form.TarDari1, form.TarHingga1);

            untungRugi = await _penyata.GetAkUntungRugi(form.JKWId, form.JPTJId, form.TarDari1, form.TarHingga1);

            dynamic dyModel = new ExpandoObject();
            dyModel.UntungRugi = untungRugi;
            dyModel.UntungRugiGrouped = untungRugi.GroupBy(b => b.Jenis);
            return View(dyModel);
        }

        private void PopulateSelectList(int? JKWId, int? JPTJId, DateTime? tarDari1, DateTime? tarHingga1)
        {
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

            // populate  tarikhHingga
            if (tarDari1 != null)
            {
                ViewData["DateFrom"] = tarDari1?.ToString("yyyy-MM-dd");
            }

            if (tarHingga1 != null)
            {
                ViewData["DateTo"] = tarHingga1?.ToString("yyyy-MM-dd");
            }
        }

        public async Task<IActionResult> PrintPDF(PenyataFormModel form)
        {
            var untungRugi = new List<_AkUntungRugi>();

            PopulateSelectList(form.JKWId, form.JPTJId, form.TarDari1, form.TarHingga1);

            if (form.JKWId != null)
            {
                untungRugi = await _penyata.GetAkUntungRugi(form.JKWId, form.JPTJId, form.TarDari1, form.TarHingga1);

                dynamic dyModel = new ExpandoObject();
                dyModel.UntungRugi = untungRugi;
                dyModel.UntungRugiGrouped = untungRugi.GroupBy(b => b.Jenis);

                var jkw = await _context.JKW.FirstOrDefaultAsync(b => b.Id == form.JKWId);
                var jptj = await _context.JPTJ.FirstOrDefaultAsync(ptj => ptj.Id == form.JPTJId);

                var company = await _userService.GetCompanyDetails();

                return new ViewAsPdf("UntungRugiPDF", dyModel,
                        new ViewDataDictionary(ViewData)
                        {
                        { "TarDari", form.TarDari1?.ToString("dd/MM/yyyy") },
                        { "TarHingga", form.TarHingga1?.ToString("dd/MM/yyyy") },
                        { "NamaKW", BelanjawanFormatter.ConvertToKW(jkw?.Kod) + " - " + jkw?.Perihal },
                        { "NamaPTJ", BelanjawanFormatter.ConvertToPTJ(jkw?.Kod,jptj?.Kod) + " - " + jptj?.Perihal },
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
                var date1 = DateTime.Now.Year.ToString() + "-01-01";
                var date2 = DateTime.Now.ToString("yyyy-MM-dd");
                ViewData["DateFrom"] = date1;
                ViewData["DateTo"] = date2;

                PopulateSelectList(form.JKWId, form.JBahagianId, form.TarDari1, form.TarHingga1);

                TempData[SD.Error] = "Sila pilih Kump. Wang";

                return View(untungRugi);
            }

        }
    }
}
