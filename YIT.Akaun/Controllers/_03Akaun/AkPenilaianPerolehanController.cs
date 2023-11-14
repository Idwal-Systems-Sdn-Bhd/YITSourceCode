using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services;
using YIT._DataAccess.Services.Cart;
using YIT._DataAccess.Services.Math;
using YIT.Akaun.Infrastructure;
using YIT.Akaun.Microservices;

namespace YIT.Akaun.Controllers._03Akaun
{
    [Authorize]
    public class AkPenilaianPerolehanController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodAkPenilaianPerolehan;
        public const string namamodul = Modules.namaAkPenilaianPerolehan;
        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;
        private readonly CartAkPenilaianPerolehan _cart;

        public AkPenilaianPerolehanController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices,
            CartAkPenilaianPerolehan cart
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
            string searchColumn
            )
        {
            DateTime? date1 = null;
            DateTime? date2 = null;

            if (!string.IsNullOrEmpty(searchDate1) && ! string.IsNullOrEmpty(searchDate2))
            {
                date1 = DateTime.Parse(searchDate1);
                date2 = DateTime.Parse(searchDate2);
            }

            PopulateFormFields(searchString, searchDate1, searchDate2);
            
            var akPP = _unitOfWork.AkPenilaianPerolehanRepo.GetResults(searchString,date1,date2,searchColumn);

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

            PopulateCartAkPenilaianPerolehanFromDb(akPP);
            return View(akPP);
        }

        public IActionResult Delete(int? id)
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

            PopulateCartAkPenilaianPerolehanFromDb(akPP);
            return View(akPP);
        }

        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            string? fullName = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.Nama;

            EmptyCart();
            PopulateDropDownList(fullName ?? "");
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.PN.GetDisplayName(),DateTime.Now.ToString("yyyy"));
            return View();
        }

        private string GenerateRunningNumber(string initNoRujukan, string tahun)
        {
            var maxRefNo = _unitOfWork.AkPenilaianPerolehanRepo.GetMaxRefNo(initNoRujukan,tahun);

            var prefix = initNoRujukan + "/" + tahun + "/";
            return RunningNumberFormatter.GenerateRunningNumber(prefix,maxRefNo,"00000");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AkPenilaianPerolehan akPP, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            string? fullName = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.Nama;

            if (ModelState.IsValid)
            {
                
                akPP.UserId = user?.UserName ?? "";
                akPP.TarMasuk = DateTime.Now;
                akPP.DPekerjaMasukId = pekerjaId;

                akPP.AkPenilaianPerolehanObjek = _cart.AkPenilaianPerolehanObjek.ToList();
                akPP.AkPenilaianPerolehanPerihal = _cart.AkPenilaianPerolehanPerihal.ToList();

                _context.Add(akPP);
                _appLog.Insert("Tambah", akPP.NoRujukan ?? "", akPP.NoRujukan ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya ditambah..!";
                return RedirectToAction(nameof(Index));
            }

            PopulateDropDownList(fullName ?? "");
            PopulateListViewFromCart();
            return View(akPP);
        }

        private void PopulateDropDownList(string fullName)
        {
            
            ViewBag.FullName = fullName;
            ViewBag.JKW = _unitOfWork.JKWRepo.GetAll();
            ViewBag.DDaftarAwam = _unitOfWork.DDaftarAwamRepo.GetAllDetailsByKategori(EnKategoriDaftarAwam.Pembekal);
            ViewBag.AkCarta = _unitOfWork.AkCartaRepo.GetResultsByParas(EnParas.Paras4);
            ViewBag.JBahagian = _unitOfWork.JBahagianRepo.GetAll();
            ViewBag.EnKaedahPerolehan = EnumHelper<EnKaedahPerolehan>.GetList();

        }

        private void PopulateCartAkPenilaianPerolehanFromDb(AkPenilaianPerolehan akPP)
        {
            if (akPP.AkPenilaianPerolehanObjek != null)
            {
                foreach (var item in akPP.AkPenilaianPerolehanObjek)
                {
                    _cart.AddItemObjek(
                            akPP.Id,
                            item.JBahagianId,
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
                var jBahagian = _unitOfWork.JBahagianRepo.GetAllDetailsById(item.JBahagianId);

                item.JBahagian = jBahagian;

                var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                item.AkCarta = akCarta;
            }

            ViewBag.akPenilaianPerolehanObjek = objek;

            List<AkPenilaianPerolehanPerihal> perihal = _cart.AkPenilaianPerolehanPerihal.ToList();

            ViewBag.akPenilaianPerolehanPerihal = perihal;
        }

        private void PopulateFormFields(string searchString, string searchDate1, string searchDate2)
        {
            ViewBag.searchString = searchString;
            ViewBag.searchDate1 = searchDate1 ?? DateTime.Now.ToString("dd/MM/yyyy");
            ViewBag.searchDate2 = searchDate2 ?? DateTime.Now.ToString("dd/MM/yyyy");
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

        public JsonResult GetJBahagianAkCarta(int JBahagianId, int AkCartaId)
        {
            try
            {
                var jBahagian = _unitOfWork.JBahagianRepo.GetById(JBahagianId);
                if (jBahagian == null)
                {
                    return Json(new { result = "Error", message = "Kod akaun tidak wujud" });
                }

                var akCarta = _unitOfWork.AkCartaRepo.GetById(AkCartaId);
                if (akCarta == null)
                {
                    return Json(new { result = "Error", message = "Kod akaun tidak wujud" });
                }

                return Json(new { result = "OK", jBahagian, akCarta });
            }
            catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }
        }

        public JsonResult SaveCartAkPenilaianPerolehanObjek(AkPenilaianPerolehanObjek akPenilaianPerolehanObjek)
        {
            try
            {
                if (akPenilaianPerolehanObjek != null)
                {
                    _cart.AddItemObjek(akPenilaianPerolehanObjek.AkPenilaianPerolehanId, akPenilaianPerolehanObjek.JBahagianId, akPenilaianPerolehanObjek.AkCartaId, akPenilaianPerolehanObjek.Amaun);
                }



                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkPenilaianPerolehanObjek(AkPenilaianPerolehanObjek akPenilaianPerolehanObjek)
        {
            try
            {
                if (akPenilaianPerolehanObjek != null)
                {
                    _cart.RemoveItemObjek(akPenilaianPerolehanObjek.JBahagianId, akPenilaianPerolehanObjek.AkCartaId);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkPenilaianPerolehanObjek(AkPenilaianPerolehanObjek akPenilaianPerolehanObjek)
        {

            try
            {
                AkPenilaianPerolehanObjek data = _cart.AkPenilaianPerolehanObjek.FirstOrDefault(x => x.JBahagianId == akPenilaianPerolehanObjek.JBahagianId && x.AkCartaId == akPenilaianPerolehanObjek.AkCartaId) ?? new AkPenilaianPerolehanObjek();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkPenilaianPerolehanObjek(AkPenilaianPerolehanObjek akPenilaianPerolehanObjek)
        {

            try
            {

                var akTO = _cart.AkPenilaianPerolehanObjek.FirstOrDefault(x => x.JBahagianId == akPenilaianPerolehanObjek.JBahagianId && x.AkCartaId == akPenilaianPerolehanObjek.AkCartaId);

                var user = _userManager.GetUserName(User);

                if (akTO != null)
                {
                    _cart.RemoveItemObjek(akPenilaianPerolehanObjek.JBahagianId, akPenilaianPerolehanObjek.AkCartaId);

                    _cart.AddItemObjek(akPenilaianPerolehanObjek.AkPenilaianPerolehanId,
                                    akPenilaianPerolehanObjek.JBahagianId,
                                    akPenilaianPerolehanObjek.AkCartaId,
                                    akPenilaianPerolehanObjek.Amaun);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetBil(int Bil)
        {
            try
            {
                var akPP = _cart.AkPenilaianPerolehanPerihal.FirstOrDefault(pp => pp.Bil == Bil);
                if (akPP != null)
                {
                    return Json(new { result = "Error", message = "Bil telah wujud" });
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }
        }

        public JsonResult SaveCartAkPenilaianPerolehanPerihal(AkPenilaianPerolehanPerihal akPenilaianPerolehanPerihal)
        {
            try
            {
                if (akPenilaianPerolehanPerihal != null)
                {
                    _cart.AddItemPerihal(akPenilaianPerolehanPerihal.AkPenilaianPerolehanId,akPenilaianPerolehanPerihal.Bil, akPenilaianPerolehanPerihal.Perihal, akPenilaianPerolehanPerihal.Kuantiti,akPenilaianPerolehanPerihal.Unit,akPenilaianPerolehanPerihal.Harga,akPenilaianPerolehanPerihal.Amaun);
                }



                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkPenilaianPerolehanPerihal(AkPenilaianPerolehanPerihal akPenilaianPerolehanPerihal)
        {
            try
            {
                if (akPenilaianPerolehanPerihal != null)
                {
                    _cart.RemoveItemPerihal(akPenilaianPerolehanPerihal.Bil);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkPenilaianPerolehanPerihal(AkPenilaianPerolehanPerihal akPenilaianPerolehanPerihal)
        {

            try
            {
                AkPenilaianPerolehanPerihal data = _cart.AkPenilaianPerolehanPerihal.FirstOrDefault(x => x.Bil == akPenilaianPerolehanPerihal.Bil) ?? new AkPenilaianPerolehanPerihal();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkPenilaianPerolehanPerihal(AkPenilaianPerolehanPerihal akPenilaianPerolehanPerihal)
        {

            try
            {

                var akPP = _cart.AkPenilaianPerolehanPerihal.FirstOrDefault(x => x.Bil == akPenilaianPerolehanPerihal.Bil);

                var user = _userManager.GetUserName(User);

                if (akPP != null)
                {
                    _cart.RemoveItemPerihal(akPenilaianPerolehanPerihal.Bil);

                    _cart.AddItemPerihal(akPenilaianPerolehanPerihal.AkPenilaianPerolehanId, akPenilaianPerolehanPerihal.Bil, akPenilaianPerolehanPerihal.Perihal, akPenilaianPerolehanPerihal.Kuantiti, akPenilaianPerolehanPerihal.Unit, akPenilaianPerolehanPerihal.Harga, akPenilaianPerolehanPerihal.Amaun);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAllItemCartAkPenilaianPerolehan()
        {

            try
            {
                List<AkPenilaianPerolehanObjek> objek = _cart.AkPenilaianPerolehanObjek.ToList();

                foreach (AkPenilaianPerolehanObjek item in objek)
                {
                    var jBahagian = _unitOfWork.JBahagianRepo.GetById(item.JBahagianId);

                    item.JBahagian = jBahagian;

                    var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                    item.AkCarta = akCarta;
                }

                List<AkPenilaianPerolehanPerihal> perihal = _cart.AkPenilaianPerolehanPerihal.ToList();

                return Json(new { result = "OK", objek = objek.OrderBy(d => d.AkCarta?.Kod), perihal = perihal.OrderBy(d => d.Bil) });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }
    }
}
