using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
                    .Where(b => b.DPanjarId == dPanjarId && b.AkRekup!.NoRujukan == "BAKI AWAL")
                    .OrderBy(b => b.Tarikh)
                    .ToList();

                lejar.AddRange(panjarLejar);

                // rekupan
                List<AkPanjarLejar> panjarLejarRekup = _context.AkPanjarLejar
                    .Include(b => b.AkCarta)
                    .Include(b => b.DPanjar)
                    .Include(b => b.AkRekup)
                    .Where(b => b.DPanjarId == dPanjarId && b.AkRekup!.NoRujukan != "BAKI AWAL" && b.AkRekup!.NoRujukan != null)
                    .OrderBy(b => b.AkRekup!.NoRujukan).ThenBy(b => b.Tarikh)
                    .ToList();

                if (panjarLejar != null)
                {
                    foreach (var item in panjarLejar)
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

                    lejar.AddRange(panjarLejarRekup);
                }
                // belum rekup
                List<AkPanjarLejar> panjarLejarBelumRekup = _context.AkPanjarLejar
                    .Include(b => b.AkCarta)
                    .Include(b => b.DPanjar)
                    .Include(b => b.AkRekup)
                    .Where(b => b.DPanjarId == dPanjarId && b.AkRekup!.NoRujukan == null)
                    .OrderBy(b => b.Tarikh)
                    .ToList();

                if (panjarLejarBelumRekup != null)
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
