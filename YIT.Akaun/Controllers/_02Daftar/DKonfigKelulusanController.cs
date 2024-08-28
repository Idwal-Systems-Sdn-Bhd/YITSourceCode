using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT.Akaun.Microservices;

namespace YIT.Akaun.Controllers._02Daftar
{
    
    [Authorize(Roles = Init.superAdminSupervisorRole)]
    public class DKonfigKelulusanController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodDKonfigKelulusan;
        public const string namamodul = Modules.namaDKonfigKelulusan;

        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserManager<IdentityUser> _userManager;

        public DKonfigKelulusanController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            _AppLogIRepository<AppLog, int> appLog,
            UserManager<IdentityUser> userManager
            )
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _appLog = appLog;
            _userManager = userManager;
        }
        public IActionResult Index(
           string searchString,
           string searchColumn
           )
        {
            if (searchString == null)
            {
                HttpContext.Session.Clear();
                return View();
            }

            SaveFormFields(searchString);

            var dKK = _unitOfWork.DKonfigKelulusanRepo.GetResults(searchString, searchColumn);

            return View(dKK);
        }

        private void SaveFormFields(string? searchString)
        {
            PopulateFormFields(searchString);

            if (searchString != null)
            {
                HttpContext.Session.SetString("searchString", searchString);
            }
            else
            {
                searchString = HttpContext.Session.GetString("searchString");
                ViewBag.searchString = searchString;
            }
        }

        private void PopulateFormFields(string? searchString)
        {
            ViewBag.searchString = searchString;
        }

        [Authorize(Policy = modul + "C")]
        public IActionResult Create()
        {
            PopulateDropdownList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "C")]
        public async Task<IActionResult> Create(DKonfigKelulusan konfigKelulusan, string syscode)
        {
            if (KonfigKelulusanExists(konfigKelulusan.DPekerjaId,konfigKelulusan.EnKategoriKelulusan,konfigKelulusan.EnJenisModulKelulusan,konfigKelulusan.JBahagianId) == true)
            {
                TempData[SD.Error] = "Data ini telah wujud..!";
                PopulateDropdownList();
                return View(konfigKelulusan);
            }

            else if (ModelState.IsValid)
            {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;
                    konfigKelulusan.UserId = user?.UserName ?? "";

                    konfigKelulusan.TarMasuk = DateTime.Now;
                    konfigKelulusan.DPekerjaMasukId = pekerjaId;

                    _context.Add(konfigKelulusan);
                    _appLog.Insert("Tambah", konfigKelulusan.DPekerjaId.ToString() + " - " + konfigKelulusan.Tandatangan, konfigKelulusan.DPekerjaId.ToString(), 0, 0, pekerjaId, modul, syscode, namamodul, user);
                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya ditambah..!";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString")});
            }
            //TempData[SD.Error] = "Data ini telah wujud..!";
            PopulateDropdownList();
            return View(konfigKelulusan);
        }

        [Authorize(Policy = modul)]
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var konfigKelulusan = _unitOfWork.DKonfigKelulusanRepo.GetAllDetailsById((int)id);
            if (konfigKelulusan == null)
            {
                return NotFound();
            }

            return View(konfigKelulusan);
        }

        [Authorize(Policy = modul + "E")]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var konfigKelulusan = _unitOfWork.DKonfigKelulusanRepo.GetAllDetailsById((int)id);
            if (konfigKelulusan == null)
            {
                return NotFound();
            }
            PopulateDropdownList();
            return View(konfigKelulusan);
        }
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "E")]
        public async Task<IActionResult> Edit(int id, DKonfigKelulusan konfigKelulusan, string syscode)
        {
            if (id != konfigKelulusan.Id)
            {
                return NotFound();
            }

            if (konfigKelulusan.DPekerjaId != 0 && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = await _context.DKonfigKelulusan.FirstOrDefaultAsync(x => x.Id == konfigKelulusan.Id)!;
                    if (objAsal != null)
                    {
                        konfigKelulusan.UserId = objAsal.UserId;
                        konfigKelulusan.TarMasuk = objAsal.TarMasuk;
                        konfigKelulusan.DPekerjaMasukId = objAsal?.DPekerjaMasukId;

                        _context.Entry(objAsal!).State = EntityState.Detached;

                        konfigKelulusan.UserIdKemaskini = user?.UserName ?? "";

                        konfigKelulusan.TarKemaskini = DateTime.Now;
                        konfigKelulusan.DPekerjaKemaskiniId = pekerjaId;

                        _unitOfWork.DKonfigKelulusanRepo.Update(konfigKelulusan);

                        _appLog.Insert("Ubah", konfigKelulusan.Id.ToString() + " - " + konfigKelulusan.DPekerjaId, konfigKelulusan?.Id.ToString() ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                        await _context.SaveChangesAsync();
                        TempData[SD.Success] = "Data berjaya diubah..!";
                    }

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KonfigKelulusanExists(konfigKelulusan.Id, konfigKelulusan.EnKategoriKelulusan, konfigKelulusan.EnJenisModulKelulusan, konfigKelulusan.JBahagianId))
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
            PopulateDropdownList();
            return View(konfigKelulusan);
        }

        // GET: Konfig/Delete/5
        [Authorize(Policy = modul + "D")]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var konfigKelulusan = _unitOfWork.DKonfigKelulusanRepo.GetAllDetailsById((int)id);
            if (konfigKelulusan == null)
            {
                return NotFound();
            }

            return View(konfigKelulusan);
        }

        // GET: Konfig/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "D")]
        public async Task<IActionResult> DeleteConfirmed(int id, string syscode)
        {
            var konfigKelulusan = _unitOfWork.DKonfigKelulusanRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (konfigKelulusan != null && konfigKelulusan.DPekerjaId != 0)
            {
                konfigKelulusan.UserIdKemaskini = user?.UserName ?? "";
                konfigKelulusan.TarKemaskini = DateTime.Now;
                konfigKelulusan.DPekerjaKemaskiniId = pekerjaId;

                _context.DKonfigKelulusan.Remove(konfigKelulusan);
                _appLog.Insert("Hapus", konfigKelulusan.DPekerjaId.ToString() + " - " + konfigKelulusan.JBahagianId, konfigKelulusan?.DPekerjaId.ToString() ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
                //_appLog.Insert("Hapus", konfigKelulusan.Id.ToString() + " - " + konfigKelulusan.DPekerjaId, konfigKelulusan?.Id.ToString() ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dihapuskan..!";
                
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString") });
        }

        [Authorize(Policy = modul + "R")]
        public async Task<IActionResult> RollBack(int id, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            var obj = await _context.DKonfigKelulusan.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.DKonfigKelulusan.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.Id.ToString() + " - " + obj.DPekerjaId, obj.Id.ToString() ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString") });
        }

        private bool KonfigKelulusanExists(int dPekerjaId, EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan, int? jBahagianId)
        {
            return _unitOfWork.DKonfigKelulusanRepo
            .IsExist(p => p.DPekerjaId == dPekerjaId && p.EnKategoriKelulusan == enKategoriKelulusan && p.EnJenisModulKelulusan == enJenisModulKelulusan && p.JBahagianId == jBahagianId);
        }

        private void PopulateDropdownList()
        {

            ViewBag.JBahagian = _unitOfWork.JBahagianRepo.GetAll();

            var kategoriKelulusan = EnumHelper<EnKategoriKelulusan>.GetList();

            ViewBag.EnKategoriKelulusan = kategoriKelulusan;

            var jenisModulKelulusan = EnumHelper<EnJenisModulKelulusan>.GetList();

            ViewBag.EnJenisModulKelulusan = jenisModulKelulusan;

        }
    }
}
