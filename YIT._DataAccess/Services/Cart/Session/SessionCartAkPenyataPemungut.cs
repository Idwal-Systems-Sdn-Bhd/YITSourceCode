using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Services.Cart.Session
{
    public class SessionCartAkPenyataPemungut : CartAkPenyataPemungut
    {
        public static CartAkPenyataPemungut GetCart(IServiceProvider services)
        {
            ISession session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext!.Session!;
            SessionCartAkPenyataPemungut cart = session?.GetJson<SessionCartAkPenyataPemungut>("CartAkPenyataPemungut") ?? new SessionCartAkPenyataPemungut();
            cart.Session = session;
            return cart;
        }

        private ISession? Session { get; set; }

        public override void AddItemObjek(int akPenyataPemungutId, int akTerimaTunggalId, decimal bil, int jKWPTJBahagianId, int akCartaId, decimal amaun)
        {
            base.AddItemObjek(akPenyataPemungutId, akTerimaTunggalId, bil, jKWPTJBahagianId, akCartaId, amaun);
            Session?.SetJson("CartAkPenyataPemungut", this);
        }

        public override void RemoveItemObjek(decimal bil)
        {
            base.RemoveItemObjek(bil);
            Session?.SetJson("CartAkPenyataPemungut", this);
        }

        public override void ClearObjek()
        {
            base.ClearObjek();
            Session?.Remove("CartAkPenyataPemungut");
        }
    }
}
