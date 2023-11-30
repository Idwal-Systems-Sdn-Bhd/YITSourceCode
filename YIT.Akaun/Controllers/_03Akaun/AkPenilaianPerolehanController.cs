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
            DateTime? date1 = null;
            DateTime? date2 = null;

            if (!string.IsNullOrEmpty(searchDate1) && ! string.IsNullOrEmpty(searchDate2))
            {
                date1 = DateTime.Parse(searchDate1);
                date2 = DateTime.Parse(searchDate2);
            }

            PopulateFormFields(searchString, searchDate1, searchDate2);
            
            var akPP = _unitOfWork.AkPenilaianPerolehanRepo.GetResults(searchString,date1,date2,searchColumn, EnStatusBorang.Semua);

            return View(akPP);
        }

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
                return (RedirectToAction(nameof(Index)));
            }
            EmptyCart();
            PopulateCartAkPenilaianPerolehanFromDb(akPP);
            return View(akPP);
        }

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
                return (RedirectToAction(nameof(Index)));
            }
            EmptyCart();
            PopulateCartAkPenilaianPerolehanFromDb(akPP);
            return View(akPP);
        }

        [HttpPost, ActionName("BatalLulus")]
        [ValidateAntiForgeryToken]
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
                    return RedirectToAction(nameof(Index));
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

            return RedirectToAction(nameof(Index));
        }

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
                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModulKelulusan.Penilaian, EnKategoriKelulusan.Pengesah, item.JKWPTJBahagianId, akPP.Jumlah) == false)
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

                _context.Add(akPP);
                _appLog.Insert("Tambah", akPP.NoRujukan ?? "", akPP.NoRujukan ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya ditambah..!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.PN.GetDisplayName(), akPP.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
            PopulateDropDownList(fullName ?? "", akPP.JKWId);
            PopulateListViewFromCart();
            return View(akPP);
        }

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
                return (RedirectToAction(nameof(Index)));
            }

            EmptyCart();
            PopulateDropDownList(akPP.DPemohon?.Nama ?? "", akPP.JKWId);
            PopulateCartAkPenilaianPerolehanFromDb(akPP);
            return View(akPP);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModulKelulusan.Penilaian, EnKategoriKelulusan.Pengesah, item.JKWPTJBahagianId, akPP.Jumlah) == false)
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

                return RedirectToAction(nameof(Index));
            }

            PopulateDropDownList(fullName ?? "", akPP.JKWId);
            PopulateListViewFromCart();
            return View(akPP);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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

            return RedirectToAction(nameof(Index));
        }

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

            return RedirectToAction(nameof(Index));
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

        private void PopulateFormFields(string searchString, string searchDate1, string searchDate2)
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
                    _cart.AddItemPerihal(akPenilaianPerolehanPerihal.AkPenilaianPerolehanId,akPenilaianPerolehanPerihal.Bil, akPenilaianPerolehanPerihal.Perihal, akPenilaianPerolehanPerihal.Kuantiti,akPenilaianPerolehanPerihal.Unit,akPenilaianPerolehanPerihal.Harga,akPenilaianPerolehanPerihal.Amaun);
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

                    _cart.AddItemPerihal(akPenilaianPerolehanPerihal.AkPenilaianPerolehanId, akPenilaianPerolehanPerihal.Bil, akPenilaianPerolehanPerihal.Perihal, akPenilaianPerolehanPerihal.Kuantiti, akPenilaianPerolehanPerihal.Unit, akPenilaianPerolehanPerihal.Harga, akPenilaianPerolehanPerihal.Amaun);
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
    }
}
