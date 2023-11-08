using EntityFrameworkCore.Triggered;
using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities.Bases;
using YIT._DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Services
{
    public class SoftDeleteTrigger : IBeforeSaveTrigger<IGenericFields>
    {
        private readonly ApplicationDbContext _context;

        public SoftDeleteTrigger(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task BeforeSave(ITriggerContext<IGenericFields> context, CancellationToken cancellationToken)
        {
            if (context.ChangeType == ChangeType.Deleted)
            {
                var entry = _context.Entry(context.Entity);
                context.Entity.TarHapus = DateTime.UtcNow;
                context.Entity.FlHapus = 1;
                entry.State = EntityState.Modified;
            }
            await Task.CompletedTask;
        }
    }
}
