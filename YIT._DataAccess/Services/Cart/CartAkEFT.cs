using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Services.Cart
{
    public class CartAkEFT
    {
        private List<AkEFTPenerima> collectionPenerima = new List<AkEFTPenerima>();

        public virtual void AddItemPenerima(
            int akEFTId,
            int akPVId,
            EnStatusProses enStatusEFT,
            string? sebabGagal,
            DateTime? tarikhKredit,
            int? bil,
            string? noPendaftaranPenerima,
            string? namaPenerima,
            string? catatan,
            int jCaraBayarId,
            int? jBankId,
            string? noakaunBank,
            string? emel,
            string? kodM2E,
            string? noRujukanCaraBayar,
            decimal amaun,
            EnJenisId enJenisId
            )
        {
            AkEFTPenerima line = collectionPenerima.FirstOrDefault(ep => ep.Bil == bil)!;

            if ( line == null )
            {
                collectionPenerima.Add(new AkEFTPenerima
                {
                    AkEFTId = akEFTId,
                    AkPVId = akPVId,
                    EnStatusEFT = enStatusEFT,
                    SebabGagal = sebabGagal,
                    TarikhKredit = tarikhKredit,
                    Bil = bil,
                    NoPendaftaranPenerima = noPendaftaranPenerima,
                    NamaPenerima = namaPenerima,
                    Catatan = catatan,
                    JCaraBayarId = jCaraBayarId,
                    JBankId = jBankId,
                    NoAkaunBank = noakaunBank,
                    Emel = emel,
                    KodM2E = kodM2E,
                    NoRujukanCaraBayar = noRujukanCaraBayar,
                    Amaun = amaun,
                    EnJenisId = enJenisId
                });
            }
        }

        public virtual void UpdateItemPenerima(
            int akEFTId,
            int akPVId,
            EnStatusProses enStatusEFT,
            string? sebabGagal,
            DateTime? tarikhKredit,
            int? bil,
            string? noPendaftaranPenerima,
            string? namaPenerima,
            string? catatan,
            int jCaraBayarId,
            int? jBankId,
            string? noakaunBank,
            string? emel,
            string? kodM2E,
            string? noRujukanCaraBayar,
            decimal amaun,
            EnJenisId enJenisId
            )
        {
            AkEFTPenerima line = collectionPenerima.FirstOrDefault(ep => ep.Bil == bil)!;

            if ( line != null )
            {
                collectionPenerima.Remove(line);

                collectionPenerima.Add(new AkEFTPenerima
                {
                    AkEFTId = akEFTId,
                    AkPVId = akPVId,
                    EnStatusEFT = enStatusEFT,
                    SebabGagal = sebabGagal,
                    TarikhKredit = tarikhKredit,
                    Bil = bil,
                    NoPendaftaranPenerima = noPendaftaranPenerima,
                    NamaPenerima = namaPenerima,
                    Catatan = catatan,
                    JCaraBayarId = jCaraBayarId,
                    JBankId = jBankId,
                    NoAkaunBank = noakaunBank,
                    Emel = emel,
                    KodM2E = kodM2E,
                    NoRujukanCaraBayar = noRujukanCaraBayar,
                    Amaun = amaun,
                    EnJenisId = enJenisId
                });
            }
        }

        public virtual void RemoveItemPenerima(int? bil, int akPVId) => collectionPenerima.RemoveAll(i => i.Bil == bil && i.AkPVId == akPVId);

        public virtual void ClearPenerima() => collectionPenerima.Clear();

        public virtual IEnumerable<AkEFTPenerima> AkEFTPenerima => collectionPenerima;
    }
}
