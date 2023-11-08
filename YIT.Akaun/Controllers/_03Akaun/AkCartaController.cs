using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services;
using YIT.Akaun.Infrastructure;
using YIT.Akaun.Microservices;
using YIT.Akaun.Models.ViewModels.Common;

namespace YIT.Akaun.Controllers._03Akaun
{
    [Authorize]
    public class AkCartaController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = "AK001";
        public const string namamodul = "Akaun Carta";
        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;

        public AkCartaController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices
            )
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _appLog = appLog;
            _userServices = userServices;
        }
        public IActionResult Index()
        {
            return View(_unitOfWork.AkCartaRepo.GetAll());
        }

        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akCarta = _unitOfWork.AkCartaRepo.GetById((int)id);
            if (akCarta == null)
            {
                return NotFound();
            }

            return View(akCarta);
        }

        public IActionResult Create()
        {
            PopulateDropDownList();
            return View();
        }

        private void PopulateDropDownList()
        {
            var enParas = EnumHelper<EnParas>.GetList();
            ViewBag.EnParas = enParas;
            var enJenis = EnumHelper<EnJenisCarta>.GetList();
            ViewBag.EnJenis = enJenis;
        }

        // POST: Carta/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AkCarta akCarta, string syscode)
        {
            if (akCarta.Kod != null && KodCartaExists(akCarta.Kod) == false)
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    akCarta.UserId = user?.UserName ?? "";

                    akCarta.TarMasuk = DateTime.Now;
                    akCarta.DPekerjaMasukId = pekerjaId;

                    _context.Add(akCarta);
                    _appLog.Insert("Tambah", akCarta.Kod + " - " + akCarta.Perihal, akCarta.Kod, 0, 0, pekerjaId, modul, syscode, namamodul, user);
                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya ditambah..!";
                    return RedirectToAction(nameof(Index));

                }
            }
            else
            {
                TempData[SD.Error] = "Kod ini telah wujud..!";
            }
            PopulateDropDownList();
            return View(akCarta);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var carta = _unitOfWork.AkCartaRepo.GetById((int)id);
            if (carta == null)
            {
                return NotFound();
            }
            PopulateDropDownList();
            return View(carta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AkCarta carta, string syscode)
        {
            if (id != carta.Id)
            {
                return NotFound();
            }

            if (carta.Kod != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = await _context.AkCarta.FirstOrDefaultAsync(x => x.Id == carta.Id);
                    var kodAsal = objAsal!.Kod;
                    var perihalAsal = objAsal.Perihal;
                    carta.UserId = objAsal.UserId;
                    carta.TarMasuk = objAsal.TarMasuk;
                    carta.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                    _context.Entry(objAsal).State = EntityState.Detached;

                    carta.UserIdKemaskini = user?.UserName ?? "";

                    carta.TarKemaskini = DateTime.Now;
                    carta.DPekerjaKemaskiniId = pekerjaId;

                    _unitOfWork.AkCartaRepo.Update(carta);

                    _appLog.Insert("Ubah", kodAsal + " -> " + carta.Kod + ", " + perihalAsal + " -> " + carta.Perihal + ", ", carta.Kod, id, 0, pekerjaId, modul, syscode, namamodul, user);

                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CartaExists(carta.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateDropDownList();
            return View(carta);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var carta = _unitOfWork.AkCartaRepo.GetById((int)id);
            if (carta == null)
            {
                return NotFound();
            }

            return View(carta);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string syscode)
        {
            var carta = _unitOfWork.AkCartaRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (carta != null && carta.Kod != null)
            {
                carta.UserIdKemaskini = user?.UserName ?? "";
                carta.TarKemaskini = DateTime.Now;
                carta.DPekerjaKemaskiniId = pekerjaId;

                _context.AkCarta.Remove(carta);
                _appLog.Insert("Hapus", carta.Kod + " - " + carta.Perihal, carta.Kod, id, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dihapuskan..!";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> RollBack(int id, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            var obj = await _context.AkCarta.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.AkCarta.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.Kod + " - " + obj.Perihal, obj.Kod ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index));
        }

        // printing List of Carta
        [AllowAnonymous]
        public async Task<IActionResult> PrintCarta()
        {
            IEnumerable<AkCarta> akCarta = _unitOfWork.AkCartaRepo.GetAll();

            var company = await _userServices.GetCompanyDetails();
            //string customSwitches = "--page-offset 0 --footer-center [page] / [toPage] --footer-font-size 6";

            return new ViewAsPdf(modul + EnJenisFail.PDF, akCarta,
                new ViewDataDictionary(ViewData) {
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
        // printing List of Carta end
        private bool CartaExists(int id)
        {
            return _unitOfWork.AkCartaRepo.IsExist(c => c.Id == id);
        }

        private bool KodCartaExists(string kod)
        {
            return _unitOfWork.AkCartaRepo.IsExist(c => c.Kod == kod);
        }
    }
}
