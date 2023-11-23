using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT.Akaun.Controllers._03Akaun
{
    [Authorize]
    public class AkBankController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = "AK002";
        public const string namamodul = "Akaun Bank";
        private readonly _IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserManager<IdentityUser> _userManager;

        public AkBankController(
            _IUnitOfWork unitOfWork,
            ApplicationDbContext context,
            _AppLogIRepository<AppLog, int> appLog,
            UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _appLog = appLog;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View(_unitOfWork.AkBankRepo.GetAllDetails());
        }
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akBank = _unitOfWork.AkBankRepo.GetAllDetailsById((int)id);
            if (akBank == null)
            {
                return NotFound();
            }

            return View(akBank);
        }

        public IActionResult Create()
        {
            PopulateDropdownList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AkBank akBank, string syscode)
        {
            if (akBank.NoAkaun != null && NoAkaunBankExists(akBank.NoAkaun) == false)
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    akBank.UserId = user?.UserName ?? "";

                    akBank.TarMasuk = DateTime.Now;
                    akBank.DPekerjaMasukId = pekerjaId;

                    _context.Add(akBank);
                    _appLog.Insert("Tambah", akBank.Perihal + " - " + akBank.NoAkaun, akBank.Perihal ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
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
            return View(akBank);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akBank = _unitOfWork.AkBankRepo.GetAllDetailsById((int)id);
            if (akBank == null)
            {
                return NotFound();
            }
            PopulateDropdownList();
            return View(akBank);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AkBank akBank, string syscode)
        {
            if (id != akBank.Id)
            {
                return NotFound();
            }

            if (akBank.Perihal != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = await _context.AkBank.FirstOrDefaultAsync(x => x.Id == akBank.Id);
                    var kodAsal = objAsal!.Kod;
                    var perihalAsal = objAsal.Perihal;
                    akBank.UserId = objAsal.UserId;
                    akBank.TarMasuk = objAsal.TarMasuk;
                    akBank.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                    _context.Entry(objAsal).State = EntityState.Detached;

                    akBank.UserIdKemaskini = user?.UserName ?? "";

                    akBank.TarKemaskini = DateTime.Now;
                    akBank.DPekerjaKemaskiniId = pekerjaId;

                    _unitOfWork.AkBankRepo.Update(akBank);

                    _appLog.Insert("Ubah", kodAsal + " -> " + akBank.Perihal + ", " + perihalAsal + " -> " + akBank.NoAkaun + ", ", akBank.Perihal, id, 0, pekerjaId, modul, syscode, namamodul, user);

                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BankExists(akBank.Id))
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
            return View(akBank);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akBank = _unitOfWork.AkBankRepo.GetAllDetailsById((int)id);
            if (akBank == null)
            {
                return NotFound();
            }

            return View(akBank);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string syscode)
        {
            var akBank = _unitOfWork.AkBankRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akBank != null && akBank.Perihal != null)
            {
                akBank.UserIdKemaskini = user?.UserName ?? "";
                akBank.TarKemaskini = DateTime.Now;
                akBank.DPekerjaKemaskiniId = pekerjaId;

                _context.AkBank.Remove(akBank);
                _appLog.Insert("Hapus", akBank.Perihal + " - " + akBank.NoAkaun, akBank.Perihal, id, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dihapuskan..!";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> RollBack(int id, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            var obj = await _context.AkBank.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.AkBank.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.Perihal + " - " + obj.NoAkaun, obj.Perihal ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BankExists(int id)
        {
            return _unitOfWork.AkBankRepo.IsExist(b => b.Id == id);
        }

        private bool NoAkaunBankExists(string noAkaun)
        {
            return _unitOfWork.AkBankRepo.IsExist(b => b.NoAkaun == noAkaun);
        }

        private void PopulateDropdownList()
        {
            ViewBag.JBank = _unitOfWork.JBankRepo.GetAll();
            ViewBag.JKW = _unitOfWork.JKWRepo.GetAll();
            ViewBag.AkCarta = _unitOfWork.AkCartaRepo.GetResultsByJenis(EnJenisCarta.Aset, EnParas.Paras4);
        }
    }
}
