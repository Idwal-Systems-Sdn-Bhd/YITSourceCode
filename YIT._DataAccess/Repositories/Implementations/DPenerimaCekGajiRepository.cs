using Microsoft.EntityFrameworkCore;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class DPenerimaCekGajiRepository : _GenericRepository<DPenerimaCekGaji>, IDPenerimaCekGajiRepository
    {
        private readonly ApplicationDbContext _context;

        public DPenerimaCekGajiRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public DPenerimaCekGaji GetAllDetailsById(int id)
        {
            return _context.DPenerimaCekGaji
                .Include(jak => jak.DDaftarAwam)
                .Include(jak => jak.JCaraBayar)
                .Include(jak => jak.JCawangan)



                .Where(jak => jak.Id == id).FirstOrDefault() ?? new DPenerimaCekGaji();
        }
        public List<DPenerimaCekGaji> GetAllDetails()
        {
            return _context.DPenerimaCekGaji
                 .Include(df => df.DDaftarAwam)
                 .Include(df => df.JCaraBayar)
                 .Include(df => df.JCawangan)


                 .ToList();
        }
        public List<AkJanaanProfil> GetAllDetailsById()
        {
            return _context.AkJanaanProfil
                 .Include(df => df.NoRujukan)
                 .Include(df => df.EnJenisModulProfil)
                 .Include(df => df.Tarikh)
                 .Include(df => df.Jumlah)

                 .ToList();
        }

        public string GetMaxRefNo()
        {
            var max = _context.DPenerimaCekGaji.OrderByDescending(df => df.Kod).ToList();

            if (max != null)
            {

                var refNo = max.FirstOrDefault()?.Kod;
                return refNo ?? "";
            }
            else
            {
                return "";
            }

        }
    }
}
