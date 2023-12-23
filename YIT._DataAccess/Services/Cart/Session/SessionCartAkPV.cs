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
    public class SessionCartAkPV : CartAkPV
    {
        public static CartAkPV GetCart(IServiceProvider services)
        {
            ISession session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext!.Session!;
            SessionCartAkPV cart = session?.GetJson<SessionCartAkPV>("CartAkPV") ?? new SessionCartAkPV();
            cart.Session = session;
            return cart;
        }

        private ISession? Session {  get; set; }

        // PVObjek
        public override void AddItemObjek(int akPVId, int jKWPTJBahagianId, int akCartaId, int? jCukaiId, decimal amaun)
        {
            base.AddItemObjek(akPVId, jKWPTJBahagianId, akCartaId, jCukaiId, amaun);
            Session?.SetJson("CartAkPV", this);
        }

        public override void RemoveItemObjek(int jKWPTJBahagianId, int akCartaId)
        {
            base.RemoveItemObjek(jKWPTJBahagianId, akCartaId);
            Session?.SetJson("CartAkPV", this);
        }

        public override void ClearObjek()
        {
            base.ClearObjek();
            Session?.Remove("CartAkPV");
        }
        //

        // PVInvois
        public override void AddItemInvois(int akPVId, bool isTanggungan, int akBelianId, decimal amaun)
        {
            base.AddItemInvois(akPVId, isTanggungan, akBelianId, amaun);
            Session?.SetJson("CartAkPV", this);
        }

        public override void RemoveItemInvois(int akBelianId)
        {
            base.RemoveItemInvois(akBelianId);
            Session?.SetJson("CartAkPV", this);
        }

        public override void ClearInvois()
        {
            base.ClearInvois();
            Session?.SetJson("CartAkPV", this);
        }
        //

        // PVPenerima
        public override void AddItemPenerima(int id, int akPVId, int? akJanaanProfilPenerimaId, EnKategoriDaftarAwam enKategoriDaftarAwam,int? dDaftarAwamId, int? dPekerjaId, string? noPendaftaranPenerima, string? namaPenerima, string? noPendaftaranPemohon, string? catatan, int? akEFTPenerimaId, int jCaraBayarId, int? jBankId, string? noAkaunBank, string? alamat1, string? alamat2, string? alamat3, string? emel, string? kodM2E, string? noCek, DateTime? tarikhCek, decimal amaun, string? noRujukanMohon, int? akRekupId, int? akPanjarId, bool isCekDitunaikan, DateTime? tarikhCekDitunaikan, EnStatusProses enStatusEFT, int? bil)
        {
            base.AddItemPenerima(id, akPVId, akJanaanProfilPenerimaId, enKategoriDaftarAwam, dDaftarAwamId, dPekerjaId, noPendaftaranPenerima, namaPenerima, noPendaftaranPemohon, catatan, akEFTPenerimaId, jCaraBayarId, jBankId, noAkaunBank, alamat1, alamat2, alamat3, emel, kodM2E, noCek, tarikhCek, amaun, noRujukanMohon, akRekupId, akPanjarId, isCekDitunaikan, tarikhCekDitunaikan, enStatusEFT, bil);
            Session?.SetJson("CartAkPV", this);
        }

        public override void UpdateItemPenerima(int id, int akPVId, int? akJanaanProfilPenerimaId, EnKategoriDaftarAwam enKategoriDaftarAwam,int? dDaftarAwamId, int? dPekerjaId, string? noPendaftaranPenerima, string? namaPenerima, string? noPendaftaranPemohon, string? catatan, int? akEFTPenerimaId, int jCaraBayarId, int? jBankId, string? noAkaunBank, string? alamat1, string? alamat2, string? alamat3, string? emel, string? kodM2E, string? noCek, DateTime? tarikhCek, decimal amaun, string? noRujukanMohon, int? akRekupId, int? akPanjarId, bool isCekDitunaikan, DateTime? tarikhCekDitunaikan, EnStatusProses enStatusEFT, int bil)
        {
            base.UpdateItemPenerima(id, akPVId, akJanaanProfilPenerimaId, enKategoriDaftarAwam,dDaftarAwamId, dPekerjaId, noPendaftaranPenerima, namaPenerima, noPendaftaranPemohon, catatan, akEFTPenerimaId, jCaraBayarId, jBankId, noAkaunBank, alamat1, alamat2, alamat3, emel, kodM2E, noCek, tarikhCek, amaun, noRujukanMohon, akRekupId, akPanjarId, isCekDitunaikan, tarikhCekDitunaikan, enStatusEFT, bil);
            Session?.SetJson("CartAkPV", this);

        }

        public override void RemoveItemPenerima(int bil)
        {
            base.RemoveItemPenerima(bil);
            Session?.SetJson("CartAkPV", this);
        }
        public override void ClearPenerima()
        {
            base.ClearPenerima();
            Session?.SetJson("CartAkPV", this);
        }
        //
    }
}
