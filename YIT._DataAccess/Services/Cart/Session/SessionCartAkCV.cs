using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Services.Cart.Session
{
    public class SessionCartAkCV : CartAkCV
    {
        public static CartAkCV GetCart(IServiceProvider services)
        {
            ISession session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext!.Session!;
            SessionCartAkCV cart = session?.GetJson<SessionCartAkCV>("CartAkCV") ?? new SessionCartAkCV();
            cart.Session = session;
            return cart;
        }

        private ISession? Session { get; set; }

        // CVObjek
        public override void AddItemObjek(int akCVId, int jKWPTJBahagianId, int akCartaId, decimal amaun)
        {
            base.AddItemObjek(akCVId, jKWPTJBahagianId, akCartaId, amaun);

            Session?.SetJson("CartAkCV", this);
        }

        public override void RemoveItemObjek(int jKWPTJBahagianId, int akCartaId)
        {
            base.RemoveItemObjek(jKWPTJBahagianId, akCartaId);
            Session?.SetJson("CartAkCV", this);
        }

        public override void ClearObjek()
        {
            base.ClearObjek();
            Session?.Remove("CartAkCV");
        }
        //
    }
}
