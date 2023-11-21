using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using YIT._DataAccess.Data;
using YIT.Akaun.Models.ViewModels.Administrations;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT._DataAccess.Repositories.Interfaces;
using System.Data;
using System.Drawing;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;

namespace YIT.Akaun.Controller
{
    public class AccountController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string syscode = "MAIN";
        public const string modul = "SY001";
        public const string namamodul = "Maklumat Pengguna";

        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly _IUnitOfWork _unitOfWork;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMemoryCache _cache;

        public AccountController(
            ApplicationDbContext db,
            IConfiguration configuration,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            _IUnitOfWork unitOfWork,
            _AppLogIRepository<AppLog, int> appLog,
            IWebHostEnvironment webHostEnvironment,
            IMemoryCache cache)
        {
            _db = db;
            _configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _appLog = appLog;
            _webHostEnvironment=webHostEnvironment;
            _cache = cache;
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Register(string? returnurl = null)
        {
            if (!await _roleManager.RoleExistsAsync("SuperAdmin"))
            {
                //create role
                await _roleManager.CreateAsync(new IdentityRole("SuperAdmin"));
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
                await _roleManager.CreateAsync(new IdentityRole("Supervisor"));
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }

            List<SelectListItem> listItems = new List<SelectListItem>
            {
                new SelectListItem()
                {
                    Value = "User",
                    Text = "User"
                }
            };

            ViewData["ReturnUrl"] = returnurl;
            RegisterViewModel registerViewModel = new RegisterViewModel()
            {
                RoleList = listItems
            };

            ViewBag.DPekerja = _unitOfWork.DPekerjaRepo.GetAll();

            ViewBag.JBahagian = _db.JBahagian.ToList();

            return View(registerViewModel);
        }

        // on change kod pembekal controller
        [HttpPost]
        public JsonResult JsonGetEmailFromDPekerja(int data)
        {
            try
            {
                var result = _unitOfWork.DPekerjaRepo.GetById(data);

                return Json(new { result = "OK", record = result });
            }
            catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }
        }
        //on change kod pembekal controller end

