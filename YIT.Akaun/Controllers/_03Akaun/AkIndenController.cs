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
            DateTime? date1 = null;
            DateTime? date2 = null;

            if (!string.IsNullOrEmpty(searchDate1) && !string.IsNullOrEmpty(searchDate2))
            {
                date1 = DateTime.Parse(searchDate1);
                date2 = DateTime.Parse(searchDate2);
            }

            PopulateFormFields(searchString, searchDate1, searchDate2);

            var akInden = _unitOfWork.AkIndenRepo.GetResults(searchString, date1, date2, searchColumn, EnStatusBorang.Semua);

            return View(akInden);
        }

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

            PopulateCartAkIndenFromDb(akInden);
            return View(akInden);
        }

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
                return (RedirectToAction(nameof(Index)));
            }

            PopulateCartAkIndenFromDb(akInden);
            return View(akInden);
        }

        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);

            EmptyCart();
            PopulateDropDownList();
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.IK.GetDisplayName(), DateTime.Now.ToString("yyyy"));
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AkInden akInden, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            // check if there is pengesah available or not based on modul, kelulusan, and bahagian
            if (_cart.AkIndenObjek != null && _cart.AkIndenObjek.Count() > 0)
            {
                foreach (var item in _cart.AkIndenObjek)
                {
                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModul.Perolehan, EnKategoriKelulusan.Pelulus, item.JKWPTJBahagianId, akInden.Jumlah) == false)
                    {
                        TempData[SD.Error] = "Tiada Pelulus yang wujud untuk senarai kod bahagian berikut.";
                        ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.PN.GetDisplayName(), akInden.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
                        PopulateDropDownList();
                        PopulateListViewFromCart();
                        return View(akInden);
                    }
                }
            }
            //

            if (ModelState.IsValid)
            {

                akInden.UserId = user?.UserName ?? "";
                akInden.TarMasuk = DateTime.Now;
                akInden.DPekerjaMasukId = pekerjaId;

                akInden.AkIndenObjek = _cart.AkIndenObjek?.ToList();
                akInden.AkIndenPerihal = _cart.AkIndenPerihal.ToList();

                _context.Add(akInden);
                _appLog.Insert("Tambah", akInden.NoRujukan ?? "", akInden.NoRujukan ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya ditambah..!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.IK.GetDisplayName(), akInden.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
            PopulateDropDownList();
            PopulateListViewFromCart();
            return View(akInden);
        }

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

            if (akInden.EnStatusBorang != EnStatusBorang.None)
            {
                TempData[SD.Error] = "Ubah data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index)));
            }

            EmptyCart();
            PopulateDropDownList();
            PopulateCartAkIndenFromDb(akInden);
            return View(akInden);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AkInden akInden, string? fullName, string syscode)
        {
            if (id != akInden.Id)
            {
                return NotFound();
            }

            // check if there is pengesah available or not based on modul, kelulusan, and bahagian
            if (_cart.AkIndenObjek != null && _cart.AkIndenObjek.Count() > 0)
            {
                foreach (var item in _cart.AkIndenObjek)
                {
                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModul.Perolehan, EnKategoriKelulusan.Pengesah, item.JKWPTJBahagianId, akInden.Jumlah) == false)
                    {
                        TempData[SD.Error] = "Tiada Pengesah yang wujud untuk senarai kod bahagian berikut.";
                        PopulateDropDownList();
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

                return RedirectToAction(nameof(Index));
            }

            PopulateDropDownList();
            PopulateListViewFromCart();
            return View(akInden);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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

            return RedirectToAction(nameof(Index));
        }

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

            return RedirectToAction(nameof(Index));
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
                                return RedirectToAction(nameof(Index));
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

            return RedirectToAction(nameof(Index));
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

        private void PopulateDropDownList()
        {
            ViewBag.JKW = _unitOfWork.JKWRepo.GetAll();
            ViewBag.DDaftarAwam = _unitOfWork.DDaftarAwamRepo.GetAllDetailsByKategori(EnKategoriDaftarAwam.Pembekal);
            ViewBag.AkCarta = _unitOfWork.AkCartaRepo.GetResultsByParas(EnParas.Paras4);
            ViewBag.JBahagian = _unitOfWork.JBahagianRepo.GetAll();
            ViewBag.AkPenilaianPerolehan = _unitOfWork.AkPenilaianPerolehanRepo.GetAllByJenis(1);
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
                        item.Unit,
                        item.Harga,
                        item.Amaun
                        );
                }

                PopulateListViewFromCart();
            }
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

        public JsonResult GetJBahagianAkCarta(int JKWPTJBahagianId, int AkCartaId)
        {
            try
            {
                var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetById(JKWPTJBahagianId);
                if (jkwPtjBahagian == null)
                {
                    return Json(new { result = "Error", message = "Kod akaun tidak wujud" });
                }

                jkwPtjBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.Kod);

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

                var akTO = _cart.AkIndenObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akIndenObjek.JKWPTJBahagianId && x.AkCartaId == akIndenObjek.AkCartaId);

                var user = _userManager.GetUserName(User);

                if (akTO != null)
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
                    _cart.AddItemPerihal(akIndenPerihal.AkIndenId, akIndenPerihal.Bil, akIndenPerihal.Perihal, akIndenPerihal.Kuantiti, akIndenPerihal.Unit, akIndenPerihal.Harga, akIndenPerihal.Amaun);
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

                    _cart.AddItemPerihal(akIndenPerihal.AkIndenId, akIndenPerihal.Bil, akIndenPerihal.Perihal, akIndenPerihal.Kuantiti, akIndenPerihal.Unit, akIndenPerihal.Harga, akIndenPerihal.Amaun);
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
                    var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetById(item.JKWPTJBahagianId);

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
    }
}
