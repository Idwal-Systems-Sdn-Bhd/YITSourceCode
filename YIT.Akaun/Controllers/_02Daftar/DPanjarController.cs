using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Implementations;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services.Cart;
using YIT._DataAccess.Services.Math;
using YIT.Akaun.Models.ViewModels;

namespace YIT.Akaun.Controllers._02Daftar
{
    [Authorize]
    public class DPanjarController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodDPanjar;
        public const string namamodul = Modules.namaDPanjar;
        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly IAkPanjarLejarRepository _panjarLejar;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly CartDPanjar _cart;

        public DPanjarController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            IAkPanjarLejarRepository panjarLejar,
            _AppLogIRepository<AppLog, int> appLog,
            UserManager<IdentityUser> userManager,
            CartDPanjar cart)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _panjarLejar = panjarLejar;
            _appLog = appLog;
            _userManager = userManager;
            _cart = cart;
        }
        public IActionResult Index(string searchString,
            string searchColumn)
        {
            if (searchString == null)
            {
                HttpContext.Session.Clear();
                return View();
            }

            SaveFormFields(searchString);

            var dPanjar = _unitOfWork.DPanjarRepo.GetResults(searchString, searchColumn);

            return View(dPanjar);
        }

        private void SaveFormFields(string searchString)
        {
            PopulateFormFields(searchString);

            if(searchString != null)
            {
                HttpContext.Session.SetString("searchString", searchString);
            }
            else
            {
                searchString = HttpContext.Session.GetString("searchString") ?? "";
                ViewBag.SearchString = searchString;
            }
        }

        private void PopulateFormFields(string searchString)
        {
            ViewBag.searchString = searchString;
        }

        public IActionResult Create()
        {
            ViewBag.Kod = GenerateRunningNumber();
            PopulateDropDownList();
            return View();
        }

        private dynamic GenerateRunningNumber()
        {
            var maxRefNo = _unitOfWork.DPanjarRepo.GetMaxRefNo();
            return RunningNumberFormatter.GenerateRunningNumber("", maxRefNo, "00");
        }

        private void PopulateDropDownList()
        {
            ViewBag.JKWPTJBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetails();
            ViewBag.JCawangan = _unitOfWork.JCawanganRepo.GetAll();
            ViewBag.AkCarta = _unitOfWork.AkCartaRepo.GetResultsByParas(EnParas.Paras4);
            ViewBag.DPekerja = _unitOfWork.DPekerjaRepo.GetAll();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DPanjar panjar, string syscode)
        {
            if (panjar.AkCartaId == 0)
            {
                TempData[SD.Error] = "Sila Pilih Kod Akaun Panjar..!";
            }
            else
            {
                if (panjar.JCawanganId != 0)
                {
                    if (ModelState.IsValid)
                    {
                        panjar.Kod = GenerateRunningNumber();
                        var user = await _userManager.GetUserAsync(User);
                        int? pekerjaId = _context.ApplicationUsers.FirstOrDefault(p => p.Id == user!.Id)!.DPekerjaId;
                        panjar.UserId = user?.UserName ?? "";

                        panjar.TarMasuk = DateTime.Now;
                        panjar.DPekerjaMasukId = pekerjaId;

                        panjar.DPanjarPemegang = _cart.DPanjarPemegang?.ToList();
                        _context.Add(panjar);

                        _appLog.Insert("Tambah", "Panjar bagi cawangan " + panjar.JCawangan?.Kod + " - " + panjar.JCawangan?.Perihal, panjar.JCawangan?.Kod ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);

                        await _context.SaveChangesAsync();

                        TempData[SD.Success] = "Data berjaya ditambah..!";
                        return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString") });

                    }
                    else
                    {
                        TempData[SD.Error] = "Data gagal disimpan..!";
                    }
                }
                else
                {
                    TempData[SD.Error] = "Cawangan ini telah wujud..!";
                }
            }
            PopulateListViewFromCart(panjar);
            PopulateDropDownList();
            return View(panjar);
        }

        private bool CawanganPanjarExists(int jCawanganId)
        {
            return _unitOfWork.DPanjarRepo.IsExist(p => p.JCawanganId == jCawanganId);
        }

        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var panjar = _unitOfWork.DPanjarRepo.GetAllDetailsById((int)id);
            if (panjar == null)
            {
                return NotFound();
            }
            EmptyCart();

            PopulateCartDPanjarFromDb(panjar);
            return View(panjar);
        }

        private void PopulateCartDPanjarFromDb(DPanjar panjar)
        {
            if (panjar.DPanjarPemegang != null)
            {
                foreach (var item in panjar.DPanjarPemegang)
                {
                    _cart.AddItemPemegang(item.DPanjarId, item.DPekerjaId, item.JangkaMasaDari, item.JangkaMasaHingga, item.IsAktif);
                }
            }
            PopulateListViewFromCart(panjar);

        }

        private void PopulateListViewFromCart(DPanjar panjar)
        {
            List<DPanjarPemegang> pemegang = _cart.DPanjarPemegang.ToList();

            foreach (var item in pemegang)
            {
                var dPekerja = _unitOfWork.DPekerjaRepo.GetAllDetailsById(item.DPekerjaId);

                item.DPekerja = dPekerja;
            }

            ViewBag.dPanjarPemegang = pemegang;

            // add akPanjarLejar into viewbag
            List<AkPanjarLejar> lejar = new List<AkPanjarLejar>();

            if (panjar != null)
            {
                lejar = _panjarLejar.GetListByDPanjarId(panjar.Id);
            }

            ViewBag.akPanjarLejar = lejar;

        }

        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var panjar = _unitOfWork.DPanjarRepo.GetAllDetailsById((int)id);
            if (panjar == null)
            {
                return NotFound();
            }
            EmptyCart();
            PopulateDropDownList();
            PopulateCartDPanjarFromDb(panjar);
            return View(panjar);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DPanjar panjar, decimal bakiDiTangan, string syscode)
        {
            if (id != panjar.Id)
            {
                return NotFound();
            }

            if (panjar.JCawanganId != 0 && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.FirstOrDefault(b => b.Id == user!.Id)!.DPekerjaId;

                    var objAsal = _unitOfWork.DPanjarRepo.GetAllDetailsById(panjar.Id);

                    if (objAsal != null)
                    {
                        // if no transaction exist for this dPanjarId
                        if (!_unitOfWork.AkRekupRepo.IsExist(r => r.DPanjarId == id))
                        {

                            panjar.HadJumlah = bakiDiTangan;

                            _unitOfWork.AkRekupRepo.Add(new AkRekup { DPanjarId = id, NoRujukan = "BAKI AWAL", Jumlah = bakiDiTangan });

                        }
                        else
                        {
                            // if baki awal already exist, update new baki awal
                            if (_unitOfWork.AkRekupRepo.IsExist(r => r.NoRujukan == "BAKI AWAL" && r.DPanjarId == id))
                            {
                                panjar.HadJumlah = bakiDiTangan;

                                var akRekup = _unitOfWork.AkRekupRepo.GetDetailsByBakiAwalAndDPanjarId("BAKI AWAL", id, false);

                                akRekup.Jumlah = bakiDiTangan;

                                _unitOfWork.AkRekupRepo.Update(akRekup);
                            }
                        }

                        panjar.JCawanganId = objAsal.JCawanganId;
                        panjar.UserId = objAsal.UserId;
                        panjar.TarMasuk = objAsal.TarMasuk;
                        panjar.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                        if (objAsal.DPanjarPemegang != null && objAsal.DPanjarPemegang.Count > 0)
                        {
                            foreach (var item in objAsal.DPanjarPemegang)
                            {
                                var model = _context.DPanjarPemegang.FirstOrDefault(b => b.Id == item.Id);
                                if (model != null) _context.Remove(model);
                            }
                        }

                        _context.Entry(objAsal).State = EntityState.Detached;

                        panjar.UserIdKemaskini = user?.UserName ?? "";
                        panjar.TarKemaskini = DateTime.Now;
                        panjar.DPekerjaKemaskiniId = pekerjaId;
                        panjar.DPanjarPemegang = _cart.DPanjarPemegang?.ToList();


                    }

                    _unitOfWork.DPanjarRepo.Update(panjar);

                    _appLog.Insert("Ubah", "Panjar bagi cawangan " + panjar.JCawangan?.Kod + " - " + panjar.JCawangan?.Perihal, panjar.JCawangan?.Kod ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya diubah..!";

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CawanganPanjarExists(panjar.JCawanganId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString") });
            }
            PopulateDropDownList();
            return View(panjar);

        }

        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var panjar = _unitOfWork.DPanjarRepo.GetAllDetailsById((int)id);
            if (panjar == null) return NotFound();

            PopulateCartDPanjarFromDb(panjar);
            return View(panjar);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string syscode)
        {
            var panjar = _unitOfWork.DPanjarRepo.GetById(id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.FirstOrDefault(b => b.Id == user!.Id)!.DPekerjaId;

            if (panjar != null && panjar.JCawanganId != 0)
            {
                panjar.UserIdKemaskini = user?.UserName ?? "";
                panjar.TarKemaskini = DateTime.Now;
                panjar.DPekerjaKemaskiniId = pekerjaId;

                _context.DPanjar.Remove(panjar);

                _appLog.Insert("Hapus", "Panjar bagi cawangan " + panjar.JCawangan?.Kod + " - " + panjar.JCawangan?.Perihal, panjar.JCawangan?.Kod ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();

                TempData[SD.Success] = "Data berjaya dihapuskan..!";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString") });
        }

        public async Task<IActionResult> RollBack(int id, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.FirstOrDefault(b => b.Id == user!.Id)!.DPekerjaId;

            var obj = await _context.DPanjar.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.DPanjar.Update(obj);

                // Batal operation end
                _appLog.Insert("RollBack", "Panjar bagi cawangan " + obj.JCawangan?.Kod + " - " + obj.JCawangan?.Perihal, obj.JCawangan?.Kod ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString") });
        }

        public JsonResult EmptyCart()
        {
            try
            {
                _cart.ClearPemegang();

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR" , message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartDPanjarPemegang(DPanjarPemegang dPanjarPemegang)
        {

            try
            {
                DPanjarPemegang data = _cart.DPanjarPemegang.FirstOrDefault(x => x.DPekerjaId == dPanjarPemegang.DPekerjaId) ?? new DPanjarPemegang();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartDPanjarPemegang(DPanjarPemegang dPanjarPemegang)
        {

            try
            {

                var data = _cart.DPanjarPemegang.FirstOrDefault(x => x.DPekerjaId == dPanjarPemegang.DPekerjaId);

                var user = _userManager.GetUserName(User);

                if (data != null)
                {
                    _cart.RemoveItemPemegang(dPanjarPemegang.DPekerjaId);

                    if (dPanjarPemegang.IsAktif == false) dPanjarPemegang.JangkaMasaHingga = DateTime.Now;

                    _cart.AddItemPemegang(dPanjarPemegang.DPanjarId, dPanjarPemegang.DPekerjaId, dPanjarPemegang.JangkaMasaDari, dPanjarPemegang.JangkaMasaHingga, dPanjarPemegang.IsAktif);

                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAllItemCartDPanjar()
        {
            try
            {
                List<DPanjarPemegang> pemegang = _cart.DPanjarPemegang.ToList();

                foreach (DPanjarPemegang item in pemegang)
                {
                    var dPekerja = _unitOfWork.DPekerjaRepo.GetAllDetailsById(item.DPekerjaId);

                    item.DPekerja = dPekerja;
                }

                return Json(new { result = "OK", pemegang = pemegang.OrderBy(p => p.DPekerja!.NoGaji).ToList() });
            }
            catch (Exception ex)
            {
                return Json(new {result = "ERROR", message = ex.Message});
            }
        }

        public JsonResult GetDPekerja(int DPekerjaId)
        {
            try
            {
                var dPekerja = _unitOfWork.DPekerjaRepo.GetById(DPekerjaId);

                if (dPekerja == null) return Json(new { result = "ERROR", message = "Pekerja tidak wujud" });

                return Json(new { result = "OK", dPekerja });
            }
            catch (Exception ex) { 
                return Json(new { result = "ERROR", message = ex.Message }); 
            }
        }

        public JsonResult SaveCartDPanjarPemegang(DPanjarPemegang dPanjarPemegang)
        {
            try
            {
                if (dPanjarPemegang != null)
                {
                    _cart.AddItemPemegang(dPanjarPemegang.DPanjarId, dPanjarPemegang.DPekerjaId, dPanjarPemegang.JangkaMasaDari, dPanjarPemegang.JangkaMasaHingga, dPanjarPemegang.IsAktif);

                    return Json(new { result = "OK" });
                }
                else
                {
                    return Json(new { result = "ERROR", message = "data tidak lengkap" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartDPanjarPemegang(DPanjarPemegang dPanjarPemegang)
        {
            try
            {
                if (dPanjarPemegang != null)
                {
                    _cart.RemoveItemPemegang(dPanjarPemegang.DPekerjaId); 
                    return Json(new { result = "OK" });
                }
                else
                {
                    return Json(new { result = "ERROR", message = "data tidak lengkap" });
                }

            }
            catch(Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }
    }

}
