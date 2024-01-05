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
    public class SessionCartAkEFT : CartAkEFT
    {
        public static CartAkEFT GetCart(IServiceProvider services)
        {
            ISession session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext!.Session!;
            SessionCartAkEFT cart = session?.GetJson<SessionCartAkEFT>("CartAkEFT") ?? new SessionCartAkEFT();
            cart.Session = session;
            return cart;
        }

        private ISession? Session { get; set; }

        public override void AddItemPenerima(int akEFTId, int akPVId, EnStatusProses enStatusEFT, string? sebabGagal, DateTime? tarikhKredit, int? bil, string? noPendaftaranPenerima, string? namaPenerima, string? catatan, int jCaraBayarId, int? jBankId, string? noakaunBank,string? emel, string? kodM2E, string? noCek, decimal amaun, EnJenisId enJenisId)
        {
            base.AddItemPenerima(akEFTId, akPVId, enStatusEFT, sebabGagal, tarikhKredit, bil, noPendaftaranPenerima, namaPenerima, catatan, jCaraBayarId, jBankId, noakaunBank, emel, kodM2E, noCek, amaun, enJenisId);
            Session?.SetJson("CartAkEFT", this);
        }

        public override void UpdateItemPenerima(int akEFTId, int akPVId, EnStatusProses enStatusEFT, string? sebabGagal, DateTime? tarikhKredit, int? bil, string? noPendaftaranPenerima, string? namaPenerima, string? catatan, int jCaraBayarId, int? jBankId, string? noakaunBank, string? emel, string? kodM2E, string? noCek, decimal amaun, EnJenisId enJenisId)
        {
            base.UpdateItemPenerima(akEFTId, akPVId, enStatusEFT, sebabGagal, tarikhKredit, bil, noPendaftaranPenerima, namaPenerima, catatan, jCaraBayarId, jBankId, noakaunBank, emel, kodM2E, noCek, amaun, enJenisId);
            Session?.SetJson("CartAkEFT", this);
        }

        public override void RemoveItemPenerima(int? bil, int akPVId)
        {
            base.RemoveItemPenerima(bil, akPVId);
            Session?.SetJson("CartAkEFT", this);
        }

        public override void ClearPenerima()
        {
            base.ClearPenerima();
            Session?.SetJson("CartAkEFT", this);
        }
    }
}
