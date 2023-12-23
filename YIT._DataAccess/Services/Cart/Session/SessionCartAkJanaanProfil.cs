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
    public class SessionCartAkJanaanProfil : CartAkJanaanProfil
    {
        public static CartAkJanaanProfil GetCart(IServiceProvider services)
        {
            ISession session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext!.Session!;
            SessionCartAkJanaanProfil cart = session?.GetJson<SessionCartAkJanaanProfil>("CartAkJanaanProfil") ?? new SessionCartAkJanaanProfil();
            cart.Session = session;
            return cart;
        }

        private ISession? Session {  get; set; }

        // JanaanProfilPenerima
        public override void AddItemPenerima(int id, int? bil, int akJanaanProfilId, EnKategoriDaftarAwam enKategoriDaftarAwam, int? dPenerimaZakatId, int? dDaftarAwamId, int? dPekerjaId, string? noPendaftaranPenerima, string? namaPenerima, string? noPendaftaranPemohon, string? catatan, int jCaraBayarId, int? jBankId, string? noAkaunBank, string? alamat1, string? alamat2, string? alamat3, string? emel, string? kodM2E, decimal amaun, string? noRujukanMohon, int? akRekupId)
        {
            base.AddItemPenerima(id, bil, akJanaanProfilId, enKategoriDaftarAwam, dPenerimaZakatId, dDaftarAwamId, dPekerjaId, noPendaftaranPenerima, namaPenerima, noPendaftaranPemohon, catatan, jCaraBayarId, jBankId, noAkaunBank, alamat1, alamat2, alamat3, emel, kodM2E, amaun, noRujukanMohon, akRekupId);
            Session?.SetJson("CartAkJanaanProfil", this);
        }

        public override void UpdateItemPenerima(int id, int? bil, int akJanaanProfilId, EnKategoriDaftarAwam enKategoriDaftarAwam, int? dPenerimaZakatId, int? dDaftarAwamId, int? dPekerjaId, string? noPendaftaranPenerima, string? namaPenerima, string? noPendaftaranPemohon, string? catatan, int jCaraBayarId, int? jBankId, string? noAkaunBank, string? alamat1, string? alamat2, string? alamat3, string? emel, string? kodM2E, decimal amaun, string? noRujukanMohon, int? akRekupId)
        {
            base.UpdateItemPenerima(id, bil, akJanaanProfilId, enKategoriDaftarAwam, dPenerimaZakatId, dDaftarAwamId, dPekerjaId, noPendaftaranPenerima, namaPenerima, noPendaftaranPemohon, catatan, jCaraBayarId, jBankId, noAkaunBank, alamat1, alamat2, alamat3, emel, kodM2E, amaun, noRujukanMohon, akRekupId);
            Session?.SetJson("CartAkJanaanProfil", this);
        }

        public override void RemoveItemPenerima(int bil)
        {
            base.RemoveItemPenerima(bil);
            Session?.SetJson("CartAkJanaanProfil", this);
        }

        public override void ClearPenerima()
        {
            base.ClearPenerima();
            Session?.SetJson("CartAkJanaanProfil", this);
        }
        //
    }
}
