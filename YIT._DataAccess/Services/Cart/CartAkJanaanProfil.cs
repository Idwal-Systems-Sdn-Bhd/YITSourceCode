using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Services.Cart
{
    public class CartAkJanaanProfil
    {
        private List<AkJanaanProfilPenerima> collectionPenerima = new List<AkJanaanProfilPenerima>();

        public virtual void AddItemPenerima(
            int id,
            int? bil,
            int akJanaanProfilId,
            EnKategoriDaftarAwam enKategoriDaftarAwam,
            int? dPenerimaZakatId,
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
            decimal amaun,
            string? noRujukanMohon,
            int? akRekupId,
            EnJenisId enJenisId
            )
        {
            AkJanaanProfilPenerima line = collectionPenerima.FirstOrDefault(pp => pp.Bil == bil)!;

            if (line == null )
            {
                collectionPenerima.Add(new AkJanaanProfilPenerima()
                {
                    Id = id,
                    Bil = bil,
                    AkJanaanProfilId = akJanaanProfilId,
                    EnKategoriDaftarAwam = enKategoriDaftarAwam,
                    DPenerimaZakatId = dPenerimaZakatId,
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
                    Amaun = amaun,
                    NoRujukanMohon = noRujukanMohon,
                    AkRekupId = akRekupId,
                    EnJenisId = enJenisId
                });
            }
        }

        public virtual void UpdateItemPenerima(
            int id,
            int? bil,
            int akJanaanProfilId,
            EnKategoriDaftarAwam enKategoriDaftarAwam,
            int? dPenerimaZakatId,
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
            decimal amaun,
            string? noRujukanMohon,
            int? akRekupId,
            EnJenisId enJenisId
            )
        {
            AkJanaanProfilPenerima line = collectionPenerima.FirstOrDefault(pp => pp.Bil == bil)!;

            if (line != null)
            {
                collectionPenerima.Remove(line);

                collectionPenerima.Add(new AkJanaanProfilPenerima()
                {
                    Id = id,
                    Bil = bil,
                    AkJanaanProfilId = akJanaanProfilId,
                    EnKategoriDaftarAwam = enKategoriDaftarAwam,
                    DPenerimaZakatId = dPenerimaZakatId,
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
                    Amaun = amaun,
                    NoRujukanMohon = noRujukanMohon,
                    AkRekupId = akRekupId,
                    EnJenisId = enJenisId
                });
            }
        }

        public virtual void RemoveItemPenerima(int bil) => collectionPenerima.RemoveAll(l => l.Bil == bil);

        public virtual void ClearPenerima() => collectionPenerima.Clear();
        public virtual IEnumerable<AkJanaanProfilPenerima> AkJanaanProfilPenerima => collectionPenerima;
    }
}
