﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._50LHDN;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface ILHDNUnitUkuranRepository : _IGenericRepository<LHDNUnitUkuran>
    {
        Task<LHDNUnitUkuran> GetByCodeAsync(string code);
    }
}