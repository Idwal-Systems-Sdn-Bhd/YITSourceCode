using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Services.Cart.Session
{
    public class SessionCartAkPenyesuaianBank : CartAkPenyesuaianBank
    {
        public static CartAkPenyesuaianBank GetCart(IServiceProvider services)
        {
            ISession session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext!.Session!;
            SessionCartAkPenyesuaianBank cart = session?.GetJson<SessionCartAkPenyesuaianBank>("CartAkPenyesuaianBank") ?? new SessionCartAkPenyesuaianBank();
            cart.Session = session;
            return cart;
        }
        private ISession? Session { get; set; }

        public override void AddItemPenyataBank(int akPenyesuaianBankId, decimal bil,string? indeks, string? noAkaunBank, DateTime tarikh, string? kodCawanganBank, string? kodTransaksi, string? perihalTransaksi, string? noDokumen, string? noDokumenTambahan1, string? noDokumenTambahan2, string? noDokumenTambahan3, decimal debit, decimal kredit, string? signDebitKredit, decimal baki, bool isPadan)
        {
            base.AddItemPenyataBank(akPenyesuaianBankId, bil, indeks, noAkaunBank, tarikh, kodCawanganBank, kodTransaksi, perihalTransaksi, noDokumen, noDokumenTambahan1, noDokumenTambahan2, noDokumenTambahan3, debit, kredit, signDebitKredit, baki, isPadan);

            Session?.SetJson("CartAkPenyesuaianBank", this);
        }

        public override void RemoveItemPenyataBank(decimal bil)
        {
            base.RemoveItemPenyataBank(bil);
            Session?.SetJson("CartAkPenyesuaianBank", this);

        }

        public override void ClearPenyataBank()
        {
            base.ClearPenyataBank();
            Session?.Remove("CartAkPenyesuaianBank");


        }

        public override void AddItemPenyataSistem(int id, int? akAkaunId, DateTime tarikh, string? noRujukan, string? perihal, string? noSlip, decimal debit, decimal kredit, bool isPV, int? jCaraBayarId, bool isPadan)
        {
            base.AddItemPenyataSistem(id, akAkaunId, tarikh, noRujukan, perihal, noSlip, debit, kredit, isPV, jCaraBayarId, isPadan);
            Session?.SetJson("CartAkPenyesuaianBank", this);
        }

        public override void RemoveItemPenyataSistem(int id, bool isPV)
        {
            base.RemoveItemPenyataSistem(id, isPV);
            Session?.SetJson("CartAkPenyesuaianBank", this);
        }

        public override void ClearPenyataSistem()
        {
            base.ClearPenyataSistem();
            Session?.Remove("CartAkPenyesuaianBank");
        }

        public override void AddItemPadanan(int? akAkaunId, int akPenyesuaianBankPenyataBankId, int? akPVPenerimaId, decimal bil, bool isAutoMatch, int? jCaraBayarId, decimal debit, decimal kredit, DateTime tarikh)
        {
            base.AddItemPadanan(akAkaunId, akPenyesuaianBankPenyataBankId, akPVPenerimaId, bil, isAutoMatch, jCaraBayarId, debit, kredit, tarikh);
            Session?.SetJson("CartAkPenyesuaianBank", this);
        }

        public override void RemoveItemPadanan(int? akAkaunId, int akPenyesuaianBankPenyataBankId, int? akPVPenerimaId, bool isPV)
        {
            base.RemoveItemPadanan(akAkaunId, akPenyesuaianBankPenyataBankId, akPVPenerimaId, isPV);
            Session?.SetJson("CartAkPenyesuaianBank", this);
        }

        public override void ClearPadanan()
        {
            base.ClearPadanan();
            Session?.Remove("CartAkPenyesuaianBank");
        }
    }
}
