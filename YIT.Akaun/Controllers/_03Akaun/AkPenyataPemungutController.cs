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
    [Authorize(Roles = Init.allExceptAdminRole)]

    public class AkPenyataPemungutController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodAkPenyataPemungut;
        public const string namamodul = Modules.namaAkPenyataPemungut;

        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;
        private readonly CartAkPenyataPemungut _cart;

        public AkPenyataPemungutController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices,
            CartAkPenyataPemungut cart)
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

            var akPenyataPemungut = _unitOfWork.AkPenyataPemungutRepo.GetResults(searchString, date1, date2, searchColumn, EnStatusBorang.Semua);

            return View(akPenyataPemungut);
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

            var akPenyataPemungut = _unitOfWork.AkPenyataPemungutRepo.GetDetailsById((int)id);
            if (akPenyataPemungut == null)
            {
                return NotFound();
            }
            EmptyCart();
            PopulateCartAkPenyataPemungutFromDb(akPenyataPemungut);
            return View(akPenyataPemungut);
        }

        [Authorize(Policy = modul + "D")]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akPenyataPemungut = _unitOfWork.AkPenyataPemungutRepo.GetDetailsById((int)id);
            if (akPenyataPemungut == null)
            {
                return NotFound();
            }

            if (akPenyataPemungut.EnStatusBorang != EnStatusBorang.None)
            {
                TempData[SD.Error] = "Hapus data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkPenyataPemungutFromDb(akPenyataPemungut);
            return View(akPenyataPemungut);
        }

        [Authorize(Policy = modul + "L")]
        public async Task<IActionResult> Posting(int? id, string syscode)
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


                    // get akPenyataPemungut 
                    var akPenyataPemungut = _unitOfWork.AkPenyataPemungutRepo.GetDetailsById((int)id);
                    if (akPenyataPemungut == null)
                    {
                        TempData[SD.Error] = "Data tidak wujud.";
                    }
                    else
                    {

                        if (akPenyataPemungut.NoRujukan != null)
                        {
                            // check is it posted or not
                            if (await _unitOfWork.AkPenyataPemungutRepo.IsPostedAsync((int)id, akPenyataPemungut.NoRujukan) == true)
                            {
                                TempData[SD.Error] = "Data sudah posting.";
                                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                            }

                            // posting start here
                            _unitOfWork.AkPenyataPemungutRepo.UpdateNoSlipAkTerimaTunggalAkAkaun(akPenyataPemungut, user?.UserName ?? "", pekerjaId);

                            //insert applog
                            _appLog.Insert("Posting", "Posting Data", akPenyataPemungut.NoRujukan, (int)id, akPenyataPemungut.Jumlah, pekerjaId, modul, syscode, namamodul, user);

                            //insert applog end

                            await _context.SaveChangesAsync();

                            TempData[SD.Success] = "Posting data berjaya";
                        }
                        else
                        {
                            TempData[SD.Error] = "No Rujukan Tiada";
                        }



                    }

                }
                catch (Exception)
                {
                    TempData[SD.Error] = "Berlaku ralat semasa transaksi. Data gagal posting.";

                }
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


                    // get akPenyataPemungut 
                    var akPenyataPemungut = _unitOfWork.AkPenyataPemungutRepo.GetDetailsById((int)id);
                    if (akPenyataPemungut == null)
                    {
                        TempData[SD.Error] = "Data tidak wujud.";
                    }
                    else
                    {

                        if (akPenyataPemungut.NoRujukan != null)
                        {
                            // check is it posted or not
                            if (await _unitOfWork.AkPenyataPemungutRepo.IsPostedAsync((int)id, akPenyataPemungut.NoRujukan) == false)
                            {
                                TempData[SD.Error] = "Data belum diposting.";
                                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                            }

                            // posting start here
                            _unitOfWork.AkPenyataPemungutRepo.RemoveNoSlipFromAkTerimaTunggalAkAkaun(akPenyataPemungut, user?.UserName ?? "");

                            //insert applog
                            _appLog.Insert("UnPosting", "UnPosting Data", akPenyataPemungut.NoRujukan, (int)id, akPenyataPemungut.Jumlah, pekerjaId, modul, syscode, namamodul, user);

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

        [Authorize(Policy = modul + "C")]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);

            EmptyCart();
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.PP.GetDisplayName(), DateTime.Now.ToString("yyyy"));
            ManipulateHiddenDiv(1);
            PopulateDropDownList(1);
            return View();
        }

        private string GenerateRunningNumber(string initNoRujukan, string tahun)
        {
            var maxRefNo = _unitOfWork.AkPenyataPemungutRepo.GetMaxRefNo(initNoRujukan, tahun);

            var prefix = initNoRujukan + "/" + tahun + "/";
            return RunningNumberFormatter.GenerateRunningNumber(prefix, maxRefNo, "00000");
        }

        private void ManipulateHiddenDiv(int JCaraBayarId)
        {
            var caraBayar = _unitOfWork.JCaraBayarRepo.GetById(JCaraBayarId);

            if (caraBayar != null && caraBayar.Perihal != null)
            {
                if (caraBayar.Perihal.Contains("CEK"))
                {
                    ViewBag.DivJenisCek = "";
                }
                else
                {
                    ViewBag.DivJenisCek = "hidden";
                }
            }
        }

        private void PopulateDropDownList(int JPTJId)
        {
            ViewBag.JPTJ = _unitOfWork.JPTJRepo.GetAll();
            ViewBag.JCawangan = _unitOfWork.JCawanganRepo.GetAll();
            ViewBag.AkBank = _unitOfWork.AkBankRepo.GetAll();
            ViewBag.AkCarta = _unitOfWork.AkCartaRepo.GetResultsByParas(EnParas.Paras4);
            ViewBag.JCaraBayar = _unitOfWork.JCaraBayarRepo.GetAll();
            ViewBag.JKWPTJBahagianByJPTJ = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsByJPTJId(JPTJId);
            ViewBag.EnJenisCek = EnumHelper<EnJenisCek>.GetList();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "C")]
        public async Task<IActionResult> Create(AkPenyataPemungut akPenyataPemungut, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (ModelState.IsValid)
            {

                akPenyataPemungut.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.PP.GetDisplayName(), akPenyataPemungut.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
                akPenyataPemungut.UserId = user?.UserName ?? "";
                akPenyataPemungut.TarMasuk = DateTime.Now;
                akPenyataPemungut.DPekerjaMasukId = pekerjaId;
                akPenyataPemungut.BilResit = _cart.AkPenyataPemungutObjek.GroupBy(ppo => ppo.AkTerimaTunggalId).Count();
                akPenyataPemungut.AkPenyataPemungutObjek = _cart.AkPenyataPemungutObjek?.ToList();

                _context.Add(akPenyataPemungut);
                _appLog.Insert("Tambah", akPenyataPemungut.NoRujukan ?? "", akPenyataPemungut.NoRujukan ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya ditambah..!";
                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
            }
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.PP.GetDisplayName(), akPenyataPemungut.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
            PopulateDropDownList(akPenyataPemungut.JPTJId);
            ManipulateHiddenDiv(akPenyataPemungut.JCaraBayarId);
            PopulateListViewFromCart();
            return View(akPenyataPemungut);
        }

        [Authorize(Policy = modul + "E")]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akPenyataPemungut = _unitOfWork.AkPenyataPemungutRepo.GetDetailsById((int)id);
            if (akPenyataPemungut == null)
            {
                return NotFound();
            }

            EmptyCart();
            PopulateDropDownList(akPenyataPemungut.JPTJId);
            ManipulateHiddenDiv(akPenyataPemungut.JCaraBayarId);

            PopulateCartAkPenyataPemungutFromDb(akPenyataPemungut);
            return View(akPenyataPemungut);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "E")]
        public async Task<IActionResult> Edit(int id, AkPenyataPemungut akPenyataPemungut, string syscode)
        {
            if (id != akPenyataPemungut.Id)
            {
                return NotFound();
            }

            if (akPenyataPemungut.NoRujukan != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = _unitOfWork.AkPenyataPemungutRepo.GetDetailsById(id);
                    var jumlahAsal = objAsal!.Jumlah;
                    akPenyataPemungut.NoRujukan = objAsal.NoRujukan;
                    akPenyataPemungut.JCaraBayarId = objAsal.JCaraBayarId;
                    akPenyataPemungut.AkBankId = objAsal.AkBankId;
                    akPenyataPemungut.JPTJId = objAsal.JPTJId;
                    akPenyataPemungut.JCawanganId = objAsal.JCawanganId;
                    akPenyataPemungut.UserId = objAsal.UserId;
                    akPenyataPemungut.TarMasuk = objAsal.TarMasuk;
                    akPenyataPemungut.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                    if (objAsal.AkPenyataPemungutObjek != null && objAsal.AkPenyataPemungutObjek.Count > 0)
                    {
                        foreach (var item in objAsal.AkPenyataPemungutObjek)
                        {
                            var model = _context.AkPenyataPemungutObjek.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null)
                            {
                                _context.Remove(model);
                            }
                        }
                    }

                    _context.Entry(objAsal).State = EntityState.Detached;

                    akPenyataPemungut.UserIdKemaskini = user?.UserName ?? "";

                    akPenyataPemungut.TarKemaskini = DateTime.Now;
                    akPenyataPemungut.DPekerjaKemaskiniId = pekerjaId;
                    akPenyataPemungut.BilResit = _cart.AkPenyataPemungutObjek.GroupBy(ppo => ppo.AkTerimaTunggalId).Count();

                    akPenyataPemungut.AkPenyataPemungutObjek = _cart.AkPenyataPemungutObjek.ToList();

                    _unitOfWork.AkPenyataPemungutRepo.Update(akPenyataPemungut);

                    if (jumlahAsal != akPenyataPemungut.Jumlah)
                    {
                        _appLog.Insert("Ubah", Convert.ToDecimal(jumlahAsal).ToString("#,##0.00") + " -> " + Convert.ToDecimal(akPenyataPemungut.Jumlah).ToString("#,##0.00") + " : " + akPenyataPemungut.NoRujukan ?? "", akPenyataPemungut.NoRujukan ?? "", id, akPenyataPemungut.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }
                    else
                    {
                        _appLog.Insert("Ubah", akPenyataPemungut.NoRujukan ?? "", akPenyataPemungut.NoRujukan ?? "", id, akPenyataPemungut.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }

                    await _context.SaveChangesAsync();

                    TempData[SD.Success] = "Data berjaya diubah..!";

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AkPenyataPemungutExists(akPenyataPemungut.Id))
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
            ManipulateHiddenDiv(akPenyataPemungut.JCaraBayarId);

            PopulateDropDownList(akPenyataPemungut.JPTJId);
            PopulateListViewFromCart();
            return View(akPenyataPemungut);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "D")]
        public async Task<IActionResult> DeleteConfirmed(int id, string sebabHapus, string syscode)
        {
            var akPenyataPemungut = _unitOfWork.AkPenyataPemungutRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akPenyataPemungut != null && await _unitOfWork.AkPenyataPemungutRepo.IsPostedAsync(id, akPenyataPemungut.NoRujukan ?? "") == false)
            {
                akPenyataPemungut.UserIdKemaskini = user?.UserName ?? "";
                akPenyataPemungut.TarKemaskini = DateTime.Now;
                akPenyataPemungut.DPekerjaKemaskiniId = pekerjaId;
                akPenyataPemungut.SebabHapus = sebabHapus;

                _context.AkPenyataPemungut.Remove(akPenyataPemungut);
                _appLog.Insert("Hapus", akPenyataPemungut.NoRujukan ?? "", akPenyataPemungut.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dihapuskan..!";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
        }

        [Authorize(Policy = modul + "R")]
        public async Task<IActionResult> RollBack(int id, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            var obj = await _context.AkPenyataPemungut.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);
            
            // Batal operation

            if (obj != null)
            {
                // check if already have overlapping AkPenyataPemungutObjek that have same AkTerimaTunggalId, JKWPTJBahagianId, AkCartaId
                List<AkPenyataPemungutObjek> akPenyataPemungutObjek = _unitOfWork.AkPenyataPemungutRepo.GetAkPenyataPemungutObjekByAkPenyataPemungutId(obj.Id);

                if (akPenyataPemungutObjek != null && akPenyataPemungutObjek.Any())
                {
                    foreach (var item in akPenyataPemungutObjek)
                    {
                        if (_unitOfWork.AkPenyataPemungutRepo.IsAkPenyataPemungutObjekExist(pp => 
                            pp.AkTerimaTunggalId == item.AkTerimaTunggalId
                            && pp.JKWPTJBahagianId == item.JKWPTJBahagianId
                            && pp.AkCartaId == item.AkCartaId
                            && pp.AkPenyataPemungut != null
                            && pp.AkPenyataPemungut.FlHapus == 0 
                            && pp.AkPenyataPemungut.FlBatal == 0))
                        {
                            TempData[SD.Error] = "Terdapat data sama pada senarai penyata pemungut sedia ada";
                            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                        }
                    }
                }
                //check end

                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.AkPenyataPemungut.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.NoRujukan ?? "", obj.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
        }

        private bool AkPenyataPemungutExists(int id)
        {
            return _unitOfWork.AkPenyataPemungutRepo.IsExist(b => b.Id == id);
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

        [HttpGet]
        public JsonResult GetKod(string tahun)
        {
            try
            {
                if (tahun != null)
                {
                    var noRujukan = GenerateRunningNumber(EnInitNoRujukan.PP.GetDisplayName(), tahun);

                    return Json(new { result = "OK", record = noRujukan });
                }
                else
                {
                    return Json(new { result = "OK", record = "" });
                }

            }
            catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetCaraBayar(int id)
        {
            try
            {

                var caraBayar = await _context.JCaraBayar.FirstOrDefaultAsync(b => b.Id == id);

                if (caraBayar != null && caraBayar.Perihal != null)
                {
                    if (caraBayar.Perihal.Contains("CEK"))
                    {
                        return Json(new { result = "CEK" });
                    }
                    else
                    {
                        return Json(new { result = "LAIN" });
                    }
                }
                else
                {
                    return Json(new { result = "LAIN" });
                }
                

            }
            catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetAkTerimaTunggalList(int Id, DateTime tarikhDari, DateTime tarikhHingga, int jCaraBayarId, EnJenisCek jenisCek, int jCawanganId, int akBankId, int jPTJId)
        {
            try
            {
                EmptyCart();

                List<AkTerimaTunggal> results = _unitOfWork.AkTerimaTunggalRepo.GetResults(null, tarikhDari, tarikhHingga, "Tarikh");

                results = results
                    .Where(tt => tt.EnJenisCek == jenisCek && tt.AkBankId == akBankId && tt.JCawanganId == jCawanganId && tt.EnStatusBorang == EnStatusBorang.Lulus).ToList();

                List<AkTerimaTunggalObjek> akTerimaTunggalObjekList = new List<AkTerimaTunggalObjek>();

                // filter results
                if (results != null && results.Any())
                {
                    foreach (var result in results)
                    {
                        if (result.AkTerimaTunggalObjek != null && result.AkTerimaTunggalObjek.Any())
                        {
                            foreach (var item in result.AkTerimaTunggalObjek)
                            {
                                // check if AkTerimaTunggalId, JKWPTJBahagianId, AkCartaId already exist in AkPenyataPemungut, continue
                                if (_unitOfWork.AkTerimaTunggalRepo.IsLinkedWithAkPenyataPemungut(item)) continue;

                                if (item.JKWPTJBahagian != null && item.JKWPTJBahagian.JPTJId == jPTJId) akTerimaTunggalObjekList.Add(item);

                            }
                        }
                    }
                }

                //add to cart
                if (akTerimaTunggalObjekList != null && akTerimaTunggalObjekList.Any())
                {

                    decimal bil = 0;

                    foreach (var item in akTerimaTunggalObjekList)
                    {

                            bil++;
                            _cart.AddItemObjek(Id, item.AkTerimaTunggalId, bil, item.JKWPTJBahagianId, item.AkCartaId, item.Amaun);
                        
                    }
                }

                return Json(new { result = "OK", akTerimaTunggalObjekList });

            }
            catch (Exception ex)
            {

                return Json(new { result = "Error", message = ex.Message });
            }

        }

        private void PopulateCartAkPenyataPemungutFromDb(AkPenyataPemungut akPenyataPemungut)
        {
            if (akPenyataPemungut.AkPenyataPemungutObjek != null)
            {
                foreach (var item in akPenyataPemungut.AkPenyataPemungutObjek)
                {
                    _cart.AddItemObjek(
                            akPenyataPemungut.Id,
                            item.AkTerimaTunggalId,
                            item.Bil,
                            item.JKWPTJBahagianId,
                            item.AkCartaId,
                            item.Amaun);
                }
            }

            PopulateListViewFromCart();
        }

        private void PopulateListViewFromCart()
        {
            List<AkPenyataPemungutObjek> objek = _cart.AkPenyataPemungutObjek.ToList();

            foreach (AkPenyataPemungutObjek item in objek)
            {
                var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                item.JKWPTJBahagian = jkwPtjBahagian;

                item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                item.AkCarta = akCarta;

                var akTerimaTunggal = _unitOfWork.AkTerimaTunggalRepo.GetById(item.AkTerimaTunggalId);

                item.AkTerimaTunggal = akTerimaTunggal;


            }

            ViewBag.akPenyataPemungutObjek = objek;
        }

        public JsonResult GetBil(int Bil)
        {
            try
            {
                var akPenyataPemungut = _cart.AkPenyataPemungutObjek.FirstOrDefault(pp => pp.Bil == Bil);
                if (akPenyataPemungut != null)
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

        public JsonResult GetJBahagianAkCartaAkTerimaTunggal(int JKWPTJBahagianId, int AkCartaId, int AkTerimaTunggalId)
        {
            try
            {
                var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetById(JKWPTJBahagianId);
                if (jkwPtjBahagian == null)
                {
                    return Json(new { result = "Error", message = "Kod tidak wujud" });
                }

                var akCarta = _unitOfWork.AkCartaRepo.GetById(AkCartaId);
                if (akCarta == null)
                {
                    return Json(new { result = "Error", message = "Kod akaun tidak wujud" });
                }

                var akTerimaTunggal = _unitOfWork.AkTerimaTunggalRepo.GetById(AkTerimaTunggalId);
                if (akTerimaTunggal == null)
                {
                    return Json(new { result = "Error", message = "Resit tidak wujud" });
                }

                return Json(new { result = "OK", jkwPtjBahagian, akCarta, akTerimaTunggal });
            }
            catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkPenyataPemungutObjek(AkPenyataPemungutObjek akPenyataPemungutObjek)
        {
            try
            {
                if (akPenyataPemungutObjek != null)
                {
                    _cart.RemoveItemObjek(akPenyataPemungutObjek.Bil);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAllItemCartAkPenyataPemungut()
        {

            try
            {
                List<AkPenyataPemungutObjek> objek = _cart.AkPenyataPemungutObjek.ToList();

                foreach (AkPenyataPemungutObjek item in objek)
                {
                    var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    item.JKWPTJBahagian = jkwPtjBahagian;

                    item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                    var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                    item.AkCarta = akCarta;

                    var akTerimaTunggal = _unitOfWork.AkTerimaTunggalRepo.GetById(item.AkTerimaTunggalId);

                    item.AkTerimaTunggal = akTerimaTunggal;

                }

                return Json(new { result = "OK", objek = objek.OrderBy(d => d.AkCarta?.Kod) });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }
    }
}
