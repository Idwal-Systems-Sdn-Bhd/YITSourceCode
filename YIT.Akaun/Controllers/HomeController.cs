using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Administrations;
using YIT._DataAccess.Data;
using YIT.Akaun.Infrastructure;
using YIT.Akaun.Models.ViewModels.Administrations;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.Security.Claims;
using static YIT.Akaun.Models.ViewModels.Administrations.UserClaimsViewModel;

namespace YIT.Akaun.Controller
{
    public class HomeController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly UserServices _userService;
        public HomeController(ApplicationDbContext context,
            ILogger<HomeController> logger,
            UserManager<IdentityUser> userManager,
            UserServices userService)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _userService = userService;
        }

        public IActionResult Index()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null)
            {
                return RedirectToAction(nameof(AccountController.Login), "Account");
            }
            else
            {
                if (!User.IsInRole("SuperAdmin"))
                {
                    bool IsUnderMaintainance = _userService.IsUnderMaintainance();
                    if (IsUnderMaintainance == true)
                    {
                        return View("UnderMaintainance");
                    }
                }

                dynamic dyModel = new ExpandoObject();

                return View(dyModel);
            }

        }

        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            //test
            return View();
        }

        public IActionResult UnderMaintainance()
        {
            //test
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // get details of exception features
            // errormessage, Username, StackTrace etc.
            var contextException = HttpContext.Features.Get<IExceptionHandlerFeature>();
            // get details of request feature
            // path, url requested etc.
            var contextRequest = HttpContext.Features.Get<IHttpRequestFeature>();

            ExceptionLogger logger = new ExceptionLogger();

            var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            var IsExistLogger = _context.ExceptionLogger.FirstOrDefault(b => b.TraceIdentifier == traceId);

            // error handler logs for View
            if (IsExistLogger == null)
            {
                logger.LogTime = DateTime.Now;
                logger.UserName = HttpContext.User.Identity!.Name ?? "";
                logger.TraceIdentifier = traceId;
                logger.ControllerName = ControllerContext.ActionDescriptor.DisplayName ?? "";
                logger.ExceptionMessage = contextException!.Error.Message;
                logger.ExceptionStackTrace = contextException.Error.StackTrace ?? "";
                logger.ExceptionType = contextException.Error.GetType().FullName ?? "";
                logger.Source = contextException.Error.Source ?? "";
                logger.UrlRequest = contextRequest!.RawTarget;

                _context.Add(logger);
                _context.SaveChanges();
            }


            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("/Home/HandleError/{code:int}")]
        public IActionResult HandleError(int code)
        {
            ViewData["ErrorMessage"] = $"{code}";
            return View("~/Views/Shared/HandleError.cshtml");
        }

        // printing List of Carta
        [AllowAnonymous]
        public async Task<IActionResult> PrintPermohonanCapaian()
        {

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            var existingUserClaims = await _userManager.GetClaimsAsync(user);

            var model = new UserClaimsViewModel()
            {
                UserId = user.Id
            };

            foreach (Claim claim in ClaimStore.claimList.OrderBy(b => b.Type))
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

            //string customSwitches = "--page-offset 0 --footer-center [page] / [toPage] --footer-font-size 6";

            return new ViewAsPdf("PermohonanCapaianPrintPDF", model)
            {
                PageMargins = { Left = 15, Bottom = 15, Right = 15, Top = 15 },
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--footer-center \"[page]/[toPage]\"" +
                        " --footer-line --footer-font-size \"7\" --footer-spacing 1 --footer-font-name \"Segoe UI\"",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
            };
        }
        // printing List of Carta end
    }
}
