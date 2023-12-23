using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class AkJanaanProfilRepository : _GenericRepository<AkJanaanProfil>, IAkJanaanProfilRepository
    {
        private readonly ApplicationDbContext _context;

        public AkJanaanProfilRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public AkJanaanProfil GetDetailsById(int id)
        {
            return _context.AkJanaanProfil
                .IgnoreQueryFilters()
                .Include(p => p.JCawangan)
                .Include(j => j.AkJanaanProfilPenerima)!
                    .ThenInclude(p => p.DPenerimaZakat)
                .Include(j => j.AkJanaanProfilPenerima)!
                    .ThenInclude(p => p.DDaftarAwam)
                .Include(j => j.AkJanaanProfilPenerima)!
                    .ThenInclude(p => p.DPekerja)
                .Include(j => j.AkJanaanProfilPenerima)!
                    .ThenInclude(p => p.JCaraBayar)
                .Include(j => j.AkJanaanProfilPenerima)!
                    .ThenInclude(p => p.JBank)
                .Include(j => j.AkJanaanProfilPenerima)!
                    .ThenInclude(p => p.AkRekup)
                .FirstOrDefault(j => j.Id == id) ?? new AkJanaanProfil();
        }

        public string GetMaxRefNo(string initNoRujukan, string tahun)
        {
            //var date = new DateTime(int.Parse(tahun), 1, 1);
            var max = _context.AkJanaanProfil.Where(pp => pp.Tarikh.Year == int.Parse(tahun)).OrderByDescending(pp => pp.NoRujukan).ToList();

            if (max != null)
            {
                var refNo = max.FirstOrDefault()?.NoRujukan?.Substring(8, 5);
                return refNo ?? "";
            }
            else
            {
                return "";
            }
        }

        public List<AkJanaanProfil> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy, EnStatusBorang enStatusBorang)
        {
            if (searchString == null && dateFrom == null && dateTo == null && orderBy == null)
            {
                return new List<AkJanaanProfil>();
            }

            var akJanaanProfilList = _context.AkJanaanProfil
                .IgnoreQueryFilters()
                .Include(p => p.JCawangan)
                .Include(j => j.AkJanaanProfilPenerima)!
                    .ThenInclude(p => p.DPenerimaZakat)
                .Include(j => j.AkJanaanProfilPenerima)!
                    .ThenInclude(p => p.DDaftarAwam)
                .Include(j => j.AkJanaanProfilPenerima)!
                    .ThenInclude(p => p.DPekerja)
                .Include(j => j.AkJanaanProfilPenerima)!
                    .ThenInclude(p => p.JCaraBayar)
                .Include(j => j.AkJanaanProfilPenerima)!
                    .ThenInclude(p => p.JBank)
                .Include(j => j.AkJanaanProfilPenerima)!
                    .ThenInclude(p => p.AkRekup)
                .ToList();

            // searchstring filters
            if (searchString != null)
            {
                akJanaanProfilList = akJanaanProfilList.Where(t =>
                t.NoRujukan!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            // searchString filters end

            // order by filters
            if (orderBy != null)
            {
                switch (orderBy)
                {
                    case "Tarikh":
                        akJanaanProfilList = akJanaanProfilList.OrderBy(t => t.Tarikh).ToList(); break;
                    default:
                        akJanaanProfilList = akJanaanProfilList.OrderBy(t => t.NoRujukan).ToList();
                        break;
                }
            }
            // order by filters end

            return akJanaanProfilList;
        }
    }
}
