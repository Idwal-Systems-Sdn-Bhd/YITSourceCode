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
    public class AbWaranLulusController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodLulusAbWaran;
        public const string namamodul = Modules.namaLulusAbWaran;
        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;
        private readonly CartAbWaran _cart;

        public AbWaranLulusController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices,
            CartAbWaran cart)
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

            List<AbWaran> abWaran = new List<AbWaran>();
            if (!string.IsNullOrEmpty(searchDate1) && !string.IsNullOrEmpty(searchDate2))
            {
                date1 = DateTime.Parse(searchDate1);
                date2 = DateTime.Parse(searchDate2);
            }

            if (dKonfigKelulusanId != null)
            {
                // cek is user and password valid or not
                HttpContext.Session.SetInt32("DPelulusId", (int)dKonfigKelulusanId);

                if (_unitOfWork.DKonfigKelulusanRepo.IsValidUser((int)dKonfigKelulusanId, password, EnJenisModulKelulusan.Waran, EnKategoriKelulusan.Pelulus) == false)
                {
                    TempData[SD.Error] = "Katalaluan Tidak Sah";
                    return View();
                }
                else
                {

                    abWaran = _unitOfWork.AbWaranRepo.GetResultsByDPekerjaIdFromDKonfigKelulusan(searchString, date1, date2, searchColumn, EnStatusBorang.Semak, (int)dKonfigKelulusanId, EnKategoriKelulusan.Pelulus, EnJenisModulKelulusan.Waran);

                }
            }

            return View(abWaran);

        }

        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var abWaran = _unitOfWork.AbWaranRepo.GetDetailsById((int)id);
            if (abWaran == null)
            {
                return NotFound();
            }
            EmptyCart();
            ViewBag.DKonfigKelulusanId = HttpContext.Session.GetInt32("DPelulusId");
            PopulateCartAbWaranFromDb(abWaran);
            return View(abWaran);
        }

        // jsonResults
        public JsonResult EmptyCart()
        {
            try
            {
                _cart.ClearObjek();

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public async Task<IActionResult> Lulus(int id, int dKonfigKelulusanId, string syscode)
        {
            var abWaran = _unitOfWork.AbWaranRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (abWaran != null)
            {

                _unitOfWork.AbWaranRepo.Lulus(id, dKonfigKelulusanId, user?.UserName ?? "");

                _appLog.Insert("Posting", "Melulus " + abWaran.NoRujukan ?? "" + "; pelulusId: " + dKonfigKelulusanId.ToString(), abWaran.NoRujukan ?? "", id, abWaran.Jumlah, pekerjaId, modul, syscode, namamodul, user);
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
            var abWaran = _unitOfWork.AbWaranRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (abWaran != null)
            {
                _unitOfWork.AbWaranRepo.HantarSemula(id, tindakan, user?.UserName ?? "");

                _appLog.Insert("Ubah", "Hantar Semula " + abWaran.NoRujukan ?? "", abWaran.NoRujukan ?? "", id, abWaran.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                _context.SaveChanges();
                TempData[SD.Success] = "Data berjaya dihantar semula..!";
            }
            else
            {
                TempData[SD.Error] = "Data tidak wujud.";
            }

            return RedirectToAction(nameof(Index));
        }
        private void PopulateCartAbWaranFromDb(AbWaran abWaran)
        {
            if (abWaran.AbWaranObjek != null)
            {
                foreach (var item in abWaran.AbWaranObjek)
                {
                    _cart.AddItemObjek(
                           abWaran.Id,
                           item.JKWPTJBahagianId,
                           item.AkCartaId,
                           item.Amaun,
                           item.TK);
                }
            }
            PopulateListViewFromCart();
        }

        private void PopulateListViewFromCart()
        {
            List<AbWaranObjek> objek = _cart.AbWaranObjek.ToList();

            foreach (AbWaranObjek item in objek)
            {
                var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                item.JKWPTJBahagian = jkwPtjBahagian;

                item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                item.AkCarta = akCarta;
            }

            ViewBag.abWaranObjek = objek;
        }

        private void PopulateFormFields(string searchString, string password, string searchDate1, string searchDate2)
        {
            ViewBag.searchString = searchString;
            ViewBag.password = password;
            ViewBag.searchDate1 = searchDate1 ?? DateTime.Now.ToString("dd/MM/yyyy");
            ViewBag.searchDate2 = searchDate2 ?? DateTime.Now.ToString("dd/MM/yyyy");
            ViewBag.DKonfigKelulusan = _unitOfWork.DKonfigKelulusanRepo.GetResultsByCategoryGroupByDPekerja(EnKategoriKelulusan.Pelulus, EnJenisModulKelulusan.Waran);
        }

    }
}
