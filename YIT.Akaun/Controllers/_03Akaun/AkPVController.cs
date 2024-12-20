﻿using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
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
using YIT.Akaun.Views.AkCarta;

namespace YIT.Akaun.Controllers._03Akaun
{
    [Authorize(Roles = Init.allExceptAdminRole)]
    public class AkPVController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodAkPV;
        public const string namamodul = Modules.namaAkPV;

        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;
        private readonly CartAkPV _cart;

        public AkPVController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices,
            CartAkPV cart)
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

            var akPV = _unitOfWork.AkPVRepo.GetResults(searchString, date1, date2, searchColumn, EnStatusBorang.Semua, null);

            return View(akPV);
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

            var akPV = _unitOfWork.AkPVRepo.GetDetailsById((int)id);
            if (akPV == null)
            {
                return NotFound();
            }
            EmptyCart();
            ManipulateHiddenDiv(akPV.EnJenisBayaran);
            PopulateCartAkPVFromDb(akPV);
            return View(akPV);
        }

        [Authorize(Policy = modul + "D")]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akPV = _unitOfWork.AkPVRepo.GetDetailsById((int)id);
            if (akPV == null)
            {
                return NotFound();
            }

            if (akPV.EnStatusBorang != EnStatusBorang.None)
            {
                TempData[SD.Error] = "Hapus data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            ManipulateHiddenDiv(akPV.EnJenisBayaran);
            PopulateCartAkPVFromDb(akPV);
            return View(akPV);
        }

        [Authorize(Policy = modul + "BL")]
        public IActionResult BatalLulus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akPV = _unitOfWork.AkPVRepo.GetDetailsById((int)id);
            if (akPV == null)
            {
                return NotFound();
            }

            if (akPV.EnStatusBorang != EnStatusBorang.Lulus)
            {
                TempData[SD.Error] = "Data belum diluluskan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkPVFromDb(akPV);
            return View(akPV);
        }

        [HttpPost, ActionName("BatalLulus")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "BL")]
        public async Task<IActionResult> BatalLulusConfirmed(int id, string tindakan, string syscode)
        {
            var akPV = _unitOfWork.AkPVRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akPV != null && !string.IsNullOrEmpty(akPV.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkPVRepo.IsPostedAsync((int)id, akPV.NoRujukan) == false)
                {
                    TempData[SD.Error] = "Data belum diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkPVRepo.IsLulusAsync(id) == false)
                {
                    TempData[SD.Error] = "Data belum diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkPVRepo.BatalLulus(id, tindakan, user?.Email);

                _appLog.Insert("UnPosting", "Batal Lulus " + akPV.NoRujukan ?? "", akPV.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var akPV = _unitOfWork.AkPVRepo.GetDetailsById((int)id);
            if (akPV == null)
            {
                return NotFound();
            }

            if (akPV.EnStatusBorang != EnStatusBorang.Lulus)
            {
                TempData[SD.Error] = "Data belum diluluskan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkPVFromDb(akPV);
            return View(akPV);
        }

        [HttpPost, ActionName("BatalPos")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "BL")]
        public async Task<IActionResult> BatalPosConfirmed(int id, string tindakan, string syscode)
        {
            var akPV = _unitOfWork.AkPVRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akPV != null && !string.IsNullOrEmpty(akPV.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkPVRepo.IsPostedAsync((int)id, akPV.NoRujukan) == false)
                {
                    TempData[SD.Error] = "Data belum diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkPVRepo.IsLulusAsync(id) == false)
                {
                    TempData[SD.Error] = "Data belum diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkPVRepo.BatalPos(id, tindakan, user?.UserName);

                _appLog.Insert("UnPosting", "Batal Pos " + akPV.NoRujukan ?? "", akPV.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var obj = await _context.AkPV.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Pos operation

            if (obj != null && !string.IsNullOrEmpty(obj.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkPVRepo.IsPostedAsync((int)id, obj.NoRujukan))
                {
                    TempData[SD.Error] = "Data sudah diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkPVRepo.IsLulusAsync(id))
                {
                    TempData[SD.Error] = "Data telah diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkPVRepo.Lulus(id, pekerjaId, user?.UserName);

                // Batal operation end
                _appLog.Insert("Posting", obj.NoRujukan ?? "", obj.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya pos semula..!";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
        }

        [Authorize(Policy = modul + "C")]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);

            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.PV.GetDisplayName(), DateTime.Now.ToString("yyyy"));
            ManipulateHiddenDiv(EnJenisBayaran.Am);
            EmptyCart();
            PopulateDropDownList(1);
            return View();
        }

        private string GenerateRunningNumber(string initNoRujukan, string tahun)
        {
            var maxRefNo = _unitOfWork.AkPVRepo.GetMaxRefNo(initNoRujukan, tahun);

            var prefix = initNoRujukan + "/" + tahun + "/";
            return RunningNumberFormatter.GenerateRunningNumber(prefix, maxRefNo, "00000");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "C")]
        public async Task<IActionResult> Create(AkPV akPV, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            // check if there is pelulus available or not based on modul, kelulusan, and bahagian
            if (_cart.AkPVObjek != null && _cart.AkPVObjek.Count() > 0)
            {
                foreach (var item in _cart.AkPVObjek)
                {
                    var jKWPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModulKelulusan.PV, EnKategoriKelulusan.Pelulus, jKWPtjBahagian.JBahagianId, akPV.Jumlah) == false)
                    {
                        TempData[SD.Error] = "Tiada Pelulus yang wujud untuk senarai kod bahagian berikut.";
                        ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.PV.GetDisplayName(), akPV.Tahun ?? DateTime.Now.ToString("yyyy"));
                        PopulateDropDownList(akPV.JKWId);
                        PopulateListViewFromCart();
                        ManipulateHiddenDiv(akPV.EnJenisBayaran);
                        
                        return View(akPV);
                    }
                }
            }
            //

            // double check if every penerima have jCaraBayarId or not
            if (_cart.AkPVPenerima != null && _cart.AkPVPenerima.Count() > 0)
            {

                if (_cart.AkPVPenerima.Count() > 1) akPV.IsGanda = true;

                foreach (var item in _cart.AkPVPenerima)
                {
                    if (item.JCaraBayarId == 0)
                    {
                        TempData[SD.Error] = "Terdapat penerima yang tiada pilihan cara bayar.";
                        ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.PV.GetDisplayName(), akPV.Tahun ?? DateTime.Now.ToString("yyyy"));
                        PopulateDropDownList(akPV.JKWId);
                        PopulateListViewFromCart();
                        ManipulateHiddenDiv(akPV.EnJenisBayaran);
                        return View(akPV);
                    }
                }
            }
            // 

            // akPVInvois condition
            if (_cart.AkPVInvois != null && _cart.AkPVInvois.Count() > 0)
            {
                akPV.IsInvois = true;
                

                foreach (var item in _cart.AkPVInvois)
                {
                    var akBelian = _unitOfWork.AkBelianRepo.GetDetailsById(item.AkBelianId);
                    if (akBelian != null)
                    {
                        if (item.IsTanggungan == true)
                        {

                            akPV.IsTanggungan = true;
                        }

                        if (akBelian.AkAkaunAkruId != null)
                        {
                            akPV.IsAkru = true;
                        }
                    }
                    
                    
                }
            }
            //

            if (ModelState.IsValid)
            {

                akPV.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.PV.GetDisplayName(), akPV.Tahun ?? DateTime.Now.ToString("yyyy"));

                akPV.UserId = user?.UserName ?? "";
                akPV.TarMasuk = DateTime.Now;
                akPV.DPekerjaMasukId = pekerjaId;

                akPV.AkPVObjek = _cart.AkPVObjek?.ToList();
                akPV.AkPVInvois = _cart.AkPVInvois?.ToList();
                akPV.AkPVPenerima = _cart.AkPVPenerima?.ToList();

                // change akRekup isLinked
                if (akPV.AkPVPenerima != null && akPV.AkPVPenerima.Any())
                {
                    foreach (var item in akPV.AkPVPenerima)
                    {
                        if (item.AkRekupId != null)
                        {
                            var rekup = _unitOfWork.AkRekupRepo.GetById((int)item.AkRekupId);

                            if (rekup != null)
                            {
                                rekup.IsLinked = true;

                                _context.AkRekup.Update(rekup);
                            }
                        }
                    }
                }
                //

                _context.Add(akPV);
                _appLog.Insert("Tambah", akPV.NoRujukan ?? "", akPV.NoRujukan ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya ditambah..!";
                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
            }
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.PV.GetDisplayName(), akPV.Tahun ?? DateTime.Now.ToString("yyyy"));
            PopulateDropDownList(akPV.JKWId);
            PopulateListViewFromCart();
            ManipulateHiddenDiv(akPV.EnJenisBayaran);
            return View(akPV);
        }

        private void ManipulateHiddenDiv(EnJenisBayaran enJenisBayaran)
        {
            switch (enJenisBayaran)
            {
                case EnJenisBayaran.Invois:
                    ViewBag.DivInvois = "";
                    ViewBag.DivJanaanProfil = "hidden";
                    break;
                case EnJenisBayaran.JanaanProfil:
                    ViewBag.DivInvois = "hidden";
                    ViewBag.DivJanaanProfil = "";
                    break;
                default:
                    ViewBag.DivInvois = "hidden";
                    ViewBag.DivJanaanProfil = "hidden";
                    break;
            }
        }

        [Authorize(Policy = modul + "E")]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akPV = _unitOfWork.AkPVRepo.GetDetailsById((int)id);
            if (akPV == null)
            {
                return NotFound();
            }

            if (akPV.EnStatusBorang != EnStatusBorang.None && akPV.EnStatusBorang != EnStatusBorang.Kemaskini)
            {
                TempData[SD.Error] = "Ubah data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }

            EmptyCart();
            ManipulateHiddenDiv(akPV.EnJenisBayaran);
            PopulateDropDownList(akPV.JKWId);
            PopulateCartAkPVFromDb(akPV);
            return View(akPV);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "E")]
        public async Task<IActionResult> Edit(int id, AkPV akPV, string? fullName, string syscode)
        {
            if (id != akPV.Id)
            {
                return NotFound();
            }

            // check if there is pelulus available or not based on modul, kelulusan, and bahagian
            if (_cart.AkPVObjek != null && _cart.AkPVObjek.Count() > 0)
            {
                foreach (var item in _cart.AkPVObjek)
                {
                    var jKWPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModulKelulusan.PV, EnKategoriKelulusan.Pelulus, jKWPtjBahagian.JBahagianId, akPV.Jumlah) == false)
                    {
                        TempData[SD.Error] = "Tiada Pelulus yang wujud untuk senarai kod bahagian berikut.";
                        PopulateDropDownList(akPV.JKWId);
                        PopulateListViewFromCart();
                        ManipulateHiddenDiv(akPV.EnJenisBayaran);
                        return View(akPV);
                    }
                }
            }
            //

            // double check if every penerima have jCaraBayarId or not
            decimal jumlahPenerima = 0;

            if (_cart.AkPVPenerima != null && _cart.AkPVPenerima.Count() > 0)
            {

                if (_cart.AkPVPenerima.Count() > 1) akPV.IsGanda = true;
                
                foreach (var item in _cart.AkPVPenerima)
                {
                    if (item.JCaraBayarId == 0)
                    {
                        TempData[SD.Error] = "Terdapat penerima yang tiada pilihan cara bayar.";
                        PopulateDropDownList(akPV.JKWId);
                        PopulateListViewFromCart();
                        ManipulateHiddenDiv(akPV.EnJenisBayaran);
                        return View(akPV);
                    }
                    jumlahPenerima += item.Amaun;

                }
            }
            // 

            // check if Jumlah equal to Jumlah Penerima RM
            if (akPV.Jumlah != jumlahPenerima)
            {
                TempData[SD.Error] = "Jumlah RM tidak sama dengan Jumlah Penerima RM.";
                PopulateDropDownList(akPV.JKWId);
                PopulateListViewFromCart();
                ManipulateHiddenDiv(akPV.EnJenisBayaran);
                return View(akPV);
            }
            //

            if (akPV.NoRujukan != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = _unitOfWork.AkPVRepo.GetDetailsById(id);
                    var jumlahAsal = objAsal!.Jumlah;
                    akPV.NoRujukan = objAsal.NoRujukan;
                    akPV.JKWId = objAsal.JKWId;
                    akPV.UserId = objAsal.UserId;
                    akPV.TarMasuk = objAsal.TarMasuk;
                    akPV.DPekerjaMasukId = objAsal.DPekerjaMasukId;
                    akPV.EnStatusBorang = objAsal.EnStatusBorang;

                    if (objAsal.AkPVObjek != null && objAsal.AkPVObjek.Count > 0)
                    {
                        foreach (var item in objAsal.AkPVObjek)
                        {
                            var model = _context.AkPVObjek.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null) _context.Remove(model);
                        }
                    }

                    if (objAsal.AkPVInvois != null && objAsal.AkPVInvois.Count > 0)
                    {
                        foreach (var item in objAsal.AkPVInvois)
                        {
                            var model = _context.AkPVInvois.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null) _context.Remove(model);
                        }
                    }

                    if (objAsal.AkPVPenerima != null && objAsal.AkPVPenerima.Count > 0)
                    {
                        foreach (var item in objAsal.AkPVPenerima)
                        {
                            var model = _context.AkPVPenerima.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null)
                            {
                                if (model.AkRekupId != null)
                                {
                                    var rekup = _unitOfWork.AkRekupRepo.GetById((int)model.AkRekupId);

                                    if (rekup != null)
                                    {
                                        rekup.IsLinked = true;

                                        _context.AkRekup.Update(rekup);
                                    }
                                }
                                _context.Remove(model);
                            }
                        }
                    }

                    _context.Entry(objAsal).State = EntityState.Detached;

                    akPV.UserIdKemaskini = user?.UserName ?? "";
                    akPV.TarKemaskini = DateTime.Now;
                    akPV.AkPVObjek = _cart.AkPVObjek?.ToList();
                    akPV.AkPVInvois = _cart.AkPVInvois.ToList();
                    akPV.AkPVPenerima = _cart.AkPVPenerima?.ToList();

                    // change akRekup isLinked
                    if (akPV.AkPVPenerima != null && akPV.AkPVPenerima.Any())
                    {
                        foreach (var item in akPV.AkPVPenerima)
                        {
                            if (item.AkRekupId != null)
                            {
                                var rekup = _unitOfWork.AkRekupRepo.GetById((int)item.AkRekupId);

                                if (rekup != null)
                                {
                                    rekup.IsLinked = true;

                                    _context.AkRekup.Update(rekup);
                                }
                            }
                        }
                    }
                    //

                    _unitOfWork.AkPVRepo.Update(akPV);

                    if (jumlahAsal != akPV.Jumlah)
                    {
                        _appLog.Insert("Ubah", Convert.ToDecimal(jumlahAsal).ToString("#,##0.00") + " -> " + Convert.ToDecimal(akPV.Jumlah).ToString("#,##0.00") + " : " + akPV.NoRujukan ?? "", akPV.NoRujukan ?? "", id, akPV.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }
                    else
                    {
                        _appLog.Insert("Ubah", akPV.NoRujukan ?? "", akPV.NoRujukan ?? "", id, akPV.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }

                    await _context.SaveChangesAsync();

                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AkPVExist(akPV.Id))
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

            PopulateDropDownList(akPV.JKWId);
            PopulateListViewFromCart();
            ManipulateHiddenDiv(akPV.EnJenisBayaran);
            return View(akPV);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "D")]
        public async Task<IActionResult> DeleteConfirmed(int id, string sebabHapus, string syscode)
        {
            var akPV = _unitOfWork.AkPVRepo.GetById(id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akPV != null && await _unitOfWork.AkPVRepo.IsSahAsync(id) == false)
            {
                akPV.UserIdKemaskini = user?.UserName ?? "";
                akPV.TarKemaskini = DateTime.Now;
                akPV.DPekerjaKemaskiniId = pekerjaId;
                akPV.SebabHapus = sebabHapus;

                _context.AkPV.Remove(akPV);
                _appLog.Insert("Hapus", akPV.NoRujukan ?? "", akPV.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();

                // change akRekup isLinked
                var pvPenerima = await _context.AkPVPenerima.Where(pvp => pvp.AkPVId == id).ToListAsync();

                if (pvPenerima != null && pvPenerima.Any())
                {
                    foreach (var item in pvPenerima)
                    {
                        if (item.AkRekupId != null)
                        {
                            var rekup = _unitOfWork.AkRekupRepo.GetById((int)item.AkRekupId);

                            if (rekup != null)
                            {
                                rekup.IsLinked = false;

                                _context.AkRekup.Update(rekup);
                            }
                        }
                    }
                }
                //

                await _context.SaveChangesAsync();

                TempData[SD.Success] = "Data berjaya dihapuskan..!";
            }
            else
            {
                TempData[SD.Error] = "Data telah diluluskan";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
        }

        [Authorize(Policy = modul + "R")]
        public async Task<IActionResult> RollBack(int id, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            var obj = await _context.AkPV.IgnoreQueryFilters()
                .Include(pv => pv.AkPVPenerima)
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                // change akRekup isLinked
                if (obj.AkPVPenerima != null && obj.AkPVPenerima.Any())
                {
                    foreach (var item in obj.AkPVPenerima)
                    {
                        if (item.AkRekupId != null)
                        {
                            var rekup = _unitOfWork.AkRekupRepo.GetById((int)item.AkRekupId);

                            if (rekup != null)
                            {
                                rekup.IsLinked = true;

                                _context.AkRekup.Update(rekup);
                            }
                        }
                    }
                }
                //

                _context.AkPV.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.NoRujukan ?? "", obj.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
        }

        private bool AkPVExist(int id)
        {
            return _unitOfWork.AkPVRepo.IsExist(b => b.Id == id);
        }

        private void PopulateDropDownList(int JKWId)
        {
            ViewBag.JKW = _unitOfWork.JKWRepo.GetAll();
            ViewBag.JCukai = _unitOfWork.JCukaiRepo.GetAll();
            ViewBag.AkJanaanProfil = _unitOfWork.AkJanaanProfilRepo.GetAll();
            ViewBag.JCaraBayar = _unitOfWork.JCaraBayarRepo.GetAll();
            ViewBag.JBank = _unitOfWork.JBankRepo.GetAll();
            ViewBag.JCawangan = _unitOfWork.JCawanganRepo.GetAll();
            ViewBag.AkBank = _unitOfWork.AkBankRepo.GetAll();
            ViewBag.AkCarta = _unitOfWork.AkCartaRepo.GetResultsByParas(EnParas.Paras4);
            ViewBag.JKWPTJBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetails();
            ViewBag.JKWPTJBahagianByJKW = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsByJKWId(JKWId);
            ViewBag.AkBelian = _unitOfWork.AkBelianRepo.GetAllByStatus(EnStatusBorang.Lulus);
            var jenisBayaran = EnumHelper<EnJenisBayaran>.GetList();

            ViewBag.EnJenisBayaran = jenisBayaran;

            var kategoriDaftarAwam = EnumHelper<EnKategoriDaftarAwam>.GetList();

            ViewBag.EnKategoriDaftarAwam = kategoriDaftarAwam;

            var jenisId = EnumHelper<EnJenisId>.GetList();

            ViewBag.EnJenisId = jenisId;

            ViewBag.AkRekup = _unitOfWork.AkRekupRepo.GetAllFilteredBy(false);
            ViewBag.DPanjar = _unitOfWork.DPanjarRepo.GetAllDetails();
            //ViewBag.SuGajiBulanan = _context.SuGajiBulanan.ToList();
        }

        private void PopulateCartAkPVFromDb(AkPV akPV)
        {
            if (akPV.AkPVObjek != null)
            {
                foreach (var item in akPV.AkPVObjek)
                {
                    _cart.AddItemObjek(
                            akPV.Id,
                            item.JKWPTJBahagianId,
                            item.AkCartaId,
                            item.JCukaiId,
                            item.Amaun);
                }
            }

            if (akPV.AkPVInvois != null)
            {
                foreach (var item in akPV.AkPVInvois)
                {
                    _cart.AddItemInvois(akPV.Id,
                                        item.IsTanggungan,
                                        item.AkBelianId,
                                        item.Amaun);
                }
            }

            if (akPV.AkPVPenerima != null)
            {
                foreach (var item in akPV.AkPVPenerima)
                {
                    _cart.AddItemPenerima(item.Id,
                                          akPV.Id,
                                          item.AkJanaanProfilPenerimaId,
                                          item.EnKategoriDaftarAwam,
                                          item.DDaftarAwamId,
                                          item.DPekerjaId,
                                          item.NoPendaftaranPenerima,
                                          item.NamaPenerima,
                                          item.NoPendaftaranPemohon,
                                          item.Catatan,
                                          item.JCaraBayarId,
                                          item.JBankId,
                                          item.NoAkaunBank,
                                          item.Alamat1,
                                          item.Alamat2,
                                          item.Alamat3,
                                          item.Emel,
                                          item.KodM2E,
                                          item.NoRujukanCaraBayar,
                                          item.TarikhCaraBayar,
                                          item.Amaun,
                                          item.NoRujukanMohon,
                                          item.AkRekupId,
                                          item.DPanjarId,
                                          item.IsCekDitunaikan,
                                          item.TarikhCekDitunaikan,
                                          item.EnStatusEFT,
                                          item.Bil,
                                          item.EnJenisId);
                }
            }
            PopulateListViewFromCart();
        }

        private void PopulateListViewFromCart()
        {
            List<AkPVObjek> objek = _cart.AkPVObjek.ToList();

            foreach (AkPVObjek item in objek)
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

            ViewBag.akPVObjek = objek;

            List<AkPVInvois> invois = _cart.AkPVInvois.ToList();

            foreach (AkPVInvois item in invois)
            {
                var akBelian = _unitOfWork.AkBelianRepo.GetDetailsById(item.AkBelianId);

                item.AkBelian = akBelian;

            }
            ViewBag.akPVInvois = invois;

            List<AkPVPenerima> penerima = _cart.AkPVPenerima.ToList();

            foreach (var item in penerima)
            {
                if (item.DDaftarAwamId != null)
                {
                    var dDaftarAwam = _unitOfWork.DDaftarAwamRepo.GetAllDetailsById((int)item.DDaftarAwamId);
                    item.DDaftarAwam = dDaftarAwam;
                }
                
                if (item.DPekerjaId != null)
                {
                    var dPekerja = _unitOfWork.DPekerjaRepo.GetAllDetailsById((int)item.DPekerjaId);
                    item.DPekerja = dPekerja;
                }
                
            }
            ViewBag.akPVPenerima = penerima;
        }

        // jsonResult
        public JsonResult EmptyCart()
        {
            try
            {
                _cart.ClearObjek();
                _cart.ClearInvois();
                _cart.ClearPenerima();

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAkBelian(int AkBelianId)
        {
            try
            {
                if (AkBelianId != 0)
                {
                    var data = _unitOfWork.AkBelianRepo.GetDetailsById(AkBelianId);

                    if (data != null)
                    {
                        data = _unitOfWork.AkBelianRepo.GetBalanceAdjustmentFromAkDebitKreditDiterima(data);

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

        public JsonResult SaveCartAkPVObjek(AkPVObjek akPVObjek)
        {
            try
            {
                var jCukai = new JCukai();
                if (akPVObjek != null)
                {
                    _cart.AddItemObjek(akPVObjek.AkPVId, akPVObjek.JKWPTJBahagianId, akPVObjek.AkCartaId, akPVObjek.JCukaiId, akPVObjek.Amaun);

                    if (akPVObjek.JCukaiId != null)
                    {
                        jCukai = _unitOfWork.JCukaiRepo.GetById((int)akPVObjek.JCukaiId);
                    }

                }

                return Json(new { result = "OK", jCukai = jCukai });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkPVObjek(AkPVObjek akPVObjek)
        {
            try
            {
                if (akPVObjek != null)
                {
                    _cart.RemoveItemObjek(akPVObjek.JKWPTJBahagianId, akPVObjek.AkCartaId);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkPVObjek(AkPVObjek akPVObjek)
        {

            try
            {
                AkPVObjek data = _cart.AkPVObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akPVObjek.JKWPTJBahagianId && x.AkCartaId == akPVObjek.AkCartaId) ?? new AkPVObjek();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkPVObjek(AkPVObjek akPVObjek)
        {

            try
            {

                var data = _cart.AkPVObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akPVObjek.JKWPTJBahagianId && x.AkCartaId == akPVObjek.AkCartaId);

                var user = _userManager.GetUserName(User);

                if (data != null)
                {
                    _cart.RemoveItemObjek(akPVObjek.JKWPTJBahagianId, akPVObjek.AkCartaId);

                    _cart.AddItemObjek(akPVObjek.AkPVId,
                                    akPVObjek.JKWPTJBahagianId,
                                    akPVObjek.AkCartaId,
                                    akPVObjek.JCukaiId,
                                    akPVObjek.Amaun);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveCartAkPVInvois(AkPVInvois akPVInvois)
        {
            try
            {
                var data = _unitOfWork.AkBelianRepo.GetDetailsById(akPVInvois.AkBelianId);

                if (data != null)
                {
                    if (data.AkBelianObjek != null)
                    {
                        foreach (var item in data.AkBelianObjek)
                        {
                            
                            var currentObjek = _cart.AkPVObjek.FirstOrDefault(i => i.JKWPTJBahagianId == item.JKWPTJBahagianId && i.AkCartaId == item.AkCartaId);
                            if (currentObjek != null)
                            {
                                _cart.RemoveItemObjek(item.JKWPTJBahagianId, item.AkCartaId);

                                decimal totalAmaun = currentObjek.Amaun;
                                totalAmaun += akPVInvois.Amaun;

                                _cart.AddItemObjek(
                                    akPVInvois.AkPVId,
                                    item.JKWPTJBahagianId,
                                    item.AkCartaId,
                                    null,
                                    totalAmaun);
                            }
                            else
                            {
                                _cart.AddItemObjek(
                                    akPVInvois.AkPVId,
                                    item.JKWPTJBahagianId,
                                    item.AkCartaId,
                                    null,
                                    akPVInvois.Amaun);
                            }
                            
                        }

                        if (data.AkPOId != null)
                        {
                            _cart.AddItemInvois(
                                akPVInvois.AkPVId,
                                true,
                                data.Id,
                                akPVInvois.Amaun);

                        }
                        else
                        {
                            _cart.AddItemInvois(
                            akPVInvois.AkPVId,
                            false,
                            data.Id,
                            akPVInvois.Amaun
                            );
                        }

                        var currentPenerima = _cart.AkPVPenerima.FirstOrDefault(p => p.DDaftarAwamId == data.DDaftarAwamId);

                        var bil = _cart.AkPVPenerima.Count() + 1;

                        if (currentPenerima != null)
                        {
                            _cart.RemoveItemPenerima((int)currentPenerima.Bil!);


                            var totalAmaun = currentPenerima.Amaun;
                            totalAmaun += akPVInvois.Amaun;

                            _cart.AddItemPenerima(0,
                                              akPVInvois.AkPVId,
                                              null,
                                              EnKategoriDaftarAwam.Pembekal,
                                              data.DDaftarAwamId,
                                              null,
                                              data.DDaftarAwam?.NoPendaftaran,
                                              data.DDaftarAwam?.Nama,
                                              "",
                                              data.Ringkasan,
                                              0,
                                              data.DDaftarAwam?.JBankId,
                                              data.DDaftarAwam?.NoAkaunBank,
                                              data.DDaftarAwam?.Alamat1,
                                              data.DDaftarAwam?.Alamat2,
                                              data.DDaftarAwam?.Alamat3,
                                              data.DDaftarAwam?.Emel,
                                              data.DDaftarAwam?.KodM2E,
                                              null,
                                              null,
                                              totalAmaun,
                                              null,
                                              null,
                                              null,
                                              false,
                                              null,
                                              EnStatusProses.None,
                                              bil,
                                              data.DDaftarAwam?.EnJenisId ?? EnJenisId.None);
                        }
                        else
                        {
                            _cart.AddItemPenerima(0,
                                              akPVInvois.AkPVId,
                                              null,
                                              EnKategoriDaftarAwam.Pembekal,
                                              data.DDaftarAwamId,
                                              null,
                                              data.DDaftarAwam?.NoPendaftaran,
                                              data.DDaftarAwam?.Nama,
                                              "",
                                              data.Ringkasan,
                                              0,
                                              data.DDaftarAwam?.JBankId,
                                              data.DDaftarAwam?.NoAkaunBank,
                                              data.DDaftarAwam?.Alamat1,
                                              data.DDaftarAwam?.Alamat2,
                                              data.DDaftarAwam?.Alamat3,
                                              data.DDaftarAwam?.Emel,
                                              data.DDaftarAwam?.KodM2E,
                                              null,
                                              null,
                                              akPVInvois.Amaun,
                                              null,
                                              null,
                                              null,
                                              false,
                                              null,
                                              EnStatusProses.None,
                                              bil,
                                              data.DDaftarAwam?.EnJenisId ?? EnJenisId.None);
                        }
                        

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

        public JsonResult RemoveCartAkPVInvois(AkPVInvois akPVInvois)
        {
            try
            {
                var invois = _cart.AkPVInvois.FirstOrDefault(i => i.AkBelianId == akPVInvois.AkBelianId);

                if (invois != null)
                {
                    _cart.RemoveItemInvois(invois.AkBelianId);

                    var data = _unitOfWork.AkBelianRepo.GetDetailsById(invois.AkBelianId);

                    if (data != null)
                    {
                        if (data.AkBelianObjek != null)
                        {
                            // akPVObjek
                            foreach (var item in data.AkBelianObjek)
                            {
                                
                                var currentObjek = _cart.AkPVObjek.FirstOrDefault(i => i.JKWPTJBahagianId == item.JKWPTJBahagianId && i.AkCartaId == item.AkCartaId);
                                // check if akBelianObjek same with akPVObjek amount is greater or not
                                if (currentObjek != null && currentObjek.Amaun > akPVInvois.Amaun)
                                {
                                    // if greater, minus amount, add new akPVObjek with new amount
                                    _cart.RemoveItemObjek(item.JKWPTJBahagianId, item.AkCartaId);

                                    decimal totalAmaun = currentObjek.Amaun;
                                    totalAmaun -= akPVInvois.Amaun;

                                    _cart.AddItemObjek(
                                        invois.AkPVId,
                                        item.JKWPTJBahagianId,
                                        item.AkCartaId,
                                        null,
                                        totalAmaun);
                                }
                                else
                                {
                                    // else, remove akPVObjek
                                    _cart.RemoveItemObjek(item.JKWPTJBahagianId, item.AkCartaId);

                                }

                            }
                            //

                            // akPVBelian
                            _cart.RemoveItemInvois(akPVInvois.AkBelianId);
                            //

                            // akPVPenerima
                            var currentPenerima = _cart.AkPVPenerima.FirstOrDefault(p => p.DDaftarAwamId == data.DDaftarAwamId);

                            // check if akBelianObjek same with akPVObjek amount is greater or not
                            if (currentPenerima != null && currentPenerima.Amaun > akPVInvois.Amaun)
                            {
                                // if greater, minus amount, add new akPVPenerima with new amount
                                var bil = currentPenerima.Bil;
                                _cart.RemoveItemPenerima((int)currentPenerima.Bil!);


                                var totalAmaun = currentPenerima.Amaun;
                                totalAmaun -= akPVInvois.Amaun;

                                _cart.AddItemPenerima(0,
                                                  invois.AkPVId,
                                                  null,
                                                  EnKategoriDaftarAwam.Pembekal,
                                                  data.DDaftarAwamId,
                                                  null,
                                                  data.DDaftarAwam?.NoPendaftaran,
                                                  data.DDaftarAwam?.Nama,
                                                  "",
                                                  data.Ringkasan,
                                                  0,
                                                  data.DDaftarAwam?.JBankId,
                                                  data.DDaftarAwam?.NoAkaunBank,
                                                  data.DDaftarAwam?.Alamat1,
                                                  data.DDaftarAwam?.Alamat2,
                                                  data.DDaftarAwam?.Alamat3,
                                                  data.DDaftarAwam?.Emel,
                                                  data.DDaftarAwam?.KodM2E,
                                                  null,
                                                  null,
                                                  totalAmaun,
                                                  null,
                                                  null,
                                                  null,
                                                  false,
                                                  null,
                                                  EnStatusProses.None,
                                                  bil,
                                                  data.DDaftarAwam?.EnJenisId ?? EnJenisId.None);
                            }
                            else
                            {
                                // else, remove akPVPenerima
                                _cart.RemoveItemPenerima((int)currentPenerima?.Bil!);
                            }
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

        public JsonResult GetAnItemFromCartAkPVInvois(AkPVInvois akPVInvois)
        {

            try
            {
                AkPVInvois data = _cart.AkPVInvois.FirstOrDefault(x => x.AkBelianId == akPVInvois.AkBelianId) ?? new AkPVInvois();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkPVInvois(AkPVInvois akPVInvois)
        {

            try
            {

                var data = _cart.AkPVInvois.FirstOrDefault(x => x.AkBelianId == akPVInvois.AkBelianId);

                if (data != null)
                {

                    var akBelian = _unitOfWork.AkBelianRepo.GetDetailsById(akPVInvois.AkBelianId);

                    _cart.RemoveItemInvois(akPVInvois.AkBelianId);

                    if (akBelian.AkPOId != null)
                    {
                        _cart.AddItemInvois(
                            data.AkPVId,
                            true,
                            data.AkBelianId,
                            akPVInvois.Amaun);

                    }
                    else
                    {
                        _cart.AddItemInvois(
                        data.AkPVId,
                        false,
                        data.AkBelianId,
                        akPVInvois.Amaun
                        );
                    }

                    // akPVObjek
                    if (akBelian.AkBelianObjek != null)
                    {
                        foreach (var item in akBelian.AkBelianObjek)
                        {

                            var currentObjek = _cart.AkPVObjek.FirstOrDefault(i => i.JKWPTJBahagianId == item.JKWPTJBahagianId && i.AkCartaId == item.AkCartaId);
                            // check if akBelianObjek same with akPVObjek amount is greater or not
                            if (currentObjek != null && currentObjek.Amaun != akPVInvois.Amaun)
                            {
                                // if greater, minus amount, add new akPVObjek with new amount
                                _cart.RemoveItemObjek(item.JKWPTJBahagianId, item.AkCartaId);

                                decimal totalAmaun = currentObjek.Amaun;
                                totalAmaun = totalAmaun - data.Amaun + akPVInvois.Amaun;

                                _cart.AddItemObjek(
                                    currentObjek.AkPVId,
                                    item.JKWPTJBahagianId,
                                    item.AkCartaId,
                                    null,
                                    totalAmaun);
                            }

                        }
                    }
                    //

                    // akPVPenerima
                    var currentPenerima = _cart.AkPVPenerima.FirstOrDefault(p => p.DDaftarAwamId == akBelian.DDaftarAwamId);

                    if (currentPenerima != null && currentPenerima.Amaun != akPVInvois.Amaun)
                    {
                        // if greater, minus amount, add new akPVPenerima with new amount
                        _cart.RemoveItemPenerima((int)currentPenerima.Bil!);


                        var totalAmaun = currentPenerima.Amaun;
                        totalAmaun = totalAmaun - data.Amaun + akPVInvois.Amaun;

                        _cart.AddItemPenerima(0,
                                          data.AkPVId,
                                          null,
                                          EnKategoriDaftarAwam.Pembekal,
                                          akBelian.DDaftarAwamId,
                                          null,
                                          akBelian.DDaftarAwam?.NoPendaftaran,
                                          akBelian.DDaftarAwam?.Nama,
                                          "",
                                          akBelian.Ringkasan,
                                          0,
                                          akBelian.DDaftarAwam?.JBankId,
                                          akBelian.DDaftarAwam?.NoAkaunBank,
                                          akBelian.DDaftarAwam?.Alamat1,
                                          akBelian.DDaftarAwam?.Alamat2,
                                          akBelian.DDaftarAwam?.Alamat3,
                                          akBelian.DDaftarAwam?.Emel,
                                          akBelian.DDaftarAwam?.KodM2E,
                                          null,
                                          null,
                                          totalAmaun,
                                          null,
                                          null,
                                          null,
                                          false,
                                          null,
                                          EnStatusProses.None,
                                          currentPenerima.Bil,
                                          akBelian.DDaftarAwam?.EnJenisId ?? EnJenisId.None);
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


        public JsonResult GetBil(int Bil)
        {
            try
            {
                var akPV = _cart.AkPVPenerima.FirstOrDefault(pp => pp.Bil == Bil);
                if (akPV != null)
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


        public JsonResult SaveCartAkPVPenerima(AkPVPenerima akPVPenerima)
        {
            try
            {
                if (akPVPenerima != null)
                {
                    switch (akPVPenerima.EnKategoriDaftarAwam)
                    {
                        case EnKategoriDaftarAwam.Pembekal:
                            var dDaftarAwam = _unitOfWork.DDaftarAwamRepo.GetAllDetailsById((int)akPVPenerima.DDaftarAwamId!);
                            akPVPenerima.NamaPenerima = akPVPenerima.NamaPenerima ?? dDaftarAwam.Nama;
                            akPVPenerima.NoPendaftaranPenerima = akPVPenerima.NoPendaftaranPenerima ?? dDaftarAwam.NoPendaftaran;
                            akPVPenerima.NoPendaftaranPemohon = akPVPenerima.NoPendaftaranPemohon ?? dDaftarAwam.NoPendaftaran;
                            akPVPenerima.Alamat1 = akPVPenerima.Alamat1 ?? dDaftarAwam.Alamat1;
                            akPVPenerima.Alamat2 = akPVPenerima.Alamat2 ?? dDaftarAwam.Alamat2;
                            akPVPenerima.Alamat3 = akPVPenerima.Alamat3 ?? dDaftarAwam.Alamat3;
                            akPVPenerima.JBankId = akPVPenerima.JBankId ?? dDaftarAwam.JBankId;
                            akPVPenerima.Emel = akPVPenerima.Emel ?? dDaftarAwam.Emel;
                            akPVPenerima.KodM2E = akPVPenerima.KodM2E ?? dDaftarAwam.KodM2E;
                            akPVPenerima.NoAkaunBank = akPVPenerima.NoAkaunBank ?? dDaftarAwam.NoAkaunBank;
                            akPVPenerima.EnJenisId = dDaftarAwam.EnJenisId;
                            break;
                        case EnKategoriDaftarAwam.Pekerja:
                            var dPekerja = _unitOfWork.DPekerjaRepo.GetAllDetailsById((int)akPVPenerima.DPekerjaId!);
                            akPVPenerima.NamaPenerima = akPVPenerima.NamaPenerima ?? dPekerja.Nama;
                            akPVPenerima.NoPendaftaranPenerima = akPVPenerima.NoPendaftaranPenerima ?? dPekerja.NoKp;
                            akPVPenerima.NoPendaftaranPemohon = akPVPenerima.NoPendaftaranPemohon ?? dPekerja.NoKp;
                            akPVPenerima.Alamat1 = akPVPenerima.Alamat1 ?? dPekerja.Alamat1;
                            akPVPenerima.Alamat2 = akPVPenerima.Alamat2 ?? dPekerja.Alamat2;
                            akPVPenerima.Alamat3 = akPVPenerima.Alamat3 ?? dPekerja.Alamat3;
                            akPVPenerima.JBankId = akPVPenerima.JBankId ?? dPekerja.JBankId;
                            akPVPenerima.Emel = akPVPenerima.Emel ?? dPekerja.Emel;
                            akPVPenerima.KodM2E = akPVPenerima.KodM2E ?? dPekerja.KodM2E;
                            akPVPenerima.NoAkaunBank = akPVPenerima.NoAkaunBank ?? dPekerja.NoAkaunBank;
                            akPVPenerima.EnJenisId = dPekerja.EnJenisId;
                            break;
                        case EnKategoriDaftarAwam.LainLain:
                            // rekupan / baki awal
                            if (akPVPenerima.AkRekupId != null)
                            {
                                var akRekup = _unitOfWork.AkRekupRepo.GetDetailsById((int)akPVPenerima.AkRekupId);

                                if (akRekup != null && akRekup.DPanjar != null && akRekup.DPanjar.DPanjarPemegang != null && akRekup.DPanjar.DPanjarPemegang.Count() > 0)
                                {
                                    // find active pemegang panjar
                                    foreach (var pemegang in akRekup.DPanjar.DPanjarPemegang)
                                    {
                                        if (pemegang.DPekerja != null)
                                        {
                                            if (pemegang.IsAktif)
                                            {
                                                akPVPenerima.NamaPenerima = akPVPenerima.NamaPenerima ?? pemegang.DPekerja.Nama;
                                                akPVPenerima.NoPendaftaranPenerima = akPVPenerima.NoPendaftaranPenerima ?? pemegang.DPekerja.NoKp;
                                                akPVPenerima.NoPendaftaranPemohon = akPVPenerima.NoPendaftaranPemohon ?? pemegang.DPekerja.NoKp;
                                                akPVPenerima.Alamat1 = akPVPenerima.Alamat1 ?? pemegang.DPekerja.Alamat1;
                                                akPVPenerima.Alamat2 = akPVPenerima.Alamat2 ?? pemegang.DPekerja.Alamat2;
                                                akPVPenerima.Alamat3 = akPVPenerima.Alamat3 ?? pemegang.DPekerja.Alamat3;
                                                akPVPenerima.JBankId = akPVPenerima.JBankId ?? pemegang.DPekerja.JBankId;
                                                akPVPenerima.Emel = akPVPenerima.Emel ?? pemegang.DPekerja.Emel;
                                                akPVPenerima.KodM2E = akPVPenerima.KodM2E ?? pemegang.DPekerja.KodM2E;
                                                akPVPenerima.NoAkaunBank = akPVPenerima.NoAkaunBank ?? pemegang.DPekerja.NoAkaunBank;
                                                akPVPenerima.EnJenisId = pemegang.DPekerja.EnJenisId;
                                                akPVPenerima.DPanjarId = akRekup.DPanjarId;
                                                akPVPenerima.Amaun = akRekup.Jumlah;

                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }
                                        
                                    }

                                    var objek = new AkPVObjek()
                                    {
                                        AkCartaId = akRekup.DPanjar.AkCartaId,
                                        JKWPTJBahagianId = (int)akRekup.DPanjar.JKWPTJBahagianId!,
                                        Amaun = akRekup.Jumlah

                                    };
                                    // insert if empty, update if available
                                    if (_cart.AkPVObjek.Any(o => o.AkCartaId == akRekup.DPanjar.AkCartaId && o.JKWPTJBahagianId == akRekup.DPanjar.JKWPTJBahagianId))
                                    {
                                        SaveAnItemFromCartAkPVObjek(objek);
                                    }
                                    else
                                    {
                                        SaveCartAkPVObjek(objek);
                                    }
                                }
                                
                            }
                            // tambah / kurangkan had limit pemegang
                            else
                            {
                                if (akPVPenerima.DPanjarId != null)
                                {
                                    var dPanjar = _unitOfWork.DPanjarRepo.GetAllDetailsById((int)akPVPenerima.DPanjarId);
                                    if (dPanjar != null && dPanjar.DPanjarPemegang != null && dPanjar.DPanjarPemegang.Count() > 0)
                                    {
                                        // find active pemegang panjar
                                        foreach (var pemegang in dPanjar.DPanjarPemegang)
                                        {
                                            if (pemegang.DPekerja != null)
                                            {
                                                if (pemegang.IsAktif)
                                                {
                                                    akPVPenerima.NamaPenerima = akPVPenerima.NamaPenerima ?? pemegang.DPekerja.Nama;
                                                    akPVPenerima.NoPendaftaranPenerima = akPVPenerima.NoPendaftaranPenerima ?? pemegang.DPekerja.NoKp;
                                                    akPVPenerima.NoPendaftaranPemohon = akPVPenerima.NoPendaftaranPemohon ?? pemegang.DPekerja.NoKp;
                                                    akPVPenerima.Alamat1 = akPVPenerima.Alamat1 ?? pemegang.DPekerja.Alamat1;
                                                    akPVPenerima.Alamat2 = akPVPenerima.Alamat2 ?? pemegang.DPekerja.Alamat2;
                                                    akPVPenerima.Alamat3 = akPVPenerima.Alamat3 ?? pemegang.DPekerja.Alamat3;
                                                    akPVPenerima.JBankId = akPVPenerima.JBankId ?? pemegang.DPekerja.JBankId;
                                                    akPVPenerima.Emel = akPVPenerima.Emel ?? pemegang.DPekerja.Emel;
                                                    akPVPenerima.KodM2E = akPVPenerima.KodM2E ?? pemegang.DPekerja.KodM2E;
                                                    akPVPenerima.NoAkaunBank = akPVPenerima.NoAkaunBank ?? pemegang.DPekerja.NoAkaunBank;
                                                    akPVPenerima.EnJenisId = pemegang.DPekerja.EnJenisId;
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                            }

                                        }
                                    }
                                }
                            }
                            break;
                    }
                    if (akPVPenerima.JCaraBayarId != 0)
                    {
                        // check if carabayar exist or not
                        var caraBayar = _unitOfWork.JCaraBayarRepo.GetById(akPVPenerima.JCaraBayarId);
                        if (caraBayar != null)
                        {

                            // check if carabayar bypass limit or not
                            if(caraBayar.IsLimit == true)
                            {
                                if (akPVPenerima.Amaun > caraBayar.MaksAmaun)
                                {
                                    return Json(new { result = "ERROR", message = "Amaun melebihi had mengikut had maksimum cara bayar" });
                                }
                            }

                            int? bil = _cart.AkPVPenerima.OrderByDescending(b => b.Bil).Max()?.Bil ?? 0;

                            bil += 1;

                            _cart.AddItemPenerima(akPVPenerima.Id, akPVPenerima.AkPVId, akPVPenerima.AkJanaanProfilPenerimaId, akPVPenerima.EnKategoriDaftarAwam,
                                akPVPenerima.DDaftarAwamId, akPVPenerima.DPekerjaId, akPVPenerima.NoPendaftaranPenerima, akPVPenerima.NamaPenerima, akPVPenerima.NoPendaftaranPemohon, akPVPenerima.Catatan, akPVPenerima.JCaraBayarId, akPVPenerima.JBankId, akPVPenerima.NoAkaunBank, akPVPenerima.Alamat1, akPVPenerima.Alamat2, akPVPenerima.Alamat3, akPVPenerima.Emel, akPVPenerima.KodM2E, akPVPenerima.NoRujukanCaraBayar, akPVPenerima.TarikhCaraBayar, akPVPenerima.Amaun, akPVPenerima.NoRujukanMohon, akPVPenerima.AkRekupId, akPVPenerima.DPanjarId, akPVPenerima.IsCekDitunaikan, akPVPenerima.TarikhCekDitunaikan, EnStatusProses.None, bil, akPVPenerima.EnJenisId);
                        }
                        else
                        {
                            return Json(new { result = "ERROR", message = "Cara bayar tidak wujud" });
                        }
                        
                    }
                    else
                    {
                        return Json(new { result = "ERROR", message = "Sila pilih cara bayar" });
                    }
                    

                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkPVPenerima(AkPVPenerima akPVPenerima)
        {
            try
            {
                _cart.RemoveItemPenerima((int)akPVPenerima.Bil!);

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkPVPenerima(AkPVPenerima akPVPenerima)
        {

            try
            {
                AkPVPenerima data = _cart.AkPVPenerima.FirstOrDefault(x => x.Bil == akPVPenerima.Bil) ?? new AkPVPenerima();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkPVPenerima(AkPVPenerima akPVPenerima)
        {

            try
            {

                var akPV = _cart.AkPVPenerima.FirstOrDefault(x => x.Bil == akPVPenerima.Bil);

                var user = _userManager.GetUserName(User);

                if (akPV != null && akPVPenerima.Bil != null )
                {
                    switch (akPVPenerima.EnKategoriDaftarAwam)
                    {
                        case EnKategoriDaftarAwam.Pembekal:
                            var dDaftarAwam = _unitOfWork.DDaftarAwamRepo.GetAllDetailsById((int)akPVPenerima.DDaftarAwamId!);
                            akPVPenerima.NamaPenerima = akPVPenerima.NamaPenerima ?? dDaftarAwam.Nama;
                            akPVPenerima.NoPendaftaranPenerima = akPVPenerima.NoPendaftaranPenerima ?? dDaftarAwam.NoPendaftaran;
                            akPVPenerima.NoPendaftaranPemohon = akPVPenerima.NoPendaftaranPemohon ?? dDaftarAwam.NoPendaftaran;
                            akPVPenerima.Alamat1 = akPVPenerima.Alamat1 ?? dDaftarAwam.Alamat1;
                            akPVPenerima.Alamat2 = akPVPenerima.Alamat2 ?? dDaftarAwam.Alamat2;
                            akPVPenerima.Alamat3 = akPVPenerima.Alamat3 ?? dDaftarAwam.Alamat3;
                            akPVPenerima.JBankId = akPVPenerima.JBankId ?? dDaftarAwam.JBankId;
                            akPVPenerima.Emel = akPVPenerima.Emel ?? dDaftarAwam.Emel;
                            akPVPenerima.KodM2E = akPVPenerima.KodM2E ?? dDaftarAwam.KodM2E;
                            akPVPenerima.NoAkaunBank = akPVPenerima.NoAkaunBank ?? dDaftarAwam.NoAkaunBank;
                            akPVPenerima.EnJenisId = dDaftarAwam.EnJenisId;
                            break;
                        case EnKategoriDaftarAwam.Pekerja:
                            var dPekerja = _unitOfWork.DPekerjaRepo.GetAllDetailsById((int)akPVPenerima.DPekerjaId!);
                            akPVPenerima.NamaPenerima = akPVPenerima.NamaPenerima ?? dPekerja.Nama;
                            akPVPenerima.NoPendaftaranPenerima = akPVPenerima.NoPendaftaranPenerima ?? dPekerja.NoKp;
                            akPVPenerima.NoPendaftaranPemohon = akPVPenerima.NoPendaftaranPemohon ?? dPekerja.NoKp;
                            akPVPenerima.Alamat1 = akPVPenerima.Alamat1 ?? dPekerja.Alamat1;
                            akPVPenerima.Alamat2 = akPVPenerima.Alamat2 ?? dPekerja.Alamat2;
                            akPVPenerima.Alamat3 = akPVPenerima.Alamat3 ?? dPekerja.Alamat3;
                            akPVPenerima.JBankId = akPVPenerima.JBankId ?? dPekerja.JBankId;
                            akPVPenerima.Emel = akPVPenerima.Emel ?? dPekerja.Emel;
                            akPVPenerima.KodM2E = akPVPenerima.KodM2E ?? dPekerja.KodM2E;
                            akPVPenerima.NoAkaunBank = akPVPenerima.NoAkaunBank ?? dPekerja.NoAkaunBank;
                            akPVPenerima.EnJenisId = dPekerja.EnJenisId;
                            break;
                        case EnKategoriDaftarAwam.LainLain:
                            if (akPVPenerima.AkRekupId != null)
                            {
                                var akRekup = _unitOfWork.AkRekupRepo.GetDetailsById((int)akPVPenerima.AkRekupId);

                                if (akRekup != null && akRekup.DPanjar != null && akRekup.DPanjar.DPanjarPemegang != null && akRekup.DPanjar.DPanjarPemegang.Count() > 0)
                                {
                                    // find active pemegang panjar
                                    foreach (var pemegang in akRekup.DPanjar.DPanjarPemegang)
                                    {
                                        if (pemegang.DPekerja != null)
                                        {
                                            if (pemegang.IsAktif)
                                            {
                                                akPVPenerima.NamaPenerima = akPVPenerima.NamaPenerima ?? pemegang.DPekerja.Nama;
                                                akPVPenerima.NoPendaftaranPenerima = akPVPenerima.NoPendaftaranPenerima ?? pemegang.DPekerja.NoKp;
                                                akPVPenerima.NoPendaftaranPemohon = akPVPenerima.NoPendaftaranPemohon ?? pemegang.DPekerja.NoKp;
                                                akPVPenerima.Alamat1 = akPVPenerima.Alamat1 ?? pemegang.DPekerja.Alamat1;
                                                akPVPenerima.Alamat2 = akPVPenerima.Alamat2 ?? pemegang.DPekerja.Alamat2;
                                                akPVPenerima.Alamat3 = akPVPenerima.Alamat3 ?? pemegang.DPekerja.Alamat3;
                                                akPVPenerima.JBankId = akPVPenerima.JBankId ?? pemegang.DPekerja.JBankId;
                                                akPVPenerima.Emel = akPVPenerima.Emel ?? pemegang.DPekerja.Emel;
                                                akPVPenerima.KodM2E = akPVPenerima.KodM2E ?? pemegang.DPekerja.KodM2E;
                                                akPVPenerima.NoAkaunBank = akPVPenerima.NoAkaunBank ?? pemegang.DPekerja.NoAkaunBank;
                                                akPVPenerima.EnJenisId = pemegang.DPekerja.EnJenisId;
                                                akPVPenerima.DPanjarId = akRekup.DPanjarId;
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }

                                    }
                                    var objek = new AkPVObjek()
                                    {
                                        AkCartaId = akRekup.DPanjar.AkCartaId,
                                        JKWPTJBahagianId = (int)akRekup.DPanjar.JKWPTJBahagianId!,
                                        Amaun = akRekup.Jumlah

                                    };
                                    // insert if empty, update if available
                                    if (_cart.AkPVObjek.Any(o => o.AkCartaId == akRekup.DPanjar.AkCartaId && o.JKWPTJBahagianId == akRekup.DPanjar.JKWPTJBahagianId))
                                    {
                                        SaveAnItemFromCartAkPVObjek(objek);
                                    }
                                    else
                                    {
                                        SaveCartAkPVObjek(objek);
                                    }
                                }

                            }
                            // tambah / kurangkan had limit pemegang
                            else
                            {
                                if (akPVPenerima.DPanjarId != null)
                                {
                                    var dPanjar = _unitOfWork.DPanjarRepo.GetAllDetailsById((int)akPVPenerima.DPanjarId);
                                    if (dPanjar != null && dPanjar.DPanjarPemegang != null && dPanjar.DPanjarPemegang.Count() > 0)
                                    {
                                        // find active pemegang panjar
                                        foreach (var pemegang in dPanjar.DPanjarPemegang)
                                        {
                                            if (pemegang.DPekerja != null)
                                            {
                                                if (pemegang.IsAktif)
                                                {
                                                    akPVPenerima.NamaPenerima = akPVPenerima.NamaPenerima ?? pemegang.DPekerja.Nama;
                                                    akPVPenerima.NoPendaftaranPenerima = akPVPenerima.NoPendaftaranPenerima ?? pemegang.DPekerja.NoKp;
                                                    akPVPenerima.NoPendaftaranPemohon = akPVPenerima.NoPendaftaranPemohon ?? pemegang.DPekerja.NoKp;
                                                    akPVPenerima.Alamat1 = akPVPenerima.Alamat1 ?? pemegang.DPekerja.Alamat1;
                                                    akPVPenerima.Alamat2 = akPVPenerima.Alamat2 ?? pemegang.DPekerja.Alamat2;
                                                    akPVPenerima.Alamat3 = akPVPenerima.Alamat3 ?? pemegang.DPekerja.Alamat3;
                                                    akPVPenerima.JBankId = akPVPenerima.JBankId ?? pemegang.DPekerja.JBankId;
                                                    akPVPenerima.Emel = akPVPenerima.Emel ?? pemegang.DPekerja.Emel;
                                                    akPVPenerima.KodM2E = akPVPenerima.KodM2E ?? pemegang.DPekerja.KodM2E;
                                                    akPVPenerima.NoAkaunBank = akPVPenerima.NoAkaunBank ?? pemegang.DPekerja.NoAkaunBank;
                                                    akPVPenerima.EnJenisId = pemegang.DPekerja.EnJenisId;
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                            }

                                        }
                                    }
                                }
                            }
                            break;
                    }
                    if (akPVPenerima.JCaraBayarId != 0)
                    {
                        // check if carabayar exist or not
                        var caraBayar = _unitOfWork.JCaraBayarRepo.GetById(akPVPenerima.JCaraBayarId);
                        if (caraBayar != null)
                        {

                            // check if carabayar bypass limit or not
                            if (caraBayar.IsLimit == true)
                            {
                                if (akPVPenerima.Amaun > caraBayar.MaksAmaun)
                                {
                                    return Json(new { result = "ERROR", message = "Amaun melebihi had mengikut had maksimum cara bayar" });
                                }
                            }

                            _cart.UpdateItemPenerima(akPVPenerima.Id,
                                                 akPVPenerima.AkPVId,
                                                 akPVPenerima.AkJanaanProfilPenerimaId,
                                                 akPVPenerima.EnKategoriDaftarAwam,
                                                 akPVPenerima.DDaftarAwamId,
                                                 akPVPenerima.DPekerjaId,
                                                 akPVPenerima.NoPendaftaranPenerima,
                                                 akPVPenerima.NamaPenerima,
                                                 akPVPenerima.NoPendaftaranPemohon,
                                                 akPVPenerima.Catatan,
                                                 akPVPenerima.JCaraBayarId,
                                                 akPVPenerima.JBankId,
                                                 akPVPenerima.NoAkaunBank,
                                                 akPVPenerima.Alamat1,
                                                 akPVPenerima.Alamat2,
                                                 akPVPenerima.Alamat3,
                                                 akPVPenerima.Emel,
                                                 akPVPenerima.KodM2E,
                                                 akPVPenerima.NoRujukanCaraBayar,
                                                 akPVPenerima.TarikhCaraBayar,
                                                 akPVPenerima.Amaun,
                                                 akPVPenerima.NoRujukanMohon,
                                                 akPVPenerima.AkRekupId,
                                                 akPVPenerima.DPanjarId,
                                                 akPVPenerima.IsCekDitunaikan,
                                                 akPVPenerima.TarikhCekDitunaikan,
                                                 akPVPenerima.EnStatusEFT,
                                                 (int)akPVPenerima.Bil,
                                                 akPVPenerima.EnJenisId);
                        }
                        else
                        {
                            return Json(new { result = "ERROR", message = "Cara bayar tidak wujud" });
                        }

                    }
                    else
                    {
                        return Json(new { result = "ERROR", message = "Sila pilih cara bayar" });
                    }

                    
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAllItemCartAkPV()
        {

            try
            {
                List<AkPVObjek> objek = _cart.AkPVObjek.ToList();

                foreach (AkPVObjek item in objek)
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

                List<AkPVInvois> invois = _cart.AkPVInvois.ToList();

                foreach(AkPVInvois pv in invois)
                {
                    var AkBelian = _unitOfWork.AkBelianRepo.GetDetailsById(pv.AkBelianId);

                    pv.AkBelian = AkBelian;
                }

                List<AkPVPenerima> penerima = _cart.AkPVPenerima.ToList();

                return Json(new { result = "OK", objek = objek.OrderBy(d => d.AkCarta?.Kod), invois = invois.OrderBy(i => i.AkBelian!.NoRujukan), penerima = penerima.OrderBy(d => d.Bil) });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        // printing akPV
        public async Task<IActionResult> PrintPDFById(int id)
        {
            AkPV akPV = _unitOfWork.AkPVRepo.GetDetailsById(id);

            var company = await _userServices.GetCompanyDetails();
            EmptyCart();
            PopulateCartAkPVFromDb(akPV);
            //string customSwitches = "--page-offset 0 --footer-center [page] / [toPage] --footer-font-size 6";

            return new ViewAsPdf(modul + EnJenisFail.PDF, akPV,
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
