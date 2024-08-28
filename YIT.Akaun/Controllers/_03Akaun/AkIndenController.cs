using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services;
using YIT._DataAccess.Services.Cart;
using YIT._DataAccess.Services.Math;
using YIT.Akaun.Infrastructure;
using YIT.Akaun.Microservices;

namespace YIT.Akaun.Controllers._03Akaun
{
    [Authorize(Roles = Init.allExceptAdminRole)]
    public class AkIndenController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodAkInden;
        public const string namamodul = Modules.namaAkInden;
        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;
        private readonly CartAkInden _cart;

        public AkIndenController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices,
            CartAkInden cart)
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

            var akInden = _unitOfWork.AkIndenRepo.GetResults(searchString, date1, date2, searchColumn, EnStatusBorang.Semua);

            return View(akInden);
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

        [Authorize(Policy = modul)]
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akInden = _unitOfWork.AkIndenRepo.GetDetailsById((int)id);
            if (akInden == null)
            {
                return NotFound();
            }
            EmptyCart();
            PopulateCartAkIndenFromDb(akInden);
            return View(akInden);
        }

        [Authorize(Policy = modul + "D")]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akInden = _unitOfWork.AkIndenRepo.GetDetailsById((int)id);
            if (akInden == null)
            {
                return NotFound();
            }

            if (akInden.EnStatusBorang != EnStatusBorang.None)
            {
                TempData[SD.Error] = "Hapus data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkIndenFromDb(akInden);
            return View(akInden);
        }

        [Authorize(Policy = modul + "BL")]
        public IActionResult BatalLulus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akInden = _unitOfWork.AkIndenRepo.GetDetailsById((int)id);
            if (akInden == null)
            {
                return NotFound();
            }

            if (akInden.EnStatusBorang != EnStatusBorang.Lulus)
            {
                TempData[SD.Error] = "Data belum diluluskan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkIndenFromDb(akInden);
            return View(akInden);
        }

        [HttpPost, ActionName("BatalLulus")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "BL")]
        public async Task<IActionResult> BatalLulusConfirmed(int id, string tindakan, string syscode)
        {
            var akInden = _unitOfWork.AkIndenRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akInden != null && !string.IsNullOrEmpty(akInden.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkIndenRepo.IsPostedAsync((int)id, akInden.NoRujukan) == false)
                {
                    TempData[SD.Error] = "Data belum diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkIndenRepo.IsLulusAsync(id) == false)
                {
                    TempData[SD.Error] = "Data belum diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkIndenRepo.BatalLulus(id, tindakan, user?.Email);

                _appLog.Insert("UnPosting", "Batal Lulus " + akInden.NoRujukan ?? "", akInden.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var akInden = _unitOfWork.AkIndenRepo.GetDetailsById((int)id);
            if (akInden == null)
            {
                return NotFound();
            }

            if (akInden.EnStatusBorang != EnStatusBorang.Lulus)
            {
                TempData[SD.Error] = "Data belum diluluskan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkIndenFromDb(akInden);
            return View(akInden);
        }

        [HttpPost, ActionName("BatalPos")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "BL")]
        public async Task<IActionResult> BatalPosConfirmed(int id, string tindakan, string syscode)
        {
            var akInden = _unitOfWork.AkIndenRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akInden != null && !string.IsNullOrEmpty(akInden.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkIndenRepo.IsPostedAsync((int)id, akInden.NoRujukan) == false)
                {
                    TempData[SD.Error] = "Data belum diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkIndenRepo.IsLulusAsync(id) == false)
                {
                    TempData[SD.Error] = "Data belum diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkIndenRepo.BatalPos(id, tindakan, user?.UserName);

                _appLog.Insert("UnPosting", "Batal Pos " + akInden.NoRujukan ?? "", akInden.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var obj = await _context.AkInden.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Pos operation

            if (obj != null && !string.IsNullOrEmpty(obj.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkIndenRepo.IsPostedAsync((int)id, obj.NoRujukan))
                {
                    TempData[SD.Error] = "Data sudah diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkIndenRepo.IsLulusAsync(id))
                {
                    TempData[SD.Error] = "Data telah diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkIndenRepo.Lulus(id, pekerjaId, user?.UserName);

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
            PopulateDropDownList(1);
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.IK.GetDisplayName(), DateTime.Now.ToString("yyyy"));
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "C")]
        public async Task<IActionResult> Create(AkInden akInden, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            // check if there is pelulus available or not based on modul, kelulusan, and bahagian
            if (_cart.AkIndenObjek != null && _cart.AkIndenObjek.Count() > 0)
            {
                foreach (var item in _cart.AkIndenObjek)
                {
                    var jKWPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModulKelulusan.Inden, EnKategoriKelulusan.Pelulus, jKWPtjBahagian.JBahagianId, akInden.Jumlah) == false)
                    {
                        TempData[SD.Error] = "Tiada Pelulus yang wujud untuk senarai kod bahagian berikut.";
                        ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.IK.GetDisplayName(), akInden.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
                        PopulateDropDownList(akInden.JKWId);
                        PopulateListViewFromCart();
                        return View(akInden);
                    }
                }
            }
            //

            if (ModelState.IsValid)
            {

                akInden.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.IK.GetDisplayName(), akInden.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
                akInden.UserId = user?.UserName ?? "";
                akInden.TarMasuk = DateTime.Now;
                akInden.DPekerjaMasukId = pekerjaId;

                akInden.AkIndenObjek = _cart.AkIndenObjek?.ToList();
                akInden.AkIndenPerihal = _cart.AkIndenPerihal.ToList();

                if (akInden.AkIndenPerihal != null && akInden.AkIndenPerihal.Any())
                {
                    decimal jumlahCukai = 0;
                    decimal jumlahTanpaCukai = 0;
                    foreach (var item in akInden.AkIndenPerihal)
                    {
                        jumlahCukai += item.AmaunCukai;
                        jumlahTanpaCukai += (item.Harga * item.Kuantiti);
                    }

                    akInden.JumlahCukai = jumlahCukai;
                    akInden.JumlahTanpaCukai = jumlahTanpaCukai;
                }

                _context.Add(akInden);
                _appLog.Insert("Tambah", akInden.NoRujukan ?? "", akInden.NoRujukan ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya ditambah..!";
                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
            }
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.IK.GetDisplayName(), akInden.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
            PopulateDropDownList(akInden.JKWId);
            PopulateListViewFromCart();
            return View(akInden);
        }

        [Authorize(Policy = modul + "E")]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akInden = _unitOfWork.AkIndenRepo.GetDetailsById((int)id);
            if (akInden == null)
            {
                return NotFound();
            }

            if (akInden.EnStatusBorang != EnStatusBorang.None && akInden.EnStatusBorang != EnStatusBorang.Kemaskini)
            {
                TempData[SD.Error] = "Ubah data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }

            EmptyCart();
            PopulateDropDownList(akInden.JKWId);
            PopulateCartAkIndenFromDb(akInden);
            return View(akInden);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "E")]
        public async Task<IActionResult> Edit(int id, AkInden akInden, string? fullName, string syscode)
        {
            if (id != akInden.Id)
            {
                return NotFound();
            }

            // check if there is pelulus available or not based on modul, kelulusan, and bahagian
            if (_cart.AkIndenObjek != null && _cart.AkIndenObjek.Count() > 0)
            {
                foreach (var item in _cart.AkIndenObjek)
                {
                    var jKWPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModulKelulusan.Inden, EnKategoriKelulusan.Pelulus, jKWPtjBahagian.JBahagianId, akInden.Jumlah) == false)
                    {
                        TempData[SD.Error] = "Tiada Pelulus yang wujud untuk senarai kod bahagian berikut.";
                        PopulateDropDownList(akInden.JKWId);
                        PopulateListViewFromCart();
                        return View(akInden);
                    }
                }
            }
            //

            if (akInden.NoRujukan != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = _unitOfWork.AkIndenRepo.GetDetailsById(id);
                    var jumlahAsal = objAsal!.Jumlah;
                    akInden.NoRujukan = objAsal.NoRujukan;
                    akInden.UserId = objAsal.UserId;
                    akInden.TarMasuk = objAsal.TarMasuk;
                    akInden.DPekerjaMasukId = objAsal.DPekerjaMasukId;
                    akInden.EnStatusBorang = objAsal.EnStatusBorang;

                    if (objAsal.AkIndenObjek != null && objAsal.AkIndenObjek.Count > 0)
                    {
                        foreach (var item in objAsal.AkIndenObjek)
                        {
                            var model = _context.AkIndenObjek.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null) _context.Remove(model);
                        }
                    }

                    if (objAsal.AkIndenPerihal != null && objAsal.AkIndenPerihal.Count > 0)
                    {
                        foreach (var item in objAsal.AkIndenPerihal)
                        {
                            var model = _context.AkIndenPerihal.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null) _context.Remove(model);
                        }
                    }

                    _context.Entry(objAsal).State = EntityState.Detached;

                    akInden.UserIdKemaskini = user?.UserName ?? "";
                    akInden.TarKemaskini = DateTime.Now;
                    akInden.AkIndenObjek = _cart.AkIndenObjek?.ToList();
                    akInden.AkIndenPerihal = _cart.AkIndenPerihal.ToList();

                    if (akInden.AkIndenPerihal != null && akInden.AkIndenPerihal.Any())
                    {
                        decimal jumlahCukai = 0;
                        decimal jumlahTanpaCukai = 0;
                        foreach (var item in akInden.AkIndenPerihal)
                        {
                            jumlahCukai += item.AmaunCukai;
                            jumlahTanpaCukai += (item.Harga * item.Kuantiti);
                        }

                        akInden.JumlahCukai = jumlahCukai;
                        akInden.JumlahTanpaCukai = jumlahTanpaCukai;
                    }

                    _unitOfWork.AkIndenRepo.Update(akInden);

                    if (jumlahAsal != akInden.Jumlah)
                    {
                        _appLog.Insert("Ubah", Convert.ToDecimal(jumlahAsal).ToString("#,##0.00") + " -> " + Convert.ToDecimal(akInden.Jumlah).ToString("#,##0.00") + " : " + akInden.NoRujukan ?? "", akInden.NoRujukan ?? "", id, akInden.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }
                    else
                    {
                        _appLog.Insert("Ubah", akInden.NoRujukan ?? "", akInden.NoRujukan ?? "", id, akInden.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }

                    await _context.SaveChangesAsync();

                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AkIndenExist(akInden.Id))
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

            PopulateDropDownList(akInden.JKWId);
            PopulateListViewFromCart();
            return View(akInden);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "D")]
        public async Task<IActionResult> DeleteConfirmed(int id, string sebabHapus, string syscode)
        {
            var akInden = _unitOfWork.AkIndenRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akInden != null && await _unitOfWork.AkIndenRepo.IsSahAsync(id) == false)
            {
                akInden.UserIdKemaskini = user?.UserName ?? "";
                akInden.TarKemaskini = DateTime.Now;
                akInden.DPekerjaKemaskiniId = pekerjaId;
                akInden.SebabHapus = sebabHapus;

                _context.AkInden.Remove(akInden);
                _appLog.Insert("Hapus", akInden.NoRujukan ?? "", akInden.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var obj = await _context.AkInden.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.AkInden.Update(obj);

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


                    // get akInden 
                    var akInden = _unitOfWork.AkIndenRepo.GetDetailsById((int)id);
                    if (akInden == null)
                    {
                        TempData[SD.Error] = "Data tidak wujud.";
                    }
                    else
                    {

                        if (akInden.NoRujukan != null)
                        {
                            // check is it posted or not
                            if (await _unitOfWork.AkIndenRepo.IsPostedAsync((int)id, akInden.NoRujukan) == false)
                            {
                                TempData[SD.Error] = "Data belum diposting.";
                                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                            }

                            // posting start here
                            _unitOfWork.AkIndenRepo.RemovePostingFromAbBukuVot(akInden, user?.UserName ?? "");

                            //insert applog
                            _appLog.Insert("UnPosting", "UnPosting Data", akInden.NoRujukan, (int)id, akInden.Jumlah, pekerjaId, modul, syscode, namamodul, user);

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
        private bool AkIndenExist(int id)
        {
            return _unitOfWork.AkIndenRepo.IsExist(b => b.Id == id);
        }

        private void PopulateListViewFromCart()
        {
            List<AkIndenObjek> objek = _cart.AkIndenObjek.ToList();

            foreach (AkIndenObjek item in objek)
            {
                var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                item.JKWPTJBahagian = jkwPtjBahagian;

                item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                item.AkCarta = akCarta;
            }

            ViewBag.akIndenObjek = objek;

            List<AkIndenPerihal> perihal = _cart.AkIndenPerihal.ToList();

            ViewBag.akIndenPerihal = perihal;
        }

        private string GenerateRunningNumber(string initNoRujukan, string tahun)
        {
            var maxRefNo = _unitOfWork.AkIndenRepo.GetMaxRefNo(initNoRujukan, tahun);

            var prefix = initNoRujukan + "/" + tahun + "/";
            return RunningNumberFormatter.GenerateRunningNumber(prefix, maxRefNo, "00000");
        }

        private void PopulateDropDownList(int JKWId)
        {
            ViewBag.JKW = _unitOfWork.JKWRepo.GetAll();
            ViewBag.AkCarta = _unitOfWork.AkCartaRepo.GetResultsByParas(EnParas.Paras4);
            ViewBag.JKWPTJBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetails();
            ViewBag.JKWPTJBahagianByJKW = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsByJKWId(JKWId);
            ViewBag.AkPenilaianPerolehan = _unitOfWork.AkPenilaianPerolehanRepo.GetAllByJenis(1);
            ViewBag.EnLHDNJenisCukai = EnumHelper<EnLHDNJenisCukai>.GetList();
            ViewBag.LHDNMSIC = _unitOfWork.LHDNMSICRepo.GetAll();
            ViewBag.LHDNKodKlasifikasi = _unitOfWork.LHDNKodKlasifikasiRepo.GetAll();
            ViewBag.LHDNUnitUkuran = _unitOfWork.LHDNUnitUkuranRepo.GetAll();
        }

        private void PopulateCartAkIndenFromDb(AkInden akInden)
        {
            if (akInden.AkIndenObjek != null)
            {
                foreach (var item in akInden.AkIndenObjek)
                {
                    _cart.AddItemObjek(
                            akInden.Id,
                            item.JKWPTJBahagianId,
                            item.AkCartaId,
                            item.Amaun);
                }
            }

            if (akInden.AkIndenPerihal != null)
            {
                foreach (var item in akInden.AkIndenPerihal)
                {
                    _cart.AddItemPerihal(
                        akInden.Id,
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

        private void PopulateFormFields(string? searchString, string? searchDate1, string? searchDate2)
        {
            ViewBag.searchString = searchString;
            ViewBag.searchDate1 = searchDate1 ?? DateTime.Now.ToString("dd/MM/yyyy");
            ViewBag.searchDate2 = searchDate2 ?? DateTime.Now.ToString("dd/MM/yyyy");
        }

        // jsonResults
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

        public JsonResult GetAkPenilaianPerolehanDetails(int id, int? akIndenId)
        {
            try
            {
                var data = _unitOfWork.AkPenilaianPerolehanRepo.GetDetailsById(id);

                if (data != null)
                {
                    if (data.AkPenilaianPerolehanObjek != null)
                    {
                        foreach (var item in data.AkPenilaianPerolehanObjek)
                        {
                            _cart.AddItemObjek(
                                    akIndenId ?? 0,
                                    item.JKWPTJBahagianId,
                                    item.AkCartaId,
                                    item.Amaun);
                        }
                    }

                    if (data.AkPenilaianPerolehanPerihal != null)
                    {
                        foreach (var item in data.AkPenilaianPerolehanPerihal)
                        {
                            _cart.AddItemPerihal(
                                akIndenId ?? 0,
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
                    return Json(new { result = "OK", record = data });
                }
                else
                {
                    return Json(new { result = "Error", message = "data tidak wujud!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }
        }

        public JsonResult SaveCartAkIndenObjek(AkIndenObjek akIndenObjek)
        {
            try
            {
                if (akIndenObjek != null)
                {
                    _cart.AddItemObjek(akIndenObjek.AkIndenId, akIndenObjek.JKWPTJBahagianId, akIndenObjek.AkCartaId, akIndenObjek.Amaun);
                }



                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkIndenObjek(AkIndenObjek akIndenObjek)
        {
            try
            {
                if (akIndenObjek != null)
                {
                    _cart.RemoveItemObjek(akIndenObjek.JKWPTJBahagianId, akIndenObjek.AkCartaId);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkIndenObjek(AkIndenObjek akIndenObjek)
        {

            try
            {
                AkIndenObjek data = _cart.AkIndenObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akIndenObjek.JKWPTJBahagianId && x.AkCartaId == akIndenObjek.AkCartaId) ?? new AkIndenObjek();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkIndenObjek(AkIndenObjek akIndenObjek)
        {

            try
            {

                var akInden = _cart.AkIndenObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akIndenObjek.JKWPTJBahagianId && x.AkCartaId == akIndenObjek.AkCartaId);

                var user = _userManager.GetUserName(User);

                if (akInden != null)
                {
                    _cart.RemoveItemObjek(akIndenObjek.JKWPTJBahagianId, akIndenObjek.AkCartaId);

                    _cart.AddItemObjek(akIndenObjek.AkIndenId,
                                    akIndenObjek.JKWPTJBahagianId,
                                    akIndenObjek.AkCartaId,
                                    akIndenObjek.Amaun);
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
                var akInden = _cart.AkIndenPerihal.FirstOrDefault(pp => pp.Bil == Bil);
                if (akInden != null)
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

        public JsonResult SaveCartAkIndenPerihal(AkIndenPerihal akIndenPerihal)
        {
            try
            {
                if (akIndenPerihal != null)
                {
                    _cart.AddItemPerihal(akIndenPerihal.AkIndenId, akIndenPerihal.Bil, akIndenPerihal.Perihal, akIndenPerihal.Kuantiti, akIndenPerihal.LHDNKodKlasifikasiId ?? _unitOfWork.LHDNKodKlasifikasiRepo.GetByCodeAsync("022").Result.Id, akIndenPerihal.LHDNUnitUkuranId ?? _unitOfWork.LHDNUnitUkuranRepo.GetByCodeAsync("C62").Result.Id,akIndenPerihal.Unit,akIndenPerihal.EnLHDNJenisCukai,akIndenPerihal.KadarCukai, akIndenPerihal.AmaunCukai, akIndenPerihal.Harga, akIndenPerihal.Amaun);
                }



                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkIndenPerihal(AkIndenPerihal akIndenPerihal)
        {
            try
            {
                if (akIndenPerihal != null)
                {
                    _cart.RemoveItemPerihal(akIndenPerihal.Bil);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkIndenPerihal(AkIndenPerihal akIndenPerihal)
        {

            try
            {
                AkIndenPerihal data = _cart.AkIndenPerihal.FirstOrDefault(x => x.Bil == akIndenPerihal.Bil) ?? new AkIndenPerihal();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkIndenPerihal(AkIndenPerihal akIndenPerihal)
        {

            try
            {

                var akInden = _cart.AkIndenPerihal.FirstOrDefault(x => x.Bil == akIndenPerihal.Bil);

                var user = _userManager.GetUserName(User);

                if (akInden != null)
                {
                    _cart.RemoveItemPerihal(akIndenPerihal.Bil);

                    _cart.AddItemPerihal(akIndenPerihal.AkIndenId, akIndenPerihal.Bil, akIndenPerihal.Perihal, akIndenPerihal.Kuantiti, akIndenPerihal.LHDNKodKlasifikasiId ?? _unitOfWork.LHDNKodKlasifikasiRepo.GetByCodeAsync("022").Result.Id, akIndenPerihal.LHDNUnitUkuranId ?? _unitOfWork.LHDNUnitUkuranRepo.GetByCodeAsync("C62").Result.Id, akIndenPerihal.Unit, akIndenPerihal.EnLHDNJenisCukai, akIndenPerihal.KadarCukai, akIndenPerihal.AmaunCukai, akIndenPerihal.Harga, akIndenPerihal.Amaun);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAllItemCartAkInden()
        {

            try
            {
                List<AkIndenObjek> objek = _cart.AkIndenObjek.ToList();

                foreach (AkIndenObjek item in objek)
                {
                    var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    item.JKWPTJBahagian = jkwPtjBahagian;

                    item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                    var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                    item.AkCarta = akCarta;
                }

                List<AkIndenPerihal> perihal = _cart.AkIndenPerihal.ToList();

                return Json(new { result = "OK", objek = objek.OrderBy(d => d.AkCarta?.Kod), perihal = perihal.OrderBy(d => d.Bil) });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        // printing akPenilaianPerolehan
        public async Task<IActionResult> PrintPDFById(int id)
        {
            AkInden akInden = _unitOfWork.AkIndenRepo.GetDetailsById(id);

            var company = await _userServices.GetCompanyDetails();
            EmptyCart();
            PopulateCartAkIndenFromDb(akInden);
            //string customSwitches = "--page-offset 0 --footer-center [page] / [toPage] --footer-font-size 6";

            return new ViewAsPdf(modul + EnJenisFail.PDF, akInden,
                new ViewDataDictionary(ViewData) {
                    { "NamaSyarikat", company.NamaSyarikat },
                    { "AlamatSyarikat1", company.AlamatSyarikat1 },
                    { "AlamatSyarikat2", company.AlamatSyarikat2 },
                    { "AlamatSyarikat3", company.AlamatSyarikat3 }
                })
            {
                PageMargins = { Left = 15, Bottom = 10, Right = 15, Top = 10 },
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                //CustomSwitches = "--footer-center \"[page]/[toPage]\"" +
                //" --footer-line --footer-font-size \"7\" --footer-spacing 1 --footer-font-name \"Segoe UI\"",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
            };
        }
        // printing akPenilaianPerolehan end
    }
}
