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
    public class JPTJController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodJPTJ;
        public const string namamodul = Modules.namaJPTJ;

        private readonly _IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserManager<IdentityUser> _userManager;

        public JPTJController(
            _IUnitOfWork unitOfWork,
            ApplicationDbContext context,
            _AppLogIRepository<AppLog,int> appLog,
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
            return View(_unitOfWork.JPTJRepo.GetAllDetails());
        }

        // GET: PTJ/Details/5
        [Authorize(Policy = modul)]
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ptj = _unitOfWork.JPTJRepo.GetAllDetailsById((int)id);
            if (ptj == null)
            {
                return NotFound();
            }

            return View(ptj);
        }

        // GET: PTJ/Create
        [Authorize(Policy = modul + "C")]
        public IActionResult Create()
        {
            PopulateDropdownList();
            return View();
        }

        // POST: PTJ/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "C")]
        public async Task<IActionResult> Create(JPTJ ptj, string syscode)
        {
            if (ptj.Kod != null && PerihalPTJExists(ptj.Kod) == false)
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    ptj.UserId = user?.UserName ?? "";

                    ptj.TarMasuk = DateTime.Now;
                    ptj.DPekerjaMasukId = pekerjaId;

                    _context.Add(ptj);
                    _appLog.Insert("Tambah", ptj.Kod + " - " + ptj.Perihal, ptj.Kod, 0, 0, pekerjaId, modul, syscode, namamodul, user);
                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya ditambah..!";
                    return RedirectToAction(nameof(Index));

                }
            }
            else
            {
                TempData[SD.Error] = "Perihal ini telah wujud..!";
            }

            PopulateDropdownList();
            return View(ptj);
        }

        // GET: PTJ/Edit/5
        [Authorize(Policy = modul + "E")]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ptj = _unitOfWork.JPTJRepo.GetAllDetailsById((int)id);
            if (ptj == null)
            {
                return NotFound();
            }
            PopulateDropdownList();
            return View(ptj);
        }

        // POST: PTJ/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "E")]
        public async Task<IActionResult> Edit(int id, JPTJ ptj, string syscode)
        {
            if (id != ptj.Id)
            {
                return NotFound();
            }

            if (ptj.Kod != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = await _context.JPTJ.FirstOrDefaultAsync(x => x.Id == ptj.Id);
                    var kodAsal = objAsal!.Kod;
                    var perihalAsal = objAsal.Perihal;
                    ptj.UserId = objAsal.UserId;
                    ptj.TarMasuk = objAsal.TarMasuk;
                    ptj.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                    _context.Entry(objAsal).State = EntityState.Detached;

                    ptj.UserIdKemaskini = user?.UserName ?? "";

                    ptj.TarKemaskini = DateTime.Now;
                    ptj.DPekerjaKemaskiniId = pekerjaId;

                    _unitOfWork.JPTJRepo.Update(ptj);

                    _appLog.Insert("Ubah", kodAsal + " -> " + ptj.Kod + ", " + perihalAsal + " -> " + ptj.Perihal + ", ", ptj.Kod, id, 0, pekerjaId, modul, syscode, namamodul, user);

                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PTJExists(ptj.Id))
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
            return View(ptj);
        }

        // GET: PTJ/Delete/5
        [Authorize(Policy = modul + "D")]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ptj = _unitOfWork.JPTJRepo.GetAllDetailsById((int)id);
            if (ptj == null)
            {
                return NotFound();
            }

            return View(ptj);
        }

        // POST: PTJ/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "D")]
        public async Task<IActionResult> DeleteConfirmed(int id, string syscode)
        {
            var ptj = _unitOfWork.JPTJRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (ptj != null && ptj.Kod != null)
            {
                ptj.UserIdKemaskini = user?.UserName ?? "";
                ptj.TarKemaskini = DateTime.Now;
                ptj.DPekerjaKemaskiniId = pekerjaId;

                _context.JPTJ.Remove(ptj);
                _appLog.Insert("Hapus", ptj.Kod + " - " + ptj.Perihal, ptj.Kod, id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var obj = await _context.JPTJ.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.JPTJ.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.Kod + " - " + obj.Perihal, obj.Kod ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index));
        }
        private bool PTJExists(int id)
        {
            return _unitOfWork.JPTJRepo.IsExist(b => b.Id == id);
        }

        private bool PerihalPTJExists(string perihal)
        {
            return _unitOfWork.JPTJRepo.IsExist(e => e.Perihal == perihal);
        }
        public void PopulateDropdownList()
        {
            ViewBag.JKW = _unitOfWork.JKWRepo.GetAll();
        }
    }
}
