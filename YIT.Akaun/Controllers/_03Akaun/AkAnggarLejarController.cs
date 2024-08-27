using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services;
using YIT._DataAccess.Services.Math;
using YIT.Akaun.Infrastructure;
using YIT.Akaun.Models.ViewModels.Common;
using YIT.Akaun.Models.ViewModels.Forms;

namespace YIT.Akaun.Controllers._03Akaun
{
    public class AkAnggarLejarController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly _IUnitOfWork _unitOfWork;
        private readonly IAkAnggarLejarRepository<AkAnggarLejar> _akAnggarLejarRepository;
        private readonly UserServices _userService;
        private readonly ApplicationDbContext _context;

        public AkAnggarLejarController(_IUnitOfWork unitOfWork,
            IAkAnggarLejarRepository<AkAnggarLejar> akAnggarLejarRepository,
            UserServices userService,
            ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _akAnggarLejarRepository = akAnggarLejarRepository;
            _userService = userService;
            _context = context;
        }

        public async Task<IActionResult> Index(
           PenyataFormModel form
           )

        {
            PopulateSelectList(form.JKWId, form.JPTJId, form.AkCartaId, form.TarDari1, form.TarHingga1);

            List<AkAnggarLejar> akAnggarLejarList = new List<AkAnggarLejar>();

            if (form.JKWId != null && form.AkCartaId != null && form.TarDari1 != null && form.TarHingga1 != null)
            {
                // insert baki awal
                //akAnggarLejarList.Add(await _akaunRepository.GetPreviousBalanceByStartingDate(form.JKWId, form.JPTJId, form.AkCartaId, form.TarDari1));

                // insert list akaun
                akAnggarLejarList.AddRange(await _akAnggarLejarRepository.GetResults(form.JKWId, form.AkCartaId, form.TarDari1, form.TarHingga1));
            }
            return View(akAnggarLejarList.OrderBy(ak => ak.Tarikh));
        }

        private void PopulateSelectList(int? JKWId, int? JPTJId, int? AkCarta1Id, DateTime? tarDari1, DateTime? tarHingga1)
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

            // populate list AkCarta 
            List<AkCarta> cartaList = _unitOfWork.AkCartaRepo.GetResultsByParas(EnParas.Paras4);

            var cartaSelect = new List<SelectListItem>();

            if (cartaList != null)
            {
                foreach (var item in cartaList)
                {
                    cartaSelect.Add(new SelectListItem()
                    {
                        Text = item.Kod + " - " + item.Perihal,
                        Value = item.Id.ToString()
                    });
                }
                ViewBag.AkCarta = new SelectList(cartaSelect, "Value", "Text", AkCarta1Id);
            }
            else
            {
                cartaSelect.Add(new SelectListItem()
                {
                    Text = "-- Tiada Carta Berdaftar --",
                    Value = ""
                });

                ViewBag.AkCarta = new SelectList(cartaList, "Value", "Text", null);
            }
            // populate list AkCarta1 end

            // populate tarikhDari and tarikhHingga
            if (tarDari1 != null)
            {
                ViewData["DateFrom"] = tarDari1?.ToString("yyyy-MM-dd");
                ViewData["DateTo"] = tarHingga1?.ToString("yyyy-MM-dd");
            }


        }

        // printing list of akaun
        [AllowAnonymous]
        public async Task<IActionResult> PrintPDF(
            PrintFormModel form)
        {

            List<AkAnggarLejar> akAnggarList = new List<AkAnggarLejar>();


            if (form.JKWId != null && form.AkCartaId != null && form.tarDari1 != null && form.tarHingga1 != null)
            {

                // insert list akaun
                akAnggarList.AddRange(await _akAnggarLejarRepository.GetResults(form.JKWId, form.AkCartaId, form.tarDari1, form.tarHingga1));
            }

            var carta = await _context.AkCarta.FirstOrDefaultAsync(x => x.Id == form.AkCartaId);

            var jkw = await _context.JKW.FirstOrDefaultAsync(b => b.Id == form.JKWId);

            var jptj = await _context.JPTJ.FirstOrDefaultAsync(ptj => ptj.Id == form.JPTJId);
            var jbahagian = await _context.JBahagian.FirstOrDefaultAsync(jbahagian => jbahagian.Id == form.JBahagianId);


            var company = await _userService.GetCompanyDetails();

            return new ViewAsPdf("LejarAkAnggarPDF", akAnggarList.OrderBy(ak => ak.Tarikh),
                new ViewDataDictionary(ViewData) { { "NamaSyarikat", company.NamaSyarikat },
                     { "AlamatSyarikat1", company.AlamatSyarikat1 },
                     { "AlamatSyarikat2", company.AlamatSyarikat2 },
                     { "AlamatSyarikat3", company.AlamatSyarikat3 },
                     { "Tahun", form.Tahun1 },
                     { "Bulan", form.Bulan },
                     { "TarDari", form.tarDari1?.ToString("dd-MM-yyyy") },
                     { "TarHingga", form.tarHingga1?.ToString("dd-MM-yyyy") },
                     { "NamaKW", BelanjawanFormatter.ConvertToKW(jkw?.Kod) + " - " + jkw?.Perihal },
                     { "NamaPTJ", BelanjawanFormatter.ConvertToPTJ(jkw?.Kod,jptj?.Kod) + " - " + jptj?.Perihal },
                     { "NamaBahagian", BelanjawanFormatter.ConvertToBahagian(jkw?.Kod,jptj?.Kod,jbahagian?.Kod) + " - " + jbahagian?.Perihal },
            })
            {
                PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                CustomSwitches = "--footer-center \"[page]/[toPage]\"" +
                        " --footer-line --footer-font-size \"7\" --footer-spacing 1 --footer-font-name \"Segoe UI\"",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
            };
        }
        // printing list of akaun end
    }
}
