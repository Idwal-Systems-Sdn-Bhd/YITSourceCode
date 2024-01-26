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
    public class JAgamaController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodJAgama;
        public const string namamodul = Modules.namaJAgama;

        private readonly _IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;

        public JAgamaController(
            _IUnitOfWork unitOfWork,
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _userManager = userManager;
            _appLog = appLog;
        }
        public IActionResult Index()
        {
            return View(_unitOfWork.JAgamaRepo.GetAll());
        }

        // GET: KW/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var agama = _unitOfWork.JAgamaRepo.GetById((int)id);
            if (agama == null)
            {
                return NotFound();
            }

            return View(agama);
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
        public async Task<IActionResult> Create(JAgama agama, string syscode)
        {
            if (agama.Perihal != null && PerihalBangsaExists(agama.Perihal) == false)
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    agama.UserId = user?.UserName ?? "";

                    agama.TarMasuk = DateTime.Now;
                    agama.DPekerjaMasukId = pekerjaId;

                    _context.Add(agama);
                    _appLog.Insert("Tambah", agama.Perihal ?? "", agama.Perihal ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya ditambah..!";
                    return RedirectToAction(nameof(Index));

                }
            }
            else
            {
                TempData[SD.Error] = "Perihal ini telah wujud..!";
            }

            return View(agama);
        }

        [Authorize(Roles = "SuperAdmin")]
        // GET: KW/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var agama = _unitOfWork.JAgamaRepo.GetById((int)id);
            if (agama == null)
            {
                return NotFound();
            }
            return View(agama);
        }

        // POST: KW/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, JAgama agama, string syscode)
        {
            if (id != agama.Id)
            {
                return NotFound();
            }

            if (agama.Perihal != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = await _context.JAgama.FirstOrDefaultAsync(x => x.Id == agama.Id);
                    var perihalAsal = objAsal?.Perihal;
                    agama.UserId = objAsal?.UserId ?? "";
                    agama.TarMasuk = objAsal?.TarMasuk ?? new DateTime();
                    agama.DPekerjaMasukId = objAsal?.DPekerjaMasukId;

                    _unitOfWork.JAgamaRepo.Detach(objAsal!);

                    agama.UserIdKemaskini = user?.UserName ?? "";

                    agama.TarKemaskini = DateTime.Now;
                    agama.DPekerjaKemaskiniId = pekerjaId;

                    _unitOfWork.JAgamaRepo.Update(agama);

                    _appLog.Insert("Ubah", perihalAsal + " -> " + agama.Perihal + ", ", agama.Perihal ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BangsaExists(agama.Id))
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
            return View(agama);
        }

        // GET: KW/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var agama = _unitOfWork.JAgamaRepo.GetById((int)id);
            if (agama == null)
            {
                return NotFound();
            }

            return View(agama);
        }

        // POST: KW/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string syscode)
        {
            var agama = _unitOfWork.JAgamaRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (agama != null && agama.Perihal != null)
            {
                agama.UserIdKemaskini = user?.UserName ?? "";
                agama.TarKemaskini = DateTime.Now;
                agama.DPekerjaKemaskiniId = pekerjaId;

                _context.JAgama.Remove(agama);
                _appLog.Insert("Hapus", agama.Perihal ?? "", agama.Perihal ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dihapuskan..!";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> RollBack(int id, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            var obj = await _context.JAgama.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.JAgama.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.Perihal ?? "", obj.Perihal ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index));
        }
        private bool BangsaExists(int id)
        {
            return _unitOfWork.JAgamaRepo.IsExist(b => b.Id == id);
        }

        private bool PerihalBangsaExists(string perihal)
        {
            return _unitOfWork.JAgamaRepo.IsExist(e => e.Perihal == perihal);
        }
    }
}
