using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._01Jadual;
using YIT.__Domain.Entities.Models._03Akaun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Services.Cart.Session
{
    public class SessionCartAkTerimaTunggal : CartAkTerimaTunggal
    {
        public static CartAkTerimaTunggal GetCart(IServiceProvider services)
        {
            ISession session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext!.Session!;
            SessionCartAkTerimaTunggal cart = session?.GetJson<SessionCartAkTerimaTunggal>("CartAkTerimaTunggal") ?? new SessionCartAkTerimaTunggal();
            cart.Session = session;
            return cart;
        }
        private ISession? Session { get; set; }

        //TerimaTunggalObjek
        public override void AddItemObjek(
            int akTerimaTunggalId,
            int jBahagianId,
            int akCartaId,
            decimal amaun
           )
        {
            base.AddItemObjek(akTerimaTunggalId,
                          jBahagianId,
                          akCartaId,
                          amaun);

            Session?.SetJson("CartAkTerimaTunggal", this);
        }
        public override void RemoveItemObjek(int jBahagianId,int akCartaId)
        {
            base.RemoveItemObjek(jBahagianId,akCartaId);
            Session?.SetJson("CartAkTerimaTunggal", this);
        }
        public override void ClearObjek()
        {
            base.ClearObjek();
            Session?.Remove("CartAkTerimaTunggal");
        }
        //TerimaTunggalObjek End

        //TerimaTunggalInvois
        public override void AddItemInvois(int akTerimaTunggalId, int akInvoisId, decimal amaun)
        {
            base.AddItemInvois(akTerimaTunggalId, akInvoisId, amaun);
            Session?.SetJson("CartAkTerimaTunggal", this);
        }
        public override void RemoveItemInvois(int akInvoisId)
        {
            base.RemoveItemInvois(akInvoisId);
            Session?.SetJson("CartAkTerimaTunggal", this);
        }
        public override void ClearInvois()
        {
            base.ClearInvois();
            Session?.Remove("CartAkTerimaTunggal");
        }
        //TerimaTunggalInvois End

    }
}
