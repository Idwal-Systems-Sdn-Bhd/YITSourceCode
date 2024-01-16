using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Services.Cart
{
    public class CartAkJurnal
    {
        // JurnalObjek
        private List<AkJurnalObjek> collectionObjek = new List<AkJurnalObjek>();
        public virtual void AddItemObjek(
            int akJurnalId,
            int jKWPTJBahagianDebitId,
            int akCartaDebitId,
            int jKWPTJBahagianKreditId,
            int akCartaKreditId,
            decimal amaun,
            bool isDebitAbBukuVot,
            bool isKreditAbBukuVot)
        {
            AkJurnalObjek line = collectionObjek.FirstOrDefault(jo =>
            jo.JKWPTJBahagianDebitId == jKWPTJBahagianDebitId && jo.AkCartaDebitId == akCartaDebitId
            && jo.JKWPTJBahagianKreditId == jKWPTJBahagianKreditId && jo.AkCartaKreditId == akCartaKreditId)!;

            if (line == null)
            {
                collectionObjek.Add(new AkJurnalObjek()
                {
                    AkJurnalId = akJurnalId,
                    AkCartaDebitId = akCartaDebitId,
                    AkCartaKreditId = akCartaKreditId,
                    JKWPTJBahagianDebitId = jKWPTJBahagianDebitId,
                    JKWPTJBahagianKreditId = jKWPTJBahagianKreditId,
                    Amaun = amaun,
                    IsDebitAbBukuVot = isDebitAbBukuVot,
                    IsKreditAbBukuVot = isKreditAbBukuVot
                });
            }

        }

        public virtual void RemoveItemObjek(int jKWPTJBahagianDebitId, int akCartaDebitId, int jKWPTJBahagianKreditId, int akCartaKreditId) =>
            collectionObjek.RemoveAll(l =>
            l.JKWPTJBahagianDebitId == jKWPTJBahagianDebitId &&
            l.AkCartaDebitId == akCartaDebitId &&
            l.JKWPTJBahagianKreditId == jKWPTJBahagianKreditId &&
            l.AkCartaKreditId == akCartaKreditId);

        public virtual void ClearObjek() => collectionObjek.Clear();

        public virtual IEnumerable<AkJurnalObjek> AkJurnalObjek => collectionObjek;
        //

        // Jurnal Penerima Cek Batal
        private List<AkJurnalPenerimaCekBatal> collectionPenerimaCekBatal = new List<AkJurnalPenerimaCekBatal>();

        public virtual void AddItemPenerimaCekBatal(
            int akJurnalId,
            int bil,
            int akPVId,
            string? namaPenerima,
            string? noCek,
            decimal amaun)
        {
            AkJurnalPenerimaCekBatal line = collectionPenerimaCekBatal.FirstOrDefault(jp => jp.Bil == bil && jp.AkPVId == akPVId)!;

            if (line == null)
            {
                collectionPenerimaCekBatal.Add(new AkJurnalPenerimaCekBatal()
                {
                    AkJurnalId = akJurnalId,
                    Bil = bil,
                    AkPVId = akPVId,
                    NamaPenerima = namaPenerima,
                    NoCek = noCek,
                    Amaun = amaun
                });
            }
        }
        public virtual void RemoveItemPenerimaCekBatal(int bil, int akPVId) => collectionPenerimaCekBatal.RemoveAll(p => p.Bil.Equals(bil) && p.AkPVId.Equals(akPVId));

        public virtual void ClearPenerimaCekBatal() => collectionPenerimaCekBatal.Clear();

        public virtual IEnumerable<AkJurnalPenerimaCekBatal> AkJurnalPenerimaCekBatal => collectionPenerimaCekBatal;
        
        //
    }
}
