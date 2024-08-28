using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using YIT.__Domain.Entities.Administrations;
using YIT._DataAccess.Data;
using YIT.Akaun.Infrastructure;
using YIT.Akaun.Models.ViewModels.Prints;
using System.Data;
using YIT.__Domain.Entities._Statics;

namespace SPMBNET7.App.Controller
{
    [Authorize(Roles = Init.superAdminAdminRole)]
    public class AppLogController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly UserServices _userService;

        public AppLogController(ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            UserServices userService)
        {
            this._context=context;
            this._userManager=userManager;
            this._userService=userService;
        }
        public async Task<IActionResult> Index(string searchUsername,
            string searchModulFrom,
            string searchModulTo,
            string searchDateFrom,
            string searchDateTo)
        {
            var appLog = new List<AppLog>();

            if (!String.IsNullOrEmpty(searchUsername) ||
                !String.IsNullOrEmpty(searchModulFrom) ||
                !String.IsNullOrEmpty(searchModulTo) ||
                !String.IsNullOrEmpty(searchDateFrom) ||
                !String.IsNullOrEmpty(searchDateTo))
            {
                appLog = _context.AppLog.ToList();
            }

            //function search userList
            var userList = await _context.ApplicationUsers.Include(x => x.DPekerja).Where(b => b.DPekerjaId != null).ToListAsync();

            List<SelectListItem> userSelect = new();
            userSelect.Add(new SelectListItem() { Text = "-- Pilih Pengguna --", Value = "" });
            foreach (var q in userList)
            {
                userSelect.Add(new SelectListItem() { Text = q.Nama + " (" + q.UserName + ")", Value = q.UserName });
            }

            if (!String.IsNullOrEmpty(searchUsername))
            {
                ViewBag.username = new SelectList(userSelect, "Value", "Text", searchUsername);
                appLog = appLog.Where(x => x.UserId == searchUsername).ToList();
            }
            else
            {
                ViewBag.username = new SelectList(userSelect, "Value", "Text", "");
            }
            //function search userList end

            //function search date
            if (!String.IsNullOrEmpty(searchDateFrom) && !String.IsNullOrEmpty(searchDateTo))
            {
                DateTime date1 = DateTime.Parse(searchDateFrom);
                DateTime date2 = DateTime.Parse(searchDateTo);

                appLog = appLog.Where(x => x.LgDate >= date1 && x.LgDate <= date2).ToList();

                ViewData["DateFrom"] = searchDateFrom;
                ViewData["DateTo"] = searchDateTo;
                //akAkaun = akAkaun.OrderByDescending(c => c.Tarikh.Date).ThenBy(c => c.Tarikh.TimeOfDay);

            }
            //function search date end

            //function search modul range
            if (!String.IsNullOrEmpty(searchModulFrom) && !String.IsNullOrEmpty(searchModulTo))
            {
                Tuple<string, string> range = Tuple.Create(searchModulFrom, searchModulTo);
                appLog = appLog.Where(s =>
                        range.Item1.CompareTo(s.LgModule!.Substring(0, range.Item1.Length)) <= 0 &&
                        s.LgModule.Substring(0, range.Item2.Length).CompareTo(range.Item2) <= 0)
                        .ToList();

                ViewData["ModulFrom"] = searchModulFrom;
                ViewData["ModulTo"] = searchModulTo;
            }
            // function search modul range end
            return View(appLog.OrderBy(b => b.LgDate).ThenBy(b => b.LgModule).ToList());
        }

        public async Task<IActionResult> PrintPdf(
                string searchUsernamePrint,
                string searchModulFrom,
                string searchModulTo,
                string searchDateFrom,
                string searchDateTo)
        {
            var appLog = new List<AppLog>();

            if (!String.IsNullOrEmpty(searchUsernamePrint) ||
                !String.IsNullOrEmpty(searchModulFrom) ||
                !String.IsNullOrEmpty(searchModulTo) ||
                !String.IsNullOrEmpty(searchDateFrom) ||
                !String.IsNullOrEmpty(searchDateTo))
            {
                appLog = await _context.AppLog.Include(b => b.DPekerja).Where(b => b.DPekerjaId != null).ToListAsync();
            }

            if (!String.IsNullOrEmpty(searchUsernamePrint))
            {
                appLog = appLog.Where(x => x.UserId == searchUsernamePrint).ToList();
            }
            //function search userList end

            //function search date
            if (!String.IsNullOrEmpty(searchDateFrom) && !String.IsNullOrEmpty(searchDateTo))
            {
                DateTime date1 = DateTime.Parse(searchDateFrom);
                DateTime date2 = DateTime.Parse(searchDateTo);

                appLog = appLog.Where(x => x.LgDate >= date1 && x.LgDate <= date2).ToList();

                //akAkaun = akAkaun.OrderByDescending(c => c.Tarikh.Date).ThenBy(c => c.Tarikh.TimeOfDay);

            }
            //function search date end

            //function search modul range
            if (!String.IsNullOrEmpty(searchModulFrom) && !String.IsNullOrEmpty(searchModulTo))
            {
                Tuple<string, string> range = Tuple.Create(searchModulFrom, searchModulTo);
                appLog = appLog.Where(s =>
                        range.Item1.CompareTo(s.LgModule!.Substring(0, range.Item1.Length)) <= 0 &&
                        s.LgModule.Substring(0, range.Item2.Length).CompareTo(range.Item2) <= 0)
                        .ToList();

            }
            // function search modul range end

            AppLogPrintModel data = new AppLogPrintModel();

            data.CompanyDetail = await _userService.GetCompanyDetails();
            data.AppLog = appLog;

            DateTime dateFrom = Convert.ToDateTime(searchDateFrom);
            DateTime dateTo = Convert.ToDateTime(searchDateTo);

            return new ViewAsPdf("AppLogPrintPDF", data,
                new ViewDataDictionary(ViewData) {
                    { "User", searchUsernamePrint },
                    { "ModulFrom", searchModulFrom },
                    { "ModulTo", searchModulTo },
                    { "DateFrom", dateFrom.ToString("dd/MM/yyyy hh:mm:ss tt") },
                    { "DateTo", dateTo.ToString("dd/MM/yyyy hh:mm:ss tt") }
                })
            {
                PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                CustomSwitches = "--footer-center \"[page]/[toPage]\"" +
                        " --footer-line --footer-font-size \"7\" --footer-spacing 1 --footer-font-name \"Segoe UI\"",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
            };
        }
    }
}
