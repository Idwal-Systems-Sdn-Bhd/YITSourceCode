using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services.Cart;
using YIT._DataAccess.Services.Math;
using YIT.Akaun.Infrastructure;

namespace YIT.Akaun.Controllers._03Akaun
{
    [Authorize(Roles = Init.superAdminSupervisorRole)]
    public class AkNotaDebitKreditDikeluarkanLulusController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodLulusAkNotaDebitKreditDikeluarkan;
        public const string namamodul = Modules.namaLulusAkNotaDebitKreditDikeluarkan;
        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;
        private readonly CartAkNotaDebitKreditDikeluarkan _cart;

        public AkNotaDebitKreditDikeluarkanLulusController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices,
            CartAkNotaDebitKreditDikeluarkan cart
            )
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _appLog = appLog;
            _userServices = userServices;
            _cart = cart;
        }

        public IActionResult Index(
                string searchString,
                string searchDate1,
                string searchDate2,
                string searchColumn,
                int? dKonfigKelulusanId,
                string password)
        {
            // load data
            DateTime? date1 = null;
            DateTime? date2 = null;

            PopulateFormFields(searchString, password, searchDate1, searchDate2);

            List<AkNotaDebitKreditDikeluarkan> akNotaDebitKreditDikeluarkan = new List<AkNotaDebitKreditDikeluarkan>();
            if (!string.IsNullOrEmpty(searchDate1) && !string.IsNullOrEmpty(searchDate2))
            {
                date1 = DateTime.Parse(searchDate1);
                date2 = DateTime.Parse(searchDate2);
            }

            if (dKonfigKelulusanId != null)
            {
                // cek is user and password valid or not
                HttpContext.Session.SetInt32("DPelulusId", (int)dKonfigKelulusanId);

                if (_unitOfWork.DKonfigKelulusanRepo.IsValidUser((int)dKonfigKelulusanId, password, EnJenisModulKelulusan.NotaDebitKreditDikeluarkan, EnKategoriKelulusan.Pelulus) == false)
                {
                    TempData[SD.Error] = "Katalaluan Tidak Sah";
                    return View();
                }
                else
                {

                    akNotaDebitKreditDikeluarkan = _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.GetResultsByDPekerjaIdFromDKonfigKelulusan(searchString, date1, date2, searchColumn, EnStatusBorang.None, (int)dKonfigKelulusanId, EnKategoriKelulusan.Pelulus, EnJenisModulKelulusan.NotaDebitKreditDikeluarkan);


                }
            }

            return View(akNotaDebitKreditDikeluarkan);
        }

        private void PopulateFormFields(string searchString, string password, string searchDate1, string searchDate2)
        {
            ViewBag.searchString = searchString;
            ViewBag.password = password;
            ViewBag.searchDate1 = searchDate1 ?? DateTime.Now.ToString("dd/MM/yyyy");
            ViewBag.searchDate2 = searchDate2 ?? DateTime.Now.ToString("dd/MM/yyyy");
            ViewBag.DKonfigKelulusan = _unitOfWork.DKonfigKelulusanRepo.GetResultsByCategoryGroupByDPekerja(EnKategoriKelulusan.Pelulus, EnJenisModulKelulusan.NotaDebitKreditDikeluarkan);
        }

        [Authorize(Policy = modul + "L")]
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akNotaDebitKreditDikeluarkan = _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.GetDetailsById((int)id);
            if (akNotaDebitKreditDikeluarkan == null)
            {
                return NotFound();
            }
            EmptyCart();
            ViewBag.DKonfigKelulusanId = HttpContext.Session.GetInt32("DPelulusId");
            PopulateCartAkNotaDebitKreditDikeluarkanFromDb(akNotaDebitKreditDikeluarkan);
            return View(akNotaDebitKreditDikeluarkan);
        }

        // jsonResults
        public JsonResult EmptyCart()
        {
            try
            {
                _cart.ClearObjek();
                _cart.ClearPerihal();

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        private void PopulateCartAkNotaDebitKreditDikeluarkanFromDb(AkNotaDebitKreditDikeluarkan akNotaDebitKreditDikeluarkan)
        {
            if (akNotaDebitKreditDikeluarkan.AkNotaDebitKreditDikeluarkanObjek != null)
            {
                foreach (var item in akNotaDebitKreditDikeluarkan.AkNotaDebitKreditDikeluarkanObjek)
                {
                    _cart.AddItemObjek(
                            akNotaDebitKreditDikeluarkan.Id,
                            item.JKWPTJBahagianId,
                            item.AkCartaId,
                            item.Amaun);
                }
            }

            if (akNotaDebitKreditDikeluarkan.AkNotaDebitKreditDikeluarkanPerihal != null)
            {
                foreach (var item in akNotaDebitKreditDikeluarkan.AkNotaDebitKreditDikeluarkanPerihal)
                {
                    _cart.AddItemPerihal(
                        akNotaDebitKreditDikeluarkan.Id,
                        item.Bil,
                        item.Perihal,
                        item.Kuantiti,
                        item.LHDNKodKlasifikasiId ?? _unitOfWork.LHDNKodKlasifikasiRepo.GetByCodeAsync("022").Result.Id,
                        item.LHDNUnitUkuranId ?? _unitOfWork.LHDNUnitUkuranRepo.GetByCodeAsync("C62").Result.Id,
                        item.Unit,
                        item.EnLHDNJenisCukai,
                        item.KadarCukai,
                        item.AmaunCukai,
                        item.Harga,
                        item.Amaun
                        );
                }
            }

            PopulateListViewFromCart();
        }

        private void PopulateListViewFromCart()
        {
            List<AkNotaDebitKreditDikeluarkanObjek> objek = _cart.AkNotaDebitKreditDikeluarkanObjek.ToList();

            foreach (AkNotaDebitKreditDikeluarkanObjek item in objek)
            {
                var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                item.JKWPTJBahagian = jkwPtjBahagian;

                item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                item.AkCarta = akCarta;

            }

            ViewBag.akNotaDebitKreditDikeluarkanObjek = objek;

            List<AkNotaDebitKreditDikeluarkanPerihal> perihal = _cart.AkNotaDebitKreditDikeluarkanPerihal.ToList();

            ViewBag.akNotaDebitKreditDikeluarkanPerihal = perihal;
        }

        [Authorize(Policy = modul + "L")]
        public async Task<IActionResult> Lulus(int id, int dKonfigKelulusanId, string syscode)
        {
            var akNotaDebitKreditDikeluarkan = _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akNotaDebitKreditDikeluarkan != null)
            {

                _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.Lulus(id, dKonfigKelulusanId, user?.UserName ?? "");

                _appLog.Insert("Posting", "Melulus " + akNotaDebitKreditDikeluarkan.NoRujukan ?? "" + "; pelulusId: " + dKonfigKelulusanId.ToString(), akNotaDebitKreditDikeluarkan.NoRujukan ?? "", id, akNotaDebitKreditDikeluarkan.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                _context.SaveChanges();
                TempData[SD.Success] = "Data berjaya diluluskan..!";
            }
            else
            {
                TempData[SD.Error] = "Data tidak wujud.";
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = modul + "E")]
        public async Task<IActionResult> HantarSemulaAsync(int id, string? tindakan, string syscode)
        {
            var akNotaDebitKreditDikeluarkan = _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akNotaDebitKreditDikeluarkan != null)
            {
                _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.HantarSemula(id, tindakan, user?.UserName ?? "");

                _appLog.Insert("Ubah", "Hantar Semula " + akNotaDebitKreditDikeluarkan.NoRujukan ?? "", akNotaDebitKreditDikeluarkan.NoRujukan ?? "", id, akNotaDebitKreditDikeluarkan.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                _context.SaveChanges();
                TempData[SD.Success] = "Data berjaya dihantar semula..!";
            }
            else
            {
                TempData[SD.Error] = "Data tidak wujud.";
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
