using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._02Daftar;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Services.Cart
{
    public class CartAkPV
    {
        //PVObjek
        private List<AkPVObjek> collectionObjek = new List<AkPVObjek>();

        public virtual void AddItemObjek(
            int akPVId,
            int jKWPTJBahagianId,
            int akCartaId,
            int? jCukaiId,
            decimal amaun
            )
        {
            AkPVObjek line = collectionObjek.FirstOrDefault(pp => pp.JKWPTJBahagianId == jKWPTJBahagianId && pp.AkCartaId == akCartaId)!;

            if (line == null)
            {
                collectionObjek.Add(new AkPVObjek()
                {
                    AkPVId = akPVId,
                    JKWPTJBahagianId = jKWPTJBahagianId,
                    AkCartaId = akCartaId,
                    JCukaiId = jCukaiId,
                    Amaun = amaun
                });
            }
        }

        public virtual void RemoveItemObjek(int jKWPTJBahagianId, int akCartaId) => collectionObjek.RemoveAll(l => l.JKWPTJBahagianId == jKWPTJBahagianId && l.AkCartaId == akCartaId);

        public virtual void ClearObjek() => collectionObjek.Clear();

        public virtual IEnumerable<AkPVObjek> AkPVObjek => collectionObjek;
        //

        // PVInvois
        private List<AkPVInvois> collectionInvois = new List<AkPVInvois>();

        public virtual void AddItemInvois(
            int akPVId,
            bool isTanggungan,
            int akBelianId,
            decimal amaun
            )
        {
            AkPVInvois line = collectionInvois.FirstOrDefault(pp => pp.AkBelianId == akBelianId)!;

            if (line == null)
            {
                collectionInvois.Add(new AkPVInvois()
                {
                    AkPVId = akPVId,
                    IsTanggungan = isTanggungan,
                    AkBelianId = akBelianId,
                    Amaun = amaun
                });
            }
        }

        public virtual void RemoveItemInvois(int akBelianId) => collectionInvois.RemoveAll(l => l.AkBelianId == akBelianId);

        public virtual void ClearInvois() => collectionInvois.Clear();

        public virtual IEnumerable<AkPVInvois> AkPVInvois => collectionInvois;
        //

        // PVPenerima
        private List<AkPVPenerima> collectionPenerima = new List<AkPVPenerima>();

        public virtual void AddItemPenerima(
            int id,
            int akPVId,
            int? akJanaanProfilPenerimaId,
            EnKategoriDaftarAwam enKategoriDaftarAwam,
            int? dDaftarAwamId,
            int? dPekerjaId,
            string? noPendaftaranPenerima,
            string? namaPenerima,
            string? noPendaftaranPemohon,
            string? catatan,
            int jCaraBayarId,
            int? jBankId,
            string? noAkaunBank,
            string? alamat1,
            string? alamat2,
            string? alamat3,
            string? emel,
            string? kodM2E,
            string? noRujukanCaraBayar,
            DateTime? tarikhCaraBayar,
            decimal amaun,
            string? noRujukanMohon,
            int? akRekupId,
            int? akPanjarId,
            bool isCekDitunaikan,
            DateTime? tarikhCekDitunaikan,
            EnStatusProses enStatusEFT,
            int? bil,
            EnJenisId enJenisId
            )
        {
            AkPVPenerima line = collectionPenerima.FirstOrDefault(pp => pp.Bil == bil)!;


            if (line == null)
            {
                collectionPenerima.Add(new AkPVPenerima()
                {
                    Id = id,
                    AkPVId = akPVId,
                    AkJanaanProfilPenerimaId = akJanaanProfilPenerimaId,
                    EnKategoriDaftarAwam = enKategoriDaftarAwam,
                    DDaftarAwamId = dDaftarAwamId,
                    DPekerjaId = dPekerjaId,
                    NoPendaftaranPenerima = noPendaftaranPenerima,
                    NamaPenerima = namaPenerima,
                    NoPendaftaranPemohon = noPendaftaranPemohon,
                    Catatan = catatan,
                    JCaraBayarId = jCaraBayarId,
                    JBankId = jBankId,
                    NoAkaunBank = noAkaunBank,
                    Alamat1 = alamat1,
                    Alamat2 = alamat2,
                    Alamat3 = alamat3,
                    Emel = emel,
                    KodM2E = kodM2E,
                    NoRujukanCaraBayar = noRujukanCaraBayar,
                    TarikhCaraBayar = tarikhCaraBayar,
                    Amaun = amaun,
                    NoRujukanMohon = noRujukanMohon,
                    AkRekupId = akRekupId,
                    DPanjarId = akPanjarId,
                    IsCekDitunaikan = isCekDitunaikan,
                    TarikhCekDitunaikan = tarikhCekDitunaikan,
                    EnStatusEFT = enStatusEFT,
                    Bil = bil,
                    EnJenisId = enJenisId
                });
            }
        }

        public virtual void UpdateItemPenerima(
            int id,
            int akPVId,
            int? akJanaanProfilPenerimaId,
            EnKategoriDaftarAwam enKategoriDaftarAwam,
            int? dDaftarAwamId,
            int? dPekerjaId,
            string? noPendaftaranPenerima,
            string? namaPenerima,
            string? noPendaftaranPemohon,
            string? catatan,
            int jCaraBayarId,
            int? jBankId,
            string? noAkaunBank,
            string? alamat1,
            string? alamat2,
            string? alamat3,
            string? emel,
            string? kodM2E,
            string? noRujukanCaraBayar,
            DateTime? tarikhCaraBayar,
            decimal amaun,
            string? noRujukanMohon,
            int? akRekupId,
            int? akPanjarId,
            bool isCekDitunaikan,
            DateTime? tarikhCekDitunaikan,
            EnStatusProses enStatusEFT,
            int bil,
            EnJenisId enJenisId
            )
        {
            AkPVPenerima line = collectionPenerima.FirstOrDefault(pp => pp.Bil == bil)!;

            if (line != null)
            {
                collectionPenerima.Remove(line);

                collectionPenerima.Add(new AkPVPenerima()
                {
                    Id = id,
                    AkPVId = akPVId,
                    AkJanaanProfilPenerimaId = akJanaanProfilPenerimaId,
                    EnKategoriDaftarAwam = enKategoriDaftarAwam,
                    DDaftarAwamId = dDaftarAwamId,
                    DPekerjaId = dPekerjaId,
                    NoPendaftaranPenerima = noPendaftaranPenerima,
                    NamaPenerima = namaPenerima,
                    NoPendaftaranPemohon = noPendaftaranPemohon,
                    Catatan = catatan,
                    JCaraBayarId = jCaraBayarId,
                    JBankId = jBankId,
                    NoAkaunBank = noAkaunBank,
                    Alamat1 = alamat1,
                    Alamat2 = alamat2,
                    Alamat3 = alamat3,
                    Emel = emel,
                    KodM2E = kodM2E,
                    NoRujukanCaraBayar = noRujukanCaraBayar,
                    TarikhCaraBayar = tarikhCaraBayar,
                    Amaun = amaun,
                    NoRujukanMohon = noRujukanMohon,
                    AkRekupId = akRekupId,
                    DPanjarId = akPanjarId,
                    IsCekDitunaikan = isCekDitunaikan,
                    TarikhCekDitunaikan = tarikhCekDitunaikan,
                    EnStatusEFT = enStatusEFT,
                    Bil = bil,
                    EnJenisId = enJenisId
                });

            }


        }

        public virtual void RemoveItemPenerima(int bil) => collectionPenerima.RemoveAll(l => l.Bil == bil);
        public virtual void ClearPenerima() => collectionPenerima.Clear();

        public virtual IEnumerable<AkPVPenerima> AkPVPenerima => collectionPenerima;
        //
    }
}
