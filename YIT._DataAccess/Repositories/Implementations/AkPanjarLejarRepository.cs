using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;

namespace YIT._DataAccess.Repositories.Implementations
{
    public class AkPanjarLejarRepository : IAkPanjarLejarRepository
    {
        private readonly ApplicationDbContext _context;

        public AkPanjarLejarRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void UpdateRange(List<AkPanjarLejar> akPanjarLejarList)
        {
            _context.AkPanjarLejar.UpdateRange(akPanjarLejarList);
        }
        public AkPanjarLejar GetDetailsLastByDPanjarId(int dPanjarId)
        {
            var panjarLejar = new AkPanjarLejar();

            List<AkPanjarLejar> panjarLejarList = _context.AkPanjarLejar.Where(r => r.DPanjarId == dPanjarId && r.NoRujukan != "BAKI AWAL").OrderBy(r => r.NoRujukan).ToList();

            if (panjarLejarList.Any())
            {
                foreach (var item in panjarLejarList) panjarLejar = item;
            }
            return panjarLejar;
        }
        public async Task<bool> IsExistAsync(Expression<Func<AkPanjarLejar, bool>> predicate)
        {
            return await _context.AkPanjarLejar.AnyAsync(predicate);
        }
        public  List<AkPanjarLejar> GetListByDPanjarId(int dPanjarId)
        {
            List<AkPanjarLejar> lejar = new List<AkPanjarLejar>();

            if (dPanjarId != 0) 
            {
                // baki awal
                List<AkPanjarLejar> panjarLejar = _context.AkPanjarLejar
                    .Include(b => b.AkCarta)
                    .Include(b => b.DPanjar)
                    .Include(b => b.AkRekup)
                    .Include(b => b.AkPV)
                    .Include(b => b.AkCV)
                    .Include(b => b.AkJurnal)
                    .Where(b => b.DPanjarId == dPanjarId && b.AkRekup!.NoRujukan == "BAKI AWAL")
                    .OrderBy(b => b.Tarikh)
                    .ToList();

                lejar.AddRange(panjarLejar);

                // rekupan
                List<AkPanjarLejar> panjarLejarRekup = _context.AkPanjarLejar
                    .Include(b => b.AkCarta)
                    .Include(b => b.DPanjar)
                    .Include(b => b.AkRekup)
                    .Include(b => b.AkPV)
                    .Include(b => b.AkCV)
                    .Include(b => b.AkJurnal)
                    .Where(b => b.DPanjarId == dPanjarId && b.AkRekup!.NoRujukan != "BAKI AWAL" && b.AkRekup!.NoRujukan != null)
                    .OrderBy(b => b.AkRekup!.NoRujukan).ThenBy(b => b.Tarikh)
                    .ToList();

                if (panjarLejarRekup != null && panjarLejarRekup.Count() > 0)
                {
                    foreach (var item in panjarLejarRekup)
                    {
                        item.NoRekup = item.AkRekup?.NoRujukan;

                        var noRujukan = "";
                        var butiran = "";

                        if (item.AkPV != null)
                        {
                            noRujukan = item.AkPV.NoRujukan;
                            butiran = item.AkPV.NamaPenerima;
                        }
                        if (item.AkCV != null)
                        {
                            noRujukan = item.AkCV.NoRujukan;
                            butiran = item.AkCV.NamaPenerima;
                        }
                        if (item.AkJurnal != null)
                        {
                            noRujukan = item.AkJurnal.NoRujukan;
                            butiran = item.AkJurnal.Ringkasan?.Substring(0,100);
                        }

                        item.NoRujukan = noRujukan;
                        item.Butiran = butiran;
                    }

                    lejar.AddRange(panjarLejarRekup);
                }
                // belum rekup
                List<AkPanjarLejar> panjarLejarBelumRekup = _context.AkPanjarLejar
                    .Include(b => b.AkCarta)
                    .Include(b => b.DPanjar)
                    .Include(b => b.AkRekup)
                    .Include(b => b.AkPV)
                    .Include(b => b.AkCV)
                    .Include(b => b.AkJurnal)
                    .Where(b => b.DPanjarId == dPanjarId && b.AkRekup!.NoRujukan == null)
                    .OrderBy(b => b.Tarikh)
                    .ToList();

                if (panjarLejarBelumRekup != null && panjarLejarBelumRekup.Count() > 0)
                {
                    foreach (var item in panjarLejarBelumRekup)
                    {
                        var noRujukan = "";

                        if (item.AkPV != null)
                        {
                            noRujukan = item.AkPV.NoRujukan;
                        }
                        if (item.AkCV != null)
                        {
                            noRujukan = item.AkCV.NoRujukan;
                        }
                        if (item.AkJurnal != null)
                        {
                            noRujukan = item.AkJurnal.NoRujukan;
                        }

                        item.NoRujukan = noRujukan;
                    }

                    lejar.AddRange(panjarLejarBelumRekup);
                }

            }

            return lejar;
        }
    }
}
