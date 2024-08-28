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

namespace YIT.Akaun.Controllers._03Akaun
{
    [Authorize(Roles = Init.allExceptAdminRole)]
    public class AkNotaDebitKreditDiterimaController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodAkNotaDebitKreditDiterima;
        public const string namamodul = Modules.namaAkNotaDebitKreditDiterima;
        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;
        private readonly CartAkNotaDebitKreditDiterima _cart;

        public AkNotaDebitKreditDiterimaController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices,
            CartAkNotaDebitKreditDiterima cart
            )
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

            var akNotaDebitKreditDiterima = _unitOfWork.AkNotaDebitKreditDiterimaRepo.GetResults(searchString, date1, date2, searchColumn, EnStatusBorang.Semua);

            return View(akNotaDebitKreditDiterima);
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

            var akNotaDebitKreditDiterima = _unitOfWork.AkNotaDebitKreditDiterimaRepo.GetDetailsById((int)id);
            if (akNotaDebitKreditDiterima == null)
            {
                return NotFound();
            }
            EmptyCart();
            PopulateCartAkNotaDebitKreditDiterimaFromDb(akNotaDebitKreditDiterima);
            return View(akNotaDebitKreditDiterima);
        }

        [Authorize(Policy = modul + "D")]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akNotaDebitKreditDiterima = _unitOfWork.AkNotaDebitKreditDiterimaRepo.GetDetailsById((int)id);
            if (akNotaDebitKreditDiterima == null)
            {
                return NotFound();
            }

            if (akNotaDebitKreditDiterima.EnStatusBorang != EnStatusBorang.None)
            {
                TempData[SD.Error] = "Hapus data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkNotaDebitKreditDiterimaFromDb(akNotaDebitKreditDiterima);
            return View(akNotaDebitKreditDiterima);
        }

        [Authorize(Policy = modul + "BL")]
        public IActionResult BatalLulus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akNotaDebitKreditDiterima = _unitOfWork.AkNotaDebitKreditDiterimaRepo.GetDetailsById((int)id);
            if (akNotaDebitKreditDiterima == null)
            {
                return NotFound();
            }

            if (akNotaDebitKreditDiterima.EnStatusBorang != EnStatusBorang.Lulus)
            {
                TempData[SD.Error] = "Data belum diluluskan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkNotaDebitKreditDiterimaFromDb(akNotaDebitKreditDiterima);
            return View(akNotaDebitKreditDiterima);
        }

        [HttpPost, ActionName("BatalLulus")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "BL")]
        public async Task<IActionResult> BatalLulusConfirmed(int id, string tindakan, string syscode)
        {
            var akNotaDebitKreditDiterima = _unitOfWork.AkNotaDebitKreditDiterimaRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akNotaDebitKreditDiterima != null && !string.IsNullOrEmpty(akNotaDebitKreditDiterima.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkNotaDebitKreditDiterimaRepo.IsPostedAsync((int)id, akNotaDebitKreditDiterima.NoRujukan) == false)
                {
                    TempData[SD.Error] = "Data belum diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkNotaDebitKreditDiterimaRepo.IsLulusAsync(id) == false)
                {
                    TempData[SD.Error] = "Data belum diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkNotaDebitKreditDiterimaRepo.BatalLulus(id, tindakan, user?.Email);

                _appLog.Insert("UnPosting", "Batal Lulus " + akNotaDebitKreditDiterima.NoRujukan ?? "", akNotaDebitKreditDiterima.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var akNotaDebitKreditDiterima = _unitOfWork.AkNotaDebitKreditDiterimaRepo.GetDetailsById((int)id);
            if (akNotaDebitKreditDiterima == null)
            {
                return NotFound();
            }

            if (akNotaDebitKreditDiterima.EnStatusBorang != EnStatusBorang.Lulus)
            {
                TempData[SD.Error] = "Data belum diluluskan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkNotaDebitKreditDiterimaFromDb(akNotaDebitKreditDiterima);
            return View(akNotaDebitKreditDiterima);
        }

        [HttpPost, ActionName("BatalPos")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "BL")]
        public async Task<IActionResult> BatalPosConfirmed(int id, string tindakan, string syscode)
        {
            var akNotaDebitKreditDiterima = _unitOfWork.AkNotaDebitKreditDiterimaRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akNotaDebitKreditDiterima != null && !string.IsNullOrEmpty(akNotaDebitKreditDiterima.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkNotaDebitKreditDiterimaRepo.IsPostedAsync((int)id, akNotaDebitKreditDiterima.NoRujukan) == false)
                {
                    TempData[SD.Error] = "Data belum diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkNotaDebitKreditDiterimaRepo.IsLulusAsync(id) == false)
                {
                    TempData[SD.Error] = "Data belum diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkNotaDebitKreditDiterimaRepo.BatalPos(id, tindakan, user?.UserName);

                _appLog.Insert("UnPosting", "Batal Pos " + akNotaDebitKreditDiterima.NoRujukan ?? "", akNotaDebitKreditDiterima.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var obj = await _context.AkNotaDebitKreditDiterima.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Pos operation

            if (obj != null && !string.IsNullOrEmpty(obj.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkNotaDebitKreditDiterimaRepo.IsPostedAsync((int)id, obj.NoRujukan))
                {
                    TempData[SD.Error] = "Data sudah diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkNotaDebitKreditDiterimaRepo.IsLulusAsync(id))
                {
                    TempData[SD.Error] = "Data telah diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkNotaDebitKreditDiterimaRepo.Lulus(id, pekerjaId, user?.UserName);

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
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.ND.GetDisplayName(), DateTime.Now.ToString("yyyy"));
            return View();
        }
        private string GenerateRunningNumber(string initNoRujukan, string tahun)
        {
            var maxRefNo = _unitOfWork.AkNotaDebitKreditDiterimaRepo.GetMaxRefNo(initNoRujukan, tahun);

            var prefix = initNoRujukan + "/" + tahun + "/";
            return RunningNumberFormatter.GenerateRunningNumber(prefix, maxRefNo, "00000");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "C")]
        public async Task<IActionResult> Create(AkNotaDebitKreditDiterima akNotaDebitKreditDiterima, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            // check if there is pelulus available or not based on modul, kelulusan, and bahagian
            if (_cart.AkNotaDebitKreditDiterimaObjek != null && _cart.AkNotaDebitKreditDiterimaObjek.Count() > 0)
            {
                foreach (var item in _cart.AkNotaDebitKreditDiterimaObjek)
                {
                    var jKWPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModulKelulusan.NotaDebitKreditDiterima, EnKategoriKelulusan.Pelulus, jKWPtjBahagian.JBahagianId, akNotaDebitKreditDiterima.Jumlah) == false)
                    {
                        TempData[SD.Error] = "Tiada Pelulus yang wujud untuk senarai kod bahagian berikut.";
                        ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.ND.GetDisplayName(), akNotaDebitKreditDiterima.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
                        PopulateDropDownList(akNotaDebitKreditDiterima.JKWId);
                        PopulateListViewFromCart();
                        return View(akNotaDebitKreditDiterima);
                    }
                }
            }
            //

            if (ModelState.IsValid)
            {

                akNotaDebitKreditDiterima.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.ND.GetDisplayName(), akNotaDebitKreditDiterima.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
                akNotaDebitKreditDiterima.UserId = user?.UserName ?? "";
                akNotaDebitKreditDiterima.TarMasuk = DateTime.Now;
                akNotaDebitKreditDiterima.DPekerjaMasukId = pekerjaId;

                akNotaDebitKreditDiterima.AkNotaDebitKreditDiterimaObjek = _cart.AkNotaDebitKreditDiterimaObjek?.ToList();
                akNotaDebitKreditDiterima.AkNotaDebitKreditDiterimaPerihal = _cart.AkNotaDebitKreditDiterimaPerihal.ToList();

                if (akNotaDebitKreditDiterima.AkNotaDebitKreditDiterimaPerihal != null && akNotaDebitKreditDiterima.AkNotaDebitKreditDiterimaPerihal.Any())
                {
                    decimal jumlahCukai = 0;
                    decimal jumlahTanpaCukai = 0;
                    foreach (var item in akNotaDebitKreditDiterima.AkNotaDebitKreditDiterimaPerihal)
                    {
                        jumlahCukai += item.AmaunCukai;
                        jumlahTanpaCukai += (item.Harga * item.Kuantiti);
                    }

                    akNotaDebitKreditDiterima.JumlahCukai = jumlahCukai;
                    akNotaDebitKreditDiterima.JumlahTanpaCukai = jumlahTanpaCukai;
                }

                _context.Add(akNotaDebitKreditDiterima);
                _appLog.Insert("Tambah", akNotaDebitKreditDiterima.NoRujukan ?? "", akNotaDebitKreditDiterima.NoRujukan ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya ditambah..!";
                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
            }
            PopulateDropDownList(akNotaDebitKreditDiterima.JKWId);
            PopulateListViewFromCart();
            GenerateRunningNumber(EnInitNoRujukan.ND.GetDisplayName(), akNotaDebitKreditDiterima.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
            return View(akNotaDebitKreditDiterima);
        }

        [Authorize(Policy = modul + "E")]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akNotaDebitKreditDiterima = _unitOfWork.AkNotaDebitKreditDiterimaRepo.GetDetailsById((int)id);
            if (akNotaDebitKreditDiterima == null)
            {
                return NotFound();
            }

            if (akNotaDebitKreditDiterima.EnStatusBorang != EnStatusBorang.None && akNotaDebitKreditDiterima.EnStatusBorang != EnStatusBorang.Kemaskini)
            {
                TempData[SD.Error] = "Ubah data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }

            EmptyCart();
            PopulateDropDownList(akNotaDebitKreditDiterima.JKWId);
            PopulateCartAkNotaDebitKreditDiterimaFromDb(akNotaDebitKreditDiterima);
            return View(akNotaDebitKreditDiterima);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "E")]
        public async Task<IActionResult> Edit(int id, AkNotaDebitKreditDiterima akNotaDebitKreditDiterima, string? fullName, string syscode)
        {
            if (id != akNotaDebitKreditDiterima.Id)
            {
                return NotFound();
            }

            // check if there is pelulus available or not based on modul, kelulusan, and bahagian
            if (_cart.AkNotaDebitKreditDiterimaObjek != null && _cart.AkNotaDebitKreditDiterimaObjek.Count() > 0)
            {
                foreach (var item in _cart.AkNotaDebitKreditDiterimaObjek)
                {
                    var jKWPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModulKelulusan.NotaDebitKreditDiterima, EnKategoriKelulusan.Pelulus, jKWPtjBahagian.JBahagianId, akNotaDebitKreditDiterima.Jumlah) == false)
                    {
                        TempData[SD.Error] = "Tiada Pelulus yang wujud untuk senarai kod bahagian berikut.";
                        PopulateDropDownList(akNotaDebitKreditDiterima.JKWId);
                        PopulateListViewFromCart();
                        return View(akNotaDebitKreditDiterima);
                    }
                }
            }
            //

            if (akNotaDebitKreditDiterima.NoRujukan != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = _unitOfWork.AkNotaDebitKreditDiterimaRepo.GetDetailsById(id);
                    var jumlahAsal = objAsal!.Jumlah;
                    akNotaDebitKreditDiterima.NoRujukan = objAsal.NoRujukan;
                    akNotaDebitKreditDiterima.JKWId = objAsal.JKWId;
                    akNotaDebitKreditDiterima.AkBelianId = objAsal.AkBelianId;
                    akNotaDebitKreditDiterima.UserId = objAsal.UserId;
                    akNotaDebitKreditDiterima.TarMasuk = objAsal.TarMasuk;
                    akNotaDebitKreditDiterima.DPekerjaMasukId = objAsal.DPekerjaMasukId;
                    akNotaDebitKreditDiterima.EnStatusBorang = objAsal.EnStatusBorang;

                    if (objAsal.AkNotaDebitKreditDiterimaObjek != null && objAsal.AkNotaDebitKreditDiterimaObjek.Count > 0)
                    {
                        foreach (var item in objAsal.AkNotaDebitKreditDiterimaObjek)
                        {
                            var model = _context.AkNotaDebitKreditDiterimaObjek.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null) _context.Remove(model);
                        }
                    }

                    if (objAsal.AkNotaDebitKreditDiterimaPerihal != null && objAsal.AkNotaDebitKreditDiterimaPerihal.Count > 0)
                    {
                        foreach (var item in objAsal.AkNotaDebitKreditDiterimaPerihal)
                        {
                            var model = _context.AkNotaDebitKreditDiterimaPerihal.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null) _context.Remove(model);
                        }
                    }

                    _context.Entry(objAsal).State = EntityState.Detached;

                    akNotaDebitKreditDiterima.UserIdKemaskini = user?.UserName ?? "";
                    akNotaDebitKreditDiterima.TarKemaskini = DateTime.Now;
                    akNotaDebitKreditDiterima.AkNotaDebitKreditDiterimaObjek = _cart.AkNotaDebitKreditDiterimaObjek?.ToList();
                    akNotaDebitKreditDiterima.AkNotaDebitKreditDiterimaPerihal = _cart.AkNotaDebitKreditDiterimaPerihal.ToList();

                    if (akNotaDebitKreditDiterima.AkNotaDebitKreditDiterimaPerihal != null && akNotaDebitKreditDiterima.AkNotaDebitKreditDiterimaPerihal.Any())
                    {
                        decimal jumlahCukai = 0;
                        decimal jumlahTanpaCukai = 0;
                        foreach (var item in akNotaDebitKreditDiterima.AkNotaDebitKreditDiterimaPerihal)
                        {
                            jumlahCukai += item.AmaunCukai;
                            jumlahTanpaCukai += (item.Harga * item.Kuantiti);
                        }

                        akNotaDebitKreditDiterima.JumlahCukai = jumlahCukai;
                        akNotaDebitKreditDiterima.JumlahTanpaCukai = jumlahTanpaCukai;
                    }

                    _unitOfWork.AkNotaDebitKreditDiterimaRepo.Update(akNotaDebitKreditDiterima);

                    if (jumlahAsal != akNotaDebitKreditDiterima.Jumlah)
                    {
                        _appLog.Insert("Ubah", Convert.ToDecimal(jumlahAsal).ToString("#,##0.00") + " -> " + Convert.ToDecimal(akNotaDebitKreditDiterima.Jumlah).ToString("#,##0.00") + " : " + akNotaDebitKreditDiterima.NoRujukan ?? "", akNotaDebitKreditDiterima.NoRujukan ?? "", id, akNotaDebitKreditDiterima.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }
                    else
                    {
                        _appLog.Insert("Ubah", akNotaDebitKreditDiterima.NoRujukan ?? "", akNotaDebitKreditDiterima.NoRujukan ?? "", id, akNotaDebitKreditDiterima.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }

                    await _context.SaveChangesAsync();

                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AkNotaDebitKreditDiterimaExist(akNotaDebitKreditDiterima.Id))
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

            PopulateDropDownList(akNotaDebitKreditDiterima.JKWId);
            PopulateListViewFromCart();
            return View(akNotaDebitKreditDiterima);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "D")]
        public async Task<IActionResult> DeleteConfirmed(int id, string sebabHapus, string syscode)
        {
            var akNotaDebitKreditDiterima = _unitOfWork.AkNotaDebitKreditDiterimaRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akNotaDebitKreditDiterima != null && await _unitOfWork.AkNotaDebitKreditDiterimaRepo.IsLulusAsync(id) == false)
            {
                akNotaDebitKreditDiterima.UserIdKemaskini = user?.UserName ?? "";
                akNotaDebitKreditDiterima.TarKemaskini = DateTime.Now;
                akNotaDebitKreditDiterima.DPekerjaKemaskiniId = pekerjaId;
                akNotaDebitKreditDiterima.SebabHapus = sebabHapus;

                _context.AkNotaDebitKreditDiterima.Remove(akNotaDebitKreditDiterima);
                _appLog.Insert("Hapus", akNotaDebitKreditDiterima.NoRujukan ?? "", akNotaDebitKreditDiterima.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var obj = await _context.AkNotaDebitKreditDiterima.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.AkNotaDebitKreditDiterima.Update(obj);

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


                    // get akNotaDebitKreditDiterima 
                    var akNotaDebitKreditDiterima = _unitOfWork.AkNotaDebitKreditDiterimaRepo.GetDetailsById((int)id);
                    if (akNotaDebitKreditDiterima == null)
                    {
                        TempData[SD.Error] = "Data tidak wujud.";
                    }
                    else
                    {

                        if (akNotaDebitKreditDiterima.NoRujukan != null)
                        {
                            // check is it posted or not
                            if (await _unitOfWork.AkNotaDebitKreditDiterimaRepo.IsPostedAsync((int)id, akNotaDebitKreditDiterima.NoRujukan) == false)
                            {
                                TempData[SD.Error] = "Data belum diposting.";
                                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                            }

                            // posting start here
                            _unitOfWork.AkNotaDebitKreditDiterimaRepo.RemovePostingFromAbBukuVot(akNotaDebitKreditDiterima, user?.UserName ?? "");
                            _unitOfWork.AkNotaDebitKreditDiterimaRepo.RemovePostingFromAkAkaun(akNotaDebitKreditDiterima);

                            //insert applog
                            _appLog.Insert("UnPosting", "UnPosting Data", akNotaDebitKreditDiterima.NoRujukan, (int)id, akNotaDebitKreditDiterima.Jumlah, pekerjaId, modul, syscode, namamodul, user);

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

        private void PopulateDropDownList(int JKWId)
        {
            ViewBag.JKW = _unitOfWork.JKWRepo.GetAll();
            ViewBag.AkCarta = _unitOfWork.AkCartaRepo.GetResultsByParas(EnParas.Paras4);
            ViewBag.JKWPTJBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetails();
            ViewBag.JKWPTJBahagianByJKW = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsByJKWId(JKWId);
            ViewBag.AkBelian = _unitOfWork.AkBelianRepo.GetAllByStatus(EnStatusBorang.Lulus);
            ViewBag.EnLHDNJenisCukai = EnumHelper<EnLHDNJenisCukai>.GetList();
            ViewBag.LHDNMSIC = _unitOfWork.LHDNMSICRepo.GetAll();
            ViewBag.LHDNKodKlasifikasi = _unitOfWork.LHDNKodKlasifikasiRepo.GetAll();
            ViewBag.LHDNUnitUkuran = _unitOfWork.LHDNUnitUkuranRepo.GetAll();
        }

        private bool AkNotaDebitKreditDiterimaExist(int id)
        {
            return _unitOfWork.AkNotaDebitKreditDiterimaRepo.IsExist(b => b.Id == id);
        }
        private void PopulateCartAkNotaDebitKreditDiterimaFromDb(AkNotaDebitKreditDiterima akNotaDebitKreditDiterima)
        {
            if (akNotaDebitKreditDiterima.AkNotaDebitKreditDiterimaObjek != null)
            {
                foreach (var item in akNotaDebitKreditDiterima.AkNotaDebitKreditDiterimaObjek)
                {
                    _cart.AddItemObjek(
                            akNotaDebitKreditDiterima.Id,
                            item.JKWPTJBahagianId,
                            item.AkCartaId,
                            item.Amaun);
                }
            }

            if (akNotaDebitKreditDiterima.AkNotaDebitKreditDiterimaPerihal != null)
            {
                foreach (var item in akNotaDebitKreditDiterima.AkNotaDebitKreditDiterimaPerihal)
                {
                    _cart.AddItemPerihal(
                        akNotaDebitKreditDiterima.Id,
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
            List<AkNotaDebitKreditDiterimaObjek> objek = _cart.AkNotaDebitKreditDiterimaObjek.ToList();

            foreach (AkNotaDebitKreditDiterimaObjek item in objek)
            {
                var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                item.JKWPTJBahagian = jkwPtjBahagian;

                item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                item.AkCarta = akCarta;

            }

            ViewBag.akNotaDebitKreditDiterimaObjek = objek;

            List<AkNotaDebitKreditDiterimaPerihal> perihal = _cart.AkNotaDebitKreditDiterimaPerihal.ToList();

            ViewBag.akNotaDebitKreditDiterimaPerihal = perihal;
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

        public JsonResult GetAkBelianDetails(int id, int? akNotaDebitKreditDiterimaId)
        {
            try
            {
                EmptyCart();
                var data = _unitOfWork.AkBelianRepo.GetDetailsById(id);

                if (data != null)
                {
                    if (data.AkBelianObjek != null)
                    {
                        foreach (var item in data.AkBelianObjek)
                        {
                            _cart.AddItemObjek(
                                    akNotaDebitKreditDiterimaId ?? 0,
                                    item.JKWPTJBahagianId,
                                    item.AkCartaId,
                                    item.Amaun);
                        }
                    }

                    if (data.AkBelianPerihal != null)
                    {
                        foreach (var item in data.AkBelianPerihal)
                        {
                            _cart.AddItemPerihal(
                                akNotaDebitKreditDiterimaId ?? 0,
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

        public JsonResult SaveCartAkNotaDebitKreditDiterimaObjek(AkNotaDebitKreditDiterimaObjek akNotaDebitKreditDiterimaObjek)
        {
            try
            {
                var jCukai = new JCukai();
                if (akNotaDebitKreditDiterimaObjek != null)
                {
                    _cart.AddItemObjek(akNotaDebitKreditDiterimaObjek.AkNotaDebitKreditDiterimaId, akNotaDebitKreditDiterimaObjek.JKWPTJBahagianId, akNotaDebitKreditDiterimaObjek.AkCartaId, akNotaDebitKreditDiterimaObjek.Amaun);


                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkNotaDebitKreditDiterimaObjek(AkNotaDebitKreditDiterimaObjek akNotaDebitKreditDiterimaObjek)
        {
            try
            {
                if (akNotaDebitKreditDiterimaObjek != null)
                {
                    _cart.RemoveItemObjek(akNotaDebitKreditDiterimaObjek.JKWPTJBahagianId, akNotaDebitKreditDiterimaObjek.AkCartaId);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkNotaDebitKreditDiterimaObjek(AkNotaDebitKreditDiterimaObjek akNotaDebitKreditDiterimaObjek)
        {

            try
            {
                AkNotaDebitKreditDiterimaObjek data = _cart.AkNotaDebitKreditDiterimaObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akNotaDebitKreditDiterimaObjek.JKWPTJBahagianId && x.AkCartaId == akNotaDebitKreditDiterimaObjek.AkCartaId) ?? new AkNotaDebitKreditDiterimaObjek();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkNotaDebitKreditDiterimaObjek(AkNotaDebitKreditDiterimaObjek akNotaDebitKreditDiterimaObjek)
        {

            try
            {

                var data = _cart.AkNotaDebitKreditDiterimaObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akNotaDebitKreditDiterimaObjek.JKWPTJBahagianId && x.AkCartaId == akNotaDebitKreditDiterimaObjek.AkCartaId);

                var user = _userManager.GetUserName(User);

                if (data != null)
                {
                    _cart.RemoveItemObjek(akNotaDebitKreditDiterimaObjek.JKWPTJBahagianId, akNotaDebitKreditDiterimaObjek.AkCartaId);

                    _cart.AddItemObjek(akNotaDebitKreditDiterimaObjek.AkNotaDebitKreditDiterimaId,
                                    akNotaDebitKreditDiterimaObjek.JKWPTJBahagianId,
                                    akNotaDebitKreditDiterimaObjek.AkCartaId,
                                    akNotaDebitKreditDiterimaObjek.Amaun);
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
                var akNotaDebitKreditDiterima = _cart.AkNotaDebitKreditDiterimaPerihal.FirstOrDefault(pp => pp.Bil == Bil);
                if (akNotaDebitKreditDiterima != null)
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

        public JsonResult SaveCartAkNotaDebitKreditDiterimaPerihal(AkNotaDebitKreditDiterimaPerihal akNotaDebitKreditDiterimaPerihal)
        {
            try
            {
                if (akNotaDebitKreditDiterimaPerihal != null)
                {
                    _cart.AddItemPerihal(akNotaDebitKreditDiterimaPerihal.AkNotaDebitKreditDiterimaId, akNotaDebitKreditDiterimaPerihal.Bil, akNotaDebitKreditDiterimaPerihal.Perihal, akNotaDebitKreditDiterimaPerihal.Kuantiti, akNotaDebitKreditDiterimaPerihal.LHDNKodKlasifikasiId ?? _unitOfWork.LHDNKodKlasifikasiRepo.GetByCodeAsync("022").Result.Id, akNotaDebitKreditDiterimaPerihal.LHDNUnitUkuranId ?? _unitOfWork.LHDNUnitUkuranRepo.GetByCodeAsync("C62").Result.Id, akNotaDebitKreditDiterimaPerihal.Unit, akNotaDebitKreditDiterimaPerihal.EnLHDNJenisCukai, akNotaDebitKreditDiterimaPerihal.KadarCukai, akNotaDebitKreditDiterimaPerihal.AmaunCukai, akNotaDebitKreditDiterimaPerihal.Harga, akNotaDebitKreditDiterimaPerihal.Amaun);
                }



                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkNotaDebitKreditDiterimaPerihal(AkNotaDebitKreditDiterimaPerihal akNotaDebitKreditDiterimaPerihal)
        {
            try
            {
                if (akNotaDebitKreditDiterimaPerihal != null)
                {
                    _cart.RemoveItemPerihal(akNotaDebitKreditDiterimaPerihal.Bil);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkNotaDebitKreditDiterimaPerihal(AkNotaDebitKreditDiterimaPerihal akNotaDebitKreditDiterimaPerihal)
        {

            try
            {
                AkNotaDebitKreditDiterimaPerihal data = _cart.AkNotaDebitKreditDiterimaPerihal.FirstOrDefault(x => x.Bil == akNotaDebitKreditDiterimaPerihal.Bil) ?? new AkNotaDebitKreditDiterimaPerihal();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkNotaDebitKreditDiterimaPerihal(AkNotaDebitKreditDiterimaPerihal akNotaDebitKreditDiterimaPerihal)
        {

            try
            {

                var akNotaDebitKreditDiterima = _cart.AkNotaDebitKreditDiterimaPerihal.FirstOrDefault(x => x.Bil == akNotaDebitKreditDiterimaPerihal.Bil);

                var user = _userManager.GetUserName(User);

                if (akNotaDebitKreditDiterima != null)
                {
                    _cart.RemoveItemPerihal(akNotaDebitKreditDiterimaPerihal.Bil);

                    _cart.AddItemPerihal(akNotaDebitKreditDiterimaPerihal.AkNotaDebitKreditDiterimaId, akNotaDebitKreditDiterimaPerihal.Bil, akNotaDebitKreditDiterimaPerihal.Perihal, akNotaDebitKreditDiterimaPerihal.Kuantiti, akNotaDebitKreditDiterimaPerihal.LHDNKodKlasifikasiId ?? _unitOfWork.LHDNKodKlasifikasiRepo.GetByCodeAsync("022").Result.Id, akNotaDebitKreditDiterimaPerihal.LHDNUnitUkuranId ?? _unitOfWork.LHDNUnitUkuranRepo.GetByCodeAsync("C62").Result.Id, akNotaDebitKreditDiterimaPerihal.Unit, akNotaDebitKreditDiterimaPerihal.EnLHDNJenisCukai, akNotaDebitKreditDiterimaPerihal.KadarCukai, akNotaDebitKreditDiterimaPerihal.AmaunCukai, akNotaDebitKreditDiterimaPerihal.Harga, akNotaDebitKreditDiterimaPerihal.Amaun);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAllItemCartAkNotaDebitKreditDiterima()
        {

            try
            {
                List<AkNotaDebitKreditDiterimaObjek> objek = _cart.AkNotaDebitKreditDiterimaObjek.ToList();

                foreach (AkNotaDebitKreditDiterimaObjek item in objek)
                {
                    var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    item.JKWPTJBahagian = jkwPtjBahagian;

                    item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                    var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                    item.AkCarta = akCarta;

                }

                List<AkNotaDebitKreditDiterimaPerihal> perihal = _cart.AkNotaDebitKreditDiterimaPerihal.ToList();

                return Json(new { result = "OK", objek = objek.OrderBy(d => d.AkCarta?.Kod), perihal = perihal.OrderBy(d => d.Bil) });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }
    }
}
