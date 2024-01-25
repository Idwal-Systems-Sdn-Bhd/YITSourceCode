using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services;
using YIT._DataAccess.Services.Math;
using YIT.Akaun.Infrastructure;
using YIT.Akaun.Microservices;
using YIT.Akaun.Models.ViewModels.Common;
using YIT.Akaun.Models.ViewModels.Forms;

namespace YIT.Akaun.Controllers._03Akaun
{
    [Authorize]
    public class AkPenyataTimbangDugaController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodPAkTimbangDuga;
        public const string namamodul = Modules.namaPAkTimbangDuga;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly UserServices _userService;
        private readonly IPenyataRepository _penyata;
        private readonly _IUnitOfWork _unitOfWork;

        public AkPenyataTimbangDugaController(ApplicationDbContext context,
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

            var timbangDuga = new List<_AkTimbangDuga>();

            if (form.TarHingga1 == null)
            {
                form.TarHingga1 = DateTime.Now;
            }

            PopulateSelectList(form.JKWId, form.JPTJId, form.TarHingga1, form.EnParas);

            if (form.EnParas != 0)
            {
                timbangDuga = await _penyata.GetAkTimbangDuga(form.JKWId, form.JPTJId, form.TarHingga1?.AddHours(23.99), form.EnParas);
            }
            

            return View(timbangDuga);
        }

        private void PopulateSelectList(int? JKWId, int? JPTJId, DateTime? tarHingga1,EnParas enParas)
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

            // populate paras
            List<ListItemViewModel> parasList = EnumHelper<EnParas>.GetList();
            var parasSelect = new List<SelectListItem>();

            foreach (var item in parasList)
            {
                parasSelect.Add(new SelectListItem()
                {
                    Text = item.perihal,
                    Value = item.id.ToString()
                });

            }
            ViewBag.EnParas = new SelectList(parasSelect, "Value", "Text", enParas.GetDisplayCode());
            // populate paras end

            // populate  tarikhHingga
            if (tarHingga1 != null)
            {
                ViewData["DateTo"] = tarHingga1?.ToString("yyyy-MM-dd");
            }
        }

        public async Task<IActionResult> PrintPDF(PenyataFormModel form)
        {
            var timbangDuga = new List<_AkTimbangDuga>();

            if (form.EnParas != 0)
            {

                timbangDuga = await _penyata.GetAkTimbangDuga(form.JKWId, form.JPTJId, form.TarHingga1?.AddHours(23.99), form.EnParas);

                var jkw = await _context.JKW.FirstOrDefaultAsync(b => b.Id == form.JKWId);

                var jptj = await _context.JPTJ.FirstOrDefaultAsync(ptj => ptj.Id == form.JPTJId);

                var company = await _userService.GetCompanyDetails();

                return new ViewAsPdf("TimbangDugaPDF", timbangDuga,
                    new ViewDataDictionary(ViewData)
                    {
                        { "NamaKW", BelanjawanFormatter.ConvertToKW(jkw?.Kod) + " - " + jkw?.Perihal },
                        { "NamaPTJ", BelanjawanFormatter.ConvertToPTJ(jkw?.Kod,jptj?.Kod) + " - " + jptj?.Perihal },
                        { "Paras", form.EnParas.GetDisplayName().ToUpper() },
                        { "TarHingga", form.TarHingga1?.AddHours(23.99).ToString("dd/MM/yyyy hh:mm:ss tt") },
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
                TempData[SD.Error] = "Sila pilih paras";

                PopulateSelectList(form.JKWId, form.JPTJId, form.TarHingga1, form.EnParas);

                return View(timbangDuga);
            }
        }
    }
}
