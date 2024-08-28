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
    public class JCawanganController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodJCawangan;
        public const string namamodul = Modules.namaJCawangan;

        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;

        public JCawanganController(ApplicationDbContext context,
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
            PopulateDropdownList();
            return View(_unitOfWork.JCawanganRepo.GetAll());
        }

        // GET: JCawangan/Details/5
        [Authorize(Policy = modul)]
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var jc = _unitOfWork.JCawanganRepo.GetAllDetailsById((int)id);
            if (jc == null)
            {
                return NotFound();
            }
            return View(jc);
        }

        // GET: jCawangan/Create
        [Authorize(Policy = modul + "C")]
        public IActionResult Create()
        {
            PopulateDropdownList();
            return View();
        }

        // POST: KW/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "C")]
        public async Task<IActionResult> Create(JCawangan jCawangan, string syscode)
        {
            if (jCawangan.Kod != null && KodJCawanganExists(jCawangan.Kod) == false)
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    if (jCawangan.AkBankId == 0)
                    {
                        jCawangan.AkBankId = null;
                    }

                    jCawangan.UserId = user?.UserName ?? "";

                    jCawangan.TarMasuk = DateTime.Now;
                    jCawangan.DPekerjaMasukId = pekerjaId;

                    _context.Add(jCawangan);
                    _appLog.Insert("Tambah", jCawangan.Kod + " - " + jCawangan.Perihal, jCawangan.Kod, 0, 0, pekerjaId, modul, syscode, namamodul, user);
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
            return View(jCawangan);
        }

        // GET: jCawangan/Edit/5
        [Authorize(Policy = modul + "E")]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var jCawangan = _unitOfWork.JCawanganRepo.GetById((int)id);
            if (jCawangan == null)
            {
                return NotFound();
            }
            PopulateDropdownList();
            return View(jCawangan);
        }

        // POST: jCawangan/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "E")]
        public async Task<IActionResult> Edit(int id, JCawangan jCawangan, string syscode)
        {
            if (id != jCawangan.Id)
            {
                return NotFound();
            }

            if (jCawangan.Kod != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = await _context.JCawangan.FirstOrDefaultAsync(x => x.Id == jCawangan.Id);
                    var kodAsal = objAsal!.Kod;
                    var perihalAsal = objAsal.Perihal;
                    jCawangan.UserId = objAsal.UserId;
                    jCawangan.TarMasuk = objAsal.TarMasuk;
                    jCawangan.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                    _context.Entry(objAsal).State = EntityState.Detached;

                    if (jCawangan.AkBankId == 0)
                    {
                        jCawangan.AkBankId = null;
                    }

                    jCawangan.UserIdKemaskini = user?.UserName ?? "";

                    jCawangan.TarKemaskini = DateTime.Now;
                    jCawangan.DPekerjaKemaskiniId = pekerjaId;

                    _unitOfWork.JCawanganRepo.Update(jCawangan);

                    _appLog.Insert("Ubah", kodAsal + " -> " + jCawangan.Kod + ", " + perihalAsal + " -> " + jCawangan.Perihal + ", ", jCawangan.Kod, id, 0, pekerjaId, modul, syscode, namamodul, user);

                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JCawanganExists(jCawangan.Id))
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
            return View(jCawangan);
        }

        // GET: jCawangan/Delete/5
        [Authorize(Policy = modul + "D")]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var jc = _unitOfWork.JCawanganRepo.GetAllDetailsById((int)id);
            if (jc == null)
            {
                return NotFound();
            }
            return View(jc);
        }

        // POST: jCawangan/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "D")]
        public async Task<IActionResult> DeleteConfirmed(int id, string syscode)
        {
            var jCawangan = _unitOfWork.JCawanganRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (jCawangan != null && jCawangan.Kod != null)
            {
                jCawangan.UserIdKemaskini = user?.UserName ?? "";
                jCawangan.TarKemaskini = DateTime.Now;
                jCawangan.DPekerjaKemaskiniId = pekerjaId;

                _context.JCawangan.Remove(jCawangan);
                _appLog.Insert("Hapus", jCawangan.Kod + " - " + jCawangan.Perihal, jCawangan.Kod, id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var obj = await _context.JCawangan.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.JCawangan.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.Kod + " - " + obj.Perihal, obj.Kod ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index));
        }
        private bool JCawanganExists(int id)
        {
            return _unitOfWork.JCawanganRepo.IsExist(b => b.Id == id);
        }

        private bool KodJCawanganExists(string kod)
        {
            return _unitOfWork.JCawanganRepo.IsExist(e => e.Kod == kod);
        }
        public void PopulateDropdownList()
        {
            ViewBag.DPekerja = _unitOfWork.DPekerjaRepo.GetAll();
            ViewBag.AkBank = _unitOfWork.AkBankRepo.GetAll();
        }
    }
}
