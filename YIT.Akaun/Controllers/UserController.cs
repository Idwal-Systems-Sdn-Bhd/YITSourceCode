using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT.Akaun.Infrastructure;
using YIT.Akaun.Models.ViewModels.Administrations;
using System.Data;
using System.Security.Claims;
using static YIT.Akaun.Models.ViewModels.Administrations.UserClaimsViewModel;

namespace YIT.Akaun.Controller
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class UserController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string syscode = "MAIN";
        public const string modul = "SY001";
        public const string namamodul = "Sistem Pengguna";

        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly _AppLogIRepository<AppLog, int> _appLog;
        private UserServices _userServices;

        public UserController(
            ApplicationDbContext db,
            UserManager<IdentityUser> userManager,
            _AppLogIRepository<AppLog, int> appLog,
            UserServices userService)
        {
            _db = db;
            _userManager = userManager;
            _appLog = appLog;
            _userServices = userService;
        }

        public IActionResult Index()
        {

            var userList = _db.ApplicationUsers.ToList();
            var userRole = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();
            foreach (var user in userList)
            {
                List<string> namaRole = new List<string>();
                var userRoles = userRole.Where(u => u.UserId == user.Id).ToList();
                if (userRoles == null)
                {
                    namaRole.Add("None");
                }
                else
                {
                    foreach (var item in userRoles)
                    {
                        var RoleName = _db.Roles.FirstOrDefault(b => b.Id == item.RoleId)?.Name;
                        if (RoleName != null)
                            namaRole.Add(RoleName);
                    }
                }
                user.UserRoles = namaRole;
            }
            //hide superadmin
            //userList = userList.Where(x => x.Role != "SuperAdmin").ToList();

            return View(userList);
        }

        public IActionResult Create()
        {
            return RedirectToAction(nameof(AccountController.Register), "Account");
        }

        public IActionResult Edit(string userId)
        {
            var objFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == userId);
            if (objFromDb == null)
            {
                return NotFound();
            }
            var userRole = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();
            var role = userRole.Where(u => u.UserId == userId).ToList();
            List<string> namaRole = new List<string>();

            if (role != null)
            {
                objFromDb.SelectedRoleList = role;
                foreach (var item in role)
                {
                    var RoleName = _db.Roles.FirstOrDefault(b => b.Id == item.RoleId)?.Name;
                    if (RoleName != null)
                        namaRole.Add(RoleName);
                }
            }

            ViewBag.SelectedRole = namaRole;

            objFromDb.RoleList = _db.Roles.Where(x => x.Name != "SuperAdmin").Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id
            });

            // get selected multiple dropdown for bahagian
            ViewBag.JBahagian = _db.JBahagian.ToList();

            string[] arr = objFromDb.JBahagianList!.Split(',');
            List<string> nama = new List<string>();

            foreach (var item in arr)
            {
                var bahagian = _db.JBahagian.FirstOrDefault(x => x.Id == int.Parse(item));

                if (bahagian != null)
                    nama.Add(bahagian.Perihal!);

            }
            ViewBag.SelectedJBahagian = nama;

            return View(objFromDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ApplicationUser user, string? SelectedJBahagianList, List<string>? SelectedRoleList)
        {
            if (SelectedRoleList != null)
            {
                if (ModelState.IsValid)
                {

                    foreach (var item in SelectedRoleList)
                    {
                        var role = new IdentityUserRole<string>
                        {
                            RoleId = item
                        };

                        user.SelectedRoleList!.Add(role);
                    }

                    var objFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == user.Id);
                    List<string> roleAsal = new List<string>();
                    List<string> roleBaru = new List<string>();

                    if (objFromDb == null)
                    {
                        return NotFound();
                    }


                    if (user.SelectedRoleList != null)
                    {
                        var userRoles = _db.UserRoles.Where(u => u.UserId == objFromDb.Id).ToList();

                        if (userRoles != null)
                        {
                            //removing old roles
                            foreach (var item in userRoles)
                            {
                                var previousRoleName = _db.Roles.Where(u => u.Id == item.RoleId).Select(e => e.Name).FirstOrDefault();
                                if (previousRoleName != null)
                                {
                                    roleAsal.Add(previousRoleName);
                                    //removing old role
                                    await _userManager.RemoveFromRoleAsync(objFromDb, previousRoleName);
                                }
                                    
                            }

                            //add new roles
                            foreach (var item in SelectedRoleList)
                            {
                                var newRoleName = _db.Roles.Where(u => u.Id == item).Select(e => e.Name).FirstOrDefault();
                                if (newRoleName != null)
                                {
                                    roleBaru.Add(newRoleName);
                                    //add new role
                                    await _userManager.AddToRoleAsync(objFromDb, newRoleName);
                                }
                                
                            }

                        }
                        else
                        {
                            //add new roles
                            foreach (var item in SelectedRoleList)
                            {
                                var newRoleName = _db.Roles.Where(u => u.Id == item).Select(e => e.Name).FirstOrDefault();
                                if (newRoleName != null)
                                //add new role
                                    await _userManager.AddToRoleAsync(objFromDb, newRoleName);
                            }
                        }
                    }

                    // select multiple dropdownlist
                    if (user.SelectedJBahagianList != null)
                    {
                        user.JBahagianList = String.Join(",", user.SelectedJBahagianList);
                    }
                    else
                    {
                        if (objFromDb.JBahagianList != null)
                        {
                            user.JBahagianList = objFromDb.JBahagianList;
                        }
                    }
                    // select multiple dropdownlist end

                    objFromDb.Nama = user.Nama;
                    objFromDb.JBahagianList = user.JBahagianList;
                    int? pekerjaId = _db.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

                    if (roleAsal != roleBaru && !string.IsNullOrEmpty(user.Email))
                    {
                        _appLog.Insert("Ubah", String.Join(",", roleAsal) + " -> " + String.Join(",", roleBaru), user.Email, 0, 0,pekerjaId,modul, syscode, namamodul, user);

                    }
                    _db.SaveChanges();
                    TempData[SD.Success] = "Data pengguna berjaya diubah.";
                    return RedirectToAction(nameof(Index));
                }
            }

            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem()
            {
                Value = "Admin",
                Text = "Admin"
            });
            listItems.Add(new SelectListItem()
            {
                Value = "Supervisor",
                Text = "Supervisor"
            });
            listItems.Add(new SelectListItem()
            {
                Value = "User",
                Text = "User"
            });

            user.RoleList = listItems;

            //user.RoleList = _db.Roles.Select(u => new SelectListItem
            //{
            //    Text = u.Name,
            //    Value = u.Id
            //});
            TempData[SD.Error] = "Sila Pilih Tahap Pengguna..!";
            return View(user);
        }
        public IActionResult LockUnlock(string userId)
        {
            var objFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == userId);
            if (objFromDb == null)
            {
                return NotFound();
            }
            if (objFromDb.LockoutEnd!=null && objFromDb.LockoutEnd>DateTime.Now)
            {
                // user is locked and will remain locked until lockoutend time
                //clicking on this action will unlock time
                objFromDb.LockoutEnd = DateTime.Now;
                TempData[SD.Success] = "Buka kunci pengguna berjaya.";

            }
            else
            {
                //user is not locked, and we want to lock user
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
                TempData[SD.Success] = "Kunci pengguna berjaya.";
            }
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));

        }

        [HttpPost]
        public async Task<IActionResult> Delete(string userId)
        {
            var objFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == userId);
            if (objFromDb != null && !string.IsNullOrEmpty(objFromDb.Email))
            {
                _db.ApplicationUsers.Remove(objFromDb);
                var user = await _userManager.GetUserAsync(User);
                int? pekerjaId = _db.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;
                _appLog.Insert("Hapus", objFromDb.Email + " - " + objFromDb.Nama, objFromDb.Email, 0, 0, pekerjaId, modul, syscode, namamodul, user);

                _db.SaveChanges();
                TempData[SD.Success] = "Hapus pengguna berjaya.";
                return RedirectToAction(nameof(Index));
                
            }
            else
            {
                return NotFound();
            }
            

        }
        [HttpGet]
        public async Task<IActionResult> ManageUserClaims(string userId)
        {
            IdentityUser user = await _userManager.FindByIdAsync(userId) ?? new IdentityUser();
            if (user == null)
            {
                return NotFound();
            }

            var existingUserClaims = await _userManager.GetClaimsAsync(user);

            var model = new UserClaimsViewModel()
            {
                UserId = userId
            };

            foreach (Claim claim in ClaimStore.claimList)
            {
                UserClaim userClaim = new UserClaim
                {
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value
                };
                if (existingUserClaims.Any(c => c.Type == claim.Type))
                {
                    userClaim.IsSelected = true;
                }
                model.Claims.Add(userClaim);
            }
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserClaims(UserClaimsViewModel userClaimsViewModel)
        {
            IdentityUser user = await _userManager.FindByIdAsync(userClaimsViewModel.UserId) ?? new IdentityUser();
            if (user == null)
            {
                return NotFound();
            }

            var claims = await _userManager.GetClaimsAsync(user);
            var result = await _userManager.RemoveClaimsAsync(user, claims);
            int? pekerjaId = _db.ApplicationUsers.Where(b => b.Id == user!.Id).FirstOrDefault()!.DPekerjaId;

            if (!result.Succeeded)
            {
                TempData[SD.Error] = "Ralat ketika menghapuskan capaian.";
            }

            result = await _userManager.AddClaimsAsync(user,
                userClaimsViewModel.Claims.Where(c => c.IsSelected).Select(c => new Claim(c.ClaimType, c.IsSelected.ToString())));

            if (!result.Succeeded)
            {
                TempData[SD.Error] = "Ralat ketika menambah capaian.";
            }

            if (!string.IsNullOrEmpty(user.Email))
                
                _appLog.Insert("Ubah", user.Email + " - Ubah Capaian", user.Email, 0, 0, pekerjaId, modul, syscode, namamodul, user);

            _db.SaveChanges();
            TempData[SD.Success] = "Capaian berjaya diubah.";
            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public async Task<ActionResult> ImpersonateUser(string userId)
        {
            await _userServices.Impersonate(userId);
            return RedirectToAction("Index", "Home");
        }
    }
}
