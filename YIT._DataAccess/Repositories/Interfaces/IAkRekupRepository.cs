﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IAkRekupRepository : _IGenericRepository<AkRekup>
    {
        List<AkRekup> GetAllDetails();
        List<AkRekup> GetAllFilteredBy(bool isLinked);
        AkRekup GetDetailsByBakiAwalAndDPanjarId(string noRujukan, int dPanjarId, bool isLinked);
        AkRekup GetDetailsById(int id);
    }
}
