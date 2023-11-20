﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;

namespace YIT._DataAccess.Services.Cart.Session
{
    public class SessionCartAbWaran : CartAbWaran
    {
        public static CartAbWaran GetCart(IServiceProvider services)
        {
            ISession session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext!.Session!;
            SessionCartAbWaran cart = session?.GetJson<SessionCartAbWaran>("CartAbWaran") ?? new SessionCartAbWaran();
            cart.Session = session;
            return cart;
        }
        private ISession? Session { get; set; }

        //TerimaObjek
        public override void AddItemObjek(
            int akTerimaId,
            int jBahagianId,
            int akCartaId,
            decimal amaun,
            string? TK
           )
        {
            base.AddItemObjek(akTerimaId,
                          jBahagianId,
                          akCartaId,
                          amaun,
                          TK);

            Session?.SetJson("CartAbWaran", this);
        }
        public override void RemoveItemObjek(int jBahagianId, int akCartaId)
        {
            base.RemoveItemObjek(jBahagianId, akCartaId);
            Session?.SetJson("CartAbWaran", this);
        }
        public override void ClearObjek()
        {
            base.ClearObjek();
            Session?.Remove("CartAbWaran");
        }
        //TerimaObjek End

    }
}
