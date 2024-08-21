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
    public class SessionCartAkNotaMinta : CartAkNotaMinta
    {
        public static CartAkNotaMinta GetCart(IServiceProvider services)
        {
            ISession session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext!.Session!;
            SessionCartAkNotaMinta cart = session?.GetJson<SessionCartAkNotaMinta>("CartAkNotaMinta") ?? new SessionCartAkNotaMinta();
            cart.Session = session;
            return cart;
        }
        private ISession? Session { get; set; }

        // NotaMintaObjek
        public override void AddItemObjek(int akNotaMintaId, int jBahagianId, int akCartaId, decimal amaun)
        {
            base.AddItemObjek(akNotaMintaId, jBahagianId, akCartaId, amaun);

            Session?.SetJson("CartAkNotaMinta", this);
        }

        public override void RemoveItemObjek(int jBahagianId, int akCartaId)
        {
            base.RemoveItemObjek(jBahagianId, akCartaId);
            Session?.SetJson("CartAkNotaMinta", this);
        }

        public override void ClearObjek()
        {
            base.ClearObjek();
            Session?.Remove("CartAkNotaMinta");
        }
        //

        //NotaMintaPerihal
        public override void AddItemPerihal(int akNotaMintaId, decimal bil, string? perihal, decimal kuantiti, int? lHDNKodKlasifikasiId, int? lHDNUnitUkuranId, string? unit, EnLHDNJenisCukai enLHDNJenisCukai, decimal kadarCukai, decimal amaunCukai, decimal harga, decimal amaun)
        {
            base.AddItemPerihal(akNotaMintaId, bil, perihal, kuantiti, lHDNKodKlasifikasiId, lHDNUnitUkuranId, unit, enLHDNJenisCukai, kadarCukai, amaunCukai, harga, amaun);
            Session?.SetJson("CartAkNotaMinta", this);
        }

        public override void RemoveItemPerihal(decimal bil)
        {
            base.RemoveItemPerihal(bil);
            Session?.SetJson("CartAkNotaMinta", this);
        }

        public override void ClearPerihal()
        {
            base.ClearPerihal();
            Session?.Remove("CartAkNotaMinta");
        }
        //
    }
}
