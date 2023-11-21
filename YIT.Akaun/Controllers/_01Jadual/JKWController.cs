using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Implementations;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services.Cart;
using YIT._DataAccess.Services.Math;

namespace YIT.Akaun.Controllers._01Jadual
{
    [Authorize(Roles = "SuperAdmin,Supervisor")]
    public class JKWController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodJKW;
        public const string namamodul = Modules.namaJKW;

        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly CartJKW _cart;

        public JKWController(ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            CartJKW cart)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _appLog = appLog;
            _cart = cart;
        }
        public IActionResult Index()
        {
            return View(_unitOfWork.JKWRepo.GetAll());
        }

        // GET: KW/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kW = _unitOfWork.JKWRepo.GetDetailsById((int)id);
            if (kW == null)
            {
                return NotFound();
            }
            EmptyCart();
            PopulateCartJKWFromDb(kW);
            return View(kW);
        }

        private void PopulateCartJKWFromDb(JKW kW)
        {
            if (kW.JKWPTJBahagian != null)
            {
                foreach (var item in kW.JKWPTJBahagian)
                {
                    _cart.AddItemList(item.JKWId, item.JPTJId, item.JBahagianId, item.Kod);
                }
            }

            PopulateListViewFromCart();

        }

        private void PopulateListViewFromCart()
        {
            List<JKWPTJBahagian> jKWPTJBahagian = _cart.JKWPTJBahagian.ToList();

            foreach (JKWPTJBahagian item in jKWPTJBahagian)
            {
                var jBahagian = _unitOfWork.JBahagianRepo.GetAllDetailsById(item.JBahagianId);

                item.JBahagian = jBahagian;

                var jPTJ = _unitOfWork.JPTJRepo.GetAllDetailsById(item.JPTJId);

                item.JPTJ = jPTJ;

                var jKW = _unitOfWork.JKWRepo.GetById(item.JKWId);

                item.JKW = jKW;

            }

            ViewBag.JKWPTJBahagian = jKWPTJBahagian;
        }

        // GET: KW/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: KW/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JKW kW, string syscode)
        {
            if (kW.Kod != null && KodKWExists(kW.Kod) == false)
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    kW.UserId = user?.UserName ?? "";

                    kW.TarMasuk = DateTime.Now;
                    kW.DPekerjaMasukId = pekerjaId;

                    _context.Add(kW);
                    _appLog.Insert("Tambah", kW.Kod + " - " + kW.Perihal, kW.Kod, 0, 0, pekerjaId, modul, syscode, namamodul, user);
                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya ditambah..!";
                    return RedirectToAction(nameof(Index));

                }
            }
            else
            {
                TempData[SD.Error] = "Kod ini telah wujud..!";
            }

            return View(kW);
        }

        [Authorize(Roles = "SuperAdmin")]
        // GET: KW/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kW = _unitOfWork.JKWRepo.GetDetailsById((int)id);
            if (kW == null)
            {
                return NotFound();
            }
            EmptyCart();
            PopulateDropDownList(kW);
            PopulateCartJKWFromDb(kW);
            return View(kW);
        }

        public JsonResult EmptyCart()
        {
            try
            {
                _cart.ClearList();

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        private void PopulateDropDownList(JKW jKW)
        {
            var kwList = new List<JKW>{jKW};

            ViewBag.JKW = kwList;
            ViewBag.JPTJ = _unitOfWork.JPTJRepo.GetAll();
            ViewBag.JBahagian = _unitOfWork.JBahagianRepo.GetAll();
        }

        // POST: KW/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, JKW kW, string syscode)
        {
            if (id != kW.Id)
            {
                return NotFound();
            }

            if (kW.Kod != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = await _context.JKW.FirstOrDefaultAsync(x => x.Id == kW.Id);
                    var kodAsal = objAsal!.Kod;
                    var perihalAsal = objAsal.Perihal;
                    kW.UserId = objAsal.UserId;
                    kW.TarMasuk = objAsal.TarMasuk;
                    kW.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                    _context.Entry(objAsal).State = EntityState.Detached;

                    kW.UserIdKemaskini = user?.UserName ?? "";

                    kW.TarKemaskini = DateTime.Now;
                    kW.DPekerjaKemaskiniId = pekerjaId;

                    _unitOfWork.JKWRepo.Update(kW);

                    _appLog.Insert("Ubah", kodAsal + " -> " + kW.Kod + ", " + perihalAsal + " -> " + kW.Perihal + ", ", kW.Kod, id, 0, pekerjaId, modul, syscode, namamodul, user);

                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KWExists(kW.Id))
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
            return View(kW);
        }

        // GET: KW/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kW = _unitOfWork.JKWRepo.GetById((int)id);
            if (kW == null)
            {
                return NotFound();
            }

            return View(kW);
        }

        // POST: KW/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string syscode)
        {
            var kW = _unitOfWork.JKWRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;
            
            if (kW != null && kW.Kod != null)
            {
                kW.UserIdKemaskini = user?.UserName ?? "";
                kW.TarKemaskini = DateTime.Now;
                kW.DPekerjaKemaskiniId = pekerjaId;

                _context.JKW.Remove(kW);
                _appLog.Insert("Hapus", kW.Kod + " - " + kW.Perihal, kW.Kod, id, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dihapuskan..!";
            }
            
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> RollBack(int id, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            var obj = await _context.JKW.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.JKW.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.Kod + " - " + obj.Perihal, obj.Kod ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }
            
            return RedirectToAction(nameof(Index));
        }
        private bool KWExists(int id)
        {
            return _unitOfWork.JKWRepo.IsExist(b => b.Id == id);
        }

        private bool KodKWExists(string kod)
        {
            return _unitOfWork.JKWRepo.IsExist(e => e.Kod == kod);
        }

        public JsonResult GetJKWPTJBahagian(int JBahagianId, int JPTJId, int JKWId)
        {
            try
            {
                var jBahagian = _unitOfWork.JBahagianRepo.GetById(JBahagianId);
                if (jBahagian == null)
                {
                    return Json(new { result = "Error", message = "Kod Bahagian tidak wujud" });
                }

                var jPTJ = _unitOfWork.JPTJRepo.GetById(JPTJId);
                if (jPTJ == null)
                {
                    return Json(new { result = "Error", message = "Kod PTJ tidak wujud" });
                }

                var jKW = _unitOfWork.JKWRepo.GetById(JKWId);
                if (jKW == null)
                {
                    return Json(new { result = "Error", message = "Kod Kump. Wang tidak wujud" });
                }

                return Json(new { result = "OK", jBahagian, jPTJ, jKW });
            }
            catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }
        }

        public JsonResult GetJKWPTJBahagianAkCarta(int JKWPTJBahagianId, int AkCartaId)
        {
            try
            {
                var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(JKWPTJBahagianId);
                if (jkwPtjBahagian == null)
                {
                    return Json(new { result = "Error", message = "Kod akaun tidak wujud" });
                }

                var akCarta = _unitOfWork.AkCartaRepo.GetById(AkCartaId);
                if (akCarta == null)
                {
                    return Json(new { result = "Error", message = "Kod akaun tidak wujud" });
                }

                return Json(new { result = "OK", jkwPtjBahagian, akCarta });
            }
            catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }
        }

        public JsonResult SaveCartJKW(JKWPTJBahagian jKWPTJBahagian)
        {
            try
            {
                if (jKWPTJBahagian != null)
                {
                    _cart.AddItemList(jKWPTJBahagian.JKWId, jKWPTJBahagian.JPTJId, jKWPTJBahagian.JBahagianId, jKWPTJBahagian.Kod);
                }


                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartJKW(JKWPTJBahagian jKWPTJBahagian)
        {
            try
            {
                if (jKWPTJBahagian != null)
                {
                    _cart.RemoveItemList(jKWPTJBahagian.JKWId,jKWPTJBahagian.JPTJId, jKWPTJBahagian.JBahagianId);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartJKW(JKWPTJBahagian jKWPTJBahagian)
        {

            try
            {
                JKWPTJBahagian data = _cart.JKWPTJBahagian.FirstOrDefault(x => x.JBahagianId == jKWPTJBahagian.JBahagianId && x.JPTJId == jKWPTJBahagian.JPTJId && jKWPTJBahagian.JKWId == jKWPTJBahagian.JKWId) ?? new JKWPTJBahagian();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartJKW(JKWPTJBahagian jKWPTJBahagian)
        {

            try
            {

                var data = _cart.JKWPTJBahagian.FirstOrDefault(x => x.JBahagianId == jKWPTJBahagian.JBahagianId && x.JPTJId == jKWPTJBahagian.JPTJId && x.JKWId == jKWPTJBahagian.JKWId);

                var user = _userManager.GetUserName(User);

                if (data != null)
                {
                    _cart.RemoveItemList(data.JKWId,data.JBahagianId,data.JBahagianId);

                    _cart.AddItemList(jKWPTJBahagian.JKWId, jKWPTJBahagian.JPTJId, jKWPTJBahagian.JBahagianId, jKWPTJBahagian.Kod);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAllItemCartJKW()
        {

            try
            {
                List<JKWPTJBahagian> jKWPTJBahagian = _cart.JKWPTJBahagian.ToList();

                foreach (JKWPTJBahagian item in jKWPTJBahagian)
                {
                    var jKW = _unitOfWork.JKWRepo.GetById(item.JKWId);

                    item.JKW = jKW;

                    var jPTJ = _unitOfWork.JPTJRepo.GetById(item.JPTJId);

                    item.JPTJ = jPTJ;

                    var jBahagian = _unitOfWork.JBahagianRepo.GetById(item.JBahagianId);

                    item.JBahagian = jBahagian;

                }

                return Json(new { result = "OK", jKWPTJBahagian = jKWPTJBahagian.OrderBy(d => d.Kod) });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetJKWPTJBahagianList(int JKWId)
        {
            try
            {
                var jKWPTJBahagianList = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsByJKWId(JKWId);
                if (jKWPTJBahagianList != null && jKWPTJBahagianList.Count > 0)
                {
                    return Json(new { result = "OK", list = jKWPTJBahagianList });
                }
                else
                {
                    return Json(new { result = "Error", message = "Kump. Wang / PTJ / Bahagian belum dihubungkan" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }
        }
    }
}
