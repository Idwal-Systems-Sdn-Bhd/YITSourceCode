using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT._DataAccess.Services;
using YIT._DataAccess.Services.Math;
using YIT.Akaun.Infrastructure;
using YIT.Akaun.Models.ViewModels.Administrations;

namespace YIT.Akaun.Controllers._02Daftar
{
    
    [Authorize(Roles = Init.allExceptAdminRole)]
    public class DPenerimaCekGajiController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodDPenerimaCekGaji;
        public const string namamodul = Modules.kodDPenerimaCekGaji;

        private readonly ApplicationDbContext _context;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly UserServices _userServices;


        public DPenerimaCekGajiController(ApplicationDbContext context,
            _IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userServices)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _appLog = appLog;
            _userServices = userServices;
        }
        public IActionResult Index()
        {
            DPenerimaCekGajiViewModel dPenerimaCekGajiViewModel = new DPenerimaCekGajiViewModel();
            dPenerimaCekGajiViewModel.DPenerimaCekGaji = GetDPenerimaCekGaji();
            dPenerimaCekGajiViewModel.AkJanaanProfil = GetAkJanaanProfil();
            PopulateDropdownList();
            return View(dPenerimaCekGajiViewModel);
        }

        private List<DPenerimaCekGaji> GetDPenerimaCekGaji()
        {
            List<DPenerimaCekGaji> dPenerimaCekGaji = _unitOfWork.DPenerimaCekGajiRepo.GetAllDetails();
            return dPenerimaCekGaji;
        }
        private List<AkJanaanProfil> GetAkJanaanProfil()
        {
            List<AkJanaanProfil> akJanaanProfil = _context.AkJanaanProfil.Where(jp => jp.EnJenisModulProfil == EnJenisModulProfil.Gaji).OrderByDescending(jp => jp.NoRujukan).ToList();

            return akJanaanProfil;
        }

        [Authorize(Policy = modul)]
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var jak = _unitOfWork.DPenerimaCekGajiRepo.GetAllDetailsById((int)id);

            if (jak == null)
            {
                return NotFound();
            }

            PopulateDropdownList();
            return View(jak);
        }

        // GET: jCawangan/Create
        [Authorize(Policy = modul + "C")]
        public IActionResult Create()
        {
            PopulateDropdownList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "C")]
        public async Task<IActionResult> Create(DPenerimaCekGaji dPenerimaCekGaji, string syscode)
        {


            if (dPenerimaCekGaji.Kod != null && KodDPenerimaCekGajiExists(dPenerimaCekGaji.Kod) == false)
            {
                if (ModelState.IsValid)
                {


                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;




                    dPenerimaCekGaji.UserId = user?.UserName ?? "";

                    dPenerimaCekGaji.TarMasuk = DateTime.Now;
                    dPenerimaCekGaji.DPekerjaMasukId = pekerjaId;

                    _context.Add(dPenerimaCekGaji);
                    _appLog.Insert("Tambah", dPenerimaCekGaji.Id + " - " + dPenerimaCekGaji.DDaftarAwam?.Nama, dPenerimaCekGaji.Kod, 0, 0, pekerjaId, modul, syscode, namamodul, user);
                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya ditambah..!";
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                TempData[SD.Error] = "Kod ini telah wujud..!";
            }
            PopulateDropdownList();
            return View(dPenerimaCekGaji);
        }

        
        // GET: jCawangan/Edit/5
        [Authorize(Policy = modul + "E")]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dPenerimaCekGaji = _unitOfWork.DPenerimaCekGajiRepo.GetById((int)id);
            if (dPenerimaCekGaji == null)
            {
                return NotFound();
            }
            PopulateDropdownList();
            return View(dPenerimaCekGaji);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "E")]
        public async Task<IActionResult> Edit(int id, DPenerimaCekGaji dPenerimaCekGaji, string syscode)
        {
            if (id != dPenerimaCekGaji.Id)
            {
                return NotFound();
            }

            if (dPenerimaCekGaji.Kod != null && ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;
                    //tambah ddaftarawamid(farhan)
                    var objAsal = await _context.DPenerimaCekGaji.FirstOrDefaultAsync(x => x.Id == dPenerimaCekGaji.Id);
                    var kodAsal = objAsal!.Id; 
                    var perihalAsal = objAsal.Id; //var namadaftarawam(farhan)
                    dPenerimaCekGaji.UserId = objAsal.UserId;
                    dPenerimaCekGaji.TarMasuk = objAsal.TarMasuk;
                    dPenerimaCekGaji.DPekerjaMasukId = objAsal.DPekerjaMasukId;

                    _context.Entry(objAsal).State = EntityState.Detached;

       



                    dPenerimaCekGaji.UserIdKemaskini = user?.UserName ?? "";

                    dPenerimaCekGaji.TarKemaskini = DateTime.Now;
                    dPenerimaCekGaji.DPekerjaKemaskiniId = pekerjaId;

                    _unitOfWork.DPenerimaCekGajiRepo.Update(dPenerimaCekGaji);

                    _appLog.Insert("Ubah", kodAsal + " -> " + dPenerimaCekGaji.Id + ", " + perihalAsal + " -> " + dPenerimaCekGaji.Kod + ", ", dPenerimaCekGaji.Kod, id, 0, pekerjaId, modul, syscode, namamodul, user);

                    await _context.SaveChangesAsync();
                    TempData[SD.Success] = "Data berjaya diubah..!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!dPenerimaCekGajiExist(dPenerimaCekGaji.Id))
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
            PopulateDropdownList();
            return View(dPenerimaCekGaji);
        }

        // GET: jCawangan/Delete/5
        [Authorize(Policy = modul + "D")]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var df = _unitOfWork.DPenerimaCekGajiRepo.GetAllDetailsById((int)id);
            if (df == null)  //tukar jak jd df(farhan)
            {
                return NotFound();

            }
            PopulateDropdownList();
            return View(df);
        }

        // POST: jCawangan/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "D")]
        public async Task<IActionResult> DeleteConfirmed(int id, string syscode)
        {
            var dPenerimaCekGaji = _unitOfWork.DPenerimaCekGajiRepo.GetById(id);

            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (dPenerimaCekGaji != null && dPenerimaCekGaji.Kod != null)
            {
                dPenerimaCekGaji.UserIdKemaskini = user?.UserName ?? "";
                dPenerimaCekGaji.TarKemaskini = DateTime.Now;
                dPenerimaCekGaji.DPekerjaKemaskiniId = pekerjaId;

                _context.DPenerimaCekGaji.Remove(dPenerimaCekGaji);
                _appLog.Insert("Hapus", dPenerimaCekGaji.Id + " - " + dPenerimaCekGaji.Id, dPenerimaCekGaji.Id.ToString(), id, 0, pekerjaId, modul, syscode, namamodul, user);
                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dihapuskan..!";
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = modul + "R")]
        public async Task<IActionResult> RollBack(int id, string syscode)
        {
            var user = await _userManager.GetUserAsync(User);
            int? pekerjaId = _context.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            var obj = await _context.DPenerimaCekGaji.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);


            if (obj != null)
            {
                obj.FlHapus = 0;
                obj.UserIdKemaskini = user?.UserName ?? "";
                obj.TarKemaskini = DateTime.Now;
                obj.DPekerjaKemaskiniId = pekerjaId;

                _context.DPenerimaCekGaji.Update(obj);

                // Batal operation end
                _appLog.Insert("Rollback", obj.Id + " - " + obj.SuratJabatan, obj.Kod ?? "", id, 0, pekerjaId, modul, syscode, namamodul, user);

                await _context.SaveChangesAsync();
                TempData[SD.Success] = "Data berjaya dikembalikan..!";
            }

            return RedirectToAction(nameof(Index));
        }
        private bool dPenerimaCekGajiExist(int id)
        {
            return _unitOfWork.DPenerimaCekGajiRepo.IsExist(b => b.Id == id);
        }

        private bool KodDPenerimaCekGajiExists(string Kod)
        {
            return _unitOfWork.DPenerimaCekGajiRepo.IsExist(e => e.Kod == Kod);
        }
        public void PopulateDropdownList()
        {
            ViewBag.JCaraBayar = _unitOfWork.JCaraBayarRepo.GetAll();
            ViewBag.JCawangan = _unitOfWork.JCawanganRepo.GetAll();

        }
        private string GenerateRunningNumber(string Kod)
        {
            if (KodDPenerimaCekGajiExists(Kod))
            {
                return _context.DPenerimaCekGaji.FirstOrDefault(df => df.Kod == Kod)?.Kod ?? "";
            }

            var maxRefNo = _unitOfWork.DPenerimaCekGajiRepo.GetMaxRefNo();

            return RunningNumberFormatter.GenerateRunningNumber("", maxRefNo, "000");
        }
        [HttpPost]
        public JsonResult GetKod(string Kod)
        {
            try
            {
                var result = "";
                if (Kod != null)
                {
                    result = GenerateRunningNumber(Kod);
                }
                return Json(new { result = "OK", record = result });
            }
            catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }

        }
        public JsonResult GetDDaftarAwamDetails(int id, int? DPenerimaCekGajiId)
        {
            try
            {
                var data = _unitOfWork.DDaftarAwamRepo.GetAllDetailsById(id);
                return Json(new { result = "OK", record = data });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }
        
        public async Task<IActionResult> PrintPenerimaCekGaji(int id)
        {
            DPenerimaCekGaji dPenerimaCekGaji = _unitOfWork.DPenerimaCekGajiRepo.GetAllDetailsById(id);

            var company = await _userServices.GetCompanyDetails();
            PopulateDropdownList();
            //string customSwitches = "--page-offset 0 --footer-center [page] / [toPage] --footer-font-size 6"; 

            return new ViewAsPdf(modul + EnJenisFail.PDF, dPenerimaCekGaji,
                new ViewDataDictionary(ViewData) {
            { "NamaSyarikat", company.NamaSyarikat },
            { "AlamatSyarikat1", company.AlamatSyarikat1 },
            { "AlamatSyarikat2", company.AlamatSyarikat2 },
            { "AlamatSyarikat3", company.AlamatSyarikat3 }
                })
            {
                PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--footer-center \"[page]/[toPage]\"" +
                        " --footer-line --footer-font-size \"7\" --footer-spacing 1 --footer-font-name \"Segoe UI\"",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
            };
        }

        public IActionResult Generate()
        {

            var dPenerimaCekGajiList = _unitOfWork.DPenerimaCekGajiRepo.GetAllDetails();
            if (dPenerimaCekGajiList == null)
            {
                return NotFound();
            }
            return View(dPenerimaCekGajiList);
        }

        [HttpPost, ActionName("Generate")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = modul + "E")]
        public async Task<IActionResult> GenerateDPenerimaCekGajiToAkJanaanProfilPenerima()
        {
            //cari senarai penerima cek gaji
            //select * from DPenerimaCekGaji
            var DPenerimaCekGajiList = _unitOfWork.DPenerimaCekGajiRepo.GetAllDetails();   //tambah satu repo(farhan)
            var AkJanaanProfil = new AkJanaanProfil(); //var huruf kecik(farhan)
           
            //Generate No. Rujukan
            AkJanaanProfil.NoRujukan = GenerateRunningNumber(EnInitNoRujukan.JP.GetDisplayName(), DateTime.Now.ToString("yyyy"));
            //Jumlah
            //  SELECT SUM(AmaunGaji)
            //FROM DPenerimaCekGaji
            //Buat loop tuk setiap satu
            decimal jumlah = 0;
            int bil = 1;
            //
            List<AkJanaanProfilPenerima> akJanaanProfilPenerimaList = new List<AkJanaanProfilPenerima>();
            foreach (var dPenerimaCekGaji in DPenerimaCekGajiList)
            {
                jumlah += dPenerimaCekGaji.AmaunGaji;
                var AkJanaanProfilPenerima = new AkJanaanProfilPenerima();
                AkJanaanProfilPenerima.Bil = bil;
                bil++;

                AkJanaanProfilPenerima.EnKategoriDaftarAwam = EnKategoriDaftarAwam.Pembekal;
                AkJanaanProfilPenerima.DPenerimaZakatId = null;
                AkJanaanProfilPenerima.DDaftarAwamId = dPenerimaCekGaji.DDaftarAwamId;
                AkJanaanProfilPenerima.DPekerjaId = null;
                AkJanaanProfilPenerima.NoPendaftaranPenerima = dPenerimaCekGaji.DDaftarAwam?.NoPendaftaran;
                AkJanaanProfilPenerima.NamaPenerima = dPenerimaCekGaji.DDaftarAwam?.Nama;
                AkJanaanProfilPenerima.NoPendaftaranPemohon = dPenerimaCekGaji.DDaftarAwam?.NoPendaftaran;
                AkJanaanProfilPenerima.Catatan = null;
                AkJanaanProfilPenerima.JCaraBayarId = dPenerimaCekGaji.JCaraBayarId;
                AkJanaanProfilPenerima.JBankId = dPenerimaCekGaji.DDaftarAwam?.JBankId;
                AkJanaanProfilPenerima.NoAkaunBank = dPenerimaCekGaji.DDaftarAwam?.NoAkaunBank;
                AkJanaanProfilPenerima.Alamat1 = dPenerimaCekGaji.DDaftarAwam?.Alamat1;
                AkJanaanProfilPenerima.Alamat2 = dPenerimaCekGaji.DDaftarAwam?.Alamat2;
                AkJanaanProfilPenerima.Alamat3 = dPenerimaCekGaji.DDaftarAwam?.Alamat3;
                AkJanaanProfilPenerima.Emel = dPenerimaCekGaji.DDaftarAwam?.Emel;
                AkJanaanProfilPenerima.KodM2E = dPenerimaCekGaji.DDaftarAwam?.KodM2E;
                AkJanaanProfilPenerima.Amaun = dPenerimaCekGaji.AmaunGaji;
                AkJanaanProfilPenerima.NoRujukanMohon = null;
                AkJanaanProfilPenerima.AkRekupId = null;
                AkJanaanProfilPenerima.EnJenisId = EnJenisId.KodPembekal;
                akJanaanProfilPenerimaList.Add(AkJanaanProfilPenerima);
            }

            AkJanaanProfil.JCawanganId = 29; //jangan hard code(farhan)
            AkJanaanProfil.Jumlah = jumlah;
            AkJanaanProfil.Tarikh = DateTime.Now;
            AkJanaanProfil.EnJenisModulProfil = EnJenisModulProfil.Gaji;
            AkJanaanProfil.DPekerjaMasukId = null;
            AkJanaanProfil.AkJanaanProfilPenerima = akJanaanProfilPenerimaList;
            AkJanaanProfil.DPekerjaMasukId = null;
            AkJanaanProfil.UserId = AkJanaanProfil.UserId; //get user id(farhan)
            AkJanaanProfil.TarMasuk = DateTime.Now;
            AkJanaanProfil.DPekerjaKemaskiniId = null;
            AkJanaanProfil.UserIdKemaskini = AkJanaanProfil.UserIdKemaskini; //get user id(farhan)
            AkJanaanProfil.TarKemaskini = DateTime.Now;
            AkJanaanProfil.FlHapus = 0;
            AkJanaanProfil.TarHapus = DateTime.Now;
            AkJanaanProfil.SebabHapus = null;


            _context.AkJanaanProfil.Add(AkJanaanProfil);
            await _context.SaveChangesAsync();
            TempData[SD.Success] = "Data berjaya dijana..!";
            return RedirectToAction(nameof(Index));

        }

        private string GenerateRunningNumber(string initNoRujukan, string tahun) //generate running number untuk janaan profil(farhan)
        {
            var maxRefNo = _unitOfWork.AkJanaanProfilRepo.GetMaxRefNo(initNoRujukan, tahun);

            var prefix = initNoRujukan + "/" + tahun + "/";
            return RunningNumberFormatter.GenerateRunningNumber(prefix, maxRefNo, "00000");
        }

    }
}
//org yg jana shj boleh delete...user id yg sama
