using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services;
using YIT._DataAccess.Services.Banking;
using YIT._DataAccess.Services.Cart;
using YIT._DataAccess.Services.Math;
using YIT.Akaun.Infrastructure;
using YIT.Akaun.Microservices;
using YIT.Akaun.Models.ViewModels.Common;

namespace YIT.Akaun.Controllers._03Akaun
{
    [Authorize(Roles = Init.allExceptAdminRole)]
    public class AkPenyesuaianBankController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodAkPenyesuaianBank;
        public const string namamodul = Modules.namaAkPenyesuaianBank;
        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;
        private readonly _IBanking _banking;
        private readonly IAkAkaunRepository<AkAkaun> _akaun;
        private readonly IAkAkaunPenyataBankRepository<AkAkaunPenyataBank> _padananPenyata;
        private readonly CartAkPenyesuaianBank _cart;

        private decimal _jumlahPadanan = 0;
        private decimal _bakiPenyataBank = 0;
        private decimal _beza = 0;

        public AkPenyesuaianBankController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices,
            _IBanking banking,
            IAkAkaunRepository<AkAkaun> akaun,
            IAkAkaunPenyataBankRepository<AkAkaunPenyataBank> padananPenyata,
            CartAkPenyesuaianBank cart)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _appLog = appLog;
            _userServices = userServices;
            _banking = banking;
            _akaun = akaun;
            _padananPenyata = padananPenyata;
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

            var akPenyesuaianBank = _unitOfWork.AkPenyesuaianBankRepo.GetResults(searchString, date1, date2, searchColumn, EnStatusBorang.Semua);

            return View(akPenyesuaianBank);
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

        [Authorize(Policy = modul + "C")]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);

            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.PB.GetDisplayName(), DateTime.Now.ToString("yyyy"));
            PopulateDropDownList();
            return View();
        }

        private string GenerateRunningNumber(string initNoRujukan, string tahun)
        {
            var maxRefNo = _unitOfWork.AkPenyesuaianBankRepo.GetMaxRefNo(initNoRujukan, tahun);

            var prefix = initNoRujukan + "/" + tahun + "/";
            return RunningNumberFormatter.GenerateRunningNumber(prefix, maxRefNo, "00000");
        }

        private void PopulateDropDownList()
        {
            ViewBag.AkBank = _unitOfWork.AkBankRepo.GetAll();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "C")]
        public async Task<IActionResult> Create(AkPenyesuaianBank akPenyesuaianBank, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (AkPenyesuaianBankExists(akPenyesuaianBank.AkBankId, akPenyesuaianBank.Tahun, akPenyesuaianBank.Bulan))
            {
                TempData[SD.Error] = "Data bagi tahun, bulan dan kod akaun bank telah wujud..!";
                ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.PB.GetDisplayName(), akPenyesuaianBank.Tahun ?? DateTime.Now.ToString("yyyy"));
                PopulateDropDownList();
                return View(akPenyesuaianBank);
            }

            if (ModelState.IsValid)
            {


                akPenyesuaianBank.Tarikh = DateTime.Now;
                akPenyesuaianBank.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.PB.GetDisplayName(), akPenyesuaianBank.Tahun ?? DateTime.Now.ToString("yyyy"));
                akPenyesuaianBank.UserId = user?.UserName ?? "";
                akPenyesuaianBank.TarMasuk = DateTime.Now;
                akPenyesuaianBank.DPekerjaMasukId = pekerjaId;

                _context.Add(akPenyesuaianBank);
                _appLog.Insert("Tambah", akPenyesuaianBank.NoRujukan ?? "", akPenyesuaianBank.NoRujukan ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya ditambah..!";
                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
            }
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.PB.GetDisplayName(), akPenyesuaianBank.Tahun ?? DateTime.Now.ToString("yyyy"));
            PopulateDropDownList();
            return View(akPenyesuaianBank);
        }

        [Authorize(Policy = modul + "D")]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akPenyesuaianBank = _unitOfWork.AkPenyesuaianBankRepo.GetDetailsById((int)id);
            if (akPenyesuaianBank == null)
            {
                return NotFound();
            }

            if (akPenyesuaianBank.EnStatusBorang != EnStatusBorang.None)
            {
                TempData[SD.Error] = "Hapus data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            return View(akPenyesuaianBank);
        }

        [Authorize(Policy = modul)]
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akPenyesuaianBank = _unitOfWork.AkPenyesuaianBankRepo.GetDetailsById((int)id);
            if (akPenyesuaianBank == null)
            {
                return NotFound();
            }
            EmptyCart();
            return View(akPenyesuaianBank);
        }

        [Authorize(Policy = modul + "E")]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akPenyesuaianBank = _unitOfWork.AkPenyesuaianBankRepo.GetDetailsById((int)id);
            if (akPenyesuaianBank == null)
            {
                return NotFound();
            }

            EmptyCart();
            PopulateDropDownList();

            return View(akPenyesuaianBank);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "E")]
        public async Task<IActionResult> Edit(int id, AkPenyesuaianBank akPenyesuaianBank, string syscode)
        {
            if (id != akPenyesuaianBank.Id)
            {
                return NotFound();
            }

            if (akPenyesuaianBank.NoRujukan != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = _unitOfWork.AkPenyesuaianBankRepo.GetDetailsById(id);
                    var jumlahAsal = objAsal!.BakiPenyata;
                    akPenyesuaianBank.NoRujukan = objAsal.NoRujukan;
                    akPenyesuaianBank.AkBankId = objAsal.AkBankId;
                    akPenyesuaianBank.UserId = objAsal.UserId;
                    akPenyesuaianBank.TarMasuk = objAsal.TarMasuk;
                    akPenyesuaianBank.Tarikh = objAsal.Tarikh;
                    akPenyesuaianBank.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                    _context.Entry(objAsal).State = EntityState.Detached;

                    akPenyesuaianBank.UserIdKemaskini = user?.UserName ?? "";

                    akPenyesuaianBank.TarKemaskini = DateTime.Now;
                    akPenyesuaianBank.DPekerjaKemaskiniId = pekerjaId;

                    _unitOfWork.AkPenyesuaianBankRepo.Update(akPenyesuaianBank);

                    if (jumlahAsal != akPenyesuaianBank.BakiPenyata)
                    {
                        _appLog.Insert("Ubah", Convert.ToDecimal(jumlahAsal).ToString("#,##0.00") + " -> " + Convert.ToDecimal(akPenyesuaianBank.BakiPenyata).ToString("#,##0.00") + " : " + akPenyesuaianBank.NoRujukan ?? "", akPenyesuaianBank.NoRujukan ?? "", id, akPenyesuaianBank.BakiPenyata, pekerjaId, modul, syscode, namamodul, user);
                    }
                    else
                    {
                        _appLog.Insert("Ubah", akPenyesuaianBank.NoRujukan ?? "", akPenyesuaianBank.NoRujukan ?? "", id, akPenyesuaianBank.BakiPenyata, pekerjaId, modul, syscode, namamodul, user);
                    }

                    await _context.SaveChangesAsync();

                    TempData[SD.Success] = "Data berjaya diubah..!";

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AkPenyesuaianBankExists(akPenyesuaianBank.Id))
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
            PopulateDropDownList();
            return View(akPenyesuaianBank);
        }

        [Authorize(Policy = modul)]
        public IActionResult Upload(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akPenyesuaianBank = _unitOfWork.AkPenyesuaianBankRepo.GetDetailsById((int)id);
            if (akPenyesuaianBank == null)
            {
                return NotFound();
            }
            EmptyCart();
            return View(akPenyesuaianBank);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "E")]
        public async Task<IActionResult> Upload(int id, AkPenyesuaianBank akPenyesuaianBank, string syscode)
        {
            if (id != akPenyesuaianBank.Id)
            {
                return NotFound();
            }

            if (akPenyesuaianBank.NoRujukan != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = _unitOfWork.AkPenyesuaianBankRepo.GetDetailsById(id);
                    var jumlahAsal = objAsal!.BakiPenyata;
                    akPenyesuaianBank.NoRujukan = objAsal.NoRujukan;
                    akPenyesuaianBank.AkBankId = objAsal.AkBankId;
                    akPenyesuaianBank.UserId = objAsal.UserId;
                    akPenyesuaianBank.TarMasuk = objAsal.TarMasuk;
                    akPenyesuaianBank.Tarikh = objAsal.Tarikh;
                    akPenyesuaianBank.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                    if (objAsal.AkPenyesuaianBankPenyataBank != null && objAsal.AkPenyesuaianBankPenyataBank.Count > 0)
                    {
                        foreach (var item in objAsal.AkPenyesuaianBankPenyataBank)
                        {
                            var model = _context.AkPenyesuaianBankPenyataBank.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null)
                            {
                                _context.Remove(model);
                            }
                        }
                    }

                    _context.Entry(objAsal).State = EntityState.Detached;

                    akPenyesuaianBank.UserIdKemaskini = user?.UserName ?? "";

                    akPenyesuaianBank.TarKemaskini = DateTime.Now;
                    akPenyesuaianBank.DPekerjaKemaskiniId = pekerjaId;
                    akPenyesuaianBank.IsMuatNaik = true;
                    akPenyesuaianBank.TarikhMuatNaik = DateTime.Now;

                    akPenyesuaianBank.AkPenyesuaianBankPenyataBank = _cart.AkPenyesuaianBankPenyataBank.ToList();


                    _unitOfWork.AkPenyesuaianBankRepo.Update(akPenyesuaianBank);

                    if (jumlahAsal != akPenyesuaianBank.BakiPenyata)
                    {
                        _appLog.Insert("Ubah", Convert.ToDecimal(jumlahAsal).ToString("#,##0.00") + " -> " + Convert.ToDecimal(akPenyesuaianBank.BakiPenyata).ToString("#,##0.00") + " : " + akPenyesuaianBank.NoRujukan ?? "", akPenyesuaianBank.NoRujukan ?? "", id, akPenyesuaianBank.BakiPenyata, pekerjaId, modul, syscode, namamodul, user);
                    }
                    else
                    {
                        _appLog.Insert("Ubah", akPenyesuaianBank.NoRujukan ?? "", akPenyesuaianBank.NoRujukan ?? "", id, akPenyesuaianBank.BakiPenyata, pekerjaId, modul, syscode, namamodul, user);
                    }

                    await _context.SaveChangesAsync();

                    TempData[SD.Success] = "Data berjaya diubah..!";

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AkPenyesuaianBankExists(akPenyesuaianBank.Id))
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
            PopulateDropDownList();
            return View(akPenyesuaianBank);
        }
        [Authorize(Policy = modul)]
        public IActionResult Reconcile(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akPenyesuaianBank = _unitOfWork.AkPenyesuaianBankRepo.GetDetailsById((int)id);
            if (akPenyesuaianBank == null)
            {
                return NotFound();
            }
            EmptyCart();

            PopulateCartFromDb(akPenyesuaianBank.Id);
            PopulateSummaryFields(akPenyesuaianBank.AkBankId, int.Parse(akPenyesuaianBank.Tahun ?? DateTime.Now.ToString("yyyy")), int.Parse(akPenyesuaianBank.Bulan ?? DateTime.Now.ToString("MM")));
            
            return View(akPenyesuaianBank);
        }

        private void PopulateCartFromDb(int id)
        {
            EmptyCart();

            decimal bayaranBelumAkuiBukuTunai = 0;
            decimal terimaanBelumAkuiBukuTunai = 0;

            var akPenyesuaianBankPenyataBank = _unitOfWork.AkPenyesuaianBankRepo.GetAkPenyesuaianBankPenyataBankByAkPenyesuaianBankId(id);
            if (akPenyesuaianBankPenyataBank != null && akPenyesuaianBankPenyataBank.Any())
            {
                foreach (var item in akPenyesuaianBankPenyataBank)
                {
                    _bakiPenyataBank += item.Debit;
                    _bakiPenyataBank -= item.Kredit;

                    if (item.IsPadan == false)
                    {
                        bayaranBelumAkuiBukuTunai += item.Debit;
                        terimaanBelumAkuiBukuTunai -= item.Kredit;
                    }
                }

            }

            ViewBag.BayaranBelumAkuiBukuTunai = bayaranBelumAkuiBukuTunai;
            ViewBag.TerimaanBelumAkuiBukuTunai = 0 - terimaanBelumAkuiBukuTunai;
            ViewBag.BakiSepatutnyaPenyataBank = bayaranBelumAkuiBukuTunai + terimaanBelumAkuiBukuTunai;
            ViewBag.BakiPenyataBank = _bakiPenyataBank;

        }

        private void PopulateSummaryFields(int akBankId, int year, int month)
        {
            _jumlahPadanan = 0;
            decimal bayaranBelumJelasPenyataBank = 0;

            decimal terimaanBelumJelasPenyataBank = 0;

            // get AkCarta from AkBank
            var bank = _unitOfWork.AkBankRepo.GetAllDetailsById(akBankId);

            // get list of buku tunai (akAkaun) where  date <= year, month, end of day
            var akaunList = _context.AkAkaun
                        .Where(b => b.FlHapus == 0 && b.AkCarta1Id == bank.AkCartaId
                                && b.Tarikh.Year <= year 
                                && (b.Tarikh.Year == year && b.Tarikh.Month <= month ))
                        .ToList();

            if (akaunList != null && akaunList.Any())
            {
                foreach (var item in akaunList)
                {
                    if (item.NoRujukan != null)
                    {
                        // get list of buku tunai (akPVPenerima)
                        if (item.NoRujukan.StartsWith("PV/"))
                        {
                            var pv = _unitOfWork.AkPVRepo.GetByIdIgnoreQueryFiltersAsync(tt => tt.NoRujukan == item.NoRujukan).Result;

                            if (pv != null)
                            {
                                pv = _unitOfWork.AkPVRepo.GetDetailsById(pv.Id);

                                if (pv.AkPVPenerima != null && pv.AkPVPenerima.Any())
                                {
                                    foreach (var penerima in pv.AkPVPenerima)
                                    {
                                        // if isPadan == true 
                                        // bakiBukuTunai += amaun
                                        
                                        if (penerima.IsCekDitunaikan)
                                        {
                                            _jumlahPadanan += penerima.Amaun;
                                        }
                                        // if isPadan == false
                                        // bayaranBelumJelasPenyataBank += amaun
                                        else
                                        {
                                            bayaranBelumJelasPenyataBank += penerima.Amaun;
                                        }
                                    }
                                }
                            }

                        }
                        else
                        {
                            // if isPadan == true 
                            if (item.IsPadan)
                            {
                                // if debit != 0, bakiBukuTunai += debit
                                if (item.Debit != 0)
                                {
                                    _jumlahPadanan += item.Debit;
                                }
                                // if kredit != 0, bakiBukuTunai -= kredit
                                else
                                {
                                    _jumlahPadanan -= item.Kredit;
                                }
                                
                            }

                            // if isPadan == false
                            else
                            {
                                // if debit != 0, bayaranBelumJelasPenyataBank += debit
                                if (item.Debit != 0)
                                {
                                    bayaranBelumJelasPenyataBank += item.Debit;
                                }
                                // if kredit != 0, terimaanBelumJelasPenyataBank -= kredit
                                else
                                {
                                    terimaanBelumJelasPenyataBank -= item.Kredit;
                                }
                            }

                        }
                        // get list of buku tunai (akPVPenerima) end
                    }
                }
            }
            
            // get list of buku tunai (akAkaun) end

            

            // get list of penyata bank
            _beza = _jumlahPadanan - _bakiPenyataBank;
            ViewBag.JumlahPadanan = _jumlahPadanan;
            ViewBag.BayaranBelumJelasPenyataBank = bayaranBelumJelasPenyataBank;
            ViewBag.TerimaanBelumJelasPenyataBank = 0 - terimaanBelumJelasPenyataBank;
            ViewBag.Beza = _beza;
        }

        private List<AkPenyesuaianBankPenyataBank> GetAkPenyesuaianBankPenyataBank(int id, DateTime tarikhDari, DateTime tarikhHingga,bool isPadan)
        {
            List<AkPenyesuaianBankPenyataBank> data = _unitOfWork.AkPenyesuaianBankRepo.GetAkPenyesuaianBankPenyataBankByAkPenyesuaianBankId(id);

            tarikhHingga = tarikhHingga.AddHours(23.99);
            data = data.Where(x => x.Tarikh >= tarikhDari && x.Tarikh <= tarikhHingga && x.IsPadan == isPadan).ToList();

            if (data != null)
            {
                foreach (var item in data)
                {
                    _cart.AddItemPenyataBank(
                            id,
                            item.Bil,
                            item.Indeks,
                            item.NoAkaunBank,
                            item.Tarikh,
                            item.KodCawanganBank,
                            item.KodTransaksi,
                            item.PerihalTransaksi,
                            item.NoDokumen,
                            item.NoDokumenTambahan1,
                            item.NoDokumenTambahan2,
                            item.NoDokumenTambahan3,
                            item.Debit,
                            item.Kredit,
                            item.SignDebitKredit,
                            item.Baki,
                            item.IsPadan);
                }
            }
            return data ?? new List<AkPenyesuaianBankPenyataBank>();

        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "D")]
        public async Task<IActionResult> DeleteConfirmed(int id, string sebabHapus, string syscode)
        {
            var akPenyesuaianBank = _unitOfWork.AkPenyesuaianBankRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akPenyesuaianBank != null && await _unitOfWork.AkPenyesuaianBankRepo.IsPostedAsync(id, akPenyesuaianBank.NoRujukan ?? "") == false)
            {
                akPenyesuaianBank.UserIdKemaskini = user?.UserName ?? "";
                akPenyesuaianBank.TarKemaskini = DateTime.Now;
                akPenyesuaianBank.DPekerjaKemaskiniId = pekerjaId;
                akPenyesuaianBank.SebabHapus = sebabHapus;
                akPenyesuaianBank.IsMuatNaik = false;
                akPenyesuaianBank.TarikhMuatNaik = null;

                _context.AkPenyesuaianBank.Remove(akPenyesuaianBank);
                _appLog.Insert("Hapus", akPenyesuaianBank.NoRujukan ?? "", akPenyesuaianBank.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var obj = await _context.AkPenyesuaianBank.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);



            // Rollback operation

            if (obj != null)
            {
                // check if already exist data with same tahun, bulan and akaun bank
                if (AkPenyesuaianBankExists(obj.AkBankId, obj.Tahun, obj.Bulan))
                {
                    TempData[SD.Error] = "Data sama telah wujud";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.AkPenyesuaianBank.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.NoRujukan ?? "", obj.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
        }

        private bool AkPenyesuaianBankExists(int id)
        {
            return _unitOfWork.AkPenyesuaianBankRepo.IsExist(b => b.Id == id);
        }
        private bool AkPenyesuaianBankExists(int akBankId, string? tahun, string? bulan)
        {
            return _unitOfWork.AkPenyesuaianBankRepo.IsExist(b => b.AkBankId == akBankId && b.Tahun == tahun && b.Bulan == bulan);
        }

        public JsonResult EmptyCart()
        {
            try
            {
                _cart.ClearPenyataBank();
                _cart.ClearPadanan();
                _cart.ClearPenyataSistem();

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }

        }

        [HttpPost]
        public async Task<JsonResult> UploadBankStatement(string jsonData, int akPenyesuaianBankId, int akBankId)
        {
            try
            {
                // convert to list
                List<AkPenyesuaianBankPenyataBank>? penyataBank = await _banking.ConvertToAkPenyesuaianPenyataBankList(jsonData, akPenyesuaianBankId, akBankId);

                if (penyataBank == null)
                {
                    return Json(new { result = "ERROR", message = "Format fail tidak dapat dibaca" });
                }
                else
                {
                    foreach (var item in penyataBank)
                    {
                        _cart.AddItemPenyataBank(
                            akPenyesuaianBankId,
                            item.Bil,
                            item.Indeks,
                            item.NoAkaunBank,
                            item.Tarikh,
                            item.KodCawanganBank,
                            item.KodTransaksi,
                            item.PerihalTransaksi,
                            item.NoDokumen,
                            item.NoDokumenTambahan1,
                            item.NoDokumenTambahan2,
                            item.NoDokumenTambahan3,
                            item.Debit,
                            item.Kredit,
                            item.SignDebitKredit,
                            item.Baki,
                            item.IsPadan
                            );
                    }
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetAllItemCartAkPenyesuaianBank(bool isPadan)
        {
            try
            {
                List<AkPenyesuaianBankPenyataBank> penyataBank = _cart.AkPenyesuaianBankPenyataBank.Where(pb => pb.IsPadan == isPadan).ToList();
                List<AkPenyesuaianBankPenyataSistem> penyataSistem = _cart.AkPenyesuaianBankPenyataSistem.Where(pb => pb.IsPadan == isPadan).ToList();


                return Json(new { result = "OK", penyataBank = penyataBank.OrderBy(p => p.Tarikh), penyataSistem = penyataSistem.OrderBy(p => p.Tarikh) });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        // get list of bank statement search result
        public JsonResult GetBankStatementList(int id, DateTime tarDari, DateTime tarHingga, bool isPadan)
        {

            try
            {
                List<AkPenyesuaianBankPenyataBank> data = GetAkPenyesuaianBankPenyataBank(id,tarDari, tarHingga, isPadan);

                if (data != null && data.Any())
                {
                    _cart.ClearPenyataBank();
                    foreach (var item in data)
                    {
                        _cart.AddItemPenyataBank(
                            id,
                            item.Bil,
                            item.Indeks,
                            item.NoAkaunBank,
                            item.Tarikh,
                            item.KodCawanganBank,
                            item.KodTransaksi,
                            item.PerihalTransaksi,
                            item.NoDokumen,
                            item.NoDokumenTambahan1,
                            item.NoDokumenTambahan2,
                            item.NoDokumenTambahan3,
                            item.Debit,
                            item.Kredit,
                            item.SignDebitKredit,
                            item.Baki,
                            item.IsPadan
                            );
                    }
                    return Json(new { result = "OK", record = data.OrderBy(b => b.Tarikh) });
                }
                else
                {
                    return Json(new { result = "OK", record = data?.OrderBy(b => b.Tarikh) });
                }

                
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }
        // get list of bank statement search result end

        // get list of system statement search result
        public async Task<JsonResult> GetSystemStatementList(DateTime tarDari, DateTime tarHingga, int akBankId, bool isPadan)
        {

            try
            {
                List<AkPenyesuaianBankPenyataSistem> data = new List<AkPenyesuaianBankPenyataSistem>();

                tarHingga = tarHingga.AddHours(23.99);

                
                // get AkCarta from AkBank
                var bank = _unitOfWork.AkBankRepo.GetAllDetailsById(akBankId);

                if (bank != null && bank.AkCarta != null)
                {
                    // Akaun --
                    // select akaun
                    var akaunList = await _context.AkAkaun
                        .Where(b => b.FlHapus == 0 && b.AkCarta1Id == bank.AkCartaId
                                && b.Tarikh >= tarDari && b.Tarikh <= tarHingga)
                        .ToListAsync();
                    var resit = new AkTerimaTunggal();
                    var jurnal = new AkJurnal();
                     if (akaunList != null && akaunList.Any())
                    {
                        foreach (var item in akaunList)
                        {

                            var parentId = item.Id;

                            if (item.IsPadan != isPadan)
                            {
                                continue;
                            }
                            var perihal = "";

                            switch (item.NoRujukan?.Substring(0, 2))
                            {
                                // AkPVPenerima
                                case "PV":
                                    perihal = "Baucer Pembayaran";
                                    var pv = await _unitOfWork.AkPVRepo.GetByIdIgnoreQueryFiltersAsync(tt => tt.NoRujukan == item.NoRujukan);

                                    if (pv != null)
                                    {
                                        pv = _unitOfWork.AkPVRepo.GetDetailsById(pv.Id);

                                        if (pv != null && pv.AkPVPenerima != null && pv.AkPVPenerima.Any())
                                        {
                                            foreach (var penerima in pv.AkPVPenerima)
                                            {
                                                if (penerima.IsCekDitunaikan != isPadan)
                                                {
                                                    continue;
                                                }

                                                data.Add(new AkPenyesuaianBankPenyataSistem
                                                {
                                                    Id = penerima.Id,
                                                    AkAkaunId = item.Id,
                                                    Tarikh = pv.Tarikh,
                                                    NoRujukan = pv.NoRujukan,
                                                    Perihal = pv.Ringkasan,
                                                    NoSlip = penerima.NoRujukanCaraBayar,
                                                    Debit = penerima.Amaun,
                                                    Kredit = 0,
                                                    IsPV = true,
                                                    JCarabayarId = penerima.JCaraBayarId,
                                                    IsPadan = penerima.IsCekDitunaikan
                                                });
                                            }
                                        }
                                    }
                                    continue;

                                // AKPVPenerima END --
                                case "RR":
                                    resit = await _unitOfWork.AkTerimaTunggalRepo.GetByIdIgnoreQueryFiltersAsync(tt => tt.NoRujukan == item.NoRujukan);
                                    perihal = resit.Ringkasan;
                                    parentId = resit.Id;
                                    break;
                                default:
                                    jurnal = await _unitOfWork.AkJurnalRepo.GetByIdIgnoreQueryFiltersAsync(tt => tt.NoRujukan == item.NoRujukan);
                                    perihal = jurnal.Ringkasan;
                                    parentId = jurnal.Id;
                                    break;
                            }
                            data.Add(new AkPenyesuaianBankPenyataSistem
                            {
                                Id = parentId,
                                AkAkaunId = item.Id,
                                Tarikh = item.Tarikh,
                                NoRujukan = item.NoRujukan,
                                Perihal = perihal,
                                NoSlip = item.NoSlip,
                                Debit = item.Debit,
                                Kredit = item.Kredit,
                                IsPV = false,
                                JCarabayarId = resit.JCaraBayarId,
                                IsPadan = item.IsPadan
                            });
                        }
                    }
                    
                    // Akaun END --
                }

                // insert to cart penyata sistem
                if (data != null && data.Any())
                {
                    _cart.ClearPenyataSistem();

                    foreach (var item in data)
                    {
                        _cart.AddItemPenyataSistem(
                            item.Id,
                            item.AkAkaunId,
                            item.Tarikh,
                            item.NoRujukan,
                            item.Perihal,
                            item.NoSlip,
                            item.Debit,
                            item.Kredit,
                            item.IsPV,
                            item.JCarabayarId,
                            item.IsPadan
                            );
                    }
                    return Json(new { result = "OK", record = data.OrderBy(b => b.Tarikh) });
                }
                else
                {
                    return Json(new { result = "OK", record = data?.OrderBy(b => b.Tarikh) });
                }
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }
        // get list of system statement search result end

        // match bank and system statement
        public async Task<JsonResult> MatchStatementList(
            int id,
            int indek,
            int idBank,
            decimal amaunBank,
            string noSlip,
            List<ListItemViewModel> arrayOfValues)
        {
            // insert 
            try
            {
                var type = "";
                var akPVPenerima = new List<AkPVPenerima>();
                var akTerimaTunggal = new AkTerimaTunggal();
                var akJurnal = new AkJurnal();

                foreach (var item in arrayOfValues) 
                {
                    var cartPenyataSistem = new AkPenyesuaianBankPenyataSistem();
                    type = item.perihal?.Substring(0, 2);
                    int? jCaraBayarId = null;
                    decimal bil = 1;
                    // insert into cart AkPenyesuaianBankPenyataSistem
                    switch (type)
                    {
                        case "PV":
                            
                            var tarPV = _unitOfWork.AkPVRepo.GetById(item.id).Tarikh;
                            akPVPenerima = _unitOfWork.AkPVRepo.GetAkPVPenerimaByAkPVId(item.id);
                            if (akPVPenerima != null && akPVPenerima.Any())
                            {
                                foreach (var penerima in akPVPenerima)
                                {
                                    jCaraBayarId = penerima.JCaraBayarId;

                                    var pv = await _unitOfWork.AkPVRepo.GetByIdIgnoreQueryFiltersAsync(tt => tt.NoRujukan == item.perihal);

                                    if (pv != null)
                                    {

                                        // insert item penyataSistem
                                        penerima.IsCekDitunaikan = true;
                                        penerima.TarikhCekDitunaikan = DateTime.Now;
                                        penerima.NoRujukanCaraBayar = noSlip;
                                        penerima.TarikhCaraBayar = DateTime.Now;

                                        _context.AkPVPenerima.Update(penerima);

                                        _cart.RemoveItemPenyataSistem(penerima.Id, true);

                                        _cart.AddItemPenyataSistem(item.id, item.indek, tarPV, item.perihal, pv.Ringkasan, noSlip, penerima.Amaun, 0, true, penerima.JCaraBayarId, true);

                                        // get bil from cart AkAkaun PenyataBank
                                    bil = _cart.AkAkaunPenyataBank.FirstOrDefault(x => x.AkPenyesuaianBankPenyataBankId == idBank && x.AkPVPenerimaId == penerima.Id)?.Bil ?? 1;

                                        // remove item akaun PenyataBank
                                        _cart.RemoveItemPadanan(null, idBank, penerima.Id, item.isPV);

                                        // insert item akaun PenyataBank
                                        _cart.AddItemPadanan(
                                            item.indek,
                                            idBank, 
                                            penerima.Id,
                                            bil,
                                            false,
                                            penerima.JCaraBayarId,
                                            item.debit,
                                            item.kredit,
                                            tarPV
                                            );

                                        _padananPenyata.Add(new AkAkaunPenyataBank
                                        {
                                            AkAkaunId = item.indek == 0 ? null: item.indek,
                                            AkPenyesuaianBankPenyataBankId = idBank,
                                            AkPVPenerimaId = penerima.Id == 0 ? null: penerima.Id,
                                            Bil = bil,
                                            IsAutoMatch = false,
                                            JCaraBayarId = penerima.JCaraBayarId,
                                            Debit = item.debit,
                                            Kredit = item.kredit,
                                            Tarikh = tarPV
                                        });
                                    }
                                    
                                }
                            }
                            break;
                        case "RR":
                            akTerimaTunggal = _unitOfWork.AkTerimaTunggalRepo.GetById(item.id);
                            if (akTerimaTunggal != null)
                            {
                                // remove item penyataSistem
                                _cart.RemoveItemPenyataSistem(item.indek, false);

                                // insert item penyataSistem
                                _cart.AddItemPenyataSistem(item.id, item.indek, akTerimaTunggal.Tarikh, item.perihal, akTerimaTunggal.Ringkasan, noSlip, 0, akTerimaTunggal.Jumlah, true, akTerimaTunggal.JCaraBayarId, true);

                                bil = _cart.AkAkaunPenyataBank.FirstOrDefault(x => x.AkPenyesuaianBankPenyataBankId == idBank && x.AkAkaunId == item.id)?.Bil ?? 1;

                                _cart.RemoveItemPadanan(null, idBank, akTerimaTunggal.Id, item.isPV);

                                _cart.AddItemPadanan(
                                    item.indek,
                                    idBank, 
                                    null,
                                    bil,
                                    false,
                                    akTerimaTunggal.JCaraBayarId,
                                    item.debit,
                                    item.kredit,
                                    akTerimaTunggal.Tarikh
                                    );

                                _padananPenyata.Add(new AkAkaunPenyataBank
                                {
                                    AkAkaunId = item.indek,
                                    AkPenyesuaianBankPenyataBankId = idBank,
                                    AkPVPenerimaId = null,
                                    Bil = bil,
                                    IsAutoMatch = false,
                                    JCaraBayarId = akTerimaTunggal.JCaraBayarId,
                                    Debit = item.debit,
                                    Kredit = item.kredit,
                                    Tarikh = akTerimaTunggal.Tarikh
                                });
                            }
                            break;
                        case "JU":
                            akJurnal = _unitOfWork.AkJurnalRepo.GetById(item.id);
                            if (akJurnal != null)
                            {
                                AkAkaun akaun = await _akaun.GetByIdAsync(item.id);
                                if (akaun != null)
                                {
                                    // remove item penyataSistem
                                    _cart.RemoveItemPenyataSistem(item.indek, false);

                                    // insert item penyataSistem
                                    _cart.AddItemPenyataSistem(item.id, item.indek, akJurnal.Tarikh, item.perihal, akJurnal.Ringkasan, noSlip, akaun.Debit, akaun.Kredit, true, null,true);

                                    bil = _cart.AkAkaunPenyataBank.FirstOrDefault(x => x.AkPenyesuaianBankPenyataBankId == idBank && x.AkAkaunId == item.id)?.Bil ?? 1;

                                    _cart.RemoveItemPadanan(null, idBank, akJurnal.Id, item.isPV);

                                    _cart.AddItemPadanan(
                                        item.indek,
                                        idBank,
                                        null,
                                        bil,
                                        false,
                                        null,
                                        item.debit,
                                        item.kredit,
                                        akJurnal.Tarikh
                                        );

                                    _padananPenyata.Add(new AkAkaunPenyataBank
                                    {
                                        AkAkaunId = item.indek,
                                        AkPenyesuaianBankPenyataBankId = idBank,
                                        AkPVPenerimaId = null,
                                        Bil = bil,
                                        IsAutoMatch = false,
                                        JCaraBayarId = null,
                                        Debit = item.debit,
                                        Kredit = item.kredit,
                                        Tarikh = akTerimaTunggal?.Tarikh ?? DateTime.Now
                                    });
                                }
                               
                            }
                            break;
                    }
                }
                AkPenyesuaianBankPenyataBank penyataBank = await _context.AkPenyesuaianBankPenyataBank.FirstOrDefaultAsync(b => b.Id == idBank) ?? new AkPenyesuaianBankPenyataBank();
                if (penyataBank != null)
                {
                    penyataBank.IsPadan = true;
                    
                    //insert into cart AkPenyesuaianBankPenyataBank
                    _cart.RemoveItemPenyataBank(indek);

                    _cart.AddItemPenyataBank(
                                id,
                                penyataBank.Bil,
                                penyataBank.Indeks,
                                penyataBank.NoAkaunBank,
                                penyataBank.Tarikh,
                                penyataBank.KodCawanganBank,
                                penyataBank.KodTransaksi,
                                penyataBank.PerihalTransaksi,
                                penyataBank.NoDokumen,
                                penyataBank.NoDokumenTambahan1,
                                penyataBank.NoDokumenTambahan2,
                                penyataBank.NoDokumenTambahan3,
                                penyataBank.Debit,
                                penyataBank.Kredit,
                                penyataBank.SignDebitKredit,
                                penyataBank.Baki,
                                penyataBank.IsPadan);

                    _context.AkPenyesuaianBankPenyataBank.Update(penyataBank);

                    await _context.SaveChangesAsync();
                }

               

                return Json(new { result = "OK", dataBank = "OK", dataSistem = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }
        // match bank and system statement end

        // get list of bank matched statement search result
        public async Task<JsonResult> GetBankMatchedStatementList(DateTime tarDari, DateTime tarHingga, int? padananId, int akPenyesuaianBankId)
        {

            try
            {
                var data = await _padananPenyata.GetPadananPenyataListByAkPenyesuaianBankIdAsync(akPenyesuaianBankId);

                if (data != null && data.Any())
                {
                    if (padananId != null)
                    {
                        data = data.Where(b => b.Id == padananId).ToList();
                    }
                    else
                    {
                        tarHingga = tarHingga.AddHours(23.99);
                        data = data.Where(x => x.Tarikh >= tarDari && x.Tarikh <= tarHingga).ToList();
                    }

                    foreach (var item in data)
                    {
                        item.AkPVPenerima = _context.AkPVPenerima.Include(pp => pp.AkPV).FirstOrDefault(pp => pp.Id == item.AkPVPenerimaId);
                        item.AkAkaun = _context.AkAkaun.Find(item.AkAkaunId);
                        item.AkPenyesuaianBankPenyataBank = _context.AkPenyesuaianBankPenyataBank.Find(item.AkPenyesuaianBankPenyataBankId);
                    }

                }
                return Json(new { result = "OK", record = data?.OrderBy(b => b.Tarikh) });

            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }
        // get list of bank matched statement search result end

        // get list of system matched statement search result
        public async Task<JsonResult> GetSystemMatchedStatementList(int akAkaunPenyataBankId)
        {

            try
            {
                var data = await _padananPenyata.GetListByIdAsync(akAkaunPenyataBankId);

                return Json(new { result = "OK", record = data.OrderBy(b => b.Tarikh) });

            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }
        // get list of system matched statement search result end

        // unmatch bank and system statement
        public async Task<JsonResult> UnMatchStatementList(
            int padananId,
            int indekBank,
            decimal amaunBank,
            List<ListItemViewModel> arrayOfValues,
            int rowSystemCount)
        {
            try
            {
                var padananPenyata = await _padananPenyata.GetListByIdAsync(padananId);
                int idBank = 0;
                if (padananPenyata != null && padananPenyata.Any())
                {
                    foreach (var item in padananPenyata)
                    {
                        idBank = item.AkPenyesuaianBankPenyataBankId;
                    }
                }
                
                var akPVPenerima = new List<AkPVPenerima>();
                var akTerimaTunggal = new AkTerimaTunggal();
                var akJurnal = new AkJurnal();

                foreach (var item in arrayOfValues)
                {
                    var cartPenyataSistem = new AkPenyesuaianBankPenyataSistem();
                    if (item.perihal != null && item.perihal.StartsWith("PV/"))
                    {
                        var tarPV = _unitOfWork.AkPVRepo.GetById(item.id).Tarikh;
                        akPVPenerima = _unitOfWork.AkPVRepo.GetAkPVPenerimaByAkPVId(item.id);
                        if (akPVPenerima != null && akPVPenerima.Any())
                        {
                            foreach (var penerima in akPVPenerima)
                            {
                                var pv = await _unitOfWork.AkPVRepo.GetByIdIgnoreQueryFiltersAsync(tt => tt.NoRujukan == item.perihal);

                                if (pv != null)
                                {

                                    // insert item penyataSistem
                                    penerima.IsCekDitunaikan = false;
                                    penerima.TarikhCekDitunaikan = null;
                                    penerima.NoRujukanCaraBayar = null;
                                    penerima.TarikhCaraBayar = null;

                                    _context.AkPVPenerima.Update(penerima);

                                }

                            }
                        }
                    }
                }

                AkPenyesuaianBankPenyataBank penyataBank = await _context.AkPenyesuaianBankPenyataBank.FirstOrDefaultAsync(b => b.Id == idBank) ?? new AkPenyesuaianBankPenyataBank();
                if (penyataBank != null)
                {
                    penyataBank.IsPadan = false;

                    _context.AkPenyesuaianBankPenyataBank.Update(penyataBank);

                    _padananPenyata.Remove(padananId);

                    await _context.SaveChangesAsync();
                }

                return Json(new { result = "OK", dataBank = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }
    }
}
