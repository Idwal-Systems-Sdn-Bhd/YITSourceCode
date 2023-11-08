using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT.Akaun.Controllers._02Daftar
{
    [Authorize]
    public class DPenyemakController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = "DF003";
        public const string namamodul = "Daftar Penyemak";

        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserManager<IdentityUser> _userManager;

        public DPenyemakController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            _AppLogIRepository<AppLog, int> appLog,
            UserManager<IdentityUser> userManager
            )
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _appLog = appLog;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View(_unitOfWork.DPenyemakRepo.GetAllDetails());
        }

        public IActionResult Create()
        {
            PopulateDropdownList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DPenyemak penyemak, string syscode)
        {
            if (PenyemakExists(penyemak.DPekerjaId) == false)
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? penyemakId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;
                    penyemak.UserId = user?.UserName ?? "";

                    penyemak.TarMasuk = DateTime.Now;
                    penyemak.DPekerjaMasukId = penyemakId;

                    _context.Add(penyemak);
                    _appLog.Insert("Tambah", penyemak.DPekerjaId.ToString() + " - " + _unitOfWork.DPekerjaRepo.GetById(penyemak.DPekerjaId).Nama, penyemak.DPekerjaId.ToString(), 0, 0, penyemakId, modul, syscode, namamodul, user);
                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya ditambah..!";
                    return RedirectToAction(nameof(Index));
                }

            }
            else
            {
                TempData[SD.Error] = "Penyemak ini telah wujud..!";
            }

            PopulateDropdownList();
            return View(penyemak);
        }

        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var penyemak = _unitOfWork.DPenyemakRepo.GetAllDetailsById((int)id);
            if (penyemak == null)
            {
                return NotFound();
            }

            return View(penyemak);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var penyemak = _unitOfWork.DPenyemakRepo.GetAllDetailsById((int)id);
            if (penyemak == null)
            {
                return NotFound();
            }
            PopulateDropdownList();
            return View(penyemak);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DPenyemak penyemak, string syscode)
        {
            if (id != penyemak.Id)
            {
                return NotFound();
            }

            if (penyemak.DPekerjaId != 0 && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? penyemakId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = await _context.DPenyemak.FirstOrDefaultAsync(x => x.Id == penyemak.Id)!;
                    if (objAsal != null)
                    {
                        penyemak.UserId = objAsal.UserId;
                        penyemak.TarMasuk = objAsal.TarMasuk;
                        penyemak.DPekerjaMasukId = objAsal?.DPekerjaMasukId;

                        _context.Entry(objAsal!).State = EntityState.Detached;

                        penyemak.UserIdKemaskini = user?.UserName ?? "";

                        penyemak.TarKemaskini = DateTime.Now;
                        penyemak.DPekerjaKemaskiniId = penyemakId;

                        _unitOfWork.DPenyemakRepo.Update(penyemak);

                        _appLog.Insert("Ubah", penyemak.DPekerjaId.ToString() + " - " + _unitOfWork.DPekerjaRepo.GetById(penyemak.DPekerjaId).Nama, penyemak.DPekerjaId.ToString(), id, 0, penyemakId, modul, syscode, namamodul, user);

                        await _context.SaveChangesAsync();
                        TempData[SD.Success] = "Data berjaya diubah..!";
                    }

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PenyemakExists(penyemak.Id))
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
            return View(penyemak);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var penyemak = _unitOfWork.DPenyemakRepo.GetAllDetailsById((int)id);
            if (penyemak == null)
            {
                return NotFound();
            }

            return View(penyemak);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string syscode)
        {
            var penyemak = _unitOfWork.DPenyemakRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? penyemakId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (penyemak != null && penyemak.DPekerjaId != 0)
            {
                penyemak.UserIdKemaskini = user?.UserName ?? "";
                penyemak.TarKemaskini = DateTime.Now;
                penyemak.DPekerjaKemaskiniId = penyemakId;

                _context.DPenyemak.Remove(penyemak);
                _appLog.Insert("Hapus", penyemak.DPekerjaId.ToString() + " - " + _unitOfWork.DPekerjaRepo.GetById(penyemak.DPekerjaId).Nama, penyemak.DPekerjaId.ToString(), id, 0, penyemakId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dihapuskan..!";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> RollBack(int id, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? penyemakId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            var obj = await _context.DPenyemak.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = penyemakId;

                _context.DPenyemak.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.DPekerjaId.ToString() + " - " + _unitOfWork.DPekerjaRepo.GetById(obj.DPekerjaId).Nama, obj.DPekerjaId.ToString() ?? "", id, 0, penyemakId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index));
        }
        private bool PenyemakExists(int dPekerjaId)
        {
            return _unitOfWork.DPenyemakRepo.IsExist(p => p.DPekerjaId == dPekerjaId);
        }

        private void PopulateDropdownList()
        {
            ViewBag.DPekerja = _unitOfWork.DPekerjaRepo.GetAll();
        }
    }
}
