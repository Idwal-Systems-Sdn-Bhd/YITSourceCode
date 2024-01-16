using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Services.Cart.Session
{
    public class SessionCartAkJurnal : CartAkJurnal
    {
        public static CartAkJurnal GetCart(IServiceProvider services)
        {
            ISession session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext!.Session!;
            SessionCartAkJurnal cart = session?.GetJson<SessionCartAkJurnal>("CartAkJurnal") ?? new SessionCartAkJurnal();
            cart.Session = session;
            return cart;
        }
        private ISession? Session { get; set; }

        // Jurnal Objek
        public override void AddItemObjek(int akJurnalId, int jKWPTJBahagianDebitId, int akCartaDebitId, int jKWPTJBahagianKreditId, int akCartaKreditId, decimal amaun, bool isDebitAbBukuVot, bool isKreditAbBukuVot)
        {
            base.AddItemObjek(akJurnalId, jKWPTJBahagianDebitId, akCartaDebitId, jKWPTJBahagianKreditId, akCartaKreditId, amaun, isDebitAbBukuVot,isKreditAbBukuVot);
            Session?.SetJson("CartAkJurnal", this);
        }

        public override void RemoveItemObjek(int jKWPTJBahagianDebitId, int akCartaDebitId, int jKWPTJBahagianKreditId, int akCartaKreditId)
        {
            base.RemoveItemObjek(jKWPTJBahagianDebitId, akCartaDebitId, jKWPTJBahagianKreditId, akCartaKreditId);
            Session?.SetJson("CartAkJurnal", this);

        }
        public override void ClearObjek()
        {
            base.ClearObjek();
            Session?.Remove("CartAkJurnal");
        }
        //

        // Jurnal Penerima Cek Batal
        public override void AddItemPenerimaCekBatal(int akJurnalId, int bil, int akPVId, string? namaPenerima, string? noCek, decimal amaun)
        {
            base.AddItemPenerimaCekBatal(akJurnalId, bil, akPVId, namaPenerima, noCek, amaun);
            Session?.SetJson("CartAkJurnal", this);

        }

        public override void RemoveItemPenerimaCekBatal(int bil, int akPVId)
        {
            base.RemoveItemPenerimaCekBatal(bil, akPVId);
            Session?.SetJson("CartAkJurnal", this);

        }

        public override void ClearPenerimaCekBatal()
        {
            base.ClearPenerimaCekBatal();
            Session?.Remove("CartAkJurnal");
        }
        //
    }
}
