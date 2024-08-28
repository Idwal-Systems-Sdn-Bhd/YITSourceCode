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
    
    [Authorize(Roles = Init.superAdminSupervisorRole)]
    public class JNegeriController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodJNegeri;
        public const string namamodul = Modules.namaJNegeri;

        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;

        public JNegeriController(ApplicationDbContext context,
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
            return View(_unitOfWork.JNegeriRepo.GetAll());
        }

        // GET: Negeri/Details/5
        [Authorize(Policy = modul)]
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var negeri = _unitOfWork.JNegeriRepo.GetById((int)id);
            if (negeri == null)
            {
                return NotFound();
            }

            return View(negeri);
        }

        // GET: Negeri/Create
        [Authorize(Policy = modul + "C")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Negeri/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "C")]
        public async Task<IActionResult> Create(JNegeri negeri, string syscode)
        {
            if (negeri.Kod != null && KodNegeriExists(negeri.Kod) == false)
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    negeri.UserId = user?.UserName ?? "";

                    negeri.TarMasuk = DateTime.Now;
                    negeri.DPekerjaMasukId = pekerjaId;

                    _context.Add(negeri);
                    _appLog.Insert("Tambah", negeri.Kod + " - " + negeri.Perihal, negeri.Kod, 0, 0, pekerjaId, modul, syscode, namamodul, user);
                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya ditambah..!";
                    return RedirectToAction(nameof(Index));

                }
            }
            else
            {
                TempData[SD.Error] = "Kod ini telah wujud..!";
            }

            return View(negeri);
        }

        // GET: Negeri/Edit/5
        [Authorize(Policy = modul + "E")]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var negeri = _unitOfWork.JNegeriRepo.GetById((int)id);
            if (negeri == null)
            {
                return NotFound();
            }
            return View(negeri);
        }

        // POST: Negeri/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "E")]
        public async Task<IActionResult> Edit(int id, JNegeri negeri, string syscode)
        {
            if (id != negeri.Id)
            {
                return NotFound();
            }

            if (negeri.Kod != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = await _context.JNegeri.FirstOrDefaultAsync(x => x.Id == negeri.Id);
                    var kodAsal = objAsal!.Kod;
                    var perihalAsal = objAsal.Perihal;
                    negeri.UserId = objAsal.UserId;
                    negeri.TarMasuk = objAsal.TarMasuk;
                    negeri.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                    _context.Entry(objAsal).State = EntityState.Detached;

                    negeri.UserIdKemaskini = user?.UserName ?? "";

                    negeri.TarKemaskini = DateTime.Now;
                    negeri.DPekerjaKemaskiniId = pekerjaId;

                    _unitOfWork.JNegeriRepo.Update(negeri);

                    _appLog.Insert("Ubah", kodAsal + " -> " + negeri.Kod + ", " + perihalAsal + " -> " + negeri.Perihal + ", ", negeri.Kod, id, 0, pekerjaId, modul, syscode, namamodul, user);

                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NegeriExists(negeri.Id))
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
            return View(negeri);
        }

        // GET: Negeri/Delete/5
        [Authorize(Policy = modul + "D")]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var negeri = _unitOfWork.JNegeriRepo.GetById((int)id);
            if (negeri == null)
            {
                return NotFound();
            }

            return View(negeri);
        }

        // POST: Negeri/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "D")]
        public async Task<IActionResult> DeleteConfirmed(int id, string syscode)
        {
            var negeri = _unitOfWork.JNegeriRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (negeri != null && negeri.Kod != null)
            {
                negeri.UserIdKemaskini = user?.UserName ?? "";
                negeri.TarKemaskini = DateTime.Now;
                negeri.DPekerjaKemaskiniId = pekerjaId;

                _context.JNegeri.Remove(negeri);
                _appLog.Insert("Hapus", negeri.Kod + " - " + negeri.Perihal, negeri.Kod, id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var obj = await _context.JNegeri.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.JNegeri.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.Kod + " - " + obj.Perihal, obj.Kod ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index));
        }
        private bool NegeriExists(int id)
        {
            return _unitOfWork.JNegeriRepo.IsExist(b => b.Id == id);
        }

        private bool KodNegeriExists(string kod)
        {
            return _unitOfWork.JNegeriRepo.IsExist(e => e.Kod == kod);
        }
    }
}
