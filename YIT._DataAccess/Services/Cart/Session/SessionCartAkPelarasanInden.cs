using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;

namespace YIT._DataAccess.Services.Cart.Session
{
    public class SessionCartAkPelarasanInden : CartAkPelarasanInden
    {
        public static CartAkPelarasanInden GetCart(IServiceProvider services)
        {
            ISession session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext!.Session!;
            SessionCartAkPelarasanInden cart = session?.GetJson<SessionCartAkPelarasanInden>("CartAkPelarasanInden") ?? new SessionCartAkPelarasanInden();
            cart.Session = session;
            return cart;
        }
        private ISession? Session { get; set; }

        // AkPelarasanIndenObjek
        public override void AddItemObjek(int akIndenId, int jKWPTJBahagianId, int akCartaId, decimal amaun)
        {
            base.AddItemObjek(akIndenId, jKWPTJBahagianId, akCartaId, amaun);

            Session?.SetJson("CartAkPelarasanInden", this);
        }

        public override void RemoveItemObjek(int jKWPTJBahagianId, int akCartaId)
        {
            base.RemoveItemObjek(jKWPTJBahagianId, akCartaId);
            Session?.SetJson("CartAkPelarasanInden", this);
        }

        public override void ClearObjek()
        {
            base.ClearObjek();
            Session?.Remove("CartAkPelarasanInden");
        }
        //

        // AkPelarasanIndenPerihal
        public override void AddItemPerihal(int akPelarasanIndenId, decimal bil, string? perihal, decimal kuantiti, int? lHDNKodKlasifikasiId, int? lHDNUnitUkuranId, string? unit, EnLHDNJenisCukai enLHDNJenisCukai, decimal kadarCukai, decimal amaunCukai, decimal harga, decimal amaun)
        {
            base.AddItemPerihal(akPelarasanIndenId, bil, perihal, kuantiti, lHDNKodKlasifikasiId, lHDNUnitUkuranId, unit, enLHDNJenisCukai, kadarCukai, amaunCukai, harga, amaun);
            Session?.SetJson("CartAkPelarasanInden", this);
        }

        public override void RemoveItemPerihal(decimal bil)
        {
            base.RemoveItemPerihal(bil);
            Session?.SetJson("CartAkPelarasanInden", this);
        }

        public override void ClearPerihal()
        {
            base.ClearPerihal();
            Session?.Remove("CartAkPelarasanInden");
        }
        //
    }
}
