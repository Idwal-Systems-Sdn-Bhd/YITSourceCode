using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YIT.__Domain.Entities._Enums;
using YIT.__Domain.Entities.Models._03Akaun;

namespace YIT._DataAccess.Services.Cart
{
    public class CartAkNotaDebitKreditDiterima
    {
        //NotaDebitKreditDiterimaObjek
        private List<AkNotaDebitKreditDiterimaObjek> collectionObjek = new List<AkNotaDebitKreditDiterimaObjek>();

        public virtual void AddItemObjek(
            int akNotaDebitKreditDiterimaId,
            int jKWPTJBahagianId,
            int akCartaId,
            decimal amaun
            )
        {
            AkNotaDebitKreditDiterimaObjek line = collectionObjek.FirstOrDefault(pp => pp.JKWPTJBahagianId == jKWPTJBahagianId && pp.AkCartaId == akCartaId)!;

            if (line == null)
            {
                collectionObjek.Add(new AkNotaDebitKreditDiterimaObjek()
                {
                    AkNotaDebitKreditDiterimaId = akNotaDebitKreditDiterimaId,
                    JKWPTJBahagianId = jKWPTJBahagianId,
                    AkCartaId = akCartaId,
                    Amaun = amaun
                });
            }
        }

        public virtual void RemoveItemObjek(int jKWPTJBahagianId, int akCartaId) => collectionObjek.RemoveAll(l => l.JKWPTJBahagianId == jKWPTJBahagianId && l.AkCartaId == akCartaId);

        public virtual void ClearObjek() => collectionObjek.Clear();

        public virtual IEnumerable<AkNotaDebitKreditDiterimaObjek> AkNotaDebitKreditDiterimaObjek => collectionObjek;
        //

        // NotaDebitKreditDiterimaPerihal

        private List<AkNotaDebitKreditDiterimaPerihal> collectionPerihal = new List<AkNotaDebitKreditDiterimaPerihal>();
        public virtual void AddItemPerihal(
            int akNotaDebitKreditDiterimaId,
            decimal bil,
            string? perihal,
            decimal kuantiti,
            int? lHDNKodKlasifikasiId,
            int? lHDNUnitUkuranId,
            string? unit,
            EnLHDNJenisCukai enLHDNJenisCukai,
            decimal kadarCukai,
            decimal amaunCukai,
            decimal harga,
            decimal amaun
            )
        {
            AkNotaDebitKreditDiterimaPerihal line = collectionPerihal.FirstOrDefault(pp => pp.Bil == bil)!;

            if (line == null)
            {
                amaunCukai = Math.PriceFormatter.GetTaxAmount(harga, kuantiti, kadarCukai);
                collectionPerihal.Add(new AkNotaDebitKreditDiterimaPerihal
                {
                    AkNotaDebitKreditDiterimaId = akNotaDebitKreditDiterimaId,
                    Bil = bil,
                    Perihal = perihal,
                    Kuantiti = kuantiti,
                    LHDNKodKlasifikasiId = lHDNKodKlasifikasiId,
                    LHDNUnitUkuranId = lHDNUnitUkuranId,
                    Unit = unit,
                    EnLHDNJenisCukai = enLHDNJenisCukai,
                    KadarCukai = kadarCukai,
                    AmaunCukai = amaunCukai,
                    Harga = harga,
                    Amaun = Math.PriceFormatter.GetTotalPayableAmount(harga, kuantiti, amaunCukai)


                });
            }
        }

        public virtual void RemoveItemPerihal(decimal bil) => collectionPerihal.RemoveAll(l => l.Bil == bil);

        public virtual void ClearPerihal() => collectionPerihal.Clear();

        public virtual IEnumerable<AkNotaDebitKreditDiterimaPerihal> AkNotaDebitKreditDiterimaPerihal => collectionPerihal;
        //
    }
}
