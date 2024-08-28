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
using YIT._DataAccess.Services;
using YIT._DataAccess.Services.Cart;
using YIT._DataAccess.Services.Math;
using YIT.Akaun.Infrastructure;
using YIT.Akaun.Microservices;

namespace YIT.Akaun.Controllers
{
    public class AkInvoisController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodAkInvois;
        public const string namamodul = Modules.namaAkInvois;

        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;
        private readonly CartAkInvois _cart;

        public AkInvoisController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices,
            CartAkInvois cart)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _appLog = appLog;
            _userServices = userServices;
            _cart = cart;
        }
        public IActionResult Index(
            string searchString,
            string searchDate1,
            string searchDate2,
            string searchColumn)
        {
            if (searchString == null && (searchDate1 == null && searchDate2 == null))
            {
                HttpContext.Session.Clear();
                return View();
            }

            DateTime? date1 = null;
            DateTime? date2 = null;

            if (!string.IsNullOrEmpty(searchDate1) && !string.IsNullOrEmpty(searchDate2))
            {
                date1 = DateTime.Parse(searchDate1);
                date2 = DateTime.Parse(searchDate2);
            }

            SaveFormFields(searchString, searchDate1, searchDate2);

            var akInvois = _unitOfWork.AkInvoisRepo.GetResults(searchString, date1, date2, searchColumn, EnStatusBorang.Semua);

            return View(akInvois);
        }

        private void SaveFormFields(string? searchString, string? searchDate1, string? searchDate2)
        {
            PopulateFormFields(searchString, searchDate1, searchDate2);

            if (searchString != null)
            {
                HttpContext.Session.SetString("searchString", searchString);
            }
            else
            {
                searchString = HttpContext.Session.GetString("searchString");
                ViewBag.searchString = searchString;
            }

            if (searchDate1 != null && searchDate2 != null)
            {
                HttpContext.Session.SetString("searchDate1", searchDate1);
                HttpContext.Session.SetString("searchDate2", searchDate2);
            }
            else
            {
                searchDate1 = HttpContext.Session.GetString("searchDate1");
                searchDate2 = HttpContext.Session.GetString("searchDate2");

                ViewBag.searchDate1 = searchDate1;
                ViewBag.searchDate2 = searchDate2;
            }
        }

        private void PopulateFormFields(string? searchString, string? searchDate1, string? searchDate2)
        {
            ViewBag.searchString = searchString;
            ViewBag.searchDate1 = searchDate1 ?? DateTime.Now.ToString("dd/MM/yyyy");
            ViewBag.searchDate2 = searchDate2 ?? DateTime.Now.ToString("dd/MM/yyyy");
        }

        [Authorize(Policy = modul)]
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akInvois = _unitOfWork.AkInvoisRepo.GetDetailsById((int)id);
            if (akInvois == null)
            {
                return NotFound();
            }
            EmptyCart();
            PopulateCartAkInvoisFromDb(akInvois);
            return View(akInvois);
        }

        [Authorize(Policy = modul + "D")]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akInvois = _unitOfWork.AkInvoisRepo.GetDetailsById((int)id);
            if (akInvois == null)
            {
                return NotFound();
            }

            if (akInvois.EnStatusBorang != EnStatusBorang.None)
            {
                TempData[SD.Error] = "Hapus data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkInvoisFromDb(akInvois);
            return View(akInvois);
        }

        [Authorize(Policy = modul + "BL")]
        public IActionResult BatalLulus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akInvois = _unitOfWork.AkInvoisRepo.GetDetailsById((int)id);
            if (akInvois == null)
            {
                return NotFound();
            }

            if (akInvois.EnStatusBorang != EnStatusBorang.Lulus)
            {
                TempData[SD.Error] = "Data belum diluluskan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkInvoisFromDb(akInvois);
            return View(akInvois);
        }

        [HttpPost, ActionName("BatalLulus")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "BL")]
        public async Task<IActionResult> BatalLulusConfirmed(int id, string tindakan, string syscode)
        {
            var akInvois = _unitOfWork.AkInvoisRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akInvois != null && !string.IsNullOrEmpty(akInvois.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkInvoisRepo.IsPostedAsync((int)id, akInvois.NoRujukan) == false)
                {
                    TempData[SD.Error] = "Data belum diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkInvoisRepo.IsLulusAsync(id) == false)
                {
                    TempData[SD.Error] = "Data belum diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkInvoisRepo.BatalLulus(id, tindakan, user?.Email);

                _appLog.Insert("UnPosting", "Batal Lulus " + akInvois.NoRujukan ?? "", akInvois.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya batal kelulusan..!";
            }
            else
            {
                TempData[SD.Error] = "Data tidak wujud";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
        }

        [Authorize(Policy = modul + "BL")]
        public IActionResult BatalPos(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akInvois = _unitOfWork.AkInvoisRepo.GetDetailsById((int)id);
            if (akInvois == null)
            {
                return NotFound();
            }

            if (akInvois.EnStatusBorang != EnStatusBorang.Lulus)
            {
                TempData[SD.Error] = "Data belum diluluskan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkInvoisFromDb(akInvois);
            return View(akInvois);
        }

        [HttpPost, ActionName("BatalPos")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "BL")]
        public async Task<IActionResult> BatalPosConfirmed(int id, string tindakan, string syscode)
        {
            var akInvois = _unitOfWork.AkInvoisRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akInvois != null && !string.IsNullOrEmpty(akInvois.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkInvoisRepo.IsPostedAsync((int)id, akInvois.NoRujukan) == false)
                {
                    TempData[SD.Error] = "Data belum diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkInvoisRepo.IsLulusAsync(id) == false)
                {
                    TempData[SD.Error] = "Data belum diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkInvoisRepo.BatalPos(id, tindakan, user?.UserName);

                _appLog.Insert("UnPosting", "Batal Pos " + akInvois.NoRujukan ?? "", akInvois.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya batal pos..!";
            }
            else
            {
                TempData[SD.Error] = "Data belum disahkan / disemak / diluluskan";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
        }

        [Authorize(Policy = modul + "L")]
        public async Task<IActionResult> PosSemula(int id, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            var obj = await _context.AkInvois.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Pos operation

            if (obj != null && !string.IsNullOrEmpty(obj.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkInvoisRepo.IsPostedAsync((int)id, obj.NoRujukan))
                {
                    TempData[SD.Error] = "Data sudah diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkInvoisRepo.IsLulusAsync(id))
                {
                    TempData[SD.Error] = "Data telah diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkInvoisRepo.Lulus(id, pekerjaId, user?.UserName);

                // Batal operation end
                _appLog.Insert("Posting", obj.NoRujukan ?? "", obj.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya pos semula..!";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
        }

        [Authorize(Policy = modul + "C")]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);

            EmptyCart();
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.ID.GetDisplayName(), DateTime.Now.ToString("yyyy"));
            PopulateDropDownList(1);
            return View();
        }

        private string GenerateRunningNumber(string initNoRujukan, string tahun)
        {
            var maxRefNo = _unitOfWork.AkInvoisRepo.GetMaxRefNo(initNoRujukan, tahun);

            var prefix = initNoRujukan + "/" + tahun + "/";
            return RunningNumberFormatter.GenerateRunningNumber(prefix, maxRefNo, "00000");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "C")]
        public async Task<IActionResult> Create(AkInvois akInvois, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            // check if there is pelulus available or not based on modul, kelulusan, and bahagian
            if (_cart.AkInvoisObjek != null && _cart.AkInvoisObjek.Count() > 0)
            {
                foreach (var item in _cart.AkInvoisObjek)
                {
                    var jKWPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModulKelulusan.Invois, EnKategoriKelulusan.Pelulus, jKWPtjBahagian.JBahagianId, akInvois.Jumlah) == false)
                    {
                        TempData[SD.Error] = "Tiada Pelulus yang wujud untuk senarai kod bahagian berikut.";
                        ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.ID.GetDisplayName(), akInvois.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
                        PopulateDropDownList(akInvois.JKWId);
                        PopulateListViewFromCart();
                        return View(akInvois);
                    }
                }
            }
            //

            if (ModelState.IsValid)
            {

                akInvois.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.ID.GetDisplayName(), akInvois.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
                akInvois.UserId = user?.UserName ?? "";
                akInvois.TarMasuk = DateTime.Now;
                akInvois.DPekerjaMasukId = pekerjaId;

                akInvois.AkInvoisObjek = _cart.AkInvoisObjek?.ToList();
                akInvois.AkInvoisPerihal = _cart.AkInvoisPerihal.ToList();

                if (akInvois.AkInvoisPerihal != null && akInvois.AkInvoisPerihal.Any())
                {
                    decimal jumlahCukai = 0;
                    decimal jumlahTanpaCukai = 0;
                    foreach (var item in akInvois.AkInvoisPerihal)
                    {
                        jumlahCukai += item.AmaunCukai;
                        jumlahTanpaCukai += (item.Harga * item.Kuantiti);
                    }

                    akInvois.JumlahCukai = jumlahCukai;
                    akInvois.JumlahTanpaCukai = jumlahTanpaCukai;
                }

                _context.Add(akInvois);
                _appLog.Insert("Tambah", akInvois.NoRujukan ?? "", akInvois.NoRujukan ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya ditambah..!";
                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
            }
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.ID.GetDisplayName(), akInvois.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
            PopulateDropDownList(akInvois.JKWId);
            PopulateListViewFromCart();
            return View(akInvois);
        }

        [Authorize(Policy = modul + "E")]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akInvois = _unitOfWork.AkInvoisRepo.GetDetailsById((int)id);
            if (akInvois == null)
            {
                return NotFound();
            }

            if (akInvois.EnStatusBorang != EnStatusBorang.None && akInvois.EnStatusBorang != EnStatusBorang.Kemaskini)
            {
                TempData[SD.Error] = "Ubah data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }

            EmptyCart();
            PopulateDropDownList(akInvois.JKWId);
            PopulateCartAkInvoisFromDb(akInvois);
            return View(akInvois);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "E")]
        public async Task<IActionResult> Edit(int id, AkInvois akInvois, string? fullName, string syscode)
        {
            if (id != akInvois.Id)
            {
                return NotFound();
            }

            // check if there is pelulus available or not based on modul, kelulusan, and bahagian
            if (_cart.AkInvoisObjek != null && _cart.AkInvoisObjek.Count() > 0)
            {
                foreach (var item in _cart.AkInvoisObjek)
                {
                    var jKWPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModulKelulusan.Invois, EnKategoriKelulusan.Pelulus, jKWPtjBahagian.JBahagianId, akInvois.Jumlah) == false)
                    {
                        TempData[SD.Error] = "Tiada Pelulus yang wujud untuk senarai kod bahagian berikut.";
                        PopulateDropDownList(akInvois.JKWId);
                        PopulateListViewFromCart();
                        return View(akInvois);
                    }
                }
            }
            //

            if (akInvois.NoRujukan != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = _unitOfWork.AkInvoisRepo.GetDetailsById(id);
                    var jumlahAsal = objAsal!.Jumlah;
                    akInvois.NoRujukan = objAsal.NoRujukan;
                    akInvois.JKWId = objAsal.JKWId;
                    akInvois.DDaftarAwamId = objAsal.DDaftarAwamId;
                    akInvois.UserId = objAsal.UserId;
                    akInvois.TarMasuk = objAsal.TarMasuk;
                    akInvois.DPekerjaMasukId = objAsal.DPekerjaMasukId;
                    akInvois.EnStatusBorang = objAsal.EnStatusBorang;

                    if (objAsal.AkInvoisObjek != null && objAsal.AkInvoisObjek.Count > 0)
                    {
                        foreach (var item in objAsal.AkInvoisObjek)
                        {
                            var model = _context.AkInvoisObjek.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null) _context.Remove(model);
                        }
                    }

                    if (objAsal.AkInvoisPerihal != null && objAsal.AkInvoisPerihal.Count > 0)
                    {
                        foreach (var item in objAsal.AkInvoisPerihal)
                        {
                            var model = _context.AkInvoisPerihal.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null) _context.Remove(model);
                        }
                    }

                    _context.Entry(objAsal).State = EntityState.Detached;

                    akInvois.UserIdKemaskini = user?.UserName ?? "";
                    akInvois.TarKemaskini = DateTime.Now;
                    akInvois.AkInvoisObjek = _cart.AkInvoisObjek?.ToList();
                    akInvois.AkInvoisPerihal = _cart.AkInvoisPerihal.ToList();

                    if (akInvois.AkInvoisPerihal != null && akInvois.AkInvoisPerihal.Any())
                    {
                        decimal jumlahCukai = 0;
                        decimal jumlahTanpaCukai = 0;
                        foreach (var item in akInvois.AkInvoisPerihal)
                        {
                            jumlahCukai += item.AmaunCukai;
                            jumlahTanpaCukai += (item.Harga * item.Kuantiti);
                        }

                        akInvois.JumlahCukai = jumlahCukai;
                        akInvois.JumlahTanpaCukai = jumlahTanpaCukai;
                    }

                    _unitOfWork.AkInvoisRepo.Update(akInvois);

                    if (jumlahAsal != akInvois.Jumlah)
                    {
                        _appLog.Insert("Ubah", Convert.ToDecimal(jumlahAsal).ToString("#,##0.00") + " -> " + Convert.ToDecimal(akInvois.Jumlah).ToString("#,##0.00") + " : " + akInvois.NoRujukan ?? "", akInvois.NoRujukan ?? "", id, akInvois.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }
                    else
                    {
                        _appLog.Insert("Ubah", akInvois.NoRujukan ?? "", akInvois.NoRujukan ?? "", id, akInvois.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }

                    await _context.SaveChangesAsync();

                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AkInvoisExist(akInvois.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
            }

            PopulateDropDownList(akInvois.JKWId);
            PopulateListViewFromCart();
            return View(akInvois);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "D")]
        public async Task<IActionResult> DeleteConfirmed(int id, string sebabHapus, string syscode)
        {
            var akInvois = _unitOfWork.AkInvoisRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akInvois != null && await _unitOfWork.AkInvoisRepo.IsLulusAsync(id) == false)
            {
                akInvois.UserIdKemaskini = user?.UserName ?? "";
                akInvois.TarKemaskini = DateTime.Now;
                akInvois.DPekerjaKemaskiniId = pekerjaId;
                akInvois.SebabHapus = sebabHapus;

                _context.AkInvois.Remove(akInvois);
                _appLog.Insert("Hapus", akInvois.NoRujukan ?? "", akInvois.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dihapuskan..!";
            }
            else
            {
                TempData[SD.Error] = "Data telah diluluskan";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
        }

        [Authorize(Policy = modul + "R")]
        public async Task<IActionResult> RollBack(int id, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            var obj = await _context.AkInvois.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.AkInvois.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.NoRujukan ?? "", obj.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
        }

        [Authorize(Policy = modul + "BL")]
        public async Task<IActionResult> UnPosting(int? id, string syscode)
        {
            if (id == null)
            {
                return NotFound();
            }
            else
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;


                    // get akInvois 
                    var akInvois = _unitOfWork.AkInvoisRepo.GetDetailsById((int)id);
                    if (akInvois == null)
                    {
                        TempData[SD.Error] = "Data tidak wujud.";
                    }
                    else
                    {

                        if (akInvois.NoRujukan != null)
                        {
                            // check is it posted or not
                            if (await _unitOfWork.AkInvoisRepo.IsPostedAsync((int)id, akInvois.NoRujukan) == false)
                            {
                                TempData[SD.Error] = "Data belum diposting.";
                                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                            }

                            // posting start here
                            _unitOfWork.AkInvoisRepo.RemovePostingFromAkAkaun(akInvois);

                            //insert applog
                            _appLog.Insert("UnPosting", "UnPosting Data", akInvois.NoRujukan, (int)id, akInvois.Jumlah, pekerjaId, modul, syscode, namamodul, user);

                            //insert applog end

                            await _context.SaveChangesAsync();
                            TempData[SD.Success] = "Batal posting data berjaya";

                        }
                        else
                        {
                            TempData[SD.Error] = "No Rujukan Tiada";
                        }
                    }
                }
                catch (Exception)
                {
                    TempData[SD.Error] = "Berlaku ralat semasa transaksi. Data gagal batal posting.";

                }
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
        }

        private bool AkInvoisExist(int id)
        {
            return _unitOfWork.AkInvoisRepo.IsExist(b => b.Id == id);
        }

        private void PopulateDropDownList(int JKWId)
        {
            ViewBag.JKW = _unitOfWork.JKWRepo.GetAll();
            ViewBag.AkCarta = _unitOfWork.AkCartaRepo.GetResultsByParas(EnParas.Paras4);
            ViewBag.JKWPTJBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetails();
            ViewBag.JKWPTJBahagianByJKW = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsByJKWId(JKWId);
            ViewBag.EnJenisPerolehan = EnumHelper<EnJenisPerolehan>.GetList();
            ViewBag.EnLHDNJenisCukai = EnumHelper<EnLHDNJenisCukai>.GetList();
            ViewBag.LHDNMSIC = _unitOfWork.LHDNMSICRepo.GetAll();
            ViewBag.LHDNKodKlasifikasi = _unitOfWork.LHDNKodKlasifikasiRepo.GetAll();
            ViewBag.LHDNUnitUkuran = _unitOfWork.LHDNUnitUkuranRepo.GetAll();
        }

        public JsonResult EmptyCart()
        {
            try
            {
                _cart.ClearObjek();
                _cart.ClearPerihal();

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        private void PopulateCartAkInvoisFromDb(AkInvois akInvois)
        {
            if (akInvois.AkInvoisObjek != null)
            {
                foreach (var item in akInvois.AkInvoisObjek)
                {
                    _cart.AddItemObjek(
                            akInvois.Id,
                            item.JKWPTJBahagianId,
                            item.AkCartaId,
                            item.Amaun);
                }
            }

            if (akInvois.AkInvoisPerihal != null)
            {
                foreach (var item in akInvois.AkInvoisPerihal)
                {
                    _cart.AddItemPerihal(
                        akInvois.Id,
                        item.Bil,
                        item.Perihal,
                        item.Kuantiti,
                        item.LHDNKodKlasifikasiId ?? _unitOfWork.LHDNKodKlasifikasiRepo.GetByCodeAsync("022").Result.Id,
                        item.LHDNUnitUkuranId ?? _unitOfWork.LHDNUnitUkuranRepo.GetByCodeAsync("C62").Result.Id,
                        item.Unit,
                        item.EnLHDNJenisCukai,
                        item.KadarCukai,
                        item.AmaunCukai,
                        item.Harga,
                        item.Amaun
                        );
                }
            }
            PopulateListViewFromCart();
        }

        private void PopulateListViewFromCart()
        {
            List<AkInvoisObjek> objek = _cart.AkInvoisObjek.ToList();

            foreach (AkInvoisObjek item in objek)
            {
                var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                item.JKWPTJBahagian = jkwPtjBahagian;

                item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                item.AkCarta = akCarta;


            }

            ViewBag.akInvoisObjek = objek;

            List<AkInvoisPerihal> perihal = _cart.AkInvoisPerihal.ToList();

            ViewBag.akInvoisPerihal = perihal;
        }

        public JsonResult SaveCartAkInvoisObjek(AkInvoisObjek akInvoisObjek)
        {
            try
            {
                var jCukai = new JCukai();
                if (akInvoisObjek != null)
                {
                    _cart.AddItemObjek(akInvoisObjek.AkInvoisId, akInvoisObjek.JKWPTJBahagianId, akInvoisObjek.AkCartaId, akInvoisObjek.Amaun);


                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkInvoisObjek(AkInvoisObjek akInvoisObjek)
        {
            try
            {
                if (akInvoisObjek != null)
                {
                    _cart.RemoveItemObjek(akInvoisObjek.JKWPTJBahagianId, akInvoisObjek.AkCartaId);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkInvoisObjek(AkInvoisObjek akInvoisObjek)
        {

            try
            {
                AkInvoisObjek data = _cart.AkInvoisObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akInvoisObjek.JKWPTJBahagianId && x.AkCartaId == akInvoisObjek.AkCartaId) ?? new AkInvoisObjek();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkInvoisObjek(AkInvoisObjek akInvoisObjek)
        {

            try
            {

                var data = _cart.AkInvoisObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akInvoisObjek.JKWPTJBahagianId && x.AkCartaId == akInvoisObjek.AkCartaId);

                var user = _userManager.GetUserName(User);

                if (data != null)
                {
                    _cart.RemoveItemObjek(akInvoisObjek.JKWPTJBahagianId, akInvoisObjek.AkCartaId);

                    _cart.AddItemObjek(akInvoisObjek.AkInvoisId,
                                    akInvoisObjek.JKWPTJBahagianId,
                                    akInvoisObjek.AkCartaId,
                                    akInvoisObjek.Amaun);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetBil(int Bil)
        {
            try
            {
                var akInvois = _cart.AkInvoisPerihal.FirstOrDefault(pp => pp.Bil == Bil);
                if (akInvois != null)
                {
                    return Json(new { result = "Error", message = "Bil telah wujud" });
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }
        }

        public JsonResult SaveCartAkInvoisPerihal(AkInvoisPerihal akInvoisPerihal)
        {
            try
            {
                if (akInvoisPerihal != null)
                {
                    _cart.AddItemPerihal(akInvoisPerihal.AkInvoisId, akInvoisPerihal.Bil, akInvoisPerihal.Perihal, akInvoisPerihal.Kuantiti, akInvoisPerihal.LHDNKodKlasifikasiId ?? _unitOfWork.LHDNKodKlasifikasiRepo.GetByCodeAsync("022").Result.Id, akInvoisPerihal.LHDNUnitUkuranId ?? _unitOfWork.LHDNUnitUkuranRepo.GetByCodeAsync("C62").Result.Id, akInvoisPerihal.Unit, akInvoisPerihal.EnLHDNJenisCukai, akInvoisPerihal.KadarCukai, akInvoisPerihal.AmaunCukai, akInvoisPerihal.Harga, akInvoisPerihal.Amaun);
                }



                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkInvoisPerihal(AkInvoisPerihal akInvoisPerihal)
        {
            try
            {
                if (akInvoisPerihal != null)
                {
                    _cart.RemoveItemPerihal(akInvoisPerihal.Bil);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkInvoisPerihal(AkInvoisPerihal akInvoisPerihal)
        {

            try
            {
                AkInvoisPerihal data = _cart.AkInvoisPerihal.FirstOrDefault(x => x.Bil == akInvoisPerihal.Bil) ?? new AkInvoisPerihal();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkInvoisPerihal(AkInvoisPerihal akInvoisPerihal)
        {

            try
            {

                var akInvois = _cart.AkInvoisPerihal.FirstOrDefault(x => x.Bil == akInvoisPerihal.Bil);

                var user = _userManager.GetUserName(User);

                if (akInvois != null)
                {
                    _cart.RemoveItemPerihal(akInvoisPerihal.Bil);

                    _cart.AddItemPerihal(akInvoisPerihal.AkInvoisId, akInvoisPerihal.Bil, akInvoisPerihal.Perihal, akInvoisPerihal.Kuantiti, akInvoisPerihal.LHDNKodKlasifikasiId ?? _unitOfWork.LHDNKodKlasifikasiRepo.GetByCodeAsync("022").Result.Id, akInvoisPerihal.LHDNUnitUkuranId ?? _unitOfWork.LHDNUnitUkuranRepo.GetByCodeAsync("C62").Result.Id, akInvoisPerihal.Unit, akInvoisPerihal.EnLHDNJenisCukai, akInvoisPerihal.KadarCukai, akInvoisPerihal.AmaunCukai, akInvoisPerihal.Harga, akInvoisPerihal.Amaun);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAllItemCartAkInvois()
        {

            try
            {
                List<AkInvoisObjek> objek = _cart.AkInvoisObjek.ToList();

                foreach (AkInvoisObjek item in objek)
                {
                    var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    item.JKWPTJBahagian = jkwPtjBahagian;

                    item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                    var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                    item.AkCarta = akCarta;
                }

                List<AkInvoisPerihal> perihal = _cart.AkInvoisPerihal.ToList();

                return Json(new { result = "OK", objek = objek.OrderBy(d => d.AkCarta?.Kod), perihal = perihal.OrderBy(d => d.Bil) });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

    }
}
