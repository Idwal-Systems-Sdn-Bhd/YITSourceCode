using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT.Akaun.Controllers._01Jadual
{
    [Authorize]
    public class JKonfigPerubahanEkuitiController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodJKonfigPerubahanEkuiti;
        public const string namamodul = Modules.namaJKonfigPerubahanEkuiti;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserManager<IdentityUser> _userManager;

        public JKonfigPerubahanEkuitiController(
            _IUnitOfWork unitOfWork,
            ApplicationDbContext context,
            _AppLogIRepository<AppLog, int> appLog,
            UserManager<IdentityUser> userManager
            )
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _appLog = appLog;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View(_unitOfWork.JKonfigPerubahanEkuitiRepo.GetAllDetails());
        }

        // GET: KonfigPerubahanEkuiti/Details/5
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
        public IActionResult Create()
        {
            PopulateDropdownList();
            return View();
        }

        public void PopulateDropdownList()
        {
            ViewBag.JKW = _unitOfWork.JKWRepo.GetAll();
        }

        // POST: KonfigPerubahanEkuiti/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JKonfigPerubahanEkuiti konfigPerubahanEkuiti, string syscode)
        {
            if (konfigPerubahanEkuiti.Tahun != null && TahunJKWKonfigPerubahanEkuitiExists(konfigPerubahanEkuiti.Tahun, konfigPerubahanEkuiti.JKWId) == false)
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    konfigPerubahanEkuiti.UserId = user?.UserName ?? "";

                    konfigPerubahanEkuiti.TarMasuk = DateTime.Now;
                    konfigPerubahanEkuiti.DPekerjaMasukId = pekerjaId;

                    var jkw = _unitOfWork.JKWRepo.GetById(konfigPerubahanEkuiti.JKWId);

                    _context.Add(konfigPerubahanEkuiti);
                    _appLog.Insert("Tambah", jkw.Kod + " - " + konfigPerubahanEkuiti.Tahun, jkw.Kod ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
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

        [Authorize(Roles = "SuperAdmin")]
        // GET: KonfigPerubahanEkuiti/Edit/5
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
            return View(konfigPerubahanEkuiti);
        }

        // POST: KonfigPerubahanEkuiti/Edit/5
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
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

                    var objAsal = await _context.JKonfigPerubahanEkuiti.FirstOrDefaultAsync(x => x.Id == konfigPerubahanEkuiti.Id);
                    var tahunAsal = objAsal?.Tahun;
                    if (objAsal != null)
                    {
                        konfigPerubahanEkuiti.UserId = objAsal.UserId;
                        konfigPerubahanEkuiti.TarMasuk = objAsal.TarMasuk;
                        konfigPerubahanEkuiti.DPekerjaMasukId = objAsal.DPekerjaMasukId;
                        _context.Entry(objAsal).State = EntityState.Detached;
                    }

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
                var jkw = _unitOfWork.JKWRepo.GetById(konfigPerubahanEkuiti.JKWId);
                _context.JKonfigPerubahanEkuiti.Remove(konfigPerubahanEkuiti);
                _appLog.Insert("Hapus", jkw.Kod + " - " + konfigPerubahanEkuiti.Tahun, jkw?.Kod ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dihapuskan..!";
            }

            return RedirectToAction(nameof(Index));
        }

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
                var jkw = _unitOfWork.JKWRepo.GetById(obj.JKWId);

                // Batal operation end
                _appLog.Insert("Rollback", jkw.Kod + " - " + obj.Tahun, jkw.Kod ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index));
        }
        private bool KonfigPerubahanEkuitiExists(int id)
        {
            return _unitOfWork.JKonfigPerubahanEkuitiRepo.IsExist(b => b.Id == id);
        }

        private bool TahunJKWKonfigPerubahanEkuitiExists(string tahun, int JKWId)
        {
            return _unitOfWork.JKonfigPerubahanEkuitiRepo.IsExist(e => e.Tahun == tahun && e.JKWId == JKWId);
        }

        public JsonResult GetBakiAwalFromPreviousYear(string? tahun, int? JKWId)
        {
            try
            {
                string tahunLepas = (int.Parse(tahun ?? DateTime.Now.ToString("yyyy")) - 1).ToString();
                var konfigPerubahanEkuitiTahunLepas = _unitOfWork.JKonfigPerubahanEkuitiRepo.GetAllDetailsByTahunAndJKW(tahunLepas,JKWId);
                decimal bakiAwal = 0;
                if (konfigPerubahanEkuitiTahunLepas != null)
                {
                    bakiAwal = konfigPerubahanEkuitiTahunLepas.BakiAkhir;

                }

                return Json(new { result = "OK", bakiAwal });
            }
            catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }
        }
    }
}
