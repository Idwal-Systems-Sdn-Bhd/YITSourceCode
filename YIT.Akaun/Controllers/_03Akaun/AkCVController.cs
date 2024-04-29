using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Implementations;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services;
using YIT._DataAccess.Services.Cart;
using YIT._DataAccess.Services.Math;
using YIT.Akaun.Infrastructure;
using YIT.Akaun.Microservices;

namespace YIT.Akaun.Controllers._03Akaun
{
    [Authorize(Roles = Init.allExceptAdminRole)]
    public class AkCVController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodAkCV;
        public const string namamodul = Modules.namaAkCV;
        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly IAkPanjarLejarRepository _panjarLejar;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;
        private readonly CartAkCV _cart;

        public AkCVController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            IAkPanjarLejarRepository panjarLejar,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices,
            CartAkCV cart)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _panjarLejar = panjarLejar;
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
            searchDate1 = searchDate1 ?? DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd");
            searchDate2 = searchDate2 ?? DateTime.Now.ToString("yyyy-MM-dd");

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

            var akCV = _unitOfWork.AkCVRepo.GetResults(searchString, date1, date2, searchColumn, EnStatusBorang.Semua);

            return View(akCV);
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

            var akCV = _unitOfWork.AkCVRepo.GetDetailsById((int)id);
            if (akCV == null)
            {
                return NotFound();
            }
            EmptyCart();
            PopulateCartAkCVFromDb(akCV);
            ManipulateHiddenDiv(akCV.EnKategoriPenerima);
            return View(akCV);
        }

        private void ManipulateHiddenDiv(EnKategoriDaftarAwam enKategoriPenerima)
        {
            switch (enKategoriPenerima)
            {
                case EnKategoriDaftarAwam.Pekerja:
                    ViewBag.DivPekerja = "";
                    ViewBag.DivLainLain = "hidden";
                    break;
                case EnKategoriDaftarAwam.LainLain:
                    ViewBag.DivPekerja = "hidden";
                    ViewBag.DivLainLain = "";
                    break;
            }
        }

        private void PopulateCartAkCVFromDb(AkCV akCV)
        {
            if (akCV.AkCVObjek != null)
            {
                foreach (var item in akCV.AkCVObjek)
                {
                    _cart.AddItemObjek(
                            akCV.Id,
                            item.JKWPTJBahagianId,
                            item.AkCartaId,
                            item.Amaun);
                }
            }

            PopulateListViewFromCart();
        }

        private void PopulateListViewFromCart()
        {
            List<AkCVObjek> objek = _cart.AkCVObjek.ToList();

            foreach (AkCVObjek item in objek)
            {
                var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                item.JKWPTJBahagian = jkwPtjBahagian;

                item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                item.AkCarta = akCarta;
            }

            ViewBag.akCVObjek = objek;
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

        [Authorize(Policy = modul + "D")]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akCV = _unitOfWork.AkCVRepo.GetDetailsById((int)id);
            if (akCV == null)
            {
                return NotFound();
            }

            if (akCV.EnStatusBorang != EnStatusBorang.None)
            {
                TempData[SD.Error] = "Hapus data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkCVFromDb(akCV);
            ManipulateHiddenDiv(akCV.EnKategoriPenerima);
            return View(akCV);
        }

        [Authorize(Policy = modul + "BL")]
        public IActionResult BatalPos(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akCV = _unitOfWork.AkCVRepo.GetDetailsById((int)id);
            if (akCV == null)
            {
                return NotFound();
            }

            if (akCV.EnStatusBorang != EnStatusBorang.Lulus)
            {
                TempData[SD.Error] = "Data belum diluluskan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }
            EmptyCart();
            PopulateCartAkCVFromDb(akCV);
            return View(akCV);
        }

        [HttpPost, ActionName("BatalPos")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "BL")]
        public async Task<IActionResult> BatalPosConfirmed(int id, string tindakan, string syscode)
        {
            var akCV = _unitOfWork.AkCVRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akCV != null && !string.IsNullOrEmpty(akCV.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkCVRepo.IsPostedAsync((int)id, akCV.NoRujukan) == false)
                {
                    TempData[SD.Error] = "Data belum diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkCVRepo.IsLulusAsync(id) == false)
                {
                    TempData[SD.Error] = "Data belum diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _panjarLejar.IsExistAsync(pl => pl.AkCVId == id) == true)
                {
                    TempData[SD.Error] = "Data telah direkup";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }
                _unitOfWork.AkCVRepo.BatalPos(id, tindakan, user?.UserName);

                _appLog.Insert("UnPosting", "Batal Pos " + akCV.NoRujukan ?? "", akCV.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var obj = await _context.AkCV.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Pos operation

            if (obj != null && !string.IsNullOrEmpty(obj.NoRujukan))
            {
                // check is it posted or not
                if (await _unitOfWork.AkCVRepo.IsPostedAsync((int)id, obj.NoRujukan))
                {
                    TempData[SD.Error] = "Data sudah diposting.";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                if (await _unitOfWork.AkCVRepo.IsLulusAsync(id))
                {
                    TempData[SD.Error] = "Data telah diluluskan";
                    return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
                }

                _unitOfWork.AkCVRepo.Lulus(id, pekerjaId, user?.UserName);

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
            PopulateDropDownList();
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.CV.GetDisplayName(), DateTime.Now.ToString("yyyy"));
            ViewBag.Jumlah = 0;
            ManipulateHiddenDiv(EnKategoriDaftarAwam.LainLain);
            return View();
        }

        private dynamic GenerateRunningNumber(string initNoRujukan, string tahun)
        {
            var maxRefNo = _unitOfWork.AkCVRepo.GetMaxRefNo(initNoRujukan, tahun);

            var prefix = initNoRujukan + "/" + tahun + "/";
            return RunningNumberFormatter.GenerateRunningNumber(prefix, maxRefNo, "00000");
        }

        private void PopulateDropDownList()
        {
            ViewBag.DPanjar = _unitOfWork.DPanjarRepo.GetAllDetails();
            ViewBag.AkCarta = _unitOfWork.AkCartaRepo.GetResultsByParas(EnParas.Paras4);
            ViewBag.JKWPTJBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetails();
            var kategoriPenerima = EnumHelper<EnKategoriDaftarAwam>.GetList();

            kategoriPenerima = kategoriPenerima.Where(p => p.perihal!.Contains("Pekerja",StringComparison.OrdinalIgnoreCase) || p.perihal!.Contains("Lain-lain", StringComparison.OrdinalIgnoreCase)).ToList();

            ViewBag.EnKategoriPenerima = kategoriPenerima;

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "C")]
        public async Task<IActionResult> Create(AkCV akCV, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            // check if there is baki awal for the panjar in AkPanjarLejar
            if (!_context.AkPanjarLejar.Any(pl => pl.DPanjarId == akCV.DPanjarId && pl.NoRujukan!.Contains("BAKI AWAL")))
            {
                TempData[SD.Error] = "Baki awal tidak wujud bagi panjar ini..!";
                ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.CV.GetDisplayName(), akCV.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
                ViewBag.Jumlah = akCV.Jumlah;
                PopulateDropDownList();
                ManipulateHiddenDiv(akCV.EnKategoriPenerima);
                PopulateListViewFromCart();
                return View(akCV);
            }
            else
            {
                // check if still in budget
                decimal baki = 0;
                var panjarLejarList = await _context.AkPanjarLejar.Where(pl => (pl.DPanjarId == akCV.DPanjarId && pl.NoRujukan!.Contains("BAKI AWAL")) || (pl.DPanjarId == akCV.DPanjarId && pl.IsPaid == false)).ToListAsync();

                if (panjarLejarList != null && panjarLejarList.Any())
                {
                    foreach (var item in panjarLejarList) baki += item.Debit - item.Kredit;
                }

                if (baki < akCV.Jumlah)
                {
                    TempData[SD.Error] = "Baki tidak mencukupi. Sila buat rekupan dahulu sebelum meneruskan transaksi.";
                    ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.CV.GetDisplayName(), akCV.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
                    ViewBag.Jumlah = akCV.Jumlah;
                    PopulateDropDownList();
                    ManipulateHiddenDiv(akCV.EnKategoriPenerima);
                    PopulateListViewFromCart();
                    return View(akCV);
                }
            }



            if (ModelState.IsValid)
            {

                if (akCV.EnKategoriPenerima == EnKategoriDaftarAwam.Pekerja && akCV.DPekerjaId != null)
                {
                    var pekerja = await _context.DPekerja.FirstOrDefaultAsync(p => p.Id == akCV.DPekerjaId);

                    if (pekerja != null)
                    {
                        akCV.NoPendaftaranPenerima = pekerja.NoKp ?? pekerja.NoKpLama;
                        akCV.NamaPenerima = pekerja.Nama;
                        akCV.Alamat1 = pekerja.Alamat1;
                        akCV.Alamat2 = pekerja.Alamat2;
                        akCV.Alamat3 = pekerja.Alamat3;
                    }
                    else
                    {
                        TempData[SD.Error] = "Pekerja tidak wujud..!";
                        ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.CV.GetDisplayName(), akCV.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
                        ViewBag.Jumlah = akCV.Jumlah;
                        PopulateDropDownList();
                        ManipulateHiddenDiv(akCV.EnKategoriPenerima);
                        PopulateListViewFromCart();
                        return View(akCV);
                    }
                }

                akCV.UserId = user?.UserName ?? "";
                akCV.TarMasuk = DateTime.Now;
                akCV.DPekerjaMasukId = pekerjaId;

                akCV.AkCVObjek = _cart.AkCVObjek?.ToList();

                _context.Add(akCV);
                _appLog.Insert("Tambah", akCV.NoRujukan ?? "", akCV.NoRujukan ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya ditambah..!";
                return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
            }
            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.CV.GetDisplayName(), akCV.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
            ViewBag.Jumlah = akCV.Jumlah;
            PopulateDropDownList();
            ManipulateHiddenDiv(akCV.EnKategoriPenerima);
            PopulateListViewFromCart();
            return View(akCV);
        }

        [Authorize(Policy = modul + "E")]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akCV = _unitOfWork.AkCVRepo.GetDetailsById((int)id);
            if (akCV == null)
            {
                return NotFound();
            }

            if (akCV.EnStatusBorang != EnStatusBorang.None)
            {
                TempData[SD.Error] = "Ubah data tidak dibenarkan..!";
                return (RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") }));
            }

            EmptyCart();
            PopulateDropDownList();
            PopulateCartAkCVFromDb(akCV);
            ManipulateHiddenDiv(akCV.EnKategoriPenerima);
            return View(akCV);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "E")]
        public async Task<IActionResult> Edit(int id, AkCV akCV, string? fullName, string syscode)
        {
            if (id != akCV.Id)
            {
                return NotFound();
            }

            if (akCV.NoRujukan != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = _unitOfWork.AkCVRepo.GetDetailsById(id);
                    var jumlahAsal = objAsal!.Jumlah;
                    akCV.NoRujukan = objAsal.NoRujukan;
                    akCV.UserId = objAsal.UserId;
                    akCV.TarMasuk = objAsal.TarMasuk;
                    akCV.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                    if (objAsal.AkCVObjek != null && objAsal.AkCVObjek.Count > 0)
                    {
                        foreach (var item in objAsal.AkCVObjek)
                        {
                            var model = _context.AkCVObjek.FirstOrDefault(b => b.Id == item.Id);
                            if (model != null) _context.Remove(model);
                        }
                    }

                    _context.Entry(objAsal).State = EntityState.Detached;

                    if (akCV.EnKategoriPenerima == EnKategoriDaftarAwam.Pekerja && akCV.DPekerjaId != null)
                    {
                        var pekerja = await _context.DPekerja.FirstOrDefaultAsync(p => p.Id == akCV.DPekerjaId);

                        if (pekerja != null)
                        {
                            akCV.NoPendaftaranPenerima = pekerja.NoKp ?? pekerja.NoKpLama;
                            akCV.NamaPenerima = pekerja.Nama;
                            akCV.Alamat1 = pekerja.Alamat1;
                            akCV.Alamat2 = pekerja.Alamat2;
                            akCV.Alamat3 = pekerja.Alamat3;
                        }
                        else
                        {
                            TempData[SD.Error] = "Pekerja tidak wujud..!";
                            ViewBag.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.CV.GetDisplayName(), akCV.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"));
                            PopulateDropDownList();
                            ManipulateHiddenDiv(akCV.EnKategoriPenerima);
                            PopulateListViewFromCart();
                            return View(akCV);
                        }
                    }

                    akCV.UserIdKemaskini = user?.UserName ?? "";
                    akCV.TarKemaskini = DateTime.Now;
                    akCV.AkCVObjek = _cart.AkCVObjek?.ToList();

                    _unitOfWork.AkCVRepo.Update(akCV);

                    if (jumlahAsal != akCV.Jumlah)
                    {
                        _appLog.Insert("Ubah", Convert.ToDecimal(jumlahAsal).ToString("#,##0.00") + " -> " + Convert.ToDecimal(akCV.Jumlah).ToString("#,##0.00") + " : " + akCV.NoRujukan ?? "", akCV.NoRujukan ?? "", id, akCV.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }
                    else
                    {
                        _appLog.Insert("Ubah", akCV.NoRujukan ?? "", akCV.NoRujukan ?? "", id, akCV.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }

                    await _context.SaveChangesAsync();

                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AkCVExist(akCV.Id))
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
            PopulateListViewFromCart();
            ManipulateHiddenDiv(akCV.EnKategoriPenerima);
            return View(akCV);
        }

        private bool AkCVExist(int id)
        {
            return _unitOfWork.AkCVRepo.IsExist(b => b.Id == id);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "D")]
        public async Task<IActionResult> DeleteConfirmed(int id, string sebabHapus, string syscode)
        {
            var akCV = _unitOfWork.AkCVRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akCV != null && await _unitOfWork.AkCVRepo.IsLulusAsync(id) == false)
            {
                akCV.UserIdKemaskini = user?.UserName ?? "";
                akCV.TarKemaskini = DateTime.Now;
                akCV.DPekerjaKemaskiniId = pekerjaId;
                akCV.SebabHapus = sebabHapus;

                _context.AkCV.Remove(akCV);
                _appLog.Insert("Hapus", akCV.NoRujukan ?? "", akCV.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var obj = await _context.AkCV.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.AkCV.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.NoRujukan ?? "", obj.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index), new { searchString = HttpContext.Session.GetString("searchString"), searchDate1 = HttpContext.Session.GetString("searchDate1"), searchDate2 = HttpContext.Session.GetString("searchDate2") });
        }

        public JsonResult SaveCartAkCVObjek(AkCVObjek akCVObjek)
        {
            try
            {
                if (akCVObjek != null)
                {
                    _cart.AddItemObjek(akCVObjek.AkCVId, akCVObjek.JKWPTJBahagianId, akCVObjek.AkCartaId, akCVObjek.Amaun);
                }



                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult RemoveCartAkCVObjek(AkCVObjek akCVObjek)
        {
            try
            {
                if (akCVObjek != null)
                {
                    _cart.RemoveItemObjek(akCVObjek.JKWPTJBahagianId, akCVObjek.AkCartaId);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAnItemFromCartAkCVObjek(AkCVObjek akCVObjek)
        {

            try
            {
                AkCVObjek data = _cart.AkCVObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akCVObjek.JKWPTJBahagianId && x.AkCartaId == akCVObjek.AkCartaId) ?? new AkCVObjek();

                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult SaveAnItemFromCartAkCVObjek(AkCVObjek akCVObjek)
        {

            try
            {

                var akCV = _cart.AkCVObjek.FirstOrDefault(x => x.JKWPTJBahagianId == akCVObjek.JKWPTJBahagianId && x.AkCartaId == akCVObjek.AkCartaId);

                var user = _userManager.GetUserName(User);

                if (akCV != null)
                {
                    _cart.RemoveItemObjek(akCVObjek.JKWPTJBahagianId, akCVObjek.AkCartaId);

                    _cart.AddItemObjek(akCVObjek.AkCVId,
                                    akCVObjek.JKWPTJBahagianId,
                                    akCVObjek.AkCartaId,
                                    akCVObjek.Amaun);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        public JsonResult GetAllItemCartAkCV()
        {

            try
            {
                List<AkCVObjek> objek = _cart.AkCVObjek.ToList();

                foreach (AkCVObjek item in objek)
                {
                    var jkwPtjBahagian = _unitOfWork.JKWPTJBahagianRepo.GetAllDetailsById(item.JKWPTJBahagianId);

                    item.JKWPTJBahagian = jkwPtjBahagian;

                    item.JKWPTJBahagian.Kod = BelanjawanFormatter.ConvertToBahagian(jkwPtjBahagian.JKW?.Kod, jkwPtjBahagian.JPTJ?.Kod, jkwPtjBahagian.JBahagian?.Kod);

                    var akCarta = _unitOfWork.AkCartaRepo.GetById(item.AkCartaId);

                    item.AkCarta = akCarta;
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
