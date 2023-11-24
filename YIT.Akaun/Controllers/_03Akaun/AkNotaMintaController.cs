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

namespace YIT.Akaun.Controllers._03Akaun
{
    [Authorize]
    public class AkNotaMintaController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodAkNotaMinta;
        public const string namamodul = Modules.namaAkNotaMinta;
        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;
        private readonly CartAkNotaMinta _cart;

        public AkNotaMintaController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices,
            CartAkNotaMinta cart
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

            if (!string.IsNullOrEmpty(searchDate1) && !string.IsNullOrEmpty(searchDate2))
            {
                date1 = DateTime.Parse(searchDate1);
                date2 = DateTime.Parse(searchDate2);
            }

            PopulateFormFields(searchString, searchDate1, searchDate2);

            var akNotaMinta = _unitOfWork.AkNotaMintaRepo.GetResults(searchString, date1, date2, searchColumn, EnStatusBorang.Semua);

            return View(akNotaMinta);
        }

        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akNotaMinta = _unitOfWork.AkNotaMintaRepo.GetDetailsById((int)id);
            if (akNotaMinta == null)
            {
                return NotFound();
            }
            EmptyCart();
            PopulateCartAkNotaMintaFromDb(akNotaMinta);
            return View(akNotaMinta);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akNotaMinta = _unitOfWork.AkNotaMintaRepo.GetDetailsById((int)id);
            if (akNotaMinta == null)
            {
                return NotFound();
            }

            if (akNotaMinta.EnStatusBorang != EnStatusBorang.None)
            {
                TempData[SD.Error] = "Hapus data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index)));
            }
            EmptyCart();
            PopulateCartAkNotaMintaFromDb(akNotaMinta);
            return View(akNotaMinta);
        }

        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            string? fullName = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.Nama;

            EmptyCart();
            PopulateDropDownList(fullName ?? "");
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.NM.GetDisplayName(), DateTime.Now.ToString("yyyy"));
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AkNotaMinta akNotaMinta, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            string? fullName = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.Nama;

            // check if there is pengesah available or not based on modul, kelulusan, and bahagian
            if (_cart.AkNotaMintaObjek != null && _cart.AkNotaMintaObjek.Count() > 0)
            {
                foreach (var item in _cart.AkNotaMintaObjek)
                {
                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModulKelulusan.Penilaian, EnKategoriKelulusan.Pengesah, item.JKWPTJBahagianId, akNotaMinta.Jumlah) == false)
                    {
                        TempData[SD.Error] = "Tiada Pengesah yang wujud untuk senarai kod bahagian berikut.";
                        ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.NM.GetDisplayName(), akNotaMinta.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
                        PopulateDropDownList(fullName ?? "");
                        PopulateListViewFromCart();
                        return View(akNotaMinta);
                    }
                }
            }
            //

            if (ModelState.IsValid)
            {

                akNotaMinta.UserId = user?.UserName ?? "";
                akNotaMinta.TarMasuk = DateTime.Now;
                akNotaMinta.DPekerjaMasukId = pekerjaId;

                akNotaMinta.AkNotaMintaObjek = _cart.AkNotaMintaObjek?.ToList();
                akNotaMinta.AkNotaMintaPerihal = _cart.AkNotaMintaPerihal.ToList();

                _context.Add(akNotaMinta);
                _appLog.Insert("Tambah", akNotaMinta.NoRujukan ?? "", akNotaMinta.NoRujukan ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya ditambah..!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.NM.GetDisplayName(), akNotaMinta.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
            PopulateDropDownList(fullName ?? "");
            PopulateListViewFromCart();
            return View(akNotaMinta);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akNotaMinta = _unitOfWork.AkNotaMintaRepo.GetDetailsById((int)id);
            if (akNotaMinta == null)
            {
                return NotFound();
            }

            if (akNotaMinta.EnStatusBorang != EnStatusBorang.None)
            {
                TempData[SD.Error] = "Ubah data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index)));
            }

            EmptyCart();
            PopulateDropDownList(akNotaMinta.DPemohon?.Nama ?? "");
            PopulateCartAkNotaMintaFromDb(akNotaMinta);
            return View(akNotaMinta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AkNotaMinta akNotaMinta, string? fullName, string syscode)
        {
            if (id != akNotaMinta.Id)
            {
                return NotFound();
            }

            // check if there is pengesah available or not based on modul, kelulusan, and bahagian
            if (_cart.AkNotaMintaObjek != null && _cart.AkNotaMintaObjek.Count() > 0)
            {
                foreach (var item in _cart.AkNotaMintaObjek)
                {
                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModulKelulusan.Penilaian, EnKategoriKelulusan.Pengesah, item.JKWPTJBahagianId, akNotaMinta.Jumlah) == false)
                    {
                        TempData[SD.Error] = "Tiada Pengesah yang wujud untuk senarai kod bahagian berikut.";
                        PopulateDropDownList(fullName ?? "");
                        PopulateListViewFromCart();
                        return View(akNotaMinta);
                    }
                }
            }
            //

            if (akNotaMinta.NoRujukan != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = _unitOfWork.AkNotaMintaRepo.GetDetailsById(id);
                    var jumlahAsal = objAsal!.Jumlah;
                    akNotaMinta.NoRujukan = objAsal.NoRujukan;
                    akNotaMinta.UserId = objAsal.UserId;
                    akNotaMinta.TarMasuk = objAsal.TarMasuk;
                    akNotaMinta.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                    if (objAsal.AkNotaMintaObjek != null && objAsal.AkNotaMintaObjek.Count > 0)
                    {
                        foreach (var item in objAsal.AkNotaMintaObjek)
                        {
                            var model = _context.AkNotaMintaObjek.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null) _context.Remove(model);
                        }
                    }

                    if (objAsal.AkNotaMintaPerihal != null && objAsal.AkNotaMintaPerihal.Count > 0)
                    {
                        foreach (var item in objAsal.AkNotaMintaPerihal)
                        {
                            var model = _context.AkNotaMintaPerihal.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null) _context.Remove(model);
                        }
                    }

                    _context.Entry(objAsal).State = EntityState.Detached;

                    akNotaMinta.UserIdKemaskini = user?.UserName ?? "";
                    akNotaMinta.TarKemaskini = DateTime.Now;
                    akNotaMinta.AkNotaMintaObjek = _cart.AkNotaMintaObjek?.ToList();
                    akNotaMinta.AkNotaMintaPerihal = _cart.AkNotaMintaPerihal.ToList();

                    _unitOfWork.AkNotaMintaRepo.Update(akNotaMinta);

                    if (jumlahAsal != akNotaMinta.Jumlah)
                    {
                        _appLog.Insert("Ubah", Convert.ToDecimal(jumlahAsal).ToString("#,##0.00") + " -> " + Convert.ToDecimal(akNotaMinta.Jumlah).ToString("#,##0.00") + " : " + akNotaMinta.NoRujukan ?? "", akNotaMinta.NoRujukan ?? "", id, akNotaMinta.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }
                    else
                    {
                        _appLog.Insert("Ubah", akNotaMinta.NoRujukan ?? "", akNotaMinta.NoRujukan ?? "", id, akNotaMinta.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }

                    await _context.SaveChangesAsync();

                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AkNotaMintaExist(akNotaMinta.Id))
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

            PopulateDropDownList(fullName ?? "");
            PopulateListViewFromCart();
            return View(akNotaMinta);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string sebabHapus, string syscode)
        {
            var akNotaMinta = _unitOfWork.AkNotaMintaRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akNotaMinta != null && await _unitOfWork.AkNotaMintaRepo.IsSahAsync(id) == false)
            {
                akNotaMinta.UserIdKemaskini = user?.UserName ?? "";
                akNotaMinta.TarKemaskini = DateTime.Now;
                akNotaMinta.DPekerjaKemaskiniId = pekerjaId;
                akNotaMinta.SebabHapus = sebabHapus;

                _context.AkNotaMinta.Remove(akNotaMinta);
                _appLog.Insert("Hapus", akNotaMinta.NoRujukan ?? "", akNotaMinta.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var obj = await _context.AkNotaMinta.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.AkNotaMinta.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.NoRujukan ?? "", obj.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index));
        }
        private bool AkNotaMintaExist(int id)
        {
            return _unitOfWork.AkNotaMintaRepo.IsExist(b => b.Id == id);
        }

        private string GenerateRunningNumber(string initNoRujukan, string tahun)
        {
            var maxRefNo = _unitOfWork.AkNotaMintaRepo.GetMaxRefNo(initNoRujukan, tahun);

            var prefix = initNoRujukan + "/" + tahun + "/";
            return RunningNumberFormatter.GenerateRunningNumber(prefix, maxRefNo, "00000");
        }

        private void PopulateDropDownList(string fullName)
        {

            ViewBag.FullName = fullName;
            ViewBag.JKW = _unitOfWork.JKWRepo.GetAll();
            ViewBag.DDaftarAwam = _unitOfWork.DDaftarAwamRepo.GetAllDetailsByKategori(EnKategoriDaftarAwam.Pembekal);
            ViewBag.AkCarta = _unitOfWork.AkCartaRepo.GetResultsByParas(EnParas.Paras4);
            ViewBag.JBahagian = _unitOfWork.JBahagianRepo.GetAll();

        }

        private void PopulateCartAkNotaMintaFromDb(AkNotaMinta akNotaMinta)
        {
            if (akNotaMinta.AkNotaMintaObjek != null)
            {
                foreach (var item in akNotaMinta.AkNotaMintaObjek)
                {
                    _cart.AddItemObjek(
                            akNotaMinta.Id,
                            item.JKWPTJBahagianId,
                            item.AkCartaId,
                            item.Amaun);
                }
            }

            if (akNotaMinta.AkNotaMintaPerihal != null)
            {
                foreach (var item in akNotaMinta.AkNotaMintaPerihal)
                {
                    _cart.AddItemPerihal(
                        akNotaMinta.Id,
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
            List<AkNotaMintaObjek> objek = _cart.AkNotaMintaObjek.ToList();

            foreach (AkNotaMintaObjek item in objek)
            {
                var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                item.JKWPTJBahagian = jkwPtjBahagian;

                item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                item.AkCarta = akCarta;
            }

            ViewBag.akNotaMintaObjek = objek;

            List<AkNotaMintaPerihal> perihal = _cart.AkNotaMintaPerihal.ToList();

            ViewBag.akNotaMintaPerihal = perihal;
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

        public JsonResult GetJBahagianAkCarta(int JKWPTJBahagianId, int AkCartaId)
        {
            try
            {
                var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetById(JKWPTJBahagianId);
                if (jkwPtjBahagian == null)
                {
                    return Json(new { result = "Error", message = "Kod akaun tidak wujud" });
                }

                jkwPtjBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                var akCarta = _unitOfWork.AkCartaRepo.GetById(AkCartaId);
                if (akCarta == null)
                {
                    return Json(new { result = "Error", message = "Kod akaun tidak wujud" });
                }

                return Json(new { result = "OK", jkwPtjBahagian, akCarta });
            }
            catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }
        }

        public JsonResult SaveCartAkNotaMintaObjek(AkNotaMintaObjek akNotaMintaObjek)
        {
            try
            {
                if (akNotaMintaObjek != null)
                {
                    _cart.AddItemObjek(akNotaMintaObjek.AkNotaMintaId, akNotaMintaObjek.JKWPTJBahagianId, akNotaMintaObjek.AkCartaId, akNotaMintaObjek.Amaun);
                }



                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkNotaMintaObjek(AkNotaMintaObjek akNotaMintaObjek)
        {
            try
            {
                if (akNotaMintaObjek != null)
                {
                    _cart.RemoveItemObjek(akNotaMintaObjek.JKWPTJBahagianId, akNotaMintaObjek.AkCartaId);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkNotaMintaObjek(AkNotaMintaObjek akNotaMintaObjek)
        {

            try
            {
                AkNotaMintaObjek data = _cart.AkNotaMintaObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akNotaMintaObjek.JKWPTJBahagianId && x.AkCartaId == akNotaMintaObjek.AkCartaId) ?? new AkNotaMintaObjek();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkNotaMintaObjek(AkNotaMintaObjek akNotaMintaObjek)
        {

            try
            {

                var akTO = _cart.AkNotaMintaObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akNotaMintaObjek.JKWPTJBahagianId && x.AkCartaId == akNotaMintaObjek.AkCartaId);

                var user = _userManager.GetUserName(User);

                if (akTO != null)
                {
                    _cart.RemoveItemObjek(akNotaMintaObjek.JKWPTJBahagianId, akNotaMintaObjek.AkCartaId);

                    _cart.AddItemObjek(akNotaMintaObjek.AkNotaMintaId,
                                    akNotaMintaObjek.JKWPTJBahagianId,
                                    akNotaMintaObjek.AkCartaId,
                                    akNotaMintaObjek.Amaun);
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
                var akNotaMinta = _cart.AkNotaMintaPerihal.FirstOrDefault(pp => pp.Bil == Bil);
                if (akNotaMinta != null)
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

        public JsonResult SaveCartAkNotaMintaPerihal(AkNotaMintaPerihal akNotaMintaPerihal)
        {
            try
            {
                if (akNotaMintaPerihal != null)
                {
                    _cart.AddItemPerihal(akNotaMintaPerihal.AkNotaMintaId, akNotaMintaPerihal.Bil, akNotaMintaPerihal.Perihal, akNotaMintaPerihal.Kuantiti, akNotaMintaPerihal.Unit, akNotaMintaPerihal.Harga, akNotaMintaPerihal.Amaun);
                }



                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkNotaMintaPerihal(AkNotaMintaPerihal akNotaMintaPerihal)
        {
            try
            {
                if (akNotaMintaPerihal != null)
                {
                    _cart.RemoveItemPerihal(akNotaMintaPerihal.Bil);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkNotaMintaPerihal(AkNotaMintaPerihal akNotaMintaPerihal)
        {

            try
            {
                AkNotaMintaPerihal data = _cart.AkNotaMintaPerihal.FirstOrDefault(x => x.Bil == akNotaMintaPerihal.Bil) ?? new AkNotaMintaPerihal();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkNotaMintaPerihal(AkNotaMintaPerihal akNotaMintaPerihal)
        {

            try
            {

                var akNotaMinta = _cart.AkNotaMintaPerihal.FirstOrDefault(x => x.Bil == akNotaMintaPerihal.Bil);

                var user = _userManager.GetUserName(User);

                if (akNotaMinta != null)
                {
                    _cart.RemoveItemPerihal(akNotaMintaPerihal.Bil);

                    _cart.AddItemPerihal(akNotaMintaPerihal.AkNotaMintaId, akNotaMintaPerihal.Bil, akNotaMintaPerihal.Perihal, akNotaMintaPerihal.Kuantiti, akNotaMintaPerihal.Unit, akNotaMintaPerihal.Harga, akNotaMintaPerihal.Amaun);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAllItemCartAkNotaMinta()
        {

            try
            {
                List<AkNotaMintaObjek> objek = _cart.AkNotaMintaObjek.ToList();

                foreach (AkNotaMintaObjek item in objek)
                {
                    var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetById(item.JKWPTJBahagianId);

                    item.JKWPTJBahagian = jkwPtjBahagian;

                    item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                    var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                    item.AkCarta = akCarta;
                }

                List<AkNotaMintaPerihal> perihal = _cart.AkNotaMintaPerihal.ToList();

                return Json(new { result = "OK", objek = objek.OrderBy(d => d.AkCarta?.Kod), perihal = perihal.OrderBy(d => d.Bil) });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }
    }
}
