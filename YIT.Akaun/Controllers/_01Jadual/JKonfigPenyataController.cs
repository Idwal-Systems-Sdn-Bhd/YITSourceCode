using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services;
using YIT._DataAccess.Services.Cart;
using YIT.Akaun.Microservices;

namespace YIT.Akaun.Controllers._01Jadual
{
    [Authorize]
    public class JKonfigPenyataController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodJKonfigPenyata;
        public const string namamodul = Modules.namaJKonfigPenyata;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly CartJKonfigPenyata _cart;

        public JKonfigPenyataController(
            _IUnitOfWork unitOfWork,
            ApplicationDbContext context,
            _AppLogIRepository<AppLog, int> appLog,
            UserManager<IdentityUser> userManager,
            CartJKonfigPenyata cart

            )
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _appLog = appLog;
            _userManager = userManager;
            _cart = cart;
        }
        public IActionResult Index()
        {
            return View(_unitOfWork.JKonfigPenyataRepo.GetAllIncludeDeleted());
        }

        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var konfigPenyata = _unitOfWork.JKonfigPenyataRepo.GetAllDetailsById((int)id);

            if (konfigPenyata == null)
            {
                return NotFound();
            }

            return View(konfigPenyata);
        }

        public IActionResult Create()
        {
            EmptyCart();
            PopulateDropdownList();
            return View();
        }

        private void PopulateDropdownList()
        {
            var kategoriTajuk = EnumHelper<EnKategoriTajuk>.GetList();
            ViewBag.EnKategoriTajuk = kategoriTajuk;

            var kategoriJumlah = EnumHelper<EnKategoriJumlah>.GetList();
            ViewBag.EnKategoriJumlah = kategoriJumlah;

            var jenisCarta = EnumHelper<EnJenisCarta>.GetList();

            ViewBag.EnJenisCartaList = jenisCarta;

            ViewBag.KodList = _unitOfWork.AkCartaRepo.GetResultsByParas(EnParas.Paras4);
        }

        private JsonResult EmptyCart()
        {
            try
            {
                _cart.ClearBaris();
                _cart.ClearBarisFormula();

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult EmptyBarisCart()
        {
            try
            {
                _cart.ClearBarisFormulaByBarisBil();

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new {result = "ERROR", message = ex.Message});
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JKonfigPenyata konfigPenyata, string syscode)
        {
            if (konfigPenyata.Tahun != null && !TahunKodPenyataExists(konfigPenyata.Tahun, konfigPenyata.Kod))
            {
                if (ModelState.IsValid)
                {
                    if (_cart.JKonfigPenyataBaris != null && _cart.JKonfigPenyataBaris.Any())
                    {
                        foreach(var baris in _cart.JKonfigPenyataBaris)
                        {
                            if (baris.JKonfigPenyataBarisFormula != null && baris.JKonfigPenyataBarisFormula.Any())
                            {
                                foreach (var formula in baris.JKonfigPenyataBarisFormula)
                                {
                                    formula.Id = 0;
                                    formula.JKonfigPenyataBarisId = 0;
                                }
                            }

                            baris.JKonfigPenyataId = 0;
                        }
                    }
                    konfigPenyata.JKonfigPenyataBaris = _cart.JKonfigPenyataBaris?.ToList();
                    
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    konfigPenyata.UserId = user?.UserName ?? "";

                    konfigPenyata.TarMasuk = DateTime.Now;
                    konfigPenyata.DPekerjaMasukId = pekerjaId;

                    _context.Add(konfigPenyata);
                    _appLog.Insert("Tambah", konfigPenyata.Tahun + " - " + konfigPenyata.Perihal, konfigPenyata.Kod ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya ditambah..!";
                    return RedirectToAction(nameof(Index));
                }
                
            }
            else
            {
                TempData[SD.Error] = "Tahun untuk kod laporan ini telah wujud..!";
            }

            PopulateDropdownList();
            return View(konfigPenyata);
        }

        private bool TahunKodPenyataExists(string tahun, string? kod)
        {
            return _context.JKonfigPenyata.Any(kp => kp.Tahun == tahun && kp.Kod == kod);
        }

        // GET: KonfigPenyata/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var konfigPenyata = _unitOfWork.JKonfigPenyataRepo.GetAllDetailsById((int)id);
            if (konfigPenyata == null)
            {
                return NotFound();
            }
            PopulateDropdownList();
            EmptyCart();
            PopulateCartJKonfigPenyataFromDb(konfigPenyata);
            return View(konfigPenyata);
        }

        private void PopulateCartJKonfigPenyataFromDb(JKonfigPenyata konfigPenyata)
        {
            if (konfigPenyata.JKonfigPenyataBaris != null)
            {
                foreach (var baris in konfigPenyata.JKonfigPenyataBaris)
                {
                    var formula = new List<JKonfigPenyataBarisFormula>();

                    if (baris.JKonfigPenyataBarisFormula != null && baris.JKonfigPenyataBarisFormula.Count > 0)
                    {
                        foreach (var foo in baris.JKonfigPenyataBarisFormula)
                        {
                            foo.JKonfigPenyataBaris = null;
                        }

                        formula.AddRange(baris.JKonfigPenyataBarisFormula);
                    }
                    baris.JKonfigPenyata = null;

                    _cart.AddItemBaris(baris.Bil, baris.JKonfigPenyataId,baris.EnKategoriTajuk,baris.Perihal,baris.Susunan,baris.IsFormula, baris.EnKategoriJumlah, baris.JumlahSusunanList, formula);
                }
            }
            PopulateListViewFromCart();
        }

        private void PopulateListViewFromCart()
        {
            List<JKonfigPenyataBaris> baris = _cart.JKonfigPenyataBaris.ToList();

            ViewBag.JKonfigPenyataBaris = baris.OrderBy(b => b.Susunan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, JKonfigPenyata konfigPenyata, string syscode)
        {
            if (id != konfigPenyata.Id)
            {
                return NotFound();
            }

            if (konfigPenyata.Tahun != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = _unitOfWork.JKonfigPenyataRepo.GetAllDetailsById(konfigPenyata.Id);
                    var tahunAsal = objAsal?.Tahun;
                    if (objAsal != null)
                    {
                        konfigPenyata.UserId = objAsal.UserId;
                        konfigPenyata.TarMasuk = objAsal.TarMasuk;
                        konfigPenyata.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                        if (objAsal.JKonfigPenyataBaris != null && objAsal.JKonfigPenyataBaris.Count > 0)
                        {
                            foreach (var item in objAsal.JKonfigPenyataBaris)
                            {
                                if (item.JKonfigPenyataBarisFormula != null && item.JKonfigPenyataBarisFormula.Count > 0)
                                {
                                    foreach (var foo in item.JKonfigPenyataBarisFormula)
                                    {
                                        var formula = _context.JKonfigPenyataBarisFormula.FirstOrDefault(f => f.Id == foo.Id);
                                        if (formula != null) _context.Remove(foo);
                                    }
                                    
                                }

                                var model = _context.JKonfigPenyataBaris.FirstOrDefault(b => b.Id == item.Id);
                                if (model != null) _context.Remove(model);
                            }
                        }

                        _context.Entry(objAsal).State = EntityState.Detached;
                    }

                    konfigPenyata.JKonfigPenyataBaris = _cart.JKonfigPenyataBaris?.ToList();
                    konfigPenyata.UserIdKemaskini = user?.UserName ?? "";

                    konfigPenyata.TarKemaskini = DateTime.Now;
                    konfigPenyata.DPekerjaKemaskiniId = pekerjaId;

                    _unitOfWork.JKonfigPenyataRepo.Update(konfigPenyata);

                    _appLog.Insert("Ubah", tahunAsal ?? "", tahunAsal ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KonfigPenyataExists(konfigPenyata.Id))
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
            PopulateDropdownList();
            return View(konfigPenyata);
        }

        private bool KonfigPenyataExists(int id)
        {
            return _unitOfWork.JKonfigPenyataRepo.IsExist(b => b.Id == id);
        }
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var konfigPenyata = _unitOfWork.JKonfigPenyataRepo.GetAllDetailsById((int)id);

            if (konfigPenyata == null) return NotFound();

            return View(konfigPenyata);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string syscode)
        {
            var konfigPenyata = _unitOfWork.JKonfigPenyataRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (konfigPenyata != null && konfigPenyata.Tahun != null)
            {
                konfigPenyata.UserIdKemaskini = user?.UserName ?? "";
                konfigPenyata.TarKemaskini = DateTime.Now;
                konfigPenyata.DPekerjaKemaskiniId = pekerjaId;

                _context.JKonfigPenyata.Remove(konfigPenyata);
                _appLog.Insert("Hapus", konfigPenyata.Tahun + " - " + konfigPenyata.Tahun, konfigPenyata.Kod ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dihapuskan..!";
            }

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> RollBack(int id, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            var obj = await _context.JKonfigPenyata.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.JKonfigPenyata.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.Tahun + " - " + obj.Tahun, obj.Kod ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index));
        }

        public JsonResult GetItemsBasedOnYear(string? tahun, string kod)
        {
            try
            {
                var result = _unitOfWork.JKonfigPenyataRepo.GetAllDetailsByTahunOrKod(tahun, kod);

                if (result.Id == 0)
                {
                    result = _unitOfWork.JKonfigPenyataRepo.GetAllDetailsByTahunOrKod((int.Parse(tahun ?? DateTime.Now.Year.ToString()) - 1).ToString(), kod);
                    if (result.Id == 0)
                    {
                        result = _unitOfWork.JKonfigPenyataRepo.GetAllDetailsByTahunOrKod(tahun, null);
                        if (result.Id == 0)
                        {
                            result = _unitOfWork.JKonfigPenyataRepo.GetAllDetailsByTahunOrKod((int.Parse(tahun ?? DateTime.Now.Year.ToString()) - 1).ToString(), null);
                        }
                    }
                }

                if (result != null)
                {
                    PopulateCartJKonfigPenyataFromDb(result);
                }

                return Json(new { result = "OK", record = result });

            }
            catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }



        }

        public JsonResult RemoveCartJKonfigPerubahanEkuitiBaris(JKonfigPenyataBaris baris)
        {
            try
            {
                _cart.RemoveItemBaris(baris.Bil);

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAllItemCartJKonfigPenyata()
        {

            try
            {
                List<JKonfigPenyataBaris> baris = _cart.JKonfigPenyataBaris.ToList();

                foreach (var bar in baris) 
                { 
                    //bar.JKonfigPenyataBarisFormula = _cartBaris.JKonfigPenyataBarisFormula.Where(x => x.BarisBil == bar.Bil).OrderBy(f => f.BarisBil).ToList();
                }

                return Json(new { result = "OK", baris = baris.OrderBy(d => d.Susunan) });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartJKonfigPenyataBaris(JKonfigPenyataBaris baris)
        {

            try
            {
                JKonfigPenyataBaris data = _cart.JKonfigPenyataBaris.FirstOrDefault(x => x.Bil == baris.Bil) ?? new JKonfigPenyataBaris();

                if (data != null && data.JKonfigPenyataBarisFormula != null && data.JKonfigPenyataBarisFormula.Count > 0)
                {
                    foreach(var formula in data.JKonfigPenyataBarisFormula)
                    {
                        _cart.AddItemBarisFormula(formula.BarisBil, formula.JKonfigPenyataBarisId, formula.EnJenisOperasi, formula.IsPukal, formula.EnJenisCartaList, formula.IsKecuali, formula.KodList, formula.SetKodList);
                    }
                }

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetBilJKonfigPenyataBaris()
        {
            try
            {
                int? bil =  _cart.JKonfigPenyataBaris.OrderByDescending(b => b.Bil).FirstOrDefault()?.Bil ?? 0;

                bil += 1;

                return Json(new { result = "OK", bil });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }
        public JsonResult SaveAnItemFromCartJKonfigPenyataBaris(JKonfigPenyataBaris baris)
        {
            try
            {
                var data = _cart.JKonfigPenyataBaris.FirstOrDefault(x => x.Bil == baris.Bil);
                int? bil = _cart.JKonfigPenyataBaris.OrderByDescending(b => b.Bil).FirstOrDefault()?.Bil ?? 0;

                int? susunan = null;
                bil += 1;
                if (data != null)
                {
                    bil = baris.Bil;
                    susunan = data.Susunan;

                    _cart.RemoveItemBaris(baris.Bil);

                    _cart.AddItemBaris((int)bil,0, baris.EnKategoriTajuk,baris.Perihal, baris.Susunan, baris.IsFormula, baris.EnKategoriJumlah, baris.JumlahSusunanList, _cart.JKonfigPenyataBarisFormula?.ToList() ?? new List<JKonfigPenyataBarisFormula>());
                }
                else
                {
                    _cart.AddItemBaris((int)bil, 0, baris.EnKategoriTajuk, baris.Perihal, baris.Susunan, baris.IsFormula, baris.EnKategoriJumlah, baris.JumlahSusunanList, _cart.JKonfigPenyataBarisFormula?.ToList() ?? new List<JKonfigPenyataBarisFormula>());
                }

                _cart.ClearBarisFormulaByBarisBil();

                return Json(new { result = "OK", susunan });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetBilJKonfigPenyataBarisFormula()
        {
            try
            {
                int? bil = _cart.JKonfigPenyataBarisFormula.OrderByDescending(b => b.BarisBil).FirstOrDefault()?.BarisBil ?? 0;

                bil += 1;

                return Json(new { result = "OK", bil });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartJKonfigPerubahanEkuitiBarisFormula(JKonfigPenyataBarisFormula formula)
        {
            try
            {
                _cart.RemoveItemBarisFormula(formula.EnJenisOperasi, formula.BarisBil);

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartJKonfigPenyataBarisFormula(JKonfigPenyataBarisFormula formula)
        {

            try
            {
                JKonfigPenyataBarisFormula data = _cart.JKonfigPenyataBarisFormula.FirstOrDefault(x => x.EnJenisOperasi == formula.EnJenisOperasi) ?? new JKonfigPenyataBarisFormula();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartJKonfigPenyataBarisFormula(JKonfigPenyataBarisFormula formula)
        {
            try
            {
                var data = _cart.JKonfigPenyataBarisFormula.FirstOrDefault(x => x.EnJenisOperasi == formula.EnJenisOperasi && x.BarisBil == formula.BarisBil);
                int? bil = _cart.JKonfigPenyataBarisFormula.OrderByDescending(b => b.BarisBil).FirstOrDefault()?.BarisBil ?? 0;

                bil += 1;

                formula.SetKodList = _unitOfWork.AkCartaRepo.GetSetOfCartaStringList(formula.IsPukal, formula.EnJenisCartaList, formula.IsKecuali, formula.KodList);

                if (data != null)
                {
                    bil = formula.BarisBil;
                    _cart.RemoveItemBarisFormula(formula.EnJenisOperasi, formula.BarisBil);

                    _cart.AddItemBarisFormula((int)bil, formula.JKonfigPenyataBarisId, formula.EnJenisOperasi, formula.IsPukal, formula.EnJenisCartaList, formula.IsKecuali, formula.KodList, formula.SetKodList);
                }
                else
                {
                    _cart.AddItemBarisFormula((int)bil, formula.JKonfigPenyataBarisId, formula.EnJenisOperasi, formula.IsPukal, formula.EnJenisCartaList, formula.IsKecuali, formula.KodList, formula.SetKodList);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAllItemCartJKonfigPenyataBaris()
        {

            try
            {

                List<JKonfigPenyataBarisFormula> formula = _cart.JKonfigPenyataBarisFormula.ToList();

                foreach (var item in formula.OrderBy(b => b.BarisBil).ThenBy(b => b.EnJenisOperasi))
                {

                    string sentence = _unitOfWork.AkCartaRepo.FormulaInSentence(item.EnJenisOperasi, item.EnJenisCartaList, item.IsKecuali, item.KodList);

                    item.BarisDescription = "Operasi " + item.EnJenisOperasi.GetDisplayName();
                    item.FormulaDescription = sentence;
                }

                return Json(new { result = "OK", formula = formula.OrderBy(d => d.BarisBil) });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }
    }
}
