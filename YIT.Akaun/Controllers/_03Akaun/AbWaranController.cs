using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Implementations;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services.Cart;
using YIT.Akaun.Infrastructure;

namespace YIT.Akaun.Controllers._03Akaun
{
    [Authorize(Roles = "SuperAdmin,Supervisor")]
    public class AbWaranController : Microsoft.AspNetCore.Mvc.Controller
    {

        public const string modul = Modules.kodAbWaran;
        public const string namamodul = Modules.namaAbWaran;

        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
 

        public AbWaranController(ApplicationDbContext context,
             _IUnitOfWork unitOfWork,
             UserManager<IdentityUser> userManager,
             _AppLogIRepository<AppLog, int> appLog
)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _appLog = appLog;

        }
        public IActionResult Index()
        {
            return View(_unitOfWork.AbWaranRepo.GetAllDetails());
        }

        // GET: KW/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var a = _unitOfWork.AbWaranRepo.GetAllDetailsById((int)id);
            if (a == null)
            {
                return NotFound();
            }
            
            return View(a);
        }

        // GET: KW/Create
        public IActionResult Create()
        {
            
            PopulateDropdownList();
            return View();
        }

        // POST: KW/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AbWaran abWaran, string syscode)
        {
            if (abWaran.NoRujukan != null && KodAbWaranExists(abWaran.NoRujukan) == false)
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    abWaran.UserId = user?.UserName ?? "";

                    abWaran.TarMasuk = DateTime.Now;
                    abWaran.DPekerjaMasukId = pekerjaId;

                    _context.Add(abWaran);
                    _appLog.Insert("Tambah", abWaran.Id + " - " + abWaran.NoRujukan, abWaran.Id.ToString(), 0, 0, pekerjaId, modul, syscode, namamodul, user);
                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya ditambah..!";
                    return RedirectToAction(nameof(Index));

                }
            }
            else
            {
                TempData[SD.Error] = "Kod ini telah wujud..!";
            }
            PopulateDropdownList();
            
            return View(abWaran);
        }

        [Authorize(Roles = "SuperAdmin")]
        // GET: KW/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var abWaran = _unitOfWork.AbWaranRepo.GetAllDetailsById((int)id);
            if (abWaran == null)
            {
                return NotFound();
            }
            return View(abWaran);
        }

        // POST: KW/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AbWaran abWaran, string syscode)
        {
            if (id != abWaran.Id)
            {
                return NotFound();
            }

            if (abWaran.NoRujukan != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = await _context.AbWaran.FirstOrDefaultAsync(x => x.Id == abWaran.Id);
                    var kodAsal = objAsal!.Id;
                    var perihalAsal = objAsal.NoRujukan;
                    abWaran.UserId = objAsal.UserId;
                    abWaran.TarMasuk = objAsal.TarMasuk;
                    abWaran.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                    _context.Entry(objAsal).State = EntityState.Detached;

                    abWaran.UserIdKemaskini = user?.UserName ?? "";

                    abWaran.TarKemaskini = DateTime.Now;
                    abWaran.DPekerjaKemaskiniId = pekerjaId;

                    _unitOfWork.AbWaranRepo.Update(abWaran);

                    _appLog.Insert("Ubah", kodAsal + " -> " + abWaran.Id + ", " + perihalAsal + " -> " + abWaran.NoRujukan + ", ", abWaran.Id.ToString(), id, 0, pekerjaId, modul, syscode, namamodul, user);

                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AbWaranExists(abWaran.Id))
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
            return View(abWaran);
        }

        // GET: KW/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var abWaran = _unitOfWork.AbWaranRepo.GetAllDetailsById((int)id);
            if (abWaran == null)
            {
                return NotFound();
            }
            
            return View(abWaran);
        }

        // POST: KW/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string syscode)
        {
            var abWaran = _unitOfWork.AbWaranRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (abWaran != null && abWaran.NoRujukan != null)
            {
                abWaran.UserIdKemaskini = user?.UserName ?? "";
                abWaran.TarKemaskini = DateTime.Now;
                abWaran.DPekerjaKemaskiniId = pekerjaId;

                _context.AbWaran.Remove(abWaran);
                _appLog.Insert("Hapus", abWaran.Id + " - " + abWaran.NoRujukan, abWaran.Id.ToString(), id, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dihapuskan..!";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> RollBack(int id, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            var obj = await _context.AbWaran.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.AbWaran.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.Id + " - " + obj.NoRujukan, obj.Id.ToString() ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index));
        }
        private bool AbWaranExists(int id)
        {
            return _unitOfWork.AbWaranRepo.IsExist(b => b.Id == id);
        }

        private bool KodAbWaranExists(string kod)
        {
            return _unitOfWork.AbWaranRepo.IsExist(e => e.NoRujukan == kod);
        }
        public void PopulateDropdownList()
        {
            ViewBag.AkCarta = _unitOfWork.AkCartaRepo.GetResultsByParas(EnParas.Paras4);
            ViewBag.JBahagian = _unitOfWork.JBahagianRepo.GetAllDetails();
        }

        // jsonResults
        //public JsonResult EmptyCart()
        //{
        //    try
        //    {

        //        _cart.ClearAbWaranObjek();
               

        //        return Json(new { result = "OK" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { result = "ERROR", message = ex.Message });
        //    }
        //}

        //public JsonResult GetJBahagianAkCarta(int JBahagianId, int AkCartaId)
        //{
        //    try
        //    {
        //        var jBahagian = _unitOfWork.JBahagianRepo.GetById(JBahagianId);
        //        if (jBahagian == null)
        //        {
        //            return Json(new { result = "Error", message = "Kod akaun tidak wujud" });
        //        }

        //        var akCarta = _unitOfWork.AkCartaRepo.GetById(AkCartaId);
        //        if (akCarta == null)
        //        {
        //            return Json(new { result = "Error", message = "Kod akaun tidak wujud" });
        //        }

        //        return Json(new { result = "OK", jBahagian, akCarta });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { result = "Error", message = ex.Message });
        //    }
        //}

        //public JsonResult SaveCartAbWaranObjek(AbWaranObjek abWaranObjek)
        //{
        //    try
        //    {
        //        if (abWaranObjek != null)
        //        {
        //            _cart.AddItemAbWaranObjek(abWaranObjek.AbWaranId, abWaranObjek.JBahagianId, abWaranObjek.AkCartaId, abWaranObjek.Amaun, abWaranObjek.TK);
        //        }



        //        return Json(new { result = "OK" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { result = "ERROR", message = ex.Message });
        //    }
        //}

        //public JsonResult RemoveCartAbWaranObjek(AbWaranObjek abWaranObjek)
        //{
        //    try
        //    {
        //        if (abWaranObjek != null)
        //        {
        //            _cart.RemoveItemAbWaranObjek(abWaranObjek.JBahagianId, abWaranObjek.AkCartaId);
        //        }

        //        return Json(new { result = "OK" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { result = "ERROR", message = ex.Message });
        //    }
        //}

        //public JsonResult GetAnItemFromCartAbWaranObjek(AbWaranObjek abWaranObjek)
        //{

        //    try
        //    {
        //        AbWaranObjek data = _cart.abWaranObjek.FirstOrDefault(x => x.JBahagianId == abWaranObjek.JBahagianId && x.AkCartaId == abWaranObjek.AkCartaId && x.TK == abWaranObjek.TK);

        //        return Json(new { result = "OK", record = data });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { result = "ERROR", message = ex.Message });
        //    }
        //}

        //public JsonResult SaveAnItemFromCartAbWaranObjek(AbWaranObjek abWaranObjek)
        //{

        //    try
        //    {

        //        var akTO = _cart.abWaranObjek.FirstOrDefault(x => x.JBahagianId == abWaranObjek.JBahagianId && x.AkCartaId == abWaranObjek.AkCartaId && x.TK == abWaranObjek.TK);

        //        var user = _userManager.GetUserName(User);

        //        if (akTO != null)
        //        {
        //            _cart.RemoveItemAbWaranObjek(abWaranObjek.JBahagianId, abWaranObjek.AkCartaId);

        //            _cart.AddItemAbWaranObjek(abWaranObjek.AbWaranId,
        //                            abWaranObjek.JBahagianId,
        //                            abWaranObjek.AkCartaId,
        //                            abWaranObjek.Amaun,
        //                            abWaranObjek.TK);
        //        }

        //        return Json(new { result = "OK" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { result = "ERROR", message = ex.Message });
        //    }
        //}

        //public JsonResult GetAllItemCartAbWaran()
        //{

        //    try
        //    {
        //        List<AbWaranObjek> objek = _cart.abWaranObjek.ToList();

        //        foreach (AbWaranObjek item in objek)
        //        {
        //            var jBahagian = _unitOfWork.JBahagianRepo.GetById(item.JBahagianId);

        //            item.JBahagian = jBahagian;

        //            var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

        //            item.AkCarta = akCarta;
        //        }

               

        //        return Json(new { result = "OK", objek = objek.OrderBy(d => d.AkCarta?.Kod) });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { result = "ERROR", message = ex.Message });
        //    }
        //}
        //private void PopulateListViewFromCart()
        //{
        //    List<AbWaranObjek> objek = _cart.abWaranObjek.ToList();

        //    foreach (AbWaranObjek item in objek)
        //    {
        //        var jBahagian = _unitOfWork.JBahagianRepo.GetAllDetailsById(item.JBahagianId);

        //        item.JBahagian = jBahagian;

        //        var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

        //        item.AkCarta = akCarta;
        //    }

        //    ViewBag.AbWaranObjek = objek;

            
        //}
    }
}
