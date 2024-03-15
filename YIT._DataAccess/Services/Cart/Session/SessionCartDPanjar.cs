using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Services.Cart.Session
{
    public class SessionCartDPanjar : CartDPanjar
    {
        public static CartDPanjar GetCart(IServiceProvider services)
        {
            ISession session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext!.Session!;
            SessionCartDPanjar cart = session?.GetJson<SessionCartDPanjar>("CartDPanjar") ?? new SessionCartDPanjar();
            cart.Session = session;
            return cart;
        }
        private ISession? Session { get; set; }

        // DPanjarPemegang
        public override void AddItemPemegang(int dPanjarId, int dPekerjaId, DateTime jangkaMasaDari, DateTime? jangkaMasaHingga, bool isAktif)
        {
            base.AddItemPemegang(dPanjarId, dPekerjaId, jangkaMasaDari, jangkaMasaHingga, isAktif);
            Session?.SetJson("CartDPanjar", this);
        }

        public override void RemoveItemPemegang(int dPekerjaId)
        {
            base.RemoveItemPemegang(dPekerjaId);
            Session?.SetJson("CartDPanjar", this);
        }

        public override void ClearPemegang()
        {
            base.ClearPemegang();
            Session?.Remove("CartDPanjar");
        }
        // DPanjarPemegan end
    }
}
