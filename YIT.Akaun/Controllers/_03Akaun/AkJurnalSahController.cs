using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services.Cart;
using YIT._DataAccess.Services.Math;
using YIT.Akaun.Infrastructure;

namespace YIT.Akaun.Controllers._03Akaun
{
    [Authorize(Roles = Init.superAdminSupervisorRole)]
    public class AkJurnalSahController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodSahAkJurnal;
        public const string namamodul = Modules.namaSahAkJurnal;
        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;
        private readonly CartAkJurnal _cart;

        public AkJurnalSahController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices,
            CartAkJurnal cart)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _appLog = appLog;
            _userServices = userServices;
            _cart = cart;
        }
        public IActionResult Index(string searchString,
            string searchDate1,
            string searchDate2,
            string searchColumn,
            int? dKonfigKelulusanId,
            string? password,
            int isInSession)
        {
            // load data
            DateTime date1 = DateTime.Now;
            DateTime date2 = DateTime.Now;

            List<AkJurnal> akJurnal = new List<AkJurnal>();
            if (!string.IsNullOrEmpty(searchDate1) && !string.IsNullOrEmpty(searchDate2))
            {
                date1 = DateTime.Parse(searchDate1);
                date2 = DateTime.Parse(searchDate2);
            }

            if (isInSession == 1)
            {
                if (dKonfigKelulusanId == null) dKonfigKelulusanId = HttpContext.Session.GetInt32("dKonfigKelulusanId") ?? null;
                if (password == null) password = HttpContext.Session.GetString("password") ?? null;
            }
            else
            {
                HttpContext.Session.Clear();
            }

            if (dKonfigKelulusanId != null && !string.IsNullOrEmpty(password))
            {
                // cek is user and password valid or not
                HttpContext.Session.SetInt32("dKonfigKelulusanId", (int)dKonfigKelulusanId);
                HttpContext.Session.SetString("password", password ?? "");
                HttpContext.Session.SetString("searchString", searchString ?? "");
                HttpContext.Session.SetString("searchDate1", date1.ToString("yyyy-MM-dd"));
                HttpContext.Session.SetString("searchDate2", date2.ToString("yyyy-MM-dd"));

                if (_unitOfWork.DKonfigKelulusanRepo.IsValidUser((int)dKonfigKelulusanId, password ?? "", EnJenisModulKelulusan.Jurnal, EnKategoriKelulusan.Pengesah) == false)
                {
                    TempData[SD.Error] = "Katalaluan Tidak Sah";
                }
                else
                {

                    akJurnal = _unitOfWork.AkJurnalRepo.GetResultsByDPekerjaIdFromDKonfigKelulusan(searchString, date1, date2, searchColumn, EnStatusBorang.None, (int)dKonfigKelulusanId, EnKategoriKelulusan.Pengesah, EnJenisModulKelulusan.Jurnal);

                }
            }

            PopulateFormFields(dKonfigKelulusanId, searchString ?? "", password ?? "", searchDate1, searchDate2, isInSession);

            return View(akJurnal);
        }

        private void PopulateFormFields(int? dKonfigKelulusanId, string searchString, string password, string searchDate1, string searchDate2, int isInSession)
        {
            ViewBag.isInSession = isInSession;
            ViewBag.dKonfigKelulusanId = HttpContext.Session.GetInt32("dKonfigKelulusanId") ?? dKonfigKelulusanId;
            ViewBag.searchString = HttpContext.Session.GetString("searchString") ?? searchString;
            ViewBag.password = HttpContext.Session.GetString("password") ?? password;
            ViewBag.searchDate1 = HttpContext.Session.GetString("searchDate1") ?? DateTime.Now.ToString("yyyy-MM-dd");
            ViewBag.searchDate2 = HttpContext.Session.GetString("searchDate2") ?? DateTime.Now.ToString("yyyy-MM-dd");
            ViewBag.DKonfigKelulusan = _unitOfWork.DKonfigKelulusanRepo.GetResultsByCategoryGroupByDPekerja(EnKategoriKelulusan.Pengesah, EnJenisModulKelulusan.Jurnal);
        }

        [Authorize(Policy = modul + "S")]
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
            ViewBag.dKonfigKelulusanId = HttpContext.Session.GetInt32("dKonfigKelulusanId");
            ManipulateHiddenDiv(akJurnal.EnJenisJurnal);
            PopulateCartAkJurnalFromDb(akJurnal);
            return View(akJurnal);
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

            foreach (AkJurnalPenerimaCekBatal item in penerimaCekBatal)
            {
                var akPV = _unitOfWork.AkPVRepo.GetById(item.AkPVId);
                item.AkPV = akPV;
            }

            ViewBag.akJurnalPenerimaCekBatal = penerimaCekBatal;
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

        [Authorize(Policy = modul + "S")]
        public async Task<IActionResult> Sah(int id,int dKonfigKelulusanId, string syscode)
        {
            var akJurnal = _unitOfWork.AkJurnalRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akJurnal != null)
            {

                _unitOfWork.AkJurnalRepo.Sah(id, dKonfigKelulusanId, user?.UserName ?? "");
                
                _appLog.Insert("Posting", "Mengesah " + akJurnal.NoRujukan ?? "" + "; pengesahId: " + dKonfigKelulusanId.ToString(), akJurnal.NoRujukan ?? "", id, akJurnal.JumlahDebit, pekerjaId, modul, syscode, namamodul, user);
                _context.SaveChanges();
                TempData[SD.Success] = "Data berjaya disahkan..!";
            }
            else
            {
                TempData[SD.Error] = "Data tidak wujud.";
            }

            return RedirectToAction("Index", new
            {
                searchString = HttpContext.Session.GetString("searchString") ?? "",
                searchDate1 = HttpContext.Session.GetString("searchDate1") ?? DateTime.Now.ToString("yyyy-MM-dd"),
                searchDate2 = HttpContext.Session.GetString("searchDate2") ?? DateTime.Now.ToString("yyyy-MM-dd"),
                isInSession = 1

            });
        }

        [Authorize(Policy = modul + "E")]
        public async Task<IActionResult> HantarSemula(int id, string? tindakan, string syscode)
        {
            var akJurnal = _unitOfWork.AkJurnalRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akJurnal != null)
            {
                _unitOfWork.AkJurnalRepo.HantarSemula(id, tindakan, user?.UserName ?? "");

                _appLog.Insert("Ubah", "Hantar Semula " + akJurnal.NoRujukan ?? "", akJurnal.NoRujukan ?? "", id, akJurnal.JumlahDebit, pekerjaId, modul, syscode, namamodul, user);
                _context.SaveChanges();
                TempData[SD.Success] = "Data berjaya dihantar semula..!";
            }
            else
            {
                TempData[SD.Error] = "Data tidak wujud.";
            }

            return RedirectToAction("Index", new
            {
                searchString = HttpContext.Session.GetString("searchString") ?? "",
                searchDate1 = HttpContext.Session.GetString("searchDate1") ?? DateTime.Now.ToString("yyyy-MM-dd"),
                searchDate2 = HttpContext.Session.GetString("searchDate2") ?? DateTime.Now.ToString("yyyy-MM-dd"),
                isInSession = 1

            });
        }
    }
}
