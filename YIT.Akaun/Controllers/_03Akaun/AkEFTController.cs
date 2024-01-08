using DocumentFormat.OpenXml.Office.CustomUI;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoreLinq;
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
using YIT.Akaun.Models.ViewModels.TextFiles;

namespace YIT.Akaun.Controllers._03Akaun
{
    public class AkEFTController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodAkEFTMaybank2E;
        public const string namamodul = Modules.namaAkEFTMaybank2E;

        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;
        private readonly CartAkEFT _cart;

        public AkEFTController(
            ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices,
            CartAkEFT cart)
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

            var akEFT = _unitOfWork.AkEFTRepo.GetResults(searchString, date1, date2, searchColumn);

            return View(akEFT);
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

            var akEFT = _unitOfWork.AkEFTRepo.GetDetailsById((int)id);
            if (akEFT == null)
            {
                return NotFound();
            }
            EmptyCart();
            PopulateCartAkEFTFromDb(akEFT);
            return View(akEFT);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akEFT = _unitOfWork.AkEFTRepo.GetDetailsById((int)id);
            if (akEFT == null)
            {
                return NotFound();
            }

            EmptyCart();
            PopulateCartAkEFTFromDb(akEFT);
            return View(akEFT);
        }

        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);

            ViewBag.NoRujukan = EnInitNoRujukan.EF.GetDisplayName() + "/" + GetNamaFail(DateTime.Now, DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MM"));
            ViewBag.NamaFail = EnInitNoRujukan.EF.GetDisplayName() + GetNamaFail(DateTime.Now, DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MM")) + ".txt";

            EmptyCart();
            PopulateDropDownList();
            return View();
        }

        [HttpPost]
        public JsonResult GetNoRujukan(DateTime tarikh, string noRujukan)
        {
            try
            {
                if (noRujukan != "Invalid date")
                {
                    noRujukan = GetNamaFail(tarikh, tarikh.ToString("yyyy"), tarikh.ToString("MM"));

                    return Json(new { result = "OK", record = noRujukan });
                }
                else
                {
                    return Json(new { result = "OK", record = "" });
                }

            }
            catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult GetPenerimaList(DateTime? tarDari, DateTime? tarHingga, string produk, int? akBankId)
        {
            try
            {
                if (tarDari  == null || tarHingga == null)
                {
                    return Json(new { result = "Error", message = "Sila isi ruangan tarikh dari dan tarikh hingga" });
                }

                // find all pv within date range where statusBorang = lulus only
                List<AkPV> pvList = _unitOfWork.AkPVRepo.GetResults(null, tarDari, tarHingga, "NoRujukan", EnStatusBorang.Lulus, akBankId);
                // get all information fron AkPVPenerima
                var akBank = new AkBank();
                if (akBankId != null)
                {
                    akBank = _unitOfWork.AkBankRepo.GetAllDetailsById((int)akBankId);
                    
                }
                
                if (produk == "EFT")
                {
                    pvList = pvList.Where(pv => pv.AkJanaanProfilId == null).ToList();
                }
                else
                {
                    pvList = pvList.Where(pv => pv.AkJanaanProfilId != null).ToList();
                }

                if (pvList != null && pvList.Count > 0)
                {
                    var pvPenerimaTable = new List<AkPVPenerima>();
                    foreach (AkPV pv in pvList)
                    {
                        if (pv.AkPVPenerima != null && pv.AkPVPenerima.Count > 0)
                        {
                            foreach (AkPVPenerima pvPenerima in pv.AkPVPenerima)
                            {
                                // check if penerima status EFT = none, then proceed
                                if (pvPenerima.EnStatusEFT == EnStatusProses.None && pvPenerima.JCaraBayarId != 1 && pvPenerima.JBankId != null) // 1 = Tunai
                                {
                                    // check if already create penerima in akEFTPenerima
                                    if (!_context.AkEFTPenerima.Any(ep => ep.AkPVId == pv.Id && ep.Bil == pvPenerima.Bil && pvPenerima.EnStatusEFT == EnStatusProses.None))
                                    {
                                        int bil = (int)pvPenerima.Bil! - 1;
                                        _cart.AddItemPenerima(0, pv.Id, EnStatusProses.None, "", null, pvPenerima.Bil, pvPenerima.NoPendaftaranPenerima, pvPenerima.NamaPenerima, pvPenerima.Catatan, pvPenerima.JCaraBayarId, pvPenerima.JBankId, pvPenerima.NoAkaunBank, pvPenerima.Emel, pvPenerima.KodM2E, pvPenerima.AkPV?.NoRujukan?.Replace("/", "") + RunningNumberFormatter.GenerateRunningNumber("", bil.ToString() ?? "0" , "000"), pvPenerima.Amaun, pvPenerima.EnJenisId);

                                        pvPenerimaTable.Add(pvPenerima);
                                    }
                                    
                                }
                            }
                        }
                    }

                    return Json(new { result = "OK", table = pvPenerimaTable, jBankId = akBank.JBankId });
                }
                else
                {
                    return Json(new { result = "Error", message = "Tiada baucer yang boleh dijana" });
                }
            }
            catch(Exception ex)
            {
                return Json(new {result = "Error", message = ex.Message});
            }
        }

        public JsonResult RemoveAkEFTPenerima(AkEFTPenerima akEFTPenerima)
        {

            try
            {

                var data = _cart.AkEFTPenerima.FirstOrDefault(x => x.Bil == akEFTPenerima.Bil && x.AkPVId == akEFTPenerima.AkPVId);

                if (data != null)
                {
                    _cart.RemoveItemPenerima(akEFTPenerima.Bil,akEFTPenerima.AkPVId);
                }

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        private string GetNamaFail(DateTime tarJana, string year, string month)
        {
            string prefix = year + month;
            int x = 1;
            string noRujukan = prefix + "000";

            var LatestNoRujukan = _context.AkEFT
                       .IgnoreQueryFilters()
                       .Where(x => x.Tarikh.Year == tarJana.Year && x.Tarikh.Month == tarJana.Month && x.NoRujukan!.Contains(prefix.ToUpper()))
                       .Max(x => x.NoRujukan);

            if (LatestNoRujukan == null)
            {
                noRujukan = string.Format("{0:" + prefix + "000}", x);
            }
            else
            {
                x = int.Parse(LatestNoRujukan.Substring(7));
                x++;
                noRujukan = string.Format("{0:" + prefix + "000}", x);
            }
            return noRujukan;
        }

        private void PopulateDropDownList()
        {
            ViewBag.JBank = _unitOfWork.JBankRepo.GetAll();
            ViewBag.AkBank = _unitOfWork.AkBankRepo.GetAllDetails();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AkEFT akEFT, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            // double check if every penerima have linked with akPVPenerima or not
            if (_cart.AkEFTPenerima != null && _cart.AkEFTPenerima.Count() > 0)
            {

                if (_cart.AkEFTPenerima.Count() > 1)

                    foreach (var item in _cart.AkEFTPenerima)
                    {
                        if (item.AkPVId == 0)
                        {
                            TempData[SD.Error] = "Terdapat penerima yang tiada kaitan dengan baucer sedia ada";
                            
                            ViewBag.NoRujukan = EnInitNoRujukan.EF.GetDisplayName() + "/" + GetNamaFail(akEFT.Tarikh, akEFT.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"),akEFT.Tarikh.ToString("MM") ?? DateTime.Now.ToString("MM"));
                            PopulateDropDownList();
                            PopulateListViewFromCart();
                            return View(akEFT);
                        }
                    }
            }
            // 

            if (ModelState.IsValid)
            {

                akEFT.NoRujukan = EnInitNoRujukan.EF.GetDisplayName() + "/" + GetNamaFail(akEFT.Tarikh, akEFT.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"), akEFT.Tarikh.ToString("MM") ?? DateTime.Now.ToString("MM"));

                akEFT.UserId = user?.UserName ?? "";
                akEFT.TarMasuk = DateTime.Now;
                akEFT.DPekerjaMasukId = pekerjaId;

                akEFT.AkEFTPenerima = _cart.AkEFTPenerima?.ToList();

                _context.Add(akEFT);

                // update no EFT in akPV
                if (akEFT.AkEFTPenerima != null)
                {
                    foreach (var item in akEFT.AkEFTPenerima)
                    {
                        var akPV = _unitOfWork.AkPVRepo.GetDetailsById(item.AkPVId);

                        if (akPV != null && akPV.AkPVPenerima != null)
                        {
                            foreach (var pvPenerima in akPV.AkPVPenerima)
                            {

                                pvPenerima.NoRujukanCaraBayar = item.NoRujukanCaraBayar;
                                pvPenerima.TarikhCaraBayar = DateTime.Now;
                            }

                            _unitOfWork.AkPVRepo.Update(akPV);
                        }
                       
                    }

                }
                // update no EFT in AkPV end

                _appLog.Insert("Tambah", akEFT.NoRujukan ?? "", akEFT.NoRujukan ?? "", 0, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya ditambah..!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.NoRujukan = EnInitNoRujukan.EF.GetDisplayName() + "/" + GetNamaFail(akEFT.Tarikh, akEFT.Tarikh.ToString("yyyy") ?? DateTime.Now.ToString("yyyy"), akEFT.Tarikh.ToString("MM") ?? DateTime.Now.ToString("MM"));
            PopulateDropDownList();
            PopulateListViewFromCart();
            return View(akEFT);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var akEFT = _unitOfWork.AkEFTRepo.GetDetailsById((int)id);
            if (akEFT == null)
            {
                return NotFound();
            }

                if (_context.AkEFT.Any(eft => eft.EnStatusEFT != EnStatusProses.Success))
                {
                    TempData[SD.Error] = "Ubah data tidak dibenarkan..!";
                    return (RedirectToAction(nameof(Index)));
                }

            EmptyCart();
            PopulateDropDownList();
            PopulateCartAkEFTFromDb(akEFT);
            return View(akEFT);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AkEFT akEFT, string? fullName, string syscode)
        {
            if (id != akEFT.Id)
            {
                return NotFound();
            }

            if (akEFT.NoRujukan != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    var objAsal = _unitOfWork.AkEFTRepo.GetDetailsById(id);
                    var jumlahAsal = objAsal!.Jumlah;
                    akEFT.NoRujukan = objAsal.NoRujukan;
                    akEFT.UserId = objAsal.UserId;
                    akEFT.TarMasuk = objAsal.TarMasuk;
                    akEFT.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                    akEFT.UserIdKemaskini = user?.UserName ?? "";
                    akEFT.TarKemaskini = DateTime.Now;
                    akEFT.AkEFTPenerima = _cart.AkEFTPenerima.ToList();

                    _context.Entry(objAsal).State = EntityState.Detached;

                    akEFT.AkEFTPenerima = _cart.AkEFTPenerima?.ToList();

                    _unitOfWork.AkEFTRepo.Update(akEFT);

                    if (jumlahAsal != akEFT.Jumlah)
                    {
                        _appLog.Insert("Ubah", Convert.ToDecimal(jumlahAsal).ToString("#,##0.00") + " -> " + Convert.ToDecimal(akEFT.Jumlah).ToString("#,##0.00") + " : " + akEFT.NoRujukan ?? "", akEFT.NoRujukan ?? "", id, akEFT.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }
                    else
                    {
                        _appLog.Insert("Ubah", akEFT.NoRujukan ?? "", akEFT.NoRujukan ?? "", id, akEFT.Jumlah, pekerjaId, modul, syscode, namamodul, user);
                    }

                    await _context.SaveChangesAsync();

                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AkEFTExist(akEFT.Id))
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
            return View(akEFT);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string sebabHapus, string syscode)
        {
            var akEFT = _unitOfWork.AkEFTRepo.GetById((int)id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (akEFT != null)
            {
                akEFT.UserIdKemaskini = user?.UserName ?? "";
                akEFT.TarKemaskini = DateTime.Now;
                akEFT.DPekerjaKemaskiniId = pekerjaId;
                akEFT.SebabHapus = sebabHapus;

                _context.AkEFT.Remove(akEFT);
                _appLog.Insert("Hapus", akEFT.NoRujukan ?? "", akEFT.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);
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

            var obj = await _context.AkEFT.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            // Batal operation

            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.AkEFT.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.NoRujukan ?? "", obj.NoRujukan ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AkEFTExist(int id)
        {
            return _unitOfWork.AkEFTRepo.IsExist(b => b.Id == id);
        }

        private void PopulateCartAkEFTFromDb(AkEFT akEFT)
        {
            if (akEFT.AkEFTPenerima != null && akEFT.AkEFTPenerima.Count > 0)
            {
                foreach (var item in akEFT.AkEFTPenerima)
                {
                    _cart.AddItemPenerima(akEFT.Id, item.AkPVId, item.EnStatusEFT, item.SebabGagal, item.TarikhKredit, item.Bil, item.NoPendaftaranPenerima, item.NamaPenerima, item.Catatan, item.JCaraBayarId, item.JBankId,item.NoAkaunBank, item.Emel, item.KodM2E, item.NoRujukanCaraBayar, item.Amaun, item.EnJenisId);
                }
            }
            PopulateListViewFromCart();
            
        }

        private void PopulateListViewFromCart()
        {
            List<AkEFTPenerima> penerima = _cart.AkEFTPenerima.ToList();

            foreach (var item in penerima)
            {
                if (item.AkPVId != 0)
                {
                    var akPV = _unitOfWork.AkPVRepo.GetDetailsById(item.AkPVId);
                    item.AkPV = akPV;
                }
                
                if (item.JBankId != null)
                {
                    var jBank = _unitOfWork.JBankRepo.GetById((int)item.JBankId); 
                    item.JBank = jBank;
                }

                if (item.JCaraBayarId != 0)
                {
                    var jCaraBayar = _unitOfWork.JCaraBayarRepo.GetById(item.JCaraBayarId);
                    item.JCaraBayar = jCaraBayar;
                }

            }
            ViewBag.akEFTPenerima = penerima;
        }

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

        [HttpPost]
        public JsonResult GenerateTxt(int id)
        {
            try
            {
                AkEFT eft = _unitOfWork.AkEFTRepo.GetDetailsById(id);

                if (eft != null)
                {
                    List<string> txt = GenerateMaybank2ETxt(eft);
                    
                    
                    return Json(new { result = "OK", record = txt });
                }
                else
                {
                    return Json(new { result = "Error", message = "data tidak wujud" });
                }


            }
            catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }
        }

        private List<string> GenerateMaybank2ETxt(AkEFT eft)
        {
            string l = "|";
            List<string> txt = new List<string>();
            // batch header record
            var header = "00|MYYIT|" + eft.NoRujukan?.Replace("/", "") + string.Concat(Enumerable.Repeat(l, 26));
            // batch header record end
            txt.Add(header);

            if (eft.AkEFTPenerima != null && eft.AkEFTPenerima.Count > 0)
            {
                foreach (var penerima in eft.AkEFTPenerima)
                {
                    // detail record
                    var kodBNMEFT = _unitOfWork.JBankRepo.GetById((int)penerima.JBankId!).KodBNMEFT;

                    var paymode = "IG";

                    if (kodBNMEFT != null && kodBNMEFT == "MBBEMYKL")
                    {
                        paymode = "IT";
                    }

                    var productProvider = "Domestic Payments (MY)";

                    var valueDate = eft.TarikhBayar.ToString("ddMMyyyy");

                    var noPendaftaran = "";
                    switch (penerima.EnJenisId)
                    {
                        case EnJenisId.KPBaru:
                            noPendaftaran = penerima.NoPendaftaranPenerima?.Trim() + string.Concat(Enumerable.Repeat(l, 3));
                            break;
                        case EnJenisId.KPLama:
                            noPendaftaran = l + penerima.NoPendaftaranPenerima?.Trim() + string.Concat(Enumerable.Repeat(l, 2));
                            break;
                        case EnJenisId.KodPembekal:
                            noPendaftaran = string.Concat(Enumerable.Repeat(l, 2)) + penerima.NoPendaftaranPenerima?.Trim() + l;
                            break;
                        case EnJenisId.NoTentera:
                            noPendaftaran = string.Concat(Enumerable.Repeat(l, 3)) + penerima.NoPendaftaranPenerima?.Trim();
                            break;

                    }

                    var maxCatatanLength = 55;
                    if (penerima.Catatan?.Length > maxCatatanLength)
                        penerima.Catatan = penerima.Catatan.Substring(0, maxCatatanLength);


                    var maxNamaPenerimaLength = 40;

                    if (penerima.NamaPenerima?.Length > maxNamaPenerimaLength)
                        penerima.NamaPenerima = penerima.NamaPenerima.Substring(0, maxNamaPenerimaLength);

                    var maxRingkasanLength = 55;

                    if (penerima.AkPV?.Ringkasan?.Length > maxRingkasanLength)
                        penerima.AkPV.Ringkasan = penerima.AkPV.Ringkasan.Substring(0, maxRingkasanLength);

                    var detailRow = "01" + l +
                                   paymode + l +
                                   productProvider + l +
                                   valueDate + string.Concat(Enumerable.Repeat(l, 3)) +
                                   penerima.NoRujukanCaraBayar + l +
                                   penerima.NoPendaftaranPenerima?.Trim() + l +
                                   penerima.Catatan + l +
                                   "MYR" + l +
                                   penerima.Amaun.ToString("####.00") + l +
                                   "Y" + l +
                                   "MYR" + l +
                                   eft.AkBank!.NoAkaun?.Trim() + l +
                                   penerima.NoAkaunBank?.Trim() + l +
                                   penerima.KodM2E + l +
                                   l +
                                   "Y" + l +
                                   penerima.NamaPenerima + string.Concat(Enumerable.Repeat(l, 5)) +
                                   noPendaftaran + string.Concat(Enumerable.Repeat(l, 9)) +
                                   kodBNMEFT + string.Concat(Enumerable.Repeat(l, 67)) +
                                   penerima.NoRujukanCaraBayar + l +
                                   "" + string.Concat(Enumerable.Repeat(l, 6)) +
                                   "01" + l +
                                   "" + string.Concat(Enumerable.Repeat(l, 225));

                    txt.Add(detailRow);

                    var adviseRow = "02" + l +
                                    "PA" + l +
                                    penerima.NoRujukanCaraBayar + l +
                                    penerima.Emel + string.Concat(Enumerable.Repeat(l, 3)) +
                                    penerima.AkPV?.Ringkasan + string.Concat(Enumerable.Repeat(l, 7)) +
                                    penerima.Amaun.ToString("####.00") + string.Concat(Enumerable.Repeat(l, 27));

                    txt.Add(adviseRow);

                    // update noRujukanCaraBayar, tarikhCaraBayar akPVPenerima
                    var pvPenerima = _context.AkPVPenerima.FirstOrDefault(pp => pp.Bil == penerima.Bil && pp.AkPVId ==  penerima.AkPVId);

                    if (pvPenerima != null)
                    {
                        pvPenerima.NoRujukanCaraBayar = penerima.NoRujukanCaraBayar;
                        pvPenerima.TarikhCaraBayar = DateTime.Now;
                        pvPenerima.EnStatusEFT = EnStatusProses.Pending;

                        _context.AkPVPenerima.Update(pvPenerima);
                    }

                    penerima.TarikhKredit = DateTime.Now;
                    penerima.EnStatusEFT = EnStatusProses.Pending;
                    _context.AkEFTPenerima.Update(penerima);


                }
            }

            eft.EnStatusEFT = EnStatusProses.Pending;
            _context.AkEFT.Update(eft);
            _context.SaveChanges();

            return txt;
        }

        public JsonResult GetAllItemCartAkEFT()
        {

            try
            {

                List<AkEFTPenerima> penerimaList = _cart.AkEFTPenerima.ToList();

                if (penerimaList != null && penerimaList.Count > 0)
                {
                    foreach (var item in penerimaList)
                    {
                        var pv = _unitOfWork.AkPVRepo.GetDetailsById(item.AkPVId);

                        if (pv != null)
                        {
                            item.AkPV = pv;
                        }
                        
                    }
                    
                }
                return Json(new { result = "OK", table = penerimaList!.OrderBy(d => d.Bil) });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }
    }
}
