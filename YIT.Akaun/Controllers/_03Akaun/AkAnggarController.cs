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
using YIT._DataAccess.Repositories.Implementations;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services;
using YIT._DataAccess.Services.Cart;
using YIT._DataAccess.Services.Math;
using YIT.Akaun.Infrastructure;
using YIT.Akaun.Models.ViewModels.Forms;

namespace YIT.Akaun.Controllers._03Akaun
{
    [Authorize]
    public class AkAnggarController : Microsoft.AspNetCore.Mvc.Controller
    {

        public const string modul = Modules.kodAkAnggar;
        public const string namamodul = Modules.namaAkAnggar;

        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;
        private readonly CartAkAnggar _cart;

        public AkAnggarController(ApplicationDbContext context,
             _IUnitOfWork unitOfWork,
             UserManager<IdentityUser> userManager,
             _AppLogIRepository<AppLog, int> appLog,
             UserServices userServices,
             CartAkAnggar cart
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

            var akAnggar = _unitOfWork.AkAnggarRepo.GetResults(searchString, date1, date2, searchColumn, EnStatusBorang.Semua);

            return View(akAnggar);

        }

        private void SaveFormFields(string? searchString,
           string? searchDate1,
           string? searchDate2)
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

        // GET: KW/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akAnggar = _unitOfWork.AkAnggarRepo.GetDetailsById((int)id);
            if (akAnggar == null)
            {
                return NotFound();
            }
            EmptyCart();
            PopulateCartAkAnggarFromDb(akAnggar);
            return View(akAnggar);
        }

        // GET: KW/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akAnggar = _unitOfWork.AkAnggarRepo.GetDetailsById((int)id);
            if (akAnggar == null)
            {
                return NotFound();
            }

            if (akAnggar.EnStatusBorang != EnStatusBorang.None)
            {
                TempData[SD.Error] = "Hapus data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkAnggarFromDb(akAnggar);
            return View(akAnggar);
        }

        public IActionResult BatalLulus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akAnggar = _unitOfWork.AkAnggarRepo.GetDetailsById((int)id);
            if (akAnggar == null)
            {
                return NotFound();
            }

            if (akAnggar.EnStatusBorang != EnStatusBorang.Lulus)
            {
                TempData[SD.Error] = "Data belum diluluskan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkAnggarFromDb(akAnggar);
            return View(akAnggar);
        }

