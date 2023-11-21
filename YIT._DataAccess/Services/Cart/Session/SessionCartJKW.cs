using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._01Jadual;

namespace YIT._DataAccess.Services.Cart.Session
{
    public class SessionCartJKW : CartJKW
    {
        public static CartJKW GetCart(IServiceProvider services)
        {
            ISession session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext!.Session!;
            SessionCartJKW cart = session?.GetJson<SessionCartJKW>("CartJKW") ?? new SessionCartJKW();
            cart.Session = session;
            return cart;
        }

        private ISession? Session { get; set; }

        public override void AddItemList(int jKWId, int jPTJId, int jBahagianId, string? kod)
        {
            base.AddItemList(jKWId, jPTJId, jBahagianId, kod);
            Session?.SetJson("CartJKW", this);
        }

        public override void RemoveItemList(int jKWId, int jPTJId, int jBahagianId)
        {
            base.RemoveItemList(jKWId, jPTJId, jBahagianId);
            Session?.SetJson("CartJKW", this);
        }
        public override void ClearList()
        {
            base.ClearList();
            Session?.Remove("CartJKW");
        }
    }
}
