﻿using Microsoft.AspNetCore.Authorization;
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
    public class AkPOController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodAkPO;
        public const string namamodul = Modules.namaAkPO;
        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;
        private readonly CartAkPO _cart;

        public AkPOController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices,
            CartAkPO cart)
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

            var akPO = _unitOfWork.AkPORepo.GetResults(searchString, date1, date2, searchColumn, EnStatusBorang.Semua);

            return View(akPO);
        }

        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akPO = _unitOfWork.AkPORepo.GetDetailsById((int)id);
            if (akPO == null)
            {
                return NotFound();
            }
            EmptyCart();
            PopulateCartAkPOFromDb(akPO);
            return View(akPO);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akPO = _unitOfWork.AkPORepo.GetDetailsById((int)id);
            if (akPO == null)
            {
                return NotFound();
            }

            if (akPO.EnStatusBorang != EnStatusBorang.None)
            {
                TempData[SD.Error] = "Hapus data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index)));
            }
            EmptyCart();
            PopulateCartAkPOFromDb(akPO);
            return View(akPO);
        }

        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);

            EmptyCart();
            PopulateDropDownList(1);
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.PO.GetDisplayName(), DateTime.Now.ToString("yyyy"));
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AkPO akPO, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            // check if there is pengesah available or not based on modul, kelulusan, and bahagian
            if (_cart.AkPOObjek != null && _cart.AkPOObjek.Count() > 0)
            {
                foreach (var item in _cart.AkPOObjek)
                {
                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModul.Perolehan, EnKategoriKelulusan.Pelulus, item.JKWPTJBahagianId, akPO.Jumlah) == false)
                    {
                        TempData[SD.Error] = "Tiada Pelulus yang wujud untuk senarai kod bahagian berikut.";
                        ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.PO.GetDisplayName(), akPO.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
                        PopulateDropDownList(akPO.JKWId);
                        PopulateListViewFromCart();
                        return View(akPO);
                    }
                }
            }
            //

            if (ModelState.IsValid)
            {

                akPO.UserId = user?.UserName ?? "";
                akPO.TarMasuk = DateTime.Now;
                akPO.DPekerjaMasukId = pekerjaId;

                akPO.AkPOObjek = _cart.AkPOObjek?.ToList();
                akPO.AkPOPerihal = _cart.AkPOPerihal.ToList();

                _context.Add(akPO);
                _appLog.Insert("Tambah", akPO.NoRujukan ?? "", akPO.NoRujukan ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya ditambah..!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.PO.GetDisplayName(), akPO.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
            PopulateDropDownList(akPO.JKWId);
            PopulateListViewFromCart();
            return View(akPO);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akPO = _unitOfWork.AkPORepo.GetDetailsById((int)id);
            if (akPO == null)
            {
                return NotFound();
            }

            if (akPO.EnStatusBorang != EnStatusBorang.None)
            {
                TempData[SD.Error] = "Ubah data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index)));
            }

            EmptyCart();
            PopulateDropDownList(akPO.JKWId);
            PopulateCartAkPOFromDb(akPO);
            return View(akPO);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AkPO akPO, string? fullName, string syscode)
        {
            if (id != akPO.Id)
            {
                return NotFound();
            }

            // check if there is pengesah available or not based on modul, kelulusan, and bahagian
            if (_cart.AkPOObjek != null && _cart.AkPOObjek.Count() > 0)
            {
                foreach (var item in _cart.AkPOObjek)
                {
                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModul.Perolehan, EnKategoriKelulusan.Pengesah, item.JKWPTJBahagianId, akPO.Jumlah) == false)
                    {
                        TempData[SD.Error] = "Tiada Pengesah yang wujud untuk senarai kod bahagian berikut.";
                        PopulateDropDownList(akPO.JKWId);
                        PopulateListViewFromCart();
                        return View(akPO);
                    }
                }
            }
            //

            if (akPO.NoRujukan != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = _unitOfWork.AkPORepo.GetDetailsById(id);
                    var jumlahAsal = objAsal!.Jumlah;
                    akPO.NoRujukan = objAsal.NoRujukan;
                    akPO.UserId = objAsal.UserId;
                    akPO.TarMasuk = objAsal.TarMasuk;
                    akPO.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                    if (objAsal.AkPOObjek != null && objAsal.AkPOObjek.Count > 0)
                    {
                        foreach (var item in objAsal.AkPOObjek)
                        {
                            var model = _context.AkPOObjek.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null) _context.Remove(model);
                        }
                    }

                    if (objAsal.AkPOPerihal != null && objAsal.AkPOPerihal.Count > 0)
                    {
                        foreach (var item in objAsal.AkPOPerihal)
                        {
                            var model = _context.AkPOPerihal.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null) _context.Remove(model);
                        }
                    }

                    _context.Entry(objAsal).State = EntityState.Detached;

                    akPO.UserIdKemaskini = user?.UserName ?? "";
                    akPO.TarKemaskini = DateTime.Now;
                    akPO.AkPOObjek = _cart.AkPOObjek?.ToList();
                    akPO.AkPOPerihal = _cart.AkPOPerihal.ToList();

                    _unitOfWork.AkPORepo.Update(akPO);

                    if (jumlahAsal != akPO.Jumlah)
                    {
                        _appLog.Insert("Ubah", Convert.ToDecimal(jumlahAsal).ToString("#,##0.00") + " -> " + Convert.ToDecimal(akPO.Jumlah).ToString("#,##0.00") + " : " + akPO.NoRujukan ?? "", akPO.NoRujukan ?? "", id, akPO.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }
                    else
                    {
                        _appLog.Insert("Ubah", akPO.NoRujukan ?? "", akPO.NoRujukan ?? "", id, akPO.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }

                    await _context.SaveChangesAsync();

                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AkPOExist(akPO.Id))
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

            PopulateDropDownList(akPO.JKWId);
            PopulateListViewFromCart();
            return View(akPO);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string sebabHapus, string syscode)
        {
            var akPO = _unitOfWork.AkPORepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akPO != null && await _unitOfWork.AkPORepo.IsSahAsync(id) == false)
            {
                akPO.UserIdKemaskini = user?.UserName ?? "";
                akPO.TarKemaskini = DateTime.Now;
                akPO.DPekerjaKemaskiniId = pekerjaId;
                akPO.SebabHapus = sebabHapus;

                _context.AkPO.Remove(akPO);
                _appLog.Insert("Hapus", akPO.NoRujukan ?? "", akPO.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var obj = await _context.AkPO.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.AkPO.Update(obj);

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


                    // get akPO 
                    var akPO = _unitOfWork.AkPORepo.GetDetailsById((int)id);
                    if (akPO == null)
                    {
                        TempData[SD.Error] = "Data tidak wujud.";
                    }
                    else
                    {

                        if (akPO.NoRujukan != null)
                        {
                            // check is it posted or not
                            if (await _unitOfWork.AkPORepo.IsPostedAsync((int)id, akPO.NoRujukan) == false)
                            {
                                TempData[SD.Error] = "Data belum diposting.";
                                return RedirectToAction(nameof(Index));
                            }

                            // posting start here
                            _unitOfWork.AkPORepo.RemovePostingFromAbBukuVot(akPO, user?.UserName ?? "");

                            //insert applog
                            _appLog.Insert("UnPosting", "UnPosting Data", akPO.NoRujukan, (int)id, akPO.Jumlah, pekerjaId, modul, syscode, namamodul, user);

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
        private bool AkPOExist(int id)
        {
            return _unitOfWork.AkPORepo.IsExist(b => b.Id == id);
        }

        private void PopulateListViewFromCart()
        {
            List<AkPOObjek> objek = _cart.AkPOObjek.ToList();

            foreach (AkPOObjek item in objek)
            {
                var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                item.JKWPTJBahagian = jkwPtjBahagian;

                item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                item.AkCarta = akCarta;
            }

            ViewBag.akPOObjek = objek;

            List<AkPOPerihal> perihal = _cart.AkPOPerihal.ToList();

            ViewBag.akPOPerihal = perihal;
        }

        private string GenerateRunningNumber(string initNoRujukan, string tahun)
        {
            var maxRefNo = _unitOfWork.AkPORepo.GetMaxRefNo(initNoRujukan, tahun);

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
            ViewBag.AkPenilaianPerolehan = _unitOfWork.AkPenilaianPerolehanRepo.GetAllByJenis(0);
            ViewBag.EnJenisPerolehan = EnumHelper<EnJenisPerolehan>.GetList();
        }

        private void PopulateCartAkPOFromDb(AkPO akPO)
        {
            if (akPO.AkPOObjek != null)
            {
                foreach (var item in akPO.AkPOObjek)
                {
                    _cart.AddItemObjek(
                            akPO.Id,
                            item.JKWPTJBahagianId,
                            item.AkCartaId,
                            item.Amaun);
                }
            }

            if (akPO.AkPOPerihal != null)
            {
                foreach (var item in akPO.AkPOPerihal)
                {
                    _cart.AddItemPerihal(
                        akPO.Id,
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

        public JsonResult GetAkPenilaianPerolehanDetails(int id, int? akPOId)
        {
            try
            {
                EmptyCart();
                var data = _unitOfWork.AkPenilaianPerolehanRepo.GetDetailsById(id);

                if (data != null)
                {
                    if (data.AkPenilaianPerolehanObjek != null)
                    {
                        foreach (var item in data.AkPenilaianPerolehanObjek)
                        {
                            _cart.AddItemObjek(
                                    akPOId ?? 0,
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
                                akPOId ?? 0,
                                item.Bil,
                                item.Perihal,
                                item.Kuantiti,
                                item.Unit,
                                item.Harga,
                                item.Amaun
                                );
                        }
                    }
                    return Json(new {result = "OK", record = data});
                }
                else
                {
                    return Json(new { result = "Error", message = "data tidak wujud!" });
                }
            } catch (Exception ex)
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

        public JsonResult SaveCartAkPOObjek(AkPOObjek akPOObjek)
        {
            try
            {
                if (akPOObjek != null)
                {
                    _cart.AddItemObjek(akPOObjek.AkPOId, akPOObjek.JKWPTJBahagianId, akPOObjek.AkCartaId, akPOObjek.Amaun);
                }



                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkPOObjek(AkPOObjek akPOObjek)
        {
            try
            {
                if (akPOObjek != null)
                {
                    _cart.RemoveItemObjek(akPOObjek.JKWPTJBahagianId, akPOObjek.AkCartaId);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkPOObjek(AkPOObjek akPOObjek)
        {

            try
            {
                AkPOObjek data = _cart.AkPOObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akPOObjek.JKWPTJBahagianId && x.AkCartaId == akPOObjek.AkCartaId) ?? new AkPOObjek();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkPOObjek(AkPOObjek akPOObjek)
        {

            try
            {

                var data = _cart.AkPOObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akPOObjek.JKWPTJBahagianId && x.AkCartaId == akPOObjek.AkCartaId);

                var user = _userManager.GetUserName(User);

                if (data != null)
                {
                    _cart.RemoveItemObjek(akPOObjek.JKWPTJBahagianId, akPOObjek.AkCartaId);

                    _cart.AddItemObjek(akPOObjek.AkPOId,
                                    akPOObjek.JKWPTJBahagianId,
                                    akPOObjek.AkCartaId,
                                    akPOObjek.Amaun);
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
                var akPO = _cart.AkPOPerihal.FirstOrDefault(pp => pp.Bil == Bil);
                if (akPO != null)
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

        public JsonResult SaveCartAkPOPerihal(AkPOPerihal akPOPerihal)
        {
            try
            {
                if (akPOPerihal != null)
                {
                    _cart.AddItemPerihal(akPOPerihal.AkPOId, akPOPerihal.Bil, akPOPerihal.Perihal, akPOPerihal.Kuantiti, akPOPerihal.Unit, akPOPerihal.Harga, akPOPerihal.Amaun);
                }



                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkPOPerihal(AkPOPerihal akPOPerihal)
        {
            try
            {
                if (akPOPerihal != null)
                {
                    _cart.RemoveItemPerihal(akPOPerihal.Bil);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkPOPerihal(AkPOPerihal akPOPerihal)
        {

            try
            {
                AkPOPerihal data = _cart.AkPOPerihal.FirstOrDefault(x => x.Bil == akPOPerihal.Bil) ?? new AkPOPerihal();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkPOPerihal(AkPOPerihal akPOPerihal)
        {

            try
            {

                var akPO = _cart.AkPOPerihal.FirstOrDefault(x => x.Bil == akPOPerihal.Bil);

                var user = _userManager.GetUserName(User);

                if (akPO != null)
                {
                    _cart.RemoveItemPerihal(akPOPerihal.Bil);

                    _cart.AddItemPerihal(akPOPerihal.AkPOId, akPOPerihal.Bil, akPOPerihal.Perihal, akPOPerihal.Kuantiti, akPOPerihal.Unit, akPOPerihal.Harga, akPOPerihal.Amaun);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAllItemCartAkPO()
        {

            try
            {
                List<AkPOObjek> objek = _cart.AkPOObjek.ToList();

                foreach (AkPOObjek item in objek)
                {
                    var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    item.JKWPTJBahagian = jkwPtjBahagian;

                    item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                    var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                    item.AkCarta = akCarta;
                }

                List<AkPOPerihal> perihal = _cart.AkPOPerihal.ToList();

                return Json(new { result = "OK", objek = objek.OrderBy(d => d.AkCarta?.Kod), perihal = perihal.OrderBy(d => d.Bil) });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }
    }
}
