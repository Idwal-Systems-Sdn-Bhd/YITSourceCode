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
    public class SessionCartAkBelian : CartAkBelian
    {
        public static CartAkBelian GetCart(IServiceProvider services)
        {
            ISession session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext!.Session!;
            SessionCartAkBelian cart = session?.GetJson<SessionCartAkBelian>("CartAkBelian") ?? new SessionCartAkBelian();
            cart.Session = session;
            return cart;
        }
        private ISession? Session { get; set; }

        // BelianObjek
        public override void AddItemObjek(int akBelianId, int jKWPTJBahagianId, int akCartaId,int? jCukaiId, decimal amaun)
        {
            base.AddItemObjek(akBelianId, jKWPTJBahagianId, akCartaId, jCukaiId, amaun);

            Session?.SetJson("CartAkBelian", this);
        }

        public override void RemoveItemObjek(int jKWPTJBahagianId, int akCartaId)
        {
            base.RemoveItemObjek(jKWPTJBahagianId, akCartaId);
            Session?.SetJson("CartAkBelian", this);
        }

        public override void ClearObjek()
        {
            base.ClearObjek();
            Session?.Remove("CartAkBelian");
        }
        //

        //BelianPerihal
        public override void AddItemPerihal(int akBelianId, decimal bil, string? perihal, decimal kuantiti, int? lHDNKodKlasifikasiId, int? lHDNUnitUkuranId, string? unit, EnLHDNJenisCukai enLHDNJenisCukai, decimal kadarCukai, decimal amaunCukai, decimal harga, decimal amaun)
        {
            base.AddItemPerihal(akBelianId, bil, perihal, kuantiti, lHDNKodKlasifikasiId, lHDNUnitUkuranId, unit, enLHDNJenisCukai, kadarCukai, amaunCukai, harga, amaun);
            Session?.SetJson("CartAkBelian", this);
        }

        public override void RemoveItemPerihal(decimal bil)
        {
            base.RemoveItemPerihal(bil);
            Session?.SetJson("CartAkBelian", this);
        }

        public override void ClearPerihal()
        {
            base.ClearPerihal();
            Session?.Remove("CartAkBelian");
        }
        //
    }
}
