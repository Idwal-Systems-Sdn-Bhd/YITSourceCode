using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YIT._DataAccess.Data;

namespace YIT.Akaun.Controller
{

    [Authorize(Roles = "SuperAdmin, Admin")]
    public class RolesController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly ApplicationDbContext db;

        public RolesController(ApplicationDbContext db)
        {
            this.db=db;
        }
        public IActionResult Index()
        {
            var roles = db.Roles.ToList();
            return View(roles);
        }
    }
}
