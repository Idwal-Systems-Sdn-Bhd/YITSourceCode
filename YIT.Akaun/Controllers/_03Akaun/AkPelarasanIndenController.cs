using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    [Authorize]
    public class AkPelarasanIndenController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodAkPelarasanInden;
        public const string namamodul = Modules.namaAkPelarasanInden;
        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;
        private readonly CartAkPelarasanInden _cart;

        public AkPelarasanIndenController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices,
            CartAkPelarasanInden cart)
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

            var akPelarasanInden = _unitOfWork.AkPelarasanIndenRepo.GetResults(searchString, date1, date2, searchColumn, EnStatusBorang.Semua);

            return View(akPelarasanInden);
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

        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akPelarasanInden = _unitOfWork.AkPelarasanIndenRepo.GetDetailsById((int)id);
            if (akPelarasanInden == null)
            {
                return NotFound();
            }
            EmptyCart();
            PopulateCartAkPelarasanIndenFromDb(akPelarasanInden);
            return View(akPelarasanInden);
        }

        private void PopulateCartAkPelarasanIndenFromDb(AkPelarasanInden akPelarasanInden)
        {
            if (akPelarasanInden.AkPelarasanIndenObjek != null)
            {
                foreach (var item in akPelarasanInden.AkPelarasanIndenObjek)
                {
                    _cart.AddItemObjek(
                            akPelarasanInden.Id,
                            item.JKWPTJBahagianId,
                            item.AkCartaId,
                            item.Amaun);
                }
            }

            if (akPelarasanInden.AkPelarasanIndenPerihal != null)
            {
                foreach (var item in akPelarasanInden.AkPelarasanIndenPerihal)
                {
                    _cart.AddItemPerihal(
                        akPelarasanInden.Id,
                        item.Bil,
                        item.Perihal,
                        item.Kuantiti,
                        item.Unit,
                        item.Harga,
                        item.Amaun
                        );
                }

                PopulateListViewFromCart();
            }
        }

        private void PopulateListViewFromCart()
        {
            List<AkPelarasanIndenObjek> objek = _cart.AkPelarasanIndenObjek.ToList();

            foreach (AkPelarasanIndenObjek item in objek)
            {
                var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                item.JKWPTJBahagian = jkwPtjBahagian;

                item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                item.AkCarta = akCarta;
            }

            ViewBag.akPelarasanIndenObjek = objek;

            List<AkPelarasanIndenPerihal> perihal = _cart.AkPelarasanIndenPerihal.ToList();

            ViewBag.akPelarasanIndenPerihal = perihal;
        }

        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akPelarasanInden = _unitOfWork.AkPelarasanIndenRepo.GetDetailsById((int)id);
            if (akPelarasanInden == null)
            {
                return NotFound();
            }

            if (akPelarasanInden.EnStatusBorang != EnStatusBorang.None)
            {
                TempData[SD.Error] = "Hapus data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkPelarasanIndenFromDb(akPelarasanInden);
            return View(akPelarasanInden);
        }

        public IActionResult BatalLulus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akPelarasanInden = _unitOfWork.AkPelarasanIndenRepo.GetDetailsById((int)id);
            if (akPelarasanInden == null)
            {
                return NotFound();
            }

            if (akPelarasanInden.EnStatusBorang != EnStatusBorang.Lulus)
            {
                TempData[SD.Error] = "Data belum diluluskan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkPelarasanIndenFromDb(akPelarasanInden);
            return View(akPelarasanInden);
        }

        [HttpPost, ActionName("BatalLulus")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatalLulusConfirmed(int id, string tindakan, string syscode)
        {
            var akPelarasanInden = _unitOfWork.AkPelarasanIndenRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akPelarasanInden != null && !string.IsNullOrEmpty(akPelarasanInden.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkPelarasanIndenRepo.IsPostedAsync((int)id, akPelarasanInden.NoRujukan) == false)
                {
                    TempData[SD.Error] = "Data belum diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkPelarasanIndenRepo.IsLulusAsync(id) == false)
                {
                    TempData[SD.Error] = "Data belum diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkPelarasanIndenRepo.BatalLulus(id, tindakan, user?.Email);

                _appLog.Insert("UnPosting", "Batal Lulus " + akPelarasanInden.NoRujukan ?? "", akPelarasanInden.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya batal kelulusan..!";
            }
            else
            {
                TempData[SD.Error] = "Data tidak wujud";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
        }

        public IActionResult BatalPos(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akPelarasanInden = _unitOfWork.AkPelarasanIndenRepo.GetDetailsById((int)id);
            if (akPelarasanInden == null)
            {
                return NotFound();
            }

            if (akPelarasanInden.EnStatusBorang != EnStatusBorang.Lulus)
            {
                TempData[SD.Error] = "Data belum diluluskan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkPelarasanIndenFromDb(akPelarasanInden);
            return View(akPelarasanInden);
        }

        [HttpPost, ActionName("BatalPos")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatalPosConfirmed(int id, string tindakan, string syscode)
        {
            var akPelarasanInden = _unitOfWork.AkPelarasanIndenRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akPelarasanInden != null && !string.IsNullOrEmpty(akPelarasanInden.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkPelarasanIndenRepo.IsPostedAsync((int)id, akPelarasanInden.NoRujukan) == false)
                {
                    TempData[SD.Error] = "Data belum diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkPelarasanIndenRepo.IsLulusAsync(id) == false)
                {
                    TempData[SD.Error] = "Data belum diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkPelarasanIndenRepo.BatalPos(id, tindakan, user?.UserName);

                _appLog.Insert("UnPosting", "Batal Pos " + akPelarasanInden.NoRujukan ?? "", akPelarasanInden.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya batal pos..!";
            }
            else
            {
                TempData[SD.Error] = "Data belum disahkan / disemak / diluluskan";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
        }

        public async Task<IActionResult> PosSemula(int id, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            var obj = await _context.AkPelarasanInden.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Pos operation

            if (obj != null && !string.IsNullOrEmpty(obj.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkPelarasanIndenRepo.IsPostedAsync((int)id, obj.NoRujukan))
                {
                    TempData[SD.Error] = "Data sudah diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkPelarasanIndenRepo.IsLulusAsync(id))
                {
                    TempData[SD.Error] = "Data telah diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkPelarasanIndenRepo.Lulus(id, pekerjaId, user?.UserName);

                // Batal operation end
                _appLog.Insert("Posting", obj.NoRujukan ?? "", obj.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya pos semula..!";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
        }
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);

            EmptyCart();
            PopulateDropDownList(1);
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.IX.GetDisplayName(), DateTime.Now.ToString("yyyy"));
            return View();
        }

        private dynamic GenerateRunningNumber(string initNoRujukan, string tahun)
        {
            var maxRefNo = _unitOfWork.AkPelarasanIndenRepo.GetMaxRefNo(initNoRujukan, tahun);

            var prefix = initNoRujukan + "/" + tahun + "/";
            return RunningNumberFormatter.GenerateRunningNumber(prefix, maxRefNo, "00000");
        }

        private void PopulateDropDownList(int JKWId)
        {
            ViewBag.JKW = _unitOfWork.JKWRepo.GetAll();
            ViewBag.DDaftarAwam = _unitOfWork.DDaftarAwamRepo.GetAllDetailsByKategori(EnKategoriDaftarAwam.Pembekal);
            ViewBag.AkCarta = _unitOfWork.AkCartaRepo.GetResultsByParas(EnParas.Paras4);
            ViewBag.JKWPTJBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetails();
            ViewBag.JKWPTJBahagianByJKW = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsByJKWId(JKWId);
            ViewBag.AkInden = _unitOfWork.AkIndenRepo.GetAllByStatus(EnStatusBorang.Lulus);
            ViewBag.EnJenisPerolehan = EnumHelper<EnJenisPerolehan>.GetList();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AkPelarasanInden akPelarasanInden, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            // check if there is pelulus available or not based on modul, kelulusan, and bahagian
            if (_cart.AkPelarasanIndenObjek != null && _cart.AkPelarasanIndenObjek.Count() > 0)
            {
                foreach (var item in _cart.AkPelarasanIndenObjek)
                {
                    var jKWPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModulKelulusan.PelarasanInden, EnKategoriKelulusan.Pelulus, jKWPtjBahagian.JBahagianId, akPelarasanInden.Jumlah) == false)
                    {
                        TempData[SD.Error] = "Tiada Pelulus yang wujud untuk senarai kod bahagian berikut.";
                        ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.IX.GetDisplayName(), akPelarasanInden.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
                        PopulateDropDownList(akPelarasanInden.JKWId);
                        PopulateListViewFromCart();
                        return View(akPelarasanInden);
                    }
                }
            }
            //

            if (ModelState.IsValid)
            {

                akPelarasanInden.UserId = user?.UserName ?? "";
                akPelarasanInden.TarMasuk = DateTime.Now;
                akPelarasanInden.DPekerjaMasukId = pekerjaId;

                akPelarasanInden.AkPelarasanIndenObjek = _cart.AkPelarasanIndenObjek?.ToList();
                akPelarasanInden.AkPelarasanIndenPerihal = _cart.AkPelarasanIndenPerihal.ToList();

                _context.Add(akPelarasanInden);
                _appLog.Insert("Tambah", akPelarasanInden.NoRujukan ?? "", akPelarasanInden.NoRujukan ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya ditambah..!";
                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
            }
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.IX.GetDisplayName(), akPelarasanInden.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
            PopulateDropDownList(akPelarasanInden.JKWId);
            PopulateListViewFromCart();
            return View(akPelarasanInden);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akPelarasanInden = _unitOfWork.AkPelarasanIndenRepo.GetDetailsById((int)id);
            if (akPelarasanInden == null)
            {
                return NotFound();
            }

            if (akPelarasanInden.EnStatusBorang != EnStatusBorang.None)
            {
                TempData[SD.Error] = "Ubah data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }

            EmptyCart();
            PopulateDropDownList(akPelarasanInden.JKWId);
            PopulateCartAkPelarasanIndenFromDb(akPelarasanInden);
            return View(akPelarasanInden);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AkPelarasanInden akPelarasanInden, string? fullName, string syscode)
        {
            if (id != akPelarasanInden.Id)
            {
                return NotFound();
            }

            // check if there is pelulus available or not based on modul, kelulusan, and bahagian
            if (_cart.AkPelarasanIndenObjek != null && _cart.AkPelarasanIndenObjek.Count() > 0)
            {
                foreach (var item in _cart.AkPelarasanIndenObjek)
                {
                    var jKWPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModulKelulusan.PelarasanInden, EnKategoriKelulusan.Pelulus, jKWPtjBahagian.JBahagianId, akPelarasanInden.Jumlah) == false)
                    {
                        TempData[SD.Error] = "Tiada Pelulus yang wujud untuk senarai kod bahagian berikut.";
                        PopulateDropDownList(akPelarasanInden.JKWId);
                        PopulateListViewFromCart();
                        return View(akPelarasanInden);
                    }
                }
            }
            //

            if (akPelarasanInden.NoRujukan != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = _unitOfWork.AkPelarasanIndenRepo.GetDetailsById(id);
                    var jumlahAsal = objAsal!.Jumlah;
                    akPelarasanInden.NoRujukan = objAsal.NoRujukan;
                    akPelarasanInden.UserId = objAsal.UserId;
                    akPelarasanInden.TarMasuk = objAsal.TarMasuk;
                    akPelarasanInden.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                    if (objAsal.AkPelarasanIndenObjek != null && objAsal.AkPelarasanIndenObjek.Count > 0)
                    {
                        foreach (var item in objAsal.AkPelarasanIndenObjek)
                        {
                            var model = _context.AkPelarasanIndenObjek.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null) _context.Remove(model);
                        }
                    }

                    if (objAsal.AkPelarasanIndenPerihal != null && objAsal.AkPelarasanIndenPerihal.Count > 0)
                    {
                        foreach (var item in objAsal.AkPelarasanIndenPerihal)
                        {
                            var model = _context.AkPelarasanIndenPerihal.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null) _context.Remove(model);
                        }
                    }

                    _context.Entry(objAsal).State = EntityState.Detached;

                    akPelarasanInden.UserIdKemaskini = user?.UserName ?? "";
                    akPelarasanInden.TarKemaskini = DateTime.Now;
                    akPelarasanInden.AkPelarasanIndenObjek = _cart.AkPelarasanIndenObjek?.ToList();
                    akPelarasanInden.AkPelarasanIndenPerihal = _cart.AkPelarasanIndenPerihal.ToList();

                    _unitOfWork.AkPelarasanIndenRepo.Update(akPelarasanInden);

                    if (jumlahAsal != akPelarasanInden.Jumlah)
                    {
                        _appLog.Insert("Ubah", Convert.ToDecimal(jumlahAsal).ToString("#,##0.00") + " -> " + Convert.ToDecimal(akPelarasanInden.Jumlah).ToString("#,##0.00") + " : " + akPelarasanInden.NoRujukan ?? "", akPelarasanInden.NoRujukan ?? "", id, akPelarasanInden.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }
                    else
                    {
                        _appLog.Insert("Ubah", akPelarasanInden.NoRujukan ?? "", akPelarasanInden.NoRujukan ?? "", id, akPelarasanInden.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }

                    await _context.SaveChangesAsync();

                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AkPelarasanIndenExist(akPelarasanInden.Id))
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

            PopulateDropDownList(akPelarasanInden.JKWId);
            PopulateListViewFromCart();
            return View(akPelarasanInden);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string sebabHapus, string syscode)
        {
            var akPelarasamInden = _unitOfWork.AkPelarasanIndenRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akPelarasamInden != null && await _unitOfWork.AkPelarasanIndenRepo.IsSahAsync(id) == false)
            {
                akPelarasamInden.UserIdKemaskini = user?.UserName ?? "";
                akPelarasamInden.TarKemaskini = DateTime.Now;
                akPelarasamInden.DPekerjaKemaskiniId = pekerjaId;
                akPelarasamInden.SebabHapus = sebabHapus;

                _context.AkPelarasanInden.Remove(akPelarasamInden);
                _appLog.Insert("Hapus", akPelarasamInden.NoRujukan ?? "", akPelarasamInden.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dihapuskan..!";
            }
            else
            {
                TempData[SD.Error] = "Data telah diluluskan";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
        }

        public async Task<IActionResult> RollBack(int id, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            var obj = await _context.AkPelarasanInden.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.AkPelarasanInden.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.NoRujukan ?? "", obj.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
        }

        private bool AkPelarasanIndenExist(int id)
        {
            return _unitOfWork.AkPelarasanIndenRepo.IsExist(b => b.Id == id);
        }

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


                    // get akPelarasanInden 
                    var akPelarasanInden = _unitOfWork.AkPelarasanIndenRepo.GetDetailsById((int)id);
                    if (akPelarasanInden == null)
                    {
                        TempData[SD.Error] = "Data tidak wujud.";
                    }
                    else
                    {

                        if (akPelarasanInden.NoRujukan != null)
                        {
                            // check is it posted or not
                            if (await _unitOfWork.AkPelarasanIndenRepo.IsPostedAsync((int)id, akPelarasanInden.NoRujukan) == false)
                            {
                                TempData[SD.Error] = "Data belum diposting.";
                                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                            }

                            // posting start here
                            _unitOfWork.AkPelarasanIndenRepo.RemovePostingFromAbBukuVot(akPelarasanInden, user?.UserName ?? "");

                            //insert applog
                            _appLog.Insert("UnPosting", "UnPosting Data", akPelarasanInden.NoRujukan, (int)id, akPelarasanInden.Jumlah, pekerjaId, modul, syscode, namamodul, user);

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

        private JsonResult EmptyCart()
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

        public JsonResult GetAkIndenDetails(int id, int? akPelarasanIndenId)
        {
            try
            {
                EmptyCart();
                var data = _unitOfWork.AkIndenRepo.GetDetailsById(id);

                if (data != null)
                {
                    if (data.AkIndenObjek != null)
                    {
                        foreach (var item in data.AkIndenObjek)
                        {
                            _cart.AddItemObjek(
                                    akPelarasanIndenId ?? 0,
                                    item.JKWPTJBahagianId,
                                    item.AkCartaId,
                                    item.Amaun);
                        }
                    }

                    if (data.AkIndenPerihal != null)
                    {
                        foreach (var item in data.AkIndenPerihal)
                        {
                            _cart.AddItemPerihal(
                                akPelarasanIndenId ?? 0,
                                item.Bil,
                                item.Perihal,
                                item.Kuantiti,
                                item.Unit,
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

        public JsonResult SaveCartAkPelarasanIndenObjek(AkPelarasanIndenObjek akPelarasanIndenObjek)
        {
            try
            {
                if (akPelarasanIndenObjek != null)
                {
                    _cart.AddItemObjek(akPelarasanIndenObjek.AkPelarasanIndenId, akPelarasanIndenObjek.JKWPTJBahagianId, akPelarasanIndenObjek.AkCartaId, akPelarasanIndenObjek.Amaun);
                }



                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkPelarasanIndenObjek(AkPelarasanIndenObjek akPelarasanIndenObjek)
        {
            try
            {
                if (akPelarasanIndenObjek != null)
                {
                    _cart.RemoveItemObjek(akPelarasanIndenObjek.JKWPTJBahagianId, akPelarasanIndenObjek.AkCartaId);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkPelarasanIndenObjek(AkPelarasanIndenObjek akPelarasanIndenObjek)
        {

            try
            {
                AkPelarasanIndenObjek data = _cart.AkPelarasanIndenObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akPelarasanIndenObjek.JKWPTJBahagianId && x.AkCartaId == akPelarasanIndenObjek.AkCartaId) ?? new AkPelarasanIndenObjek();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkPelarasanIndenObjek(AkPelarasanIndenObjek akPelarasanIndenObjek)
        {

            try
            {

                var data = _cart.AkPelarasanIndenObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akPelarasanIndenObjek.JKWPTJBahagianId && x.AkCartaId == akPelarasanIndenObjek.AkCartaId);

                var user = _userManager.GetUserName(User);

                if (data != null)
                {
                    _cart.RemoveItemObjek(akPelarasanIndenObjek.JKWPTJBahagianId, akPelarasanIndenObjek.AkCartaId);

                    _cart.AddItemObjek(akPelarasanIndenObjek.AkPelarasanIndenId,
                                    akPelarasanIndenObjek.JKWPTJBahagianId,
                                    akPelarasanIndenObjek.AkCartaId,
                                    akPelarasanIndenObjek.Amaun);
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
                var akPelarasanInden = _cart.AkPelarasanIndenPerihal.FirstOrDefault(pp => pp.Bil == Bil);
                if (akPelarasanInden != null)
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

        public JsonResult SaveCartAkPelarasanIndenPerihal(AkPelarasanIndenPerihal akPelarasanIndenPerihal)
        {
            try
            {
                if (akPelarasanIndenPerihal != null)
                {
                    _cart.AddItemPerihal(akPelarasanIndenPerihal.AkPelarasanIndenId, akPelarasanIndenPerihal.Bil, akPelarasanIndenPerihal.Perihal, akPelarasanIndenPerihal.Kuantiti, akPelarasanIndenPerihal.Unit, akPelarasanIndenPerihal.Harga, akPelarasanIndenPerihal.Amaun);
                }



                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkPelarasanIndenPerihal(AkPelarasanIndenPerihal akPelarasanIndenPerihal)
        {
            try
            {
                if (akPelarasanIndenPerihal != null)
                {
                    _cart.RemoveItemPerihal(akPelarasanIndenPerihal.Bil);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkPelarasanIndenPerihal(AkPelarasanIndenPerihal akPelarasanIndenPerihal)
        {

            try
            {
                AkPelarasanIndenPerihal data = _cart.AkPelarasanIndenPerihal.FirstOrDefault(x => x.Bil == akPelarasanIndenPerihal.Bil) ?? new AkPelarasanIndenPerihal();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkPelarasanIndenPerihal(AkPelarasanIndenPerihal akPelarasanIndenPerihal)
        {

            try
            {

                var akPelarasanInden = _cart.AkPelarasanIndenPerihal.FirstOrDefault(x => x.Bil == akPelarasanIndenPerihal.Bil);

                var user = _userManager.GetUserName(User);

                if (akPelarasanInden != null)
                {
                    _cart.RemoveItemPerihal(akPelarasanIndenPerihal.Bil);

                    _cart.AddItemPerihal(akPelarasanIndenPerihal.AkPelarasanIndenId, akPelarasanIndenPerihal.Bil, akPelarasanIndenPerihal.Perihal, akPelarasanIndenPerihal.Kuantiti, akPelarasanIndenPerihal.Unit, akPelarasanIndenPerihal.Harga, akPelarasanIndenPerihal.Amaun);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAllItemCartAkPelarasanInden()
        {

            try
            {
                List<AkPelarasanIndenObjek> objek = _cart.AkPelarasanIndenObjek.ToList();

                foreach (AkPelarasanIndenObjek item in objek)
                {
                    var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    item.JKWPTJBahagian = jkwPtjBahagian;

                    item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                    var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                    item.AkCarta = akCarta;
                }

                List<AkPelarasanIndenPerihal> perihal = _cart.AkPelarasanIndenPerihal.ToList();

                return Json(new { result = "OK", objek = objek.OrderBy(d => d.AkCarta?.Kod), perihal = perihal.OrderBy(d => d.Bil) });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

    }
}