        // redirect to login controller
        [HttpGet]
        public async Task<JsonResult> JsonLogOff()
        {
            try
            {
                await LogOff();

                return Json(new { result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "Error", message = ex.Message });
            }
        }
        //redirect to login end

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnurl = null)
        {


            ViewData["ReturnUrl"] = returnurl;
            returnurl = returnurl ?? Url.Content("~/");

            if (model.DPekerjaId != null && model.DPekerjaId != 0)
            {
                //check if user already exist in DPekerja or not
                //if true then form is valid
                var pekerja = _unitOfWork.DPekerjaRepo.GetById((int)model.DPekerjaId);
                if (pekerja != null && pekerja.Emel == model.Email)
                {
                    model.Nama = pekerja?.Nama ?? "";
                    model.Password = "Spmb1234#";
                    // select multiple dropdownlist
                    if (model.SelectedJBahagianList != null)
                    {
                        model.JBahagianList = string.Join(",", model.SelectedJBahagianList);
                    }
                    else
                    {
                        TempData[SD.Error] = "Sila pilih Bahagian bagi pengguna ini.";
                        return View(model);
                    }
                    // select multiple dropdownlist end
                    if (ModelState.IsValid)
                    {
                        var user = new ApplicationUser
                        {
                            UserName = model.Email,
                            Email = model.Email,
                            Nama = pekerja?.Nama ?? "",
                            DPekerjaId = model.DPekerjaId,
                            JKWPTJBahagianList = model.JBahagianList
                        };
                        var result = await _userManager.CreateAsync(user, model.Password);
                        if (result.Succeeded)
                        {
                            if (model.RoleSelected != null && model.RoleSelected.Length > 0 && model.RoleSelected == "Admin")
                            {
                                await _userManager.AddToRoleAsync(user, "Admin");
                            }
                            else
                            {
                                await _userManager.AddToRoleAsync(user, "User");
                            }

                            TempData[SD.Success] = "Data pengguna berjaya ditambah.";
                            _appLog.Insert("Tambah", model.Email + " - " + pekerja?.Nama, model.Email, 0, 0, model.DPekerjaId, modul,syscode,namamodul, user);
                            _db.SaveChanges();
                            return RedirectToAction(nameof(UserController.Index), "User");
                            //}


                        }
                        AddErrors(result);

                    }
                }
                else
                {
                    TempData[SD.Error] = "Pengguna belum didaftar pada Jadual Anggota.";
                }

            }

            List<SelectListItem> listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem()
            {
                Value = "User",
                Text = "User"
            });

            model.RoleList = listItems;

            ViewBag.DPekerja = _unitOfWork.DPekerjaRepo.GetAll();

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            HttpContext.Session.Remove("Username");
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Log_In()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnurl = null)
        {
            ViewData["ReturnUrl"] = returnurl;
            returnurl = returnurl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync
                    (
                        model.Emel,
                        model.Katalaluan,
                        model.IngatSaya,
                        lockoutOnFailure: true
                    );

                if (result.Succeeded)
                {
                    var user = _db.ApplicationUsers.FirstOrDefault(b => b.UserName == model.Emel);
                    var roles = _db.UserRoles.FirstOrDefault(b => b.RoleId == "1f24d001-e893-491e-bbc1-974d2ee2e0f1");
                    if (!string.IsNullOrEmpty(user?.UserName))
                        HttpContext.Session.SetString("Username", user.UserName);
                    return LocalRedirect(returnurl);
                }
                if (result.IsLockedOut)
                {
                    return View("Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Cubaan log masuk tidak sah");
                    return View(model);
                }

            }


            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        protected IDbConnection CreateConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Emel);
                if (user == null)
                {
                    return RedirectToAction("ForgotPasswordError");
                }
                await SendMail(model);

                return RedirectToAction("ForgotPasswordConfirmation");
            }

            return View(model);
        }

        public async Task<int> SendMail(ForgotPasswordViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Emel);

            var code = await _userManager.GeneratePasswordResetTokenAsync(user!);

            var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user!.Id, code = code }, protocol: HttpContext.Request.Scheme);

            var profileName = _configuration["ProfileName"];

            var html = "<h4>Set Semula Katalaluan</h4>" +
                        "</ br>" +
                        "<p>Sila set semula katalaluan anda dengan melayari pautan ini:</p>" +
                        "<a href=" + callbackUrl + ">" + callbackUrl + "</a>";
            try
            {
                var query = "EXEC msdb.dbo.sp_send_dbmail " +
                            "@profile_name = '" + profileName + "', " +
                            "@recipients = '" + model.Emel + "', " +
                            "@body = '" + html + "', " +
                            "@body_format = 'HTML'," +
                            "@subject = 'Set Semula Katalaluan - Mesej Automatik'; ";

                var parameters = new DynamicParameters();
                parameters.Add("ProfileName", profileName, DbType.String);
                parameters.Add("Email", model.Emel, DbType.String);
                parameters.Add("CallbackUrl", callbackUrl, DbType.String);

                using (var connection = CreateConnection())
                {
                    return await connection.ExecuteAsync(query, parameters);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        [HttpGet]
        public IActionResult ForgotPasswordError()
        {
            return View();
        }
        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string? code = null)
        {
            return code == null ? View("Error") : View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if(model.Email != null && model.Password != null)
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user == null)
                    {
                        return RedirectToAction("ResetPasswordConfirmation");
                    }
                    model.Code = await _userManager.GeneratePasswordResetTokenAsync(user);

                    var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("ResetPasswordConfirmation");
                    }
                    AddErrors(result);

                }
            }
            

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        [Authorize(Roles = Init.allExceptSuperadminRole)]
        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _db.ApplicationUsers.FirstOrDefaultAsync(x => x.UserName == User.Identity!.Name);

            ResetPasswordViewModel viewModel = new ResetPasswordViewModel();

            viewModel.Email = user?.Email ?? "";

            return View(viewModel);
        }

        [HttpGet]
        [Authorize(Roles = Init.allRole)]
        public async Task<IActionResult> ProfileSetting()
        {
            var user = await _db.ApplicationUsers.FirstOrDefaultAsync(x => x.UserName == User.Identity!.Name);
            ApplicationUserViewModel viewModel = new ApplicationUserViewModel();
            viewModel.id = user!.Id;
            viewModel.Nama = Regex.Replace(user?.Nama ?? "", "[^a-zA-Z0-9_]+", "");
            viewModel.Id = user!.Id;
            viewModel.GambarSediaAda = user?.Tandatangan ?? "";
            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = Init.allRole)]
        [SupportedOSPlatform("windows")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProfilSetting(string id, ApplicationUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var obj = await _db.ApplicationUsers.FirstOrDefaultAsync(x => x.Id == model.id);
                if (obj != null)
                {
                    if (model.Gambar != null)
                    {
                        if (model.GambarSediaAda != null)
                        {
                            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "img\\signature", model.GambarSediaAda);

                            if (Directory.Exists(filePath))
                            {
                                var image = Image.FromFile(filePath);

                                image.Dispose();

                                System.IO.File.Delete(filePath);
                            }

                        }

                    }

                    obj.Tandatangan = ProcessUploadedFile(model);

                    _db.Update(obj);
                    _appLog.Insert("SYSTEM", "SYSTEM - Kemaskini tandatangan", obj?.Email ?? "", 0, 0, obj?.DPekerjaId ?? 0, modul, syscode, namamodul, obj);
                    await _db.SaveChangesAsync();
                    TempData[SD.Success] = "Kemaskini tandatangan berjaya";
                }
                
            }
            TempData[SD.Error] = "Kemaskini tandatangan gagal";
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        private string ProcessUploadedFile(ApplicationUserViewModel model)
        {
            string uniqueFileName;

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "img\\signature");
            string str = Regex.Replace(model.Nama, "[^a-zA-Z0-9_]+", "");
            uniqueFileName = str + ".png";
            //uniqueFileName = model.Gambar.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                if (model.Gambar != null) model.Gambar.CopyTo(fileStream);
            }

            return uniqueFileName;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Init.allExceptSuperadminRole)]
        public async Task<IActionResult> ChangePassword(ResetPasswordViewModel model)
        {
            if (model.Email != null && model.Password != null)
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    int? pekerjaId = _db.ApplicationUsers.FirstOrDefault(b => b.Id == user!.Id)!.DPekerjaId;

                    if (user != null)
                    {
                        model.Code = await _userManager.GeneratePasswordResetTokenAsync(user);

                        var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
                        if (result.Succeeded)
                        {
                            TempData[SD.Success] = "Tukar Katalaluan berjaya..!";
                            _appLog.Insert("SYSTEM", "SYSTEM - tukar katalaluan", model.Email, 0, 0, pekerjaId, modul, syscode, namamodul, user);
                            return RedirectToAction(nameof(HomeController.Index), "Home");
                        }
                        AddErrors(result);
                    }

                }
            }
            

            TempData[SD.Error] = "Tukar Katalaluan Gagal..!";
            return View(model);
        }

        [HttpGet]
        public virtual ActionResult Download(string fileGuid, string fileName)
        {
            if (_cache.Get<byte[]>(fileGuid) != null)
            {
                byte[] data = _cache.Get<byte[]>(fileGuid)!;
                _cache.Remove(fileGuid); //cleanup here as we don't need it in cache anymore
                return File(data!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            else
            {
                // Something has gone wrong...
                return View("Error"); // or whatever/wherever you want to return the user
            }
        }
    }
}
