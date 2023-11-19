using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public override void AddItemObjek(int akPOId, int jBahagianId, int akCartaId, decimal amaun)
        {
            base.AddItemObjek(akPOId, jBahagianId, akCartaId, amaun);

            Session?.SetJson("CartAkPO", this);
        }

        public override void RemoveItemObjek(int jBahagianId, int akCartaId)
        {
            base.RemoveItemObjek(jBahagianId, akCartaId);
            Session?.SetJson("CartAkPO", this);
        }

        public override void ClearObjek()
        {
            base.ClearObjek();
            Session?.Remove("CartAkPO");
        }
        //

        //POPerihal
        public override void AddItemPerihal(int akPOId, decimal bil, string? perihal, decimal kuantiti, string? unit, decimal harga, decimal amaun)
        {
            base.AddItemPerihal(akPOId, bil, perihal, kuantiti, unit, harga, amaun);
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
