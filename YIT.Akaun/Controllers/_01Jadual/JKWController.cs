using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Implementations;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT.Akaun.Controllers._01Jadual
{
    [Authorize(Roles = "SuperAdmin,Supervisor")]
    public class JKWController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = "JD001";
        public const string namamodul = "Jadual Kumpulan Wang";

        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;

        public JKWController(ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _appLog = appLog;
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

            var kW = _unitOfWork.JKWRepo.GetById((int)id);
            if (kW == null)
            {
                return NotFound();
            }

            return View(kW);
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

            var kW = _unitOfWork.JKWRepo.GetById((int)id);
            if (kW == null)
            {
                return NotFound();
            }
            return View(kW);
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
    }
}
