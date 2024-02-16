using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services.Math;
using YIT.Akaun.Microservices;
using YIT.Akaun.Models.ViewModels.Common;

namespace YIT.Akaun.Controllers._02Daftar
{
    [Authorize]
    public class DPekerjaController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = "DF001";
        public const string namamodul = "Daftar Anggota";

        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserManager<IdentityUser> _userManager;

        public DPekerjaController(
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
        public IActionResult Index(string searchString,
           string searchColumn
           )
        {

            if (searchString == null)
            {
                HttpContext.Session.Clear();
                return View();
            }

            SaveFormFields(searchString);

            var dP = _unitOfWork.DPekerjaRepo.GetResults(searchString, searchColumn);

            return View(dP);
        }

        private void SaveFormFields(string searchString)
        {
            PopulateFormFields(searchString);

            if (searchString != null)
            {
                HttpContext.Session.SetString("searchString", searchString);
            }
            else
            {
                searchString = HttpContext.Session.GetString("searchString");
                ViewBag.searchString = searchString;
            }
        }

        private void PopulateFormFields(string searchString)
        {
            ViewBag.searchString = searchString;
        }

        public IActionResult Create()
        {
            ViewBag.NoGaji = GenerateRunningNumber();
            PopulateDropdownList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DPekerja pekerja, string syscode)
        {
            if (pekerja.NoKp != null && NoKPExists(pekerja.NoKp) == false)
            {
                if (ModelState.IsValid)
                {
                    //pekerja.NoGaji = GenerateRunningNumber();

                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;
                    pekerja.UserId = user?.UserName ?? "";

                    pekerja.TarMasuk = DateTime.Now;
                    pekerja.DPekerjaMasukId = pekerjaId;

                    _context.Add(pekerja);
                    _appLog.Insert("Tambah", pekerja.NoGaji + " - " + pekerja.Nama, pekerja.NoGaji ?? "Tiada No Gaji", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya ditambah..!";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString") });
                }
                
            }
            else
            {
                TempData[SD.Error] = "Kad Pengenalan ini telah wujud..!";
            }

            //ViewBag.NoGaji = GenerateRunningNumber();
            PopulateDropdownList();
            return View(pekerja);
        }

        // GET: PTJ/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pekerja = _unitOfWork.DPekerjaRepo.GetAllDetailsById((int)id);
            if (pekerja == null)
            {
                return NotFound();
            }

            return View(pekerja);
        }

        // GET: PTJ/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pekerja = _unitOfWork.DPekerjaRepo.GetAllDetailsById((int)id);
            if (pekerja == null)
            {
                return NotFound();
            }
            PopulateDropdownList();
            return View(pekerja);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DPekerja pekerja, string syscode)
        {
            if (id != pekerja.Id)
            {
                return NotFound();
            }

            if (pekerja.NoKp != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = await _context.DPekerja.FirstOrDefaultAsync(x => x.Id == pekerja.Id);
                    var NoKpAsal = objAsal!.NoKp;
                    pekerja.UserId = objAsal.UserId;
                    pekerja.TarMasuk = objAsal.TarMasuk;
                    pekerja.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                    _context.Entry(objAsal).State = EntityState.Detached;

                    pekerja.UserIdKemaskini = user?.UserName ?? "";

                    pekerja.TarKemaskini = DateTime.Now;
                    pekerja.DPekerjaKemaskiniId = pekerjaId;

                    _unitOfWork.DPekerjaRepo.Update(pekerja);

                    _appLog.Insert("Ubah", pekerja.NoGaji + " - " + pekerja.Nama, pekerja?.NoGaji ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PekerjaExists(pekerja.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString") });
            }
            PopulateDropdownList();
            return View(pekerja);
        }

        private bool PekerjaExists(int id)
        {
            return _unitOfWork.DPekerjaRepo.IsExist(b => b.Id == id);
        }

        // GET: Pekerja/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pekerja = _unitOfWork.DPekerjaRepo.GetAllDetailsById((int)id);
            if (pekerja == null)
            {
                return NotFound();
            }

            return View(pekerja);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string syscode)
        {
            var pekerja = _unitOfWork.DPekerjaRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (pekerja != null && pekerja.NoKp != null)
            {
                pekerja.UserIdKemaskini = user?.UserName ?? "";
                pekerja.TarKemaskini = DateTime.Now;
                pekerja.DPekerjaKemaskiniId = pekerjaId;

                _context.DPekerja.Remove(pekerja);
                _appLog.Insert("Hapus", pekerja.NoGaji + " - " + pekerja.Nama, pekerja?.NoGaji ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dihapuskan..!";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString") });
        }

        public async Task<IActionResult> RollBack(int id, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            var obj = await _context.DPekerja.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.DPekerja.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.NoGaji + " - " + obj.Nama, obj.NoGaji ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString") });

        }

        private bool NoKPExists(string noKp)
        {
            return _unitOfWork.DPekerjaRepo.IsExist(p => p.NoKp == noKp);
        }

        public string GenerateRunningNumber()
        {
            var maxRefNo = _unitOfWork.DPekerjaRepo.GetMaxRefNo();
            return RunningNumberFormatter.GenerateRunningNumber("", maxRefNo, "00000");
        }

        public void PopulateDropdownList()
        {
            ViewBag.JAgama = _unitOfWork.JAgamaRepo.GetAll();
            ViewBag.JBank = _unitOfWork.JBankRepo.GetAll();
            ViewBag.JBangsa = _unitOfWork.JBangsaRepo.GetAll();
            ViewBag.JCaraBayar = _unitOfWork.JCaraBayarRepo.GetAll();
            ViewBag.JNegeri = _unitOfWork.JNegeriRepo.GetAll();
            ViewBag.JBahagian = _unitOfWork.JBahagianRepo.GetAllDetails();
            ViewBag.JPTJ = _unitOfWork.JPTJRepo.GetAllDetails();
            ViewBag.JCawangan = _unitOfWork.JCawanganRepo.GetAll();
            var statusKahwin = EnumHelper<EnStatusKahwin>.GetList();

            ViewBag.EnStatusKahwin = statusKahwin;
        }
    }
}
