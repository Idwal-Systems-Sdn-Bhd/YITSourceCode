using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

namespace YIT.Akaun.Controllers._03Akaun
{
    [Authorize]
    public class AkJanaanProfilController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodAkJanaanProfil;
        public const string namamodul = Modules.namaAkJanaanProfil;

        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;
        private readonly CartAkJanaanProfil _cart;

        public AkJanaanProfilController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices,
            CartAkJanaanProfil cart)
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

            var akJanaanProfil = _unitOfWork.AkJanaanProfilRepo.GetResults(searchString, date1, date2, searchColumn, EnStatusBorang.Semua);

            return View(akJanaanProfil);
        }

        private void PopulateFormFields(string searchString, string searchDate1, string searchDate2)
        {
            ViewBag.searchString = searchString;
            ViewBag.searchDate1 = searchDate1 ?? DateTime.Now.ToString("dd/MM/yyyy");
            ViewBag.searchDate2 = searchDate2 ?? DateTime.Now.ToString("dd/MM/yyyy");
        }

        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akJanaanProfil = _unitOfWork.AkJanaanProfilRepo.GetDetailsById((int)id);
            if (akJanaanProfil == null)
            {
                return NotFound();
            }
            EmptyCart();
            PopulateCartAkJanaanProfilFromDb(akJanaanProfil);
            return View(akJanaanProfil);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akJanaanProfil = _unitOfWork.AkJanaanProfilRepo.GetDetailsById((int)id);
            if (akJanaanProfil == null)
            {
                return NotFound();
            }

            if (_unitOfWork.AkPVRepo.HaveAkJanaanProfil((int)id))
            {
                TempData[SD.Error] = "Hapus data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index)));
            }
            EmptyCart();
            PopulateCartAkJanaanProfilFromDb(akJanaanProfil);
            return View(akJanaanProfil);
        }

        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);

            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.JP.GetDisplayName(), DateTime.Now.ToString("yyyy"));
            EmptyCart();
            PopulateDropDownList();
            return View();
        }

        private void PopulateDropDownList()
        {
            ViewBag.DDaftarAwam = _unitOfWork.DDaftarAwamRepo.GetAllDetailsByKategori(EnKategoriDaftarAwam.Pembekal);
            ViewBag.DPekerja = _unitOfWork.DPekerjaRepo.GetAllDetails();
            ViewBag.JCaraBayar = _unitOfWork.JCaraBayarRepo.GetAll();
            ViewBag.JBank = _unitOfWork.JBankRepo.GetAll();
            ViewBag.JCawangan = _unitOfWork.JCawanganRepo.GetAll();
            var kategoriDaftarAwam = EnumHelper<EnKategoriDaftarAwam>.GetList();

            ViewBag.EnKategoriDaftarAwam = kategoriDaftarAwam;
            var jenisModulProfil = EnumHelper<EnJenisModulProfil>.GetList();

            ViewBag.EnJenisModulProfil = jenisModulProfil;

            var jenisId = EnumHelper<EnJenisId>.GetList();
            ViewBag.EnJenisId = jenisId;
        }

        private string GenerateRunningNumber(string initNoRujukan, string tahun)
        {
            var maxRefNo = _unitOfWork.AkJanaanProfilRepo.GetMaxRefNo(initNoRujukan, tahun);

            var prefix = initNoRujukan + "/" + tahun + "/";
            return RunningNumberFormatter.GenerateRunningNumber(prefix, maxRefNo, "00000");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AkJanaanProfil akJanaanProfil, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            // double check if every penerima have jCaraBayarId or not
            if (_cart.AkJanaanProfilPenerima != null && _cart.AkJanaanProfilPenerima.Count() > 0)
            {

                if (_cart.AkJanaanProfilPenerima.Count() > 1) 

                foreach (var item in _cart.AkJanaanProfilPenerima)
                {
                    if (item.JCaraBayarId == 0)
                    {
                        TempData[SD.Error] = "Terdapat penerima yang tiada pilihan cara bayar.";
                        ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.JP.GetDisplayName(), akJanaanProfil.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
                        PopulateDropDownList();
                        PopulateListViewFromCart();
                        return View(akJanaanProfil);
                    }
                }
            }
            // 

            if (ModelState.IsValid)
            {

                akJanaanProfil.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.JP.GetDisplayName(), akJanaanProfil.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));

                akJanaanProfil.UserId = user?.UserName ?? "";
                akJanaanProfil.TarMasuk = DateTime.Now;
                akJanaanProfil.DPekerjaMasukId = pekerjaId;

                akJanaanProfil.AkJanaanProfilPenerima = _cart.AkJanaanProfilPenerima?.ToList();

                _context.Add(akJanaanProfil);
                _appLog.Insert("Tambah", akJanaanProfil.NoRujukan ?? "", akJanaanProfil.NoRujukan ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya ditambah..!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.JP.GetDisplayName(), akJanaanProfil.Tarikh.ToString() ?? DateTime.Now.ToString("yyyy"));
            PopulateDropDownList();
            PopulateListViewFromCart();
            return View(akJanaanProfil);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akJanaanProfil = _unitOfWork.AkJanaanProfilRepo.GetDetailsById((int)id);
            if (akJanaanProfil == null)
            {
                return NotFound();
            }

            if (_unitOfWork.AkPVRepo.HaveAkJanaanProfil((int)id))
            {
                if (_context.AkPV.Any(pv => pv.AkJanaanProfilId == (int)id && (pv.EnStatusBorang != EnStatusBorang.None || pv.EnStatusBorang != EnStatusBorang.Kemaskini)))
                {
                    TempData[SD.Error] = "Ubah data tidak dibenarkan..!";
                    return (RedirectToAction(nameof(Index)));
                }
            }

            EmptyCart();
            PopulateDropDownList();
            PopulateCartAkJanaanProfilFromDb(akJanaanProfil);
            return View(akJanaanProfil);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AkJanaanProfil akJanaanProfil, string? fullName, string syscode)
        {
            if (id != akJanaanProfil.Id)
            {
                return NotFound();
            }

            if (akJanaanProfil.NoRujukan != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = _unitOfWork.AkJanaanProfilRepo.GetDetailsById(id);
                    var jumlahAsal = objAsal!.Jumlah;
                    akJanaanProfil.NoRujukan = objAsal.NoRujukan;
                    akJanaanProfil.UserId = objAsal.UserId;
                    akJanaanProfil.TarMasuk = objAsal.TarMasuk;
                    akJanaanProfil.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                    akJanaanProfil.UserIdKemaskini = user?.UserName ?? "";
                    akJanaanProfil.TarKemaskini = DateTime.Now;
                    akJanaanProfil.AkJanaanProfilPenerima = _cart.AkJanaanProfilPenerima.ToList();
                    int akPVId = 0;

                    if (!_unitOfWork.AkPVRepo.HaveAkJanaanProfil((int)id))
                    {
                        if(objAsal.AkJanaanProfilPenerima != null && objAsal.AkJanaanProfilPenerima.Count > 0)
                        {
                            foreach (var item in objAsal.AkJanaanProfilPenerima)
                            {
                                var model = _context.AkJanaanProfilPenerima.FirstOrDefault(b => b.Id == item.Id);
                                if (model != null) _context.Remove(model);
                            }
                        }
                    }
                    else
                    {
                        if (_context.AkPV.Any(pv => pv.AkJanaanProfilId == (int)id && (pv.EnStatusBorang == EnStatusBorang.None || pv.EnStatusBorang == EnStatusBorang.Kemaskini)))
                        {
                            if (objAsal.AkJanaanProfilPenerima != null && objAsal.AkJanaanProfilPenerima.Count > 0)
                            {
                                foreach (var item in objAsal.AkJanaanProfilPenerima)
                                {
                                    var model = _context.AkJanaanProfilPenerima.FirstOrDefault(b => b.Id == item.Id);
                                    if (model != null)
                                    {
                                        var akPVPenerima = _context.AkPVPenerima.FirstOrDefault(pp => pp.AkJanaanProfilPenerimaId == item.Id);

                                        if (akPVPenerima != null)
                                        {
                                            akPVId = akPVPenerima.AkPVId;
                                            _context.Remove(akPVPenerima);

                                        }

                                        _context.Remove(model);
                                    }
                                }
                            }
                        }
                        else
                        {
                            TempData[SD.Error] = "Data telah diluluskan";
                            PopulateDropDownList();
                            PopulateListViewFromCart();
                            return View(akJanaanProfil);
                        }
                        
                    }

                    if (_cart.AkJanaanProfilPenerima != null && akPVId != 0)
                    {
                        int bil = 1;
                        foreach (var akJanaanProfilPenerima in _cart.AkJanaanProfilPenerima)
                        {

                            _context.Add(new AkPVPenerima
                            {
                                AkPVId = akPVId,
                                AkJanaanProfilPenerimaId = akJanaanProfilPenerima.Id,
                                EnKategoriDaftarAwam = akJanaanProfilPenerima.EnKategoriDaftarAwam,
                                DDaftarAwamId = akJanaanProfilPenerima.DDaftarAwamId,
                                DPekerjaId = akJanaanProfilPenerima.DPekerjaId,
                                NoPendaftaranPenerima = akJanaanProfilPenerima.NoPendaftaranPenerima,
                                NamaPenerima = akJanaanProfilPenerima.NamaPenerima,
                                NoPendaftaranPemohon = akJanaanProfilPenerima.NoPendaftaranPemohon,
                                JCaraBayarId = akJanaanProfilPenerima.JCaraBayarId,
                                JBankId = akJanaanProfilPenerima.JBankId,
                                NoAkaunBank = akJanaanProfilPenerima.NoAkaunBank,
                                Alamat1 = akJanaanProfilPenerima.Alamat1,
                                Alamat2 = akJanaanProfilPenerima.Alamat2,
                                Alamat3 = akJanaanProfilPenerima.Alamat3,
                                Emel = akJanaanProfilPenerima.Emel,
                                KodM2E = akJanaanProfilPenerima.KodM2E,
                                Amaun = akJanaanProfilPenerima.Amaun,
                                NoRujukanMohon = akJanaanProfilPenerima.NoRujukanMohon,
                                AkRekupId = akJanaanProfilPenerima.AkRekupId,
                                Bil = bil
                            });
                            bil++;
                        }
                    }

                    _context.Entry(objAsal).State = EntityState.Detached;

                    akJanaanProfil.AkJanaanProfilPenerima = _cart.AkJanaanProfilPenerima?.ToList();

                    _unitOfWork.AkJanaanProfilRepo.Update(akJanaanProfil);

                    if (jumlahAsal != akJanaanProfil.Jumlah)
                    {
                        _appLog.Insert("Ubah", Convert.ToDecimal(jumlahAsal).ToString("#,##0.00") + " -> " + Convert.ToDecimal(akJanaanProfil.Jumlah).ToString("#,##0.00") + " : " + akJanaanProfil.NoRujukan ?? "", akJanaanProfil.NoRujukan ?? "", id, akJanaanProfil.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }
                    else
                    {
                        _appLog.Insert("Ubah", akJanaanProfil.NoRujukan ?? "", akJanaanProfil.NoRujukan ?? "", id, akJanaanProfil.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }

                    await _context.SaveChangesAsync();

                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AkJanaanProfilExist(akJanaanProfil.Id))
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
            return View(akJanaanProfil);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string sebabHapus, string syscode)
        {
            var akJanaanProfil = _unitOfWork.AkJanaanProfilRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akJanaanProfil != null && _unitOfWork.AkPVRepo.HaveAkJanaanProfil(id) == false)
            {
                akJanaanProfil.UserIdKemaskini = user?.UserName ?? "";
                akJanaanProfil.TarKemaskini = DateTime.Now;
                akJanaanProfil.DPekerjaKemaskiniId = pekerjaId;
                akJanaanProfil.SebabHapus = sebabHapus;

                _context.AkJanaanProfil.Remove(akJanaanProfil);
                _appLog.Insert("Hapus", akJanaanProfil.NoRujukan ?? "", akJanaanProfil.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var obj = await _context.AkJanaanProfil.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.AkJanaanProfil.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.NoRujukan ?? "", obj.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AkJanaanProfilExist(int id)
        {
            return _unitOfWork.AkJanaanProfilRepo.IsExist(b => b.Id == id);
        }

        private void PopulateCartAkJanaanProfilFromDb(AkJanaanProfil akJanaanProfil)
        {

            if (akJanaanProfil.AkJanaanProfilPenerima != null)
            {
                foreach (var item in akJanaanProfil.AkJanaanProfilPenerima)
                {
                    _cart.AddItemPenerima(item.Id,
                                          item.Bil,
                                          item.AkJanaanProfilId,
                                          item.EnKategoriDaftarAwam,
                                          item.DPenerimaZakatId,
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
                                          item.Amaun,
                                          item.NoRujukanMohon,
                                          item.AkRekupId,
                                          item.EnJenisId);
                }
            }
            PopulateListViewFromCart();
        }

        private void PopulateListViewFromCart()
        {
            List<AkJanaanProfilPenerima> penerima = _cart.AkJanaanProfilPenerima.ToList();

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
            ViewBag.akJanaanProfilPenerima = penerima;
        }

        // jsonResult
        public JsonResult EmptyCart()
        {
            try
            {
                _cart.ClearPenerima();

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetDDaftarAwam(int DDaftarAwamId)
        {
            try
            {
                if (DDaftarAwamId != 0)
                {
                    var data = _unitOfWork.DDaftarAwamRepo.GetAllDetailsById(DDaftarAwamId);

                    if (data != null)
                    {
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

        public JsonResult GetDPekerja(int DPekerjaId)
        {
            try
            {
                if (DPekerjaId != 0)
                {
                    var data = _unitOfWork.DPekerjaRepo.GetAllDetailsById(DPekerjaId);

                    if (data != null)
                    {
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

        public JsonResult SaveCartAkJanaanProfilPenerima(AkJanaanProfilPenerima akJanaanProfilPenerima)
        {
            try
            {
                if (akJanaanProfilPenerima != null)
                {
                    switch (akJanaanProfilPenerima.EnKategoriDaftarAwam)
                    {
                        case EnKategoriDaftarAwam.Pembekal:
                            var dDaftarAwam = _unitOfWork.DDaftarAwamRepo.GetAllDetailsById((int)akJanaanProfilPenerima.DDaftarAwamId!);
                            akJanaanProfilPenerima.NamaPenerima = akJanaanProfilPenerima.NamaPenerima ?? dDaftarAwam.Nama;
                            akJanaanProfilPenerima.NoPendaftaranPenerima = akJanaanProfilPenerima.NoPendaftaranPenerima ?? dDaftarAwam.NoPendaftaran;
                            akJanaanProfilPenerima.NoPendaftaranPemohon = akJanaanProfilPenerima.NoPendaftaranPemohon ?? dDaftarAwam.NoPendaftaran;
                            akJanaanProfilPenerima.Alamat1 = akJanaanProfilPenerima.Alamat1 ?? dDaftarAwam.Alamat1;
                            akJanaanProfilPenerima.Alamat2 = akJanaanProfilPenerima.Alamat2 ?? dDaftarAwam.Alamat2;
                            akJanaanProfilPenerima.Alamat3 = akJanaanProfilPenerima.Alamat3 ?? dDaftarAwam.Alamat3;
                            akJanaanProfilPenerima.JBankId = akJanaanProfilPenerima.JBankId ?? dDaftarAwam.JBankId;
                            akJanaanProfilPenerima.Emel = akJanaanProfilPenerima.Emel ?? dDaftarAwam.Emel;
                            akJanaanProfilPenerima.KodM2E = akJanaanProfilPenerima.KodM2E ?? dDaftarAwam.KodM2E;
                            akJanaanProfilPenerima.NoAkaunBank = akJanaanProfilPenerima.NoAkaunBank ?? dDaftarAwam.NoAkaunBank;
                            akJanaanProfilPenerima.EnJenisId = dDaftarAwam.EnJenisId;
                            break;
                        case EnKategoriDaftarAwam.Pekerja:
                            var dPekerja = _unitOfWork.DPekerjaRepo.GetAllDetailsById((int)akJanaanProfilPenerima.DPekerjaId!);
                            akJanaanProfilPenerima.NamaPenerima = akJanaanProfilPenerima.NamaPenerima ?? dPekerja.Nama;
                            akJanaanProfilPenerima.NoPendaftaranPenerima = akJanaanProfilPenerima.NoPendaftaranPenerima ?? dPekerja.NoKp;
                            akJanaanProfilPenerima.NoPendaftaranPemohon = akJanaanProfilPenerima.NoPendaftaranPemohon ?? dPekerja.NoKp;
                            akJanaanProfilPenerima.Alamat1 = akJanaanProfilPenerima.Alamat1 ?? dPekerja.Alamat1;
                            akJanaanProfilPenerima.Alamat2 = akJanaanProfilPenerima.Alamat2 ?? dPekerja.Alamat2;
                            akJanaanProfilPenerima.Alamat3 = akJanaanProfilPenerima.Alamat3 ?? dPekerja.Alamat3;
                            akJanaanProfilPenerima.JBankId = akJanaanProfilPenerima.JBankId ?? dPekerja.JBankId;
                            akJanaanProfilPenerima.Emel = akJanaanProfilPenerima.Emel ?? dPekerja.Emel;
                            akJanaanProfilPenerima.KodM2E = akJanaanProfilPenerima.KodM2E ?? dPekerja.KodM2E;
                            akJanaanProfilPenerima.NoAkaunBank = akJanaanProfilPenerima.NoAkaunBank ?? dPekerja.NoAkaunBank;
                            akJanaanProfilPenerima.EnJenisId = dPekerja.EnJenisId;
                            break;
                    }
                    if (akJanaanProfilPenerima.JCaraBayarId != 0)
                    {
                        // check if carabayar exist or not
                        var caraBayar = _unitOfWork.JCaraBayarRepo.GetById(akJanaanProfilPenerima.JCaraBayarId);
                        if (caraBayar != null)
                        {

                            // check if carabayar bypass limit or not
                            if (caraBayar.IsLimit == true)
                            {
                                if (akJanaanProfilPenerima.Amaun > caraBayar.MaksAmaun)
                                {
                                    return Json(new { result = "ERROR", message = "Amaun melebihi had mengikut had maksimum cara bayar" });
                                }
                            }
                            int bil = 1;
                            if (_cart.AkJanaanProfilPenerima.Any())
                            {
                                bil = _cart.AkJanaanProfilPenerima.Max(p => p.Bil) ?? 0;
                                bil += 1;
                            }

                            _cart.AddItemPenerima(akJanaanProfilPenerima.Id,
                                                  bil,
                                                  akJanaanProfilPenerima.AkJanaanProfilId,
                                                  akJanaanProfilPenerima.EnKategoriDaftarAwam,
                                                  akJanaanProfilPenerima.DPenerimaZakatId,
                                                  akJanaanProfilPenerima.DDaftarAwamId,
                                                  akJanaanProfilPenerima.DPekerjaId,
                                                  akJanaanProfilPenerima.NoPendaftaranPenerima,
                                                  akJanaanProfilPenerima.NamaPenerima,
                                                  akJanaanProfilPenerima.NoPendaftaranPemohon,
                                                  akJanaanProfilPenerima.Catatan,
                                                  akJanaanProfilPenerima.JCaraBayarId,
                                                  akJanaanProfilPenerima.JBankId,
                                                  akJanaanProfilPenerima.NoAkaunBank,
                                                  akJanaanProfilPenerima.Alamat1,
                                                  akJanaanProfilPenerima.Alamat2,
                                                  akJanaanProfilPenerima.Alamat3,
                                                  akJanaanProfilPenerima.Emel,
                                                  akJanaanProfilPenerima.KodM2E,
                                                  akJanaanProfilPenerima.Amaun,
                                                  akJanaanProfilPenerima.NoRujukanMohon,
                                                  akJanaanProfilPenerima.AkRekupId,
                                                  akJanaanProfilPenerima.EnJenisId);
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

        public JsonResult RemoveCartAkJanaanProfilPenerima(AkJanaanProfilPenerima akJanaanProfilPenerima)
        {
            try
            {
                _cart.RemoveItemPenerima((int)akJanaanProfilPenerima.Bil!);

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkJanaanProfilPenerima(AkJanaanProfilPenerima akJanaanProfilPenerima)
        {

            try
            {
                AkJanaanProfilPenerima data = _cart.AkJanaanProfilPenerima.FirstOrDefault(x => x.Bil == akJanaanProfilPenerima.Bil) ?? new AkJanaanProfilPenerima();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkJanaanProfilPenerima(AkJanaanProfilPenerima akJanaanProfilPenerima)
        {

            try
            {

                var akJanaanProfil = _cart.AkJanaanProfilPenerima.FirstOrDefault(x => x.Bil == akJanaanProfilPenerima.Bil);

                var user = _userManager.GetUserName(User);

                if (akJanaanProfil != null && akJanaanProfilPenerima.Bil != null)
                {
                    switch (akJanaanProfilPenerima.EnKategoriDaftarAwam)
                    {
                        case EnKategoriDaftarAwam.Pembekal:
                            var dDaftarAwam = _unitOfWork.DDaftarAwamRepo.GetAllDetailsById((int)akJanaanProfilPenerima.DDaftarAwamId!);
                            akJanaanProfilPenerima.NamaPenerima = akJanaanProfilPenerima.NamaPenerima ?? dDaftarAwam.Nama;
                            akJanaanProfilPenerima.NoPendaftaranPenerima = akJanaanProfilPenerima.NoPendaftaranPenerima ?? dDaftarAwam.NoPendaftaran;
                            akJanaanProfilPenerima.NoPendaftaranPemohon = akJanaanProfilPenerima.NoPendaftaranPemohon ?? dDaftarAwam.NoPendaftaran;
                            akJanaanProfilPenerima.Alamat1 = akJanaanProfilPenerima.Alamat1 ?? dDaftarAwam.Alamat1;
                            akJanaanProfilPenerima.Alamat2 = akJanaanProfilPenerima.Alamat2 ?? dDaftarAwam.Alamat2;
                            akJanaanProfilPenerima.Alamat3 = akJanaanProfilPenerima.Alamat3 ?? dDaftarAwam.Alamat3;
                            akJanaanProfilPenerima.JBankId = akJanaanProfilPenerima.JBankId ?? dDaftarAwam.JBankId;
                            akJanaanProfilPenerima.Emel = akJanaanProfilPenerima.Emel ?? dDaftarAwam.Emel;
                            akJanaanProfilPenerima.KodM2E = akJanaanProfilPenerima.KodM2E ?? dDaftarAwam.KodM2E;
                            akJanaanProfilPenerima.NoAkaunBank = akJanaanProfilPenerima.NoAkaunBank ?? dDaftarAwam.NoAkaunBank;
                            akJanaanProfilPenerima.EnJenisId = dDaftarAwam.EnJenisId;
                            break;
                        case EnKategoriDaftarAwam.Pekerja:
                            var dPekerja = _unitOfWork.DPekerjaRepo.GetAllDetailsById((int)akJanaanProfilPenerima.DPekerjaId!);
                            akJanaanProfilPenerima.NamaPenerima = akJanaanProfilPenerima.NamaPenerima ?? dPekerja.Nama;
                            akJanaanProfilPenerima.NoPendaftaranPenerima = akJanaanProfilPenerima.NoPendaftaranPenerima ?? dPekerja.NoKp;
                            akJanaanProfilPenerima.NoPendaftaranPemohon = akJanaanProfilPenerima.NoPendaftaranPemohon ?? dPekerja.NoKp;
                            akJanaanProfilPenerima.Alamat1 = akJanaanProfilPenerima.Alamat1 ?? dPekerja.Alamat1;
                            akJanaanProfilPenerima.Alamat2 = akJanaanProfilPenerima.Alamat2 ?? dPekerja.Alamat2;
                            akJanaanProfilPenerima.Alamat3 = akJanaanProfilPenerima.Alamat3 ?? dPekerja.Alamat3;
                            akJanaanProfilPenerima.JBankId = akJanaanProfilPenerima.JBankId ?? dPekerja.JBankId;
                            akJanaanProfilPenerima.Emel = akJanaanProfilPenerima.Emel ?? dPekerja.Emel;
                            akJanaanProfilPenerima.KodM2E = akJanaanProfilPenerima.KodM2E ?? dPekerja.KodM2E;
                            akJanaanProfilPenerima.NoAkaunBank = akJanaanProfilPenerima.NoAkaunBank ?? dPekerja.NoAkaunBank;
                            akJanaanProfilPenerima.EnJenisId = dPekerja.EnJenisId;
                            break;
                    }
                    if (akJanaanProfilPenerima.JCaraBayarId != 0)
                    {
                        // check if carabayar exist or not
                        var caraBayar = _unitOfWork.JCaraBayarRepo.GetById(akJanaanProfilPenerima.JCaraBayarId);
                        if (caraBayar != null)
                        {

                            // check if carabayar bypass limit or not
                            if (caraBayar.IsLimit == true)
                            {
                                if (akJanaanProfilPenerima.Amaun > caraBayar.MaksAmaun)
                                {
                                    return Json(new { result = "ERROR", message = "Amaun melebihi had mengikut had maksimum cara bayar" });
                                }
                            }

                            _cart.UpdateItemPenerima(akJanaanProfilPenerima.Id,
                                                 (int)akJanaanProfilPenerima.Bil,
                                                 akJanaanProfilPenerima.AkJanaanProfilId,
                                                 akJanaanProfilPenerima.EnKategoriDaftarAwam,
                                                 akJanaanProfilPenerima.DPenerimaZakatId,
                                                 akJanaanProfilPenerima.DDaftarAwamId,
                                                 akJanaanProfilPenerima.DPekerjaId,
                                                 akJanaanProfilPenerima.NoPendaftaranPenerima,
                                                 akJanaanProfilPenerima.NamaPenerima,
                                                 akJanaanProfilPenerima.NoPendaftaranPemohon,
                                                 akJanaanProfilPenerima.Catatan,
                                                 akJanaanProfilPenerima.JCaraBayarId,
                                                 akJanaanProfilPenerima.JBankId,
                                                 akJanaanProfilPenerima.NoAkaunBank,
                                                 akJanaanProfilPenerima.Alamat1,
                                                 akJanaanProfilPenerima.Alamat2,
                                                 akJanaanProfilPenerima.Alamat3,
                                                 akJanaanProfilPenerima.Emel,
                                                 akJanaanProfilPenerima.KodM2E,
                                                 akJanaanProfilPenerima.Amaun,
                                                 akJanaanProfilPenerima.NoRujukanMohon,
                                                 akJanaanProfilPenerima.AkRekupId,
                                                 akJanaanProfilPenerima.EnJenisId);
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

        public JsonResult GetAllItemCartAkJanaanProfil()
        {

            try
            {
                
                List<AkJanaanProfilPenerima> penerima = _cart.AkJanaanProfilPenerima.ToList();

                return Json(new { result = "OK", penerima = penerima.OrderBy(d => d.Bil) });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }
    }
}
