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
    public class AkPelarasanPOLulusController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodLulusAkPelarasanPO;
        public const string namamodul = Modules.namaLulusAkPelarasanPO;
        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;
        private readonly CartAkPelarasanPO _cart;

        public AkPelarasanPOLulusController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices,
            CartAkPelarasanPO cart
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

            List<AkPelarasanPO> akPelarasanPO = new List<AkPelarasanPO>();
            if (!string.IsNullOrEmpty(searchDate1) && !string.IsNullOrEmpty(searchDate2))
            {
                date1 = DateTime.Parse(searchDate1);
                date2 = DateTime.Parse(searchDate2);
            }

            if (dKonfigKelulusanId != null)
            {
                // cek is user and password valid or not
                HttpContext.Session.SetInt32("DPelulusId", (int)dKonfigKelulusanId);

                if (_unitOfWork.DKonfigKelulusanRepo.IsValidUser((int)dKonfigKelulusanId, password, EnJenisModulKelulusan.PelarasanPO, EnKategoriKelulusan.Pelulus) == false)
                {
                    TempData[SD.Error] = "Katalaluan Tidak Sah";
                    return View();
                }
                else
                {

                    akPelarasanPO = _unitOfWork.AkPelarasanPORepo.GetResultsByDPekerjaIdFromDKonfigKelulusan(searchString, date1, date2, searchColumn, EnStatusBorang.None, (int)dKonfigKelulusanId, EnKategoriKelulusan.Pelulus, EnJenisModulKelulusan.PelarasanPO);


                }
            }

            return View(akPelarasanPO);
        }

        private void PopulateFormFields(string searchString, string password, string searchDate1, string searchDate2)
        {
            ViewBag.searchString = searchString;
            ViewBag.password = password;
            ViewBag.searchDate1 = searchDate1 ?? DateTime.Now.ToString("dd/MM/yyyy");
            ViewBag.searchDate2 = searchDate2 ?? DateTime.Now.ToString("dd/MM/yyyy");
            ViewBag.DKonfigKelulusan = _unitOfWork.DKonfigKelulusanRepo.GetResultsByCategoryGroupByDPekerja(EnKategoriKelulusan.Pelulus, EnJenisModulKelulusan.PelarasanPO);
        }

        [Authorize(Policy = modul + "L")]
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akPelarasanPO = _unitOfWork.AkPelarasanPORepo.GetDetailsById((int)id);
            if (akPelarasanPO == null)
            {
                return NotFound();
            }
            EmptyCart();
            ViewBag.DKonfigKelulusanId = HttpContext.Session.GetInt32("DPelulusId");
            PopulateCartAkPelarasanPOFromDb(akPelarasanPO);
            return View(akPelarasanPO);
        }

        private void PopulateCartAkPelarasanPOFromDb(AkPelarasanPO akPelarasanPO)
        {
            if (akPelarasanPO.AkPelarasanPOObjek != null)
            {
                foreach (var item in akPelarasanPO.AkPelarasanPOObjek)
                {
                    _cart.AddItemObjek(
                            akPelarasanPO.Id,
                            item.JKWPTJBahagianId,
                            item.AkCartaId,
                            item.Amaun);
                }
            }

            if (akPelarasanPO.AkPelarasanPOPerihal != null)
            {
                foreach (var item in akPelarasanPO.AkPelarasanPOPerihal)
                {
                    _cart.AddItemPerihal(
                        akPelarasanPO.Id,
                        item.Bil,
                        item.Perihal,
                        item.Kuantiti,
                        item.LHDNKodKlasifikasiId ?? _unitOfWork.LHDNKodKlasifikasiRepo.GetByCodeAsync("022").Result.Id,
                        item.LHDNUnitUkuranId ?? _unitOfWork.LHDNUnitUkuranRepo.GetByCodeAsync("C62").Result.Id, item.Unit, item.EnLHDNJenisCukai, item.KadarCukai, item.AmaunCukai,
                        item.Harga,
                        item.Amaun
                        );
                }
            }

            PopulateListViewFromCart();
        }

        private void PopulateListViewFromCart()
        {
            List<AkPelarasanPOObjek> objek = _cart.AkPelarasanPOObjek.ToList();

            foreach (AkPelarasanPOObjek item in objek)
            {
                var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                item.JKWPTJBahagian = jkwPtjBahagian;

                item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                item.AkCarta = akCarta;
            }

            ViewBag.akPelarasanPOObjek = objek;

            List<AkPelarasanPOPerihal> perihal = _cart.AkPelarasanPOPerihal.ToList();

            ViewBag.akPelarasanPOPerihal = perihal;
        }

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

        [Authorize(Policy = modul + "L")]
        public async Task<IActionResult> Lulus(int id, int dKonfigKelulusanId, string syscode)
        {
            var akPelarasanPO = _unitOfWork.AkPelarasanPORepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akPelarasanPO != null)
            {

                _unitOfWork.AkPelarasanPORepo.Lulus(id, dKonfigKelulusanId, user?.UserName ?? "");

                _appLog.Insert("Posting", "Melulus " + akPelarasanPO.NoRujukan ?? "" + "; pelulusId: " + dKonfigKelulusanId.ToString(), akPelarasanPO.NoRujukan ?? "", id, akPelarasanPO.Jumlah, pekerjaId, modul, syscode, namamodul, user);
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
            var akPelarasanPO = _unitOfWork.AkPelarasanPORepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akPelarasanPO != null)
            {
                _unitOfWork.AkPelarasanPORepo.HantarSemula(id, tindakan, user?.UserName ?? "");

                _appLog.Insert("Ubah", "Hantar Semula " + akPelarasanPO.NoRujukan ?? "", akPelarasanPO.NoRujukan ?? "", id, akPelarasanPO.Jumlah, pekerjaId, modul, syscode, namamodul, user);
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
