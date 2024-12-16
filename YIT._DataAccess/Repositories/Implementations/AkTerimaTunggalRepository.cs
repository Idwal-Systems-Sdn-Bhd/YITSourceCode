using Microsoft.EntityFrameworkCore;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class AkTerimaTunggalRepository : _GenericRepository<AkTerimaTunggal>, IAkTerimaTunggalRepository
    {
        private readonly ApplicationDbContext _context;

        public AkTerimaTunggalRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public string GetMaxRefNo(string initNoRujukan, string tahun)
        {
            var max = _context.AkTerimaTunggal.Where(pp => pp.Tahun == tahun).OrderByDescending(pp => pp.NoRujukan).ToList();

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


        public List<AkTerimaTunggal> GetResults(string? searchString, DateTime? dateFrom, DateTime? dateTo, string? orderBy)
        {
            if (searchString == null && dateFrom == null && dateTo == null && orderBy == null)
            {
                return new List<AkTerimaTunggal>();
            }

            var akTerimaTunggal = _context.AkTerimaTunggal
                .IgnoreQueryFilters()
                .Include(t => t.JKW)
                .Include(t => t.DPekerjaPosting)
                .Include(t => t.JNegeri)
                .Include(t => t.JCawangan)
                .Include(t => t.AkBank)
                    .ThenInclude(b => b!.JBank)
                .Include(t => t.AkBank)
                    .ThenInclude(b => b!.JKW)
                .Include(t => t.AkBank)
                    .ThenInclude(b => b!.AkCarta)
                .Include(t => t.DDaftarAwam)
                .Include(tcb => tcb.JCaraBayar)
                //.Include(t => t.AkTerimaTunggalObjek)!
                //    .ThenInclude(to => to.AkCarta)
                //.Include(t => t.AkTerimaTunggalObjek)!
                //    .ThenInclude(to => to.JKWPTJBahagian)
                //        .ThenInclude(b => b!.JKW)
                .Include(t => t.AkTerimaTunggalObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                //        .ThenInclude(b => b!.JPTJ)
                //.Include(t => t.AkTerimaTunggalObjek)!
                //    .ThenInclude(to => to.JKWPTJBahagian)
                //        .ThenInclude(b => b!.JBahagian)
                .ToList();

            // searchstring filters
            if (searchString != null)
            {
                akTerimaTunggal = akTerimaTunggal.Where(t => 
                t.Nama!.Contains(searchString, StringComparison.OrdinalIgnoreCase) 
                || t.NoRujukan!.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            // searchString filters end

            // date filters
            if (dateFrom != null && dateTo != null)
            {
                akTerimaTunggal = akTerimaTunggal.Where(t => t.Tarikh >= dateFrom && t.Tarikh <= dateTo.Value.AddHours(23.99)).ToList();
            }
            // date filters end

            // order by filters
            if (orderBy != null)
            {
                switch (orderBy)
                {
                    case "Nama":
                        akTerimaTunggal = akTerimaTunggal.OrderBy(t => t.Nama).ToList();
                        break;
                    case "Tarikh":
                        akTerimaTunggal = akTerimaTunggal.OrderBy(t => t.Tarikh).ToList(); break;
                    default:
                        akTerimaTunggal = akTerimaTunggal.OrderBy(t => t.NoRujukan).ToList();
                        break;
                }
                    
            }
            // order by filters end

            return akTerimaTunggal;
        }

        public List<AkTerimaTunggal> GetResults1(DateTime? dateFrom, DateTime? dateTo, int? jCawanganId, int? jKWId)
        {
            if (dateFrom == null && dateTo == null && jCawanganId == null && jKWId == null)
            {
                return new List<AkTerimaTunggal>();
            }

            var akTerimaTunggal = _context.AkTerimaTunggal
                .IgnoreQueryFilters()
                .Include(t => t.JKW)
                .Include(t => t.DPekerjaPosting)
                .Include(t => t.JNegeri)
                .Include(t => t.JCawangan)
                .Include(t => t.AkBank)
                    .ThenInclude(b => b!.JBank)
                .Include(t => t.AkBank)
                    .ThenInclude(b => b!.JKW)
                .Include(t => t.AkBank)
                    .ThenInclude(b => b!.AkCarta)
                .Include(t => t.DDaftarAwam)
                .Include(tcb => tcb.JCaraBayar)
                //.Include(t => t.AkTerimaTunggalObjek)!
                //    .ThenInclude(to => to.AkCarta)
                //.Include(t => t.AkTerimaTunggalObjek)!
                //    .ThenInclude(to => to.JKWPTJBahagian)
                //        .ThenInclude(b => b!.JKW)
                .Include(t => t.AkTerimaTunggalObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                //        .ThenInclude(b => b!.JPTJ)
                //.Include(t => t.AkTerimaTunggalObjek)!
                //    .ThenInclude(to => to.JKWPTJBahagian)
                //        .ThenInclude(b => b!.JBahagian)
                .ToList();

            // date filters
            if (dateFrom != null && dateTo != null)
            {
                akTerimaTunggal = akTerimaTunggal.Where(t => t.Tarikh >= dateFrom && t.Tarikh <= dateTo.Value.AddHours(23.99)).ToList();
            }
            // date filters end

            if (jCawanganId != null)
            {
                akTerimaTunggal = akTerimaTunggal.Where(p => p.JCawanganId == jCawanganId).ToList();
            }

            if (jKWId != null)
            {
                akTerimaTunggal = akTerimaTunggal.Where(p => p.JKWId == jKWId).ToList();
            }

            return akTerimaTunggal;
        }

        public AkTerimaTunggal GetDetailsById(int id)
        {
            return _context.AkTerimaTunggal.Include(t => t.JKW)
                .Include(t => t.DPekerjaPosting)
                .Include(t => t.JNegeri)
                .Include(t => t.JCawangan)
                .Include(t => t.AkBank)
                    .ThenInclude(b => b!.JBank)
                .Include(t => t.AkBank)
                    .ThenInclude(b => b!.JKW)
                .Include(t => t.AkBank)
                    .ThenInclude(b => b!.AkCarta)
                .Include(t => t.DDaftarAwam)
                .Include(tcb => tcb.JCaraBayar)
                .Include(t => t.AkTerimaTunggalObjek)!
                    .ThenInclude(to => to.AkCarta)
                .Include(t => t.AkTerimaTunggalObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JKW)
                .Include(t => t.AkTerimaTunggalObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JPTJ)
                .Include(t => t.AkTerimaTunggalObjek)!
                    .ThenInclude(to => to.JKWPTJBahagian)
                        .ThenInclude(b => b!.JBahagian)
                .Include(t => t.AkTerimaTunggalInvois)!
                    .ThenInclude(t => t.AkInvois)
                    .FirstOrDefault(t => t.Id == id) ?? new AkTerimaTunggal();
        }

        public async Task<List<_AkTerimaTunggalResult>> GetResultsGroupByTarikh(string? tarikhDari, string? tarikhHingga, int? jCawanganId)
        {
            if (tarikhDari == null || tarikhHingga == null || jCawanganId == null)
            {
                return new List<_AkTerimaTunggalResult>();
            }

            DateTime date1 = DateTime.Parse(tarikhDari).Date;
            DateTime date2 = DateTime.Parse(tarikhHingga).Date.AddDays(1).AddTicks(-1);

            var akTT = await _context.AkTerimaTunggal
                .Where(b => b.Tarikh >= date1 && b.Tarikh <= date2 && b.JCawanganId == jCawanganId)
                .OrderBy(a => a.Tarikh)
                .ThenBy(a => a.NoRujukan)
                .ToListAsync();

            var akTerimaTunggalResults = new List<_AkTerimaTunggalResult>();

            if (akTT.Any())
            {
                var result = new _AkTerimaTunggalResult
                {
                    FirstNoRujukan = akTT.FirstOrDefault()?.NoRujukan,
                    LastNoRujukan = akTT.LastOrDefault()?.NoRujukan,
                    ResitDibatalkan = akTT.Where(r => r.FlBatal == 1).ToList()
                };

                if (!string.IsNullOrWhiteSpace(result.FirstNoRujukan) || !string.IsNullOrWhiteSpace(result.LastNoRujukan) ||
                    result.ResitDibatalkan.Any())
                {
                    akTerimaTunggalResults.Add(result);
                }
            }

            return akTerimaTunggalResults;
        }

        public async Task<List<AkTerimaTunggal>> GetResultsGroupByTarikh1(string? tarikhDari, string? tarikhHingga, int? jCawanganId, int? jKWId)
        {
            if (tarikhDari == null || tarikhHingga == null || jCawanganId == null || jKWId == null)
            {
                return new List<AkTerimaTunggal>();
            }

            DateTime date1 = DateTime.Parse(tarikhDari).Date;
            DateTime date2 = DateTime.Parse(tarikhHingga).Date.AddDays(1).AddTicks(-1);

            var akTT = await _context.AkTerimaTunggal
                .Where(b => b.FlBatal == 1 && b.Tarikh >= date1 && b.Tarikh <= date2 && b.JCawanganId == jCawanganId && b.JKWId == jKWId)
                .OrderBy(a => a.Tarikh)
                .ThenBy(a => a.NoRujukan)
                .ToListAsync();

            return akTT;
        }

        public async Task<bool> IsPostedAsync(int id, string noRujukan)
        {
            bool isPosted = await _context.AkTerimaTunggal.AnyAsync(t => t.Id == id && t.FlPosting == 1);
            if (isPosted)
            {
                return true;
            }

            bool isExistInAkAkaun = await _context.AkAkaun.AnyAsync(t => t.NoRujukan == noRujukan);

            if (isExistInAkAkaun)
            {
                return true;
            }

            return false;
        }

        public void PostingToAkAkaun(AkTerimaTunggal akTerimaTunggal, string userId, int? dPekerjaMasukId)
        {
            PostingToAkAnggarLejar(akTerimaTunggal, userId, dPekerjaMasukId);

            List<AkAkaun> akaunList = new List<AkAkaun>();

            int? akAkaunPenghutangId = null;
            if (akTerimaTunggal.AkTerimaTunggalInvois != null && akTerimaTunggal.AkTerimaTunggalInvois.Any())
            {
                
                foreach (var item in akTerimaTunggal.AkTerimaTunggalInvois)
                {
                    akAkaunPenghutangId = item.AkInvois?.AkAkaunAkruId;
                }

                if (akTerimaTunggal.AkTerimaTunggalObjek != null)
                {
                    foreach (var t in akTerimaTunggal.AkTerimaTunggalObjek)
                    {
                        if (akAkaunPenghutangId != null)
                        {
                            var akAkaun1 = new AkAkaun();

                            akAkaun1.JKWId = t.JKWPTJBahagian?.JKWId ?? akTerimaTunggal.JKWId;
                            akAkaun1.JPTJId = t.JKWPTJBahagian!.JPTJId;
                            akAkaun1.JBahagianId = t.JKWPTJBahagian.JBahagianId;
                            akAkaun1.AkCarta1Id = akTerimaTunggal.AkBank!.AkCartaId;
                            akAkaun1.AkCarta2Id = (int)akAkaunPenghutangId;
                            akAkaun1.NoRujukan = akTerimaTunggal.NoRujukan;
                            akAkaun1.Tarikh = akTerimaTunggal.Tarikh;
                            akAkaun1.Debit = t.Amaun;
                            akAkaun1.Kredit = 0;
                            akAkaun1.UserId = userId;
                            akAkaun1.DPekerjaMasukId = dPekerjaMasukId;
                            akAkaun1.TarMasuk = DateTime.Now;
                            akAkaun1.NoSlip = akTerimaTunggal.NoSlip;
                            akAkaun1.TarikhSlip = akTerimaTunggal.TarikhSlip;

                            akaunList.Add(akAkaun1);

                            var akAkaun2 = new AkAkaun();

                            akAkaun2.JKWId = t.JKWPTJBahagian?.JKWId ?? akTerimaTunggal.JKWId;
                            akAkaun2.JPTJId = t.JKWPTJBahagian!.JPTJId;
                            akAkaun2.JBahagianId = t.JKWPTJBahagian.JBahagianId;
                            akAkaun2.AkCarta1Id = (int)akAkaunPenghutangId;
                            akAkaun2.AkCarta2Id = akTerimaTunggal.AkBank!.AkCartaId;
                            akAkaun2.NoRujukan = akTerimaTunggal.NoRujukan;
                            akAkaun2.Tarikh = akTerimaTunggal.Tarikh;
                            akAkaun2.Debit = 0;
                            akAkaun2.Kredit = t.Amaun;
                            akAkaun2.UserId = userId;
                            akAkaun2.DPekerjaMasukId = dPekerjaMasukId;
                            akAkaun2.TarMasuk = DateTime.Now;
                            akAkaun2.NoSlip = akTerimaTunggal.NoSlip;
                            akAkaun2.TarikhSlip = akTerimaTunggal.TarikhSlip;

                            akaunList.Add(akAkaun2);
                        }
                        
                    }

                }
            }
            else
            {
                if (akTerimaTunggal.AkTerimaTunggalObjek != null)
                {
                    foreach (var t in akTerimaTunggal.AkTerimaTunggalObjek)
                    {
                        var akAkaun1 = new AkAkaun();

                        akAkaun1.JKWId = t.JKWPTJBahagian?.JKWId ?? akTerimaTunggal.JKWId;
                        akAkaun1.JPTJId = t.JKWPTJBahagian!.JPTJId;
                        akAkaun1.JBahagianId = t.JKWPTJBahagian.JBahagianId;
                        akAkaun1.AkCarta1Id = akTerimaTunggal.AkBank!.AkCartaId;
                        akAkaun1.AkCarta2Id = t.AkCartaId;
                        akAkaun1.NoRujukan = akTerimaTunggal.NoRujukan;
                        akAkaun1.Tarikh = akTerimaTunggal.Tarikh;
                        akAkaun1.Debit = t.Amaun;
                        akAkaun1.Kredit = 0;
                        akAkaun1.UserId = userId;
                        akAkaun1.DPekerjaMasukId = dPekerjaMasukId;
                        akAkaun1.TarMasuk = DateTime.Now;
                        akAkaun1.NoSlip = akTerimaTunggal.NoSlip;
                        akAkaun1.TarikhSlip = akTerimaTunggal.TarikhSlip;

                        akaunList.Add(akAkaun1);

                        var akAkaun2 = new AkAkaun();

                        akAkaun2.JKWId = t.JKWPTJBahagian?.JKWId ?? akTerimaTunggal.JKWId;
                        akAkaun2.JPTJId = t.JKWPTJBahagian!.JPTJId;
                        akAkaun2.JBahagianId = t.JKWPTJBahagian.JBahagianId;
                        akAkaun2.AkCarta1Id = t.AkCartaId;
                        akAkaun2.AkCarta2Id = akTerimaTunggal.AkBank!.AkCartaId;
                        akAkaun2.NoRujukan = akTerimaTunggal.NoRujukan;
                        akAkaun2.Tarikh = akTerimaTunggal.Tarikh;
                        akAkaun2.Debit = 0;
                        akAkaun2.Kredit = t.Amaun;
                        akAkaun2.UserId = userId;
                        akAkaun2.DPekerjaMasukId = dPekerjaMasukId;
                        akAkaun2.TarMasuk = DateTime.Now;
                        akAkaun2.NoSlip = akTerimaTunggal.NoSlip;
                        akAkaun2.TarikhSlip = akTerimaTunggal.TarikhSlip;

                        akaunList.Add(akAkaun2);
                    }

                }
            }
            

            _context.AkAkaun.AddRange(akaunList);

            // update akTerimaTunggal posting fields
            akTerimaTunggal.DPekerjaPostingId = dPekerjaMasukId;
            akTerimaTunggal.TarikhPosting = DateTime.Now;
            akTerimaTunggal.FlPosting = 1;
            akTerimaTunggal.EnStatusBorang = EnStatusBorang.Lulus;

            _context.AkTerimaTunggal.Update(akTerimaTunggal);
        }

        public void RemovePostingFromAkAkaun(AkTerimaTunggal akTerimaTunggal, string userId)
        {
            List<AkAkaun> akaunList = _context.AkAkaun.Where(ak => ak.NoRujukan == akTerimaTunggal.NoRujukan).ToList();

            if (akaunList.Count > 0)
            {
                _context.AkAkaun.RemoveRange(akaunList);
            }

            // update akTerimaTunggal posting fields
            akTerimaTunggal.DPekerjaPostingId = null;
            akTerimaTunggal.TarikhPosting = null;
            akTerimaTunggal.FlPosting = 0;
            akTerimaTunggal.EnStatusBorang = EnStatusBorang.None;

            akTerimaTunggal.UserIdKemaskini = userId;
            akTerimaTunggal.TarKemaskini = DateTime.Now;

            _context.AkTerimaTunggal.Update(akTerimaTunggal);
        }

        public void PostingToAkAnggarLejar(AkTerimaTunggal akTerimaTunggal, string userId, int? dPekerjaMasukId)
        {
            List<AkAnggarLejar> anggarList = new List<AkAnggarLejar>();
            if (akTerimaTunggal.AkTerimaTunggalObjek != null)
            {
                foreach (var t in akTerimaTunggal.AkTerimaTunggalObjek)
                {
                    var akAnggarLejar = new AkAnggarLejar();

                    akAnggarLejar.Tahun = akTerimaTunggal.Tahun;
                    akAnggarLejar.NoRujukan = akTerimaTunggal.NoRujukan;
                    akAnggarLejar.Tarikh = akTerimaTunggal.Tarikh;
                    akAnggarLejar.JKWPTJBahagianId = t.JKWPTJBahagianId;
                    akAnggarLejar.AkCartaId = t.AkCartaId;
                    akAnggarLejar.Amaun = t.Amaun;
                    akAnggarLejar.Baki = -t.Amaun;

                    anggarList.Add(akAnggarLejar);

                }

            }

            _context.AkAnggarLejar.AddRange(anggarList);

        }


        public bool IsLinkedWithAkPenyataPemungut(AkTerimaTunggalObjek objek)
        {
            var akPenyataPemungutObjek = _context.AkPenyataPemungutObjek
                .Include(tto => tto.AkPenyataPemungut)
                .Include(tto => tto.AkTerimaTunggal)
                    .ThenInclude(tt => tt!.AkTerimaTunggalObjek)
                .Where(o => o.AkTerimaTunggalId == objek.AkTerimaTunggalId && o.JKWPTJBahagianId == objek.JKWPTJBahagianId && o.AkCartaId == objek.AkCartaId 
                && o.AkPenyataPemungut!.FlHapus == 0 && o.AkPenyataPemungut.FlBatal == 0)
                .FirstOrDefault();

            if (akPenyataPemungutObjek != null && akPenyataPemungutObjek.AkPenyataPemungut != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}