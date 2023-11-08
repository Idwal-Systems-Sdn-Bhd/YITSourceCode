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
    public class JBahagianController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = "JD003";
        public const string namamodul = "Jadual Bahagian";

        private readonly _IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserManager<IdentityUser> _userManager;

        public JBahagianController(
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
            return View(_unitOfWork.JBahagianRepo.GetAllDetails());
        }

        // GET: Bahagian/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bahagian = _unitOfWork.JBahagianRepo.GetAllDetailsById((int)id);
            if (bahagian == null)
            {
                return NotFound();
            }

            return View(bahagian);
        }

        // GET: Bahagian/Create
        public IActionResult Create()
        {
            PopulateDropdownList();
            return View();
        }

        // POST: Bahagian/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JBahagian bahagian, string syscode)
        {
            if (bahagian.Kod != null && KodBahagianExists(bahagian.Kod) == false)
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    bahagian.UserId = user?.UserName ?? "";

                    bahagian.TarMasuk = DateTime.Now;
                    bahagian.DPekerjaMasukId = pekerjaId;

                    _context.Add(bahagian);
                    _appLog.Insert("Tambah", bahagian.Kod + " - " + bahagian.Perihal, bahagian.Kod, 0, 0, pekerjaId, modul, syscode, namamodul, user);
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
            return View(bahagian);
        }

        [Authorize(Roles = "SuperAdmin")]
        // GET: Bahagian/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bahagian = _unitOfWork.JBahagianRepo.GetAllDetailsById((int)id);
            if (bahagian == null)
            {
                return NotFound();
            }
            PopulateDropdownList();
            return View(bahagian);
        }

        // POST: Bahagian/Edit/5
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, JBahagian bahagian, string syscode)
        {
            if (id != bahagian.Id)
            {
                return NotFound();
            }

            if (bahagian.Kod != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = await _context.JBahagian.FirstOrDefaultAsync(x => x.Id == bahagian.Id);
                    var kodAsal = objAsal!.Kod;
                    var perihalAsal = objAsal.Perihal;
                    bahagian.UserId = objAsal.UserId;
                    bahagian.TarMasuk = objAsal.TarMasuk;
                    bahagian.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                    _context.Entry(objAsal).State = EntityState.Detached;

                    bahagian.UserIdKemaskini = user?.UserName ?? "";

                    bahagian.TarKemaskini = DateTime.Now;
                    bahagian.DPekerjaKemaskiniId = pekerjaId;

                    _unitOfWork.JBahagianRepo.Update(bahagian);

                    _appLog.Insert("Ubah", kodAsal + " -> " + bahagian.Kod + ", " + perihalAsal + " -> " + bahagian.Perihal + ", ", bahagian.Kod, id, 0, pekerjaId, modul, syscode, namamodul, user);

                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BahagianExists(bahagian.Id))
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
            return View(bahagian);
        }

        // GET: Bahagian/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bahagian = _unitOfWork.JBahagianRepo.GetAllDetailsById((int)id);
            if (bahagian == null)
            {
                return NotFound();
            }

            return View(bahagian);
        }

        // POST: Bahagian/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string syscode)
        {
            var bahagian = _unitOfWork.JBahagianRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (bahagian != null && bahagian.Kod != null)
            {
                bahagian.UserIdKemaskini = user?.UserName ?? "";
                bahagian.TarKemaskini = DateTime.Now;
                bahagian.DPekerjaKemaskiniId = pekerjaId;

                _context.JBahagian.Remove(bahagian);
                _appLog.Insert("Hapus", bahagian.Kod + " - " + bahagian.Perihal, bahagian.Kod, id, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dihapuskan..!";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> RollBack(int id, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            var obj = await _context.JBahagian.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.JBahagian.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.Kod + " - " + obj.Perihal, obj.Kod ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index));
        }
        private bool BahagianExists(int id)
        {
            return _unitOfWork.JBahagianRepo.IsExist(b => b.Id == id);
        }

        private bool KodBahagianExists(string kod)
        {
            return _unitOfWork.JBahagianRepo.IsExist(e => e.Kod == kod);
        }
        public void PopulateDropdownList()
        {
            ViewBag.JPTJ = _unitOfWork.JPTJRepo.GetAll();
        }
    }
}
