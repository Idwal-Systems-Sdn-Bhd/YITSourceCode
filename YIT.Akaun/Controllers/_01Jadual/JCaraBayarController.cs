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
    [Authorize(Roles = "SuperAdmin,Supervisor")]
    public class JCaraBayarController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodJCaraBayar;
        public const string namamodul = Modules.namaJCaraBayar;

        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;

        public JCaraBayarController(ApplicationDbContext context,
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
            return View(_unitOfWork.JCaraBayarRepo.GetAll());
        }

        // GET: CaraBayar/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caraBayar = _unitOfWork.JCaraBayarRepo.GetById((int)id);
            if (caraBayar == null)
            {
                return NotFound();
            }

            return View(caraBayar);
        }

        // GET: CaraBayar/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CaraBayar/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JCaraBayar caraBayar, string syscode)
        {
            if (caraBayar.Kod != null && KodCaraBayarExists(caraBayar.Kod) == false)
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    caraBayar.UserId = user?.UserName ?? "";

                    caraBayar.TarMasuk = DateTime.Now;
                    caraBayar.DPekerjaMasukId = pekerjaId;

                    _context.Add(caraBayar);
                    _appLog.Insert("Tambah", caraBayar.Kod + " - " + caraBayar.Perihal, caraBayar.Kod, 0, 0, pekerjaId, modul, syscode, namamodul, user);
                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya ditambah..!";
                    return RedirectToAction(nameof(Index));

                }
            }
            else
            {
                TempData[SD.Error] = "Kod ini telah wujud..!";
            }

            return View(caraBayar);
        }

        [Authorize(Roles = "SuperAdmin")]
        // GET: CaraBayar/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caraBayar = _unitOfWork.JCaraBayarRepo.GetById((int)id);
            if (caraBayar == null)
            {
                return NotFound();
            }
            return View(caraBayar);
        }

        // POST: CaraBayar/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, JCaraBayar caraBayar, string syscode)
        {
            if (id != caraBayar.Id)
            {
                return NotFound();
            }

            if (caraBayar.Kod != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = await _context.JCaraBayar.FirstOrDefaultAsync(x => x.Id == caraBayar.Id);
                    var kodAsal = objAsal!.Kod;
                    var perihalAsal = objAsal.Perihal;
                    caraBayar.UserId = objAsal.UserId;
                    caraBayar.TarMasuk = objAsal.TarMasuk;
                    caraBayar.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                    _context.Entry(objAsal).State = EntityState.Detached;

                    caraBayar.UserIdKemaskini = user?.UserName ?? "";

                    caraBayar.TarKemaskini = DateTime.Now;
                    caraBayar.DPekerjaKemaskiniId = pekerjaId;

                    _unitOfWork.JCaraBayarRepo.Update(caraBayar);

                    _appLog.Insert("Ubah", kodAsal + " -> " + caraBayar.Kod + ", " + perihalAsal + " -> " + caraBayar.Perihal + ", ", caraBayar.Kod, id, 0, pekerjaId, modul, syscode, namamodul, user);

                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CaraBayarExists(caraBayar.Id))
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
            return View(caraBayar);
        }

        // GET: CaraBayar/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caraBayar = _unitOfWork.JCaraBayarRepo.GetById((int)id);
            if (caraBayar == null)
            {
                return NotFound();
            }

            return View(caraBayar);
        }

        // POST: CaraBayar/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string syscode)
        {
            var caraBayar = _unitOfWork.JCaraBayarRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (caraBayar != null && caraBayar.Kod != null)
            {
                caraBayar.UserIdKemaskini = user?.UserName ?? "";
                caraBayar.TarKemaskini = DateTime.Now;
                caraBayar.DPekerjaKemaskiniId = pekerjaId;

                _context.JCaraBayar.Remove(caraBayar);
                _appLog.Insert("Hapus", caraBayar.Kod + " - " + caraBayar.Perihal, caraBayar.Kod, id, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dihapuskan..!";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> RollBack(int id, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            var obj = await _context.JCaraBayar.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.JCaraBayar.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.Kod + " - " + obj.Perihal, obj.Kod ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index));
        }
        private bool CaraBayarExists(int id)
        {
            return _unitOfWork.JCaraBayarRepo.IsExist(b => b.Id == id);
        }

        private bool KodCaraBayarExists(string kod)
        {
            return _unitOfWork.JCaraBayarRepo.IsExist(e => e.Kod == kod);
        }
    }
}
