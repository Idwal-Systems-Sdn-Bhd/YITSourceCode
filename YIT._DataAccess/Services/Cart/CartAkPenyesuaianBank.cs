using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Services.Cart
{
    public class CartAkPenyesuaianBank
    {
        // penyata bank
        private List<AkPenyesuaianBankPenyataBank> collectionPenyataBank = new List<AkPenyesuaianBankPenyataBank>();

        public virtual void AddItemPenyataBank(
            int akPenyesuaianBankId,
            decimal bil,
            string? indeks,
            string? noAkaunBank,
            DateTime tarikh,
            string? kodCawanganBank,
            string? kodTransaksi,
            string? perihalTransaksi,
            string? noDokumen,
            string? noDokumenTambahan1,
            string? noDokumenTambahan2,
            string? noDokumenTambahan3,
            decimal debit,
            decimal kredit,
            string? signDebitKredit,
            decimal baki,
            bool isPadan
            )
        {
            AkPenyesuaianBankPenyataBank line = collectionPenyataBank.FirstOrDefault(pb => pb.Bil == bil)!;

            if (line == null)
            {
                collectionPenyataBank.Add(new AkPenyesuaianBankPenyataBank
                {
                    Bil = bil,
                    Indeks = indeks,
                    NoAkaunBank = noAkaunBank,
                    Tarikh = tarikh,
                    KodCawanganBank = kodCawanganBank,
                    KodTransaksi = kodTransaksi,
                    PerihalTransaksi = perihalTransaksi,
                    NoDokumen = noDokumen,
                    NoDokumenTambahan1 = noDokumenTambahan1,
                    NoDokumenTambahan2 = noDokumenTambahan2,
                    NoDokumenTambahan3 = noDokumenTambahan3,
                    Debit = debit,
                    Kredit = kredit,
                    SignDebitKredit = signDebitKredit,
                    Baki = baki,
                    IsPadan = isPadan
                });
            }
        }

        public virtual void RemoveItemPenyataBank(decimal bil) => collectionPenyataBank.RemoveAll(pb => pb.Bil == bil);

        public virtual void ClearPenyataBank() => collectionPenyataBank.Clear();

        public virtual IEnumerable<AkPenyesuaianBankPenyataBank> AkPenyesuaianBankPenyataBank => collectionPenyataBank;
        // penyata bank end

        // buku tunai (sistem)
        private List<AkPenyesuaianBankPenyataSistem> collectionPenyataSistem = new List<AkPenyesuaianBankPenyataSistem>();

        public virtual void AddItemPenyataSistem(
            int id,
            int? akAkaunId,
            DateTime tarikh,
            string? noRujukan,
            string? perihal,
            string? noSlip,
            decimal debit,
            decimal kredit,
            bool isPV,
            int? jCaraBayarId,
            bool isPadan
            )
        {
            AkPenyesuaianBankPenyataSistem line = collectionPenyataSistem.FirstOrDefault(pb => pb.AkAkaunId == akAkaunId)!;

            if (line == null)
            {
                collectionPenyataSistem.Add(new AkPenyesuaianBankPenyataSistem
                {
                    Id = id,
                    AkAkaunId = akAkaunId,
                    Tarikh = tarikh,
                    Perihal = perihal,
                    NoSlip = noSlip,
                    Debit = debit,
                    Kredit = kredit,
                    IsPV = isPV,
                    JCarabayarId = jCaraBayarId,
                    IsPadan = isPadan
                });
            }
        }

        public virtual void RemoveItemPenyataSistem(int id, bool isPV)
        {
            if (isPV == true)
            {
                collectionPenyataSistem.RemoveAll(pb => pb.Id == id && pb.IsPV == isPV);
            }
            else
            {
                collectionPenyataSistem.RemoveAll(pb => pb.AkAkaunId == id && pb.IsPV == isPV);
            }
        }
        public virtual void ClearPenyataSistem() => collectionPenyataSistem.Clear();

        public virtual IEnumerable<AkPenyesuaianBankPenyataSistem> AkPenyesuaianBankPenyataSistem => collectionPenyataSistem;
        // buku tunai (sistem) end

        // akaun penyata bank
        private List<AkAkaunPenyataBank> collectionPadanan = new List<AkAkaunPenyataBank>();

        public virtual void AddItemPadanan(
            int? akAkaunId,
            int akPenyesuaianBankPenyataBankId,
            int? akPVPenerimaId,
            decimal bil,
            bool isAutoMatch,
            int? jCaraBayarId,
            decimal debit,
            decimal kredit,
            DateTime tarikh
            )
        {
            AkAkaunPenyataBank line = collectionPadanan.FirstOrDefault(pb => pb.Bil == bil)!;

            if (line == null)
            {
                collectionPadanan.Add(new AkAkaunPenyataBank
                {
                    AkAkaunId = akAkaunId,
                    AkPenyesuaianBankPenyataBankId = akPenyesuaianBankPenyataBankId,
                    AkPVPenerimaId = akPVPenerimaId,
                    Bil = bil,
                    IsAutoMatch = isAutoMatch,
                    JCaraBayarId = jCaraBayarId,
                    Debit = debit,
                    Kredit = kredit,
                    Tarikh = tarikh
                });
            }
        }

        public virtual void RemoveItemPadanan(int? akAkaunId, int akPenyesuaianBankPenyataBankId, int? akPVPenerimaId, bool isPV)
        {
            if (isPV)
            {
                collectionPadanan.RemoveAll(pb => pb.AkPenyesuaianBankPenyataBankId == akPenyesuaianBankPenyataBankId && pb.AkPVPenerimaId == akPVPenerimaId);
            }
            else
            {
                collectionPadanan.RemoveAll(pb => pb.AkAkaunId == akAkaunId && pb.AkPenyesuaianBankPenyataBankId == akPenyesuaianBankPenyataBankId);
            }
            
        }

        public virtual void ClearPadanan() => collectionPadanan.Clear();

        public virtual IEnumerable<AkAkaunPenyataBank> AkAkaunPenyataBank => collectionPadanan;
        // akaun penyata bank end
    }
}
