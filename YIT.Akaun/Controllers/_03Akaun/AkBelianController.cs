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
    [Authorize]
    public class AkBelianController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodAkBelian;
        public const string namamodul = Modules.namaAkBelian;

        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;
        private readonly CartAkBelian _cart;

        public AkBelianController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices,
            CartAkBelian cart)
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

            var akBelian = _unitOfWork.AkBelianRepo.GetResults(searchString, date1, date2, searchColumn, EnStatusBorang.Semua);

            return View(akBelian);
        }

        private void SaveFormFields(string searchString, string searchDate1, string searchDate2)
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

        private void PopulateFormFields(string searchString, string searchDate1, string searchDate2)
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

            var akBelian = _unitOfWork.AkBelianRepo.GetDetailsById((int)id);
            if (akBelian == null)
            {
                return NotFound();
            }
            EmptyCart();
            PopulateCartAkBelianFromDb(akBelian);
            return View(akBelian);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akBelian = _unitOfWork.AkBelianRepo.GetDetailsById((int)id);
            if (akBelian == null)
            {
                return NotFound();
            }

            if (akBelian.EnStatusBorang != EnStatusBorang.None)
            {
                TempData[SD.Error] = "Hapus data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkBelianFromDb(akBelian);
            return View(akBelian);
        }

        public IActionResult BatalLulus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akBelian = _unitOfWork.AkBelianRepo.GetDetailsById((int)id);
            if (akBelian == null)
            {
                return NotFound();
            }

            if (akBelian.EnStatusBorang != EnStatusBorang.Lulus)
            {
                TempData[SD.Error] = "Data belum diluluskan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkBelianFromDb(akBelian);
            return View(akBelian);
        }

        [HttpPost, ActionName("BatalLulus")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatalLulusConfirmed(int id, string tindakan, string syscode)
        {
            var akBelian = _unitOfWork.AkBelianRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akBelian != null && !string.IsNullOrEmpty(akBelian.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkBelianRepo.IsPostedAsync((int)id, akBelian.NoRujukan) == false)
                {
                    TempData[SD.Error] = "Data belum diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkBelianRepo.IsLulusAsync(id) == false)
                {
                    TempData[SD.Error] = "Data belum diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkBelianRepo.BatalLulus(id, tindakan, user?.Email);

                _appLog.Insert("UnPosting", "Batal Lulus " + akBelian.NoRujukan ?? "", akBelian.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var akBelian = _unitOfWork.AkBelianRepo.GetDetailsById((int)id);
            if (akBelian == null)
            {
                return NotFound();
            }

            if (akBelian.EnStatusBorang != EnStatusBorang.Lulus)
            {
                TempData[SD.Error] = "Data belum diluluskan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkBelianFromDb(akBelian);
            return View(akBelian);
        }

        [HttpPost, ActionName("BatalPos")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatalPosConfirmed(int id, string tindakan, string syscode)
        {
            var akBelian = _unitOfWork.AkBelianRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akBelian != null && !string.IsNullOrEmpty(akBelian.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkBelianRepo.IsPostedAsync((int)id, akBelian.NoRujukan) == false)
                {
                    TempData[SD.Error] = "Data belum diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkBelianRepo.IsLulusAsync(id) == false)
                {
                    TempData[SD.Error] = "Data belum diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkBelianRepo.BatalPos(id, tindakan, user?.UserName);

                _appLog.Insert("UnPosting", "Batal Pos " + akBelian.NoRujukan ?? "", akBelian.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var obj = await _context.AkBelian.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Pos operation

            if (obj != null && !string.IsNullOrEmpty(obj.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkBelianRepo.IsPostedAsync((int)id, obj.NoRujukan))
                {
                    TempData[SD.Error] = "Data sudah diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkBelianRepo.IsLulusAsync(id))
                {
                    TempData[SD.Error] = "Data telah diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkBelianRepo.Lulus(id, pekerjaId, user?.UserName);

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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AkBelian akBelian, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            // check if there is pelulus available or not based on modul, kelulusan, and bahagian
            if (_cart.AkBelianObjek != null && _cart.AkBelianObjek.Count() > 0)
            {
                foreach (var item in _cart.AkBelianObjek)
                {
                    var jKWPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModulKelulusan.Belian, EnKategoriKelulusan.Pelulus, jKWPtjBahagian.JBahagianId, akBelian.Jumlah) == false)
                    {
                        TempData[SD.Error] = "Tiada Pelulus yang wujud untuk senarai kod bahagian berikut.";
                        PopulateDropDownList(akBelian.JKWId);
                        PopulateListViewFromCart();
                        return View(akBelian);
                    }
                }
            }
            //

            // check if there is another same no invois for the same daftar awam
            if (NoRujukanWithSameDaftarAwamExist(akBelian.NoRujukan?.ToUpper() ?? "",akBelian.DDaftarAwamId))
            {
                TempData[SD.Error] = "No Invois bagi pembekal ini telah wujud.";
                PopulateDropDownList(akBelian.JKWId);
                PopulateListViewFromCart();
                return View(akBelian);
            }
            //

            if (ModelState.IsValid)
            {

                akBelian.NoRujukan = EnInitNoRujukan.IN.GetDisplayName() + "/" + akBelian.NoRujukan?.ToUpper();
                akBelian.UserId = user?.UserName ?? "";
                akBelian.TarMasuk = DateTime.Now;
                akBelian.DPekerjaMasukId = pekerjaId;

                akBelian.AkBelianObjek = _cart.AkBelianObjek?.ToList();
                akBelian.AkBelianPerihal = _cart.AkBelianPerihal.ToList();

                _context.Add(akBelian);
                _appLog.Insert("Tambah", akBelian.NoRujukan ?? "", akBelian.NoRujukan ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya ditambah..!";
                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
            }
            PopulateDropDownList(akBelian.JKWId);
            PopulateListViewFromCart();
            return View(akBelian);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akBelian = _unitOfWork.AkBelianRepo.GetDetailsById((int)id);
            if (akBelian == null)
            {
                return NotFound();
            }

            if (akBelian.EnStatusBorang != EnStatusBorang.None)
            {
                TempData[SD.Error] = "Ubah data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }

            EmptyCart();
            PopulateDropDownList(akBelian.JKWId);
            PopulateCartAkBelianFromDb(akBelian);
            return View(akBelian);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AkBelian akBelian, string? fullName, string syscode)
        {
            if (id != akBelian.Id)
            {
                return NotFound();
            }

            // check if there is pelulus available or not based on modul, kelulusan, and bahagian
            if (_cart.AkBelianObjek != null && _cart.AkBelianObjek.Count() > 0)
            {
                foreach (var item in _cart.AkBelianObjek)
                {
                    var jKWPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModulKelulusan.Belian, EnKategoriKelulusan.Pelulus, jKWPtjBahagian.JBahagianId, akBelian.Jumlah) == false)
                    {
                        TempData[SD.Error] = "Tiada Pelulus yang wujud untuk senarai kod bahagian berikut.";
                        PopulateDropDownList(akBelian.JKWId);
                        PopulateListViewFromCart();
                        return View(akBelian);
                    }
                }
            }
            //

            if (akBelian.NoRujukan != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = _unitOfWork.AkBelianRepo.GetDetailsById(id);
                    var jumlahAsal = objAsal!.Jumlah;
                    akBelian.NoRujukan = objAsal.NoRujukan;
                    akBelian.JKWId = objAsal.JKWId;
                    akBelian.DDaftarAwamId = objAsal.DDaftarAwamId;
                    akBelian.AkPOId = objAsal.AkPOId;
                    akBelian.AkNotaMintaId = objAsal.AkNotaMintaId;
                    akBelian.AkIndenId = objAsal.AkIndenId;
                    akBelian.UserId = objAsal.UserId;
                    akBelian.TarMasuk = objAsal.TarMasuk;
                    akBelian.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                    if (objAsal.AkBelianObjek != null && objAsal.AkBelianObjek.Count > 0)
                    {
                        foreach (var item in objAsal.AkBelianObjek)
                        {
                            var model = _context.AkBelianObjek.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null) _context.Remove(model);
                        }
                    }

                    if (objAsal.AkBelianPerihal != null && objAsal.AkBelianPerihal.Count > 0)
                    {
                        foreach (var item in objAsal.AkBelianPerihal)
                        {
                            var model = _context.AkBelianPerihal.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null) _context.Remove(model);
                        }
                    }

                    _context.Entry(objAsal).State = EntityState.Detached;

                    akBelian.UserIdKemaskini = user?.UserName ?? "";
                    akBelian.TarKemaskini = DateTime.Now;
                    akBelian.AkBelianObjek = _cart.AkBelianObjek?.ToList();
                    akBelian.AkBelianPerihal = _cart.AkBelianPerihal.ToList();

                    _unitOfWork.AkBelianRepo.Update(akBelian);

                    if (jumlahAsal != akBelian.Jumlah)
                    {
                        _appLog.Insert("Ubah", Convert.ToDecimal(jumlahAsal).ToString("#,##0.00") + " -> " + Convert.ToDecimal(akBelian.Jumlah).ToString("#,##0.00") + " : " + akBelian.NoRujukan ?? "", akBelian.NoRujukan ?? "", id, akBelian.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }
                    else
                    {
                        _appLog.Insert("Ubah", akBelian.NoRujukan ?? "", akBelian.NoRujukan ?? "", id, akBelian.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }

                    await _context.SaveChangesAsync();

                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AkBelianExist(akBelian.Id))
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

            PopulateDropDownList(akBelian.JKWId);
            PopulateListViewFromCart();
            return View(akBelian);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string sebabHapus, string syscode)
        {
            var akBelian = _unitOfWork.AkBelianRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akBelian != null && await _unitOfWork.AkBelianRepo.IsSahAsync(id) == false)
            {
                akBelian.UserIdKemaskini = user?.UserName ?? "";
                akBelian.TarKemaskini = DateTime.Now;
                akBelian.DPekerjaKemaskiniId = pekerjaId;
                akBelian.SebabHapus = sebabHapus;

                _context.AkBelian.Remove(akBelian);
                _appLog.Insert("Hapus", akBelian.NoRujukan ?? "", akBelian.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var obj = await _context.AkBelian.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.AkBelian.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.NoRujukan ?? "", obj.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
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


                    // get akBelian 
                    var akBelian = _unitOfWork.AkBelianRepo.GetDetailsById((int)id);
                    if (akBelian == null)
                    {
                        TempData[SD.Error] = "Data tidak wujud.";
                    }
                    else
                    {

                        if (akBelian.NoRujukan != null)
                        {
                            // check is it posted or not
                            if (await _unitOfWork.AkBelianRepo.IsPostedAsync((int)id, akBelian.NoRujukan) == false)
                            {
                                TempData[SD.Error] = "Data belum diposting.";
                                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                            }

                            // posting start here
                            _unitOfWork.AkBelianRepo.RemovePostingFromAbBukuVot(akBelian, user?.UserName ?? "");

                            //insert applog
                            _appLog.Insert("UnPosting", "UnPosting Data", akBelian.NoRujukan, (int)id, akBelian.Jumlah, pekerjaId, modul, syscode, namamodul, user);

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

        private bool AkBelianExist(int id)
        {
            return _unitOfWork.AkBelianRepo.IsExist(b => b.Id == id);
        }

        private bool NoRujukanWithSameDaftarAwamExist(string noRujukan, int dDaftarAwamId)
        {
            return _unitOfWork.AkBelianRepo.IsExist(b => b.DDaftarAwamId == dDaftarAwamId && b.NoRujukan!.EndsWith(noRujukan));
        }

        private void PopulateDropDownList(int JKWId)
        {
            ViewBag.JKW = _unitOfWork.JKWRepo.GetAll();
            ViewBag.JCukai = _unitOfWork.JCukaiRepo.GetAll();
            ViewBag.DDaftarAwam = _unitOfWork.DDaftarAwamRepo.GetAllDetailsByKategori(EnKategoriDaftarAwam.Pembekal);
            ViewBag.AkCarta = _unitOfWork.AkCartaRepo.GetResultsByParas(EnParas.Paras4);
            ViewBag.JKWPTJBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetails();
            ViewBag.JKWPTJBahagianByJKW = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsByJKWId(JKWId);
            ViewBag.AkPO = _unitOfWork.AkPORepo.GetAllByStatus(EnStatusBorang.Lulus);
            ViewBag.AkInden = _unitOfWork.AkIndenRepo.GetAllByStatus(EnStatusBorang.Lulus);
            ViewBag.AkNotaMinta = _unitOfWork.AkNotaMintaRepo.GetAllByStatus(EnStatusBorang.Lulus);
            ViewBag.EnJenisPerolehan = EnumHelper<EnJenisPerolehan>.GetList();
        }

        private void PopulateCartAkBelianFromDb(AkBelian akBelian)
        {
            if (akBelian.AkBelianObjek != null)
            {
                foreach (var item in akBelian.AkBelianObjek)
                {
                    _cart.AddItemObjek(
                            akBelian.Id,
                            item.JKWPTJBahagianId,
                            item.AkCartaId,
                            item.JCukaiId,
                            item.Amaun);
                }
            }

            if (akBelian.AkBelianPerihal != null)
            {
                foreach (var item in akBelian.AkBelianPerihal)
                {
                    _cart.AddItemPerihal(
                        akBelian.Id,
                        item.Bil,
                        item.Perihal,
                        item.Kuantiti,
                        item.Unit,
                        item.Harga,
                        item.Amaun
                        );
                }
            }
            PopulateListViewFromCart();
        }

        private void PopulateListViewFromCart()
        {
            List<AkBelianObjek> objek = _cart.AkBelianObjek.ToList();

            foreach (AkBelianObjek item in objek)
            {
                var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                item.JKWPTJBahagian = jkwPtjBahagian;

                item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                item.AkCarta = akCarta;

                if (item.JCukaiId != null)
                {
                    var jCukai = _unitOfWork.JCukaiRepo.GetById((int)item.JCukaiId);
                    item.JCukai = jCukai;
                }

            }

            ViewBag.akBelianObjek = objek;

            List<AkBelianPerihal> perihal = _cart.AkBelianPerihal.ToList();

            ViewBag.akBelianPerihal = perihal;
        }

        // jsonResult
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

        public JsonResult GetAkPODetails(int id, int? akBelianId)
        {
            try
            {
                EmptyCart();
                var data = _unitOfWork.AkPORepo.GetDetailsById(id);

                if (data != null)
                {
                    data = _unitOfWork.AkPORepo.GetBalanceAdjustmentFromAkPelarasanPO(data);

                    if (data.AkPOObjek != null)
                    {
                        foreach (var item in data.AkPOObjek)
                        {
                            _cart.AddItemObjek(
                                    akBelianId ?? 0,
                                    item.JKWPTJBahagianId,
                                    item.AkCartaId,
                                    null,
                                    item.Amaun);
                        }
                    }

                    if (data.AkPOPerihal != null)
                    {
                        foreach (var item in data.AkPOPerihal)
                        {
                            _cart.AddItemPerihal(
                                akBelianId ?? 0,
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

        public JsonResult GetAkIndenDetails(int id, int? akBelianId)
        {
            try
            {
                EmptyCart();
                var data = _unitOfWork.AkIndenRepo.GetDetailsById(id);

                if (data != null)
                {
                    data = _unitOfWork.AkIndenRepo.GetBalanceAdjustmentFromAkPelarasanInden(data);

                    if (data.AkIndenObjek != null)
                    {
                        foreach (var item in data.AkIndenObjek)
                        {
                            _cart.AddItemObjek(
                                    akBelianId ?? 0,
                                    item.JKWPTJBahagianId,
                                    item.AkCartaId,
                                    null,
                                    item.Amaun);
                        }
                    }

                    if (data.AkIndenPerihal != null)
                    {
                        foreach (var item in data.AkIndenPerihal)
                        {
                            _cart.AddItemPerihal(
                                akBelianId ?? 0,
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

        public JsonResult GetAkNotaMintaDetails(int id, int? akBelianId)
        {
            try
            {
                EmptyCart();
                var data = _unitOfWork.AkNotaMintaRepo.GetDetailsById(id);

                if (data != null)
                {
                    if (data.AkNotaMintaObjek != null)
                    {
                        foreach (var item in data.AkNotaMintaObjek)
                        {
                            _cart.AddItemObjek(
                                    akBelianId ?? 0,
                                    item.JKWPTJBahagianId,
                                    item.AkCartaId,
                                    null,
                                    item.Amaun);
                        }
                    }

                    if (data.AkNotaMintaPerihal != null)
                    {
                        foreach (var item in data.AkNotaMintaPerihal)
                        {
                            _cart.AddItemPerihal(
                                akBelianId ?? 0,
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

        public JsonResult SaveCartAkBelianObjek(AkBelianObjek akBelianObjek)
        {
            try
            {
                var jCukai = new JCukai();
                if (akBelianObjek != null)
                {
                    _cart.AddItemObjek(akBelianObjek.AkBelianId, akBelianObjek.JKWPTJBahagianId, akBelianObjek.AkCartaId, akBelianObjek.JCukaiId, akBelianObjek.Amaun);

                    if (akBelianObjek.JCukaiId != null)
                    {
                        jCukai = _unitOfWork.JCukaiRepo.GetById((int)akBelianObjek.JCukaiId);
                    }
                    
                }

                return Json(new { result = "OK", jCukai = jCukai });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkBelianObjek(AkBelianObjek akBelianObjek)
        {
            try
            {
                if (akBelianObjek != null)
                {
                    _cart.RemoveItemObjek(akBelianObjek.JKWPTJBahagianId, akBelianObjek.AkCartaId);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkBelianObjek(AkBelianObjek akBelianObjek)
        {

            try
            {
                AkBelianObjek data = _cart.AkBelianObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akBelianObjek.JKWPTJBahagianId && x.AkCartaId == akBelianObjek.AkCartaId) ?? new AkBelianObjek();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkBelianObjek(AkBelianObjek akBelianObjek)
        {

            try
            {

                var data = _cart.AkBelianObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akBelianObjek.JKWPTJBahagianId && x.AkCartaId == akBelianObjek.AkCartaId);

                var user = _userManager.GetUserName(User);

                if (data != null)
                {
                    _cart.RemoveItemObjek(akBelianObjek.JKWPTJBahagianId, akBelianObjek.AkCartaId);

                    _cart.AddItemObjek(akBelianObjek.AkBelianId,
                                    akBelianObjek.JKWPTJBahagianId,
                                    akBelianObjek.AkCartaId,
                                    akBelianObjek.JCukaiId,
                                    akBelianObjek.Amaun);
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
                var akBelian = _cart.AkBelianPerihal.FirstOrDefault(pp => pp.Bil == Bil);
                if (akBelian != null)
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

        public JsonResult SaveCartAkBelianPerihal(AkBelianPerihal akBelianPerihal)
        {
            try
            {
                if (akBelianPerihal != null)
                {
                    _cart.AddItemPerihal(akBelianPerihal.AkBelianId, akBelianPerihal.Bil, akBelianPerihal.Perihal, akBelianPerihal.Kuantiti, akBelianPerihal.Unit, akBelianPerihal.Harga, akBelianPerihal.Amaun);
                }



                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkBelianPerihal(AkBelianPerihal akBelianPerihal)
        {
            try
            {
                if (akBelianPerihal != null)
                {
                    _cart.RemoveItemPerihal(akBelianPerihal.Bil);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkBelianPerihal(AkBelianPerihal akBelianPerihal)
        {

            try
            {
                AkBelianPerihal data = _cart.AkBelianPerihal.FirstOrDefault(x => x.Bil == akBelianPerihal.Bil) ?? new AkBelianPerihal();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkBelianPerihal(AkBelianPerihal akBelianPerihal)
        {

            try
            {

                var akBelian = _cart.AkBelianPerihal.FirstOrDefault(x => x.Bil == akBelianPerihal.Bil);

                var user = _userManager.GetUserName(User);

                if (akBelian != null)
                {
                    _cart.RemoveItemPerihal(akBelianPerihal.Bil);

                    _cart.AddItemPerihal(akBelianPerihal.AkBelianId, akBelianPerihal.Bil, akBelianPerihal.Perihal, akBelianPerihal.Kuantiti, akBelianPerihal.Unit, akBelianPerihal.Harga, akBelianPerihal.Amaun);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAllItemCartAkBelian()
        {

            try
            {
                List<AkBelianObjek> objek = _cart.AkBelianObjek.ToList();

                foreach (AkBelianObjek item in objek)
                {
                    var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    item.JKWPTJBahagian = jkwPtjBahagian;

                    item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                    var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                    item.AkCarta = akCarta;

                    if (item.JCukaiId != null)
                    {
                        var jCukai = _unitOfWork.JCukaiRepo.GetById((int)item.JCukaiId);    
                        item.JCukai = jCukai;
                    }
                    else
                    {
                        item.JCukai = new JCukai() { Kod = "-" };
                    }
                }

                List<AkBelianPerihal> perihal = _cart.AkBelianPerihal.ToList();

                return Json(new { result = "OK", objek = objek.OrderBy(d => d.AkCarta?.Kod), perihal = perihal.OrderBy(d => d.Bil) });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }
    }
}
