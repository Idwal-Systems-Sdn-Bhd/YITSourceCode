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
    public class SessionCartAkPO : CartAkPO
    {
        public static CartAkPO GetCart(IServiceProvider services)
        {
            ISession session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext!.Session!;
            SessionCartAkPO cart = session?.GetJson<SessionCartAkPO>("CartAkPO") ?? new SessionCartAkPO();
            cart.Session = session;
            return cart;
        }
        private ISession? Session { get; set; }

        // POObjek
        public override void AddItemObjek(int akPOId, int jKWPTJBahagianId, int akCartaId, decimal amaun)
        {
            base.AddItemObjek(akPOId, jKWPTJBahagianId, akCartaId, amaun);

            Session?.SetJson("CartAkPO", this);
        }

        public override void RemoveItemObjek(int jKWPTJBahagianId, int akCartaId)
        {
            base.RemoveItemObjek(jKWPTJBahagianId, akCartaId);
            Session?.SetJson("CartAkPO", this);
        }

        public override void ClearObjek()
        {
            base.ClearObjek();
            Session?.Remove("CartAkPO");
        }
        //

        //POPerihal
        public override void AddItemPerihal(int akPOId, decimal bil, string? perihal, decimal kuantiti, int? lHDNKodKlasifikasiId, int? lHDNUnitUkuranId, string? unit, EnLHDNJenisCukai enLHDNJenisCukai, decimal kadarCukai, decimal amaunCukai, decimal harga, decimal amaun)
        {
            base.AddItemPerihal(akPOId, bil, perihal, kuantiti, lHDNKodKlasifikasiId, lHDNUnitUkuranId, unit, enLHDNJenisCukai, kadarCukai, amaunCukai, harga, amaun);
            Session?.SetJson("CartAkPO", this);
        }

        public override void RemoveItemPerihal(decimal bil)
        {
            base.RemoveItemPerihal(bil);
            Session?.SetJson("CartAkPO", this);
        }

        public override void ClearPerihal()
        {
            base.ClearPerihal();
            Session?.Remove("CartAkPO");
        }
        //
    }
}
