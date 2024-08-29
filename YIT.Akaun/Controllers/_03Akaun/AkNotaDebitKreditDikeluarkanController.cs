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
    public class AkNotaDebitKreditDikeluarkanController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodAkNotaDebitKreditDikeluarkan;
        public const string namamodul = Modules.namaAkNotaDebitKreditDikeluarkan;
        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;
        private readonly CartAkNotaDebitKreditDikeluarkan _cart;

        public AkNotaDebitKreditDikeluarkanController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices,
            CartAkNotaDebitKreditDikeluarkan cart
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

            var akNotaDebitKreditDikeluarkan = _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.GetResults(searchString, date1, date2, searchColumn, EnStatusBorang.Semua);

            return View(akNotaDebitKreditDikeluarkan);
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

            var akNotaDebitKreditDikeluarkan = _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.GetDetailsById((int)id);
            if (akNotaDebitKreditDikeluarkan == null)
            {
                return NotFound();
            }
            EmptyCart();
            PopulateCartAkNotaDebitKreditDikeluarkanFromDb(akNotaDebitKreditDikeluarkan);
            return View(akNotaDebitKreditDikeluarkan);
        }

        [Authorize(Policy = modul + "D")]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akNotaDebitKreditDikeluarkan = _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.GetDetailsById((int)id);
            if (akNotaDebitKreditDikeluarkan == null)
            {
                return NotFound();
            }

            if (akNotaDebitKreditDikeluarkan.EnStatusBorang != EnStatusBorang.None)
            {
                TempData[SD.Error] = "Hapus data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkNotaDebitKreditDikeluarkanFromDb(akNotaDebitKreditDikeluarkan);
            return View(akNotaDebitKreditDikeluarkan);
        }

        [Authorize(Policy = modul + "BL")]
        public IActionResult BatalLulus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akNotaDebitKreditDikeluarkan = _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.GetDetailsById((int)id);
            if (akNotaDebitKreditDikeluarkan == null)
            {
                return NotFound();
            }

            if (akNotaDebitKreditDikeluarkan.EnStatusBorang != EnStatusBorang.Lulus)
            {
                TempData[SD.Error] = "Data belum diluluskan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkNotaDebitKreditDikeluarkanFromDb(akNotaDebitKreditDikeluarkan);
            return View(akNotaDebitKreditDikeluarkan);
        }

        [HttpPost, ActionName("BatalLulus")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "BL")]
        public async Task<IActionResult> BatalLulusConfirmed(int id, string tindakan, string syscode)
        {
            var akNotaDebitKreditDikeluarkan = _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akNotaDebitKreditDikeluarkan != null && !string.IsNullOrEmpty(akNotaDebitKreditDikeluarkan.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.IsPostedAsync((int)id, akNotaDebitKreditDikeluarkan.NoRujukan) == false)
                {
                    TempData[SD.Error] = "Data belum diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.IsLulusAsync(id) == false)
                {
                    TempData[SD.Error] = "Data belum diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.BatalLulus(id, tindakan, user?.Email);

                _appLog.Insert("UnPosting", "Batal Lulus " + akNotaDebitKreditDikeluarkan.NoRujukan ?? "", akNotaDebitKreditDikeluarkan.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var akNotaDebitKreditDikeluarkan = _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.GetDetailsById((int)id);
            if (akNotaDebitKreditDikeluarkan == null)
            {
                return NotFound();
            }

            if (akNotaDebitKreditDikeluarkan.EnStatusBorang != EnStatusBorang.Lulus)
            {
                TempData[SD.Error] = "Data belum diluluskan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkNotaDebitKreditDikeluarkanFromDb(akNotaDebitKreditDikeluarkan);
            return View(akNotaDebitKreditDikeluarkan);
        }

        [HttpPost, ActionName("BatalPos")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "BL")]
        public async Task<IActionResult> BatalPosConfirmed(int id, string tindakan, string syscode)
        {
            var akNotaDebitKreditDikeluarkan = _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akNotaDebitKreditDikeluarkan != null && !string.IsNullOrEmpty(akNotaDebitKreditDikeluarkan.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.IsPostedAsync((int)id, akNotaDebitKreditDikeluarkan.NoRujukan) == false)
                {
                    TempData[SD.Error] = "Data belum diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.IsLulusAsync(id) == false)
                {
                    TempData[SD.Error] = "Data belum diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.BatalPos(id, tindakan, user?.UserName);

                _appLog.Insert("UnPosting", "Batal Pos " + akNotaDebitKreditDikeluarkan.NoRujukan ?? "", akNotaDebitKreditDikeluarkan.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var obj = await _context.AkNotaDebitKreditDikeluarkan.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Pos operation

            if (obj != null && !string.IsNullOrEmpty(obj.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.IsPostedAsync((int)id, obj.NoRujukan))
                {
                    TempData[SD.Error] = "Data sudah diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.IsLulusAsync(id))
                {
                    TempData[SD.Error] = "Data telah diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.Lulus(id, pekerjaId, user?.UserName);

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
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.NK.GetDisplayName(), DateTime.Now.ToString("yyyy"));
            return View();
        }
        private string GenerateRunningNumber(string initNoRujukan, string tahun)
        {
            var maxRefNo = _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.GetMaxRefNo(initNoRujukan, tahun);

            var prefix = initNoRujukan + "/" + tahun + "/";
            return RunningNumberFormatter.GenerateRunningNumber(prefix, maxRefNo, "00000");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "C")]
        public async Task<IActionResult> Create(AkNotaDebitKreditDikeluarkan akNotaDebitKreditDikeluarkan, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            // check if there is pelulus available or not based on modul, kelulusan, and bahagian
            if (_cart.AkNotaDebitKreditDikeluarkanObjek != null && _cart.AkNotaDebitKreditDikeluarkanObjek.Count() > 0)
            {
                foreach (var item in _cart.AkNotaDebitKreditDikeluarkanObjek)
                {
                    var jKWPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModulKelulusan.NotaDebitKreditDikeluarkan, EnKategoriKelulusan.Pelulus, jKWPtjBahagian.JBahagianId, akNotaDebitKreditDikeluarkan.Jumlah) == false)
                    {
                        TempData[SD.Error] = "Tiada Pelulus yang wujud untuk senarai kod bahagian berikut.";
                        ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.NK.GetDisplayName(), akNotaDebitKreditDikeluarkan.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
                        PopulateDropDownList(akNotaDebitKreditDikeluarkan.JKWId);
                        PopulateListViewFromCart();
                        return View(akNotaDebitKreditDikeluarkan);
                    }
                }
            }
            //

            if (ModelState.IsValid)
            {

                akNotaDebitKreditDikeluarkan.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.NK.GetDisplayName(), akNotaDebitKreditDikeluarkan.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
                akNotaDebitKreditDikeluarkan.UserId = user?.UserName ?? "";
                akNotaDebitKreditDikeluarkan.TarMasuk = DateTime.Now;
                akNotaDebitKreditDikeluarkan.DPekerjaMasukId = pekerjaId;

                akNotaDebitKreditDikeluarkan.AkNotaDebitKreditDikeluarkanObjek = _cart.AkNotaDebitKreditDikeluarkanObjek?.ToList();
                akNotaDebitKreditDikeluarkan.AkNotaDebitKreditDikeluarkanPerihal = _cart.AkNotaDebitKreditDikeluarkanPerihal.ToList();

                if (akNotaDebitKreditDikeluarkan.AkNotaDebitKreditDikeluarkanPerihal != null && akNotaDebitKreditDikeluarkan.AkNotaDebitKreditDikeluarkanPerihal.Any())
                {
                    decimal jumlahCukai = 0;
                    decimal jumlahTanpaCukai = 0;
                    foreach (var item in akNotaDebitKreditDikeluarkan.AkNotaDebitKreditDikeluarkanPerihal)
                    {
                        jumlahCukai += item.AmaunCukai;
                        jumlahTanpaCukai += (item.Harga * item.Kuantiti);
                    }

                    akNotaDebitKreditDikeluarkan.JumlahCukai = jumlahCukai;
                    akNotaDebitKreditDikeluarkan.JumlahTanpaCukai = jumlahTanpaCukai;
                }

                _context.Add(akNotaDebitKreditDikeluarkan);
                _appLog.Insert("Tambah", akNotaDebitKreditDikeluarkan.NoRujukan ?? "", akNotaDebitKreditDikeluarkan.NoRujukan ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya ditambah..!";
                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
            }
            PopulateDropDownList(akNotaDebitKreditDikeluarkan.JKWId);
            PopulateListViewFromCart();
            GenerateRunningNumber(EnInitNoRujukan.NK.GetDisplayName(), akNotaDebitKreditDikeluarkan.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
            return View(akNotaDebitKreditDikeluarkan);
        }

        [Authorize(Policy = modul + "E")]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akNotaDebitKreditDikeluarkan = _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.GetDetailsById((int)id);
            if (akNotaDebitKreditDikeluarkan == null)
            {
                return NotFound();
            }

            if (akNotaDebitKreditDikeluarkan.EnStatusBorang != EnStatusBorang.None && akNotaDebitKreditDikeluarkan.EnStatusBorang != EnStatusBorang.Kemaskini)
            {
                TempData[SD.Error] = "Ubah data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }

            EmptyCart();
            PopulateDropDownList(akNotaDebitKreditDikeluarkan.JKWId);
            PopulateCartAkNotaDebitKreditDikeluarkanFromDb(akNotaDebitKreditDikeluarkan);
            return View(akNotaDebitKreditDikeluarkan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "E")]
        public async Task<IActionResult> Edit(int id, AkNotaDebitKreditDikeluarkan akNotaDebitKreditDikeluarkan, string? fullName, string syscode)
        {
            if (id != akNotaDebitKreditDikeluarkan.Id)
            {
                return NotFound();
            }

            // check if there is pelulus available or not based on modul, kelulusan, and bahagian
            if (_cart.AkNotaDebitKreditDikeluarkanObjek != null && _cart.AkNotaDebitKreditDikeluarkanObjek.Count() > 0)
            {
                foreach (var item in _cart.AkNotaDebitKreditDikeluarkanObjek)
                {
                    var jKWPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModulKelulusan.NotaDebitKreditDikeluarkan, EnKategoriKelulusan.Pelulus, jKWPtjBahagian.JBahagianId, akNotaDebitKreditDikeluarkan.Jumlah) == false)
                    {
                        TempData[SD.Error] = "Tiada Pelulus yang wujud untuk senarai kod bahagian berikut.";
                        PopulateDropDownList(akNotaDebitKreditDikeluarkan.JKWId);
                        PopulateListViewFromCart();
                        return View(akNotaDebitKreditDikeluarkan);
                    }
                }
            }
            //

            if (akNotaDebitKreditDikeluarkan.NoRujukan != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.GetDetailsById(id);
                    var jumlahAsal = objAsal!.Jumlah;
                    akNotaDebitKreditDikeluarkan.NoRujukan = objAsal.NoRujukan;
                    akNotaDebitKreditDikeluarkan.JKWId = objAsal.JKWId;
                    akNotaDebitKreditDikeluarkan.AkInvoisId = objAsal.AkInvoisId;
                    akNotaDebitKreditDikeluarkan.UserId = objAsal.UserId;
                    akNotaDebitKreditDikeluarkan.TarMasuk = objAsal.TarMasuk;
                    akNotaDebitKreditDikeluarkan.DPekerjaMasukId = objAsal.DPekerjaMasukId;
                    akNotaDebitKreditDikeluarkan.EnStatusBorang = objAsal.EnStatusBorang;

                    if (objAsal.AkNotaDebitKreditDikeluarkanObjek != null && objAsal.AkNotaDebitKreditDikeluarkanObjek.Count > 0)
                    {
                        foreach (var item in objAsal.AkNotaDebitKreditDikeluarkanObjek)
                        {
                            var model = _context.AkNotaDebitKreditDikeluarkanObjek.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null) _context.Remove(model);
                        }
                    }

                    if (objAsal.AkNotaDebitKreditDikeluarkanPerihal != null && objAsal.AkNotaDebitKreditDikeluarkanPerihal.Count > 0)
                    {
                        foreach (var item in objAsal.AkNotaDebitKreditDikeluarkanPerihal)
                        {
                            var model = _context.AkNotaDebitKreditDikeluarkanPerihal.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null) _context.Remove(model);
                        }
                    }

                    _context.Entry(objAsal).State = EntityState.Detached;

                    akNotaDebitKreditDikeluarkan.UserIdKemaskini = user?.UserName ?? "";
                    akNotaDebitKreditDikeluarkan.TarKemaskini = DateTime.Now;
                    akNotaDebitKreditDikeluarkan.AkNotaDebitKreditDikeluarkanObjek = _cart.AkNotaDebitKreditDikeluarkanObjek?.ToList();
                    akNotaDebitKreditDikeluarkan.AkNotaDebitKreditDikeluarkanPerihal = _cart.AkNotaDebitKreditDikeluarkanPerihal.ToList();

                    if (akNotaDebitKreditDikeluarkan.AkNotaDebitKreditDikeluarkanPerihal != null && akNotaDebitKreditDikeluarkan.AkNotaDebitKreditDikeluarkanPerihal.Any())
                    {
                        decimal jumlahCukai = 0;
                        decimal jumlahTanpaCukai = 0;
                        foreach (var item in akNotaDebitKreditDikeluarkan.AkNotaDebitKreditDikeluarkanPerihal)
                        {
                            jumlahCukai += item.AmaunCukai;
                            jumlahTanpaCukai += (item.Harga * item.Kuantiti);
                        }

                        akNotaDebitKreditDikeluarkan.JumlahCukai = jumlahCukai;
                        akNotaDebitKreditDikeluarkan.JumlahTanpaCukai = jumlahTanpaCukai;
                    }

                    _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.Update(akNotaDebitKreditDikeluarkan);

                    if (jumlahAsal != akNotaDebitKreditDikeluarkan.Jumlah)
                    {
                        _appLog.Insert("Ubah", Convert.ToDecimal(jumlahAsal).ToString("#,##0.00") + " -> " + Convert.ToDecimal(akNotaDebitKreditDikeluarkan.Jumlah).ToString("#,##0.00") + " : " + akNotaDebitKreditDikeluarkan.NoRujukan ?? "", akNotaDebitKreditDikeluarkan.NoRujukan ?? "", id, akNotaDebitKreditDikeluarkan.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }
                    else
                    {
                        _appLog.Insert("Ubah", akNotaDebitKreditDikeluarkan.NoRujukan ?? "", akNotaDebitKreditDikeluarkan.NoRujukan ?? "", id, akNotaDebitKreditDikeluarkan.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }

                    await _context.SaveChangesAsync();

                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AkNotaDebitKreditDikeluarkanExist(akNotaDebitKreditDikeluarkan.Id))
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

            PopulateDropDownList(akNotaDebitKreditDikeluarkan.JKWId);
            PopulateListViewFromCart();
            return View(akNotaDebitKreditDikeluarkan);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "D")]
        public async Task<IActionResult> DeleteConfirmed(int id, string sebabHapus, string syscode)
        {
            var akNotaDebitKreditDikeluarkan = _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akNotaDebitKreditDikeluarkan != null && await _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.IsLulusAsync(id) == false)
            {
                akNotaDebitKreditDikeluarkan.UserIdKemaskini = user?.UserName ?? "";
                akNotaDebitKreditDikeluarkan.TarKemaskini = DateTime.Now;
                akNotaDebitKreditDikeluarkan.DPekerjaKemaskiniId = pekerjaId;
                akNotaDebitKreditDikeluarkan.SebabHapus = sebabHapus;

                _context.AkNotaDebitKreditDikeluarkan.Remove(akNotaDebitKreditDikeluarkan);
                _appLog.Insert("Hapus", akNotaDebitKreditDikeluarkan.NoRujukan ?? "", akNotaDebitKreditDikeluarkan.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var obj = await _context.AkNotaDebitKreditDikeluarkan.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.AkNotaDebitKreditDikeluarkan.Update(obj);

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


                    // get akNotaDebitKreditDikeluarkan 
                    var akNotaDebitKreditDikeluarkan = _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.GetDetailsById((int)id);
                    if (akNotaDebitKreditDikeluarkan == null)
                    {
                        TempData[SD.Error] = "Data tidak wujud.";
                    }
                    else
                    {

                        if (akNotaDebitKreditDikeluarkan.NoRujukan != null)
                        {
                            // check is it posted or not
                            if (await _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.IsPostedAsync((int)id, akNotaDebitKreditDikeluarkan.NoRujukan) == false)
                            {
                                TempData[SD.Error] = "Data belum diposting.";
                                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                            }

                            // posting start here
                            _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.RemovePostingFromAkAkaun(akNotaDebitKreditDikeluarkan);

                            //insert applog
                            _appLog.Insert("UnPosting", "UnPosting Data", akNotaDebitKreditDikeluarkan.NoRujukan, (int)id, akNotaDebitKreditDikeluarkan.Jumlah, pekerjaId, modul, syscode, namamodul, user);

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
            ViewBag.AkInvois = _unitOfWork.AkInvoisRepo.GetAllByStatus(EnStatusBorang.Lulus);
            ViewBag.EnLHDNJenisCukai = EnumHelper<EnLHDNJenisCukai>.GetList();
            ViewBag.LHDNMSIC = _unitOfWork.LHDNMSICRepo.GetAll();
            ViewBag.LHDNKodKlasifikasi = _unitOfWork.LHDNKodKlasifikasiRepo.GetAll();
            ViewBag.LHDNUnitUkuran = _unitOfWork.LHDNUnitUkuranRepo.GetAll();
        }

        private bool AkNotaDebitKreditDikeluarkanExist(int id)
        {
            return _unitOfWork.AkNotaDebitKreditDikeluarkanRepo.IsExist(b => b.Id == id);
        }
        private void PopulateCartAkNotaDebitKreditDikeluarkanFromDb(AkNotaDebitKreditDikeluarkan akNotaDebitKreditDikeluarkan)
        {
            if (akNotaDebitKreditDikeluarkan.AkNotaDebitKreditDikeluarkanObjek != null)
            {
                foreach (var item in akNotaDebitKreditDikeluarkan.AkNotaDebitKreditDikeluarkanObjek)
                {
                    _cart.AddItemObjek(
                            akNotaDebitKreditDikeluarkan.Id,
                            item.JKWPTJBahagianId,
                            item.AkCartaId,
                            item.Amaun);
                }
            }

            if (akNotaDebitKreditDikeluarkan.AkNotaDebitKreditDikeluarkanPerihal != null)
            {
                foreach (var item in akNotaDebitKreditDikeluarkan.AkNotaDebitKreditDikeluarkanPerihal)
                {
                    _cart.AddItemPerihal(
                        akNotaDebitKreditDikeluarkan.Id,
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
            List<AkNotaDebitKreditDikeluarkanObjek> objek = _cart.AkNotaDebitKreditDikeluarkanObjek.ToList();

            foreach (AkNotaDebitKreditDikeluarkanObjek item in objek)
            {
                var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                item.JKWPTJBahagian = jkwPtjBahagian;

                item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                item.AkCarta = akCarta;

            }

            ViewBag.akNotaDebitKreditDikeluarkanObjek = objek;

            List<AkNotaDebitKreditDikeluarkanPerihal> perihal = _cart.AkNotaDebitKreditDikeluarkanPerihal.ToList();

            ViewBag.akNotaDebitKreditDikeluarkanPerihal = perihal;
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

        public JsonResult GetAkInvoisDetails(int id, int? akNotaDebitKreditDikeluarkanId)
        {
            try
            {
                EmptyCart();
                var data = _unitOfWork.AkInvoisRepo.GetDetailsById(id);

                if (data != null)
                {
                    if (data.AkInvoisObjek != null)
                    {
                        foreach (var item in data.AkInvoisObjek)
                        {
                            _cart.AddItemObjek(
                                    akNotaDebitKreditDikeluarkanId ?? 0,
                                    item.JKWPTJBahagianId,
                                    item.AkCartaId,
                                    item.Amaun);
                        }
                    }

                    if (data.AkInvoisPerihal != null)
                    {
                        foreach (var item in data.AkInvoisPerihal)
                        {
                            _cart.AddItemPerihal(
                                akNotaDebitKreditDikeluarkanId ?? 0,
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

        public JsonResult SaveCartAkNotaDebitKreditDikeluarkanObjek(AkNotaDebitKreditDikeluarkanObjek akNotaDebitKreditDikeluarkanObjek)
        {
            try
            {
                var jCukai = new JCukai();
                if (akNotaDebitKreditDikeluarkanObjek != null)
                {
                    _cart.AddItemObjek(akNotaDebitKreditDikeluarkanObjek.AkNotaDebitKreditDikeluarkanId, akNotaDebitKreditDikeluarkanObjek.JKWPTJBahagianId, akNotaDebitKreditDikeluarkanObjek.AkCartaId, akNotaDebitKreditDikeluarkanObjek.Amaun);


                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkNotaDebitKreditDikeluarkanObjek(AkNotaDebitKreditDikeluarkanObjek akNotaDebitKreditDikeluarkanObjek)
        {
            try
            {
                if (akNotaDebitKreditDikeluarkanObjek != null)
                {
                    _cart.RemoveItemObjek(akNotaDebitKreditDikeluarkanObjek.JKWPTJBahagianId, akNotaDebitKreditDikeluarkanObjek.AkCartaId);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkNotaDebitKreditDikeluarkanObjek(AkNotaDebitKreditDikeluarkanObjek akNotaDebitKreditDikeluarkanObjek)
        {

            try
            {
                AkNotaDebitKreditDikeluarkanObjek data = _cart.AkNotaDebitKreditDikeluarkanObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akNotaDebitKreditDikeluarkanObjek.JKWPTJBahagianId && x.AkCartaId == akNotaDebitKreditDikeluarkanObjek.AkCartaId) ?? new AkNotaDebitKreditDikeluarkanObjek();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkNotaDebitKreditDikeluarkanObjek(AkNotaDebitKreditDikeluarkanObjek akNotaDebitKreditDikeluarkanObjek)
        {

            try
            {

                var data = _cart.AkNotaDebitKreditDikeluarkanObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akNotaDebitKreditDikeluarkanObjek.JKWPTJBahagianId && x.AkCartaId == akNotaDebitKreditDikeluarkanObjek.AkCartaId);

                var user = _userManager.GetUserName(User);

                if (data != null)
                {
                    _cart.RemoveItemObjek(akNotaDebitKreditDikeluarkanObjek.JKWPTJBahagianId, akNotaDebitKreditDikeluarkanObjek.AkCartaId);

                    _cart.AddItemObjek(akNotaDebitKreditDikeluarkanObjek.AkNotaDebitKreditDikeluarkanId,
                                    akNotaDebitKreditDikeluarkanObjek.JKWPTJBahagianId,
                                    akNotaDebitKreditDikeluarkanObjek.AkCartaId,
                                    akNotaDebitKreditDikeluarkanObjek.Amaun);
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
                var akNotaDebitKreditDikeluarkan = _cart.AkNotaDebitKreditDikeluarkanPerihal.FirstOrDefault(pp => pp.Bil == Bil);
                if (akNotaDebitKreditDikeluarkan != null)
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

        public JsonResult SaveCartAkNotaDebitKreditDikeluarkanPerihal(AkNotaDebitKreditDikeluarkanPerihal akNotaDebitKreditDikeluarkanPerihal)
        {
            try
            {
                if (akNotaDebitKreditDikeluarkanPerihal != null)
                {
                    _cart.AddItemPerihal(akNotaDebitKreditDikeluarkanPerihal.AkNotaDebitKreditDikeluarkanId, akNotaDebitKreditDikeluarkanPerihal.Bil, akNotaDebitKreditDikeluarkanPerihal.Perihal, akNotaDebitKreditDikeluarkanPerihal.Kuantiti, akNotaDebitKreditDikeluarkanPerihal.LHDNKodKlasifikasiId ?? _unitOfWork.LHDNKodKlasifikasiRepo.GetByCodeAsync("022").Result.Id, akNotaDebitKreditDikeluarkanPerihal.LHDNUnitUkuranId ?? _unitOfWork.LHDNUnitUkuranRepo.GetByCodeAsync("C62").Result.Id, akNotaDebitKreditDikeluarkanPerihal.Unit, akNotaDebitKreditDikeluarkanPerihal.EnLHDNJenisCukai, akNotaDebitKreditDikeluarkanPerihal.KadarCukai, akNotaDebitKreditDikeluarkanPerihal.AmaunCukai, akNotaDebitKreditDikeluarkanPerihal.Harga, akNotaDebitKreditDikeluarkanPerihal.Amaun);
                }



                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkNotaDebitKreditDikeluarkanPerihal(AkNotaDebitKreditDikeluarkanPerihal akNotaDebitKreditDikeluarkanPerihal)
        {
            try
            {
                if (akNotaDebitKreditDikeluarkanPerihal != null)
                {
                    _cart.RemoveItemPerihal(akNotaDebitKreditDikeluarkanPerihal.Bil);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkNotaDebitKreditDikeluarkanPerihal(AkNotaDebitKreditDikeluarkanPerihal akNotaDebitKreditDikeluarkanPerihal)
        {

            try
            {
                AkNotaDebitKreditDikeluarkanPerihal data = _cart.AkNotaDebitKreditDikeluarkanPerihal.FirstOrDefault(x => x.Bil == akNotaDebitKreditDikeluarkanPerihal.Bil) ?? new AkNotaDebitKreditDikeluarkanPerihal();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkNotaDebitKreditDikeluarkanPerihal(AkNotaDebitKreditDikeluarkanPerihal akNotaDebitKreditDikeluarkanPerihal)
        {

            try
            {

                var akNotaDebitKreditDikeluarkan = _cart.AkNotaDebitKreditDikeluarkanPerihal.FirstOrDefault(x => x.Bil == akNotaDebitKreditDikeluarkanPerihal.Bil);

                var user = _userManager.GetUserName(User);

                if (akNotaDebitKreditDikeluarkan != null)
                {
                    _cart.RemoveItemPerihal(akNotaDebitKreditDikeluarkanPerihal.Bil);

                    _cart.AddItemPerihal(akNotaDebitKreditDikeluarkanPerihal.AkNotaDebitKreditDikeluarkanId, akNotaDebitKreditDikeluarkanPerihal.Bil, akNotaDebitKreditDikeluarkanPerihal.Perihal, akNotaDebitKreditDikeluarkanPerihal.Kuantiti, akNotaDebitKreditDikeluarkanPerihal.LHDNKodKlasifikasiId ?? _unitOfWork.LHDNKodKlasifikasiRepo.GetByCodeAsync("022").Result.Id, akNotaDebitKreditDikeluarkanPerihal.LHDNUnitUkuranId ?? _unitOfWork.LHDNUnitUkuranRepo.GetByCodeAsync("C62").Result.Id, akNotaDebitKreditDikeluarkanPerihal.Unit, akNotaDebitKreditDikeluarkanPerihal.EnLHDNJenisCukai, akNotaDebitKreditDikeluarkanPerihal.KadarCukai, akNotaDebitKreditDikeluarkanPerihal.AmaunCukai, akNotaDebitKreditDikeluarkanPerihal.Harga, akNotaDebitKreditDikeluarkanPerihal.Amaun);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAllItemCartAkNotaDebitKreditDikeluarkan()
        {

            try
            {
                List<AkNotaDebitKreditDikeluarkanObjek> objek = _cart.AkNotaDebitKreditDikeluarkanObjek.ToList();

                foreach (AkNotaDebitKreditDikeluarkanObjek item in objek)
                {
                    var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    item.JKWPTJBahagian = jkwPtjBahagian;

                    item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                    var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                    item.AkCarta = akCarta;

                }

                List<AkNotaDebitKreditDikeluarkanPerihal> perihal = _cart.AkNotaDebitKreditDikeluarkanPerihal.ToList();

                return Json(new { result = "OK", objek = objek.OrderBy(d => d.AkCarta?.Kod), perihal = perihal.OrderBy(d => d.Bil) });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }
    }
}
