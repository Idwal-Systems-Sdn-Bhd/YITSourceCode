using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Administrations;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    internal class _ApplicationUserRepository : _IApplicationUserRepository
    {
        private readonly ApplicationDbContext context;

        public _ApplicationUserRepository(ApplicationDbContext context)
        {
            this.context = context;
        }
        public ApplicationUser GetApplicationUser(string id)
        {
            return context.ApplicationUsers.FirstOrDefault(b => b.Id == id) ?? new ApplicationUser();
        }
    }
}
