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
    public class SessionCartAkPenilaianPerolehan : CartAkPenilaianPerolehan
    {
        public static CartAkPenilaianPerolehan GetCart(IServiceProvider services)
        {
            ISession session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext!.Session!;
            SessionCartAkPenilaianPerolehan cart = session?.GetJson<SessionCartAkPenilaianPerolehan>("CartAkPenilaianPerolehan") ?? new SessionCartAkPenilaianPerolehan();
            cart.Session = session;
            return cart;
        }
        private ISession? Session { get; set; }

        // PenilaianPerolehanObjek
        public override void AddItemObjek(int akPenilaianPerolehanId, int jBahagianId, int akCartaId, decimal amaun)
        {
            base.AddItemObjek(akPenilaianPerolehanId, jBahagianId, akCartaId, amaun);

            Session?.SetJson("CartAkPenilaianPerolehan", this);
        }

        public override void RemoveItemObjek(int jBahagianId, int akCartaId)
        {
            base.RemoveItemObjek(jBahagianId, akCartaId);
            Session?.SetJson("CartAkPenilaianPerolehan", this);
        }

        public override void ClearObjek()
        {
            base.ClearObjek();
            Session?.Remove("CartAkPenilaianPerolehan");
        }
        //

        //PenilaianPerolehanPerihal
        public override void AddItemPerihal(int akPenilaianPerolehanId, decimal bil, string? perihal, decimal kuantiti, int? lHDNKodKlasifikasiId, int? lHDNUnitUkuranId, string? unit, EnLHDNJenisCukai enLHDNJenisCukai, decimal kadarCukai, decimal amaunCukai, decimal harga, decimal amaun)
        {
            base.AddItemPerihal(akPenilaianPerolehanId, bil, perihal, kuantiti, lHDNKodKlasifikasiId, lHDNUnitUkuranId, unit, enLHDNJenisCukai, kadarCukai, amaunCukai, harga, amaun);
            Session?.SetJson("CartAkPenilaianPerolehan", this);
        }

        public override void RemoveItemPerihal(decimal bil)
        {
            base.RemoveItemPerihal(bil);
            Session?.SetJson("CartAkPenilaianPerolehan", this);
        }

        public override void ClearPerihal()
        {
            base.ClearPerihal();
            Session?.Remove("CartAkPenilaianPerolehan");
        }
        //
    }
}
