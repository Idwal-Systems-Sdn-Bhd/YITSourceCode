using Microsoft.AspNetCore.Authorization;
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
using YIT.Akaun.Models.ViewModels.Common;
using YIT.Akaun.Models.ViewModels.Forms;

namespace YIT.Akaun.Controllers._03Akaun
{
    [Authorize(Roles = Init.superAdminSupervisorRole)]
    public class AkAkaunController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly _IUnitOfWork _unitOfWork;
        private readonly IAkAkaunRepository<AkAkaun> _akaunRepository;
        private readonly UserServices _userService;
        private readonly ApplicationDbContext _context;

        public AkAkaunController(_IUnitOfWork unitOfWork,
            IAkAkaunRepository<AkAkaun> akaunRepository,
            UserServices userService,
            ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _akaunRepository = akaunRepository;
            _userService = userService;
            _context = context;
        }
        public async Task<IActionResult> Index(
            PenyataFormModel form
            )

        {
            PopulateSelectList(form.JKWId, form.JPTJId, form.AkCartaId, form.TarDari1, form.TarHingga1);

            List<AkAkaun> akAkaunList = new List<AkAkaun>();

            if (form.JKWId != null && form.AkCartaId != null && form.TarDari1 != null && form.TarHingga1 != null)
            {
                // insert baki awal
                 akAkaunList.Add(await _akaunRepository.GetPreviousBalanceByStartingDate(form.JKWId,form.JPTJId, form.AkCartaId, form.TarDari1));

                // insert list akaun
                akAkaunList.AddRange(await _akaunRepository.GetResults(form.JKWId, form.JPTJId,form.AkCartaId, form.TarDari1, form.TarHingga1));
            }
            return View(akAkaunList.OrderBy(ak => ak.Tarikh));
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

            // populate list AkCarta1 
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
        public async Task<IActionResult> PrintPDF(
            PenyataFormModel form)
        {

            List<AkAkaun> akAkaunList = new List<AkAkaun>();

            if (form.JKWId != null && form.AkCartaId != null && form.TarDari1 != null && form.TarHingga1 != null)
            {
                // insert baki awal
                akAkaunList.Add(await _akaunRepository.GetPreviousBalanceByStartingDate(form.JKWId,form.JPTJId, form.AkCartaId, form.TarDari1));

                // insert list akaun
                akAkaunList.AddRange(await _akaunRepository.GetResults(form.JKWId, form.JPTJId, form.AkCartaId, form.TarDari1, form.TarHingga1));
            }

            var kw = await _context.JKW.FirstOrDefaultAsync(x => x.Id == form.JKWId);
            var carta = await _context.AkCarta.FirstOrDefaultAsync(x => x.Id == form.AkCartaId);

            var company = await _userService.GetCompanyDetails();

            return new ViewAsPdf("LejarAkaunPDF", akAkaunList.OrderBy(ak => ak.Tarikh),
                new ViewDataDictionary(ViewData) { {"searchKW", BelanjawanFormatter.ConvertToKW(kw?.Kod) },
                {"searchCarta", carta?.Kod + " - " + carta?.Perihal  },
                {"tarDari", form.TarDari1?.ToString("dd/MM/yyyy") },
                {"tarHingga", form.TarHingga1?.AddHours(23.99).ToString("dd/MM/yyyy") },
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
        // printing list of akaun end

        public JsonResult GetNamaOrRingkasan(string noRujukan)
        {
            try
            {
                var jenis = noRujukan.Substring(0, 2);

                List<ListItemViewModel> penerima = new List<ListItemViewModel>();
                switch (jenis)
                {
                    // po
                    case "PO":
                        AkPO po = _context.AkPO.Include(b => b.DDaftarAwam).Where(b =>  b.NoRujukan == noRujukan).FirstOrDefault() ?? new AkPO();
                        if (po != null)
                        {
                            penerima.Add(new ListItemViewModel
                            {
                                id = po.Id,
                                indek = 1,
                                perihal = po.DDaftarAwam?.Nama?.ToUpper()
                            });
                        }
                        break;
                    // inden
                    case "IK":
                        AkInden inden = _context.AkInden.Include(b => b.DDaftarAwam).Where(b => b.NoRujukan == noRujukan).FirstOrDefault() ?? new AkInden();
                        if (inden != null)
                        {
                            penerima.Add(new ListItemViewModel
                            {
                                id = inden.Id,
                                indek = 1,
                                perihal = inden.DDaftarAwam?.Nama?.ToUpper()
                            });
                        }
                        break;
                    // belian (invois pembekal)
                    case "IN":
                        AkBelian belian = _context.AkBelian.Include(b => b.DDaftarAwam).Where(b => b.NoRujukan == noRujukan).FirstOrDefault() ?? new AkBelian();
                        if (belian != null)
                        {
                            penerima.Add(new ListItemViewModel
                            {
                                id = belian.Id,
                                indek = 1,
                                perihal = belian.DDaftarAwam?.Nama?.ToUpper()
                            });
                        }
                        break;
                    // nota debit kredit diterima 
                    case "ND":
                    case "NK":
                        AkNotaDebitKreditDiterima akNota = _context.AkNotaDebitKreditDiterima
                            .Include(b => b.AkBelian)
                            .ThenInclude(b => b!.DDaftarAwam)
                            .Where(b => b.NoRujukan == noRujukan).FirstOrDefault() ?? new AkNotaDebitKreditDiterima();
                        if (akNota != null)
                        {
                            penerima.Add(new ListItemViewModel
                            {
                                id = akNota.Id,
                                indek = 1,
                                perihal = akNota.AkBelian?.DDaftarAwam?.Nama?.ToUpper()
                            });
                        }
                        break;
                    // baucer
                    case "PV":
                        AkPV pv = _context.AkPV.Include(b => b.AkPVPenerima).Where(b => b.NoRujukan == noRujukan).FirstOrDefault() ?? new AkPV();
                        if (pv != null)
                        {
                            if (pv.IsGanda == true && pv.AkPVPenerima != null)
                            {
                                var bil = 1;
                                foreach (var item in pv.AkPVPenerima)
                                {
                                    penerima.Add(new ListItemViewModel
                                    {
                                        id = pv.Id,
                                        indek = bil,
                                        perihal = item.NamaPenerima?.ToUpper()
                                    });
                                    bil++;
                                }
                            }
                            else
                            {
                                penerima.Add(new ListItemViewModel
                                {
                                    id = pv.Id,
                                    indek = 1,
                                    perihal = pv.NamaPenerima?.ToUpper()
                                });
                            }

                        }
                        break;
                    // jurnal
                    case "JU":
                    case "JR":
                        AkJurnal jurnal = _context.AkJurnal.Where(b => b.NoRujukan == noRujukan).FirstOrDefault() ?? new AkJurnal();
                        if (jurnal != null)
                        {
                            penerima.Add(new ListItemViewModel
                            {
                                id = jurnal.Id,
                                indek = 1,
                                perihal = jurnal.Ringkasan?.ToUpper()
                            });
                        }
                        break;
                    // invois dikeluarkan
                    //case "ID":
                    //    AkInvois invois = _context.AkInvois.Include(b => b.AkPenghutang).Where(b => b.NoInbois == noRujukan).FirstOrDefault();
                    //    if (invois != null)
                    //    {
                    //        penerima.Add(new ListItemViewModel
                    //        {
                    //            id = invois.Id,
                    //            indek = 1,
                    //            perihal = invois.AkPenghutang.NamaSykt?.ToUpper()
                    //        });
                    //    }
                    //    break;
                    // resit rasmi
                    case "RR":
                        AkTerima resit = _context.AkTerima.Where(b => b.NoRujukan == noRujukan).FirstOrDefault() ?? new AkTerima();
                        if (resit != null)
                        {
                            penerima.Add(new ListItemViewModel
                            {
                                id = resit.Id,
                                indek = 1,
                                perihal = resit.Nama?.ToUpper()
                            });
                        }
                        break;
                }

                return Json(new { result = "OK", record = penerima });
            }
            catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }

        }
    }
}
