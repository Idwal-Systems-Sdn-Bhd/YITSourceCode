using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Statics;
using YIT._DataAccess.Data;
using YIT.Akaun.Models.ViewModels.Administrations;
using System.Drawing;
using System.Runtime.Versioning;

namespace YIT.Akaun.Controller
{
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class SiAppInfoController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodSiAppInfo;
        public const string namamodul = Modules.namaSiAppInfo;

        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _hostEnvironment;

        public SiAppInfoController(ApplicationDbContext db,
                                   UserManager<IdentityUser> userManager,
                                   RoleManager<IdentityRole> roleManager,
                                   IWebHostEnvironment hostEnvironment)
        {
            this._db=db;
            this._userManager=userManager;
            this._roleManager=roleManager;
            this._hostEnvironment=hostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            var appInfo = await _db.SiAppInfo.FirstOrDefaultAsync();
            return View(appInfo);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var obj = await _db.SiAppInfo.FirstOrDefaultAsync(x => x.Id == id);
            if (obj == null)
            {
                return NotFound();
            }

            AppInfoViewModel model = new AppInfoViewModel();

            model.Id = obj.Id;
            model.KodSistem = obj.KodSistem;
            model.TarVersi = obj.TarVersi;
            model.NamaSyarikat = obj.NamaSyarikat;
            model.NoPendaftaran = obj.NoPendaftaran;
            model.AlamatSyarikat1 = obj.AlamatSyarikat1;
            model.AlamatSyarikat2 = obj.AlamatSyarikat2;
            model.AlamatSyarikat3 = obj.AlamatSyarikat3;
            model.Bandar = obj.Bandar;
            model.Poskod = obj.Poskod;
            model.Daerah = obj.Daerah;
            model.Negeri = obj.Negeri;
            model.TelSyarikat = obj.TelSyarikat;
            model.FaksSyarikat = obj.FaksSyarikat;
            model.EmelSyarikat = obj.EmelSyarikat;
            model.TarMula = obj.TarMula;
            model.GambarSediaAda = obj.CompanyLogoWeb;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SupportedOSPlatform("windows")]
        public async Task<IActionResult> Edit(int id, AppInfoViewModel model)
        {
            var obj1 = await _db.SiAppInfo.FindAsync(model.Id);

            if (ModelState.IsValid)
            {
                var obj = await _db.SiAppInfo.FindAsync(model.Id);
                if (obj != null)
                {
                    obj.Id = model.Id;
                    obj.KodSistem = model.KodSistem;
                    obj.NamaSyarikat = model.NamaSyarikat;
                    obj.NoPendaftaran = model.NoPendaftaran;
                    obj.AlamatSyarikat1 = model.AlamatSyarikat1;
                    obj.AlamatSyarikat2 = model.AlamatSyarikat2;
                    obj.AlamatSyarikat3 = model.AlamatSyarikat3;
                    obj.Bandar = model.Bandar;
                    obj.Poskod = model.Poskod;
                    obj.Daerah = model.Daerah;
                    obj.Negeri = model.Negeri;
                    obj.TelSyarikat = model.TelSyarikat;
                    obj.FaksSyarikat = model.FaksSyarikat;
                    obj.EmelSyarikat = model.EmelSyarikat;
                    obj.TarMula = model.TarMula;

                    if (model.Gambar != null)
                    {
                        if (model.GambarSediaAda != null)
                        {
                            string? filePath = Path.Combine(_hostEnvironment.WebRootPath, "img", model.GambarSediaAda);

                            var image = Image.FromFile(filePath);

                            image.Dispose();

                            System.IO.File.Delete(filePath);
                        }

                        obj.CompanyLogoWeb = ProcessUploadedFile(model);
                    }

                    _db.Update(obj);
                }
                
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        private string ProcessUploadedFile(AppInfoViewModel model)
        {
            string uniqueFileName = "";

            if (model.Gambar != null)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "img");
                uniqueFileName = "MainLogo_Syarikat.webp";
                //uniqueFileName = model.Gambar.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.Gambar.CopyTo(fileStream);
                }
            }

            return uniqueFileName;
        }
    }
}
