using YIT.__Domain.Entities.Models._03Akaun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Repositories.Interfaces
{
    public interface IAkTerimaTunggalRepository : _IGenericRepository<AkTerimaTunggal>
    {
        public List<AkTerimaTunggal> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy);
        public AkTerimaTunggal GetDetailsById(int id);
        public Task<bool> IsPostedAsync(int id, string noRujukan);
        public void PostingToAkAkaun(AkTerimaTunggal akTerimaTunggal,string userId, int? dPekerjaMasukId);
        public void RemovePostingFromAkAkaun(AkTerimaTunggal akTerimaTunggal,string userId);
        string GetMaxRefNo(string initNoRujukan, string tahun);
        bool IsLinkedWithAkPenyataPemungut(AkTerimaTunggalObjek objek);
    }
}
