using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations.Schema;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Bases;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class DKonfigKelulusanRepository : _GenericRepository<DKonfigKelulusan>, IDKonfigKelulusanRepository
    {
        private readonly ApplicationDbContext _context;

        public DKonfigKelulusanRepository(ApplicationDbContext context) : base(context) 
        {
            _context = context;
        }

        public List<DKonfigKelulusan> GetAllDetails()
        {
            return _context.DKonfigKelulusan.Include(p => p.DPekerja)
                .Include(p => p.JBahagian)
                    .ToList();
        }

        public DKonfigKelulusan GetAllDetailsById(int id)
        {
            return _context.DKonfigKelulusan.Include(p => p.DPekerja).Include(p => p.JBahagian).FirstOrDefault(p => p.Id == id) ?? new DKonfigKelulusan();
        }

        public List<DKonfigKelulusan> GetResultsByCategoryGroupByDPekerja(EnKategoriKelulusan enKategoriKelulusan, EnJenisModulKelulusan enJenisModulKelulusan)
        {
            var results = _context.DKonfigKelulusan
                .Include(kk => kk.DPekerja)
                .Include(kk => kk.JBahagian)
                .Where(b => b.EnKategoriKelulusan == enKategoriKelulusan)
                .Where(b => b.EnJenisModulKelulusan == enJenisModulKelulusan)
                .GroupBy(b => b.DPekerjaId).Select(l => new DKonfigKelulusan
            {
                Id = l.First().DPekerjaId,
                DPekerjaId = l.First().DPekerjaId,
                DPekerja = l.First().DPekerja,
            }).ToList();

            return results ?? new List<DKonfigKelulusan>();
        }

        public bool IsPersonAvailable(EnJenisModulKelulusan enJenisModulKelulusan, EnKategoriKelulusan enKategoriKelulusan, int jBahagianId, decimal jumlah)
        {
            return _context.DKonfigKelulusan.Any(kk => kk.EnJenisModulKelulusan == enJenisModulKelulusan && kk.EnKategoriKelulusan == enKategoriKelulusan && (kk.JBahagianId == jBahagianId || kk.JBahagianId == null));
        }

        public bool IsValidUser(int dPekerjaId, string password, EnJenisModulKelulusan enJenisModulKelulusan, EnKategoriKelulusan enKategoriKelulusan)
        {
            return _context.DKonfigKelulusan.Any(kk => kk.DPekerjaId == dPekerjaId && kk.KataLaluan == password && kk.EnJenisModulKelulusan == enJenisModulKelulusan && kk.EnKategoriKelulusan == enKategoriKelulusan);
        }
        public List<DKonfigKelulusanViewModel> GetResults(string? searchString, string? orderBy)
        {
            List<DKonfigKelulusanViewModel> vm = new List<DKonfigKelulusanViewModel>(); 

            if (searchString == null && orderBy == null)
            {
                return new List<DKonfigKelulusanViewModel>();
            }

            var dKKList = _context.DKonfigKelulusan
                .IgnoreQueryFilters()
                .Include(t => t.DPekerja)
                .Include(t => t.JBahagian) 
                .ToList();

            if (dKKList != null && dKKList.Any())
            {
                foreach (var dKK in dKKList)
                {
                    var bahagian = "SEMUA";

                    if (dKK.JBahagian != null)
                    {
                        bahagian = dKK.JBahagian.Kod + " - " + dKK.JBahagian.Perihal;
                    }
                    vm.Add(new DKonfigKelulusanViewModel
                    {
                        Id = dKK.Id,
                        EnJenisModulKelulusan = dKK.EnJenisModulKelulusan.ToString(),
                        EnKategoriKelulusan = dKK.EnKategoriKelulusan.ToString(),
                        JBahagian = bahagian,
                        DPekerja = dKK.DPekerja,
                        MinAmaun = dKK.MinAmaun,
                        MaksAmaun = dKK.MaksAmaun,
                        FlHapus = dKK.FlHapus
                    });
                }
            }

            // searchstring filters
            if (searchString != null)
            {
                vm = vm.Where(v => 
                v.JBahagian!.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                v.EnJenisModulKelulusan!.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                v.EnKategoriKelulusan!.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                v.DPekerja!.Nama!.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                v.MinAmaun!.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                v.MaksAmaun!.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)
                ).ToList();          
            }
            // searchString filters end

            // order by filters
            if (orderBy != null)
            {
                switch (orderBy)
                {
                    case "EnKategoriKelulusan":
                        vm = vm.OrderBy(v => v.EnKategoriKelulusan).ToList(); 
                        break;
                    default:
                        vm = vm.OrderBy(v => v.EnJenisModulKelulusan).ToList();
                        break;
                }

            }
            // order by filters end

            return vm;
        } 

    }

}