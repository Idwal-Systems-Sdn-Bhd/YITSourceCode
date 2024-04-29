using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YIT.__Domain.Entities._Statics;
using YIT._DataAccess.Data;

namespace YIT.Akaun.Controller
{

    [Authorize(Roles = Init.superAdminAdminRole)]
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
