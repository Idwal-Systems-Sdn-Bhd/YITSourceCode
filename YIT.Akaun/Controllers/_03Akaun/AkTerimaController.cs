﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services;
using YIT._DataAccess.Services.Cart;
using YIT.Akaun.Infrastructure;
using YIT.Akaun.Microservices;
using YIT.Akaun.Models.ViewModels.Forms;
using System.Drawing;
using YIT._DataAccess.Services.Math;
using YIT.__Domain.Entities.Models._01Jadual;

namespace YIT.Akaun.Controllers._03Akaun
{
    [Authorize(Roles = Init.allExceptAdminRole)]
    public class AkTerimaController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodAkTerima;
        public const string namamodul = Modules.namaAkTerima;
        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;
        private readonly CartAkTerima _cart;

        public AkTerimaController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices,
            CartAkTerima cart
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

            SaveFormFields(searchString,searchDate1,searchDate2 );
            var akTerima = _unitOfWork.AkTerimaRepo.GetResults(searchString, date1, date2, searchColumn);

            return View(akTerima);
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

            var akTerima = _unitOfWork.AkTerimaRepo.GetDetailsById((int)id);
            if (akTerima == null)
            {
                return NotFound();
            }
            EmptyCart();
            ManipulateHiddenDiv(akTerima.EnJenisTerimaan, akTerima.EnKategoriDaftarAwam);
            PopulateCartAkTerimaFromDb(akTerima);
            return View(akTerima);
        }

        [Authorize(Policy = modul + "D")]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akTerima = _unitOfWork.AkTerimaRepo.GetDetailsById((int)id);
            if (akTerima == null)
            {
                return NotFound();
            }
            EmptyCart();
            ManipulateHiddenDiv(akTerima.EnJenisTerimaan, akTerima.EnKategoriDaftarAwam);
            PopulateCartAkTerimaFromDb(akTerima);
            return View(akTerima);
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
            var maxRefNo = _unitOfWork.AkTerimaRepo.GetMaxRefNo(initNoRujukan, tahun);

            var prefix = initNoRujukan + "/" + tahun + "/";
            return RunningNumberFormatter.GenerateRunningNumber(prefix, maxRefNo, "00000");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "C")]
        public async Task<IActionResult> Create(AkTerima akTerima, string syscode)
        {

            if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    akTerima.UserId = user?.UserName ?? "";

                    akTerima.TarMasuk = DateTime.Now;
                    akTerima.DPekerjaMasukId = pekerjaId;

                    akTerima.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.RR.GetDisplayName(), akTerima.Tahun ?? DateTime.Now.ToString("yyyy"));
                    if (akTerima.EnJenisTerimaan == EnJenisTerimaan.Invois) akTerima.EnKategoriDaftarAwam = EnKategoriDaftarAwam.Penghutang;
                    akTerima.AkTerimaObjek = _cart.AkTerimaObjek.ToList();

                akTerima.AkTerimaCaraBayar = _cart.AkTerimaCaraBayar.ToList();
                akTerima.AkTerimaInvois = _cart.AkTerimaInvois.ToList();


                _context.Add(akTerima);
                    _appLog.Insert("Tambah", akTerima.NoRujukan ?? "", akTerima.NoRujukan ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya ditambah..!";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });

                }
            
            PopulateDropDownList(akTerima.JKWId);
            ManipulateHiddenDiv(akTerima.EnJenisTerimaan, akTerima.EnKategoriDaftarAwam);
            PopulateListViewFromCart();
            return View(akTerima);
        }

        [Authorize(Policy = modul + "E")]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akTerima = _unitOfWork.AkTerimaRepo.GetDetailsById((int)id);
            if (akTerima == null)
            {
                return NotFound();
            }

            EmptyCart();
            PopulateDropDownList(akTerima.JKWId);
            ManipulateHiddenDiv(akTerima.EnJenisTerimaan, akTerima.EnKategoriDaftarAwam);
            PopulateCartAkTerimaFromDb(akTerima);
            return View(akTerima);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "E")]
        public async Task<IActionResult> Edit(int id, AkTerima akTerima, string syscode)
        {
            if (id != akTerima.Id)
            {
                return NotFound();
            }

            if (akTerima.NoRujukan != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = _unitOfWork.AkTerimaRepo.GetDetailsById(id);
                    var jumlahAsal = objAsal!.Jumlah;
                    akTerima.NoRujukan = objAsal.NoRujukan;
                    akTerima.EnKategoriDaftarAwam = objAsal.EnKategoriDaftarAwam;
                    akTerima.EnJenisTerimaan = objAsal.EnJenisTerimaan;
                    akTerima.UserId = objAsal.UserId;
                    akTerima.TarMasuk = objAsal.TarMasuk;
                    akTerima.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                    if (objAsal.AkTerimaObjek != null && objAsal.AkTerimaObjek.Count > 0)
                    {
                        foreach (var item in objAsal.AkTerimaObjek)
                        {
                            var model = _context.AkTerimaObjek.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null)
                            {
                                _context.Remove(model);
                            }
                        }
                    }

                    if (objAsal.AkTerimaCaraBayar != null && objAsal.AkTerimaCaraBayar.Count > 0)
                    {
                        foreach (var item in objAsal.AkTerimaCaraBayar)
                        {
                            var model = _context.AkTerimaCaraBayar.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null)
                            {
                                _context.Remove(model);
                            }
                        }
                    }

                    if (objAsal.AkTerimaInvois != null && objAsal.AkTerimaInvois.Count > 0)
                    {
                        foreach (var item in objAsal.AkTerimaInvois)
                        {
                            var model = _context.AkTerimaInvois.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null)
                            {
                                _context.Remove(model);
                            }
                        }
                    }

                    _context.Entry(objAsal).State = EntityState.Detached;

                    akTerima.UserIdKemaskini = user?.UserName ?? "";

                    akTerima.TarKemaskini = DateTime.Now;
                    akTerima.DPekerjaKemaskiniId = pekerjaId;

                    akTerima.AkTerimaObjek = _cart.AkTerimaObjek.ToList();

                    akTerima.AkTerimaCaraBayar = _cart.AkTerimaCaraBayar.ToList();

                    akTerima.AkTerimaInvois = _cart.AkTerimaInvois.ToList();

                    _unitOfWork.AkTerimaRepo.Update(akTerima);

                    if (jumlahAsal != akTerima.Jumlah)
                    {
                        _appLog.Insert("Ubah", Convert.ToDecimal(jumlahAsal).ToString("#,##0.00") + " -> " + Convert.ToDecimal(akTerima.Jumlah).ToString("#,##0.00") + " : " + akTerima.NoRujukan ?? "", akTerima.NoRujukan ?? "", id, akTerima.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }
                    else
                    {
                        _appLog.Insert("Ubah",akTerima.NoRujukan ?? "", akTerima.NoRujukan ?? "", id, akTerima.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }

                    await _context.SaveChangesAsync();

                    TempData[SD.Success] = "Data berjaya diubah..!";
                    
                }
                catch(DbUpdateConcurrencyException)
                {
                    if (!AkTerimaExists(akTerima.Id))
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

            ManipulateHiddenDiv(akTerima.EnJenisTerimaan, akTerima.EnKategoriDaftarAwam);
            PopulateDropDownList(akTerima.JKWId);
            PopulateListViewFromCart();
            return View(akTerima);
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


                    // get akTerima 
                    var akTerima = _unitOfWork.AkTerimaRepo.GetDetailsById((int)id);
                    if (akTerima == null)
                    {
                        TempData[SD.Error] = "Data tidak wujud.";
                    }
                    else
                    {
                        
                        if (akTerima.NoRujukan != null)
                        {
                            // check is it posted or not
                            if (await _unitOfWork.AkTerimaRepo.IsPostedAsync((int)id, akTerima.NoRujukan) == true)
                            {
                                TempData[SD.Error] = "Data sudah posting.";
                                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                            }

                            // posting start here
                            _unitOfWork.AkTerimaRepo.PostingToAkAkaun(akTerima,user?.UserName ?? "",pekerjaId);

                            //insert applog
                            _appLog.Insert("Posting", "Posting Data", akTerima.NoRujukan, (int)id, akTerima.Jumlah, pekerjaId, modul, syscode, namamodul, user);

                            //insert applog end

                            await _context.SaveChangesAsync();

                            TempData[SD.Success] = "Posting data berjaya";
                        } 
                        else
                        {
                            TempData[SD.Error] = "No Rujukan Tiada";
                        }
                        


                    }
                    
                } catch (Exception)
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


                    // get akTerima 
                    var akTerima = _unitOfWork.AkTerimaRepo.GetDetailsById((int)id);
                    if (akTerima == null)
                    {
                        TempData[SD.Error] = "Data tidak wujud.";
                    }
                    else
                    {

                        if (akTerima.NoRujukan != null)
                        {
                            // check is it posted or not
                            if (await _unitOfWork.AkTerimaRepo.IsPostedAsync((int)id, akTerima.NoRujukan) == false)
                            {
                                TempData[SD.Error] = "Data belum diposting.";
                                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                            }

                            // posting start here
                            _unitOfWork.AkTerimaRepo.RemovePostingFromAkAkaun(akTerima, user?.UserName ?? "");

                            //insert applog
                            _appLog.Insert("UnPosting", "UnPosting Data", akTerima.NoRujukan, (int)id, akTerima.Jumlah, pekerjaId, modul, syscode, namamodul, user);

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
            var akTerima = _unitOfWork.AkTerimaRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akTerima != null && await _unitOfWork.AkTerimaRepo.IsPostedAsync(id,akTerima.NoRujukan ?? "") == false)
            {
                akTerima.UserIdKemaskini = user?.UserName ?? "";
                akTerima.TarKemaskini = DateTime.Now;
                akTerima.DPekerjaKemaskiniId = pekerjaId;

                _context.AkTerima.Remove(akTerima);
                _appLog.Insert("Hapus", akTerima.NoRujukan ?? "", akTerima.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var obj = await _context.AkTerima.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.AkTerima.Update(obj);

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

        private void PopulateCartAkTerimaFromDb(AkTerima akTerima)
        {
            if (akTerima.AkTerimaObjek != null)
            {
                foreach (var item in akTerima.AkTerimaObjek)
                {
                    _cart.AddItemObjek(
                            akTerima.Id,
                            item.JKWPTJBahagianId,
                            item.AkCartaId,
                            item.Amaun);
                }
            }

            if (akTerima.AkTerimaCaraBayar != null)
            {
                foreach (var item in akTerima.AkTerimaCaraBayar)
                {
                    _cart.AddItemCaraBayar(
                        akTerima.Id,
                        item.JCaraBayarId,
                        item.Amaun,
                        item.NoCekMK,
                        item.EnJenisCek,
                        item.KodBankCek,
                        item.TempatCek,
                        item.NoSlip,
                        item.TarikhSlip
                        );
                }
            }

            if (akTerima.AkTerimaInvois != null)
            {
                foreach (var item in akTerima.AkTerimaInvois)
                {
                    _cart.AddItemInvois(akTerima.Id,
                                        item.AkInvoisId,
                                        item.Amaun);
                }
            }

            PopulateListViewFromCart();
        }

        private void PopulateListViewFromCart()
        {
            List<AkTerimaObjek> objek = _cart.AkTerimaObjek.ToList();

            foreach (AkTerimaObjek item in objek)
            {
                var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                item.JKWPTJBahagian = jkwPtjBahagian;

                var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                item.AkCarta = akCarta;
            }

            ViewBag.akTerimaObjek = objek;

            List<AkTerimaCaraBayar> caraBayar = _cart.AkTerimaCaraBayar.ToList();

            foreach (AkTerimaCaraBayar item in caraBayar)
            {
                var jCaraBayar = _unitOfWork.JCaraBayarRepo.GetById(item.JCaraBayarId);

                item.JCaraBayar = jCaraBayar;
            }

            ViewBag.akTerimaCaraBayar = caraBayar;

            List<AkTerimaInvois> invois = _cart.AkTerimaInvois.ToList();

            foreach (AkTerimaInvois item in invois)
            {
                var akInvois = _unitOfWork.AkInvoisRepo.GetDetailsById(item.AkInvoisId);

                item.AkInvois = akInvois;

            }
            ViewBag.akTerimaInvois = invois;

        }

        private bool AkTerimaExists(int id)
        {
            return _unitOfWork.AkTerimaRepo.IsExist(b => b.Id == id);
        }
        //

        // jsonResults
        public JsonResult EmptyCart()
        {
            try
            {
                
                _cart.ClearObjek();
                _cart.ClearCaraBayar();
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

        public JsonResult SaveCartAkTerimaObjek(AkTerimaObjek akTerimaObjek)
        {
            try
            {
                if (akTerimaObjek != null)
                {
                    _cart.AddItemObjek(akTerimaObjek.AkTerimaId,akTerimaObjek.JKWPTJBahagianId, akTerimaObjek.AkCartaId, akTerimaObjek.Amaun);
                }



                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkTerimaObjek(AkTerimaObjek akTerimaObjek)
        {
            try
            {
                if (akTerimaObjek != null)
                {
                    _cart.RemoveItemObjek(akTerimaObjek.JKWPTJBahagianId, akTerimaObjek.AkCartaId);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkTerimaObjek(AkTerimaObjek akTerimaObjek)
        {

            try
            {
                AkTerimaObjek data = _cart.AkTerimaObjek.FirstOrDefault(x =>x.JKWPTJBahagianId == akTerimaObjek.JKWPTJBahagianId && x.AkCartaId == akTerimaObjek.AkCartaId) ?? new AkTerimaObjek();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkTerimaObjek(AkTerimaObjek akTerimaObjek)
        {

            try
            {

                var akTO = _cart.AkTerimaObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akTerimaObjek.JKWPTJBahagianId && x.AkCartaId == akTerimaObjek.AkCartaId);

                var user = _userManager.GetUserName(User);

                if (akTO != null)
                {
                    _cart.RemoveItemObjek(akTerimaObjek.JKWPTJBahagianId, akTerimaObjek.AkCartaId);

                    _cart.AddItemObjek(akTerimaObjek.AkTerimaId,
                                    akTerimaObjek.JKWPTJBahagianId,
                                    akTerimaObjek.AkCartaId,
                                    akTerimaObjek.Amaun);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAllItemCartAkTerima()
        {

            try
            {
                List<AkTerimaObjek> objek = _cart.AkTerimaObjek.ToList();

                foreach (AkTerimaObjek item in objek)
                {
                    var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    item.JKWPTJBahagian = jkwPtjBahagian;

                    item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                    var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                    item.AkCarta = akCarta;

                }

                List<AkTerimaCaraBayar> caraBayar = _cart.AkTerimaCaraBayar.ToList();

                foreach (AkTerimaCaraBayar item in caraBayar)
                {
                    var jCaraBayar = _unitOfWork.JCaraBayarRepo.GetById(item.JCaraBayarId);

                    item.JCaraBayar = jCaraBayar;

                }

                List<AkTerimaInvois> invois = _cart.AkTerimaInvois.ToList();

                foreach (AkTerimaInvois item in invois)
                {
                    var AkInvois = _unitOfWork.AkInvoisRepo.GetDetailsById(item.AkInvoisId);

                    item.AkInvois = AkInvois;
                }

                return Json(new { result = "OK", objek = objek.OrderBy(d => d.AkCarta?.Kod), invois = invois.OrderBy(i => i.AkInvois!.NoRujukan), caraBayar = caraBayar.OrderBy(d => d.JCaraBayar?.Kod) });
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

                return Json(new { result = "OK", jCaraBayar});
            }
            catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }
        }

        public JsonResult SaveCartAkTerimaCaraBayar(AkTerimaCaraBayar akTerimaCaraBayar)
        {
            try
            {
                if (akTerimaCaraBayar != null)
                {
                    _cart.AddItemCaraBayar(
                        akTerimaCaraBayar.AkTerimaId,
                        akTerimaCaraBayar.JCaraBayarId, 
                        akTerimaCaraBayar.Amaun,
                        akTerimaCaraBayar.NoCekMK ?? "",
                        akTerimaCaraBayar.EnJenisCek,
                        akTerimaCaraBayar.KodBankCek ?? "",
                        akTerimaCaraBayar.TempatCek ?? "",
                        akTerimaCaraBayar.NoSlip ?? "",
                        akTerimaCaraBayar.TarikhSlip
                        );
                }

                var enJenisCek = akTerimaCaraBayar?.EnJenisCek.GetDisplayName();

                return Json(new { result = "OK"  , enJenisCek});
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkTerimaCaraBayar(AkTerimaCaraBayar akTerimaCaraBayar)
        {
            try
            {
                if (akTerimaCaraBayar != null)
                {
                    _cart.RemoveItemCaraBayar(akTerimaCaraBayar.JCaraBayarId);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkTerimaCaraBayar(AkTerimaCaraBayar akTerimaCaraBayar)
        {

            try
            {
                AkTerimaCaraBayar data = _cart.AkTerimaCaraBayar.FirstOrDefault(x => x.JCaraBayarId == akTerimaCaraBayar.JCaraBayarId) ?? new AkTerimaCaraBayar();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkTerimaCaraBayar(AkTerimaCaraBayar akTerimaCaraBayar)
        {

            try
            {

                var akCB = _cart.AkTerimaCaraBayar.FirstOrDefault(x => x.JCaraBayarId == akTerimaCaraBayar.JCaraBayarId);

                var user = _userManager.GetUserName(User);

                if (akCB != null)
                {
                    _cart.RemoveItemCaraBayar(akTerimaCaraBayar.JCaraBayarId);

                    _cart.AddItemCaraBayar(
                        akTerimaCaraBayar.AkTerimaId,
                        akTerimaCaraBayar.JCaraBayarId,
                        akTerimaCaraBayar.Amaun,
                        akTerimaCaraBayar.NoCekMK ?? "",
                        akTerimaCaraBayar.EnJenisCek,
                        akTerimaCaraBayar.KodBankCek ?? "",
                        akTerimaCaraBayar.TempatCek ?? "",
                        akTerimaCaraBayar.NoSlip ?? "",
                        akTerimaCaraBayar.TarikhSlip);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }
        //
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

        public JsonResult SaveCartAkTerimaInvois(AkTerimaInvois akTerimaInvois)
        {
            try
            {
                var data = _unitOfWork.AkInvoisRepo.GetDetailsById(akTerimaInvois.AkInvoisId);

                if (data != null)
                {
                    if (data.AkInvoisObjek != null)
                    {
                        foreach (var item in data.AkInvoisObjek)
                        {

                            var currentObjek = _cart.AkTerimaObjek.FirstOrDefault(i => i.JKWPTJBahagianId == item.JKWPTJBahagianId && i.AkCartaId == item.AkCartaId);
                            if (currentObjek != null)
                            {
                                _cart.RemoveItemObjek(item.JKWPTJBahagianId, item.AkCartaId);

                                decimal totalAmaun = currentObjek.Amaun;
                                totalAmaun += akTerimaInvois.Amaun;

                                _cart.AddItemObjek(
                                    akTerimaInvois.AkTerimaId,
                                    item.JKWPTJBahagianId,
                                    item.AkCartaId,
                                    totalAmaun);
                            }
                            else
                            {
                                _cart.AddItemObjek(
                                    akTerimaInvois.AkTerimaId,
                                    item.JKWPTJBahagianId,
                                    item.AkCartaId,
                                    akTerimaInvois.Amaun);
                            }

                        }

                        _cart.AddItemInvois(
                        akTerimaInvois.AkTerimaId,
                        data.Id,
                        akTerimaInvois.Amaun
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

        public JsonResult RemoveCartAkTerimaInvois(AkTerimaInvois akTerimaInvois)
        {
            try
            {
                var invois = _cart.AkTerimaInvois.FirstOrDefault(i => i.AkInvoisId == akTerimaInvois.AkInvoisId);

                if (invois != null)
                {
                    _cart.RemoveItemInvois(invois.AkInvoisId);

                    var data = _unitOfWork.AkInvoisRepo.GetDetailsById(invois.AkInvoisId);

                    if (data != null)
                    {
                        if (data.AkInvoisObjek != null)
                        {
                            // akTerimaObjek
                            foreach (var item in data.AkInvoisObjek)
                            {

                                var currentObjek = _cart.AkTerimaObjek.FirstOrDefault(i => i.JKWPTJBahagianId == item.JKWPTJBahagianId && i.AkCartaId == item.AkCartaId);
                                // check if akInvoisObjek same with akTerimaObjek amount is greater or not
                                if (currentObjek != null && currentObjek.Amaun > akTerimaInvois.Amaun)
                                {
                                    // if greater, minus amount, add new akTerimaObjek with new amount
                                    _cart.RemoveItemObjek(item.JKWPTJBahagianId, item.AkCartaId);

                                    decimal totalAmaun = currentObjek.Amaun;
                                    totalAmaun -= akTerimaInvois.Amaun;

                                    _cart.AddItemObjek(
                                        invois.AkTerimaId,
                                        item.JKWPTJBahagianId,
                                        item.AkCartaId,
                                        totalAmaun);
                                }
                                else
                                {
                                    // else, remove akTerimaObjek
                                    _cart.RemoveItemObjek(item.JKWPTJBahagianId, item.AkCartaId);

                                }

                            }
                            //

                            // akTerimaInvois
                            _cart.RemoveItemInvois(akTerimaInvois.AkInvoisId);
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

        public JsonResult GetAnItemFromCartAkTerimaInvois(AkTerimaInvois akTerimaInvois)
        {

            try
            {
                AkTerimaInvois data = _cart.AkTerimaInvois.FirstOrDefault(x => x.AkInvoisId == akTerimaInvois.AkInvoisId) ?? new AkTerimaInvois();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkTerimaInvois(AkTerimaInvois akTerimaInvois)
        {

            try
            {

                var data = _cart.AkTerimaInvois.FirstOrDefault(x => x.AkInvoisId == akTerimaInvois.AkInvoisId);

                if (data != null)
                {

                    var akInvois = _unitOfWork.AkInvoisRepo.GetDetailsById(akTerimaInvois.AkInvoisId);

                    _cart.RemoveItemInvois(akTerimaInvois.AkInvoisId);

                    _cart.AddItemInvois(
                    data.AkTerimaId,
                    data.AkInvoisId,
                    akTerimaInvois.Amaun
                    );

                    // akTerimaObjek
                    if (akInvois.AkInvoisObjek != null)
                    {
                        foreach (var item in akInvois.AkInvoisObjek)
                        {

                            var currentObjek = _cart.AkTerimaObjek.FirstOrDefault(i => i.JKWPTJBahagianId == item.JKWPTJBahagianId && i.AkCartaId == item.AkCartaId);
                            // check if akInvoisObjek same with akTerimaObjek amount is greater or not
                            if (currentObjek != null && currentObjek.Amaun != akTerimaInvois.Amaun)
                            {
                                // if greater, minus amount, add new akTerimaObjek with new amount
                                _cart.RemoveItemObjek(item.JKWPTJBahagianId, item.AkCartaId);

                                decimal totalAmaun = currentObjek.Amaun;
                                totalAmaun = totalAmaun - data.Amaun + akTerimaInvois.Amaun;

                                _cart.AddItemObjek(
                                    currentObjek.AkTerimaId,
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
