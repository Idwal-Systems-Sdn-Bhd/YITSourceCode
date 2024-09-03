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
    public class AkTerimaTunggalController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodAkTerima;
        public const string namamodul = Modules.namaAkTerima;
        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;
        private readonly CartAkTerimaTunggal _cart;

        public AkTerimaTunggalController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices,
            CartAkTerimaTunggal cart
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
            var akTerimaTunggal = _unitOfWork.AkTerimaTunggalRepo.GetResults(searchString, date1, date2, searchColumn);

            return View(akTerimaTunggal);
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

            var akTerimaTunggal = _unitOfWork.AkTerimaTunggalRepo.GetDetailsById((int)id);
            if (akTerimaTunggal == null)
            {
                return NotFound();
            }
            EmptyCart();
            ManipulateHiddenDiv(akTerimaTunggal.EnJenisTerimaan, akTerimaTunggal.EnKategoriDaftarAwam);
            PopulateCartAkTerimaTunggalFromDb(akTerimaTunggal);
            return View(akTerimaTunggal);
        }

        [Authorize(Policy = modul + "D")]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akTerimaTunggal = _unitOfWork.AkTerimaTunggalRepo.GetDetailsById((int)id);
            if (akTerimaTunggal == null)
            {
                return NotFound();
            }
            EmptyCart();
            ManipulateHiddenDiv(akTerimaTunggal.EnJenisTerimaan, akTerimaTunggal.EnKategoriDaftarAwam);
            PopulateCartAkTerimaTunggalFromDb(akTerimaTunggal);
            return View(akTerimaTunggal);
        }

        [Authorize(Policy = modul + "C")]
        public IActionResult Create()
        {
            ManipulateHiddenDiv(EnJenisTerimaan.Am, EnKategoriDaftarAwam.LainLain);
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.RR.GetDisplayName(), DateTime.Now.ToString("yyyy"));
            EmptyCart();
            PopulateDropDownList(1);
            return View();
        }

        private void ManipulateHiddenDiv(EnJenisTerimaan enJenisTerimaan, EnKategoriDaftarAwam enKategoriDaftarAwam)
        {
            switch (enJenisTerimaan)
            {
                case EnJenisTerimaan.Invois:
                    ViewBag.DivInvois = "";
                    break;
                default:
                    ViewBag.DivInvois = "hidden";
                    break;
            }

            switch (enKategoriDaftarAwam)
            {
                case EnKategoriDaftarAwam.Penghutang:
                case EnKategoriDaftarAwam.DaftarAwam:
                    ViewBag.DivDaftarAwam = "";
                    ViewBag.DivPekerja = "hidden";
                    break;
                case EnKategoriDaftarAwam.Pekerja:
                    ViewBag.DivDaftarAwam = "hidden";
                    ViewBag.DivPekerja = "";
                    break;
                default:
                    ViewBag.DivDaftarAwam = "hidden";
                    ViewBag.DivPekerja = "hidden";
                    break;
            }
        }


        private string GenerateRunningNumber(string initNoRujukan, string tahun)
        {
            var maxRefNo = _unitOfWork.AkTerimaTunggalRepo.GetMaxRefNo(initNoRujukan, tahun);

            var prefix = initNoRujukan + "/" + tahun + "/";
            return RunningNumberFormatter.GenerateRunningNumber(prefix, maxRefNo, "00000");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "C")]
        public async Task<IActionResult> Create(AkTerimaTunggal akTerimaTunggal, string syscode)
        {

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                akTerimaTunggal.UserId = user?.UserName ?? "";

                akTerimaTunggal.TarMasuk = DateTime.Now;
                akTerimaTunggal.DPekerjaMasukId = pekerjaId;

                akTerimaTunggal.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.RR.GetDisplayName(), akTerimaTunggal.Tahun ?? DateTime.Now.ToString("yyyy"));
                if (akTerimaTunggal.EnJenisTerimaan == EnJenisTerimaan.Invois) akTerimaTunggal.EnKategoriDaftarAwam = EnKategoriDaftarAwam.Penghutang;
                
                akTerimaTunggal.AkTerimaTunggalObjek = _cart.AkTerimaTunggalObjek.ToList();

                akTerimaTunggal.AkTerimaTunggalInvois = _cart.AkTerimaTunggalInvois.ToList();


                _context.Add(akTerimaTunggal);
                _appLog.Insert("Tambah", akTerimaTunggal.NoRujukan ?? "", akTerimaTunggal.NoRujukan ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya ditambah..!";
                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });

            }

            PopulateDropDownList(akTerimaTunggal.JKWId);
            ManipulateHiddenDiv(akTerimaTunggal.EnJenisTerimaan, akTerimaTunggal.EnKategoriDaftarAwam);
            PopulateListViewFromCart();
            return View(akTerimaTunggal);
        }

        [Authorize(Policy = modul + "E")]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akTerimaTunggal = _unitOfWork.AkTerimaTunggalRepo.GetDetailsById((int)id);
            if (akTerimaTunggal == null)
            {
                return NotFound();
            }

            EmptyCart();
            PopulateDropDownList(akTerimaTunggal.JKWId);
            ManipulateHiddenDiv(akTerimaTunggal.EnJenisTerimaan, akTerimaTunggal.EnKategoriDaftarAwam);
            PopulateCartAkTerimaTunggalFromDb(akTerimaTunggal);
            return View(akTerimaTunggal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "E")]
        public async Task<IActionResult> Edit(int id, AkTerimaTunggal akTerimaTunggal, string syscode)
        {
            if (id != akTerimaTunggal.Id)
            {
                return NotFound();
            }

            if (akTerimaTunggal.NoRujukan != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = _unitOfWork.AkTerimaTunggalRepo.GetDetailsById(id);
                    var jumlahAsal = objAsal!.Jumlah;
                    akTerimaTunggal.NoRujukan = objAsal.NoRujukan;
                    akTerimaTunggal.EnKategoriDaftarAwam = objAsal.EnKategoriDaftarAwam;
                    akTerimaTunggal.EnJenisTerimaan = objAsal.EnJenisTerimaan;
                    akTerimaTunggal.UserId = objAsal.UserId;
                    akTerimaTunggal.TarMasuk = objAsal.TarMasuk;
                    akTerimaTunggal.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                    if (objAsal.AkTerimaTunggalObjek != null && objAsal.AkTerimaTunggalObjek.Count > 0)
                    {
                        foreach (var item in objAsal.AkTerimaTunggalObjek)
                        {
                            var model = _context.AkTerimaTunggalObjek.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null)
                            {
                                _context.Remove(model);
                            }
                        }
                    }

                    if (objAsal.AkTerimaTunggalInvois != null && objAsal.AkTerimaTunggalInvois.Count > 0)
                    {
                        foreach (var item in objAsal.AkTerimaTunggalInvois)
                        {
                            var model = _context.AkTerimaTunggalInvois.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null)
                            {
                                _context.Remove(model);
                            }
                        }
                    }

                    _context.Entry(objAsal).State = EntityState.Detached;

                    akTerimaTunggal.UserIdKemaskini = user?.UserName ?? "";

                    akTerimaTunggal.TarKemaskini = DateTime.Now;
                    akTerimaTunggal.DPekerjaKemaskiniId = pekerjaId;

                    akTerimaTunggal.AkTerimaTunggalObjek = _cart.AkTerimaTunggalObjek.ToList();

                    akTerimaTunggal.AkTerimaTunggalInvois = _cart.AkTerimaTunggalInvois.ToList();

                    _unitOfWork.AkTerimaTunggalRepo.Update(akTerimaTunggal);

                    if (jumlahAsal != akTerimaTunggal.Jumlah)
                    {
                        _appLog.Insert("Ubah", Convert.ToDecimal(jumlahAsal).ToString("#,##0.00") + " -> " + Convert.ToDecimal(akTerimaTunggal.Jumlah).ToString("#,##0.00") + " : " + akTerimaTunggal.NoRujukan ?? "", akTerimaTunggal.NoRujukan ?? "", id, akTerimaTunggal.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }
                    else
                    {
                        _appLog.Insert("Ubah", akTerimaTunggal.NoRujukan ?? "", akTerimaTunggal.NoRujukan ?? "", id, akTerimaTunggal.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }

                    await _context.SaveChangesAsync();

                    TempData[SD.Success] = "Data berjaya diubah..!";

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AkTerimaTunggalExists(akTerimaTunggal.Id))
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

            ManipulateHiddenDiv(akTerimaTunggal.EnJenisTerimaan, akTerimaTunggal.EnKategoriDaftarAwam);
            PopulateDropDownList(akTerimaTunggal.JKWId);
            PopulateListViewFromCart();
            return View(akTerimaTunggal);
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


                    // get akTerimaTunggal 
                    var akTerimaTunggal = _unitOfWork.AkTerimaTunggalRepo.GetDetailsById((int)id);
                    if (akTerimaTunggal == null)
                    {
                        TempData[SD.Error] = "Data tidak wujud.";
                    }
                    else
                    {

                        if (akTerimaTunggal.NoRujukan != null)
                        {
                            // check is it posted or not
                            if (await _unitOfWork.AkTerimaTunggalRepo.IsPostedAsync((int)id, akTerimaTunggal.NoRujukan) == true)
                            {
                                TempData[SD.Error] = "Data sudah posting.";
                                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                            }

                            // posting start here
                            _unitOfWork.AkTerimaTunggalRepo.PostingToAkAkaun(akTerimaTunggal, user?.UserName ?? "", pekerjaId);

                            //insert applog
                            _appLog.Insert("Posting", "Posting Data", akTerimaTunggal.NoRujukan, (int)id, akTerimaTunggal.Jumlah, pekerjaId, modul, syscode, namamodul, user);

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


                    // get akTerimaTunggal 
                    var akTerimaTunggal = _unitOfWork.AkTerimaTunggalRepo.GetDetailsById((int)id);
                    if (akTerimaTunggal == null)
                    {
                        TempData[SD.Error] = "Data tidak wujud.";
                    }
                    else
                    {

                        if (akTerimaTunggal.NoRujukan != null)
                        {
                            // check is it posted or not
                            if (await _unitOfWork.AkTerimaTunggalRepo.IsPostedAsync((int)id, akTerimaTunggal.NoRujukan) == false)
                            {
                                TempData[SD.Error] = "Data belum diposting.";
                                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                            }

                            // posting start here
                            _unitOfWork.AkTerimaTunggalRepo.RemovePostingFromAkAkaun(akTerimaTunggal, user?.UserName ?? "");

                            //insert applog
                            _appLog.Insert("UnPosting", "UnPosting Data", akTerimaTunggal.NoRujukan, (int)id, akTerimaTunggal.Jumlah, pekerjaId, modul, syscode, namamodul, user);

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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "D")]
        public async Task<IActionResult> DeleteConfirmed(int id, string syscode)
        {
            var akTerimaTunggal = _unitOfWork.AkTerimaTunggalRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akTerimaTunggal != null && await _unitOfWork.AkTerimaTunggalRepo.IsPostedAsync(id, akTerimaTunggal.NoRujukan ?? "") == false)
            {
                akTerimaTunggal.UserIdKemaskini = user?.UserName ?? "";
                akTerimaTunggal.TarKemaskini = DateTime.Now;
                akTerimaTunggal.DPekerjaKemaskiniId = pekerjaId;

                _context.AkTerimaTunggal.Remove(akTerimaTunggal);
                _appLog.Insert("Hapus", akTerimaTunggal.NoRujukan ?? "", akTerimaTunggal.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var obj = await _context.AkTerimaTunggal.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.AkTerimaTunggal.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.NoRujukan ?? "", obj.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
        }

        // functions and services
        private void PopulateFormFields(string? searchString, string? searchDate1, string? searchDate2)
        {
            ViewBag.searchString = searchString;
            ViewBag.searchDate1 = searchDate1 ?? DateTime.Now.ToString("dd/MM/yyyy");
            ViewBag.searchDate2 = searchDate2 ?? DateTime.Now.ToString("dd/MM/yyyy");
        }

        private void PopulateDropDownList(int JKWId)
        {
            ViewBag.JKW = _unitOfWork.JKWRepo.GetAll();
            ViewBag.JKWPTJBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetails();
            ViewBag.JKWPTJBahagianByJKW = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsByJKWId(JKWId);
            ViewBag.AkBank = _unitOfWork.AkBankRepo.GetAllDetails();
            ViewBag.JNegeri = _unitOfWork.JNegeriRepo.GetAll();
            ViewBag.AkCarta = _unitOfWork.AkCartaRepo.GetResultsByParas(EnParas.Paras4);
            ViewBag.JCaraBayar = _unitOfWork.JCaraBayarRepo.GetAll();
            ViewBag.JCawangan = _unitOfWork.JCawanganRepo.GetAll();

            ViewBag.EnJenisCek = EnumHelper<EnJenisCek>.GetList();

            var jenisTerimaan = EnumHelper<EnJenisTerimaan>.GetList();

            ViewBag.EnJenisTerimaan = jenisTerimaan;

            var kategoriDaftarAwam = EnumHelper<EnKategoriDaftarAwam>.GetList();

            ViewBag.EnKategoriDaftarAwam = kategoriDaftarAwam;

            ViewBag.AkInvois = _unitOfWork.AkInvoisRepo.GetAllByStatus(EnStatusBorang.Lulus);

        }

        private void PopulateCartAkTerimaTunggalFromDb(AkTerimaTunggal akTerimaTunggal)
        {
            if (akTerimaTunggal.AkTerimaTunggalObjek != null)
            {
                foreach (var item in akTerimaTunggal.AkTerimaTunggalObjek)
                {
                    _cart.AddItemObjek(
                            akTerimaTunggal.Id,
                            item.JKWPTJBahagianId,
                            item.AkCartaId,
                            item.Amaun);
                }
            }

            if (akTerimaTunggal.AkTerimaTunggalInvois != null)
            {
                foreach (var item in akTerimaTunggal.AkTerimaTunggalInvois)
                {
                    _cart.AddItemInvois(akTerimaTunggal.Id,
                                        item.AkInvoisId,
                                        item.Amaun);
                }
            }

            PopulateListViewFromCart();
        }

        private void PopulateListViewFromCart()
        {
            List<AkTerimaTunggalObjek> objek = _cart.AkTerimaTunggalObjek.ToList();

            foreach (AkTerimaTunggalObjek item in objek)
            {
                var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                item.JKWPTJBahagian = jkwPtjBahagian;

                var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                item.AkCarta = akCarta;
            }

            ViewBag.akTerimaTunggalObjek = objek;

            List<AkTerimaTunggalInvois> invois = _cart.AkTerimaTunggalInvois.ToList();

            foreach (AkTerimaTunggalInvois item in invois)
            {
                var akInvois = _unitOfWork.AkInvoisRepo.GetDetailsById(item.AkInvoisId);

                item.AkInvois = akInvois;

            }
            ViewBag.akTerimaTunggalInvois = invois;

        }

        private bool AkTerimaTunggalExists(int id)
        {
            return _unitOfWork.AkTerimaTunggalRepo.IsExist(b => b.Id == id);
        }
        //

        // jsonResults
        public JsonResult EmptyCart()
        {
            try
            {

                _cart.ClearObjek();
                _cart.ClearInvois();

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

        public JsonResult SaveCartAkTerimaTunggalObjek(AkTerimaTunggalObjek akTerimaTunggalObjek)
        {
            try
            {
                if (akTerimaTunggalObjek != null)
                {
                    _cart.AddItemObjek(akTerimaTunggalObjek.AkTerimaTunggalId, akTerimaTunggalObjek.JKWPTJBahagianId, akTerimaTunggalObjek.AkCartaId, akTerimaTunggalObjek.Amaun);
                }



                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkTerimaTunggalObjek(AkTerimaTunggalObjek akTerimaTunggalObjek)
        {
            try
            {
                if (akTerimaTunggalObjek != null)
                {
                    _cart.RemoveItemObjek(akTerimaTunggalObjek.JKWPTJBahagianId, akTerimaTunggalObjek.AkCartaId);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkTerimaTunggalObjek(AkTerimaTunggalObjek akTerimaTunggalObjek)
        {

            try
            {
                AkTerimaTunggalObjek data = _cart.AkTerimaTunggalObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akTerimaTunggalObjek.JKWPTJBahagianId && x.AkCartaId == akTerimaTunggalObjek.AkCartaId) ?? new AkTerimaTunggalObjek();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkTerimaTunggalObjek(AkTerimaTunggalObjek akTerimaTunggalObjek)
        {

            try
            {

                var akTO = _cart.AkTerimaTunggalObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akTerimaTunggalObjek.JKWPTJBahagianId && x.AkCartaId == akTerimaTunggalObjek.AkCartaId);

                var user = _userManager.GetUserName(User);

                if (akTO != null)
                {
                    _cart.RemoveItemObjek(akTerimaTunggalObjek.JKWPTJBahagianId, akTerimaTunggalObjek.AkCartaId);

                    _cart.AddItemObjek(akTerimaTunggalObjek.AkTerimaTunggalId,
                                    akTerimaTunggalObjek.JKWPTJBahagianId,
                                    akTerimaTunggalObjek.AkCartaId,
                                    akTerimaTunggalObjek.Amaun);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAllItemCartAkTerimaTunggal()
        {

            try
            {
                List<AkTerimaTunggalObjek> objek = _cart.AkTerimaTunggalObjek.ToList();

                foreach (AkTerimaTunggalObjek item in objek)
                {
                    var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    item.JKWPTJBahagian = jkwPtjBahagian;

                    item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                    var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                    item.AkCarta = akCarta;

                }

                List<AkTerimaTunggalInvois> invois = _cart.AkTerimaTunggalInvois.ToList();

                foreach (AkTerimaTunggalInvois item in invois)
                {
                    var AkInvois = _unitOfWork.AkInvoisRepo.GetDetailsById(item.AkInvoisId);

                    item.AkInvois = AkInvois;
                }

                return Json(new { result = "OK", objek = objek.OrderBy(d => d.AkCarta?.Kod), invois = invois.OrderBy(i => i.AkInvois!.NoRujukan) });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetJCaraBayar(int JCaraBayarId)
        {
            try
            {
                var jCaraBayar = _unitOfWork.JCaraBayarRepo.GetById(JCaraBayarId);
                if (jCaraBayar == null)
                {
                    return Json(new { result = "Error", message = "Kod Cara Bayar tidak wujud" });
                }

                return Json(new { result = "OK", jCaraBayar });
            }
            catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }
        }

        public JsonResult GetAkInvois(int AkInvoisId)
        {
            try
            {
                if (AkInvoisId != 0)
                {
                    var data = _unitOfWork.AkInvoisRepo.GetDetailsById(AkInvoisId);

                    if (data != null)
                    {
                        data = _unitOfWork.AkInvoisRepo.GetBalanceAdjustmentFromAkDebitKreditDikeluarkan(data);

                        if (data.Jumlah <= 0)
                        {
                            return Json(new { result = "Error", message = "Amaun kurang dari RM 0.00" });
                        }

                        return Json(new { result = "OK", record = data });
                    }
                    else
                    {
                        return Json(new { result = "Error", message = "data tidak wujud!" });
                    }
                }
                //EmptyCart();
                else
                {
                    return Json(new { result = "None" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }
        }

        public JsonResult SaveCartAkTerimaTunggalInvois(AkTerimaTunggalInvois akTerimaTunggalInvois)
        {
            try
            {
                var data = _unitOfWork.AkInvoisRepo.GetDetailsById(akTerimaTunggalInvois.AkInvoisId);

                if (data != null)
                {
                    if (data.AkInvoisObjek != null)
                    {
                        foreach (var item in data.AkInvoisObjek)
                        {

                            var currentObjek = _cart.AkTerimaTunggalObjek.FirstOrDefault(i => i.JKWPTJBahagianId == item.JKWPTJBahagianId && i.AkCartaId == item.AkCartaId);
                            if (currentObjek != null)
                            {
                                _cart.RemoveItemObjek(item.JKWPTJBahagianId, item.AkCartaId);

                                decimal totalAmaun = currentObjek.Amaun;
                                totalAmaun += akTerimaTunggalInvois.Amaun;

                                _cart.AddItemObjek(
                                    akTerimaTunggalInvois.AkTerimaTunggalId,
                                    item.JKWPTJBahagianId,
                                    item.AkCartaId,
                                    totalAmaun);
                            }
                            else
                            {
                                _cart.AddItemObjek(
                                    akTerimaTunggalInvois.AkTerimaTunggalId,
                                    item.JKWPTJBahagianId,
                                    item.AkCartaId,
                                    akTerimaTunggalInvois.Amaun);
                            }

                        }

                        _cart.AddItemInvois(
                        akTerimaTunggalInvois.AkTerimaTunggalId,
                        data.Id,
                        akTerimaTunggalInvois.Amaun
                        );

                        return Json(new { result = "OK" });
                    }
                    else
                    {
                        return Json(new { result = "ERROR", message = "Objek invois tidak wujud!" });
                    }

                }
                else
                {
                    return Json(new { result = "ERROR", message = "data tidak wujud!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkTerimaTunggalInvois(AkTerimaTunggalInvois akTerimaTunggalInvois)
        {
            try
            {
                var invois = _cart.AkTerimaTunggalInvois.FirstOrDefault(i => i.AkInvoisId == akTerimaTunggalInvois.AkInvoisId);

                if (invois != null)
                {
                    _cart.RemoveItemInvois(invois.AkInvoisId);

                    var data = _unitOfWork.AkInvoisRepo.GetDetailsById(invois.AkInvoisId);

                    if (data != null)
                    {
                        if (data.AkInvoisObjek != null)
                        {
                            // akTerimaTunggalObjek
                            foreach (var item in data.AkInvoisObjek)
                            {

                                var currentObjek = _cart.AkTerimaTunggalObjek.FirstOrDefault(i => i.JKWPTJBahagianId == item.JKWPTJBahagianId && i.AkCartaId == item.AkCartaId);
                                // check if akInvoisObjek same with akTerimaTunggalObjek amount is greater or not
                                if (currentObjek != null && currentObjek.Amaun > akTerimaTunggalInvois.Amaun)
                                {
                                    // if greater, minus amount, add new akTerimaTunggalObjek with new amount
                                    _cart.RemoveItemObjek(item.JKWPTJBahagianId, item.AkCartaId);

                                    decimal totalAmaun = currentObjek.Amaun;
                                    totalAmaun -= akTerimaTunggalInvois.Amaun;

                                    _cart.AddItemObjek(
                                        invois.AkTerimaTunggalId,
                                        item.JKWPTJBahagianId,
                                        item.AkCartaId,
                                        totalAmaun);
                                }
                                else
                                {
                                    // else, remove akTerimaTunggalObjek
                                    _cart.RemoveItemObjek(item.JKWPTJBahagianId, item.AkCartaId);

                                }

                            }
                            //

                            // akTerimaTunggalInvois
                            _cart.RemoveItemInvois(akTerimaTunggalInvois.AkInvoisId);
                            //

                            return Json(new { result = "OK" });
                        }
                        else
                        {
                            return Json(new { result = "ERROR", message = "Objek invois tidak wujud!" });
                        }

                    }
                    else
                    {
                        return Json(new { result = "ERROR", message = "data tidak wujud!" });
                    }
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkTerimaTunggalInvois(AkTerimaTunggalInvois akTerimaTunggalInvois)
        {

            try
            {
                AkTerimaTunggalInvois data = _cart.AkTerimaTunggalInvois.FirstOrDefault(x => x.AkInvoisId == akTerimaTunggalInvois.AkInvoisId) ?? new AkTerimaTunggalInvois();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkTerimaTunggalInvois(AkTerimaTunggalInvois akTerimaTunggalInvois)
        {

            try
            {

                var data = _cart.AkTerimaTunggalInvois.FirstOrDefault(x => x.AkInvoisId == akTerimaTunggalInvois.AkInvoisId);

                if (data != null)
                {

                    var akInvois = _unitOfWork.AkInvoisRepo.GetDetailsById(akTerimaTunggalInvois.AkInvoisId);

                    _cart.RemoveItemInvois(akTerimaTunggalInvois.AkInvoisId);

                    _cart.AddItemInvois(
                    data.AkTerimaTunggalId,
                    data.AkInvoisId,
                    akTerimaTunggalInvois.Amaun
                    );

                    // akTerimaTunggalObjek
                    if (akInvois.AkInvoisObjek != null)
                    {
                        foreach (var item in akInvois.AkInvoisObjek)
                        {

                            var currentObjek = _cart.AkTerimaTunggalObjek.FirstOrDefault(i => i.JKWPTJBahagianId == item.JKWPTJBahagianId && i.AkCartaId == item.AkCartaId);
                            // check if akInvoisObjek same with akTerimaTunggalObjek amount is greater or not
                            if (currentObjek != null && currentObjek.Amaun != akTerimaTunggalInvois.Amaun)
                            {
                                // if greater, minus amount, add new akTerimaTunggalObjek with new amount
                                _cart.RemoveItemObjek(item.JKWPTJBahagianId, item.AkCartaId);

                                decimal totalAmaun = currentObjek.Amaun;
                                totalAmaun = totalAmaun - data.Amaun + akTerimaTunggalInvois.Amaun;

                                _cart.AddItemObjek(
                                    currentObjek.AkTerimaTunggalId,
                                    item.JKWPTJBahagianId,
                                    item.AkCartaId,
                                    totalAmaun);
                            }

                        }
                    }
                    //
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }
    }
}
