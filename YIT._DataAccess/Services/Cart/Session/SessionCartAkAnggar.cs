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
    public class SessionCartAkAnggar : CartAkAnggar
    {
        public static CartAkAnggar GetCart(IServiceProvider services)
        {
            ISession session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext!.Session!;
            SessionCartAkAnggar cart = session?.GetJson<SessionCartAkAnggar>("CartAkAnggar") ?? new SessionCartAkAnggar();
            cart.Session = session;
            return cart;
        }
        private ISession? Session { get; set; }

        //AnggarObjek
        public override void AddItemObjek(
            int akAnggarId,
            int jKWPTJBahagianId,
            int akCartaId,
            decimal amaun
           )
        {
            base.AddItemObjek(akAnggarId,
                          jKWPTJBahagianId,
                          akCartaId,
                          amaun);

            Session?.SetJson("CartAkAnggar", this);
        }
        public override void RemoveItemObjek(int jKWPTJBahagianId, int akCartaId)
        {
            base.RemoveItemObjek(jKWPTJBahagianId, akCartaId);
            Session?.SetJson("CartAkAnggar", this);
        }
        public override void ClearObjek()
        {
            base.ClearObjek();
            Session?.Remove("CartAkAnggar");
        }
        //AnggarObjek End

    }
}
