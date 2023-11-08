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
    public class JBankController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = "JD004";
        public const string namamodul = "Jadual Bank";

        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;

        public JBankController(ApplicationDbContext context,
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
            return View(_unitOfWork.JBankRepo.GetAll());
        }

        // GET: Bank/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bank = _unitOfWork.JBankRepo.GetById((int)id);
            if (bank == null)
            {
                return NotFound();
            }

            return View(bank);
        }

        // GET: Bank/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Bank/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JBank bank, string syscode)
        {
            if (bank.Kod != null && KodBankExists(bank.Kod) == false)
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    bank.UserId = user?.UserName ?? "";

                    bank.TarMasuk = DateTime.Now;
                    bank.DPekerjaMasukId = pekerjaId;

                    _context.Add(bank);
                    _appLog.Insert("Tambah", bank.Kod + " - " + bank.Perihal, bank.Kod, 0, 0, pekerjaId, modul, syscode, namamodul, user);
                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya ditambah..!";
                    return RedirectToAction(nameof(Index));

                }
            }
            else
            {
                TempData[SD.Error] = "Kod ini telah wujud..!";
            }

            return View(bank);
        }

        [Authorize(Roles = "SuperAdmin")]
        // GET: Bank/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bank = _unitOfWork.JBankRepo.GetById((int)id);
            if (bank == null)
            {
                return NotFound();
            }
            return View(bank);
        }

        // POST: Bank/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, JBank bank, string syscode)
        {
            if (id != bank.Id)
            {
                return NotFound();
            }

            if (bank.Kod != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = await _context.JBank.FirstOrDefaultAsync(x => x.Id == bank.Id);
                    var kodAsal = objAsal!.Kod;
                    var perihalAsal = objAsal.Perihal;
                    bank.UserId = objAsal.UserId;
                    bank.TarMasuk = objAsal.TarMasuk;
                    bank.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                    _context.Entry(objAsal).State = EntityState.Detached;

                    bank.UserIdKemaskini = user?.UserName ?? "";

                    bank.TarKemaskini = DateTime.Now;
                    bank.DPekerjaKemaskiniId = pekerjaId;

                    _unitOfWork.JBankRepo.Update(bank);

                    _appLog.Insert("Ubah", kodAsal + " -> " + bank.Kod + ", " + perihalAsal + " -> " + bank.Perihal + ", ", bank.Kod, id, 0, pekerjaId, modul, syscode, namamodul, user);

                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BankExists(bank.Id))
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
            return View(bank);
        }

        // GET: Bank/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bank = _unitOfWork.JBankRepo.GetById((int)id);
            if (bank == null)
            {
                return NotFound();
            }

            return View(bank);
        }

        // POST: Bank/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string syscode)
        {
            var bank = _unitOfWork.JBankRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (bank != null && bank.Kod != null)
            {
                bank.UserIdKemaskini = user?.UserName ?? "";
                bank.TarKemaskini = DateTime.Now;
                bank.DPekerjaKemaskiniId = pekerjaId;

                _context.JBank.Remove(bank);
                _appLog.Insert("Hapus", bank.Kod + " - " + bank.Perihal, bank.Kod, id, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dihapuskan..!";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> RollBack(int id, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            var obj = await _context.JBank.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.JBank.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.Kod + " - " + obj.Perihal, obj.Kod ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index));
        }
        private bool BankExists(int id)
        {
            return _unitOfWork.JBankRepo.IsExist(b => b.Id == id);
        }

        private bool KodBankExists(string kod)
        {
            return _unitOfWork.JBankRepo.IsExist(e => e.Kod == kod);
        }
    }
}
