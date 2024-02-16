using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._01Jadual;

namespace YIT._DataAccess.Services.Cart.Session
{
    public class SessionCartJKonfigPenyata : CartJKonfigPenyata
    {
        public static CartJKonfigPenyata GetCart(IServiceProvider services)
        {
            ISession session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext.Session!;

            SessionCartJKonfigPenyata cart = session?.GetJson<SessionCartJKonfigPenyata> ("CartJKonfigPenyata") ?? new SessionCartJKonfigPenyata();
            
            cart.Session = session;
            return cart;
        }

        private ISession? Session { get; set; }

        // Baris
        public override void AddItemBaris(int bil, int jKonfigPenyataId, EnKategoriTajuk enKategoriTajuk,string? perihal, int susunan, bool isFormula,EnKategoriJumlah enKategoriJumlah, string? jumlahSusunanList, List<JKonfigPenyataBarisFormula> jKonfigPenyataBarisFormulas)
        {
            base.AddItemBaris(bil, jKonfigPenyataId, enKategoriTajuk,perihal, susunan, isFormula,enKategoriJumlah, jumlahSusunanList, jKonfigPenyataBarisFormulas);
            Session?.SetJson("CartJKonfigPenyata", this);
        }

        public override void RemoveItemBaris(int bil)
        {
            base.RemoveItemBaris(bil);
            Session?.SetJson("CartJKonfigPenyata", this);

        }

        public override void ClearBaris()
        {
            base.ClearBaris();
            Session?.Remove("CartJKonfigPenyata");
        }
        // Baris end

        // Baris Formula
        public override void AddItemBarisFormula(int barisBil, int jKonfigPenyataBarisId, EnJenisOperasi enJenisOperasi, bool isPukal, string? enJenisCartaList, bool isKecuali, string? kodList, string? setKodList)
        {
            base.AddItemBarisFormula(barisBil, jKonfigPenyataBarisId, enJenisOperasi, isPukal, enJenisCartaList, isKecuali, kodList, setKodList);
            Session?.SetJson("CartJKonfigPenyata", this);
        }

        public override void RemoveItemBarisFormula(EnJenisOperasi enJenisOperasi, int barisBil)
        {
            base.RemoveItemBarisFormula(enJenisOperasi, barisBil);
            Session?.SetJson("CartJKonfigPenyata", this);
        }

        public override void ClearBarisFormulaByBarisBil()
        {
            base.ClearBarisFormulaByBarisBil();
            Session?.SetJson("CartJKonfigPenyata", this);

        }
        public override void ClearBarisFormula()
        {
            base.ClearBarisFormula();
            Session?.Remove("CartJKonfigPenyata");

        }
        // Baris Formula end
    }
}
