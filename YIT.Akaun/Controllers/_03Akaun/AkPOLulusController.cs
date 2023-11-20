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
    [Authorize]
    public class AkPOLulusController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodLulusAkPO;
        public const string namamodul = Modules.namaLulusAkPO;
        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;
        private readonly CartAkPO _cart;

        public AkPOLulusController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices,
            CartAkPO cart
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

            List<AkPO> akPO = new List<AkPO>();
            if (!string.IsNullOrEmpty(searchDate1) && !string.IsNullOrEmpty(searchDate2))
            {
                date1 = DateTime.Parse(searchDate1);
                date2 = DateTime.Parse(searchDate2);
            }

            if (dKonfigKelulusanId != null)
            {
                // cek is user and password valid or not
                HttpContext.Session.SetInt32("DPelulusId", (int)dKonfigKelulusanId);

                if (_unitOfWork.DKonfigKelulusanRepo.IsValidUser((int)dKonfigKelulusanId, password, EnJenisModul.Perolehan, EnKategoriKelulusan.Pelulus) == false)
                {
                    TempData[SD.Error] = "Katalaluan Tidak Sah";
                    return View();
                }
                else
                {

                    akPO = _unitOfWork.AkPORepo.GetResultsByDPekerjaIdFromDKonfigKelulusan(searchString, date1, date2, searchColumn, EnStatusBorang.None, (int)dKonfigKelulusanId, EnKategoriKelulusan.Pelulus, EnJenisModul.Tanggungan);


                }
            }

            return View(akPO);
        }

        private void PopulateFormFields(string searchString, string password, string searchDate1, string searchDate2)
        {
            ViewBag.searchString = searchString;
            ViewBag.password = password;
            ViewBag.searchDate1 = searchDate1 ?? DateTime.Now.ToString("dd/MM/yyyy");
            ViewBag.searchDate2 = searchDate2 ?? DateTime.Now.ToString("dd/MM/yyyy");
            ViewBag.DKonfigKelulusan = _unitOfWork.DKonfigKelulusanRepo.GetResultsByCategoryGroupByDPekerja(EnKategoriKelulusan.Pelulus);
        }

        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akPO = _unitOfWork.AkPORepo.GetDetailsById((int)id);
            if (akPO == null)
            {
                return NotFound();
            }
            EmptyCart();
            ViewBag.DKonfigKelulusanId = HttpContext.Session.GetInt32("DPelulusId");
            PopulateCartAkPOFromDb(akPO);
            return View(akPO);
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

        private void PopulateCartAkPOFromDb(AkPO akPO)
        {
            if (akPO.AkPOObjek != null)
            {
                foreach (var item in akPO.AkPOObjek)
                {
                    _cart.AddItemObjek(
                            akPO.Id,
                            item.JBahagianId,
                            item.AkCartaId,
                            item.Amaun);
                }
            }

            if (akPO.AkPOPerihal != null)
            {
                foreach (var item in akPO.AkPOPerihal)
                {
                    _cart.AddItemPerihal(
                        akPO.Id,
                        item.Bil,
                        item.Perihal,
                        item.Kuantiti,
                        item.Unit,
                        item.Harga,
                        item.Amaun
                        );
                }
            }

            PopulateListViewFromCart();
        }

        private void PopulateListViewFromCart()
        {
            List<AkPOObjek> objek = _cart.AkPOObjek.ToList();

            foreach (AkPOObjek item in objek)
            {
                var jBahagian = _unitOfWork.JBahagianRepo.GetAllDetailsById(item.JBahagianId);

                item.JBahagian = jBahagian;

                item.JBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jBahagian.JPTJ?.JKW?.Kod, jBahagian.JPTJ?.Kod, jBahagian.Kod);

                var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                item.AkCarta = akCarta;
            }

            ViewBag.akPOObjek = objek;

            List<AkPOPerihal> perihal = _cart.AkPOPerihal.ToList();

            ViewBag.akPOPerihal = perihal;
        }

        public async Task<IActionResult> Lulus(int id, int dKonfigKelulusanId, string syscode)
        {
            var akPO = _unitOfWork.AkPORepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akPO != null)
            {

                _unitOfWork.AkPORepo.Lulus(id, dKonfigKelulusanId, user?.UserName ?? "");

                _appLog.Insert("Posting", "Melulus " + akPO.NoRujukan ?? "" + "; pelulusId: " + dKonfigKelulusanId.ToString(), akPO.NoRujukan ?? "", id, akPO.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                _context.SaveChanges();
                TempData[SD.Success] = "Data berjaya diluluskan..!";
            }
            else
            {
                TempData[SD.Error] = "Data tidak wujud.";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> HantarSemulaAsync(int id, string? tindakan, string syscode)
        {
            var akPO = _unitOfWork.AkPORepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akPO != null)
            {
                _unitOfWork.AkPORepo.BatalLulus(id, tindakan, user?.UserName ?? "");

                _appLog.Insert("Ubah", "Hantar Semula " + akPO.NoRujukan ?? "", akPO.NoRujukan ?? "", id, akPO.Jumlah, pekerjaId, modul, syscode, namamodul, user);
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
