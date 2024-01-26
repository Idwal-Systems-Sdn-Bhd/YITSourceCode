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
    [Authorize]
    public class JBangsaController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodJBangsa;
        public const string namamodul = Modules.namaJBangsa;

        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;

        public JBangsaController(ApplicationDbContext context,
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
            return View(_unitOfWork.JBangsaRepo.GetAll());
        }

        // GET: KW/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bangsa = _unitOfWork.JBangsaRepo.GetById((int)id);
            if (bangsa == null)
            {
                return NotFound();
            }

            return View(bangsa);
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
        public async Task<IActionResult> Create(JBangsa bangsa, string syscode)
        {
            if (bangsa.Perihal != null && PerihalBangsaExists(bangsa.Perihal) == false)
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    bangsa.UserId = user?.UserName ?? "";

                    bangsa.TarMasuk = DateTime.Now;
                    bangsa.DPekerjaMasukId = pekerjaId;

                    _context.Add(bangsa);
                    _appLog.Insert("Tambah", bangsa.Perihal ?? "", bangsa.Perihal ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya ditambah..!";
                    return RedirectToAction(nameof(Index));

                }
            }
            else
            {
                TempData[SD.Error] = "Perihal ini telah wujud..!";
            }

            return View(bangsa);
        }

        [Authorize(Roles = "SuperAdmin")]
        // GET: KW/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bangsa = _unitOfWork.JBangsaRepo.GetById((int)id);
            if (bangsa == null)
            {
                return NotFound();
            }
            return View(bangsa);
        }

        // POST: KW/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, JBangsa bangsa, string syscode)
        {
            if (id != bangsa.Id)
            {
                return NotFound();
            }

            if (bangsa.Perihal != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = await _context.JBangsa.FirstOrDefaultAsync(x => x.Id == bangsa.Id);
                    var perihalAsal = objAsal?.Perihal;
                    bangsa.UserId = objAsal?.UserId ?? "";
                    bangsa.TarMasuk = objAsal?.TarMasuk ?? new DateTime();
                    bangsa.DPekerjaMasukId = objAsal?.DPekerjaMasukId;

                    _unitOfWork.JBangsaRepo.Detach(objAsal!);

                    bangsa.UserIdKemaskini = user?.UserName ?? "";

                    bangsa.TarKemaskini = DateTime.Now;
                    bangsa.DPekerjaKemaskiniId = pekerjaId;

                    _unitOfWork.JBangsaRepo.Update(bangsa);

                    _appLog.Insert("Ubah", perihalAsal + " -> " + bangsa.Perihal + ", ", bangsa.Perihal ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BangsaExists(bangsa.Id))
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
            return View(bangsa);
        }

        // GET: KW/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bangsa = _unitOfWork.JBangsaRepo.GetById((int)id);
            if (bangsa == null)
            {
                return NotFound();
            }

            return View(bangsa);
        }

        // POST: KW/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string syscode)
        {
            var bangsa = _unitOfWork.JBangsaRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (bangsa != null && bangsa.Perihal != null)
            {
                bangsa.UserIdKemaskini = user?.UserName ?? "";
                bangsa.TarKemaskini = DateTime.Now;
                bangsa.DPekerjaKemaskiniId = pekerjaId;

                _context.JBangsa.Remove(bangsa);
                _appLog.Insert("Hapus", bangsa.Perihal ?? "", bangsa.Perihal ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dihapuskan..!";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> RollBack(int id, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            var obj = await _context.JBangsa.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.JBangsa.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.Perihal ?? "", obj.Perihal ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index));
        }
        private bool BangsaExists(int id)
        {
            return _unitOfWork.JBangsaRepo.IsExist(b => b.Id == id);
        }

        private bool PerihalBangsaExists(string perihal)
        {
            return _unitOfWork.JBangsaRepo.IsExist(e => e.Perihal == perihal);
        }
    }
}
