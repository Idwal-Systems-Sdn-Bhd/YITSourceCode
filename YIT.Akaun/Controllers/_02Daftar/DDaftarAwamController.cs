using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services.Math;
using YIT.Akaun.Microservices;
using YIT.Akaun.Models.ViewModels.Common;

namespace YIT.Akaun.Controllers._02Daftar
{
    [Authorize]
    public class DDaftarAwamController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = "DF002";
        public const string namamodul = "Daftar Daftar Awam";

        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserManager<IdentityUser> _userManager;

        public DDaftarAwamController(
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
            return View(_unitOfWork.DDaftarAwamRepo.GetAllDetails());
        }

        public IActionResult Create()
        {
            PopulateDropdownList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DDaftarAwam daftarAwam, string syscode)
        {
            if (daftarAwam.EnKategoriDaftarAwam == EnKategoriDaftarAwam.LainLain)
            {
                TempData[SD.Error] = "Sila Pilih Kategori..!";
                ViewBag.KodSyarikat = GenerateRunningNumber(daftarAwam.NamaSyarikat?.Substring(1) ?? "A");
                PopulateDropdownList();
                return View(daftarAwam);
            }

            if (daftarAwam.NamaSyarikat != null && NamaSyarikatExists(daftarAwam.NamaSyarikat) == false)
            {
                if (ModelState.IsValid)
                {
                    daftarAwam.KodSyarikat = GenerateRunningNumber(daftarAwam.NamaSyarikat);

                    var user = await _userManager.GetUserAsync(User);
                    int? daftarAwamId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;
                    daftarAwam.UserId = user?.UserName ?? "";

                    daftarAwam.TarMasuk = DateTime.Now;
                    daftarAwam.DPekerjaMasukId = daftarAwamId;

                    _context.Add(daftarAwam);
                    _appLog.Insert("Tambah", daftarAwam.KodSyarikat + " - " + daftarAwam.NamaSyarikat, daftarAwam.KodSyarikat, 0, 0, daftarAwamId, modul, syscode, namamodul, user);
                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya ditambah..!";
                    return RedirectToAction(nameof(Index));
                }

            }
            else
            {
                TempData[SD.Error] = "Kod Syarikat ini telah wujud..!";
            }

            ViewBag.KodSyarikat = GenerateRunningNumber(daftarAwam.NamaSyarikat?.Substring(1) ?? "A");
            PopulateDropdownList();
            return View(daftarAwam);
        }

        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var daftarAwam = _unitOfWork.DDaftarAwamRepo.GetAllDetailsById((int)id);
            if (daftarAwam == null)
            {
                return NotFound();
            }

            return View(daftarAwam);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var daftarAwam = _unitOfWork.DDaftarAwamRepo.GetAllDetailsById((int)id);
            if (daftarAwam == null)
            {
                return NotFound();
            }
            PopulateDropdownList();
            return View(daftarAwam);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DDaftarAwam daftarAwam, string syscode)
        {
            if (id != daftarAwam.Id)
            {
                return NotFound();
            }

            if (daftarAwam.NamaSyarikat != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? daftarAwamId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = await _context.DDaftarAwam.FirstOrDefaultAsync(x => x.Id == daftarAwam.Id);
                    if (objAsal != null)
                    {
                        daftarAwam.NamaSyarikat = objAsal.NamaSyarikat;
                        daftarAwam.KodSyarikat = objAsal.KodSyarikat;
                        daftarAwam.UserId = objAsal.UserId;
                        daftarAwam.TarMasuk = objAsal.TarMasuk;
                        daftarAwam.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                        _context.Entry(objAsal).State = EntityState.Detached;

                        daftarAwam.UserIdKemaskini = user?.UserName ?? "";

                        daftarAwam.TarKemaskini = DateTime.Now;
                        daftarAwam.DPekerjaKemaskiniId = daftarAwamId;

                    }

                    _unitOfWork.DDaftarAwamRepo.Update(daftarAwam);

                    _appLog.Insert("Ubah", daftarAwam.KodSyarikat + " - " + daftarAwam.NamaSyarikat, daftarAwam?.KodSyarikat ?? "", id, 0, daftarAwamId, modul, syscode, namamodul, user);

                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NamaSyarikatExists(daftarAwam.NamaSyarikat ?? ""))
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
            return View(daftarAwam);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var daftarAwam = _unitOfWork.DDaftarAwamRepo.GetAllDetailsById((int)id);
            if (daftarAwam == null)
            {
                return NotFound();
            }

            return View(daftarAwam);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string syscode)
        {
            var daftarAwam = _unitOfWork.DDaftarAwamRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (daftarAwam != null && daftarAwam.KodSyarikat != null)
            {
                daftarAwam.UserIdKemaskini = user?.UserName ?? "";
                daftarAwam.TarKemaskini = DateTime.Now;
                daftarAwam.DPekerjaKemaskiniId = pekerjaId;

                _context.DDaftarAwam.Remove(daftarAwam);
                _appLog.Insert("Hapus", daftarAwam.KodSyarikat + " - " + daftarAwam.NamaSyarikat, daftarAwam.KodSyarikat, id, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dihapuskan..!";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> RollBack(int id, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            var obj = await _context.DDaftarAwam.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.DDaftarAwam.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.KodSyarikat + " - " + obj.NamaSyarikat, obj.KodSyarikat ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool NamaSyarikatExists(string namaSyarikat)
        {
            return _unitOfWork.DDaftarAwamRepo.IsExist(df => df.NamaSyarikat == namaSyarikat);
        }

        private void PopulateDropdownList()
        {
            ViewBag.JBank = _unitOfWork.JBankRepo.GetAll();
            ViewBag.JNegeri = _unitOfWork.JNegeriRepo.GetAll();
            var kategoriDaftarAwam = EnumHelper<EnKategoriDaftarAwam>.GetList();

            ViewBag.EnKategoriDaftarAwam = kategoriDaftarAwam;
        }

        private string GenerateRunningNumber(string namaSyarikat)
        {
            var maxRefNo = _unitOfWork.DDaftarAwamRepo.GetMaxRefNo(namaSyarikat);
            var nama = namaSyarikat.Substring(0, 1).ToUpper();
            return namaSyarikat.Substring(0,1).ToUpper() + RunningNumberFormatter.GenerateRunningNumber("", maxRefNo, 0);
        }

        [HttpPost]
        public JsonResult GetKodSyarikat(string namaSyarikat)
        {
            try
            {
                var result = "";
                if (namaSyarikat != null)
                {
                    result = GenerateRunningNumber(namaSyarikat);
                }
                return Json(new { result = "OK", record = result });
            } catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }
            
        }

    }
}
