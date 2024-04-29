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
    public class AkJurnalController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodAkJurnal;
        public const string namamodul = Modules.namaAkJurnal;
        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly IAbBukuVotRepository<AbBukuVot> _abBukuVotRepo;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;
        private readonly CartAkJurnal _cart;

        public AkJurnalController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            IAbBukuVotRepository<AbBukuVot> abBukuVotRepo,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices,
            CartAkJurnal cart)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _abBukuVotRepo = abBukuVotRepo;
            _userManager = userManager;
            _appLog = appLog;
            _userServices = userServices;
            _cart = cart;
        }
        public IActionResult Index(string searchString,
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

            var akJurnal = _unitOfWork.AkJurnalRepo.GetResults(searchString, date1, date2, searchColumn, EnStatusBorang.Semua);

            return View(akJurnal);
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

            var akJurnal = _unitOfWork.AkJurnalRepo.GetDetailsById((int)id);
            if (akJurnal == null)
            {
                return NotFound();
            }
            EmptyCart();
            ManipulateHiddenDiv(akJurnal.EnJenisJurnal);
            PopulateCartAkJurnalFromDb(akJurnal);
            return View(akJurnal);
        }

        private void PopulateCartAkJurnalFromDb(AkJurnal akJurnal)
        {
            if (akJurnal.AkJurnalObjek != null)
            {
                foreach (var item in akJurnal.AkJurnalObjek)
                {
                    _cart.AddItemObjek(
                            akJurnal.Id,
                            item.JKWPTJBahagianDebitId,
                            item.AkCartaDebitId,
                            item.JKWPTJBahagianKreditId,
                            item.AkCartaKreditId,
                            item.Amaun,
                            item.IsDebitAbBukuVot,
                            item.IsKreditAbBukuVot);
                }
            }

            if (akJurnal.AkJurnalPenerimaCekBatal != null)
            {
                foreach (var item in akJurnal.AkJurnalPenerimaCekBatal)
                {
                    _cart.AddItemPenerimaCekBatal(
                        akJurnal.Id,
                        item.Bil,
                        item.AkPVId,
                        item.NamaPenerima,
                        item.NoCek,
                        item.Amaun
                        );
                }

                PopulateListViewFromCart();
            }
        }

        private void PopulateListViewFromCart()
        {
            List<AkJurnalObjek> objek = _cart.AkJurnalObjek.ToList();

            foreach (AkJurnalObjek item in objek)
            {
                var jkwPtjBahagianDebit = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianDebitId);

                item.JKWPTJBahagianDebit = jkwPtjBahagianDebit;

                item.JKWPTJBahagianDebit.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagianDebit.JKW?.Kod, jkwPtjBahagianDebit.JPTJ?.Kod, jkwPtjBahagianDebit.JBahagian?.Kod);

                var akCartaDebit = _unitOfWork.AkCartaRepo.GetById(item.AkCartaDebitId);

                item.AkCartaDebit = akCartaDebit;

                var jkwPtjBahagianKredit = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianKreditId);

                item.JKWPTJBahagianKredit = jkwPtjBahagianKredit;

                item.JKWPTJBahagianKredit.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagianKredit.JKW?.Kod, jkwPtjBahagianKredit.JPTJ?.Kod, jkwPtjBahagianKredit.JBahagian?.Kod);

                var akCartaKredit = _unitOfWork.AkCartaRepo.GetById(item.AkCartaKreditId);

                item.AkCartaKredit = akCartaKredit;
            }

            ViewBag.akJurnalObjek = objek;

            List<AkJurnalPenerimaCekBatal> penerimaCekBatal = _cart.AkJurnalPenerimaCekBatal.ToList();

            foreach (AkJurnalPenerimaCekBatal item in  penerimaCekBatal)
            {
                var akPV = _unitOfWork.AkPVRepo.GetById(item.AkPVId);
                item.AkPV = akPV;
            }

            ViewBag.akJurnalPenerimaCekBatal = penerimaCekBatal;
        }


        [Authorize(Policy = modul + "D")]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akJurnal = _unitOfWork.AkJurnalRepo.GetDetailsById((int)id);
            if (akJurnal == null)
            {
                return NotFound();
            }

            if (akJurnal.EnStatusBorang != EnStatusBorang.None)
            {
                TempData[SD.Error] = "Hapus data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            ManipulateHiddenDiv(akJurnal.EnJenisJurnal);
            PopulateCartAkJurnalFromDb(akJurnal);
            return View(akJurnal);
        }
        
        [Authorize(Policy = modul + "BL")]
        public IActionResult BatalLulus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akJurnal = _unitOfWork.AkJurnalRepo.GetDetailsById((int)id);
            if (akJurnal == null)
            {
                return NotFound();
            }

            if (akJurnal.EnStatusBorang != EnStatusBorang.Lulus)
            {
                TempData[SD.Error] = "Data belum diluluskan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkJurnalFromDb(akJurnal);
            ManipulateHiddenDiv(akJurnal.EnJenisJurnal);
            return View(akJurnal);
        }

        [HttpPost, ActionName("BatalLulus")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "BL")]
        public async Task<IActionResult> BatalLulusConfirmed(int id, string tindakan, string syscode)
        {
            var akJurnal = _unitOfWork.AkJurnalRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akJurnal != null && !string.IsNullOrEmpty(akJurnal.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkJurnalRepo.IsPostedAsync((int)id, akJurnal.NoRujukan) == false)
                {
                    TempData[SD.Error] = "Data belum diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkJurnalRepo.IsLulusAsync(id) == false)
                {
                    TempData[SD.Error] = "Data belum diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkJurnalRepo.BatalLulus(id, tindakan, user?.Email);

                _appLog.Insert("UnPosting", "Batal Lulus " + akJurnal.NoRujukan ?? "", akJurnal.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var akJurnal = _unitOfWork.AkJurnalRepo.GetDetailsById((int)id);
            if (akJurnal == null)
            {
                return NotFound();
            }

            if (akJurnal.EnStatusBorang != EnStatusBorang.Lulus)
            {
                TempData[SD.Error] = "Data belum diluluskan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkJurnalFromDb(akJurnal);
            ManipulateHiddenDiv(akJurnal.EnJenisJurnal);
            return View(akJurnal);
        }

        [HttpPost, ActionName("BatalPos")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "BL")]
        public async Task<IActionResult> BatalPosConfirmed(int id, string tindakan, string syscode)
        {
            var akJurnal = _unitOfWork.AkJurnalRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akJurnal != null && !string.IsNullOrEmpty(akJurnal.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkJurnalRepo.IsPostedAsync((int)id, akJurnal.NoRujukan) == false)
                {
                    TempData[SD.Error] = "Data belum diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkJurnalRepo.IsLulusAsync(id) == false)
                {
                    TempData[SD.Error] = "Data belum diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkJurnalRepo.BatalPos(id, tindakan, user?.UserName);

                _appLog.Insert("UnPosting", "Batal Pos " + akJurnal.NoRujukan ?? "", akJurnal.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var obj = await _context.AkJurnal.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Pos operation

            if (obj != null && !string.IsNullOrEmpty(obj.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkJurnalRepo.IsPostedAsync((int)id, obj.NoRujukan))
                {
                    TempData[SD.Error] = "Data sudah diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkJurnalRepo.IsLulusAsync(id))
                {
                    TempData[SD.Error] = "Data telah diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkJurnalRepo.Lulus(id, pekerjaId, user?.UserName);

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

            EmptyCart();
            PopulateDropDownList(1);
            ManipulateHiddenDiv(EnJenisJurnal.LainLain);
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.JU.GetDisplayName(), DateTime.Now.ToString("yyyy"));
            return View();
        }

        private void ManipulateHiddenDiv(EnJenisJurnal enJenisJurnal)
        {
            switch (enJenisJurnal)
            {
                case EnJenisJurnal.Baucer:
                    ViewBag.DivAkPV = "";
                    ViewBag.DivPenerimaCekBatal = "hidden";
                    break;
                case EnJenisJurnal.CekBatal:
                    ViewBag.DivAkPV = "hidden";
                    ViewBag.DivPenerimaCekBatal = "";
                    break;
                default:
                    ViewBag.DivAkPV = "hidden";
                    ViewBag.DivPenerimaCekBatal = "hidden";
                    break;
            }
        }
        private string GenerateRunningNumber(string initNoRujukan, string tahun)
        {
            var maxRefNo = _unitOfWork.AkJurnalRepo.GetMaxRefNo(initNoRujukan, tahun);

            var prefix = initNoRujukan + "/" + tahun + "/";
            return RunningNumberFormatter.GenerateRunningNumber(prefix, maxRefNo, "00000");
        }

        private void PopulateDropDownList(int JKWId)
        {
            ViewBag.JKW = _unitOfWork.JKWRepo.GetAll();
            ViewBag.AkPVPenerima = _context.AkPVPenerima.Include(p => p.AkPV).Where(p => p.AkPV!.EnStatusBorang == EnStatusBorang.Lulus && p.EnStatusEFT != EnStatusProses.Fail).ToList();
            ViewBag.AkPV = _unitOfWork.AkPVRepo.GetAllByStatus(EnStatusBorang.Lulus);
            ViewBag.AkCarta = _unitOfWork.AkCartaRepo.GetResultsByParas(EnParas.Paras4);
            ViewBag.JKWPTJBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsByJKWId(JKWId);
            var jenisJurnal = EnumHelper<EnJenisJurnal>.GetList();

            ViewBag.EnJenisJurnal = jenisJurnal;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "C")]
        public async Task<IActionResult> Create(AkJurnal akJurnal, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            // check if there is pengesah available or not based on modul, kelulusan, and bahagian
            if (_cart.AkJurnalObjek != null && _cart.AkJurnalObjek.Count() > 0)
            {
                foreach (var item in _cart.AkJurnalObjek)
                {
                    var jKWPtjBahagianDebit = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianDebitId);

                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModulKelulusan.Jurnal, EnKategoriKelulusan.Pengesah, jKWPtjBahagianDebit.JBahagianId, akJurnal.JumlahDebit) == false)
                    {
                        TempData[SD.Error] = "Tiada Pengesah yang wujud untuk senarai kod bahagian debit berikut.";
                        ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.JU.GetDisplayName(), akJurnal.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
                        PopulateDropDownList(akJurnal.JKWId);
                        PopulateListViewFromCart();
                        return View(akJurnal);
                    }

                    var jKWPtjBahagianKredit = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianKreditId);

                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModulKelulusan.Jurnal, EnKategoriKelulusan.Pengesah, jKWPtjBahagianKredit.JBahagianId, akJurnal.JumlahKredit) == false)
                    {
                        TempData[SD.Error] = "Tiada Pengesah yang wujud untuk senarai kod bahagian kredit berikut.";
                        ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.JU.GetDisplayName(), akJurnal.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
                        PopulateDropDownList(akJurnal.JKWId);
                        ManipulateHiddenDiv(akJurnal.EnJenisJurnal);
                        PopulateListViewFromCart();
                        return View(akJurnal);
                    }
                    
                }
            }
            //

            if (ModelState.IsValid)
            {

                akJurnal.UserId = user?.UserName ?? "";
                akJurnal.TarMasuk = DateTime.Now;
                akJurnal.DPekerjaMasukId = pekerjaId;

                akJurnal.AkJurnalObjek = _cart.AkJurnalObjek?.ToList();
                akJurnal.AkJurnalPenerimaCekBatal = _cart.AkJurnalPenerimaCekBatal.ToList();

                _context.Add(akJurnal);
                _appLog.Insert("Tambah", akJurnal.NoRujukan ?? "", akJurnal.NoRujukan ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya ditambah..!";
                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
            }
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.JU.GetDisplayName(), akJurnal.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
            PopulateDropDownList(akJurnal.JKWId);
            ManipulateHiddenDiv(akJurnal.EnJenisJurnal);
            PopulateListViewFromCart();
            return View(akJurnal);
        }

        [Authorize(Policy = modul + "E")]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akJurnal = _unitOfWork.AkJurnalRepo.GetDetailsById((int)id);
            if (akJurnal == null)
            {
                return NotFound();
            }

            if (akJurnal.EnStatusBorang != EnStatusBorang.None)
            {
                TempData[SD.Error] = "Ubah data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }

            EmptyCart();
            PopulateDropDownList(akJurnal.JKWId);
            ManipulateHiddenDiv(akJurnal.EnJenisJurnal);
            PopulateCartAkJurnalFromDb(akJurnal);
            return View(akJurnal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "E")]
        public async Task<IActionResult> Edit(int id, AkJurnal akJurnal, string? fullName, string syscode)
        {
            if (id != akJurnal.Id)
            {
                return NotFound();
            }

            // check if there is pengesah available or not based on modul, kelulusan, and bahagian
            if (_cart.AkJurnalObjek != null && _cart.AkJurnalObjek.Count() > 0)
            {
                foreach (var item in _cart.AkJurnalObjek)
                {
                    var jKWPtjBahagianDebit = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianDebitId);

                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModulKelulusan.Jurnal, EnKategoriKelulusan.Pengesah, jKWPtjBahagianDebit.JBahagianId, akJurnal.JumlahDebit) == false)
                    {
                        TempData[SD.Error] = "Tiada Pengesah yang wujud untuk senarai kod bahagian debit berikut.";
                        PopulateDropDownList(akJurnal.JKWId);
                        ManipulateHiddenDiv(akJurnal.EnJenisJurnal);
                        PopulateListViewFromCart();
                        return View(akJurnal);
                    }

                    var jKWPtjBahagianKredit = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianKreditId);

                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModulKelulusan.Jurnal, EnKategoriKelulusan.Pengesah, jKWPtjBahagianKredit.JBahagianId, akJurnal.JumlahKredit) == false)
                    {
                        TempData[SD.Error] = "Tiada Pengesah yang wujud untuk senarai kod bahagian kredit berikut.";
                        PopulateDropDownList(akJurnal.JKWId);
                        ManipulateHiddenDiv(akJurnal.EnJenisJurnal);
                        PopulateListViewFromCart();
                        return View(akJurnal);
                    }

                    // check if exist kod akaun in AbBukuVot
                    var JKWPTJBahagianDebit = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianDebitId);
                    var JKWPTJBahagianKredit = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianKreditId);
                    if (JKWPTJBahagianDebit != null && JKWPTJBahagianKredit != null)
                    {
                        if (await _abBukuVotRepo.IsExistByJKWPTJBahagianAkCartaId(JKWPTJBahagianDebit.JKWId, JKWPTJBahagianDebit.JPTJId, JKWPTJBahagianDebit.JBahagianId, item.AkCartaDebitId)
                            && await _abBukuVotRepo.IsExistByJKWPTJBahagianAkCartaId(JKWPTJBahagianKredit.JKWId, JKWPTJBahagianKredit.JPTJId, JKWPTJBahagianKredit.JBahagianId, item.AkCartaKreditId))
                        {
                            item.IsDebitAbBukuVot = true;
                        }
                    }

                }
            }
            //

            if (akJurnal.NoRujukan != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = _unitOfWork.AkJurnalRepo.GetDetailsById(id);
                    var jumlahDebitAsal = objAsal!.JumlahDebit;
                    var jumlahKreditAsal = objAsal!.JumlahKredit;
                    akJurnal.NoRujukan = objAsal.NoRujukan;
                    akJurnal.UserId = objAsal.UserId;
                    akJurnal.TarMasuk = objAsal.TarMasuk;
                    akJurnal.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                    if (objAsal.AkJurnalObjek != null && objAsal.AkJurnalObjek.Count > 0)
                    {
                        foreach (var item in objAsal.AkJurnalObjek)
                        {
                            var model = _context.AkJurnalObjek.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null) _context.Remove(model);
                        }
                    }

                    if (objAsal.AkJurnalPenerimaCekBatal != null && objAsal.AkJurnalPenerimaCekBatal.Count > 0)
                    {
                        foreach (var item in objAsal.AkJurnalPenerimaCekBatal)
                        {
                            var model = _context.AkJurnalPenerimaCekBatal.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null) _context.Remove(model);
                        }
                    }

                    _context.Entry(objAsal).State = EntityState.Detached;

                    akJurnal.UserIdKemaskini = user?.UserName ?? "";
                    akJurnal.TarKemaskini = DateTime.Now;
                    akJurnal.AkJurnalObjek = _cart.AkJurnalObjek?.ToList();
                    akJurnal.AkJurnalPenerimaCekBatal = _cart.AkJurnalPenerimaCekBatal.ToList();

                    _unitOfWork.AkJurnalRepo.Update(akJurnal);

                    if (jumlahDebitAsal != akJurnal.JumlahDebit || jumlahKreditAsal != akJurnal.JumlahKredit)
                    {
                        _appLog.Insert("Ubah","Debit :" + Convert.ToDecimal(jumlahDebitAsal).ToString("#,##0.00") + " -> " + Convert.ToDecimal(akJurnal.JumlahDebit).ToString("#,##0.00")
                            + "; Kredit : "+ Convert.ToDecimal(jumlahKreditAsal).ToString("#,##0.00") + " -> " + Convert.ToDecimal(akJurnal.JumlahKredit).ToString("#,##0.00") 
                            + " : " + akJurnal.NoRujukan ?? "", akJurnal.NoRujukan ?? "", id, akJurnal.JumlahDebit, pekerjaId, modul, syscode, namamodul, user);
                    }
                    else
                    {
                        _appLog.Insert("Ubah", akJurnal.NoRujukan ?? "", akJurnal.NoRujukan ?? "", id, akJurnal.JumlahDebit, pekerjaId, modul, syscode, namamodul, user);
                    }

                    await _context.SaveChangesAsync();

                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AkJurnalExist(akJurnal.Id))
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

            PopulateDropDownList(akJurnal.JKWId);
            ManipulateHiddenDiv(akJurnal.EnJenisJurnal);
            PopulateListViewFromCart();
            return View(akJurnal);
        }

        private bool AkJurnalExist(int id)
        {
            return _unitOfWork.AkJurnalRepo.IsExist(b => b.Id == id);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "D")]
        public async Task<IActionResult> DeleteConfirmed(int id, string sebabHapus, string syscode)
        {
            var akJurnal = _unitOfWork.AkJurnalRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akJurnal != null && await _unitOfWork.AkJurnalRepo.IsSahAsync(id) == false)
            {
                akJurnal.UserIdKemaskini = user?.UserName ?? "";
                akJurnal.TarKemaskini = DateTime.Now;
                akJurnal.DPekerjaKemaskiniId = pekerjaId;
                akJurnal.SebabHapus = sebabHapus;

                _context.AkJurnal.Remove(akJurnal);
                _appLog.Insert("Hapus", akJurnal.NoRujukan ?? "", akJurnal.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var obj = await _context.AkJurnal.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.AkJurnal.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.NoRujukan ?? "", obj.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
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


                    // get akJurnal 
                    var akJurnal = _unitOfWork.AkJurnalRepo.GetDetailsById((int)id);
                    if (akJurnal == null)
                    {
                        TempData[SD.Error] = "Data tidak wujud.";
                    }
                    else
                    {

                        if (akJurnal.NoRujukan != null)
                        {
                            // check is it posted or not
                            if (await _unitOfWork.AkJurnalRepo.IsPostedAsync((int)id, akJurnal.NoRujukan) == false)
                            {
                                TempData[SD.Error] = "Data belum diposting.";
                                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                            }

                            // posting start here
                            _unitOfWork.AkJurnalRepo.RemovePostingFromAkAkaun(akJurnal);

                            //insert applog
                            _appLog.Insert("UnPosting", "UnPosting Data", akJurnal.NoRujukan, (int)id, akJurnal.JumlahDebit, pekerjaId, modul, syscode, namamodul, user);

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

        // jsonResults
        public JsonResult EmptyCart()
        {
            try
            {
                _cart.ClearObjek();
                _cart.ClearPenerimaCekBatal();

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public async Task<JsonResult> SaveCartAkJurnalObjek(AkJurnalObjek akJurnalObjek)
        {
            try
            {
                if (akJurnalObjek != null)
                {
                    if (akJurnalObjek.JKWPTJBahagianDebitId != 0 && akJurnalObjek.JKWPTJBahagianKreditId != 0)
                    {
                        var jkwptjbahagianDebit = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(akJurnalObjek.JKWPTJBahagianDebitId);
                        var jkwptjbahagianKredit = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(akJurnalObjek.JKWPTJBahagianKreditId);
                        if (akJurnalObjek.IsDebitAbBukuVot == false)
                        {
                            if (await _abBukuVotRepo.IsExistByJKWPTJBahagianAkCartaId(jkwptjbahagianDebit.JKWId, jkwptjbahagianDebit.JPTJId, jkwptjbahagianDebit.JBahagianId, akJurnalObjek.AkCartaDebitId))
                            {
                                akJurnalObjek.IsDebitAbBukuVot = true;

                            }
                        }
                        else
                        {
                            if (!await _abBukuVotRepo.IsExistByJKWPTJBahagianAkCartaId(jkwptjbahagianDebit.JKWId, jkwptjbahagianDebit.JPTJId, jkwptjbahagianDebit.JBahagianId, akJurnalObjek.AkCartaDebitId))
                            {
                                return Json(new { result = "ERROR", message = "Sila isi dahulu maklumat buku vot bagi bahagian dan kod akaun debit ini sebelum meneruskan operasi" });

                            }
                        }

                        if (akJurnalObjek.IsKreditAbBukuVot == false)
                        {
                            if (await _abBukuVotRepo.IsExistByJKWPTJBahagianAkCartaId(jkwptjbahagianKredit.JKWId, jkwptjbahagianKredit.JPTJId, jkwptjbahagianKredit.JBahagianId, akJurnalObjek.AkCartaKreditId))
                            {
                                akJurnalObjek.IsKreditAbBukuVot = true;

                            }
                        }
                        else
                        {
                            if (!await _abBukuVotRepo.IsExistByJKWPTJBahagianAkCartaId(jkwptjbahagianKredit.JKWId, jkwptjbahagianKredit.JPTJId, jkwptjbahagianKredit.JBahagianId, akJurnalObjek.AkCartaKreditId))
                            {
                                return Json(new { result = "ERROR", message = "Sila isi dahulu maklumat buku vot bagi bahagian dan kod akaun kredit ini sebelum meneruskan operasi" });

                            }
                        }

                        _cart.AddItemObjek(akJurnalObjek.AkJurnalId, akJurnalObjek.JKWPTJBahagianDebitId, akJurnalObjek.AkCartaDebitId, akJurnalObjek.JKWPTJBahagianKreditId, akJurnalObjek.AkCartaKreditId, akJurnalObjek.Amaun, akJurnalObjek.IsDebitAbBukuVot, akJurnalObjek.IsKreditAbBukuVot);

                        return Json(new { result = "OK" });
                    }
                    else
                    {
                        return Json(new { result = "ERROR", message = "sila pilih Bahagian debit dan bahagian kredit" });
                    }
                }
                else
                {
                    return Json(new { result = "ERROR", message = "{object} null" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkJurnalObjek(AkJurnalObjek akJurnalObjek)
        {
            try
            {
                if (akJurnalObjek != null)
                {
                    _cart.RemoveItemObjek(akJurnalObjek.JKWPTJBahagianDebitId, akJurnalObjek.AkCartaDebitId, akJurnalObjek.JKWPTJBahagianKreditId, akJurnalObjek.AkCartaKreditId);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkJurnalObjek(AkJurnalObjek akJurnalObjek)
        {

            try
            {
                AkJurnalObjek data = _cart.AkJurnalObjek.FirstOrDefault(x => x.JKWPTJBahagianDebitId == akJurnalObjek.JKWPTJBahagianDebitId && x.AkCartaDebitId == akJurnalObjek.AkCartaDebitId &&
                x.JKWPTJBahagianKreditId == akJurnalObjek.JKWPTJBahagianKreditId && x.AkCartaKreditId == akJurnalObjek.AkCartaKreditId) ?? new AkJurnalObjek();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public async Task<JsonResult> SaveAnItemFromCartAkJurnalObjek(AkJurnalObjek akJurnalObjek)
        {

            try
            {

                var data = _cart.AkJurnalObjek.FirstOrDefault(x => x.JKWPTJBahagianDebitId == akJurnalObjek.JKWPTJBahagianDebitId && x.AkCartaDebitId == akJurnalObjek.AkCartaDebitId &&
                x.JKWPTJBahagianKreditId == akJurnalObjek.JKWPTJBahagianKreditId && x.AkCartaKreditId == akJurnalObjek.AkCartaKreditId);

                var user = _userManager.GetUserName(User);

                if (data != null)
                {
                    var jkwptjbahagianDebit = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(akJurnalObjek.JKWPTJBahagianDebitId);
                    var jkwptjbahagianKredit = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(akJurnalObjek.JKWPTJBahagianKreditId);
                    if (akJurnalObjek.IsDebitAbBukuVot == false)
                    {
                        if (await _abBukuVotRepo.IsExistByJKWPTJBahagianAkCartaId(jkwptjbahagianDebit.JKWId, jkwptjbahagianDebit.JPTJId, jkwptjbahagianDebit.JBahagianId, akJurnalObjek.AkCartaDebitId))
                        {
                            akJurnalObjek.IsDebitAbBukuVot = true;

                        }
                    }
                    else
                    {
                        if (!await _abBukuVotRepo.IsExistByJKWPTJBahagianAkCartaId(jkwptjbahagianDebit.JKWId, jkwptjbahagianDebit.JPTJId, jkwptjbahagianDebit.JBahagianId, akJurnalObjek.AkCartaDebitId))
                        {
                            return Json(new { result = "ERROR", message = "Sila isi dahulu maklumat buku vot bagi bahagian dan kod akaun debit ini sebelum meneruskan operasi" });

                        }
                    }

                    if (akJurnalObjek.IsKreditAbBukuVot == false)
                    {
                        if (await _abBukuVotRepo.IsExistByJKWPTJBahagianAkCartaId(jkwptjbahagianKredit.JKWId, jkwptjbahagianKredit.JPTJId, jkwptjbahagianKredit.JBahagianId, akJurnalObjek.AkCartaKreditId))
                        {
                            akJurnalObjek.IsKreditAbBukuVot = true;

                        }
                    }
                    else
                    {
                        if (!await _abBukuVotRepo.IsExistByJKWPTJBahagianAkCartaId(jkwptjbahagianKredit.JKWId, jkwptjbahagianKredit.JPTJId, jkwptjbahagianKredit.JBahagianId, akJurnalObjek.AkCartaKreditId))
                        {
                            return Json(new { result = "ERROR", message = "Sila isi dahulu maklumat buku vot bagi bahagian dan kod akaun kredit ini sebelum meneruskan operasi" });

                        }
                    }

                    _cart.RemoveItemObjek(akJurnalObjek.JKWPTJBahagianDebitId, akJurnalObjek.AkCartaDebitId, akJurnalObjek.JKWPTJBahagianKreditId, akJurnalObjek.AkCartaKreditId);


                    _cart.AddItemObjek(akJurnalObjek.AkJurnalId, akJurnalObjek.JKWPTJBahagianDebitId, akJurnalObjek.AkCartaDebitId, akJurnalObjek.JKWPTJBahagianKreditId, akJurnalObjek.AkCartaKreditId, akJurnalObjek.Amaun, akJurnalObjek.IsDebitAbBukuVot, akJurnalObjek.IsKreditAbBukuVot);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        // penerima cek batal
        public JsonResult SaveCartAkJurnalPenerimaCekBatal(AkJurnalPenerimaCekBatal akJurnalPenerimaCekBatal)
        {
            try
            {
                if (akJurnalPenerimaCekBatal != null)
                {
                    
                    if (_cart.AkJurnalPenerimaCekBatal.Any(pcb => pcb.Bil == akJurnalPenerimaCekBatal.Bil && pcb.AkPVId == akJurnalPenerimaCekBatal.AkPVId))
                        return Json(new { result = "Error", message = "penerima telah wujud" });

                    _cart.AddItemPenerimaCekBatal(akJurnalPenerimaCekBatal.AkJurnalId, akJurnalPenerimaCekBatal.Bil, akJurnalPenerimaCekBatal.AkPVId, akJurnalPenerimaCekBatal.NamaPenerima, akJurnalPenerimaCekBatal.NoCek, akJurnalPenerimaCekBatal.Amaun);

                }

                return Json(new { result = "OK" });
            } catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkJurnalPenerimaCekBatal(AkJurnalPenerimaCekBatal akJurnalPenerimaCekBatal)
        {
            try
            {
                _cart.RemoveItemPenerimaCekBatal(akJurnalPenerimaCekBatal.Bil, akJurnalPenerimaCekBatal.AkPVId);
                return Json(new { result = "OK" });
            } catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkJurnalPenerimaCekBatal(AkJurnalPenerimaCekBatal akJurnalPenerimaCekBatal)
        {

            try
            {
                AkJurnalPenerimaCekBatal data = _cart.AkJurnalPenerimaCekBatal.FirstOrDefault(x => x.Bil == akJurnalPenerimaCekBatal.Bil) ?? new AkJurnalPenerimaCekBatal();

                // get id for akPVPenerima by bil and akPVId
                var akPVPenerima = _context.AkPVPenerima.FirstOrDefault(pp => pp.Bil == data.Bil && pp.AkPVId == data.AkPVId);

                if (akPVPenerima == null) return Json(new {result = "Error", message = "data ralat" });

                return Json(new { result = "OK", record = data, akPVPenerima });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkJurnalPenerimaCekBatal(AkJurnalPenerimaCekBatal akJurnalPenerimaCekBatal)
        {
            try
            {
                AkJurnalPenerimaCekBatal data = _cart.AkJurnalPenerimaCekBatal.FirstOrDefault(x => x.Bil == akJurnalPenerimaCekBatal.Bil) ?? new AkJurnalPenerimaCekBatal();

                if (data != null)
                {
                    _cart.RemoveItemPenerimaCekBatal(akJurnalPenerimaCekBatal.Bil, akJurnalPenerimaCekBatal.AkPVId);
                    var akPVPenerima = _context.AkPVPenerima.FirstOrDefault(x => x.Bil ==  data.Bil && x.AkPVId == akJurnalPenerimaCekBatal.AkPVId);

                    if (akPVPenerima != null)
                    {
                        _cart.AddItemPenerimaCekBatal(data.AkJurnalId, data.Bil, akJurnalPenerimaCekBatal.AkPVId, akPVPenerima.NamaPenerima, akPVPenerima.NoRujukanCaraBayar, akPVPenerima.Amaun);

                        return Json(new { result = "OK" });
                    }
                    else
                    {
                        return Json(new { result = "ERROR", message = "No Baucer tidak wujud" });
                    }
                    
                }
                else
                {
                    return Json(new { result = "ERROR", message = "data tidak wujud" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });

            }
        }
        //

        public JsonResult GetAkPVPenerima(int Id)
        {
            try
            {
                var akPVPenerima = _context.AkPVPenerima.FirstOrDefault(pp => pp.Id == Id);
                if (akPVPenerima == null)
                {
                    return Json(new { result = "Error", message = "penerima tidak wujud" });
                }

                return Json(new { result = "OK", akPVPenerima });
            }
            catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }

        }
        public JsonResult GetAllItemCartAkJurnal()
        {

            try
            {

                List<AkJurnalObjek> objek = _cart.AkJurnalObjek.ToList();

                foreach (AkJurnalObjek item in objek)
                {
                    var jkwPtjBahagianDebit = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianDebitId);

                    item.JKWPTJBahagianDebit = jkwPtjBahagianDebit;

                    item.JKWPTJBahagianDebit.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagianDebit.JKW?.Kod, jkwPtjBahagianDebit.JPTJ?.Kod, jkwPtjBahagianDebit.JBahagian?.Kod);

                    var akCartaDebit = _unitOfWork.AkCartaRepo.GetById(item.AkCartaDebitId);

                    item.AkCartaDebit = akCartaDebit;

                    var jkwPtjBahagianKredit = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianKreditId);

                    item.JKWPTJBahagianKredit = jkwPtjBahagianKredit;

                    item.JKWPTJBahagianKredit.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagianKredit.JKW?.Kod, jkwPtjBahagianKredit.JPTJ?.Kod, jkwPtjBahagianKredit.JBahagian?.Kod);

                    var akCartaKredit = _unitOfWork.AkCartaRepo.GetById(item.AkCartaKreditId);

                    item.AkCartaKredit = akCartaKredit;
                }


                List<AkJurnalPenerimaCekBatal> penerimaCekBatal = _cart.AkJurnalPenerimaCekBatal.ToList();

                foreach (AkJurnalPenerimaCekBatal item in penerimaCekBatal)
                {
                    var akPV = _unitOfWork.AkPVRepo.GetById(item.AkPVId);
                    item.AkPV = akPV;
                }


                return Json(new { result = "OK", objek = objek.OrderBy(d => d.JKWPTJBahagianDebit?.JBahagian?.Kod), penerimaCekBatal = penerimaCekBatal.OrderBy(p => p.NamaPenerima) });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }
    }
}
