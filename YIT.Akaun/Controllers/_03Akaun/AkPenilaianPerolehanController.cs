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
    public class AkPenilaianPerolehanController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodAkPenilaianPerolehan;
        public const string namamodul = Modules.namaAkPenilaianPerolehan;
        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;
        private readonly CartAkPenilaianPerolehan _cart;

        public AkPenilaianPerolehanController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices,
            CartAkPenilaianPerolehan cart
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
            string searchColumn
            )
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

            var akPP = _unitOfWork.AkPenilaianPerolehanRepo.GetResults(searchString,date1,date2,searchColumn, EnStatusBorang.Semua);

            foreach( var item in akPP)
            {
                var akPO = _context.AkPO.FirstOrDefault(p => p.AkPenilaianPerolehanId == item.Id);
                if (akPO != null ) item.AkPO = akPO;
                var akInden = _context.AkInden.FirstOrDefault(p => p.AkPenilaianPerolehanId == item.Id);
                if (akInden != null) item.AkInden = akInden;
                var akPVInvois = _context.AkPVInvois.FirstOrDefault(i => i.AkBelian!.AkPO!.AkPenilaianPerolehanId == item.Id) ?? _context.AkPVInvois.FirstOrDefault(i => i.AkBelian!.AkInden!.AkPenilaianPerolehanId == item.Id);
                if (akPVInvois != null)
                {
                    var akPV = _context.AkPV.FirstOrDefault(pv => pv.Id == akPVInvois.AkPVId);
                    if (akPV != null) item.AkPV = akPV;
                }
                

            }
            return View(akPP);
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

            var akPP = _unitOfWork.AkPenilaianPerolehanRepo.GetDetailsById((int)id);
            if (akPP == null)
            {
                return NotFound();
            }
            EmptyCart();
            PopulateCartAkPenilaianPerolehanFromDb(akPP);
            return View(akPP);
        }

        
        [Authorize(Policy = modul + "D")]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akPP = _unitOfWork.AkPenilaianPerolehanRepo.GetDetailsById((int)id);
            if (akPP == null)
            {
                return NotFound();
            }

            if (akPP.EnStatusBorang != EnStatusBorang.None)
            {
                TempData[SD.Error] = "Hapus data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkPenilaianPerolehanFromDb(akPP);
            return View(akPP);
        }

        
        [Authorize(Policy = modul + "BL")]
        public IActionResult BatalLulus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akPP = _unitOfWork.AkPenilaianPerolehanRepo.GetDetailsById((int)id);
            if (akPP == null)
            {
                return NotFound();
            }

            if (akPP.EnStatusBorang != EnStatusBorang.Lulus)
            {
                TempData[SD.Error] = "Data belum diluluskan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkPenilaianPerolehanFromDb(akPP);
            return View(akPP);
        }

        [HttpPost, ActionName("BatalLulus")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "BL")]
        public async Task<IActionResult> BatalLulusConfirmed(int id, string tindakan, string syscode)
        {
            var akPP = _unitOfWork.AkPenilaianPerolehanRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akPP != null)
            {
                if (await _unitOfWork.AkPenilaianPerolehanRepo.IsLulusAsync(id) == false)
                {
                    TempData[SD.Error] = "Data belum diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkPenilaianPerolehanRepo.BatalLulus(id, tindakan, user?.Email);

                _appLog.Insert("UnPosting", "Batal Lulus " + akPP.NoRujukan ?? "", akPP.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var akPenilaianPerolehan = _unitOfWork.AkPenilaianPerolehanRepo.GetDetailsById((int)id);
            if (akPenilaianPerolehan == null)
            {
                return NotFound();
            }

            if (akPenilaianPerolehan.EnStatusBorang != EnStatusBorang.Lulus)
            {
                TempData[SD.Error] = "Data belum diluluskan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkPenilaianPerolehanFromDb(akPenilaianPerolehan);
            return View(akPenilaianPerolehan);
        }

        [HttpPost, ActionName("BatalPos")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "BL")]
        public async Task<IActionResult> BatalPosConfirmed(int id, string tindakan, string syscode)
        {
            var akPenilaianPerolehan = _unitOfWork.AkPenilaianPerolehanRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akPenilaianPerolehan != null && !string.IsNullOrEmpty(akPenilaianPerolehan.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkPenilaianPerolehanRepo.IsPostedAsync((int)id, akPenilaianPerolehan.NoRujukan) == false)
                {
                    TempData[SD.Error] = "Data belum diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkPenilaianPerolehanRepo.IsLulusAsync(id) == false)
                {
                    TempData[SD.Error] = "Data belum diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkPenilaianPerolehanRepo.BatalPos(id, tindakan, user?.UserName);

                _appLog.Insert("UnPosting", "Batal Pos " + akPenilaianPerolehan.NoRujukan ?? "", akPenilaianPerolehan.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var obj = await _context.AkPenilaianPerolehan.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Pos operation

            if (obj != null && !string.IsNullOrEmpty(obj.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkPenilaianPerolehanRepo.IsPostedAsync((int)id, obj.NoRujukan))
                {
                    TempData[SD.Error] = "Data sudah diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkPenilaianPerolehanRepo.IsLulusAsync(id))
                {
                    TempData[SD.Error] = "Data telah diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkPenilaianPerolehanRepo.Lulus(id, pekerjaId, user?.UserName);

                // Batal operation end
                _appLog.Insert("Posting", obj.NoRujukan ?? "", obj.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya pos semula..!";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
        }
        
        [Authorize(Policy = modul+ "C")]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            string? fullName = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.Nama;

            EmptyCart();
            PopulateDropDownList(fullName ?? "", 1);
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.PN.GetDisplayName(),DateTime.Now.ToString("yyyy"));
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "C")]
        public async Task<IActionResult> Create(AkPenilaianPerolehan akPP, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            string? fullName = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.Nama;

            // check if there is pengesah available or not based on modul, kelulusan, and bahagian
            if (_cart.AkPenilaianPerolehanObjek != null && _cart.AkPenilaianPerolehanObjek.Count() > 0)
            {
                foreach (var item in _cart.AkPenilaianPerolehanObjek)
                {
                    var jKWPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModulKelulusan.Penilaian, EnKategoriKelulusan.Pengesah, jKWPtjBahagian.JBahagianId, akPP.Jumlah) == false)
                    {
                        TempData[SD.Error] = "Tiada Pengesah yang wujud untuk senarai kod bahagian berikut.";
                        ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.PN.GetDisplayName(), akPP.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
                        PopulateDropDownList(fullName ?? "", akPP.JKWId);
                        PopulateListViewFromCart();
                        return View(akPP);
                    }
                }
            }
            //

            if (ModelState.IsValid)
            {
                
                akPP.UserId = user?.UserName ?? "";
                akPP.TarMasuk = DateTime.Now;
                akPP.DPekerjaMasukId = pekerjaId;

                akPP.AkPenilaianPerolehanObjek = _cart.AkPenilaianPerolehanObjek?.ToList();
                akPP.AkPenilaianPerolehanPerihal = _cart.AkPenilaianPerolehanPerihal.ToList();
                
                if (akPP.AkPenilaianPerolehanPerihal != null && akPP.AkPenilaianPerolehanPerihal.Any())
                {
                    decimal jumlahCukai = 0;
                    decimal jumlahTanpaCukai = 0;
                    foreach (var item in akPP.AkPenilaianPerolehanPerihal)
                    {
                        jumlahCukai += item.AmaunCukai;
                        jumlahTanpaCukai += (item.Harga * item.Kuantiti);
                    }

                    akPP.JumlahCukai = jumlahCukai;
                    akPP.JumlahTanpaCukai = jumlahTanpaCukai;
                }
                _context.Add(akPP);
                _appLog.Insert("Tambah", akPP.NoRujukan ?? "", akPP.NoRujukan ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya ditambah..!";
                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
            }
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.PN.GetDisplayName(), akPP.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
            PopulateDropDownList(fullName ?? "", akPP.JKWId);
            PopulateListViewFromCart();
            return View(akPP);
        }

        [Authorize(Policy = modul + "E")]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akPP = _unitOfWork.AkPenilaianPerolehanRepo.GetDetailsById((int)id);
            if (akPP == null)
            {
                return NotFound();
            }

            if (akPP.EnStatusBorang != EnStatusBorang.None)
            {
                TempData[SD.Error] = "Ubah data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }

            EmptyCart();
            PopulateDropDownList(akPP.DPemohon?.Nama ?? "", akPP.JKWId);
            PopulateCartAkPenilaianPerolehanFromDb(akPP);
            return View(akPP);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "E")]
        public async Task<IActionResult> Edit(int id, AkPenilaianPerolehan akPP,string? fullName, string syscode)
        {
            if (id != akPP.Id)
            {
                return NotFound();
            }

            // check if there is pengesah available or not based on modul, kelulusan, and bahagian
            if (_cart.AkPenilaianPerolehanObjek != null && _cart.AkPenilaianPerolehanObjek.Count() > 0)
            {
                foreach (var item in _cart.AkPenilaianPerolehanObjek)
                {
                    var jKWPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModulKelulusan.Penilaian, EnKategoriKelulusan.Pengesah, jKWPtjBahagian.JBahagianId, akPP.Jumlah) == false)
                    {
                        TempData[SD.Error] = "Tiada Pengesah yang wujud untuk senarai kod bahagian berikut.";
                        PopulateDropDownList(fullName ?? "", akPP.JKWId);
                        PopulateListViewFromCart();
                        return View(akPP);
                    }
                }
            }
            //

            if (akPP.NoRujukan != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = _unitOfWork.AkPenilaianPerolehanRepo.GetDetailsById(id);
                    var jumlahAsal = objAsal!.Jumlah;
                    akPP.NoRujukan = objAsal.NoRujukan;
                    akPP.UserId = objAsal.UserId;
                    akPP.TarMasuk = objAsal.TarMasuk;
                    akPP.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                    if (objAsal.AkPenilaianPerolehanObjek != null && objAsal.AkPenilaianPerolehanObjek.Count > 0)
                    {
                        foreach (var item in objAsal.AkPenilaianPerolehanObjek)
                        {
                            var model = _context.AkPenilaianPerolehanObjek.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null) _context.Remove(model);
                        }
                    }

                    if (objAsal.AkPenilaianPerolehanPerihal != null && objAsal.AkPenilaianPerolehanPerihal.Count > 0)
                    {
                        foreach(var item in objAsal.AkPenilaianPerolehanPerihal)
                        {
                            var model = _context.AkPenilaianPerolehanPerihal.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null) _context.Remove(model);
                        }
                    }

                    _context.Entry(objAsal).State = EntityState.Detached;

                    akPP.UserIdKemaskini = user?.UserName ?? "";
                    akPP.TarKemaskini = DateTime.Now;
                    akPP.AkPenilaianPerolehanObjek = _cart.AkPenilaianPerolehanObjek?.ToList();
                    akPP.AkPenilaianPerolehanPerihal = _cart.AkPenilaianPerolehanPerihal.ToList();

                    if (akPP.AkPenilaianPerolehanPerihal != null && akPP.AkPenilaianPerolehanPerihal.Any())
                    {
                        decimal jumlahCukai = 0;
                        decimal jumlahTanpaCukai = 0;
                        foreach (var item in akPP.AkPenilaianPerolehanPerihal)
                        {
                            jumlahCukai += item.AmaunCukai;
                            jumlahTanpaCukai += (item.Harga * item.Kuantiti);
                        }

                        akPP.JumlahCukai = jumlahCukai;
                        akPP.JumlahTanpaCukai = jumlahTanpaCukai;
                    }

                    _unitOfWork.AkPenilaianPerolehanRepo.Update(akPP);

                    if (jumlahAsal != akPP.Jumlah)
                    {
                        _appLog.Insert("Ubah", Convert.ToDecimal(jumlahAsal).ToString("#,##0.00") + " -> " + Convert.ToDecimal(akPP.Jumlah).ToString("#,##0.00") + " : " + akPP.NoRujukan ?? "", akPP.NoRujukan ?? "", id, akPP.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }
                    else
                    {
                        _appLog.Insert("Ubah", akPP.NoRujukan ?? "", akPP.NoRujukan ?? "", id, akPP.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }

                    await _context.SaveChangesAsync();

                    TempData[SD.Success] = "Data berjaya diubah..!";
                } catch (DbUpdateConcurrencyException)
                {
                    if (!AkPenilaianPerolehanExist(akPP.Id))
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

            PopulateDropDownList(fullName ?? "", akPP.JKWId);
            PopulateListViewFromCart();
            return View(akPP);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "D")]
        public async Task<IActionResult> DeleteConfirmed(int id,string sebabHapus, string syscode)
        {
            var akPP = _unitOfWork.AkPenilaianPerolehanRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akPP != null && await _unitOfWork.AkPenilaianPerolehanRepo.IsSahAsync(id) == false)
            {
                akPP.UserIdKemaskini = user?.UserName ?? "";
                akPP.TarKemaskini = DateTime.Now;
                akPP.DPekerjaKemaskiniId = pekerjaId;
                akPP.SebabHapus = sebabHapus;

                _context.AkPenilaianPerolehan.Remove(akPP);
                _appLog.Insert("Hapus", akPP.NoRujukan ?? "", akPP.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dihapuskan..!";
            }
            else
            {
                TempData[SD.Error] = "Data telah disahkan / disemak / diluluskan";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
        }

        [Authorize(Policy = modul + "R")]
        public async Task<IActionResult> RollBack(int id, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            var obj = await _context.AkPenilaianPerolehan.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.AkPenilaianPerolehan.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.NoRujukan ?? "", obj.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
        }
        private bool AkPenilaianPerolehanExist(int id)
        {
            return _unitOfWork.AkPenilaianPerolehanRepo.IsExist(b => b.Id == id);
        }

        private string GenerateRunningNumber(string initNoRujukan, string tahun)
        {
            var maxRefNo = _unitOfWork.AkPenilaianPerolehanRepo.GetMaxRefNo(initNoRujukan, tahun);

            var prefix = initNoRujukan + "/" + tahun + "/";
            return RunningNumberFormatter.GenerateRunningNumber(prefix, maxRefNo, "00000");
        }

        private void PopulateDropDownList(string fullName, int JKWId)
        {
            
            ViewBag.FullName = fullName;
            ViewBag.JKW = _unitOfWork.JKWRepo.GetAllDetails();
            ViewBag.DDaftarAwam = _unitOfWork.DDaftarAwamRepo.GetAllDetailsByKategori(EnKategoriDaftarAwam.Pembekal);
            ViewBag.AkCarta = _unitOfWork.AkCartaRepo.GetResultsByParas(EnParas.Paras4);
            ViewBag.JKWPTJBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetails();
            ViewBag.JKWPTJBahagianByJKW = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsByJKWId(JKWId);
            ViewBag.EnKaedahPerolehan = EnumHelper<EnKaedahPerolehan>.GetList();
            ViewBag.EnLHDNJenisCukai = EnumHelper<EnLHDNJenisCukai>.GetList();
            ViewBag.LHDNMSIC = _unitOfWork.LHDNMSICRepo.GetAll();
            ViewBag.LHDNKodKlasifikasi = _unitOfWork.LHDNKodKlasifikasiRepo.GetAll();
            ViewBag.LHDNUnitUkuran = _unitOfWork.LHDNUnitUkuranRepo.GetAll();

        }

        private void PopulateCartAkPenilaianPerolehanFromDb(AkPenilaianPerolehan akPP)
        {
            if (akPP.AkPenilaianPerolehanObjek != null)
            {
                foreach (var item in akPP.AkPenilaianPerolehanObjek)
                {
                    _cart.AddItemObjek(
                            akPP.Id,
                            item.JKWPTJBahagianId,
                            item.AkCartaId,
                            item.Amaun);
                }
            }

            if (akPP.AkPenilaianPerolehanPerihal != null)
            {
                foreach (var item in akPP.AkPenilaianPerolehanPerihal)
                {
                    _cart.AddItemPerihal(
                        akPP.Id,
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

                PopulateListViewFromCart();
            }
        }

        private void PopulateListViewFromCart()
        {
            List<AkPenilaianPerolehanObjek> objek = _cart.AkPenilaianPerolehanObjek.ToList();

            foreach (AkPenilaianPerolehanObjek item in objek)
            {
                var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                item.JKWPTJBahagian = jkwPtjBahagian;

                item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                item.AkCarta = akCarta;
            }

            ViewBag.akPenilaianPerolehanObjek = objek;

            List<AkPenilaianPerolehanPerihal> perihal = _cart.AkPenilaianPerolehanPerihal.ToList();

            ViewBag.akPenilaianPerolehanPerihal = perihal;
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

        public JsonResult SaveCartAkPenilaianPerolehanObjek(AkPenilaianPerolehanObjek akPenilaianPerolehanObjek)
        {
            try
            {
                if (akPenilaianPerolehanObjek != null)
                {
                    _cart.AddItemObjek(akPenilaianPerolehanObjek.AkPenilaianPerolehanId, akPenilaianPerolehanObjek.JKWPTJBahagianId, akPenilaianPerolehanObjek.AkCartaId, akPenilaianPerolehanObjek.Amaun);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }   
        }

        public JsonResult RemoveCartAkPenilaianPerolehanObjek(AkPenilaianPerolehanObjek akPenilaianPerolehanObjek)
        {
            try
            {
                if (akPenilaianPerolehanObjek != null)
                {
                    _cart.RemoveItemObjek(akPenilaianPerolehanObjek.JKWPTJBahagianId, akPenilaianPerolehanObjek.AkCartaId);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkPenilaianPerolehanObjek(AkPenilaianPerolehanObjek akPenilaianPerolehanObjek)
        {

            try
            {
                AkPenilaianPerolehanObjek data = _cart.AkPenilaianPerolehanObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akPenilaianPerolehanObjek.JKWPTJBahagianId && x.AkCartaId == akPenilaianPerolehanObjek.AkCartaId) ?? new AkPenilaianPerolehanObjek();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkPenilaianPerolehanObjek(AkPenilaianPerolehanObjek akPenilaianPerolehanObjek)
        {

            try
            {

                var akTO = _cart.AkPenilaianPerolehanObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akPenilaianPerolehanObjek.JKWPTJBahagianId && x.AkCartaId == akPenilaianPerolehanObjek.AkCartaId);

                var user = _userManager.GetUserName(User);

                if (akTO != null)
                {
                    _cart.RemoveItemObjek(akPenilaianPerolehanObjek.JKWPTJBahagianId, akPenilaianPerolehanObjek.AkCartaId);

                    _cart.AddItemObjek(akPenilaianPerolehanObjek.AkPenilaianPerolehanId,
                                    akPenilaianPerolehanObjek.JKWPTJBahagianId,
                                    akPenilaianPerolehanObjek.AkCartaId,
                                    akPenilaianPerolehanObjek.Amaun);
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
                var akPP = _cart.AkPenilaianPerolehanPerihal.FirstOrDefault(pp => pp.Bil == Bil);
                if (akPP != null)
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

        public JsonResult SaveCartAkPenilaianPerolehanPerihal(AkPenilaianPerolehanPerihal akPenilaianPerolehanPerihal)
        {
            try
            {
                if (akPenilaianPerolehanPerihal != null)
                {
                    _cart.AddItemPerihal(akPenilaianPerolehanPerihal.AkPenilaianPerolehanId, akPenilaianPerolehanPerihal.Bil, akPenilaianPerolehanPerihal.Perihal, akPenilaianPerolehanPerihal.Kuantiti, akPenilaianPerolehanPerihal.LHDNKodKlasifikasiId ?? _unitOfWork.LHDNKodKlasifikasiRepo.GetByCodeAsync("022").Result.Id, akPenilaianPerolehanPerihal.LHDNUnitUkuranId ?? _unitOfWork.LHDNUnitUkuranRepo.GetByCodeAsync("C62").Result.Id, akPenilaianPerolehanPerihal.Unit, akPenilaianPerolehanPerihal.EnLHDNJenisCukai, akPenilaianPerolehanPerihal.KadarCukai, akPenilaianPerolehanPerihal.AmaunCukai, akPenilaianPerolehanPerihal.Harga, akPenilaianPerolehanPerihal.Amaun);
                }



                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkPenilaianPerolehanPerihal(AkPenilaianPerolehanPerihal akPenilaianPerolehanPerihal)
        {
            try
            {
                if (akPenilaianPerolehanPerihal != null)
                {
                    _cart.RemoveItemPerihal(akPenilaianPerolehanPerihal.Bil);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkPenilaianPerolehanPerihal(AkPenilaianPerolehanPerihal akPenilaianPerolehanPerihal)
        {

            try
            {
                AkPenilaianPerolehanPerihal data = _cart.AkPenilaianPerolehanPerihal.FirstOrDefault(x => x.Bil == akPenilaianPerolehanPerihal.Bil) ?? new AkPenilaianPerolehanPerihal();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkPenilaianPerolehanPerihal(AkPenilaianPerolehanPerihal akPenilaianPerolehanPerihal)
        {

            try
            {

                var akPP = _cart.AkPenilaianPerolehanPerihal.FirstOrDefault(x => x.Bil == akPenilaianPerolehanPerihal.Bil);

                var user = _userManager.GetUserName(User);

                if (akPP != null)
                {
                    _cart.RemoveItemPerihal(akPenilaianPerolehanPerihal.Bil);

                    _cart.AddItemPerihal(akPenilaianPerolehanPerihal.AkPenilaianPerolehanId, akPenilaianPerolehanPerihal.Bil, akPenilaianPerolehanPerihal.Perihal, akPenilaianPerolehanPerihal.Kuantiti, akPenilaianPerolehanPerihal.LHDNKodKlasifikasiId ?? _unitOfWork.LHDNKodKlasifikasiRepo.GetByCodeAsync("022").Result.Id, akPenilaianPerolehanPerihal.LHDNUnitUkuranId ?? _unitOfWork.LHDNUnitUkuranRepo.GetByCodeAsync("C62").Result.Id, akPenilaianPerolehanPerihal.Unit, akPenilaianPerolehanPerihal.EnLHDNJenisCukai, akPenilaianPerolehanPerihal.KadarCukai, akPenilaianPerolehanPerihal.AmaunCukai, akPenilaianPerolehanPerihal.Harga, akPenilaianPerolehanPerihal.Amaun);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAllItemCartAkPenilaianPerolehan()
        {

            try
            {
                List<AkPenilaianPerolehanObjek> objek = _cart.AkPenilaianPerolehanObjek.ToList();

                foreach (AkPenilaianPerolehanObjek item in objek)
                {
                    var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    item.JKWPTJBahagian = jkwPtjBahagian;

                    item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                    var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                    item.AkCarta = akCarta;
                }

                List<AkPenilaianPerolehanPerihal> perihal = _cart.AkPenilaianPerolehanPerihal.ToList();

                return Json(new { result = "OK", objek = objek.OrderBy(d => d.AkCarta?.Kod), perihal = perihal.OrderBy(d => d.Bil) });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        // printing akPenilaianPerolehan
        [AllowAnonymous]
        public async Task<IActionResult> PrintPDFById(int id)
        {
            AkPenilaianPerolehan akPenilaianPerolehan = _unitOfWork.AkPenilaianPerolehanRepo.GetDetailsById(id);

            var company = await _userServices.GetCompanyDetails();
            EmptyCart();
            PopulateCartAkPenilaianPerolehanFromDb(akPenilaianPerolehan);
            //string customSwitches = "--page-offset 0 --footer-center [page] / [toPage] --footer-font-size 6";

            return new ViewAsPdf(modul + EnJenisFail.PDF, akPenilaianPerolehan,
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
                //        " --footer-line --footer-font-size \"7\" --footer-spacing 1 --footer-font-name \"Segoe UI\"",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
            };
        }
        // printing akPenilaianPerolehan end
    }
}
