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
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services;
using YIT._DataAccess.Services.Cart;
using YIT._DataAccess.Services.Math;
using YIT.Akaun.Microservices;

namespace YIT.Akaun.Controllers._01Jadual
{
    [Authorize(Roles = Init.superAdminSupervisorRole)]
    public class JKonfigPerubahanEkuitiController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodJKonfigPerubahanEkuiti;
        public const string namamodul = Modules.namaJKonfigPerubahanEkuiti;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly CartJKonfigPerubahanEkuiti _cart;

        public JKonfigPerubahanEkuitiController(
            _IUnitOfWork unitOfWork,
            ApplicationDbContext context,
            _AppLogIRepository<AppLog, int> appLog,
            UserManager<IdentityUser> userManager,
            CartJKonfigPerubahanEkuiti cart
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
            return View(_unitOfWork.JKonfigPerubahanEkuitiRepo.GetAllDetails());
        }

        // GET: KonfigPerubahanEkuiti/Details/5
        [Authorize(Policy = modul)]
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var konfigPerubahanEkuiti = _unitOfWork.JKonfigPerubahanEkuitiRepo.GetAllDetailsById((int)id);
            if (konfigPerubahanEkuiti == null)
            {
                return NotFound();
            }

            return View(konfigPerubahanEkuiti);
        }


        // GET: KonfigPerubahanEkuiti/Create
        [Authorize(Policy = modul + "C")]
        public IActionResult Create()
        {
            EmptyCart();
            PopulateDropdownList();
            return View();
        }

        // jsonResult
        public JsonResult EmptyCart()
        {
            try
            {
                _cart.ClearBaris();

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }


        public void PopulateDropdownList()
        {
            ViewBag.JKW = _unitOfWork.JKWRepo.GetAll();
            var jenisEkuiti = EnumHelper<EnJenisLajurJadualPerubahanEkuiti>.GetList();

            ViewBag.EnLajurJadual = jenisEkuiti;

            var jenisCarta = EnumHelper<EnJenisCarta>.GetList();

            ViewBag.EnJenisCartaList = jenisCarta;

            ViewBag.KodList = _unitOfWork.AkCartaRepo.GetResultsByParas(EnParas.Paras4);
        }

        // POST: KonfigPerubahanEkuiti/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "C")]
        public async Task<IActionResult> Create(JKonfigPerubahanEkuiti konfigPerubahanEkuiti, 
            string syscode)
        {
            if (konfigPerubahanEkuiti.Tahun != null && TahunEnJenisEkuitiKonfigPerubahanEkuitiExists(konfigPerubahanEkuiti.Tahun, konfigPerubahanEkuiti.EnLajurJadual) == false)
            {
                if (ModelState.IsValid)
                {

                    konfigPerubahanEkuiti.JKonfigPerubahanEkuitiBaris = _cart.JKonfigPerubahanEkuitiBaris?.ToList();

                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    konfigPerubahanEkuiti.UserId = user?.UserName ?? "";

                    konfigPerubahanEkuiti.TarMasuk = DateTime.Now;
                    konfigPerubahanEkuiti.DPekerjaMasukId = pekerjaId;
                    
                    _context.Add(konfigPerubahanEkuiti);
                    _appLog.Insert("Tambah", konfigPerubahanEkuiti.EnLajurJadual.GetDisplayName() + " - " + konfigPerubahanEkuiti.Tahun, konfigPerubahanEkuiti.EnLajurJadual.GetDisplayName() ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya ditambah..!";
                    return RedirectToAction(nameof(Index));

                }
            }
            else
            {
                TempData[SD.Error] = "Tahun untuk Kump. Wang ini telah wujud..!";
            }

            PopulateDropdownList();
            return View(konfigPerubahanEkuiti);
        }

        // GET: KonfigPerubahanEkuiti/Edit/5
        [Authorize(Policy = modul + "E")]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var konfigPerubahanEkuiti = _unitOfWork.JKonfigPerubahanEkuitiRepo.GetAllDetailsById((int)id);
            if (konfigPerubahanEkuiti == null)
            {
                return NotFound();
            }
            PopulateDropdownList();
            EmptyCart();
            PopulateCartJKonfigPerubahanEkuitiFromDb(konfigPerubahanEkuiti);
            return View(konfigPerubahanEkuiti);
        }

        private void PopulateCartJKonfigPerubahanEkuitiFromDb(JKonfigPerubahanEkuiti konfigPerubahanEkuiti)
        {
            if (konfigPerubahanEkuiti.JKonfigPerubahanEkuitiBaris != null)
            {
                foreach (var item in konfigPerubahanEkuiti.JKonfigPerubahanEkuitiBaris)
                {
                    _cart.AddItemBaris(konfigPerubahanEkuiti.Id, item.EnBaris, item.EnJenisOperasi, item.IsPukal, item.EnJenisCartaList, item.IsKecuali, item.KodList, item.SetKodList);
                }
            }
            PopulateListViewFromCart();
        }

        private void PopulateListViewFromCart()
        {
            List<JKonfigPerubahanEkuitiBaris> baris = _cart.JKonfigPerubahanEkuitiBaris.ToList();

            string barisBefore = "";

            foreach (var item in baris.OrderBy(b => b.EnBaris).ThenBy(b => b.EnJenisOperasi))
            {
                
                    string barisSentences = item.EnBaris.GetDisplayName();
                    if (barisSentences == barisBefore)
                    {
                        barisSentences = "";
                    }
                    string sentence = _unitOfWork.AkCartaRepo.FormulaInSentence(item.EnJenisOperasi, item.EnJenisCartaList, item.IsKecuali, item.KodList);

                    item.BarisDescription = barisSentences;
                    item.FormulaDescription = sentence;

                    barisBefore = item.EnBaris.GetDisplayName();
            }

            ViewBag.jKonfigPerubahanEkuitiBaris = baris.OrderBy(b => b.EnBaris).ThenBy(b => b.EnJenisOperasi);
        }
        // POST: KonfigPerubahanEkuiti/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "E")]
        public async Task<IActionResult> Edit(int id, JKonfigPerubahanEkuiti konfigPerubahanEkuiti, string syscode)
        {
            if (id != konfigPerubahanEkuiti.Id)
            {
                return NotFound();
            }

            if (konfigPerubahanEkuiti.Tahun != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = _unitOfWork.JKonfigPerubahanEkuitiRepo.GetAllDetailsById(konfigPerubahanEkuiti.Id);
                    var tahunAsal = objAsal?.Tahun;
                    if (objAsal != null)
                    {
                        konfigPerubahanEkuiti.UserId = objAsal.UserId;
                        konfigPerubahanEkuiti.TarMasuk = objAsal.TarMasuk;
                        konfigPerubahanEkuiti.DPekerjaMasukId = objAsal.DPekerjaMasukId;
                        
                        if (objAsal.JKonfigPerubahanEkuitiBaris != null && objAsal.JKonfigPerubahanEkuitiBaris.Count > 0)
                        {
                            foreach (var item in objAsal.JKonfigPerubahanEkuitiBaris)
                            {
                                var model = _context.JKonfigPerubahanEkuitiBaris.FirstOrDefault(b => b.Id == item.Id);
                                if (model != null) _context.Remove(model);
                            }
                        }

                        _context.Entry(objAsal).State = EntityState.Detached;
                    }

                    konfigPerubahanEkuiti.JKonfigPerubahanEkuitiBaris = _cart.JKonfigPerubahanEkuitiBaris?.ToList();
                    konfigPerubahanEkuiti.UserIdKemaskini = user?.UserName ?? "";

                    konfigPerubahanEkuiti.TarKemaskini = DateTime.Now;
                    konfigPerubahanEkuiti.DPekerjaKemaskiniId = pekerjaId;

                    _unitOfWork.JKonfigPerubahanEkuitiRepo.Update(konfigPerubahanEkuiti);

                    _appLog.Insert("Ubah", tahunAsal ?? "",tahunAsal ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KonfigPerubahanEkuitiExists(konfigPerubahanEkuiti.Id))
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
            return View(konfigPerubahanEkuiti);
        }

        // GET: KonfigPerubahanEkuiti/Delete/5
        [Authorize(Policy = modul + "D")]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var konfigPerubahanEkuiti = _unitOfWork.JKonfigPerubahanEkuitiRepo.GetAllDetailsById((int)id);
            if (konfigPerubahanEkuiti == null)
            {
                return NotFound();
            }
            return View(konfigPerubahanEkuiti);
        }

        // POST: KonfigPerubahanEkuiti/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "D")]
        public async Task<IActionResult> DeleteConfirmed(int id, string syscode)
        {
            var konfigPerubahanEkuiti = _unitOfWork.JKonfigPerubahanEkuitiRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (konfigPerubahanEkuiti != null && konfigPerubahanEkuiti.Tahun != null)
            {
                konfigPerubahanEkuiti.UserIdKemaskini = user?.UserName ?? "";
                konfigPerubahanEkuiti.TarKemaskini = DateTime.Now;
                konfigPerubahanEkuiti.DPekerjaKemaskiniId = pekerjaId;

                _context.JKonfigPerubahanEkuiti.Remove(konfigPerubahanEkuiti);
                _appLog.Insert("Hapus", konfigPerubahanEkuiti.EnLajurJadual.GetDisplayName() + " - " + konfigPerubahanEkuiti.Tahun, konfigPerubahanEkuiti.EnLajurJadual.GetDisplayName(), id, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dihapuskan..!";
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = modul + "R")]
        public async Task<IActionResult> RollBack(int id, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            var obj = await _context.JKonfigPerubahanEkuiti.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.JKonfigPerubahanEkuiti.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.EnLajurJadual.GetDisplayName() + " - " + obj.Tahun, obj.EnLajurJadual.GetDisplayName(), id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index));
        }
        private bool KonfigPerubahanEkuitiExists(int id)
        {
            return _unitOfWork.JKonfigPerubahanEkuitiRepo.IsExist(b => b.Id == id);
        }

        private bool TahunEnJenisEkuitiKonfigPerubahanEkuitiExists(string tahun, EnJenisLajurJadualPerubahanEkuiti enJenisEkuiti)
        {
            return _unitOfWork.JKonfigPerubahanEkuitiRepo.IsExist(e => e.Tahun == tahun && e.EnLajurJadual == enJenisEkuiti);
        }

        public JsonResult SaveBaris(JKonfigPerubahanEkuitiBaris baris)
        {
            try
            {
                if (baris != null)
                {
                    var barisCart = _cart.JKonfigPerubahanEkuitiBaris.FirstOrDefault(b => b.EnBaris == baris.EnBaris && b.EnJenisOperasi == baris.EnJenisOperasi);

                    if (barisCart != null)
                    {
                        _cart.RemoveItemBaris(baris.EnBaris, baris.EnJenisOperasi);
                    }

                    baris.SetKodList = _unitOfWork.AkCartaRepo.GetSetOfCartaStringList(baris.IsPukal, baris.EnJenisCartaList, baris.IsKecuali, baris.KodList);

                    _cart.AddItemBaris(0, baris.EnBaris, baris.EnJenisOperasi, baris.IsPukal, baris.EnJenisCartaList, baris.IsKecuali, baris.KodList, baris.SetKodList);
                }
                return Json(new { result = "OK", record = EnBarisPerubahanEkuiti.BakiAwal });
            } catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }
        }
        public JsonResult GetItemsBasedOnYear(string? tahun, EnJenisLajurJadualPerubahanEkuiti? enJenisEkuiti)
        {
            try
            {
                var result = _unitOfWork.JKonfigPerubahanEkuitiRepo.GetAllDetailsByTahunOrJenisEkuiti(tahun, enJenisEkuiti);

                if (result.Id == 0)
                {
                    result = _unitOfWork.JKonfigPerubahanEkuitiRepo.GetAllDetailsByTahunOrJenisEkuiti((int.Parse(tahun ?? DateTime.Now.Year.ToString()) - 1).ToString(), enJenisEkuiti);
                    if (result.Id == 0)
                    {
                        result = _unitOfWork.JKonfigPerubahanEkuitiRepo.GetAllDetailsByTahunOrJenisEkuiti(tahun, null);
                        if ( result.Id == 0)
                        {
                            result = _unitOfWork.JKonfigPerubahanEkuitiRepo.GetAllDetailsByTahunOrJenisEkuiti((int.Parse(tahun ?? DateTime.Now.Year.ToString()) - 1).ToString(), null);
                        }
                    }
                }

                if(result != null)
                {
                    PopulateCartJKonfigPerubahanEkuitiFromDb(result);
                }

                return Json(new { result = "OK", record = result });

            } catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }
            

            
        }

        public JsonResult GetAllItemCartJKonfigPerubahanEkuiti()
        {

            try
            {
                List<JKonfigPerubahanEkuitiBaris> baris = _cart.JKonfigPerubahanEkuitiBaris.ToList();

                string barisBefore = "";

                foreach (var item in baris.OrderBy(b => b.EnBaris).ThenBy(b => b.EnJenisOperasi))
                {

                    string barisSentences = item.EnBaris.GetDisplayName();
                    if (barisSentences == barisBefore)
                    {
                        barisSentences = "";
                    }
                    string sentence = _unitOfWork.AkCartaRepo.FormulaInSentence(item.EnJenisOperasi, item.EnJenisCartaList, item.IsKecuali, item.KodList);

                    item.BarisDescription = barisSentences;
                    item.FormulaDescription = sentence;

                    barisBefore = item.EnBaris.GetDisplayName();
                }

                return Json(new { result = "OK", baris = baris.OrderBy(d => d.EnBaris).ThenBy(d => d.EnJenisOperasi) });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartJKonfigPerubahanEkuitiBaris(JKonfigPerubahanEkuitiBaris baris)
        {

            try
            {
                JKonfigPerubahanEkuitiBaris data = _cart.JKonfigPerubahanEkuitiBaris.FirstOrDefault(x => x.EnBaris == baris.EnBaris && x.EnJenisOperasi == baris.EnJenisOperasi) ?? new JKonfigPerubahanEkuitiBaris();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }


        public JsonResult SaveAnItemFromCartJKonfigPerubahanEkuitiBaris(JKonfigPerubahanEkuitiBaris baris)
        {
            try
            {
                var data = _cart.JKonfigPerubahanEkuitiBaris.FirstOrDefault(x => x.EnBaris == baris.EnBaris && x.EnJenisOperasi == baris.EnJenisOperasi);

                if (data != null)
                {
                    _cart.RemoveItemBaris(baris.EnBaris, baris.EnJenisOperasi);

                    baris.SetKodList = _unitOfWork.AkCartaRepo.GetSetOfCartaStringList( baris.IsPukal, baris.EnJenisCartaList, baris.IsKecuali, baris.KodList);

                    _cart.AddItemBaris(baris.JKonfigPerubahanEkuitiId,baris.EnBaris,baris.EnJenisOperasi, baris.IsPukal, baris.EnJenisCartaList,baris.IsKecuali,baris.KodList, baris.SetKodList);
                }
                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

    }
}