        [HttpPost, ActionName("BatalLulus")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatalLulusConfirmed(int id, string tindakan, string syscode)
        {
            var akAnggar = _unitOfWork.AkAnggarRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akAnggar != null && !string.IsNullOrEmpty(akAnggar.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkAnggarRepo.IsPostedAsync((int)id, akAnggar.NoRujukan) == false)
                {
                    TempData[SD.Error] = "Data belum diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkAnggarRepo.IsLulusAsync(id) == false)
                {
                    TempData[SD.Error] = "Data belum diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkAnggarRepo.BatalLulus(id, tindakan, user?.Email);

                _appLog.Insert("UnPosting", "Batal Lulus " + akAnggar.NoRujukan ?? "", akAnggar.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var akAnggar = _unitOfWork.AkAnggarRepo.GetDetailsById((int)id);
            if (akAnggar == null)
            {
                return NotFound();
            }

            if (akAnggar.EnStatusBorang != EnStatusBorang.Lulus)
            {
                TempData[SD.Error] = "Data belum diluluskan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkAnggarFromDb(akAnggar);
            return View(akAnggar);
        }

        [HttpPost, ActionName("BatalPos")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatalPosConfirmed(int id, string tindakan, string syscode)
        {
            var akAnggar = _unitOfWork.AkAnggarRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akAnggar != null && !string.IsNullOrEmpty(akAnggar.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkAnggarRepo.IsPostedAsync((int)id, akAnggar.NoRujukan) == false)
                {
                    TempData[SD.Error] = "Data belum diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkAnggarRepo.IsLulusAsync(id) == false)
                {
                    TempData[SD.Error] = "Data belum diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkAnggarRepo.BatalPos(id, tindakan, user?.UserName);

                _appLog.Insert("UnPosting", "Batal Pos " + akAnggar.NoRujukan ?? "", akAnggar.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var obj = await _context.AkAnggar.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Pos operation

            if (obj != null && !string.IsNullOrEmpty(obj.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkAnggarRepo.IsPostedAsync((int)id, obj.NoRujukan))
                {
                    TempData[SD.Error] = "Data sudah diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkAnggarRepo.IsLulusAsync(id))
                {
                    TempData[SD.Error] = "Data telah diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkAnggarRepo.Lulus(id, pekerjaId, user?.UserName);

                // Batal operation end
                _appLog.Insert("Posting", obj.NoRujukan ?? "", obj.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya pos semula..!";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
        }

        // GET: KW/Create
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);

            EmptyCart();
            PopulateDropDownList(1);
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.AH.GetDisplayName(), DateTime.Now.ToString("yyyy"));
            return View();
        }

        // POST: KW/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AkAnggar akAnggar, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            // check if there is pengesah available or not based on modul, kelulusan, and bahagian
            if (_cart.AkAnggarObjek != null && _cart.AkAnggarObjek.Count() > 0)
            {
                foreach (var item in _cart.AkAnggarObjek)
                {
                    var jKWPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModulKelulusan.AnggaranHasil, EnKategoriKelulusan.Pengesah, jKWPtjBahagian.JBahagianId, akAnggar.Jumlah) == false)
                    {
                        TempData[SD.Error] = "Tiada Pengesah yang wujud untuk senarai kod bahagian berikut.";
                        ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.AH.GetDisplayName(), akAnggar.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
                        PopulateDropDownList(akAnggar.JKWId);
                        PopulateListViewFromCart();
                        return View(akAnggar);
                    }
                }
            }

            if (ModelState.IsValid)
            {

                akAnggar.UserId = user?.UserName ?? "";
                akAnggar.TarMasuk = DateTime.Now;
                akAnggar.DPekerjaMasukId = pekerjaId;

                akAnggar.AkAnggarObjek = _cart.AkAnggarObjek?.ToList();

                _context.Add(akAnggar);
                _appLog.Insert("Tambah", akAnggar.NoRujukan ?? "", akAnggar.NoRujukan ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya ditambah..!";
                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
            }
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.AH.GetDisplayName(), akAnggar.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
            PopulateDropDownList(akAnggar.JKWId);
            PopulateListViewFromCart();
            return View(akAnggar);

        }

        // GET: KW/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akAnggar = _unitOfWork.AkAnggarRepo.GetDetailsById((int)id);
            if (akAnggar == null)
            {
                return NotFound();
            }
            if (akAnggar.EnStatusBorang != EnStatusBorang.None && akAnggar.EnStatusBorang != EnStatusBorang.Kemaskini)
            {
                TempData[SD.Error] = "Ubah data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateDropDownList(akAnggar.JKWId);
            PopulateCartAkAnggarFromDb(akAnggar);
            return View(akAnggar);
        }

        // POST: KW/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AkAnggar akAnggar, string? fullName, string syscode)
        {
            if (id != akAnggar.Id)
            {
                return NotFound();
            }

            // check if there is pengesah available or not based on modul, kelulusan, and bahagian
            if (_cart.AkAnggarObjek != null && _cart.AkAnggarObjek.Count() > 0)
            {
                foreach (var item in _cart.AkAnggarObjek)
                {
                    var jKWPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModulKelulusan.AnggaranHasil, EnKategoriKelulusan.Pengesah, jKWPtjBahagian.JBahagianId, akAnggar.Jumlah) == false)
                    {
                        TempData[SD.Error] = "Tiada Pengesah yang wujud untuk senarai kod bahagian berikut.";
                        PopulateDropDownList(akAnggar.JKWId);
                        PopulateListViewFromCart();
                        return View(akAnggar);
                    }
                }
            }

            if (akAnggar.NoRujukan != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = _unitOfWork.AkAnggarRepo.GetDetailsById(id);
                    var jumlahAsal = objAsal!.Jumlah;
                    akAnggar.NoRujukan = objAsal.NoRujukan;
                    akAnggar.UserId = objAsal.UserId;
                    akAnggar.TarMasuk = objAsal.TarMasuk;
                    akAnggar.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                    if (objAsal.AkAnggarObjek != null && objAsal.AkAnggarObjek.Count > 0)
                    {
                        foreach (var item in objAsal.AkAnggarObjek)
                        {
                            var model = _context.AkAnggarObjek.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null) _context.Remove(model);
                        }
                    }

                    _context.Entry(objAsal).State = EntityState.Detached;

                    akAnggar.UserIdKemaskini = user?.UserName ?? "";
                    akAnggar.TarKemaskini = DateTime.Now;
                    akAnggar.AkAnggarObjek = _cart.AkAnggarObjek?.ToList();


                    _unitOfWork.AkAnggarRepo.Update(akAnggar);

                    if (jumlahAsal != akAnggar.Jumlah)
                    {
                        _appLog.Insert("Ubah", Convert.ToDecimal(jumlahAsal).ToString("#,##0.00") + " -> " + Convert.ToDecimal(akAnggar.Jumlah).ToString("#,##0.00") + " : " + akAnggar.NoRujukan ?? "", akAnggar.NoRujukan ?? "", id, akAnggar.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }
                    else
                    {
                        _appLog.Insert("Ubah", akAnggar.NoRujukan ?? "", akAnggar.NoRujukan ?? "", id, akAnggar.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }

                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AkAnggarExist(akAnggar.Id))
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
            PopulateDropDownList(akAnggar.JKWId);
            PopulateListViewFromCart();
            return View(akAnggar);
        }

        // POST: KW/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string sebabHapus, string syscode)
        {
            var akAnggar = _unitOfWork.AkAnggarRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akAnggar != null && await _unitOfWork.AkAnggarRepo.IsSahAsync(id) == false)
            {
                akAnggar.UserIdKemaskini = user?.UserName ?? "";
                akAnggar.TarKemaskini = DateTime.Now;
                akAnggar.DPekerjaKemaskiniId = pekerjaId;
                akAnggar.SebabHapus = sebabHapus;

                _context.AkAnggar.Remove(akAnggar);

                _appLog.Insert("Hapus", akAnggar.NoRujukan ?? "", akAnggar.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dihapuskan..!";
            }
            else
            {
                TempData[SD.Error] = "Data telah disahkan / disemak / diluluskan";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
        }

        public async Task<IActionResult> RollBack(int id, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            var obj = await _context.AkAnggar.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.AkAnggar.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.NoRujukan ?? "", obj.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
        }

        private bool AkAnggarExist(int id)
        {
            return _unitOfWork.AkAnggarRepo.IsExist(b => b.Id == id);
        }

        private void PopulateListViewFromCart()
        {
            List<AkAnggarObjek> objek = _cart.AkAnggarObjek.ToList();

            foreach (AkAnggarObjek item in objek)
            {
                var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                item.JKWPTJBahagian = jkwPtjBahagian;

                item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                item.AkCarta = akCarta;
            }

            ViewBag.AkAnggarObjek = objek;
        }

        private void PopulateDropDownList(int JKWId)
        {
            ViewBag.JKW = _unitOfWork.JKWRepo.GetAll();
            ViewBag.AkCarta = _unitOfWork.AkCartaRepo.GetResultsByParas(EnParas.Paras4);
            ViewBag.JKWPTJBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetails();
            ViewBag.JKWPTJBahagianByJKW = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsByJKWId(JKWId);
        }

        private void PopulateCartAkAnggarFromDb(AkAnggar akAnggar)
        {
            if (akAnggar.AkAnggarObjek != null)
            {
                foreach (var item in akAnggar.AkAnggarObjek)
                {
                    _cart.AddItemObjek(
                            akAnggar.Id,
                            item.JKWPTJBahagianId,
                            item.AkCartaId,
                            item.Amaun);
                }
            }

            PopulateListViewFromCart();
        }

        public JsonResult EmptyCart()
        {
            try
            {

                _cart.ClearObjek();


                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetJKWPTJBahagianAkCarta(int JKWPTJBahagianId, int AkCartaId)
        {
            try
            {
                var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetById(JKWPTJBahagianId);
                if (jkwPtjBahagian == null)
                {

                    return Json(new { result = "Error", message = "Kod akaun tidak wujud" });
                }

                var akCarta = _unitOfWork.AkCartaRepo.GetById(AkCartaId);
                if (akCarta == null)
                {
                    return Json(new { result = "Error", message = "Kod akaun tidak wujud" });
                }
                if (jkwPtjBahagian.JBahagian != null && jkwPtjBahagian.JBahagian.Kod != null) jkwPtjBahagian.JBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian?.JKW?.Kod, jkwPtjBahagian?.JPTJ?.Kod, jkwPtjBahagian?.JBahagian?.Kod);


                return Json(new { result = "OK", jkwPtjBahagian, akCarta });
            }
            catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }
        }

        public JsonResult SaveCartAkAnggarObjek(AkAnggarObjek akAnggarObjek)
        {
            try
            {
                if (akAnggarObjek != null)
                {
                    _cart.AddItemObjek(akAnggarObjek.AkAnggarId, akAnggarObjek.JKWPTJBahagianId, akAnggarObjek.AkCartaId, akAnggarObjek.Amaun);
                }



                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkAnggarObjek(AkAnggarObjek akAnggarObjek)
        {
            try
            {
                if (akAnggarObjek != null)
                {
                    _cart.RemoveItemObjek(akAnggarObjek.JKWPTJBahagianId, akAnggarObjek.AkCartaId);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkAnggarObjek(AkAnggarObjek akAnggarObjek)
        {

            try
            {
                AkAnggarObjek data = _cart.AkAnggarObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akAnggarObjek.JKWPTJBahagianId && x.AkCartaId == akAnggarObjek.AkCartaId) ?? new AkAnggarObjek();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkAnggarObjek(AkAnggarObjek akAnggarObjek)
        {

            try
            {

                var data = _cart.AkAnggarObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akAnggarObjek.JKWPTJBahagianId && x.AkCartaId == akAnggarObjek.AkCartaId);

                var user = _userManager.GetUserName(User);

                if (data != null)
                {
                    _cart.RemoveItemObjek(akAnggarObjek.JKWPTJBahagianId, akAnggarObjek.AkCartaId);

                    _cart.AddItemObjek(akAnggarObjek.AkAnggarId,
                                    akAnggarObjek.JKWPTJBahagianId,
                                    akAnggarObjek.AkCartaId,
                                    akAnggarObjek.Amaun);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }
        public JsonResult GetAllItemCartAkAnggar()
        {

            try
            {
                List<AkAnggarObjek> objek = _cart.AkAnggarObjek.ToList();

                foreach (AkAnggarObjek item in objek)
                {
                    var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    item.JKWPTJBahagian = jkwPtjBahagian;
                    item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian?.JBahagian?.Kod);

                    var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                    item.AkCarta = akCarta;
                }
                return Json(new { result = "OK", objek = objek.OrderBy(d => d.AkCarta?.Kod) });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        private string GenerateRunningNumber(string initNoRujukan, string tahun)
        {
            var maxRefNo = _unitOfWork.AkAnggarRepo.GetMaxRefNo(initNoRujukan, tahun);

            var prefix = initNoRujukan + "/" + tahun + "/";
            return RunningNumberFormatter.GenerateRunningNumber(prefix, maxRefNo, "00000");
        }

    }


}
