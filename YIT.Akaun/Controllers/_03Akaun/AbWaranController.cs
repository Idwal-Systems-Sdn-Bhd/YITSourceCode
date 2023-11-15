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
using YIT._DataAccess.Services.Math;
using YIT.Akaun.Infrastructure;
using YIT.Akaun.Microservices;

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
        private readonly UserServices _userServices;
        private readonly CartAbWaran _cart;


        public AbWaranController(ApplicationDbContext context,
             _IUnitOfWork unitOfWork,
             UserManager<IdentityUser> userManager,
             _AppLogIRepository<AppLog, int> appLog,
             UserServices userServices,
             CartAbWaran cart
)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _appLog = appLog;
            _userServices = userServices;
            _cart = cart;

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

            var abWaran = _unitOfWork.AbWaranRepo.GetAllDetailsById((int)id);
            if (abWaran == null)
            {
                return NotFound();
            }
            PopulateCartAbWaranFromDb(abWaran);
            return View(abWaran);
        }

        // GET: KW/Create
        public IActionResult Create()
        {

            EmptyCart();
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
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                abWaran.UserId = user?.UserName ?? "";

                abWaran.TarMasuk = DateTime.Now;
                abWaran.DPekerjaMasukId = pekerjaId;

                abWaran.AbWaranObjek = _cart.abWaranObjek.ToList();
                abWaran.NoRujukan = GetNoRujukan(abWaran.Tahun);


                _context.Add(abWaran);
                _appLog.Insert("Tambah", abWaran.NoRujukan ?? "", abWaran.NoRujukan ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya ditambah..!";
                return RedirectToAction(nameof(Index));

            }
            ViewBag.NoRujukan = GetNoRujukan(abWaran.Tahun);
            PopulateDropdownList();
            PopulateListViewFromCart();
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
            EmptyCart();
            PopulateDropdownList();
            PopulateCartAbWaranFromDb(abWaran);
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
            PopulateListViewFromCart();
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
            PopulateCartAbWaranFromDb(abWaran);
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
                _appLog.Insert("Rollback", obj.NoRujukan ?? "", obj.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

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
            ViewBag.JKW = _unitOfWork.JKWRepo.GetAllDetails();
        }

        private void PopulateCartAbWaranFromDb(AbWaran abWaran)
        {
            if (abWaran.AbWaranObjek != null)
            {
                foreach (var item in abWaran.AbWaranObjek)
                {
                    _cart.AddItemObjek(
                            abWaran.Id,
                            item.JBahagianId,
                            item.AkCartaId,
                            item.Amaun,
                            item.TK);
                }
            }

            PopulateListViewFromCart();
        }

        private void PopulateListViewFromCart()
        {
            List<AbWaranObjek> objek = _cart.abWaranObjek.ToList();

            foreach (AbWaranObjek item in objek)
            {
                var jBahagian = _unitOfWork.JBahagianRepo.GetAllDetailsById(item.JBahagianId);

                item.JBahagian = jBahagian;

                var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                item.AkCarta = akCarta;
            }

            ViewBag.abWaranObjek = objek;
        }
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
                    if (jBahagian.Kod != null) jBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jBahagian.JPTJ?.JKW?.Kod, jBahagian.JPTJ?.Kod, jBahagian.Kod);


                return Json(new { result = "OK", jBahagian, akCarta });
                }
                catch (Exception ex)
                {
                    return Json(new { result = "Error", message = ex.Message });
                }
            }

            public JsonResult SaveCartAbWaranObjek(AbWaranObjek abWaranObjek)
            {
                try
                {
                    if (abWaranObjek != null)
                    {
                        _cart.AddItemObjek(abWaranObjek.AbWaranId, abWaranObjek.JBahagianId, abWaranObjek.AkCartaId, abWaranObjek.Amaun, abWaranObjek.TK);
                    }



                    return Json(new { result = "OK" });
                }
                catch (Exception ex)
                {
                    return Json(new { result = "ERROR", message = ex.Message });
                }
            }

            public JsonResult RemoveCartAbWaranObjek(AbWaranObjek abWaranObjek)
            {
                try
                {
                    if (abWaranObjek != null)
                    {
                        _cart.RemoveItemObjek(abWaranObjek.JBahagianId, abWaranObjek.AkCartaId);
                    }

                    return Json(new { result = "OK" });
                }
                catch (Exception ex)
                {
                    return Json(new { result = "ERROR", message = ex.Message });
                }
            }

            public JsonResult GetAnItemFromCartAbWaranObjek(AbWaranObjek abWaranObjek)
            {

                try
                {
                    AbWaranObjek data = _cart.abWaranObjek.FirstOrDefault(x => x.JBahagianId == abWaranObjek.JBahagianId && x.AkCartaId == abWaranObjek.AkCartaId);

                    return Json(new { result = "OK", record = data });
                }
                catch (Exception ex)
                {
                    return Json(new { result = "ERROR", message = ex.Message });
                }
            }

            public JsonResult SaveAnItemFromCartAbWaranObjek(AbWaranObjek abWaranObjek)
            {

                try
                {

                    var abTO = _cart.abWaranObjek.FirstOrDefault(x => x.JBahagianId == abWaranObjek.JBahagianId && x.AkCartaId == abWaranObjek.AkCartaId);

                    var user = _userManager.GetUserName(User);

                    if (abTO != null)
                    {
                        _cart.RemoveItemObjek(abWaranObjek.JBahagianId, abWaranObjek.AkCartaId);

                        _cart.AddItemObjek(abWaranObjek.AbWaranId,
                                        abWaranObjek.JBahagianId,
                                        abWaranObjek.AkCartaId,
                                        abWaranObjek.Amaun,
                                        abWaranObjek.TK);
                    }

                    return Json(new { result = "OK" });
                }
                catch (Exception ex)
                {
                    return Json(new { result = "ERROR", message = ex.Message });
                }
            }
        public JsonResult GetAllItemCartAbWaran()
        {

            try
            {
                List<AbWaranObjek> objek = _cart.abWaranObjek.ToList();

                foreach (AbWaranObjek item in objek)
                {
                    var jBahagian = _unitOfWork.JBahagianRepo.GetAllDetailsById(item.JBahagianId);

                    item.JBahagian = jBahagian;
                    item.JBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jBahagian.JPTJ?.JKW?.Kod,jBahagian.JPTJ?.Kod,jBahagian.Kod);

                    var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                    item.AkCarta = akCarta;
                }
                return Json(new { result = "OK", objek = objek.OrderBy(d => d.AkCarta?.Kod) });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult JsonGetKod(string year)
        {
            try
            {
                var result = GetNoRujukan(year);

                return Json(new { result = "OK", record = result });
            }
            catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }
        }

        private string GetNoRujukan(string year)
        {
            string prefix = "WR/" + year + "/";
            int x = 1;
            string noRujukan = prefix + "0000";

            var LatestNoRujukan = _context.AbWaran
                       .IgnoreQueryFilters()
                       .Where(x => x.Tahun == year)
                       .Max(x => x.NoRujukan);

            if (LatestNoRujukan == null)
            {
                noRujukan = string.Format("{0:" + prefix + "0000}", x);
            }
            else
            {
                x = int.Parse(LatestNoRujukan.Substring(9));
                x++;
                noRujukan = string.Format("{0:" + prefix + "0000}", x);
            }
            return noRujukan;
        }

    }
}
