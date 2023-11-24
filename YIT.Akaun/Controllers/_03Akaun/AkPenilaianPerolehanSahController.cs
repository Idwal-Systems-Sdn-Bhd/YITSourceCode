using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services.Cart;
using YIT._DataAccess.Services.Math;
using YIT.Akaun.Infrastructure;

namespace YIT.Akaun.Controllers._03Akaun
{
    [Authorize]
    public class AkPenilaianPerolehanSahController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodSahAkPenilaianPerolehan;
        public const string namamodul = Modules.namaSahAkPenilaianPerolehan;
        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;
        private readonly CartAkPenilaianPerolehan _cart;

        public AkPenilaianPerolehanSahController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices,
            CartAkPenilaianPerolehan cart)
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

            List<AkPenilaianPerolehan> akPP = new List<AkPenilaianPerolehan>();
            if (!string.IsNullOrEmpty(searchDate1) && !string.IsNullOrEmpty(searchDate2))
            {
                date1 = DateTime.Parse(searchDate1);
                date2 = DateTime.Parse(searchDate2);
            }

            if (dKonfigKelulusanId != null)
            {
                // cek is user and password valid or not
                HttpContext.Session.SetInt32("DPengesahId", (int)dKonfigKelulusanId);

                if (_unitOfWork.DKonfigKelulusanRepo.IsValidUser((int)dKonfigKelulusanId,password,EnJenisModulKelulusan.Penilaian,EnKategoriKelulusan.Pengesah) == false)
                {
                    TempData[SD.Error] = "Katalaluan Tidak Sah";
                    return View();
                }
                else
                {
                    
                    akPP = _unitOfWork.AkPenilaianPerolehanRepo.GetResultsByDPekerjaIdFromDKonfigKelulusan(searchString, date1, date2, searchColumn, EnStatusBorang.None,(int)dKonfigKelulusanId,EnKategoriKelulusan.Pengesah, EnJenisModulKelulusan.Penilaian);

                }
            }

            return View(akPP);
            
        }

        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akPP = _unitOfWork.AkPenilaianPerolehanRepo.GetDetailsById((int)id);
            if (akPP == null)
            {
                return NotFound();
            }
            EmptyCart();
            ViewBag.DKonfigKelulusanId = HttpContext.Session.GetInt32("DPengesahId");
            PopulateCartAkPenilaianPerolehanFromDb(akPP);
            return View(akPP);
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

        public async Task<IActionResult> Sah(int id,int dKonfigKelulusanId, string syscode)
        {
            var akPP = _unitOfWork.AkPenilaianPerolehanRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akPP != null)
            {

                _unitOfWork.AkPenilaianPerolehanRepo.Sah(id, dKonfigKelulusanId, user?.UserName ?? "");
                
                _appLog.Insert("Posting", "Mengesah " + akPP.NoRujukan ?? "" + "; pengesahId: " + dKonfigKelulusanId.ToString(), akPP.NoRujukan ?? "", id, akPP.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                _context.SaveChanges();
                TempData[SD.Success] = "Data berjaya disahkan..!";
            }
            else
            {
                TempData[SD.Error] = "Data tidak wujud.";
            }

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> HantarSemula(int id, string? tindakan, string syscode)
        {
            var akPP = _unitOfWork.AkPenilaianPerolehanRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akPP != null)
            {
                _unitOfWork.AkPenilaianPerolehanRepo.BatalSah(id, tindakan, user?.UserName ?? "");

                _appLog.Insert("Ubah", "Hantar Semula " + akPP.NoRujukan ?? "", akPP.NoRujukan ?? "", id, akPP.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                _context.SaveChanges();
                TempData[SD.Success] = "Data berjaya dihantar semula..!";
            }
            else
            {
                TempData[SD.Error] = "Data tidak wujud.";
            }

            return RedirectToAction(nameof(Index));
        }
        private void PopulateCartAkPenilaianPerolehanFromDb(AkPenilaianPerolehan akPP)
        {
            if (akPP.AkPenilaianPerolehanObjek != null)
            {
                foreach (var item in akPP.AkPenilaianPerolehanObjek)
                {
                    _cart.AddItemObjek(
                            akPP.Id,
                            item.JKWPTJBahagianId,
                            item.AkCartaId,
                            item.Amaun);
                }
            }

            if (akPP.AkPenilaianPerolehanPerihal != null)
            {
                foreach (var item in akPP.AkPenilaianPerolehanPerihal)
                {
                    _cart.AddItemPerihal(
                        akPP.Id,
                        item.Bil,
                        item.Perihal,
                        item.Kuantiti,
                        item.Unit,
                        item.Harga,
                        item.Amaun
                        );
                }

                PopulateListViewFromCart();
            }
        }

        private void PopulateListViewFromCart()
        {
            List<AkPenilaianPerolehanObjek> objek = _cart.AkPenilaianPerolehanObjek.ToList();

            foreach (AkPenilaianPerolehanObjek item in objek)
            {
                var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                item.JKWPTJBahagian = jkwPtjBahagian;

                item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                item.AkCarta = akCarta;
            }

            ViewBag.akPenilaianPerolehanObjek = objek;

            List<AkPenilaianPerolehanPerihal> perihal = _cart.AkPenilaianPerolehanPerihal.ToList();

            ViewBag.akPenilaianPerolehanPerihal = perihal;
        }

        void PopulateFormFields(string searchString, string password, string searchDate1, string searchDate2)
        {
            ViewBag.searchString = searchString;
            ViewBag.password = password;
            ViewBag.searchDate1 = searchDate1 ?? DateTime.Now.ToString("dd/MM/yyyy");
            ViewBag.searchDate2 = searchDate2 ?? DateTime.Now.ToString("dd/MM/yyyy");
            ViewBag.DKonfigKelulusan = _unitOfWork.DKonfigKelulusanRepo.GetResultsByCategoryGroupByDPekerja(EnKategoriKelulusan.Pengesah, EnJenisModulKelulusan.Penilaian);
        }
    }
}
