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
    [Authorize]
    public class AkPelarasanPOController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodAkPelarasanPO;
        public const string namamodul = Modules.namaAkPelarasanPO;
        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;
        private readonly CartAkPelarasanPO _cart;

        public AkPelarasanPOController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices,
            CartAkPelarasanPO cart)
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

            var akPelarasanPO = _unitOfWork.AkPelarasanPORepo.GetResults(searchString, date1, date2, searchColumn, EnStatusBorang.Semua);

            return View(akPelarasanPO);
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

            var akPelarasanPO = _unitOfWork.AkPelarasanPORepo.GetDetailsById((int)id);
            if (akPelarasanPO == null)
            {
                return NotFound();
            }

            PopulateCartAkPelarasanPOFromDb(akPelarasanPO);
            return View(akPelarasanPO);
        }

        private void PopulateCartAkPelarasanPOFromDb(AkPelarasanPO akPelarasanPO)
        {
            if (akPelarasanPO.AkPelarasanPOObjek != null)
            {
                foreach (var item in akPelarasanPO.AkPelarasanPOObjek)
                {
                    _cart.AddItemObjek(
                            akPelarasanPO.Id,
                            item.JKWPTJBahagianId,
                            item.AkCartaId,
                            item.Amaun);
                }
            }

            if (akPelarasanPO.AkPelarasanPOPerihal != null)
            {
                foreach (var item in akPelarasanPO.AkPelarasanPOPerihal)
                {
                    _cart.AddItemPerihal(
                        akPelarasanPO.Id,
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

        private void PopulateListViewFromCart()
        {
            List<AkPelarasanPOObjek> objek = _cart.AkPelarasanPOObjek.ToList();

            foreach (AkPelarasanPOObjek item in objek)
            {
                var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                item.JKWPTJBahagian = jkwPtjBahagian;

                item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                item.AkCarta = akCarta;
            }

            ViewBag.akPelarasanPOObjek = objek;

            List<AkPelarasanPOPerihal> perihal = _cart.AkPelarasanPOPerihal.ToList();

            ViewBag.akPelarasanPOPerihal = perihal;
        }

        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akPelarasanPO = _unitOfWork.AkPelarasanPORepo.GetDetailsById((int)id);
            if (akPelarasanPO == null)
            {
                return NotFound();
            }

            if (akPelarasanPO.EnStatusBorang != EnStatusBorang.None)
            {
                TempData[SD.Error] = "Hapus data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index)));
            }
            EmptyCart();
            PopulateCartAkPelarasanPOFromDb(akPelarasanPO);
            return View(akPelarasanPO);
        }

        public IActionResult BatalLulus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akPelarasanPO = _unitOfWork.AkPelarasanPORepo.GetDetailsById((int)id);
            if (akPelarasanPO == null)
            {
                return NotFound();
            }

            if (akPelarasanPO.EnStatusBorang != EnStatusBorang.Lulus)
            {
                TempData[SD.Error] = "Data belum diluluskan..!";
                return (RedirectToAction(nameof(Index)));
            }
            EmptyCart();
            PopulateCartAkPelarasanPOFromDb(akPelarasanPO);
            return View(akPelarasanPO);
        }

        [HttpPost, ActionName("BatalLulus")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatalLulusConfirmed(int id, string tindakan, string syscode)
        {
            var akPelarasanPO = _unitOfWork.AkPelarasanPORepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akPelarasanPO != null && !string.IsNullOrEmpty(akPelarasanPO.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkPelarasanPORepo.IsPostedAsync((int)id, akPelarasanPO.NoRujukan) == false)
                {
                    TempData[SD.Error] = "Data belum diposting.";
                    return RedirectToAction(nameof(Index));
                }

                if (await _unitOfWork.AkPelarasanPORepo.IsLulusAsync(id) == false)
                {
                    TempData[SD.Error] = "Data belum diluluskan";
                    return RedirectToAction(nameof(Index));
                }

                _unitOfWork.AkPelarasanPORepo.BatalLulus(id, tindakan, user?.Email);

                _appLog.Insert("UnPosting", "Batal Lulus " + akPelarasanPO.NoRujukan ?? "", akPelarasanPO.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya batal kelulusan..!";
            }
            else
            {
                TempData[SD.Error] = "Data tidak wujud";
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult BatalPos(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akPelarasanPO = _unitOfWork.AkPelarasanPORepo.GetDetailsById((int)id);
            if (akPelarasanPO == null)
            {
                return NotFound();
            }

            if (akPelarasanPO.EnStatusBorang != EnStatusBorang.Lulus)
            {
                TempData[SD.Error] = "Data belum diluluskan..!";
                return (RedirectToAction(nameof(Index)));
            }
            EmptyCart();
            PopulateCartAkPelarasanPOFromDb(akPelarasanPO);
            return View(akPelarasanPO);
        }

        [HttpPost, ActionName("BatalPos")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatalPosConfirmed(int id, string tindakan, string syscode)
        {
            var akPelarasanPO = _unitOfWork.AkPelarasanPORepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akPelarasanPO != null && !string.IsNullOrEmpty(akPelarasanPO.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkPelarasanPORepo.IsPostedAsync((int)id, akPelarasanPO.NoRujukan) == false)
                {
                    TempData[SD.Error] = "Data belum diposting.";
                    return RedirectToAction(nameof(Index));
                }

                if (await _unitOfWork.AkPelarasanPORepo.IsLulusAsync(id) == false)
                {
                    TempData[SD.Error] = "Data belum diluluskan";
                    return RedirectToAction(nameof(Index));
                }

                _unitOfWork.AkPelarasanPORepo.BatalPos(id, tindakan, user?.UserName);

                _appLog.Insert("UnPosting", "Batal Pos " + akPelarasanPO.NoRujukan ?? "", akPelarasanPO.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya batal pos..!";
            }
            else
            {
                TempData[SD.Error] = "Data belum disahkan / disemak / diluluskan";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> PosSemula(int id, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            var obj = await _context.AkPelarasanPO.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Pos operation

            if (obj != null && !string.IsNullOrEmpty(obj.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkPelarasanPORepo.IsPostedAsync((int)id, obj.NoRujukan))
                {
                    TempData[SD.Error] = "Data sudah diposting.";
                    return RedirectToAction(nameof(Index));
                }

                if (await _unitOfWork.AkPelarasanPORepo.IsLulusAsync(id))
                {
                    TempData[SD.Error] = "Data telah diluluskan";
                    return RedirectToAction(nameof(Index));
                }

                _unitOfWork.AkPelarasanPORepo.Lulus(id, pekerjaId, user?.UserName);

                // Batal operation end
                _appLog.Insert("Posting", obj.NoRujukan ?? "", obj.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya pos semula..!";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);

            EmptyCart();
            PopulateDropDownList(1);
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.PX.GetDisplayName(), DateTime.Now.ToString("yyyy"));
            return View();
        }

        private dynamic GenerateRunningNumber(string initNoRujukan, string tahun)
        {
            var maxRefNo = _unitOfWork.AkPelarasanPORepo.GetMaxRefNo(initNoRujukan, tahun);

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
            ViewBag.AkPO = _unitOfWork.AkPORepo.GetAllByStatus(EnStatusBorang.Lulus);
            ViewBag.EnJenisPerolehan = EnumHelper<EnJenisPerolehan>.GetList();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AkPelarasanPO akPelarasanPO, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            // check if there is pelulus available or not based on modul, kelulusan, and bahagian
            if (_cart.AkPelarasanPOObjek != null && _cart.AkPelarasanPOObjek.Count() > 0)
            {
                foreach (var item in _cart.AkPelarasanPOObjek)
                {
                    var jKWPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModulKelulusan.PelarasanPO, EnKategoriKelulusan.Pelulus, jKWPtjBahagian.JBahagianId, akPelarasanPO.Jumlah) == false)
                    {
                        TempData[SD.Error] = "Tiada Pelulus yang wujud untuk senarai kod bahagian berikut.";
                        ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.PX.GetDisplayName(), akPelarasanPO.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
                        PopulateDropDownList(akPelarasanPO.JKWId);
                        PopulateListViewFromCart();
                        return View(akPelarasanPO);
                    }
                }
            }
            //

            if (ModelState.IsValid)
            {

                akPelarasanPO.UserId = user?.UserName ?? "";
                akPelarasanPO.TarMasuk = DateTime.Now;
                akPelarasanPO.DPekerjaMasukId = pekerjaId;

                akPelarasanPO.AkPelarasanPOObjek = _cart.AkPelarasanPOObjek?.ToList();
                akPelarasanPO.AkPelarasanPOPerihal = _cart.AkPelarasanPOPerihal.ToList();

                _context.Add(akPelarasanPO);
                _appLog.Insert("Tambah", akPelarasanPO.NoRujukan ?? "", akPelarasanPO.NoRujukan ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya ditambah..!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.PX.GetDisplayName(), akPelarasanPO.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
            PopulateDropDownList(akPelarasanPO.JKWId);
            PopulateListViewFromCart();
            return View(akPelarasanPO);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akPelarasanPO = _unitOfWork.AkPelarasanPORepo.GetDetailsById((int)id);
            if (akPelarasanPO == null)
            {
                return NotFound();
            }

            if (akPelarasanPO.EnStatusBorang != EnStatusBorang.None)
            {
                TempData[SD.Error] = "Ubah data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index)));
            }

            EmptyCart();
            PopulateDropDownList(akPelarasanPO.JKWId);
            PopulateCartAkPelarasanPOFromDb(akPelarasanPO);
            return View(akPelarasanPO);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AkPelarasanPO akPelarasanPO, string? fullName, string syscode)
        {
            if (id != akPelarasanPO.Id)
            {
                return NotFound();
            }

            // check if there is pelulus available or not based on modul, kelulusan, and bahagian
            if (_cart.AkPelarasanPOObjek != null && _cart.AkPelarasanPOObjek.Count() > 0)
            {
                foreach (var item in _cart.AkPelarasanPOObjek)
                {
                    var jKWPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    if (_unitOfWork.DKonfigKelulusanRepo.IsPersonAvailable(EnJenisModulKelulusan.PelarasanPO, EnKategoriKelulusan.Pelulus, jKWPtjBahagian.JBahagianId, akPelarasanPO.Jumlah) == false)
                    {
                        TempData[SD.Error] = "Tiada Pelulus yang wujud untuk senarai kod bahagian berikut.";
                        PopulateDropDownList(akPelarasanPO.JKWId);
                        PopulateListViewFromCart();
                        return View(akPelarasanPO);
                    }
                }
            }
            //

            if (akPelarasanPO.NoRujukan != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = _unitOfWork.AkPelarasanPORepo.GetDetailsById(id);
                    var jumlahAsal = objAsal!.Jumlah;
                    akPelarasanPO.NoRujukan = objAsal.NoRujukan;
                    akPelarasanPO.UserId = objAsal.UserId;
                    akPelarasanPO.TarMasuk = objAsal.TarMasuk;
                    akPelarasanPO.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                    if (objAsal.AkPelarasanPOObjek != null && objAsal.AkPelarasanPOObjek.Count > 0)
                    {
                        foreach (var item in objAsal.AkPelarasanPOObjek)
                        {
                            var model = _context.AkPelarasanPOObjek.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null) _context.Remove(model);
                        }
                    }

                    if (objAsal.AkPelarasanPOPerihal != null && objAsal.AkPelarasanPOPerihal.Count > 0)
                    {
                        foreach (var item in objAsal.AkPelarasanPOPerihal)
                        {
                            var model = _context.AkPelarasanPOPerihal.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null) _context.Remove(model);
                        }
                    }

                    _context.Entry(objAsal).State = EntityState.Detached;

                    akPelarasanPO.UserIdKemaskini = user?.UserName ?? "";
                    akPelarasanPO.TarKemaskini = DateTime.Now;
                    akPelarasanPO.AkPelarasanPOObjek = _cart.AkPelarasanPOObjek?.ToList();
                    akPelarasanPO.AkPelarasanPOPerihal = _cart.AkPelarasanPOPerihal.ToList();

                    _unitOfWork.AkPelarasanPORepo.Update(akPelarasanPO);

                    if (jumlahAsal != akPelarasanPO.Jumlah)
                    {
                        _appLog.Insert("Ubah", Convert.ToDecimal(jumlahAsal).ToString("#,##0.00") + " -> " + Convert.ToDecimal(akPelarasanPO.Jumlah).ToString("#,##0.00") + " : " + akPelarasanPO.NoRujukan ?? "", akPelarasanPO.NoRujukan ?? "", id, akPelarasanPO.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }
                    else
                    {
                        _appLog.Insert("Ubah", akPelarasanPO.NoRujukan ?? "", akPelarasanPO.NoRujukan ?? "", id, akPelarasanPO.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }

                    await _context.SaveChangesAsync();

                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AkPelarasanPOExist(akPelarasanPO.Id))
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

            PopulateDropDownList(akPelarasanPO.JKWId);
            PopulateListViewFromCart();
            return View(akPelarasanPO);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string sebabHapus, string syscode)
        {
            var akPelarasamPO = _unitOfWork.AkPelarasanPORepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akPelarasamPO != null && await _unitOfWork.AkPelarasanPORepo.IsSahAsync(id) == false)
            {
                akPelarasamPO.UserIdKemaskini = user?.UserName ?? "";
                akPelarasamPO.TarKemaskini = DateTime.Now;
                akPelarasamPO.DPekerjaKemaskiniId = pekerjaId;
                akPelarasamPO.SebabHapus = sebabHapus;

                _context.AkPelarasanPO.Remove(akPelarasamPO);
                _appLog.Insert("Hapus", akPelarasamPO.NoRujukan ?? "", akPelarasamPO.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var obj = await _context.AkPelarasanPO.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.AkPelarasanPO.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.NoRujukan ?? "", obj.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AkPelarasanPOExist(int id)
        {
            return _unitOfWork.AkPelarasanPORepo.IsExist(b => b.Id == id);
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


                    // get akPelarasanPO 
                    var akPelarasanPO = _unitOfWork.AkPelarasanPORepo.GetDetailsById((int)id);
                    if (akPelarasanPO == null)
                    {
                        TempData[SD.Error] = "Data tidak wujud.";
                    }
                    else
                    {

                        if (akPelarasanPO.NoRujukan != null)
                        {
                            // check is it posted or not
                            if (await _unitOfWork.AkPelarasanPORepo.IsPostedAsync((int)id, akPelarasanPO.NoRujukan) == false)
                            {
                                TempData[SD.Error] = "Data belum diposting.";
                                return RedirectToAction(nameof(Index));
                            }

                            // posting start here
                            _unitOfWork.AkPelarasanPORepo.RemovePostingFromAbBukuVot(akPelarasanPO, user?.UserName ?? "");

                            //insert applog
                            _appLog.Insert("UnPosting", "UnPosting Data", akPelarasanPO.NoRujukan, (int)id, akPelarasanPO.Jumlah, pekerjaId, modul, syscode, namamodul, user);

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

        private JsonResult EmptyCart()
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

        public JsonResult GetAkPODetails(int id, int? akPelarasanPOId)
        {
            try
            {
                EmptyCart();
                var data = _unitOfWork.AkPORepo.GetDetailsById(id);

                if (data != null)
                {
                    if (data.AkPOObjek != null)
                    {
                        foreach (var item in data.AkPOObjek)
                        {
                            _cart.AddItemObjek(
                                    akPelarasanPOId ?? 0,
                                    item.JKWPTJBahagianId,
                                    item.AkCartaId,
                                    item.Amaun);
                        }
                    }

                    if (data.AkPOPerihal != null)
                    {
                        foreach (var item in data.AkPOPerihal)
                        {
                            _cart.AddItemPerihal(
                                akPelarasanPOId ?? 0,
                                item.Bil,
                                item.Perihal,
                                item.Kuantiti,
                                item.Unit,
                                item.Harga,
                                item.Amaun
                                );
                        }
                    }
                    return Json(new { result = "OK", record = data });
                }
                else
                {
                    return Json(new { result = "Error", message = "data tidak wujud!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }
        }

        public JsonResult SaveCartAkPelarasanPOObjek(AkPelarasanPOObjek akPelarasanPOObjek)
        {
            try
            {
                if (akPelarasanPOObjek != null)
                {
                    _cart.AddItemObjek(akPelarasanPOObjek.AkPelarasanPOId, akPelarasanPOObjek.JKWPTJBahagianId, akPelarasanPOObjek.AkCartaId, akPelarasanPOObjek.Amaun);
                }



                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkPelarasanPOObjek(AkPelarasanPOObjek akPelarasanPOObjek)
        {
            try
            {
                if (akPelarasanPOObjek != null)
                {
                    _cart.RemoveItemObjek(akPelarasanPOObjek.JKWPTJBahagianId, akPelarasanPOObjek.AkCartaId);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkPelarasanPOObjek(AkPelarasanPOObjek akPelarasanPOObjek)
        {

            try
            {
                AkPelarasanPOObjek data = _cart.AkPelarasanPOObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akPelarasanPOObjek.JKWPTJBahagianId && x.AkCartaId == akPelarasanPOObjek.AkCartaId) ?? new AkPelarasanPOObjek();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkPelarasanPOObjek(AkPelarasanPOObjek akPelarasanPOObjek)
        {

            try
            {

                var data = _cart.AkPelarasanPOObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akPelarasanPOObjek.JKWPTJBahagianId && x.AkCartaId == akPelarasanPOObjek.AkCartaId);

                var user = _userManager.GetUserName(User);

                if (data != null)
                {
                    _cart.RemoveItemObjek(akPelarasanPOObjek.JKWPTJBahagianId, akPelarasanPOObjek.AkCartaId);

                    _cart.AddItemObjek(akPelarasanPOObjek.AkPelarasanPOId,
                                    akPelarasanPOObjek.JKWPTJBahagianId,
                                    akPelarasanPOObjek.AkCartaId,
                                    akPelarasanPOObjek.Amaun);
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
                var akPelarasanPO = _cart.AkPelarasanPOPerihal.FirstOrDefault(pp => pp.Bil == Bil);
                if (akPelarasanPO != null)
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

        public JsonResult SaveCartAkPelarasanPOPerihal(AkPelarasanPOPerihal akPelarasanPOPerihal)
        {
            try
            {
                if (akPelarasanPOPerihal != null)
                {
                    _cart.AddItemPerihal(akPelarasanPOPerihal.AkPelarasanPOId, akPelarasanPOPerihal.Bil, akPelarasanPOPerihal.Perihal, akPelarasanPOPerihal.Kuantiti, akPelarasanPOPerihal.Unit, akPelarasanPOPerihal.Harga, akPelarasanPOPerihal.Amaun);
                }



                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkPelarasanPOPerihal(AkPelarasanPOPerihal akPelarasanPOPerihal)
        {
            try
            {
                if (akPelarasanPOPerihal != null)
                {
                    _cart.RemoveItemPerihal(akPelarasanPOPerihal.Bil);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkPelarasanPOPerihal(AkPelarasanPOPerihal akPelarasanPOPerihal)
        {

            try
            {
                AkPelarasanPOPerihal data = _cart.AkPelarasanPOPerihal.FirstOrDefault(x => x.Bil == akPelarasanPOPerihal.Bil) ?? new AkPelarasanPOPerihal();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkPelarasanPOPerihal(AkPelarasanPOPerihal akPelarasanPOPerihal)
        {

            try
            {

                var akPelarasanPO = _cart.AkPelarasanPOPerihal.FirstOrDefault(x => x.Bil == akPelarasanPOPerihal.Bil);

                var user = _userManager.GetUserName(User);

                if (akPelarasanPO != null)
                {
                    _cart.RemoveItemPerihal(akPelarasanPOPerihal.Bil);

                    _cart.AddItemPerihal(akPelarasanPOPerihal.AkPelarasanPOId, akPelarasanPOPerihal.Bil, akPelarasanPOPerihal.Perihal, akPelarasanPOPerihal.Kuantiti, akPelarasanPOPerihal.Unit, akPelarasanPOPerihal.Harga, akPelarasanPOPerihal.Amaun);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAllItemCartAkPelarasanPO()
        {

            try
            {
                List<AkPelarasanPOObjek> objek = _cart.AkPelarasanPOObjek.ToList();

                foreach (AkPelarasanPOObjek item in objek)
                {
                    var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    item.JKWPTJBahagian = jkwPtjBahagian;

                    item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                    var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                    item.AkCarta = akCarta;
                }

                List<AkPelarasanPOPerihal> perihal = _cart.AkPelarasanPOPerihal.ToList();

                return Json(new { result = "OK", objek = objek.OrderBy(d => d.AkCarta?.Kod), perihal = perihal.OrderBy(d => d.Bil) });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }
    }
}
