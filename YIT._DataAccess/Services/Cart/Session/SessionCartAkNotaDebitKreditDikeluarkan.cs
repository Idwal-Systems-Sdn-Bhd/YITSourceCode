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
    public class SessionCartAkNotaDebitKreditDikeluarkan : CartAkNotaDebitKreditDikeluarkan
    {
        public static CartAkNotaDebitKreditDikeluarkan GetCart(IServiceProvider services)
        {
            ISession session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext!.Session!;
            SessionCartAkNotaDebitKreditDikeluarkan cart = session?.GetJson<SessionCartAkNotaDebitKreditDikeluarkan>("CartAkNotaDebitKreditDikeluarkan") ?? new SessionCartAkNotaDebitKreditDikeluarkan();
            cart.Session = session;
            return cart;
        }
        private ISession? Session { get; set; }

        // NotaDebitKreditDikeluarkanObjek
        public override void AddItemObjek(int akNotaDebitKreditDikeluarkanId, int jKWPTJBahagianId, int akCartaId, decimal amaun)
        {
            base.AddItemObjek(akNotaDebitKreditDikeluarkanId, jKWPTJBahagianId, akCartaId, amaun);

            Session?.SetJson("CartAkNotaDebitKreditDikeluarkan", this);
        }

        public override void RemoveItemObjek(int jKWPTJBahagianId, int akCartaId)
        {
            base.RemoveItemObjek(jKWPTJBahagianId, akCartaId);
            Session?.SetJson("CartAkNotaDebitKreditDikeluarkan", this);
        }

        public override void ClearObjek()
        {
            base.ClearObjek();
            Session?.Remove("CartAkNotaDebitKreditDikeluarkan");
        }
        //

        //NotaDebitKreditDikeluarkanPerihal
        public override void AddItemPerihal(int akNotaDebitKreditDikeluarkanId, decimal bil, string? perihal, decimal kuantiti, int? lHDNKodKlasifikasiId, int? lHDNUnitUkuranId, string? unit, EnLHDNJenisCukai enLHDNJenisCukai, decimal kadarCukai, decimal amaunCukai, decimal harga, decimal amaun)
        {
            base.AddItemPerihal(akNotaDebitKreditDikeluarkanId, bil, perihal, kuantiti, lHDNKodKlasifikasiId, lHDNUnitUkuranId, unit, enLHDNJenisCukai, kadarCukai, amaunCukai, harga, amaun);
            Session?.SetJson("CartAkNotaDebitKreditDikeluarkan", this);
        }

        public override void RemoveItemPerihal(decimal bil)
        {
            base.RemoveItemPerihal(bil);
            Session?.SetJson("CartAkNotaDebitKreditDikeluarkan", this);
        }

        public override void ClearPerihal()
        {
            base.ClearPerihal();
            Session?.Remove("CartAkNotaDebitKreditDikeluarkan");
        }
        //
    }
}
