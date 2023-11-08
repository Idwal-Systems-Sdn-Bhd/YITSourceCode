using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services.Math;

namespace YIT.Akaun.Controllers._02Daftar
{
    [Authorize]
    public class DPelulusController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = "DF004";
        public const string namamodul = "Daftar Pelulus";

        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserManager<IdentityUser> _userManager;

        public DPelulusController(
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
            return View(_unitOfWork.DPelulusRepo.GetAllDetails());
        }

        public IActionResult Create()
        {
            PopulateDropdownList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DPelulus pelulus, string syscode)
        {
            if (PelulusExists(pelulus.DPekerjaId) == false)
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pelulusId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;
                    pelulus.UserId = user?.UserName ?? "";

                    pelulus.TarMasuk = DateTime.Now;
                    pelulus.DPekerjaMasukId = pelulusId;

                    _context.Add(pelulus);
                    _appLog.Insert("Tambah", pelulus.DPekerjaId.ToString() + " - " + _unitOfWork.DPekerjaRepo.GetById(pelulus.DPekerjaId).Nama, pelulus.DPekerjaId.ToString(), 0, 0, pelulusId, modul, syscode, namamodul, user);
                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya ditambah..!";
                    return RedirectToAction(nameof(Index));
                }

            }
            else
            {
                TempData[SD.Error] = "Anggota ini telah wujud..!";
            }

            PopulateDropdownList();
            return View(pelulus);
        }

        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pelulus = _unitOfWork.DPelulusRepo.GetAllDetailsById((int)id);
            if (pelulus == null)
            {
                return NotFound();
            }

            return View(pelulus);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pelulus = _unitOfWork.DPelulusRepo.GetAllDetailsById((int)id);
            if (pelulus == null)
            {
                return NotFound();
            }
            PopulateDropdownList();
            return View(pelulus);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DPelulus pelulus, string syscode)
        {
            if (id != pelulus.Id)
            {
                return NotFound();
            }

            if (pelulus.DPekerjaId != 0 && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pelulusId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = await _context.DPelulus.FirstOrDefaultAsync(x => x.Id == pelulus.Id)!;
                    if (objAsal != null)
                    {
                        pelulus.UserId = objAsal.UserId;
                        pelulus.TarMasuk = objAsal.TarMasuk;
                        pelulus.DPekerjaMasukId = objAsal?.DPekerjaMasukId;

                        _context.Entry(objAsal!).State = EntityState.Detached;

                        pelulus.UserIdKemaskini = user?.UserName ?? "";

                        pelulus.TarKemaskini = DateTime.Now;
                        pelulus.DPekerjaKemaskiniId = pelulusId;

                        _unitOfWork.DPelulusRepo.Update(pelulus);

                        _appLog.Insert("Ubah", pelulus.DPekerjaId.ToString() + " - " + _unitOfWork.DPekerjaRepo.GetById(pelulus.DPekerjaId).Nama, pelulus.DPekerjaId.ToString(), id, 0, pelulusId, modul, syscode, namamodul, user);

                        await _context.SaveChangesAsync();
                        TempData[SD.Success] = "Data berjaya diubah..!";
                    }
                    
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PelulusExists(pelulus.Id))
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
            return View(pelulus);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pelulus = _unitOfWork.DPelulusRepo.GetAllDetailsById((int)id);
            if (pelulus == null)
            {
                return NotFound();
            }

            return View(pelulus);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string syscode)
        {
            var pelulus = _unitOfWork.DPelulusRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pelulusId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (pelulus != null && pelulus.DPekerjaId != 0)
            {
                pelulus.UserIdKemaskini = user?.UserName ?? "";
                pelulus.TarKemaskini = DateTime.Now;
                pelulus.DPekerjaKemaskiniId = pelulusId;

                _context.DPelulus.Remove(pelulus);
                _appLog.Insert("Hapus", pelulus.DPekerjaId.ToString() + " - " + _unitOfWork.DPekerjaRepo.GetById(pelulus.DPekerjaId).Nama, pelulus.DPekerjaId.ToString(), id, 0, pelulusId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dihapuskan..!";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> RollBack(int id, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pelulusId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            var obj = await _context.DPelulus.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pelulusId;

                _context.DPelulus.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.DPekerjaId.ToString() + " - " + _unitOfWork.DPekerjaRepo.GetById(obj.DPekerjaId).Nama, obj.DPekerjaId.ToString() ?? "", id, 0, pelulusId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index));
        }
        private bool PelulusExists(int dPekerjaId)
        {
            return _unitOfWork.DPelulusRepo.IsExist(p => p.DPekerjaId == dPekerjaId);
        }

        private void PopulateDropdownList()
        {
            ViewBag.DPekerja = _unitOfWork.DPekerjaRepo.GetAll();
        }
    }
}
