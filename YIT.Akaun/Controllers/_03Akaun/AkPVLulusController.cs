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
    [Authorize]
    public class AkPVLulusController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodLulusAkPV;
        public const string namamodul = Modules.namaLulusAkPV;
        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;
        private readonly CartAkPV _cart;

        public AkPVLulusController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices,
            CartAkPV cart
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
                string searchColumn,
                int? dKonfigKelulusanId,
                string password)
        {
            // load data
            DateTime? date1 = null;
            DateTime? date2 = null;

            PopulateFormFields(searchString, password, searchDate1, searchDate2);

            List<AkPV> akPV = new List<AkPV>();
            if (!string.IsNullOrEmpty(searchDate1) && !string.IsNullOrEmpty(searchDate2))
            {
                date1 = DateTime.Parse(searchDate1);
                date2 = DateTime.Parse(searchDate2);
            }

            if (dKonfigKelulusanId != null)
            {
                // cek is user and password valid or not
                HttpContext.Session.SetInt32("DPelulusId", (int)dKonfigKelulusanId);

                if (_unitOfWork.DKonfigKelulusanRepo.IsValidUser((int)dKonfigKelulusanId, password, EnJenisModulKelulusan.PV, EnKategoriKelulusan.Pelulus) == false)
                {
                    TempData[SD.Error] = "Katalaluan Tidak Sah";
                    return View();
                }
                else
                {

                    akPV = _unitOfWork.AkPVRepo.GetResultsByDPekerjaIdFromDKonfigKelulusan(searchString, date1, date2, searchColumn, EnStatusBorang.None, (int)dKonfigKelulusanId, EnKategoriKelulusan.Pelulus, EnJenisModulKelulusan.PV);


                }
            }

            return View(akPV);
        }

        private void PopulateFormFields(string searchString, string password, string searchDate1, string searchDate2)
        {
            ViewBag.searchString = searchString;
            ViewBag.password = password;
            ViewBag.searchDate1 = searchDate1 ?? DateTime.Now.ToString("dd/MM/yyyy");
            ViewBag.searchDate2 = searchDate2 ?? DateTime.Now.ToString("dd/MM/yyyy");
            ViewBag.DKonfigKelulusan = _unitOfWork.DKonfigKelulusanRepo.GetResultsByCategoryGroupByDPekerja(EnKategoriKelulusan.Pelulus, EnJenisModulKelulusan.PV);
        }

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
            ViewBag.DKonfigKelulusanId = HttpContext.Session.GetInt32("DPelulusId");
            PopulateCartAkPVFromDb(akPV);
            return View(akPV);
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
                                          item.AkEFTPenerimaId,
                                          item.JCaraBayarId,
                                          item.JBankId,
                                          item.NoAkaunBank,
                                          item.Alamat1,
                                          item.Alamat2,
                                          item.Alamat3,
                                          item.Emel,
                                          item.KodM2E,
                                          item.NoCek,
                                          item.TarikhCek,
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

        public async Task<IActionResult> Lulus(int id, int dKonfigKelulusanId, string syscode)
        {
            var akPV = _unitOfWork.AkPVRepo.GetDetailsById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akPV != null)
            {
                decimal jumlahPenerima = 0;
                if (akPV.AkPVPenerima != null && akPV.AkPVPenerima.Count > 0)
                {
                    foreach (var item in akPV.AkPVPenerima)
                    {
                        jumlahPenerima += item.Amaun;
                    }

                    if (jumlahPenerima != akPV.Jumlah)
                    {
                        TempData[SD.Error] = "Jumlah RM tidak sama dengan Jumlah Penerima RM";
                        return RedirectToAction(nameof(Details),new { id });
                    }
                }

                _unitOfWork.AkPVRepo.Lulus(id, dKonfigKelulusanId, user?.UserName ?? "");

                _appLog.Insert("Posting", "Melulus " + akPV.NoRujukan ?? "" + "; pelulusId: " + dKonfigKelulusanId.ToString(), akPV.NoRujukan ?? "", id, akPV.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                _context.SaveChanges();
                TempData[SD.Success] = "Data berjaya diluluskan..!";
            }
            else
            {
                TempData[SD.Error] = "Data tidak wujud.";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> HantarSemulaAsync(int id, string? tindakan, string syscode)
        {
            var akPV = _unitOfWork.AkPVRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akPV != null)
            {
                _unitOfWork.AkPVRepo.HantarSemula(id, tindakan, user?.UserName ?? "");

                _appLog.Insert("Ubah", "Hantar Semula " + akPV.NoRujukan ?? "", akPV.NoRujukan ?? "", id, akPV.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                _context.SaveChanges();
                TempData[SD.Success] = "Data berjaya dihantar semula..!";
            }
            else
            {
                TempData[SD.Error] = "Data tidak wujud.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
